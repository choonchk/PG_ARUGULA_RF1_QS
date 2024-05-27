using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Ivi.Visa.Interop;
using System.Threading;
using System.Collections;
using System.IO;
using Ivi.Visa.Interop;
using System.Diagnostics;
//using NiVstCommonLib;
using InstrLib;
using TestLib;
using SnP_BuddyFileBuilder;

namespace LibFBAR
{
    //#region "Enum"

    //#endregion

    //#region "Structure"
    //public struct s_Result
    //{
    //    public int TestNumber;
    //    public bool Enable;
    //    public bool b_MultiResult;
    //    public string Result_Header;
    //    public double Result_Data;
    //    public s_mRslt[] Multi_Results;
    //    public int Misc;
    //    //public string Result_Unit;  // if required
    //}
    //public struct s_mRslt
    //{
    //    public bool Enable;
    //    public string Result_Header;
    //    public double Result_Data;
    //}

    //struct s_TestSenario
    //{
    //    public bool Multithread;
    //    public int Start_Items;
    //    public int Stop_Items;
    //    public int Items;
    //}
    //#endregion
    struct s_Classes
    {
        public bool FBAR;
        public bool DC;
        public bool Switch;
        public bool DMM;
        public bool PA;
        public bool MM;
        public bool COMMON;
    }
    public class cAlgorithm : ClothoLibStandard.Lib_Var //Added by KCC
    {
        #region "Declarations"
        public static string ClassName = "Algorithm Class";

        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();
        
        //private static cExcel.cExcel_Lib Excel = new cExcel.cExcel_Lib();
        public string TCF_FileName;

        public int TotalTest;
        public s_Result[] Results;
        static int ModeSet = 1;

        // Declare Classes
        private s_Classes Class_Initialization;
        public cFBAR FBAR;
        public cDC_PowerSupply DC;
        public cDMM DMM;
        public cSwitch SWITCH;
        public cMM MM;
        public cCommon COMMON;

        // Test Condition Declaration
        private static Dictionary<string, int> Test_Parameters = new Dictionary<string, int>();
        private static int TestCondition_StartRow;
        private static int TestCondition_EndRow;
        private static string[] TestCondition_Test;
        private static string[] TestCondition_TestMode;
        private static int[] UsePrevious_Settings;

        //KCC - Added to perform FBAR band switching
        private static string[] TestCondition_Band;

        //CM Wong
        private static string[] TestCondition_PowerMode;
        private static string[] TestCondition_MipiDacBit;
        private static string[] TestCondition_MipiDacBit2;
        private static string[] TestCondition_FreqLog;

        // Multi Test Condition Declaration
        private static s_TestSenario[] TestSenario;
        private static int MultiTest_Senarios;
        private static bool MultiTest_Auto_Flag;
        private static Dictionary<int, int> MultiTest_StepSenario = new Dictionary<int, int>();

        // Data Triggering Declaration
        private static cFBAR.s_SParam_Grab[] DataTriggered;
        private static int DataTrigger_No;

        // Error Flag Declaration
        private static bool RaiseError;
        private static bool RaiseError_Calibration;

        private static bool DC_Bias_Flag;

        public string ENA_Address;

        //KCC - Name for TCF
        private string TestConditionFBAR = "Test_Condition_FBAR";

        public double[] TestTime;
        #endregion
        public s_SNPFile SNPFile;
        public int tmpUnit_No;
        public bool b_FirstTest = false;
        public bool b_SDIServer = false;
        public List<string> TempFolderName = new List<string>();
        public string NA_StateFile;
        public List<Dictionary<string, string>> DicTestCondTempNA;
        public bool b_DC_Init;

        public cAlgorithm()
        {
            FBAR = new cFBAR();
            DC = new cDC_PowerSupply();
            SWITCH = new cSwitch();
            COMMON = new cCommon();
        }

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.0.01";        //  10/11/2011       KKL             New and example for new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " - Version = v" + VersionStr);
        }

        public void Initialization()
        {
            tmpUnit_No = 1;

            RaiseError = false;
            LoadExcel(TCF_FileName);

            DMM = new cDMM();
            DMM.InitEq("");
            DC.Load_DC_ChannelSettings();

            Load_TestParameterHeader();
            Load_TestCondition();

            if (Class_Initialization.FBAR) Initialize_FBAR();
            //if (Class_Initialization.DC) Initialize_DC();

            for (int iTest = 0; iTest < TotalTest; iTest++)
            {
                Initialize_TestCondition(iTest);
            }
            Initialize_MultiThreadingProcedure();

        }

        public void LoadExcel(string Filename)
        {
            if (Filename != "" && Filename != null)
            {
                cExtract.Load_File(Filename);
            }
        }

        public void CloseExcel()
        {
            cExtract.Close_File();
        }

        public FormattedIO488 parse_ENA_IO
        {
            set
            {
                FBAR.parseNA_IO = value;
                //Load_FBAR();
            }
        }

        public void Initialize_FBAR()
        {
            FBAR.InitEq(ENA_Address);
            Load_FBAR();
        }

        public void Initialize_DC()
        {

            if (b_DC_Init) DC.InitEquipment();
        }

        public void Load_FBAR_State()
        {
            if (NA_StateFile != "" && NA_StateFile != null)
            {
                FBAR.Load_StateFile(NA_StateFile);
            }
        }

        public void Save_FBAR_State()
        {
            if (NA_StateFile != "" && NA_StateFile != null)
            {
                FBAR.Save_StateFile(NA_StateFile);
            }
        }

        public void Load_FBAR()
        {
            Load_FBAR_State();
            FBAR.Init_Channel();
            FBAR.Init_SegmentParam();
            //FBAR.SetTraceMatching();
            FBAR.Verify_SegmentParam();
            FBAR.Init_PortMatching(); //seoul
            FBAR.GetFrequencyList();
            //FBAR.SetTrigger(e_TriggerSource.BUS);
            FBAR.SetTriggerSingle(e_TriggerSource.BUS);
        }

        private string AddText(string InputStr, string AdditionStr, int Position)
        {
            string[] temp = InputStr.Split('_');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Position; i++)
            {
                sb.AppendFormat("{0}_", temp[i]);
            }
            sb.AppendFormat("{0}_", AdditionStr);
            for (int i = Position; i < temp.Length; i++)
            {
                sb.AppendFormat("{0}_", temp[i]);
            }
            return sb.ToString().Trim('_');
        }
        private string RemoveText(string InputStr, int Position)
        {
            int count = 0;
            string[] temp = InputStr.Split('\\');
            count = temp.Length-1;
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < Position; i++)
            //{
            //    sb.AppendFormat("{0}_", temp[i]);
            //}
            return temp[count-1];
        }

        public void Init_SNPFile(int Test)
        {
            string NewPath = "";
            if (TestCondition_Test[Test].ToUpper() == "TRIGGER")
            {
                FBAR.TestClass[Test].Trigger.FileOutput_Enable = SNPFile.FileOutput_Enable;
                if (SNPFile.FileOutput_Enable)
                {
                    if (FBAR.TestClass[Test].Trigger.FileOutput_Mode != "")
                    {
                        //NewPath = AddText(SNPFile.FileOutput_Path, FBAR.TestClass[Test].Trigger.FileOutput_Mode + "_CHAN" + FBAR.TestClass[Test].Trigger.ChannelNumber, 1);
                        NewPath = AddText(SNPFile.FileOutput_Path, FBAR.TestClass[Test].Trigger.FileOutput_Mode+"_CH"+FBAR.TestClass[Test].Trigger.ChannelNumber , 1);

                    }
                    else
                    {
                        NewPath = SNPFile.FileOutput_Path;

                    }
                    
                    
                    bool testbool = System.IO.Directory.Exists(NewPath);

                    FBAR.TestClass[Test].Trigger.FileOutput_FileName = SNPFile.FileOutput_FileName + "_" + string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now) + "_";
                    //SNPFile.FileOutput_FileName = FBAR.TestClass[Test].Trigger.FileOutput_FileName;

                    if (!System.IO.Directory.Exists(NewPath))
                    {
                        System.IO.Directory.CreateDirectory(NewPath);
                        Generate_SNP_Header(NewPath + SNPFile.FileOutput_FileName + ".txt", SNPFile.FileOutput_HeaderName);
                        //SNPFile.FileOutput_Path = NewPath;
                        SNPFile.FileOutput_HeaderCount++;
                        TempFolderName.Add(NewPath);
                        
                    }
                     FBAR.TestClass[Test].Trigger.FileOutput_Path = NewPath;
                    //FBAR.TestClass[Test].Trigger.FileOutput_Mode = "";
                    FBAR.TestClass[Test].Trigger.FileOutput_Unit = tmpUnit_No;
                    FBAR.TestClass[Test].Trigger.FileOutput_Count = SNPFile.FileOuuput_Count;

                }
            }
            else if (TestCondition_Test[Test].ToUpper() == "TRIGGER2")
            {
                FBAR.TestClass[Test].Trigger2.FileOutput_Enable = SNPFile.FileOutput_Enable;
                if (SNPFile.FileOutput_Enable)
                {
                    FBAR.TestClass[Test].Trigger2.FileOutput_Path = SNPFile.FileOutput_Path;
                    FBAR.TestClass[Test].Trigger2.FileOutput_FileName = SNPFile.FileOutput_FileName + "_" + string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now) + "_";
                    //FBAR.TestClass[Test].Trigger2.FileOutput_Mode = "";
                    FBAR.TestClass[Test].Trigger2.FileOutput_Unit = tmpUnit_No;
                }
            }
        }

        public void SNP_SDI_Compression(string ProductTag, string lotId, string SublotId, string TesterIP, string sdi_inbox_wave)
        {

            ClothoLibStandard.IO_TextFile IO = new ClothoLibStandard.IO_TextFile();
            string defaultpath = @"C:\Avago.ATF.Common\DataLog\";
            string folderpath = ProductTag + "_" + lotId + "_" + SublotId + "_" + TesterIP;
            int ListCount = 0;
            ListCount = TempFolderName.Count();

            string TempString;

            for (int i = 0; i < ListCount; i++)
            {
                if (ProductTag != "" && lotId != "")
                {
                    TempString = RemoveText(TempFolderName[i], 1);
                    IO.CompressSDIFileInDirectory(defaultpath, TempString);
                    if (b_SDIServer)
                    {
                        IO.CopyFile(defaultpath + TempString + @".tar.bz2", sdi_inbox_wave + TempString + @".tar.bz2");
                        //IO.DeleteFile(defaultpath + TempString + @".tar.bz2");
                        IO.DeleteFolder(defaultpath + TempString);
                    }
                        //File.Delete(defaultpath + folderpath + @".tar.bz2");
                }
                else
                {
                    TempString = TempFolderName[i];
                    IO.CompressSDIFileInDirectory(defaultpath, TempString);
                    if (b_SDIServer)
                    {
                        IO.CopyFile(defaultpath + TempString + @".tar.bz2", sdi_inbox_wave + folderpath + @".tar.bz2");
                        //IO.DeleteFile(defaultpath + TempString + @".tar.bz2");
                        IO.DeleteFolder(defaultpath + TempString);
                    }
                }
            }
        }

        //KCC - For lot number on folder
        public void Init_SNPFile()
        {
            for (int iRow = 0; iRow < TotalTest; iRow++)
            {
                switch (TestCondition_Test[iRow])
                {
                    case "TRIGGER":
                        FBAR.TestClass[iRow].Trigger.FileOutput_Enable = SNPFile.FileOutput_Enable;
                        FBAR.TestClass[iRow].Trigger.FileOutput_FileName = SNPFile.FileOutput_FileName + "_" + string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now) + "_";
                        FBAR.TestClass[iRow].Trigger.FileOutput_Path = SNPFile.FileOutput_Path;
                        break;
                }
            }
        }

        public void Load_TestParameterHeader()
        {
            int RowNo, ColNo;
            bool Found;
            bool FoundHeader;
            string tmpStr;
            RowNo = 1;
            ColNo = 2;
            Found = false;
            FoundHeader = false;
            TotalTest = 0;

            do
            {
                tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, 1);
                if (tmpStr.ToUpper() == "#START")
                {
                    TestCondition_StartRow = RowNo + 1;
                    do
                    {
                        tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, ColNo);
                        if (tmpStr != "")
                        {
                            try
                            {
                                Test_Parameters.Add(tmpStr, ColNo);
                            }
                            catch
                            {
                                RaiseError = true;
                                General.DisplayError(ClassName + " --> Test Parameters", "Error in processing Test Parameters in Test Condition", "Possible duplicate header at Column " + General.convertInt2ExcelColumn(ColNo));
                            }

                        }
                        ColNo++;
                        if (tmpStr.ToUpper() == "#END") FoundHeader = true;
                    } while (FoundHeader == false);
                }
                RowNo++;
                tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, 1);
                if (tmpStr.ToUpper() == "#END")
                {
                    TestCondition_EndRow = RowNo;
                    Found = true;
                }
                else if (FoundHeader)
                {
                    tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, Test_Parameters["Test Mode"]);
                    if (tmpStr != "")
                    {
                        TotalTest++;
                    }
                }
            } while (Found == false);
            if (!(Found && FoundHeader) || (TotalTest < 1))
            {
                General.DisplayError(ClassName, "Error processing Test Condition Header", "Missing Test Condition or Test Condition Header");
            }
        }

        //KCC - Added band condition
        public void Load_TestCondition()
        {
            #region "Declaration and Initialization"
            string tmpStr;
            string tmpTestMode;
            string tmpTestCondition;
            string TestCondition;
            string tmpHeader;

            //KCC 
            string tmpBand;
            //

            //CM WONG
            string tmpPowerMode;
            string tmpMipiDacBit;
            string tmpMipiDacBit2;
            string tmpFreqLog;

            // Internal Variables
            int Previous_Test_1;
            int Previous_Test_2;

            int TestCnt;
            int Previous_TestNo;
            bool b_Use_Previous;

            // For Multi Threading Procedure and Mechanism
            string tmpPreviousStr;      // {Theading} - check for Use Previous Test to avoid crashes in Multithreading
            string threadingStr;        // {Theading}
            bool b_Threading;           // {Theading}
            bool b_Threading_UsePrevious;   // {Theading}

            FBAR.TestClass = new cFBAR.cTestClasses[TotalTest];
            DC.TestClass = new cDC_PowerSupply.cTestClasses[TotalTest];
            //MM.TestClass = new cMM.cTestClasses[TotalTest];
            COMMON.TestClass = new cCommon.cTestClasses[TotalTest];
            //DMM.TestClass = new cDMM.cTestClasses[TotalTest];

            TestTime = new double[TotalTest];

            e_SParametersDef SParamDef;

            TestCondition = TestConditionFBAR;
            TestCnt = 0;
            MultiTest_Senarios = 0;     // {Theading}
            threadingStr = "";          // {Theading}
            b_Threading = false;        // {Theading}   
            b_Threading_UsePrevious = false;    // {Theading}
            MultiTest_Auto_Flag = true;        // {Theading}

            // Invoke Test Conditions
            TestCondition_Test = new string[TotalTest];
            TestCondition_TestMode = new string[TotalTest];

            UsePrevious_Settings = new int[TotalTest];      // {Theading}
            Previous_TestNo = 0;

            //KCC
            TestCondition_Band = new string[TotalTest];
            //

            //CM WONG
            TestCondition_PowerMode = new string[TotalTest];
            TestCondition_MipiDacBit = new string[TotalTest];
            TestCondition_MipiDacBit2 = new string[TotalTest];
            TestCondition_FreqLog = new string[TotalTest];

            // Invoke for FBAR Trigger
            DataTrigger_No = 0;
            DataTriggered = new cFBAR.s_SParam_Grab[1];

            //Modified by KCC
            //DataTriggered[DataTrigger_No].SParam_Grab = new bool[24];
            DataTriggered[DataTrigger_No].SParam_Grab = new bool[28];
            //

            // Invoke Tests Results
            Results = new s_Result[TotalTest];
            FBAR.Init(TotalTest);
            DC.Init(TotalTest);
            //DMM.Init(TotalTest);
            COMMON.Init(TotalTest);
            //MM.Init(TotalTest);

            #endregion

            //for (int iRow = TestCondition_StartRow; iRow < TestCondition_EndRow; iRow++)
            //{
            foreach (Dictionary<string, string> TestCond in DicTestCondTempNA)
            {
                //tmpTestMode = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Test Mode"]);
                tmpTestMode = GetStr(TestCond, "Test Mode");
                b_Use_Previous = false;
                if (tmpTestMode.ToUpper() != "COMMON")
                {
                    //if (TestCnt == 171)
                    //    MessageBox.Show("A");
                    // Check for Previous Test

                    //tmpPreviousStr = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Use_Previous"]);
                    tmpPreviousStr = GetStr(TestCond, "Use_Previous");

                    if (tmpPreviousStr == "")
                    {
                        Previous_TestNo = TestCnt;
                        UsePrevious_Settings[TestCnt] = 0;    // {Theading}
                    }
                    else if (tmpPreviousStr.ToUpper() != "V")
                    {
                        Previous_TestNo = int.Parse(tmpPreviousStr);
                        b_Use_Previous = true;
                        UsePrevious_Settings[TestCnt] = 0;          // {Theading}
                    }
                    else if (tmpPreviousStr.ToUpper() == "V")
                    {
                        b_Use_Previous = true;
                    }
                }
                else
                {
                    tmpPreviousStr = "";
                    Previous_TestNo = TestCnt;
                    UsePrevious_Settings[TestCnt] = 0;    // {Theading}
                }
            

                //--------------------------------------------------------------

                #region "Initialize Test Classes"

                TestCondition_TestMode[TestCnt] = tmpTestMode;
                //tmpTestCondition = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Test Parameter"]);
                tmpTestCondition = GetStr(TestCond, "Test Parameter");

                //KCC
                //tmpBand = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Band"]);
                tmpBand = GetStr(TestCond, "Band");

                if (tmpBand != "" && tmpBand != null)
                {
                    TestCondition_Band[TestCnt] = tmpBand;
                }
                else
                {
                    TestCondition_Band[TestCnt] = "None";
                }
                //

                //CM WONG
                //tmpPowerMode = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Power_Mode"]);
                tmpPowerMode = GetStr(TestCond, "Power_Mode");

                if (tmpPowerMode != "" && tmpPowerMode != null)
                {
                    TestCondition_PowerMode[TestCnt] = tmpPowerMode;
                }
                else
                {
                    TestCondition_PowerMode[TestCnt] = "None";
                }
                //tmpMipiDacBit = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["MIPI_DACQ1"]);
                tmpMipiDacBit = GetStr(TestCond, "MIPI_DACQ1"); 

                if (tmpMipiDacBit != "" && tmpMipiDacBit != null)
                {
                    TestCondition_MipiDacBit[TestCnt] = tmpMipiDacBit;
                }
                else
                {
                    TestCondition_MipiDacBit[TestCnt] = "None";
                }
                //tmpMipiDacBit2 = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["MIPI_DACQ2"]);
                tmpMipiDacBit2 = GetStr(TestCond, "MIPI_DACQ2");
                    
                if (tmpMipiDacBit2 != "" && tmpMipiDacBit2 != null)
                {
                    TestCondition_MipiDacBit2[TestCnt] = tmpMipiDacBit2;
                }
                else
                {
                    TestCondition_MipiDacBit2[TestCnt] = "None";
                }

                tmpFreqLog = GetStr(TestCond, "Freq_Log");

                if (tmpFreqLog != "" && tmpFreqLog != null)
                {
                    TestCondition_FreqLog[TestCnt] = tmpFreqLog;
                }
                else
                {
                    TestCondition_FreqLog[TestCnt] = "None";
                }
                

                try
                {
                    switch (tmpTestMode.ToUpper())
                    {
                        case "FBAR":
                            Class_Initialization.FBAR = true;
                            FBAR.TestClass[TestCnt] = new cFBAR.cTestClasses();
                            #region "FBAR"
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "TRIGGER":
                                    #region "Trigger"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Trigger = new cFBAR.cTestClasses.cTrigger();
                                    FBAR.TestClass[TestCnt].Trigger.TestNo = TestCnt;
                                    //FBAR.TestClass[TestCnt].Trigger.ChannelNumber = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Channel Number"]));
                                    FBAR.TestClass[TestCnt].Trigger.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));                                
                                    //FBAR.TestClass[TestCnt].Trigger.Sleep_ms = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Sleep (Wait - ms)"]));
                                    FBAR.TestClass[TestCnt].Trigger.Sleep_ms = int.Parse(GetStr(TestCond, "Sleep (Wait - ms)"));
                                    //FBAR.TestClass[TestCnt].Trigger.Misc_Settings = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Misc"]);
                                    FBAR.TestClass[TestCnt].Trigger.Misc_Settings = GetStr(TestCond, "Misc");
                                    FBAR.TestClass[TestCnt].Trigger.FileOutput_Counting = 0;
                                    //Init_SNPFile(TestCnt);

                                    //FBAR.TestClass[TestCnt].Trigger.SnPFile_Name = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Search_Method"]);
                                    FBAR.TestClass[TestCnt].Trigger.SnPFile_Name = GetStr(TestCond, "Search_Method");
                                    if (Test_Parameters.ContainsKey("Power_Mode"))
                                    {
                                        FBAR.TestClass[TestCnt].Trigger.FileOutput_Mode = GetStr(TestCond, "Power_Mode");
                                    }
                                    break;
                                    #endregion
                                case "TRIGGER2":
                                    #region "Trigger2"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Trigger2 = new cFBAR.cTestClasses.cTrigger2();
                                    FBAR.TestClass[TestCnt].Trigger2.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Trigger2.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Trigger2.Sleep_ms = int.Parse(GetStr(TestCond, "Sleep (Wait - ms)"));
                                    FBAR.TestClass[TestCnt].Trigger2.Misc_Settings = GetStr(TestCond, "Misc");
                                    Init_SNPFile(TestCnt);
                                    if (Test_Parameters.ContainsKey("Power_Mode"))
                                    {
                                        FBAR.TestClass[TestCnt].Trigger2.FileOutput_Mode = GetStr(TestCond, "Power_Mode");
                                    }

                                    break;
                                    #endregion
                                case "MAG_AT":
                                    #region "Mag_AT"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Mag_At = new cFBAR.cTestClasses.cMag_At();
                                    FBAR.TestClass[TestCnt].Mag_At.TestNo = TestCnt;
                                    if (GetStr(TestCond, "Target_Freq") == "")
                                    {
                                        //FBAR.TestClass[TestCnt].Mag_At.setHeader = GetStr(TestCond, "Parameter Header")
                                        //                                        + "_C" + GetStr(TestCond, "Channel Number")
                                        //                                        + "_" + GetStr(TestCond, "S-Parameter")
                                        //                                        + "_" + GetStr(TestCond, "Start_Freq")
                                        //                                        + "_" + GetStr(TestCond, "Stop_Freq");
                                        FBAR.TestClass[TestCnt].Mag_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_" + GetStr(TestCond, "Band")
                                                                                + "_" + GetStr(TestCond, "Power_Mode")
                                                                                + "_" + GetStr(TestCond, "Start_Freq")
                                                                                + "_" + GetStr(TestCond, "Stop_Freq");
                                        
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Mag_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_C" + GetStr(TestCond, "Channel Number")
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "Target_Freq");
                                    }
                                    FBAR.TestClass[TestCnt].Mag_At.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Mag_At.SParameters = GetStr(TestCond, "S-Parameter");
                                    if (!b_Use_Previous) FBAR.TestClass[TestCnt].Mag_At.Frequency_At = General.convertStr2Val(GetStr(TestCond, "Target_Freq"));
                                    FBAR.TestClass[TestCnt].Mag_At.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Mag_At.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    if (b_Use_Previous)
                                    {
                                        FBAR.TestClass[TestCnt].Mag_At.Previous_TestNo = Previous_TestNo;
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Mag_At.Previous_TestNo = -1; //Prevent Error
                                    }
                                    break;
                                    #endregion
                                case "MAG_AT_LIN":
                                    #region "Mag_AT_LIN"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Mag_At_Lin = new cFBAR.cTestClasses.cMag_At_Lin();
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_C" + GetStr(TestCond, "Channel Number")
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "Target_Freq");
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.SParameters = GetStr(TestCond, "S-Parameter");
                                    if (!b_Use_Previous) FBAR.TestClass[TestCnt].Mag_At_Lin.Frequency_At = General.convertStr2Val(GetStr(TestCond, "Target_Freq"));
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Mag_At_Lin.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    if (b_Use_Previous)
                                    {
                                        FBAR.TestClass[TestCnt].Mag_At_Lin.Previous_TestNo = Previous_TestNo;
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Mag_At_Lin.Previous_TestNo = -1; //Prevent Error
                                    }
                                    break;
                                    #endregion
                                case "REAL_AT":
                                    #region "REAL_AT"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Real_At = new cFBAR.cTestClasses.cReal_At();
                                    FBAR.TestClass[TestCnt].Real_At.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Real_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_C" + GetStr(TestCond, "Channel Number")
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "Target_Freq");
                                    FBAR.TestClass[TestCnt].Real_At.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Real_At.SParameters = GetStr(TestCond, "S-Parameter");
                                    if (!b_Use_Previous) FBAR.TestClass[TestCnt].Real_At.Frequency_At = General.convertStr2Val(GetStr(TestCond, "Target_Freq"));
                                    FBAR.TestClass[TestCnt].Real_At.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Real_At.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    if (b_Use_Previous)
                                    {
                                        FBAR.TestClass[TestCnt].Real_At.Previous_TestNo = Previous_TestNo;
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Real_At.Previous_TestNo = -1; //Prevent Error
                                    }
                                    break;
                                    #endregion
                                case "IMAG_AT":
                                    #region "IMAG_AT"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Imag_At = new cFBAR.cTestClasses.cImag_At();
                                    FBAR.TestClass[TestCnt].Imag_At.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Imag_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_C" + GetStr(TestCond, "Channel Number")
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "Target_Freq");
                                    FBAR.TestClass[TestCnt].Imag_At.ChannelNumber = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Imag_At.SParameters = GetStr(TestCond, "S-Parameter");
                                    if (!b_Use_Previous) FBAR.TestClass[TestCnt].Imag_At.Frequency_At = General.convertStr2Val(GetStr(TestCond, "Target_Freq"));
                                    FBAR.TestClass[TestCnt].Imag_At.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Imag_At.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    if (b_Use_Previous)
                                    {
                                        FBAR.TestClass[TestCnt].Imag_At.Previous_TestNo = Previous_TestNo;
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Imag_At.Previous_TestNo = -1; //Prevent Error
                                    }
                                    break;
                                    #endregion
                                case "PHASE_AT":
                                    #region "PHASE_AT"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Phase_At = new cFBAR.cTestClasses.cPhase_At();
                                    FBAR.TestClass[TestCnt].Phase_At.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Phase_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + "_C" + GetStr(TestCond, "Channel Number")
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "Target_Freq");
                                    FBAR.TestClass[TestCnt].Phase_At.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Phase_At.SParameters = GetStr(TestCond, "S-Parameter");
                                    if (!b_Use_Previous) FBAR.TestClass[TestCnt].Phase_At.Target_Frequency = General.convertStr2Val(GetStr(TestCond, "Target_Freq"));
                                    FBAR.TestClass[TestCnt].Phase_At.Interpolation = GetStr(TestCond, "Interpolation");
                                    //FBAR.TestClass[TestCnt].Phase_At.ErrorRaise = General.convertStr2Val(cExtract.Get_Data_Zero(TestCondition, iRow, Test_Parameters["Offset"]));
                                    //if (b_Use_Previous)
                                    //{
                                    //    FBAR.TestClass[TestCnt].Phase_At.Previous_TestNo = Previous_TestNo;
                                    //}
                                    //else
                                    //{
                                    //    FBAR.TestClass[TestCnt].Phase_At.Previous_TestNo = -1; //Prevent Error
                                    //}
                                    break;
                                    #endregion
                                case "FREQ_AT":
                                    #region "Freq_AT"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Freq_At = new cFBAR.cTestClasses.cFreq_At();
                                    FBAR.TestClass[TestCnt].Freq_At.TestNo = TestCnt;
                                    //FBAR.TestClass[TestCnt].Freq_At.setHeader = GetStr(TestCond, "Parameter Header")
                                    //                                            + "_C" + GetStr(TestCond, "Channel Number")
                                    //                                            + "_" + GetStr(TestCond, "S-Parameter")
                                    //                                            + "_" + GetStr(TestCond, "Start_Freq")
                                    //                                            + "_" + GetStr(TestCond, "Stop_Freq");
                                    if (GetStr(TestCond, "Start_Freq").ToUpper() == GetStr(TestCond, "Stop_Freq").ToUpper())
                                    {
                                        FBAR.TestClass[TestCnt].Freq_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0];
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Freq_At.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0]
                                                                                    + "_" + GetStr(TestCond, "Stop_Freq").Split(' ')[0];
                                    }
                                    FBAR.TestClass[TestCnt].Freq_At.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Freq_At.SParameters = GetStr(TestCond, "S-Parameter");
                                    FBAR.TestClass[TestCnt].Freq_At.StartFreq = General.convertStr2Val(GetStr(TestCond, "Start_Freq"));
                                    FBAR.TestClass[TestCnt].Freq_At.StopFreq = General.convertStr2Val(GetStr(TestCond, "Stop_Freq"));
                                    FBAR.TestClass[TestCnt].Freq_At.Search_DirectionMethod = GetStr(TestCond, "Search_Direction");
                                    FBAR.TestClass[TestCnt].Freq_At.Search_Type = GetStr(TestCond, "Search_Method");
                                    FBAR.TestClass[TestCnt].Freq_At.Search_Value = GetStr(TestCond, "Search_Value");
                                    FBAR.TestClass[TestCnt].Freq_At.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Freq_At.b_Invert_Search = General.CStr2Bool(GetStr(TestCond, "Misc"));
                                    FBAR.TestClass[TestCnt].Freq_At.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    //Below for Second Report File
                                    FBAR.TestClass[TestCnt].Freq_At.Band = GetStr(TestCond, "Band");
                                    FBAR.TestClass[TestCnt].Freq_At.PowerMode = GetStr(TestCond, "Power_Mode");

                                    foreach (Tuple<string,string,string> value in SNPFile.Impedance_Dictionary)
                                    {
                                        if (value.Item1.Contains(GetStr(TestCond, "S-Parameter")) && value.Item2.Contains(GetStr(TestCond, "Band")))
                                        {
                                            FBAR.TestClass[TestCnt].Freq_At.Port_Impedance = value.Item3;
                                            break;
                                        }
                                    }


                                    break;
                                    #endregion
                                case "MAG_BETWEEN":
                                    #region "Mag_Between"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Mag_Between = new cFBAR.cTestClasses.cMag_Between();
                                    FBAR.TestClass[TestCnt].Mag_Between.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Mag_Between.setHeader = GetStr(TestCond, "Parameter Header")
                                        //+ "_C" + GetStr(TestCond, "Channel Number"])
                                                                                    + "_" + GetStr(TestCond, "S-Parameter")
                                                                                    + "_" + ((GetStr(TestCond, "Start_Freq")).Split(' ')[0])
                                                                                    + "_" + ((GetStr(TestCond, "Stop_Freq")).Split(' ')[0]);
                                    FBAR.TestClass[TestCnt].Mag_Between.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Mag_Between.SParameters = GetStr(TestCond, "S-Parameter");
                                    FBAR.TestClass[TestCnt].Mag_Between.StartFreq = General.convertStr2Val(GetStr(TestCond, "Start_Freq"));
                                    FBAR.TestClass[TestCnt].Mag_Between.StopFreq = General.convertStr2Val(GetStr(TestCond, "Stop_Freq"));
                                    FBAR.TestClass[TestCnt].Mag_Between.Search_MethodType = GetStr(TestCond, "Search_Method");
                                    FBAR.TestClass[TestCnt].Mag_Between.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].Mag_Between.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    FBAR.TestClass[TestCnt].Mag_Between.Non_Inverted = GetStr(TestCond, "Non_Inverted");
                                    FBAR.TestClass[TestCnt].Mag_Between.Freq_Log = GetStr(TestCond, "Freq_Log");
                                    
                                    break;
                                    #endregion
                                case "CPL_BETWEEN":
                                    #region "CPL_BETWEEN"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].CPL_Between = new cFBAR.cTestClasses.cCPL_Between();
                                    FBAR.TestClass[TestCnt].CPL_Between.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].CPL_Between.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_C" + GetStr(TestCond, "Channel Number")
                                                                                    + "_" + GetStr(TestCond, "S-Parameter")
                                                                                    + "_" + GetStr(TestCond, "S-Parameter_2")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq")
                                                                                    + "_" + GetStr(TestCond, "Stop_Freq");
                                    FBAR.TestClass[TestCnt].CPL_Between.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].CPL_Between.SParameters1 = GetStr(TestCond, "S-Parameter");
                                    FBAR.TestClass[TestCnt].CPL_Between.SParameters2 = GetStr(TestCond, "S-Parameter_2");
                                    FBAR.TestClass[TestCnt].CPL_Between.StartFreq = General.convertStr2Val(GetStr(TestCond, "Start_Freq"));
                                    FBAR.TestClass[TestCnt].CPL_Between.StopFreq = General.convertStr2Val(GetStr(TestCond, "Stop_Freq"));
                                    FBAR.TestClass[TestCnt].CPL_Between.Search_MethodType = GetStr(TestCond, "Search_Method");
                                    FBAR.TestClass[TestCnt].CPL_Between.Interpolation = GetStr(TestCond, "Interpolation");
                                    FBAR.TestClass[TestCnt].CPL_Between.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    break;
                                    #endregion
                                case "RIPPLE_BETWEEN":
                                    #region "Ripple_Between"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Ripple_Between = new cFBAR.cTestClasses.cRipple_Between();
                                    FBAR.TestClass[TestCnt].Ripple_Between.TestNo = TestCnt;
                                    //FBAR.TestClass[TestCnt].Ripple_Between.setHeader = GetStr(TestCond, "Parameter Header")
                                    //                                            + "_C" + GetStr(TestCond, "Channel Number")
                                    //                                            + "_" + GetStr(TestCond, "S-Parameter")
                                    //                                            + "_" + GetStr(TestCond, "Start_Freq")
                                    //                                            + "_" + GetStr(TestCond, "Stop_Freq");
                                    if (GetStr(TestCond, "Start_Freq").ToUpper() == GetStr(TestCond, "Stop_Freq").ToUpper())
                                    {
                                        FBAR.TestClass[TestCnt].Ripple_Between.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0];
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Ripple_Between.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0]
                                                                                    + "_" + GetStr(TestCond, "Stop_Freq").Split(' ')[0];
                                    }
                                    FBAR.TestClass[TestCnt].Ripple_Between.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Ripple_Between.SParameters = GetStr(TestCond, "S-Parameter");
                                    FBAR.TestClass[TestCnt].Ripple_Between.StartFreq = General.convertStr2Val(GetStr(TestCond, "Start_Freq"));
                                    FBAR.TestClass[TestCnt].Ripple_Between.StopFreq = General.convertStr2Val(GetStr(TestCond, "Stop_Freq"));
                                    FBAR.TestClass[TestCnt].Ripple_Between.b_Absolute = General.CStr2Bool(GetStr(TestCond, "Absolute Value"));
                                    FBAR.TestClass[TestCnt].Ripple_Between.Offset = General.convertStr2Val(GetStr_Zero(TestCond, "Offset"));
                                    FBAR.TestClass[TestCnt].Ripple_Between.Sampling_Mode = GetStr(TestCond, "Sampling_Mode");
                                    if (FBAR.TestClass[TestCnt].Ripple_Between.Sampling_Mode.ToUpper() == "V")
                                    {
                                        FBAR.TestClass[TestCnt].Ripple_Between.Sampling_BW = General.convertStr2Val(GetStr(TestCond, "Sampling_BW"));
                                        FBAR.TestClass[TestCnt].Ripple_Between.Sampling_Interval = General.convertStr2Val(GetStr(TestCond, "Sampling_Interval"));
                                    }
                                    break;
                                    #endregion
                                case "BALANCE":
                                    #region "Balance"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Balance = new cFBAR.cTestClasses.cBalance();
                                    FBAR.TestClass[TestCnt].Balance.TestNo = TestCnt;
                                    FBAR.TestClass[TestCnt].Balance.setHeader = GetStr(TestCond, "Parameter Header")
                                        //+ "_C" + GetStr(TestCond, "Channel Number"])
                                                                                + "_" + GetStr(TestCond, "S-Parameter")
                                                                                + "_" + GetStr(TestCond, "S-Parameter_2")
                                                                                + "_" + ((GetStr(TestCond, "Start_Freq")).Split(' ')[0])
                                                                                + "_" + ((GetStr(TestCond, "Stop_Freq")).Split(' ')[0]);
                                    //+ "_" + GetStr(TestCond, "Balance_Type"]);
                                    FBAR.TestClass[TestCnt].Balance.Channel_Number = int.Parse(GetStr(TestCond, "Channel Number"));
                                    FBAR.TestClass[TestCnt].Balance.Search_Type = GetStr(TestCond, "Search_Method");
                                    FBAR.TestClass[TestCnt].Balance.BalanceType = GetStr(TestCond, "Balance_Type");
                                    FBAR.TestClass[TestCnt].Balance.SParameters_1 = GetStr(TestCond, "S-Parameter");
                                    FBAR.TestClass[TestCnt].Balance.SParameters_2 = GetStr(TestCond, "S-Parameter_2");
                                    FBAR.TestClass[TestCnt].Balance.StartFreq = General.convertStr2Val(GetStr(TestCond, "Start_Freq"));
                                    FBAR.TestClass[TestCnt].Balance.StopFreq = General.convertStr2Val(GetStr(TestCond, "Stop_Freq"));
                                    FBAR.TestClass[TestCnt].Balance.b_Absolute = General.CStr2Bool(GetStr(TestCond, "Absolute Value"));
                                    break;
                                    #endregion
                                case "CHANNEL_AVERAGING":
                                    #region "CHANNEL_AVERAGING"
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    FBAR.TestClass[TestCnt].Channel_Averaging = new cFBAR.cTestClasses.cChannel_Averaging();

                                    //FBAR.TestClass[TestCnt].Channel_Averaging.b_DualSite = b_DualSite;
                                    //FBAR.TestClass[TestCnt].Channel_Averaging.SiteNumber = Offset_SiteNumber(SiteNumber);
                                    FBAR.TestClass[TestCnt].Channel_Averaging.TestNo = TestCnt;
                                    //FBAR.TestClass[TestCnt].Channel_Averaging.setHeader = GetStr(TestCond,"Parameter Header")
                                    //                                            + "_C" + GetStr(TestCond,"Channel Number")
                                    //                                            + "_" + GetStr(TestCond,"S-Parameter")
                                    //                                            + "_" + GetStr(TestCond,"Start_Freq")
                                    //                                            + "_" + GetStr(TestCond,"Stop_Freq");
                                    if (GetStr(TestCond, "Start_Freq").ToUpper() == GetStr(TestCond, "Stop_Freq").ToUpper())
                                    {
                                        FBAR.TestClass[TestCnt].Channel_Averaging.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0]
                                                                                    + "_" + GetStr(TestCond, "Stop_Freq").Split(' ')[0];
                                    }
                                    else
                                    {
                                        FBAR.TestClass[TestCnt].Channel_Averaging.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                    + "_" + GetStr(TestCond, "Band")
                                                                                    + "_" + GetStr(TestCond, "Power_Mode")
                                                                                    + "_" + GetStr(TestCond, "Start_Freq").Split(' ')[0]
                                                                                    + "_" + GetStr(TestCond, "Stop_Freq").Split(' ')[0];
                                    }
                                    FBAR.TestClass[TestCnt].Channel_Averaging.Channel_Number = int.Parse(GetStr(TestCond,"Channel Number"));
                                    FBAR.TestClass[TestCnt].Channel_Averaging.SParameters = GetStr(TestCond,"S-Parameter");
                                    FBAR.TestClass[TestCnt].Channel_Averaging.StartFreq = General.convertStr2Val(GetStr(TestCond,"Start_Freq"));
                                    FBAR.TestClass[TestCnt].Channel_Averaging.StopFreq = General.convertStr2Val(GetStr(TestCond,"Stop_Freq"));
                                    FBAR.TestClass[TestCnt].Channel_Averaging.Offset = General.convertStr2Val(GetStr_Zero(TestCond,"Offset"));

                                    break;
                                    #endregion
                                default:
                                    RaiseError = true;
                                    General.DisplayError(ClassName, "Unrecognized FBAR Test Parameter", "Error in Processing FBAR Test Parameter : " + tmpTestCondition);
                                    break;
                            }
                            #endregion
                            break;
                        case "DC":
                            Class_Initialization.DC = true;
                            DC.TestClass[TestCnt] = new cDC_PowerSupply.cTestClasses();
                            #region "DC"
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "DC_SETTINGS":
                                case "DC_SETTING":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    DC.TestClass[TestCnt].SMU_DC_Setting = new cDC_PowerSupply.cTestClasses.cSMU_DC_Setting();
                                    DC.TestClass[TestCnt].SMU_DC_Setting.TestNo = TestCnt;
                                    DC.TestClass[TestCnt].SMU_DC_Setting.TotalChannel = GetInt(TestCond, "Total_DC_Channel");
                                    DC.TestClass[TestCnt].SMU_DC_Setting.DC_Set = GetInt(TestCond, "DC_PS_Set");
                                    DC.TestClass[TestCnt].SMU_DC_Setting.InitArray();
                                    for (int iDC = 0; iDC < DC.TestClass[TestCnt].SMU_DC_Setting.TotalChannel; iDC++)
                                    {
                                        DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].b_Enable = true;
                                        DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].Channel = iDC;
                                        tmpHeader = cExtract.Get_Data(TestCondition, 1, Test_Parameters["V_CH" + (iDC + 1).ToString()]);
                                        if (tmpHeader == "")
                                        {
                                            DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].Header = "I_CH" + (iDC + 1).ToString();
                                        }
                                        else
                                        {
                                            DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].Header = tmpHeader.Replace("V", "I");
                                        }
                                        DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].Voltage = GetDbl(TestCond, "V_CH" + (iDC + 1).ToString());
                                        DC.TestClass[TestCnt].SMU_DC_Setting.DC_Setting[iDC].Current = GetDbl(TestCond, "I_CH" + (iDC + 1).ToString());
                                    }
                                    //DC.TestClass[TestCnt].SMU_DC_Setting.MIPI_DACbit = GetStr(TestCond, "MIPI_DAC1"]);
                                    DC.TestClass[TestCnt].SMU_DC_Setting.PowerMode = GetStr(TestCond, "Power_Mode");

                                    DC.TestClass[TestCnt].SMU_DC_Setting.Sleep_ms = GetInt(TestCond, "Sleep (Wait - ms)");
                                    DC.TestClass[TestCnt].SMU_DC_Setting.Ignore_Read = General.CInt2Bool(GetInt(TestCond, "Misc"));   // if set to 1, then will ignore read and will not parse result back
                                    break;
                                default:
                                    RaiseError = true;
                                    General.DisplayError(ClassName, "Unrecognized DC Test Parameter", "Error in Processing DC Test Parameter : " + tmpTestCondition);
                                    break;
                            }
                            #endregion
                            DC_Bias_Flag = true;
                            break;
                        case "DMM":
                            Class_Initialization.DMM = true;
                            DMM.TestClass[TestCnt] = new cDMM.cTestClasses();
                            #region "DMM"
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "DC_VOLTAGE":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    DMM.TestClass[TestCnt].DC_Voltage = new cDMM.cTestClasses.cDC_Voltage();
                                    DMM.TestClass[TestCnt].DC_Voltage.TestNo = TestCnt;
                                    DMM.TestClass[TestCnt].DC_Voltage.Range = GetStr(TestCond, "Range");
                                    DMM.TestClass[TestCnt].DC_Voltage.Resolution = GetStr(TestCond, "Resolution");
                                    break;
                                case "DC_CURRENT":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    DMM.TestClass[TestCnt].DC_Current = new cDMM.cTestClasses.cDC_Current();
                                    DMM.TestClass[TestCnt].DC_Current.TestNo = TestCnt;
                                    DMM.TestClass[TestCnt].DC_Current.Range = GetStr(TestCond, "Range");
                                    DMM.TestClass[TestCnt].DC_Current.Resolution = GetStr(TestCond, "Resolution");
                                    break;
                                default:
                                    RaiseError = true;
                                    General.DisplayError(ClassName, "Unrecognized DMM Test Parameter", "Error in Processing DMM Test Parameter : " + tmpTestCondition);
                                    break;
                            }
                            #endregion
                            break;
                        case "SWITCH":
                            Class_Initialization.Switch = true;
                            SWITCH.TestClass[TestCnt] = new cSwitch.cTestClasses();
                            #region "Switch"
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "OPENCLOSE":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    SWITCH.TestClass[TestCnt].OpenClose = new cSwitch.cTestClasses.cOpenClose();
                                    SWITCH.TestClass[TestCnt].OpenClose.TestNo = TestCnt;
                                    SWITCH.TestClass[TestCnt].OpenClose.inputStr = GetStr(TestCond, "Misc");
                                    break;
                                case "OPEN":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    SWITCH.TestClass[TestCnt].Open = new cSwitch.cTestClasses.cOpen();
                                    SWITCH.TestClass[TestCnt].Open.TestNo = TestCnt;
                                    SWITCH.TestClass[TestCnt].Open.inputStr = GetStr(TestCond, "Misc");
                                    break;
                                case "CLOSE":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    SWITCH.TestClass[TestCnt].Close = new cSwitch.cTestClasses.cClose();
                                    SWITCH.TestClass[TestCnt].Close.TestNo = TestCnt;
                                    SWITCH.TestClass[TestCnt].Close.inputStr = GetStr(TestCond, "Misc");
                                    break;
                                default:
                                    RaiseError = true;
                                    General.DisplayError(ClassName, "Unrecognized Switch Test Parameter", "Error in Processing Switch Test Parameter : " + tmpTestCondition);
                                    break;
                            }
                            #endregion
                            break;
                        case "MM":
                            Class_Initialization.MM = true;
                            #region "MM"
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "NF_GAIN":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    MM.TestClass[TestCnt].NF_Gain = new cMM.cTestClasses.cNF_Gain();
                                    MM.TestClass[TestCnt].NF_Gain.TestNo = TestCnt;
                                    MM.TestClass[TestCnt].NF_Gain.setHeader = GetStr(TestCond, "Parameter Header")
                                                                                + GetStr(TestCond, "Target_Freq");
                                    MM.TestClass[TestCnt].NF_Gain.Frequency = GetDbl(TestCond, "Target_Freq");
                                    MM.TestClass[TestCnt].NF_Gain.Average_Data_Count = GetInt(TestCond, "Average_Count");
                                    MM.TestClass[TestCnt].NF_Gain.Loss_Input = GetDbl(TestCond, "Loss_Input");
                                    MM.TestClass[TestCnt].NF_Gain.Loss_Output = GetDbl(TestCond, "Loss_Output");
                                    MM.TestClass[TestCnt].NF_Gain.NF_Offset = GetDbl(TestCond, "NF_Offset");
                                    MM.TestClass[TestCnt].NF_Gain.Gain_Offset = GetDbl(TestCond, "Gain_Offset");
                                    MM.TestClass[TestCnt].NF_Gain.StateFile = GetStr(TestCond, "State_File");
                                    break;
                            }
                            #endregion
                            break;
                        case "COMMON":
                            Class_Initialization.COMMON = true;
                            COMMON.TestClass[TestCnt] = new cCommon.cTestClasses();
                            #region "Common"
                            switch (tmpTestCondition.ToUpper())
                            {

                                case "DELTA":
                                    string _tempFixNumber;
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    COMMON.TestClass[TestCnt].Delta = new cCommon.cTestClasses.cDelta();
                                    COMMON.TestClass[TestCnt].Delta.TestNo = TestCnt;
                                    COMMON.TestClass[TestCnt].Delta.setHeader = GetStr(TestCond, "Parameter Header") + "_" + GetStr(TestCond, "Band");
                                    COMMON.TestClass[TestCnt].Delta.Previous_Info = GetStr(TestCond, "Use_Previous");
                                    COMMON.TestClass[TestCnt].Delta.b_Absolute = General.CStr2Bool(GetStr(TestCond, "Absolute Value"));
                                    _tempFixNumber = COMMON.TestClass[TestCnt].Delta.Fix_Number = GetStr(TestCond, "Fix_Number");
                                    string[] tmp_Info;
                                    string tmp_info2;
                                    if (_tempFixNumber.ToUpper() != "V")
                                    {
                                        tmp_Info = COMMON.TestClass[TestCnt].Delta.Previous_Info.Split(',');
                                        if (tmp_Info.Length == 2)
                                        {
                                            //CheeOn: Changed Name "Delta"
                                            Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) + 2;
                                            Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) + 2;
                                            COMMON.TestClass[TestCnt].Delta.setHeader = GetStr(TestCond, "Parameter Header") + "_" + GetStr(TestCond, "Band");
                                            //+ cExtract.Get_Data(TestCondition, Previous_Test_1, Test_Parameters["Parameter Header"]);
                                            //+ "_" + cExtract.Get_Data(TestCondition, Previous_Test_2, Test_Parameters["Parameter Header"]);

                                        }
                                    }
                                    else
                                    {
                                        tmp_info2 = COMMON.TestClass[TestCnt].Delta.Previous_Info;

                                        //CheeOn: Changed Name "Delta"
                                        Previous_Test_1 = Convert.ToInt32(tmp_info2) + 2;
                                        COMMON.TestClass[TestCnt].Delta.setHeader = GetStr(TestCond, "Parameter Header") + "_" + GetStr(TestCond, "Band");

                                    }
                                    break;

                                case "SUM":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    COMMON.TestClass[TestCnt].Sum = new cCommon.cTestClasses.cSum();
                                    COMMON.TestClass[TestCnt].Sum.TestNo = TestCnt;
                                    COMMON.TestClass[TestCnt].Sum.setHeader = GetStr(TestCond, "Parameter Header");
                                    COMMON.TestClass[TestCnt].Sum.Previous_Info = GetStr(TestCond, "Use_Previous");
                                    COMMON.TestClass[TestCnt].Sum.b_Absolute = General.CStr2Bool(GetStr(TestCond, "Absolute Value"));

                                    tmp_Info = COMMON.TestClass[TestCnt].Sum.Previous_Info.Split(',');
                                    if (tmp_Info.Length == 2)
                                    {
                                        //CheeOn: Changed Name "Delta"
                                        Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) + 2;
                                        Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) + 2;
                                        COMMON.TestClass[TestCnt].Sum.setHeader = GetStr(TestCond, "Parameter Header");
                                        //+ cExtract.Get_Data(TestCondition, Previous_Test_1, Test_Parameters["Parameter Header"]);
                                        //+ "_" + cExtract.Get_Data(TestCondition, Previous_Test_2, Test_Parameters["Parameter Header"]);

                                    }

                                    break;
                                case "RELATIVE_GAIN":
                                    TestCondition_Test[TestCnt] = tmpTestCondition.ToUpper();
                                    COMMON.TestClass[TestCnt].RelativeGain = new cCommon.cTestClasses.cRelativeGainDelta();
                                    COMMON.TestClass[TestCnt].RelativeGain.TestNo = TestCnt;
                                    COMMON.TestClass[TestCnt].RelativeGain.setHeader = GetStr(TestCond, "Parameter Header");
                                    COMMON.TestClass[TestCnt].RelativeGain.Previous_Info = GetStr(TestCond, "Use_Previous");
                                    COMMON.TestClass[TestCnt].RelativeGain.b_Absolute = General.CStr2Bool(GetStr(TestCond, "Absolute Value"));

                                    tmp_Info = COMMON.TestClass[TestCnt].RelativeGain.Previous_Info.Split(',');
                                    if (tmp_Info.Length == 2)
                                    {
                                        //CheeOn: Changed Name
                                        Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) + 2;
                                        COMMON.TestClass[TestCnt].RelativeGain.setHeader = GetStr(TestCond, "Parameter Header")
                                        //+ cExtract.Get_Data(TestCondition, Previous_Test_1, Test_Parameters["Parameter Header"]);
                                        + GetStr(DicTestCondTempNA[Previous_Test_1], "Parameter Header");
                                        //+ "_" + GetStr(TestCond, "Power_Mode"]);
                                    }


                                    break;
                            }
                            #endregion
                            break;
                        case "X":
                            //Do nothing
                            break;

                        default:
                            RaiseError = true;
                            General.DisplayError(ClassName, "Unrecognized Test Mode", "Error in Processing Test Mode : " + tmpTestMode);
                            break;
                    }

                }   //end of foreach

                catch (Exception)
                {
                    RaiseError = true;
                    General.DisplayError(ClassName, "Unrecognized Test Parameter", "Error in Processing Test Parameter : " + tmpTestCondition);
                    //throw;
                }
                #endregion

                // Setting Up Data Triggering and Grabbing Mechanism
                #region "Data Triggering and Grabbing"
                switch (TestCondition_Test[TestCnt])
                {
                    case "TRIGGER":
                    case "TRIGGER2":
                        if (TestCnt > 1)
                        {
                            if (!(TestCondition_Test[TestCnt - 1].Contains("TRIGGER")))
                            {

                                DataTrigger_No++;
                                Array.Resize(ref DataTriggered, DataTrigger_No + 1);
                                DataTriggered[DataTrigger_No] = new cFBAR.s_SParam_Grab();
                                //Modified by KCC
                                //DataTriggered[DataTrigger_No].SParam_Grab = new bool[24];
                                DataTriggered[DataTrigger_No].SParam_Grab = new bool[28];
                                //
                            }
                        }
                        break;
                }

                tmpStr = GetStr(TestCond, "S-Parameter").ToUpper();
                if (tmpStr != "")
                {
                    SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                    DataTriggered[DataTrigger_No].SParam_Grab[SParamDef.GetHashCode()] = true;
                }

                tmpStr = GetStr(TestCond, "S-Parameter_2").ToUpper();
                if (tmpStr != "")
                {
                    SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                    DataTriggered[DataTrigger_No].SParam_Grab[SParamDef.GetHashCode()] = true;
                }
                #endregion

                // Test Senarios for MultiThreading Procedure
                #region "Multi Threading Procedure"
                if (Test_Parameters.ContainsKey("Threading"))
                {
                    tmpStr = GetStr(TestCond, "Test Parameter");
                    if ((threadingStr != tmpStr) && (tmpStr != ""))
                    {
                        if (threadingStr != "")
                        {
                            MultiTest_StepSenario.Add(MultiTest_Senarios, TestCnt);
                            MultiTest_Senarios++;
                            //b_Threading = true;
                        }
                        threadingStr = tmpStr;
                        if (tmpPreviousStr != "")
                        {
                            if (!b_Threading_UsePrevious)
                            {
                                b_Threading_UsePrevious = true;
                                MultiTest_Senarios++;
                            }
                        }
                        else
                        {
                            b_Threading_UsePrevious = false;
                        }
                    }
                }
                else
                {
                    switch (tmpTestMode)
                    {
                        case "FBAR":
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "TRIGGER":
                                case "TRIGGER2":
                                    MultiTest_Senarios++;
                                    b_Threading = false;
                                    break;
                                default:
                                    if (!b_Threading)
                                    {
                                        b_Threading = true;
                                        MultiTest_Senarios++;
                                    }
                                    if (tmpPreviousStr != "")
                                    {
                                        if (!b_Threading_UsePrevious)
                                        {
                                            b_Threading_UsePrevious = true;
                                            MultiTest_Senarios++;
                                        }
                                    }
                                    else
                                    {
                                        b_Threading_UsePrevious = false;
                                    }
                                    break;
                            }
                            break;
                        case "DC":
                            if (tmpPreviousStr != "")
                            {
                                if (!b_Threading_UsePrevious)
                                {
                                    b_Threading_UsePrevious = true;
                                    MultiTest_Senarios++;
                                }
                            }
                            else
                            {
                                b_Threading_UsePrevious = false;
                                MultiTest_Senarios++;
                            }
                            break;
                        case "DMM":
                            if (tmpPreviousStr != "")
                            {
                                if (!b_Threading_UsePrevious)
                                {
                                    b_Threading_UsePrevious = true;
                                    MultiTest_Senarios++;
                                }
                            }
                            else
                            {
                                b_Threading_UsePrevious = false;
                                MultiTest_Senarios++;
                            }
                            break;
                        case "SWITCH":
                            if (tmpPreviousStr != "")
                            {
                                if (!b_Threading_UsePrevious)
                                {
                                    b_Threading_UsePrevious = true;
                                    MultiTest_Senarios++;
                                }
                            }
                            else
                            {
                                b_Threading_UsePrevious = false;
                                MultiTest_Senarios++;
                            }
                            break;
                        case "MM":
                            if (tmpPreviousStr != "")
                            {
                                if (!b_Threading_UsePrevious)
                                {
                                    b_Threading_UsePrevious = true;
                                    MultiTest_Senarios++;
                                }
                            }
                            else
                            {
                                b_Threading_UsePrevious = false;
                                MultiTest_Senarios++;
                            }
                            break;
                        case "COMMON":
                            switch (tmpTestCondition.ToUpper())
                            {
                                case "DELTA":
                                    MultiTest_Senarios++;
                                    b_Threading = false;
                                    break;
                                default:
                                    if (!b_Threading)
                                    {
                                        b_Threading = true;
                                        MultiTest_Senarios++;
                                    }
                                    if (tmpPreviousStr != "")
                                    {
                                        if (!b_Threading_UsePrevious)
                                        {
                                            b_Threading_UsePrevious = true;
                                            MultiTest_Senarios++;
                                        }
                                    }
                                    else
                                    {
                                        b_Threading_UsePrevious = false;
                                    }
                                    break;
                            }
                            break;
                    }
                }
                #endregion

                TestCnt++;
                Console.WriteLine(TestCnt.ToString());
           
                // Setting up Triggering for FBAR
                FBAR.parse_SParamGrab = DataTriggered;
                Get_Results();
            }
        }
        public string GetStr(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                return dic[theKey];
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

        public string GetStr_Zero(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                if (dic[theKey] == "") return "0";
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

        public int GetInt(Dictionary<string, string> dic, string theKey)
        {
            string valStr = "";
            try
            {
                valStr = dic[theKey];
                if (valStr.ToUpper() == "X") return 0;
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            int valInt = 0;
            try
            {
                valInt = Convert.ToInt16(valStr);
            }
            catch
            {
                MessageBox.Show("Test Condition File contains non-number \"" + valStr + "\" in column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            return valInt;
        }

        public double GetDbl(Dictionary<string, string> dic, string theKey)
        {
            string valStr = "";
            try
            {
                valStr = dic[theKey];
                if (valStr.ToUpper() == "X") return 0;
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            double valDbl = 0;
            try
            {
                valDbl = Convert.ToDouble(valStr);
            }
            catch
            {
                MessageBox.Show("Test Condition File contains non-number \"" + valStr + "\" in column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            return valDbl;
        }

        public void Initialize_TestCondition(int iTest)
        {
            switch (TestCondition_TestMode[iTest])
            {
                case "FBAR":
                    switch (TestCondition_Test[iTest])
                    {
                        case "TRIGGER":
                            FBAR.TestClass[iTest].Trigger.InitSettings();
                            break;
                        case "TRIGGER2":
                            FBAR.TestClass[iTest].Trigger2.InitSettings();
                            break;
                        case "MAG_AT":
                            FBAR.TestClass[iTest].Mag_At.InitSettings();
                            break;
                        case "MAG_AT_LIN":
                            FBAR.TestClass[iTest].Mag_At_Lin.InitSettings();
                            break;
                        case "REAL_AT":
                            FBAR.TestClass[iTest].Real_At.InitSettings();
                            break;
                        case "IMAG_AT":
                            FBAR.TestClass[iTest].Imag_At.InitSettings();
                            break;
                        case "PHASE_AT":
                            FBAR.TestClass[iTest].Phase_At.InitSettings();
                            break;
                        case "FREQ_AT":
                            FBAR.TestClass[iTest].Freq_At.InitSettings();
                            break;
                        case "MAG_BETWEEN":
                            FBAR.TestClass[iTest].Mag_Between.InitSettings();
                            break;
                        case "CPL_BETWEEN":
                            FBAR.TestClass[iTest].CPL_Between.InitSettings();
                            break;
                        case "BALANCE":
                            FBAR.TestClass[iTest].Balance.InitSettings();
                            break;
                        case "RIPPLE_BETWEEN":
                            FBAR.TestClass[iTest].Ripple_Between.InitSettings();
                            break;
                        case "CHANNEL_AVERAGING":
                            FBAR.TestClass[iTest].Channel_Averaging.InitSettings();
                            break;
                    }
                    break;
                case "DC":
                    switch (TestCondition_Test[iTest])
                    {
                        case "DC_SETTINGS":
                        case "DC_SETTING":
                            DC.TestClass[iTest].SMU_DC_Setting.InitSettings_Pxi();
                            break;
                    }
                    break;
                case "DMM":
                    switch (TestCondition_Test[iTest])
                    {
                        case "DC_Voltage":
                            DMM.TestClass[iTest].DC_Voltage.InitSettings();
                            break;
                        case "DC_Current":
                            DMM.TestClass[iTest].DC_Voltage.InitSettings();
                            break;
                    }
                    break;
                case "SWITCH":
                    switch (TestCondition_Test[iTest])
                    {
                        case "OPENCLOSE":
                            SWITCH.TestClass[iTest].OpenClose.InitSettings();
                            break;
                        case "OPEN":
                            SWITCH.TestClass[iTest].Open.InitSettings();
                            break;
                        case "CLOSE":
                            SWITCH.TestClass[iTest].Close.InitSettings();
                            break;
                    }
                    break;
                case "COMMON":
                    switch (TestCondition_Test[iTest])
                    {
                        case "DELTA":
                            COMMON.TestClass[iTest].Delta.InitSettings();
                            break;
                        case "SUM":
                            COMMON.TestClass[iTest].Sum.InitSettings();
                            break;
                        case "RELATIVE_GAIN":
                            COMMON.TestClass[iTest].RelativeGain.InitSettings();
                            break;
                    }
                    break;
                case "X":
                    //Do nothing
                    break;
            }

        }

        public void Initialize_MultiThreadingProcedure()
        {
            int Senario_Cnt;
            bool b_UsePrevious;

            Senario_Cnt = 0;
            b_UsePrevious = false;

            TestSenario = new s_TestSenario[MultiTest_Senarios];

            TestSenario[0].Multithread = true;
            TestSenario[0].Start_Items = 0;
            TestSenario[0].Stop_Items = 1;
            TestSenario[0].Items = 1;
            if (MultiTest_Auto_Flag)
            {
                for (int iTest = 0; iTest < TotalTest; iTest++)
                {
                    switch (TestCondition_Test[iTest])
                    {
                        case "TRIGGER":
                        case "TRIGGER2":
                            if ((Senario_Cnt > 0) && b_UsePrevious == false) Senario_Cnt++;
                            TestSenario[Senario_Cnt].Multithread = false;
                            TestSenario[Senario_Cnt].Start_Items = iTest;
                            TestSenario[Senario_Cnt].Stop_Items = iTest + 1;
                            TestSenario[Senario_Cnt].Items = 1;
                            Senario_Cnt++;
                            TestSenario[Senario_Cnt].Multithread = true;
                            TestSenario[Senario_Cnt].Start_Items = iTest + 1;
                            b_UsePrevious = false;
                            break;
                        default:
                            if (UsePrevious_Settings[iTest] > 0)
                            {
                                if (!b_UsePrevious)
                                {
                                    b_UsePrevious = true;
                                    if (Senario_Cnt < MultiTest_Senarios)
                                    {
                                        Senario_Cnt++;
                                        TestSenario[Senario_Cnt].Multithread = true;
                                        TestSenario[Senario_Cnt].Start_Items = iTest;
                                    }
                                }
                            }
                            else
                            {
                                if (b_UsePrevious)
                                {
                                    Senario_Cnt++;
                                    TestSenario[Senario_Cnt].Multithread = true;
                                    TestSenario[Senario_Cnt].Start_Items = iTest;
                                }
                                b_UsePrevious = false;
                            }
                            TestSenario[Senario_Cnt].Stop_Items = iTest + 1;
                            TestSenario[Senario_Cnt].Items = (iTest - TestSenario[Senario_Cnt].Start_Items) + 1;
                            break;
                    }
                }
            }
            else
            {
                for (int iTest = 0; iTest < TotalTest; iTest++)
                {
                    if (MultiTest_StepSenario[Senario_Cnt] == iTest)
                    {
                        Senario_Cnt++;
                        TestSenario[Senario_Cnt].Multithread = true;
                        TestSenario[Senario_Cnt].Start_Items = iTest + 1;
                    }
                    switch (TestCondition_Test[iTest])
                    {
                        case "TRIGGER":
                        case "TRIGGER2":
                            if ((Senario_Cnt > 1) && b_UsePrevious == false) Senario_Cnt++;
                            TestSenario[Senario_Cnt].Multithread = false;
                            TestSenario[Senario_Cnt].Start_Items = iTest;
                            Senario_Cnt++;
                            TestSenario[Senario_Cnt].Multithread = true;
                            TestSenario[Senario_Cnt].Start_Items = iTest + 1;
                            b_UsePrevious = false;
                            break;
                        default:
                            if (UsePrevious_Settings[iTest] > 0)
                            {
                                if (!b_UsePrevious)
                                {
                                    b_UsePrevious = true;
                                    if (Senario_Cnt < MultiTest_Senarios)
                                    {
                                        Senario_Cnt++;
                                        TestSenario[Senario_Cnt].Multithread = true;
                                        TestSenario[Senario_Cnt].Start_Items = iTest;
                                    }
                                }
                            }
                            else
                            {
                                if (b_UsePrevious)
                                {
                                    Senario_Cnt++;
                                    TestSenario[Senario_Cnt].Multithread = true;
                                    TestSenario[Senario_Cnt].Start_Items = iTest;
                                }
                                b_UsePrevious = false;
                            }
                            TestSenario[Senario_Cnt].Stop_Items = iTest;
                            TestSenario[Senario_Cnt].Items = (iTest - TestSenario[Senario_Cnt].Start_Items) + 1;
                            break;
                    }
                }
            }
        }


        #region "FBAR Calibration Portion"
        /// <summary>
        /// Calibration Procedure
        /// </summary>
        /// 
        //Parsing setting for Calibration.
        public void Initialization_Calibration(string Mode)
        {
            RaiseError = false;
            LoadExcel(TCF_FileName);

            if (Mode.ToUpper() == "FBAR")
            {
                FBAR.InitEq(ENA_Address);
                Load_Calibration();
                Initialize_FBAR();

            }
            CloseExcel();
        }

        public void ThruResponseCal()
        {
            FBAR.ThruReponseCal();
        }

        public int CalibrateMeasurePathWithNA(string band, Operation operation, CableCal.OnboardAtten onboardAttenDb, string AntPortName)
        {
            SwitchMatrix.PortCombo harmonicPorts = SwitchMatrix.Maps.Activate(band, operation);

            if (PathAlreadyCalibrated(harmonicPorts, band, operation))
                return 0;

            if (!CableCal.runCableCal) return 0;

            General.DisplayMessage(ClassName, "Cable Calibration - Harmonic Measurement", "Connect " + harmonicPorts.dutPort + " (ANT_" + AntPortName + ") cable to ENA Port 1 cable.\n\n Connect ENA Port 2 cable to the power sensor port of the switchmatrix box. \n\n Click OK when finished.");

            //ENA.defineTracex(1, 1, "S21");
            FBAR.defineTracex(1, 1, e_SParametersDef.S21);

            //List<double> freqData = ENA.ReadFreqList(1);
            //double[] traceData = ENA.ReadENATrace(1, 1);
            
            List<double> freqData = FBAR.GetFrequencyList(1);
            double[] traceData = FBAR.ReadENATrace(1, 1);

            string strTargetCalDataFile = CableCal.calFileDir + harmonicPorts.instrPort + "_" + harmonicPorts.dutPort.ToString() + ".csv";

            StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile);

            for (int i = 0; i < traceData.Length; i++)
            {
                swCalDataFile.Write(freqData[i] + "," + (traceData[i] - (float)onboardAttenDb) + "\n");
            }

            swCalDataFile.Close();

            return 1;
        }

        private bool PathAlreadyCalibrated(SwitchMatrix.PortCombo ports, string band, Operation operation)
        {
            CableCal.calibratedPathsPerBand[band, operation] = ports;
            CableCal.CalPathIsBandSpecific[operation] = band != null && band != "";

            if (CableCal.allCalibratedPaths.Contains(ports))
            {
                return true;  // if this path was already calibrated, don't calibrate the same path twice
            }
            else
            {
                CableCal.allCalibratedPaths.Add(ports);
                return false;
            }
        }

        public void Load_Calibration()
        {
            int CalPortMethod = 0;
            int CalPortCounter = 0;
            int ChannelNo = 0;
            int RowNo, ColNo;
            int CalProcedure_No;
            int CalKit_No;
            int CalKit_Location;
            bool found;
            string tmpStr;
            string tmpHeader;
            string CalibrationStr = "Calibration Procedure";

            //Initialize_Handler();

            cFBAR.cCalibrationClasses.s_CalibrationTotalPort[] CalTotalPort;
            CalTotalPort = new cFBAR.cCalibrationClasses.s_CalibrationTotalPort[1];

            e_CalibrationType Cal_Type;

            cFBAR.cCalibrationClasses.s_CalibrationProcedure[] Procedure;

            Dictionary<string, int> Cal_Header = new Dictionary<string, int>();

            found = false;
            RowNo = 1;
            ColNo = 2;
            CalProcedure_No = 0;
            Procedure = new cFBAR.cCalibrationClasses.s_CalibrationProcedure[1];

            FBAR.Calibration_Class = new cFBAR.cCalibrationClasses();
          
            do
            {
                tmpStr = cExtract.Get_Data(CalibrationStr, RowNo, 1);

                if (tmpStr.ToUpper() == "MODE")
                {
                    FBAR.Calibration_Class.b_Mode = General.convertAutoStr2Bool(cExtract.Get_Data(CalibrationStr, RowNo, 3));
                }
                else if (tmpStr.ToUpper() == ("#End").ToUpper())
                {
                    found = false;
                }

                if (found)
                {
                    if (CalProcedure_No > 0)
                    {
                        Array.Resize(ref Procedure, CalProcedure_No + 1);
                        Array.Resize(ref CalTotalPort, cFBAR.TotalChannel);
                    }

                    CalKit_Location = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["CALKIT_LocNumber"]);
                    if (CalKit_Location > 0)
                    {
                        Procedure[CalProcedure_No].CKit_LocNum = CalKit_Location;
                        Procedure[CalProcedure_No].CKit_Label = cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["CALKIT_Label"]);
                    }

                    CalKit_No = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Standard_Number"]);
                    if (CalKit_No > 0)
                    {
                        Procedure[CalProcedure_No].CalKit = CalKit_No;
                        Procedure[CalProcedure_No].b_CalKit = true;
                    }
                    Procedure[CalProcedure_No].ChannelNumber = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Channel"]);
                    if (ChannelNo != Procedure[CalProcedure_No].ChannelNumber)
                    {
                        if (ChannelNo == 0)
                        {
                            ChannelNo = Procedure[CalProcedure_No].ChannelNumber;
                        }
                        else
                        {
                            ChannelNo = Procedure[CalProcedure_No].ChannelNumber;
                            //FBAR.Calibration_Class.iPortMethod[ChannelNo - 1] = CalPortMethod;
                            CalPortMethod = 0;
                            CalPortCounter = 0;
                        }
                    }
                    Procedure[CalProcedure_No].Message = cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Message_Remarks"]);
                   
                    Cal_Type = (e_CalibrationType)Enum.Parse(typeof(e_CalibrationType), cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Calibration_Type"]));
                    Procedure[CalProcedure_No].Sleep = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Wait [ms]"]);
                    Procedure[CalProcedure_No].CalType = Cal_Type;
                    Procedure[CalProcedure_No].Switch = cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Switch"]);
                    Procedure[CalProcedure_No].No_Ports = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Number_of_Ports"]);
                    switch (Cal_Type)
                    {
                        case e_CalibrationType.ECAL:

                            Procedure[CalProcedure_No].No_Ports = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Number_of_Ports"]);

                            for (int iPort = 0; iPort < Procedure[CalProcedure_No].No_Ports; iPort++)
                            {
                                switch (iPort)
                                {
                                    case 0:
                                        Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_1"]);
                                        break;
                                    case 1:
                                        Procedure[CalProcedure_No].Port_2 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_2"]);
                                        break;
                                    case 2:
                                        Procedure[CalProcedure_No].Port_3 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_3"]);
                                        break;
                                    case 3:
                                        Procedure[CalProcedure_No].Port_4 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_4"]);
                                        break;
                                }
                            }
                            break;
                        case e_CalibrationType.SUBCLASS:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.TRLLINE:
                        case e_CalibrationType.TRLTHRU:
                        case e_CalibrationType.ISOLATION:
                            Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_1"]);
                            Procedure[CalProcedure_No].Port_2 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_2"]);
                            if (Procedure[CalProcedure_No].No_Ports == 0)
                            {
                                if (Procedure[CalProcedure_No].Port_1 > CalPortMethod)
                                {
                                    CalPortMethod = Procedure[CalProcedure_No].Port_1;
                                    CalTotalPort[ChannelNo - 1].No_Ports = CalPortMethod;
                                    //FBAR.Calibration_Class.iPortMethod[ChannelNo - 1] = CalPortMethod;
                                }
                                if (Procedure[CalProcedure_No].Port_2 > CalPortMethod)
                                {
                                    CalPortMethod = Procedure[CalProcedure_No].Port_2;
                                    CalTotalPort[ChannelNo - 1].No_Ports = CalPortMethod;
                                    //FBAR.Calibration_Class.iPortMethod[ChannelNo - 1] = CalPortMethod;
                                }
                            }
                            else
                            {
                                CalTotalPort[ChannelNo - 1].No_Ports = Procedure[CalProcedure_No].No_Ports;
                            }
                            break;

                        case e_CalibrationType.OPEN:
                            Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_1"]);
                            switch (CalPortCounter)
                            {
                                case 0:
                                    CalTotalPort[ChannelNo - 1].PortNo_1 = Procedure[CalProcedure_No].Port_1;
                                    break;
                                case 1:
                                    CalTotalPort[ChannelNo - 1].PortNo_2 = Procedure[CalProcedure_No].Port_1;
                                    break;
                                case 2:
                                    CalTotalPort[ChannelNo - 1].PortNo_3 = Procedure[CalProcedure_No].Port_1;
                                    break;
                                case 3:
                                    CalTotalPort[ChannelNo - 1].PortNo_4 = Procedure[CalProcedure_No].Port_1;
                                    break;
                            }
                            CalPortCounter++; //Note : Max Counter only 4 port for every channel
                            break;
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.TRLREFLECT:
                            Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port_1"]);
                            break;
                        default:
                            General.DisplayError(ClassName, "Loading Calibration Procedure", "Unable to recognized calibration procedure " + (CalProcedure_No + 1).ToString() + " : " + Cal_Type.ToString());
                            RaiseError_Calibration = true;
                            break;
                    }
                    CalProcedure_No++;
                }
                if (tmpStr.ToUpper() == ("#Start").ToUpper())
                {
                    found = true;
                    do
                    {
                        tmpHeader = cExtract.Get_Data(CalibrationStr, RowNo, ColNo);
                        if (tmpHeader.Trim() != "")
                        {
                            Cal_Header.Add(tmpHeader, ColNo);
                        }
                        ColNo++;
                    } while (tmpHeader.ToUpper() != ("#End").ToUpper());
                }

                RowNo++;
            } while (tmpStr.ToUpper() != ("#End").ToUpper());
            //FBAR.Calibration_Class.iPortMethod[ChannelNo - 1] = CalPortMethod;
            //FBAR.Calibration_Class.iPortMethod = CalPortMethod;
            FBAR.Calibration_Class.parse_Procedure = Procedure;
            FBAR.Calibration_Class.parse_CalTotalPort = CalTotalPort;

        }

        // Running Calibration 
        public void Run_CalibrationProcedure(InstrLib.HandlerS1 yadada)
        {
            yadada.InitHandler(100, "GPIB1::05::INSTR");

            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }

            FBAR.Calibration_Class.Calibrate(yadada);
            if (!FBAR.Calibration_Class.CalKit_FailCheck)
            {
                //FBAR.Save_StateFile();
                //Save_FBAR_State();
            }


        }

        public void Run_CalibrationProcedure()
        {
            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }

            FBAR.Calibration_Class.Calibrate();
            if (!FBAR.Calibration_Class.CalKit_FailCheck)
            {
                //FBAR.Save_StateFile();
                //Save_FBAR_State();
            }
        }
        #endregion


        /*
        public void Load_Calibration()
        {
            int RowNo, ColNo;
            int CalProcedure_No;
            int CalKit_No;
            bool found;
            string tmpStr;
            string tmpHeader;
            string CalibrationStr = "Calibration Procedure";

            e_CalibrationType Cal_Type;

            cFBAR.cCalibrationClasses.s_CalibrationProcedure[] Procedure;

            Dictionary<string, int> Cal_Header = new Dictionary<string, int>();

            found = false;
            RowNo = 1;
            ColNo = 2;
            CalProcedure_No = 0;
            Procedure = new cFBAR.cCalibrationClasses.s_CalibrationProcedure[1];

            FBAR.Calibration_Class = new cFBAR.cCalibrationClasses();
            do
            {
                tmpStr = cExtract.Get_Data(CalibrationStr, RowNo, 1);

                if (tmpStr.ToUpper() == "MODE")
                {
                    FBAR.Calibration_Class.b_Mode = General.convertAutoStr2Bool(cExtract.Get_Data(CalibrationStr, RowNo, 3));
                }
                else if (tmpStr.ToUpper() == ("#End").ToUpper())
                {
                    found = false;
                }

                if (found)
                {
                    if (CalProcedure_No > 0)
                    {
                        Array.Resize(ref Procedure, CalProcedure_No + 1);
                    }
                    CalKit_No = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Standard Number"]);
                    if (CalKit_No > 0)
                    {
                        Procedure[CalProcedure_No].CalKit = CalKit_No;
                        Procedure[CalProcedure_No].b_CalKit = true;
                    }
                    Procedure[CalProcedure_No].ChannelNumber = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Channel"]);
                    Procedure[CalProcedure_No].Message = cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Message Remarks"]);
                    //KCC
                    Procedure[CalProcedure_No].Switch = cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Switch"]);
                    Procedure[CalProcedure_No].No_Ports = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Number of Ports"]);

                    Cal_Type = (e_CalibrationType)Enum.Parse(typeof(e_CalibrationType), cExtract.Get_Data(CalibrationStr, RowNo, Cal_Header["Calibration Type"]));
                    Procedure[CalProcedure_No].CalType = Cal_Type;
                    switch (Cal_Type)
                    {
                        case e_CalibrationType.ECAL:

                            Procedure[CalProcedure_No].No_Ports = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Number of Ports"]);

                            for (int iPort = 0; iPort < Procedure[CalProcedure_No].No_Ports; iPort++)
                            {
                                switch (iPort)
                                {
                                    case 0:
                                        Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 1"]);
                                        break;
                                    case 1:
                                        Procedure[CalProcedure_No].Port_2 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 2"]);
                                        break;
                                    case 2:
                                        Procedure[CalProcedure_No].Port_3 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 3"]);
                                        break;
                                    case 3:
                                        Procedure[CalProcedure_No].Port_4 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 4"]);
                                        break;
                                }
                            }
                            break;
                        case e_CalibrationType.SUBCLASS:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.TRLLINE:
                        case e_CalibrationType.TRLTHRU:
                        case e_CalibrationType.ISOLATION:
                            Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 1"]);
                            Procedure[CalProcedure_No].Port_2 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 2"]);
                            break;
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.TRLREFLECT:
                            Procedure[CalProcedure_No].Port_1 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 1"]);
                            break;
                        default:
                            General.DisplayError(ClassName, "Loading Calibration Procedure", "Unable to recognized calibration procedure " + (CalProcedure_No + 1).ToString() + " : " + Cal_Type.ToString());
                            RaiseError_Calibration = true;
                            break;
                    }
                    CalProcedure_No++;
                }
                if (tmpStr.ToUpper() == ("#Start").ToUpper())
                {
                    found = true;
                    do
                    {
                        tmpHeader = cExtract.Get_Data(CalibrationStr, RowNo, ColNo);
                        if (tmpHeader.Trim() != "")
                        {
                            Cal_Header.Add(tmpHeader, ColNo);
                        }
                        ColNo++;
                    } while (tmpHeader.ToUpper() != ("#End").ToUpper());
                }
                RowNo++;
            } while (tmpStr.ToUpper() != ("#End").ToUpper());
            FBAR.Calibration_Class.parse_Procedure = Procedure;
        }

        public void Run_CalibrationProcedure()
        {         
            

            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }
            FBAR.Calibration_Class.Calibrate();
        }
        */
        public bool Run_Manual_SW(string Sw_Band)
        {
            bool Sw_Test = true;
            //switch (Sw_Band.ToUpper())
            //{
            //    case "1":
            //    case "6":
            //        ºmyLibSwitch.SetPath(ºSwFBAR_B1);
            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B1));
            //        break;
            //    case "2":
            //    case "7":
            //        ºmyLibSwitch.SetPath(ºSwFBAR_B2);
            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B2));
            //        break;
            //    case "3":
            //    case "8":
            //        ºmyLibSwitch.SetPath(ºSwFBAR_B3);
            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B3));
            //        break;
            //    case "4":
            //        ºmyLibSwitch.SetPath(ºSwFBAR_B4);
            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B4));
            //        break;
            //    case "5":
            //        ºmyLibSwitch.SetPath(ºSwFBAR_B34B39);
            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B34B39));
            //        break;
            //    case "X":
            //        Sw_Test = false;
            //        break;
            //}

            SwitchMatrix.Maps.Activate(Sw_Band.ToUpper().Trim(), Operation.ENAtoRFIN);
            SwitchMatrix.Maps.Activate(Sw_Band.ToUpper().Trim(), Operation.ENAtoRFOUT);
            SwitchMatrix.Maps.Activate(Sw_Band.ToUpper().Trim(), Operation.ENAtoRX);

            return Sw_Test;

        }
        public void Run_Tests(SnP_BuddyFileBuilder.SnPFileBuilder ReferenceBuddy)
        {
            if (Class_Initialization.FBAR) FBAR.Clear_Results();
            if (Class_Initialization.DC) DC.Clear_Results();
            if (Class_Initialization.MM) MM.Clear_Results();
            if (Class_Initialization.COMMON) COMMON.Clear_Results();
            //DMM.Clear_Results();
            Stopwatch watch = new Stopwatch();

            switch (ModeSet)
            {
                case 0:
                case 1:
                    FBAR.Clear_TraceData();
                    for (int Test = 0; Test < TotalTest; Test++)
                    {
                        watch.Reset();
                        watch.Start();
                        Test_No_Threading(Test, ReferenceBuddy);
                        watch.Stop();
                        TestTime[Test] = (double)watch.ElapsedTicks;
                    }
                    break;
                case 2:
                    for (int MultiTest_Step = 0; MultiTest_Step < MultiTest_Senarios; MultiTest_Step++)
                    {
                        watch.Reset();
                        watch.Start();
                        Test_Threading(TestSenario[MultiTest_Step].Start_Items, TestSenario[MultiTest_Step].Stop_Items, TestSenario[MultiTest_Step].Items, TestSenario[MultiTest_Step].Multithread);
                        watch.Stop();
                        TestTime[MultiTest_Step] = (double)watch.ElapsedTicks;
                    }
                    break;
            }
            if (DC_Bias_Flag) DC.No_Bias(); //DC Power
            //tmpUnit_No++;
            if (!b_FirstTest) b_FirstTest = true; // Once only
        }

        public void Get_Results()
        {
            //Results = FBAR.Result_setting;
            for (int iRslt = 0; iRslt < TotalTest; iRslt++)
            {
                switch (TestCondition_TestMode[iRslt])
                {
                    case "FBAR":
                        Results[iRslt] = FBAR.Result_setting[iRslt];
                        break;
                    case "DC":
                        Results[iRslt] = DC.Result_setting[iRslt];
                        break;
                    case "DMM":
                        //
                        break;
                    case "SWITCH":
                        //
                        break;
                    case "MM":
                        Results[iRslt] = MM.Result_setting[iRslt];
                        break;
                    case "COMMON":
                        Results[iRslt] = COMMON.Result_setting[iRslt];
                        break;
                    case "X":
                        //Do nothing
                        break;
                }
            }
        }

        public void Test_No_Threading(int Test, SnP_BuddyFileBuilder.SnPFileBuilder SecResFile)
        {
            //Debug purpose:
            //if (Test == 69)
            //{
            //    Debugger.Break();
            //}

            switch (TestCondition_TestMode[Test])
            {
                    case "FBAR":

                    switch (TestCondition_Test[Test])
                    {
                        case "TRIGGER":
                            try
                            {
                                #region MIPI settings

                                HSDIO.Instrument.SendVector(TestCondition_PowerMode[Test].ToUpper().Trim() + TestCondition_Band[Test].ToUpper().Trim());
                                if (TestCondition_MipiDacBit2[Test].Trim() != "") HSDIO.Instrument.SendVector("dacQ2" + TestCondition_MipiDacBit2[Test].ToUpper().Trim());
                                if (TestCondition_MipiDacBit[Test].Trim() != "") HSDIO.Instrument.SendVector("dacQ1" + TestCondition_MipiDacBit[Test].ToUpper().Trim());
                                
                                #endregion MIPI settings

                                #region Switch settings
                                SwitchMatrix.Maps.Activate(TestCondition_Band[Test].ToUpper().Trim(), Operation.ENAtoRFIN);
                                SwitchMatrix.Maps.Activate(TestCondition_Band[Test].ToUpper().Trim(), Operation.ENAtoRFOUT);
                                SwitchMatrix.Maps.Activate(TestCondition_Band[Test].ToUpper().Trim(), Operation.ENAtoRX);
                                #endregion Switch settings
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            if (!b_FirstTest)
                            {
                                Init_SNPFile(Test);
                            }

                            FBAR.TestClass[Test].Trigger.FileOutput_Unit = tmpUnit_No;
                            FBAR.TestClass[Test].Trigger.RunTest();
                            FBAR.TestClass[Test].Trigger.FileOutput_Counting++;
                            FBAR.incr_DataTrigger();
                            break;
                        case "TRIGGER2":
                            FBAR.TestClass[Test].Trigger2.FileOutput_Unit = tmpUnit_No;
                            FBAR.TestClass[Test].Trigger2.RunTest();
                            FBAR.incr_DataTrigger();
                            break;
                        case "MAG_AT":
                            FBAR.TestClass[Test].Mag_At.RunTest();
                            break;
                        case "MAG_AT_LIN":
                            FBAR.TestClass[Test].Mag_At_Lin.RunTest();
                            break;
                        case "REAL_AT":
                            FBAR.TestClass[Test].Real_At.RunTest();
                            break;
                        case "IMAG_AT":
                            FBAR.TestClass[Test].Imag_At.RunTest();
                            break;
                        case "PHASE_AT":
                            FBAR.TestClass[Test].Phase_At.RunTest();
                            break;
                        case "FREQ_AT":
                            FBAR.TestClass[Test].Freq_At.RunTest();
                            FBAR.TestClass[Test].Freq_At.GenerateSecondReportFile(SecResFile, SNPFile.FileOutput_Iteration, SNPFile.FileOuuput_Count);
                            break;
                        case "MAG_BETWEEN":
                            FBAR.TestClass[Test].Mag_Between.RunTest();
                            break;
                        case "CPL_BETWEEN":
                            FBAR.TestClass[Test].CPL_Between.RunTest();
                            break;
                        case "BALANCE":
                            FBAR.TestClass[Test].Balance.RunTest();
                            break;
                        case "RIPPLE_BETWEEN":
                            FBAR.TestClass[Test].Ripple_Between.RunTest();
                            break;
                        case "CHANNEL_AVERAGING":
                            FBAR.TestClass[Test].Channel_Averaging.RunTest();
                            break;
                        default:
                            RaiseError = true;
                            General.DisplayError(ClassName, "Unrecognized FBAR Test to Run", "Error in Running (No Threading) FBAR Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                            break;
                    }
                    Results[Test] = FBAR.Result_setting[Test];
                    break;
                case "DC":
                    switch (TestCondition_Test[Test])
                    {
                        case "DC_SETTINGS":
                        case "DC_SETTING":

                            DC.TestClass[Test].SMU_DC_Setting.RunTest();
                            break;
                        default:
                            RaiseError = true;
                            General.DisplayError(ClassName, "Unrecognized DC Test to Run", "Error in Running (No Threading) DC Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                            break;
                    }
                    Results[Test] = DC.Result_setting[Test];
                    break;
                case "DMM":
                    switch (TestCondition_Test[Test])
                    {
                        case "DC_VOLTAGE":
                            DMM.TestClass[Test].DC_Voltage.RunTest();
                            break;
                        case "DC_CURRENT":
                            DMM.TestClass[Test].DC_Current.RunTest();
                            break;
                        default:
                            RaiseError = true;
                            General.DisplayError(ClassName, "Unrecognized DMM Test to Run", "Error in Running (No Threading) DMM Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                            break;
                    }
                    break;
                case "SWITCH":
                    switch (TestCondition_Test[Test])
                    {
                        case "OPENCLOSE":
                            SWITCH.TestClass[Test].OpenClose.RunTest();
                            break;
                        case "OPEN":
                            SWITCH.TestClass[Test].Open.RunTest();
                            break;
                        case "CLOSE":
                            SWITCH.TestClass[Test].Close.RunTest();
                            break;
                        default:
                            RaiseError = true;
                            General.DisplayError(ClassName, "Unrecognized Switch Test to Run", "Error in Running (No Threading) Switch Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                            break;
                    }
                    break;
                case "COMMON":
                    COMMON.Result_setting = Results;    // Parse in Results
                    switch (TestCondition_Test[Test])
                    {
                        case "DELTA":
                            COMMON.TestClass[Test].Delta.RunTest();
                            break;
                        case "SUM":
                            COMMON.TestClass[Test].Sum.RunTest();
                            break;
                    }
                    switch (TestCondition_Test[Test])
                    {
                        case "RELATIVE_GAIN":
                            COMMON.TestClass[Test].RelativeGain.RunTest();
                            break;
                    }
                    Results[Test] = COMMON.Result_setting[Test];
                    break;

                case "X":
                    //Do nothing
                    break;
            }

        }

        public void Test_Threading(int First_Test, int Last_Test, int Items, bool b_Thread)
        {
            if (RaiseError)
            {
                return;
            }

            int test_item;
            test_item = 0;
            ManualResetEvent[] wHand = new ManualResetEvent[Items];
            for (int Test = First_Test; Test < Last_Test; Test++)
            {
                wHand[test_item] = new ManualResetEvent(false);
                switch (TestCondition_TestMode[Test])
                {

                    case "FBAR":
                        #region "FBAR Condition Test"
                        switch (TestCondition_Test[Test])
                        {
                            case "TRIGGER":
                                FBAR.TestClass[Test].Trigger.FileOutput_Unit = tmpUnit_No;
                                FBAR.TestClass[Test].Trigger._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Trigger.CallBack, test_item);
                                break;
                            case "TRIGGER2":
                                FBAR.TestClass[Test].Trigger2.FileOutput_Unit = tmpUnit_No;
                                FBAR.TestClass[Test].Trigger2._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Trigger2.CallBack, test_item);
                                break;
                            case "MAG_AT":
                                FBAR.TestClass[Test].Mag_At._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Mag_At.CallBack, test_item);
                                break;
                            case "FREQ_AT":
                                FBAR.TestClass[Test].Freq_At._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Freq_At.CallBack, test_item);
                                break;
                            case "MAG_BETWEEN":
                                FBAR.TestClass[Test].Mag_Between._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Mag_Between.CallBack, test_item);
                                break;
                            case "CPL_BETWEEN":
                                FBAR.TestClass[Test].CPL_Between._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].CPL_Between.CallBack, test_item);
                                break;
                            case "RIPPLE_BETWEEN":
                                FBAR.TestClass[Test].Ripple_Between._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(FBAR.TestClass[Test].Ripple_Between.CallBack, test_item);
                                break;
                            default:
                                RaiseError = true;
                                General.DisplayError(ClassName, "Unrecognized FBAR Test to Run", "Error in Running (Threading) FBAR Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                                break;
                        }
                        #endregion
                        break;
                    case "DC":
                        #region "DC Condition Test"
                        switch (TestCondition_Test[Test])
                        {
                            case "DC_SETTINGS":
                            case "DC_SETTING":
                                DC.TestClass[Test].SMU_DC_Setting._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(DC.TestClass[Test].SMU_DC_Setting.CallBack, test_item);
                                break;
                            default:
                                RaiseError = true;
                                General.DisplayError(ClassName, "Unrecognized DC Test to Run", "Error in Running (Threading) DC Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                                break;
                        }
                        #endregion
                        break;
                    case "DMM":
                        #region "DMM Test Condition"
                        switch (TestCondition_Test[Test])
                        {
                            case "DC_VOLTAGE":
                                DMM.TestClass[Test].DC_Voltage._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(DMM.TestClass[Test].DC_Voltage.CallBack, test_item);
                                break;
                            case "DC_CURRENT":
                                DMM.TestClass[Test].DC_Current._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(DMM.TestClass[Test].DC_Current.CallBack, test_item);
                                break;
                            default:
                                RaiseError = true;
                                General.DisplayError(ClassName, "Unrecognized DMM Test to Run", "Error in Running (Threading) DMM Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                                break;
                        }
                        #endregion
                        break;
                    case "SWITCH":
                        #region "Switch"
                        switch (TestCondition_Test[Test])
                        {
                            case "OPENCLOSE":
                                SWITCH.TestClass[Test].OpenClose._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(SWITCH.TestClass[Test].OpenClose.CallBack, test_item);
                                break;
                            case "OPEN":
                                SWITCH.TestClass[Test].Open._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(SWITCH.TestClass[Test].Open.CallBack, test_item);
                                break;
                            case "CLOSE":
                                SWITCH.TestClass[Test].Close._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(SWITCH.TestClass[Test].Close.CallBack, test_item);
                                break;
                            default:
                                RaiseError = true;
                                General.DisplayError(ClassName, "Unrecognized Switch Test to Run", "Error in Running (Threading) Switch Test : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                                break;
                        }
                        #endregion
                        break;
                    case "COMMON":
                        #region "Common"
                        switch (TestCondition_Test[Test])
                        {
                            case "DELTA":
                                COMMON.TestClass[Test].Delta._mre = wHand[test_item];
                                ThreadPool.QueueUserWorkItem(COMMON.TestClass[Test].Delta.CallBack, test_item);
                                break;
                        }
                        #endregion
                        break;
                    default:
                        RaiseError = true;
                        General.DisplayError(ClassName, "Unrecognized Test Mode to Run", "Error Test Mode (Threading) : " + Test.ToString() + " - " + TestCondition_Test[Test]);
                        break;
                }

                test_item++;
            }
            WaitHandle.WaitAll(wHand);
            if ((!b_Thread) && (TestCondition_TestMode[First_Test] == "FBAR"))
            {
                FBAR.incr_DataTrigger();
            }

        }

        //KCC - SNP Header format
        public void Generate_SNP_Header(string FilePath, List<string> Contains)
        {
            int i = 0,
                j = 0;
            ClothoLibStandard.IO_TextFile IO_TXT = new ClothoLibStandard.IO_TextFile();
            IO_TXT.CreateFileInDirectory(FilePath);
            j = Contains.Count();

            for (i = 0; i < j; i++)
            {
                IO_TXT.WriteNewLineToExistTextFile(FilePath, Contains[i]);
            }
        }
        public void Close_SNP_Header(string FilePath, string Contain)
        {
            ClothoLibStandard.IO_TextFile IO_TXT = new ClothoLibStandard.IO_TextFile();
            IO_TXT.WriteNewLineToExistTextFile(FilePath, Contain);
        }

        public void Close_SNP_Header2(string Contain)
        {
            ClothoLibStandard.IO_TextFile IO_TXT = new ClothoLibStandard.IO_TextFile();
            int i;
            int j=0;
            j = TempFolderName.Count; 
            for (i = 0; i < j; i++)
            {
                IO_TXT.WriteNewLineToExistTextFile(TempFolderName[i] + SNPFile.FileOutput_FileName + ".txt", Contain);
            }
        }
        //KCC - Added for Clotho Uninit
        public void UnInit()
        {
            Test_Parameters.Clear();
        }


    }
}
