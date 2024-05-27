using System;

namespace SparamTestLib
{
    public class SparamImpedanceAt : SparamTestCase.TestCaseAbstract 
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
            int usePrevious = int.Parse(UsePrevious);

            if (childErrorRaise == true)
            {
                _Result.Result[0] = -999;
                return 1;
            }

            if (usePrevious > 0)
            {
                i_TargetFreqCnt = tmpResult[usePrevious].Misc;
                rtnResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[i_TargetFreqCnt].DB;
            }
            else
            {
                //need to calculate the iTargetCnt + 1 as the variable will be overwrite.
                EqLib.NA.ComplexNumber objMath1 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt + 1];
                EqLib.NA.ComplexNumber objMath2 = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iTargetCnt];

                if (blnInterStart)
                {
                    objMath1.conv_SParam_to_Impedance(Z0);
                    objMath2.conv_SParam_to_Impedance(Z0);
                    rtnResult = objMath2.Impedance + (partialGradient * (objMath1.Impedance - objMath2.Impedance));
                }
                else
                {
                    objMath2.conv_SParam_to_Impedance(Z0);
                    rtnResult = objMath2.Impedance;
                }
            }

            _Result.Result[TestNo] = rtnResult;

            return 0;

        }
    }
}
