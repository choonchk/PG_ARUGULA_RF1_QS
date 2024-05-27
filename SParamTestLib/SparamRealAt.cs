using System;

namespace SparamTestLib
{
    public class SparamRealAt : SparamTestCase.TestCaseAbstract 
    {
        private int iTargetCnt;
        private bool blnInterStart;
        private double partialGradient;

        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            for (int seg = 0; seg < tmpSparamRaw[TriggerID].Freq.Length; seg++)
            {
                if (TargetFreq >= tmpSparamRaw[TriggerID].Freq[seg] && TargetFreq < tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iTargetCnt = seg;
                    if (StartFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStart = false;
                    else
                    {
                        blnInterStart = true;
                        partialGradient = (tmpSparamRaw[TriggerID].Freq[iTargetCnt + 1] -
                                            tmpSparamRaw[TriggerID].Freq[iTargetCnt + 1]);
                        partialGradient = partialGradient * (TargetFreq - tmpSparamRaw[TriggerID].Freq[iTargetCnt]);
                    }
                }
            }

            Array.Resize(ref _Result.Result, 2);
            Array.Resize(ref _Result.Header, 2);

            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                    "_" + TargetFreq / 1e6 + "MHz_";
            //_Result.Header[1] = TcfHeader +
            //                    "_" + TargetFreq / 1e6 + "MHz_FREQ";

            _Result.Header[0] = TcfHeader;
            _Result.Header[1] = TcfHeader + "_FREQ";

            _Result.Result[0] = -9999;
            _Result.Result[1] = -9999;

            return true;

        }

        public override int RunTest()
        {
            double rtnResult;
            int i_TargetFreqCnt = 0;
            int usePrevious = int.Parse(UsePrevious);

            if (childErrorRaise == true)
            {
                _Result.Result[0] = -999;
                _Result.Result[1] = -999;
                return 1;
            }

            if (usePrevious > 0)
            {
                i_TargetFreqCnt = tmpResult[usePrevious].Misc;
                rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i_TargetFreqCnt].Real;
            }
            else
            {
                //need to calculate the iTargetCnt + 1 as the variable will be overwrite.
                if (blnInterStart)
                {
                    double p2 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt + 1].Real;
                    double p1 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Real;
                    rtnResult = p1 + (partialGradient * (p2 - p1));
                }
                else
                {
                    rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Real;
                }
            }

            _Result.Result[TestNo] = rtnResult;
            _Result.Result[1] = tmpSparamRaw[TriggerID].Freq[iTargetCnt];

            return 0;

        }
    }
}
