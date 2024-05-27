using System;

namespace SparamTestLib
{
    public class SparamFreqAt : SparamTestCase.TestCaseAbstract 
    {
        private int iStartCnt, iStopCnt;
        private bool blnInterStart, blnInterStop;

        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            for (int seg = 0; seg < tmpSparamRaw[TriggerID].Freq.Length; seg++)
            {
                if (StartFreq >= tmpSparamRaw[TriggerID].Freq[seg] && StartFreq < tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iStartCnt = seg;
                    blnInterStart = true;
                    if (StartFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStart = false;
                }
                if (StopFreq >= tmpSparamRaw[TriggerID].Freq[seg] && StopFreq < tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iStopCnt = seg;
                    blnInterStop = true;
                    if (StopFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStop = false;
                }
            }

            Array.Resize(ref _Result.Result, 2);
            Array.Resize(ref _Result.Header, 2);


            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                    "_" + StartFreq / 1e6 + "MHz_" + StopFreq / 1e6 + "MHz";
            //_Result.Header[1] = TcfHeader +
            //                    "_" + StartFreq / 1e6 + "MHz_" + StopFreq / 1e6 + "MHz_FREQ";

            _Result.Header[0] = TcfHeader;
            _Result.Header[1] = TcfHeader + "_FREQ";
            _Result.Result[0] = -9999;
            _Result.Result[1] = -9999;

            return true;

        }

        public override int RunTest()
        {
            int intPeakCnt;
            int intResultCnt;
            double rtnFreq = 99e9;
            bool b_PositiveValue;
            int iStopCntR = iStopCnt, iStartCntR = iStartCnt;

            if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStartCnt].DB > 0)
            {
                b_PositiveValue = true;
            }
            else
            {
                b_PositiveValue = false;
            }

            switch (SearchDirection)
            {
                case ESearchDirection.FROM_MAX_LEFT:
                    intPeakCnt = SearchMaxValue(iStartCnt, iStopCnt);
                    iStopCntR = intPeakCnt;
                    if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStartCntR].DB > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStopCntR].DB)
                        b_PositiveValue = true;
                    else
                        b_PositiveValue = false;
                    break;
                case ESearchDirection.FROM_MAX_RIGHT:
                    intPeakCnt = SearchMaxValue(iStartCnt, iStopCnt);
                    iStartCntR = intPeakCnt;
                    if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStartCntR].DB > tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStopCntR].DB)
                        b_PositiveValue = true;
                    else
                        b_PositiveValue = false;
                    break;
            }

            intResultCnt = (int)SearchData(iStartCntR, iStopCntR, b_PositiveValue);

            if (tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[intResultCnt].DB != SearchValue)
            {
                double f1 = tmpSparamRaw[TriggerID].Freq[intResultCnt];
                double f2 = tmpSparamRaw[TriggerID].Freq[intResultCnt - 1];
                double p1 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[intResultCnt].DB;
                double p2 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[intResultCnt - 1].DB;
                double Gradient = (f2 - f1) / (p2 - p1);

                rtnFreq = Math.Round(((SearchValue - p1) * Gradient) + f1, 0);
            }
            else
                rtnFreq = tmpSparamRaw[TriggerID].Freq[intResultCnt];

            _Result.Result[0] = SearchValue;
            _Result.Result[1] = rtnFreq;

            return 0;
        }
    }
}
