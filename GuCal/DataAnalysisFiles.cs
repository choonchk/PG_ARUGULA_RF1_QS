﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Reflection;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.LogService;
using Avago.ATF.SchemaTypes;
using Avago.ATF.Validators;
using Ionic.Zip;
using System.Web.Script.Serialization;


namespace GuCal
{
    public partial class GU
    {
        public static bool isMagicBox = false;
        public static string calibrationDirectoryName = "";

        private static class DataAnalysisFiles
        {
            public class AddOnInformation
            {
                public int V;
                public int GUBatch;
                public string CF_File;
                public string ContactorID;

                public AddOnInformation(int version)
                {
                    V = version;
                }
            }

            public static string computerName;
            public static string testPlanVersion;
            public static string dibID;
            public static string[] dibIDArray;
            public static string handlerSN;
            public static int SiteNo;
            public static string lotID;
            public static string sublotID;
            public static string opID;
            public static string waferID;
            public static string testPlanName;
            public static string fileNameRoot;
            public static string calOrVrfy;
            public static string guDataDir;
            public static string guDataRemoteDir;

            public static string closingTimeCodeDatabaseFriendly;   // 11-Aug-2014 JJ Low
            public static string closingTimeCodeHumanFriendly;  // 11-Aug-2014 JJ Low
            public static string closingTimeCodeGalaxyFriendly; // 11-Aug-2014 JJ Low
            public static string stdResultFileName; // 11-Aug-2014 JJ Low
            public static string resultFilePath;    // 11-Aug-2014 JJ Low
            public static string remoteSharePath;   // 11-Aug-2014 JJ Low
            public static string contactorID;
            public static string[] contactorIDArray;
            public static string ipAddress;
            public static string InstrumentInfo;

            public static string zipFilePath;
            public static List<string> allAnalysisFiles = new List<string>();   //

            private const string IccCalAnalysisDir = "2_IccCalAnalysis";
            private const string CorrAnalysisDir = "3_CorrAnalysis";
            private const string VerifyAnalysisDir = "4_VerifyAnalysis";
            private const string RefDataDir = "1_RefDataAnalysis";

            public static Dictionary<int, SortedList<int, List<string>>> refDataRepeatabilityLog = new Dictionary<int, SortedList<int, List<string>>>();

            public static void WriteAll()
            {
                try
                {
                    // Generate header info




                    DateTime datetime = DateTime.Now;   // 11-Aug-2014 JJ Low

                    closingTimeCodeDatabaseFriendly = string.Format("{0:yyyy-MM-dd_HH:mm:ss}", datetime);   // 11-Aug-2014 JJ Low
                    closingTimeCodeHumanFriendly = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", datetime);  // 11-Aug-2014 JJ Low
                    closingTimeCodeGalaxyFriendly = string.Format("{0:yyyy_M_d H:m:s}", datetime);  // 11-Aug-2014 JJ Low
                    contactorID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CONTACTOR_ID, ""); // 11-Aug-2014 JJ Low
                    contactorIDArray = contactorID.Split('_');

                    computerName = System.Environment.MachineName;
                    testPlanVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TP_VER, "");
                    dibID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_DIB_ID, "");
                    dibIDArray = dibID.Split('_');
                    handlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
                    lotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
                    sublotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "");
                    opID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "");
                    waferID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "");
                    string siteNoStr = string.Format("_Site{0}", GU.siteNo + 1);
                    testPlanName = Path.GetFileNameWithoutExtension(GetTestPlanPath().TrimEnd('\\')) + siteNoStr;
                    ipAddress = GetLocalIPAddress();
                    InstrumentInfo = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_INSTRUMENT_INFO, "");
                    SiteNo = Convert.ToInt32(GU.siteNo + 1);

                    fileNameRoot = testPlanName + "_" + CorrFinishTimeHumanFriendly;

                    calOrVrfy =
                        //(GuMode.Contains(GuModes.IccCorrVrfy) ? GuModes.IccCorrVrfy :
                        (GuMode.Contains(GuModes.CorrVrfy) ? GuModes.CorrVrfy :
                            GuModes.Vrfy).ToString();

                    string passFailIndicator = "";
                    //if (GuMode.Contains(GuModes.IccCorrVrfy)) passFailIndicator += GuIccCalFailed.Contains(true) ? "F" : "P";
                    //if (GuMode.Contains(GuModes.IccCorrVrfy) | GuMode.Contains(GuModes.CorrVrfy)) passFailIndicator += GuCorrFailed.Contains(true) ? "F" : "P";
                    if (GuMode.Contains(GuModes.CorrVrfy)) passFailIndicator += GuCorrFailed.Contains(true) ? "F" : "P";
                    passFailIndicator += GuVerifyFailed.Contains(true) ? "F" : "P";

                    guDataDir = @"C:/Avago.ATF.Common.x64/AutoGUcalResults/" + CorrFinishTimeHumanFriendly + "_" + testPlanName + "_" + calOrVrfy + "_" + passFailIndicator + @"/";

                    allAnalysisFiles.Clear();

                    if (!Directory.Exists(guDataDir + RefDataDir)) Directory.CreateDirectory(guDataDir + RefDataDir);
                    WriteRefFinalData(guDataDir + RefDataDir + "/GuRefFinalData_" + fileNameRoot + ".csv");
                    WriteRefDemoData(guDataDir + RefDataDir + "/GuRefDemoData_" + fileNameRoot + ".csv", UnitType.Demo);
                    WriteRefDemoData(guDataDir + RefDataDir + "/GuRefPreDemoData_" + fileNameRoot + ".csv", UnitType.Loose);
                    WriteRefDemoOffsets(guDataDir + RefDataDir + "/GuRefDemoOffsets_" + fileNameRoot + ".csv");
                    WriteRefRepeatFile(guDataDir + RefDataDir + "/GuRefRepeatability_" + fileNameRoot + ".txt");
                    WriteRefLooseDemoCorrCoeff(guDataDir + RefDataDir + "/GuRefLooseDemoCorrCoeff_" + fileNameRoot + ".csv");

                    //if (GuMode.Contains(GuModes.IccCorrVrfy))
                    //{
                    //    if (!Directory.Exists(guDataDir + IccCalAnalysisDir)) Directory.CreateDirectory(guDataDir + IccCalAnalysisDir);
                    //    WriteIccCalfactor(guDataDir + IccCalAnalysisDir + "/GuIccCalFactor_" + fileNameRoot + ".csv");
                    //    WriteIccCalData(guDataDir + IccCalAnalysisDir + "/GuIccCalData_" + fileNameRoot + ".csv");
                    //    WriteIccAvgError(guDataDir + IccCalAnalysisDir + "/GuIccAvgVrfyError_" + fileNameRoot + ".csv");
                    //}

                    //if (GuMode.Contains(GuModes.IccCorrVrfy) || GuMode.Contains(GuModes.CorrVrfy))
                    if (GuMode.Contains(GuModes.CorrVrfy))
                    {
                        if (!Directory.Exists(guDataDir + CorrAnalysisDir)) Directory.CreateDirectory(guDataDir + CorrAnalysisDir);
                        WriteCorrRawData(guDataDir + CorrAnalysisDir + "/GuCorrRawData_" + fileNameRoot + ".csv");
                        WriteCorrFactor(guDataDir + CorrAnalysisDir + "/GuCorrFactor_" + fileNameRoot + ".csv");
                        WriteCorrFactorNoDemo(guDataDir + CorrAnalysisDir + "/GuCorrFactorNoDemoOffset_" + fileNameRoot + ".csv");
                    }

                    if (!Directory.Exists(guDataDir + VerifyAnalysisDir)) Directory.CreateDirectory(guDataDir + VerifyAnalysisDir);
                    WriteRawData(guDataDir + VerifyAnalysisDir + "/GuRawData_" + fileNameRoot + ".csv");
                    WriteVrfyData(guDataDir + VerifyAnalysisDir + "/GuVrfyData_" + fileNameRoot + ".csv");
                    WriteVrfyError(guDataDir + VerifyAnalysisDir + "/GuVrfyError_" + fileNameRoot + ".csv");
                    WriteCorrCoeff(guDataDir + VerifyAnalysisDir + "/GuCorrCoeff_" + fileNameRoot + ".csv");

                    WriteLogFile(guDataDir + "/GuLogPrintout_" + fileNameRoot + ".txt");

                    AddOnInformation adi = new AddOnInformation(1);
                    adi.ContactorID = contactorIDArray[GU.siteNo];
                    adi.CF_File = Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, ""));
                    adi.GUBatch = selectedBatch;
                    WriteAddOnInfoLogFile(guDataDir + "/GuAddOnInfo_" + fileNameRoot + ".adi", adi);

                    // track cable cal info of this gu cal - JJ Low
                    if (GU.isMagicBox == true)
                    {
                        string magicbox_adi_folder = @"D:/ExpertCalSystem.Data/MagicBox/" + GU.calibrationDirectoryName + @"/Data.Current/";

                        if (Directory.Exists(magicbox_adi_folder))
                        {
                            if (File.Exists(magicbox_adi_folder + @"MagicBoxMetaData.adi"))
                            {
                                File.Copy(magicbox_adi_folder + @"MagicBoxMetaData.adi", guDataDir + "GuMagicBoxMetaData_" + fileNameRoot + ".adi");

                                allAnalysisFiles.Add(guDataDir + "GuMagicBoxMetaData_" + fileNameRoot + ".adi");
                            }
                        }
                    }

                    //zip everything up for convenience
                    zipFilePath = ipAddress == "" ?
                        (guDataDir + CorrFinishTimeHumanFriendly + "_" + testPlanName + calOrVrfy + "_" + passFailIndicator + "_" + RandomString(7) + ".zip") :
                        (guDataDir + "IP" + ipAddress + "_" + CorrFinishTimeHumanFriendly + "_" + testPlanName + calOrVrfy + "_" + passFailIndicator + "_" + RandomString(7) + ".zip");

                    using (ZipFile zip = new ZipFile(zipFilePath))
                    {
                        foreach (string file in allAnalysisFiles)
                        {
                            string dir = (Path.GetDirectoryName(file) == Path.GetDirectoryName(guDataDir)) ? "" : Path.GetFileName(Path.GetDirectoryName(file));

                            zip.AddFile(file, dir);
                        }

                        // Add previous Correlation and Icc Cal factor file
                        if (File.Exists(correlationFilePath))
                            zip.AddFile(correlationFilePath, "PreviousProgramFactorFiles");
                        else
                            zip.AddEntry("PreviousProgramFactorFiles\\NoPreviousCorrFactorFile.txt", "");
                        if (File.Exists(iccCalFilePath))
                            zip.AddFile(iccCalFilePath, "PreviousProgramFactorFiles");
                        else
                            zip.AddEntry("PreviousProgramFactorFiles\\NoPreviousIccCalFactorFile.txt", "");

                        zip.AddFile(benchDataPath, RefDataDir);

                        AddZipToZip(zip, @"C:\Avago.ATF.Common\Results\ProgramReport.zip", "ProgramReport");

                        zip.Save();
                    }

                    if (Directory.Exists(guDataRemoteDir)) File.Copy(zipFilePath, guDataRemoteDir + "\\" + Path.GetFileNameWithoutExtension(zipFilePath) + ".gucal");

                    // 25-Sept-2014 JJ Low
                    DateTime currentDateTime = DateTime.Now;

                    string dutInBatch = string.Join("+", dutIdAllLoose[selectedBatch]);

                    stdResultFileName = string.Format("{0}{1}_{2}_{3}_{4:yyyyMMdd_HHmmss}_{5}_{6}.csv",
                        prodTag,
                        calOrVrfy,
                        selectedBatch,
                        dutInBatch,
                        currentDateTime,
                        (ipAddress == null || ipAddress.Length == 0) ? "IP" : string.Format("IP{0}", ATFRTE.Instance.IPAddress),
                        UnisysTimestampEncoder.GetUnisysEncodeTimestamp(currentDateTime)
                        ).ToUpper();

                    WriteStdResultFile(stdResultFileName, selectedBatch.ToString(), dutInBatch);

                    // package_name : AFEM-8230-AP1-RF1_BE-PXI-NI_v0012
                    // gucal_filename: IP172.16.7.149_2022-05-08_17.38.56_ACFM-WH13-AP1-RF1_BE-ZNB_V0028_GuCorrVrfy_PF_WIML8I6.gucal
                    // total_param_count : TOTAL_TESTS
                    // attempt_count : <CorrelationFailures>9</CorrelationFailures>
                    // tester_name : computer name
                    // handler_name : SJHandlerSim1Site02
                    // product_tag  : product_tag


                    LocalGUdatabase.GUsqlite Db = new LocalGUdatabase.GUsqlite();

                    string Gucal_filename = Path.GetFileNameWithoutExtension(zipFilePath) + ".gucal";

                    Db.GUwriter.OpenDB();

                    Db.GUwriter.GenerateNewGUattempt(prodTag);


                    foreach (int site in runningGU.AllIndexOf(true))
                    {

                        int attempt_count = thisProductsGuStatus[site].verificationFailures;
                        int CorrErrorCount = DicCorrError.Count();
                        int VerifyErrorCount = 0;
                        int Total_ParaCount = testedTestNameList.Count();
                        bool Flag = true;

                        Db.GUwriter.InsertGUSummary(prodTag,
                           Gucal_filename,
                           Total_ParaCount,
                           attempt_count,
                           GuMode.Contains(GuModes.CorrVrfy) ? LocalGUdatabase.GUsqlite.GUType.GUCorrVrfy : LocalGUdatabase.GUsqlite.GUType.GUVrfy,
                           computerName,
                           handlerSN,
                           SiteNo
                           );

                        Db.GUwriter.InsertGUCorrSummary(selectedBatch, CorrErrorCount, CorrErrorCount == 0 ? true : false);


                        foreach (int Para_Num in testedTestNameList.Keys)
                        {
                            string Parameter = testedTestNameList[Para_Num];

                            bool Status_GU_CorrFactor = DicCorrError.ContainsKey(Parameter) == true ? false : true;
                            bool Status_Verify_VerificationFactor = false;

                            bool Status_Corr = GuCorrFailed.Contains(true) ? false : true;
                            bool Status_Verify = GuVerifyFailed[site] == false ? true : false;

                            double GU_Corr_Raw_Data_Value = 0f;
                            double GU_CF_Factor_Value = GuCalFactorsDict[site, Parameter];

                            double GU_CF_Upper_Limit = hiLimCalMultiplyDict[testedTestNameList[Para_Num]];
                            double GU_CF_Lower_Limit = loLimCalMultiplyDict[testedTestNameList[Para_Num]];

                            double GU_Verify_Upper_Limit = hiLimVrfyDict[testedTestNameList[Para_Num]];
                            double GU_Verify_Lower_Limit = loLimVrfyDict[testedTestNameList[Para_Num]];

                            double GU_Verify_Final_Ref_Value = 0f;
                            double GU_Verify_Error_Value = 0f;

                            double Measureddatawithoffset = 0f;

                            LocalGUdatabase.GUsqlite.CFType CF_Type = factorMultiplyEnabledTests.Contains(Parameter) == true ? LocalGUdatabase.GUsqlite.CFType.Multiply : LocalGUdatabase.GUsqlite.CFType.Add;

                            // if(!factorMultiplyEnabledTests.Contains(Parameter) && !factorAddEnabledTests.Contains(Parameter) 
                            Db.GUwriter.InsertGuCorrFactor(GU_CF_Factor_Value,
                                Status_GU_CorrFactor,
                                Parameter,
                                CF_Type);


                            foreach (int dutID in dutIdLooseUserReducedList)
                            {

                                Status_Verify_VerificationFactor = DicVerifyError[dutID].ContainsKey(Parameter) == true ? false : true;
                                GU_Verify_Final_Ref_Value = finalRefDataDict[selectedBatch, Parameter, dutID];
                                GU_Corr_Raw_Data_Value = rawAllCorrDataDict[site, testedTestNameList[Para_Num], dutID];
                                Measureddatawithoffset = correctedMsrDataDict[site, Parameter, dutID];
                                GU_Verify_Error_Value = correctedMsrErrorDict[site, Parameter, dutID];

                                VerifyErrorCount = DicVerifyError[dutID].Count();

                                Db.GUwriter.InsertGuCorrRawData(Para_Num,
                                   Parameter,
                                   GU_Corr_Raw_Data_Value,
                                   GU_CF_Upper_Limit,
                                   GU_CF_Lower_Limit,
                                   dutID,
                                   GU_Verify_Final_Ref_Value,
                                   Status_GU_CorrFactor);
                                                               
                                Db.GUwriter.InserGuVerifyRawData(Para_Num,
                                    Parameter,
                                    Measureddatawithoffset,
                                    GU_Verify_Upper_Limit,
                                    GU_Verify_Lower_Limit,
                                    dutID,
                                    GU_Verify_Final_Ref_Value,
                                    GU_CF_Factor_Value,
                                    GU_Verify_Error_Value,
                                    Status_Verify_VerificationFactor);

                                if (Flag)
                                {
                                    Db.GUwriter.InsertGuPareto(dutID, VerifyErrorCount);

                                    Db.GUwriter.InsertGuVerifySummary(dutID,
                                                           selectedBatch,
                                                           VerifyErrorCount,
                                                           Status_Verify);
                                }
                            }
                            Flag = false;
                        }
                    }


                    Db.GUwriter.Commit();
                    DicVerifyError = new Dictionary<int, Dictionary<string, VerifyError>>();
                    DicCorrError = new Dictionary<string, CorrError>();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error while saving GU analysis files\n\n" + e.ToString());
                }

            }


            private static string RandomString(int length)
            {
                string randomString = "";

                Random r = new Random();

                for (int i = 0; i < length; i++)
                {
                    int randomByte = r.Next(48, 90);
                    while (randomByte >= 58 && randomByte <= 64) randomByte = r.Next(48, 90);   // avoid unwanted characters

                    randomString += Convert.ToChar(randomByte);
                }

                return randomString;
            }


            private static void printHeader(StreamWriter sw, string startTime, string finishTime)
            {
                sw.WriteLine("--- Global Info:");
                sw.WriteLine("Date," + finishTime);
                sw.WriteLine("StartTime," + startTime);
                sw.WriteLine("FinishTime," + finishTime);
                sw.WriteLine("TestPlanVersion," + testPlanVersion);
                sw.WriteLine("Product," + prodTag);
                sw.WriteLine("TestPlan," + testPlanName + ".cs");
                sw.WriteLine("Lot," + lotID);
                sw.WriteLine("Sublot," + sublotID);
                sw.WriteLine("Wafer," + waferID);
                sw.WriteLine("TesterName," + computerName);
                sw.WriteLine("TesterIPaddress," + ipAddress);
                sw.WriteLine("Operator," + opID);
                sw.WriteLine("Handler ID," + handlerSN);
                sw.WriteLine("LoadBoardName," + dibIDArray[GU.siteNo]);
                sw.WriteLine("ContactorID," + contactorIDArray[GU.siteNo]);
                sw.WriteLine("InstrumentInfo," + InstrumentInfo);                
            }

            private static void printSummary(StreamWriter sw)
            {
                sw.WriteLine();

                foreach (int site in runningGU.AllIndexOf(true))
                {
                    sw.WriteLine("\n");

                    //if (GuMode[site] == GuModes.IccCorrVrfy)
                    //{
                    //    if (!GuIccCalFailed[site])
                    //    {
                    //        //sw.WriteLine("\n\n#Site " + (site + 1) + " GU Icc Calibration PASSED");
                    //        sw.WriteLine("#GU Icc Calibration Summary,PASSED");
                    //    }
                    //    else
                    //    {
                    //        //sw.WriteLine("\n\n#Site " + (site + 1) + " GU Icc Calibration FAILED");
                    //        sw.WriteLine("#GU Icc Calibration Summary,FAILED");
                    //    }
                    //}

                    //if (GuMode[site] == GuModes.IccCorrVrfy || GuMode[site] == GuModes.CorrVrfy)
                    if (GuMode[site] == GuModes.CorrVrfy)
                    {
                        if (!GuCorrFailed[site])
                        {
                            //sw.WriteLine("#Site " + (site + 1) + " GU Correlation PASSED");
                            sw.WriteLine("#GU Correlation Summary,PASSED");
                        }
                        else
                        {
                            //sw.WriteLine("#Site " + (site + 1) + " GU Correlation FAILED");
                            sw.WriteLine("#GU Correlation Summary,FAILED");
                        }
                    }

                    if (!GuVerifyFailed[site])
                    {
                        //sw.WriteLine("#Site " + (site + 1) + " GU Verification PASSED");
                        sw.WriteLine("#GU Verification Summary,PASSED");
                    }
                    else
                    {
                        //sw.WriteLine("#Site " + (site + 1) + " GU Verification FAILED");
                        sw.WriteLine("#GU Verification Summary,FAILED");
                    }
                }

            }

            internal static string GetLocalIPAddress()
            {
                return ATFRTE.Instance.IPAddress;
            }


            public static void WriteCorrFactor(string corrFactorFilePath)
            {
                using (StreamWriter corrFactorFile = new StreamWriter(new FileStream(corrFactorFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(corrFactorFile, CorrStartTime, CorrFinishTime);

                    corrFactorFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorFile.Write(testName + ",");
                    }
                    corrFactorFile.WriteLine("");

                    // write test numbers
                    corrFactorFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorFile.Write(testNumDict[testName] + ",");
                    }

                    corrFactorFile.WriteLine("");

                    // write units
                    corrFactorFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorFile.Write(unitsDict[testName] + ",");
                    }

                    corrFactorFile.WriteLine("");

                    // write high limits
                    corrFactorFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        try
                        {
                            if (factorMultiplyEnabledTests.Contains(testName))
                            {
                                corrFactorFile.Write(hiLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                            else
                            {
                                corrFactorFile.Write(hiLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                        }
                        catch (Exception ex)
                        {

                        }


                    }
                    corrFactorFile.WriteLine("");

                    // write low limits
                    corrFactorFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        if (factorMultiplyEnabledTests.Contains(testName))
                        {
                            corrFactorFile.Write(loLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }
                        else
                        {
                            corrFactorFile.Write(loLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }
                    }
                    corrFactorFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        // correlation factor file
                        corrFactorFile.Write("999,,,,," + (site + 1) + ",,,,,");
                        foreach (string testName in testedTestNameList.Values)
                        {
                            corrFactorFile.Write(GuCalFactorsDict[site, testName] + ",");
                        }
                        corrFactorFile.WriteLine("");

                    } // site loop

                    printSummary(corrFactorFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Correlation Factor Data saved to " + corrFactorFilePath);
                allAnalysisFiles.Add(corrFactorFilePath);
            }


            public static void WriteCorrFactorNoDemo(string corrFactorNoDemoFilePath)
            {
                using (StreamWriter corrFactorNoDemoFile = new StreamWriter(new FileStream(corrFactorNoDemoFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(corrFactorNoDemoFile, CorrStartTime, CorrFinishTime);

                    corrFactorNoDemoFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorNoDemoFile.Write(testName + ",");
                    }
                    corrFactorNoDemoFile.WriteLine("");

                    // write test numbers
                    corrFactorNoDemoFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorNoDemoFile.Write(testNumDict[testName] + ",");
                    }

                    corrFactorNoDemoFile.WriteLine("");

                    // write units
                    corrFactorNoDemoFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        corrFactorNoDemoFile.Write(unitsDict[testName] + ",");
                    }

                    corrFactorNoDemoFile.WriteLine("");

                    // write high limits
                    corrFactorNoDemoFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        if (factorMultiplyEnabledTests.Contains(testName))
                        {
                            corrFactorNoDemoFile.Write(hiLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }
                        else
                        {
                            corrFactorNoDemoFile.Write(hiLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }

                    }
                    corrFactorNoDemoFile.WriteLine("");

                    // write low limits
                    corrFactorNoDemoFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        if (factorMultiplyEnabledTests.Contains(testName))
                        {
                            corrFactorNoDemoFile.Write(loLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }
                        else
                        {
                            corrFactorNoDemoFile.Write(loLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                        }
                    }
                    corrFactorNoDemoFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        // correlation factor file
                        corrFactorNoDemoFile.Write("999,,,,," + (site + 1) + ",,,,,");
                        foreach (string testName in testedTestNameList.Values)
                        {
                            corrFactorNoDemoFile.Write((GuCalFactorsDict[site, testName] - demoBoardOffsets[selectedBatch, testName]) + ",");
                        }
                        corrFactorNoDemoFile.WriteLine("");

                    } // site loop

                    printSummary(corrFactorNoDemoFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Correlation Factor (without Demo offsets) Data saved to " + corrFactorNoDemoFilePath);
                allAnalysisFiles.Add(corrFactorNoDemoFilePath);
            }


            public static void WriteRawData(string rawDataFilePath)
            {

                using (StreamWriter rawDataFile = new StreamWriter(new FileStream(rawDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {

                    printHeader(rawDataFile, CorrStartTime, CorrFinishTime);

                    rawDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawDataFile.Write(testName + ",");
                    }
                    rawDataFile.WriteLine("");

                    // write test numbers
                    rawDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawDataFile.Write(testNumDict[testName] + ",");
                    }

                    rawDataFile.WriteLine("");

                    // write units
                    rawDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawDataFile.Write(unitsDict[testName] + ",");
                    }

                    rawDataFile.WriteLine("");

                    // write high limits
                    rawDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawDataFile.Write("1,");   // ***these limits don't really apply to the data!
                    }
                    rawDataFile.WriteLine("");

                    // write low limits
                    rawDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawDataFile.Write("-1,");   // ***these limits don't really apply to the data!
                    }
                    rawDataFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        foreach (int dutID in dutIdLooseUserReducedList)
                        {
                            // calibration data file, all runs of raw data
                            //rawDataFile.Write(dutID + "-run" + run + ",,,,," + site + ",,,,,");
                            rawDataFile.Write("PID-" + dutID + ",,,,," + (site + 1) + ",,,,,");
                            foreach (string testName in testedTestNameList.Values)
                            {
                                rawDataFile.Write(rawAllMsrDataDict[site, testName, dutID] + ",");
                            }
                            rawDataFile.WriteLine("");

                            if (dutIDtestedDead.Contains(dutID)) continue;

                        } // dut loop

                    } // site loop

                    printSummary(rawDataFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Raw Data saved to " + rawDataFilePath);
                allAnalysisFiles.Add(rawDataFilePath);

            }


            //public static void WriteIccCalfactor(string iccCalFactorFilePath)
            //{

            //    using (StreamWriter iccCalFactorFile = new StreamWriter(new FileStream(iccCalFactorFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            //    {
            //        printHeader(iccCalFactorFile, IccCalStartTime, IccCalFinishTime);

            //        List<string> IccTestNameList = new List<string>();
            //        if (GuMode.Contains(GuModes.IccCorrVrfy))
            //        {
            //            //IccTestNameList = new List<string>(IccCalFactorsTempDict[sitesUserReducedList[0]].Keys);
            //            IccTestNameList = IccCalTestNames.Key.Keys.ToList();
            //        }

            //        iccCalFactorFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

            //        // write test names
            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(testName + ",");
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        // write test numbers
            //        iccCalFactorFile.Write("Test#,,,,,,,,,,");
            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(testNumDict[IccCalTestNames.Key[testName].PoutTestName] + ",");
            //        }

            //        iccCalFactorFile.WriteLine("");

            //        // write units
            //        iccCalFactorFile.Write("Unit,,,,,,,,,,");

            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(unitsDict[IccCalTestNames.Key[testName].PoutTestName] + ",");
            //        }

            //        iccCalFactorFile.WriteLine("");

            //        // write high limits
            //        iccCalFactorFile.Write("HighL,,,,,,,,,,");
            //        foreach (string testName in IccTestNameList)
            //        {
            //            if (hiLimCalAddDict.ContainsKey(testName))
            //            {
            //                iccCalFactorFile.Write(hiLimCalAddDict[testName] + ",");
            //            }
            //            else
            //            {
            //                iccCalFactorFile.Write("999,");
            //            }
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        // write low limits
            //        iccCalFactorFile.Write("LowL,,,,,,,,,,");

            //        foreach (string testName in IccTestNameList)
            //        {
            //            if (hiLimCalAddDict.ContainsKey(testName))
            //            {
            //                iccCalFactorFile.Write(loLimCalAddDict[testName] + ",");
            //            }
            //            else
            //            {
            //                iccCalFactorFile.Write("-999,");
            //            }
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        // write data
            //        foreach (int site in runningGU.AllIndexOf(true))
            //        {
            //            if (GuMode[site] != GuModes.IccCorrVrfy) continue;

            //            iccCalFactorFile.Write("InputGain,,,,," + (site + 1) + ",,,,,");
            //            foreach (string testName in IccTestNameList)
            //            {
            //                iccCalFactorFile.Write(GuCalFactorsDict[site, testName + IccCalGain.InputGain] + ",");
            //            }
            //            iccCalFactorFile.WriteLine("");

            //            iccCalFactorFile.Write("OutputGain,,,,," + (site + 1) + ",,,,,");
            //            foreach (string testName in IccTestNameList)
            //            {
            //                iccCalFactorFile.Write(GuCalFactorsDict[site, testName + IccCalGain.OutputGain] + ",");
            //            }
            //            iccCalFactorFile.WriteLine("");
            //        } // site loop

            //        printSummary(iccCalFactorFile);
            //    }  // Streamwriters

            //    LogToLogServiceAndFile(LogLevel.HighLight, "Icc Cal Factors saved to " + iccCalFactorFilePath);
            //    allAnalysisFiles.Add(iccCalFactorFilePath);
            //}


            //public static void WriteIccAvgError(string iccCalAvgErrorFilePath)
            //{

            //    using (StreamWriter iccCalFactorFile = new StreamWriter(new FileStream(iccCalAvgErrorFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            //    {
            //        printHeader(iccCalFactorFile, IccCalStartTime, IccCalFinishTime);

            //        List<string> IccTestNameList = IccCalTestNames.Icc.Keys.ToList();

            //        iccCalFactorFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

            //        // write test names
            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(testName + ",");
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        // write test numbers
            //        iccCalFactorFile.Write("Test#,,,,,,,,,,");
            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(testNumDict[testName] + ",");
            //        }

            //        iccCalFactorFile.WriteLine("");

            //        // write units
            //        iccCalFactorFile.Write("Unit,,,,,,,,,,");

            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(unitsDict[testName] + ",");
            //        }

            //        iccCalFactorFile.WriteLine("");

            //        // write high limits
            //        iccCalFactorFile.Write("HighL,,,,,,,,,,");
            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(IccCalAvgErrorDict[GuMode.AllIndexOf(GuModes.IccCorrVrfy).First(), 1, testName].HiLim + ",");
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        // write low limits
            //        iccCalFactorFile.Write("LowL,,,,,,,,,,");

            //        foreach (string testName in IccTestNameList)
            //        {
            //            iccCalFactorFile.Write(IccCalAvgErrorDict[GuMode.AllIndexOf(GuModes.IccCorrVrfy).First(), 1, testName].LoLim + ",");
            //        }
            //        iccCalFactorFile.WriteLine("");

            //        foreach (int site in IccCalAvgErrorDict.Keys)
            //        {
            //            foreach (int attemptNum in IccCalAvgErrorDict[site].Keys)
            //            {
            //                iccCalFactorFile.Write("run-" + attemptNum + ",,,,," + (site + 1) + ",,,,,");
            //                foreach (string testName in IccTestNameList)
            //                {
            //                    iccCalFactorFile.Write(IccCalAvgErrorDict[site, attemptNum, testName].AvgError + ",");
            //                }
            //                iccCalFactorFile.WriteLine("");
            //            }
            //        }

            //        printSummary(iccCalFactorFile);
            //    }  // Streamwriters

            //    LogToLogServiceAndFile(LogLevel.HighLight, "Icc Cal Average Error saved to " + iccCalAvgErrorFilePath);
            //    string lastFolderName = Path.GetFileName(Path.GetDirectoryName(iccCalAvgErrorFilePath));
            //    allAnalysisFiles.Add(iccCalAvgErrorFilePath);
            //}
            public static void WriteCorrRawData(string corrRawDataFilePath)
            {
                using (StreamWriter rawCorrDataFile = new StreamWriter(new FileStream(corrRawDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {

                    printHeader(rawCorrDataFile, CorrStartTime, CorrFinishTime);

                    rawCorrDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawCorrDataFile.Write(testName + ",");
                    }
                    rawCorrDataFile.WriteLine("");

                    // write test numbers
                    rawCorrDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawCorrDataFile.Write(testNumDict[testName] + ",");
                    }

                    rawCorrDataFile.WriteLine("");

                    // write units
                    rawCorrDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawCorrDataFile.Write(unitsDict[testName] + ",");
                    }

                    rawCorrDataFile.WriteLine("");

                    // write high limits
                    rawCorrDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawCorrDataFile.Write("1,");   // ***these limits don't really apply to the data!
                    }
                    rawCorrDataFile.WriteLine("");

                    // write low limits
                    rawCorrDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        rawCorrDataFile.Write("-1,");   // ***these limits don't really apply to the data!
                    }
                    rawCorrDataFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        foreach (int dutID in dutIdLooseUserReducedList)
                        {
                            // calibration data file, all runs of raw data
                            //rawDataFile.Write(dutID + "-run" + run + ",,,,," + site + ",,,,,");
                            rawCorrDataFile.Write("PID-" + dutID + ",,,,," + (site + 1) + ",,,,,");
                            foreach (string testName in testedTestNameList.Values)
                            {
                                rawCorrDataFile.Write(rawAllCorrDataDict[site, testName, dutID] + ",");
                            }
                            rawCorrDataFile.WriteLine("");

                            if (dutIDtestedDead.Contains(dutID)) continue;

                        } // dut loop

                    } // site loop

                    printSummary(rawCorrDataFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Corr Raw Data saved to " + corrRawDataFilePath);
                allAnalysisFiles.Add(corrRawDataFilePath);
            }


            public static void WriteVrfyData(string vrfyDataFilePath)
            {
                using (StreamWriter vrfyDataFile = new StreamWriter(new FileStream(vrfyDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(vrfyDataFile, CorrStartTime, CorrFinishTime);

                    vrfyDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyDataFile.Write(testName + ",");
                    }
                    vrfyDataFile.WriteLine("");

                    // write test numbers
                    vrfyDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyDataFile.Write(testNumDict[testName] + ",");
                    }

                    vrfyDataFile.WriteLine("");

                    // write units
                    vrfyDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyDataFile.Write(unitsDict[testName] + ",");
                    }

                    vrfyDataFile.WriteLine("");

                    // write high limits
                    vrfyDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyDataFile.Write("1,");   // ***these limits don't really apply to the data!
                    }
                    vrfyDataFile.WriteLine("");

                    // write low limits
                    vrfyDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyDataFile.Write("-1,");   // ***these limits don't really apply to the data!
                    }
                    vrfyDataFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        foreach (int dutID in dutIdLooseUserReducedList)
                        {
                            if (dutIDtestedDead.Contains(dutID)) continue;

                            vrfyDataFile.Write("PID-" + dutID + ",,,,," + (site + 1) + ",,,,,");
                            foreach (string testName in testedTestNameList.Values)
                            {
                                vrfyDataFile.Write(correctedMsrDataDict[site, testName, dutID] + ",");  // verification data file, the last run's error with correlation factors applied
                            }
                            vrfyDataFile.WriteLine("");
                        } // dut loop

                    } // site loop

                    printSummary(vrfyDataFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Verification Data saved to " + vrfyDataFilePath);
                allAnalysisFiles.Add(vrfyDataFilePath);
            }


            public static void WriteVrfyError(string vrfyErrorFilePath)
            {
                using (StreamWriter vrfyErrorFile = new StreamWriter(new FileStream(vrfyErrorFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(vrfyErrorFile, CorrStartTime, CorrFinishTime);

                    vrfyErrorFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyErrorFile.Write(testName + ",");
                    }
                    vrfyErrorFile.WriteLine("");

                    // write test numbers
                    vrfyErrorFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyErrorFile.Write(testNumDict[testName] + ",");
                    }

                    vrfyErrorFile.WriteLine("");

                    // write units
                    vrfyErrorFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyErrorFile.Write(unitsDict[testName] + ",");
                    }

                    vrfyErrorFile.WriteLine("");

                    // write high limits
                    vrfyErrorFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyErrorFile.Write(hiLimVrfyDict[testName] + ",");   // ***these limits don't really apply to the data!
                    }
                    vrfyErrorFile.WriteLine("");

                    // write low limits
                    vrfyErrorFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyErrorFile.Write(loLimVrfyDict[testName] + ",");   // ***these limits don't really apply to the data!
                    }
                    vrfyErrorFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        foreach (int dutID in dutIdLooseUserReducedList)
                        {
                            if (dutIDtestedDead.Contains(dutID)) continue;

                            vrfyErrorFile.Write("PID-" + dutID + ",,,,," + (site + 1) + ",,,,,");
                            foreach (string testName in testedTestNameList.Values)
                            {
                                vrfyErrorFile.Write(correctedMsrErrorDict[site, testName, dutID] + ",");
                            }
                            vrfyErrorFile.WriteLine("");
                        } // dut loop

                    } // site loop

                    printSummary(vrfyErrorFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Verification Error saved to " + vrfyErrorFilePath);
                allAnalysisFiles.Add(vrfyErrorFilePath);
            }


            public static void WriteCorrCoeff(string vrfyCorrCoeffFilePath)
            {
                using (StreamWriter vrfyCorrCoeffFile = new StreamWriter(new FileStream(vrfyCorrCoeffFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(vrfyCorrCoeffFile, CorrStartTime, CorrFinishTime);

                    vrfyCorrCoeffFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(testName + ",");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write test numbers
                    vrfyCorrCoeffFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(testNumDict[testName] + ",");
                    }

                    vrfyCorrCoeffFile.WriteLine("");

                    // write units
                    vrfyCorrCoeffFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(unitsDict[testName] + ",");
                    }

                    vrfyCorrCoeffFile.WriteLine("");

                    // write high limits
                    vrfyCorrCoeffFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write("1,");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write low limits
                    vrfyCorrCoeffFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write("-1,");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        vrfyCorrCoeffFile.Write("999,,,,," + (site + 1) + ",,,,,");
                        foreach (string testName in testedTestNameList.Values)
                        {
                            vrfyCorrCoeffFile.Write(corrCoeffDict[site, testName] + ",");
                        }
                        vrfyCorrCoeffFile.WriteLine("");

                    } // site loop

                    printSummary(vrfyCorrCoeffFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Verification Correlation Coefficients saved to " + vrfyCorrCoeffFilePath);
                allAnalysisFiles.Add(vrfyCorrCoeffFilePath);
            }


            public static void WriteLogFile(string logMessagesFilePath)
            {
                using (StreamWriter logMessagesFile = new StreamWriter(new FileStream(logMessagesFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(logMessagesFile, IccCalStartTime, CorrFinishTime);

                    logMessagesFile.WriteLine("\r\n\r\nMessages logged during " + "GU Calibration" + ":\r\n--------------------------------------------------------------\r\n\r\n");
                    foreach (string str in loggedMessages)
                    {
                        logMessagesFile.WriteLine(str);
                    }

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Log Messages saved to " + logMessagesFilePath);
                allAnalysisFiles.Add(logMessagesFilePath);
            }

            public static void WriteAddOnInfoLogFile(string logPath, AddOnInformation obj)
            {
                var json = new JavaScriptSerializer().Serialize(obj);

                using (StreamWriter logMessagesFile = new StreamWriter(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    logMessagesFile.WriteLine(json);
                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Misc Log Messages saved to " + logPath);
                allAnalysisFiles.Add(logPath);
            }

            public static void WriteIccCalData(string iccCalDataFilePath)
            {

                using (StreamWriter iccCalDataFile = new StreamWriter(new FileStream(iccCalDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(iccCalDataFile, IccCalStartTime, IccCalFinishTime);

                    iccCalDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in IccCalTestNames.All)
                    {
                        iccCalDataFile.Write(testName + ",");
                    }
                    iccCalDataFile.WriteLine("");

                    // write test numbers
                    iccCalDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in IccCalTestNames.All)
                    {
                        iccCalDataFile.Write(testNumDict[testName] + ",");
                    }

                    iccCalDataFile.WriteLine("");

                    // write units
                    iccCalDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in IccCalTestNames.All)
                    {
                        iccCalDataFile.Write(unitsDict[testName] + ",");
                    }

                    iccCalDataFile.WriteLine("");

                    // write high limits
                    iccCalDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in IccCalTestNames.All)
                    {
                        iccCalDataFile.Write("999,");
                    }
                    iccCalDataFile.WriteLine("");

                    // write low limits
                    iccCalDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in IccCalTestNames.All)
                    {
                        iccCalDataFile.Write("-999,");
                    }
                    iccCalDataFile.WriteLine("");



                    // write data
                    foreach (int site in runningGU.AllIndexOf(true))
                    {
                        foreach (int dutID in dutIdLooseUserReducedList)
                        {
                            // calibration data file, all runs of raw data
                            //rawDataFile.Write(dutID + "-run" + run + ",,,,," + site + ",,,,,");
                            iccCalDataFile.Write("PID-" + dutID + ",,,,," + (site + 1) + ",,,,,");
                            foreach (string testName in IccCalTestNames.All)
                            {
                                iccCalDataFile.Write(rawIccCalMsrDataDict[site, testName, dutID] + ",");
                            }
                            iccCalDataFile.WriteLine("");

                            if (dutIDtestedDead.Contains(dutID)) continue;

                        } // dut loop

                    } // site loop



                    printSummary(iccCalDataFile);
                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Icc Cal Data saved to " + iccCalDataFilePath);
                allAnalysisFiles.Add(iccCalDataFilePath);
            }


            public static void WriteRefFinalData(string benchDataFilePath)
            {

                using (StreamWriter benchDataFile = new StreamWriter(new FileStream(benchDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    benchDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in testedTestNameList.Values)
                    {
                        benchDataFile.Write(testName + ",");
                    }
                    benchDataFile.WriteLine("");

                    // write test numbers
                    benchDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        benchDataFile.Write(testNumDict[testName] + ",");
                    }

                    benchDataFile.WriteLine("");

                    // write units
                    benchDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        benchDataFile.Write(unitsDict[testName] + ",");
                    }

                    benchDataFile.WriteLine("");

                    // write high limits
                    benchDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in testedTestNameList.Values)
                    {
                        benchDataFile.Write("1,");
                    }
                    benchDataFile.WriteLine("");

                    // write low limits
                    benchDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in testedTestNameList.Values)
                    {
                        benchDataFile.Write("-1,");
                    }
                    benchDataFile.WriteLine("");

                    foreach (int dutID in dutIdLooseUserReducedList)
                    {
                        benchDataFile.Write("PID-" + dutID + ",,,,,,,,,,");
                        foreach (string testName in testedTestNameList.Values)
                        {
                            benchDataFile.Write(finalRefDataDict[selectedBatch, testName, dutID] + ",");
                        }
                        benchDataFile.WriteLine("");

                    } // dut loop

                    printSummary(benchDataFile);
                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Final Reference Data saved to " + benchDataFilePath);
                allAnalysisFiles.Add(benchDataFilePath);
            }


            public static void WriteRefDemoData(string demoDataFilePath, UnitType unitType)
            {

                using (StreamWriter demoDataFile = new StreamWriter(new FileStream(demoDataFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    demoDataFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoDataFile.Write(testName + ",");
                    }
                    demoDataFile.WriteLine("");

                    // write test numbers
                    demoDataFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoDataFile.Write(testNumDict[testName] + ",");
                    }

                    demoDataFile.WriteLine("");

                    // write units
                    demoDataFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoDataFile.Write(unitsDict[testName] + ",");
                    }

                    demoDataFile.WriteLine("");

                    // write high limits
                    demoDataFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoDataFile.Write("1,");
                    }
                    demoDataFile.WriteLine("");

                    // write low limits
                    demoDataFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoDataFile.Write("-1,");
                    }
                    demoDataFile.WriteLine("");

                    foreach (int dutID in dutIdAllDemo[selectedBatch])
                    {
                        demoDataFile.Write("PID-" + dutID + ",,,,,,,,,,");
                        foreach (string testName in benchTestNameList.Values)
                        {
                            demoDataFile.Write(demoDataDict[selectedBatch, testName, unitType, dutID] + ",");
                        }
                        demoDataFile.WriteLine("");

                    } // dut loop

                    printSummary(demoDataFile);
                }  // Streamwriters

                if (unitType == UnitType.Demo)
                    LogToLogServiceAndFile(LogLevel.HighLight, "Reference Demo Data saved to " + demoDataFilePath);
                else
                    LogToLogServiceAndFile(LogLevel.HighLight, "Reference Pre-Demo Data saved to " + demoDataFilePath);

                allAnalysisFiles.Add(demoDataFilePath);
            }


            public static void WriteRefDemoOffsets(string demoOffsetFilePath)
            {

                using (StreamWriter demoOffsetFile = new StreamWriter(new FileStream(demoOffsetFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    demoOffsetFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoOffsetFile.Write(testName + ",");
                    }
                    demoOffsetFile.WriteLine("");

                    // write test numbers
                    demoOffsetFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoOffsetFile.Write(testNumDict[testName] + ",");
                    }

                    demoOffsetFile.WriteLine("");

                    // write units
                    demoOffsetFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoOffsetFile.Write(unitsDict[testName] + ",");
                    }

                    demoOffsetFile.WriteLine("");

                    // write high limits
                    demoOffsetFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoOffsetFile.Write("1,");
                    }
                    demoOffsetFile.WriteLine("");

                    // write low limits
                    demoOffsetFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        demoOffsetFile.Write("-1,");
                    }
                    demoOffsetFile.WriteLine("");

                    // write data
                    foreach (int dutID in dutIdAllDemo[selectedBatch])
                    {
                        demoOffsetFile.Write("PID-" + dutID + ",,,,,,,,,,");
                        foreach (string testName in benchTestNameList.Values)
                        {
                            demoOffsetFile.Write(demoBoardOffsetsPerDut[selectedBatch, testName, dutID] + ",");
                        }
                        demoOffsetFile.WriteLine("");

                    } // dut loop

                    printSummary(demoOffsetFile);
                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "DemoBoard offsets saved to " + demoOffsetFilePath);
                allAnalysisFiles.Add(demoOffsetFilePath);
            }


            public static void WriteRefRepeatFile(string refRepeatFilePath)
            {

                using (StreamWriter refRepeatFile = new StreamWriter(new FileStream(refRepeatFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(refRepeatFile, "", "");

                    refRepeatFile.WriteLine("\r\n\r\nREADME:");
                    refRepeatFile.WriteLine("    Please include units in reference data file so that decibel values are ranked correctly.\r\n");
                    refRepeatFile.WriteLine("    Higher Rank = better repeatability. Rank values are between +-10\r\n");
                    refRepeatFile.WriteLine("    Formula:  DataRange = +- 2^(-Rank) * T\r\n");
                    refRepeatFile.WriteLine("    If units are non-decibel, then T = 1% (of median value).");
                    refRepeatFile.WriteLine("       Ranks are as follows: (extending to +-10)");
                    refRepeatFile.WriteLine("       Rank    DataRange");
                    refRepeatFile.WriteLine("       -3      +-8%");
                    refRepeatFile.WriteLine("       -2      +-4%");
                    refRepeatFile.WriteLine("       -1      +-2%");
                    refRepeatFile.WriteLine("        0      +-1%");
                    refRepeatFile.WriteLine("       +1      +-0.5%");
                    refRepeatFile.WriteLine("       +2      +-0.25%");
                    refRepeatFile.WriteLine("       +3      +-0.125%");

                    refRepeatFile.WriteLine("\r\n    If units are decibel (dB/dBm/dBc), then T is a number between 0.1dB at [median = +30dB] and 0.4dB at [median = -70dB].");
                    refRepeatFile.WriteLine("       Ranks are as follows: (extending to +-10) (for median value = 30dB, therefore T = 0.1dB)");
                    refRepeatFile.WriteLine("       Rank    DataRange");
                    refRepeatFile.WriteLine("       -3      +-0.8dB");
                    refRepeatFile.WriteLine("       -2      +-0.4dB");
                    refRepeatFile.WriteLine("       -1      +-0.2dB");
                    refRepeatFile.WriteLine("        0      +-0.1dB");
                    refRepeatFile.WriteLine("       +1      +-0.05dB");
                    refRepeatFile.WriteLine("       +2      +-0.025dB");
                    refRepeatFile.WriteLine("       +3      +-0.0125dB\r\n");

                    refRepeatFile.WriteLine("    * Asterisk indicates an outlier. Outliers are removed from the data.\r\n");

                    foreach (int repRank in refDataRepeatabilityLog[selectedBatch].Keys)
                    {
                        refRepeatFile.WriteLine("\r\n");

                        refDataRepeatabilityLog[selectedBatch][repRank].Sort();

                        foreach (string msg in refDataRepeatabilityLog[selectedBatch][repRank])
                        {
                            refRepeatFile.WriteLine("Rank " + repRank.ToString("+#;-#;0") + ", " + msg);
                        }
                    }

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Ref Data Repeatability Check saved to " + refRepeatFilePath);
                allAnalysisFiles.Add(refRepeatFilePath);
            }


            public static void WriteRefLooseDemoCorrCoeff(string refLooseDemoCorrCoeff)
            {
                using (StreamWriter vrfyCorrCoeffFile = new StreamWriter(new FileStream(refLooseDemoCorrCoeff, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    printHeader(vrfyCorrCoeffFile, "", "");

                    vrfyCorrCoeffFile.Write("\nParameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                    // write test names
                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(testName + ",");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write test numbers
                    vrfyCorrCoeffFile.Write("Test#,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(testNumDict[testName] + ",");
                    }

                    vrfyCorrCoeffFile.WriteLine("");

                    // write units
                    vrfyCorrCoeffFile.Write("Unit,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(unitsDict[testName] + ",");
                    }

                    vrfyCorrCoeffFile.WriteLine("");

                    // write high limits
                    vrfyCorrCoeffFile.Write("HighL,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write("1,");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write low limits
                    vrfyCorrCoeffFile.Write("LowL,,,,,,,,,,");

                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write("-1,");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    // write data
                    vrfyCorrCoeffFile.Write("999,,,,,,,,,,");
                    foreach (string testName in benchTestNameList.Values)
                    {
                        vrfyCorrCoeffFile.Write(demoLooseCorrCoeff[selectedBatch, testName] + ",");
                    }
                    vrfyCorrCoeffFile.WriteLine("");

                    printSummary(vrfyCorrCoeffFile);

                }  // Streamwriters

                LogToLogServiceAndFile(LogLevel.HighLight, "Verification Correlation Coefficients saved to " + refLooseDemoCorrCoeff);
                allAnalysisFiles.Add(refLooseDemoCorrCoeff);
            }


            public static void AddZipToZip(ZipFile zipToUpdate, string zipToAdd, string folder)
            {
                if (File.Exists(zipToAdd))
                {
                    using (ZipFile zip1 = ZipFile.Read(zipToAdd))
                    {
                        foreach (ZipEntry z in zip1)
                        {
                            MemoryStream stream = new MemoryStream();
                            z.Extract(stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            zipToUpdate.AddEntry(folder + "\\" + z.FileName, stream);
                        }
                    }
                }
            }

            // 15-Aug-2014 (JJ Low)
            private static string GetSystemConfigFile()
            {
                string configPath = Application.StartupPath + @"\Configuration\ATFConfig.xml";

                if (!File.Exists(configPath))
                {
                    configPath = @"C:\Avago.ATF." + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + @"\Configuration\ATFConfig.xml";
                }
                return configPath;
            }

            private static string GetSystemResultBackupPath()
            {
                //string resultBackupPath = @"C:\Avago.ATF.Common\Results.Backup";
                string resultBackupPath = @"C:\Avago.ATF.Common\CustomGUCalData";
                if (!Directory.Exists(resultBackupPath)) Directory.CreateDirectory(resultBackupPath);

                // available for Clotho 2.1.5 and above
                //resultFilePath = ATFMiscConstants.ClothoResultsAbsBackupPath;

                // for Clotho 2.1.4 and below
                //if (ATFConfig.Instance.getSpecificItem(ATFConfigConstants.TagSystemResultBackupPath) != null)
                //{
                //    resultFilePath = ATFConfig.Instance.getSpecificItem(ATFConfigConstants.TagSystemResultBackupPath).Value;
                //}

                return resultBackupPath;
            }

            public static void WriteStdResultFile(string resultFileName, string lotID, string subLotID)
            {
                try
                {
                    // read config file
                    ATFConfig.Instance.ResetConfig();

                    string ConfigFile = GetSystemConfigFile();
                    string strlog = XMLValidate.ValidateXMLFile(ConfigFile, ATFMiscConstants.ATFConfigFileXSDName);

                    if (strlog.EndsWith("Succeed"))
                    {
                        strlog = ATFConfig.Instance.ParseConfigXMLFileIntoMemory(ConfigFile);

                        resultFilePath = GetSystemResultBackupPath();

                        //if (strlog.EndsWith("Succeed"))
                        //{
                        //    if (ATFConfig.Instance.getSpecificItem(ATFConfigConstants.TagSystemResultRemoteSharePath) != null)
                        //    {
                        //        remoteSharePath = ATFConfig.Instance.getSpecificItem(ATFConfigConstants.TagSystemResultRemoteSharePath).Value;
                        //    }
                        //}
                        //else
                        //{
                        //    throw new Exception("Failed to parse configuration file");
                        //}
                    }
                    else
                    {
                        throw new Exception("Unable to find configuration file");
                    }

                    if (!Directory.Exists(resultFilePath))
                    {
                        Directory.CreateDirectory(resultFilePath);
                    }

                    using (StreamWriter resultFile = new StreamWriter(new FileStream(resultFilePath + @"\" + resultFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                    {
                        resultFile.WriteLine("--- Global Info:");
                        resultFile.WriteLine("Date," + closingTimeCodeDatabaseFriendly);
                        resultFile.WriteLine("SetupTime, ");
                        resultFile.WriteLine("StartTime, ");
                        resultFile.WriteLine("FinishTime, ");
                        resultFile.WriteLine("TestPlan," + testPlanName + ".cs");
                        resultFile.WriteLine("TestPlanVersion," + testPlanVersion);
                        resultFile.WriteLine("Lot," + lotID);
                        resultFile.WriteLine("Sublot," + subLotID);
                        resultFile.WriteLine("Wafer," + waferID);
                        resultFile.WriteLine("WaferOrientation,NA");
                        resultFile.WriteLine("TesterName," + computerName);
                        resultFile.WriteLine("TesterType," + computerName);
                        resultFile.WriteLine("Product," + prodTag);
                        resultFile.WriteLine("Operator," + opID);
                        resultFile.WriteLine("ExecType,");
                        resultFile.WriteLine("ExecRevision,");
                        resultFile.WriteLine("RtstCode,");
                        resultFile.WriteLine("PackageType,");
                        resultFile.WriteLine("Family,");
                        resultFile.WriteLine("SpecName," + Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TL_FULLPATH, "")));
                        resultFile.WriteLine("SpecVersion,");
                        resultFile.WriteLine("FlowID,");
                        resultFile.WriteLine("DesignRevision,");
                        resultFile.WriteLine("--- Site details:,");
                        resultFile.WriteLine("Testing sites,");
                        resultFile.WriteLine("Handler ID," + handlerSN);
                        resultFile.WriteLine("Handler type,");
                        resultFile.WriteLine("LoadBoardName," + dibIDArray[GU.siteNo]);
                        resultFile.WriteLine("ContactorID," + contactorIDArray[GU.siteNo]);
                        resultFile.WriteLine("--- Options:,");
                        resultFile.WriteLine("UnitsMode,");
                        resultFile.WriteLine("--- ConditionName:,");
                        resultFile.WriteLine("EMAIL_ADDRESS,");
                        resultFile.WriteLine("Translator,");
                        resultFile.WriteLine("Wafer_Diameter,");
                        resultFile.WriteLine("Facility,");
                        resultFile.WriteLine("HostIpAddress," + ipAddress);
                        resultFile.WriteLine("Temperature,");
                        resultFile.WriteLine("PcbLot,");
                        resultFile.WriteLine("AssemblyLot,");
                        resultFile.WriteLine("VerificationUnit,");
                        resultFile.WriteLine(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");

                        // write CF
                        string correlationFile = Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, ""));

                        foreach (int site in runningGU.AllIndexOf(true))
                        {
                            resultFile.Write("#CF," + correlationFile + ",,,,,,,,,");
                            foreach (string testName in testedTestNameList.Values)
                            {
                                resultFile.Write(GuCalFactorsDict[site, testName] + ",");
                            }
                            resultFile.WriteLine("");
                        }

                        // write test names
                        resultFile.Write("Parameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");

                        // write test names
                        foreach (string testName in testedTestNameList.Values)
                        {
                            resultFile.Write(testName + ",");
                        }
                        resultFile.WriteLine("");

                        // write test numbers
                        resultFile.Write("Tests#,,,,,,Sec,,,,");

                        foreach (string testName in testedTestNameList.Values)
                        {
                            resultFile.Write(testNumDict[testName] + ",");
                        }
                        resultFile.WriteLine("");

                        // write units
                        resultFile.Write("Unit,,,,,,,,,,");

                        foreach (string testName in testedTestNameList.Values)
                        {
                            resultFile.Write(unitsDict[testName] + ",");
                        }
                        resultFile.WriteLine("");

                        // write high limits
                        resultFile.Write("HighL,,,,,,,,,,");
                        foreach (string testName in testedTestNameList.Values)
                        {
                            if (factorMultiplyEnabledTests.Contains(testName))
                            {
                                resultFile.Write(hiLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                            else
                            {
                                resultFile.Write(hiLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                        }
                        resultFile.WriteLine("");

                        // write low limits
                        resultFile.Write("LowL,,,,,,,,,,");

                        foreach (string testName in testedTestNameList.Values)
                        {
                            if (factorMultiplyEnabledTests.Contains(testName))
                            {
                                resultFile.Write(loLimCalMultiplyDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                            else
                            {
                                resultFile.Write(loLimCalAddDict[testName] + ",");   // ***these limits don't really apply to the data!
                            }
                        }
                        resultFile.WriteLine("");

                        // write data
                        foreach (int site in runningGU.AllIndexOf(true))
                        {
                            foreach (int dutID in dutIdLooseUserReducedList)
                            {
                                if (dutIDtestedDead.Contains(dutID)) continue;

                                resultFile.Write("PID-" + dutID + ",,,0,0," + site + ",," + testedTestNameList.Count.ToString() + ",,,");
                                foreach (string testName in testedTestNameList.Values)
                                {
                                    resultFile.Write(correctedMsrDataDict[site, testName, dutID] + ",");  // verification data file, the last run's error with correlation factors applied
                                }
                                resultFile.WriteLine("");
                            } // dut loop

                        } // site loop
                        printSummary(resultFile);

                        resultFile.Close();
                    }  // Streamwriters

                    //if (remoteSharePath.Length > 0)
                    //{
                    //    DirectoryInfo dirInfo = new DirectoryInfo(remoteSharePath);
                    //    if (dirInfo.Exists)
                    //    {
                    //        File.Copy(resultFilePath + @"\" + resultFileName, remoteSharePath + @"\" + resultFileName);
                    //    }
                    //}

                    LogToLogServiceAndFile(LogLevel.Info, "STD Result file saved to " + resultFilePath + @"\" + resultFileName);
                }
                catch (Exception ex)
                {
                    LogToLogServiceAndFile(LogLevel.Error, "Unable to save STD Result to " + resultFilePath + @"\" + resultFileName + ". Error: " + ex.Message);
                }
            }


        }
    }
}



