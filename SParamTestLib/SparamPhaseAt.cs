using System;

namespace SparamTestLib
{
    public class SparamPhaseAt : SparamTestCase.TestCaseAbstract 
    {
        private int iTargetCnt;
        private bool blnInterStart;

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
                        blnInterStart = true;
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
            _Result.Header[1] = TcfHeader +"_FREQ";
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
                rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i_TargetFreqCnt].Phase;
            }
            else
            {
                if(blnInterStart)
                {
                    double p1 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Phase;
                    double p2 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt + 1].Phase;
                    if (((p1 + p2) / 2) > 180)
                        rtnResult = ((p1 + p2) / 2) - 360;
                    else
                        rtnResult = (p1 + p2) / 2;
                }
                else
                {
                    if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Phase > 180)
                    {
                        rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Phase - 360;
                    }
                    else
                    {
                        rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt].Phase;
                    }
                }
                
            }

            _Result.Result[0] = rtnResult;
            _Result.Result[1] = tmpSparamRaw[TriggerID].Freq[iTargetCnt];

            return 0;

        }
    }
}
