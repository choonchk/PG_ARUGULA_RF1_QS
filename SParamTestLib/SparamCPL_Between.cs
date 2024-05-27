using System;

namespace SparamTestLib
{
    public class SparamCPL_Between : SparamTestCase.TestCaseAbstract 
    {
        SparamMagBetween objMag = new SparamMagBetween();
        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            objMag.TestNo = TestNo;
            objMag.ChannelNumber = ChannelNumber;
            objMag.Interpolate = Interpolate;
            objMag.SearchType = SearchType;
            objMag.SParam = SParam;
            objMag.StartFreq = StartFreq;
            objMag.StopFreq = StopFreq;
            objMag.Initialize(finalScript);

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
            double Mag1, Mag2;

            if (childErrorRaise == true)
            {
                tmpResult[TestNo].Result[0] = -999;
                return 1;
            }

            objMag.RunTest();
            Mag1 = _Result.Result[0];

            objMag.SParam = SParam2;
            objMag.RunTest();
            Mag2 = _Result.Result[0];

            _Result.Result[0] = Mag1 - Mag2;

            return 0;

        }
    }
}
