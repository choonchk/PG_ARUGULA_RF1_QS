using System;

namespace SparamTestLib
{
    public class SparamMagBetween : SparamTestCase.TestCaseAbstract 
    {
        private int iStartCnt, iStopCnt;
        private bool blnInterStart, blnInterStop;
        public override bool Initialize(bool finalScript)
        {
            bool blnDoneStart = false , blnDoneStop = false;

            _Result = new SResult();

            for (int seg = 0; seg < tmpSparamRaw[TriggerID].Freq.Length; seg++)
            {
                if (StartFreq >= tmpSparamRaw[TriggerID].Freq[seg] && StartFreq <= tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iStartCnt = seg;
                    blnInterStart = true;
                    blnDoneStart = true;
                    if (StartFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStart = false;
                }
                if (StopFreq >= tmpSparamRaw[TriggerID].Freq[seg] && StopFreq <= tmpSparamRaw[TriggerID].Freq[seg + 1])
                {
                    iStopCnt = seg;
                    blnInterStop = true;
                    blnDoneStop = true;
                    if (StopFreq == tmpSparamRaw[TriggerID].Freq[seg])
                        blnInterStop = false;
                }
                if (blnDoneStart && blnDoneStop)
                    break;
            }

            int ResultCount = 2;

            if (Vswr) ResultCount = 3;

            Array.Resize(ref _Result.Result, ResultCount);
            Array.Resize(ref _Result.Header, ResultCount);

            if (!blnDoneStart && !blnDoneStop)
                DisplayError(this.ToString(), "Test Case class init error", "TCF Start & Stop Frequency " + StartFreq + " & " + StopFreq + 
                    " not within channel frequecy set in NA segment table.");

            _Result.Enable = true;

            if (Vswr)
            {
                
                _Result.Header[0] = TcfHeader.Replace("VSWR", "DB");
                _Result.Header[1] = TcfHeader + "_FREQ";
                _Result.Header[2] = TcfHeader.Replace("DB", "VSWR");

                _Result.Result[2] = -9999;
            }
            else
            {
                _Result.Header[0] = TcfHeader;
                _Result.Header[1] = TcfHeader + "_FREQ";
            }

            _Result.Result[0] = -9999;
            _Result.Result[1] = -9999;
            
            return true;

        }
        public override int RunTest()
        {
            bool b_PositiveValue = true;
            double tmpSearchResult;
            double Rslt1, Rslt2;
            int iCnt;
            if (childErrorRaise == true)
            {
                tmpResult[TestNo].Result[0] = -999;
                tmpResult[TestNo].Result[1] = -999;
                if(Vswr) tmpResult[TestNo].Result[2] = -999;
                return 1;
            }

            iCnt = SearchData(iStartCnt, iStopCnt, b_PositiveValue);
            tmpSearchResult = tmpSparamRaw[TriggerID].SParamData[GetSparamIndex()].SParam[iCnt].DB;

            Rslt1 = SearchInterpolatedData(blnInterStart, StartFreq, iStartCnt);
            Rslt2 = SearchInterpolatedData(blnInterStop, StopFreq, iStopCnt);

            _Result.Result[0] = ProcessData(tmpSearchResult, Rslt1, Rslt2, b_PositiveValue);
            _Result.Result[1] = tmpSparamRaw[TriggerID].Freq[iCnt];


            if (Vswr)
            {
                double VswrData = -9999;
                double dbData = _Result.Result[0];

                VswrData = (1 + (Math.Pow(10, dbData / 20))) / (1 - (Math.Pow(10, dbData / 20)));
                _Result.Result[2] = VswrData;
            }

            return 0;

        }

        private double ProcessData(double Rslt, double Rslt1, double Rslt2, bool PositiveValue)
        {
            double rtnRslt;
            //rtnRslt = -999;
            rtnRslt = Rslt;
            switch (SearchType)
            {
                case ESearchType.MAX:
                    //if (PositiveValue)
                    //{
                        if (Rslt < Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }
                        if (Rslt < Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    //}
                    //else
                    //{
                    //    if (Rslt > Rslt1)
                    //    {
                    //        rtnRslt = Rslt1;
                    //    }
                    //    if (Rslt > Rslt2)
                    //    {
                    //        rtnRslt = Rslt2;
                    //    }
                    //}
                    break;
                case ESearchType.MIN:
                    //if (PositiveValue)
                    //{
                        if (Rslt > Rslt1)
                        {
                            rtnRslt = Rslt1;
                        }
                        if (Rslt > Rslt2)
                        {
                            rtnRslt = Rslt2;
                        }
                    //}
                    //else
                    //{
                    //    if (Rslt < Rslt1)
                    //    {
                    //        rtnRslt = Rslt1;
                    //    }
                    //    if (Rslt < Rslt2)
                    //    {
                    //        rtnRslt = Rslt2;
                    //    }
                    //}
                    break;
            }
            return (rtnRslt);
        }
    }
}
