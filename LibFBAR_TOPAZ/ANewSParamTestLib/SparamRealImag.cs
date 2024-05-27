using MPAD_TestTimer;
using System;

namespace LibFBAR_TOPAZ.ANewSParamTestLib
{
    public class cReal_At : TestCaseBase
    {
        public override void InitSettings()
        {
            InitWaveformInterpolation();
        }

        public override void RunTest()
        {
            double result = MeasureResult();
            SetResult(result);
        }

        private double MeasureResult()
        {
            double result;
            if (IsPreviousTestDefined)
            {
                int i_TargetFreqCnt = Previous_Test.GetResult().Misc;
                result = m_model1.GetRealAt(i_TargetFreqCnt);
            }
            else
            {
                result = m_model1.GetRealAt();
            }

            return result;
        }
    }

    public class cImag_At : TestCaseBase
    {
        public override void InitSettings()
        {
            InitWaveformInterpolation();
        }

        public override void RunTest()
        {
            double result = MeasureResult();
            SetResult(result);
        }

        private double MeasureResult()
        {
            double result;
            if (IsPreviousTestDefined)
            {
                int i_TargetFreqCnt = Previous_Test.GetResult().Misc;
                result = m_model1.GetImagAt(i_TargetFreqCnt);
            }
            else
            {
                result = m_model1.GetImagAt();
            }

            return result;
        }
    }

    public class cMag_At : TestCaseBase
    {
        #region "Declarations"

        public bool b_Absolute; //Seoul

        #endregion "Declarations"

        public override void InitSettings()
        {
            InitWaveformInterpolation();
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            double rtnResult;
            if (IsPreviousTestDefined)
            {
                int i_TargetFreqCnt = Previous_Test.GetResult().Misc;
                rtnResult = m_model1.GetMagAt(i_TargetFreqCnt);
            }
            else
            {
                rtnResult = m_model1.GetMagAt();
            }

            return rtnResult;
        }

        protected override void SetResult(double rtnResult)
        {
            if (b_Absolute) rtnResult = -(rtnResult); //Seoul
            base.SetResult(rtnResult);
        }
    }

    //KCC - Added for Apollo
    public class cMag_At_Lin : TestCaseBase
    {
        public override void InitSettings()
        {
            InitWaveformInterpolation();
        }

        public override void RunTest()
        {
            double result = MeasureResult();
            SetResult(result);
        }

        private double MeasureResult()
        {
            double rtnResult;
            if (IsPreviousTestDefined)
            {
                int i_TargetFreqCnt = Previous_Test.GetResult().Misc;
                rtnResult = m_model1.GetMagAtLin(i_TargetFreqCnt);
            }
            else
            {
                rtnResult = m_model1.GetMagAtLin();
            }

            return rtnResult;
        }
    }

    public class cPhase_At : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int TestNo;

        public string PowerMode;
        public string Band;
        public string Selected_Port;
        public double Target_Frequency;
        public string ParameterHeader;

        public string Frequency;

        #endregion "Declarations"

        public override void InitSettings()
        {
            Frequency_At = Target_Frequency;
            //InitWaveformInterpolation();

            InitSparam();
            //ChannelNumber = ChannelNumber - 1;
            //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
            if (IsPreviousTestDefined) return;

            m_model1.InitPhaseAt(m_currentSp, SegmentParam[ChannelNumber - 1], Frequency_At);
            if (m_model1.IsError)
            {
                ShowError(this, m_model1.ErrorMessage);
                SaveResult.Result_Data = -999;
            }
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }   //Function to call and run test. Not in used for FBAR

        private double MeasureResult()
        {
            double rtnResult = m_model1.GetPhaseAt();
            return rtnResult;
        }

        protected override void SetResult(double rtnResult)
        {
            base.SetResult(rtnResult);
            SetResultPhase(rtnResult);
        }

        private void SetResultPhase(double rtnResult)
        {
            if (!SaveResult.IsHeaderContains("PHASE")) return;

            string keyName = "";
            if (SaveResult.IsHeaderContains("GEN"))
            {
                string[] split = SaveResult.Result_Header.Split('_');
                string GenNB = "";
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].Contains("GEN"))
                    {
                        GenNB = split[i];
                        break;
                    }
                }

                keyName = string.Format("{0}_{1}_PHASE_{2}_{3}_{4}_{5}", Band, ParameterHeader, PowerMode, Frequency,
                    Selected_Port, GenNB);
                //Genspec.Add(SaveResult.Result_Header, rtnResult);
                SparamDelta.Genspec.Add(keyName, rtnResult);
            }
            else
            {
                keyName = string.Format("{0}_{1}_PHASE_{2}_{3}_{2}_{4}", Band, ParameterHeader, PowerMode, Frequency,
                    ChannelNumber);
                //Genspec.Add(SaveResult.Result_Header, rtnResult);
                try
                {
                    SparamDelta.Genspec.Add(keyName, rtnResult);
                }
                catch (Exception e)
                {
                    PromptManager.Instance.ShowError("Error adding this to Genspec " + "\r\n" + keyName);
                    string meupnow = "";

                    SparamDelta.Genspec.Add(keyName, rtnResult);
                    //throw;
                }
            }

            //else if(SaveResult.IsHeaderContains("GEN"))
            //{
            //    PhaseDelta.Add(Add, rtnResult);
            //}
        }
    }
}