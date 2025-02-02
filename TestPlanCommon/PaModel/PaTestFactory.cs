﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using ClothoLibAlgo;
using EqLib;
using MPAD_TestTimer;
using TestLib;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.PaModel
{
    public class PaTestFactory
    {
        private PaTestConditionFactory testConFactory;
        private byte site;

        private Dictionary<string, string> Digital_Definitions_Part_Specific;
        private Dictionary<byte, int[]> EqSiteTriggerArray;
        private Dictionary<string, string> TCF_Setting;
        private ValidationDataObject m_validationDo;
        string ClothoRootDir = "";

        public ValidationDataObject ValDataObject
        {
            get { return m_validationDo; }
        }

        public PaTestFactory()
        {
            m_validationDo = new ValidationDataObject();
            ClothoRootDir = GetTestPlanPath();
        }

        public PaTestFactory(byte site, PaTestConditionFactory paramFactory)
        {
            this.site = site;
            testConFactory = paramFactory;
            m_validationDo = new ValidationDataObject();
            ClothoRootDir = GetTestPlanPath();
        }

        //TODO Need TCF PinSweepTraceEnable, and DDPS mipi1 and mipi2 slave address.
        public void Initialize(Dictionary<string, string> Digital_Definitions_Part_Specific2, Dictionary<byte, int[]> SiteTriggerArray,
            Dictionary<string, string> TCF_Setting2)
        {
            Digital_Definitions_Part_Specific = Digital_Definitions_Part_Specific2;
            EqSiteTriggerArray = SiteTriggerArray;
            TCF_Setting = TCF_Setting2;
        }
         
        private string GetTestPlanPath()
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

        /// <summary>
        /// CCT Entry point. Create test.
        /// </summary>
        public iTest[][] PopulateAllPaTests(PaTestConditionReader tcfReader,
            TestPlanStateModel m_modelTpState)
        {
            tcfReader.FillConditionPaTab();

            iTest[][] allPaTests = PopulateAllPaTests(
                tcfReader.DicTestCondTemp, tcfReader.DcResourceSheet, tcfReader.TCF_Setting);
            m_modelTpState.SetLoadFail(ValDataObject.IsValidated);
            return allPaTests;
        }

        private iTest[][] PopulateAllPaTests(List<Dictionary<string, string>> DicTestCondTemp,
            TcfSheetReader DcResourceSheet, Dictionary<string, string> TCF_Setting)
        {
            iTest[][] allPaTests = new iTest[Eq.NumSites][];

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                this.site = site;
                allPaTests[site] = new iTest[DicTestCondTemp.Count];

                for (int testIndex = 0; testIndex < DicTestCondTemp.Count; testIndex++)
                {
                    testConFactory = new PaTestConditionFactory(DicTestCondTemp[testIndex], DcResourceSheet);

                    string testMode = testConFactory.GetStr("Test Mode").Trim().ToUpper();

                    switch (testMode)
                    {
                        case "TIMING":

                            {
                                if (testConFactory.GetStr("Pout") != "")
                                {
                                    TimingTestFixedOutputPower test = CreateTimingFixedPoutTest();
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                    bool isSuccess = test.TryGettingCalFactors();
                                    m_validationDo.Validate(isSuccess);
                                    test.TestCon.TimingCondition = GetTimingMipiCommands(DicTestCondTemp[testIndex]);
                                    allPaTests[site][testIndex] = test;
                                    SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                                }
                                else
                                {
                                    TimingTestFixedInputPower test = CreateTimingFixedPinTest();
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                    bool isSuccess = test.TryGettingCalFactors();
                                    test.TestCon.TimingCondition = GetTimingMipiCommands(DicTestCondTemp[testIndex]);
                                    m_validationDo.Validate(isSuccess);
                                    allPaTests[site][testIndex] = test;
                                    SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                                }

                            }

                            continue;


                        case "RF":

                            {
                                RFTestFixedOutputPower test = CreateRfFixedPoutTest();

                                bool isSuccess = test.TryGettingCalFactors();
                                // Get Mipi Commands for this test
                                test.TestCon.TestMode = testMode;
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                test.TestCon.TxleakageCondition = GetTxlkgMipiCommands(DicTestCondTemp[testIndex]);
                                m_validationDo.Validate(isSuccess);
                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;

                        case "RF_FIXED_PIN":

                            {
                                RFTestFixedInputPower test = CreateRfFixedPinTest();
                                test.TestCon.TestMode = testMode;
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                test.TestCon.TxleakageCondition = GetTxlkgMipiCommands(DicTestCondTemp[testIndex]);
                                bool isSuccess = test.TryGettingCalFactors();
                                m_validationDo.Validate(isSuccess);

                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;
                        case "IIP3":

                            {
                                IIP3Test test = CreateIIP3Test();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                test.TestCon.Iteration = testIndex;
                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;

                        case "DC":

                            {
                                DcTest test = CreateDcTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;

                        case "DC_LEAKAGE":

                            {
                                DcLeakageTest test = CreateDcLeakageTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;

                        case "PIN_SWEEP":

                            {
                                PinSweepTest test = CreatePinSweepTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                bool isSuccess = test.TryGettingCalFactors();
                                m_validationDo.Validate(isSuccess);

                                allPaTests[site][testIndex] = test;
                                SMU_Compliance_Check(test.TestCon.TestParaName, test.TestCon.DcSettings, test.TestCon.CktID);
                            }

                            continue;

                        case "RF_ONOFFTIME":
                        case "RF_ONOFFTIME_SW":
                            {
                                RfOnOffTest test = CreateRfOnOffTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                allPaTests[site][testIndex] = test;
                            }

                            continue;
                        case "CONTINUITY":

                            {
                                ContinuityTest test = CreateContinuityTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                allPaTests[site][testIndex] = test;
                            }

                            continue;

                        case "CALC":
                            {
                                Calculate test = CreateCalculateTest();

                                allPaTests[site][testIndex] = test;
                            }

                            continue;

                        case "MIPI":

                            {
                                MipiTest test = CreateMipiTest();
                                test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                test.TestCon.TesterID = "01"; //TCF_Setting["TesterID"];                               
                                allPaTests[site][testIndex] = test;

                                Regex regexQC = new Regex(@"(T|R)XQC(\d?[1-9]|[1-9]0)");
                                var isreqLoadVec = regexQC.Match(test.TestCon.PowerMode.ToUpper());

                                if (isreqLoadVec.Success)
                                {
                                    test.TestCon.RunningVectorBags.Add(isreqLoadVec.Value);
                                }
                            }

                            continue;

                        case "OTP":

                            {
                                string waveformName = testConFactory.GetStr("Waveform").ToUpper();                                
                                OtpTest test;


                                if (waveformName.Contains("READ")|| waveformName.Contains("CHECK"))
                                {
                                    test = CreateOtpReadTest(site);
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);
                                    allPaTests[site][testIndex] = test;

                                    if (testConFactory.GetStr("waveformname").ToUpper() != "")
                                        waveformName = testConFactory.GetStr("waveformname").ToUpper();


                                    if (waveformName.Contains("FLAG") || waveformName.Contains("LOCK"))
                                    {
                                        switch (TCF_Setting["Tester_Type"].Trim().ToUpper())
                                        {
                                            case "PA":                                               
                                                if (waveformName.Contains("READ_RF1")||waveformName.Contains("READ-RF1"))
                                                {
                                                    test.TestCon.RequiresDataCheckFirst = true;
                                                }
                                                break;

                                            case "FBAR":
                                                if (waveformName.Contains("READ_RF2")|| waveformName.Contains("READ-RF2"))
                                                {
                                                    test.TestCon.RequiresDataCheckFirst = true;
                                                }
                                                break;

                                            case "BOTH":
                                            default:
                                                if (waveformName.Contains("READ_RF") || waveformName.Contains("READ-RF"))
                                                {
                                                    test.TestCon.RequiresDataCheckFirst = true;
                                                }
                                                break;
                                        }
                                    }
                                }
                                else if (waveformName.Contains("OTP_MOD_ID_SELECT"))
                                {
                                    test = CreateOtpReadTest(site);
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                    allPaTests[site][testIndex] = test;
                                    OTP_Procedure.EnableModuleIDselect = true;
                                }
                                else if (waveformName.Contains("OTP_2DID_SELECT"))
                                {
                                    test = CreateOtpReadTest(site);
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                    allPaTests[site][testIndex] = test;
                                    OTP_Procedure.Enable2DIDselect = true;
                                }
                                else
                                {
                                    test = CreateOtpBurnTest(site);
                                    test.TestCon.MipiCommands = GetMipiCommands(DicTestCondTemp[testIndex]);

                                    if (testConFactory.GetStr("waveformname").ToUpper() != "")
                                        waveformName = testConFactory.GetStr("waveformname").ToUpper();

                                    switch (waveformName)  // used to pass in parameters to Burn Code.
                                    {
                                        case "OTP_BURN_MOD_ID":
                                        case "OTP_BURN_MOD_ID2":
                                            test.TestCon.MFG_ID_to_Burn = "";  // optional if you want to specify a particular MFG ID or Mod ID. For cases other than strip level OTP or Module level, Server based ID numbers in SJC
                                            test.TestCon.Module_ID_to_Burn = "";
                                            break;

                                        case "OTP_BURN_REV_ID":
                                        case "OTP_BURN_REV_ID_TX":
                                        case "OTP_BURN_REV_ID_RX":
                                            test.TestCon.REV_ID_to_Burn = TCF_Setting["Sample_Version"];
                                            break;

                                        case "OTP_BURN_FBAR_NOISE_PASS_FLAG":
                                        case "OTP_BURN_NFR_PASS_FLAG":
                                        case "OTP_BURN_RF1_PASS_FLAG":
                                        case "OTP_BURN_RF2_PASS_FLAG":
                                        case "OTP_BURN_NFR-PASS-FLAG":
                                        case "OTP_BURN_RF1-PASS-FLAG":
                                        case "OTP_BURN_RF2-PASS-FLAG":
                                        case "OTP_BURN_LOCK_BIT":
                                        case "OTP_BURN_LOCK_BIT_RX":
                                        case "OTP_BURN_LOCK_BIT_TX":
                                        case "OTP_BURN_LOCKBIT_TX":
                                        case "OTP_BURN_LOCKBIT_RX":
                                        case "OTP_LOCKBIT_TX":
                                        case "OTP_LOCKBIT_RX":
                                            test.TestCon.RequiresDataCheckFirst = true; //need to confirm before transfer to Prod 
                                            break;

                                        default:  // All other OTP Burn tests
                                            break;
                                    }
                                    allPaTests[site][testIndex] = test;
                                }

                            }

                            continue;
                    }
                }
            }

            return allPaTests;
        }

        public void SMU_Compliance_Check(string TestParaName, Dictionary.Ordered<string, DcSetting> DcSettings, string CktID)
        {
            if (ResultBuilder.All != null)
            {
                foreach (string PinName in DcSettings.Keys)
                {

                    string Full_name = CktID + DcSettings[PinName].iParaName + TestParaName + "x";

                    foreach (KeyValuePair<int, Avago.ATF.Shares.SerialDef> Bin in ResultBuilder.All)
                    {
                        if (Bin.Value.RangeCollection.ContainsKey(Full_name))
                        {
                            Avago.ATF.Shares.ParameterRangeDef Spec = Bin.Value.RangeCollection[Full_name];
                            if (PinName.ToUpper().Contains("VIO"))
                            {
                                if (DcSettings[PinName].Current <= Spec.Range.TheMax || DcSettings[PinName].Current >= 100 * Spec.Range.TheMax)
                                {
                                    if (Spec.Range.TheMax < 999)
                                    {
                                        MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + DcSettings.GetType().Name + " : " + DcSettings[PinName].iParaName +
                                            "\n Compliance : " + DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    }
                                }
                            }
                            else
                            {
                                if (DcSettings[PinName].Current <= Spec.Range.TheMax)
                                {
                                    if (Spec.Range.TheMax < 999)
                                    {
                                        MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + DcSettings.GetType().Name + " : " + DcSettings[PinName].iParaName +
                                            "\n Compliance : " + DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
        public RFTestFixedOutputPower CreateRfFixedPoutTest()
        {
            RFTestFixedOutputPower thisTest = new RFTestFixedOutputPower();

            //thisTest.MathVariable = testConFactory.GetVariable();
            thisTest.resetSA = testConFactory.resetSA("ResetSA");
            thisTest.MathVariable = testConFactory.GetCustomVariable1();
            thisTest.Site = site;
            thisTest.TestCon.CktID = testConFactory.GetCktID();
           // thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();
            thisTest.TestCon.Band = testConFactory.GetBand();
            //thisTest.TestCon.CABand = testConFactory.GetCABand();

            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();
            thisTest.TestCon.customVsaOperation = testConFactory.GetCustomVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;
            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.DutyCyclePDM = TCF_Setting["DUTYCYCLE_PDM"];

            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();

            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestGain = testConFactory.GetGainEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.TestIcc = testConFactory.GetIccEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIcc2 = testConFactory.GetIcc2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIbatt = testConFactory.GetIbattEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIdd = testConFactory.GetIddEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIeff = testConFactory.GetIeffEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPcon = testConFactory.GetPconEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPae = testConFactory.GetPaeEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestAcp1 = testConFactory.GetAcp1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestAcp2 = testConFactory.GetAcp2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestEUTRA = testConFactory.GetEUTRAEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestEvm = testConFactory.GetEvmEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIio1 = testConFactory.GetIio1Enabled(thisTest.TestCon.SpecNumber);

            foreach (string NsTestName in Calc.NStestConditions.Mem.Keys)
            {
                thisTest.TestCon.TestNS[NsTestName] = testConFactory.IsTest(testConFactory.GetStr("Para." + NsTestName));
            }
            foreach (string customTestName in CustomPowerMeasurement.Mem.Keys)
            {
                thisTest.TestCon.TestCustom[customTestName] = testConFactory.IsTest(customTestName, testConFactory.GetStr("Para." + customTestName), thisTest.TestCon.SpecNumber);
            }

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();


            thisTest.TestCon.TargetPout = testConFactory.GetPout();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();
            thisTest.TestCon.ACPaverages = testConFactory.GetAcpAverages();

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();

            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote + "_";

            string mode = thisTest.TestCon.PowerMode;

            string waveformName = thisTest.TestCon.ModulationStd;

            if (!String.IsNullOrEmpty(thisTest.TestCon.WaveformName))
                waveformName = waveformName + "_" + thisTest.TestCon.WaveformName;
           
            //string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "x" : thisTest.TestCon.WaveformName;
            string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "" : thisTest.TestCon.WaveformName;

            if (thisTest.TestCon.TestEvm)
            {
                SetWaveformname = SetWaveformname.Replace("FAST", "");
            }

            if (thisTest.TestCon.CktID.Equals("PR_"))
                mode = testConFactory.GetGainModeHeader();

            //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
            //    + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //     + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_" + Extra + "_" + "NOTE" + "_";

            //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
            //   + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //    + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_x_"  + "NOTE_";

            thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
               + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "NOTE_";

            if (thisTest.TestCon.TestCustom["TxLeakage"])
            {
                thisTest.TestCon.TestParaNameForTxleakage = "x_" + thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                 + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_";
            }

            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);

            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            //thisTest.dcTrigLine = EqLib.TriggerLine.PxiTrig1;  //CHANGE TO 0 TO TRI
            thisTest.dcTrigLine = EqLib.TriggerLine.PxiTrig0;  //Use generator to trigger
            //Quadsite: To support single controller dualsite
            thisTest.EqSiteTriggerArray = EqSiteTriggerArray;

            thisTest.TestCon.DutyCycle = testConFactory.GetCustomVariable2() == "" ? 0 : Convert.ToDouble(testConFactory.GetCustomVariable2());
            thisTest.TestCon.pconMultiplier = testConFactory.GetCustomVariable3() == "" ? 0 : Convert.ToDouble(testConFactory.GetCustomVariable3());

            thisTest.TestCon.InputPort = testConFactory.GetINportHeader();
            thisTest.TestCon.OutputPort = testConFactory.GetOUTportHeader();
            thisTest.TestCon.Harm2MeasBW = testConFactory.GetHarm2MeasBW();

            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();
            for(int i = 0; i<Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public RFTestFixedInputPower CreateRfFixedPinTest()
        {
            RFTestFixedInputPower thisTest = new RFTestFixedInputPower();
            thisTest.TestCon.TestReadReg1C = testConFactory.GetReadReg1CEnabled(thisTest.TestCon.SpecNumber);
            thisTest.resetSA = testConFactory.resetSA("ResetSA");
            thisTest.MathVariable = testConFactory.GetCustomVariable1();
            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            //thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();
            //thisTest.TestCon.CABand = testConFactory.GetCABand();
            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();
            thisTest.TestCon.customVsaOperation = testConFactory.GetCustomVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" && 
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();

            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestGain = testConFactory.GetGainEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.TestIcc = testConFactory.GetIccEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIcc2 = testConFactory.GetIcc2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIbatt = testConFactory.GetIbattEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIdd = testConFactory.GetIddEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIeff = testConFactory.GetIeffEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPcon = testConFactory.GetPconEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPae = testConFactory.GetPaeEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestAcp1 = testConFactory.GetAcp1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestAcp2 = testConFactory.GetAcp2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestEUTRA = testConFactory.GetEUTRAEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestEvm = testConFactory.GetEvmEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIio1 = testConFactory.GetIio1Enabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcPinForIccCal = testConFactory.GetDcPinForIccCal();

            foreach (string NsTestName in Calc.NStestConditions.Mem.Keys)
            {
                thisTest.TestCon.TestNS[NsTestName] = testConFactory.IsTest(testConFactory.GetStr("Para." + NsTestName));
            }
            foreach (string customTestName in CustomPowerMeasurement.Mem.Keys)
            {
                thisTest.TestCon.TestCustom[customTestName] = testConFactory.IsTest(customTestName, testConFactory.GetStr("Para." + customTestName), thisTest.TestCon.SpecNumber);
            }
            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.TargetPin = testConFactory.GetPin();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();
            thisTest.TestCon.ACPaverages = testConFactory.GetAcpAverages();           

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();

            thisTest.TestCon.Soak_Delay = testConFactory.GetSoakDelay();

            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote;  //hosein 05052020

            string mode = thisTest.TestCon.PowerMode;
            //if (thisTest.TestCon.CktID.Equals("PR_"))
            //    mode = testConFactory.GetGainModeHeader();
            
            string waveformName = thisTest.TestCon.ModulationStd;

            if (!String.IsNullOrEmpty(thisTest.TestCon.WaveformName))
                waveformName = waveformName + "_" + thisTest.TestCon.WaveformName;

            //string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "x" : thisTest.TestCon.WaveformName;  //hosein 04272020
            string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "" : thisTest.TestCon.WaveformName;


            if (testConFactory.GetCktID().Equals("PR_"))
            {
                //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                //     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_" + "NOTE" + "_";

                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_NOTE_";
            }
            else
            {
                //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                //     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_" + Extra + "_" + "NOTE" + "_";

                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() +  "_" + "NOTE" + "_";  // hosein "_" + Extra + 05012020
            }


            string sdasd = testConFactory.GetDacForHeader();

            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);  //hosien 05012020

            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            thisTest.dcTrigLine = EqLib.TriggerLine.PxiTrig0;
            // Quadsite: To support single controller dualsite
            thisTest.EqSiteTriggerArray = EqSiteTriggerArray;

            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public IIP3Test CreateIIP3Test()
        {
            IIP3Test thisTest = new IIP3Test();

            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            //thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();

            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();

            thisTest.TestCon.resetSA = testConFactory.resetSA("ResetSA");
            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);


            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.TargetPin = testConFactory.GetPin();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();

            thisTest.TestCon.TestIIP3 = testConFactory.GetIIP3Enabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();

            //string Extra = thisTest.TestCon.ParameterNote == "" ? "x" : thisTest.TestCon.ParameterNote;
            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote;


            string mode = thisTest.TestCon.PowerMode;
            //if (thisTest.TestCon.CktID.Equals("PR_"))
            //    mode = testConFactory.GetGainModeHeader();

            string waveformName = thisTest.TestCon.ModulationStd;
            if (!String.IsNullOrEmpty(thisTest.TestCon.WaveformName))
                waveformName = waveformName + "_" + thisTest.TestCon.WaveformName;

            //string Set_Rx_Output = "x";
            string Set_Rx_Output = "";


            Set_Rx_Output = testConFactory.GetRXportHeader();

            //thisTest.TestCon.TestParaName = "x_" + thisTest.TestCon.Band + "_" + mode + "_" + "TwoTone_Lower" + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm"
            //    + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //    + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_NOTE" + "_";

            thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "TwoTone_Lower" + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm"
                + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_NOTE" + "_";



            //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "TwoTone_Lower"
            //   + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //    + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader();

            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);

            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            thisTest.dcTrigLine = EqLib.TriggerLine.PxiTrig0;
            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public Calculate CreateCalculateTest()
        {
            Calculate thisTest = new Calculate();

            string power = testConFactory.GetStr("Pin");
            if (power == "")
                power = testConFactory.GetStr("Pout");
            string mode = testConFactory.GetPowerMode();
            if (testConFactory.GetCktID().Equals("PR_"))
                mode = testConFactory.GetGainModeHeader();
            string waveformName = testConFactory.GetModulationID();
            if (!String.IsNullOrEmpty(testConFactory.GetWaveformName()))
                waveformName = waveformName + "_" + testConFactory.GetWaveformName();
            thisTest.MathVariable = testConFactory.GetCustomVariable1();
            thisTest.Site = site;
            thisTest.DcSettings = testConFactory.GetDcSettings();
            thisTest.CktID = testConFactory.GetCktID();
            Dictionary<string, string> dummy = new Dictionary<string, string>() { };
            thisTest.TestGain = testConFactory.GetGainEnabled(dummy);
            thisTest.TestAcp1 = testConFactory.GetAcp1Enabled(dummy);
            thisTest.TestAcp2 = testConFactory.GetAcp2Enabled(dummy);
            thisTest.TestEUTRA = testConFactory.GetEUTRAEnabled(dummy);

            thisTest.TestParaName = testConFactory.GetCktID() + testConFactory.GetBand() + "_" + testConFactory.GetPowerMode() + "_" + testConFactory.GetModulationID() + "_" + power + "dBm" + "_" + testConFactory.GetFreqSG() + "MHz" + "_" + thisTest.DcSettings["Vcc"].Volts + "Vcc"
            + "_" + thisTest.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "NOTE" + "_";
            thisTest.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestParaName);

            return thisTest;
        }

        public DcTest CreateDcTest()
        {
            DcTest thisTest = new DcTest();

            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            thisTest.MathVariable = testConFactory.GetCustomVariable1();
            thisTest.TestCon.TestIcc = testConFactory.GetIccEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIcc2 = testConFactory.GetIcc2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIbatt = testConFactory.GetIbattEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIdd = testConFactory.GetIddEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISdata1 = testConFactory.GetISdata1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISclk1 = testConFactory.GetISclk1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIiO1 = testConFactory.GetIio1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISdata2 = testConFactory.GetISdata2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISclk2 = testConFactory.GetISclk2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIiO2 = testConFactory.GetIio2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestWatt = testConFactory.GetWattEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.DelayCurrent = (int)testConFactory.GetDbl("Delay.Current");
            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();


            //string Extra = thisTest.TestCon.ParameterNote == "" ? "x" : thisTest.TestCon.ParameterNote;
            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote;
            string inport = testConFactory.GetINportHeader() == "" ? "" : testConFactory.GetINportHeader();  //hosein 04272020
            string outport = testConFactory.GetOUTportHeader() == "" ? "" : testConFactory.GetOUTportHeader();  //hosein 04272020

            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();


            if (testConFactory.GetCktID().Equals("PR_"))
            {
                //string ass = testConFactory.GetDacForHeader();
                //thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //  + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_" + "NOTE" + "_";

                string ass = testConFactory.GetDacForHeader();
                thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                  + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_"  + inport + "_" + outport + "_" + "NOTE" + "_" + Extra;  //hosein 05042020
            }
            else
            {
                //string ass = testConFactory.GetDacForHeader();
                //thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //     + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_" + Extra + "_" + "NOTE" + "_";

                string ass = testConFactory.GetDacForHeader();
                thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                     + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "NOTE" + "_" + Extra;  //hosein 05042020


            }

            string sad = testConFactory.GetOUTportHeader();

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";

            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();
            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public DcLeakageTest CreateDcLeakageTest()
        {
            DcLeakageTest thisTest = new DcLeakageTest();

            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName(); 
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            thisTest.MathVariable = testConFactory.GetCustomVariable1();
            thisTest.TestCon.TestIcc = testConFactory.GetIccEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIcc2 = testConFactory.GetIcc2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIbatt = testConFactory.GetIbattEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIdd = testConFactory.GetIddEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISdata1 = testConFactory.GetISdata1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISclk1 = testConFactory.GetISclk1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIiO1 = testConFactory.GetIio1Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISdata2 = testConFactory.GetISdata2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestISclk2 = testConFactory.GetISclk2Enabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestIiO2 = testConFactory.GetIio2Enabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.DelayCurrent = (int)testConFactory.GetDbl("Delay.Current");

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();


            //string Extra = thisTest.TestCon.ParameterNote == "" ? "x" : thisTest.TestCon.ParameterNote;
            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote;


            string sd = testConFactory.GetDacForHeader();

            //thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + "x" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //     + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + "x" + "_" + "x" + "_" + Extra + "_" + "NOTE" + "_";

            thisTest.TestCon.TestParaName = "_Q_" + thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                 + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + "NOTE" + "_" + Extra;  //hosein 05042020



            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            //thisTest.TestCon.TestParaName = "_" + thisTest.TestCon.PowerMode + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
            //+ "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd";


            // thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);
            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }
            return thisTest;
        }

        public PinSweepTest CreatePinSweepTest()
        {
            PinSweepTest thisTest = new PinSweepTest();


            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.resetSA = testConFactory.resetSA("ResetSA");
            thisTest.PinSweepTraceEnable = TCF_Setting["Pin_Sweep_Trace_Enable"];
            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            //thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();

            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestGain = testConFactory.GetGainEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPae = testConFactory.GetPaeEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.PinSweepStart = testConFactory.GetPinSweepStart();
            thisTest.TestCon.PinSweepStop = testConFactory.GetPinSweepStop();
            thisTest.TestCon.TargetpoutAtRefgain = testConFactory.GetCustomVariable1() == "" ?
                                                    float.NaN : Convert.ToSingle(testConFactory.GetCustomVariable1());

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();


            if (thisTest.TestCon.PinSweepStop - thisTest.TestCon.PinSweepStart > 40.0)
            {
                throw new Exception("Pin Sweep range must be less than 30dB");
            }

            thisTest.TestCon.ExpectedGain = (float)testConFactory.GetDbl("ExpectedGain");
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();
            string mode = thisTest.TestCon.PowerMode;
            //if (thisTest.TestCon.CktID.Equals("PR_"))
            // mode = testConFactory.GetGainModeHeader();


            string Extra = thisTest.TestCon.ParameterNote == "" ? "" : thisTest.TestCon.ParameterNote;  //hosein 05042020 remove x


            string Set_Rx_Output = "x";
            if (testConFactory.GetCktID().Equals("PR_"))
            {
                Set_Rx_Output = testConFactory.GetRXportHeader();

                //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "CW" + "_" + "Sweep" + "_" + "x" + "_" + "x"
                //  + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //  + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_NOTE" + "_";

                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "CW" + "_" + "Sweep"
                  + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                  + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Extra + "_NOTE" + "_";
            }
            else
            {
                //thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "CW" + "_" + "Sweep" + "_" + "x" + "_" + "x"
                //   + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                //   + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Set_Rx_Output + "_" + Extra + "_NOTE" + "_";

                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + "CW" + "_" + "Sweep"
                   + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                   + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + Set_Rx_Output + "_" + Extra + "_NOTE" + "_";
            }

            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            try
            {
                string[] compPoints = testConFactory.GetStr("PinSweepCompPoints").Split(',').ToArray();
                foreach (string compPoint in compPoints)
                {
                    thisTest.TestCon.TestCompression[Convert.ToDouble(compPoint)] = true;
                }
            }
            catch (Exception e)
            {
                throw new Exception("TCF PinSweepCompPoints column was not formatted correctly.\nCorrect format example: 1,2,3,7\n    which tests P1dB, P2dB, P3dB, and P7dB\n\nValid values are 0.5 to 7.0 in 0.5dB increments\n\n" + e.ToString());
            }

            //    thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);
            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            //keng shan Added
            //thisTest.TestCon.TestGainLinearity = testConFactory.GetGainLinearityEnabled();
            thisTest.TestCon.TestPoutDrop = testConFactory.GetPoutDropEnabled(thisTest.TestCon.SpecNumber);
            thisTest.dcTrigLine = (TriggerLine)EqSiteTriggerArray[site][0];

            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;

        }

        public ContinuityTest CreateContinuityTest()
        {
            ContinuityTest thisTest = new ContinuityTest();

            thisTest.Site = site;

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.DelayCurrent = (int)testConFactory.GetDbl("Delay.Current");
            thisTest.TestCon.TestParaName = "CONTINUITY";

            string paramNote = testConFactory.GetStr("ParameterNote");
            if (paramNote != "") thisTest.TestCon.TestParaName += "_" + paramNote;  // Just add additional info to test name

            return thisTest;
        }

        public MipiTest CreateMipiTest()
        {
            MipiTest thisTest = new MipiTest();


            thisTest.Site = site;

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();


            thisTest.TestCon.WaveformName = testConFactory.GetStr("Waveform");    // "TestCon.WaveformName"  "Waveform"

            thisTest.TestCon.TestParaName = "M_" + "MIPI";
            thisTest.TestCon.PowerMode = testConFactory.GetStr("Power_Mode");
            thisTest.TestCon.TRX = testConFactory.GetStr("TRX");
            thisTest.TestCon.paramNote = testConFactory.GetStr("ParameterNote");


            if (thisTest.TestCon.WaveformName.ToUpper().Contains("RX"))
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            else
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];


            if (thisTest.TestCon.PowerMode.ToUpper().Contains("QC"))
            {
                thisTest.TestCon.Reg_hex = testConFactory.GetStr("Waveform");
            }
            else if (thisTest.TestCon.PowerMode.ToUpper().Contains("READ"))
            {
                thisTest.TestCon.Reg_hex = testConFactory.GetStr("Waveform");
            }
            else if (thisTest.TestCon.PowerMode.ToUpper().Contains("UNITID"))
            {
                thisTest.TestCon.Reg_hex = testConFactory.GetStr("Waveform");
            }
            else if (thisTest.TestCon.PowerMode.ToUpper().Contains("OTP"))
            {

                thisTest.TestCon.TestParaName = "M_OTP_";
                thisTest.TestCon.Rev = testConFactory.GetStr("Waveform");
                thisTest.TestCon.Reg_hex = testConFactory.GetStr("Waveform");
            }



            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();




            //if (thisTest.TestCon.WaveformName == "")
            //{
            //    try
            //    {
            //        string reg_Read = testConFactory.GetStr("DigitalRegister").Trim().ToUpper();
            //        if (reg_Read.StartsWith("0X"))
            //        {
            //            thisTest.TestCon.Reg_hex = reg_Read.Replace("0X", "");
            //        }
            //        else if (reg_Read.StartsWith("0B") && reg_Read.Trim().Length == 10)
            //        {
            //            thisTest.TestCon.Reg_hex = Convert.ToInt32(reg_Read.Replace("0B", ""), 2).ToString("X");
            //        }
            //        else
            //        {
            //            throw new Exception("TCF DigitalRegister column was not formatted correctly.\nBad value " + testConFactory.GetStr("DigitalRegister") + "\nCorrect format is either hex (precede with 0x) or binary (8 digits preceded by 0b)\n    for example 0xF or 0b00001111");
            //        }
            //        thisTest.TestCon.TestParaName = "M_" + "MIPI_ReadReg0x" + thisTest.TestCon.Reg_hex;
            //        thisTest.TestCon.WaveformName = "ReadReg" + thisTest.TestCon.Reg_hex;
            //    }
            //    catch (Exception e)
            //    {
            //        throw new Exception("TCF DigitalData or DigitalRegister column was not formatted correctly.\nBad entry is either " + testConFactory.GetStr("DigitalRegister") + " or " + testConFactory.GetStr("DigitalData") + "/nCorrect format is either hex (precede with 0x) or binary (precede with 0b)\n    for example 0xF or 0b00001111\n\n" + e.ToString());
            //    }
            //}
            //if (thisTest.TestCon.WaveformName != "")
            //{
            //    thisTest.TestCon.Reg_hex = testConFactory.GetStr("Waveform");
            //}

            //if (thisTest.TestCon.PowerMode.Contains("QC"))
            //{

            //}

            //if (thisTest.TestCon.WaveformName.Contains("READTS"))
            //{
            //    thisTest.TestCon.Reg_hex = Digital_Definitions_Part_Specific["TEMPERATURE_REG"];
            //    thisTest.TestCon.minTemp = Convert.ToInt16(Digital_Definitions_Part_Specific["MIN_TEMP"]);
            //    thisTest.TestCon.maxTemp = Convert.ToInt16(Digital_Definitions_Part_Specific["MAX_TEMP"]);
            //    thisTest.TestCon.numTempSteps = 255; Convert.ToInt16(Digital_Definitions_Part_Specific["NUM_TEMP_STEPS"]);
            //}


            //if (thisTest.TestCon.paramNote != "") thisTest.TestCon.TestParaName += "_" + thisTest.TestCon.paramNote;  // Just add additional info to test name

            return thisTest;
        }

        public OtpBurnTest CreateOtpBurnTest(byte site)
        {
            OtpBurnTest thisTest = new OtpBurnTest();

            thisTest.Site = site;

            string waveformName = testConFactory.GetWaveformName();
            string waveformName2 = testConFactory.GetStr("waveformname").ToUpper();  //20190709 New Otptest

            thisTest.TestCon.TestParaName = "M_" + waveformName;
            thisTest.TestCon.TestType = waveformName;

            if (waveformName2 != "")
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName.Replace(waveformName, waveformName2);

            if (waveformName.ToUpper().Contains("LNA"))
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            else
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];


            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();



            string paramNote = testConFactory.GetStr("ParameterNote");
            if (paramNote != "") thisTest.TestCon.TestParaName += "_" + paramNote;  // Just add additional info to test name

            return thisTest;
        }

        public OtpReadTest CreateOtpReadTest(byte site)
        {
            OtpReadTest thisTest = new OtpReadTest();

            thisTest.Site = site;

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            string waveformName = testConFactory.GetWaveformName();
            string waveformName2 = testConFactory.GetStr("waveformname").ToUpper(); //20190709 New Otptest

            thisTest.TestCon.TestParaName = "M_" + waveformName;
            thisTest.TestCon.TestType = waveformName;

            if (waveformName2 != "")
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName.Replace(waveformName, waveformName2);


            if (waveformName.ToUpper().Contains("LNA"))
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            else
                thisTest.TestCon.SlaveAddr = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];

            string paramNote = testConFactory.GetStr("ParameterNote");
            if (paramNote != "") thisTest.TestCon.TestParaName += "_" + paramNote;  // Just add additional info to test name

            return thisTest;
        }

        public RfOnOffTest CreateRfOnOffTest()
        {
            RfOnOffTest thisTest = new RfOnOffTest();

            thisTest.Site = site;
            string testMode = testConFactory.GetTestMode();
            thisTest.TestCon.TestSw = testMode.Contains("SW");
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.CktID = testConFactory.GetCktID();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            //thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain() - 5; Not sure why -5  KH
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.TXRX = testConFactory.GetTXRX();

            thisTest.TestCon.UsePreviousPin = (testConFactory.GetStr("Pin").ToUpper() == "UP") ? true : false; //use to set use previous Pin from servo

            if (!thisTest.TestCon.UsePreviousPin)
            {
                if (thisTest.TestCon.TXRX.Contains("RX"))    // KH need to do fixed pin if testing switch from antenna to RX port
                {
                    thisTest.TestCon.TargetPin = testConFactory.GetPin(); //thisTest.TestCon.TargetPout - thisTest.TestCon.ExpectedGain;
                }
                else
                    thisTest.TestCon.TargetPout = testConFactory.GetPout();
            }

            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();
            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            if (thisTest.TestCon.TXRX.Contains("RX"))    // KH need to do fixed pin if testing switch from antenna to RX port
            {
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" +
                thisTest.TestCon.FreqSG + "MHz" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader();
            }
            else
            {
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + thisTest.TestCon.PowerMode + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" +
                thisTest.TestCon.FreqSG + "MHz" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader();
            }

            thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);
            //keng shan added
            thisTest.TestCon.SWTIMECUSTOM = testConFactory.GetStr("SWTIMECUSTOM").Trim().ToUpper();
            //thisTest.TestCon.strSWLvlSpec = testConFactory.GetStr("SwLvlSpec");
            //thisTest.TestCon.strSWSpecPos = testConFactory.GetStr("SwSpecPos");             
            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            if (testConFactory.IsTest(testConFactory.GetStr("Para.Cpl")))
                thisTest.TestCon.Cpl = testConFactory.GetCplPrefix();
            else
                thisTest.TestCon.Cpl = "";

            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }
            return thisTest;
        }

        public TimingTestFixedOutputPower CreateTimingFixedPoutTest()
        {
            TimingTestFixedOutputPower thisTest = new TimingTestFixedOutputPower();

            thisTest.resetSA = testConFactory.resetSA("ResetSA");
            //thisTest.MathVariable = testConFactory.GetVariable();
            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.CktID = testConFactory.GetCktID();

            thisTest.TestCon.TestMode = testConFactory.GetTestMode();
            //thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();  //enabled by hosein
            //thisTest.TestCon.CABand = testConFactory.GetCABand();
            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();
            //   thisTest.TestCon.vs = testConFactory.GetVsaOperation();
            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();

            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestGain = testConFactory.GetGainEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPae = testConFactory.GetPaeEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcPinForIccCal = testConFactory.GetDcPinForIccCal();

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.TargetPout = testConFactory.GetPout();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();

            thisTest.TestCon.Regcustom = testConFactory.GetRegcustom();

            thisTest.TestCon.Threshold = Convert.ToDouble(testConFactory.GetCustomVariable2());
            thisTest.TestCon.IsForwarSearch = testConFactory.GetCustomVariable1().Contains("F") ? true : false;

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();

            string Extra = thisTest.TestCon.ParameterNote == "" ? "x" : thisTest.TestCon.ParameterNote;

            string mode = thisTest.TestCon.PowerMode;
            //if (thisTest.TestCon.CktID.Equals("PR_"))
            //    mode = testConFactory.GetGainModeHeader();
            string waveformName = thisTest.TestCon.ModulationStd;
            if (!String.IsNullOrEmpty(thisTest.TestCon.WaveformName))
                waveformName = waveformName + "_" + thisTest.TestCon.WaveformName;

            thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + waveformName
               + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader();


            string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "x" : thisTest.TestCon.WaveformName;

            if (testConFactory.GetCktID().Equals("PR_"))
            {
                string asd = testConFactory.GetDacForHeader();
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_" + "NOTE" + "_" + "x";
            }
            else
            {
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPout" + "_" + thisTest.TestCon.TargetPout + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_" + "x" + "_" + "NOTE" + "_" + "x";
            }

            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            //   thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);
            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            thisTest.dcTrigLine = (TriggerLine)EqSiteTriggerArray[site][2];
            // Quadsite: To support single controller dualsite
            thisTest.EqSiteTriggerArray = EqSiteTriggerArray;
            /*
            foreach (string PinName in thisTest.TestCon.DcSettings.Keys)
            {

                string Full_name = thisTest.TestCon.CktID + thisTest.TestCon.DcSettings[PinName].iParaName + thisTest.TestCon.TestParaName + "x";

                foreach (KeyValuePair<int, Avago.ATF.Shares.SerialDef> Bin in ResultBuilder.All)
                {
                    if (Bin.Value.RangeCollection.ContainsKey(Full_name))
                    {
                        Avago.ATF.Shares.ParameterRangeDef Spec = Bin.Value.RangeCollection[Full_name];
                        if (PinName.ToUpper().Contains("VIO"))
                        {
                            if (thisTest.TestCon.DcSettings[PinName].Current <= Spec.Range.TheMax || thisTest.TestCon.DcSettings[PinName].Current >= 100 * Spec.Range.TheMax)
                            {
                                if (Spec.Range.TheMax < 999)
                                {
                                    MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + thisTest.GetType().Name + " : " + thisTest.TestCon.DcSettings[PinName].iParaName +
                                        "\n Compliance : " + thisTest.TestCon.DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            if (thisTest.TestCon.DcSettings[PinName].Current <= Spec.Range.TheMax)
                            {
                                if (Spec.Range.TheMax < 999)
                                {
                                    MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + thisTest.GetType().Name + " : " + thisTest.TestCon.DcSettings[PinName].iParaName +
                                        "\n Compliance : " + thisTest.TestCon.DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    return null;
                                }
                            }
                        }

                    }
                }
            }
            */
            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public TimingTestFixedInputPower CreateTimingFixedPinTest()
        {
            TimingTestFixedInputPower thisTest = new TimingTestFixedInputPower();
            thisTest.resetSA = testConFactory.resetSA("ResetSA");
            //thisTest.MathVariable = testConFactory.GetVariable();
            thisTest.Site = site;
            thisTest.TestCon.Band = testConFactory.GetBand();
            thisTest.TestCon.CktID = testConFactory.GetCktID();

            thisTest.TestCon.TestMode = testConFactory.GetTestMode();
            //thisTest.TestCon.CplpreFix = testConFactory.GetCplPrefix();
            //thisTest.TestCon.CABand = testConFactory.GetCABand();
            thisTest.TestCon.VsgOperation = testConFactory.GetVsgOperation();
            thisTest.TestCon.VsaOperation = testConFactory.GetVsaOperation();

            thisTest.SkipOutputPortOnFail = TCF_Setting["Skip_Output_Port_On_Fail"] == "TRUE" &&
                                            (thisTest.TestCon.VsaOperation == Operation.VSAtoANT ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT1 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT2 ||
                                            thisTest.TestCon.VsaOperation == Operation.VSAtoANT3)
                                            ? true : false;

            thisTest.Mordor = TCF_Setting["MORDOR"] == "TRUE";
            thisTest.TestCon.PowerMode = testConFactory.GetPowerMode();
            thisTest.TestCon.ModulationStd = testConFactory.GetModulationID();
            thisTest.TestCon.WaveformName = testConFactory.GetWaveformName();

            thisTest.TestCon.TestPout = testConFactory.GetPoutEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPin = testConFactory.GetPinEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestGain = testConFactory.GetGainEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.TestItotal = testConFactory.GetItotalEnabled(thisTest.TestCon.SpecNumber);
            thisTest.TestCon.TestPae = testConFactory.GetPaeEnabled(thisTest.TestCon.SpecNumber);

            thisTest.TestCon.DcPinForIccCal = testConFactory.GetDcPinForIccCal();

            thisTest.TestCon.DcSettings = testConFactory.GetDcSettings();

            thisTest.TestCon.TargetPin = testConFactory.GetPin();
            thisTest.TestCon.ExpectedGain = testConFactory.GetExpectedGain();
            thisTest.TestCon.FreqSG = testConFactory.GetFreqSG();

            thisTest.TestCon.Regcustom = testConFactory.GetRegcustom();

            thisTest.TestCon.Threshold = Convert.ToDouble(testConFactory.GetCustomVariable2());
            thisTest.TestCon.IsForwarSearch = testConFactory.GetCustomVariable1().Contains("F") ? true : false;

            thisTest.TestCon.ParameterNote = testConFactory.GetParameterNote();

            string Extra = thisTest.TestCon.ParameterNote == "" ? "x" : thisTest.TestCon.ParameterNote;

            string mode = thisTest.TestCon.PowerMode;
            //if (thisTest.TestCon.CktID.Equals("PR_"))
            //    mode = testConFactory.GetGainModeHeader();
            string waveformName = thisTest.TestCon.ModulationStd;
            if (!String.IsNullOrEmpty(thisTest.TestCon.WaveformName))
                waveformName = waveformName + "_" + thisTest.TestCon.WaveformName;

            thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + waveformName
               + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader();


            string SetWaveformname = thisTest.TestCon.WaveformName == "" ? "x" : thisTest.TestCon.WaveformName;

            if (testConFactory.GetCktID().Equals("PR_"))
            {
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + "x" + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_x_" + "NOTE" + "_" + "x";
            }
            else
            {
                thisTest.TestCon.TestParaName = thisTest.TestCon.Band + "_" + mode + "_" + thisTest.TestCon.ModulationStd + "_" + SetWaveformname
                     + "_" + "FixedPin" + "_" + thisTest.TestCon.TargetPin + "dBm" + "_" + thisTest.TestCon.FreqSG + "MHz" + "_" + thisTest.TestCon.DcSettings["Vcc"].Volts + "Vcc"
                      + "_" + thisTest.TestCon.DcSettings["Vdd"].Volts + "Vdd" + "_" + testConFactory.GetDacForHeader() + "_" + testConFactory.GetINportHeader() + "_" + testConFactory.GetOUTportHeader() + "_" + "x" + "_x_" + "NOTE" + "_" + "x";
            }





            if (thisTest.TestCon.CktID.Equals("PT_") && testConFactory.GetRXportHeader().Contains("OUT"))
                thisTest.TestCon.TestParaName = thisTest.TestCon.TestParaName + "_" + testConFactory.GetRXportHeader();
            //   thisTest.TestCon.TestParaName = testConFactory.AppendParamNoteToParamName(thisTest.TestCon.TestParaName);
            thisTest.TestCon.VIO32MA = testConFactory.GetVIO32MAEnabled();
            thisTest.TestCon.VIORESET = testConFactory.GetVIORESETEnabled();

            thisTest.dcTrigLine = (TriggerLine) EqSiteTriggerArray[site][2];
            // Quadsite: To support single controller dualsite
            thisTest.EqSiteTriggerArray = EqSiteTriggerArray;
            /*
            foreach (string PinName in thisTest.TestCon.DcSettings.Keys)
            {

                string Full_name = thisTest.TestCon.CktID + thisTest.TestCon.DcSettings[PinName].iParaName + thisTest.TestCon.TestParaName + "x";

                foreach (KeyValuePair<int, Avago.ATF.Shares.SerialDef> Bin in ResultBuilder.All)
                {
                    if (Bin.Value.RangeCollection.ContainsKey(Full_name))
                    {
                        Avago.ATF.Shares.ParameterRangeDef Spec = Bin.Value.RangeCollection[Full_name];
                        if (PinName.ToUpper().Contains("VIO"))
                        {
                            if (thisTest.TestCon.DcSettings[PinName].Current <= Spec.Range.TheMax || thisTest.TestCon.DcSettings[PinName].Current >= 100 * Spec.Range.TheMax)
                            {
                                if (Spec.Range.TheMax < 999)
                                {
                                    MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + thisTest.GetType().Name + " : " + thisTest.TestCon.DcSettings[PinName].iParaName +
                                        "\n Compliance : " + thisTest.TestCon.DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            if (thisTest.TestCon.DcSettings[PinName].Current <= Spec.Range.TheMax)
                            {
                                if (Spec.Range.TheMax < 999)
                                {
                                    MessageBox.Show("Please Check Compliance and USL, not allow these setting Compliance <= USL or Compliance >= 100 * USL. \n" + thisTest.GetType().Name + " : " + thisTest.TestCon.DcSettings[PinName].iParaName +
                                        "\n Compliance : " + thisTest.TestCon.DcSettings[PinName].Current + "\n USL : " + Spec.Range.TheMax + "\n " + Full_name);
                                    return null;
                                }
                            }
                        }

                    }
                }
            }
            */
            thisTest.TestCon.TestDPAT = testConFactory.GetDPAT();

            for (int i = 0; i < Eq.NumSites; i++)
            {
                foreach (string Key in thisTest.TestCon.TestDPAT.Keys)
                {
                    if (ResultBuilder.All == null)
                    {
                        double SpecL = -999;
                        double SpecH = 999;

                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, SpecL, SpecH, i);
                    }
                    else
                    {
                        DPAT.AddPara(Key, thisTest.TestCon.TestDPAT[Key].Fomula, thisTest.TestCon.TestDPAT[Key].SpecCondition, thisTest.TestCon.TestDPAT[Key].SetValue, Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMin), Convert.ToDouble(ResultBuilder.All[1].RangeCollection[Key].Range.TheMax), i);
                    }
                }
            }

            return thisTest;
        }

        public List<MipiSyntaxParser.ClsMIPIFrame> GetMipiCommands(Dictionary<string, string> TestCon)
        {
            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommandsTemp = new List<MipiSyntaxParser.ClsMIPIFrame>();
            //string MipiCommand_tcf = GetStr(TestCon, "MipiCommands").Trim().ToUpper();
            string MipiCommandString_tcf = GetStr(TestCon, "MIPI Commands").Trim().ToUpper();
            string MipiRegcustomUDR = GetRegcustomUDRMipiCommands(TestCon);
            string VaribleData = "";

            //if (MIPI_Command_Strings.ContainsKey(MipiCommand_tcf))
            //{
            //MipiCommandsTemp = MipiSyntaxParser.CreateListOfMipiFrames(MIPI_Command_Strings[MipiCommand_tcf]);
            MipiCommandsTemp = MipiSyntaxParser.CreateListOfMipiFrames(MipiCommandString_tcf);

            if (MipiRegcustomUDR != "" && MipiRegcustomUDR != null)
                MipiCommandsTemp.AddRange(MipiSyntaxParser.CreateListOfMipiFrames(MipiRegcustomUDR));

            // Replace Varibles in command syntax with Data values from the TCF
            foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommandsTemp)
            {
                if (!command.IsValidFrame)  // Indicates there is a non valid Hex number or Varible name
                {
                    VaribleData = GetStr(TestCon, command.Data_hex).Trim().ToUpper();  // search the header conditions for a match to the Varible name.
                    if (VaribleData == "")
                        MessageBox.Show("Warning: Varible name found in MIPI Command Syntax:" + command.Data_hex + " No column header with this condtion exists", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        VaribleData.Replace("0X", "");
                        command.Data_hex = VaribleData;
                    }
                }
            }



            //}

            //else
            //    MessageBox.Show("Warning: Requested MipiCommand for test" + MipiCommand_tcf + "not found on MIPI tab of TCF", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            #region Old Method

            //string TestMode_tcf = GetStr("Test Mode").Trim().ToUpper();
            //string PowerMode_tcf = GetPowerMode().Trim().ToUpper();
            //string waveform_tcf = GetWaveformName().Trim().ToUpper();

            //// MIPI and DC Leakage Tests
            //if ((TestMode_tcf.Contains("MIPI")) || (TestMode_tcf.Contains("DC_LEAKAGE")))
            //{
            //    if ((waveform_tcf.Contains("PDM")) || (PowerMode_tcf.Contains("PDM")))
            //    {
            //        MipiWaveformNames = EqHSDIO.CreateWaveformList("PDM");
            //    }

            //    else if ((waveform_tcf.Contains("VIOOFF")) || (PowerMode_tcf.Contains("VIOOFF")))
            //    {
            //        MipiWaveformNames = EqHSDIO.CreateWaveformList("VIOOFF");
            //    }

            //    else if ((waveform_tcf.Contains("VIOON")) || (PowerMode_tcf.Contains("VIOON")))
            //    {
            //        MipiWaveformNames = EqHSDIO.CreateWaveformList("VIOON");
            //    }

            //    else if (waveform_tcf.Contains("FUNCTIONAL"))
            //    {
            //        MipiWaveformNames = EqHSDIO.CreateWaveformList("FUNCTIONAL");
            //    }
            //}
            //// All other tests
            //else
            //{
            //    string nameInMemory = "";
            //    string sWconfig = "";
            //    string band = GetBand();
            //    string rxport = GetRXport();
            //    string txport = GetTXport();


            //    string MipiDacBitQ1 = GetDacQ1();
            //    string MipiDacBitQ2 = GetDacQ2();

            //    if (MipiDacBitQ1 == "" || MipiDacBitQ2 == "")
            //    {
            //        MessageBox.Show("Warning: Blank value in the Daq column", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return MipiWaveformNames;
            //    }
            //    else
            //    {
            //        int DacQ1 = Convert.ToInt32(MipiDacBitQ1);
            //        int DacQ2 = Convert.ToInt32(MipiDacBitQ2);
            //        string dacQ1Bit = EqHSDIO.dacQ1NamePrefix + DacQ1.ToString();
            //        string dacQ2Bit = EqHSDIO.dacQ2NamePrefix + DacQ2.ToString();
            //        string coupler = GetCoupler();


            //        if ((txport.Contains("TXISO"))|| GetPowerMode().Contains("RX"))
            //        {
            //            nameInMemory = band + txport + rxport;
            //        }
            //        else if (GetModulationID().Contains ("WCDMA"))// for WCDMA
            //        {
            //            nameInMemory = "WCDMA" + PowerMode_tcf + band + txport + rxport;
            //        }

            //        else //add power mode
            //        {
            //            nameInMemory = PowerMode_tcf + band + txport + rxport;
            //        }


            //        sWconfig = "";

            //        if (TestMode_tcf.Contains("RFONOFF"))
            //        {
            //            string rfOnOffMode = TestMode_tcf + this.GetBand();
            //            //keng shan eddited
            //            MipiWaveformNames = EqHSDIO.CreateWaveformList(nameInMemory, dacQ1Bit, dacQ2Bit); //, sWconfig, rfOnOffMode);
            //        }
            //        else  // Most tests
            //        {
            //            if (coupler != "")
            //                MipiWaveformNames = EqHSDIO.CreateWaveformList(nameInMemory, dacQ1Bit, dacQ2Bit, coupler);
            //            else
            //                MipiWaveformNames = EqHSDIO.CreateWaveformList(nameInMemory, dacQ1Bit, dacQ2Bit);
            //        }
            //    }
            //}
            #endregion Old Method

            return MipiCommandsTemp;
        }

        public TimingTestCondition.cTimingCondition GetTimingMipiCommands(Dictionary<string, string> TestCon)
        {

            TimingTestCondition.cTimingCondition TimingCondition = new TimingTestCondition.cTimingCondition();
            string strMipiMipiCommand = GetRegcustomTimingMipiCommands(ref TimingCondition.nBefore_Command, ref TimingCondition.Before_Udelay, ref TimingCondition.After_Udelay, TestCon);

            string VaribleData = "";

            if (strMipiMipiCommand != null)
            {
                TimingCondition.Mipi = MipiSyntaxParser.CreateListOfMipiFrames(strMipiMipiCommand);
            }

            // Replace Varibles in command syntax with Data values from the TCF
            foreach (MipiSyntaxParser.ClsMIPIFrame command in TimingCondition.Mipi)
            {
                if (!command.IsValidFrame)  // Indicates there is a non valid Hex number or Varible name
                {
                    VaribleData = GetStr(TestCon, command.Data_hex).Trim().ToUpper();  // search the header conditions for a match to the Varible name.
                    if (VaribleData == "")
                        MessageBox.Show("Warning: Varible name found in MIPI Command Syntax:" + command.Data_hex + " No column header with this condtion exists", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        VaribleData.Replace("0X", "");
                        command.Data_hex = VaribleData;
                    }
                }
            }

            return TimingCondition;
        }

        public string GetStr(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                return dic[theKey];
            }
            catch
            {
                string msg = string.Format("Test Condition File doesn't contain column \"{0}\"", theKey);
                PromptManager.Instance.ShowError(msg);
                m_validationDo.ErrorMessage = msg;
            }

            return "";
        }

        public string GetRegcustomUDRMipiCommands(Dictionary<string, string> TestCon)
        {
            string strRegcustom = GetStr(TestCon, "REGCUSTOM").Trim().ToUpper();

            if (strRegcustom == "") return null;


            string TCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");
            string DUTRevision = TCF_Setting["Sample_Version"];
            StreamReader reader;

            //foreach (string tempRevison in Path.GetFileName(TCFpath).Split('_'))
            //{
            //    if (System.Text.RegularExpressions.Regex.IsMatch(tempRevison, @"^[AB]{1}\d{1}[A-Z]{1}"))
            //    {
            //        DUTRevision = tempRevison;
            //        break;
            //    }
            //}

            string[] arrRegcustom = strRegcustom.Split('.');

            string strFullmipiCommands = "";
            string currSlaveAdd = "", previouSlaveAdd = "";

            foreach (string currentRegcustom in arrRegcustom)
            {
                if (currentRegcustom.Contains("OUT"))
                {
                    //string ScriptPath = Path.GetDirectoryName(TCFpath).Replace("TCF", "Script");
                    string ScriptPath = ClothoRootDir + @"Script\";
                    reader = new StreamReader(Path.Combine(ScriptPath, DUTRevision, "TUNABLE", currentRegcustom + ".txt"), System.Text.Encoding.Default);

                    while (!reader.EndOfStream)
                    {
                        string Getstring = reader.ReadLine().Trim().ToUpper();

                        if (Getstring.ToUpper() == "#START")
                        {
                            while (!reader.EndOfStream)
                            {
                                Getstring = reader.ReadLine().Trim().ToUpper();

                                if (Getstring.ToUpper() == "#START")
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        Getstring = reader.ReadLine().Trim().ToUpper();
                                        if (Getstring.ToUpper() != "")
                                        {
                                            if (Getstring.Contains("TX")) currSlaveAdd = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
                                            else if (Getstring.Contains("RX")) currSlaveAdd = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];

                                            if (previouSlaveAdd != currSlaveAdd)
                                            {
                                                previouSlaveAdd = currSlaveAdd;
                                                strFullmipiCommands += currSlaveAdd;
                                            }

                                            if (Getstring.ToUpper() != "#END")
                                            {
                                                Getstring = Getstring.Replace("RXREG", "");
                                                Getstring = Getstring.Replace("TXREG", "");

                                                string[] arrMipiHex = Getstring.Split(':');

                                                strFullmipiCommands += ("(0x" + arrMipiHex[0] + ",0x" + arrMipiHex[1] + ")");
                                            }
                                            else if (Getstring.ToUpper() == "#END")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }

            return strFullmipiCommands;
        }

        public string GetRegcustomTimingMipiCommands(ref int nBeforeCommand, ref double BeforeDelay, ref double AfterDelay, Dictionary<string, string> TestCon)
        {
            string strRegcustom = GetStr(TestCon, "REGCUSTOM").Trim().ToUpper();
            bool FoundFirstDelay = false;
            if (strRegcustom == "") return null;


            string TCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");
            string DUTRevision = TCF_Setting["Sample_Version"];
            StreamReader reader;

            //foreach (string tempRevison in Path.GetFileName(TCFpath).Split('_'))
            //{
            //    if (System.Text.RegularExpressions.Regex.IsMatch(tempRevison, @"^[AB]{1}\d{1}[A-Z]{1}"))
            //    {
            //        DUTRevision = tempRevison;
            //        break;
            //    }
            //}

            string[] arrRegcustom = strRegcustom.Split('.');

            string strFullmipiCommands = "";
            string currSlaveAdd = "", previouSlaveAdd = "";
            int nCommand = 0;
            bool IsCombinedPair = false;
            bool IsSecondPair = false;
            foreach (string currentRegcustom in arrRegcustom)
            {
                if (!currentRegcustom.Contains("OUT"))
                {
                    //string ScriptPath = Path.GetDirectoryName(TCFpath).Replace("TCF", "Script");
                    string ScriptPath = ClothoRootDir + @"Script\";
                    reader = new StreamReader(Path.Combine(ScriptPath, DUTRevision, "TIMING", currentRegcustom + ".txt"), System.Text.Encoding.Default);

                    while (!reader.EndOfStream)
                    {
                        string Getstring = reader.ReadLine().Trim().ToUpper();

                        if (Getstring.ToUpper() == "#START")
                        {
                            while (!reader.EndOfStream)
                            {
                                Getstring = reader.ReadLine().Trim().ToUpper();

                                if (Getstring.ToUpper() == "#START")
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        Getstring = reader.ReadLine().Trim().ToUpper();
                                        if (Getstring.ToUpper() != "")
                                        {
                                            if (Getstring.Contains("TX")) currSlaveAdd = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
                                            else if (Getstring.Contains("RX"))
                                            {
                                                currSlaveAdd = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
                                                IsSecondPair = true;
                                            }

                                            if (previouSlaveAdd != currSlaveAdd)
                                            {
                                                previouSlaveAdd = currSlaveAdd;
                                                strFullmipiCommands += currSlaveAdd;
                                                
                                                if (IsCombinedPair)
                                                {
                                                    MessageBox.Show("Can not combine Mipi Pair at sametime", "Timing Mipi Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                                IsCombinedPair = true;
                                            }

                                            if (Getstring.ToUpper() != "#END")
                                            {
                                                if (Getstring.Contains("DELAY"))
                                                {
                                                    if (FoundFirstDelay)
                                                    {
                                                        AfterDelay = Convert.ToDouble(Getstring.Split(':')[1]);
                                                    }
                                                    else
                                                    {
                                                        BeforeDelay = Convert.ToDouble(Getstring.Split(':')[1]);
                                                        nBeforeCommand = nCommand;
                                                        FoundFirstDelay = true;
                                                    }
                                                }
                                                else
                                                {
                                                    Getstring = Getstring.Replace("RXREG", "");
                                                    Getstring = Getstring.Replace("TXREG", "");

                                                    string[] arrMipiHex = Getstring.Split(':');

                                                    strFullmipiCommands += ("(0x" + arrMipiHex[0] + ",0x" + arrMipiHex[1] + ")");
                                                    nCommand++;
                                                }
                                            }
                                            else if (Getstring.ToUpper() == "#END")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }

            if(IsSecondPair)
            {
                strFullmipiCommands = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"] + "(None)" + strFullmipiCommands;
            }

            return strFullmipiCommands;
        }

        public List<RfTestCondition.cTxleakageCondition> GetTxlkgMipiCommands(Dictionary<string, string> TestCon)
        {

            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommandsTemp = new List<MipiSyntaxParser.ClsMIPIFrame>();
            Dictionary<int, string> Txleakage_SpecNumber = new Dictionary<int, string>();
            Dictionary<int, double> Txleakage_Referencelevel = new Dictionary<int, double>();
            Dictionary<int, double> Txleakage_SpanforTxL = new Dictionary<int, double>();
            Dictionary<string, Dictionary<string, string>> dicMipiRegcustomTxlkg = GetRegcustomTxleakageMipiCommands(TestCon, ref Txleakage_SpecNumber, ref Txleakage_Referencelevel, ref Txleakage_SpanforTxL);
            List<RfTestCondition.cTxleakageCondition> TxleakageCondition = new List<RfTestCondition.cTxleakageCondition>();

            string VaribleData = "";

            if (dicMipiRegcustomTxlkg != null)
            {
                int Count = 0;
                foreach (string Currmipicommand in dicMipiRegcustomTxlkg.Keys)
                {
                    Count++;
                    RfTestCondition.cTxleakageCondition tempCondition = new RfTestCondition.cTxleakageCondition();

                    tempCondition.Mipi = MipiSyntaxParser.CreateListOfMipiFrames(Currmipicommand);
                    foreach (string currBandPort in dicMipiRegcustomTxlkg[Currmipicommand].Keys)
                    {
                        tempCondition.RxActiveBand = currBandPort;
                        tempCondition.Port = dicMipiRegcustomTxlkg[Currmipicommand][currBandPort];
                        switch (tempCondition.Port)
                        {
                            case "OUT1": tempCondition.ePort = Operation.VSAtoRX1; break;
                            case "OUT2": tempCondition.ePort = Operation.VSAtoRX2; break;
                            case "OUT3": tempCondition.ePort = Operation.VSAtoRX3; break;
                            case "OUT4": tempCondition.ePort = Operation.VSAtoRX4; break;
                            default: tempCondition.ePort = Operation.VSAtoRX; break;
                        }

                        tempCondition.SpecNumber = Txleakage_SpecNumber[Count];
                    }
                    tempCondition.ReferenceLevel = Txleakage_Referencelevel[Count];
                    tempCondition.SpanforTxL = Txleakage_SpanforTxL[Count];

                    TxleakageCondition.Add(tempCondition);
                }
            }

            // Replace Varibles in command syntax with Data values from the TCF
            foreach (RfTestCondition.cTxleakageCondition currMipiCommandsTemp in TxleakageCondition)
            {
                foreach (MipiSyntaxParser.ClsMIPIFrame command in currMipiCommandsTemp.Mipi)
                {
                    if (!command.IsValidFrame)  // Indicates there is a non valid Hex number or Varible name
                    {
                        VaribleData = GetStr(TestCon, command.Data_hex).Trim().ToUpper();  // search the header conditions for a match to the Varible name.
                        if (VaribleData == "")
                            MessageBox.Show("Warning: Varible name found in MIPI Command Syntax:" + command.Data_hex + " No column header with this condtion exists", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                        {
                            VaribleData.Replace("0X", "");
                            command.Data_hex = VaribleData;
                        }
                    }
                }
            }

            //}

            //else
            //    MessageBox.Show("Warning: Requested MipiCommand for test" + MipiCommand_tcf + "not found on MIPI tab of TCF", "", MessageBoxButtons.OK, MessageBoxIcon.Error);


            return TxleakageCondition;
        }

        public Dictionary<string, Dictionary<string, string>> GetRegcustomTxleakageMipiCommands(Dictionary<string, string> TestCon, ref Dictionary<int, string> Txleakage_SpecNumber, ref Dictionary<int, double> Txleakage_Referencelevel, ref Dictionary<int, double> Txleakage_SpanforTxL)
        {
            string strRegcustom = GetStr(TestCon, "REGCUSTOM").Trim().ToUpper();

            if (strRegcustom == "") return null;

            string TCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");
            string DUTRevision = TCF_Setting["Sample_Version"];
            StreamReader reader;

            //foreach (string tempRevison in Path.GetFileName(TCFpath).Split('_'))
            //{
            //    if (System.Text.RegularExpressions.Regex.IsMatch(tempRevison, @"^[AB]{1}\d{1}[A-Z]{1}"))
            //    {
            //        DUTRevision = tempRevison;
            //        break;
            //    }
            //}

            string[] arrRegcustom = strRegcustom.Split('.');


            Dictionary<string, Dictionary<string, string>> dicTxlkgMipiCommand = new Dictionary<string, Dictionary<string, string>>();


            foreach (string currentRegcustom in arrRegcustom)
            {
                if (strRegcustom.Contains("TXLEAKAGE"))
                {
                    //string ScriptPath = Path.GetDirectoryName(TCFpath).Replace("TCF", "Script");
                    string ScriptPath = ClothoRootDir + @"Script\";
                    reader = new StreamReader(Path.Combine(ScriptPath, DUTRevision, "TXLEAKAGE",
                        GetStr(TestCon, "BAND").Trim().ToUpper() + "_" + currentRegcustom + ".txt"), System.Text.Encoding.Default);

                    while (!reader.EndOfStream)
                    {
                        string Getstring = reader.ReadLine().Trim().ToUpper();

                        if (Getstring.ToUpper() == "#START")
                        {
                            int Count = 0;
                            int ReflevelCount = 0;
                            int SpanCount = 0;

                            while (!reader.EndOfStream)
                            {
                                Getstring = reader.ReadLine().Trim().ToUpper();

                                if (Getstring.ToUpper() == "#START")
                                {
                                    string strFullmipiCommands = "";
                                    string currSlaveAdd = "", previouSlaveAdd = "";
                                    string strBand = null, strPath = null;
                                    Dictionary<string, string> dicPortBand = new Dictionary<string, string>();

                                    string[] Split = new string[1];


                                    while (!reader.EndOfStream)
                                    {
                                        Getstring = reader.ReadLine().Trim().ToUpper();


                                        if (Getstring.ToUpper() != "")
                                        {
                                            if (Getstring.Contains("SPEC"))
                                            {
                                                Count++;
                                                Split = Getstring.Trim().Split(':');
                                                Txleakage_SpecNumber.Add(Count, Split[1].Replace('_', '-'));
                                                continue;
                                            }
                                            else if (Getstring.Contains("REFLEVEL"))
                                            {
                                                ReflevelCount++;
                                                Split = Getstring.Trim().Split(':');
                                                Txleakage_Referencelevel.Add(ReflevelCount, Convert.ToDouble(Split[1].Replace('_', '-')));

                                                continue;
                                            }

                                            //mario
                                            else if (Getstring.Contains("SPANFORTXL"))
                                            {
                                                SpanCount++;
                                                Split = Getstring.Trim().Split(':');
                                                Txleakage_SpanforTxL.Add(SpanCount, Convert.ToDouble(Split[1].Replace('_', '-')));

                                                continue;
                                            }

                                            if (Getstring.Contains("TXREG")) currSlaveAdd = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
                                            else if (Getstring.Contains("RXREG")) currSlaveAdd = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];


                                            if (previouSlaveAdd != Getstring.Substring(0, 2) + currSlaveAdd)
                                            {
                                                previouSlaveAdd = Getstring.Substring(0, 2) + currSlaveAdd;
                                                strFullmipiCommands += currSlaveAdd;
                                            }

                                            //if (previouSlaveAdd != currSlaveAdd)
                                            //{
                                            //    previouSlaveAdd = currSlaveAdd;
                                            //    strFullmipiCommands += currSlaveAdd;
                                            //}

                                            if (Getstring.ToUpper() != "#END")
                                            {
                                                if (Getstring.ToUpper().Contains("BAND"))
                                                {
                                                    strBand = Getstring.ToUpper().Split(':')[1];
                                                }
                                                else if (Getstring.ToUpper().Contains("PATH"))
                                                {
                                                    strPath = Getstring.ToUpper().Split(':')[1];
                                                }
                                                else
                                                {
                                                    Getstring = Getstring.Replace("RXREG", "");
                                                    Getstring = Getstring.Replace("TXREG", "");

                                                    string[] arrMipiHex = Getstring.Split(':');

                                                    strFullmipiCommands += ("(0x" + arrMipiHex[0] + ",0x" + arrMipiHex[1] + ")");
                                                }
                                            }
                                            else if (Getstring.ToUpper() == "#END")
                                            {
                                                strFullmipiCommands += "-" + Count; //Yoonchun // To prevent duplication of the same key 
                                                dicPortBand.Add(strBand, strPath);
                                                dicTxlkgMipiCommand.Add(strFullmipiCommands, dicPortBand);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }

            return dicTxlkgMipiCommand;
        }



    }

}