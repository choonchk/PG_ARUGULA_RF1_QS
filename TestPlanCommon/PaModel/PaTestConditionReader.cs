using System;
using System.Collections.Generic;
using EqLib;
using TestLib;
using TestPlanCommon.CommonModel;
using MPAD_TestTimer;
using System.IO;

namespace TestPlanCommon.PaModel
{
    public class PaTestConditionReader
    {
        /// <summary>
        /// Main Tab.
        /// </summary>
        public Dictionary<string, string> TCF_Setting;
        /// <summary>
        /// Condition PA tab.
        /// </summary>
        public List<Dictionary<string, string>> DicTestCondTemp { get; private set; }
        /// <summary>
        /// Condition PA tab.
        /// </summary>
        public TcfSheetReader DcResourceSheet { get; private set; }
        public ClothoLibAlgo.Dictionary.Ordered<string, string[]> DcResourceTempList;
        /// <summary>
        /// Local Settings File
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> DicLocalfile = new Dictionary<string, Dictionary<string, string>>();


        public void FillMainTab()
        {
            #region Get Settings from TCF Main Tab


            TCF_Setting = new Dictionary<string, string>();

            TcfSheetReader MainSheet = new TcfSheetReader("Main", 100, 10);


            // Determine Tester Type.  PA or FBAR
            TCF_Setting.Add("Tester_Type", "PA");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Tester_Type")
                {
                    TCF_Setting["Tester_Type"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }


            // Determine Handler Type. S1,S9 or Turrett
            TCF_Setting.Add("Handler_Type", "S9");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Handler_Type")
                {
                    TCF_Setting["Handler_Type"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }

            }


            // Determine SKU
            TCF_Setting.Add("SKU", "");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Product_Type")
                {
                    TCF_Setting["SKU"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }


            // Determine REV_ID
            TCF_Setting.Add("Rev_ID", "");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Revision_ID")
                {
                    TCF_Setting["Rev_ID"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }

            // Check for Pin sweep traceSave_Spar_Files
            TCF_Setting.Add("Pin_Sweep_Trace_Enable", "FALSE");
            string Value = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Pin_Sweep_Trace")
                {
                    Value = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (Value == "TRUE")
                        TCF_Setting["Pin_Sweep_Trace_Enable"] = "TRUE";
                    break;
                }
            }


            // Check for Save Spar Files 
            TCF_Setting.Add("Save_Spar_Files", "FALSE");
            Value = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "ENA_SnPFile_Enable")
                {
                    Value = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (Value == "TRUE")
                    {
                        TCF_Setting["Save_Spar_Files"] = "TRUE";
                        Legacy_FbarTest.DataFiles.SNP.Enable = true;
                    }
                    break;
                }
            }

            // Check for LNA OTP Revision
            TCF_Setting.Add("LNA_OTP_Revision", "0");
            string Revision = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "LNA_OTP_Revision")
                {
                    Revision = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    OTP_Procedure.LNA_OTP_Revision = Revision;
                    TCF_Setting["LNA_OTP_Revision"] = Revision;
                    break;
                }
            }

            // Check for CMOS DIE TYPE
            TCF_Setting.Add("CMOS_DIE_TYPE", "0");
            string CMOStype = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "CMOS_DIE_TYPE")
                {
                    CMOStype = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    TCF_Setting["CMOS_DIE_TYPE"] = CMOStype;
                    break;
                }
            }

            // Check for ENA State File
            TCF_Setting.Add("ENA_StatusFileName", "MERLIN_REV2_FBAR.STA");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "ENA_StatusFileName")
                {
                    TCF_Setting["ENA_StatusFileName"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }


            // Check for GU Cal Show skip button on popup
            TCF_Setting.Add("GU_EngineeringMode", "FALSE");
            Value = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "GU_EngineeringMode")
                {
                    Value = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (Value == "TRUE")
                    {
                        TCF_Setting["GU_EngineeringMode"] = "TRUE";
                        Legacy_FbarTest.DataFiles.SNP.Enable = true;
                        break;
                    }
                }
            }
            TCF_Setting.Add("SwitchTime_Trace", "FALSE");
            string numOfTrace = "0";javascript:;

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "SwitchTimeTraceFile_Enable")
                {
                    RfOnOffTest.RFOnOffTraceEnable = MainSheet.allContents.Item3[Row, 1].ToUpper();
                }

                if (MainSheet.allContents.Item3[Row, 0] == "TraceFile_Enable")
                {
                    numOfTrace = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    RfOnOffTest.TraceFileNum = Convert.ToInt16(numOfTrace);
                }

            }





            #endregion Get Settings from TCF Main Tab
        }

        /// <summary>
        /// Pinot variant.
        /// </summary>
        public void FillMainTab2()
        {
            TCF_Setting = new Dictionary<string, string>();

            TcfSheetReader MainSheet = new TcfSheetReader("Main", 100, 10);

            FillTcfSetting("Tester_Type", "PA", MainSheet);
            FillTcfSetting("Handler_Type", "MANUAL", MainSheet);    //S1,S9 or Turrett
            FillTcfSetting("SKU", "", "Product_Type", MainSheet);
            FillTcfSetting("Rev_ID", "A4A", "Sample_Version", MainSheet);
            FillTcfSetting("CMOS_DIE_TYPE", "0", MainSheet);
            FillTcfSetting("Sample_Version", "", MainSheet);
            // Check for Pin sweep traceSave_Spar_Files
            FillTcfSettingOnTrue("Pin_Sweep_Trace_Enable", "FALSE", "PinSweepTraceFile_Enable", MainSheet);
            FillTcfSetting("LNA_OTP_Revision", "0", MainSheet);
            OTP_Procedure.LNA_OTP_Revision = TCF_Setting["LNA_OTP_Revision"];
            // Check for GU Cal Show skip button on popup
            FillTcfSetting("GU_EngineeringMode", "TRUE", MainSheet);

            if (TCF_Setting["GU_EngineeringMode"] == "TRUE")
            {
                Legacy_FbarTest.DataFiles.SNP.Enable = true;
            }

            FillTcfSetting("SwitchTimeTraceFile_Enable", "FALSE", MainSheet);
            FillTcfSetting("TraceFile_Enable", "0", MainSheet);
            RfOnOffTest.RFOnOffTraceEnable = TCF_Setting["SwitchTimeTraceFile_Enable"];
            
            string numOfTrace = TCF_Setting["TraceFile_Enable"];
            RfOnOffTest.TraceFileNum = Convert.ToInt16(numOfTrace);
            TimingTestBase.SwitchTimeTraceFile_Enable = (RfOnOffTest.TraceFileNum > 0) ? "TRUE" : "FALSE";
            TCF_Setting.Add("SwitchTimeTraceFile_Enable", "FALSE");
        }

        private void FillTcfSetting(string settingName, string defaultValue, TcfSheetReader mainSheet)
        {
            TCF_Setting.Add(settingName, defaultValue);

            for (int Row = 1; Row < 100; Row++)
            {
                if (mainSheet.allContents.Item3[Row, 0] == settingName)
                {
                    TCF_Setting[settingName] = mainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }
        }

        private void FillQCTestTcfSetting(TcfSheetReader mainSheet)
        {
            bool QCFlag = false;
            for (int Row = 1; Row < 100; Row++)
            {
                if (mainSheet.allContents.Item3[Row, 0].Contains("RFFE_Vectors_QCTest"))
                {
                    TCF_Setting[mainSheet.allContents.Item3[Row, 0]] = mainSheet.allContents.Item3[Row, 1].ToUpper();
                    QCFlag = true;
                }
            }

            if(QCFlag == false)
            {
                LoggingManager.Instance.LogError("QC vector files name missing from TCF Main sheet");
            }
        }

        private void FillTcfSetting(string settingName, string defaultValue, string sheetSettingName, TcfSheetReader mainSheet)
        {
            TCF_Setting.Add(settingName, defaultValue);

            for (int Row = 1; Row < 100; Row++)
            {
                if (mainSheet.allContents.Item3[Row, 0] == sheetSettingName)
                {
                    TCF_Setting[settingName] = mainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }
        }

        private void FillTcfSettingOnTrue(string settingName, string defaultValue, TcfSheetReader mainSheet)
        {
            TCF_Setting.Add(settingName, defaultValue);

            for (int Row = 1; Row < 100; Row++)
            {
                if (mainSheet.allContents.Item3[Row, 0] == settingName)
                {
                    string zValue = mainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (zValue != "TRUE") break;
                    TCF_Setting[settingName] = zValue;
                    break;
                }
            }
        }

        private void FillTcfSettingOnTrue(string settingName, string defaultValue, string sheetSettingName, TcfSheetReader mainSheet)
        {
            TCF_Setting.Add(settingName, defaultValue);

            for (int Row = 1; Row < 100; Row++)
            {
                if (mainSheet.allContents.Item3[Row, 0] == sheetSettingName)
                {
                    string zValue = mainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (zValue != "TRUE") break;
                    TCF_Setting[settingName] = zValue;
                    break;
                }
            }
        }

        public void FillMainTabTester(bool SparaTest = false)
        {
            TCF_Setting = new Dictionary<string, string>();
            TcfSheetReader MainSheet = new TcfSheetReader("Main", 100, 10);
            FillTcfSetting("Tester_Type", "PA", MainSheet);
            FillTcfSetting("Handler_Type", "MANUAL", MainSheet);    //S1,S9 or Turrett
            if (SparaTest)
                FillTcfSetting("GU_EngineeringMode", "TRUE", MainSheet);
        }

        /// <summary>
        /// Pinot variant 2 Pinot PA.
        /// </summary>
        public void FillMainTab3()
        {
            TCF_Setting = new Dictionary<string, string>();

            TcfSheetReader MainSheet = new TcfSheetReader("Main", 100, 10);
            FillTcfSetting("GuBenchDataFile_RelPath", "NA", MainSheet);
            FillTcfSetting("GuCorrTemplate_RelPath", "PA", MainSheet);
            FillTcfSetting("Tester_Type", "PA", MainSheet);
            FillTcfSetting("Handler_Type", "MANUAL", MainSheet);    //S1,S9 or Turrett
            FillTcfSetting("SKU", "", "Product_Type", MainSheet);
            FillTcfSetting("Rev_ID", "A6A", "Revision_ID", MainSheet);            
            // Check for Pin sweep traceSave_Spar_Files
            FillTcfSettingOnTrue("Pin_Sweep_Trace_Enable", "FALSE", MainSheet);
            FillTcfSettingOnTrue("Save_Spar_Files", "FALSE", "ENA_SnPFile_Enable", MainSheet);
            FillTcfSettingOnTrue("Skip_Output_Port_On_Fail", "FALSE", MainSheet);
            FillTcfSettingOnTrue("MORDOR", "FALSE", MainSheet);
            FillTcfSettingOnTrue("DUTYCYCLE_PDM", "FALSE", MainSheet);
            FillTcfSetting("LNA_OTP_Revision", "0", MainSheet);
            OTP_Procedure.LNA_OTP_Revision = TCF_Setting["LNA_OTP_Revision"];
            FillTcfSetting("CMOS_DIE_TYPE", "0", MainSheet);
            FillTcfSettingOnTrue("ENA_StatusFileName", "MERLIN_REV2_FBAR.STA", MainSheet);
            FillTcfSetting("GU_EngineeringMode", "TRUE", MainSheet);

            if (TCF_Setting["GU_EngineeringMode"] == "TRUE")
            {
                Legacy_FbarTest.DataFiles.SNP.Enable = true;
            }
            FillTcfSettingOnTrue("PauseTestOnDuplicateModuleID", "FALSE", MainSheet);

            // Check for GU Cal Show skip button on popup

            FillTcfSetting("SwitchTimeTraceFile_Enable", "FALSE", MainSheet);
            FillTcfSetting("TraceFile_Enable", "0", MainSheet);
            RfOnOffTest.RFOnOffTraceEnable = TCF_Setting["SwitchTimeTraceFile_Enable"];
            string numOfTrace = TCF_Setting["TraceFile_Enable"];
            RfOnOffTest.TraceFileNum = Convert.ToInt16(numOfTrace);

            TimingTestBase.SwitchTimeTraceFile_Enable = TCF_Setting["SwitchTimeTraceFile_Enable"];

            FillTcfSettingOnTrue("SaveTestTime", "FALSE", MainSheet); //"FALSE"
            FillTcfSetting("LocalSettingFile_Path", "", MainSheet);
            FillTcfSetting("Sample_Version", "", MainSheet);
            FillTcfSetting("GuPartNo", "", MainSheet);

            FillTcfSetting("StopOnContinueFail_1A", "0", MainSheet);
            FillTcfSetting("StopOnContinueFail_2A", "0", MainSheet);
            FillTcfSetting("ADD_SUBLOTID", "", MainSheet);
            FillTcfSetting("ADD_DEVICEID", "", MainSheet);
            FillTcfSetting("HandlerInfo", "FALSE", MainSheet);
            FillTcfSetting("EVMCAL", "FALSE", MainSheet);

            FillTcfSetting("2DID_VALIDATION", "FALSE", MainSheet);

            FillTcfSetting("WebQueryValidation", "FALSE", MainSheet);
            FillTcfSetting("WebServerURL", "", MainSheet);
            FillQCTestTcfSetting(MainSheet);
            FillTcfSetting("ForceQCVectorToDigiPatternRegeneration", "FALSE", MainSheet);
            FillTcfSetting("ADJUST_BusIDCapTuningInHex", "", MainSheet);
            FillTcfSetting("MQTT_ENABLE", "FALSE", MainSheet);
        }

        public void FillConditionPaTab()
        {
            TcfSheetReader resourceSheet = new TcfSheetReader("Condition_PA", 10, 100);
            //DcResourceTempList = resourceSheet.GetDcResourceDefinitions();
            TcfSheetReader paSheet = new TcfSheetReader("Condition_PA", 5000, 150);
            DcResourceSheet = resourceSheet;
            DicTestCondTemp = paSheet.testPlan;
        }

        public ClothoLibAlgo.Dictionary.Ordered<string, string[]> GetDcResourceDefinitions()
        {
            ClothoLibAlgo.Dictionary.Ordered<string, string[]> DcResourceTempList = 
                new ClothoLibAlgo.Dictionary.Ordered<string, string[]>();

            for (int col = 0; col < DcResourceSheet.Header.Count; col++)
            {
                string head = DcResourceSheet.Header[col];

                if (head.ToUpper().StartsWith("V."))
                {
                    string dcPinName = head.Replace("V.", "");

                    DcResourceTempList[dcPinName] = new string[Eq.NumSites];

                    for (byte site = 0; site < Eq.NumSites; site++)
                    {
                        DcResourceTempList[dcPinName][site] = 
                            DcResourceSheet.allContents.Item3[DcResourceSheet.headerRow - 1 - site, col].Trim();
                    }
                }
            }

            return DcResourceTempList;
        }

        public bool GenerateDicLocalFile(string dirpath)
        {
            try
            {
                if (!File.Exists(@dirpath))
                {
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                }
                else
                {
                    using (StreamReader reader = File.OpenText(@dirpath))
                    {
                        string line = "";
                        string[] templine;
                        bool IsKeepRead = true;
                        while (IsKeepRead)
                        {

                            if (line.Contains("["))
                            {
                                Dictionary<string, string> DicLocalfileParamName = new Dictionary<string, string>();
                                string GroupName = line.Trim('[', ']');

                                char[] temp = { };
                                line = reader.ReadLine();
                                while (line != null && line != "")
                                {
                                    //DicLocalfileParamName.Add("","");


                                    templine = line.ToString().Split(new char[] { '=' });
                                    temp = line.ToCharArray();
                                    if (temp[0] == '[' && temp[temp.Length - 1] == ']')
                                        break;
                                    if (temp[0] == '\'')
                                    {
                                        line = reader.ReadLine();
                                        continue;
                                    }

                                    if (templine.Length == 2)
                                    {


                                        string v1 = templine[0].ToString().Trim(' ');
                                        string v2 = templine[1].ToString().TrimStart();
                                        try
                                        {
                                            DicLocalfileParamName.Add(v1, v2);
                                            line = reader.ReadLine();
                                        }
                                        catch
                                        {
                                            line = reader.ReadLine();
                                        }

                                        //DicLocalfile
                                        continue;
                                    }
                                    line = reader.ReadLine();
                                }

                                DicLocalfile.Add(GroupName, new Dictionary<string, string>(DicLocalfileParamName));

                                if ((line = reader.ReadLine()) == null)
                                {
                                    IsKeepRead = false;
                                }

                                continue;
                            }

                            if ((line = reader.ReadLine()) == null)
                            {
                                IsKeepRead = false;
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(dirpath + " Cannot Read from the file!");
            }

            return false;
        }
    }
}
