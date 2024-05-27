using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using TestPlanCommon.SParaModel;

namespace TestPlanCommon.CommonModel
{
    public class TcfHeaderGenerator
    {
        private SortedDictionary<int, string> HeaderEntries;
        string Header = "";
        string RXHeader = "";
        string DriveBias = "";
        string MainBias = "";
        private string Biasheader = "";

        string Biasheader_MB = "";
        string Biasheader_HB = "";

        string Vbattval = "";
        string VbiasTval = "";
        string Vcplval = "";
        string VccValue = "";
        string VddValue = "";

        private SParaEnaTestConditionReader m_reader;

        public void SetHeaderEntries(SortedDictionary<int, string> tcfHeaderOrder)
        {
            HeaderEntries = tcfHeaderOrder;
        }

        public void SetCurrentTestCondition(Dictionary<string, string> TestCond)
        {
            m_reader = new SParaEnaTestConditionReader(TestCond);
        }

        public void SetDc(string zTxReg1, string zTxReg2, string zRxreg)
        {
            DriveBias = zTxReg1;
            MainBias = zTxReg2;
            Biasheader = zRxreg;
        }

        public void SetDc2(string zVbatt, string zVbiasT,
            string zVcpl, string zVcc, string zVdd, string zDriveBias)
        {
            Vbattval = zVbatt;
            VbiasTval = zVbiasT;
            Vcplval = zVcpl;
            VccValue = zVcc;
            VddValue = zVdd;
            DriveBias = zDriveBias;
        }

        public void ResetHeader()
        {
            DriveBias = Biasheader_HB; MainBias = Biasheader_MB; Header = RXHeader;
        }

        public string GetJHeader1()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                            + "-" + m_reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                            + "_" + m_reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                            + "_" + m_reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                                          // Mode
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                            + "_" + VccValue + "V"                                                                                      // Vcc
                            + "_" + VddValue + "Vdd"                                                                                    // Vdd
                            + "_CH" + Convert.ToString(m_reader.ReadTcfDataInt("Channel Number"))                                                //Channel
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                              // Sparameter
                            + "_" + Convert.ToString(m_reader.GetFrequency("Start_Freq")) + "MHz"                    // Start Freq
                            + "_" + Convert.ToString(m_reader.GetFrequency("Stop_Freq")) + "MHz"
                            + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                            + "_" + Biasheader
                            + "_NOTE_" + m_reader.ReadTcfData("Para.Spec");
            return j;
        }

        public string GetJHeader2a()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                            + "_" + m_reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                            + "_" + m_reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                                          // Mode
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                            + "_" + VccValue + "V"                                                                                      // Vcc
                            + "_" + VddValue + "Vdd"                                                                                    // Vdd
                            + "_CH" + Convert.ToString(m_reader.ReadTcfDataInt("Channel Number"))                                                //Channel
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                   // Sparameter
                            + "_" + convertMhz(m_reader.ReadTcfDataDouble("Target_Freq")) + "MHz"                           // Start Freq
                            + "_x"
                            + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                            + "_" + Biasheader
                            + "_NOTE_" + m_reader.ReadTcfData("Para.Spec");
            return j;
        }

        public string GetJHeader2b()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                            + "_" + m_reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                            + "_" + m_reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                                          // Mode
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                            + "_" + VccValue + "V"                                                                                      // Vcc
                            + "_" + VddValue + "Vdd"                                                                                    // Vdd
                            + "_CH" + Convert.ToString(m_reader.ReadTcfDataInt("Channel Number"))                                                //Channel
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                   // Sparameter
                            + "_" + convertMhz(m_reader.ReadTcfDataDouble("Target_Freq")) + "MHz"                           // Start Freq
                            + "_x"
                            + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                            + "_" + Biasheader;
            return j;
        }

        public string GetJHeader3()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                            // Parameter Class
                            + "_" + m_reader.ReadTcfData("BAND").Split('-')[0]                                              // Band
                            + "_" + m_reader.ReadTcfData("Switch_In")                                                                   // Meas_Port1.
                            + "_" + m_reader.ReadTcfData("Switch_ANT")                                                                   // Meas_Port1.
                            + "_" + m_reader.ReadTcfData("Switch_Out")                                                                  // Meas_Port2
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                                      // Mode.
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                                       // Gain Mode
                            + "_" + VccValue + "V"                                                                                  // Vcc
                            + "_" + VddValue + "Vdd"                                                                                // Vdd
                            + "_CH" + m_reader.ReadTcfData("Channel Number")                                                              // Channel
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                                 // Sparameter
                            + "_" + ((m_reader.ReadTcfData("Start_Freq")).Split(' ')[0]) + "MHz"                                        // Start Freq
                            + "_" + ((m_reader.ReadTcfData("Stop_Freq")).Split(' ')[0]) + "MHz"                                         // Stop Freq

                            + "_0x" + DriveBias + "_0x" + MainBias                                                                  // DAC1, DAC2

                            + "_" + Biasheader
                            + "_NOTE_" + m_reader.ReadTcfData("Para.Spec");
            return j;
        }

        public string GetJHeader3b()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("BAND")                                                // Band
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                          // Mode
                            + "_" + DriveBias + "_" + MainBias                                                          // DAC1, DAC2
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                         // Gain Mode
                            + "_" + Header + "V"                                                                        // Vcc
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                     // Sparameter
                            + "_" + ((m_reader.ReadTcfData("Start_Freq")).Split(' ')[0])                                    // Start Freq
                            + "_" + ((m_reader.ReadTcfData("Stop_Freq")).Split(' ')[0])                                     // Stop Freq
                            + "_" + "x";                                                                                 // Antenna
            return j;
        }

        public string GetJHeader4()
        {
            string pw = m_reader.ReadTcfData("Power_Mode");
            string pw2 = (pw.Contains("_") ? pw.Replace("_", "to") : pw); // Gain Mode
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("BAND")                                                  // Band
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                          // Mode
                            + "_" + DriveBias + "_" + MainBias                                                          // DAC1, DAC2
                            + "_" + pw2
                            + "_" + Header + "V"                                                                        // Vcc
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                     // Sparameter
                            + "_" + ((m_reader.ReadTcfData("Start_Freq")).Split(' ')[0])                                    // Start Freq
                            + "_" + ((m_reader.ReadTcfData("Stop_Freq")).Split(' ')[0])                                     // Stop Freq
                            + "_" + "x"                                                                                 // Antenna
                            + (m_reader.ReadTcfData("Selected_Port") == "" ? "x" : m_reader.ReadTcfData("Selected_Port"));   // RX Path                                                                                 // Antenna
            return j;
        }

        public string GetJHeader4b()
        {
            string pw = m_reader.ReadTcfData("Power_Mode");
            string pw2 = (pw.Contains("_") ? pw.Replace("_", "to") : pw); // Gain Mode
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("BAND")                                                  // Band
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                          // Mode
                            + "_" + DriveBias + "_" + MainBias                                                          // DAC1, DAC2
                            + "_" + pw2
                            + "_" + Header + "V"                                                                        // Vcc
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                     // Sparameter
                            + "_" + ((m_reader.ReadTcfData("Start_Freq")).Split(' ')[0])                                    // Start Freq
                            + "_" + ((m_reader.ReadTcfData("Stop_Freq")).Split(' ')[0])                                     // Stop Freq
                            + "_" + "x";                                                                                 // Antenna
            return j;
        }

        public string GetJHeaderNf1()
        {
            string j = "F_" + m_reader.ReadTcfData("N-Parameter-Class")                                                 // Parameter Class
                            + "_" + m_reader.ReadTcfData("BAND")                                                 // Band
                            + "_" + m_reader.ReadTcfData("N_Meas_Port1 (Sxy)")                                              // Meas_Port1
                            + "_" + m_reader.ReadTcfData("N_Meas_Port2 (Sxy)")                                              // Meas_Port2
                            + "_" + m_reader.ReadTcfData("N_Mode")                                                          // Mode
                            + "_" + DriveBias + "_" + MainBias                                                          // DAC1, DAC2
                            + "_" + m_reader.ReadTcfData("Power_Mode")                                          // Gain Mode
                            + "_" + Header + "V"                                                                        // Vcc
                            + "_" + m_reader.ReadTcfData("S-Parameter")                                                     // Sparameter
                            + "_" + ((m_reader.ReadTcfData("Start_Freq")).Split(' ')[0])                                    // Start Freq
                            + "_" + ((m_reader.ReadTcfData("Stop_Freq")).Split(' ')[0])                                     // Stop Freq
                            + "_" + "x"                                                                                 // Antenna
                            + "_" + (m_reader.ReadTcfData("Selected_Port") == "" ? "x" : m_reader.ReadTcfData("Selected_Port"));   // RX Path                                                                               // Antenna
            return j;
        }

        public string HearderName(string Reg07, string Reg08, string Reg09, string Reg0A, string Reg0B, string Reg0C, string Reg0D, string Reg0E, string Reg0F)
        {
            string SparaHearder = "";

            if (Reg07 != "00")
            {
                SparaHearder += "0x" + Reg07 + "_";
            }
            if (Reg08 != "00")
            {
                SparaHearder += "0x" + Reg08 + "_";
            }
            if (Reg09 != "00")
            {
                SparaHearder += "0x" + Reg09 + "_";
            }
            if (Reg0A != "00")
            {
                SparaHearder += "0x" + Reg0A + "_";
            }
            if (Reg0B != "00")
            {
                SparaHearder += "0x" + Reg0B + "_";
            }
            if (Reg0C != "00")
            {
                SparaHearder += "0x" + Reg0C + "_";
            }
            if (Reg0D != "00")
            {
                SparaHearder += "0x" + Reg0D + "_";
            }
            if (Reg0E != "00")
            {
                SparaHearder += "0x" + Reg0E + "_";
            }
            if (Reg0F != "00")
            {
                SparaHearder += "0x" + Reg0F + "_";
            }



            if (SparaHearder == "")
            {
                SparaHearder = "0x00";

            }
            else
            {
                if (SparaHearder.Substring(SparaHearder.Length - 1, 1) == "_")
                {
                    SparaHearder = SparaHearder.Substring(0, SparaHearder.Length - 1);
                }
            }
            return SparaHearder;
        }

        public string HearderName(string[] listReg)
        {
            string SparaHearder = "";

            foreach (string tempRegKey in listReg)
            {
                string TempRegValue = m_reader.ReadTcfData(tempRegKey);
                if (TempRegValue != "00" && TempRegValue != "0") SparaHearder += "0x" + TempRegValue + "-";
            }

            if (SparaHearder != "")
                SparaHearder = SparaHearder.Remove(SparaHearder.Length - 1);
            else
                SparaHearder = "x";

            if (SparaHearder.Contains("0x0")) System.Diagnostics.Debugger.Break();

            return SparaHearder;
        }

        public string HearderName(string[] listReg, bool removeblanks)
        {
            string SparaHearder = "";
            List<string> content = new List<string>();

            foreach (string tempRegKey in listReg)
            {
                string TempRegValue = m_reader.ReadTcfData(tempRegKey);

                if (TempRegValue != "00" && TempRegValue != "0" && TempRegValue != "") 
                {
                    content.Add("0x" + TempRegValue);
                }
            }

            return SparaHearder = (content.Count != 0 ? String.Join("_", content.ToArray()) : "");
            
        }

        public string MunchDcHeader()
        {
            return Munch(Header, RXHeader, MainBias, Biasheader);
        }

        public string Munch(string zVcpl, string zVBatt, string zVBiast)
        {
            return Munch(Header, RXHeader, zVBatt, zVBiast, zVcpl, MainBias, Biasheader);
        }

        /// <summary>
        /// Pinot Joker variant.
        /// </summary>
        public string MunchDcHeader2()
        {
            string zfinalheader = "";
            List<string> content  = new List<string>();

            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                switch (x.Value)
                {
                    case "CUSTOM_REG":
                        // DriveBias = TXREG0B, MainBias = TXREG0C, Biasheader = list of cProject.Reg - already has 0x prefix.
                        string v1 = string.Format("0x{0}_0x{1}_{2}", DriveBias, MainBias, Biasheader);
                        content.Add(v1);
                        break;
                    default:
                        string headerEntry = m_reader.ReadTcfData(x.Value);
                        string zmunchedval = HeaderValueMuncher(headerEntry, x.Key, x.Value);
                        if (headerEntry == "") { continue; }
                        content.Add(zmunchedval);
                        break;
                }
            }

            char[] burntends =  new char[1] {'_'};
            zfinalheader = (String.Join("_", content.ToArray())).Trim(burntends);

            return zfinalheader;
        }

        /// <summary>
        /// Replace Start Stop Freq with Target Freq.
        /// </summary>
        /// <param name="targetFreq"></param>
        /// <returns></returns>
        public string MunchDcHeaderMagAt(double targetFreq)
        {
            string zfinalheader = "";
            List<string> content = new List<string>();

            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                switch (x.Value)
                {
                    case "CUSTOM_REG":
                        // DriveBias = TXREG0B, MainBias = TXREG0C, Biasheader = list of cProject.Reg - already has 0x prefix.
                        // There is a x_ prefix for Pinot Joker, to ease splitting of reg columns. Only for MunchDcHeaderMagAt not for MunchDcHeader2!
                        string v1 = (string.Format("x_0x{0}_0x{1}_{2}", DriveBias, MainBias, Biasheader));
                        content.Add(v1);
                        break;
                    //case "Start_Freq":
                    //    string f1 = convertMhz(targetFreq) + "MHz";
                    //    content.Add(f1);
                    //    break;
                    //case "Stop_Freq":
                    //    break;
                    default:
                        string headerEntry = m_reader.ReadTcfData(x.Value);
                        string zmunchedval = HeaderValueMuncher(headerEntry, x.Key, x.Value);
                        if (headerEntry == "") { continue; }
                        content.Add(zmunchedval);
                        break;
                }
            }

            zfinalheader = (String.Join("_", content.ToArray())).Trim('_');

            return zfinalheader;
        }

        private string Munch(string zVcc, string zVdd, string zTxreg, string zRxreg)
        {
            string zfinalheader = "";
            string zmunchedval = "";
            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                string headerEntry = m_reader.ReadTcfData(x.Value, true);
                zmunchedval = HeaderValueMuncher(headerEntry, x.Key, x.Value);
                if (headerEntry == "") { continue; }
                zfinalheader += zmunchedval + "_";
            }
            zfinalheader += string.Format("{0}Vcc_{1}Vlna_DACQ1x{2}_{3}", 
                zVcc, zVdd, zTxreg, zRxreg);
            return zfinalheader;
        }

        public string Munch(string zVcc, string zVdd, string zVBatt, string zVBiast, string zVcpl, string zTxreg, string zRxreg)
        {
            string zfinalheader = "";
            string zmunchedval = "";
            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                string headerEntry = m_reader.ReadTcfData(x.Value, true);
                zmunchedval = HeaderValueMuncher(headerEntry, x.Key, x.Value);
                if (headerEntry == "") { continue; }
                zfinalheader += zmunchedval + "_";
            }
            zfinalheader += string.Format("{0}Vcc_{1}Vlna_{2}Vbatt_{3}Vbiast_{4}Vcpl_DACQ1x{5}_{6}", 
                zVcc, zVdd, zVBatt, zVBiast, zVcpl, zTxreg, zRxreg);
            return zfinalheader;
        }

        public string SNPHeaderMuncher()
        {

            string zfinalheader = "";
            string zmunchedval = "";

            string a = HeaderValueMuncher(m_reader.ReadTcfData("Channel Number"), 1, "Channel Number");
            string[] zorder_header = new string[] { "Channel Number", "TUNABLE_BAND", "Switch_ANT", "Selected_Port", "Power_Mode", "S-Parameter" };



            foreach (string x in zorder_header)
            {
                string headerEntry = m_reader.ReadTcfData(x);

                zmunchedval = HeaderValueMuncher(headerEntry, 0, x);
                if (headerEntry == "") { continue; }
                zfinalheader += zmunchedval + "_";
            }

            return zfinalheader;
        }

        // Temporary hook.
        public string ReadTcfData(string key)
        {
            return m_reader.ReadTcfData(key);
        }

        private string HeaderValueMuncher(string TestConditionValue, int HeaderEntryIndex, string zCats)
        {
            string retval = TestConditionValue;

            if (TestConditionValue != "")
            {
                #region Saved for reference
                switch (zCats)
                {
                    case "Test Mode":
                        switch (TestConditionValue)
                        {
                            case "FBAR":
                            case "DC":
                            case "COMMON":
                                retval = "F";
                                break;
                                //case "DC":
                                //    retval = "DC";
                                //    break;
                                //case "COMMON":
                                //    retval = "COM";
                                //    break;
                        }

                        break;
                    case "Channel Number":
                        retval = "CH" + TestConditionValue;
                        break;
                    case "N-Parameter-Class":
                        // This line is not GE compliant.
                        //retval = retval.Replace("-", "_");
                        break;
                    case "N_Mode":
                        //nothing to be done here yet
                        break;
                    case "Temp":
                        //nothing to be done here yet
                        break;
                    case "TUNABLE_BAND":
                        //nothing to be done here yet
                        break;
                    case "Selected_Port":
                        //nothing to be done here yet
                        break;
                    case "Power_Mode":

                        break;
                    case "S-Parameter":

                        break;
                    case "Start_Freq":
                    case "Stop_Freq":
                        retval = retval.Replace(" ", "") + "Hz";
                        break;
                    case "Switch_ANT":

                        break;
                    case "Modulation":

                        break;
                    case "Waveform":

                        break;
                    case "Pout":
                        retval = TestConditionValue + "dBm";
                        break;
                    case "Freq":
                        retval = TestConditionValue + "MHz";
                        break;
                    case "ParameterNote":
                        retval = (TestConditionValue != "" ? "_NOTE_" + TestConditionValue : "");
                        break;
                    case "CPL_Ctrl":
                        retval = (TestConditionValue != "" ? TestConditionValue : "");
                        break;
                    case "V_CH1":
                        retval = TestConditionValue + "V";
                        break;
                    case "V_CH3":
                        retval = TestConditionValue + "Vdd";
                        break;
                    case "Para.Spec":
                        // Case HLS2 Joker.
                        //retval = "_NOTE_" + TestConditionValue;
                        retval = "NOTE_" + TestConditionValue;
                        break;
                }
                #endregion Saved for reference
            }

            return retval;
        }

        private double convertMhz(double input)
        {
            return ((input / Math.Pow(10, 6)));//Math.Round
        }

    }
}