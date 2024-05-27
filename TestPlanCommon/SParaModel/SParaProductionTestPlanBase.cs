using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.Outlier;
using Avago.ATF.Shares;
using Avago.ATF.StandardLibrary;
using EqLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;
using TestLib;
using TestPlanCommon.CommonModel;
using ToBeObsoleted;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// FBAR, SDI, Icc Cal and OCR
    /// </summary>
    public class SParaProductionTestPlanBase
    {
        public int Unit_ID;
        private static bool FirstTest = true;
        private static List<string> FileContains = new List<string>();
        public bool FBAR_Test { get; set; }
        protected string ProductTag { get; set; }

        private DateTime m_dateTimeStart;

        protected static string
            tPVersion = "",
            lotId = "",
            SublotId = "",
            WaferId = "",
            OpId = "",
            HandlerSN = "",
            TesterHostName = "",
            TesterIP = "",
            baseDir = @"C:\Avago.ATF.Common.x64\DataLog\",
            activeDir = @"C:\Avago.ATF.Common.x64\DataLog\ENATRACE\";

        //sdi_inbox_wave = @"C:\Avago.ATF.Common\Results\";

        private string m_currentTestResultFileName;
        private List<string> dutTData = new List<string>(); //For new OQA bin trace saving

        protected TigerTraceFileModel m_modelTiger;

        /// <summary>
        /// Set on SNPFile.FileOutput_Path.
        /// </summary>
        public string ActiveDirectory
        {
            get { return activeDir; }
        }

        public string SNP_Files_Dir
        {
            get { return @"C:\Avago.ATF.Common.x64\SNP\"; }
        }

        public ProductionTestTimeController TestTimeLogController { get; set; }

        public SParaProductionTestPlanBase()
        {
            FBAR_Test = true;
            TestTimeLogController = new ProductionTestTimeController();
            m_modelTiger = new TigerTraceFileModel();
        }

        [Obsolete("ProductTag no longer used.")]
        public void SetProductTag(string productTag)
        {
            ProductTag = productTag;
        }

        public void Reset()
        {
            Unit_ID = 0;
            FirstTest = true;
            tPVersion = "";
            lotId = "";
            SublotId = "";
            WaferId = "";
            OpId = "";
            HandlerSN = "";
            TesterHostName = "";
            TesterIP = "";

            FileContains.Clear();
        }

        /// <summary>
        /// Return CurrentTestResultFileName.
        /// </summary>
        public string SetTestResult(string productName)
        {
            if (FirstTest == true)
            {
                FirstTest = false;
                //Retrieve lot ID#
                tPVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_VER, "1");
                ProductTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, productName);
                lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "LOT-ID");
                SublotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "SUBLOT-ID");
                WaferId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "WAFER-ID");
                OpId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "OPERATOR");
                HandlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
                TesterIP = Avago.ATF.Shares.NetworkHelper.GetStaticIPAddress();
                m_dateTimeStart = DateTime.Now;
                string fn = string.Format("{0}_{1:yyyyMMdd_HHmmss}_IP{2}", ProductTag, m_dateTimeStart, TesterIP);
                m_currentTestResultFileName =
                    ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, fn);
                return m_currentTestResultFileName;
            }

            return String.Empty;
        }

        /// <summary>
        /// Return CurrentTestResultFileName.
        /// </summary>
        public string SetTestResult2()
        {
            if (FirstTest == true)
            {
                FirstTest = false;
                //Retrieve lot ID#
                tPVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_VER, "1");
                ProductTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "AFEM-9096");
                lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "LOT-ID");
                SublotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "SUBLOT-ID");
                WaferId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "WAFER-ID");
                OpId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "OPERATOR");
                HandlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
                TesterHostName = System.Net.Dns.GetHostName();
                TesterIP = Avago.ATF.Shares.NetworkHelper.GetStaticIPAddress();
                m_dateTimeStart = DateTime.Now;

                m_currentTestResultFileName = ATFCrossDomainWrapper.GetStringFromCache(
                    PublishTags.PUBTAG_CUR_RESULT_FILE, string.Format("{0}_{1:yyyyMMdd_HHmmss}_IP{2}",
                        ProductTag, m_dateTimeStart, TesterIP));
                return m_currentTestResultFileName;
            }

            return String.Empty;
        }

        public void DoAtfUnInit(string productTag)
        {
            FileContains.Clear();
        }

        public s_SNPFile DoAtfTest2(string clothoRootDir)
        {
            s_SNPFile result = new s_SNPFile();
            result.FileOutput_FileName = String.Empty;

            if (Unit_ID == 0)
            {
                FileContains.Add("DATE_START=" + m_dateTimeStart.ToString("yyyyMMdd"));
                FileContains.Add("TIME_START=" + m_dateTimeStart.ToString("HHmmss"));
                FileContains.Add("ENTITY=NA");
                FileContains.Add("SOURCE=FBAR");
                FileContains.Add("GROUP=DUPLEXER");
                FileContains.Add("PRODUCT_TAG=" + ProductTag);
                FileContains.Add("LOT_NUMBER=" + lotId);
                FileContains.Add("SUB_LOT_NUMBER=" + SublotId);
                FileContains.Add("WAFER_ID=" + WaferId);
                FileContains.Add("OPERATOR_NAME=" + OpId);
                FileContains.Add("TESTER_NAME=FEM");
                FileContains.Add("TESTER_HOST_NAME=NA");
                FileContains.Add("HANLDER_NAME=" + HandlerSN);
                FileContains.Add("TEST_PLAN_VERSION=" + tPVersion);

                activeDir = baseDir + m_currentTestResultFileName.Replace(".CSV", "") + @"\";
                //LibFbar.SNPFile.FileOutput_FileName = lotId;

                ////Parse information to LibFbar
                //LibFbar.SNPFile.FileOutput_Path = activeDir;

                ////Generate SNP Header file
                //LibFbar.SNPFile.FileOutput_HeaderName = FileContains;
                result.FileOutput_FileName = lotId;
                result.FileOutput_Path = ActiveDirectory;
                result.FileOutput_HeaderName = FileContains;

                //Generate Tiger header:
                m_modelTiger.Initialize(clothoRootDir, SublotId, ProductTag);
            }

            return result;
        }

        public void IncrementUnitId()
        {
            Unit_ID++;
        }

        protected void Zip(ProdLib1Wrapper m_wrapper4, bool w1IsRunning)
        {
            ProgressiveZipDataObject pzDo = new ProgressiveZipDataObject();
            pzDo.IsRunning = w1IsRunning;
            pzDo.ActiveDir = ActiveDirectory; //TODO CC Can refactor.
                m_wrapper4.Wrapper2.CreateTraceFiles(pzDo);
        }

        public void SaveTraceTiger(List<string> trace, int manufacturerId)
        {
            try
            {
                m_modelTiger.Save(trace, Module_ID, manufacturerId, ProductTag, Unit_ID);
            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogErrorTestPlan("Tiger is dead due to => " + ex.ToString());
            }
        }

        public void SaveTraceTiger(List<string> trace, int manufacturerId, int moduleId)
        {
            try
            {
                Module_ID = moduleId;
                m_modelTiger.Save(trace, Module_ID, manufacturerId, ProductTag, Unit_ID);
            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogErrorTestPlan("Tiger is dead due to => " + ex.ToString());
            }
        } //ChoonChin -20191203 - Add module ID for tiger function.

        public void SaveTraceCn(List<string> trace)
        {
            bool isDisabled = trace.Count == 0;
            if (isDisabled) return;

            //For new OQA bin trace saving
            int iMfgID = 0, iCmID = 0, iUnitID = 0, iFlags = 0;

            dutTData.Add("MfgID_" + iMfgID + ",ModuleID_" + iUnitID);
            dutTData.Add("*******************************************************");
            dutTData.AddRange(trace);
            // For both 1-Clotho-vs-1-Single-Site-handler and M-Clotho-vs-1-Multiple-Sites-handler case, ALWAYS mark as "1"
            int siteIdx = 1;
            ATFCrossDomainWrapper.SetTraceData(siteIdx, dutTData);
            dutTData.Clear();
        }
        public void SaveTraceCn(List<string> trace, int MfgID, int ModuleID) //ChoonChin - 20191203 - Add mfg, module id for cntrace function.
        {
            bool isDisabled = trace.Count == 0;
            if (isDisabled) return;

            //For new OQA bin trace saving
            int iMfgID = 0, iCmID = 0, iUnitID = 0, iFlags = 0;
            iMfgID = MfgID;
            iUnitID = ModuleID;

            dutTData.Add("MfgID_" + iMfgID + ",ModuleID_" + iUnitID);
            dutTData.Add("*******************************************************");
            dutTData.AddRange(trace);
            // For both 1-Clotho-vs-1-Single-Site-handler and M-Clotho-vs-1-Multiple-Sites-handler case, ALWAYS mark as "1"
            int siteIdx = 1;
            ATFCrossDomainWrapper.SetTraceData(siteIdx, dutTData);
            dutTData.Clear();
        }

        protected static void WriteTestTimeFile(List<s_Result> result, List<double> resultTime)
        {
            string ttPath = "C:\\Avago.ATF.Common.x64\\Production\\TestTime\\Fbar_Test_Parameters.txt";
            StreamWriter TestTime_Log_fbar = File.CreateText(ttPath);

            for (int Rslt = 0; Rslt < result.Count; Rslt++)
            {
                double ttime1 = -1;
                if (Rslt < resultTime.Count)
                {
                    ttime1 = resultTime[Rslt];
                }

                TestTime_Log_fbar.WriteLine(
                    result[Rslt].Result_Header + " =" + ttime1);
            }

            TestTime_Log_fbar.Close();
        }

        public void SetDPatOutlier(bool DPAT_Flag)
        {
            ATFRTE.Instance.SetOutlierCheckFlag(DPAT_Flag);
            LoggingManager.Instance.LogInfo("DPAT is set to " + DPAT_Flag.ToString());
        }

        /// <summary>
        /// Returns true if OTP_Read_DPAT_Pass_Flag is true. False otherwise.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="Digital_Definitions_Part_Specific"></param>
        /// <returns></returns>
        public bool CheckDPatOutlier(ATFReturnResult results,
            Dictionary<string, string> Digital_Definitions_Part_Specific)
        {
            //DPAT Outlier
            string OutlierFlagParamName = "M_Flag_OutlierBit";
            int OutlierFlagValue = 0; //default is 0
            int Dpatloop = 0;

            if (ATFRTE.Instance.IsOutlierCheckOn)
            {
                int sumVal = 0; //Default to 0

                try
                {
                    if (Avago.ATF.Outlier.LotOutlierRecord.Instance.CheckDUTOutlier(Mfg_ID, Module_ID, out sumVal))
                    {
                        if (sumVal > 0) //only burn DPAT flag if sumval >0
                        {
                            ATFLogControl.Instance.Log(LogLevel.HighLight, LogSource.eTestPlan, string.Format("MfgID={0} + ModuleID={1} is Outlier", Mfg_ID, Module_ID));

                            #region Burn Outlier Flag

                            for (Dpatloop = 0; Dpatloop <= 3; Dpatloop++) //4 tries
                            {
                                //Burn the DUT Outlier flag and sumVal to DUT h/w
                                Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
                                Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.15);
                                Eq.Site[0].HSDIO.RegWrite("2B", "0F");
                                Eq.Site[0].HSDIO.Burn("8", false, 3); //burn (1)000
                                Eq.Site[0].HSDIO.RegWrite("1C", "40");
                                Eq.Site[0].HSDIO.SendVector("VIOOFF");
                                Thread.Sleep(1);
                                Eq.Site[0].HSDIO.SendVector("VIOON");
                                Eq.Site[0].HSDIO.RegWrite("2B", "0F");
                                Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.0001);
                                Thread.Sleep(10);
                                Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                                bool TempRead = OTP_Procedure.OTP_Read_DPAT_Pass_Flag(0);

                                if (TempRead == true)
                                {
                                    OutlierFlagValue = 1;
                                    break;
                                }
                            }

                            if (Dpatloop == 4) //failed to burn
                            {
                                OutlierFlagValue = -1;
                            }

                            #endregion Burn Outlier Flag
                        }
                    }
                    ATFResultBuilder.AddResult(ref results, OutlierFlagParamName, "", OutlierFlagValue);
                    ATFResultBuilder.AddResult(ref results, ATFOutlierConstants.PARATAG_SUMVAL, "", sumVal);
                }
                catch//Outlier table not found but flag is ON, trigger error and stop testing.
                {
                    ATFResultBuilder.AddResult(ref results, OutlierFlagParamName, "", 0);
                    ATFResultBuilder.AddResult(ref results, ATFOutlierConstants.PARATAG_SUMVAL, "", 0);
                    ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, "Outlier check is enable but Outlier table not found!");
                    //programLoadSuccess = false;
                    MessageBox.Show("Outlier check is enable but Outlier table not found! Please contact Engineer for help.");
                }
            }
            else //Flag is OFF, QA and REQ
            {
                Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
                Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
                bool TempRead = OTP_Procedure.OTP_Read_DPAT_Pass_Flag(0);
                if (TempRead == true)
                {
                    OutlierFlagValue = 1;
                    return true;
                }

                ATFResultBuilder.AddResult(ref results, OutlierFlagParamName, "", OutlierFlagValue);
                ATFResultBuilder.AddResult(ref results, ATFOutlierConstants.PARATAG_SUMVAL, "", -1); //-1 is skip outlier
            }

            return false;
        }

        private int Module_ID = 999999;
        private int Mfg_ID = 99999;
        private int DuplicatedModuleID = 0;

        protected int last_unit_id = -1; // Hontect PnP handler double-unit detection

        /// <summary>
        /// Double unit detection. Set ID for module, last unit and Mfg.
        /// </summary>
        public void SetUnitId(List<s_Result> libfbarResult)
        {
            foreach (s_Result iRes in libfbarResult)
            {
                if (iRes.IsHeaderContains("OTP_MODULE_ID"))
                {
                    int ModuleIdNumber = (int)iRes.Result_Data;
                    Module_ID = ModuleIdNumber; //For cntrace table

                    if (Unit_ID == 0) //First unit testing just read added Chee On
                    {
                        last_unit_id = ModuleIdNumber;
                    }
                    else
                    {
                        if (last_unit_id != ModuleIdNumber)
                        {
                            last_unit_id = ModuleIdNumber;
                        }
                    }
                }

                //DPAT Outlier
                if (iRes.IsHeaderContains("MFG_ID"))
                {
                    Mfg_ID = (int)iRes.Result_Data;
                }
            }
        }

        public bool DetectDoubleUnit(bool settingPauseTestOnDuplicateModuleID, List<s_Result> libfbarResult)
        {
            bool isSuccess = true;

            foreach (s_Result iRes in libfbarResult)
            {
                if (!iRes.IsHeaderContains("OTP_MODULE_ID")) continue;

                if (Unit_ID == 0) //First unit testing just read added Chee On
                {
                }
                else       // Not the first unit.
                {
                    int ModuleIdNumber = (int)iRes.Result_Data;
                    if (last_unit_id != ModuleIdNumber)
                    {
                    }
                    else
                    {
                        if (!settingPauseTestOnDuplicateModuleID) continue;

                        if (DuplicatedModuleID < 3) //still within 3 times
                        {
                            DuplicatedModuleID++;
                            string msg = string.Format(
                                "Duplicated Module ID detected, test aborted.{0}Please inspect test socket and then rectify the problem to resume test",
                                Environment.NewLine);
                            LoggingManager.Instance.LogWarningTestPlan(msg);
                            PromptManager.Instance.ShowInfo(msg);
                            //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);
                        }
                        else //Prevent test to run after 2nd warning
                        {
                            string msg =
                                "Duplicated Module ID detected for > 3 times, reload test plan to continue";
                            LoggingManager.Instance.LogError(msg);
                            isSuccess = false;
                        }
                    }
                }
            }

            return isSuccess;
        }


        public void ShowInputGui(string defaultLoadBoardId)
        {
            LoggingManager.Instance.LogInfoTestPlan("Waiting for Production Information...");

            ProductionLib2.ProductionTestInputForm prodInputForm = new ProductionLib2.ProductionTestInputForm();

            string tempLotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
            prodInputForm.LotID = tempLotID;        //Pass LotID to production GUI.

            prodInputForm.productTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "");

            bool isAvailableEquipmentHsdio = Eq.Site[0].HSDIO != null;
            string load_board_id = defaultLoadBoardId;
            if (isAvailableEquipmentHsdio)
            {
                string load_board_id2 = Eq.Site[0].HSDIO.EepromRead();
                if (!load_board_id2.Contains("ÿ"))
                {
                    load_board_id = load_board_id2;
                }
            }

            prodInputForm.LoadBoardID = load_board_id;

            //#if (!DEBUG)
            DialogResult rslt = prodInputForm.ShowDialog();

            bool completedField;
            if (rslt == DialogResult.OK)
            {
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, prodInputForm.OperatorID);
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_LOT_ID, prodInputForm.LotID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, prodInputForm.SublotID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, prodInputForm.LoadBoardID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_CONTACTOR_ID, prodInputForm.ContactorID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_HANDLER_SN, prodInputForm.HandlerID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "NA");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "NA");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_ASSEMBLY_ID, "MFG" + prodInputForm.MfgLotID);
                completedField = true;
            }
            //#endif
            //OTP - (1) Get MSB, LSB and RevID -RON
            //string MSB = "0", LSB = "0", RevID = "61";

            //if (prodInputForm.MfgLotID != "" && prodInputForm.DeviceID != "" && prodInputForm.LotID != "")
            //{
            //    Convert_MfgID_DeviceID_LotID(prodInputForm.MfgLotID, prodInputForm.DeviceID, prodInputForm.LotID, ref RevID, ref MSB, ref LSB);
            //}

            //Lib_Var.ºRevID = RevID;
            //Lib_Var.ºLot_MSB = MSB;
            //Lib_Var.ºLot_LSB = LSB;
        }
    }

    public class TigerTraceFileModel
    {
        private string closingTimeCodeDatabaseFriendly;
        private List<string> TigerHeader;

        public void Initialize(string clothoRootDir, string SublotId, string ProductTag)
        {
            TigerHeader = GenerateTigerHeader(clothoRootDir, SublotId, ProductTag);
        }

        public void Save(List<string> traceContent, int Module_ID, int mfgId,
                    string ProductTag, int Unit_ID)
        {
            bool isDisabled = traceContent.Count == 0;
            if (isDisabled) return;

            List<string> TigerData = new List<string>();
            TigerData.AddRange(TigerHeader);
            TigerData.Add("PID," + Unit_ID);
            TigerData.Add("");
            TigerData.Add("MfgID_" + mfgId + ",ModuleID_" + Module_ID);
            TigerData.Add("*******************************************************");
            TigerData.AddRange(traceContent);

            //Export to csv and zip
            string lot = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");

            if (lot == "")
            {
                lot = "EVAL";
            }

            string StdFolderName = ProductTag + "_" + lot + "_" + closingTimeCodeDatabaseFriendly;
            StdFolderName = StdFolderName.Replace(":", "");
            string TigerFolder = "";
            string TigerFileName = ProductTag + "_" + lot + "_PID" + (Unit_ID + 1); //ChoonChin - 20191204 - Need to plus 1 here so that PID never starts with 0
            TigerFolder = "C:\\Avago.ATF.Common.x64\\DataLog\\" + StdFolderName;

            if (!System.IO.Directory.Exists(TigerFolder))
            {
                System.IO.Directory.CreateDirectory(TigerFolder);
            }

            var TigerTrace = File.CreateText(TigerFolder + "\\" + TigerFileName + ".cntracer");

            foreach (string TigerLine in TigerData)
            {
                TigerTrace.WriteLine(TigerLine);
            }
            TigerTrace.Close();

            //zip
            using (Ionic.Zip.ZipFile tigerzip = new Ionic.Zip.ZipFile())
            {
                tigerzip.AddFile(TigerFolder + "\\" + TigerFileName + ".cntracer", "");
                tigerzip.Save(TigerFolder + "\\" + TigerFileName + ".tiger");
            }
            File.Delete(TigerFolder + "\\" + TigerFileName + ".cntracer");
            LoggingManager.Instance.LogInfo("A TIGER file is saved with PID#" + Unit_ID);
            TigerData.Clear();
        }

        private List<string> GenerateTigerHeader(string clothoRootDir, string SublotId, string ProductTag)
        {
            DateTime datetime = DateTime.Now;
            closingTimeCodeDatabaseFriendly = string.Format("{0:yyyy-MM-dd_HH:mm:ss}", datetime);   // 11-Aug-2014 JJ Low
            string closingTimeCodeHumanFriendly = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", datetime);  // 11-Aug-2014 JJ Low
            string closingTimeCodeGalaxyFriendly = string.Format("{0:yyyy_M_d H:m:s}", datetime);  // 11-Aug-2014 JJ Low
            string contactorID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CONTACTOR_ID, ""); // 11-Aug-2014 JJ Low

            string computerName = System.Environment.MachineName;
            string testPlanVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TP_VER, "");
            string dibID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_DIB_ID, "");
            string handlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
            string lotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
            string opID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "");
            string waferID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "");
            string testPlanName = Path.GetFileNameWithoutExtension(clothoRootDir.TrimEnd('\\'));
            string ipAddress = ATFRTE.Instance.IPAddress;

            //string fileNameRoot = testPlanName + "_" + CorrFinishTimeHumanFriendly;
            List<string> header = new List<string>();

            header.Add("--- Global Info:");
            header.Add("Date," + closingTimeCodeDatabaseFriendly.Replace("_", " "));
            header.Add("SetupTime," + closingTimeCodeDatabaseFriendly.Replace("_", " "));
            header.Add("StartTime," + closingTimeCodeDatabaseFriendly.Replace("_", " "));
            header.Add("FinishTime, ");
            header.Add("ProgramName," + testPlanName + ".cs");
            header.Add("ProgramRevision," + testPlanVersion);
            header.Add("Lot," + lotID);
            header.Add("Sublot," + SublotId);
            header.Add("Wafer," + waferID);
            header.Add("WaferOrientation,NA");
            header.Add("TesterName," + computerName);
            header.Add("TesterType," + computerName);
            header.Add("Product," + ProductTag);
            header.Add("Operator," + opID);
            header.Add("ExecType,Avago.ATF.UIs");
            header.Add("ExecRevision,3.1.0.2");
            header.Add("RtstCode,");
            header.Add("PackageType,NA");
            header.Add("Family,TIGER");
            header.Add("SpecName," + Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TL_FULLPATH, "")));
            header.Add("SpecVersion,1");
            header.Add("FlowID,");
            header.Add("DesignRevision,");
            header.Add("--- Site details:,Head#1");
            header.Add("Testing sites,");
            header.Add("Handler ID," + handlerSN);
            header.Add("Handler type,HandlerHontech7145NI6509SwitchMatrixSmpadV1");
            header.Add("LoadBoardName," + dibID);
            //TigerHeader.Add("ContactorID," + contactorID);
            header.Add("--- Options:,");
            header.Add("UnitsMode,normalized");
            header.Add("--- ConditionName:,");
            header.Add("EMAIL_ADDRESS,");
            header.Add("Translator,");
            header.Add("Wafer_Diameter,");
            header.Add("Facility,");
            header.Add("HostIpAddress," + ipAddress);
            header.Add("Temperature,");
            header.Add("PcbLot,NA");
            header.Add("AssemblyLot,MFG");
            header.Add("VerificationUnit,NA");
            header.Add("FullPackageID,NA");
            header.Add("ContactorID,NA");
            header.Add("InstrumentInfo,NA");
            header.Add("ClothoMode,21");
            header.Add("TCFVer,1");
            header.Add("MfgID,NA");
            header.Add("Misc,NA");
            //TigerHeader.Add("");
            return header;
        }
    }

    public class ProductionTestTimeController
    {
        private string m_dataFilePath;
        private List<string> m_passedPcdList;
        private ProductionTestTimeReportGenerator m_model;
        private TestLineFixedCondition m_fixedCond;
        private ProductionTestTimeDebug m_debugModel;

        /// <summary>
        /// Default is deactivated. After Initialize(), this is set and won't change.
        /// </summary>
        public bool IsDeactivated { get; set; }

        public ProductionTestTimeController()
        {
            IsDeactivated = true;
            m_model = new ProductionTestTimeReportGenerator();
            m_debugModel = new ProductionTestTimeDebug();
            m_debugModel.IsDebugMode = false;
        }

        public void Initialize(TestPlanStateModel modelTpState, string atfConfigFilePath,
            List<Dictionary<string, string>> dicTestCondTempNa)
        {
            string pcd = m_debugModel.GetPcd();
            Initialize(pcd);
            if (IsDeactivated) return;

            Dictionary<string, string> atfConfig = modelTpState.GetAtfConfig(atfConfigFilePath);
            TestLineFixedCondition fc = new TestLineFixedCondition();
            fc.SetContractManufacturer(atfConfig["ATFResultRemoteSharePath"]);
            fc.TesterName = atfConfig["TesterID"];
            fc.PcdPackageName = pcd;
            fc.TestType = "RF2";

            AddFixedCondition(fc);
            AddTest(dicTestCondTempNa);

            StopWatchManager.Instance.IsActivated = true;
        }

        public void Save()
        {
            if (IsDeactivated) return;

            bool isTestPlanPassed = m_debugModel.IsPass();
            if (!isTestPlanPassed) return;

            List<PaStopwatch2> swList = StopWatchManager.Instance.GetList(0);
            Save(swList);
            // Set to false to disable.
            StopWatchManager.Instance.IsActivated = !m_debugModel.IsToDeactivate();
        }

        private void Initialize(string currentPcdName)
        {
            // empty in LiteDriver mode.
            if (String.IsNullOrEmpty(currentPcdName))
            {
                IsDeactivated = true;
                return;
            }

            m_dataFilePath = GetReportFileFullPath("TestTimeLog.txt");
            bool isFileExist = File.Exists(m_dataFilePath);
            if (!isFileExist)
            {
                IsDeactivated = false;
                return;
            }

            m_passedPcdList = ReadSavedPcdList();
            bool isCurrentPassed = m_passedPcdList.Contains(currentPcdName);

            if (isCurrentPassed)
            {
                IsDeactivated = true;     // once set, will no longer activate.
                return;
            }

            IsDeactivated = false;
        }

        private void AddFixedCondition(TestLineFixedCondition fixedc)
        {
            if (IsDeactivated) return;
            m_model.AddFixedCondition(fixedc);
            m_fixedCond = fixedc;
        }

        private void AddTest(
            List<Dictionary<string, string>> DicTestCondTemp)
        {
            if (IsDeactivated) return;
            m_model.AddTest(DicTestCondTemp);
        }

        private void Save(List<PaStopwatch2> swList)
        {
            // If has checked already, skip checking to speed up.
            if (IsDeactivated) return;
            Save(m_fixedCond.PcdPackageName, swList, m_fixedCond.ContractManufacturer);
            // after first successful save, don't do again.
            IsDeactivated = m_debugModel.IsToDeactivate();
        }

        private void Save(string currentPcdName, List<PaStopwatch2> swList,
            string cmName)
        {
            // Create report.
            string reportContent = m_model.CreateReport(swList);
            string destReportFileName = m_model.GetReportFileName();
            string destReportFullPath = GetReportFileFullPath(destReportFileName);

            StreamWriter sw = File.CreateText(destReportFullPath);
            sw.Write(reportContent);
            sw.Flush();
            sw.Close();

            // Transfer to database server.
            CopyToServer(cmName, destReportFullPath);

            // Save to database.
            m_passedPcdList = ReadSavedPcdList();
            bool isExist = m_passedPcdList.Contains(currentPcdName);
            if (!isExist)
            {
                if (!String.IsNullOrEmpty(currentPcdName))
                {
                    m_passedPcdList.Add(currentPcdName);
                }

                StringBuilder sb = new StringBuilder();
                foreach (string pcd in m_passedPcdList)
                {
                    sb.AppendLine(pcd);
                }
                sw = File.CreateText(m_dataFilePath);
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
            }

            string msg2 = String.Format("Test Time dashboard file generated in {0}", destReportFullPath);
            LoggingManager.Instance.LogInfoTestPlan(msg2);
            msg2 = String.Format("Test Time database log generated in {0}", m_dataFilePath);
            LoggingManager.Instance.LogInfoTestPlan(msg2);
        }

        private List<string> ReadSavedPcdList()
        {
            m_passedPcdList = new List<string>();
            bool isFirstTime = !File.Exists(m_dataFilePath);
            if (isFirstTime)
            {
                return m_passedPcdList;
            }

            FileInfo fi1 = new FileInfo(m_dataFilePath);
            StreamReader sr = new StreamReader(fi1.OpenRead());

            while (sr.Peek() >= 0)
            {
                m_passedPcdList.Add(sr.ReadLine());
            }

            sr.Close();

            return m_passedPcdList;
        }

        private bool CopyToServer(string cmName, string sourceFullPath)
        {
            // Default : BRCMPENANG
            string serverIp = "10.12.112.47";
            string userName = @"BRCMLTD\pcdreader";
            string password = "pc6re@6er";

            switch (cmName)
            {
                case "Inari P13":
                    serverIp = "172.16.11.13";
                    userName = "avago_user";
                    password = "@vag0u23r";
                    break;

                case "ASEK":
                    serverIp = "59.7.230.37";
                    userName = "avagoadm";
                    password = "avagoadm0319";
                    break;

                default:
                    cmName = "BRCMPENANG";
                    break;
            }

            Tuple<Boolean, String> result = ZDB.ShareLib.XFer.Push(sourceFullPath, serverIp, userName, password);
            string msg = String.Format("TTD file copied from {0} to {1} server.",
                sourceFullPath, cmName);
            LoggingManager.Instance.LogInfoTestPlan(msg);

            if (!result.Item1)
            {
                msg = String.Format("Failed to copy TTD file from {0} to {1} server.",
                    sourceFullPath, cmName);
                ATFLogControl.Instance.Log(LogLevel.Warn, LogSource.eTestPlan, msg);
            }

            return result.Item1;
        }

        private string GetReportFileFullPath(string fileName)
        {
            string dbPath = "C:\\TEMP";
            if (!Directory.Exists(dbPath))
            {
                dbPath = Path.GetTempPath();
            }

            string fp = Path.Combine(dbPath, fileName);
            return fp;
        }
    }

    public class ProductionTestTimeReportGenerator
    {
        private List<TestLineConditionJokerSPara> m_condList;
        private TestLineFixedCondition m_fixedCond;

        public ProductionTestTimeReportGenerator()
        {
            m_separator = ',';
        }

        public void AddFixedCondition(TestLineFixedCondition fixedc)
        {
            m_fixedCond = fixedc;
        }

        public void AddTest(
            List<Dictionary<string, string>> DicTestCondTemp)
        {
            m_condList = new List<TestLineConditionJokerSPara>();

            for (int testIndex = 0; testIndex < DicTestCondTemp.Count; testIndex++)
            {
                TestLineConditionJokerSPara tlc = FormPaBandInfo(DicTestCondTemp[testIndex]);
                if (tlc == null) continue;
                tlc.LineNumber = testIndex;     // start with 1 not 0.
                m_condList.Add(tlc);
            }
        }

        private TestLineConditionJokerSPara FormPaBandInfo(Dictionary<string, string> tcLine)
        {
            TestLineConditionJokerSPara c = new TestLineConditionJokerSPara();

            string testMode = tcLine["Test Parameter"].Trim().ToUpper();
            c.TestMode = testMode;

            bool isToIgnore = true;

            switch (testMode)
            {
                case "TRIGGER_NF":
                case "TRIGGER":
                    c.Band = tcLine["BAND"];
                    c.PowerMode = tcLine["Power_Mode"];
                    c.SwitchIN = tcLine["Switch_In"];
                    c.SwitchOut = tcLine["Switch_Out"];
                    c.SwitchANT = tcLine["Switch_ANT"];
                    isToIgnore = false;
                    break;

                default:
                    break;
            }

            return isToIgnore ? null : c;
        }

        public string GetReportFileName()
        {
            // Example: 2018-12-19T031143
            string sortableDt = DateTime.Now.ToString("yyyy-MM-ddThhmmss");
            string fn = String.Format("{0}_{1}_{2}.ttd",
                m_fixedCond.PcdPackageName, m_fixedCond.TesterName, sortableDt);
            return fn;
        }

        public string CreateReport(List<PaStopwatch2> swList)
        {
            return CreateReport2(swList);
        }

        private string CreateReport3(List<PaStopwatch2> swList)
        {
            if (swList.Count == 0) return String.Empty;

            // filter stopwatch to get only test. Combine stopwatch and test condition.
            List<PaStopwatch2> testSwList = new List<PaStopwatch2>();

            foreach (PaStopwatch2 sw in swList)
            {
                bool isATest = sw.ParentName == String.Empty && sw.IsHasNameType;
                if (!isATest) continue;
                testSwList.Add(sw);
            }

            // stopwatch count will be more than test count. Due to BurnOtpFlag() after run test.
            string msg = String.Format("Stopwatch count: {0}, Test parameter count: {1}",
                testSwList.Count, m_condList.Count);
            LoggingManager.Instance.LogInfo(msg);

            List<TestLineCollectionJokerSPara> reportLines = new List<TestLineCollectionJokerSPara>();

            bool isValidCount = testSwList.Count >= m_condList.Count;

            if (!isValidCount)
            {
                string errorMsg = String.Format("Error: {0}", msg);
                return errorMsg;
            }

            double sumOfTest = 0;

            for (int i = 0; i < m_condList.Count; i++)
            {
                TestLineCollectionJokerSPara rl = new TestLineCollectionJokerSPara();
                rl.TcfIndex = m_condList[i].LineNumber;
                rl.ElapsedMs = testSwList[i].ElapsedMs;
                rl.TestConditions = m_condList[i];
                reportLines.Add(rl);
                sumOfTest = sumOfTest + testSwList[i].ElapsedMs;
            }

            // Add Total test time
            TestLineCollectionJokerSPara totalTestTimeRl = new TestLineCollectionJokerSPara();
            totalTestTimeRl.TcfIndex = -1;
            totalTestTimeRl.ElapsedMs = sumOfTest;
            totalTestTimeRl.TestConditions = new TestLineConditionJokerSPara();
            totalTestTimeRl.TestConditions.TestMode = "TIME_Total";
            reportLines.Add(totalTestTimeRl);

            foreach (PaStopwatch2 sw in swList)
            {
                bool executionTime = sw.Name == "TIME_DoATFTest";
                if (!executionTime) continue;
                TestLineCollectionJokerSPara rl = new TestLineCollectionJokerSPara();
                rl.TcfIndex = -1;
                rl.ElapsedMs = sw.ElapsedMs - sumOfTest;
                rl.TestConditions = new TestLineConditionJokerSPara();
                rl.TestConditions.TestMode = "TIME_Overhead";
                reportLines.Add(rl);
            }

            StringBuilder sb = new StringBuilder();

            // generate header.
            sb.AppendLine(CreateColumnHeader1());

            List<string> m_fixedColumns = new List<string>();
            // d1 = 30/12/2018   d2 = 30/12/2018 10:08 AM
            string d1 = DateTime.Now.ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-MY"));
            string d2 = DateTime.Now.ToString("g", System.Globalization.CultureInfo.CreateSpecificCulture("en-MY"));

            m_fixedColumns.AddRange(new string[] { d1, d2,
                m_fixedCond.PcdPackageName, m_fixedCond.TestType,
                m_fixedCond.TesterName, m_fixedCond.ContractManufacturer });
            string fixedColumns = String.Join(m_separator.ToString(), m_fixedColumns.ToArray());

            // generate content.
            foreach (TestLineCollectionJokerSPara testConditionLine in reportLines)
            {
                FormLine(sb, fixedColumns, testConditionLine);
            }

            return sb.ToString();
        }

        private char m_separator;

        public char Separator
        {
            get { return m_separator; }
            set
            {
                if (value == '\0') return;        // null character.
                if (Char.IsWhiteSpace(value)) return;
                m_separator = value;
            }
        }

        private string CreateColumnHeader1()
        {
            List<string> col = new List<string>();
            col.AddRange(new string[] { "Date", "DateTime", "PCDPackageName", "TestType" });
            col.AddRange(new string[] { "TesterName", "CM" });
            col.AddRange(new string[] { "TestNumber", "Time_ms", "TestMode", "Band", "PowerMode", "Switch_IN" });
            col.AddRange(new string[] { "Switch_ANT", "Switch_Out" });

            string c1 = String.Join(m_separator.ToString(), col.ToArray());
            return c1;
        }

        private void FormLine(StringBuilder sb, string fixedColumns, TestLineCollectionJokerSPara tlList)
        {
            sb.Append(fixedColumns);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TcfIndex);         // TestNumber
            sb.Append(m_separator, 1);
            string ms = Convert(tlList.ElapsedMs);
            sb.Append(ms);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.TestMode);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.Band);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.PowerMode);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.SwitchIN);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.SwitchANT);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.SwitchOut);

            sb.AppendLine();
        }

        private double GetSumOfTestTime(List<PaStopwatch2> testSwList)
        {
            double sumOfTest = 0;
            for (int i = 0; i < m_condList.Count; i++)
            {
                sumOfTest = sumOfTest + testSwList[i].ElapsedMs;
            }

            return sumOfTest;
        }

        private double GetSumOfTestTime(List<TestLineCollectionJokerSPara> reportLines)
        {
            double sumOfTest = 0;
            foreach (TestLineCollectionJokerSPara l in reportLines)
            {
                sumOfTest = sumOfTest + l.ElapsedMs;
            }

            return sumOfTest;
        }

        private double GetWatch(string watchName, List<PaStopwatch2> swList)
        {
            double timeTf = 0;
            foreach (PaStopwatch2 sw in swList)
            {
                bool isFound = sw.Name == watchName;
                if (isFound) return sw.ElapsedMs;
            }

            return timeTf;
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }

        private TestLineCollectionJokerSPara CreateMetricsReportLine(string name, double timeMs)
        {
            TestLineCollectionJokerSPara metrics = new TestLineCollectionJokerSPara();
            metrics.TcfIndex = -1;
            metrics.ElapsedMs = timeMs;
            metrics.TestConditions = new TestLineConditionJokerSPara();
            metrics.TestConditions.TestMode = name;

            string msg = String.Format("Test Time Dashboard: {0}\t\t{1}", metrics.TestConditions.TestMode, metrics.ElapsedMs);
            LoggingManager.Instance.LogInfo(msg);
            return metrics;
        }

        /// <summary>
        /// Create specifically for Joker RF2 stopwatch collection entries.
        /// </summary>
        /// <param name="swList">TRIGGER test has a children with SaveTrace. TRIGGERNF does not have children.</param>
        /// <returns></returns>
        private string CreateReport2(List<PaStopwatch2> swList)
        {
            if (swList.Count == 0) return String.Empty;

            Stack<TestLineCollectionJokerSPara> rlStack = new Stack<TestLineCollectionJokerSPara>();

            int conditionIndex = 0;
            for (int i = 0; i < swList.Count; i++)
            {
                PaStopwatch2 sw = swList[i];
                bool isATriggerTest = sw.ParentName == String.Empty && sw.IsHasNameType && sw.NameType == "TRIGGER";
                bool isATriggerNfTest = sw.ParentName == String.Empty && sw.IsHasNameType && sw.NameType == "TRIGGERNF";

                if (isATriggerTest || isATriggerNfTest)
                {
                    TestLineCollectionJokerSPara rl = new TestLineCollectionJokerSPara();
                    rl.TcfIndex = m_condList[conditionIndex].LineNumber;
                    rl.ElapsedMs = sw.ElapsedMs;
                    rl.TestConditions = m_condList[conditionIndex];
                    rlStack.Push(rl);
                    conditionIndex++;
                    continue;
                }

                // Have to assume TTSaveTrace belongs under the current test.
                bool isChildOfTriggerTest = sw.Name.EndsWith("TTSaveTrace");
                if (isChildOfTriggerTest)
                {
                    TestLineCollectionJokerSPara rl = rlStack.Pop();
                    double ttSaveTrace = sw.ElapsedMs;
                    double ttTotal = rl.ElapsedMs;
                    double ttActual = ttTotal - ttSaveTrace;
                    rl.ElapsedMs = ttActual;        // where ttActual + ttSaveTrace = ttTotal.
                    string msg2 = String.Format("SaveTrace calculation: {0} + {1} = {2} [{3}]", ttActual, ttSaveTrace,
                        ttTotal, sw.Name);
                    LoggingManager.Instance.LogInfoTestPlan(msg2);
                    rlStack.Push(rl);
                }
            }

            string msg = String.Format("Stopwatch count: {0}, Test parameter count: {1}",
                rlStack.Count, m_condList.Count);
            LoggingManager.Instance.LogInfo(msg);

            bool isValidCount = rlStack.Count >= m_condList.Count;

            if (!isValidCount)
            {
                string errorMsg = String.Format("Error: {0}", msg);
                return errorMsg;
            }

            List<TestLineCollectionJokerSPara> reportLines = rlStack.Reverse().ToList();

            // Add metrics.
            double ttTotal2 = GetWatch("TIME_DoATFTest", swList);
            double ttActual2 = GetSumOfTestTime(reportLines);
            double ttOverhead = ttTotal2 - ttActual2;

            TestLineCollectionJokerSPara metricsLine = CreateMetricsReportLine("TIME_TestOverhead", ttOverhead);
            reportLines.Add(metricsLine);
            metricsLine = CreateMetricsReportLine("TIME_ActualTest", ttActual2);
            reportLines.Add(metricsLine);
            metricsLine = CreateMetricsReportLine("TIME_Total", ttTotal2);
            reportLines.Add(metricsLine);

            StringBuilder sb = new StringBuilder();

            // generate header.
            sb.AppendLine(CreateColumnHeader1());

            List<string> m_fixedColumns = new List<string>();
            // d1 = 30/12/2018   d2 = 30/12/2018 10:08 AM
            string d1 = DateTime.Now.ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-MY"));
            string d2 = DateTime.Now.ToString("g", System.Globalization.CultureInfo.CreateSpecificCulture("en-MY"));

            m_fixedColumns.AddRange(new string[] { d1, d2,
                m_fixedCond.PcdPackageName, m_fixedCond.TestType,
                m_fixedCond.TesterName, m_fixedCond.ContractManufacturer });
            string fixedColumns = String.Join(m_separator.ToString(), m_fixedColumns.ToArray());

            // generate content.
            foreach (TestLineCollectionJokerSPara testConditionLine in reportLines)
            {
                FormLine(sb, fixedColumns, testConditionLine);
            }

            return sb.ToString();
        }
    }

    public class ProductionTestTimeDebug
    {
        public bool IsDebugMode { get; set; }
        private int m_pcdCounter;

        public ProductionTestTimeDebug()
        {
        }

        public string GetPcd()
        {
            // delete file.
            string logDb = GetReportFileFullPath("TestTimeLog.txt");
            bool isFileExist = File.Exists(logDb);
            if (isFileExist)
            {
                File.Delete(logDb);
            }

            if (IsDebugMode)
            {
                return m_pcdCounter++.ToString();
            }
            string pcd = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "");
            string pcdVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "");
            string pcdNameVersion = String.Format("{0}_{1}", pcd, pcdVersion);

            if (String.IsNullOrEmpty(pcd))
            {
                return String.Empty;
            }

            return pcdNameVersion;
        }

        public bool IsPass()
        {
            if (IsDebugMode)
            {
                return true;
            }

            if (ResultBuilder.FailedTests.Length > 0)
            {
                return ResultBuilder.FailedTests[0].Count == 0;
            }

            return false;
        }

        public bool IsToDeactivate()
        {
            return !IsDebugMode;
        }

        private string GetReportFileFullPath(string fileName)
        {
            string dbPath = "C:\\TEMP";
            if (!Directory.Exists(dbPath))
            {
                dbPath = Path.GetTempPath();
            }

            string fp = Path.Combine(dbPath, fileName);
            return fp;
        }
    }

    public class TestLineCollection
    {
        public int TcfIndex { get; set; }
        public string UniqueHeaderName { get; set; }
        public double ElapsedMs { get; set; }
        public TestLineCondition TestConditions { get; set; }
    }

    public class TestLineCollectionJokerSPara
    {
        public int TcfIndex { get; set; }
        public string UniqueHeaderName { get; set; }
        public double ElapsedMs { get; set; }
        public TestLineConditionJokerSPara TestConditions { get; set; }
    }

    /// <summary>
    /// Joker.
    /// </summary>
    public class TestLineConditionJoker
    {
        public string TestMode { get; set; }
        public string Band { get; set; }
        public string Modulation { get; set; }
        public string Switch_TX { get; set; }
        public string Switch_ANT { get; set; }
        public string Switch_RX { get; set; }
        public List<string> Para { get; set; }
    }

    public class TestLineCondition
    {
        public string TestMode { get; set; }
        public string TXInput { get; set; }
        public string Band { get; set; }
        public string Modulation { get; set; }
        public string TXOut { get; set; }
        public string RXOutput1 { get; set; }
        public string RXOutput2 { get; set; }
        public List<string> Para { get; set; }
    }

    public class TestLineConditionJokerSPara
    {
        public string TestMode { get; set; }
        public string Band { get; set; }
        public string PowerMode { get; set; }
        public string SwitchIN { get; set; }
        public string SwitchOut { get; set; }
        public string SwitchANT { get; set; }
        public int LineNumber { get; set; }
    }

    public class TestLineFixedCondition
    {
        public string PcdPackageName { get; set; }
        public string ProductTag { get; set; }
        public string TesterName { get; set; }
        public string ZDbFolder { get; set; }

        /// <summary>
        /// RF1 or RF2.
        /// </summary>
        public string TestType { get; set; }

        public string ContractManufacturer { get; set; }

        public void SetContractManufacturer(string zdbFolder)
        {
            ZDbFolder = zdbFolder;

            switch (zdbFolder)
            {
                case @"\\192.168.1.41\zdbrelay\Trace_Data":
                    ContractManufacturer = "Inari P3";
                    break;

                case @"\\192.168.11.7\zDB\ZDBFolder":
                    ContractManufacturer = "Inari P8";
                    break;

                case @"\\172.16.11.14\zDB\ZDBFolder":
                    ContractManufacturer = "Inari P13";
                    break;

                case @"\\10.50.10.35\avago\ZDBFolder":
                    ContractManufacturer = "ASEK";
                    break;

                default:
                    ContractManufacturer = "Others";
                    break;
            }
        }
    }
}