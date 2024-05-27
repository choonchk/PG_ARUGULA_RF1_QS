using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EqLib;
using MPAD_TestTimer;
using TestLib;
using TestPlanCommon.ToBeObsoleted;
using ProductionLib2;
using Avago.ATF.Shares;
using GuCal;

namespace TestPlanCommon.CommonModel
{
    /// <summary>
    /// Control test run and result.
    /// </summary>
    public class MultiSiteTestRunner
    {
        public iTest[][] AllPaTests;
        public iTest[][] AllNaTests;        // Unused.
        public Stopwatch[] stopwatchPA, stopwatchNA;
        public MQTT_MachineData[] MachineData;
        public int[] tempUnitNum;
        public SMQTTsetting setting;
        public bool MQTTENABLE;
        string MQTT_ADDRESS;

        private string m_testerType;
        private string m_isSaveSParaFile;

        public void Initialize(iTest[][] paTestListFromTcf)
        {
            AllPaTests = paTestListFromTcf;
        }

        public void Initialize(string testerType, string saveSParaFile)
        {
            m_testerType = testerType;
            m_isSaveSParaFile = saveSParaFile;
        }

        public void InitializeSiteStopwatches()
        {
            stopwatchPA = new Stopwatch[Eq.NumSites];
            stopwatchNA = new Stopwatch[Eq.NumSites];

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                stopwatchPA[site] = new Stopwatch();
                stopwatchNA[site] = new Stopwatch();
            }
        }

        public void RunTestsAllSites_ParallelFbarAndPa()
        {
            List<Task> allFbarSiteTasks = StartFbarTestsAllSitesParallel();
            List<Task> allPaSiteTasks = StartPaTestsAllSitesParallel();

            Task.WaitAll(allFbarSiteTasks.Union(allPaSiteTasks).ToArray());
        }
        public void RunTestsAllSites_ParallelFbarAndPa_parallel()
        {
            //List<Task> allFbarSiteTasks = StartFbarTestsAllSitesParallel_Parallel();
            List<Task> allPaSiteTasks = StartPaTestsAllSitesParallel_Parallel();

            Task.WaitAll(allPaSiteTasks.ToArray());
        }

        public ValidationDataObject InitializeAllPaTests(string strMQTTENABLE = "FALSE", bool isMQTTPlugin = false)
        {
            ValidationDataObject vdo = new ValidationDataObject();
            bool isLoadSuccess = true;

            MQTTENABLE = Convert.ToBoolean(strMQTTENABLE) && isMQTTPlugin;
            MQTT_ADDRESS = string.Format("192.168.0.{0}", ATFRTE.Instance.HandlerAddress);

            setting = new SMQTTsetting
            {
                MQTTflag = MQTTENABLE,
                LotInfoMQTTflag = MQTTENABLE,
                UnitInfoMQTTflagMQ = MQTTENABLE,
                TestInfoMQTTflag = MQTTENABLE,
                Source_Server = MQTT_ADDRESS,
                Source_Username = "",
                Source_Password = "",
                Topic_Source = "RTM/",
                Topic_LocalHost = "Hontech/HT7145/"
            };

            tempUnitNum = new int[Eq.NumSites];
            MachineData = new MQTT_MachineData[Eq.NumSites];

            try
            {
                bool finalScript = false;
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    //MQTT Machine Data
                    if (MQTTENABLE)
                    {
                        setting.site = site;
                        MachineData[site] = new MQTT_MachineData();
                        MachineData[site].MQTTInit(setting);
                        tempUnitNum[site] = 1;
                    }

                    foreach (iTest test in AllPaTests[site])
                    {
                        isLoadSuccess = isLoadSuccess && test.Initialize(finalScript);
                    }
                }
            }
            catch (Exception ex)
            {
                vdo = new ValidationDataObject("Error", "", ex);
            }

            if (!isLoadSuccess) vdo.IsValidated = false;
            return vdo;
        }
        public void RunThroughTests()
        {
            RfMeasListReset();
            RfTestBase.InitTest = true;

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                StopWatchManager.Instance.Start(site);
                Eq.Site[site].HSDIO.I2CTEMPSENSERead();
                //if ((site % 2) == 0) Eq.CurrentSplitTestPhase = SplitTestPhase.PhaseB;
                //else Eq.CurrentSplitTestPhase = SplitTestPhase.PhaseA;
                Eq.CurrentSplitTestPhase = SplitTestPhase.NoSplitTest;
                foreach (iTest test in AllPaTests[site])
                {
                    test.RunTest();
                }
                StopWatchManager.Instance.Stop(site);

                //Web Service 2.0
                OtpTest.Webservice2DIDflag[site] = true;
            }
        }


        public void RunTestsAllSites_ParallelFbarThenParallelPa()
        {
            List<Task> allFbarSiteTasks = StartFbarTestsAllSitesParallel();
            Task.WaitAll(allFbarSiteTasks.ToArray());

            List<Task> allPaSiteTasks = StartPaTestsAllSitesParallel();
            Task.WaitAll(allPaSiteTasks.ToArray());
        }
        public void RunTestsAllSites_ParallelFbarThenSerialPa()
        {
            List<Task> allFbarSiteTasks = StartFbarTestsAllSitesParallel();
            Task.WaitAll(allFbarSiteTasks.ToArray());

            RunPaTestsAllSitesSerial();
        }

        public void RunTestsAllSites_SerialPAthenFBAR_NoSplit()
        {
            List<Task> allPaSiteTasks = StartPaTestsAllSitesParallel_Parallel();
            Task.WaitAll(allPaSiteTasks.ToArray());

            List<Task> allFbarSiteTasks = StartFbarTestsAllSitesParallel_Parallel();
            Task.WaitAll(allFbarSiteTasks.ToArray());
        }


        public List<Task> StartFbarTestsAllSitesParallel()
        {
            List<Task> allFbarSiteTasks = new List<Task>();

            foreach (byte site in ResultBuilder.ValidSites)
            {
                //if (ResultBuilder.SitesAndPhases[site] == 1)
                {
                    allFbarSiteTasks.Add(Task.Factory.StartNew(() => RunFBARTests(site), TaskCreationOptions.LongRunning));
                }
            }

            return allFbarSiteTasks;
        }
        public List<Task> StartFbarTestsAllSitesParallel_Parallel()
        {
            List<Task> allFbarSiteTasks = new List<Task>();

            foreach (byte site in ResultBuilder.ValidSites)
            {
                //if (ResultBuilder.SitesAndPhases[site] == 1)
                {
                    allFbarSiteTasks.Add(Task.Factory.StartNew(() => RunFBARTests(site), TaskCreationOptions.LongRunning));
                }
            }

            return allFbarSiteTasks;
        }

        public List<Task> StartPaTestsAllSitesParallel()
        {
            List<Task> allPaSiteTasks = new List<Task>();

            foreach (byte site in ResultBuilder.ValidSites)
            {
                //if (ResultBuilder.SitesAndPhases[site] == 2)
                {
                    allPaSiteTasks.Add(Task.Factory.StartNew(() => RunPaTests2(site), TaskCreationOptions.LongRunning));
                }
            }

            return allPaSiteTasks;
        }
        public List<Task> StartPaTestsAllSitesParallel_Parallel()
        {
            List<Task> allPaSiteTasks = new List<Task>();

            for (byte Site = 0; Site < Eq.NumSites; Site++)
            {
                byte site = Site;
                //if (ResultBuilder.SitesAndPhases[site] == 2)
                {
                    allPaSiteTasks.Add(Task.Factory.StartNew(() => RunPaTests2(site), TaskCreationOptions.LongRunning));
                }
            }

            return allPaSiteTasks;
        }

        /// <summary>
        /// DoATfTest() calls this.
        /// </summary>
        public void RunPaTestsAllSitesSerial()
        {
            foreach (byte site in ResultBuilder.ValidSites)
            {
                //if (ResultBuilder.SitesAndPhases[site] == 2)
                {
                    RunPaTests2(site);
                }
            }
        }

        /// <summary>
        /// EDAM variant for serial test.
        /// </summary>
        public void RunPaTestsAllSitesSerial2()
        {
            RfMeasListReset();
            RfTestBase.InitTest = false;

            foreach (byte site in ResultBuilder.ValidSites)
            {
                //if (ResultBuilder.SitesAndPhases[site] == 2)
                {
                    RunPaTests2(site);
                }
            }
        }

        /// <summary>
        /// EDAM/NATHAN/NUWA variant for parallel test.
        /// </summary>
        public void RunPaTestsAllSitesParallel2()
        {
            RfMeasListReset();
            RfTestBase.InitTest = false;

            //Required for traces output during debug mode
            PinSweepTest.PreTest();
            TimingTestBase.PreTest();
            RfOnOffTest.PreTest();

            List<Task> allPaSiteTasks = new List<Task>();

            foreach (byte site in ResultBuilder.ValidSites)
            {
                if (MQTTENABLE && (tempUnitNum[site] == 1))
                {
                    MachineData[site].MQTTExexLotInfo(true, false);
                    
                    string mqttproject = Eq.Site[site].HSDIO.Digital_Definitions["PROJECT"];
                    MachineData[site].MQTTSetLotprofile(mqttproject, false);
                }

                allPaSiteTasks.Add(Task.Factory.StartNew(() => RunPaTests2(site), TaskCreationOptions.LongRunning));
            }

            Task.WaitAll(allPaSiteTasks.ToArray());
        }

        public int Selfcal_Flag = 0;

        public double[] Vcc_temperature = new double[Eq.NumSites];
        public double[] Vbatt_temperature = new double[Eq.NumSites];
        public double[] Vdd_temperature = new double[Eq.NumSites];

        public double[] SA_temperature = new double[Eq.NumSites];
        public double[] SG_temperature = new double[Eq.NumSites];
        public double[] HMU_temperature = new double[Eq.NumSites];

        public double[] load_board_temperature = new double[Eq.NumSites];

        /// <summary>
        /// Pinot variant.
        /// </summary>
        public void RunPaTests2(byte site)
        {
            stopwatchPA[site].Restart();

            // For Quadsite, single Switch DIO for dual sites 	
            byte SiteTemp = site;
            if (site.Equals(0) == false)
            {
                SiteTemp = 0;
            }
            // Skip Output Port on Contact Failure (11-Nov-2018)
            Eq.Site[SiteTemp].SwMatrix.ClearFailedDevicePort();
            ResultBuilder.ClearFailedQctest(site);

            Selfcal_Flag = 0;

            SA_temperature[site] = Eq.Site[site].RF.SA.ReadTemp();
            SG_temperature[site] = Eq.Site[site].RF.SG.ReadTemp();
            HMU_temperature[site] = Eq.Site[site].RF.RFExtd.HMU_MeasureTemperature(out HMU_temperature[site]);

            Vcc_temperature[site] = Eq.Site[site].DC["Vcc"].ReadTemp(Vcc_temperature[site]);
            Vbatt_temperature[site] = Eq.Site[site].DC["Vbatt"].ReadTemp(Vbatt_temperature[site]);
            Vdd_temperature[site] = Eq.Site[site].DC["Vdd"].ReadTemp(Vdd_temperature[site]);

            if (TemperatureAlignFlag(SA_temperature[site], site) == true)
            {
                Selfcal_Flag = 1;
                Eq.Site[site].RF.SA.SelfCalibration(1400e6, 6000e6, -60, 20);
                Eq.Site[site].RF.SG.SelfCalibration(1400e6, 6000e6, -40, 0);

                Eq.Site[site].DC["Vcc"].DeviceSelfCal();
                Eq.Site[site].DC["Vbatt"].DeviceSelfCal();
                Eq.Site[site].DC["Vdd"].DeviceSelfCal();
            }

            load_board_temperature[site] = Eq.Site[site].HSDIO.I2CTEMPSENSERead();

            Eq.Site[site].RF.SG.SetLOshare(false);
            Eq.Site[site].RF.SA.SetLOshare(false);

            foreach (iTest test in AllPaTests[site])
            {
                if ((!isOTPBurn_withVerifyTest(test)) && (!isOTPRead_withVerifyTest(test)))   // if this is an OTP burn or read test that requires Passing data verification, skip until end
                    test.RunTest();
                else
                    Thread.Sleep(2);
            }
        }

        private static void RfMeasListReset()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                Eq.Site[site].RF.CRfmxAcp.Iteration = 0;
                Eq.Site[site].RF.CRfmxAcpNR.Iteration = 0;
                Eq.Site[site].RF.CRfmxH2.Iteration = 0;
                Eq.Site[site].RF.CRfmxH3.Iteration = 0;
                Eq.Site[site].RF.CRfmxTxleakage.Iteration = 0;
                Eq.Site[site].RF.CRfmxIQ.Iteration = 0;
                Eq.Site[site].RF.CRfmxCHP.Iteration = 0;
                Eq.Site[site].RF.CRfmxIQ_EVM.Iteration = 0;
                Eq.Site[site].RF.CRfmxIQ_Timing.Iteration = 0;
                Eq.Site[site].RF.CRfmxEVM_LTE.Iteration = 0;
                Eq.Site[site].RF.CRfmxEVM_NR.Iteration = 0;
                Eq.Site[site].RF.CRfmxIIP3.Iteration = 0;
            }
        }

        public static bool TemperatureAlignFlag(double EquipmentTemperature, byte site) //return true to force alignment
        {
            FileInfo TempFile;
            StreamWriter swTempFile;
            StreamReader reader;
            double ReadTemp = 0, previousTemperature = 0, TemperatureDelta = 0;
            double ForceAlignmentDelta = 3; //allow-able temperature delta (C) before forcing full alignment
            bool TemperatureFailExist = false;
            string TempLogLocation = string.Format("C:\\Avago.ATF.Common\\Input\\TemperatureLog_{0}.txt", site);
            
            TemperatureFailExist = (File.Exists(TempLogLocation) ? true : false);

            if (!TemperatureFailExist)
            {
                //Create temperature file
                TempFile = new FileInfo(TempLogLocation);
                swTempFile = TempFile.CreateText();
                swTempFile.Close();
                swTempFile = TempFile.AppendText();
                swTempFile.WriteLine(EquipmentTemperature);
                swTempFile.Close();
                return true; //force cal
            }
            else
            {
                //Read the temperature
                reader = File.OpenText(TempLogLocation);
                ReadTemp = Convert.ToDouble(reader.ReadLine());
                reader.Close();
                previousTemperature = ReadTemp;
            }
            //Force alignment if temperature delta > setting
            TemperatureDelta = Math.Abs(previousTemperature - EquipmentTemperature);

            if (TemperatureDelta > ForceAlignmentDelta)
            {
                previousTemperature = EquipmentTemperature;
                File.Delete(TempLogLocation);
                TempFile = new FileInfo(TempLogLocation);
                swTempFile = TempFile.CreateText();
                swTempFile.Close();
                swTempFile = TempFile.AppendText();
                swTempFile.WriteLine(previousTemperature);
                swTempFile.Close();
                return true; //Force cal
            }
                    
      
            return false; //No cal needed
        }


        public void RunFBARTests(byte site)
        {
            stopwatchNA[site].Restart();
            TestTimeFile.Open("FBAR_Tests");


            Eq.Site[site].HSDIO.SendVector(EqHSDIO.Reset);

            Legacy_FbarTest.DataFiles.PreTest();

            //if (site % 2 == 0) Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_1);
            //else Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_2);   // make this generic

            //Topaz_FbarTest.DataFiles.PreTest();
            //Eq.Site[site].HSDIO.SendVector(EqHSDIO.Reset);
            foreach (iTest NATestParam in AllNaTests[site])
            {
                TestTimeFile.PreFbarTest();
                //Topaz_FbarTest.Port_Impedance = "50";                     
                NATestParam.RunTest();
                //myWatch_Fbar.Stop();
                TestTimeFile.PostNATest(NATestParam);
            }
            if (m_isSaveSParaFile == "TRUE")
                Legacy_FbarTest.DataFiles.WriteAll();

            PowerDownDcAndDigitalPins(site);
            TestTimeFile.Close();
            stopwatchNA[site].Stop();
        }

        public bool isOTPBurn_withVerifyTest(iTest test)
        {
            System.Type TestType = test.GetType();

            if (TestType.FullName == "TestLib.OtpBurnTest")
            {
                OtpBurnTest BurnTest = (OtpBurnTest)test;
                if (BurnTest.TestCon.RequiresDataCheckFirst == true)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public bool isOTPRead_withVerifyTest(iTest test)
        {
            System.Type TestType = test.GetType();

            if (TestType.FullName == "TestLib.OtpReadTest")
            {
                OtpReadTest BurnTest = (OtpReadTest)test;
                if (BurnTest.TestCon.RequiresDataCheckFirst == true)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public void PowerDownComplete(string testerType)
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                StopWatchManager.Instance.Start("TP-DoATFTest-PowerDownComplete",site);
                PowerDownComplete(site, testerType);
                StopWatchManager.Instance.Stop("TP-DoATFTest-PowerDownComplete",site);
                //PowerDownDcAndDigitalPins(site);
            }
        }

        /// <summary>
        /// Pinot variant.
        /// </summary>
        /// <param name="testerType"></param>
        public void PowerDownComplete2(string testerType)
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                PowerDownComplete(site, testerType);
            }
        }

        public void PowerDownComplete(int site, string testerType)
        {
            if ((testerType == "PA") || (testerType == "BOTH"))
            {
                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SA.Abort((byte)site);
            }
            PowerDownDcAndDigitalPins(site);
        }

        public void PowerDownComplete(bool isPaSite)
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                if (isPaSite)
                {
                    Eq.Site[site].RF.SG.Abort();
                    Eq.Site[site].RF.SA.Abort(site);
                }
                PowerDownDcAndDigitalPins(site);
            }
        }

        public void PowerDownDcAndDigitalPins(int site)
        {
            foreach (string pinName in Eq.Site[site].DC.Keys)
            {
                if (Eq.Site[site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue;
                Eq.Site[site].DC[pinName].ForceVoltage(0, Eq.Site[site].DC[pinName].priorCurrentLim);
            }

            //Eq.Site[site].HSDIO.SendVector(EqHSDIO.HiZ);
            Eq.Site[site].HSDIO.SendVector("VIOOFF"); //HiZ doesn't affect to turning off on DUT, should be set "VIOOFF"
        }

        public void PowerDownDcAndDigitalPins2()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                PowerDownDcAndDigitalPins(site);
            }
        }

        public void BuildAllResults_PaTest()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //ResultBuilder.AddResult(site, "FbarTestTime", "ms", stopwatchNA[site].ElapsedMilliseconds);
                foreach (iTest test in AllPaTests[site])
                {
                    test.BuildResults(ref ResultBuilder.results);
                }
                ResultBuilder.AddResult(site, "PaTestTime", "ms", stopwatchPA[site].ElapsedMilliseconds);

            }
        }

        /// <summary>
        /// Pinot variant.
        /// </summary>
        public void BuildAllResults_PaTest2()
        {
            foreach (byte site in ResultBuilder.ValidSites)
            {
                ResultBuilder.AddResult(site, "M_Temp_Loadboard", "degC", load_board_temperature[site]);
                ResultBuilder.AddResult(site, "M_Temp_SelfCalFlag", "degC", Selfcal_Flag);

                ResultBuilder.AddResult(site, "M_Temp_SA", "degC", SA_temperature[site]);
                ResultBuilder.AddResult(site, "M_Temp_SG", "degC", SG_temperature[site]);
                ResultBuilder.AddResult(site, "M_Temp_HMU", "degC", HMU_temperature[site]);

                ResultBuilder.AddResult(site, "M_Temp_Vcc", "degC", Vcc_temperature[site]);
                ResultBuilder.AddResult(site, "M_Temp_Vbatt", "degC", Vbatt_temperature[site]);
                ResultBuilder.AddResult(site, "M_Temp_Vdd", "degC", Vdd_temperature[site]);

                foreach (iTest test in AllPaTests[site])
                {   
                    try
                    {
                        test.BuildResults(ref ResultBuilder.results);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
                ResultBuilder.AddResult(site, "PaTestTime", "ms", stopwatchPA[site].ElapsedMilliseconds);

                if (MQTTENABLE && !GU.runningGU[site])
                {
                    bool BlnOnConnect = MachineData[site].client_source.MQTT_Connected();
                    if (!BlnOnConnect)
                    {
                        MachineData[site].client_source.Connect("BRCMLocalhost_" + MachineData[site].strTesterid + "_" + site, setting.Source_Server, setting.Source_Username,
                            setting.Source_Password, 0);
                        MachineData[site].client_source.client.MqttMsgPublishReceived += MachineData[site].Client_MqttMsgPublishReceived;
                    }
                    MachineData[site].MQTTExecUnitInfo(site, tempUnitNum[site], load_board_temperature[site]);
                    MachineData[site].MQTTExecTestInfo(site, tempUnitNum[site]);

                    tempUnitNum[site]++;
                }

            }
        }

        public void AddResultPATestTime(double fbarTestTime)
        {
            ResultBuilder.AddResult(0, "PaTestTime", "ms", (stopwatchPA[0].ElapsedMilliseconds - fbarTestTime));
        }

        #region Unused- Build Result

        public void BuildAllResults()
        {
            foreach (byte site in ResultBuilder.ValidSites)
            {
                switch (ResultBuilder.SitesAndPhases[site])
                {
                    case 1:

                        foreach (iTest NATestParam in AllNaTests[site])
                        {
                            NATestParam.BuildResults(ref ResultBuilder.results);
                        }

                        ResultBuilder.AddResult(site, "FbarTestTime", "ms", stopwatchNA[site].ElapsedMilliseconds);

                        break;

                    case 2:

                        foreach (iTest test in AllPaTests[site])
                        {
                            test.BuildResults(ref ResultBuilder.results);
                        }

                        ResultBuilder.AddResult(site, "PaTestTime", "ms", stopwatchPA[site].ElapsedMilliseconds);

                        break;
                }
            }
        }
        public void BuildAllResults_ParallelTest()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {

                //ResultBuilder.AddResult(site, "FbarTestTime", "ms", stopwatchNA[site].ElapsedMilliseconds);
                foreach (iTest test in AllPaTests[site])
                {
                    test.BuildResults(ref ResultBuilder.results);
                }

                foreach (iTest NATestParam in AllNaTests[site])
                {
                    NATestParam.BuildResults(ref ResultBuilder.results);
                }

                ResultBuilder.AddResult(site, "PaTestTime", "ms", stopwatchPA[site].ElapsedMilliseconds);
            }
        }
        public void BuildAllResults_SerialTest()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //ResultBuilder.AddResult(site, "FbarTestTime", "ms", stopwatchNA[site].ElapsedMilliseconds);
                foreach (iTest test in AllPaTests[site])
                {
                    test.BuildResults(ref ResultBuilder.results);
                }


                foreach (iTest NATestParam in AllNaTests[site])
                {
                    NATestParam.BuildResults(ref ResultBuilder.results);
                }

                ResultBuilder.AddResult(site, "PaTestTime", "ms", stopwatchPA[site].ElapsedMilliseconds);

            }
        }

        #endregion

        public long[] PreviousModID = new long[4] { 0, 0, 0, 0 };
        public string[] strPreviousModID = new string[4] { "", "", "", ""};

        public void CheckData_BurnOTP_PassFlag(bool[] isGucalSuccess, bool EngineeringMode = false)
        {
            foreach (byte site in ResultBuilder.ValidSites)
            {

                //foreach (OtpBurnTest test in AllPaTests[site])
                foreach (iTest test in AllPaTests[site])
                {
                    if (isOTPBurn_withVerifyTest(test))
                    {
                        OtpBurnTest BurnTest = (OtpBurnTest)test;
                        Eq.Site[site].HSDIO.SendVector(EqHSDIO.Reset);

                        //if ((BurnTest.TestCon.RequiresDataCheckFirst == true) && (ResultBuilder.FailedTests[site].Count == 0))
                        if (BurnTest.TestCon.RequiresDataCheckFirst == true)
                        {
                            test.RunTest();
                            test.BuildResults(ref ResultBuilder.results);
                        }
                    }
                    else if (isOTPRead_withVerifyTest(test))
                    {
                        OtpReadTest ReadTest = (OtpReadTest)test;

                        if (ReadTest.TestCon.RequiresDataCheckFirst == true)
                        {
                            test.RunTest();
                            test.BuildResults(ref ResultBuilder.results);
                        }
                    }
                    else if (test.GetType().FullName == "TestLib.OtpReadTest")
                    {
                        OtpReadTest ReadOTPTest = (OtpReadTest)test;

                        // Double unit checking
                        if (ReadOTPTest.TestCon.TestType.ToUpper() == "OTP_READ_MOD_ID")
                        {
                            ReadOTPTest.RunTest();

                            if (PreviousModID[site] != ReadOTPTest.TestResult.Module_ID)
                            {
                                PreviousModID[site] = ReadOTPTest.TestResult.Module_ID;
                            }
                            else
                            {
                                if (EngineeringMode == false) ReadOTPTest.TestResult.Module_ID = -1;
                                ResultBuilder.DuplicatedModuleID[site] = true;
                            }

                            //This feature is to use duplicated module ID feature, to prompt operator to turn
                            //off handler site that failed GUCAL
                            if (!GuCal.GU.runningGU[site] && (isGucalSuccess[site] == false))
                            {
                                ReadOTPTest.TestResult.Module_ID = -1;
                                ResultBuilder.DuplicatedModuleID[site] = true;
                            }
                        }
                        else if (ReadOTPTest.TestCon.TestType.ToUpper() == "OTP_READ_MOD_2DID")
                        {
                            string _Unique_ID = "";

                            ReadOTPTest.RunTest();

                            foreach (KeyValuePair<string, string> _S in OtpTest.Unique_ID[site])
                            {
                                _Unique_ID += _S.Value;
                            }

                            if (strPreviousModID[site] != _Unique_ID)
                            {
                                strPreviousModID[site] = _Unique_ID;
                            }
                            else
                            {
                                if (EngineeringMode == false) ReadOTPTest.TestResult.Module_2DID = -1;
                                ResultBuilder.DuplicatedModuleID[site] = true;
                            }

                            //This feature is to use duplicated module ID feature, to prompt operator to turn
                            //off handler site that failed GUCAL
                            if (!GuCal.GU.runningGU[site] && (isGucalSuccess[site] == false))
                            {                              
                                ReadOTPTest.TestResult.Module_2DID = -1;
                                ResultBuilder.DuplicatedModuleID[site] = true;
                            }
                        }
                        test.BuildResults(ref ResultBuilder.results);
                    }
                }

            }
        }

    }
}