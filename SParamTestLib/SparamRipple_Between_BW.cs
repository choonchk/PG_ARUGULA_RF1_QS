using System;
using System.Collections.Generic;
using System.Linq;

namespace SparamTestLib
{
    public class SparamRipple_Between_BW : SparamTestCase.TestCaseAbstract 
    {
        private int iStartCnt, iStopCnt, iRipCount;
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

            Array.Resize(ref _Result.Result, 1);
            Array.Resize(ref _Result.Header, 1);

            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                   "_" + StartFreq / 1e6 + "MHz_" + StopFreq / 1e6 + "MHz";

            _Result.Header[0] = TcfHeader;
            
            _Result.Result[0] = -9999;

            return true;

        }

        public override int RunTest()
        {
            double Rslt_Max;
            double Rslt_Min;
            double tmpRslt;
            List<double> tmpRsltList = new List<double>();
            int tmpStart = iStartCnt;

            if (childErrorRaise == true)
            {
                tmpResult[TestNo].Result[0] = -999;
                return 1;
            }


            Rslt_Max = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iStartCnt].DB;
            Rslt_Min = Rslt_Max;



            for (int iArr = iStartCnt; iArr <= iStopCnt; iArr++)
            {

                tmpRslt = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iArr].DB;
                MaxMinComparator(ref Rslt_Min, ref Rslt_Max, tmpRslt);

                if (tmpSparamRaw[TriggerID].Freq[iArr] >= tmpSparamRaw[TriggerID].Freq[tmpStart]+(SearchValue*1e6)|iArr == iStopCnt)
                {
                    if (Abs)
                    {
                        tmpRsltList.Add(Math.Abs(Rslt_Max - Rslt_Min));
                    }
                    else
                    {
                        tmpRsltList.Add(Rslt_Max - Rslt_Min);
                    }
                    tmpStart = iArr;
                    Rslt_Max = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[tmpStart+1].DB;
                    Rslt_Min = Rslt_Max;
                }
            }

            _Result.Result[0] = tmpRsltList.Max();
            //if (Abs)
            //{
            //    _Result.Result[0] = Math.Abs(Rslt_Max - Rslt_Min);
            //}
            //else
            //{
            //    _Result.Result[0] = Rslt_Max - Rslt_Min;
            //}
            return 0;


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
    }
}
