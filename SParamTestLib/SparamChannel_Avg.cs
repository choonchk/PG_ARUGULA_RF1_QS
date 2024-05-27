using System;

namespace SparamTestLib
{
    public class SparamChannel_Avg : SparamTestCase.TestCaseAbstract 
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

            Array.Resize(ref _Result.Result, 1);
            Array.Resize(ref _Result.Header, 1);

            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                    "_" + StartFreq / 1e6 + "MHz_" + StopFreq / 1e6 + "MHz";
            _Result.Header[0] = TcfHeader;
            _Result.Result[0] = -9999;

            return true;

        }

        public override int RunTest()
        {
            int pointNo = iStopCnt - iStartCnt + 1;
            double total_magnitude=0;

            if (childErrorRaise == true)
            {
                tmpResult[TestNo].Result[0] = -999;
                return 1;
            }


            //Start adding the magnitude base on the freq range specified earlier
            for (int iArr = iStartCnt; iArr <= iStopCnt; iArr++)
            {
                //Mag_SParam.MagAng = Math_Func.Conversion.conv_RealImag_to_MagAngle(SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iArr].ReIm);
                total_magnitude = total_magnitude + tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iArr].DB; //Mag_SParam.MagAng.Mag;
            }

            //Calculate the channel averaging
            _Result.Result[0] = total_magnitude / pointNo;

            return 0;

        }
    }
}
