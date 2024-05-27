using System;

namespace SparamTestLib
{
    public class SparamMagAt : SparamTestCase.TestCaseAbstract
    {
        private int iTargetCnt;
        private bool blnInterStart;

        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            for (int seg = 0; seg < tmpSparamRaw[TriggerID].Freq.Length; seg++)
            {
                if (TargetFreq == tmpSparamRaw[TriggerID].Freq[seg])
                {
                    iTargetCnt = seg;
                    blnInterStart = false;
                    seg = tmpSparamRaw[TriggerID].Freq.Length;
                }
                else if (TargetFreq >= tmpSparamRaw[TriggerID].Freq[seg] && TargetFreq < tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iTargetCnt = seg;
                    if (StartFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStart = false;
                    else
                        blnInterStart = true;
                }
            }

            Array.Resize(ref _Result.Result, 1);
            Array.Resize(ref _Result.Header, 1);

            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                    "_" + TargetFreq / 1e6 + "MHz";

            _Result.Header[0] = TcfHeader;
            _Result.Result[0] = -9999;

            return true;

        }

        public override int RunTest()
        {
            double rtnResult;
            int i_TargetFreqCnt = 0;
            //int usePrevious = int.Parse(UsePrevious);

            if (childErrorRaise == true)
            {
                _Result.Result[0] = -999;
                return 1;
            }
            
            //if (usePrevious > 0)
            //{
            //    i_TargetFreqCnt = tmpResult[usePrevious].Misc;
            //    rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i_TargetFreqCnt].DB;
            //}
            //else
            //{
                rtnResult = SearchInterpolatedData(blnInterStart, StartFreq, iTargetCnt);      
            //}

            _Result.Result[0] = rtnResult;
            return 0;

        }
    }
}
