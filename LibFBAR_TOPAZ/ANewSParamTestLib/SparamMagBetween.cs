using System;
using System.Collections.Generic;
using System.Linq;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;
using LibFBAR_TOPAZ.ANewEqLib;

namespace LibFBAR_TOPAZ
{
    public class cMag_Between : TestCaseBase
    {
        public int TestNo;
        public double StartFreq;
        public double StopFreq;
        public string Search_MethodType;
        public string Non_Inverted;
        public string Use_Gain;
        public double Offset;
        public bool b_Absolute; //Seoul
        private MagBetweenSearchModel m_model;

        public override void InitSettings()
        {

            InitSparam();
            bool b_NonInvert = Non_Inverted.ToUpper() == "V";

            m_model = new MagBetweenSearchModel();
            m_model.Initialize(b_Interpolation, b_NonInvert, m_currentSp, SParamData[ChannelNumber - 1]);
            
            bool isPointFound = m_model.InitStartPoint(SegmentParam[ChannelNumber - 1], StartFreq);
            if (!isPointFound)
            {
                string msg = String.Format("Unable to find Start Point : Start Frequency = {0}", StartFreq);
                ShowError(this, msg);
            }

            isPointFound = m_model.InitEndPoint(SegmentParam[ChannelNumber - 1], StopFreq);
            if (!isPointFound)
            {
                string msg = String.Format("Unable to find Stop Point for Test Number {0}: Stop Frequency = {1}",
                    TestNo, StopFreq);
                ShowError(this, msg);
            }

            if (!m_model.IsDividerFound)
            {
                string msg = String.Format(
                    "Divider Value equal 0 for Test Number {0}, Start Frequency = {1}\nStop Frequency = {2}",
                    TestNo, StartFreq, StopFreq);
                ShowError(this, msg);
            }
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            e_SearchType SearchMethodType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_MethodType.ToUpper());

            double rtnResult = m_model.MeasureResult(SearchMethodType, Offset);
            // Unused.
            //if (Use_Gain.ToUpper() == "V")
            //{
            //    Use_Gain_Value = rtnResult;
            //}

            //SaveResult.Result_Data = ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
            if (rtnResult < -200)
                rtnResult = -200;

            if (rtnResult > 999 || rtnResult < -999) rtnResult = 999;

            if (SaveResult.IsHeaderContains("VSWR"))
            {
                rtnResult = Calculate_VSWR(rtnResult);
            }

            return rtnResult;
        }

        /// <summary>
        /// The only thing that comes out of SetResult() is RX_GAIN or RL_ANT
        /// because all headers are appended with one or the other without regard to what is really being measured.
        /// There is a keyword "GEN" that skips the publishing of the data altogether based on condition
        /// </summary>
        protected override void SetResult(double rtnResult)
        {
            if (b_Absolute) rtnResult = -(rtnResult); //Seoul
            base.SetResult(rtnResult);

            //Added by MM - 04/24/2017
            //Bypassing all that other jazz and just publishing the result
            try
            {
                SparamDelta.Genspec.Add(SaveResult.Result_Header, rtnResult);
            }
            catch (Exception ex)
            {
                string msg = String.Format(
                    "Test No:{0} The header being added is {1}\r\n" + "The data being recorded is {2}",
                    TestNo, SaveResult.Result_Header, rtnResult);
                PromptManager.Instance.ShowError(msg, ex);
                LoggingManager.Instance.LogInfo(msg);
            }
        }

        private double Calculate_VSWR(double ReturnLoss_dB)
        {
            double sagot = 0;
            double zExponent = (-1 * Math.Abs(ReturnLoss_dB)) / 20;
            double zNumerator = 1 + Math.Pow(10, zExponent);
            double zDenominator = 1 - Math.Pow(10, zExponent);

            return sagot = zNumerator / zDenominator;
        }

    }

    public class MagBetweenSearchModel
    {
        private double Divider = 0.000000001;

        private bool b_Interpolation_Low;
        private bool b_Interpolation_High;
        private bool b_NonInvert;

        private FrequencyPointDataObject m_freqStart;
        private FrequencyPointDataObject m_freqStop;
        private S_ParamData m_currentSp;
        private S_Param m_sp2;

        public void Initialize(bool isInterpolate, bool isNonInverted, S_ParamData currentSp, S_Param sp2)
        {
            if (isInterpolate)
            {
                b_Interpolation_High = true;
                b_Interpolation_Low = true;
            }
            else
            {
                b_Interpolation_High = false;
                b_Interpolation_Low = false;
            }

            m_currentSp = currentSp;
            m_sp2 = sp2;
            b_NonInvert = isNonInverted;
        }

        public bool InitStartPoint(s_SegmentTable st, double StartFreq)
        {
            int totalPoint = -1;
            s_SegmentData sd = FindStartPoint(st, StartFreq, ref totalPoint);
            bool isFound = sd.Points > -1;
            if (isFound)
            {
                InitStartPoint(sd, StartFreq, totalPoint);
            }

            return isFound;
        }

        public bool InitEndPoint(s_SegmentTable st, double StopFreq)
        {
            int totalPoint = -1;
            s_SegmentData sd = FindEndPoint(st, StopFreq, ref totalPoint);
            bool isFound = sd.Points > -1;
            if (isFound)
            {
                InitEndPoint(sd, StopFreq, totalPoint);
            }

            return isFound;
        }

        public bool IsDividerFound
        {
            get { return Divider != 0; }
        }

        public double MeasureResult(e_SearchType searchType, double offSet)
        {
            FrequencyRange ws = new FrequencyRange();
            ws.StartFreq = m_freqStart;
            ws.StopFreq = m_freqStop;
            ws.Initialize(m_currentSp);

            bool b_PositiveValue;

            if (!b_NonInvert)
            {
                if (m_currentSp.sParam[m_freqStart.FrequencyPoint].dBAng.dB > 0)
                {
                    b_PositiveValue = true;
                }
                else
                {
                    b_PositiveValue = false;
                }
            }
            else
            {
                b_PositiveValue = true;
            }

            double tmpResult = ws.SearchData(searchType, b_PositiveValue);

            //Modified by KCC (FreqCnt -1)
            //double Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt - 1);
            //double Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt - 1);
            //Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt);
            //Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt);
            double Rslt1 = GetInterpolatedDataLow();
            double Rslt2 = GetInterpolatedDataHigh();

            double rtnResult = ws.ProcessData(tmpResult, Rslt1, Rslt2, searchType, b_PositiveValue) + offSet;
            return rtnResult;
        }

        private double GetInterpolatedDataLow()
        {
            double tmpData = SearchInterpolatedData(b_Interpolation_Low,
                m_freqStart.Frequency, m_freqStart.FrequencyPoint - 1);
            return tmpData;
        }

        private double GetInterpolatedDataHigh()
        {
            double tmpData = SearchInterpolatedData(b_Interpolation_High, m_freqStop.Frequency, m_freqStop.FrequencyPoint - 1);
            return tmpData;
        }

        private double SearchInterpolatedData(bool isInterpolate, double Frequency, int FreqCnt)
        {
            double tmpData;

            if (isInterpolate == false)
            {
                tmpData = m_currentSp.sParam[m_freqStart.FrequencyPoint].dBAng.dB;
            }
            else
            {
                try
                {
                    double p1db = m_currentSp.sParam[FreqCnt].dBAng.dB;
                    double p2db = m_currentSp.sParam[FreqCnt + 1].dBAng.dB;
                    double p1x = m_sp2.Freq[FreqCnt];
                    double p2x = m_sp2.Freq[FreqCnt + 1];
                    double t1 = (p2db - p1db);
                    double t2 = (p2x - p1x);
                    //double t3 = (Frequency - p1x);
                    //double t4 = t1 / t2 * t3;
                    //double t5 = p1db;
                    //double t6 = t4 + t5;
                    //double tt = ((p2db - p1db) / (p2x - p1x) * (Frequency - p1x)) + p1db;
                    tmpData = ((p2db - p1db) / (p2x - p1x) * (Frequency - p1x)) + p1db;
                    tmpData = (t1 / t2 * (Frequency - p1x)) + p1db;
                }
                catch (Exception)
                {
                    tmpData = 0;
                    string meonly = Convert.ToString(Frequency);
                    ATFLogControl.Instance.Log(LogLevel.Error,LogSource.eTestPlan, "Likely cause is number of points specified in each segment does not match state file.");
                    //throw;
                }

            }

            return tmpData;
        }

        private s_SegmentData FindStartPoint(s_SegmentTable st, double StartFreq, ref int totalPoint)
        {
            int tmpCnt = 0;

            for (int seg = 0; seg < st.segm; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (StartFreq >= sd.Start && StartFreq <= sd.Stop)
                {
                    totalPoint = tmpCnt;
                    return sd;
                }

                tmpCnt += sd.Points;
            }

            s_SegmentData nullSd = new s_SegmentData();
            nullSd.Points = -1;
            return nullSd;
        }

        private void InitStartPoint(s_SegmentData sd, double StartFreq, int tmpCnt)
        {
            Divider = (sd.Stop - sd.Start) / (sd.Points - 1);
            //StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
            var tmpChk = (StartFreq - sd.Start) % Divider;
            double tmpChk2 = (StartFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1) + tmpCnt;

            m_freqStart = new FrequencyPointDataObject();
            if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
            {
                m_freqStart.Initialize(StartFreq, tmpChk2);
                b_Interpolation_Low = false;
            }
            else
            {
                m_freqStart.Initialize(StartFreq, tmpChk2 + 1);

            }
        }

        private s_SegmentData FindEndPoint(s_SegmentTable st, double StopFreq, ref int totalPoint)
        {
            int tmpCnt = 0;

            for (int seg = 0; seg < st.SegmentData.Length; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (StopFreq >= sd.Start && StopFreq <= sd.Stop)
                {
                    totalPoint = tmpCnt;
                    return sd;
                }
                tmpCnt += sd.Points;
            }

            s_SegmentData nullSd = new s_SegmentData();
            nullSd.Points = -1;
            return nullSd;
        }

        private void InitEndPoint(s_SegmentData sd, double StopFreq, int tmpCnt)
        {
            // Reuse divider value from start point.
            //StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
            double tmpChk = (StopFreq - sd.Start) % Divider;
            double tmpChk2 = (StopFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1) + tmpCnt;
            m_freqStop = new FrequencyPointDataObject();

            if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
            {
                //StopFreqCnt = seg + 1 + tmpCnt;
                //StopFreqCnt = Convert.ToInt32(((StopFreq - sd.Start) / (sd.Stop - sd.Start) * (SegmentParam[ChannelNumber-1].SegmentData[seg].Points -1)) + tmpCnt + 1);
                //KCC: Without + 1
                m_freqStop.Initialize(StopFreq, tmpChk2);
                b_Interpolation_High = false;
            }
            else
            {
                m_freqStop.Initialize(StopFreq, tmpChk2);
            }
        }

    }


    /// <summary>
    /// Use for Start and Stop frequency.
    /// </summary>
    public class FrequencyPointDataObject
    {
        public double Frequency { get; set; }
        public int FrequencyPoint { get; set; }

        public void Initialize(double freq, double freqPoint)
        {
            Frequency = freq;
            FrequencyPoint = Convert.ToInt32(freqPoint);
        }
    }

    /// <summary>
    /// Search for value within 2 frequency points.
    /// </summary>
    public class FrequencyRange
    {
        public FrequencyPointDataObject StartFreq { get; set; }
        public FrequencyPointDataObject StopFreq { get; set; }
        private S_ParamData m_currentSp;

        public void Initialize(S_ParamData currentSp)
        {
            m_currentSp = currentSp;
        }

        public double SearchData(e_SearchType searchType, bool PositiveValue)
        {
            double tmpResult = 0;

            try
            {
                tmpResult = m_currentSp.sParam[StartFreq.FrequencyPoint].dBAng.dB;
            }
            catch (Exception ex)
            {
                //PromptManager.Instance.ShowError("Error during SearchData " + ex.ToString() + "\r\n" +
                //                                 "The Channel Number is " + ChannelNumber.ToString() + "\r\n" +
                //                                 "StartFreqCnt is " + StartFreqCnt.ToString()
                //                                 + "\r\n" + "The Test Number is " + TestNo.ToString(), ex);
            }

            switch (searchType)
            {
                //Initial tmpResult value is the start frequency amplitude
                case e_SearchType.MAX:

                    #region MAX searchtype

                    if (PositiveValue)
                    {
                        tmpResult = SearchMinDb(tmpResult);
                    }
                    else
                    {
                        tmpResult = SearchMaxDb(tmpResult);
                    }

                    break;

                #endregion MAX searchtype

                case e_SearchType.MIN:

                    #region MIN searchtype

                    if (PositiveValue)
                    {
                        tmpResult = SearchMaxDb(tmpResult);
                    }
                    else
                    {
                        tmpResult = SearchMinDb(tmpResult);
                    }

                    break;

                #endregion MIN searchtype

                case e_SearchType.AVG:
                    tmpResult = SearchAverageDb();
                    break;
                case e_SearchType.dBdown15:

                    #region 15dBdown searchtype

                    //This is not usable yet, implementation is incomplete - MM 10/03/2017

                    #region Look for the max peak

                    //Look for the max peak
                    if (PositiveValue)
                    {
                        tmpResult = SearchMinDb(tmpResult);
                    }
                    else
                    {
                        tmpResult = SearchMaxDb(tmpResult);
                    }

                    #endregion Look for the max peak

                    //Find 15 dB down from max peak

                    //Get the frequency and set tmpResult

                    break;

                    #endregion 15dBdown searchtype
            }

            return tmpResult;
        }

        public double ProcessData(double Rslt, double Rslt1, double Rslt2,
            e_SearchType Search, bool PositiveValue)
        {
            double rtnRslt;
            //rtnRslt = -999;
            rtnRslt = Rslt;
            switch (Search)
            {
                case e_SearchType.MAX:
                    if (PositiveValue)
                    {
                        if (Rslt < Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }

                        if (Rslt < Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    }
                    else
                    {
                        if (Rslt > Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }

                        if (Rslt > Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    }

                    break;

                case e_SearchType.MIN:
                    if (PositiveValue)
                    {
                        if (Rslt > Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }

                        if (Rslt > Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    }
                    else
                    {
                        if (Rslt < Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }

                        if (Rslt < Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    }

                    break;
            }

            return rtnRslt;
        }

        private double SearchMinDb(double initialMinValue)
        {
            for (int i = StartFreq.FrequencyPoint; i <= StopFreq.FrequencyPoint; i++)
            {
                if (initialMinValue < m_currentSp.sParam[i].dBAng.dB)
                {
                    initialMinValue = m_currentSp.sParam[i].dBAng.dB;
                }
            }

            return initialMinValue;
        }

        private double SearchMaxDb(double initialMaxValue)
        {
            for (int i = StartFreq.FrequencyPoint; i <= StopFreq.FrequencyPoint; i++)
            {
                if (initialMaxValue > m_currentSp.sParam[i].dBAng.dB)
                {
                    initialMaxValue = m_currentSp.sParam[i].dBAng.dB;
                }
            }

            return initialMaxValue;
        }

        private double SearchAverageDb()
        {
            double r = SearchAverageDb(StartFreq, StopFreq);
            return r;
        }

        private double SearchAverageDb(FrequencyPointDataObject fStart, FrequencyPointDataObject fStop)
        {
            List<double> numstocrunch = new List<double>();
            for (int i = fStart.FrequencyPoint; i <= fStop.FrequencyPoint; i++)
            {
                numstocrunch.Add(dBmToWatts(m_currentSp.sParam[i].dBAng.dB));
            }

            double tmpResult = WattsTodBm(numstocrunch.Average());
            return tmpResult;
        }

        private double dBmToWatts(double dBmvalue)
        {
            double retval = -999;

            try
            {
                retval = Math.Pow(10, ((dBmvalue - 30) / 10));
            }
            catch (Exception e)
            {
                PromptManager.Instance.ShowError("Problem during dBmToWatts conversion: " + e.ToString());
            }

            return retval;
        }

        private double WattsTodBm(double Wattsvalue)
        {
            var retval = (10 * Math.Log10(Wattsvalue)) + 30;
            return retval;
        }
    }

}