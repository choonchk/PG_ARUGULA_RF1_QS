using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.CrossDomainAccess;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using MPAD_TestTimer;


namespace TestLib
{
    public static class ResultBuilder
    {
        public static Dictionary<string, double>[] ParametersDict = new Dictionary<string, double>[4];
        public static Dictionary<string, int> ParametersIndexDict = new Dictionary<string, int>();
        public static int ParametersIndex = 0;
        public static Dictionary<int, SerialDef> All;
        public static bool Isfirststep;
        private static bool testLimitsExist;
        private static Dictionary<string, RangeDef> tlPassDict = new Dictionary<string, RangeDef>();
        public static List<string>[] FailedTests = new List<string>[4];
        private static List<string>[] FailedQCTests = new List<string>[4];
        private static object locker_failedQCTest = new object();
        public static bool[] DuplicatedModuleID;
        public static int[] DuplicatedModuleIDCtr;
        public static bool headerFileMode = false;
        public static bool corrFileExists;
        readonly static object locker = new object();
        public static List<int> ValidSites = new List<int>();
        public static ATFReturnResult results = new ATFReturnResult();
        public static List<int> SitesAndPhases;
        public static bool LiteDriver;
        public static bool firstData;
        static ResultBuilder()
        {
            for (int i = 0; i < FailedTests.Length; i++)
            {
                FailedTests[i] = new List<string>() { "program loading" };
                ParametersDict[i] = new Dictionary<string, double>();
                FailedQCTests[i] = new List<string>() { "program loading" };
            }

            try
            {
                All = ATFCrossDomainWrapper.TestLimit_GetAllSerials();
                tlPassDict = ATFSharedData.Instance.TestLimitData.TSF.TestLimitsRange;
                testLimitsExist = true;
            }
            catch
            {
                testLimitsExist = false;   // no test limit file
            }

            corrFileExists = "" != ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");

            LiteDriver = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "") == "";
        }
        
        public static void BeforeTest()
        {
            GetValidSites();
            Eq.CurrentSplitTestPhase = SplitTestPhase.NoSplitTest;
            InitializeResults_Parallel();
        }

        public static void BeforeTest_SplitTest()
        {
            GetValidSites();

            if (!GU.runningGU.Contains(true))
            {
                SitesAndPhases = GetSitesAndPhases();
            }

            Eq.CurrentSplitTestPhase = GetGlobalSplitPhase();

            InitializeResults();
        }

        private static SplitTestPhase GetGlobalSplitPhase()
        {
            int site = SitesAndPhases.AllIndexOf(1, 2).First();

            if (((site ^ SitesAndPhases[site]) & 1) == 1)
            {
                return SplitTestPhase.PhaseA;
            }
            else
            {
                return SplitTestPhase.PhaseB;
            }
        }

        private static void GetValidSites()
        {
            ValidSites.Clear();

            if (LiteDriver && !GU.runningGU.Contains(true) && !EVMCalibrationModel.runningEVMCAL.Contains(true))
            {
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    if(Array.IndexOf(Eq.EnabledSites, site) > -1)
                        ValidSites.Add(site);
                }                
            }
            else if (GU.runningGU.Contains(true))
            {
                ResultBuilder.ValidSites.Add(GU.siteNo);
            }
            else if( EVMCalibrationModel.runningEVMCAL.Contains(true))
            {
                ResultBuilder.ValidSites.Add(EVMCalibrationModel.siteNo);
            }
            else
            {
                if(Eq.NumSites > 1)
                {
                    List<int> ClothoValidSites = ATFCrossDomainWrapper.GetValidSitesIndexes();
                    foreach (int site in ClothoValidSites)
                    {
                        if (Array.IndexOf(Eq.EnabledSites, (byte)(site - 1)) > -1)
                            ValidSites.Add(site - 1);
                    }
                }
                else
                {
                    ValidSites.Add(0);
                }
            }
        }

        private static void InitializeResults()
        {
            for (int site = 0; site < SitesAndPhases.Count(); site++)
            {
                if (SitesAndPhases[site] != 2)
                {
                    DuplicatedModuleID[site] = false;
                    FailedTests[site].Clear();

                    results.InitializeSite(site);
                }
            }
        }
        private static void InitializeResults_Parallel()
        {
            for (int site = 0; site < Eq.NumSites; site++)
            {
                DuplicatedModuleID[site] = false;
                FailedTests[site].Clear();

                results.InitializeSite(site);
            }
        }

        private static ATFReturnResult CloneWithNan(this ATFReturnResult origResults)
        {
            ATFReturnResult finalResults = new ATFReturnResult();

            var valsAllNan = Enumerable.Repeat(double.NaN, ResultBuilder.ValidSites.Count);

            List<int> SitesReadyforDataLog;

            if (Eq.CurrentSplitTestPhase == SplitTestPhase.NoSplitTest)
            {
                SitesReadyforDataLog = new List<int>();
                for (int site = 0; site < ResultBuilder.ValidSites.Count; site++) SitesReadyforDataLog.Add(site);
            }
            else
            {
                SitesReadyforDataLog = SitesAndPhases.AllIndexOf(2).ToList();
            }

            foreach (ATFReturnPararResult singleParam in origResults.Data)
            {
                ATFReturnPararResult finalSingleParam = new ATFReturnPararResult(singleParam.Name, singleParam.Unit);

                finalSingleParam.Vals = valsAllNan.ToList();

                foreach (int site in SitesReadyforDataLog)
                {
                    finalSingleParam.Vals[site] = singleParam.Vals[site];
                }

                finalResults.Data.Add(finalSingleParam);
            }

            return finalResults;
        }

        public static ATFReturnResult FormatResultsForReturnToClotho()
        {
            if (Eq.CurrentSplitTestPhase == SplitTestPhase.NoSplitTest)
            {
                return results.CloneWithNan();
            }
            else
            {
                if (SitesAndPhases.Contains(2))
                {
                    return results.CloneWithNan();
                }
                else
                {
                    return new ATFReturnResult(TestPlanRunConstants.RunSkipFlag + " No Stage 2 Result");
                }
            }
              
        }
        public static ATFReturnResult FormatResultsForReturnToClotho_ParallelTest_nono()
        {
            return results.CloneWithNan();
        }

        public static void InitializeSite(this ATFReturnResult data, int site)
        {
            foreach (ATFReturnPararResult rps in results.Data)
            {
                while (rps.Vals.Count < site + 1) rps.Vals.Add(double.NaN);

                rps.Vals[site] = double.NaN;
            }
        }

        public static bool CheckPass(string testName, double value)
        {
            try
            {
                if (testLimitsExist)
                {
                    if (tlPassDict.ContainsKey(testName))
                        return tlPassDict[testName].checkRange(value);
                    else return true;
                }
                else
                    return true;
            }
            catch
            {
                return true;
            }
        }

        public static double GetUpperLimit(string testName)
        {
            try
            {
                if (testLimitsExist)
                    return tlPassDict[testName].TheMax;
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Clear all result before starting the next run.
        /// </summary>
        public static void Clear(byte site)
        {
            results.Data.Clear();
            ParametersDict[site].Clear();
            ParametersIndexDict.Clear();
            ParametersIndex = 0;
        }

        /// <summary>
        /// If testName exists, update. Otherwise, add.
        /// </summary>
        public static void UpdateResult(byte site, string testName, string units,
            double rawResult, byte decimalPlaces = byte.MaxValue,
            bool skipCheckSpec = false)
        {
            AddResult(site, testName, units, rawResult, false, decimalPlaces, skipCheckSpec);
        }

        /// <summary>
        /// If testName exists, show error. No duplicate.
        /// </summary>
        public static void AddResult(byte site, string testName, string units,
            double rawResult, byte decimalPlaces = byte.MaxValue,
            bool skipCheckSpec = false)
        {
            AddResult(site, testName, units, rawResult, true, decimalPlaces, skipCheckSpec);
        }

        public static void AddResult(byte site, string testName, string units, double rawResult,
            bool isCheckDuplicate, byte decimalPlaces = byte.MaxValue, bool skipSpecCheck = false)
        {
            if (headerFileMode)
            {
                ATFResultBuilder.AddResult(ref results, testName, units, 999);
                return;
            }

            if (decimalPlaces != byte.MaxValue) rawResult = Math.Round(rawResult, decimalPlaces);

            if (double.IsNaN(rawResult) || double.IsInfinity(rawResult))  // force failure if not a number
            {
                rawResult = Math.Max(999999, GetUpperLimit(testName) + 100);

                //if (GU.factorMultiplyEnabledTests.Contains(testName)) rawResult /= GU.getGUcalfactor(site, testName);
                //else rawResult -= GU.getGUcalfactor(site, testName);
            }

            if (true)  // force failed tests[] to be populated
            {
                //double corrResult = GU.factorMultiplyEnabledTests.Contains(testName) ?
                //               rawResult * GU.getGUcalfactor(site, testName) :
                //               rawResult + GU.getGUcalfactor(site, testName);
                double corrResult = GU.getValueWithCF(site, testName, rawResult);

                if (isCheckDuplicate)
                {
                    if (ParametersDict[site].ContainsKey(testName))
                    {
                        string msg = string.Format("Duplicate test parameters headers: {0}", testName);
                        PromptManager.Instance.ShowInfo(msg);
                        LoggingManager.Instance.LogInfoTestPlan(msg);
                    }
                    else
                    {
                        ParametersDict[site].Add(testName, corrResult);
                    }
                }

                if (!CheckPass(testName, corrResult) && !skipSpecCheck && !GU.runningGU[site])
                    FailedTests[site].Add(testName);
            }
            AddResult(site, testName, units, rawResult);
        }

        private static void AddResult(byte site, string testName, string units, double rawResult)
        {
            if(ValidSites.Count == 1)
            {
                ATFReturnPararResult tr = new ATFReturnPararResult(testName, units);
                // Single site normal flow.
                //if (site == 0)
                //{
                    tr.Vals.Add(rawResult);
                    results.Data.Add(tr);
                    return;
                //}
            }
            else
            {
                // Multi-Site flow. 
                int index = 0;
                if (ParametersIndexDict.ContainsKey(testName))
                {
                    index = ParametersIndexDict[testName];
                }
                else
                {
                    ParametersIndexDict.Add(testName, ParametersIndex);
                    results.Data.Add(new ATFReturnPararResult(testName, units));
                    index = ParametersIndex;
                    ParametersIndex++;
                }               
                
                while (results.Data[index].Vals.Count < site + 1) results.Data[index].Vals.Add(double.NaN);

                results.Data[index].Vals[site] = rawResult;
            }
        }

        public static void Legacy_AddResult(byte site, ref ATFReturnResult results, string testName, string units, double rawResult, byte decimalPlaces = byte.MaxValue)
        {
            if (headerFileMode)
            {
                ATFResultBuilder.AddResult(ref results, testName, units, 999);
                return;
            }

            if (decimalPlaces != byte.MaxValue) rawResult = Math.Round(rawResult, decimalPlaces);

            if (double.IsNaN(rawResult) || double.IsInfinity(rawResult))  // force failure if not a number
            {
                rawResult = Math.Max(999999, GetUpperLimit(testName) + 100);

                if (GU.factorMultiplyEnabledTests.Contains(testName)) rawResult /= GU.getGUcalfactor(site, testName);
                else rawResult -= GU.getGUcalfactor(site, testName);
            }

            double corrResult = GU.factorMultiplyEnabledTests.Contains(testName) ?
                rawResult * GU.getGUcalfactor(site, testName) :
                rawResult + GU.getGUcalfactor(site, testName);

            if (!CheckPass(testName, corrResult)) FailedTests[site].Add(testName);

            ATFResultBuilder.AddResult(ref results, testName, units, rawResult);
        }

        private static List<int> GetSitesAndPhases()
        {
            bool handlerDriverSentSOT = !(LiteDriver || ATFCrossDomainWrapper.GetTriggerByManualClickFlag()); 

            if (handlerDriverSentSOT)
            {
                return ATFCrossDomainWrapper.GetHandlerSiteStates();
            }
            else
            {
                return ManuallyCycleSitesAndPhases();
            }
        }

        private static List<int> ManuallyCycleSitesAndPhases()
        {
            List<int> sitesAndPhases;

            if (IsIrregularSitesAndPhases())
            {
                sitesAndPhases = ConstructInitialSitesAndPhases();
            }
            else if (Eq.CurrentSplitTestPhase == SplitTestPhase.PhaseA)
            {
                sitesAndPhases = ConstructSitesAndPhases(SplitTestPhase.PhaseB);
            }
            else
            {
                sitesAndPhases = ConstructSitesAndPhases(SplitTestPhase.PhaseA);
            }

            ATFCrossDomainWrapper.SetManualClickSiteStates(sitesAndPhases);

            return sitesAndPhases;
        }

        private static List<int> ConstructInitialSitesAndPhases()
        {
            List<int> SitesAndPhases_local = new List<int>();

            for (int site = 0; site < Eq.NumSites; site++)
            {
                if (site % 2 == 0) SitesAndPhases_local.Add(1);
                else SitesAndPhases_local.Add(0);
            }

            return SitesAndPhases_local;
        }

        private static List<int> ConstructSitesAndPhases(SplitTestPhase phase)
        {
            List<int> SitesAndPhases_local = new List<int>();

            switch (phase)
            {
                case SplitTestPhase.PhaseA:
                    for (int site = 0; site < Eq.NumSites; site++)
                    {
                        if (site % 2 == 0) SitesAndPhases_local.Add(1);
                        else SitesAndPhases_local.Add(2);
                    }
                    break;

                case SplitTestPhase.PhaseB:
                    for (int site = 0; site < Eq.NumSites; site++)
                    {
                        if (site % 2 == 0) SitesAndPhases_local.Add(2);
                        else SitesAndPhases_local.Add(1);
                    }

                    break;
            }

            return SitesAndPhases_local;
        }

        private static bool IsInitialSitesAndPhases()
        {
            List<int> initialSitesAndPhases = ConstructInitialSitesAndPhases();

            for (int site = 0; site < SitesAndPhases.Count(); site++)
            {
                if (SitesAndPhases[site] != initialSitesAndPhases[site]) return false;
            }

            return true;
        }

        private static bool IsIrregularSitesAndPhases()
        {
            if (SitesAndPhases == null) return true;

            return SitesAndPhases.Contains(0) && !IsInitialSitesAndPhases();
        }

        public static void AddFailedQCTest(byte site, string QCTestName)
        {
            lock (locker_failedQCTest)
            {
                if (!FailedQCTests[site].Contains(QCTestName))
                {
                    FailedQCTests[site].Add(QCTestName);
                }
            }
        }

        public static void ClearFailedQctest(byte site)
        {
            lock (locker_failedQCTest)
            {
                FailedQCTests[site].Clear();
            }
        }

        public static int FailedQctestCount(byte site)
        {
            return FailedQCTests[site].Count;
        }
    }
}
