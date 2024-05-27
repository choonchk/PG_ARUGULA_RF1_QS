using System;
using System.Collections.Generic;
using System.Threading;
using Avago.ATF.StandardLibrary;
using EqLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;
using TestLib;
using ToBeObsoleted;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// FBAR, SDI, Icc Cal and OCR
    /// </summary>
    public class SParaProductionTestPlan : SParaProductionTestPlanBase
    {

        public SParaProductionTestPlan()
        {
            FBAR_Test = true;
            TestTimeLogController = new ProductionTestTimeController();
            m_modelTiger = new TigerTraceFileModel();
        }

        public Dictionary<string, int> SetLastUnitId(Dictionary<string, string> Digital_Definitions_Part_Specific)
        {
            Dictionary<string, int> mipiDict = new Dictionary<string, int>();

            int Read_Unit_Number = 999;

            int LotID = 0;
            int ReadBackData = 0; //yoonchun_check

            int Module_ID = 999999;
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.dutSlavePairIndex = 1;
            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            Read_Unit_Number = Module_ID = OTP_Procedure.OTP_Read_Mod_ID(0);

            int MFT_ID = 999999;
            MFT_ID = OTP_Procedure.OTP_Read_MFG_ID(0,
                Digital_Definitions_Part_Specific["MFG_ID_NUM_BITS"],
                Digital_Definitions_Part_Specific["TX_EFUSE_BYTE1"],
                Digital_Definitions_Part_Specific["TX_EFUSE_BYTE0"]);

            int REV_ID = 999999;

            REV_ID = OTP_Procedure.OTP_Read_Rev_ID(0);
            // Note MODULE_ID value will change below this.
            mipiDict.Add("OTP_MODULE_ID", Module_ID);
            mipiDict.Add("MFG_ID", MFT_ID);
            mipiDict.Add("REVID", REV_ID);

            ////ChoonChin - Temp disable for proto
            //if (Unit_ID == 1) //First unit testing just read added Chee On
            //{
            //    last_unit_id = Read_Unit_Number;
            //}

            //else
            //{
            //    if (last_unit_id != Read_Unit_Number)
            //    {
            //        last_unit_id = Read_Unit_Number;
            //    }
            //    else
            //    {
            //        Module_ID = Read_Unit_Number = -1;
            //    }
            //}

            //bool DEMOBOARD = true;

            //if (DEMOBOARD) //Added by CheeOn due to DEMO board testing for single MIPI bus 13-July-2018
            //    Digital_Definitions_Part_Specific["SLAVE_ADDR_0C"] = "1";

            //Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            //EqHSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            //int MIPIErrorBit = m_wrapper2.DoATFTest2("FUNCTIONAL_TX");
            //mipiDict.Add("MIPI_FUNCTIONAL_NumBitErrors", MIPIErrorBit);

            //Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            //EqHSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            //MIPIErrorBit = m_wrapper2.DoATFTest2("FUNCTIONAL_RX");
            //mipiDict.Add("MIPI_RXFUNCTIONAL_NumBitErrors", MIPIErrorBit);

            //int TXPID = 0;
            //int RXPID = 0;
            //int TXMID = 0;
            //int RXMID = 0;
            //int TXUSID = 0;
            //int RXUSID = 0;

            //EqHSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            //TXPID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["PID_REG"]), 16);
            //mipiDict.Add("PIDTX", TXPID);
            //TXMID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["MID_REG"]), 16);
            //mipiDict.Add("MIDTX", TXMID);
            //TXUSID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["USID_REG"]), 16);
            //mipiDict.Add("USIDTX", TXUSID);

            //EqHSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            //RXPID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["PID_REG"]), 16);
            //mipiDict.Add("PIDRX", RXPID);
            //RXMID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["MID_REG"]), 16);
            //mipiDict.Add("MIDRX", RXMID);
            //RXUSID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["USID_REG"]), 16);
            //mipiDict.Add("USIDRX", RXUSID);

            //// reset VIO and verify burn
            //m_wrapper2.DoATFTestReset();

            //int RXOTPVerify = 999999;
            //RXOTPVerify = HSDIO.Instrument.SendVector_VerifyLNA_OTP("RXOTPVERIFY");
            //ATFResultBuilder.AddResult(ref results, "MIPI_OTP_BURN_LNA_ERROR", "", RXOTPVerify);

            return mipiDict;
        }

        public Dictionary<string, int> SetLastUnitId2(InstrLibWrapper m_wrapper2,
    Dictionary<string, string> Digital_Definitions_Part_Specific)
        {
            Dictionary<string, int> mipiDict = new Dictionary<string, int>();

            int Read_Unit_Number = 999;

            int LotID = 0;
            int ReadBackData = 0; //yoonchun_check

            int Module_ID = 999999;
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            Read_Unit_Number = Module_ID = OTP_Procedure.OTP_Read_Mod_ID(0);

            int MFT_ID = 999999;
            MFT_ID = OTP_Procedure.OTP_Read_MFG_ID(0);

            int REV_ID = 999999;

            REV_ID = OTP_Procedure.OTP_Read_Rev_ID(0);
            // Note MODULE_ID value will change below this.
            mipiDict.Add("OTP_MODULE_ID", Module_ID);
            mipiDict.Add("MFG_ID", MFT_ID);
            mipiDict.Add("REVID", REV_ID);

            ////ChoonChin - Temp disable for proto
            if (Unit_ID == 1) //First unit testing just read added Chee On
            {
                last_unit_id = Read_Unit_Number;
            }
            else
            {
                if (last_unit_id != Read_Unit_Number)
                {
                    last_unit_id = Read_Unit_Number;
                }
                else
                {
                    Module_ID = Read_Unit_Number = -1;
                }
            }

            bool DEMOBOARD = false;

            if (DEMOBOARD) //Added by CheeOn due to DEMO board testing for single MIPI bus 13-July-2018
                Digital_Definitions_Part_Specific["SLAVE_ADDR_0C"] = "1";

            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            int MIPIErrorBit = m_wrapper2.DoATFTest2("FUNCTIONAL_TX");
            mipiDict.Add("MIPI_FUNCTIONAL_NumBitErrors", MIPIErrorBit);

            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            MIPIErrorBit = m_wrapper2.DoATFTest2("FUNCTIONAL_RX");
            mipiDict.Add("MIPI_RXFUNCTIONAL_NumBitErrors", MIPIErrorBit);

            int TXPID = 0;
            int RXPID = 0;
            int TXMID = 0;
            int RXMID = 0;
            int TXUSID = 0;
            int RXUSID = 0;

            int TXWaferLotID = 0;
            int TXWaferID = 0;
            int TXPosX = 0;
            int TXPosY = 0;
            int LNAWaferLotID = 0;
            int LNAWaferID = 0;
            int LNAPosX = 0;
            int LNAPosY = 0;

            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            TXPID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["PID_REG"]), 16);
            mipiDict.Add("PIDTX", TXPID);
            TXMID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["MID_REG"]), 16);
            mipiDict.Add("MIDTX", TXMID);
            //TXUSID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["USID_REG"]), 16);
            TXUSID = OTP_Procedure.OTP_Read_USID(0);
            mipiDict.Add("USIDTX", TXUSID);

            TXPosX = OTP_Procedure.OTP_Read_TX_X(0);
            mipiDict.Add("MIPI_CMOS-TX-X", TXPosX);
            TXPosY = OTP_Procedure.OTP_Read_TX_Y(0);
            mipiDict.Add("MIPI_CMOS-TX-Y", TXPosY);
            TXWaferLotID = OTP_Procedure.OTP_Read_WAFER_LOT(0);
            mipiDict.Add("MIPI_CMOS-TX-WAFER-LOT", TXWaferLotID);
            TXWaferID = OTP_Procedure.OTP_Read_WAFER_ID(0);
            mipiDict.Add("MIPI_CMOS-TX-WAFER-ID", TXWaferID);

            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI2_SLAVE_ADDR"];
            RXPID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["PID_REG"]), 16);
            mipiDict.Add("PIDRX", RXPID);
            RXMID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["MID_REG"]), 16);
            mipiDict.Add("MIDRX", RXMID);
            //TODO OTP_Procedure.OTP_Read_USID(0);
            RXUSID = Convert.ToInt32(Eq.Site[0].HSDIO.RegRead(Digital_Definitions_Part_Specific["USID_REG"]), 16);
            RXUSID = OTP_Procedure.OTP_Read_USID(0);
            mipiDict.Add("USIDRX", RXUSID);

            LNAPosX = OTP_Procedure.OTP_Read_LNA_X(0);
            mipiDict.Add("MIPI_LNA-X", LNAPosX);
            LNAPosY = OTP_Procedure.OTP_Read_LNA_Y(0);
            mipiDict.Add("MIPI_LNA-Y", LNAPosY);
            LNAWaferLotID = OTP_Procedure.OTP_Read_LNA_WAFER_LOT(0);
            mipiDict.Add("MIPI_LNA-WAFER-LOT", LNAWaferLotID);
            LNAWaferID = OTP_Procedure.OTP_Read_LNA_WAFER_ID(0);
            mipiDict.Add("MIPI_LNA-WAFER-ID", LNAWaferID);

            // reset VIO and verify burn
            m_wrapper2.DoATFTestReset();

            //int RXOTPVerify = 999999;
            //RXOTPVerify = HSDIO.Instrument.SendVector_VerifyLNA_OTP("RXOTPVERIFY");
            //ATFResultBuilder.AddResult(ref results, "MIPI_OTP_BURN_LNA_ERROR", "", RXOTPVerify);

            return mipiDict;
        }

        public void DoPostTestJoker(ATFReturnResult results,
            int fbar_tester_id, int SPARA_PART_LOCKBIT_FLAG)
        {
            //ATFResultBuilder.AddResult(ref results, "SPARA_PART_LOCKBIT_FLAG", "", SPARA_PART_LOCKBIT_FLAG);
            // Joker disabled.
            // Case Joker.
            ResultBuilder.AddResult(0, "M_Flag_SPARA-Pass", "", SPARA_PART_LOCKBIT_FLAG);
            ATFResultBuilder.AddResult(ref results, "M_TestSiteNo", "", fbar_tester_id);
            ResultBuilder.AddResult(0, "M_TestSiteNo", "", fbar_tester_id);
            //ResultBuilder.AddResult(0, "M_TestSiteNo", "", fbar_tester_id);

            // Case HLS2.
            //ATFResultBuilder.AddResult(ref results, "SPARAPARTLOCKBITFLAG", "", SPARA_PART_LOCKBIT_FLAG);
        }

        public void DoPostTestS17(ATFReturnResult results, InstrLibWrapper m_wrapper2,
            int SPARA_PART_LOCKBIT_FLAG)
        {
            //ATFResultBuilder.AddResult(ref results, "SPARA_PART_LOCKBIT_FLAG", "", SPARA_PART_LOCKBIT_FLAG);
            // Joker disabled.
            ATFResultBuilder.AddResult(ref results, "SPARAPARTLOCKBITFLAG", "",
                SPARA_PART_LOCKBIT_FLAG);

            m_wrapper2.ProdBestPractice1();
            m_wrapper2.ProdBestPractice2();
        }

        public void DoPostTestHallasan2(ATFReturnResult results, InstrLibWrapper m_wrapper2, SParaTestManager LibFbar,
            Dictionary<string, string> Digital_Definitions_Part_Specific)
        {
            List<string> content = LibFbar.FileManager.GetContentCnTrace();
            SaveTraceCn(content);

            IncrementUnitId();

            #region MIPI test function & errorbit check

            Dictionary<string, int> mipiDict = SetLastUnitId2(m_wrapper2,
                Digital_Definitions_Part_Specific);
            foreach (KeyValuePair<string, int> kp in mipiDict)
            {
                ATFResultBuilder.AddResult(ref results, kp.Key, "", kp.Value);
            }

            #endregion MIPI test function & errorbit check

            List<string> trace = LibFbar.FileManager.GetContentTiger();
            SaveTraceTiger(trace, mipiDict["MFG_ID"]);
        }

        public void DoPostTestJoker2(ATFReturnResult results, SParaTestManager LibFbar, Dictionary<string, string> Digital_Definitions_Part_Specific)
        {
            #region MIPI test function & errorbit check

            Dictionary<string, int> mipiDict = SetLastUnitId(Digital_Definitions_Part_Specific);
            //foreach (KeyValuePair<string, int> kp in mipiDict)
            //{
            //    ATFResultBuilder.AddResult(ref results, kp.Key, "", kp.Value);
            //    ResultBuilder.AddResult(0, kp.Key, "x", kp.Value); //need to automation for site#

            //}

            #endregion MIPI test function & errorbit check

            List<string> content = LibFbar.FileManager.GetContentCnTrace();
            SaveTraceCn(content, mipiDict["MFG_ID"], mipiDict["OTP_MODULE_ID"]); //ChoonChin - 20191203 - Add module ID for tiger
            
            List<string> trace = LibFbar.FileManager.GetContentTiger();
            SaveTraceTiger(trace, mipiDict["MFG_ID"], mipiDict["OTP_MODULE_ID"]); //ChoonChin - 20191203 - Add module ID for tiger

            IncrementUnitId();
        }

        public void DoPostTest3(ATFReturnResult results, ProdLib1Wrapper m_wrapper4, SParaTestManager LibFbar,
            bool w1IsRunning)
        {
            try
            {
                Zip(m_wrapper4, w1IsRunning);
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError("Progressive Zip", ex);
            }

            #region FBAR write test time

            StopWatchManager.Instance.Stop("ProdFbarTest", 0);

            PaStopwatch2 sw1 = StopWatchManager.Instance.GetStopwatch("ProdFbarTest", 0);
            double FbartestTime = -1;
            if (sw1 != null)        // Null when StopWatch is deactivated.
            {
                FbartestTime = sw1.ElapsedMs;
            }
            ATFResultBuilder.AddResult(ref results, "M_TIME_FbarTest", "", FbartestTime);
            ResultBuilder.AddResult(0, "M_TIME_FbarTest", "ms", FbartestTime);

            List<s_Result> result = LibFbar.Results;
            List<double> resultTime = LibFbar.FbarTestTime;
            WriteTestTimeFile(result, resultTime);


            #endregion FBAR write test time
        }

        public void DoPostTestJoker3(ATFReturnResult results, InstrLibWrapper m_wrapper2,
            SParaTestManager LibFbar, int w3FailedTestCount,
            Dictionary<string, string> Digital_Definitions_Part_Specific,
            bool isLiteDriverMode, int fbar_tester_id)
        {
            int SPARA_PART_LOCKBIT_FLAG = BurnLockBitJoker(ref results, Digital_Definitions_Part_Specific, isLiteDriverMode);

            #region Read_Topaz temperature

            if (false)
            {
                //Topaz slot temperature
                double tempTopaz = LibFbar.Temp_Topaz();
                ATFResultBuilder.AddResult(ref results, "Temperature_Topaz", "", tempTopaz);
            }

            #endregion Read_Topaz temperature

            // For Joker
            DoPostTestJoker(results, fbar_tester_id, SPARA_PART_LOCKBIT_FLAG);
            Eq.Site[0].HSDIO.SendVector("viooff");
        }

        public void DoPostTest(ATFReturnResult results, InstrLibWrapper m_wrapper2,
            ProdLib1Wrapper m_wrapper4, SParaTestManager LibFbar,
            int w3FailedTestCount, bool w1IsRunning,
            Dictionary<string, string> Digital_Definitions_Part_Specific,
            bool isLiteDriverMode)
        {
            IncrementUnitId();

            #region MIPI test function & errorbit check

            Dictionary<string, int> mipiDict = SetLastUnitId(Digital_Definitions_Part_Specific);
            foreach (KeyValuePair<string, int> kp in mipiDict)
            {
                ATFResultBuilder.AddResult(ref results, kp.Key, "", kp.Value);
            }

            #endregion MIPI test function & errorbit check

            int SPARA_PART_LOCKBIT_FLAG = BurnLockBit(ref results,
                Digital_Definitions_Part_Specific, isLiteDriverMode);

            #region Read_Topaz temperature

            if (false)
            {
                //Topaz slot temperature
                double tempTopaz = LibFbar.Temp_Topaz();
                ATFResultBuilder.AddResult(ref results, "Temperature_Topaz", "", tempTopaz);
            }

            #endregion Read_Topaz temperature

            // For S1.7
            DoPostTestS17(results, m_wrapper2, SPARA_PART_LOCKBIT_FLAG);
            DoPostTest3(results, m_wrapper4, LibFbar, w1IsRunning);
        }

        private int x = 1;

        private int BurnLockBit(ref ATFReturnResult results,
            Dictionary<string, string> Digital_Definitions_Part_Specific,
            bool isLiteDriverMode)
        {
            int SPARA_PART_LOCKBIT_FLAG;
            StopWatchManager.Instance.Start("DoATFTest_OTP_and_Lockbit_Burn_Iter_" + x.ToString(),0);
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];

            #region OTP check and read

            //Read FBAR Noise pass flag
            bool OTP_READ_FLAG =
                OTP_Procedure.OTP_Read_FBAR_Noise_Pass_Flag(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_FBAR_Pass_Flag();
            int NOISE_PART_PASS_FLAG;
            if (OTP_READ_FLAG)
                NOISE_PART_PASS_FLAG = 1;
            else
                NOISE_PART_PASS_FLAG = 0;

            //Read PA pass flag
            OTP_READ_FLAG =
                OTP_Procedure.OTP_Read_RF1_Pass_Flag(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_RF1_Pass_Flag();
            int PA_PART_PASS_FLAG;
            if (OTP_READ_FLAG)
                PA_PART_PASS_FLAG = 1;
            else
                PA_PART_PASS_FLAG = 0;

            // For QA test plan check
            // Add result parameter intp result builder for spec checking. added by chee on 29092017
            //ResultBuilder.AddResult(0, "NOISEPARTPASSFLAG", "", NOISE_PART_PASS_FLAG, 1);
            //ResultBuilder.AddResult(0, "PAPARTPASSFLAG", "", PA_PART_PASS_FLAG, 1);
            //ATFResultBuilder.AddResult(ref results, "M_Flag_NOISE-Pass", "x", NOISE_PART_PASS_FLAG);
            //ATFResultBuilder.AddResult(ref results, "M_Flag_PA-Pass", "x", PA_PART_PASS_FLAG);
            // Case HLS2
            ATFResultBuilder.AddResult(ref results, "NOISEPARTPASSFLAG", "x", NOISE_PART_PASS_FLAG);
            ATFResultBuilder.AddResult(ref results, "PAPARTPASSFLAG", "x", PA_PART_PASS_FLAG);

            //Burn lock bit - new method
            // Case Joker.
            //string lockbitheaderlabel = "M_Flag_LockBit";
            // Case HLS2.
            string lockbitheaderlabel = "LOCKBITBURN";

            //Read SPARAM pass flag
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            OTP_READ_FLAG = OTP_Procedure.OTP_Read_Lock_Bit(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_Lock_Bit();
            if (OTP_READ_FLAG)
                SPARA_PART_LOCKBIT_FLAG = 1;
            else
                SPARA_PART_LOCKBIT_FLAG = 0;

            if ((SPARA_PART_LOCKBIT_FLAG == 1) && !(ProductTag.Contains("QA-RF2")) && !(ProductTag.Contains("REQ")))
            {
                SPARA_PART_LOCKBIT_FLAG = -1; //Previously burned unit or double unit
            }
            ////ChoonChin - Temp disable for proto
            //if ((SPARA_PART_LOCKBIT_FLAG == 1) &&
            //    !(ProductTag.Contains("QA-RF2")))
            //{
            //    SPARA_PART_LOCKBIT_FLAG = -1; //Previously burned unit or double unit
            //}

            // Add result parameter intp result builder for spec checking. added by chee on 29092017
            // S1.7 Code- commented by CCT
            //ATFResultBuilder.AddResult(ref results, "NOISEPARTPASSFLAG", "",
            //    NOISE_PART_PASS_FLAG);
            //ATFResultBuilder.AddResult(ref results, "PAPARTPASSFLAG", "", PA_PART_PASS_FLAG);

            //foreach (ATFReturnPararResult Verify in results.Data)
            //{
            //    TestLib_Legacy.ResultBuilder.Verify_Spec(Verify.Name, "", (double) Verify.Vals[0]);
            //}

            #endregion OTP check and read

            bool Q_NoFails = (ResultBuilder.FailedTests[0].Count == 0 ? true : false);

            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            bool arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);

            #region Lockbitburn debug snippet

            //In-code, Force the lock bit to be burned
            bool forcezlockbitburn = false; // true;

            if (forcezlockbitburn)
            {
                for (int i = 0; i < 1; i++)
                {
                    Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                    Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                    //PaTest.SmuResources["Vlna"].ForceVoltage(0, 0.1);
                    //PaTest.SmuResources["Vcc"].ForceVoltage(0, 0.1);
                    //Thread.Sleep(5);
                    Eq.Site[0].HSDIO.SendVector(Eq.Site[0].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));
                    //OTP_Procedure.OTP_Burn_Lock_Bit(0, 0.1);
                    //Thread.Sleep(5);
                    Eq.Site[0].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                    // reset VIO and verify burn
                    Eq.Site[0].HSDIO.SendVector("VIOOFF".ToLower());
                    Thread.Sleep(5);

                    Eq.Site[0].HSDIO.SendVector("VIOON".ToLower());
                    Thread.Sleep(5);

                    Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
                    arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);
                    ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);
                    SPARA_PART_LOCKBIT_FLAG = 1;
                }
            }

            #endregion Lockbitburn debug snippet

            bool skiplockbitburn = false;
            int Fbar_Pass_Dec = 4;
            bool condition1 = !isLiteDriverMode;
            condition1 = condition1 && Q_NoFails;
            condition1 = condition1 && (PA_PART_PASS_FLAG == 1);
            condition1 = condition1 && (NOISE_PART_PASS_FLAG == 1);
            condition1 = condition1 && (SPARA_PART_LOCKBIT_FLAG == 0);
            condition1 = condition1 && (!ProductTag.Contains("QA-RF2"));
            condition1 = condition1 && (!ProductTag.Contains("OTP"));
            condition1 = condition1 && (!ProductTag.Contains("REQ"));   //For production package on OTP lock bit

            if (condition1)
            {
                #region Joker Implementation

                //if (!skiplockbitburn)
                //{
                //    for (int i = 0; i < 1; i++) //repeat provision
                //    {
                //        Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                //        Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                //        Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                //        Eq.Site[0].HSDIO.Burn(Fbar_Pass_Dec.ToString(), false, 2);

                //        Eq.Site[0].HSDIO.RegWrite("1C", "40");
                //        Eq.Site[0].HSDIO.SendVector("VIOOFF");
                //        Thread.Sleep(1);
                //        Eq.Site[0].HSDIO.SendVector("VIOON");
                //        Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                //        Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.0001);
                //        Thread.Sleep(10);
                //        Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                //        int TempRead = Fbar_Pass_Dec & Convert.ToInt32(Eq.Site[0].HSDIO.RegRead("E2"), 16);

                //        if (TempRead == Fbar_Pass_Dec)
                //        {
                //            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                //            Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                //            Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                //            Eq.Site[0].HSDIO.Burn("1", false, 11);

                //            Eq.Site[0].HSDIO.RegWrite("1C", "40");
                //            Eq.Site[0].HSDIO.SendVector("VIOOFF");
                //            Thread.Sleep(1);
                //            Eq.Site[0].HSDIO.SendVector("VIOON");
                //            Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                //            Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.0001);
                //            Thread.Sleep(10);
                //            Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                //            arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);
                //        }

                //        if (arewedoinit)
                //        {
                //            //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);
                //            ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 1);
                //            SPARA_PART_LOCKBIT_FLAG = 1;
                //        }
                //        else
                //        {
                //            //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                //            ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 0);
                //            SPARA_PART_LOCKBIT_FLAG = 0;
                //        }
                //    }
                //}
                //else
                //{
                //    //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                //    ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 0);
                //    SPARA_PART_LOCKBIT_FLAG = 0;
                //}

                #endregion Joker Implementation

                if (!skiplockbitburn)
                {
                    for (int i = 0; i < 1; i++) //repeat provision
                    {
                        Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                        Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                        //Thread.Sleep(5);
                        Eq.Site[0].HSDIO.SendVector(Eq.Site[0].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));
                        //OTP_Procedure.OTP_Burn_Lock_Bit(0, 0.1);
                        //Thread.Sleep(5);
                        Eq.Site[0].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                        // reset VIO and verify burn
                        Eq.Site[0].HSDIO.SendVector("VIOOFF".ToLower());
                        Thread.Sleep(5);

                        Eq.Site[0].HSDIO.SendVector("VIOON".ToLower());
                        Thread.Sleep(5);

                        arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);
                        if (arewedoinit)
                        {
                            ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);
                            SPARA_PART_LOCKBIT_FLAG = 1;
                        }
                        else
                        {
                            ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                            SPARA_PART_LOCKBIT_FLAG = 0;
                        }
                    }
                }
                else
                {
                    ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                    SPARA_PART_LOCKBIT_FLAG = 0;
                }
            }
            else if (!isLiteDriverMode && (PA_PART_PASS_FLAG == 1) && (NOISE_PART_PASS_FLAG == 1) && (SPARA_PART_LOCKBIT_FLAG == 1)
                 && (ProductTag.Contains("QA-RF2")) && !(ProductTag.Contains("OTP")) || (ProductTag.Contains("REQ") && (SPARA_PART_LOCKBIT_FLAG == 1))) //For QA & REQ test package, DUT completed lock bit, no RF fail.
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);       //QA package & REQ, Lockbit DUT with PASS/FAIL RF testing
                ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 1);
            }
            else if (!isLiteDriverMode && !Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && !(ProductTag.Contains("QA-RF2")) && !(ProductTag.Contains("OTP")))
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 2, 1); //FAIL RF testing and no lock bit PROD package
                ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 2);
                SPARA_PART_LOCKBIT_FLAG = 2;
            }
            else if (Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && (ProductTag.Contains("QA-RF2"))
                || Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && (ProductTag.Contains("REQ")))
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);    // PASS RF spec but DUT not burn at QA/REQ package
                ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 0);
            }
            else
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", -1, 1);  //other unknown scenerios failure
                ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", -1);
            }

            StopWatchManager.Instance.Stop("DoATFTest_OTP_and_Lockbit_Burn_Iter_" + x.ToString(),0);

            return SPARA_PART_LOCKBIT_FLAG;

            #region Burn lock bit - old method

#if false
                                            if ((PaTest.FailedTests.Count == 0) && (PA_PART_PASS_FLAG == 1) && (NOISE_PART_PASS_FLAG == 1) && (SPARA_PART_LOCKBIT_FLAG == 0) && (!ProductTag.Contains("QA-RF2")) && (!ProductTag.Contains("OTP")))
                                            {
                                                for (int i
 = 0; i < 6; i++)
                                                {
                                                    //eFuse
                                                    HSDIO.Instrument.SendVector("VIOON");
                                                    PaTest.SmuResources["Vbatt"].ForceVoltage(5.5, 0.02);
                                                    PaTest.SmuResources["Vlna"].ForceVoltage(0, 0.1);
                                                    PaTest.SmuResources["Vcc"].ForceVoltage(0, 0.1);

                                                    HSDIO.Instrument.SendVector("VIOOFF");
                                                    HSDIO.Instrument.SendVector("VIOON");

                                                    SPARA_PART_LOCKBIT_FLAG
 = OTP_Procedure.OTP_Burn_Lock_Bit(0);  //TestLib_Legacy.OTP_Seorakson.OTP_Burn_Lock_Bit();

                                                    Thread.Sleep(2);
                                                    ////Read lock bit
                                                    //if (OTP_READ_FLAG)
                                                    //    SPARA_PART_LOCKBIT_FLAG = 1;
                                                    //else
                                                    //    SPARA_PART_LOCKBIT_FLAG = 0;

                                                    if (SPARA_PART_LOCKBIT_FLAG == 1)
                                                    {
                                                        if (i > 1) //log to to logger if more than 1 try
                                                        {
                                                            logger.Log(LogLevel.Info, "Lock bit was burned after " + i + "attempts");
                                                        }
                                                        break;
                                                    }

                                                    if (i == 5) //log to to logger if more than 1 try
                                                    {
                                                        logger.Log(LogLevel.Info, "Lock bit OTP failure even after " + i + "attempts");
                                                    }
                                                }
                                            }
                                            //ChoonChin - change lock bit to 2 if failed test > 0. Don't set for QA program
                                            else if ((PaTest.FailedTests.Count > 0) && (SPARA_PART_LOCKBIT_FLAG == 0) && (PA_PART_PASS_FLAG == 1) && (NOISE_PART_PASS_FLAG == 1))
                                            {
                                                SPARA_PART_LOCKBIT_FLAG
 = 2; //Failed RF test(s)
                                            }
#endif

            #endregion Burn lock bit - old method

        }

        private int BurnLockBitJoker(ref ATFReturnResult results, Dictionary<string, string> Digital_Definitions_Part_Specific,
    bool isLiteDriverMode)
        {
            int SPARA_PART_LOCKBIT_FLAG;
            StopWatchManager.Instance.Start("DoATFTest_OTP_and_Lockbit_Burn_Iter_" + x.ToString(),0);
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];

            #region OTP check and read

            //Read FBAR Noise pass flag
            bool OTP_READ_FLAG =
                OTP_Procedure.OTP_Read_FBAR_Noise_Pass_Flag(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_FBAR_Pass_Flag();
            int NOISE_PART_PASS_FLAG;
            if (OTP_READ_FLAG)
                NOISE_PART_PASS_FLAG = 1;
            else
                NOISE_PART_PASS_FLAG = 0;

            //Read PA pass flag
            OTP_READ_FLAG =
                OTP_Procedure.OTP_Read_RF1_Pass_Flag(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_RF1_Pass_Flag();
            int PA_PART_PASS_FLAG;
            if (OTP_READ_FLAG)
                PA_PART_PASS_FLAG = 1;
            else
                PA_PART_PASS_FLAG = 0;

            // For QA test plan check
            // Add result parameter intp result builder for spec checking. added by chee on 29092017
            //ResultBuilder.AddResult(0, "NOISEPARTPASSFLAG", "", NOISE_PART_PASS_FLAG, 1);
            //ResultBuilder.AddResult(0, "PAPARTPASSFLAG", "", PA_PART_PASS_FLAG, 1);
            ResultBuilder.AddResult(0, "M_Flag_NOISE-Pass", "x", NOISE_PART_PASS_FLAG);
            ResultBuilder.AddResult(0, "M_Flag_PA-Pass", "x", PA_PART_PASS_FLAG);

            //Burn lock bit - new method
            string lockbitheaderlabel = "M_Flag_LockBit";

            //Read SPARAM pass flag
            Eq.Site[0].HSDIO.dutSlaveAddress = Digital_Definitions_Part_Specific["MIPI1_SLAVE_ADDR"];
            OTP_READ_FLAG = OTP_Procedure.OTP_Read_Lock_Bit(0); //  TestLib_Legacy.OTP_Seorakson.OTP_Read_Lock_Bit();
            if (OTP_READ_FLAG)
                SPARA_PART_LOCKBIT_FLAG = 1;
            else
                SPARA_PART_LOCKBIT_FLAG = 0;

            ////ChoonChin - Temp disable for proto
            //if ((SPARA_PART_LOCKBIT_FLAG == 1) &&
            //    !(ProductTag.Contains("QA-RF2")))
            //{
            //    SPARA_PART_LOCKBIT_FLAG = -1; //Previously burned unit or double unit
            //}

            // Add result parameter intp result builder for spec checking. added by chee on 29092017
            // S1.7 Code- commented by CCT
            //ATFResultBuilder.AddResult(ref results, "NOISEPARTPASSFLAG", "",
            //    NOISE_PART_PASS_FLAG);
            //ATFResultBuilder.AddResult(ref results, "PAPARTPASSFLAG", "", PA_PART_PASS_FLAG);

            //foreach (ATFReturnPararResult Verify in results.Data)
            //{
            //    TestLib_Legacy.ResultBuilder.Verify_Spec(Verify.Name, "", (double) Verify.Vals[0]);
            //}

            #endregion OTP check and read

            bool Q_NoFails = (ResultBuilder.FailedTests[0].Count == 0 ? true : false);

            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
            bool arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);

            #region Lockbitburn debug snippet

            //In-code, Force the lock bit to be burned
            bool forcezlockbitburn = false;

            
            if (forcezlockbitburn)
            {
                for (int i = 0; i < 1; i++)
                {
                    OTP_Procedure.OTP_Burn_Lock_Bit(0, true);
                    arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);

                    //Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                    //Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                    ////PaTest.SmuResources["Vlna"].ForceVoltage(0, 0.1);
                    ////PaTest.SmuResources["Vcc"].ForceVoltage(0, 0.1);
                    ////Thread.Sleep(5);
                    //Eq.Site[0].HSDIO.SendVector(Eq.Site[0].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));
                    ////OTP_Procedure.OTP_Burn_Lock_Bit(0, 0.1);
                    ////Thread.Sleep(5);
                    //Eq.Site[0].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                    //// reset VIO and verify burn
                    //Eq.Site[0].HSDIO.SendVector("VIOOFF".ToLower());
                    //Thread.Sleep(5);

                    //Eq.Site[0].HSDIO.SendVector("VIOON".ToLower());
                    //Thread.Sleep(5);

                    //Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
                    //arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);
                    //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);
                    //SPARA_PART_LOCKBIT_FLAG = 1;
                }
            }

            #endregion Lockbitburn debug snippet

            bool skiplockbitburn = false;
            int Fbar_Pass_Dec = 4;
            bool condition1 = !isLiteDriverMode;
            condition1 = condition1 && Q_NoFails;
            condition1 = condition1 && (PA_PART_PASS_FLAG == 1);
            condition1 = condition1 && (NOISE_PART_PASS_FLAG == 1);
            condition1 = condition1 && (SPARA_PART_LOCKBIT_FLAG == 0);
            condition1 = condition1 && (!ProductTag.Contains("QA-RF2"));
            condition1 = condition1 && (!ProductTag.Contains("OTP"));
            condition1 = condition1 && (!ProductTag.Contains("REQ"));   //For production package on OTP lock bit

            if (condition1)
            {
                #region Joker Implementation

                if (!skiplockbitburn)
                {
                    for (int i = 0; i < 1; i++) //repeat provision
                    {
                        Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                        Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                        Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                        Eq.Site[0].HSDIO.Burn(Fbar_Pass_Dec.ToString(), false, 11);

                        Eq.Site[0].HSDIO.RegWrite("1C", "40");
                        Eq.Site[0].HSDIO.SendVector("VIOOFF");
                        Thread.Sleep(1);
                        Eq.Site[0].HSDIO.SendVector("VIOON");
                        Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                        Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.0001);
                        Thread.Sleep(10);
                        Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);

                        int TempRead = Fbar_Pass_Dec & Convert.ToInt32(Eq.Site[0].HSDIO.RegRead("EB"), 16);

                        if (TempRead == Fbar_Pass_Dec)
                        {
                            Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);

                            Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                            Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                            Eq.Site[0].HSDIO.Burn("1", false, 11);

                            Eq.Site[0].HSDIO.RegWrite("1C", "40");
                            Eq.Site[0].HSDIO.SendVector("VIOOFF");
                            Thread.Sleep(1);
                            Eq.Site[0].HSDIO.SendVector("VIOON");
                            Eq.Site[0].HSDIO.RegWrite("2B", "0F");

                            Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.0001);
                            Thread.Sleep(10);
                            Eq.Site[0].DC["Vbatt"].ForceVoltage(5.5, 0.1);

                            arewedoinit = OTP_Procedure.OTP_Read_Lock_Bit(0);
                        }

                        if (arewedoinit)
                        {
                            //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);
                            //ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 1);
                            ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 1);
                            SPARA_PART_LOCKBIT_FLAG = 1;
                        }
                        else
                        {
                            //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                            //ATFResultBuilder.AddResult(ref results, lockbitheaderlabel, "x", 0);
                            ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 0);
                            SPARA_PART_LOCKBIT_FLAG = 0;
                        }
                    }
                }
                else
                {
                    //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);
                    ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 0);
                    SPARA_PART_LOCKBIT_FLAG = 0;
                }

                #endregion Joker Implementation

            }
            else if (!isLiteDriverMode && (PA_PART_PASS_FLAG == 1) && (NOISE_PART_PASS_FLAG == 1) && (SPARA_PART_LOCKBIT_FLAG == 1)
                 && (ProductTag.Contains("QA-RF2")) && !(ProductTag.Contains("OTP")) || (ProductTag.Contains("REQ") && (SPARA_PART_LOCKBIT_FLAG == 1))) //For QA & REQ test package, DUT completed lock bit, no RF fail.
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 1, 1);       //QA package & REQ, Lockbit DUT with PASS/FAIL RF testing
                ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 1);

            }
            else if (!isLiteDriverMode && !Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && !(ProductTag.Contains("QA-RF2")) && !(ProductTag.Contains("OTP")))
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 2, 1); //FAIL RF testing and no lock bit PROD package
                ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 2);
                SPARA_PART_LOCKBIT_FLAG = 2;
            }
            else if (Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && (ProductTag.Contains("QA-RF2"))
                || Q_NoFails && (SPARA_PART_LOCKBIT_FLAG == 0) && (ProductTag.Contains("REQ")))
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", 0, 1);    // PASS RF spec but DUT not burn at QA/REQ package
                ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 0);
            }
            else if ((NOISE_PART_PASS_FLAG == 1) && (SPARA_PART_LOCKBIT_FLAG == 1)   //for Pass Bin(burned units retest 
                && (SPARA_PART_LOCKBIT_FLAG == 1) && !isLiteDriverMode && Q_NoFails)
            {
                int TempRead = Fbar_Pass_Dec & Convert.ToInt32(Eq.Site[0].HSDIO.RegRead("E3"), 16);

                if (TempRead == Fbar_Pass_Dec) SPARA_PART_LOCKBIT_FLAG = 1;

                ResultBuilder.AddResult(0, lockbitheaderlabel, "x", 1.2);

            }
            else
            {
                //ResultBuilder.AddResult(0, lockbitheaderlabel, "", -1, 1);  //other unknown scenerios failure
                ResultBuilder.AddResult(0, lockbitheaderlabel, "x", -1);
            }

            StopWatchManager.Instance.Stop("DoATFTest_OTP_and_Lockbit_Burn_Iter_" + x.ToString(),0);

            return SPARA_PART_LOCKBIT_FLAG;
        }

        /// <summary>
        /// Generate waveData, read Topaz temp.
        /// </summary>
        public void DoPreTest(ATFReturnResult results,
        SParaTestConditionFactory spDo, SParaTestManager LibFbar, string clothoRootDir)
        {
            s_SNPFile snpFile = DoAtfTest2(clothoRootDir);

            ////ChoonChin - 20191122 - Disable touch stone file when TCF settings is false
            //LibFbar.FileManager.SNP_Sampling_Enabled = spDo.SamplingTraceFileEnable;
            LibFbar.FileManager.SNP_Sampling_Enabled = spDo.EnaStateFileEnable;
            LibFbar.FileManager.SNP_Sampling_Interval = spDo.TraceFileOutput_Count_Sampling;

            LibFbar.FileManager.SetSnpFile(snpFile.FileOutput_FileName,
                snpFile.FileOutput_Path, snpFile.FileOutput_HeaderName);

            //if (String.IsNullOrEmpty(snpFile.FileOutput_FileName)) return; - ChoonChin - 20191205 - Cannot return because below function are needed.

            #region Topaz tempearature read back

            //ChoonChin - Add Topaz module temperature reading
            if (false)
            {
                ReadBackTopazTemperature(results, LibFbar);
            }

            #endregion Topaz tempearature read back

            #region Test board temperature

            //Not used : Commented out for HLS2.
            //double load_board_temperature_FBAR = Eq.Site[0].HSDIO.I2CTEMPSENSERead();
            //ATFResultBuilder.AddResult(ref results, "M_Temp_Loadboard", "C", load_board_temperature_FBAR);

            #endregion Test board temperature

            #region DUT temperature

            //To be added

            #endregion DUT temperature

            ResultBuilder.FailedTests[0].Clear();

            //For new OQA bin trace saving
            bool isCnTracerEnabled = false;
            try
            {
                isCnTracerEnabled = ATFCrossDomainWrapper.GetTraceWriteFlag(); //Older Clotho version not supporting this feature.
            }
            catch
            {
                LoggingManager.Instance.LogError("Clotho CN trace function not found!");
            }
            LibFbar.FileManager.CnTracerEnable = isCnTracerEnabled;

        }

        private static void ReadBackTopazTemperature(ATFReturnResult results,
            SParaTestManager LibFbar)
        {
//double Mod01 = Convert.ToDouble(LibFbar.TopazTempValue(1)); //Source Output
            //double Mod02 = Convert.ToDouble(LibFbar.TopazTempValue(2)); //Synthesizer
            //double Mod03 = Convert.ToDouble(LibFbar.TopazTempValue(3)); //Reference
            //double Mod04 = Convert.ToDouble(LibFbar.TopazTempValue(4)); //Synthesizer
            //double Mod05 = Convert.ToDouble(LibFbar.TopazTempValue(5)); //Distributor
            double Mod06 = Convert.ToDouble((string) LibFbar.TopazTempValue(6)); //VNA Receiver -P1
            double Mod07 = Convert.ToDouble((string) LibFbar.TopazTempValue(7)); //VNA Receiver -P2
            double Mod08 = Convert.ToDouble((string) LibFbar.TopazTempValue(8)); //VNA Receiver -P3
            double Mod09 = Convert.ToDouble((string) LibFbar.TopazTempValue(9)); //VNA Receiver -P4
            double Mod10 = Convert.ToDouble((string) LibFbar.TopazTempValue(10)); //VNA Receiver -P5
            double Mod11 = Convert.ToDouble((string) LibFbar.TopazTempValue(11)); //VNA Receiver -P6
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port1", "DegC", Mod06);
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port2", "DegC", Mod07);
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port3", "DegC", Mod08);
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port4", "DegC", Mod09);
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port5", "DegC", Mod10);
            ATFResultBuilder.AddResult(ref results, "VNA_TS_Port6", "DegC", Mod11);
        }
    }
}