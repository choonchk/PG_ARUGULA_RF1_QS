using System;
using LibFBAR_TOPAZ;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.SParaModel
{
    public class FbarCommonTestFactory
    {
        public TestCaseCalcBase TestCondition { get; set; }

        public TestCaseCalcBase Load_TestCondition(string tmpTestCondition, UsePreviousTcfModel upm,
            TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            TestCaseCalcBase tcb = null;
            SparamDelta ttd = new SparamDelta();

            switch (tmpTestCondition.ToUpper())
            {
                case "DELTA":

                    #region "DELTA"

                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.TestParameter = reader.ReadTcfData("Test Parameter");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.Band = reader.ReadTcfData("BAND");
                    ttd.ParameterHeader = reader.ReadTcfData("TUNABLE_BAND");

                    //if (COMMON.TestClass[TestCnt].Delta.PowerMode != "HP") { m_modelHeaderMuncher.ResetHeader(); } //YC added

                    //ttd.Header = modelHm.GetJHeader3(TestCond, ttd.PowerMode, ttd.Band);
                    ttd.Header = modelHm.MunchDcHeader2();

                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;

                    #endregion "DELTA"

                    break;

                case "PHASE_DELTA":

                    #region "PHASE_DELTA"
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.TestParameter = reader.ReadTcfData("Test Parameter");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.Band = reader.ReadTcfData("BAND");

                    if (ttd.PowerMode != "HP") { modelHm.ResetHeader(); } //YC added

                    ttd.Header = modelHm.GetJHeader4();
                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;

                    #endregion "PHASE_DELTA"

                    break;

                case "RL_DELTA_GEN":

                    #region "RL_DELTA_GEN"

                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttd.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");

                    ttd.TestParameter = reader.ReadTcfData("Test Parameter");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");

                    ttd.Band = reader.ReadTcfData("BAND");

                    ttd.Selected_Port = reader.ReadTcfData("Selected_Port");

                    if (ttd.PowerMode != "HP") { modelHm.ResetHeader(); } //YC added

                    ttd.Header = modelHm.GetJHeader3b();
                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;

                    #endregion "RL_DELTA_GEN"

                    break;

                case "GAIN_DELTA_GEN":

                    #region "GAIN_DELTA_GEN"

                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.StartFreq = reader.ReadTcfDataDouble("Start_Freq");
                    ttd.StopFreq = reader.ReadTcfDataDouble("Stop_Freq");
                    ttd.TestParameter = reader.ReadTcfData("Test Parameter");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.Band = reader.ReadTcfData("BAND");
                    ttd.Selected_Port = reader.ReadTcfData("Selected_Port");

                    ttd.Header = modelHm.GetJHeader3b();                                                                      // Antenna
                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;

                    #endregion "GAIN_DELTA_GEN"

                    break;

                case "PHASE_DELTA_GEN":

                    #region "PHASE_DELTA_GEN"

                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.Band = reader.ReadTcfData("BAND");
                    ttd.Selected_Port = reader.ReadTcfData("Selected_Port");
                    ttd.Frequency = reader.ReadTcfData("Target_Freq");

                    ttd.Header = modelHm.GetJHeader3b();
                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;
                    #endregion "PHASE_DELTA_GEN"

                    break;

                case "NF_DELTA":

                    #region "NF_DELTA"

                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.TestParameter = reader.ReadTcfData("Test Parameter");
                    ttd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");
                    ttd.PowerMode = reader.ReadTcfData("Power_Mode");
                    ttd.Band = reader.ReadTcfData("BAND");
                    ttd.Selected_Port = reader.ReadTcfData("Selected_Port");
                    //   COMMON.TestClass[TestCnt].Delta.Frequency =  General.convertStr2Val(reader.GetStr("Start_Freq"));

                    ttd.Header = modelHm.GetJHeaderNf1();

                    //ChoonChin
                    //ttd.SParameters = reader.ReadTcfData("S-Parameter");
                    //ttd.Vcc = reader.GetStr("V_CH1");
                    //ttd.TXREG08 = reader.GetStr("TXREG08");
                    //ttd.TXREG09 = reader.GetStr("TXREG09");
                    //ttd.TargetFreq = reader.ReadTcfData("Target_Freq");
                    SetPreviousTest(ttd, upm, modelHm, reader);
                    SetHeader(ttd, tmpTestCondition, modelHm, reader);
                    tcb = ttd;
                    #endregion "NF_DELTA"

                    break;

                case "SUM":

                    #region "SUM"

                    SparamSum tts = new SparamSum();
                    tts.Header = reader.ReadTcfData("Parameter Header");
                    tts.Previous_Info = reader.ReadTcfData("Use_Previous");
                    tts.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");

                    string[] tmp_Info = reader.ReadTcfData("Use_Previous").Split(',');
                    if (tmp_Info.Length == 2)
                    {
                        //CheeOn: Changed Name "Delta"
                        upm.Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) + 2;
                        upm.Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) + 2;
                        tts.Header = reader.ReadTcfData("Parameter Header");
                        //+ cExtract.Get_Data(TestCondition, Previous_Test_1, Test_Parameters["Parameter Header"]);
                        //+ "_" + cExtract.Get_Data(TestCondition, Previous_Test_2, Test_Parameters["Parameter Header"]);
                    }

                    tcb = tts;
                    #endregion "SUM"

                    break;
            }
            
            return tcb;
        }

        public TestCaseCalcBase Load_TestConditionRelativeGain(string tmpTestCondition,
            UsePreviousTcfModel upm,
            TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            bool isRelGain = String.Compare(tmpTestCondition, "RELATIVE_GAIN") == 0;
            if (!isRelGain) return null;

            SparamRelativeGainDelta ttrgd = new SparamRelativeGainDelta();
            ttrgd.Header = reader.ReadTcfData("Parameter Header");
            ttrgd.Previous_Info = reader.ReadTcfData("Use_Previous");
            ttrgd.b_Absolute = reader.ReadTcfDataBoolean("Absolute Value");

            string tempnote = (reader.ReadTcfData("ParameterNote"));
            if (tempnote == "X" || tempnote == "")
            {
                ttrgd.Header = modelHm.MunchDcHeader2();
            }
            else
            {
                ttrgd.Header = modelHm.MunchDcHeader2();
            }

            upm.tmp_Info = ttrgd.Previous_Info.Split(',');
            if (upm.tmp_Info.Length == 2)
            {
                //CheeOn: Changed Name
                upm.Previous_Test_1 = Convert.ToInt32(upm.tmp_Info[0]) + 2;
                upm.Previous_Test_2 = Convert.ToInt32(upm.tmp_Info[1]) + 2;
                // Case HLS2 commented out.
                //ttrgd.Header = reader.GetStr("Parameter Header")
                //                  //+ cExtract.Get_Data(TestCondition, Previous_Test_1, Test_Parameters["Parameter Header"]);
                //                  + GetStr(DicTestCondTempNA[upm.Previous_Test_1], "Parameter Header");
                //+ "_" + reader.GetStr("Power_Mode"]);
            }

            return ttrgd;

        }

        private void SetPreviousTest(SparamDelta ttd, UsePreviousTcfModel upm,
            TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            // CC Note: Fix_Number is not used always empty. Delta does not use Use_Previous also.
            string _tempFixNumber = reader.ReadTcfData("Fix_Number");
            if (_tempFixNumber.ToUpper() != "V")
            {
                if (ttd.PowerMode != "HP") { modelHm.ResetHeader(); } //YC added

                // Case HLS2 commented out.
                //ttd.Header = modelHm.GetJHeader4b(TestCond, ttd.PowerMode, ttd.Band);
                ttd.Frequency = reader.ReadTcfData("Target_Freq");
                string[] tmp_Info = reader.ReadTcfData("Use_Previous").Split(',');
                if (tmp_Info.Length == 2)
                {
                    //CheeOn: Changed Name "Delta"
                    upm.Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) + 2;
                    upm.Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) + 2;
                }
            }
            else
            {
                //CheeOn: Changed Name "Delta"
                upm.Previous_Test_1 = Convert.ToInt32(String.Empty) + 2;
                ttd.Header = reader.ReadTcfData("Band");
            }
        }

        private void SetHeader(SparamDelta ttd,
            string tmpTestCondition,
            TcfHeaderGenerator modelHm, SParaEnaTestConditionReader reader)
        {
            string result = String.Empty;
            string _tempFixNumber = reader.ReadTcfData("Fix_Number");
            if (_tempFixNumber.ToUpper() != "V")
            {
                if (tmpTestCondition.ToUpper() == "PHASE_DELTA")
                {
                    result = modelHm.GetJHeader4b();
                    ttd.Header = result;
                }
            }
            else
            {
                result = reader.ReadTcfData("Band");
                ttd.Header = result;
            }
        }
    }
}