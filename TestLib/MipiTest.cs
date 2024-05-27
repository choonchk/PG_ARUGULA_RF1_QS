using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using Avago.ATF.StandardLibrary;
using ProductionLib;

namespace TestLib
{

    public class MipiTest : TimingBase, iTest
    {
        public bool Initialize(bool finalScript)
        {
            InitializeTiming(this.TestCon.TestParaName);
            return true;
        }

        public byte Site;
        public MipiTestConditions TestCon = new MipiTestConditions();
        public MipiTestResults TestResult;
        public static Dictionary<string, int>[] PrevMipiReadbackDic = new Dictionary<string, int>[Eq.NumSites];
        public HiPerfTimer uTimer = new HiPerfTimer();

        public int RunTest()
        {
            try
            {
                SwBeginRun(Site);

                TestResult = new MipiTestResults();

                if (ResultBuilder.headerFileMode) return 0;
                Eq.Site[Site].HSDIO.dutSlaveAddress = TestCon.SlaveAddr;

                this.ConfigureVoltageAndCurrent();

                if (TestCon.MipiCommands.Count != 0)
                    Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);


                //  Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);

                RunTestCore();
                SwEndRun(Site);

                return 0;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }
        }

        private void RunTestCore()
        {
            try
            {
                //if (TestCon.InitTest)
                if (true)
                {
                    #region READ REG

                    string msg = String.Format("RunTestCore-{0}", TestCon.PowerMode);
                    SwStartRun(msg, Site);

                    if (TestCon.PowerMode.ToUpper().Contains("READTX"))
                    {
                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;

                        if ((EqHSDIO.ADJUST_BusIDCapTuningInHex != "") && (!TestCon.Reg_hex.Contains("43")))
                        {
                            Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                        }


                        if (TestCon.Reg_hex.StartsWith("E")) // NUWA 4JM5A Seoul 65nm CMOS requires a specefic process to readback from bytes E5-E8
                        {
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            Eq.Site[Site].HSDIO.RegWrite("F0", "B0", false);
                            uTimer.wait(1);

                            if (Convert.ToInt32(TestCon.Reg_hex.Substring(1), 16) < 8)
                            {
                                Eq.Site[Site].HSDIO.RegWrite("F0", "B1", false);
                                uTimer.wait(1);
                                Eq.Site[Site].HSDIO.RegWrite("F0", "A1", false);
                                uTimer.wait(1);
                            }
                            else
                            {
                                Eq.Site[Site].HSDIO.RegWrite("F0", "B2", false);
                                uTimer.wait(1);
                                Eq.Site[Site].HSDIO.RegWrite("F0", "92", false);
                                uTimer.wait(1);
                            }
                            Eq.Site[Site].HSDIO.RegWrite("F1", "1" + TestCon.Reg_hex.Substring(1), false);
                            uTimer.wait(1);
                            TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("82"), 16);
                            uTimer.wait(1);

                            Eq.Site[Site].HSDIO.RegWrite("F0", "B0", false);
                            uTimer.wait(1);
                            Eq.Site[Site].HSDIO.RegWrite("F0", "30", false);
                            uTimer.wait(1);
                        }
                        else if (TestCon.Reg_hex.Contains("81"))
                        {
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
                            TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex, true), 16);
                        }
                        else if (TestCon.Reg_hex.Contains("43"))
                        {
                            TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex), 16);
                            double temperaturePerBit = ((TestCon.maxTemp - TestCon.minTemp) / (TestCon.numTempSteps));
                            TestResult.ReadTempSense = (TestCon.minTemp + TestResult.MipiRead * temperaturePerBit);
                        }
                        else
                        {
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                            TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex), 16);
                        }

                        SwStopRun(msg, Site);
                        return;
                    }
                    else if (TestCon.PowerMode.ToUpper().Contains("READRX"))
                    {
                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 2;
                        Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                        TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex), 16);
                        SwStopRun(msg, Site);
                        return;
                    }
                    else if (TestCon.PowerMode.ToUpper().Contains("READTEMP"))
                    {
                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;
                        Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                        Eq.Site[Site].HSDIO.SendVector("VIOON");

                        Eq.Site[Site].HSDIO.RegWrite("43", "00");


                        TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex), 16);

                        double temperaturePerBit = ((TestCon.maxTemp - TestCon.minTemp) / (TestCon.numTempSteps));
                        TestResult.ReadTempSense = (TestCon.minTemp + TestResult.MipiRead * temperaturePerBit);

                        SwStopRun(msg, Site);
                        return;
                    }
                    else if (TestCon.PowerMode.ToUpper().Contains("QC"))
                    {
                        //20200707 Mario QC error vector debugging adding VIOOFF-ON
                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");
                        /////////////////////////////////////////////////////////////

                        if (TestCon.PowerMode.ToUpper().Contains("TX"))
                        {
                            Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                            Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                        }
                        else
                        {
                            Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                            Eq.Site[Site].HSDIO.dutSlavePairIndex = 2;
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                        }

                        //Eq.Site[Site].HSDIO.SendVector(TestCon.WaveformName.ToUpper());
                        //TestResult.MipiNumBitErrors = Eq.Site[Site].HSDIO.GetNumExecErrors(TestCon.WaveformName);

                        foreach (var vec in TestCon.RunningVectorBags)
                        {
                            Eq.Site[Site].HSDIO.SendVector(vec);
                        }

                        foreach (var vec in TestCon.RunningVectorBags)
                        {
                            int failc = Eq.Site[Site].HSDIO.GetNumExecErrors(vec);
                            TestResult.MipiNumBitErrors += failc;
                        }

                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");

                        if (TestResult.MipiNumBitErrors != 0)
                            ResultBuilder.AddFailedQCTest(Site, TestCon.WaveformName);
                    }
                    else if (TestCon.PowerMode.ToUpper().Contains("UNITID"))
                    {

                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;
                        Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");

                        if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                        {
                            Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                        }

                        string[] Addresses = TestCon.WaveformName.Split(',');

                        int E4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Addresses[0]), 16);
                        int E5 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Addresses[1]), 16);

                        E4 = (E4 & 0x7F) << 8;

                        TestResult.MipiRead = E4 + E5;
                    }

                    else if (TestCon.PowerMode.ToUpper().Contains("OTP"))
                    {
                        int Rev_ID_Dec = 0;
                        string Hex_Value = "";

                        Rev_ID_INFORMATION rev_id = Rev_ID_INFORMATION.NONE;
                        if (Enum.TryParse<Rev_ID_INFORMATION>(TestCon.Rev.Trim().ToUpper(), out rev_id))
                        {
                            Hex_Value = Convert.ToInt16(rev_id).ToString("X");
                            Rev_ID_Dec = int.Parse(Hex_Value, System.Globalization.NumberStyles.HexNumber);
                        }

                        string siteDefine = TestCon.TesterID;

                        if (TestCon.TRX == "TX---") // not for Pinot
                        {
                            #region
                            Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                            Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            //int TxE0Data = 0;
                            //int TxE1Data = 0;
                            int TxE2Data = Rev_ID_Dec;
                            //int TxE3Data = 0;   // pass flags
                            //int TxE4Data = Convert.ToInt16(siteDefine) - 1;
                            //int TxE5Data = 0;
                            //int TxEBData = 0;   // lock bit
                            //int sUnitID = 0;

                            bool Flag = false;

                            //try { sUnitID = Convert.ToInt32(ATFCrossDomainWrapper.GetClothoCurrentSN()); }
                            //catch { sUnitID = 0; } //Debug Mode



                            //int E0 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E0"), 16);
                            //if (E0 != 0) Flag = true;
                            //int E1 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E1"), 16);
                            //if (E1 != 0) Flag = true;
                            int E2 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E2"), 16);
                            if (E2 != 0) Flag = true;
                            //int E3 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E3"), 16);
                            //if (E3 != 0) Flag = true;
                            //int E4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E4"), 16);
                            //if (E4 != 0) Flag = true;
                            //int E5 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E5"), 16);
                            //if (E5 != 0) Flag = true;
                            //int EB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);
                            //if (EB != 0) Flag = true;

                            if (!Flag)
                            {
                                //TxE4Data = (sUnitID >> 8) | TxE4Data;
                                //TxE5Data = sUnitID & 0xFF;

                                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                                if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                                {
                                    Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                                }
                                //Eq.Site[Site].HSDIO.Burn(TxE0Data.ToString("X"), false, 0);
                                //Eq.Site[Site].HSDIO.Burn(TxE1Data.ToString("X"), false, 1);
                                Eq.Site[Site].HSDIO.Burn(TxE2Data.ToString("X"), false, 2);
                                //Eq.Site[Site].HSDIO.Burn(TxE3Data.ToString("X"), false, 3);
                                //Eq.Site[Site].HSDIO.Burn(TxE4Data.ToString("X"), false, 4);
                                //Eq.Site[Site].HSDIO.Burn(TxE5Data.ToString("X"), false, 5);

                                Eq.Site[Site].HSDIO.RegWrite("1C", "40");


                                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                                Eq.Site[Site].HSDIO.SendVector("VIOON");

                                if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                                {
                                    Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                                }

                                Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                                uTimer.wait(10);
                                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                                //E0 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E0"), 16);
                                //E1 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E1"), 16);
                                E2 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E2"), 16);
                                //E3 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E3"), 16);
                                //E4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E4"), 16);
                                //E5 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E5"), 16);

                                //if (TxE0Data == E0 && TxE1Data == E1 && TxE2Data == E2 && TxE3Data == E3)
                                //{
                                //    Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                                //    Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                                //    Eq.Site[Site].HSDIO.RegWrite("2B", "0F");

                                //    Eq.Site[Site].HSDIO.Burn(TxEBData.ToString("X"), false, 11);


                                //    Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                                //    Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                                //    Thread.Sleep(1);
                                //    Eq.Site[Site].HSDIO.SendVector("VIOON");

                                //    Eq.Site[Site].HSDIO.RegWrite("2B", "0F");

                                //    Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                                //    Thread.Sleep(10);
                                //    Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                                //    EB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);

                                //}

                            }


                            //else if (TxE0Data == E0 && TxE1Data == E1 && TxE2Data == E2 && TxE3Data == E3)
                            //{
                            //    EB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);
                            //    if (EB != 0)
                            //    {
                            //        Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                            //        Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            //        Eq.Site[Site].HSDIO.RegWrite("2B", "0F");

                            //        Eq.Site[Site].HSDIO.Burn(TxEBData.ToString("X"), false, 11);


                            //        Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            //        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                            //        Thread.Sleep(1);
                            //        Eq.Site[Site].HSDIO.SendVector("VIOON");

                            //        Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                            //        Thread.Sleep(10);
                            //        Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                            //        EB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);
                            //    }

                            //}

                            //if (TxE0Data == E0 && TxE1Data == E1 && TxE2Data == E2 && TxE3Data == E3 && TxEBData == 1) TestResult.MipiNumBitErrors = 0;
                            if (TxE2Data == E2) TestResult.MipiNumBitErrors = 0;
                            else TestResult.MipiNumBitErrors = 1;
                            #endregion
                        }
                        else if (TestCon.TRX == "TX") //PinotTx OTP
                        {
                            int TxE2Data = Rev_ID_Dec;
                            int TxE4Data = 0;
                            int TxE5Data = 1;
                            int sUnitID = 0;

                            bool flag = false;

                            try { sUnitID = Convert.ToInt32(ATFCrossDomainWrapper.GetClothoCurrentSN()); }
                            catch { sUnitID = 0; } //Debug Mode

                            TxE4Data = (sUnitID >> 8) | TxE4Data;
                            TxE5Data = sUnitID & 0xFF;

                            Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                            Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;

                            int E2 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E2"), 16);
                            if (E2 != 0) flag = true;
                            //int E3 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E3"), 16);
                            //if (E3 != 0) Flag = true;
                            int E4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E4"), 16);
                            if (E4 != 0) flag = true;
                            int E5 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E5"), 16);
                            if (E5 != 0) flag = true;
                            int EB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);
                            if (EB != 0) flag = true;

                            if (!flag)
                            {
                                //Mipi & bais Reset
                                Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                                uTimer.wait(10);
                                Eq.Site[Site].HSDIO.SendVector("VIOON");
                                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.15, 0.2);
                                //Eq.Site[Site].DC["Vio1"].ForceVoltage(2.5, 0.032);
                                //Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);

                                Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                                if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                                {
                                    Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                                }
                                //Band1 Actirve
                                Eq.Site[Site].HSDIO.RegWrite("00", "01");
                                Eq.Site[Site].HSDIO.RegWrite("02", "02");
                                Eq.Site[Site].HSDIO.RegWrite("04", "02");
                                Eq.Site[Site].HSDIO.RegWrite("05", "08");
                                Eq.Site[Site].HSDIO.RegWrite("0A", "01");
                                Eq.Site[Site].HSDIO.RegWrite("1C", "07");

                                //OTP LDO & PGM enable
                                Eq.Site[Site].HSDIO.RegWrite("C0", "80");
                                Eq.Site[Site].HSDIO.RegWrite("C0", "C0");
                                uTimer.wait(1);

                                //Burn bits - A4A
                                Eq.Site[Site].HSDIO.RegWrite("C1", "10");
                                //Eq.Site[Site].HSDIO.RegWrite("C1", "14");
                                //Eq.Site[Site].HSDIO.RegWrite("C1", "15");
                                Eq.Site[Site].HSDIO.RegWrite("C1", "16");


                                // burn Unit ID at Add          
                                int DataIndicator = 0x28;
                                int AddIndicator = 0;
                                string AddrIndicator = "";
                                bool invertData = false;

                                for (int bit = 0; bit < 16; bit++)
                                {
                                    if (bit == 8) AddIndicator = -16;
                                    AddrIndicator = Convert.ToInt16(DataIndicator + AddIndicator + bit).ToString("X");

                                    int bitVal = (int)Math.Pow(2, bit);

                                    if ((bitVal & sUnitID) == (invertData ? 0 : bitVal))
                                    {
                                        Eq.Site[Site].HSDIO.RegWrite("C1", AddrIndicator);
                                    }
                                }


                                //Burn bits - Lockbit
                                Eq.Site[Site].HSDIO.RegWrite("C1", "58");

                                //OTP LDO & PGM Disable
                                Eq.Site[Site].HSDIO.RegWrite("C0", "00");

                                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                                uTimer.wait(10);
                                Eq.Site[Site].HSDIO.SendVector("VIOON");

                            }

                            int AddressE2 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("E2"), 16);
                            int AddressEB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("EB"), 16);

                            //if (TxE2Data == E2) TestResult.MipiNumBitErrors = 0;
                            //else TestResult.MipiNumBitErrors = 1;
                        }
                        else
                        {
                            Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                            Eq.Site[Site].HSDIO.dutSlavePairIndex = 2;
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            //int AddressD4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("D4"), 16);
                            int AddressD4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("FC"), 16); //PINOT
                            int Address21 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("21"), 16);
                            //int AddressB5 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("C9"), 16); //PINOT

                            if (Address21 != Rev_ID_Dec)
                            {
                                //Eq.Site[Site].DC["Vio2"].ForceVoltage(2.5, 0.032); //Pinot

                                //Eq.Site[Site].DC["Vdd"].ForceVoltage(1.2, 0.1); //Pinot
                                Eq.Site[Site].HSDIO.RegWrite("F0", "00");
                                Eq.Site[Site].HSDIO.RegWrite("F0", "C0");

                                Eq.Site[Site].DC["Vdd"].ForceVoltage(2.5, 0.1); //Pinot

                                Eq.Site[Site].HSDIO.SendVectorOTP(Rev_ID_Dec.ToString("X"), "00", true);

                                Eq.Site[Site].HSDIO.RegWrite("F0", "20");
                                Eq.Site[Site].HSDIO.RegWrite("F0", "10");

                                Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.0001);
                                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                                uTimer.wait(1);
                                Eq.Site[Site].HSDIO.SendVector("VIOON");

                                AddressD4 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("FC"), 16); //PINOT
                                Address21 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("21"), 16);

                                //Address21 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("F8"), 16); //PINOT
                            }
                        }

                    }

                    SwStopRun(msg, Site);

                    #endregion

#if false
                    #region FUNCTIONAL
                    if (TestCon.WaveformName.ToUpper().Contains("FUNCTIONAL"))
                    {

                        Eq.Site[Site].HSDIO.SendVector(TestCon.WaveformName.ToUpper());
                        TestResult.MipiNumBitErrors = Eq.Site[Site].HSDIO.GetNumExecErrors(TestCon.WaveformName);

                    }
                    #endregion

                    #region PDM
                    if (TestCon.WaveformName.ToUpper().Contains("PDM"))
                    {

                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        TestResult.MipiNumBitErrors = Eq.Site[Site].HSDIO.GetNumExecErrors(TestCon.WaveformName);

                    }

                    #endregion

                    #region READ TEMPSENSE
                    if (TestCon.WaveformName.ToUpper().Contains("READTS"))
                    {

                        Eq.Site[Site].HSDIO.SendVector("RESET");

                        Eq.Site[Site].HSDIO.RegWrite("2B", "04", false); // set MIPI drive strength
                        Thread.Sleep(20);

                        Eq.Site[Site].HSDIO.RegWrite(TestCon.Reg_hex, "FF", false); // write to register to enable temp sense
                        Thread.Sleep(20);
                        TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex), 16);   // added to read the register. Existing code did not sent vector. KH 4/4/2016
                                                                                                                   //double TempCalc = 255 - TestResult.MipiRead;
                                                                                                                   //double TempSenseResult = 137 - (0.65 * (TempCalc - 1));  // this old formula may not be correct for all parts

                        // Hopefully more generic formula that will work for all parts - KH 4/10/2017
                        double temperaturePerBit = ((TestCon.maxTemp - TestCon.minTemp) / (TestCon.numTempSteps));
                        TestResult.ReadTempSense = (TestCon.minTemp + TestResult.MipiRead * temperaturePerBit);

                    }
                    #endregion


                    #region ReadID from register
                    if (TestCon.WaveformName.ToUpper().Contains("READPID"))
                    {
                        TestResult.MipiRead = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(TestCon.Reg_hex),16);


                        //Eq.Site[Site].HSDIO.shmoo(TestCon.Reg_hex); // used to determine the best strobe delay KH

                    }
                    #endregion
                }

#endif
                }
            }
            catch (Exception e)
            {

            }
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            if (ResultBuilder.headerFileMode) return;

            if (TestCon.PowerMode.ToUpper().Contains("READ"))
            {
                if (TestCon.PowerMode.ToUpper().Contains("TX"))
                {
                    if (TestCon.paramNote != "")
                    {
                        if(TestCon.paramNote.ToUpper().Contains("ASW_WAFER-ID_NO_MASK"))
                        {
                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE_" + TestCon.paramNote, "", TestResult.MipiRead);
                            //ADD result to dictionary for next test
                            if( PrevMipiReadbackDic[Site] == null )
                            {
                                PrevMipiReadbackDic[Site] = new Dictionary<string, int>
                                {
                                    { "ASW_WAFER-ID_NO_MASK", TestResult.MipiRead }
                                };
                            }
                            else
                            {
                                if(PrevMipiReadbackDic[Site].ContainsKey("ASW_WAFER-ID_NO_MASK"))
                                {
                                    PrevMipiReadbackDic[Site]["ASW_WAFER-ID_NO_MASK"] = TestResult.MipiRead;
                                }
                                else
                                {
                                    PrevMipiReadbackDic[Site].Add("ASW_WAFER-ID_NO_MASK", TestResult.MipiRead);
                                }
                            }
                            int ASWWaferID = (TestResult.MipiRead & 248) >> 3;
                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE_" + "ASW_WAFER-ID", "", ASWWaferID);
                            int ASWLOCKBIT = TestResult.MipiRead & 1;
                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE_" + "ASW_LOCK_BIT", "", ASWLOCKBIT);
                        }
                        else if(TestCon.paramNote.ToUpper().Contains("STATUS_ASW_WAFER-LOT"))
                        {
                            //find previous results to complete the wafer Lot 
                            int MSB = 0;
                            if (PrevMipiReadbackDic[Site].ContainsKey("ASW_WAFER-ID_NO_MASK"))
                            {
                                MSB = (int)PrevMipiReadbackDic[Site]["ASW_WAFER-ID_NO_MASK"] & 6;
                                MSB = MSB << 7; // shift 7 bits instead of 8. Because masking with "6" by default shifted 1 bit to the left
                            }
                            int ASWWAFERLOT = TestResult.MipiRead + MSB;
                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE_" + TestCon.paramNote, "", ASWWAFERLOT);
                        }
                        else
                        {
                                ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE_" + TestCon.paramNote, "", TestResult.MipiRead);
                        }
                    }
                    else
                    {
                        if (TestCon.Reg_hex.Contains("43"))
                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE", "", TestResult.ReadTempSense);
                        else

                            ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadTxReg-" + TestCon.WaveformName + "_NOTE", "", TestResult.MipiRead);
                    }
                }
                else if (TestCon.PowerMode.ToUpper().Contains("RX"))
                {
                    if (TestCon.paramNote != "")
                    {
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadRxReg-" + TestCon.WaveformName + "_NOTE_" + TestCon.paramNote, "", TestResult.MipiRead);
                    }
                    else
                    {
                        //ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadRxReg-" + TestCon.WaveformName + "_NOTE_x", "", TestResult.MipiRead);
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadRxReg-" + TestCon.WaveformName + "_NOTE_", "", TestResult.MipiRead);  //hosein 05052020
                    }
                }
                else if (TestCon.PowerMode.ToUpper().Contains("TEMP"))
                {
                    if (TestCon.paramNote != "")
                    {
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_TEMP_NOTE_" + TestCon.paramNote, "", TestResult.ReadTempSense);
                    }
                    else
                    {
                        //ResultBuilder.AddResult(Site, TestCon.TestParaName + "_TEMP_NOTE_x", "", TestResult.ReadTempSense);  //hosein 0505202
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_TEMP_NOTE_", "", TestResult.ReadTempSense);
                    }
                }

            }
            else if (TestCon.PowerMode.ToUpper().Contains("QC"))
            {
                if (TestCon.PowerMode.ToUpper().Contains("TX"))
                {
                    if (TestCon.paramNote != "")
                    {
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_" + TestCon.paramNote, "", TestResult.MipiNumBitErrors);
                    }
                    else
                    {
                        //ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_x", "", TestResult.MipiNumBitErrors);
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_", "", TestResult.MipiNumBitErrors); //hosein 05052020
                    }
                }
                else
                {
                    if (TestCon.paramNote != "")
                    {
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_" + TestCon.paramNote, "", TestResult.MipiNumBitErrors);
                    }
                    else
                    {
                        //ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_x", "", TestResult.MipiNumBitErrors);  //hosein 05052020
                        ResultBuilder.AddResult(Site, TestCon.TestParaName + "_" + TestCon.Reg_hex + "_NOTE_", "", TestResult.MipiNumBitErrors);
                    }
                }

            }
            else if (TestCon.PowerMode.ToUpper().Contains("UNITID"))
            {
                ResultBuilder.AddResult(Site, "OTP_MODULE_ID", "", TestResult.MipiRead);
            }
            else if (TestCon.PowerMode.ToUpper().Contains("OTP"))
            {
                if (TestCon.TRX.ToUpper() == "TX")
                {
                    ResultBuilder.AddResult(Site, TestCon.TestParaName + "Flag-Tx", "", TestResult.MipiNumBitErrors);
                }
                else
                {
                    ResultBuilder.AddResult(Site, TestCon.TestParaName + "Flag-Rx", "", TestResult.MipiNumBitErrors);
                }

            }

            //switch (TestCon.PowerMode.ToUpper())
            //{
            //    case "READTXPID":
            //    case "READRXPID":
            //    case "READTXMID":
            //    case "READRXMID":
            //    case "READTXUSID":
            //    case "READRXUSID":
            //    case "READTXREGE0":
            //    case "READTXREGE1":
            //    case "READTXREGE2":
            //    case "READTXREGE3":
            //    case "READTXREGE4":
            //    case "READTXREGEB":
            //    case "READRXREG21":
            //    case "READRXREGD4":


            //        break;

            //    //case "READREG":
            //    //    ResultBuilder.AddResult(Site, TestCon.TestParaName + "_ReadValue", "", TestResult.MipiRead);
            //    //    break;
            //    //case "FUNCTIONAL_TX":
            //    //case "FUNCTIONAL_RX":
            //    //case "FUNCTIONAL":
            //    //    ResultBuilder.AddResult(Site, TestCon.TestParaName + "_NumBitErrors", "", TestResult.MipiNumBitErrors);
            //    //    break;
            //    //case "READPID":
            //    //    ResultBuilder.AddResult(Site, "M_" + "OTP_PRODUCT_ID", "", TestResult.MipiRead);  // changed to match parameter name preference in Penang even though it is not really OTP. Product ID is usually hardwired KenH
            //    //    break;
            //    //case "READTS":
            //    //    ResultBuilder.AddResult(Site, TestCon.TestParaName, "", TestResult.ReadTempSense);
            //    //    break;
            //    default:
            //        break;
            // }
        }

        private void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                string msg = String.Format("ForceVoltage on pin {0}", pinName);
                SwStartRun(msg, Site);

                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper()))
                {
                    SwStopRun(msg, Site);
                    continue; // don't force voltage on MIPI pins                    
                }

                if (pinName.ToUpper() == "VIO1")
                {
                    Eq.Site[Site].HSDIO.ReInitializeVIO(TestCon.DcSettings[pinName].Volts);
                    SwStopRun(msg, Site);
                    continue;
                }

                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
                SwStopRun(msg, Site);
            }

            if (TestCon.VIORESET)
            {
                if (TestCon.PowerMode.ToUpper().Contains("TX"))
                    Eq.Site[Site].HSDIO.SendVector(EqHSDIO.PPMUVioOverrideString.RESET_TX.ToString());
                else if (TestCon.PowerMode.ToUpper().Contains("RX"))
                    Eq.Site[Site].HSDIO.SendVector(EqHSDIO.PPMUVioOverrideString.RESET_RX.ToString());

            }

            if (TestCon.VIO32MA)
            {
                if (TestCon.PowerMode.ToUpper().Contains("TX"))
                    Eq.Site[Site].HSDIO.SendVector(EqHSDIO.PPMUVioOverrideString.VIOON_TX.ToString());
                else if (TestCon.PowerMode.ToUpper().Contains("RX"))
                    Eq.Site[Site].HSDIO.SendVector(EqHSDIO.PPMUVioOverrideString.VIOON_RX.ToString());
            }

        }

        public enum Rev_ID_INFORMATION : int
        {
            NONE = 0x00,
            A1A = 0x11,
            A1B = 0x12,
            A1C = 0x13,
            A2A = 0x21,
            A2B = 0x22,
            A2C = 0x23,
            A3A = 0x31,
            A4A = 0x41,
            A5A = 0x51,
            A5B = 0x52,
            A5C = 0x53,
            A6A = 0x61,
            A6B = 0x62,
            A6C = 0x63,
            A6D = 0x64,
            A7A = 0x71,
            A8A = 0x81,
            A9A = 0x91,
            B1A = 0xB1, // for EVT3 - Carrier
            B1B = 0xB2,
            B1C = 0xB3,
            B1D = 0xB4,
            B1E = 0xB5,
            TA1A = 0xD0,
            DVT = 0xF1,
            PROD = 0xF1,
            PRODUCTION = 0xF1
        }
    }

    public class MipiTestConditions
    {
        public string paramNote;
        public string TestParaName;
        public string WaveformName;
        public string PowerMode;
        public string Rev;
        public string TRX;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public string Reg_hex;
        public string SlaveAddr;
        public double minTemp = -20;
        public double maxTemp = 130;
        public double numTempSteps = 255;
        public string TesterID;
        public List<string> RunningVectorBags = new List<string>();
        public bool VIO32MA = false;
        public bool VIORESET = false;

    }

    public class MipiTestResults
    {
        public int MipiNumBitErrors;
        public int MipiRead;
        public double ReadTempSense;
    }
}
