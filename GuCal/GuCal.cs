﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using System.Runtime.InteropServices;
using Ionic.Zip;
using System.Threading;
using EqLib;


namespace GuCal
{
    public partial class GU
    {
        #region variable declarations

        public const bool ENABLE_GUCALVERIFY_MODULE = true;   // TOUCH
        public const bool ENABLE_ICC_CAL = false;  // No more Icc cal

        internal const bool passwordProtectUI = false;  // TOUCH. determines whether or not GU UI is password protected
        internal const string password = "avago";  // TOUCH. The password required to run GU, if passwordProtectUI = true

        internal const int minDutsForGu = 1;  // TOUCH. the minimum number of DUTs for valid GU Cal, else program complains

        public static string correlationFilePath = "";
        public static string iccCalFilePath = "";
        public static string correlationTemplatePath = "";
        public static string correlationTemplatePathRel = "";
        public static string iccCalTemplatePath = "";
        public static string benchDataPath = "";
        public static string benchDataPathRel = "";

        public static bool runningDCVerify = false;
        public static bool GuInitSuccess = true;
        public static GuModes[] GuMode;
        public static bool[] runningGUVerify = new bool[64];
        public static bool[] runningGU = new bool[64];
        public static bool[] runningGUIccCal = new bool[64];
        public static List<int> currentGuDutSN = new List<int>();

        internal static List<int> sitesAllExistingList = new List<int>();
        public static int selectedBatch = 1;
        public static int lastSelectedBatch = -1;
        public static Dictionary<int, List<int>> dutIdAllLoose = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> dutIdAllDemo = new Dictionary<int, List<int>>();
        public static List<int> dutIdLooseUserReducedList = new List<int>();
        internal static List<int> dutIDtestedDead = new List<int>();
        public static HashSet<int> dutIDfailedVrfy = new HashSet<int>();

        static bool[] DCCheckFailed;
        static bool[] GuIccCalFailed;
        static bool[] GuCorrFailed;
        static bool[] GuVerifyFailed;
        public static int[] GuVerifyFailureCount;
        static bool iccCalInaccuracyDetected = false;
        public static bool forceReload = false;
        static string prodTag = "";

        const string iccCalTestNameExtension = "_IccCal";
        const string iccCalFileNameExtension = "_IccCal";

        public static string testertype;
        public static byte siteNo = 0;

        public enum IccCalGain
        {
            InputGain, OutputGain
        }

        public static Dictionary.TripleKey<int, string, int, double> finalRefDataDict = new Dictionary.TripleKey<int, string, int, double>();   // [batchID, test name, dut ID, data value]
        static Dictionary.QuadKey<int, string, UnitType, int, double> demoDataDict = new Dictionary.QuadKey<int, string, UnitType, int, double>();   // [batchID, test name, loose/demo, dut ID, data value]
        static Dictionary.DoubleKey<int, string, double> demoBoardOffsets = new Dictionary.DoubleKey<int, string, double>();  // [batchID, test name, offset value]
        static Dictionary.DoubleKey<int, string, float> demoLooseCorrCoeff = new Dictionary.DoubleKey<int, string, float>();  // [batchID, test name, corr coefficient]
        static Dictionary.TripleKey<int, string, int, double> demoBoardOffsetsPerDut = new Dictionary.TripleKey<int, string, int, double>();   // [batchID, test name, dut ID, data value]
        static Dictionary<string, float> loLimCalAddDict = new Dictionary<string, float>();   // [test name, limit]
        static Dictionary<string, float> hiLimCalAddDict = new Dictionary<string, float>();   // [test name, limit]
        static Dictionary<string, float> loLimCalMultiplyDict = new Dictionary<string, float>();   // [test name, limit]
        static Dictionary<string, float> hiLimCalMultiplyDict = new Dictionary<string, float>();   // [test name, limit]
        static Dictionary<string, float> loLimVrfyDict = new Dictionary<string, float>();   // [test name, limit]
        static Dictionary<string, float> hiLimVrfyDict = new Dictionary<string, float>();   // [test name, limit]
        public static List<string> factorAddEnabledTests = new List<string>();    // list of test names that use Add calfactor
        public static List<string> factorMultiplyEnabledTests = new List<string>();    // list of test names that use Multiply calfactor

        static Dictionary<string, string> unitsDict = new Dictionary<string, string>();    // [test name, units]
        static Dictionary<string, bool> unitsAreDb = new Dictionary<string, bool>();    // [test name, are dB?]
        static Dictionary<string, int> testNumDict = new Dictionary<string, int>();    // [test name, test num]
        static Dictionary.TripleKey<int, string, int, double> rawAllCorrDataDict = new Dictionary.TripleKey<int, string, int, double>();   // [site, test name, dut ID, data value]    this is all the correlation raw data

        static Dictionary.TripleKey<int, string, int, double> rawAllMsrDataDict = new Dictionary.TripleKey<int, string, int, double>();   // [site, test name, dut ID, data value]    this is all the raw data
        static Dictionary.TripleKey<int, string, int, double> rawIccCalMsrDataDict = new Dictionary.TripleKey<int, string, int, double>();   // [site, test name, dut ID, data value] 
        static Dictionary.TripleKey<int, int, string, IccCalError> IccCalAvgErrorDict = new Dictionary.TripleKey<int, int, string, IccCalError>();   // [site, test name, dut ID, data value]
        static Dictionary.TripleKey<int, string, int, double> correctedMsrDataDict = new Dictionary.TripleKey<int, string, int, double>();
        static Dictionary.TripleKey<int, string, int, double> correctedMsrErrorDict = new Dictionary.TripleKey<int, string, int, double>();
        static Dictionary.TripleKey<int, string, int, bool> failedVerificationDict = new Dictionary.TripleKey<int, string, int, bool>();   // [site, test name, dut ID, data value]    this is all the raw data
        static Dictionary.DoubleKey<int, string, double> GuCalFactorsDict_origFromFile = new Dictionary.DoubleKey<int, string, double>();
        static Dictionary.DoubleKey<int, string, double> GuCalFactorsDict = new Dictionary.DoubleKey<int, string, double>();
        static Dictionary.DoubleKey<int, string, double> IccCalFactorsTempDict = new Dictionary.DoubleKey<int, string, double>();  // [site, test name, loss factor]  this is a temporary dictionary that is only populated when " + "GU Calibration" + " is running
        static Dictionary.DoubleKey<int, string, float> IccServoTargetCorrection = new Dictionary.DoubleKey<int, string, float>();  // [site, test name, error]  pre-existing icc servo errors get loaded into here
        static Dictionary.DoubleKey<int, string, float> IccServoNewTargetCorrection = new Dictionary.DoubleKey<int, string, float>();  // [site, test name, error]  updated icc servo errors get loaded into here
        static Dictionary<string, bool> ApplyIccServoTargetCorrection = new Dictionary<string, bool>();    //
        public static Dictionary.DoubleKey<int, string, double> IccServoVSGlevel = new Dictionary.DoubleKey<int, string, double>();  // [site, test name, VSG level]  this is used 
        static Dictionary.DoubleKey<int, string, double> IccServoNewVSGlevel = new Dictionary.DoubleKey<int, string, double>();  // [site, test name, VSG level]  this is used 
        static Dictionary.DoubleKey<int, string, float> corrCoeffDict = new Dictionary.DoubleKey<int, string, float>();    // [site, test name, correlation coefficient]
        static SortedList<int, string> benchTestNameList = new SortedList<int, string>();  // Test names found in bench data file. This is deemed the superset. Code checks that no extra non-zero CF parameters are found in correlation file or run-time tests
        static List<string> corrFileTestNameList = new List<string>();  // Test names found in correlation file.
        static List<string> iccCalTemplateTestNameList = new List<string>();  // Test names found in Icc Cal file.
        static SortedList<int, string> testedTestNameList = new SortedList<int, string>();  // code will ensure that testedTestNameList is a subset of benchTestNameList, for all non-zero CF parameters
        static HashSet<string> testNamesFailedVrfy = new HashSet<string>();
        public static byte currentGUattemptNumber = 1;
        static byte iccCalNumInaccurateAttempts = 0;

        static Dictionary<string, string> iccCalFactorRedirect = new Dictionary<string, string>();  // so that tests can use optionally use calfactors from other tests

        static ATFLogControl logger = ATFLogControl.Instance;  // for writing to Clotho Logger
        static WindowControl wnd = new WindowControl();
        public static List<string> loggedMessages = new List<string>();

        public static bool previousIccCalFactorsExist = false;
        static bool IccCalTemplateExists = false;

        static string IccCalStartTime = "";
        static string IccCalFinishTime = "";
        static string CorrStartTime = "";
        static string CorrFinishTimeHumanFriendly = "";
        static string CorrFinishTime = "";

        public static AllProductStatusFile guStatusFile;
        public static SingleProductStatus thisProductsGuStatus;

        public static Dictionary<string, CorrError> DicCorrError = new Dictionary<string, CorrError>();
        public static Dictionary<int, Dictionary<string, VerifyError>> DicVerifyError = new Dictionary<int, Dictionary<string, VerifyError>>();

        public class IccCalTestNames
        {
            public static List<string> All = new List<string>();   //full ordered list so we don't lose order

            public static Dictionary<string, IccCalTestNames> Pin = new Dictionary<string, IccCalTestNames>();
            public static Dictionary<string, IccCalTestNames> Pout = new Dictionary<string, IccCalTestNames>();
            public static Dictionary<string, IccCalTestNames> Icc = new Dictionary<string, IccCalTestNames>();
            public static Dictionary<string, IccCalTestNames> Key = new Dictionary<string, IccCalTestNames>();
            public static Dictionary.TripleKey<double, float, string, IccCalTestNames> Freq = new Dictionary.TripleKey<double, float, string, IccCalTestNames>();

            public string PoutTestName;
            public string PinTestName;
            public string IccTestName;
            public string KeyName;
            public float TargetPout;
            public double Frequency;
            public string IQname;

            private static object locker = new object();

            private IccCalTestNames(string pinTestName, string poutTestName, string iccTestName, string keyName, float targetPout, double frequency, string iqName)
            {
                this.PoutTestName = poutTestName;
                this.PinTestName = pinTestName;
                this.IccTestName = iccTestName;
                this.KeyName = keyName;
                this.TargetPout = targetPout;
                this.Frequency = frequency;
                this.IQname = iqName;

            }

            public static void Add(string pinTestName, string poutTestName, string iccTestName, string keyName, float targetPout, double frequency, string iqName)
            {
                lock (locker)
                {
                    if (All.Contains(pinTestName)) return;

                    keyName += iccCalTestNameExtension;

                    IccCalTestNames names = new IccCalTestNames(pinTestName, poutTestName, iccTestName, keyName, targetPout, frequency, iqName);

                    Pin.Add(pinTestName, names);
                    Pout.Add(poutTestName, names);
                    Icc.Add(iccTestName, names);
                    Key.Add(keyName, names);

                    Freq[frequency, targetPout, iqName] = names;

                    //Dictionary<string, IccCalTestNames> fhahd = Freq2[2505].OrderByDescending(t => t.Key).First().Value;   // for getting highest Pout conditions at certain freq

                    All.Add(pinTestName);
                    All.Add(poutTestName);
                    All.Add(iccTestName);
                }
            }

        }

        private class IccCalError
        {
            public float AvgError;
            public float HiLim;
            public float LoLim;

            public IccCalError(float avgError, float loLim, float hiLim)
            {
                this.AvgError = avgError;
                this.HiLim = hiLim;
                this.LoLim = loLim;
            }
        }

        public class CorrError
        {
            public int Gu_id;
            public string Parameter;
            public double HiLim;
            public double LoLim;
            public double Factor;

            public CorrError(int Gu_id, string Parameter, double loLim, double hiLim, double Factor)
            {
                this.Gu_id = Gu_id;
                this.Parameter = Parameter;
                this.HiLim = hiLim;
                this.LoLim = loLim;
                this.Factor = Factor;
            }
        }

        public class VerifyError
        {
            public int Gu_id;
            public string Parameter;
            public double HiLim;
            public double LoLim;
            public double correctedMsrError;

            public VerifyError(int Gu_id, string Parameter, double loLim, double hiLim, double correctedMsrError)
            {
                this.Gu_id = Gu_id;
                this.Parameter = Parameter;
                this.HiLim = hiLim;
                this.LoLim = loLim;
                this.correctedMsrError = correctedMsrError;
            }
        }

        public enum GuModes
        {
            //IccCorrVrfy,
            DCcheckCorrVrfy,
            CorrVrfy,
            DCcheckVrfy,
            Vrfy,
            None
        }

        #endregion


        public static class ProductionSettings
        {
            public static bool b_FlagProduction;
            public static bool b_Disable_MsgFlag;
        }

        public static UI UItype;

        public enum UI
        {
            FullAutoCal
        }

        public static void DoInit_afterCustomCode(int mustGUIccCal, UI UItype, bool productionMode, bool promptEachDevice, string productTag, string guZipRemoteSavePath, int numSites = 1)
        {
            try
            {
                runningGU = new bool[numSites];
                runningGUIccCal = new bool[numSites];
                runningGU = new bool[numSites];
                DCCheckFailed = new bool[numSites];
                GuIccCalFailed = new bool[numSites];
                GuCorrFailed = new bool[numSites];
                GuVerifyFailed = new bool[numSites];
                GuVerifyFailureCount = new int[numSites];
                GuMode = new GuModes[numSites];

                GU.UItype = UItype;
                prodTag = productTag;
                DataAnalysisFiles.guDataRemoteDir = guZipRemoteSavePath;
                GuInitSuccess = true;

                if (prodTag == "") prodTag = "Test";

                correlationFilePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");

                guStatusFile = AllProductStatusFile.ReadFromFileOrNew(@"C:\Avago.ATF.Common.x64\Database\GuCalLog.xml");
                thisProductsGuStatus = guStatusFile.GetSingleProductStatus(productTag);
          


                ProductionSettings.b_FlagProduction = productionMode;
                ProductionSettings.b_Disable_MsgFlag = !promptEachDevice;   // set True if to remove all message box

                if (!ENABLE_GUCALVERIFY_MODULE)
                {
                    return;
                }

                sitesAllExistingList = new List<int>();

                for (int site = 0; site < numSites; site++)
                {
                    sitesAllExistingList.Add(site);
                }

                ReadExcelTCF();
                ReadBenchData();
                ReadGuCorrelationTemplate();
                ReadIccCalfactorTemplate();
                ReadGuCorrelationFile();  // must do this after reading bench data

                //ReadIccCalfactorFile(); // remove Icc Cal for good
                previousIccCalFactorsExist = true;
                for (int site = 0; site < numSites; site++)
                {
                    runningGUIccCal[site] = false;
                }
                //GU.GuModes.IccCorrVrfy = 0;

                #region PCD Content validation for GUDATA files
                string ERR = "";
                List<string> filesToValidate = new List<string>
                {
                    benchDataPathRel,
                    correlationTemplatePathRel,
                };
                Tuple<bool, List<int>, int> retVal = ClothoPCDAPI.ValidateFiles(filesToValidate);
                if (!retVal.Item1)
                {
                    if (retVal.Item3 == 10)
                    {
                        ERR = "ClothoPCDAPI.ValidateFiles() does not support Development Mode.";
                        ATFLogControl.Instance.Log(LogLevel.Warn, ERR);
                        // Safe to ignore this error as Test Plan should continue to run in Development Mode.
                    }
                    else if (retVal.Item3 == 11)
                    {
                        ERR = "Package JSON file missing.";
                        MessageBox.Show(ERR, "pCD Package Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        ERR = "";
                        for (int i = 0; i < retVal.Item2.Count; i++)
                        {
                            switch (retVal.Item2[i])
                            {
                                case 100:
                                    ERR += "\n" + filesToValidate[i] + " : File has been modified.";
                                    break;
                                case 101:
                                    ERR += "\n" + filesToValidate[i] + " : File does not belong to this package.";
                                    break;
                                case 102:
                                    ERR += "\n" + filesToValidate[i] + " : File is missing from package.";
                                    break;
                                default:
                                    ERR += "\n" + filesToValidate[i] + " : Other unexpected validation error.";
                                    break;
                            }
                        }
                        ERR = "One or more file(s) in the package have been modified or the package is corrupted. (" + retVal.Item3.ToString() + ")" + ERR;
                        MessageBox.Show(ERR, "pCD Package Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        forceReload = true;
                    }
                }
                #endregion

                if (!GuInitSuccess)
                {
                    MessageBox.Show(wnd.ShowOnTop(), "GU Calibration" + " is unable to run due to file errors.\nPlease see LogService for error details.",
                        "GU Calibration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    LogToLogServiceAndFile(LogLevel.Error, "*** " + "GU Calibration" + " is unable to run due to file errors, please see above for error details");
                    if (ProductionSettings.b_FlagProduction) forceReload = true;
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in DoInit_afterCustomCode\n\n" + e.ToString(), "GU Calibration");
            }

        }

        public static void DoInit_afterUI()
        {
            try
            {
                // remove the unrequested dut data from our records, this is useful for averaging requested duts
                foreach (string testName in benchTestNameList.Values)
                {
                    foreach (int dutID in dutIdAllLoose[selectedBatch])
                    {
                        if (!dutIdLooseUserReducedList.Contains(dutID))
                        {
                            finalRefDataDict[selectedBatch][testName].Remove(dutID);
                        }
                    }
                }

                foreach (int site in runningGUIccCal.AllIndexOf(true))
                {
                    GuCalFactorsDict[site] = new Dictionary<string, double>();
                }

                if (!GuInitSuccess)   // if error peviously ocurred while opening Template files (that's the only way runningGU[site]CalVrfy would be false at this point)
                {
                    MessageBox.Show(wnd.ShowOnTop(), "GU Calibration" + " cannot run due to file errors.\nPlease see LogService for error details.",
                        "GU Calibration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    LogToLogServiceAndFile(LogLevel.Error, "*** " + "GU Calibration" + " cannot run due to file errors, please see above for error details");
                    forceReload = true;
                    runningGU.SetAll(false);
                    runningGUIccCal.SetAll(false);
                    return;
                }

                GuCorrFailed.SetAll(false);
                GuVerifyFailed.SetAll(false);
                GuIccCalFailed.SetAll(false);
                DCCheckFailed.SetAll(false);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in DoInit_afterCustomCode\n\n" + e.ToString(), "GU Calibration");
            }

        }


        public static void DoBeforeTest(List<int> guDutIndices, bool firstDevice, bool firstTrayInsertion, IProgress<string> progressObserver)
        {
            try
            {
                if (!ENABLE_GUCALVERIFY_MODULE) return;

                HandleForceReload(forceReload);

                if (runningGU.Contains(true))
                {
                    if (firstTrayInsertion && firstDevice)
                    {
                        foreach (int site in runningGU.AllIndexOf(true))
                        {
                            ResetParamsForRerun(site);   // moved from after GU Cal
                        }
                    }

                    UpdateCurrentGuDutSN(guDutIndices);

                    ReportProgressToUi(progressObserver);

                    RecordCalStartTimes(firstDevice);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in DoTest_beforeCustomCode\n\n" + e.ToString(), "GU Calibration");
            }
        }


        private static void HandleForceReload(bool forceReload)
        {
            if (forceReload)
            {
                DialogResult result = DialogResult.No;
                while (result != DialogResult.Yes)
                {
                    result = MessageBox.Show("Cannot continue until program is reloaded!\n\nPlease click Yes, then reload the program.",
                        "GU Calibration",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Hand,
                        MessageBoxDefaultButton.Button2);
                    Thread.Sleep(200);
                }
            }
        }

        private static void UpdateCurrentGuDutSN(List<int> guDutIndices)
        {
            currentGuDutSN.Clear();

            foreach (int dutIndex in guDutIndices)
            {
                if (dutIndex == -1) currentGuDutSN.Add(-1);
                else currentGuDutSN.Add(GU.dutIdLooseUserReducedList[dutIndex]);
            }

            foreach (int dutSN in currentGuDutSN)
            {
                if (dutSN == -1) continue;

                if (!dutIdLooseUserReducedList.Contains(dutSN))
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Serial number " + dutSN + " was read from device, but this is not in the list of expected serial numbers. Aborting " + "GU Calibration");
                    MessageBox.Show("Serial number " + dutSN + " was read from device, but is not in the list of expected serial numbers.\n\nAborting " + "GU Calibration", "GU Calibration", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    runningGU.SetAll(false);
                    forceReload = true;
                }

                if (Eq.Handler.Handler_Type == "MANUAL")
                    MessageBox.Show(string.Format("Insert STD Unit #" + dutSN + " to Site{0} and press ok.", siteNo+1));
            }

            MessageBoxAsync.Wait();
        }

        private static void ReportProgressToUi(IProgress<string> progressObserver)
        {
            string progressText = "";

            foreach (int site in runningGU.AllIndexOf(true))
            {
                if (currentGuDutSN[0] == -1) continue;
                if (runningGUIccCal.Contains(true) && !runningGUIccCal[site]) continue;

                string modeMsg = runningGUIccCal[site] ? " for Icc Calibration" : " for Corr / Verify";
                string s = "GU Device #" + currentGuDutSN[0] + " being run on site " + (site + 1) + modeMsg;
                LogToLogServiceAndFile(LogLevel.HighLight, s);
                progressText += s + "\n";
            }

            progressObserver.Report(progressText);
        }

        private static void RecordCalStartTimes(bool firstDevice)
        {
            if (firstDevice)
            {
                if (runningGUIccCal.Contains(true))
                {
                    IccCalStartTime = string.Format("{0:yyyy_M_d H:m:s}", DateTime.Now);
                }
                else
                {
                    CorrStartTime = string.Format("{0:yyyy_M_d H:m:s}", DateTime.Now);
                }
            }
        }

        public static void DoTest_afterCustomCode(ref ATFReturnResult res)
        {
            // I believe this method will be obsolete now - db

            //try
            //{
            //    if (!runningGU[site] | !ENABLE_GUCALVERIFY_MODULE) return;

            //    StoreMeasuredData(new List<int>{1,2,3,4}, ref res);

            //    currentGuDutIndex++;
            //    if (currentGuDutIndex > dutIdLooseUserReducedList.Count - 1)  // site done, if we've tested all the duts on this site
            //    {
            //        currentGuDutIndex = 0;  // reset dut index for next site

            //        AllDutsDone();

            //    }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("Error during DoTest_afterCustomCode\n\n" + e.ToString(), "GU Calibration");
            //}
        }


        public static void StoreMeasuredData(ATFReturnResult Result, IEnumerable<int> sitesToStore, bool isCorrelation = false)
        {
            foreach (int site in sitesToStore)
            {
                if (!runningGU[site]) continue;

                foreach (ATFReturnPararResult param in Result.Data)
                {
                    if (!benchTestNameList.Values.Contains(param.Name)) continue;   // don't record tests that aren't in the bench data file

                    if (!testedTestNameList.ContainsValue(param.Name)) testedTestNameList.Add(testNumDict[param.Name], param.Name);

                    double rawMsrData = param.Vals[site];

                    rawAllMsrDataDict[site, param.Name, currentGuDutSN[site]] = rawMsrData;  // record all the raw data

                    if (isCorrelation)
                    {
                        rawAllCorrDataDict[site, param.Name, currentGuDutSN[site]] = rawMsrData;    // record all the correlation raw data (JJ - 13-Oct-2021)
                    }

                    #region Flag Bad Contacts

                    if (IccCalTestNames.Pout.ContainsKey(param.Name))
                    {
                        float deadError = 0f;
                        if (runningGUIccCal[site])
                        {
                            if (finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]] > 10f) deadError = 5f;
                            else deadError = 15f;
                        }
                        else
                        {
                            deadError = 5f;
                        }

                        if (Math.Abs(rawMsrData - finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]]) > deadError)
                        {
                            LogToLogServiceAndFile(LogLevel.Warn, "\nPossible contact issue: site " + (site + 1) + ", device" + currentGuDutSN[site] + ", Measured Pout = " + rawMsrData + ", Reference Pout = " + finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]] + ", for test " + param.Name + "\n        So, device " + currentGuDutSN[site] + " data will be ignored. " + (GuCorrFailed[site] ? "" : "GU Calibration" + " can still pass"));
                            if (!dutIDtestedDead.Contains(currentGuDutSN[site]))
                            {
                                dutIDtestedDead.Add(currentGuDutSN[site]);
                                if ((float)dutIDtestedDead.Count() >= (float)dutIdLooseUserReducedList.Count() * 0.35f)
                                {
                                    if (runningGU[site])
                                    {
                                        LogToLogServiceAndFile(LogLevel.Error, "\nToo many contact issues detected on site " + (site + 1) + ", cannot pass " + "GU Calibration");
                                    }

                                    GuCorrFailed[site] = true;
                                }
                            }
                        }
                    }
                    #endregion

                } // slicing through sb
            }
        }

        public static void StoreMeasuredData(ATFReturnResult Result, int site, bool isCorrelation = false)
        {
            //foreach (int site in sitesToStore)
            //{
            //if (!runningGU[site]) continue;

            foreach (ATFReturnPararResult param in Result.Data)
            {
                if (!benchTestNameList.Values.Contains(param.Name)) continue;   // don't record tests that aren't in the bench data file

                if (!testedTestNameList.ContainsValue(param.Name)) testedTestNameList.Add(testNumDict[param.Name], param.Name);

                //float rawMsrData = (float)param.Vals[site];
                double rawMsrData = param.Vals[0];

                //rawAllMsrDataDict[site, param.Name, currentGuDutSN[site]] = rawMsrData;  // record all the raw data
                rawAllMsrDataDict[site, param.Name, currentGuDutSN[0]] = rawMsrData;

                if (isCorrelation)
                {
                    //rawAllCorrDataDict[site, param.Name, currentGuDutSN[site]] = rawMsrData;    // record all the correlation raw data (JJ - 13-Oct-2021)
                    rawAllCorrDataDict[site, param.Name, currentGuDutSN[0]] = rawMsrData;
                }

                #region Flag Bad Contacts

                if (IccCalTestNames.Pout.ContainsKey(param.Name))
                {
                    float deadError = 0f;
                    if (runningGUIccCal[site])
                    {
                        if (finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[0]] > 10f) deadError = 5f;
                        //if (finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]] > 10f) deadError = 5f;
                        else deadError = 15f;
                    }
                    else
                    {
                        deadError = 5f;
                    }

                    //if (Math.Abs(rawMsrData - finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]]) > deadError)
                    if (Math.Abs(rawMsrData - finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[0]]) > deadError)
                    {
                        //LogToLogServiceAndFile(LogLevel.Warn, "\nPossible contact issue: site " + (site + 1) + ", device" + currentGuDutSN[site] + ", Measured Pout = " + rawMsrData + ", Reference Pout = " + finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[site]] + ", for test " + param.Name + "\n        So, device " + currentGuDutSN[site] + " data will be ignored. " + (GuCorrFailed[site] ? "" : "GU Calibration" + " can still pass"));
                        LogToLogServiceAndFile(LogLevel.Warn, "\nPossible contact issue: site " + (site + 1) + ", device" + currentGuDutSN[0] + ", Measured Pout = " + rawMsrData + ", Reference Pout = " + finalRefDataDict[selectedBatch, param.Name, currentGuDutSN[0]] + ", for test " + param.Name + "\n        So, device " + currentGuDutSN[0] + " data will be ignored. " + (GuCorrFailed[site] ? "" : "GU Calibration" + " can still pass"));
                        //if (!dutIDtestedDead.Contains(currentGuDutSN[site]))
                        if (!dutIDtestedDead.Contains(currentGuDutSN[0]))
                        {
                            //dutIDtestedDead.Add(currentGuDutSN[site]);
                            dutIDtestedDead.Add(currentGuDutSN[0]);
                            if ((float)dutIDtestedDead.Count() >= (float)dutIdLooseUserReducedList.Count() * 0.35f)
                            {
                                if (runningGU[site])
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "\nToo many contact issues detected on site " + (site + 1) + ", cannot pass " + "GU Calibration");
                                }

                                GuCorrFailed[site] = true;
                            }
                        }
                    }
                }
                #endregion

                //} // slicing through sb
            }
        }

        public static void AllDutsDoneCorrelation()
        {
            foreach (int site in runningGU.AllIndexOf(true))
            {
                foreach (string testName in testedTestNameList.Values)
                {
                    double measuredAvg = 0;
                    double benchAvg = 0;
                    double factor = 0;

                    ComputeCorrelationFactors(site, testName, ref measuredAvg, ref benchAvg, ref factor);
                }
            }

            if (runningGUIccCal.Contains(true))
            {
                FinishedIccCal();
            }
        }

        public static void AllDutsDoneVerification()
        {
            dutIDfailedVrfy.Clear();
            testNamesFailedVrfy.Clear();

            foreach (int site in runningGU.AllIndexOf(true))
            {
                foreach (string testName in testedTestNameList.Values)
                {
                    double measuredAvg = 0;
                    double benchAvg = 0;

                    ComputeAverageMeasurement(site, testName, ref measuredAvg, ref benchAvg);

                    if (!runningGUIccCal[site])
                    {
                        VerificationCheck(site, testName);

                        CorrelationCoefficientCheck(site, testName);

                        //IccCalErrorAndCompensation(site, testName, measuredAvg, benchAvg);
                    }
                }

                if (GuVerifyFailed[site] == true)
                {
                    if (GuVerifyFailureCount[site] == 0 ||
                        lastSelectedBatch != selectedBatch)
                    {
                        lastSelectedBatch = selectedBatch;
                        GuVerifyFailureCount[site] = 1;
                    }
                    else
                    {
                        GuVerifyFailureCount[site] += 1;
                    }
                }
                else
                {
                    lastSelectedBatch = selectedBatch;
                    GuVerifyFailureCount[site] = 0;
                }

                if (GU.runningDCVerify)
                {
                    DCCheckFailed[site] = true;
                }
            }

            FinishedEntireGUCal();

            string status = "";

            if (GU.thisProductsGuStatus.VerifyIsOptional(siteNo))   // pass in site
            {
                status = "VALID";
            }
            if (ENABLE_ICC_CAL && !GU.thisProductsGuStatus[siteNo].iccCalPassed)
            {
                status += "ICC FAILED  ";
            }
            if (!GU.thisProductsGuStatus[siteNo].correlationFactorsPassed)
            {
                status += "CORR FAILED  ";
            }
            if (!GU.thisProductsGuStatus[siteNo].verificationPassed)
            {
                status += "VERIFY FAILED  ";
            }

            MessageBox.Show(wnd.ShowOnTop(), status, "GU CAL STATUS", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static void ComputeAverageMeasurement(int site, string testName, ref double measuredAvg, ref double benchAvg)
        {
            // filter out dead parts
            if (dutIDtestedDead.Count() > 0 && dutIDtestedDead.Count < rawAllMsrDataDict[site][testName].Count)
            {
                measuredAvg = rawAllMsrDataDict[site][testName].Where(x => !dutIDtestedDead.Contains(x.Key)).ToDictionary(pair => pair.Key, pair => pair.Value).Values.Average();
                benchAvg = finalRefDataDict[selectedBatch][testName].Where(x => !dutIDtestedDead.Contains(x.Key)).ToDictionary(pair => pair.Key, pair => pair.Value).Values.Average();
            }
            else
            {
                measuredAvg = rawAllMsrDataDict[site][testName].Values.Average();
                benchAvg = finalRefDataDict[selectedBatch][testName].Values.Average();
            }
        }

        public static void ComputeCorrelationFactors(int site, string testName, ref double measuredAvg, ref double benchAvg, ref double factor)
        {
            measuredAvg = 0;
            benchAvg = 0;
            factor = 0; // initialize to 0 in case there is no calfactor being used for this test                    

            //if (GuMode[site] != GuModes.Vrfy)
            //    {
            ComputeAverageMeasurement(site, testName, ref measuredAvg, ref benchAvg);

            float loLimCal = 0;
            float hiLimCal = 0;

            if (runningGUIccCal[site])   // Icc Cal limits
            {
                if (IccCalTestNames.Pout.ContainsKey(testName))
                {
                    double PinMeasuredAvg = rawAllMsrDataDict[site][IccCalTestNames.Pout[testName].PinTestName].Values.Average();
                    double PinBenchAvg = finalRefDataDict[selectedBatch][IccCalTestNames.Pout[testName].PinTestName].Values.Average();

                    double iccCalFactor_inputGain = PinBenchAvg - PinMeasuredAvg;
                    double iccCalFactor_outputGain = measuredAvg - benchAvg;

                    IccCalFactorsTempDict[site, IccCalTestNames.Pout[testName].KeyName + IccCalGain.OutputGain] = iccCalFactor_outputGain;
                    IccCalFactorsTempDict[site, IccCalTestNames.Pout[testName].KeyName + IccCalGain.InputGain] = iccCalFactor_inputGain;

                    IccServoNewVSGlevel[site, IccCalTestNames.Pout[testName].KeyName] = PinBenchAvg;

                    if (factorAddEnabledTests.Contains(IccCalTestNames.Pout[testName].KeyName))
                    {
                        loLimCal = loLimCalAddDict[IccCalTestNames.Pout[testName].KeyName];
                        hiLimCal = hiLimCalAddDict[IccCalTestNames.Pout[testName].KeyName];
                    }
                    else   // if no previous Icc Calfactor file, or new test
                    {
                        loLimCal = -999;
                        hiLimCal = 999;
                    }

                    // check for limits failure
                    if (iccCalFactor_outputGain < loLimCal | iccCalFactor_outputGain > hiLimCal)
                    {
                        GuIccCalFailed[site] = true;
                        LogToLogServiceAndFile(LogLevel.Warn, "*** Failed GU Icc Cal-factor limits, site " + site + ", test " + testNumDict[testName] + ", " + IccCalTestNames.Pout[testName].KeyName + IccCalGain.OutputGain + ", LowL: " + loLimCal + ", calfactor: " + iccCalFactor_outputGain + ", HighL: " + hiLimCal);
                    }
                    if (iccCalFactor_inputGain < loLimCal | iccCalFactor_inputGain > hiLimCal)
                    {
                        GuIccCalFailed[site] = true;
                        LogToLogServiceAndFile(LogLevel.Warn, "*** Failed GU Icc Cal-factor limits, site " + site + ", test " + testNumDict[testName] + ", " + IccCalTestNames.Pout[testName].KeyName + IccCalGain.InputGain + ", LowL: " + loLimCal + ", calfactor: " + iccCalFactor_inputGain + ", HighL: " + hiLimCal);
                    }

                }

            }

            else   // Correlation limits
            {
                if (factorAddEnabledTests.Contains(testName))
                {
                    factor = benchAvg - measuredAvg;
                    loLimCal = loLimCalAddDict[testName];
                    hiLimCal = hiLimCalAddDict[testName];
                }

                else if (factorMultiplyEnabledTests.Contains(testName))   // multiplication calfactor
                {
                    factor = benchAvg / measuredAvg;
                    loLimCal = loLimCalMultiplyDict[testName];
                    hiLimCal = hiLimCalMultiplyDict[testName];
                }

                GuCalFactorsDict[site, testName] = factor;  // store the GU corr-factor to dictionary

                // check for limits failure
                if (factor < loLimCal | factor > hiLimCal)
                {
                    if (!DicCorrError.ContainsKey(testName)) DicCorrError.Add(testName, new CorrError(0,testName, loLimCal, hiLimCal, factor));
                    GuCorrFailed[site] = true;
                    LogToLogServiceAndFile(LogLevel.Warn, "*** Failed GU Corr-factor limits, site " + site + ", test " + testNumDict[testName] + ", " + testName + ", LowL: " + loLimCal + ", calfactor: " + factor + ", HighL: " + hiLimCal);
                }

            }
            //}
            //else
            //{
            //    ATFCrossDomainWrapper.Correlation_ApplyCorFactorToParameter(testName, ref factor);  this call is way too slow. will run vrfy only just like full cal   KH 1/29/2018
            //}
        }


        public static void VerificationCheck(int site, string testName)
        {
            if (runningGUIccCal[site]) return;

            if (DicVerifyError.Count == 0)
            {
                foreach (int dutID in dutIdLooseUserReducedList)
                {
                    DicVerifyError.Add(dutID, new Dictionary<string, VerifyError>());
                }
            }

            foreach (int dutID in dutIdLooseUserReducedList)
            {
                //VERIFICATION SECTION
                // loop through each device
                // apply calfactor to measured data (becomes "corrected data")
                // and check against verification limits

                // grab dictionary values for easier debug viewing
                double rawMsrData = rawAllMsrDataDict[site, testName, dutID];   // use duts final loop for verification data
                double benchData = finalRefDataDict[selectedBatch, testName, dutID];

                double correctedMsrData = 0;
                double correctedMsrError = 0;

                // apply GU calfactor to data ("corrected data") and compute error ("corrected error")
                if (factorAddEnabledTests.Contains(testName))   // addition calfactor
                {
                    correctedMsrData = rawMsrData + GuCalFactorsDict[site, testName];
                }
                else if (factorMultiplyEnabledTests.Contains(testName))   // multiplication calfactor
                {
                    correctedMsrData = rawMsrData * GuCalFactorsDict[site, testName];
                }
                else
                {
                    correctedMsrData = rawMsrData;  // no GU calfactor being applied
                }

                correctedMsrDataDict[site, testName, dutID] = correctedMsrData;

                correctedMsrError = correctedMsrData - benchData;
                correctedMsrErrorDict[site, testName, dutID] = correctedMsrError;

                // check corrected data against verification limits
                if (!dutIDtestedDead.Contains(dutID) & (correctedMsrError < loLimVrfyDict[testName] | correctedMsrError > hiLimVrfyDict[testName]))
                {
                    DicVerifyError[dutID].Add(testName, new VerifyError(dutID, testName, loLimVrfyDict[testName], hiLimVrfyDict[testName], correctedMsrError));

                    GuVerifyFailed[site] = true;
                    LogToLogServiceAndFile(LogLevel.Warn, "*** Failed GU Verification limits, site " + (site+1) + ", Device " + dutID + ", test " + testNumDict[testName] + ", " + testName + ", LowL: " + loLimVrfyDict[testName] + ", measure error: " + correctedMsrError + ", HighL: " + hiLimVrfyDict[testName]);

                    dutIDfailedVrfy.Add(dutID);
                    testNamesFailedVrfy.Add(testName);
                }
            }
        }


        public static void CorrelationCoefficientCheck(int site, string testName)
        {
            if (runningGUIccCal[site]) return;

            if (dutIDtestedDead.Count() == 0)
            {
                double[] benchArray = finalRefDataDict[selectedBatch][testName].Values.ToArray();
                double[] measArray = correctedMsrDataDict[site][testName].Values.ToArray();
                corrCoeffDict[site, testName] = pearsoncorr2(benchArray, measArray, benchArray.Length);
            }
            else if (dutIDtestedDead.Count < rawAllMsrDataDict[site][testName].Count)
            {
                double[] benchArray = finalRefDataDict[selectedBatch][testName].Where(x => !dutIDtestedDead.Contains(x.Key)).ToDictionary(pair => pair.Key, pair => pair.Value).Values.ToArray();
                double[] measArray = correctedMsrDataDict[site][testName].Where(x => !dutIDtestedDead.Contains(x.Key)).ToDictionary(pair => pair.Key, pair => pair.Value).Values.ToArray();
                corrCoeffDict[site, testName] = pearsoncorr2(benchArray, measArray, benchArray.Length);
            }
            else
            {
                corrCoeffDict[site, testName] = 0;
            }

        }


        //public static void IccCalErrorAndCompensation(int site, string testName, float measuredAvg, float benchAvg)
        //{
        //    if (runningGUIccCal[site]) return;

        //    if (GuMode[site] == GuModes.IccCorrVrfy && IccCalTestNames.Icc.Keys.Contains(testName))
        //    {
        //        float averageIccError = measuredAvg - benchAvg;  // this is raw error, not corrected error

        //        float limitFractional = 0.01f;  // 1%
        //        IccCalAvgErrorDict[site, currentGUattemptNumber, testName] = new IccCalError(averageIccError, -benchAvg * limitFractional, benchAvg * limitFractional);
        //        float fractionalError = Math.Abs(averageIccError / benchAvg);
        //        if (fractionalError > limitFractional)
        //        {
        //            iccCalInaccuracyDetected = true;
        //            LogToLogServiceAndFile(LogLevel.Warn, "*** Icc average error > " + (limitFractional * 100f) + "%, may need rerun Icc Cal to improve accuracy. Site " + (site + 1) + ", Test " + testName + ", average error: " + averageIccError + "A = " + (fractionalError * 100f) + "%");
        //        }

        //        if (ApplyIccServoTargetCorrection[IccCalTestNames.Icc[testName].KeyName])
        //            IccServoNewTargetCorrection[site, IccCalTestNames.Icc[testName].KeyName] = IccServoTargetCorrection[site, IccCalTestNames.Icc[testName].KeyName] - averageIccError * 0.8f;
        //        else
        //            IccServoNewTargetCorrection[site, IccCalTestNames.Icc[testName].KeyName] = 0;

        //    }

        //}


        public static void FinishedIccCal()
        {
            IccCalFinishTime = string.Format("{0:yyyy_M_d H:m:s}", DateTime.Now);

            foreach (int site in runningGUIccCal.AllIndexOf(true))
            {
                GuCalFactorsDict[site] = new Dictionary<string, double>(IccCalFactorsTempDict[site]);   // going into the actual GU Cal, we'll keep the Icc Calfactors only.
                rawIccCalMsrDataDict[site] = new Dictionary.DoubleKey<string, int, double>(rawAllMsrDataDict[site]);
                rawAllMsrDataDict[site].Clear();
                rawAllCorrDataDict[site].Clear();

                if (!GuIccCalFailed[site])
                {
                    IccServoVSGlevel[site] = new Dictionary<string, double>(IccServoNewVSGlevel[site]);
                }
            }

            runningGUIccCal.SetAll(false);
        }


        public static void FinishedEntireGUCal()
        {
            DateTime datetime = DateTime.Now;

            CorrFinishTimeHumanFriendly = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", datetime);
            CorrFinishTime = string.Format("{0:yyyy_M_d H:m:s}", datetime);

            UpdateGuProductStatusFile(datetime);

            //ChoonChin (20200508) - Add XML checksum
            //StoreChecksum(ReturnXMLChecksum()); Disable for NHWK RF1 - ChoonChin

            LogFinalResultSummary();

            DataAnalysisFiles.WriteAll();

            //WriteCorrAndIccCalFilesIfNecessary();
            WriteCorrFilesIfNecessary();

            currentGUattemptNumber++;

            //foreach (int site in runningGU.AllIndexOf(true))
            //{
            //    ResetParamsForRerun(site);   // todo: should we be doing before, not after GU cal?
            //}

            runningGU.SetAll(false);
        }

        //private static void  WriteCorrAndIccCalFilesIfNecessary()
        //{
        //    if (GuMode.Contains(GuModes.IccCorrVrfy) & !previousIccCalFactorsExist & !IccCalTemplateExists)
        //    {
        //        WriteIccCalfactorFile();  // create Icc Calfactor file from scratch if no file/template exists. This helps developers get going faster.
        //        runningGU.SetAll(false);
        //        forceReload = true;
        //        MessageBoxAsync.Show("GU Calibration" + " dummy run completed.\n\nA default Icc Cal file has been created at:\n\n" + iccCalFilePath + "\n\nPlease use this default Icc Cal file to create an Icc Cal Template file,\nthen reload the program and rerun " + "GU Calibration", "GU Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        //        return;
        //    }

        //    bool anySitePassedIccCal =
        //        GuMode.AllIndexOf(GuModes.IccCorrVrfy)
        //        .Intersect(GuIccCalFailed.AllIndexOf(false)).Count() > 0;

        //    bool anySitePassedCorr =
        //        GuMode.AllIndexOf(GuModes.IccCorrVrfy, GuModes.CorrVrfy)
        //        .Intersect(GuCorrFailed.AllIndexOf(false)).Count() > 0;

        //    if (anySitePassedIccCal) WriteIccCalfactorFile();
        //    if (anySitePassedCorr) WriteGuCorrelationFile();
        //}

        private static void WriteCorrFilesIfNecessary()
        {
            bool anySitePassedCorr =
                GuMode.AllIndexOf(GuModes.CorrVrfy)
                .Intersect(GuCorrFailed.AllIndexOf(false)).Count() > 0;

            if (anySitePassedCorr) WriteGuCorrelationFile();
        }

        public static void ResetDCCheckParams()
        {
            DCCheckFailed.SetAll(false);
        }

        private static void ResetParamsForRerun(int site)
        {
            GuCorrFailed[site] = false;
            GuVerifyFailed[site] = false;
            GuIccCalFailed[site] = false;
            DCCheckFailed[site] = false;

            if (GuMode[site] != GuModes.Vrfy && GuVerifyFailureCount[site] > 0)
            {
                GuVerifyFailureCount[site] = 0;
                dutIDfailedVrfy.Clear();
            }
            testNamesFailedVrfy.Clear();


            //runningGUIccCal[site] = ENABLE_ICC_CAL & GuMode[site] == GuModes.IccCorrVrfy;
            runningGUIccCal[site] = ENABLE_ICC_CAL;

            if (GuVerifyFailureCount[site] == 0)
            {
                //clear previous measurements
                if (rawAllMsrDataDict.ContainsKey(site))
                {
                    rawAllMsrDataDict[site].Clear();
                }

                if (rawAllCorrDataDict.ContainsKey(site))
                {
                    rawAllCorrDataDict[site].Clear();
                }

                if (correctedMsrDataDict.ContainsKey(site))
                {
                    correctedMsrDataDict[site].Clear();
                }

                if (correctedMsrErrorDict.ContainsKey(site))
                {
                    correctedMsrErrorDict[site].Clear();
                }

                if (dutIDtestedDead.Count > 0)
                {
                    dutIDtestedDead.Clear();
                }
            }

            iccCalInaccuracyDetected = false;

            //if (GuMode[site] == GuModes.IccCorrVrfy)
            //{
            //        if (GuCalFactorsDict.ContainsKey(site))   //Clear out Cal Factors for all but Verify only run
            //        {
            //            GuCalFactorsDict[site].Clear();
            //        }

            //    if (IccCalFactorsTempDict.ContainsKey(site))
            //    {
            //        IccCalFactorsTempDict[site].Clear();
            //    }


            //    if (IccServoNewTargetCorrection.ContainsKey(site))
            //    {
            //        // In case Icc servo needs to learn Icc target compensation
            //        IccServoTargetCorrection[site] = new Dictionary<string, float>(IccServoNewTargetCorrection[site]);

            //        IccServoNewTargetCorrection[site].Clear();
            //    }

            //    if (IccServoNewVSGlevel.ContainsKey(site))
            //    {
            //        IccServoNewVSGlevel[site].Clear();
            //    }

            //}

            if (corrCoeffDict.ContainsKey(site))
            {
                corrCoeffDict[site].Clear();
            }
        }

        private static void UpdateGuProductStatusFile(DateTime datetime)
        {
            foreach (int site in runningGU.AllIndexOf(true))
            {
                if (!ENABLE_ICC_CAL)
                {
                    thisProductsGuStatus[site].dateOfLastIccAttempt = datetime;
                    thisProductsGuStatus[site].iccCalPassed = true;
                }

                switch (GuMode[site])
                {
                    case GuModes.DCcheckCorrVrfy:

                        thisProductsGuStatus[site].dateOfLastDCCheckAttempt = datetime;
                        thisProductsGuStatus[site].dcCheckPassed = !DCCheckFailed[site];

                        goto case GuModes.CorrVrfy;

                    case GuModes.CorrVrfy:

                        thisProductsGuStatus[site].dateOfLastCorrAttempt = datetime;
                        thisProductsGuStatus[site].correlationFactorsPassed = !GuCorrFailed[site];

                        goto case GuModes.Vrfy;

                    case GuModes.DCcheckVrfy:

                        thisProductsGuStatus[site].dateOfLastDCCheckAttempt = datetime;
                        thisProductsGuStatus[site].dcCheckPassed = !DCCheckFailed[site];

                        goto case GuModes.Vrfy;

                    case GuModes.Vrfy:

            
                        thisProductsGuStatus[site].dateOfLastVerifyAttempt = datetime;
                        thisProductsGuStatus[site].verificationPassed = !GuVerifyFailed[site];

                        bool status = System.IO.Path.GetFileName(GU.correlationFilePath).ToUpper().Replace(".CSV", "") == GU.thisProductsGuStatus.IsCFFileVerify(0).ToUpper() ? true : false;

                        thisProductsGuStatus[site].cFFileCheck = System.IO.Path.GetFileName(GU.correlationFilePath).ToUpper().Replace(".CSV", "");

                        if (runningDCVerify)
                        {
                            thisProductsGuStatus[site].dateOfLastDCCheckAttempt = datetime;
                            thisProductsGuStatus[site].dcCheckPassed = !DCCheckFailed[site];
                        }

                        break;
                }
            }

            foreach (int site in runningGUVerify.AllIndexOf(true))
            {

            }

            guStatusFile.SaveToFile();
        }

        private static void LogFinalResultSummary()
        {
            StringBuilder msgFinal = new StringBuilder();

            msgFinal.Append("\r\n\r\n-------------------------------------------------------------");
            msgFinal.Append("\r\n----  " + "GU Calibration" + " COMPLETE");


            foreach (int site in runningGU.AllIndexOf(true))
            {
                switch (GuMode[site])
                {
                    //case GuModes.IccCorrVrfy:

                    //    if (GuIccCalFailed[site]) msgFinal.Append("\r\n****  Site " + site + " Final result:  GU Icc Calibration FAILED");
                    //    else msgFinal.Append("\r\n----  Site " + site + " Final result:  GU Icc Calibration PASSED");

                    //    goto case GuModes.CorrVrfy;

                    case GuModes.CorrVrfy:

                        if (GuCorrFailed[site]) msgFinal.Append("\r\n****  Site " + (site+1) + " Final result:  GU Correlation FAILED");
                        else msgFinal.Append("\r\n----  Site " + (site+1) + " Final result:  GU Correlation PASSED");

                        goto case GuModes.Vrfy;

                    case GuModes.Vrfy:

                        if (GuVerifyFailed[site]) msgFinal.Append("\r\n****  Site " + (site+1) + " Final result:  GU Verification FAILED");
                        else msgFinal.Append("\r\n----  Site " + (site+1) + " Final result:  GU Verification PASSED");

                        break;
                }

            }

            msgFinal.Append("\r\n-------------------------------------------------------------");
            LogToLogServiceAndFile(LogLevel.HighLight, msgFinal.ToString());
        }

        private static void InstructNextUnit(int site)
        {
            if (!runningGU[site]) return;

            string msgTitle = runningGUIccCal[site] ? "Next GU Unit - Icc Cal" : "Next GU Unit";

            StringBuilder msg = new StringBuilder();
            msg.Append("      Please prepare the next GU Device #s " + currentGuDutSN[site]);
            msg.Append("\n      remain using SITE " + site);

            MessageBox.Show(wnd.ShowOnTop(), msg.ToString(), msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            LogToLogServiceAndFile(LogLevel.HighLight, msg.ToString());

        }


        private static void ReadExcelTCF()
        {
            // Get GuBenchDataFile_Path from Excel test condition file

            string basePath = GetTestPlanPath();

            bool continueSheets = true;
            int sheet = 0;

            // search through Excel test condition file for GuBenchDataFile_Path
            while (continueSheets & sheet < 50)
            {
                try
                {
                    string temp = ATFCrossDomainWrapper.Excel_Get_Input(++sheet, 1, 1);
                }
                catch
                {
                    break;   // stop searching if sheet doesn't exist
                }

                int numRows = 100;
                int numCols = 2;

                Tuple<bool, string, string[,]> sheetContents = ATFCrossDomainWrapper.Excel_Get_IputRange(sheet, 1, 1, numRows, numCols);

                bool continueRows = true;
                int row = -1;

                while (continueRows & row < numRows - 1)
                {
                    string cellValue = sheetContents.Item3[++row, 0];  // row and column appear to be safe from exceptions, but sheet can throw exception if doesn't exist

                    switch (cellValue.Replace("_", "").ToLower())
                    {
                        case "gubenchdatafilerelpath":
                            benchDataPath = basePath + sheetContents.Item3[row, 1];
                            benchDataPathRel = sheetContents.Item3[row, 1];
                            break;
                        case "gucorrtemplaterelpath":
                            correlationTemplatePath = basePath + sheetContents.Item3[row, 1];
                            correlationTemplatePathRel = sheetContents.Item3[row, 1];
                            break;
                        case "guicccaltemplaterelpath":
                            iccCalTemplatePath = basePath + sheetContents.Item3[row, 1];
                            break;
                        case "#end":
                            continueRows = false;
                            break;
                    }
                }

                if (benchDataPath != "" & correlationTemplatePath != "" & iccCalTemplatePath != "") break;

            }

            if (correlationTemplatePath == "")
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: GuCorrTemplate_RelPath not found in TCF.\n        Cannot run GU Cal.");
                GuInitSuccess = false;
            }
            else if (!File.Exists(correlationTemplatePath))
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: No GU Correlation Template found at:\n" + correlationTemplatePath + "\n        Cannot run GU Cal.");
                GuInitSuccess = false;
            }

            IccCalTemplateExists = File.Exists(iccCalTemplatePath);

        }


        private static void ReadBenchData()
        {
            const int START_COL = 11;   // this first column in the csv file which is a test parameter
            int batchColumn = 0;  // Identify the Package Column {Set "DIE_X" as Package for Production)

            List<string> benchNamesTemp = new List<string>();

            Dictionary.QuadKey<int, string, int, UnitType, List<double>> allData = new Dictionary.QuadKey<int, string, int, UnitType, List<double>>();    // [batchID, testName, dut#, loose/demo, list of measurements]
            dutIdAllLoose = new Dictionary<int, List<int>>();
            dutIdAllDemo = new Dictionary<int, List<int>>();
            testNumDict = new Dictionary<string, int>();
            benchTestNameList = new SortedList<int, string>();
            unitsDict = new Dictionary<string, string>();
            unitsAreDb = new Dictionary<string, bool>();

            if (benchDataPath == "")
            {
                GuInitSuccess = false;
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: GuBenchDataFile_Path not found in Excel Test Condition File. Cannot run " + "GU Calibration" + ".");
                return;
            }
            else if (!File.Exists(benchDataPath))
            {
                GuInitSuccess = false;
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Bench Data File doesn't exist: " + benchDataPath + ". Cannot run " + "GU Calibration" + ".");
                return;
            }

            bool TolHiVrfyFound = false;
            bool TolLoVrfyFound = false;

            using (StreamReader br = new StreamReader(new FileStream(benchDataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                while (!br.EndOfStream)
                {
                    string[] csvLine = br.ReadLine().Split(',');

                    switch (csvLine[0].ToLower())
                    {
                        case "parameter":
                            //benchNamesTemp = csvLine.Skip(START_COL - 1).TakeWhile(testName => testName != "PassFail").ToList();    // uses Linq methods Skip, TakeWhile, and ToList
                            benchNamesTemp = csvLine.Skip(START_COL - 1).ToList();
                            batchColumn = Array.IndexOf(csvLine, "SBIN");
                            HashSet<string> tempNames = new HashSet<string>();
                            foreach (string name in benchNamesTemp.ToList())  // check for duplicate names
                            {
                                if (!tempNames.Add(name))
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Duplicate test: " + name + "\r\n    in " + benchDataPath + "\r\n    Cannot run " + "GU Calibration");
                                    benchNamesTemp.Remove(name);
                                    GuInitSuccess = false;
                                }
                            }
                            continue;
                        case "tests#":
                        case "test#":
                            //int[] testNumArray = Array.ConvertAll(csvLine.Skip(START_COL).ToArray(), x => int.Parse(x));   // obsolete, I just left here as a syntax reference for converting string array to int array
                            //int[] testNumArray = csvLine.Skip(START_COL - 1).Select(x => int.Parse(x)).ToArray();    // obsolete, I just left here as a syntax reference for converting string array to int array
                            for (int i = 0; i < benchNamesTemp.Count; i++)
                            {
                                try
                                {
                                    int testNum = Convert.ToInt16(csvLine[START_COL - 1 + i]);
                                    testNumDict.Add(benchNamesTemp[i], testNum);
                                    benchTestNameList.Add(testNum, benchNamesTemp[i]);
                                }
                                catch
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Test# format error, test " + benchNamesTemp[i] + "\r\n    in " + benchDataPath + "\r\nCannot run " + "GU Calibration");
                                    GuInitSuccess = false;
                                }
                            }
                            continue;
                        case "unit":
                        case "units":
                            for (int i = 0; i < benchNamesTemp.Count; i++)
                            {
                                try
                                {
                                    unitsDict.Add(benchNamesTemp[i], (csvLine[START_COL - 1 + i]));   // will generate exception if Unit row length isn't as long as Parameter row length
                                    unitsAreDb.Add(benchNamesTemp[i], unitsDict[benchNamesTemp[i]].ToLower().Contains("db"));
                                }
                                catch   // allow no units, no error thrown
                                {
                                    unitsDict.Add(benchNamesTemp[i], "");
                                }
                                if (unitsDict[benchNamesTemp[i]] == "")
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Units row must be filled out in " + benchDataPath + ", Cannot continue " + "GU Calibration");
                                    GuInitSuccess = false;
                                }
                                //scale_ary[i] = GetScale(unitsDict[benchNamesTemp[i]]);  // Clotho does not scale for units so neither does this GU module
                            }
                            continue;
                        case "highl":
                            TolHiVrfyFound = true;
                            for (int i = 0; i < benchNamesTemp.Count; i++)
                            {
                                try
                                {
                                    if (csvLine[i + START_COL - 1].ToLower() == "auto")
                                    {
                                        hiLimVrfyDict[benchNamesTemp[i]] = SetAutoVeriLimit(benchNamesTemp[i]);
                                    }
                                    else if (csvLine[i + START_COL - 1] != "")
                                    {
                                        if(Convert.ToSingle(csvLine[i + START_COL - 1]) <=1e7)
                                        {
                                            hiLimVrfyDict[benchNamesTemp[i]] = Convert.ToSingle(csvLine[i + START_COL - 1]);
                                        }
                                        else
                                        {
                                            LogToLogServiceAndFile(LogLevel.Error, "ERROR: HighL format error in test parameter " + 
                                                benchNamesTemp[i] + ", HighL = " + Convert.ToSingle(csvLine[i + START_COL - 1]) + ", Cannot continue " + "GU Calibration");
                                            GuInitSuccess = false;
                                        }
                                    }
                                    else
                                    {
                                        hiLimVrfyDict[benchNamesTemp[i]] = 9999999f;   // Allow blanks, no error thrown
                                    }
                                }
                                catch  // if bad format, throw error message and stop " + "GU Calibration" + "
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: HighL format error in file " + benchDataPath + ", test " + benchNamesTemp[i] + ", Cannot continue " + "GU Calibration");
                                    GuInitSuccess = false;
                                }
                            }
                            continue;
                        case "lowl":
                            TolLoVrfyFound = true;
                            for (int i = 0; i < benchNamesTemp.Count; i++)
                            {
                                try
                                {
                                    if (csvLine[i + START_COL - 1].ToLower() == "auto")
                                    {
                                        loLimVrfyDict[benchNamesTemp[i]] = -SetAutoVeriLimit(benchNamesTemp[i]);
                                    }

                                    else if (csvLine[i + START_COL - 1] != "")
                                    {
                                        if (Convert.ToSingle(csvLine[i + START_COL - 1]) >= -1e7)
                                        {
                                            loLimVrfyDict[benchNamesTemp[i]] = Convert.ToSingle(csvLine[i + START_COL - 1]);
                                        }
                                        else
                                        {
                                            LogToLogServiceAndFile(LogLevel.Error, "ERROR: LowL format error in test parameter " + 
                                                benchNamesTemp[i] + ", LowL = " + Convert.ToSingle(csvLine[i + START_COL - 1]) + ", Cannot continue " + "GU Calibration");
                                            GuInitSuccess = false;
                                        }
                                    }
                                    else
                                    {
                                        loLimVrfyDict[benchNamesTemp[i]] = -9999999f;   // Allow blanks, no error thrown
                                    }
                                }
                                catch  // if bad format, throw error message and stop " + "GU Calibration" + "
                                {
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: LowL format error in file " + benchDataPath + ", test " + benchNamesTemp[i] + ", Cannot continue " + "GU Calibration");
                                    GuInitSuccess = false;
                                }
                            }
                            continue;
                    }

                    int dut_id_int;

                    if (TolHiVrfyFound & TolLoVrfyFound)
                    {
                        if (ExtractDutID(csvLine[0], out dut_id_int))
                        {
                            int batch = -1;
                            try
                            {
                                batch = int.Parse(csvLine[batchColumn]);
                                if (batch < 1) throw new Exception();   // Do not allow 0, since the earlier practice was to assign batch=0 to demo units. That would now cause the demo data to be ignored.
                            }
                            catch
                            {
                                LogToLogServiceAndFile(LogLevel.Error, "ERROR: GU Batch number required in column 2 of GU Ref Data file:\n        "
                                    + benchDataPath
                                    + "\n        Valid Batch number is an integer > 0."
                                    + "\n        Cannot continue " + "GU Calibration");

                                GuInitSuccess = false;
                            }

                            if (!dutIdAllLoose.ContainsKey(batch)) dutIdAllLoose.Add(batch, new List<int>());
                            if (!dutIdAllDemo.ContainsKey(batch)) dutIdAllDemo.Add(batch, new List<int>());

                            UnitType looseOrSolder = UnitType.Loose;

                            for (int col = 0; col < START_COL; col++)
                            {
                                if (csvLine[col].ToLower().Contains("demo"))
                                {
                                    looseOrSolder = UnitType.Demo;
                                    break;
                                }
                            }

                            if (looseOrSolder == UnitType.Loose && !dutIdAllLoose[batch].Contains(dut_id_int)) dutIdAllLoose[batch].Add(dut_id_int);
                            else if (looseOrSolder == UnitType.Demo && !dutIdAllDemo[batch].Contains(dut_id_int)) dutIdAllDemo[batch].Add(dut_id_int);

                            for (int i = 0; i < benchNamesTemp.Count; i++)
                            {
                                try
                                {
                                    double dataVal = csvLine[i + START_COL - 1] == "" ? 0 : Convert.ToDouble(csvLine[i + START_COL - 1]);

                                    if (allData[batch, benchNamesTemp[i], dut_id_int, looseOrSolder] == default(List<double>)) allData[batch, benchNamesTemp[i], dut_id_int, looseOrSolder] = new List<double>();
                                    allData[batch, benchNamesTemp[i], dut_id_int, looseOrSolder].Add(dataVal);
                                }
                                catch  // if data is blank or formatted improperly, throw error and stop " + "GU Calibration" + "
                                {
                                    finalRefDataDict[selectedBatch, benchNamesTemp[i], dut_id_int] = -1;
                                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Data format error in file " + benchDataPath + ", test " + benchNamesTemp[i] + ", Device #" + dut_id_int + ", Cannot continue " + "GU Calibration");
                                    GuInitSuccess = false;
                                }
                            }
                            continue;
                        }

                    } // if limits found

                } // end reading file  
            }  // using streamreader

            // Make sure we found limits
            if (!TolHiVrfyFound)
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: HighL row not found in " + benchDataPath + ". Cannot run " + "GU Calibration");
                GuInitSuccess = false;
            }
            if (!TolLoVrfyFound)
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: LowL row not found in " + benchDataPath + ". Cannot run " + "GU Calibration");
                GuInitSuccess = false;
            }

            ///////////////////////////////////////////////////////////////////////////////////
            // Compute "simulated solder data" from loose and demo data
            ///////////////////////////////////////////////////////////////////////////////////

            foreach (int batch in dutIdAllDemo.Keys)
            {
                foreach (int dutIDdemo in dutIdAllDemo[batch].ToList())
                {
                    if (!dutIdAllLoose[batch].Contains(dutIDdemo))
                    {
                        LogToLogServiceAndFile(LogLevel.Warn, "NOTICE: In file " + benchDataPath + "\nDevice " + dutIDdemo + " has DemoBoard data but no Loose data found.\nDevice " + dutIDdemo + " DemoBoard data will be ignored");
                        dutIdAllDemo[batch].Remove(dutIDdemo);
                    }
                }

                // get list of only loose samples without demo samples
                dutIdAllLoose[batch] = new List<int>(dutIdAllLoose[batch].Except(dutIdAllDemo[batch]));


                // check for outliers / repeatability issues
                DataAnalysisFiles.refDataRepeatabilityLog[batch] = new SortedList<int, List<string>>();

                foreach (int dutID in dutIdAllLoose[batch].Union(dutIdAllDemo[batch]))
                {
                    foreach (UnitType t in Enum.GetValues(typeof(UnitType)))
                    {
                        if (t == UnitType.Demo && !dutIdAllDemo[batch].Contains(dutID)) continue;

                        foreach (string testName in benchNamesTemp)
                        {
                            double[] dutData = allData[batch, testName, dutID, t].ToArray();
                            int repeatabilityRank;

                            List<int> outliers = RankRepeatability(dutData, unitsAreDb[testName], -2, out repeatabilityRank);
                            bool removeData = outliers.Count() == 1;

                            if (removeData)
                            {
                                allData[batch, testName, dutID, t].RemoveAt(outliers[0]);   // actually remove the data from GU calculations
                            }

                            string dutDataStr = "";
                            for (int i = 0; i < dutData.Length; i++)
                            {
                                dutDataStr += dutData[i];
                                if (removeData & outliers.Contains(i)) dutDataStr += "*";  // indicate that data was removed
                                if (i != dutData.Length - 1) dutDataStr += ", ";
                            }

                            if (!DataAnalysisFiles.refDataRepeatabilityLog[batch].ContainsKey(repeatabilityRank)) DataAnalysisFiles.refDataRepeatabilityLog[batch].Add(repeatabilityRank, new List<string>());

                            DataAnalysisFiles.refDataRepeatabilityLog[batch][repeatabilityRank].Add(testName + ", " + (t == UnitType.Loose ? "Loose" : "Demo ") + " unit " + dutID + ", values: " + dutDataStr);
                        }
                    }
                }


                // Compute and apply Demo Board offsets
                foreach (string testName in benchNamesTemp)
                {
                    if (dutIdAllDemo[batch].Count() > 0)
                    {
                        //List<float> demoBrdOffsetsTemp = new List<float>();
                        foreach (int dutIDdemo in dutIdAllDemo[batch])
                        {
                            double averageDemoData = allData[batch, testName, dutIDdemo, UnitType.Demo].Average();
                            double averageLooseData = allData[batch, testName, dutIDdemo, UnitType.Loose].Average();
                            demoDataDict[batch, testName, UnitType.Demo, dutIDdemo] = averageDemoData;
                            demoDataDict[batch, testName, UnitType.Loose, dutIDdemo] = averageLooseData;
                            demoBoardOffsetsPerDut[batch, testName, dutIDdemo] = averageDemoData - averageLooseData;
                        }
                        demoBoardOffsets[batch, testName] = demoBoardOffsetsPerDut[batch][testName].Values.Average();
                    }
                    else
                    {
                        demoBoardOffsets[batch, testName] = 0;
                    }
                    foreach (int dutIDloose in dutIdAllLoose[batch])
                    {
                        finalRefDataDict[batch, testName, dutIDloose] = allData[batch, testName, dutIDloose, UnitType.Loose].Average() + demoBoardOffsets[batch, testName];
                    }
                }

                // calculate the correlation coefficient between loose and demo
                foreach (string testName in benchNamesTemp)
                {
                    if (dutIdAllDemo[batch].Count() > 0)
                    {
                        //float[] demoArray = (from pair in demoDataDict[batch][testName][UnitType.Demo] orderby pair.Key ascending select pair.Value).ToArray();     // use unique demo offsets per batch
                        //float[] looseArray = (from pair in demoDataDict[batch][testName][UnitType.Loose] orderby pair.Key ascending select pair.Value).ToArray();   // use unique demo offsets per batch
                        double[] demoArray = (from pair in demoDataDict[batch][testName][UnitType.Demo] orderby pair.Key ascending select pair.Value).ToArray();     // always use demo offsets from batch 1
                        double[] looseArray = (from pair in demoDataDict[batch][testName][UnitType.Loose] orderby pair.Key ascending select pair.Value).ToArray();   // always use demo offsets from batch 1

                        demoLooseCorrCoeff[batch, testName] = pearsoncorr2(demoArray, looseArray, demoArray.Length);
                    }
                }


                // Make sure we found device data
                if (dutIdAllLoose[batch].Count < 1)
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Insufficient data found in " + benchDataPath + "\n    for batch " + batch + ". Cannot run " + "GU Calibration");
                    GuInitSuccess = false;
                }


            }

            selectedBatch = dutIdAllDemo.Keys.First();

            allData.Clear();  // free some memory
        }


        public static string GetTestPlanPath()
        {
            string basePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "");

            if (basePath == "")   // Lite Driver mode
            {
                string tcfPath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");

                int pos1 = tcfPath.IndexOf("TestPlans") + "TestPlans".Length + 1;
                int pos2 = tcfPath.IndexOf('\\', pos1);

                basePath = tcfPath.Remove(pos2);
            }

            return basePath + "\\";
        }


        private static float SetAutoVeriLimit(string testname)
        {
            testname = testname.ToLower();

            if (testname.StartsWith("pt_pin") | testname.StartsWith("pr_pin"))
            {
                //Sj IIp3 bench measurement does not use the same pin, pout & gain
                if(!testname.Contains("twotone") && !testname.Contains("tx_rx_on"))
                    return 3f;  // 0.5
            }
            if (testname.StartsWith("pt_pout") | testname.StartsWith("pr_pout"))
            {
                //Sj IIp3 bench measurement does not use the same pin, pout & gain
                if (!testname.Contains("twotone") && !testname.Contains("tx_rx_on"))
                    return 3f; // 0.5
            }
            if (testname.StartsWith("pt_gain")| testname.StartsWith("pr_gain"))
            {
                //Sj IIp3 bench measurement does not use the same pin, pout & gain
                if (!testname.Contains("twotone") && !testname.Contains("tx_rx_on"))
                    return 3f; // 0.5
            }
            //if ((testname.StartsWith("icc") | testname.StartsWith("itotal")) & testname.Contains("dbm") & !testname.Contains("pin"))
            // Don't check verification on Itotal for Dilong because Ibatt measurement is garbage on bench
            if (testname.StartsWith("pt_icc") & testname.Contains("dbm"))
            {
                if (testname.Contains("lpm")) return 0.005f;
                else return 0.05f;
            }
            if (testname.Contains("aclr") | testname.Contains("acpr"))
            {
                return 3f;
            }
            if (testname.StartsWith("pt_cpl") | testname.StartsWith("couple"))
            {
                return 2f;  // 0.5
            }
            if (testname.StartsWith("pt_h2") | testname.StartsWith("pt_h3"))
            {
                return 10f;  // 0.5
            }

            return 9999999f;
        }



        private static float GetScale(string units)
        {

            return 1f;  // Clotho does not perform any units scaling, so neither will this GU Cal Verify module.
                        // The test developer is responsible for ensuring that limits, correlation factors, and test results have same units.

            switch (units[0])
            {
                case 'm':
                    return 1e-3f;
                case 'u':
                    return 1e-6f;
                case 'n':
                    return 1e-9f;
                case 'p':
                    return 1e-12f;
                case 'k':
                case 'K':
                    return 1e3f;
                case 'M':
                    return 1e6f;
                case 'G':
                    return 1e9f;
                default:
                    return 1f;
            }
        }



        private static bool ExtractDutID(string in_dut_id_str, out int out_dut_id_int)
        {

            out_dut_id_int = 0;

            for (int i = in_dut_id_str.Length - 1; i >= 0; i--)
            {
                int asci = (int)in_dut_id_str[i];
                if (asci < 48 | asci > 57)     // if not a numerical character
                {
                    if (i != in_dut_id_str.Length)
                    {
                        out_dut_id_int = Convert.ToInt32(in_dut_id_str.Remove(0, i + 1));
                        return true;
                    }
                    else  // The field did not end with numerical characters. No part ID could be found.
                    {
                        return false;
                    }
                }
                else if (i == 0)
                {
                    out_dut_id_int = Convert.ToInt32(in_dut_id_str);
                    return (true);
                }

            }

            return (false);
        }



        private static object LogLocker = new object();

        private static void LogToLogServiceAndFile(LogLevel logLev, string str)
        {
            lock (LogLocker)
            {
                loggedMessages.Add(str);
                logger.Log(logLev, str);
                Console.WriteLine(str);
            }
        }



        private static void WriteGuCorrelationFile()
        {
            using (StreamWriter corrFile = new StreamWriter(new FileStream(correlationFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                corrFile.Write("ParameterName,");

                foreach (int site in sitesAllExistingList)
                {
                    corrFile.Write("Factor_Add_site" + (site + 1) + ",");
                }

                corrFile.Write("Factor_Add_LowLimit,Factor_Add_HighLimit,");

                foreach (int site in sitesAllExistingList)
                {
                    corrFile.Write("Factor_Multiply_site" + (site + 1) + ",");
                }

                corrFile.WriteLine("Factor_Multiply_LowLimit,Factor_Multiply_HighLimit");

                foreach (string testName in corrFileTestNameList)   // write every test that was in previous correlation file
                {
                    if (testName.Contains(iccCalTestNameExtension)) continue;    // don't write Icc calfactors to correlation file, write to different file

                    corrFile.Write(testName + ",");

                    //if (!testedTestNameList.ContainsValue(testName) & !testName.Contains(iccCalTestNameExtension))
                    //{
                    //    LogToLogServiceAndFile(LogLevel.Warn, "NOTICE: Test " + testName + " found in previous Correlation File, but was not currently tested.\nTest will be included in new Correlation File with calfactor of 0.0");
                    //}

                    foreach (int site in sitesAllExistingList)
                    {
                        // write Factor_Add
                        if ((factorAddEnabledTests.Contains(testName) & testedTestNameList.ContainsValue(testName)) | testName.Contains(iccCalTestNameExtension))  // if theres a previous add factor and the test was actually tested, or if it's an Icc Calfactor
                        {
                            if (GuCorrFailed[site])
                            {
                                corrFile.Write(GuCalFactorsDict_origFromFile[site, testName].ToString() + ",");   // Factor_Add
                            }
                            else
                            {
                                corrFile.Write(GuCalFactorsDict[site, testName].ToString() + ",");   // Factor_Add
                            }
                        }
                        else if (factorAddEnabledTests.Contains(testName) & !testedTestNameList.ContainsValue(testName))
                        {
                            corrFile.Write("0.000011,");   // if running a reduced test list, put a non-zero offset so that full test list doesn't lose ability to update corrfactor
                        }
                        else
                        {
                            corrFile.Write("0,");   // no Factor_Add
                        }
                    } // site loop


                    // write Factor_Add limits
                    corrFile.Write(loLimCalAddDict[testName].ToString() + ",");   // Factor_Add_LowLimit
                    corrFile.Write(hiLimCalAddDict[testName].ToString() + ",");   // Factor_Add_HighLimit

                    foreach (int site in sitesAllExistingList)
                    {
                        // write Factor_Multiply
                        if (factorMultiplyEnabledTests.Contains(testName) & testedTestNameList.ContainsValue(testName))  // if theres a previous multiply factor, and the test was actually tested
                        {
                            if (GuCorrFailed[site])
                            {
                                corrFile.Write(GuCalFactorsDict_origFromFile[site, testName].ToString() + ",");  // Factor_Multiply
                            }
                            else
                            {
                                corrFile.Write(GuCalFactorsDict[site, testName].ToString() + ",");  // Factor_Multiply
                            }
                        }
                        else if (factorMultiplyEnabledTests.Contains(testName) & !testedTestNameList.ContainsValue(testName))
                        {
                            corrFile.Write("0.000011,");   // if running a reduced test list, put a non-zero offset so that full test list doesn't lose ability to update corrfactor
                        }
                        else
                        {
                            corrFile.Write("0,");   // no Factor_Multiply
                        }
                    }

                    // write Factor_Multiply limits
                    corrFile.Write(loLimCalMultiplyDict[testName].ToString() + ",");   // Factor_Multiply_LowLimit
                    corrFile.Write(hiLimCalMultiplyDict[testName].ToString() + ",");   // Factor_Multiply_HighLimit

                    corrFile.WriteLine();

                } // testName loop

            } // using StreamWriter

            // Make a backup as well
            StringBuilder corrFileNameBackup = new StringBuilder();
            corrFileNameBackup.Append(correlationFilePath);
            corrFileNameBackup.Insert(corrFileNameBackup.ToString().LastIndexOf('\\') + 1, @"Backup\");
            corrFileNameBackup.Insert(corrFileNameBackup.ToString().LastIndexOf('.'), "_Backup_" + CorrFinishTimeHumanFriendly);

            string path = Path.GetDirectoryName(corrFileNameBackup.ToString());
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            File.Copy(correlationFilePath, corrFileNameBackup.ToString());

            // add new correlation file to zip
            using (ZipFile zip = ZipFile.Read(DataAnalysisFiles.zipFilePath))
            {
                zip.AddFile(correlationFilePath, "NewProgramFactorFiles");
                zip.Save();
            }

            // Write new correlation factor file
            LogToLogServiceAndFile(LogLevel.HighLight, "Updated Correlation File saved to\n        " + correlationFilePath
                 + "\n    and also\n        " + corrFileNameBackup.ToString());

        }



        private static void ReadGuCorrelationFile()
        {

            if (!GuInitSuccess) return;  // if there was an error while opening bench data file, don't even bother opening this calfactor file

            corrFileTestNameList.Clear();

            correlationFilePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");

            // workaround for Clotho bug, suser pointing to TestPlans directory
            if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "") == "")  // is there a better way to know if I'm suser?
            {
                string correlationFileName = Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, ""));
                correlationFilePath = @"C:\Avago.ATF.Common.x64\CorrelationFiles\Development\" + correlationFileName;
            }

            if (correlationFilePath == "" | correlationFilePath == null)
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Failed to determine Correlation File path. Cannot run " + "GU Calibration" + ".");
                LogToLogServiceAndFile(LogLevel.Error, "       Please ensure that test plan header includes BuddyCorrelation in Test Plan Properties Section.");
                LogToLogServiceAndFile(LogLevel.Error, "       Cannot run " + "GU Calibration" + ".");
                GuInitSuccess = false;
                return;
            }

            Dictionary<string, int> headerColumnLocaton = new Dictionary<string, int>();

            if (!File.Exists(correlationFilePath))
            {
                if (false & wnd.usingLiteDriver)
                {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("NOTICE: No correlation file was found at:");
                    msg.Append("\n        " + correlationFilePath);
                    msg.Append("\n        " + "GU Calibration" + " can still be run, but will generate a new correlation file from scratch");
                    msg.Append("\n        with all parameters using Factor_Add and +-999 limits");

                    LogToLogServiceAndFile(LogLevel.Warn, msg.ToString());
                    MessageBox.Show(wnd.ShowOnTop(), msg.ToString(),
                        "GU Calibration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    factorAddEnabledTests = benchTestNameList.Values.ToList();   // if theres no existing calfactor file, assume all calfactors will be add factors. This provides an easy way for operator to generate calfactor file from scratch.
                    loLimCalAddDict = benchTestNameList.Values.ToDictionary(k => k, v => -999f);
                    hiLimCalAddDict = benchTestNameList.Values.ToDictionary(k => k, v => 999f);
                    loLimCalMultiplyDict = benchTestNameList.Values.ToDictionary(k => k, v => -999f);
                    hiLimCalMultiplyDict = benchTestNameList.Values.ToDictionary(k => k, v => 999f);
                    corrFileTestNameList = benchTestNameList.Values.ToList();
                }
                else
                {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("NOTICE: No correlation file was found at:");
                    msg.Append("\n        " + correlationFilePath);
                    msg.Append("\n        " + "GU Calibration" + " can not be run.");

                    LogToLogServiceAndFile(LogLevel.Warn, msg.ToString());
                    MessageBox.Show(wnd.ShowOnTop(), msg.ToString(),
                        "GU Calibration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    GuInitSuccess = false;
                }
                return;
            }

            using (StreamReader calFile = new StreamReader(new FileStream(correlationFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                bool headerFound = false;

                while (!calFile.EndOfStream)
                {
                    //string[] csvLine = calFile.ReadLine().Split(',');
                    string[] csvLine = calFile.ReadLine().Split(',').TakeWhile(v => v != "").ToArray();

                    //if (csvLine.TakeWhile(v => v != "").Count() < 7) continue;  // skip unimportant lines
                    if (csvLine.Count() < 7) continue;  // skip unimportant lines

                    string testName = csvLine[0];

                    switch (testName)
                    {
                        case "ParameterName":   // the header row
                            headerFound = true;
                            for (int i = 0; i < csvLine.Length; i++)
                            {
                                headerColumnLocaton.Add(csvLine[i], i);
                            }
                            continue;

                        default:   // calfactor data row
                            if (!headerFound)
                            {
                                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Header row (begins with ParameterName) not found in " + correlationFilePath + "\n       Cannot run " + "GU Calibration");
                                GuInitSuccess = false;
                                return;
                            }

                            corrFileTestNameList.Add(testName);

                            foreach (int site in sitesAllExistingList)
                            {
                                float factorAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Add_site" + (site + 1)]]);
                                float factorMultiply = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Multiply_site" + (site + 1)]]);

                                if (factorAdd != 0)
                                {
                                    GuCalFactorsDict_origFromFile[site, testName] = GuCalFactorsDict[site, testName] = factorAdd;   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
                                }
                                else  // store calfactor multiply, even if it's zero
                                {
                                    GuCalFactorsDict_origFromFile[site, testName] = GuCalFactorsDict[site, testName] = factorMultiply;   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
                                }
                            }  // site loop

                            continue;
                    }  // switch first cell in row
                }  // while (!calFile.EndOfStream)
            } // using streamreader

        }



        private static void ReadGuCorrelationTemplate()
        {
            if (!GuInitSuccess) return;  // if there was an error while opening bench data file, don't even bother opening this file

            if (File.Exists(correlationTemplatePath))
            {
                LogToLogServiceAndFile(LogLevel.HighLight, "GU Correlation Template found at:\n" + correlationTemplatePath);
            }
            else
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: No GU Correlation Template found at: " + correlationTemplatePath + "\r\n    Cannot run " + "GU Calibration");
                GuInitSuccess = false;
                return;
            }

            Dictionary<string, int> headerColumnLocaton = new Dictionary<string, int>();

            // reset the information, even though not necessary
            hiLimCalAddDict = new Dictionary<string, float>();
            loLimCalAddDict = new Dictionary<string, float>();
            hiLimCalMultiplyDict = new Dictionary<string, float>();
            loLimCalMultiplyDict = new Dictionary<string, float>();
            factorAddEnabledTests = new List<string>();
            factorMultiplyEnabledTests = new List<string>();

            using (StreamReader calFile = new StreamReader(new FileStream(correlationTemplatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                bool headerFound = false;

                while (!calFile.EndOfStream)
                {
                    //string[] csvLine = calFile.ReadLine().Split(',');
                    string[] csvLine = calFile.ReadLine().Split(',').TakeWhile(v => v != "").ToArray();

                    //if (csvLine.TakeWhile(v => v != "").Count() < 7) continue;  // skip unimportant lines
                    if (csvLine.Count() < 7) continue;  // skip unimportant lines

                    string testName = csvLine[0];

                    switch (testName)
                    {
                        case "ParameterName":   // the header row
                            headerFound = true;
                            for (int i = 0; i < csvLine.Length; i++)
                            {
                                headerColumnLocaton.Add(csvLine[i], i);
                            }
                            continue;

                        default:   // calfactor data row
                            if (!headerFound)
                            {
                                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Header row (begins with ParameterName) not found in " + correlationTemplatePath + "\n       Cannot run " + "GU Calibration");
                                GuInitSuccess = false;
                                return;
                            }

                            float hiLimCalAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Add_HighLimit"]]);
                            float loLimCalAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Add_LowLimit"]]);
                            float hiLimMultiplyAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Multiply_HighLimit"]]);
                            float loLimMultiplyAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Multiply_LowLimit"]]);

                            hiLimCalAddDict[testName] = hiLimCalAdd;
                            loLimCalAddDict[testName] = loLimCalAdd;
                            hiLimCalMultiplyDict[testName] = hiLimMultiplyAdd;
                            loLimCalMultiplyDict[testName] = loLimMultiplyAdd;

                            if (Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Add_site1"]]) != 0) factorAddEnabledTests.Add(testName);   // will need _Site1 for multisite
                            else if (Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Multiply_site1"]]) != 0) factorMultiplyEnabledTests.Add(testName);   // will need _Site1 for multisite

                            if ((factorAddEnabledTests.Contains(testName) | factorMultiplyEnabledTests.Contains(testName)) & !benchTestNameList.ContainsValue(testName))   // this dictionary was populated during reading of GU bench data file, should contain all test names
                            {
                                LogToLogServiceAndFile(LogLevel.Error, "ERROR: " + testName + " has non-zero factor in " + correlationTemplatePath + "\n       but not found in GU bench data file. Cannot run " + "GU Calibration" + ".");
                                GuInitSuccess = false;
                            }

                            continue;
                    }  // switch first cell in row
                }  // while (!calFile.EndOfStream)
            } // using streamreader

            foreach (string testName in corrFileTestNameList.Except(hiLimCalAddDict.Keys))  // ensure that all parameters are found in template file
            {
                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Test " + testName + " found in Correlation File, but not found in Correlation Template File. Cannot run " + "GU Calibration");
                GuInitSuccess = false;
            }

        }


        //private static void WriteIccCalfactorFile()
        //{
        //    if (!ENABLE_ICC_CAL)  return;

        //    string iccCalFilePath = correlationFilePath.ToString().Insert(correlationFilePath.LastIndexOf('.'), iccCalFileNameExtension);

        //    List<string> iccCalPoutTestNameList = IccCalTemplateExists ?
        //        iccCalTemplateTestNameList :
        //        IccCalTestNames.Key.Keys.ToList();

        //    using (StreamWriter corrFile = new StreamWriter(new FileStream(iccCalFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
        //    {
        //        corrFile.Write("ParameterName,");

        //        foreach (int site in sitesAllExistingList)
        //        {
        //            corrFile.Write("InputGain_site" + (site + 1) + ",");
        //        }

        //        foreach (int site in sitesAllExistingList)
        //        {
        //            corrFile.Write("OutputGain_site" + (site + 1) + ",");
        //        }

        //        corrFile.Write("LowLimit,HighLimit,");

        //        foreach (int site in sitesAllExistingList)
        //        {
        //            corrFile.Write("IccServoTargetCorrection_site" + (site + 1) + ",");
        //        }

        //        foreach (int site in sitesAllExistingList)
        //        {
        //            corrFile.Write("VSGlevel_site" + (site + 1) + ",");
        //        }

        //        corrFile.WriteLine();

        //        foreach (string testName in iccCalPoutTestNameList)   // write every test that was in previous correlation file
        //        {
        //            corrFile.Write(testName + ",");

        //            // write Factor_Add
        //            if (IccCalTemplateExists)
        //            {
        //                if (iccCalFactorRedirect.ContainsKey(testName))
        //                {
        //                    foreach (int site in sitesAllExistingList)
        //                    {
        //                        corrFile.Write(iccCalFactorRedirect[testName] + "," + iccCalFactorRedirect[testName] + ",");   // Factor_Add
        //                    }
        //                }
        //                else if (factorAddEnabledTests.Contains(testName))
        //                {
        //                    foreach (int site in sitesAllExistingList)
        //                    {
        //                        if (!previousIccCalFactorsExist & GuIccCalFailed[site])
        //                        {
        //                            corrFile.Write("0.01,");   // This should only happen when creating Icc Cal file from scratch and failed
        //                        }
        //                        else
        //                        {
        //                            if (GuIccCalFailed[site])
        //                            {
        //                                corrFile.Write(GuCalFactorsDict_origFromFile[site, testName + IccCalGain.InputGain].ToString() + ",");   // Input Loss
        //                            }
        //                            else
        //                            {
        //                                corrFile.Write(GuCalFactorsDict[site, testName + IccCalGain.InputGain].ToString() + ",");   // Input Loss
        //                            }
        //                        }
        //                    }
        //                    foreach (int site in sitesAllExistingList)
        //                    {
        //                        if (!previousIccCalFactorsExist & GuIccCalFailed[site])
        //                        {
        //                            corrFile.Write("0.01,");   // This should only happen when creating Icc Cal file from scratch and failed
        //                        }
        //                        else
        //                        {
        //                            if (GuIccCalFailed[site])
        //                            {
        //                                corrFile.Write(GuCalFactorsDict_origFromFile[site, testName + IccCalGain.OutputGain].ToString() + ",");   // Output Loss
        //                            }
        //                            else
        //                            {
        //                                corrFile.Write(GuCalFactorsDict[site, testName + IccCalGain.OutputGain].ToString() + ",");   // Output Loss
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (int site in sitesAllExistingList)
        //                    {
        //                        corrFile.Write("0,0,");   // Factor_Add
        //                    }
        //                }
        //            }
        //            else if (!IccCalTemplateExists & !previousIccCalFactorsExist)   // Dummy run complete. Now create default Icc Cal file from scratch.
        //            {
        //                foreach (int site in sitesAllExistingList)
        //                {
        //                    if (GuIccCalFailed[site])
        //                    {
        //                        corrFile.Write("0.01,");   // Create Icc Cal file from scratch even if failed
        //                    }
        //                    else
        //                    {
        //                        corrFile.Write(GuCalFactorsDict[site, testName + IccCalGain.InputGain].ToString() + ",");   // Input Loss
        //                    }
        //                }
        //                foreach (int site in sitesAllExistingList)
        //                {
        //                    if (GuIccCalFailed[site])
        //                    {
        //                        corrFile.Write("0.01,");   // Create Icc Cal file from scratch even if failed
        //                    }
        //                    else
        //                    {
        //                        corrFile.Write(GuCalFactorsDict[site, testName + IccCalGain.OutputGain].ToString() + ",");   // Output Loss
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception("Algorithm disturbed.");
        //            }

        //                // write Factor_Add limits
        //            if (loLimCalAddDict.ContainsKey(testName) & hiLimCalAddDict.ContainsKey(testName))
        //            {
        //                corrFile.Write(loLimCalAddDict[testName].ToString() + ",");   // Factor_Add_LowLimit
        //                corrFile.Write(hiLimCalAddDict[testName].ToString() + ",");   // Factor_Add_HighLimit
        //            }
        //            else
        //            {
        //                corrFile.Write("-999,");   // Factor_Add_LowLimit
        //                corrFile.Write("999,");   // Factor_Add_HighLimit
        //                LogToLogServiceAndFile(LogLevel.Warn, "NOTICE: " + testName + " was tested but not found in previous Icc Calfactor file\n        " + iccCalFilePath + "\n        So limits of +-999 will be written to new Icc Calfactor file.");
        //            }

        //            foreach (int site in sitesAllExistingList)
        //            {
        //                if (!previousIccCalFactorsExist & GuIccCalFailed[site])    // creating Icc Cal file from scratch even if something failed
        //                {
        //                    corrFile.Write("0,");
        //                }
        //                else
        //                {
        //                    corrFile.Write(IccServoNewTargetCorrection[site, testName] + ",");
        //                }
        //            }

        //            foreach (int site in sitesAllExistingList)
        //            {
        //                if (!previousIccCalFactorsExist & GuIccCalFailed[site])    // creating Icc Cal file from scratch even if something failed
        //                {
        //                    corrFile.Write("-50,");
        //                }
        //                else
        //                {
        //                    corrFile.Write(IccServoNewVSGlevel[site, testName] + ",");
        //                }
        //            }

        //            corrFile.WriteLine();

        //        } // testName loop

        //    } // using StreamWriter

        //    // Make a backup as well
        //    StringBuilder iccCalFileNameBackup = new StringBuilder();
        //    iccCalFileNameBackup.Append(iccCalFilePath);
        //    iccCalFileNameBackup.Insert(iccCalFileNameBackup.ToString().LastIndexOf('\\') + 1, @"Backup\");
        //    iccCalFileNameBackup.Insert(iccCalFileNameBackup.ToString().LastIndexOf('.'), "_Backup_" + CorrFinishTimeHumanFriendly);

        //    string path = Path.GetDirectoryName(iccCalFileNameBackup.ToString());
        //    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        //    File.Copy(iccCalFilePath, iccCalFileNameBackup.ToString());

        //    // add new correlation file to zip
        //    using (ZipFile zip = ZipFile.Read(DataAnalysisFiles.zipFilePath))
        //    {
        //        zip.AddFile(iccCalFilePath, "NewProgramFactorFiles");
        //        zip.Save();
        //    }

        //    LogToLogServiceAndFile(LogLevel.HighLight, "Updated Icc Calfactor File saved to\n        " + iccCalFilePath
        //        + "\n    and also\n        " + iccCalFileNameBackup.ToString());

        //}



        //private static void ReadIccCalfactorFile()
        //{
        //    if (!GuInitSuccess)
        //    {
        //        return;  // if there was an error while opening bench data file, don't even bother opening this calfactor file
        //    }

        //    if (!ENABLE_ICC_CAL)
        //    {
        //        foreach (int site in sitesAllExistingList)
        //        {
        //            GU.thisProductsGuStatus[site].iccCalPassed = true;
        //        }
        //        return;
        //    }

        //    iccCalFilePath = correlationFilePath.Insert(correlationFilePath.LastIndexOf('.'), iccCalFileNameExtension);

        //    if (iccCalFilePath == "")
        //    {
        //        GuInitSuccess = false;
        //        return;
        //    }

        //    Dictionary<string, int> headerColumnLocaton = new Dictionary<string, int>();

        //    if (!File.Exists(iccCalFilePath))
        //    {
        //        StringBuilder msg = new StringBuilder();
        //        msg.Append("NOTICE: No Icc Calfactor file was found at:");
        //        msg.Append("\r\n        " + iccCalFilePath);
        //        msg.Append("\r\n\r\nA new Icc Calfactor File will be created.");
        //        msg.Append("\r\nIcc Cal will need to pass twice.");

        //        LogToLogServiceAndFile(LogLevel.Warn, msg.ToString());
        //        //MessageBox.Show(wnd.ShowOnTop(), msg.ToString(),
        //        //    "" + "GU Calibration" + " - Icc Cal",
        //        //    MessageBoxButtons.OK,
        //        //    MessageBoxIcon.Warning);

        //        previousIccCalFactorsExist = false;

        //        foreach (int site in sitesAllExistingList)
        //        {
        //            GU.thisProductsGuStatus[site].iccCalPassed = false;
        //        }

        //        return;
        //    }
        //    else
        //    {
        //        previousIccCalFactorsExist = true;
        //    }

        //    List<string> testNamesInIccCalFile = new List<string>();

        //    using (StreamReader calFile = new StreamReader(new FileStream(iccCalFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        //    {
        //        bool headerFound = false;

        //        while (!calFile.EndOfStream)
        //        {
        //            string[] csvLine = calFile.ReadLine().Split(',');

        //            if (csvLine.TakeWhile(v => v != "").Count() < 4) continue;  // skip unimportant lines

        //            string testName = csvLine[0];

        //            switch (testName)
        //            {
        //                case "ParameterName":   // the header row
        //                    headerFound = true;
        //                    for (int i = 0; i < csvLine.Length; i++)
        //                    {
        //                        headerColumnLocaton.Add(csvLine[i].Trim(), i);
        //                    }
        //                    continue;

        //                default:   // calfactor data row
        //                    if (!headerFound)
        //                    {
        //                        LogToLogServiceAndFile(LogLevel.Error, "ERROR: Header row (begins with ParameterName) not found in " + iccCalFilePath);
        //                        LogToLogServiceAndFile(LogLevel.Error, "       Cannot run " + "GU Calibration");
        //                        GuInitSuccess = false;
        //                        return;
        //                    }

        //                    testNamesInIccCalFile.Add(testName);

        //                    foreach (int site in sitesAllExistingList)  // *** multisite needs more work
        //                    {
        //                        //if (!sitesUserReducedList.Contains(site)) continue;  // only read in the calfactors for the sites we are not running. But this list is not populated yet.

        //                        float inputGain = 0, outputGain = 0;

        //                        int columnInputGain = headerColumnLocaton["InputGain_site" + (site + 1)];
        //                        int columnOutputGain = headerColumnLocaton["OutputGain_site" + (site + 1)];

        //                        string inputGain_str = csvLine[columnInputGain];
        //                        string outputGain_str = csvLine[columnOutputGain];

        //                        if (!float.TryParse(inputGain_str, out inputGain) | !float.TryParse(outputGain_str, out outputGain))
        //                        {
        //                            if (inputGain_str.Length > outputGain_str.Length)   // always redirect both input and output loss. Do this in case use has not filled out a cell.
        //                                iccCalFactorRedirect[testName] = inputGain_str;
        //                            else
        //                                iccCalFactorRedirect[testName] = outputGain_str;
        //                        }
        //                        else
        //                        {
        //                            GuCalFactorsDict_origFromFile[site, testName + IccCalGain.InputGain] = GuCalFactorsDict[site, testName + IccCalGain.InputGain] = inputGain;   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
        //                            GuCalFactorsDict_origFromFile[site, testName + IccCalGain.OutputGain] = GuCalFactorsDict[site, testName + IccCalGain.OutputGain] = outputGain;   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
        //                        }

        //                        IccServoTargetCorrection[site, testName] = headerColumnLocaton.ContainsKey("IccServoTargetCorrection_site" + (site + 1)) ? Convert.ToSingle(csvLine[headerColumnLocaton["IccServoTargetCorrection_site" + (site + 1)]]) : 0;
        //                        IccServoVSGlevel[site, testName] = headerColumnLocaton.ContainsKey("VSGlevel_site" + (site + 1)) ? Convert.ToSingle(csvLine[headerColumnLocaton["VSGlevel_site" + (site + 1)]]) : 0;
        //                    }  // site loop

        //                    continue;
        //            }  // switch first cell in row
        //        }  // while (!calFile.EndOfStream)
        //    } // using streamreader

        //    foreach (KeyValuePair<string, string> kv in iccCalFactorRedirect)
        //    {
        //        if (!testNamesInIccCalFile.Contains(kv.Value))
        //        {
        //            LogToLogServiceAndFile(LogLevel.Error, "ERROR: \"" + kv.Value + "\" is not an Icc calibrated test name\n     but test " + kv.Key + " is trying to use its calfactor.\n     Please correct this in " + iccCalFilePath + "\n     Cannot run " + "GU Calibration");
        //            GuInitSuccess = false;
        //        }
        //    }

        //}



        private static void ReadIccCalfactorTemplate()
        {
            if (!GuInitSuccess | !ENABLE_ICC_CAL) return;  // if there was an error while opening bench data file, don't even bother opening this calfactor file

            if (previousIccCalFactorsExist)
            {
                if (iccCalTemplatePath == "")
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: GuIccCalTemplate_Path not found in TCF.\n        Cannot run GU Cal.");
                    GuInitSuccess = false;
                    return;
                }
                if (File.Exists(iccCalTemplatePath))
                {
                    LogToLogServiceAndFile(LogLevel.HighLight, "GU Icc Cal Template found at:\n" + iccCalTemplatePath);
                }
                else
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: No GU Icc Cal Template found at: " + iccCalTemplatePath + "\r\n    Cannot run " + "GU Calibration");
                    GuInitSuccess = false;
                    return;
                }
            }
            else
            {
                if (!IccCalTemplateExists)
                {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("NOTICE: No Icc Calfactor Template file was found at:");
                    msg.Append("\r\n        " + iccCalTemplatePath);
                    msg.Append("\r\n\r\nIcc Cal will run in dummy mode, in order to create a default Icc Calfactor File.");
                    msg.Append("\r\nUpon completing the dummy run,\nplease convert the default Icc Calfactor File into an Icc Cal Template File.");

                    LogToLogServiceAndFile(LogLevel.Warn, msg.ToString());
                    MessageBox.Show(wnd.ShowOnTop(), msg.ToString(),
                        "" + "GU Calibration" + " - Icc Cal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;   // if no previous Icc Cal file, allow Icc Cal file creation regardless of Template existing
                }
            }

            iccCalFactorRedirect.Clear();
            iccCalTemplateTestNameList.Clear();

            Dictionary<string, int> headerColumnLocaton = new Dictionary<string, int>();

            using (StreamReader calFile = new StreamReader(new FileStream(iccCalTemplatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                bool headerFound = false;

                while (!calFile.EndOfStream)
                {
                    string[] csvLine = calFile.ReadLine().Split(',');

                    if (csvLine.TakeWhile(v => v != "").Count() < 4) continue;  // skip unimportant lines

                    string testName = csvLine[0];

                    switch (testName)
                    {
                        case "ParameterName":   // the header row
                            headerFound = true;
                            for (int i = 0; i < csvLine.Length; i++)
                            {
                                headerColumnLocaton.Add(csvLine[i].Trim(), i);
                            }
                            continue;

                        default:   // calfactor data row
                            if (!headerFound)
                            {
                                LogToLogServiceAndFile(LogLevel.Error, "ERROR: Header row (begins with ParameterName) not found in " + iccCalTemplatePath);
                                LogToLogServiceAndFile(LogLevel.Error, "       Cannot run " + "GU Calibration");
                                GuInitSuccess = false;
                                return;
                            }

                            iccCalTemplateTestNameList.Add(testName);

                            float hiLimCalAdd = Convert.ToSingle(csvLine[headerColumnLocaton["HighLimit"]]);
                            float loLimCalAdd = Convert.ToSingle(csvLine[headerColumnLocaton["LowLimit"]]);

                            hiLimCalAddDict[testName] = hiLimCalAdd;
                            loLimCalAddDict[testName] = loLimCalAdd;

                            float inputGain = 0, outputGain = 0;

                            int columnInputGain = headerColumnLocaton["InputGain_site1"];
                            int columnOutputGain = headerColumnLocaton["OutputGain_site1"];

                            string inputGain_str = csvLine[columnInputGain];
                            string outputGain_str = csvLine[columnOutputGain];

                            if (!float.TryParse(inputGain_str, out inputGain) | !float.TryParse(outputGain_str, out outputGain))
                            {
                                if (inputGain_str.Length > outputGain_str.Length)   // always redirect both input and output loss. Do this in case use has not filled out a cell.
                                    iccCalFactorRedirect[testName] = inputGain_str;
                                else
                                    iccCalFactorRedirect[testName] = outputGain_str;
                            }
                            else
                            {
                                if (inputGain != 0 || outputGain != 0) factorAddEnabledTests.Add(testName);
                            }

                            continue;
                    }  // switch first cell in row
                }  // while (!calFile.EndOfStream)
            } // using streamreader

            foreach (KeyValuePair<string, string> kv in iccCalFactorRedirect)
            {
                if (!factorAddEnabledTests.Contains(kv.Value))
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: \"" + kv.Value + "\" is not an Icc calibrated test name\n     but test " + kv.Key + " is trying to use its calfactor.\n     Please correct this in " + iccCalTemplatePath + "\n     Cannot run " + "GU Calibration");
                    GuInitSuccess = false;
                }
            }

            if (previousIccCalFactorsExist & IccServoTargetCorrection.Count() > 0)
            {
                foreach (string testName in IccServoTargetCorrection.First().Value.Keys.Except(hiLimCalAddDict.Keys))  // ensure that all parameters are found in template file
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Test " + testName + " found in Icc Calfactor File, but not found in Icc Cal Template File. Cannot run " + "GU Calibration");
                    GuInitSuccess = false;
                }
            }

        }


        public static List<int> RankRepeatability(double[] array, bool unitsAreDb, int outlierRank, out int worstRank)
        {
            const int maxRank = 10;

            worstRank = maxRank;

            if (array.Length < 2) return new List<int>();   // can't do outlier detection on 1 sample

            double[] sortedArray = array.ToArray();
            Array.Sort(sortedArray);
            double medianIndex = ((double)sortedArray.Length + 1.0) / 2.0 - 1;
            double median = (sortedArray[(int)Math.Floor(medianIndex)] + sortedArray[(int)Math.Ceiling(medianIndex)]) / 2.0;

            double[] absDevs = new double[sortedArray.Length];

            for (int i = 0; i < array.Length; i++)
            {
                absDevs[i] = Math.Abs(array[i] - median);
            }

            List<int> outlierIndices = new List<int>();
            double safeDeviation = 0;

            if (unitsAreDb)
            {
                safeDeviation = 0.02 + 100000 / Math.Pow(Math.Max(median, -100) + 187.0, 2.6);   // formula which allows larger tolerance for smaller dB
            }
            else
            {
                safeDeviation = Math.Abs(0.01 * median);
            }


            for (int i = 0; i < array.Length; i++)
            {
                int repRank = maxRank;

                if (absDevs[i] != 0)
                {
                    repRank = -(int)(Math.Ceiling(Math.Log(absDevs[i] / safeDeviation, 2.0)));
                }

                repRank = Math.Max(-maxRank, repRank);
                repRank = Math.Min(maxRank, repRank);

                worstRank = Math.Min(worstRank, repRank);

                if (repRank <= outlierRank)
                {
                    outlierIndices.Add(i);
                }
            }

            return outlierIndices;

        }


        public static List<int> RankRepeatability(float[] array, bool unitsAreDb, int outlierRank, out int worstRank)
        {
            double[] dblArray = new double[array.Length];

            Array.Copy(array, dblArray, array.Length);

            return RankRepeatability(dblArray, unitsAreDb, outlierRank, out worstRank);

        }


        public enum UnitType
        {
            Loose,
            Demo
        }


        private class browserThread
        {
            private string title = "";
            private string file = "";

            public static string show(string caption)
            {
                browserThread bt = new browserThread(caption);
                Thread t = new Thread(bt.doWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                return bt.file;
            }

            private browserThread(string title)
            {
                this.title = title;
            }

            private void doWork()
            {   // this method used only for Lite Driver, to obtain Correlation File path

                OpenFileDialog browseFile = new OpenFileDialog();

                browseFile.Filter = "CSV Files (*.csv)|*.csv";
                string initDir = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TL_FULLPATH, "");
                if (initDir == "")
                {
                    initDir = "c:\\";
                }
                else
                {
                    initDir.Remove(initDir.LastIndexOf("\\"));
                }
                browseFile.InitialDirectory = initDir;
                browseFile.Title = title;

                if (browseFile.ShowDialog(wnd.ShowOnTop()) == DialogResult.OK)
                {
                    file = browseFile.FileName;
                }

            }

        }


        public class MessageBoxAsync
        {
            private static Thread msgBoxAsyncThread;
            private static object msgBoxAsyncLocker = new object();

            public static void Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
            {
                (new Thread(() => MessageBoxAsync2(message, title, buttons, icon, defaultButton))).Start();
                Thread.Sleep(10);   // helps ensure the correct order or appearance of message boxes
            }

            private static void MessageBoxAsync2(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
            {
                lock (msgBoxAsyncLocker)
                {
                    if (msgBoxAsyncThread != null) msgBoxAsyncThread.Join();

                    msgBoxAsyncThread = new Thread(() => MessageBox.Show(message, title, buttons, icon, defaultButton));
                    msgBoxAsyncThread.SetApartmentState(ApartmentState.STA);
                    msgBoxAsyncThread.Start();
                }
            }

            public static void Wait()
            {
                lock (msgBoxAsyncLocker)
                {
                    if (msgBoxAsyncThread != null) msgBoxAsyncThread.Join();
                }
            }
        }

        public static double getIccCalfactor(int site, string testName, IccCalGain inOrOut)
        {
            double calfactor = 0;

            if (!ENABLE_ICC_CAL | runningGUIccCal[site])
            {
                return 0;
            }

            if (iccCalFactorRedirect.ContainsKey(testName + iccCalTestNameExtension))
            {
                calfactor = GuCalFactorsDict[site, iccCalFactorRedirect[testName + iccCalTestNameExtension] + inOrOut];
            }
            else
            {
                calfactor = GuCalFactorsDict[site, testName + iccCalTestNameExtension + inOrOut];
                // if (calfactor != 0)
                //    return calfactor;
            }

            return calfactor;
        }

        // added by limpeh
        public static double getValueWithCF(int site, string testName, double raw_value)
        {
            double final_value = raw_value;

            try
            {
                if (GuMode != null)
                {

                    if (runningGU[site] & (GuMode[site] != GuModes.Vrfy)) return raw_value;

                    double calfactor = GuCalFactorsDict[site, testName];

                    if (calfactor != 0)
                    {
                        if (factorAddEnabledTests.Contains(testName))
                            final_value += calfactor;
                        else if (factorMultiplyEnabledTests.Contains(testName))
                            final_value *= calfactor;
                    }

                    //if (factorMultiplyEnabledTests.Contains(testName))
                    //{
                    //    if(calfactor != 0)
                    //        final_value *= calfactor;
                    //}
                    //if (factorAddEnabledTests.Contains(testName))
                    //{
                    //    final_value += calfactor;
                    //}
                }

                return final_value;
            }
            catch (Exception e)
            {
                return raw_value;
            }
        }

        public static double getGUcalfactor(int site, string testName)
        {
            try
            {
                if (runningGU[site] & (GuMode[site] != GuModes.Vrfy)) return 0;   // don't provide old correlation factors

                double calfactor = GuCalFactorsDict[site, testName];
                return calfactor;
            }
            catch (Exception e)
            {
                return 0;
            }
        }



        private static float pearsoncorr2(double[] x, double[] y, int n)
        {
            /*************************************************************************
            Pearson product-moment correlation coefficient

            Input parameters:
                X       -   sample 1 (array indexes: [0..N-1])
                Y       -   sample 2 (array indexes: [0..N-1])
                N       -   N>=0, sample size:
                            * if given, only N leading elements of X/Y are processed
                            * if not given, automatically determined from input sizes

            Result:
                Pearson product-moment correlation coefficient
                (zero for N=0 or N=1)

              -- ALGLIB --
                 Copyright 28.10.2010 by Bochkanov Sergey
            *************************************************************************/

            double result = 0;
            int i = 0;
            double xmean = 0;
            double ymean = 0;
            double v = 0;
            double x0 = 0;
            double y0 = 0;
            double s = 0;
            bool samex = new bool();
            bool samey = new bool();
            double xv = 0;
            double yv = 0;
            double t1 = 0;
            double t2 = 0;

            //ap.assert(n >= 0, "PearsonCorr2: N<0");
            //ap.assert(ap.len(x) >= n, "PearsonCorr2: Length(X)<N!");
            //ap.assert(ap.len(y) >= n, "PearsonCorr2: Length(Y)<N!");
            //ap.assert(apserv.isfinitevector(x, n), "PearsonCorr2: X is not finite vector");
            //ap.assert(apserv.isfinitevector(y, n), "PearsonCorr2: Y is not finite vector");

            //
            // Special case
            //
            if (n <= 1)
            {
                result = 0;
                return (float)result;
            }

            //
            // Calculate mean.
            //
            //
            // Additonally we calculate SameX and SameY -
            // flag variables which are set to True when
            // all X[] (or Y[]) contain exactly same value.
            //
            // If at least one of them is True, we return zero
            // (othwerwise we risk to get nonzero correlation
            // because of roundoff).
            //
            xmean = 0;
            ymean = 0;
            samex = true;
            samey = true;
            x0 = x[0];
            y0 = y[0];
            v = (double)1 / (double)n;
            for (i = 0; i <= n - 1; i++)
            {
                s = x[i];
                samex = samex & (double)(s) == (double)(x0);
                xmean = xmean + s * v;
                s = y[i];
                samey = samey & (double)(s) == (double)(y0);
                ymean = ymean + s * v;
            }
            if (samex | samey)
            {
                result = 0;
                return (float)result;
            }

            //
            // numerator and denominator
            //
            s = 0;
            xv = 0;
            yv = 0;
            for (i = 0; i <= n - 1; i++)
            {
                t1 = x[i] - xmean;
                t2 = y[i] - ymean;
                xv = xv + Math.Pow(t1, 2.0);
                yv = yv + Math.Pow(t2, 2.0);
                s = s + t1 * t2;
            }
            if ((double)(xv) == (double)(0) | (double)(yv) == (double)(0))
            {
                result = 0;
            }
            else
            {
                result = s / (Math.Sqrt(xv) * Math.Sqrt(yv));
            }
            return (float)result;
        }

        //ChoonChin (20200508) - For XML file checksum used by 28ohm cal and GU cal.
        private static int GetFileChecksum(string FullFilePath)
        {
            FileInfo filInfor = new FileInfo(FullFilePath);
            if (!filInfor.Exists)
            {
                FileStream strea = File.Create(FullFilePath);
                strea.Close();

            }

            StreamReader sr = new StreamReader(FullFilePath);
            string content = sr.ReadToEnd();
            sr.Close();

            char[] content_array = content.ToCharArray();
            int checksum = 0;

            foreach (char c in content_array)
            {
                checksum ^= (int)c;
            }

            return checksum;
        }
        public static int ReturnXMLChecksum()
        {
            int ReadCheckSum = GetFileChecksum(@"C:\Avago.ATF.Common.x64\Database\GuCalLog.xml");
            ReadCheckSum += GetFileChecksum(@"C:\Avago.ATF.Common.x64\Database\SubCalLog.xml");
            return ReadCheckSum;
        }
        public static void StoreChecksum(int ChecksumValue)
        {
            string CheckSumFileName = @"C:\ProgramData\Microsoft\STACKOVERFLOW.cat";
            System.IO.StreamWriter sw;

            if (!File.Exists(CheckSumFileName))
            {
                // Create a new file     
                using (sw = File.CreateText(CheckSumFileName))
                {
                    sw.WriteLine(ChecksumValue.ToString());
                }
            }
            else //Just update the value
            {
                using (sw = new StreamWriter(CheckSumFileName, false))
                {
                    sw.WriteLine(ChecksumValue.ToString());
                }
            }
            sw.Close();
        }
        public static int ReadChecksum()
        {
            string CheckSumFileName = @"C:\ProgramData\Microsoft\STACKOVERFLOW.cat";
            StreamReader sr;

            try
            {
                sr = new StreamReader(CheckSumFileName);
                string ChecksumValue = sr.ReadLine();
                int Value = Convert.ToInt16(ChecksumValue);
                sr.Close();
                return Value;
            }
            catch
            {
                return 999;
            }
        }
        public static bool CheckSumFileExist()
        {
            bool CsFileExist = File.Exists(@"C:\ProgramData\Microsoft\STACKOVERFLOW.cat");
            return CsFileExist;
        }

    }   // class

    public static class ExtentionMethods
    {
        public static IEnumerable<int> AllIndexOf<T>(this IEnumerable<T> values, params T[] searchVals)
        {
            return values.Select((val, index) => searchVals.Contains(val) ? index : -1).Where(index => index != -1);
        }

        public static void SetAll<T>(this T[] values, T setVal)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = setVal;
            }
        }
    }
}



