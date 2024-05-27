using System;
using EqLib.NA;

namespace SparamTestLib
{
    public class SparamImpedance_Between : SparamTestCase.TestCaseAbstract 
    {
        private int iStartCnt, iStopCnt;
        private bool blnInterStart, blnInterStop;
        private double tmpVal = 0, tmpFreq = 0;

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

            if (SearchType == ESearchType.MAX)
                tmpVal = -99999;
            if (SearchType == ESearchType.MIN)
                tmpVal = 99999;

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
            tmpFreq = 0;
            tmpVal = 0;

            if (childErrorRaise == true)
            {
                tmpResult[TestNo].Result[0] = -999;
                tmpResult[TestNo].Result[1] = -999;
                return 1;
            }


            for (int iArr = iStartCnt; iArr <= iStopCnt; iArr++)
            {
                ComplexNumber objMath = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iArr];
                objMath.conv_SParam_to_Impedance(Z0);
                    
                if (SearchType == ESearchType.MAX)
                {
                    if (tmpVal < objMath.Impedance)
                    {
                        tmpVal = objMath.Impedance;
                        tmpFreq = tmpSparamRaw[TriggerID].Freq[iArr];
                    }
                }
                if (SearchType == ESearchType.MIN)
                {
                    if (tmpVal > objMath.Impedance)
                    {
                        tmpVal = objMath.Impedance;
                        tmpFreq = tmpSparamRaw[TriggerID].Freq[iArr];
                    }
                }
            }
            _Result.Result[0] = tmpVal;
            _Result.Result[1] = tmpFreq;

            return 0;

        }
    }
}
