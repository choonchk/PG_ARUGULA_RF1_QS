using EqLib.NA;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TestLib;

namespace SparamTestLib
{
    /// <summary>
    /// Collection of test cases, uses NA as input, execute all cases, produce Result as output.
    /// </summary>
    public class SparamTestCase
    {
        /// <summary>
        /// test cases with their conditions from TCF lines. (Condition FBAR)
        /// </summary>
        public List<TestCaseAbstract> TestCases;

        private static bool ErrorRaise;
        private int _totalTestNo;
        private int _totalChannel;
        private static SResult[] _sParaResults;
        private static Dictionary<string, SParam> _sParamRaw = new Dictionary<string, SParam>();
        private static Dictionary<string, double> MathCalcResult = new Dictionary<string, double>();

        public SResult[] Result
        {
            get { return _sParaResults; }
        }

        public int TotalTestNo
        {
            get { return _totalTestNo; }
            set
            {
                _totalTestNo = value;
                Array.Resize(ref _sParaResults, value);
            }
        }

        public int TotalChannel
        {
            get { return _totalChannel; }
            set
            {
                _totalChannel = value;
                //Array.Resize(ref _sParamRaw, value); //change to dic so no require
            }
        }

        public Dictionary<string, SParam> SparamRaw
        {
            get { return _sParamRaw; }
        }

        public SparamTestCase()
        {
            TestCases = new List<TestCaseAbstract>();
        }

        public abstract class TestCaseAbstract : TestLib.iTest
        {
            //protected variable

            #region protected variable

            protected SResult _Result;
            private string _TCFHeader;

            #endregion protected variable

            //Class Property

            #region Class Property

            public int ChannelNumber { get; set; }

            public string TcfHeader
            {
                get { return _TCFHeader; }
                set { _TCFHeader = value.Replace(" ", "_"); }
            }

            public naEnum.ESParametersDef SParam { get; set; }

            public naEnum.ESParametersDef SParam2 { get; set; }

            public double StartFreq { get; set; }

            public double StopFreq { get; set; }

            public double TargetFreq { get; set; }

            public ESearchDirection SearchDirection { get; set; }

            public ESearchType SearchType { get; set; }

            public double SearchValue { get; set; }

            public bool Interpolate { get; set; }

            public bool Abs { get; set; }

            public bool Vswr { get; set; }

            public int TestNo { get; set; }

            public string UsePrevious { get; set; }

            public double Z0 { get; set; }

            public string Variable { get; set; }

            public string PowerMode { get; set; }

            public string Band { get; set; }

            public string TriggerID { get; set; }

            public List<EqLib.MipiSyntaxParser.ClsMIPIFrame> MipiCommands { get; set; }

            public Dictionary<string, DCSetting> SmuSettingsDictNA { get; set; }

            public List<EqLib.Operation> ActivePath { get; set; }

            public byte Site;

            public bool NFChannel { get; set; }

            #endregion Class Property

            protected SResult[] tmpResult = _sParaResults;
            protected Dictionary<string, SParam> tmpSparamRaw = _sParamRaw;
            protected Dictionary<string, double> _MathCalcResult = MathCalcResult;
            protected bool childErrorRaise = ErrorRaise;
            //protected bool EnableDatalog = SparamTestCase._blnDatalog;
            //protected int DatalogCount = SparamTestCase._DatalogCount;
            //protected string SDIFolder = SparamTestCase._SDIFolderPath;
            //protected string SDIFileName = SparamTestCase._SDIFileName;

            #region iTest interface implementation

            /// <summary>
            /// iTest interface.
            /// </summary>
            public virtual bool Initialize(bool finalScript)
            {
                return true;
            }

            /// <summary>
            /// iTest interface.
            /// </summary>
            public void BuildResults(ref ATFReturnResult results)
            {
                if (_Result.Enable)
                {
                    for (int iRst = 0; iRst < _Result.Header.Length; iRst++)
                    {
                        // Note: Its UpdateResult here not AddResult. Same result will be published more than once.
                        ResultBuilder.UpdateResult(Site, _Result.Header[iRst], "", 
                            _Result.Result[iRst]);
                    }
                }
            }

            /// <summary>
            /// iTest interface.
            /// </summary>
            public virtual int RunTest()
            {
                ErrorRaise = true;
                return 0;
            }

            #endregion iTest interface implementation

            public void BuildResults()
            {
                tmpResult[TestNo] = _Result;
                if (Variable != string.Empty & !string.IsNullOrEmpty(Variable) & Variable != "0")
                {
                    if (_MathCalcResult.ContainsKey(Variable))
                    {
                        _MathCalcResult[Variable] = _Result.Result[0];
                    }
                    else
                    {
                        _MathCalcResult.Add(Variable, _Result.Result[0]);
                    }
                }
            }

            //Private Method
            protected int SearchData(int iStartCnt, int iStopCnt, bool PositiveValue)
            {
                double tmpResult;
                int tmpRet;

                tmpResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStartCnt].DB; //SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[StartFreqCnt].dBAng.dB;
                tmpRet = iStartCnt;
                switch (SearchType)
                {
                    case ESearchType.MAX:
                        //if (PositiveValue)
                        //{
                            for (int i = iStartCnt; i <= iStopCnt; i++)
                            {
                                if (tmpResult < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                {
                                    tmpResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB;
                                    tmpRet = i;
                                }
                            }
                        //}
                        //else
                        //{
                        //    for (int i = iStartCnt; i <= iStopCnt; i++)
                        //    {
                        //        if (tmpResult > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                        //        {
                        //            tmpResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB;
                        //            tmpRet = i;
                        //        }
                        //    }
                        //}
                        break;

                    case ESearchType.MIN:
                        //if (PositiveValue)
                        //{
                            for (int i = iStartCnt; i <= iStopCnt; i++)
                            {
                                if (tmpResult > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                {
                                    tmpResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB;
                                    tmpRet = i;
                                }
                            }
                        //}
                        //else
                        //{
                        //    for (int i = iStartCnt; i <= iStopCnt; i++)
                        //    {
                        //        if (tmpResult < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                        //        {
                        //            tmpResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB;
                        //            tmpRet = i;
                        //        }
                        //    }
                        //}
                        break;

                    case ESearchType.USER:
                        tmpRet = iStartCnt;
                        if (PositiveValue)
                        {
                            if (SearchDirection != ESearchDirection.FROM_RIGHT)
                            {
                                for (int i = iStartCnt; i <= iStopCnt; i++)
                                {
                                    if (SearchValue < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i - 1].DB &&
                                        SearchValue > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                    {
                                        tmpResult = i;
                                        tmpRet = i;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = iStartCnt + (iStopCnt - iStartCnt); i != iStartCnt; i--)
                                {
                                    if (SearchValue < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i - 1].DB &&
                                        SearchValue > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                    {
                                        tmpResult = i;
                                        tmpRet = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (SearchDirection != ESearchDirection.FROM_RIGHT)
                            {
                                for (int i = iStartCnt; i <= iStopCnt; i++)
                                {
                                    if (SearchValue > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i - 1].DB &&
                                        SearchValue < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                    {
                                        tmpResult = i;
                                        tmpRet = i;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = iStartCnt + (iStopCnt - iStartCnt); i != iStartCnt; i--)
                                {
                                    if (SearchValue < tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i - 1].DB &&
                                        SearchValue > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i].DB)
                                    {
                                        tmpResult = i;
                                        tmpRet = i;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
                return (tmpRet);
            }

            protected int SearchMaxValue(int iStartCnt, int iStopCnt)
            {
                int tmpReturn = -999;
                double Peak_Value = -999999;

                for (int iArr = iStartCnt; iArr <= iStopCnt; iArr++)
                {
                    if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iArr].DB > Peak_Value)
                    {
                        Peak_Value = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iArr].DB;
                        tmpReturn = iArr;
                    }
                }

                return tmpReturn;
            }

            protected double SearchInterpolatedData(bool blnInterpolate, double Frequency, int FreqCnt)
            {
                //double tmpData;
                double y, y0, y1, x0, x1, x;
                if (blnInterpolate == false)
                {
                    y = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt].DB;
                    //tmpData = tmpSparamRaw[_TriggerID].SParamData[GetSparamIndex()].SParam[iStartCnt].DB;
                }
                else
                {
                    y0 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt].DB;
                    y1 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt + 1].DB;
                    x = Frequency;
                    x0 = tmpSparamRaw[TriggerID].Freq[FreqCnt];
                    x1 = tmpSparamRaw[TriggerID].Freq[FreqCnt + 1];

                    y = y0 + (x - x0) * ((y1 - y0) / (x1 - x0));

                    //tmpData = ((tmpSparamRaw[_TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt + 1].DB - tmpSparamRaw[_TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt].DB) /
                    //            (tmpSparamRaw[_TriggerID].Freq[FreqCnt + 1] - tmpSparamRaw[_TriggerID].Freq[FreqCnt]) *
                    //            (Frequency - tmpSparamRaw[_TriggerID].Freq[FreqCnt])) + tmpSparamRaw[_TriggerID].SParamData[GetSparamIndex()].SParam[FreqCnt].DB;
                }
                return y;// tmpData;
            }

            protected int GetSparamIndex()
            {
                int val = -1;
                //if (_TCFHeader.Contains("NF")) SParam = naEnum.ESParametersDef.NF;
                for (int t = 0; t < tmpSparamRaw[TriggerID].SParamData.Length; t++)
                {
                    if (tmpSparamRaw[TriggerID].SParamData[t].SParamDef.Equals(SParam))
                        val = t;
                }
                return val;
            }

            protected void DisplayError(string ClassName, string ErrParam, string ErrDesc)
            {
                MessageBox.Show("Class Name: " + ClassName + "\nParameters: " + ErrParam + "\n\nErrorDesciption: \n"
                    + ErrDesc, "Error found in Class " + ClassName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}