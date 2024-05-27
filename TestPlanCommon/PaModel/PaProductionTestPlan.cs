using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using EqLib;
using MPAD_TestTimer;
using TestLib;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.PaModel
{
    /// <summary>
    /// Production codes to be expanded here.
    /// </summary>
    public class PaProductionTestPlan
    {
        public ProductionTestTimeController TestTimeLogController { get; }

        public PaProductionTestPlan()
        {
            TestTimeLogController = new ProductionTestTimeController();
        }

        // CCT Entry Point.
        public void LoadModuleIDSelectList(TestPlanStateModel m_modelTpState)
        {
            m_modelTpState.SetLoadFail(LoadModuleIDSelectList2());
            m_modelTpState.SetLoadFail(LoadModule2DIDSelectList());
        }

        public bool LoadModuleIDSelectList2()
        {
            bool isSuccess = true;

            #region load Module ID select sheet

            int SheetNumberModuleID = 3;
            int ModuleIDRows = 5000;
            int ModuleIDCols = 20;
            int CurModRow = 0;
            int CurModCol = 0;
            //Dictionary<int, List<int>> ModuleIDSelect = new Dictionary<int, List<int>>();
            if (!OTP_Procedure.EnableModuleIDselect) return isSuccess;

            //Tuple<bool, string, string[,]> ModuleIDsheet = ATFCrossDomainWrapper.Excel_Get_IputRange(SheetNumberModuleID, 1, 1, ModuleIDRows, ModuleIDCols);
            TcfSheetReader resourceSheet = new TcfSheetReader("ModuleID_Select", ModuleIDRows, ModuleIDCols);
            Tuple<bool, string, string[,]> ModuleIDsheet = resourceSheet.allContents;

            if (ModuleIDsheet.Item1 == false)
            {
                PromptManager.Instance.ShowError("Error reading Excel Range", ModuleIDsheet.Item2);
                isSuccess = false;
            }

            try
            {
                bool StartFound = false;

                while (CurModCol < ModuleIDCols)
                {
                    string enableCell = ModuleIDsheet.Item3[0, CurModCol];

                    if (enableCell.ToUpper() == "#START")
                    {
                        StartFound = true;
                        CurModCol++;
                    }
                    if (StartFound)
                    {
                        String MFG_ID = ModuleIDsheet.Item3[1, CurModCol];
                        int Mfg_ID = 0;

                        if (!String.IsNullOrEmpty(MFG_ID)) Mfg_ID = Convert.ToInt32(MFG_ID);

                        int i = 2;
                        if (Mfg_ID != 0)
                        {
                            List<long> Mod_IDList = new List<long>();
                            while (true)
                            {
                                string value = ModuleIDsheet.Item3[i, CurModCol];
                                long Mod_ID = 0;
                                if (!String.IsNullOrEmpty(value)) Mod_ID = Convert.ToInt64(value);
                                if (Mod_ID != 0) Mod_IDList.Add(Mod_ID);
                                string RowEndCheck = ModuleIDsheet.Item3[i, 0];
                                i++;
                                if (RowEndCheck.ToUpper() == "#END") break;
                            }
                            OTP_Procedure.ModuleIDSelect.Add(Mfg_ID, Mod_IDList);
                        }
                    }

                    if (enableCell.ToUpper() == "#END") break;
                    CurModCol++;
                }
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
            }

            //PaTest.ModuleIDSelect = ModuleIDSelect;

            #endregion load Module ID select sheet

            return isSuccess;
        }

        public bool LoadModule2DIDSelectList()
        {
            bool isSuccess = true;

            #region load Module ID select sheet

            int ModuleIDRows = 5000;
            int ModuleIDCols = 20;
            int CurModCol = 0;

            if (!OTP_Procedure.Enable2DIDselect) return isSuccess;

            //Tuple<bool, string, string[,]> ModuleIDsheet = ATFCrossDomainWrapper.Excel_Get_IputRange(SheetNumberModuleID, 1, 1, ModuleIDRows, ModuleIDCols);
            TcfSheetReader resourceSheet = new TcfSheetReader("Select_2DID", ModuleIDRows, ModuleIDCols);
            Tuple<bool, string, string[,]> ModuleIDsheet = resourceSheet.allContents;

            if (ModuleIDsheet.Item1 == false)
            {
                PromptManager.Instance.ShowError("Error reading Excel Range", ModuleIDsheet.Item2);
                isSuccess = false;
            }

            try
            {
                bool StartFound = false;

                string enableCell = ModuleIDsheet.Item3[0, 0];

                if (enableCell.ToUpper() == "#START")
                {
                    StartFound = true;
                    CurModCol++;
                }
                if (StartFound)
                {
                    String MFG_ID = ModuleIDsheet.Item3[1, 1];

                    int i = 2;
                    if (MFG_ID != "")
                    {
                        List<string> Mod_IDList = new List<string>();
                        while (true)
                        {
                            string value = ModuleIDsheet.Item3[i, 1];
                            string value1 = ModuleIDsheet.Item3[i, 2];
                            string key_2DID = "";
                            if (!String.IsNullOrEmpty(value1))
                            {
                                if (value1.Substring(0, 1) == "'")
                                {
                                    value1 = value1.TrimStart(new char[] {'\''});
                                }

                                if (value1.Length != 24)
                                {
                                    MessageBox.Show("2DID Entries length is not equal to 24", "2DID Module Sorting");
                                }
                                key_2DID = value1.PadLeft(24, '0');
                                OTP_Procedure.Str2DIDSelect.Add(key_2DID, value);
                            }
                            string RowEndCheck = ModuleIDsheet.Item3[i, 0];
                            i++;
                            if (RowEndCheck.ToUpper() == "#END") break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
            }

            #endregion load Module ID select sheet

            return isSuccess;
        }

        public bool InitializeMagicBox(bool detect_magicbox)
        {
            string strSMsn = "";
            bool blnDetectedSM = false;

            if (detect_magicbox)
            {
                #region Switch Matrix with USB device - Detect all drives (incl. D: drive (USB) and E: drive (SD card)

                string strSMfolder = "SwitchMatrixInfo";
                string strSMfile = "SwitchMatrixSN";
                string strSMfileNamePath = "C:\\Avago.ATF.Common\\DataLog\\" + strSMfolder + "\\" + strSMfile;

                bool blnNeedCal = false;

                DriveInfo[] mydrives = DriveInfo.GetDrives();

                mydrives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToArray();

                if (mydrives.Length == 0)
                {
                    PromptManager.Instance.ShowInfo("No USB Device found. MagicBox is not being detected.", "MagicBox");
                }
                else
                {
                    foreach (DriveInfo drive in mydrives)
                    {
                        if (blnDetectedSM) break;

                        #region Method#1 - Detect all drives (incl. C, E (SD card)

                        uint serialNum, serialNumLength, flags;
                        StringBuilder volumename = new StringBuilder(256);
                        StringBuilder fstype = new StringBuilder(256);
                        bool ok = false;

                        foreach (string drives in Environment.GetLogicalDrives())
                        {
                            ok = GetVolumeInformation(drives, volumename, (uint)volumename.Capacity - 1, out serialNum,
                                                   out serialNumLength, out flags, fstype, (uint)fstype.Capacity - 1);
                            if (ok)
                            {
                                //if (drive.Name == drives)
                                if (drives == "D:\\")
                                {
                                    //Check if this is MagicBox, by detecting the MagicBox folder....
                                    if (Directory.Exists(drives + "ExpertCalSystem.Data\\MagicBox"))
                                    {
                                        //MagicBox detected
                                        blnDetectedSM = true;

                                        //Check if the SN is different / same
                                        if (File.Exists(strSMfileNamePath + ".txt"))
                                        {
                                            string f = strSMfileNamePath + ".txt";
                                            using (StreamReader r = new StreamReader(f))
                                            {
                                                string line;
                                                while ((line = r.ReadLine()) != null)
                                                {
                                                    if (Convert.ToString(serialNum) == line)
                                                    {
                                                        //MessageBox.Show("It's the same switch matrix, no action required");
                                                        strSMsn = Convert.ToString(serialNum);
                                                    }
                                                    else  //Detected different device ID
                                                    {
                                                        //MessageBox.Show("It's different switch matrix, Please calibrate secondary cal");
                                                        //blnNeedCal = true;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Directory.CreateDirectory("C:\\Avago.ATF.Common\\DataLog\\" + strSMfolder + "\\");

                                            //using (StreamWriter writer = new StreamWriter(strSMfileNamePath + ".txt"))
                                            //{
                                            //    writer.WriteLine(serialNum);
                                            //}
                                        }
                                        break;
                                    }
                                }
                            }
                            ok = false;
                        }

                        #endregion Method#1 - Detect all drives (incl. C, E (SD card)
                    }
                }

                #endregion Switch Matrix with USB device - Detect all drives (incl. D: drive (USB) and E: drive (SD card)
            }
            return blnDetectedSM;
        }

        // MagicBox
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetVolumeInformation(string Volume, StringBuilder VolumeName, uint VolumeNameSize, out uint SerialNumber, out uint SerialNumberLength, out uint flags, StringBuilder fs, uint fs_size);

        public void ShowInputGui(string sample_version = "", string addSublotID = "", string addDeviceID = "", string webQueryValidation = "", string webServerUrl = "", string GuEngineering = "")
        {
            string Clotho_User = "DEBUG_USER";
            try
            {
                Clotho_User = ATFRTE.Instance.CurUserName.ToString().ToUpper();
            }
            catch
            {
                Clotho_User = "DEBUG_USER";
            }
            LoggingManager.Instance.LogInfoTestPlan("Waiting for Production Information...");
            //ProductionLib2.ProductionTestInputForm prodInputForm = new ProductionLib2.ProductionTestInputForm(sample_version.ToUpper() != "PRODUCTION");
            ProductionLib2.ProductionTestInputForm prodInputForm = new ProductionLib2.ProductionTestInputForm(addSublotID, addDeviceID, webQueryValidation,
                webServerUrl, Clotho_User);
            string tempLotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
            prodInputForm.LotID = tempLotID;        //Pass LotID to production GUI.

            prodInputForm.productTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "");

            //string load_board_id = "LB-8220-081-NAN";

            //load_board_id = Eq.Site[0].HSDIO.UNIO_EEPROMReadID(EqHSDIO.UNIO_EEPROMType.Loadboard, 1);
            //prodInputForm.LoadBoardID = load_board_id.Contains("8240") ? load_board_id : "LB-8240-801-NAN";

            //string contactor_id = "LN-8220-NAN-NAN";
            //contactor_id = Eq.Site[0].HSDIO.UNIO_EEPROMReadID(EqHSDIO.UNIO_EEPROMType.Socket, 2);
            //prodInputForm.ContactorID = contactor_id.Contains("8240") ? contactor_id : "LN-8240-G46-NAN";

            prodInputForm.HandlerID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");

            OtpTest.engineeringMode = GuEngineering;

            bool skipProdUI = ((Clotho_User == "PPUSER") && (tempLotID.ToUpper() == "GUCAL")) ||
                ((Clotho_User == "VUSER") && !tempLotID.StartsWith("PT") && !tempLotID.StartsWith("FT"));

            //MessageBox.Show(Clotho_User);

            if (skipProdUI == false)
            {
                //#if (!DEBUG)
                DialogResult rslt = prodInputForm.ShowDialog();

                if (rslt == DialogResult.OK)
                {
                    OtpTest.mfg_ID = prodInputForm.MfgLotID;
                    ATFCrossDomainWrapper.StoreStringToCache("MFG", prodInputForm.MfgLotID);  //hosein 09302020
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, prodInputForm.OperatorID);
                    //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_LOT_ID, prodInputForm.LotID);
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, prodInputForm.SublotID);
                    //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, prodInputForm.LoadBoardID);
                    //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_CONTACTOR_ID, prodInputForm.ContactorID);
                    //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_HANDLER_SN, prodInputForm.HandlerID);
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "NA");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "NA");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_ASSEMBLY_ID, "MFG" + prodInputForm.MfgLotID);
                    string meup = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_ASSEMBLY_ID, "");
                }
            }
            else
            {
                OtpTest.mfg_ID = "000001";
                ATFCrossDomainWrapper.StoreStringToCache("MFG", "000001");  //hosein 09302020
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, "A0001");
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_LOT_ID, prodInputForm.LotID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, "1A");
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, prodInputForm.LoadBoardID);
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_CONTACTOR_ID, prodInputForm.ContactorID);
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_HANDLER_SN, prodInputForm.HandlerID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "NA");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "NA");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_ASSEMBLY_ID, "MFG" + "000001");
                prodInputForm.WebserviceByPass = true;
            }

            //Web Service 2.0 - DH

            OtpTest.Web2didData2 = prodInputForm.web_2DID;
            OtpTest.Weblotid = prodInputForm.LotID;
            OtpTest.WebByPass = prodInputForm.WebserviceByPass;
            LoggingManager.Instance.LogInfoTestPlan(" prodInputForm.web_2DID is " + prodInputForm.web_2DID);

        }

        public bool DetectDoubleUnitSoftware(string settingPauseTestOnDuplicateModuleID)
        {
            bool isSuccess = true;
            if (settingPauseTestOnDuplicateModuleID == "TRUE")
            {
                foreach (byte site in ResultBuilder.ValidSites)
                {
                    if (ResultBuilder.DuplicatedModuleID[site] == true)
                    {
                        if (ResultBuilder.DuplicatedModuleIDCtr[site] < 2)
                        {
                            ResultBuilder.DuplicatedModuleIDCtr[site]++;
                            ATFLogControl.Instance.Log(LogLevel.Warn, LogSource.eHandler, "Duplicated Module ID detected, test aborted." + Environment.NewLine + "Please inspect test socket and then rectify the problem to resume test");

                            Thread otherWindowHostingThread = new Thread(new ThreadStart(
                                () => {
                                    ProductionLib2.InspectSocketMessage msg = new ProductionLib2.InspectSocketMessage();
                                    msg.ShowDialog();
                                }
                            ));

                            otherWindowHostingThread.SetApartmentState(ApartmentState.STA);
                            otherWindowHostingThread.Start();
                            otherWindowHostingThread.Join();
                            //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);
                        }
                        else
                        {
                            ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "Duplicated Module ID detected for > 3 times, reload test plan to continue");
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        ResultBuilder.DuplicatedModuleIDCtr[site] = 0;
                    }
                }
            }

            return isSuccess;
        }

        public void LockClotho()
        {
            string clotho_tester_id = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "Clotho");
            Thread t1 = new Thread(ProductionLib2.LockClotho.LockClothoInputUI);
            //Thread t1 = new Thread(ProductionLib2.LockClotho.UnlockClothoInputUI);
            t1.Start(clotho_tester_id);
        }
    }

    public class ProductionTestTimeController
    {
        private string m_dataFilePath;
        private List<string> m_passedPcdList;
        private ProductionTestTimeReportGenerator m_model;
        private TestLineFixedCondition m_fixedCond;
        private ProductionTestTimeDebug m_debugModel;
        private int testTimeLognum = 0;

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
            List<Dictionary<string, string>> testCondList,
            TcfSheetReader dcResourceSheet)
        {
            string pcd = m_debugModel.GetPcd();
            Initialize(pcd);
            if (IsDeactivated) return;

            Dictionary<string, string> atfConfig = modelTpState.GetAtfConfig(atfConfigFilePath);
            TestLineFixedCondition fc = new TestLineFixedCondition();
            fc.SetContractManufacturer(atfConfig["ATFResultRemoteSharePath"]);
            fc.TesterName = atfConfig["TesterID"];
            fc.PcdPackageName = pcd;
            fc.TestType = "RF1";

            AddFixedCondition(fc);
            AddTest(testCondList, dcResourceSheet);

            StopWatchManager.Instance.IsActivated = true;
        }

        public void Save(byte site, byte numofSite)
        {
            if (IsDeactivated) return;

            bool isTestPlanPassed = m_debugModel.IsPass2ndTime(site);
            if (!isTestPlanPassed) return;

            List<PaStopwatch2> swList = StopWatchManager.Instance.GetList(site);
            Save(swList, site);
            // Set to false to disable. When testTimeLognum  equal to testsite number
            testTimeLognum++;
            if(testTimeLognum == numofSite)
            {
                IsDeactivated = m_debugModel.IsToDeactivate();
                StopWatchManager.Instance.IsActivated = !m_debugModel.IsToDeactivate();
            }

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
            List<Dictionary<string, string>> DicTestCondTemp, TcfSheetReader DcResourceSheet)
        {
            if (IsDeactivated) return;
            m_model.AddTest(DicTestCondTemp, DcResourceSheet);
        }

        private void Save(List<PaStopwatch2> swList, byte site)
        {
            // If has checked already, skip checking to speed up.
            if (IsDeactivated) return;
            Save(m_fixedCond.PcdPackageName, swList, m_fixedCond.ContractManufacturer, site);
        }

        private void Save(string currentPcdName, List<PaStopwatch2> swList,
            string cmName, byte site)
        {
            // Create report.
            string reportContent = m_model.CreateReport(swList);
            string destReportFileName = m_model.GetReportFileName(site);
            string destReportFullPath = GetReportFileFullPath(destReportFileName);

            StreamWriter sw = File.CreateText(destReportFullPath);
            sw.Write(reportContent);
            sw.Flush();
            sw.Close();

            // Transfer to shared folder.
            CopyToServer(cmName, destReportFullPath);

            // Save to database.
            m_passedPcdList = ReadSavedPcdList();
            bool isExist = m_passedPcdList.Contains(currentPcdName);
            if (isExist)
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
            if (!result.Item1)
            {
                msg = String.Format("Failed to copy TTD file from {0} to {1} server.",
                    sourceFullPath, cmName);
            }
            LoggingManager.Instance.LogInfoTestPlan(msg);

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
        private List<TestLineConditionPinot> m_condList;
        private TestLineFixedCondition m_fixedCond;
        private List<string> m_paraNameList;

        public ProductionTestTimeReportGenerator()
        {
            m_separator = ',';
            m_paraNameList = CreateFixedParaColumnNames();
        }

        public void AddFixedCondition(TestLineFixedCondition fixedc)
        {
            m_fixedCond = fixedc;
        }

        public void AddTest(
            List<Dictionary<string, string>> DicTestCondTemp, TcfSheetReader DcResourceSheet)
        {
            m_condList = new List<TestLineConditionPinot>();

            for (int testIndex = 0; testIndex < DicTestCondTemp.Count; testIndex++)
            {
                PaTestConditionFactory testConFactory = new PaTestConditionFactory(DicTestCondTemp[testIndex], DcResourceSheet);
                TestLineConditionPinot tlc = FormPaBandInfo(testConFactory);
                m_condList.Add(tlc);
            }
        }

        private TestLineConditionPinot FormPaBandInfo(PaTestConditionFactory tcFactory)
        {
            TestLineConditionPinot c = new TestLineConditionPinot();

            string testMode = tcFactory.GetStr("Test Mode").Trim().ToUpper();
            c.TestMode = testMode;

            switch (testMode)
            {
                case "IIP3":
                case "RF":
                case "RF_FIXED_PIN_S":
                case "RF_FIXED_PIN":
                case "PIN_SWEEP":
                // Seoraksan.
                //case "RF_ONOFFTIME":
                //case "RF_ONOFFTIME_SW":
                //    c.Band = tcFactory.GetBand();
                //    c.TXInput = tcFactory.GetTXINport();
                //    c.Modulation = tcFactory.GetModulationID();
                //    //TXOut is only for Seoraksan.
                //    //c.TXOut = tcFactory.GetTXport();
                //    TestLineCondition txinout = FormPaTxRx(tcFactory);
                //    c.TXInput = txinout.TXInput;
                //    c.TXOut = txinout.TXOut;
                //    txinout = FormPaParameters(tcFactory);
                //    c.Para = txinout.Para;
                //    break;
                case "TIMING":              // Pinot use TIMING.
                    c.Band = tcFactory.GetBand();
                    c.Modulation = tcFactory.GetModulationID();
                    TestLineConditionPinot txinout = FormPaTxRx(tcFactory);
                    c.Switch_TX = txinout.Switch_TX;
                    c.Switch_ANT = txinout.Switch_ANT;
                    c.Switch_RX = txinout.Switch_RX;
                    TestLineCondition paraList = FormPaParameters(tcFactory);
                    c.Para = paraList.Para;
                    break;

                case "DC":
                case "DC_LEAKAGE":
                    c.Band = tcFactory.GetBand();
                    paraList = FormPaParameters(tcFactory);
                    c.Para = paraList.Para;
                    break;

                case "OTP":
                case "CONTINUITY":
                case "CALC":
                case "MIPI":
                default:
                    break;
            }

            return c;
        }

        private TestLineConditionPinot FormPaTxRx(PaTestConditionFactory tcFactory)
        {
            TestLineConditionPinot c = new TestLineConditionPinot();
            c.Switch_TX = tcFactory.GetTXINport();
            c.Switch_ANT = tcFactory.GetANTport();
            string rx = tcFactory.GetRXport();
            // if TCF value is empty, rx = x. Need to revert to empty.
            if (String.Compare(rx, "x") == 0)
            {
                rx = "";
            }
            c.Switch_RX = rx;
            return c;
        }

        private TestLineCondition FormPaParameters(PaTestConditionFactory tcFactory)
        {
            TestLineCondition c = new TestLineCondition();
            List<string> enabledList = new List<string>();

            foreach (string paraName in m_paraNameList)
            {
                string tcfValue = tcFactory.GetStr(paraName);
                if (String.IsNullOrEmpty(tcfValue))
                {
                    continue;
                }

                // remove the prefix 'Para.'
                string truncateParaName = paraName.Remove(0, 5);
                enabledList.Add(truncateParaName);
            }

            c.Para = enabledList;
            return c;
        }

        public void CreateReport2(List<PaStopwatch2> swList)
        {
            //foreach (TestLineCollection tlc in m_cList)
            //{
            //    Predicate<PaStopwatch2> idFinder = (PaStopwatch2 p) => { return p.Name == tlc.UniqueHeaderName; };
            //    PaStopwatch2 result = swList.Find(idFinder);
            //    tlc.ElapsedMs = result.ElapsedMs;

            //}
        }

        public string GetReportFileName(byte site)
        {
            // Example: 2018-12-19T031143
            string sortableDt = DateTime.Now.ToString("yyyy-MM-ddThhmmss");
            string fn = String.Format("{0}_{1}_Site{2}_{3}.ttd",
                m_fixedCond.PcdPackageName, m_fixedCond.TesterName, site, sortableDt);
            return fn;
        }

        public string CreateReport(List<PaStopwatch2> swList)
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

            List<TestLineCollectionPinot> reportLines = new List<TestLineCollectionPinot>();

            bool isValidCount = testSwList.Count >= m_condList.Count;

            if (!isValidCount)
            {
                string errorMsg = String.Format("Error: {0}", msg);
                return errorMsg;
            }           

            double sumOfTest = 0;

            for (int i = 0; i < m_condList.Count; i++)
            {
                TestLineCollectionPinot rl = new TestLineCollectionPinot();
                rl.TcfIndex = i + 1;
                rl.ElapsedMs = testSwList[i].ElapsedMs;
                rl.TestConditions = m_condList[i];
                reportLines.Add(rl);
                sumOfTest = sumOfTest + testSwList[i].ElapsedMs;
            }

            // Add Total test time
            TestLineCollectionPinot totalTestTimeRl = new TestLineCollectionPinot();
            totalTestTimeRl.TcfIndex = -1;
            totalTestTimeRl.ElapsedMs = sumOfTest;
            totalTestTimeRl.TestConditions = new TestLineConditionPinot();
            totalTestTimeRl.TestConditions.TestMode = "TIME_Total";
            reportLines.Add(totalTestTimeRl);


            foreach (PaStopwatch2 sw in swList)
            {
                bool executionTime = sw.Name == "TIME_DoATFTest";
                if (!executionTime) continue;
                TestLineCollectionPinot rl = new TestLineCollectionPinot();
                rl.TcfIndex = -1;
                rl.ElapsedMs = sw.ElapsedMs - sumOfTest;
                rl.TestConditions = new TestLineConditionPinot();
                rl.TestConditions.TestMode = "TIME_Overhead";
                reportLines.Add(rl);
            }

            StringBuilder sb = new StringBuilder();

            // generate header.
            sb.AppendLine(CreateColumnHeader1());

            List<string> m_fixedColumns = new List<string>();
            m_fixedColumns.AddRange(new string[] { DateTime.Now.ToShortDateString(), DateTime.Now.ToString(),
                m_fixedCond.PcdPackageName, m_fixedCond.TestType,
                m_fixedCond.TesterName, m_fixedCond.ContractManufacturer });
            string fixedColumns = String.Join(m_separator.ToString(), m_fixedColumns.ToArray());

            // generate content.
            foreach (TestLineCollectionPinot testConditionLine in reportLines)
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
            col.AddRange(new string[] { "TestNumber", "Time_ms", "TestMode", "Band", "Modulation", "Switch_TX" });
            col.AddRange(new string[] { "Switch_ANT", "Switch_RX", "Para" });

            string c1 = String.Join(m_separator.ToString(), col.ToArray());
            return c1;
        }

        private void FormLine(StringBuilder sb, string fixedColumns, TestLineCollectionPinot tlList)
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
            sb.Append(tlList.TestConditions.Modulation);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.Switch_TX);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.Switch_ANT);
            sb.Append(m_separator, 1);
            sb.Append(tlList.TestConditions.Switch_RX);
            sb.Append(m_separator, 1);

            string para = String.Empty;
            if (tlList.TestConditions.Para != null)
            {
                para = String.Join("+", tlList.TestConditions.Para.ToArray());
            }
            sb.Append(para);
            sb.Append(m_separator, 1);
            sb.AppendLine();
        }

        private string Convert(double timeMs)
        {
            return timeMs.ToString("F4");
        }

        private List<string> CreateFixedParaColumnNames()
        {
            List<string> paraNameList = new List<string>();
            paraNameList.Add("Para.Pin");
            paraNameList.Add("Para.Pout");
            paraNameList.Add("Para.Gain");
            paraNameList.Add("Para.Ibatt");
            paraNameList.Add("Para.Idd");
            paraNameList.Add("Para.Icc");
            paraNameList.Add("Para.ISum");
            paraNameList.Add("Para.Ieff");
            paraNameList.Add("Para.Icpl");
            paraNameList.Add("Para.Cpl");
            paraNameList.Add("Para.Pcon");
            paraNameList.Add("Para.PAE");
            paraNameList.Add("Para.EUTRA");
            paraNameList.Add("Para.ACLR1");
            paraNameList.Add("Para.ACLR2");
            paraNameList.Add("Para.IIP3");
            paraNameList.Add("Para.H2");
            paraNameList.Add("Para.H3");
            paraNameList.Add("Para.H2at2G");
            paraNameList.Add("Para.H3at2G");
            paraNameList.Add("Para.PowerDrop");
            paraNameList.Add("Para.EVM");
            paraNameList.Add("Para.NS");
            paraNameList.Add("Para.TxLeakage");
            paraNameList.Add("Para.FowardISO");
            paraNameList.Add("Para.ISdata1");
            paraNameList.Add("Para.ISclk1");
            paraNameList.Add("Para.Iio1");
            paraNameList.Add("Para.ISdata2");
            paraNameList.Add("Para.ISclk2");
            paraNameList.Add("Para.Iio2");

            return paraNameList;
        }
    }

    public class ProductionTestTimeDebug
    {
        public bool IsDebugMode { get; set; }
        private int m_pcdCounter;
        private int [] m_passCount = new int[4] { 0, 0, 0, 0 };        // monitor the number of pass, 2nd pass to generate time log.

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
            if (String.IsNullOrEmpty(pcd))
            {
                return pcd;
            }
            string pcdVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "");
            string pcdNameVersion = String.Format("{0}_{1}", pcd, pcdVersion);
            return pcdNameVersion;
        }

        /// <summary>
        /// True if pass for the 2nd time in a test session.
        /// </summary>
        public bool IsPass2ndTime(byte site)
        {
            if (IsDebugMode)
            {
                m_passCount[site]++;
                string msg = String.Format("TestTimeDashboard:Pass count is {0}", m_passCount[site]);
                LoggingManager.Instance.LogInfo(msg);
                return m_passCount[site] > 1;
            }
            if (ResultBuilder.FailedTests.Length > 0)
            {
                bool isPass = ResultBuilder.FailedTests[site].Count == 0;
                if (isPass)
                {
                    m_passCount[site]++;
                }

                bool is2ndPass = m_passCount[site] == 2;
                return is2ndPass;
            }

            m_passCount[site]++;
            string msg2 = String.Format("TestTimeDashboard:Pass count is {0}", m_passCount[site]);
            LoggingManager.Instance.LogInfo(msg2);
            bool is2ndPass2 = m_passCount[site] == 2;
            return is2ndPass2;
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

    public class TestLineCollectionPinot
    {
        public int TcfIndex { get; set; }
        public string UniqueHeaderName { get; set; }
        public double ElapsedMs { get; set; }
        public TestLineConditionPinot TestConditions { get; set; }
    }

    /// <summary>
    /// Pinot.
    /// </summary>
    public class TestLineConditionPinot
    {
        public string TestMode { get; set; }
        public string Band { get; set; }
        public string Modulation { get; set; }
        public string Switch_TX { get; set; }
        public string Switch_ANT { get; set; }
        public string Switch_RX { get; set; }
        public List<string> Para { get; set; }
    }

    /// <summary>
    /// For S1.7.
    /// </summary>
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