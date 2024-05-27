using System;
using System.Collections.Generic;
using Avago.ATF.StandardLibrary;
using TestLib;
using TestPlanCommon.CommonModel;

//using TestPlan_Seoraksan1p7.PaProfile;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// TCF Sheet Reader for S-Para.
    /// </summary>
    public class SParaTestConditionReader
    {
        private SParaTestConditionFactory m_do;

        /// <summary>
        /// Main Tab.
        /// </summary>
        public Dictionary<string, string> TCF_Setting;

        public SParaTestConditionFactory DataObject
        {
            get { return m_do; }
        }

        public SParaTestConditionReader()
        {
            m_do = new SParaTestConditionFactory();
        }

        public void FillMainTab()
        {
            TCF_Setting = new Dictionary<string, string>();

            TcfSheetReader MainSheet = new TcfSheetReader("Main", 100, 10);


            // Determine Tester Type.  PA or FBAR
            TCF_Setting.Add("Tester_Type", "FBAR");

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

            TCF_Setting.Add("NA_Model", "");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "NA_Model")
                {
                    TCF_Setting["NA_Model"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    break;
                }
            }

            TCF_Setting.Add("ENA_StatusFileName", "");

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "ENA_StatusFileName")
                {
                    TCF_Setting["ENA_StatusFileName"] = MainSheet.allContents.Item3[Row, 1].ToUpper();
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

            // Check for Sample_Version
            TCF_Setting.Add("Sample_Version", "");
            string SampleVersion = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "Sample_Version")
                {
                    SampleVersion = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    TCF_Setting["Sample_Version"] = SampleVersion;
                    break;
                }
            }

            // Check for GU Cal Show skip button on popup
            TCF_Setting.Add("GU_EngineeringMode", "FALSE");
            string zValue = "";

            for (int Row = 1; Row < 100; Row++)
            {
                if (MainSheet.allContents.Item3[Row, 0] == "GU_EngineeringMode")
                {
                    zValue = MainSheet.allContents.Item3[Row, 1].ToUpper();
                    if (zValue == "TRUE")
                    {
                        TCF_Setting["GU_EngineeringMode"] = "TRUE";
                        Legacy_FbarTest.DataFiles.SNP.Enable = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Fill ENA from the Main sheet.
        /// </summary>
        public void FillEnaState()
        {
            #region ReadExcel for ENA state & Local Setting file

            // Get File_Path from Excel test condition file
            int sheet = 0;

            // search through Excel test condition file for File_Path
            while (sheet < 50)
            {
                try
                {
                    ATFCrossDomainWrapper.Excel_Get_Input(++sheet, 1, 1);
                }
                catch
                {
                    break; // stop searching if sheet doesn't exist
                }

                // Sheet exists. Note: ENA fields are found in the first sheet, 'Main'.
                bool continueRows = true;
                int row = 0;

                while (continueRows & row < 100)
                {
                    string cellValue =
                        ATFCrossDomainWrapper.Excel_Get_Input(sheet, ++row,
                            1); // row and column appear to be safe from exceptions, but sheet can throw exception if doesn't exist

                    switch (cellValue) //.Replace("_", "").ToLower())
                    {
                        case "ENA_StatusFileName":
                            m_do.EnaStateFile = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            break;
                        case "ENA_SnPFile_Enable":
                            m_do.SnpFileIteration = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.EnaStateFileEnable = (m_do.SnpFileIteration.ToUpper() == "TRUE" ? true : false);
                            if(!m_do.EnaStateFileEnable)
                            {
                                int returnnt;
                                if (int.TryParse(m_do.SnpFileIteration, out returnnt))
                                {
                                    m_do.SNPFileOutput_Count = returnnt;
                                    m_do.EnaStateFileEnable = true;
                                    m_do.EnaStateFileEnable = (m_do.SNPFileOutput_Count != 0 ? true : false);
                                }
                                else
                                {

                                    m_do.EnaStateFileEnable = false;
                                }
                            }                            
                            break;
                        case "TraceFile_Enable":
                            m_do.TraceFileMaxCount = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);  //Jerome - 20191203 - To generate Snp separatley from Trace file
                            m_do.TraceFileOutput_Count = Convert.ToInt16(m_do.TraceFileMaxCount);
                            m_do.TraceFileEnable = (m_do.TraceFileMaxCount != "0" ? true : false);
                            if(m_do.TraceFileEnable) m_do.SNPFileOutput_Count = m_do.TraceFileOutput_Count;
                            break;
                        case "TraceFile_Sampling":
                            m_do.SnpFileSampling = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.TraceFileOutput_Count_Sampling = Convert.ToInt16(m_do.SnpFileSampling);
                            m_do.SamplingTraceFileEnable = (m_do.SnpFileSampling != "0" ? true : false);
                            break;
                        case "SnPFile_Sampling":
                            m_do.SnpFileSampling = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.SnpFileOutput_Count_Sampling = Convert.ToInt16(m_do.SnpFileSampling);
                            m_do.SamplingSnpFileEnable = (m_do.SnpFileSampling != "0" ? true : false);
                            m_do.SNPFileOutput_Count = Convert.ToInt16(ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2));
                            break;
                        case "ENA_Cal_Enable":
                            m_do.Cal_Enable = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.ENA_Cal_Enable = (m_do.Cal_Enable.ToUpper() == "TRUE" ? true : false);
                            //   if (Cal_Enable.ToUpper() == "TRUE") { FBAR_Test = false; }
                            break;
                        case "PauseTestOnDuplicateModuleID": //double unit detection
                            string PauseTest = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.PauseTestOnDuplicate = (PauseTest.ToUpper() == "TRUE" ? true : false);
                            break;
                        case "DPAT_Enable": //DPAT Outlier
                            string DPAT = ATFCrossDomainWrapper.Excel_Get_Input(sheet, row, 2);
                            m_do.DPAT_Flag = (DPAT.ToUpper() == "TRUE" ? true : false);
                            break;
                        case "#END":
                        case "#end":
                        case "#End":
                            continueRows = false;
                            break;
                    }
                }

                string LocalSettingFile = "";       // from original code.
                if (m_do.EnaStateFile != "" & LocalSettingFile != "") break;
            }

            #endregion ReadExcel for ENA state & Local Setting file
        }

        public void FillEnaState2(TcfSheetReader rdr)
        {
            m_do.EnaStateFile = rdr.TableVertical["ENA_StatusFileName"];
            m_do.SnpFileIteration = rdr.TableVertical["ENA_SnPFile_Enable"];
            m_do.EnaStateFileEnable = (m_do.SnpFileIteration.ToUpper() == "TRUE" ? true : false);
            m_do.SnpFileMaxCount = rdr.TableVertical["TraceFile_Enable"];
            m_do.TraceFileOutput_Count = Convert.ToInt16(m_do.SnpFileMaxCount);
            m_do.TraceFileEnable = (m_do.SnpFileMaxCount != "0" ? true : false);
            m_do.SnpFileSampling = rdr.TableVertical["TraceFile_Sampling"];
            m_do.TraceFileOutput_Count_Sampling = Convert.ToInt16(m_do.SnpFileSampling);
            m_do.SamplingTraceFileEnable = (m_do.SnpFileSampling != "0" ? true : false);
            m_do.Cal_Enable = rdr.TableVertical["ENA_Cal_Enable"];
            m_do.ENA_Cal_Enable = (m_do.Cal_Enable.ToUpper() == "TRUE" ? true : false);
            //   if (Cal_Enable.ToUpper() == "TRUE") { FBAR_Test = false; }

            // CCT Commented out
            string LocalSettingFile = "";       // from original code.
            //if (m_do.EnaStateFile != "" & LocalSettingFile != "") break;
        }

        public static SortedDictionary<int, string> GetSheetHeaderSequence(Tuple<bool, string, string[,]> NaSheetContents)
        {
            SortedDictionary<int, string> zSortedDictHeaderEntries = new SortedDictionary<int, string>();

            //for every row, get column 0 for the prescribed order number
            //get column 1 for the category name
            //concatenate both with a comma inbetween and stuff in a list
            int mm = 0;
            while (NaSheetContents.Item3[mm, 0].ToUpper() != "#END")
            {
                if (NaSheetContents.Item3[mm, 0].ToUpper() == "#START" ||
                    NaSheetContents.Item3[mm, 0].ToUpper() == "PRESCRIBED_ORDER")
                {
                    //Do nothing
                }
                else
                {
                    int headerIndex = Convert.ToInt32(NaSheetContents.Item3[mm, 0].Trim());
                    string headerName = NaSheetContents.Item3[mm, 1].Trim();
                    zSortedDictHeaderEntries.Add(headerIndex, headerName);
                }

                mm++;
            }

            return zSortedDictHeaderEntries;
        }

        public static SortedDictionary<int, string> GetSheetHeaderSequence2(TcfSheetReader rdr)
        {
            SortedDictionary<int, string> zSortedDictHeaderEntries = new SortedDictionary<int, string>();

            rdr.TableVertical.Remove("Prescribed_Order");
            //for every row, get column 0 for the prescribed order number
            //get column 1 for the category name
            //concatenate both with a comma inbetween and stuff in a list

            foreach (KeyValuePair<string, string> kvp in rdr.TableVertical)
            {
                int headerIndex = Convert.ToInt32(kvp.Value.Trim());
                string headerName = kvp.Key.Trim();
                zSortedDictHeaderEntries.Add(headerIndex, headerName);
            }
            return zSortedDictHeaderEntries;
        }

        /// <summary>
        /// Fill DataObject (MIPI, NA).
        /// </summary>
        /// <param name="NaSheetContents"></param>
        public void FillRow(Tuple<bool, string, string[,]> NaSheetContents)
        {
            bool StartFound = false;
            List<string> Header = new List<string>();
            int currentRow = 0;

            while (true)
            {
                currentRow++;

                string enableCell = NaSheetContents.Item3[currentRow, 0];
                string testModeCell = NaSheetContents.Item3[currentRow, 1];

                if (enableCell.ToUpper() == "#START")
                {
                    StartFound = true;
                    int i = 0;
                    while (true)
                    {
                        string value = NaSheetContents.Item3[currentRow, i];
                        Header.Add(value.Trim());
                        if (value.ToUpper() == "#END") break;
                        i++;
                    }

                    continue;
                }

                if (enableCell.ToUpper() == "#END") break;

                if (!StartFound) continue;

                bool cond1 = (enableCell.Trim().ToUpper() != "X" & testModeCell.Trim() != "");
                if (!cond1) continue;
                Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

                for (int i = 0; i < Header.Count; i++)
                {
                    bool cond2 = Header[i] == "" || Header[i].ToUpper() == "#START" || Header[i].ToUpper() == "#END";
                    if (cond2) continue;
                    string value = NaSheetContents.Item3[currentRow, i];
                    currentTestDict.Add(Header[i], value);
                }

                m_do.DicTestCondTempNA.Add(currentTestDict);
                if (currentTestDict["Test Mode"] == "DC") //This is used by GenVector
                {
                    m_do.DicTestCondMipi.Add(currentTestDict);
                }
            }
        }

        public void FillRow2(TcfSheetReader rdr)
        {
            foreach (Dictionary<string, string> currentTestDict in rdr.testPlan)
            {
                currentTestDict.Remove("#Start");
                currentTestDict.Remove("#End");

                m_do.DicTestCondTempNA.Add(currentTestDict);
                if (currentTestDict["Test Mode"] == "DC")
                {
                    m_do.DicTestCondMipi.Add(currentTestDict);
                }
            }

            //Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

            //for (int row = 0; row < rdr.testPlan.Count; row++)
            //{
            //    Dictionary<string, string>.KeyCollection testNameList = rdr.testPlan[0].Keys;
            //    int col = 0;
            //    foreach (string testName in testNameList)
            //    {
            //        string value = rdr.testPlan[row][testName];
            //        string currentCol = rdr.Header[col];
            //        bool isValidCol = currentCol == "" || currentCol.ToUpper() == "#START" || currentCol.ToUpper() == "#END";
            //        if (isValidCol) continue;
            //        currentTestDict.Add(currentCol, value);
            //        col++;
            //    }
            //    m_do.DicTestCondTempNA.Add(currentTestDict);
            //    if (rdr.Header[row] == "DC")
            //    {
            //        m_do.DicTestCondMipi.Add(currentTestDict);
            //    }
            //    currentTestDict.Clear();
            //}
        }

        private string GetName(string name, string defaultValue, Tuple<bool, string, string[,]> allContents)
        {
            for (int Row = 1; Row < 100; Row++)
            {
                if (allContents.Item3[Row, 0] == name)
                {
                    string result = allContents.Item3[Row, 1].ToUpper();
                    return result;
                }
            }
            return defaultValue;
        }
    }
}