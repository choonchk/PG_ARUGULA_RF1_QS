using System;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    public class cFreq_At : TestCaseBase
    {
        public enum e_SearchDirection
        {
            NONE = 0,
            FROM_LEFT,
            FROM_RIGHT,
            FROM_MAX_LEFT,
            FROM_MAX_RIGHT,
            FROM_EXTREME_LEFT,
            FROM_EXTREME_RIGHT,
        }

        public class Waveform
        {
            // Input variable.
            public int Start_i;
            public int Stop_i;
            public int Step_i;
            // Output Variable.
            public double rtnResult;
            /// <summary>
            /// Set by Freq_At, read by Real_at, Imag_At, Mag_At for Use_Previous case.
            /// It is the frequency point found by Freq_At.
            /// </summary>
            public int rtnMiscResult;
        }

        #region "Declarations"

        // External Variables
        public int TestNo;

        public double StartFreq;
        public double StopFreq;
        public string Search_DirectionMethod;
        public string Search_Type;
        public string PowerMode;
        public string Selected_Port;
        public string Search_Value = "";
        public bool b_Invert_Search;
        public double Offset;
        public string Use_Gain; //2018-03-07 Seoul

        // Internal Variables
        private e_SearchDirection SearchDirection;

        private e_SearchType SearchType;
        private double SearchValue;
        private double Use_Gain_Value; //2018-03-07 Seoul
        private FreqAtSearchModel m_model;

        #endregion "Declarations"

        public override void InitSettings()
        {
            Use_Gain = String.Empty;

            ////ChannelNumber--;
            SearchDirection = (e_SearchDirection)Enum.Parse(typeof(e_SearchDirection), Search_DirectionMethod.ToUpper());

                m_model = new FreqAtSearchModel();
            if ((SearchDirection == e_SearchDirection.FROM_EXTREME_LEFT) || (SearchDirection == e_SearchDirection.FROM_EXTREME_RIGHT))
            {
                m_model.Initialize(StartFreq, SParamData[ChannelNumber - 1]);
            }
            else
            {
                m_model.Initialize(SegmentParam, ChannelNumber, StartFreq, StopFreq);
            }
            InitSparam();

            if (Search_Value.Trim() == "")
            {
                SearchValue = -9999;
                SearchType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_Type.ToUpper());
            }
            else
            {
                SearchValue = Convert.ToDouble(Search_Value);
                SearchType = e_SearchType.USER;
                if ((SearchValue < 0) && !((SearchDirection == e_SearchDirection.FROM_MAX_LEFT) || (SearchDirection == e_SearchDirection.FROM_MAX_RIGHT)))
                {
                    b_Invert_Search = !b_Invert_Search;
                }
            }

        }

        public override void RunTest()
        {
            Waveform w = MeasureResult();
            SetResult(w);
        }   //Function to call and run test. Not in used for FBAR

        private Waveform MeasureResult()
        {
            FrequencyRange2 ws = new FrequencyRange2();
            ws.StartFreq = m_model.StartFreq;
            ws.StopFreq = m_model.StopFreq;
            ws.Initialize(m_currentSp, SParamData[ChannelNumber - 1]);
            Waveform w = new Waveform();

            if (IsPreviousTestDefined)
            {
                SearchValue = Convert.ToDouble(Search_Value);
                double Gain_Value = Previous_Test.GetResult().Result_Data;
                SearchValue = Gain_Value + SearchValue;
            }
            //if (Use_Gain.ToUpper() != "" && Use_Gain.ToUpper() != "V") //2018-03-07 Seoul
            //{
            //    double Gain_Value;
            //    SearchValue = Convert.ToDouble(Search_Value);
            //    Gain_Value = SaveResult[Int32.Parse(Use_Gain) - 1].Result_Data;
            //    SearchValue = Gain_Value + SearchValue;
            //}

            try
            {
                int Peak_Pos_i = 0;
                if (SearchDirection != e_SearchDirection.FROM_MAX_LEFT &&
                    SearchDirection != e_SearchDirection.FROM_MAX_RIGHT)
                {
                    
                }
                else
                {
                    Peak_Pos_i = ws.GetPeakPosition();
                }

                w = ws.GetSearchStartStopStep(SearchDirection, Peak_Pos_i);

                if (SearchType == e_SearchType.MAX)
                {
                    ws.SearchMax(w);
                }
                else if (SearchType == e_SearchType.MIN)
                {
                    ws.SearchMin(w);
                }
                else
                {
                    ws.SearchNotMaxMin(w, SearchValue, b_Invert_Search, b_Interpolation);
                }

                if (Use_Gain.ToUpper() == "V") //2018-03-07 Seoul
                {
                    Use_Gain_Value = w.rtnResult;
                }

                return w;

            }
            catch
            {
                string msg = String.Format("Test Number = {0}Start = {1}Stop = {2}Step = {3}tmp_Peak = {4}",
                    TestNo, w.Start_i, w.Stop_i, w.Step_i, w.rtnMiscResult);
                ShowError(this, msg);
            }

            return w;
        }

        private void SetResult(Waveform w)
        {
            SetResult(w.rtnResult);
            SaveResult.Misc = w.rtnMiscResult;
        }
    }

    public class FreqAtSearchModel
    {
        private s_SegmentTable[] m_segmentParam;
        public FrequencyPointDataObject StartFreq;
        public FrequencyPointDataObject StopFreq;


        public void Initialize(s_SegmentTable[] segment, int Channel_Number, double startFreq, double stopFreq)
        {
            m_segmentParam = segment;
            int StartFreq2 = GetStartFreqCnt(m_segmentParam[Channel_Number - 1], startFreq);
            int StopFreq2 = GetStopFreqCnt(m_segmentParam[Channel_Number - 1], stopFreq);
            this.StartFreq = new FrequencyPointDataObject();
            this.StartFreq.Initialize(startFreq, StartFreq2);
            this.StopFreq = new FrequencyPointDataObject();
            this.StopFreq.Initialize(stopFreq, StopFreq2);
        }

        public void Initialize(double startFreq, S_Param sp)
        {
            this.StartFreq = new FrequencyPointDataObject();
            this.StartFreq.Initialize(0, startFreq);
            this.StopFreq = new FrequencyPointDataObject();
            this.StopFreq.Initialize(0, sp.NoPoints - 1);
        }

        private int GetStartFreqCnt(s_SegmentTable st, double startFreq)
        {
            int tmpCnt = 0;
            int StartFreqCnt = 0;

            for (int seg = 0; seg < st.segm; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (startFreq >= sd.Start && startFreq < sd.Stop)
                {
                    StartFreqCnt = CalculateStartStopFreq(sd, startFreq, tmpCnt);
                    break;
                }

                tmpCnt += sd.Points;
            }

            return StartFreqCnt;
        }

        private int GetStopFreqCnt(s_SegmentTable st, double stopFreq)
        {
            int tmpCnt = 0;
            int StopFreqCnt = 0;

            for (int seg = 0; seg < st.SegmentData.Length; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (stopFreq > sd.Start && stopFreq <= sd.Stop)
                {
                    StopFreqCnt = CalculateStartStopFreq(sd, stopFreq, tmpCnt);
                    break;
                }

                tmpCnt += sd.Points;
            }

            return StopFreqCnt;
        }

        private int CalculateStartStopFreq(s_SegmentData seg, double stopFreq, int tmpCnt)
        {
            double v1 = ((stopFreq - seg.Start) / (seg.Stop - seg.Start) * (seg.Points - 1)) + tmpCnt;
            int cnt = Convert.ToInt32(v1);
            return cnt;
        }
    }

    /// <summary>
    /// Search for value within 2 frequency points.
    /// </summary>
    public class FrequencyRange2
    {
        public FrequencyPointDataObject StartFreq { get; set; }
        public FrequencyPointDataObject StopFreq { get; set; }
        private S_ParamData m_currentSp;
        private S_Param m_sp2;

        public void Initialize(S_ParamData currentSp, S_Param sp2)
        {
            m_currentSp = currentSp;
            m_sp2 = sp2;

        }

        public int GetPeakPosition()
        {
            int Peak_Pos_i = 0;
            // Look for Peak Value and Position First
            double Peak_Value = -999999;
            for (int iArr = StartFreq.FrequencyPoint; iArr <= StopFreq.FrequencyPoint; iArr++)
            {
                double db = m_currentSp.sParam[iArr].dBAng.dB;
                if (db > Peak_Value)
                {
                    Peak_Value = db;
                    Peak_Pos_i = iArr;
                }
            }

            return Peak_Pos_i;
        }

        public cFreq_At.Waveform GetSearchStartStopStep(cFreq_At.e_SearchDirection searchDirection, int Peak_Pos_i)
        {
            cFreq_At.Waveform w = new cFreq_At.Waveform();
            switch (searchDirection)
            {
                case cFreq_At.e_SearchDirection.FROM_LEFT:
                case cFreq_At.e_SearchDirection.FROM_EXTREME_LEFT:
                    w.Start_i = StartFreq.FrequencyPoint;
                    w.Stop_i = StopFreq.FrequencyPoint;
                    w.Step_i = 1;
                    if (w.Start_i == w.Stop_i) w.Step_i = 0;
                    break;

                case cFreq_At.e_SearchDirection.FROM_RIGHT:
                case cFreq_At.e_SearchDirection.FROM_EXTREME_RIGHT:
                    w.Start_i = StopFreq.FrequencyPoint;
                    w.Stop_i = StartFreq.FrequencyPoint;
                    w.Step_i = -1;
                    if (w.Start_i == w.Stop_i) w.Step_i = 0;
                    break;

                case cFreq_At.e_SearchDirection.FROM_MAX_LEFT:
                    w.Start_i = Peak_Pos_i;
                    w.Stop_i = StartFreq.FrequencyPoint;
                    w.Step_i = -1;
                    if (w.Start_i == w.Stop_i) w.Step_i = 0;  // f the Max Point at Extreme End
                    break;

                case cFreq_At.e_SearchDirection.FROM_MAX_RIGHT:
                    w.Start_i = Peak_Pos_i;
                    w.Stop_i = StopFreq.FrequencyPoint;
                    w.Step_i = 1;
                    if (w.Start_i == w.Stop_i) w.Step_i = 0;  // f the Max Point at Extreme End
                    break;
            }

            return w;
        }

        public void SearchMax(cFreq_At.Waveform w)
        {
            if (w.Step_i == 0)
            {
                // Error pervention
                w.rtnResult = m_sp2.Freq[w.Start_i];
                w.rtnMiscResult = w.Start_i;
            }
            else
            {
                double compareResult = -90000000;
                double rtnResult = -90000000;
                int tmp_i = w.Start_i;
                //tmp_Peak_i = 0;
                do
                {
                    if (m_currentSp.sParam[tmp_i].dBAng.dB > compareResult)
                    {
                        rtnResult = m_sp2.Freq[tmp_i];
                        compareResult = m_currentSp.sParam[tmp_i].dBAng.dB;
                        w.rtnMiscResult = tmp_i;
                    }
                    tmp_i += w.Step_i;
                } while (tmp_i != w.Stop_i);

                w.rtnResult = rtnResult;
            }
        }

        public void SearchMin(cFreq_At.Waveform w)
        {
            int tmp_i;

            if (w.Step_i == 0)
            {
                // Error pervention
                w.rtnResult = m_sp2.Freq[w.Start_i];
                w.rtnMiscResult = w.Start_i;
            }
            else
            {
                double compareResult = 90000000;
                double rtnResult = 90000000;
                tmp_i = w.Start_i;
                //tmp_Peak_i = 0;
                do
                {
                    if (m_currentSp.sParam[tmp_i].dBAng.dB < compareResult)
                    {
                        rtnResult = m_sp2.Freq[tmp_i];
                        compareResult = m_currentSp.sParam[tmp_i].dBAng.dB;
                        w.rtnMiscResult = tmp_i;
                    }
                    tmp_i += w.Step_i;
                } while (tmp_i != w.Stop_i);

                w.rtnResult = rtnResult;
            }
        }

        public void SearchNotMaxMin(cFreq_At.Waveform w, double searchValue, bool isInvertSearch,
            bool isInterpolate)
        {
            if (w.Step_i == 0)
            {
                // Error pervention
                w.rtnResult = m_sp2.Freq[w.Start_i];
                w.rtnMiscResult = w.Start_i;
                return;
            }

            int tmp_i = w.Start_i;
            int Remain_i;
            int tmp_Peak_i = 0;
            double rtnResult;

            do
            {
                if (isInvertSearch)
                {
                    if ((searchValue > m_currentSp.sParam[tmp_i].dBAng.dB) &&
                        (searchValue < m_currentSp.sParam[tmp_i + w.Step_i].dBAng.dB))
                    {
                        tmp_Peak_i = tmp_i;
                        break;
                    }
                }
                else
                {
                    if ((searchValue < m_currentSp.sParam[tmp_i].dBAng.dB) &&
                        (searchValue > m_currentSp.sParam[tmp_i + w.Step_i].dBAng.dB))
                    {
                        tmp_Peak_i = tmp_i;
                        break;
                    }
                }
                tmp_i = tmp_i + w.Step_i;
                Remain_i = Math.Abs(w.Stop_i - tmp_i);
            } while (Remain_i > 0);

            if (isInterpolate)
            {
                if (tmp_Peak_i != 0)
                {
                    double p1y = m_currentSp.sParam[tmp_Peak_i].dBAng.dB;
                    double p1x = m_sp2.Freq[tmp_Peak_i];
                    double p2y = m_currentSp.sParam[tmp_Peak_i + w.Step_i].dBAng.dB;
                    double p2x = m_sp2.Freq[tmp_Peak_i + w.Step_i];
                    var Gradient = (p2x - p1x) / (p2y - p1y);

                    rtnResult = ((searchValue - p1y) * Gradient) + p1x;
                }
                else
                {
                    rtnResult = m_sp2.Freq[tmp_Peak_i];
                }
            }
            else
            {
                double p1y = Math.Abs(searchValue - m_currentSp.sParam[tmp_Peak_i].dBAng.dB);
                double p2y = Math.Abs(searchValue - m_currentSp.sParam[tmp_Peak_i + w.Step_i].dBAng.dB);
                if (p1y < p2y)
                {
                    rtnResult = m_sp2.Freq[tmp_Peak_i];
                }
                else
                {
                    rtnResult = m_sp2.Freq[tmp_Peak_i + w.Step_i];
                }
            }

            w.rtnResult = rtnResult;
            w.rtnMiscResult = tmp_Peak_i;
        }


    }
}