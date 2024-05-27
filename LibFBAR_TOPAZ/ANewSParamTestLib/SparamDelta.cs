using LibFBAR_TOPAZ.DataType;
using System;
using System.Collections.Generic;

namespace LibFBAR_TOPAZ
{
    public class SparamDelta : TestCaseCalcBase
    {
        #region "Declarations"

        // External Variables
        public string PowerMode;
        public string Band;
        public string ParameterHeader;
        public string Selected_Port;
        public string Frequency;
        public string TestParameter;
        public bool b_Absolute;

        public double StartFreq;
        public double StopFreq;

        public s_Result Relative_Use_Gain;

        // TestNo - 1
        public s_Result PreviousResult_1;

        // TestNo - 2
        public s_Result PreviousResult_2;

        private double rtnResult;

        #endregion "Declarations"

        /// <summary>
        /// Set by MagBetween, NFTopazAt, Phase_At. Used by Delta.
        /// </summary>
        public static Dictionary<string, double> Genspec = new Dictionary<string, double>();
        
        public override void RunTest()
        {
            double measuredResult = Measure();
            double calculatedResult = Calculate(measuredResult, Genspec);
            SetResult(calculatedResult);
        }

        private double Measure()
        {
            if (TestParameter == "PHASE_DELTA")
            {
                return rtnResult;
                // rtnResult = Math.Abs(SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data);
            }

            if (b_Absolute)
            {
                double previousResult1 = PreviousResult_1.Result_Data;
                rtnResult = Math.Abs(Relative_Use_Gain.Result_Data - previousResult1);

                //  rtnResult = Math.Abs(SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data);
            }
            else
            {
                //TODO Standardize DELTA and RELATIVE_GAIN calculation.
                // Case Joker.
                double previousResult1 = PreviousResult_1.Result_Data;
                double previousResult2 = PreviousResult_2.Result_Data;
                rtnResult = previousResult1 - previousResult2;

                //Relative_Use_Gain.Result_Data; //2018-11-01
                //  rtnResult = SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data;
                // Case HLS2.
                //double previousResult1 = PreviousResult_1.Result_Data;
                //rtnResult = Relative_Use_Gain.Result_Data - previousResult1;

                rtnResult = previousResult1 - Relative_Use_Gain.Result_Data; 

            }
            return rtnResult;
        }

        private double Calculate(double measuredResult, Dictionary<string, double> genSpec2)
        {
            double result = measuredResult;

            string[] SplitMode = PowerMode.Split('_');

            if (SaveResult2.IsHeaderContains("GEN37"))
            {
                int da = 0;
            }

            double PhaseDelta1;
            double PhaseDelta2;

            switch (TestParameter)
            {
                case "PHASE_DELTA":
                    string key1 = String.Format("{0}_{1}_PHASE_{2}_{3}_{4}", Band, ParameterHeader,
                        SplitMode[0], Frequency, Selected_Port);
                    PhaseDelta1 = genSpec2[key1];
                    string key2 = String.Format("{0}_{1}_PHASE_{2}_{3}_{4}", Band, ParameterHeader,
                        SplitMode[1], Frequency, Selected_Port);
                    PhaseDelta2 = genSpec2[key2];
                    result = CalculatePhaseDelta(PhaseDelta1, PhaseDelta2);
                    break;

                case "PHASE_DELTA_GEN":
                    result = CalculatePhaseDeltaGen(SplitMode, genSpec2);
                    break;

                case "RL_DELTA_GEN":
                    PhaseDelta1 =
                        genSpec2[Band + "_RL_ANT_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" +
                                Selected_Port];
                    PhaseDelta2 =
                        genSpec2[Band + "_RL_ANT_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" +
                                Selected_Port + "_GEN36"];
                    result = Math.Abs(PhaseDelta1 - PhaseDelta2);
                    break;

                case "GAIN_DELTA_GEN":
                    result = CalculateGainDelta(genSpec2, SplitMode);
                    break;

                case "NF_DELTA":
                    // CCT no longer supported.
                    //result = CalculateNfDelta(genSpec2, SplitMode);
                    break;
            }

            return result;
        }

        private double CalculatePhaseDelta(double PhaseDelta1, double PhaseDelta2)
        {
            if (PhaseDelta1 < 0) PhaseDelta1 = PhaseDelta1 + 360;
            if (PhaseDelta2 < 0) PhaseDelta2 = PhaseDelta2 + 360;

            double PhaseResult = Math.Abs(PhaseDelta1 - PhaseDelta2);

            if (PhaseResult > 180) return 360 - PhaseResult;

            return PhaseResult;
        }

        private double CalculateGainDelta(Dictionary<string, double> genSpec2, string[] SplitMode)
        {
            double r;
            if (SaveResult2.IsHeaderContains("GEN37"))
            {
                double PhaseDelta1 =
                    genSpec2[Band + "_RX_GAIN_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" + Selected_Port];
                double PhaseDelta2 =
                    genSpec2[
                        Band + "_RX_GAIN_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" + Selected_Port + "_GEN37"];
                r = Math.Abs(PhaseDelta1 - PhaseDelta2);
            }
            else
            {
                double PhaseDelta1 =
                    genSpec2[Band + "_RX_GAIN_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" + Selected_Port];
                double PhaseDelta2 =
                    genSpec2[
                        Band + "_RX_GAIN_" + SplitMode[0] + "_" + StartFreq + "_" + StopFreq + "_" + Selected_Port + "_GEN35"];
                r = Math.Abs(PhaseDelta1 - PhaseDelta2);
            }

            return r;
        }

        private double CalculatePhaseDeltaGen(string[] SplitMode, Dictionary<string, double> genSpec2)
        {
            double r = 0;
            if (SaveResult2.IsHeaderContains("GEN34"))
            {
                string key1 = String.Format("{0}_PHASE_{1}_{2}_{3}", Band, SplitMode[0], Frequency, Selected_Port);
                double PhaseDelta1 = genSpec2[key1];
                string key2 = String.Format("{0}_GEN34", key1);
                double PhaseDelta2 = genSpec2[key2];
                r = CalculatePhaseDelta(PhaseDelta1, PhaseDelta2);
            }
            else if (SaveResult2.IsHeaderContains("GEN39"))
            {
                string key1 = String.Format("{0}_PHASE_{1}_{2}_{3}", Band, SplitMode[0], Frequency, Selected_Port);
                double PhaseDelta1 = genSpec2[key1];
                string key2 = String.Format("{0}_GEN39", key1);
                double PhaseDelta2 = genSpec2[key2];
                r = CalculatePhaseDelta(PhaseDelta1, PhaseDelta2);
            }

            return r;
        }

        #region Not supported

        //private double CalculateNfDelta(Dictionary<string, double> genSpec2, string[] SplitMode)
        //{
        //    //ChoonChin - Get offset from CF
        //    //string PhaseDelta1Name = "NF_" + Band +  "_" + SParameters + "_" + TargetFreq.Replace(" M","") + "_" +  Vcc + "V_" + TXREG08 +"_" + TXREG09 +"_" + SplitMode[0] + "_"  + Selected_Port;
        //    //string PhaseDelta2Name = "NF_" + Band + "_GEN38_" + SParameters + "_" + TargetFreq.Replace(" M", "") + "_" + Vcc + "V_" + TXREG08 + "_" + TXREG09 + "_" + SplitMode[0] + "_" + Selected_Port;
        //    //double PhaseDeltaOffset1 = GU.getGUcalfactor(1, PhaseDelta1Name);
        //    //double PhaseDeltaOffset2 = GU.getGUcalfactor(1, PhaseDelta2Name);

        //    double PhaseDeltaOffset1 = 0;
        //    double PhaseDeltaOffset2 = 0;
        //    int TestStartNo = 0;

        //    if ((TestNo - 100) < 0) TestStartNo = 0;
        //    else TestStartNo = TestNo - 100;

        //    for (int i = TestStartNo; i < TestNo; i++)
        //    {
        //        string header = SaveResult[i].Result_Header;
        //        if (header != null)
        //        {
        //            if (header.Contains("NF_" + Band + "_" + SParameters + "_" + TargetFreq.Replace(" M", "")) &&
        //                header.Contains("G0"))
        //            {
        //                PhaseDeltaOffset1 = GU.getGUcalfactor(1, header);
        //            }

        //            if (header.Contains("NF_" + Band + "_GEN38_" + SParameters + "_" + TargetFreq.Replace(" M", "")) &&
        //                header.Contains("G0"))
        //            {
        //                PhaseDeltaOffset2 = GU.getGUcalfactor(1, header);
        //            }
        //        }
        //    }

        //    double PhaseDelta1 = genSpec2[Band + "_NF_" + SplitMode[0] + "_" + Frequency + "_" + Selected_Port] +
        //                         PhaseDeltaOffset1;
        //    double PhaseDelta2 = genSpec2[Band + "_NF_" + SplitMode[0] + "_" + Frequency + "_" + Selected_Port + "_GEN38"] +
        //                         PhaseDeltaOffset2;

        //    double result = Math.Abs(PhaseDelta1 - PhaseDelta2);
        //    return result;
        //}

        #endregion Not supported
    }

    public class SparamSum : TestCaseCalcBase
    {
        #region "Declarations"

        /// <summary>
        /// Set Previous_Test_1 and 2.
        /// </summary>
        public string Previous_Info
        {
            get { return m_previousInfo; }
            set
            {
                // Unused.
                //string[] tmp_Info = value.Split(',');
                //if (tmp_Info.Length == 2)
                //{
                //    Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) - 1;
                //    Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) - 1;
                //}

                m_previousInfo = value;
            }
        }

        public s_Result Previous_Test_1_Result { get; set; }
        public s_Result Previous_Test_2_Result { get; set; }

        private string m_previousInfo;

        public bool b_Absolute;

        #endregion "Declarations"

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            double rtnResult;
            if (b_Absolute)
            {
                rtnResult = Math.Abs(Previous_Test_1_Result.Result_Data + Previous_Test_2_Result.Result_Data);
            }
            else
            {
                rtnResult = Previous_Test_1_Result.Result_Data + Previous_Test_2_Result.Result_Data;
            }

            return rtnResult;
        }


    }

    /// <summary>
    /// Only Previous_Test_1 is used not 2.
    /// </summary>
    public class SparamRelativeGainDelta : TestCaseCalcBase
    {
        #region "Declarations"

        /// <summary>
        /// Set Previous_Test_1 and 2.
        /// </summary>
        public string Previous_Info
        {
            get { return m_previousInfo; }
            set
            {
                string[] tmp_Info = value.Split(',');
                if (tmp_Info.Length == 2)
                {
                    Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) - 1;
                    //Previous_Test_2 = tmp_Info[1];
                    Previous_Test_2Int = Convert.ToInt32(tmp_Info[1]) - 1;
                }

                m_previousInfo = value;
            }
        }

        public s_Result Previous_Test_1_Result { get; set; }
        public s_Result Previous_Test_2_Result { get; set; }

        private string m_previousInfo;
        public int Previous_Test_1;
        public int Previous_Test_2Int;

        private string Previous_Test_2; //2018-11-01

        public bool b_Absolute;

        #endregion "Declarations"

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            double rtnResult;
            string errInfo = null;
            float value = 0;
            // This line always return value 0, because Previous_Test_2 is numeric instead of a name.
            //ATFResultBuilder.RecallResultByParameterName(Previous_Test_2, ref value, ref errInfo);
            if (b_Absolute)
            {
                rtnResult = Math.Abs(Math.Abs(Previous_Test_1_Result.Result_Data) - 
                                     Math.Abs(Previous_Test_2_Result.Result_Data));
            }
            else
            {
                rtnResult = Previous_Test_1_Result.Result_Data - Previous_Test_2_Result.Result_Data;
            }

            #region Old Method

            //double Previous_Data_1 = 0,
            //       Previous_Data_2 = 0;

            //if (SaveResult[Previous_Test_1].b_MultiResult)
            //{
            //    Previous_Data_1 = SaveResult[Previous_Test_1].Multi_Results[0].Result_Data; //Magnitude data for cFreq_At
            //}
            //else
            //{
            //    Previous_Data_1 = SaveResult[Previous_Test_1].Result_Data;
            //}

            //if (SaveResult[Previous_Test_2].b_MultiResult)
            //{
            //    Previous_Data_2 = SaveResult[Previous_Test_2].Multi_Results[0].Result_Data; //Magnitude data for cFreq_At
            //}
            //else
            //{
            //    Previous_Data_2 = SaveResult[Previous_Test_2].Result_Data;
            //}

            //if (!b_FixNumber)
            //{
            //    if (b_Absolute)
            //    {
            //        //rtnResult = Math.Abs(SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data);
            //        rtnResult = Math.Abs(Previous_Data_1 - Previous_Data_2);
            //    }
            //    else
            //    {
            //        //rtnResult = SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data;
            //        rtnResult = Previous_Data_1 - Previous_Data_2;
            //    }
            //}
            //else
            //{
            //    if (b_Absolute)
            //    {
            //        //rtnResult = Math.Abs(Convert.ToDouble(Fix_Number) - SaveResult[Previous_Test_1].Result_Data);
            //        rtnResult = Math.Abs(Convert.ToDouble(Fix_Number) - Previous_Data_1);
            //    }
            //    else
            //    {
            //        //rtnResult = Convert.ToDouble(Fix_Number) - SaveResult[Previous_Test_1].Result_Data;
            //        rtnResult = Convert.ToDouble(Fix_Number) - Previous_Data_1;
            //    }
            //}

            #endregion Old Method

            return rtnResult;
        }


    }
}