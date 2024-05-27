using System;

namespace SparamTestLib
{
    public class SparamSum : SparamTestCase.TestCaseAbstract
    {
        string val_1, val_2;

        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            try
            {
                string[] tmpstr = Variable.Split('+');
                val_1 = tmpstr[0];
                val_2 = tmpstr[1];
            }
            catch
            {
                DisplayError("SparamSum", "Initialize - wrong input " + UsePrevious, "Use_Previous column data must in this format - (2+6)");
            }

            Array.Resize(ref _Result.Result, 1);
            Array.Resize(ref _Result.Header, 1);

            _Result.Enable = true;
            //_Result.Header[0] = TcfHeader +
            //                    "_" + StartFreq / 1e6 + "_MHz_" + StopFreq / 1e6 + "_MHz";

            _Result.Header[0] = TcfHeader;
            
            _Result.Result[0] = -9999;

            return true;

        }
        public override int RunTest()
        {
            double rtnVal;

            //rtnVal = tmpResult[val_1].Result[0] + tmpResult[val_2].Result[0];
            rtnVal = _MathCalcResult[val_1] + _MathCalcResult[val_2];
            if (Abs)
                rtnVal = Math.Abs(rtnVal);

            _Result.Result[0] = rtnVal;
            return 0;

        }
    }
}
