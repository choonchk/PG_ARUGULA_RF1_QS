using System;
using System.Collections.Generic;
using System.Diagnostics;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    public enum e_SearchType
    {
        MIN = 0,
        MAX,
        USER,
        AVG,
        dBdown15,
    }


    public class cMag_Sum_Between : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int TestNo;

        public double StartFreq;
        public double StopFreq;
        public string Search_MethodType;
        public string PowerMode; //Seoul
        public string Band;
        public string Non_Inverted;
        public string Use_Gain;
        public string SwitchIn;
        public string SwitchOut;
        private double Use_Gain_Value;

        public double Offset;

        // Internal Variables
        private e_SearchType SearchMethodType;

        private bool b_NonInvert;
        private bool b_Interpolation_High;
        private bool b_Interpolation_Low;
        public bool b_Absolute; //Seoul

        private int StartFreqCnt;
        private int StopFreqCnt;

        private int tmpCnt;
        private double tmpChk;

        private bool Found;

        #endregion "Declarations"

        public override void InitSettings()
        {
            double Divider;

            ////ChannelNumber--;

            SearchMethodType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_MethodType.ToUpper());

            InitSparam();

            Divider = 0.000000001;
            if (b_Interpolation == true)
            {
                b_Interpolation_High = true;
                b_Interpolation_Low = true;
            }
            else
            {
                b_Interpolation_High = false;
                b_Interpolation_Low = false;
            }

            #region "Start Point"

            tmpCnt = 0;
            Found = false;
            for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].segm; seg++)
            {
                s_SegmentData sd = SegmentParam[ChannelNumber - 1].SegmentData[seg];
                if (StartFreq >= sd.Start && StartFreq <= sd.Stop)
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
                    //StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
                    tmpChk = (StartFreq - sd.Start) % Divider;
                    if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
                    {
                        //StartFreqCnt = seg + tmpCnt;
                        StartFreqCnt = Convert.ToInt32(((StartFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt);
                        b_Interpolation_Low = false;
                    }
                    else
                    {
                        StartFreqCnt = Convert.ToInt32(((StartFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt + 1);
                    }
                    break;
                }
            }
            if (Found == false)
            {
                string msg = String.Format("Unable to find Start Point : Start Frequency = {0}", StartFreq);
                ShowError(this, msg);
            }

            #endregion "Start Point"

            #region "End Point"

            tmpCnt = 0;
            Found = false;
            for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].SegmentData.Length; seg++)
            {
                s_SegmentData sd = SegmentParam[ChannelNumber - 1].SegmentData[seg];
                if (StopFreq >= sd.Start && StopFreq <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }
                if (Found == true)
                {
                    //StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
                    tmpChk = (StopFreq - sd.Start) % Divider;
                    if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
                    {
                        //StopFreqCnt = seg + 1 + tmpCnt;
                        //StopFreqCnt = Convert.ToInt32(((StopFreq - sd.Start) / (sd.Stop - sd.Start) * (SegmentParam[ChannelNumber-1].SegmentData[seg].Points -1)) + tmpCnt + 1);
                        //KCC: Without + 1
                        StopFreqCnt = Convert.ToInt32(((StopFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt);
                        b_Interpolation_High = false;
                    }
                    else
                    {
                        StopFreqCnt = Convert.ToInt32(((StopFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt);
                    }
                    break;
                }
            }
            if (Found == false)
            {
                string msg = String.Format("Unable to find Stop Point for Test Number {0}: Stop Frequency = {1}",
                    TestNo, StopFreq);
                ShowError(this, msg);
            }

            #endregion "End Point"

            if (Divider == 0)
            {
                string msg = String.Format("Divider Value equal 0 for Test Number {0}, Start Frequency = {1}\nStop Frequency = {2}",
                    TestNo, StartFreq, StopFreq);
                ShowError(this, msg);
            }

            if (Non_Inverted.ToUpper() == "V")
            {
                b_NonInvert = true;
            }
            else
            {
                b_NonInvert = false;
            }
            //ChannelNumber = ChannelNumber - 1;
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            double rtnResult;

            bool b_PositiveValue;
            double tmpResult;
            double Rslt1, Rslt2;

            if (!b_NonInvert)
            {
                if (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[StartFreqCnt].dBAng.dB > 0)
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

            tmpResult = SearchData(SearchMethodType, b_PositiveValue);

            //Modified by KCC (FreqCnt -1)
            // Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt - 1);
            //Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt - 1);
            //Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt);
            //Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt);

            rtnResult = tmpResult; // ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
                                   //SaveResult.Result_Data = ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
            return rtnResult;
        }

        protected override void SetResult(double rtnResult)
        {
            if (TestNo == 16)
            {
                double yy;

                yy = 11;
            }
            if (rtnResult < -200)
                rtnResult = -200;

            if (rtnResult > 999 || rtnResult < -999) rtnResult = 999;
            if (b_Absolute) rtnResult = -(rtnResult); //Seoul

            base.SetResult(rtnResult);
        }

        // Additional Function Codes
        private double SearchData(e_SearchType Search, bool PositiveValue)
        {
            double tmpResult;

            tmpResult = 0;
            int point = 0;
            for (int i = StartFreqCnt; i <= StopFreqCnt; i++)
            {
                //tmpResult += SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i].dBAng.dB;
                tmpResult += Math.Sqrt(Math.Pow(SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i].ReIm.Real, 2) + Math.Pow(SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i].ReIm.Imag, 2));
                point++;
            }
            //tmpResult = tmpResult / point;
            tmpResult = 20 * Math.Log10(tmpResult / point);

            return (tmpResult);
        }
    }

    public class cCPL_Between : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int TestNo;

        public double StartFreq;
        public double StopFreq;
        public string Search_MethodType;
        public string SParameters1;
        public string SParameters2;

        public double Offset;

        // Internal Variables
        private cMag_Between MagBetween1;

        private cMag_Between MagBetween2;
        //private bool RaiseError;    //Error Checking to prevent error during measure or run test.

        #endregion "Declarations"

        public override void InitSettings()
        {
            //ChannelNumber--;
            MagBetween1 = new cMag_Between();
            MagBetween1.TestNo = TestNo;
            MagBetween1.ChannelNumber = ChannelNumber;
            MagBetween1.Interpolation = Interpolation;
            MagBetween1.Search_MethodType = Search_MethodType;
            MagBetween1.SParameters = SParameters1;
            MagBetween1.StartFreq = StartFreq;
            MagBetween1.StopFreq = StopFreq;

            MagBetween2 = new cMag_Between();
            MagBetween2.TestNo = TestNo;
            MagBetween2.ChannelNumber = ChannelNumber;
            MagBetween2.Interpolation = Interpolation;
            MagBetween2.Search_MethodType = Search_MethodType;
            MagBetween2.SParameters = SParameters2;
            MagBetween2.StartFreq = StartFreq;
            MagBetween2.StopFreq = StopFreq;

            MagBetween1.InitSettings();
            MagBetween2.InitSettings();
        }

        public override void RunTest()
        {
            double rtnResult = MeasureResult();
            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            MagBetween1.RunTest();
            MagBetween2.RunTest();
            double mb1 = MagBetween1.GetResult().Result_Data;
            double mb2 = MagBetween2.GetResult().Result_Data;

            double rtnResult = mb1 - mb2 + Offset;
            return rtnResult;
        }
    }

    public class cNF_Topaz_At : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int TestNo;

        public string Band;
        public string PowerMode;
        public string Frequency;
        public string Selected_Port;
        public double Offset;
        // Internal Variables
        private TopazEquipmentDriver m_equipment;

        #endregion "Declarations"

        public TopazEquipmentDriver EquipmentENA
        {
            get
            {
                return m_equipment;
            }
            set
            {
                m_equipment = value;
            }
        }

        public override void InitSettings()
        {
            // InitSettings is not called.
            //InitWaveformInterpolation();
        }

        public override void RunTest()
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();

            double rtnResult = MeasureResult();

            watch.Stop();
            long NFTestTime = watch.ElapsedMilliseconds;

            SetResult(rtnResult);
        }

        public void RunGainTest()
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();

            double rtnResult = MeasureGainResult();

            watch.Stop();
            long NFTestTime = watch.ElapsedMilliseconds;

            SetResult(rtnResult);
        }

        private double MeasureResult()
        {
            double rtnResult = m_equipment.GetNfResult(Frequency_At, ChannelNumber);
            return rtnResult;
        }

        private double MeasureGainResult()
        {
            double rtnResult = m_equipment.GetNfGainResult(Frequency_At, ChannelNumber);
            return rtnResult;
        }

        protected override void SetResult(double rtnResult)
        {
            base.SetResult(rtnResult);
            //                    if (TestNo == 1997)
            //                    {
            //int kkkkk = 1;
            //                    }

            if (!SaveResult.IsHeaderContains("NF")) return;

            string Add;

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
                Add = Band + "_NF_" + PowerMode + "_" + Frequency + "_" + Selected_Port + "_" + GenNB;
            }
            else
            {
                Add = Band + "_NF_" + PowerMode + "_" + Frequency + "_" + Selected_Port;
            }

            AddGenspec(Add, rtnResult);
        }

        private void AddGenspec(string key, double result)
        {
            Dictionary<string, double> Genspec = SparamDelta.Genspec;
            if (Genspec.ContainsKey(key))
            {
                Genspec[key] = result;
            }
            else
            {
                Genspec.Add(key, result);
            }
        }
    }
}