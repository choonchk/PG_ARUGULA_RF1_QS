using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    public class cRipple_Between : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int TestNo;

        public string SwitchIn; //SEOUL
        public string SwitchOut;

        public double StartFreq;
        public double StopFreq;
        private double StepFreq;
        private double zRollingBW;
        private double zRollingInverval;
        public string PowerMode;
        public string Selected_Port;
        public bool b_Absolute;
        public string Sampling_BW;
        public string Sampling_Interval;
        private int Original_StopFreqCnt;

        public double Offset;

        private RippleBetweenSearchModel m_model;

        #endregion "Declarations"
        public cRipple_Between()
        {
            Sampling_BW = "0";
            Sampling_Interval = "0";
        }
        public override void InitSettings()
        {
            InitSparam();
            m_model = new RippleBetweenSearchModel();
            m_model.Initialize(SegmentParam[ChannelNumber - 1]);
            bool isFound = m_model.FindStart(StartFreq);
            if (!isFound)
            {
                string msg = String.Format("Unable to find Start Point : Start Frequency = {0}", StartFreq);
                ShowError(this, msg);
            }

            isFound = m_model.FindEnd(StopFreq);
            if (!isFound)
            {
                string msg = String.Format("Unable to find Stop Point for Test Number {0}: Stop Frequency = {1}",
                    TestNo, StopFreq);
                ShowError(this, msg);
            }

            m_model.InitializeRolling(Sampling_BW, Sampling_Interval);

            if (!m_model.IsDividerFound)
            {
                string msg = String.Format("Divider Value equal 0 for Test Number {0}, Start Frequency = {1}\nStop Frequency = {2}",
                    TestNo, StartFreq, StopFreq);
                ShowError(this, msg);
            }
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        /// <summary>
        /// If Sampling BW & Interval columns are defined, use the rolling BW method with window and step. Otherwise use the simple method.
        /// </summary>
        /// <returns></returns>
        private double MeasureResult()
        {
            double rtnResult = 0;
            bool isBwNotDefined = Sampling_BW == "0" || Sampling_Interval == "0" || 
                String.IsNullOrEmpty(Sampling_BW) || String.IsNullOrEmpty(Sampling_Interval);
            try
            {
                if (!isBwNotDefined)
                {
                    // Case HLS2. Rolling window.
                    rtnResult = m_model.MeasureResult2(m_currentSp);
                }
                else
                {
                    // Case Joker.
                    rtnResult = m_model.MeasureResult(m_currentSp);
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Error happened during Ripple_Between, MeasureResult." + "\r\n{0}\r\n\r\n" + "CH {1}\r\n" + "Test Number is {2}\r\n\r\n" + "Exception will be thrown.", e, ChannelNumber, TestNo);
                MessageBox.Show(msg);
                //throw;
            }

            if (b_Absolute)
            {
                rtnResult = Math.Abs(rtnResult);
            }

            return rtnResult;
        }

        protected override void SetResult(double rtnResult)
        {
            if (rtnResult > 999 || rtnResult < -999) rtnResult = 999;
            base.SetResult(rtnResult);
        }
    }

    public class RippleBetweenSearchModel
    {
        private double StepFreq;
        private double PartialGradient_Start;
        private int StartFreqCnt;

        private int StopFreqCnt;
        private double PartialGradient_Stop;
        private s_SegmentTable m_st;
        double Divider = 0.000000001;
        private double zRollingBW;
        private double zRollingInverval;
        private int Original_StopFreqCnt;

        public void Initialize(s_SegmentTable st)
        {
            m_st = st;
        }

        public bool FindStart(double startFreq)
        {

            int tmpCnt = 0;
            bool Found = false;
            for (int seg = 0; seg < m_st.segm; seg++)
            {
                s_SegmentData sd = m_st.SegmentData[seg];
                if (startFreq >= sd.Start && startFreq < sd.Stop)
                {
                    Found = true;
                    Divider = (sd.Stop - sd.Start) / (sd.Points - 1);
                }
                else
                {
                    tmpCnt += sd.Points;
                }
                if (Found == true)
                {
                    double tmpChk = ((startFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt;
                    StartFreqCnt = (int)Math.Floor(tmpChk);    //Remove the Decimal Point
                    StepFreq = (sd.Stop - sd.Start) / (sd.Points - 1);
                    PartialGradient_Start = (startFreq - (((StartFreqCnt - tmpCnt) * StepFreq) + sd.Start)) / StepFreq;
                    StartFreqCnt++;
                    break;
                }
            }

            return Found;
        }

        public bool FindEnd(double stopFreq)
        {
            int tmpCnt = 0;
            bool Found = false;
            for (int seg = 0; seg < m_st.SegmentData.Length; seg++)
            {
                s_SegmentData sd = m_st.SegmentData[seg];
                if (stopFreq >= sd.Start && stopFreq <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }
                if (Found == true)
                {
                    double tmpChk = ((stopFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt;
                    StopFreqCnt = (int)Math.Floor(tmpChk);    //Remove the Decimal Point
                    StepFreq = (sd.Stop - sd.Start) / (sd.Points - 1);
                    PartialGradient_Stop = (stopFreq - (((StopFreqCnt - tmpCnt) * StepFreq) + sd.Start)) / StepFreq;
                    StopFreqCnt++;
                    break;
                }
            }

            return Found;
        }

        public void InitializeRolling(string samplingBw, string samplingInterval)
        {
            if (String.IsNullOrEmpty(samplingBw))
            {
                samplingBw = "0";
            }
            if (String.IsNullOrEmpty(samplingInterval))
            {
                samplingInterval = "0";
            }
            zRollingBW = ConvertSEtoNum(samplingBw, false);
            zRollingInverval = ConvertSEtoNum(samplingInterval, false);
            Original_StopFreqCnt = StopFreqCnt;
        }

        public bool IsDividerFound
        {
            get { return Divider != 0; }
        }

        public double MeasureResult(S_ParamData currentSp)
        {
            var Rslt_Max = currentSp.sParam[StartFreqCnt].dBAng.dB +
                           (PartialGradient_Start *
                            (currentSp.sParam[StartFreqCnt + 1].dBAng.dB - currentSp.sParam[StartFreqCnt].dBAng.dB));

            var Rslt_Min = Rslt_Max;

            var tmpRslt = currentSp.sParam[StopFreqCnt].dBAng.dB +
                          (PartialGradient_Stop *
                           (currentSp.sParam[StopFreqCnt + 1].dBAng.dB - currentSp.sParam[StopFreqCnt].dBAng.dB));

            MaxMinComparator(ref Rslt_Min, ref Rslt_Max, tmpRslt);

            double[] dsads = new double[StopFreqCnt - StartFreqCnt];
            int iii = 0;
            for (int iArr = StartFreqCnt; iArr < StopFreqCnt; iArr++)
            {
                dsads[iii] = currentSp.sParam[iArr].dBAng.dB;
                tmpRslt = currentSp.sParam[iArr].dBAng.dB;
                MaxMinComparator(ref Rslt_Min, ref Rslt_Max, tmpRslt);
                iii++;
            }
            //for (int i = 0; i < m_currentSp.sParam.Length - 1; i++)
            //{
            //    dsads1[iiii] = m_currentSp.sParam[iiii].dBAng.dB;
            //    iiii++;
            //}
            double rtnResult = Rslt_Max - Rslt_Min;
            return rtnResult;
        }

        public double MeasureResult2(S_ParamData currentSp)
        {
            List<double> zdelta_container = new List<double>();
            List<double> zval_holder = new List<double>();
            StepFreq = Math.Round(StepFreq, MidpointRounding.AwayFromZero);
            int lc_span = Original_StopFreqCnt - StartFreqCnt; //this is the span
            int bw_cnt = (int)(zRollingBW / StepFreq); //determine how many counts is the sampling BW
            int sampling_interval_count =
                (int)(zRollingInverval / StepFreq); //determine how many counts is the sampling interval

            if (sampling_interval_count <= 0
            ) //In case the sampling interval specified from the TCF is too small, for the value to be at least the size of the frequency step
            {
                sampling_interval_count = 1;
            }

            if (bw_cnt <= 0)
            {
                bw_cnt = 1;
            }

            ////StartFreqCnt dB + (partial gradient * delta between StartFreqCnt+1 dB - StartFreqCnt dB)
            //Rslt_Max = sp.sParam[StartFreqCnt].dBAng.dB + (PartialGradient_Start * (sp.sParam[StartFreqCnt + 1].dBAng.dB - sp.sParam[StartFreqCnt].dBAng.dB));
            //Rslt_Min = Rslt_Max;
            //tmpRslt = sp.sParam[StopFreqCnt].dBAng.dB + (PartialGradient_Stop * (sp.sParam[StopFreqCnt + 1].dBAng.dB - sp.sParam[StopFreqCnt].dBAng.dB));

            //Description:
            //The loop will go through the whole span of interest
            //1.  StopFreqCnt will be updated to be the StartFreqCnt + the number of indexes equivalent to the sampling BW
            //2.  The internal for loop will iterate from the StartFreqCnt to StopFreqCnt and store all the values in the zval_holder list
            //3.  Rslt_Max and Rslt_Min are found from the list
            //4.  The difference between Min and Max are found and stuffed in the zdelta_container list
            //5.  zval_holder list is cleared to be ready for next iteration.
            for (int i = StartFreqCnt; i <= (Original_StopFreqCnt - bw_cnt); i += sampling_interval_count)
            {
                StopFreqCnt = (i + bw_cnt > Original_StopFreqCnt ? Original_StopFreqCnt : i + bw_cnt);

                for (int iArr = i; iArr <= StopFreqCnt; iArr++)
                {
                    double tmpRslt = currentSp.sParam[iArr].dBAng.dB;
                    zval_holder.Add(tmpRslt);
                }

                double Rslt_Max = zval_holder.Max();
                double Rslt_Min = zval_holder.Min();
                zdelta_container.Add(Math.Abs(Rslt_Max - Rslt_Min));
                zval_holder.Clear();
            }

            var rtnResult = zdelta_container.Max();
            return rtnResult;
        }

        private void MaxMinComparator(ref double MinValue, ref double MaxValue, double parseValue)
        {
            if (parseValue > MaxValue)
            {
                MaxValue = parseValue;
            }
            if (parseValue < MinValue)
            {
                MinValue = parseValue;
            }
        }

        private double ConvertSEtoNum(string znum, bool normalizetoMHz)
        {
            double x = 0;
            string y = "";
            if (znum.ToUpper().Contains("K"))
            {
                x = 1e+3;
                y = znum.Split('K')[0].Trim();
            }
            else if (znum.ToUpper().Contains("M"))
            {
                x = 1e+6;
                y = znum.Split('M')[0].Trim();
            }
            else if (znum.ToUpper().Contains("G"))
            {
                x = 1e+9;
                y = znum.Split('G')[0].Trim();
            }
            else
            {
                //This assumes the number passed is in Hz
                x = 1;
                y = znum.Trim();
            }

            x = Convert.ToDouble(y) * x;

            if (normalizetoMHz)
            {
                x = x / 1e+6;
            }

            return x;
        }
    }

}