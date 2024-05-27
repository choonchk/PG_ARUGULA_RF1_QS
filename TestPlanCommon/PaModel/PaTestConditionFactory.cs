using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ClothoLibAlgo;
using EqLib;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.PaModel
{
    public class PaTestConditionFactory
    {
        private Dictionary<string, string> TestCond;
        private string[] DcResourceTempList;

        public PaTestConditionFactory(Dictionary<string, string> TestCond, string[] DcResourceTempList)
        {
            this.TestCond = TestCond;
            this.DcResourceTempList = DcResourceTempList;
        }

        /// <summary>
        /// Read DC resource from TCF.
        /// </summary>
        /// <param name="TestCond"></param>
        /// <param name="dcResourceSheet"></param>
        public PaTestConditionFactory(Dictionary<string, string> TestCond,
            TcfSheetReader dcResourceSheet)
        {
            this.TestCond = TestCond;
            Dictionary.Ordered<string, string[]> dcResourceDict = GetDcResourceDefinitions(dcResourceSheet);
            this.DcResourceTempList = dcResourceDict.Keys.ToArray();

        }

        public bool IsExist(string TestStr)
        {
            return TestCond.ContainsKey(TestStr);
        }

        public bool IsTest(string TestStr)
        {
            bool Result = false;
            if (TestStr.ToUpper() != "")
                Result = true;


            return Result;
        }

        public bool IsTest(string Para, string TestStr, Dictionary<string, string> SpecNumber)
        {


            bool Result = false;
            if (TestStr.ToUpper() != "")
                Result = true;

            if (TestStr.ToUpper() != "")
            {
                if (TestStr.ToUpper() == "V")
                {
                    TestStr = TestStr.Replace("V", "x");
                    SpecNumber.Add(Para, TestStr);
                }
                else
                {
                    TestStr = TestStr.Replace("_", "-");
                    SpecNumber.Add(Para, TestStr);
                }
            }

            return Result;
        }

        public string GetStr(string theKey)
        {
            try
            {
                return TestCond[theKey];
            }
            catch
            {
                throw new Exception("Test Condition File doesn't contain column \"" + theKey + "\"");
            }
        }

        public double GetDbl(string theKey)
        {
            string valStr = "";
            try
            {
                valStr = TestCond[theKey];
                if (valStr.ToUpper() == "X" || valStr.ToUpper() == "") return 0;
            }
            catch
            {
                throw new Exception("Test Condition File doesn't contain column \"" + theKey + "\"");
            }

            double valDbl = 0;
            try
            {
                valDbl = Convert.ToDouble(valStr);
            }
            catch
            {
                throw new Exception("Test Condition File contains non-number \"" + valStr + "\" in column \"" + theKey + "\"");
            }

            return valDbl;
        }

        public string GetTestMode()
        {
            string TestMode = GetStr("Test Mode").Trim().ToUpper();

            return TestMode;
        }

        public Operation GetVsaOperation()
        {
            if (GetTXRX().Contains("TX"))
            {
                switch (GetANTport())
                {
                    case "ANT1":
                        return Operation.VSAtoANT1;
                    case "ANT2":
                        return Operation.VSAtoANT2;
                    case "OUT-FBRX":    //by Hosein
                        return Operation.VSAtoRX;    //By Hosein
                    case "ANTU":
                    case "ANT3":
                    case "ANT-UAT":
                        return Operation.VSAtoANT3;
                    default:
                        MessageBox.Show("Not exist Port in Caldictional", "Assign Port Error");
                        return Operation.VSAtoANT;
                }
            }
            else
            {
                switch (GetRXport())
                {
                    case "OUT-FBRX":
                        return Operation.VSAtoRX;
                    case "OUT1-N77":
                        return Operation.VSAtoRX2;
                    case "OUT3-N79":
                        return Operation.VSAtoRX1;
                    case "OUT4-N79":
                        return Operation.VSAtoRX3;
                    case "OUT2-N77":
                        return Operation.VSAtoRX4;
                    default:
                        MessageBox.Show("Not exist Port in Caldictional", "Assign Port Error");
                        return Operation.VSAtoRX;
                }
                return Operation.VSAtoRX;
            }

        }

        public Operation GetVsgOperation()
        {
            if (GetTXRX().Contains("TX"))
            {
                switch (GetTXINport())
                {
                    case "IN-FBRX":
                    case "IN3-N77":
                        return Operation.VSGtoTX;
                    case "IN1-N77":
                        return Operation.VSGtoTX1;
                    case "IN2-N79":
                        return Operation.VSGtoTX2;
                    case "ANTL":
                        return Operation.VSGtoANT4;

                    default:
                        MessageBox.Show("Not exist Port in Caldictional", "Assign Port Error");
                        return Operation.VSGtoTX;
                }
            }
            else
            {
                switch (GetANTport())
                {
                    case "ANT1":
                        return Operation.VSGtoANT1;
                    case "ANT2":
                        return Operation.VSGtoANT2;
                    case "ANTU":
                    case "IN-FBRX":    //By Hosein
                        return Operation.VSGtoTX;   //By Hosein
                    case "ANT-UAT":
                    case "ANT3":
                        return Operation.VSGtoANT3;
                    case "ANTL":
                        return Operation.VSGtoANT4;
                    default:
                        MessageBox.Show("Not exist Port in Caldictional", "Assign Port Error");
                        return Operation.VSGtoANT;
                }
            }

        }

        public Operation GetCustomVsaOperation()
        {
            if (IsTest(GetStr("Para.H2")))
            {
                switch (GetANTport())
                {
                    case "ANT1":
                        return Operation.MeasureH2_ANT1;
                    case "ANT2":
                        return Operation.MeasureH2_ANT2;
                    case "ANTU":
                    case "ANT3":
                    case "ANT-UAT":
                        return Operation.MeasureH2_ANT3;
                }
            }
            if (IsTest(GetStr("Para.H3")))
            {
                switch (GetANTport())
                {
                    case "ANT1":
                        return Operation.MeasureH3_ANT1;
                    case "ANT2":
                        return Operation.MeasureH3_ANT2;
                    case "ANTU":
                    case "ANT3":
                    case "ANT-UAT":
                        return Operation.MeasureH3_ANT3;
                }
            }
            if (IsTest(GetStr("Para.Cpl")))
            {
                return Operation.MeasureCpl;
            }
            if (GetBand().Equals("B7") || GetBand().Equals("B30") && GetRXport().Contains("OUTMB1"))
                return Operation.VSAtoRX5;
            else if (GetRXport().Contains("1"))
                return Operation.VSAtoRX;
            else if (GetRXport().Contains("2"))
                return Operation.VSAtoRX2;
            else if (GetRXport().Contains("3"))
                return Operation.VSAtoRX3;
            else if (GetRXport().Contains("4"))
                return Operation.VSAtoRX4;

            return Operation.VSAtoRX;// default
        }
        public bool resetSA(string TestStr)
        {
            bool Result = true;
            //string str = GetStr(TestStr).ToUpper();
            //if (str.ToUpper() == "V")
            //    Result = true;
            return Result;
        }
        public string GetBand()
        {
            string band_tcf = GetStr("BAND").ToUpper();

            return band_tcf;
        }
        public string GetCABand()
        {
            string band_tcf = GetStr("RX BAND2").ToUpper();

            return band_tcf;
        }
        public string GetTXRX()
        {
            string TXRX = GetStr("TRX").ToUpper();

            return TXRX;
        }
        public string GetCktID()
        {
            if (GetTXRX().Contains("TX"))
                return "PT_";
            else
                return "PR_";
        }
        public string GetCplPrefix()
        {
            if (GetStr("CPL_Ctrl").ToUpper().Contains("REVERSE"))
                return "CplREV";
            else
                return "CplFWD";
        }
        public string GetVariable()
        {
            string Variable = GetStr("Variable").ToUpper();

            return Variable;
        }
        public string GetANTport()
        {
            string txPort_tcf = GetStr("Switch_ANT").ToUpper();

            if (txPort_tcf == "") txPort_tcf = "";  //hosein 05042020 txPort_tcf = "x"

            return txPort_tcf;
        }
        public string GetTXINport()
        {
            string txINPort_tcf = GetStr("Switch_TX").ToUpper();
            if (txINPort_tcf == "") txINPort_tcf = "x";
            return txINPort_tcf;
        }
        public string GetRXport()
        {
            string rxPort_tcf = GetStr("Switch_RX").ToUpper();

            if (rxPort_tcf == "") rxPort_tcf = "x";
            return rxPort_tcf;
        }
        public string GetINportHeader()
        {
            if (GetCktID().Equals("PT_"))
                return GetTXINport();
            else
                return GetANTport();

        }
        public string GetOUTportHeader()
        {
            if (GetCktID().Equals("PT_"))
                return GetANTport();
            else
                return GetRXportHeader();
        }
        public string GetRXportHeader()
        {
            string rxPort_tcf = GetStr("Switch_RX").ToUpper();
            //if (GetStr("RX_Output2").ToUpper().Contains("OUT"))
            //    rxPort_tcf = rxPort_tcf + "_" + GetStr("RX_Output2").ToUpper();
            return rxPort_tcf;

        }

        public float GetPout()
        {
            return (float)GetDbl("Pout");
        }
        public float GetPin()
        {
            // if(GetTXRX().Contains("TX"))
            return (float)GetDbl("Pin");
            // else
            //  return (float)GetDbl("Pin_RX");
        }
        public float GetExpectedGain()
        {
            return (float)GetDbl("ExpectedGain");
        }
        public double GetFreqSG()
        {
            //if(GetTXRX().Contains("TX"))
            return GetDbl("Freq");
            //else
            //    return GetDbl("Freq_RX");
        }
        public string GetModulationID()
        {
            string modulationStd = GetStr("Modulation").ToUpper();

            return modulationStd;
        }
        public string GetWaveformName()
        {
            string waveformName = GetStr("Waveform").ToUpper();

            //if (waveformName == "PDM") //KH 4/4/2016
            //{
            //    waveformName = "";
            //}

            return waveformName;
        }
        public string GetPowerMode()
        {
            string powerMode = GetStr("Power_Mode").ToUpper();

            return powerMode;
        }
        public string GetGainModeHeader()
        {
            string gainMode = GetStr("Gain Mode1").ToUpper();
            if (GetStr("Gain Mode2").ToUpper().Contains("G"))
                gainMode = gainMode + "_" + GetStr("Gain Mode2").ToUpper();

            return gainMode;
        }
        public string GetDacQ1()
        {
            string MipiDacBitQ1 = "";

            TestCond.TryGetValue("DACQ1", out MipiDacBitQ1);  // necessary only on MIPI products
            if (MipiDacBitQ1 != "")
            {
                int dcq1 = Convert.ToInt32(MipiDacBitQ1, 16);// / 2; used to divide by 2
                MipiDacBitQ1 = dcq1.ToString();
            }

            return MipiDacBitQ1;
        }
        public string GetDacQ2()
        {
            string MipiDacBitQ2 = "";

            TestCond.TryGetValue("DACQ2", out MipiDacBitQ2);  // necessary only on MIPI products
            if (MipiDacBitQ2 != "")
            {
                int dcq2 = Convert.ToInt32(MipiDacBitQ2, 16);// / 2;
                MipiDacBitQ2 = dcq2.ToString();
            }

            return MipiDacBitQ2;
        }
        public string GetDacForHeader()
        {
            string header = "";
            if (GetTXRX().Equals("TX"))
            {
                if (GetStr("TXDAQ1").ToUpper() == "X")
                {
                    header = "0x00_";
                }
                else
                {
                    header = "0x" + GetStr("TXDAQ1");
                }
                if (GetStr("TXDAQ2").ToUpper() == "X")
                {
                    header += "0x00";
                }
                else
                {
                    header = header + "_0x" + GetStr("TXDAQ2");
                }

            }
            else if (GetTXRX().Equals("TRX"))
            {
                string temp = GetStr("TXREG01");
                if (!temp.Equals("00"))
                    header = "DACQ1" + "x" + GetStr("TXREG01");
                temp = GetStr("RXREG07");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG07");
                temp = GetStr("RXREG08");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG08");
                temp = GetStr("RXREG09");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG09");
                temp = GetStr("RXREG0A");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0A");
                temp = GetStr("RXREG0B");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0B");
                temp = GetStr("RXREG0C");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0C");
                temp = GetStr("RXREG0D");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0D");
                temp = GetStr("RXREG0E");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0E");
                temp = GetStr("RXREG0F");
                if (!temp.Equals("00"))
                    header = header + "_" + "0x" + GetStr("RXREG0F");
            }
            else
            {
                int Count = 0;
                string temp = GetStr("N77LNA2");
                string Dummy = "_0x00";
                if (!temp.Equals("00") && !temp.Equals("0"))
                {
                    if (temp == "x")
                    {
                        temp = "00";
                        header = header + "0x" + temp; Count++;
                    }
                    else
                    {
                        header = header + "0x" + temp; Count++;
                    }
                }
                if (header != string.Empty) header += "_";
                temp = GetStr("N77LNA1");
                if (!temp.Equals("00") && !temp.Equals("0"))
                {
                    if (temp == "x")
                    {
                        temp = "00";
                        header = header + "0x" + temp; Count++;
                    }
                    else
                    {
                        header = header + "0x" + temp; Count++;
                    }
                }
                if (header != string.Empty) header += "_";
                temp = GetStr("N79LNA2");
                if (!temp.Equals("00") && !temp.Equals("0"))
                {
                    if (temp == "x")
                    {
                        temp = "00";
                        header = header + "0x" + temp; Count++;
                    }
                    else
                    {
                        header = header + "0x" + temp; Count++;
                    }
                }

                temp = GetStr("N79LNA1");
                if (!temp.Equals("00") && !temp.Equals("0"))
                {
                    if (temp == "x")
                    {
                        temp = "00";
                        header = header + "0x" + temp; Count++;
                    }
                    else
                    {
                        header = header + "0x" + temp; Count++;
                    }
                }

                temp = GetStr("RXLNA5");
                if (!temp.Equals("00") && !temp.Equals("0") && temp!=string.Empty)
                {
                    if (temp == "x")
                    {
                        temp = "00";
                        header = header + "0x" + temp; Count++;
                    }
                    else
                    {
                        header = header + "_0x00" + temp; Count++;  //_ added by Hossein then removed 04272020
                    }
                }

                if (Count == 1) { header = header + Dummy; }
            }
            if (header.StartsWith("_"))
                header = header.TrimStart('_');

            if (header == "")
            {
                header = "0x00_0x00";
            }
            return header;
        }
        public string GetRegcustom()
        {
            string Regcustom = GetStr("REGCUSTOM").ToUpper();

            return Regcustom;
        }
        public bool GetPoutEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Pout", GetStr("Para.Pout"), SpecNumber);
        }
        public bool GetPinEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Pin", GetStr("Para.Pin"), SpecNumber);
        }
        public bool GetGainEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Gain", GetStr("Para.Gain"), SpecNumber);
        }
        public bool GetReadReg1CEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ReadReg1C", GetStr("Para.ReadReg1C"), SpecNumber);
        }
        //keng shan Added
        public bool GetGainLinearityEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.GainLinearity", GetStr("Para.GainLinearity"), SpecNumber);
        }
        //keng shan Added
        public bool GetPoutDropEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.PowerDrop", GetStr("Para.PowerDrop"), SpecNumber);
        }

        public bool GetIccEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Icc", GetStr("Para.Icc"), SpecNumber);
        }

        public bool GetIcc2Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Icc2", GetStr("Para.Icc2"), SpecNumber);
        }

        public bool GetIbattEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Ibatt", GetStr("Para.Ibatt"), SpecNumber);
        }
        public bool GetWattEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Watt", GetStr("Para.Watt"), SpecNumber);
        }

        public bool GetIddEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Idd", GetStr("Para.Idd"), SpecNumber);
        }
        public bool GetItotalEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ISum", GetStr("Para.ISum"), SpecNumber);
        }
        public bool GetISdata1Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ISdata1", GetStr("Para.ISdata1"), SpecNumber);
        }
        public bool GetISclk1Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ISclk1", GetStr("Para.ISclk1"), SpecNumber);
        }
        public bool GetIio1Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Iio1", GetStr("Para.Iio1"), SpecNumber);
        }
        public bool GetISdata2Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ISdata2", GetStr("Para.ISdata2"), SpecNumber);
        }
        public bool GetISclk2Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ISclk2", GetStr("Para.ISclk2"), SpecNumber);
        }
        public bool GetIio2Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Iio2", GetStr("Para.Iio2"), SpecNumber);
        }
        public bool GetIeffEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Ieff", GetStr("Para.Ieff"), SpecNumber);
        }
        public bool GetPconEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.Pcon", GetStr("Para.Pcon"), SpecNumber);
        }
        public bool GetPaeEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.PAE", GetStr("Para.PAE"), SpecNumber);
        }

        public bool GetAcp1Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ACLR1", GetStr("Para.ACLR1"), SpecNumber);
        }
        public bool GetAcp2Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.ACLR2", GetStr("Para.ACLR2"), SpecNumber);
        }

        public bool GetEUTRAEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.EUTRA", GetStr("Para.EUTRA"), SpecNumber);
        }


        public bool GetEvmEnabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.EVM", GetStr("Para.EVM"), SpecNumber);
        }

        public bool GetIIP3Enabled(Dictionary<string, string> SpecNumber)
        {
            return IsTest("Para.IIP3", GetStr("Para.IIP3"), SpecNumber);
        }

        public bool GetVIO32MAEnabled()
        {
            return IsTest(GetStr("VIO32MA"));
        }

        public bool GetVIORESETEnabled()
        {
            return IsTest(GetStr("VIORESET"));
        }

        public int GetAcpAverages()
        {
            return (int)GetDbl("Avg.ACLR");
        }

        public int GetSoakDelay()
        {
            return (int)GetDbl("SOAK_Delay(ms)");
        }

        public string GetDcPinForIccCal()
        {
            return "Vcc";
        }

        public float GetPinSweepStart()
        {
            try
            {
                //if (GetTXRX().Equals("TX"))
                {
                    string[] pinRange = TestCond["Pin"].Split(':').ToArray();
                    return Convert.ToSingle(pinRange[0]);
                }
                //else
                //{
                //    string[] pinRange = TestCond["Pin_RX"].Split(':').ToArray();
                //    return Convert.ToSingle(pinRange[0]);
                //}
            }
            catch (Exception e)
            {
                throw new Exception("TCF Pin column was not formatted correctly for Pin Sweep.\nCorrect format example: -20:3\n    which sweeps from -20dBm to 3dBm\nMax sweep range is 30dB\n\n" + e.ToString());
            }
        }

        public float GetPinSweepStop()
        {
            try
            {
                //if (GetTXRX().Equals("TX"))
                {
                    string[] pinRange = TestCond["Pin"].Split(':').ToArray();
                    return Convert.ToSingle(pinRange[1]);
                }
                //else
                //{
                //    string[] pinRange = TestCond["Pin_RX"].Split(':').ToArray();
                //    return Convert.ToSingle(pinRange[1]);
                //}
            }
            catch (Exception e)
            {
                throw new Exception("TCF Pin column was not formatted correctly for Pin Sweep.\nCorrect format example: -20:3\n    which sweeps from -20dBm to 3dBm\nMax sweep range is 30dB\n\n" + e.ToString());
            }
        }

        public string GetCustomVariable1()
        {
            string CustomVariable1 = GetStr("CustomVariable1").ToUpper();

            return CustomVariable1;
        }

        public string GetCustomVariable2()
        {
            string CustomVariable2 = GetStr("CustomVariable2").ToUpper();

            return CustomVariable2;
        }

        public string GetCustomVariable3()
        {
            string CustomVariable3 = GetStr("CustomVariable3").ToUpper();

            return CustomVariable3;
        }

        public double GetHarm2MeasBW()
        {
            return GetDbl("Harm2_measBW");
        }

        public Dictionary<string, TestLib.DPAT_Variable> GetDPAT()
        {
            string[] DPAT = TestCond.ContainsKey("DPAT") ? GetStr("DPAT").Split(',') : new string[1] { "" };

            Dictionary<string, TestLib.DPAT_Variable> Dic = new Dictionary<string, TestLib.DPAT_Variable>();

            if (DPAT[0] != "")
            {
                for (int i = 0; i < DPAT.Length; i++)
                {
                    string[] StringA = DPAT[i].Split('(');
                    if (StringA.Length < 2 || StringA.Length > 2) MessageBox.Show("Please Check the parameter name for DPAT");
                    else
                    {
                        StringA[1] = StringA[1].Replace(")", string.Empty);

                        TestLib.DPAT_Variable Value = new TestLib.DPAT_Variable();
                        Value.Fomula = StringA[1].Split('.')[0];
                        Value.SpecCondition = StringA[1].Split('.')[1];
                        Value.SetValue = StringA[1].Split('.')[2];

                        Dic.Add(StringA[0], Value);
                    }
                }
            }

            return Dic;
        }

        public Dictionary.Ordered<string, DcSetting> GetDcSettings()
        {
            Dictionary.Ordered<string, DcSetting> dcSettings = new Dictionary.Ordered<string, DcSetting>();

            foreach (string dcPinName in DcResourceTempList)
            {
                DcSetting settings = new DcSetting();

                settings.Volts = GetDbl("V." + dcPinName);
                settings.Current = GetDbl("I." + dcPinName);

                if (dcPinName.StartsWith("V"))
                {
                    settings.iParaName = "I" + dcPinName.Substring(1);
                }
                else
                {
                    settings.iParaName = "I" + dcPinName;
                }
                settings.Test = IsTest(GetStr("Para." + settings.iParaName));

                if (TestCond.ContainsKey("Avg." + settings.iParaName) && IsExist("Avg." + settings.iParaName))
                    settings.Avgs = (int)GetDbl("Avg." + settings.iParaName);
                else
                    settings.Avgs = 1;

                dcSettings.Add(dcPinName, settings);
            }

            return dcSettings;
        }

        public string GetParameterNote()
        {
            string GetParameterNote = GetStr("ParameterNote").ToUpper();

            return GetParameterNote;
        }

        private Dictionary.Ordered<string, string[]> GetDcResourceDefinitions(TcfSheetReader reader)
        {
            Dictionary.Ordered<string, string[]> dcResourceList =
                new Dictionary.Ordered<string, string[]>();

            for (int col = 0; col < reader.Header.Count; col++)
            {
                string head = reader.Header[col];

                if (head.ToUpper().StartsWith("V."))
                {
                    string dcPinName = head.Replace("V.", "");

                    dcResourceList[dcPinName] = new string[Eq.NumSites];

                    for (byte site = 0; site < Eq.NumSites; site++)
                    {
                        dcResourceList[dcPinName][site] =
                            reader.allContents.Item3[reader.headerRow - 1 - site, col].Trim();
                    }
                }
            }

            return dcResourceList;
        }


        public string AppendParamNoteToParamName(string paramName)
        {
            string paramNote = GetStr("ParameterNote").ToUpper();

            if (paramNote != "") paramName += paramNote + "_";

            return paramName;
        }

        public string AppendCouplerInfo(string paramName)
        {

            return paramName;
        }
    }
}