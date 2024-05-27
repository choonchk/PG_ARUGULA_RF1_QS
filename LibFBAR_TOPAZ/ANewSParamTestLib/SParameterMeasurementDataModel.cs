using System;
using System.Collections.Generic;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;
using SParamTestCommon;

namespace LibFBAR_TOPAZ.ANewSParamTestLib
{
    /// <summary>
    /// Initialize SParam measurement object from TCF Trace and Segment table.
    /// </summary>
    public class SParameterMeasurementDataModel
    {
        // Output data object.
        public S_Param[] SParamData { get; set; }
        public s_TraceMatching[] TraceMatch { get; set; }
        public s_SegmentTable[] SegmentParam { get; set; }


        // Input read from TCF.
        private static string TraceTabName = "Trace";
        private static string SegmentTabName = "Segment";
        private Tuple<bool, string, string[,]> TempTrace; //seoul
        private TcfSheetTrace m_sheetTrace;
        private TcfSheetSegmentTable m_sheetSegmentTable;
        private TcfSpeedLoader m_stub1;

        private S_CMRRnBal_Param[] SBalanceParamData { get; set; }
        private int TotalChannel { get; set; }

        public void DefineTraceData(TcfSheetTrace sheetTcf, TcfSheetSegmentTable sheetSegmentTable,
            Tuple<bool, string, string[,]> _TempTrace) //seoul
        {
            m_sheetTrace = sheetTcf;
            m_sheetSegmentTable = sheetSegmentTable;
            TempTrace = _TempTrace;
        }

        public void Init_Channel()
        {
            // CCT Temp shortcut hard coding.
            m_stub1 = new TcfSpeedLoader();
            m_stub1.SetTraceData(m_sheetTrace, m_sheetSegmentTable);
            m_stub1.Init_Channel();
            SParamData = m_stub1.SParamData;
            TraceMatch = m_stub1.TraceMatch;
            TotalChannel = SParamData.Length;
        }

        public void Init_SegmentParam(bool[] cprojectPortEnable, int CalColmnIndexNFset, bool isDiva = false)
        {
            //SegmentParam = Init_SegmentParam2(cprojectPortEnable, CalColmnIndexNFset);
            m_stub1.Init_SegmentParam(cprojectPortEnable, CalColmnIndexNFset, TotalChannel, isDiva);
            SegmentParam = m_stub1.SegmentParam;
        }

        public List<StateFileDataObject> InitChannelAndGetStateFileGenerationInput()
        {
            m_stub1.Init_Channel();
            SParamData = m_stub1.SParamData;
            TraceMatch = m_stub1.TraceMatch;
            TotalChannel = SParamData.Length;

            List<StateFileDataObject> sfList = m_stub1.GetStateFileGenerationInput(SParamData.Length);
            return sfList;
        }

        private s_PortMatchSetting[] PortMatching;
        private bool PortMatchingState;

        /// <summary>
        /// Unused, by Seoul.
        /// </summary>
        private void Init_PortMatching()
        {
            int RowNo;
            int ChannelNumber;

            int ChannelCnt;
            string tmpStr;
            string FixtureStr = "Fixture Analysis";

            ChannelCnt = 0;
            PortMatchingState = convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, 1, 3));
            if (PortMatchingState == true)
            {
                PortMatching = new s_PortMatchSetting[1];
                RowNo = 2;
                do
                {
                    tmpStr = cExtract.Get_Data(FixtureStr, RowNo, 1);
                    if ((int.TryParse(tmpStr, out ChannelNumber)) && (tmpStr != ""))
                    {
                        if (ChannelNumber > 0)
                        {
                            ChannelCnt++;
                            if (ChannelCnt > 1)
                            {
                                Array.Resize(ref PortMatching, ChannelCnt);
                            }
                            PortMatching[ChannelCnt - 1].ChannelNumber = ChannelNumber;
                            PortMatching[ChannelCnt - 1].Enable = convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, RowNo, 2));
                            if (PortMatching[ChannelCnt - 1].Enable)
                            {
                                PortMatching[ChannelCnt - 1].Port = new s_PortMatchDetailSetting[SParamData[ChannelCnt - 1].NoPorts];
                                for (int iPort = 0; iPort < SParamData[ChannelCnt - 1].NoPorts; iPort++)
                                {
                                    PortMatching[ChannelCnt - 1].Port[iPort].MatchType = (e_PortMatchType)Enum.Parse(typeof(e_PortMatchType), cExtract.Get_Data(FixtureStr, RowNo, 4).ToUpper());
                                    switch (PortMatching[ChannelCnt - 1].Port[iPort].MatchType)
                                    {
                                        case e_PortMatchType.NONE:
                                            break;

                                        case e_PortMatchType.USER:
                                            PortMatching[ChannelCnt - 1].Port[iPort].UserFile = cExtract.Get_Data(FixtureStr, RowNo + iPort, 9);
                                            break;

                                        case e_PortMatchType.PCSL:
                                        case e_PortMatchType.PLPC:
                                        case e_PortMatchType.PLSC:
                                        case e_PortMatchType.SCPL:
                                        case e_PortMatchType.SLPC:
                                            PortMatching[ChannelCnt - 1].Port[iPort].R = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 5);
                                            PortMatching[ChannelCnt - 1].Port[iPort].L = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 6);
                                            PortMatching[ChannelCnt - 1].Port[iPort].C = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 7);
                                            PortMatching[ChannelCnt - 1].Port[iPort].G = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 8);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    RowNo++;
                } while (tmpStr.ToUpper() != ("#EndMatching").ToUpper());
            }
        }

        private bool convertEnableDisable2Bool(string input)
        {
            if (input.ToUpper() == "ENABLE")
            {
                return true;
            }
            else
            {
                return false;
            }
        }       
    }

    public class WaveformInterpolation
    {
        private int Point1;
        private int Point2;
        private double PartialGradient;
        private double TargetFreqCnt;
        private S_ParamData m_currentSp;

        public string Interpolation
        {
            get { return b_Interpolation.ToString(); }
            set
            {
                b_Interpolation = CStr2Bool(value);
            }
        }

        public bool b_Interpolation;

        public string ErrorMessage { get; set; }
        public bool IsError { get; set; }

        public void Init(S_ParamData currentSp, s_SegmentTable st, double Frequency_At)
        {
            m_currentSp = currentSp;

            double tmpCnt = 0;
            bool Found = false;
            for (int seg = 0; seg < st.segm; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (Frequency_At >= sd.Start && Frequency_At <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }

                if (!Found) continue;

                TargetFreqCnt = ((Frequency_At - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt;

                if (b_Interpolation)
                {
                    Point1 = (int)Math.Floor(TargetFreqCnt);    //Remove the Decimal Point
                    Point2 = Point1 + 1;
                    double StepFreq = (sd.Stop - sd.Start) / (sd.Points - 1);
                    PartialGradient = (Frequency_At - (((Point1 - tmpCnt) * StepFreq) + sd.Start)) / StepFreq;
                }
                break;
            }

            if (Found == false)
            {
                string msg = String.Format("Unable to find Target Point for Target Frequency = {1}", Frequency_At);
                ErrorMessage = msg;
                IsError = true;
            }
        }

        /// <summary>
        /// Phase At code is slightly different.
        /// </summary>
        public void InitPhaseAt(S_ParamData currentSp, s_SegmentTable st, double Frequency_At)
        {
            m_currentSp = currentSp;

            #region "Target Point"
            double tmpCnt = 0;
            bool Found = false;

            for (int seg = 0; seg < st.segm; seg++)
            {
                s_SegmentData sd = st.SegmentData[seg];

                if (Frequency_At >= sd.Start && Frequency_At <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }

                if (!Found) continue;
                TargetFreqCnt = (int)Math.Floor((Frequency_At - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt;
                break;
            }
            if (Found == false)
            {
                string msg = String.Format("Unable to find Target Point for Target Frequency = {1}", Frequency_At);
                ErrorMessage = msg;
                IsError = true;
            }
            #endregion        // Calculate for Start Point
        }

        public double GetMagAt(int targetFreqCnt)
        {
            return m_currentSp.sParam[targetFreqCnt].dBAng.dB;
        }

        public double GetMagAtLin(int targetFreqCnt)
        {
            return m_currentSp.sParam[targetFreqCnt].MagAng.Mag;
        }

        public double GetRealAt(int targetFreqCnt)
        {
            return m_currentSp.sParam[targetFreqCnt].ReIm.Real;
        }
        public double GetImagAt(int targetFreqCnt)
        {
            return m_currentSp.sParam[targetFreqCnt].ReIm.Imag;
        }

        public double GetMagAt()
        {
            double rtnResult;
            s_DataType[] sp = m_currentSp.sParam;
            if (b_Interpolation)
            {
                rtnResult = sp[Point1].dBAng.dB + (PartialGradient * (sp[Point2].dBAng.dB - sp[Point1].dBAng.dB));
                return rtnResult;
            }
            int i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
            rtnResult = sp[i_TargetFreqCnt].dBAng.dB;
            return rtnResult;
        }

        public double GetMagAtLin()
        {
            double rtnResult;
            s_DataType[] sp = m_currentSp.sParam;
            if (b_Interpolation)
            {
                rtnResult = sp[Point1].MagAng.Mag + (PartialGradient * (sp[Point2].MagAng.Mag - sp[Point1].MagAng.Mag));
                return rtnResult;
            }
            int i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
            rtnResult = sp[i_TargetFreqCnt].MagAng.Mag;
            return rtnResult;
        }

        public double GetRealAt()
        {
            double rtnResult;
            s_DataType[] sp = m_currentSp.sParam;
            if (b_Interpolation)
            {
                rtnResult = sp[Point1].ReIm.Real + (PartialGradient * (sp[Point2].ReIm.Real - sp[Point1].ReIm.Real));
                return rtnResult;
            }
            int i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
            rtnResult = sp[i_TargetFreqCnt].ReIm.Real;
            return rtnResult;
        }

        public double GetImagAt()
        {
            double rtnResult;
            s_DataType[] sp = m_currentSp.sParam;
            if (b_Interpolation)
            {
                rtnResult = sp[Point1].ReIm.Imag + (PartialGradient * (sp[Point2].ReIm.Imag - sp[Point1].ReIm.Imag));
                return rtnResult;
            }
            int i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
            rtnResult = sp[i_TargetFreqCnt].ReIm.Imag;
            return rtnResult;
        }

        public double GetPhaseAt()
        {
            double rtnResult;
            int i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);

            if (b_Interpolation)
            {

                double Phase1 = m_currentSp.sParam[i_TargetFreqCnt].dBAng.Angle;
                double Phase2 = m_currentSp.sParam[i_TargetFreqCnt + 1].dBAng.Angle;
                if (((Phase1 + Phase2) / 2) > 180)
                {
                    rtnResult = ((Phase1 + Phase2) / 2) - 360;
                }
                else
                {
                    rtnResult = ((Phase1 + Phase2) / 2);
                }
                return rtnResult;
            }

            dB_Angle dBAngle = m_currentSp.sParam[i_TargetFreqCnt].dBAng;
            if (dBAngle.Angle > 180)
            {
                rtnResult = dBAngle.Angle - 360;
            }
            else
            {
                rtnResult = dBAngle.Angle;
            }
            return rtnResult;
        }

        protected bool CStr2Bool(string Input)
        {
            if (Input.Trim() == "1" || Input.ToUpper().Trim() == "YES" || Input.ToUpper().Trim() == "ON" || Input.ToUpper().Trim() == "V")
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }
}