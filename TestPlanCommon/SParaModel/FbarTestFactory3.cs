using System.Collections.Generic;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using EqLib;
using LibFBAR_TOPAZ;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.ANewTestLib;
using MPAD_TestTimer;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.SParaModel
{
    public class FbarTestFactory3
    {
        private TcfSpeedLoader m_stub1;

        /// <summary>
        /// Set in this class, passed to all Trigger tests.
        /// </summary>
        public DataTriggeredDataModel DataTriggered2 { get; set; }

        public FbarTestFactory3()
        {
            DataTriggered2 = new DataTriggeredDataModel();
            m_stub1 = new TcfSpeedLoader();
        }

        public void Initialize(string[,] sheetCondFbar)
        {
            // DC and FBAR test needs the Test parameters.
            Test_Parameters = Load_TestParameterHeader(sheetCondFbar);
            m_cacheRowAboveTp = GetRowAboveTestParameterRow(sheetCondFbar, Test_Parameters);
        }

        public SparamTrigger Load_TestConditionTrigger(string tmpTestCondition, 
            int TestCnt, SParaEnaTestConditionReader reader)
        {
            SparamTrigger tt = null;

            switch (tmpTestCondition.ToUpper())
            {
                case "TRIGGER":

                    #region "Trigger"

                    tt = new SparamTrigger();
                    tt.TestNo = TestCnt;
                    // FBAR.TestClass[TestCnt].Trigger.ChannelNumber = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Channel Number"]));
                    tt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    //FBAR.TestClass[TestCnt].Trigger.Sleep_ms = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Sleep (Wait - ms)"]));
                    tt.Sleep_ms = reader.ReadTcfDataInt("Sleep (Wait - ms)");
                    //FBAR.TestClass[TestCnt].Trigger.Misc_Settings = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Misc"]);
                    tt.Misc_Settings = reader.ReadTcfData("Misc");
                    tt.SwitchIn = reader.ReadTcfData("Switch_In");
                    tt.SwitchAnt = reader.ReadTcfData("Switch_ANT");
                    tt.SwitchOut = reader.ReadTcfData("Switch_Out");
                    tt.Band = reader.ReadTcfData("BAND");
                    tt.ParameterNote = reader.ReadTcfData("ParameterNote");

                    // Case HLS2.
                    //tt.Select_RX = reader.ReadTcfData("Selected_Port");
                    //SetTrigger(TestCnt);
                    // FBAR.TestClass[TestCnt].Trigger.SnPFile_Name  = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Search_Method"]);
                    tt.SnPFile_Name = reader.ReadTcfData("Search_Method");

                    if (Test_Parameters.ContainsKey("Power_Mode"))
                    {
                        //FBAR.TestClass[TestCnt].Trigger.FileOutput_Mode = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Power_Mode"]);
                        tt.FileOutput_Mode = reader.ReadTcfData("Power_Mode");
                    }

                    break;

                #endregion "Trigger"

                case "TRIGGER_NF":
                case "TRIGGER_NF_DUAL":

                    #region "Trigger NF"

                    SparamTrigger ttNf = new SparamTrigger();
                    ttNf.TestNo = TestCnt;
                    // FBAR.TestClass[TestCnt].Trigger.ChannelNumber = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Channel Number"]));

                    // For NF Dual band testing //START, yoonchun 20161214
                    if (reader.ReadTcfData("Channel Number").Contains(","))
                    {
                        string[] CH = reader.ReadTcfData("Channel Number").Trim().Replace(" ", "").Split(',');

                        ttNf.ChannelNumber = int.Parse(CH[0]);

                        ttNf.MasterChannel = int.Parse(CH[0]);
                        ttNf.SlaveChannel = int.Parse(CH[1]);
                    }
                    else
                    {
                        ttNf.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    }
                    // For NF Dual band testing //END

                    //FBAR.TestClass[TestCnt].Trigger.Sleep_ms = int.Parse(cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Sleep (Wait - ms)"]));
                    ttNf.Sleep_ms = reader.ReadTcfDataInt("Sleep (Wait - ms)");
                    //FBAR.TestClass[TestCnt].Trigger.Misc_Settings = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Misc"]);
                    ttNf.Misc_Settings = reader.ReadTcfData("Misc");
                    //SetTrigger(TestCnt);
                    // FBAR.TestClass[TestCnt].Trigger.SnPFile_Name  = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Search_Method"]);

                    // For NF Dual band testing //START, yoonchun 20161214
                    if (reader.ReadTcfData("BAND").Contains(","))
                    {
                        string[] band_Array = reader.ReadTcfData("BAND").Trim().Replace(" ", "").Split(',');
                        ttNf.Band = band_Array[0];

                        ttNf.Master_Band = band_Array[0];
                        ttNf.Slave_Band = band_Array[1];
                    }
                    else
                    {
                        //FBAR.TestClass[TestCnt].Trigger.SwitchIn = reader.ReadTcfData("Switch_In");
                        ttNf.Band = reader.ReadTcfData("BAND");
                    }

                    // Case Joker
                    ttNf.SwitchIn = reader.ReadTcfData("Switch_In");
                    ttNf.SwitchAnt = reader.ReadTcfData("Switch_ANT");
                    ttNf.SwitchOut = reader.ReadTcfData("Switch_Out");

                    // Case HLS2
                    //if (reader.ReadTcfData("Selected_Port").Contains(","))
                    //{
                    //    string[] selectedPort_Array = reader.ReadTcfData("Selected_Port").Trim().Replace(" ", "").Split(',');
                    //    ttNf.Select_RX = selectedPort_Array[0];

                    //    ttNf.Master_RX = selectedPort_Array[0];
                    //    ttNf.Slave_RX = selectedPort_Array[1];
                    //}
                    //else
                    //{
                    //    ttNf.Select_RX = reader.ReadTcfData("Selected_Port");
                    //}
                    // For NF Dual band testing //END

                    ttNf.SnPFile_Name = reader.ReadTcfData("Search_Method");

                    if (Test_Parameters.ContainsKey("Power_Mode"))
                    {
                        //FBAR.TestClass[TestCnt].Trigger.FileOutput_Mode = cExtract.Get_Data(TestCondition, iRow, Test_Parameters["Power_Mode"]);
                        ttNf.FileOutput_Mode = reader.ReadTcfData("Power_Mode");
                    }

                    tt = ttNf;
                    break;

                #endregion "Trigger NF"

                default:
                    break;
            }

            return tt;
        }

        public TestCaseBase Load_TestConditionMeasure(string tmpTestCondition, int TestCnt, UsePreviousTcfModel upm, TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            TestCaseBase tcb = null;

            switch (tmpTestCondition.ToUpper())
            {
                case "NF_FREQ_AT":

                    #region "NF_Freq_AT"

                    cNF_Topaz_At ttNfTopazAt = new cNF_Topaz_At();
                    ttNfTopazAt.TestNo = TestCnt;
                    ttNfTopazAt.Band = reader.ReadTcfData("BAND");
                    ttNfTopazAt.PowerMode = reader.ReadTcfData("Power_Mode");
                    // Case HLS2.
                    //ttNfTopazAt.Selected_Port = reader.ReadTcfData("Selected_Port");
                    ttNfTopazAt.Selected_Port = reader.ReadTcfData("Switch_Out");

                    ttNfTopazAt.Frequency = reader.ReadTcfData("Target_Freq");
                    ttNfTopazAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttNfTopazAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) ttNfTopazAt.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    ttNfTopazAt.Interpolation = reader.ReadTcfData("Interpolation");
                    ttNfTopazAt.Offset = reader.ReadTcfDataDouble2("Offset");
                    //if (upm.b_Use_Previous) ttNfTopazAt.Previous_TestNo = upm.Previous_TestNo;
                    //else ttNfTopazAt.Previous_TestNo = -1; //Prevent Error

                    //TODO Case HLS2: Handle for Joker.
                    ttNfTopazAt.Header = modelHm.MunchDcHeaderMagAt(ttNfTopazAt.Frequency_At);
                    //FBAR.TestClass[TestCnt].NF_Topaz_At.Header =
                    //    HeaderMuncher(TestCond, Header, RXHeader, MainBias, Biasheader);

                    //FBAR.TestClass[TestCnt].NF_Topaz_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].NF_Topaz_At.ChannelNumber)                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].NF_Topaz_At.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].NF_Topaz_At.Frequency_At) + "MHz"                           // Start Freq
                    //                   + "_x"
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                                // LNA DAC
                    tcb = ttNfTopazAt;
                    break;

                #endregion "NF_Freq_AT"

                case "MAG_AT":

                    #region "Mag_AT"

                    cMag_At ttMagAt = new cMag_At();
                    //ttMagAt.TestNo = TestCnt;
                    //ttMagAt.SwitchIn = reader.ReadTcfData("Switch_In");
                    //ttMagAt.SwitchOut = reader.ReadTcfData("Switch_Out");
                    //ttMagAt.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttMagAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttMagAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) ttMagAt.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    ttMagAt.Interpolation = reader.ReadTcfData("Interpolation");
                    ttMagAt.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    //ttMagAt.Offset = reader.ReadTcfDataDouble2("Offset");
                    if (upm.b_Use_Previous) ttMagAt.Previous_Test = upm.Previous_Test;
                    //if (upm.b_Use_Previous) ttMagAt.Previous_TestNo = upm.Previous_TestNo;
                    //else ttMagAt.Previous_TestNo = -1; //Prevent Error

                    //TODO Case HLS2: Handle for Joker.
                    //ttMagAt.Selected_Port = reader.ReadTcfData("Selected_Port");
                    ttMagAt.Header = modelHm.MunchDcHeaderMagAt(ttMagAt.Frequency_At);
                    tcb = ttMagAt;
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Mag_At.Header =
                    //                 "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //                  + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //                  + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //                  + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //                  + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //                  + "_" + VccValue + "V"                                                                                      // Vcc
                    //                  + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //                  + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Mag_At.ChannelNumber)                                                //Channel
                    //                  + "_" + FBAR.TestClass[TestCnt].Mag_At.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_At.Frequency_At) + "MHz"                           // Start Freq
                    //                   + "_x"
                    //                  + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //                  + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                            // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Mag_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Mag_At.ChannelNumber)                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Mag_At.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_At.Frequency_At) + "MHz"                           // Start Freq
                    //                   + "_x"
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                                   // LNA DAC
                    //}
                    break;

                #endregion "Mag_AT"

                case "PHASE_AT":

                    #region "PHASE_AT"

                    cPhase_At phaseAt = new cPhase_At();
                    phaseAt.TestNo = TestCnt;
                    phaseAt.PowerMode = reader.ReadTcfData("Power_Mode");
                    phaseAt.Band = reader.ReadTcfData("BAND");
                    phaseAt.Frequency = reader.ReadTcfData("Target_Freq");
                    phaseAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    phaseAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) phaseAt.Target_Frequency = reader.ReadTcfDataDouble("Target_Freq");
                    phaseAt.Interpolation = reader.ReadTcfData("Interpolation");

                    if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    {
                        //phaseAt.Header = modelHm.GetJHeader2a();
                        phaseAt.Header = modelHm.MunchDcHeaderMagAt(phaseAt.Target_Frequency);
                    }
                    else
                    {
                        //phaseAt.Header = modelHm.GetJHeader2a();
                        phaseAt.Header = modelHm.MunchDcHeaderMagAt(phaseAt.Target_Frequency);
                    }

                    tcb = phaseAt;
                    break;

                #endregion "PHASE_AT"

                case "FREQ_AT":

                    #region "Freq_AT"

                    cFreq_At freqAt = new cFreq_At();
                    freqAt.TestNo = TestCnt;
                    // Case HLS2.
                    //freqAt.Selected_Port = reader.ReadTcfData("Selected_Port");
                    freqAt.PowerMode = reader.ReadTcfData("Power_Mode");
                    freqAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    freqAt.SParameters = reader.ReadTcfData("S-Parameter");
                    freqAt.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    freqAt.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    freqAt.Search_DirectionMethod = reader.ReadTcfData("Search_Direction");
                    freqAt.Search_Type = reader.ReadTcfData("Search_Method");
                    freqAt.Search_Value = reader.ReadTcfData("Search_Value");
                    freqAt.Interpolation = reader.ReadTcfData("Interpolation");
                    freqAt.b_Invert_Search = reader.ReadTcfDataBoolean("Misc");
                    freqAt.Offset = reader.ReadTcfDataDouble2("Offset");
                    //freqAt.Use_Gain = reader.ReadTcfData("Use_Previous"); //2018-03-07 Seoul
                    // CCT Modified. Prev test here means an arbitrary test number, not the previous.
                    //if (upm.b_Use_Previous) freqAt.Previous_TestNo = upm.Previous_TestNo;
                    //if (upm.b_Use_Previous) freqAt.Previous_Test = upm.Previous_Test;
                    
                    //TODO Case HLS2: Handle for Joker.
                    freqAt.Header = modelHm.MunchDcHeader2();
                    tcb = freqAt;

                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Freq_At.Header =
                    //                 "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //                  + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //                  + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //                  + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //                  + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //                  + "_" + VccValue + "V"                                                                                      // Vcc
                    //                  + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //                  + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Freq_At.ChannelNumber)                                                //Channel
                    //                  + "_" + FBAR.TestClass[TestCnt].Freq_At.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Freq_At.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_x"                            // Stop Freq
                    //                  + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //                  + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                            // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Freq_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Freq_At.ChannelNumber)                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Freq_At.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Freq_At.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_x"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                                  // LNA DAC
                    //}

                    break;

                #endregion "Freq_AT"

                case "MAG_BETWEEN":

                    #region "Mag_Between"

                    cMag_Between ttmb = new cMag_Between();
                    ttmb.TestNo = TestCnt;
                    //ttmb.SwitchIn = reader.ReadTcfData("Switch_In");
                    //ttmb.SwitchOut = reader.ReadTcfData("Switch_Out");
                    //ttmb.PowerMode = reader.ReadTcfData("Power_Mode");
                    //ttmb.Band = reader.ReadTcfData("BAND").Split('-')[0];
                    ttmb.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttmb.SParameters = reader.ReadTcfData("S-Parameter");
                    ttmb.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttmb.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    ttmb.Search_MethodType = reader.ReadTcfData("Search_Method");
                    ttmb.Interpolation = reader.ReadTcfData("Interpolation");
                    ttmb.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttmb.Offset = reader.ReadTcfDataDouble2("Offset");
                    ttmb.Non_Inverted = reader.ReadTcfData("Non_Inverted");
                    ttmb.Use_Gain = reader.ReadTcfData("Use_Previous");

                    //TODO Case HLS2 -Handle for Joker
                    ttmb.Header = modelHm.MunchDcHeader2();
                    tcb = ttmb;

                    #region Commented Section - keeping for ref only
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Mag_Between.Header =
                    //                 "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //                  + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //                  + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //                  + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //                  + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //                  + "_" + VccValue + "V"                                                                                      // Vcc
                    //                  + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //                  + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Mag_Between.ChannelNumber)                                                //Channel
                    //                  + "_" + FBAR.TestClass[TestCnt].Mag_Between.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //                  + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //                  + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                           // LNA DAC

                    //}
                    //else if (reader.ReadTcfData("N-Parameter-Class").Contains("Gain-Tx-RxBand"))
                    //{
                    //    FBAR.TestClass[TestCnt].Mag_Between.Header =
                    //                 "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //                   + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //                  + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //                  + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //                  + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //                  + "_" + VccValue + "V"                                                                                      // Vcc
                    //                  + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //                  + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Mag_Between.ChannelNumber)                                                //Channel
                    //                  + "_" + FBAR.TestClass[TestCnt].Mag_Between.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //                  + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //                  + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                            // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Mag_Between.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Mag_Between.ChannelNumber)                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Mag_Between.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                                   // LNA DAC
                    //} 
                    #endregion

                    break;

                #endregion "Mag_Between"

                case "RIPPLE_BETWEEN":

                    #region "Ripple_Between"

                    cRipple_Between ttrb = new cRipple_Between();
                    ttrb.TestNo = TestCnt;
                    ttrb.SwitchIn = reader.ReadTcfData("Switch_In");
                    ttrb.SwitchOut = reader.ReadTcfData("Switch_Out");
                    ttrb.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttrb.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttrb.SParameters = reader.ReadTcfData("S-Parameter");
                    ttrb.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttrb.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    ttrb.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttrb.Offset = reader.ReadTcfDataDouble2("Offset");

                    // Case Pinot.
                    ttrb.Sampling_BW = reader.ReadTcfData("Sampling_BW");
                    ttrb.Sampling_Interval = reader.ReadTcfData("Sampling_Interval");

                    // Case HLS2. Case Joker is uncommented.
                    //ttrb.Selected_Port = reader.ReadTcfData("Selected_Port");
                    //ttrb.Sampling_BW = reader.ReadTcfData("Sampling_BW");
                    //ttrb.Sampling_Interval = reader.ReadTcfData("Sampling_Interval");

                    //TODO Case HLS2: Handle for Joker.
                    ttrb.Header = modelHm.MunchDcHeader2();
                    tcb = ttrb;
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Ripple_Between.Header =
                    //                 "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //                  + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //                  + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //                  + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //                  + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //                  + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //                  + "_" + VccValue + "V"                                                                                      // Vcc
                    //                  + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //                  + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Ripple_Between.ChannelNumber)                                                //Channel
                    //                  + "_" + FBAR.TestClass[TestCnt].Ripple_Between.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Ripple_Between.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Ripple_Between.StopFreq) + "MHz"                            // Stop Freq
                    //                  + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //                  + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                            // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Ripple_Between.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + reader.ReadTcfData("Power_Mode")                                                                          // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + Convert.ToString(FBAR.TestClass[TestCnt].Ripple_Between.ChannelNumber)                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Ripple_Between.SParameters                                                     // Sparameter
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Ripple_Between.StartFreq) + "MHz"                           // Start Freq
                    //                   + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Ripple_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader
                    //                  + "_NOTE_" + reader.ReadTcfData("Para.Spec");                                                                                                   // LNA DAC
                    //}

                    break;

                #endregion "Ripple_Between"
            }

            return tcb;
        }

        public TestCaseBase Load_TestConditionUnused(string tmpTestCondition, int TestCnt, UsePreviousTcfModel upm,
TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            TestCaseBase tcb = null;

            switch (tmpTestCondition.ToUpper())
            {
                case "MAG_SUM_BETWEEN":

                    #region "Mag_Sum_Between"
                    cMag_Sum_Between ttMagsb = new cMag_Sum_Between();
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.TestNo = TestCnt;
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.Selected_Port = reader.ReadTcfData("Selected_Port");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.PowerMode = reader.ReadTcfData("Power_Mode");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.SParameters = reader.ReadTcfData("S-Parameter");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.Search_MethodType = reader.ReadTcfData("Search_Method");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.Interpolation = reader.ReadTcfData("Interpolation");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value"));
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.Offset = reader.ReadTcfDataDouble2("Offset");
                    //FBAR.TestClass[TestCnt].Mag_Sum_Between.Non_Inverted = reader.ReadTcfData("Non_Inverted");

                    ttMagsb.TestNo = TestCnt;
                    ttMagsb.SwitchIn = reader.ReadTcfData("Switch_In");
                    ttMagsb.SwitchOut = reader.ReadTcfData("Switch_Out");
                    ttMagsb.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttMagsb.Band = reader.ReadTcfData("BAND").Split('-')[0];
                    ttMagsb.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttMagsb.SParameters = reader.ReadTcfData("S-Parameter");
                    ttMagsb.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttMagsb.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    ttMagsb.Search_MethodType = reader.ReadTcfData("Search_Method");
                    ttMagsb.Interpolation = reader.ReadTcfData("Interpolation");
                    ttMagsb.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttMagsb.Offset = reader.ReadTcfDataDouble2("Offset");
                    ttMagsb.Non_Inverted = reader.ReadTcfData("Non_Inverted");
                    ttMagsb.Use_Gain = reader.ReadTcfData("Use_Previous");

                    if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    {
                        ttMagsb.Header = modelHm.GetJHeader1();
                    }
                    else
                    {
                        ttMagsb.Header = modelHm.GetJHeader1();
                    }                                                                                                           // LNA DAC

                    tcb = ttMagsb;
                    break;

                #endregion "Mag_Sum_Between"

                case "CPL_BETWEEN":

                    #region "CPL_BETWEEN"

                    cCPL_Between tt = new cCPL_Between();
                    tt.TestNo = TestCnt;
                    tt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    tt.SParameters1 = reader.ReadTcfData("S-Parameter");
                    tt.SParameters2 = reader.ReadTcfData("S-Parameter_2");
                    tt.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    tt.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    tt.Search_MethodType = reader.ReadTcfData("Search_Method");
                    tt.Interpolation = reader.ReadTcfData("Interpolation");
                    tt.Offset = reader.ReadTcfDataDouble2("Offset");
                    tcb = tt;
                    // if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].CPL_Between.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].CPL_Between.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].CPL_Between.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].CPL_Between.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                         // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].CPL_Between.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].CPL_Between.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].CPL_Between.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].CPL_Between.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                          // LNA DAC
                    //}                                                                         // LNA DAC

                    break;

                #endregion "CPL_BETWEEN"

                case "BALANCE":

                    #region "Balance"

                    SparamBalance ttb = new SparamBalance();
                    ttb.TestNo = TestCnt;
                    ttb.Channel_Number = reader.ReadTcfDataInt("Channel Number");
                    ttb.Search_Type = reader.ReadTcfData("Search_Method");
                    ttb.BalanceType = reader.ReadTcfData("Balance_Type");
                    ttb.SParameters_1 = reader.ReadTcfData("S-Parameter");
                    ttb.SParameters_2 = reader.ReadTcfData("S-Parameter_2");
                    ttb.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttb.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    ttb.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    tcb = ttb;
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Balance.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Balance.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Balance.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Balance.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                         // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Balance.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Balance.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Balance.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Balance.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                          // LNA DAC
                    //}                                                                                       // LNA DAC

                    break;

                #endregion "Balance"

                case "NF_GAIN_AT":

                    #region "NF_Gain_AT"

                    cNF_Topaz_At ttNfTopazAt = new cNF_Topaz_At();
                    ttNfTopazAt.TestNo = TestCnt;
                    ttNfTopazAt.Band = reader.ReadTcfData("BAND");
                    ttNfTopazAt.PowerMode = reader.ReadTcfData("Power_Mode");
                    // Case HLS2.
                    //ttNfTopazAt.Selected_Port = reader.ReadTcfData("Selected_Port");
                    ttNfTopazAt.Selected_Port = reader.ReadTcfData("Switch_Out");
                    ttNfTopazAt.Frequency = reader.ReadTcfData("Target_Freq");
                    ttNfTopazAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    ttNfTopazAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) ttNfTopazAt.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    ttNfTopazAt.Interpolation = reader.ReadTcfData("Interpolation");
                    ttNfTopazAt.Offset = reader.ReadTcfDataDouble2("Offset");
                    //if (upm.b_Use_Previous) ttNfTopazAt.Previous_TestNo = upm.Previous_TestNo;
                    //else ttNfTopazAt.Previous_TestNo = -1; //Prevent Error

                    if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    {
                        ttNfTopazAt.Header = modelHm.GetJHeader2a();
                    }
                    else
                    {
                        ttNfTopazAt.Header = modelHm.GetJHeader2b();
                        // LNA DAC
                    }

                    tcb = ttNfTopazAt;
                    break;

                #endregion "NF_Gain_AT"

                case "MAG_AT_LIN":

                    #region "Mag_AT_LIN"

                    cMag_At_Lin magAtLin = new cMag_At_Lin();
                    //magAtLin.TestNo = TestCnt;
                    magAtLin.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    magAtLin.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) magAtLin.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    magAtLin.Interpolation = reader.ReadTcfData("Interpolation");
                    //magAtLin.Offset = reader.ReadTcfDataDouble2("Offset");
                    if (upm.b_Use_Previous) magAtLin.Previous_Test = upm.Previous_Test;
                    //if (upm.b_Use_Previous) magAtLin.Previous_TestNo = upm.Previous_TestNo;
                    //else magAtLin.Previous_TestNo = -1; //Prevent Error
                    tcb = magAtLin;
                    break;

                #endregion "Mag_AT_LIN"

                case "REAL_AT":

                    #region "REAL_AT"

                    cReal_At realAt = new cReal_At();
                    //realAt.TestNo = TestCnt;
                    realAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    realAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) realAt.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    realAt.Interpolation = reader.ReadTcfData("Interpolation");
                    reader.ReadTcfDataDouble2("Offset");
                    if (upm.b_Use_Previous)
                    {
                        realAt.Previous_Test = upm.Previous_Test;
                        //realAt.Previous_TestNo = upm.Previous_TestNo;
                    }
                    else
                    {
                        //realAt.Previous_TestNo = -1; //Prevent Error
                    }

                    tcb = realAt;
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Real_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Real_At.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Real_At.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Real_At.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                         // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Real_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Real_At.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Real_At.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Real_At.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                          // LNA DAC
                    //}

                    break;

                #endregion "REAL_AT"

                case "IMAG_AT":

                    #region "IMAG_AT"

                    cImag_At imagAt = new cImag_At();
                    //imagAt.TestNo = TestCnt;
                    imagAt.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    imagAt.SParameters = reader.ReadTcfData("S-Parameter");
                    if (!upm.b_Use_Previous) imagAt.Frequency_At = reader.ReadTcfDataDouble("Target_Freq");
                    imagAt.Interpolation = reader.ReadTcfData("Interpolation");
                    //imagAt.Offset = reader.ReadTcfDataDouble2("Offset");
                    if (upm.b_Use_Previous)
                    {
                        imagAt.Previous_Test = upm.Previous_Test;
                        //imagAt.Previous_TestNo = upm.Previous_TestNo;
                    }
                    else
                    {
                        //imagAt.Previous_TestNo = -1; //Prevent Error
                    }

                    tcb = imagAt;
                    //if (reader.ReadTcfData("TUNABLE_BAND").Contains("CA"))
                    //{
                    //    FBAR.TestClass[TestCnt].Imag_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Imag_At.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Imag_At.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Imag_At.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                         // LNA DAC

                    //}
                    //else
                    //{
                    //    FBAR.TestClass[TestCnt].Imag_At.Header =
                    //      "F_" + reader.ReadTcfData("N-Parameter-Class")                                                                 // Parameter Class
                    //       + "_" + reader.ReadTcfData("TUNABLE_BAND")                                                                    // Band
                    //       + "_" + reader.ReadTcfData("Switch_In")                                                                       // Meas_Port1
                    //       + "_" + reader.ReadTcfData("Switch_ANT")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("Switch_Out")                                                                      // Meas_Port2
                    //       + "_" + reader.ReadTcfData("N_Mode")                                                                          // Mode
                    //       + "_" + FBAR.TestClass[TestCnt].Imag_At.PowerMode                                                       // Gain Mode
                    //       + "_" + VccValue + "V"                                                                                      // Vcc
                    //       + "_" + VddValue + "Vdd"                                                                                    // Vdd
                    //       + "_CH" + FBAR.TestClass[TestCnt].Imag_At.ChannelNumber                                                //Channel
                    //       + "_" + FBAR.TestClass[TestCnt].Imag_At.SParameters                                                     // Sparameter
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StartFreq) + "MHz"                           // Start Freq
                    //       + "_" + General.convertMhz(FBAR.TestClass[TestCnt].Mag_Between.StopFreq) + "MHz"                            // Stop Freq
                    //       + "_0x" + DriveBias + "_0x" + MainBias                                                                      // DAC1, DAC2
                    //       + "_" + Biasheader;                                                                                          // LNA DAC
                    //}                                                                 // LNA DAC

                    break;

                #endregion "IMAG_AT"
            }

            return tcb;
        }

        public DCMipiOtpTestController Load_TestConditionDc(string tmpTestCondition, int TestCnt,
            Dictionary<string, string> TestCond,
            TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader, string[] listLNAReg)
        {
            string Header = "";
            string RXHeader = "";
            string DriveBias = "";
            string MainBias = "";
            string Biasheader = "";

            string Biasheader_MB = "";
            string Biasheader_HB = "";

            string Vbattval = "";
            string VbiasTval = "";
            string Vcplval = "";
            string VccValue = "";
            string VddValue = "";

            DCMipiOtpTestController t = null;

            switch (tmpTestCondition.ToUpper())
            {
                case "DC_SETTINGS":
                case "DC_SETTING":
                case "TEMP":
                case "MIPI":
                case "DC_CURRENT":
                    //DriveBias = Dc_header = reader.ReadTcfData("TXREG08");
                    //MainBias = Dc_header = reader.ReadTcfData("TXREG09");
                    t = new DCMipiOtpTestController();
                    t.TestNo = TestCnt;
                    t.PowerMode = reader.ReadTcfData("Power_Mode");
                    t.TotalChannel = reader.ReadTcfDataInt("Total_DC_Channel");
                    t.DC_PS_Set = reader.ReadTcfDataInt("DC_PS_Set");
                    t.Band = reader.ReadTcfData("BAND");
                    t.NParameterClass = reader.ReadTcfData("N-Parameter-Class");
                    t.ParameterHeader = reader.ReadTcfData("Para.Spec");// DC.TestClass[TestCnt].SMU_DC_Setting.Header;
                                                                      //Biasheader = HearderName(TestCond, cProject.listLNAReg);
                    //t.ChannelNumber = reader.ReadTcfDataInt("Channel Number");
                    t.ChannelNumberList = reader.ReadTcfDataIntList("Channel Number");
                    t.MipiCommands_NA = GetMipiCommands(TestCond);

                    //ChoonChin (20191206) - Added Switching port
                    t.Switch_IN = reader.ReadTcfData("Switch_In");
                    t.Switch_ANT = reader.ReadTcfData("Switch_ANT");
                    t.Switch_OUT = reader.ReadTcfData("Switch_Out");

                    // Case HLS2
                    //Header = reader.ReadTcfData("V_CH4");
                    //RXHeader = reader.ReadTcfData("V_CH2");
                    //Vbattval = reader.ReadTcfData("V_CH1");
                    //VbiasTval = reader.ReadTcfData("V_CH3");
                    //Vcplval = reader.ReadTcfData("V_CH5");
                    //DriveBias = reader.ReadTcfData("TXREG00");
                    //MainBias = reader.ReadTcfData("TXREG01");
                    //Biasheader = modelHm.HearderName(reader.ReadTcfData("RXREG07"), reader.ReadTcfData("RXREG08"), reader.ReadTcfData("RXREG09"), reader.ReadTcfData("RXREG0A"), reader.ReadTcfData("RXREG0B"), reader.ReadTcfData("RXREG0C"), reader.ReadTcfData("RXREG0D"), reader.ReadTcfData("RXREG0E"), reader.ReadTcfData("RXREG0F"));

                    //modelHm.SetDc(Header, RXHeader, MainBias, Biasheader);
                    //t.Header = modelHm.Munch(Vcplval, Vbattval, VbiasTval);
                    //t.Header = HeaderMuncher(TestCond, Header, RXHeader, Vbattval,
                    //    VbiasTval, Vcplval, MainBias, Biasheader);

                    // Case Joker
                    //VccValue = reader.ReadTcfData("V_CH1");
                    //VddValue = reader.ReadTcfData("V_CH3");

                    //RXHeader = reader.ReadTcfData("REGCUSTOM");
                    DriveBias = reader.ReadTcfData("TXREG07");
                    MainBias = reader.ReadTcfData("TXREG08");
                    Biasheader = modelHm.HearderName(listLNAReg, true);
                    modelHm.SetDc(DriveBias, MainBias, Biasheader);
                    t.Header = modelHm.MunchDcHeader();
                    //t.Header = reader.ReadTcfData("N-Parameter-Class");

                    #region Fill DC Channel Setting sheet

                    List<PowerSupplyDCSettingDataType> dcSettingList = new List<PowerSupplyDCSettingDataType>();

                    for (int iDC = 0; iDC < t.TotalChannel; iDC++)
                    {
                        PowerSupplyDCSettingDataType dcSetting = new PowerSupplyDCSettingDataType();
                        dcSetting.Channel = iDC;
                        string rowAboveHeader = m_stub1.GetRowAboveTestParameterRow(Test_Parameters)[iDC];
                        if (rowAboveHeader == "")
                        {
                            dcSetting.Header = "I_CH" + (iDC + 1).ToString();
                        }
                        else
                        {
                            dcSetting.Header = rowAboveHeader.Replace("V", "I");
                        }
                        dcSetting.Voltage = reader.ReadTcfDataDouble("V_CH" + (iDC + 1).ToString());
                        dcSetting.Current = reader.ReadTcfDataDouble("I_CH" + (iDC + 1).ToString());

                        dcSettingList.Add(dcSetting);
                    }

                    t.DC_Setting = dcSettingList;

                    #endregion Fill DC Channel Setting sheet

                    //t.MIPI_DACbit = reader.ReadTcfData("MIPI_DACbit");
                    //t.PowerMode = reader.ReadTcfData("Power_Mode");

                    //if (tmpTestCondition != "TEMP")
                    //{
                    //    t.NAScriptDic = GenVectorLib.cGenVector.NAScriptList[Script_Number]; Script_Number++;
                    //}

                    t.Sleep_ms = reader.ReadTcfDataInt("Sleep (Wait - ms)");
                    t.Ignore_Read = reader.CInt2Bool("Misc");   // if set to 1, then will ignore read and will not parse result back

                    //t.Initialize(listdcs);

                    break;

                default:
                    break;
            }

            return t;
        }

        /// <summary>
        /// If it's Trigger, create new trigger object. Otherwise, set the S-Parameter.
        /// </summary>
        public void SetupDataTriggerObject(TestConditionDataObject tc, int TestCnt, int firstTrigOfRaw,
            string tcSParameter, SParaTestFactory tcFactory)
        {
            switch (tc.TestParameterColumn)
            {
                case "TRIGGER":
                case "TRIGGER2":
                case "TRIGGER_NF":
                case "TRIGGER_NF_DUAL":
                    //if (TestCnt > 2)
                    if (TestCnt > firstTrigOfRaw)  //@need to Automation to set Index...
                    {
                        TestConditionDataObject previousTc = tcFactory.GetItem(TestCnt - 1);
                        bool isPrevTcTrigger = previousTc.TestParameterColumn.Contains("TRIGGER");
                        if (!isPrevTcTrigger)
                        {
                            DataTriggered2.AddTrigger();
                        }
                    }
                    break;
            }

            // S-Parameter is defined in measurement (not trigger).
            DataTriggered2.SetSParameterNumber(tcSParameter);
        }

        /// <summary>
        /// Called when all test conditions are loaded to make sure the last trigger are added in as well.
        /// </summary>
        public void FinalizeDataTriggerObject()
        {
            DataTriggered2.AddTrigger();
        }

        private static Dictionary<string, int> Test_Parameters;
        private List<string> m_cacheRowAboveTp;

        private Dictionary<string, int> Load_TestParameterHeader(string[,] sheetCondFbar)
        {
            Dictionary<string, int> tpList = new Dictionary<string, int>();

            var RowNo = 1;
            var ColNo = 2;
            var Found = false;
            var FoundHeader = false;
            do
            {
                //tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, 1);
                string tmpStr = sheetCondFbar[RowNo - 1, 0];

                if (tmpStr.ToUpper() == "#START")
                {
                    do
                    {
                        //tmpStr = cExtract.Get_Data(TestConditionFBAR, RowNo, ColNo);
                        tmpStr = sheetCondFbar[RowNo - 1, ColNo - 1];
                        if (tmpStr != "")
                        {
                            try
                            {
                                tpList.Add(tmpStr, ColNo);
                            }
                            catch
                            {
                                string msg = "Possible duplicate header at Column " + ColNo;
                                PromptManager.Instance.ShowError(msg, "Error in processing Test Parameters in Test Condition");
                            }
                        }
                        ColNo++;
                        if (tmpStr.ToUpper() == "#END") FoundHeader = true;
                    } while (FoundHeader == false);
                }
                RowNo++;
                tmpStr = sheetCondFbar[RowNo - 1, 0];
                if (tmpStr.ToUpper() == "#END")
                {
                    Found = true;
                }
            } while (Found == false);
            if (!(Found && FoundHeader))
            {
                string msg = "Missing Test Condition or Test Condition Header";
                PromptManager.Instance.ShowError(msg, "Error processing Test Condition Header");
            }

            return tpList;
        }

        /// <summary>
        /// Speed up repetitive call to get the same DC value. By cache.
        /// </summary>
        /// <param name="Test_Parameters"></param>
        /// <returns></returns>
        private List<string> GetRowAboveTestParameterRow(string[,] sheetCondFbar, Dictionary<string, int> Test_Parameters)
        {
            if (m_cacheRowAboveTp != null) return m_cacheRowAboveTp;

            m_cacheRowAboveTp = new List<string>();

            //TODO Case HLS2 is 5, Joker is 3. Hardcoded.
            for (int iDC = 0; iDC < 3; iDC++)
            {
                int colNo = Test_Parameters["V_CH" + (iDC + 1)];
                string rowAboveHeader = sheetCondFbar[1, colNo];
                m_cacheRowAboveTp.Add(rowAboveHeader);
            }

            return m_cacheRowAboveTp;
        }

        private string GetStr(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                return dic[theKey];
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

        private List<EqLib.MipiSyntaxParser.ClsMIPIFrame> GetMipiCommands(Dictionary<string, string> TestCon)
        {
            List<EqLib.MipiSyntaxParser.ClsMIPIFrame> MipiCommandsTemp = new List<EqLib.MipiSyntaxParser.ClsMIPIFrame>();
            //string MipiCommand_tcf = GetStr(TestCon, "MipiCommands").Trim().ToUpper();
            string MipiCommandString_tcf = GetStr(TestCon, "MIPI Commands").Trim().ToUpper();
            string MipiRegcustomUDR = GetRegcustomUDRMipiCommands(TestCon);
            string VaribleData = "";

            //if (MIPI_Command_Strings.ContainsKey(MipiCommand_tcf))
            //{
            //MipiCommandsTemp = MipiSyntaxParser.CreateListOfMipiFrames(MIPI_Command_Strings[MipiCommand_tcf]);
            MipiCommandsTemp = EqLib.MipiSyntaxParser.CreateListOfMipiFrames(MipiCommandString_tcf);

            if (MipiRegcustomUDR != "" && MipiRegcustomUDR != null)
                MipiCommandsTemp.AddRange(EqLib.MipiSyntaxParser.CreateListOfMipiFrames(MipiRegcustomUDR));

            // Replace Varibles in command syntax with Data values from the TCF
            foreach (EqLib.MipiSyntaxParser.ClsMIPIFrame command in MipiCommandsTemp)
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

            return MipiCommandsTemp;
        }

        private string GetRegcustomUDRMipiCommands(Dictionary<string, string> TestCon)
        {
            string strRegcustom = GetStr(TestCon, "REGCUSTOM").Trim().ToUpper();

            if (strRegcustom == "") return null;

            string TCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");
            string DUTRevision = "";
            System.IO.StreamReader reader;

            foreach (string tempRevison in System.IO.Path.GetFileName(TCFpath).Split('_'))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(tempRevison, @"^[AB]{1}\d{1}[A-Z]{1}"))
                {
                    DUTRevision = tempRevison;
                    break;
                }
            }
            //ChoonChuin - for PROD, always set it to B1A
            DUTRevision = "B1A";

            string[] arrRegcustom = strRegcustom.Split('.');

            string strFullmipiCommands = "";
            string currSlaveAdd = "", previouSlaveAdd = "";

            foreach (string currentRegcustom in arrRegcustom)
            {
                if (!currentRegcustom.Contains("OUT")) continue;
                string ScriptPath = System.IO.Path.GetDirectoryName(TCFpath).Replace("TCF", "Script");
                ScriptPath += "\\BandX\\Script";
                reader = new System.IO.StreamReader(System.IO.Path.Combine(ScriptPath, DUTRevision, "TUNABLE", currentRegcustom + ".txt"), System.Text.Encoding.Default);

                while (!reader.EndOfStream)
                {
                    string Getstring = reader.ReadLine().Trim().ToUpper();

                    if (Getstring.ToUpper() != "#START") continue;
                    while (!reader.EndOfStream)
                    {
                        Getstring = reader.ReadLine().Trim().ToUpper();

                        if (Getstring.ToUpper() != "#START") continue;
                        while (!reader.EndOfStream)
                        {
                            Getstring = reader.ReadLine().Trim().ToUpper();
                            if (Getstring.ToUpper() == "") continue;
                            if (Getstring.Contains("TX")) currSlaveAdd = Eq.Site[0].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                            else if (Getstring.Contains("RX")) currSlaveAdd = Eq.Site[0].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];

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
                reader.Close();
            }

            return strFullmipiCommands;
        }
    }
}