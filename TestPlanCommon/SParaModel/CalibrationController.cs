using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using EqLib;
using MPAD_TestTimer;
using ToBeObsoleted;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// Holds instance of cal input, populated from ProjectSpecificFactor. Data source to be improved. Define in TCF instead of being in project specific.
    /// </summary>
    public class CalibrationInputDataObject
    {
        /// <summary>
        /// Always true.
        /// </summary>
        public bool CalContinue { get; set; }
        /// <summary>
        /// Project specific global setting.
        /// </summary>
        public bool CalDone { get; set; }
        public List<string> CalSegment { get; set; }
        public Dictionary<string, bool> cCalSelection { get; set; }
        /// <summary>
        /// Set to false after VerifySubCal().
        /// </summary>
        public bool Fbar_cal { get; set; }
        public int iCalCount { get; set; }
        public int TopazCalPower { get; set; }
        /// <summary>
        /// Always true.
        /// </summary>
        public bool Using28Ohm { get; set; }
    }

    /// <summary>
    /// Cal Entry point. Calls->CalibrationModel->CalibrationModel3. To redesign Calibration classes.
    /// </summary>
    public class CalibrationController
    {
        string verificationFlag = "SPARA";

        public void ReadSmPad(ProdLib1Wrapper wrapper4)
        {
            #region Scratchpad for ZTM15

            //ModularZT64.USB_ZT xman = new USB_ZT();
            //string meup = "";
            //string zulu = "";
            //string zcommand = "SP6T:3:STATE:1";
            //string zcommand2 = ":ClearAll";
            //xman.Get_Available_SN_List(ref meup); //The serial numbers are listed, separated by space
            //xman.Get_Available_Address_List(ref zulu);
            //xman.Connect(ref ZTM15_1_SN);
            //xman.Send_SCPI(ref zcommand2, ref zulu);
            //xman.Send_SCPI(ref zcommand, ref zulu);
            //xman.Connect(ref ZTM15_2_SN);
            //xman.Send_SCPI(ref zcommand2, ref zulu);
            //xman.Send_SCPI(ref zcommand, ref zulu);

            //Console.WriteLine("String Me Up returned this:  " + meup + "\r\n");
            //Console.WriteLine("Zulu returned this:   " + zulu + "\r\n");

            //string kicker = "stop here";

            #endregion Scratchpad for ZTM15

            #region Read SMPAD ID

            wrapper4.Wrapper2.ReadSmPadId();

            #endregion Read SMPAD ID        
        }

        public bool ENA_Cal_Enable { get; set; }
        public string calibrationFlag { get; set; }
        public int[] ListDoCalCh { get; set; }

        public CalibrationInputDataObject DataObject { get; set; }

        public CalibrationController()
        {
            DataObject = new CalibrationInputDataObject();
        }

        public DialogResult PromptAutoSubCal()
        {
            string calMsg = String.Format("{0}{1}{0}{2}{0}{3}{0}{4}", Environment.NewLine,
                "Do you want to perform FBAR auto-subcal?", " Yes = Automatic substrate calibration.",
                "No = Manual ECAL calibration", "Cancel = Don't do substrate calibration");
            DialogResult dr =
                PromptManager.Instance.ShowDialogYesNoCancel(calMsg, "Network Analyzer Calibration");
            return dr;
        }

        public void PerformManualECal(ProdLib1Wrapper m_wrapper4)
        {
            PromptManager.Instance.ShowInfo("Manual Ecal Calibration will now start." + "\r\n");
            int me = 1;
            List<string> zbandlist = new List<string>();

            //ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);

            //ActivatePath(TxPort, dicSwitchConfig[TxPort]);

            //ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);

            foreach (string zvalue in zbandlist)
            {
                string zbandinfo2 = zvalue;
                m_wrapper4.ActivateManualECal(zbandinfo2);
                PromptManager.Instance.ShowInfo(
                    "\r\n" + "This is " + zbandinfo2 + "\r\n" +
                    "Calibrate the corresponding channel based on the worksheet" + "\r\n" +
                    "Click OK when finished.");

                //MessageBox.Show("Do manual ECAL on Channel " + me.ToString() + " of the VNA" + "\r\n" +
                //    "This is " + zbandinfo2 + "\r\n" + "Follow the worksheet for other instructions" + "\r\n"
                //    + "Take the time to manually verify the results of the cal before proceeding" + "\r\n"
                //    + "Click OK when finished.");

                me++;
            }

            PromptManager.Instance.ShowInfo("Manual ECAL is finished");
        }

        /// <summary>
        /// Perform Auto Cal or perform Verify. Returns if manual cal is chosen.
        /// </summary>
        /// <returns>SW for manual cal, empty string for no action, ABORT to set Fbar_Cal to false.</returns>
        public string PromptAutoManualOrVerify(CalibrationModel model, int PortPower)
        {
            string calResponseString = String.Empty;
            //Original
            //string strConnect = Interaction.InputBox("Auto ENA Calibration? please enter \"SCAL\"\r\nManual Calibration? please enter \"SW\".", "Network Analyzer Calibration", "SCAL", 150, 150);

            //string strConnect = Interaction.InputBox("Auto ENA Calibration? please enter \"SCAL\"\r\nManual Calibration? please enter \"SW\"\r\nVerification? please enter \"VERI\"", 
            //    "Network Analyzer Calibration", "SCAL", 150, 150);
            Dictionary<string, string> selectionList = new Dictionary<string, string>();
            selectionList.Add("SCAL", "Auto ENA Calibration");
            selectionList.Add("SW", "Manual Calibration");
            selectionList.Add("VERI", "Verification");
            string chosenCalType = PromptManager.Instance.ShowMultiSelectionDialog(
                "Choose a calibration type",
                "Network Analyzer Calibration", selectionList, "SCAL");
            if (chosenCalType.ToUpper() == "SW")
            {
                calResponseString = "SW";
                return calResponseString;       // let test plan handle product-specific manual switching.
            }

            bool SW_TEST2 = true;
            if (chosenCalType.ToUpper() == "VERI" && SW_TEST2)
            {
                //string SW_cmd = Interaction.InputBox("S-parameter Verification? please enter \"SPARA\"\r\nNF verification? please enter \"NF\"\r\nif you want to stop enter? please enter \"X\"", "Network Analyzer Calibration", "X", 150, 150);
                selectionList = new Dictionary<string, string>();
                selectionList.Add("SPARA", "S-parameter Verification");
                selectionList.Add("NF", "NF verification");
                selectionList.Add("X", "Stop");
                string SW_cmd = PromptManager.Instance.ShowMultiSelectionDialog(
                    "Choose a verification type",
                    "Network Analyzer Calibration", selectionList, "X");
                switch (SW_cmd.ToUpper())
                {
                    case "X":
                    case "":
                        model.SetTrigger();
                        break;
                    case "SPARA":
                        //model.Verification_Spara();

                        ////ChoonChin - 20191204 - Enable verification only without needing to run subcal, and change port power to cal power.
                        MessageBox.Show("Please insert verification load (28 Ohm) and press OK...", "Verification box");
                        //Detect instrument type
                        #region Check VNA

                        bool DivaInstrument = false;
                        string ConfigFilePath = @"C:\Users\Public\Documents\Network Analyzer\VnaConfig.txt";
                        try
                        {
                            if (File.Exists(ConfigFilePath))
                            {
                                //Read
                                StreamReader dSR = new StreamReader(ConfigFilePath);
                                string RS = dSR.ReadLine();
                                string[] aRS = RS.Split('=');

                                if (aRS[1].ToUpper().Contains("TRUE"))
                                {
                                    DivaInstrument = true;
                                }
                                else
                                    DivaInstrument = false;

                                dSR.Close();
                            }
                            else
                            {
                                //Not DIVA
                                DivaInstrument = false;
                            }
                        }
                        catch
                        {
                            //Not DIVA
                            DivaInstrument = false;
                        }


                        #endregion Check VNA
                        //Run Verification
                        int portPow = PortPower;
                        //DataObject.TopazCalPower;
                        model.VerificationOnly(DataObject.Using28Ohm, portPow);
                        break;
                    case "NF":
                        model.Verification_NF();
                        break;
                }
            }

            if (chosenCalType.ToUpper() == "SCAL")
            {
                //Added by ChoonChin
                //string strScal = Interaction.InputBox("Auto SCAL+NFCAL? please enter \"AUTO\"\r\n" +
                //                                      "Only perform SPARAM Partial Cal? please enter \"SPARTIAL\"\r\n" +
                //                                      "Only perform SCAL? please enter \"SPARA\"\r\n" +
                //                                      "Only perform NF CAL? please enter \"NF\"" +
                //                                      "\r\nIf you want to stop? please enter \"X\"", "Network Analyzer Calibration", "AUTO", 150, 150);
                selectionList = new Dictionary<string, string>();
                selectionList.Add("AUTO", "Auto SCAL+NFCAL");
                selectionList.Add("SPARTIAL", "Only perform SPARAM Partial Cal");
                selectionList.Add("SPARA", "Only perform SCAL");
                selectionList.Add("NF", "Only perform NF CAL");
                selectionList.Add("X", "Stop");
                string strScal = PromptManager.Instance.ShowMultiSelectionDialog(
                    "Choose a calibration type",
                    "Network Analyzer Calibration", selectionList, "AUTO");

                if (strScal.ToUpper() == "AUTO")
                {
                    SetCal1();
                    PromptManager.Instance.ShowInfo(
                        "Auto Sub Cal will start after loading.");

                    //ChoonCHin - Auto fill Clotho's field to aviod error message
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, "A1234");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, "1A");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, "1");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "1");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "1");

                    ENA_Cal_Enable = true; //To subcal in run test
                    calResponseString = "ABORT";

                    calibrationFlag = "ALL"; //To select CAL mode
                }

                if (strScal.ToUpper() == "SPARTIAL")
                {
                    selectionList = new Dictionary<string, string>();
                    selectionList.Add("LATESTFAIL", "Auto Select from latest Cal fail");
                    selectionList.Add("MANUAL", "Select enabele channel manually");
                    string strScal2 = PromptManager.Instance.ShowMultiSelectionDialog(
                        "Choose a SPARAM Partial Cal Method",
                        "Network Analyzer Calibration", selectionList, "LATESTFAIL");

                    string VerifyFilePath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
                    ListDoCalCh = CalEnableChannelSelector(strScal2, VerifyFilePath);

                    SetCal1();
                    PromptManager.Instance.ShowInfo(
                        "Auto Sub PARTIAL Cal will start after loading.");

                    //ChoonCHin - Auto fill Clotho's field to aviod error message
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, "A1234");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, "1A");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "1");
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "1");

                    ENA_Cal_Enable = true; //To subcal in run test
                    calResponseString = "ABORT";

                    calibrationFlag = "SPART"; //To select CAL mode
                }

                if (strScal.ToUpper() == "SPARA")
                {
                    SetCal1();
                    PromptManager.Instance.ShowInfo(
                        "Sparameter Cal will start after loading.");
                    ENA_Cal_Enable = true; //To subcal in run test
                    calResponseString = "ABORT";

                    calibrationFlag = "SPARA"; //To select CAL mode
                }

                if (strScal.ToUpper() == "NF")
                {
                    SetCal1();
                    PromptManager.Instance.ShowInfo("NF Cal will start after loading.");
                    ENA_Cal_Enable = true; //To subcal in run test
                    calResponseString = "ABORT";

                    calibrationFlag = "NF"; //To select CAL mode
                }

                if (strScal.ToUpper() == "X")
                {
                    model.SetTrigger();
                }

                //else if (strScal.ToUpper() == "MAN")
                //{
                //    LibFbar.Run_CalibrationProcedure();

                //    switch (MessageBox.Show("Do you want to perform FBAR cal again?", "Penang NPI", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question))
                //    {
                //        case DialogResult.Cancel:
                //            Fbar_cal = false;
                //            //zHandler.SendTermination();
                //            break;
                //        case DialogResult.Retry:
                //            Fbar_cal = true;
                //            break;
                //        default:
                //            break;
                //    }

                //    LibFbar.Save_FBAR_State();
                //}
            }

            //ChoonChin - 20191205 - 'cancel button'
            if (chosenCalType.ToUpper() == "")
            {
                calResponseString = "CANCEL";
            }

            #region Sub-Cal Verification

                //Auto Verification
                //DialogResult verifySubCal = MessageBox.Show("Do you want to verify FBAR sub-cal?", "Penang NPI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                //if (verifySubCal == DialogResult.Yes)
                //{
                //    //MessageBox.Show("Please prepare the cal substrate for verification", "Cal Substrate Verification");
                //    //foreach (string value in band)
                //    //{
                //    //    switch (band)
                //    //    {
                //    //        case "B7":
                //    //            verLimitLowLoad = 0;
                //    //            verLimitHiLoad = 0;
                //    //            verLimitLowThru = 0;
                //    //            verLimistHiThru = 0;
                //    //            verFreq = 2560;
                //    //    }

                //    //    SwitchMatrix.Maps.Activate(value, Operation.ENAtoRFIN);
                //    //    SwitchMatrix.Maps.Activate(value, Operation.ENAtoRFOUT);
                //    //    SwitchMatrix.Maps.Activate(value, Operation.ENAtoRX);

                //    //    LibFBAR.CalSubstrateVerification(band, verLimitLowLoad, verLimitHiLoad, "Load");
                //    //    LibFBAR.CalSubstrateVerification(band, verLimitLowThru, verLimitHiThru, "Thru");
                //    //}                            
                //    //#endregion Cal Substrate Verification

                //}

                #endregion

                //ChoonChin - Moved to Manual sub cal
                //switch (MessageBox.Show("Do you want to perform FBAR cal again?", "Penang NPI", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question))
                //{
                //    case DialogResult.Cancel:
                //        Fbar_cal = false;
                //        //zHandler.SendTermination();
                //        break;
                //    case DialogResult.Retry:
                //        Fbar_cal = true;
                //        break;
                //    default:
                //        break;
                //}

                return calResponseString;
        }

        /// <summary>
        /// Prompt to cal during ATFTest().
        /// </summary>
        public void Calibrate(CalibrationModel model, ATFReturnResult results)
        {
            #region FBAR Auto Sub Cal

            EnableCalContinue();

            // Note: for first run, Fbar_cal is true.
            while (DataObject.Fbar_cal)
            {
                if (DataObject.CalDone)
                {
                    //bool isVerDone = VerifySubCal(model, ref verificationFlag);
                    //if (!isVerDone) continue;
                    //ChoonChin - Change to return dummy so CLotho will send Bin2    
                    VerifySubCal2(model, DataObject.Using28Ohm);

                    //ATFResultBuilder.AddResult(ref results, "M_TIME_FbarTest", "", 5.0f);
                    return;
                }

                // Start of Auto Sub Cal
                //Change the Topaz output power level to 7dBm for high power calibration, 
                //added by Cheeon 28-July-2017
                // Case Joker is 10, Case HLS2 is 15
                //LibFBAR_TOPAZ.SParaTestManager.TopazCalPower = 10;
                //DataObject.TopazCalPower = 15;

                switch (calibrationFlag)
                {
                    case "ALL":
                        model.Run_CalibrationProcedure_Auto(DataObject);
                        break;
                    case "SPART":
                        model.Run_CalibrationProcedure_SpraramPartial(DataObject, ListDoCalCh);
                        break;
                    case "SPARA":
                        model.Run_CalibrationProcedure_SPARA(DataObject);
                        break;

                    case "NF":
                        model.Run_CalibrationProcedure_NF(DataObject);
                        break;

                    default:
                        PromptManager.Instance.ShowInfo("Need to check Sub calibration mode");
                        break;
                }

                // Note: for first run, CalContinue is true.
                if (!DataObject.CalContinue) model.Save_StateFile();

                //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);

                //ChoonChin - Change to return dummy so CLotho will send Bin2                                
                //ATFResultBuilder.AddResult(ref results, "M_TIME_FbarTest", "", 5.0f);
                return;

                //switch (MessageBox.Show("Do you want to perform FBAR cal again?", "Penang NPI", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question))
                //{
                //    case DialogResult.Cancel:
                //        Lib_Var.Fbar_cal = false;
                //        Lib_Var.CalDone = true;
                //        break;
                //    case DialogResult.Retry:
                //        Lib_Var.Fbar_cal = true;                            
                //        break;
                //    default:
                //        break;
                //} 
            }

            #endregion
        }

        /// <summary>
        /// Differs only in the VerifySubCal section.
        /// </summary>
        public void Calibrate2(CalibrationModel model, ATFReturnResult results)
        {
            #region FBAR Auto Sub Cal

            EnableCalContinue();


            while (DataObject.Fbar_cal)
            {
                if (DataObject.CalDone)
                {
                    VerifySubCal2(model, DataObject.Using28Ohm);
                    return;
                }

                // Start of Auto Sub Cal
                //Change the Topaz output power level to 7dBm for high power calibration, 
                //added by Cheeon 28-July-2017
                DataObject.TopazCalPower = 10;

                switch (calibrationFlag)
                {
                    case "ALL":
                        model.Run_CalibrationProcedure_Auto(DataObject);
                        break;
                    case "SPART":
                        model.Run_CalibrationProcedure_SpraramPartial(DataObject, ListDoCalCh);
                        break;
                    case "SPARA":
                        model.Run_CalibrationProcedure_SPARA(DataObject);
                        break;

                    case "NF":
                        model.Run_CalibrationProcedure_NF(DataObject);
                        break;

                    default:
                        PromptManager.Instance.ShowInfo("Need to check Sub calibration mode");
                        break;
                }

                if (!DataObject.CalContinue) model.Save_StateFile();

                //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);

                //ChoonChin - Change to return dummy so CLotho will send Bin2                                
                ATFResultBuilder.AddResult(ref results, "Fbar_Testtime", "", 5.0f);
                return;

                //switch (MessageBox.Show("Do you want to perform FBAR cal again?", "Penang NPI", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question))
                //{
                //    case DialogResult.Cancel:
                //        Lib_Var.Fbar_cal = false;
                //        Lib_Var.CalDone = true;
                //        break;
                //    case DialogResult.Retry:
                //        Lib_Var.Fbar_cal = true;                            
                //        break;
                //    default:
                //        break;
                //} 
            }

            #endregion
        }
        private bool VerifySubCal(CalibrationModel model, ref string verificationFlag)
        {
            PromptManager.Instance.ShowInfo("Calibration has finished.");
            //break;

            #region Sub-Cal Verification

            //Sprarmeter Verification                           
            switch (verificationFlag)
            {
                case "SPARA":
                    model.Verification_Spara();
                    // changed to NF Flag for the next "LOAD"insertion for NF verification  
                    verificationFlag = "NF";
                    return true;
                case "NF":
                    model.Verification_NF();
                    model.Save_StateFile();
                    DataObject.Fbar_cal = false;
                    return true;
            }

            #endregion

            return false;
        }
        private void VerifySubCal2(CalibrationModel model, bool isUsing28Ohm)
        {
            PromptManager.Instance.ShowInfo("Calibration has finished.");
            model.VerificationAll(isUsing28Ohm);
            model.Save_StateFile();
            DataObject.Fbar_cal = false;
        }

        private int[] CalEnableChannelSelector(string _Type, string filePathVerify)
        {
            try
            {
                int[] _DoCalChannel = null;
                Dictionary<string, int> dicDoCalChannel = new Dictionary<string, int>();

                if (_Type.ToUpper() == "LATESTFAIL")
                {
                    string LastestFileName = GetLastFileInDirectory(filePathVerify);

                    string[] TxtLine = System.IO.File.ReadAllLines(filePathVerify + LastestFileName);
                    
                    foreach (string sline in TxtLine)
                    {                        
                        if (sline.Contains("Channel"))
                        {
                            string ErrorCh = sline.Split(new string[] { "Channel: " }, StringSplitOptions.None)[1];
                            string ErrorCh1 = ErrorCh.Split(new string[] { "  Parameter:" }, StringSplitOptions.None)[0].ToString();
                            if(!dicDoCalChannel.ContainsValue(Convert.ToInt16(ErrorCh1)))
                                dicDoCalChannel.Add(sline, Convert.ToInt16(ErrorCh1));
                        }
                    }

                    _DoCalChannel = new int[dicDoCalChannel.Keys.Count];
                    dicDoCalChannel.Values.CopyTo(_DoCalChannel, 0);                  
                }

                if (_Type.ToUpper() == "MANUAL")
                {
                    bool IsSelected = false;
                    while (!IsSelected)
                    {
                        //TODO ZSD3
                        string title = "Network Analyzer Calibration";
                        string msg1 = "Please enter Channel number that need to calibration";
                        string msg2 = "ex) 1,2,3";

                        string strConnect = PromptManager.Instance.ShowTextInputDialog(msg1, msg2, title, "1,2,3");

                        try
                        {
                            string[] DoCalChannel = strConnect.Split(',');
                            _DoCalChannel = Array.ConvertAll(DoCalChannel, s => int.Parse(s));
                            IsSelected = true;
                        }
                        catch
                        {
                            _DoCalChannel = null;
                        }
                    }

                }

                return _DoCalChannel;
            }
            catch(Exception e)
            {
                return null;
            }
        }


        private string GetLastFileInDirectory(string directory, string pattern = "*.txt")
        {
            string filename = "";

            if (directory.Trim().Length == 0)
                return string.Empty; //Error handler can go here

            if ((pattern.Trim().Length == 0) || (pattern.Substring(pattern.Length - 1) == "."))
                return string.Empty; //Error handler can go here

            if (Directory.GetFiles(directory, pattern).Length == 0)
                return string.Empty; //Error handler can go here
                       
            var dirInfo = new DirectoryInfo(directory);

            FileInfo[] files = dirInfo.GetFiles();
            DateTime lastWrite = DateTime.MinValue;

            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastWrite)
                {
                    lastWrite = file.LastWriteTime;
                    filename = file.Name; 
                }
            }
            return filename;
        }

        private void SetCal1()
        {
            ProjectSpecificFactor.CalDone = false;
            ProjectSpecificFactor.Fbar_cal = true;
        }

        private void EnableCalContinue()
        {
            DataObject.CalContinue = true;
            DataObject.Fbar_cal = true;
            //ChoonChin - Enable 28 Ohm
            DataObject.Using28Ohm = true; 
        }
    }

    
}