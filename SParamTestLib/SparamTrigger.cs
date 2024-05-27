using System;
using System.Collections.Generic;
using EqLib;
using EqLib.NA;

namespace SparamTestLib
{
    public class SparamTrigger : SparamTestCase.TestCaseAbstract
    {
        protected NetworkAnalyzerAbstract _EqNa;

        public NetworkAnalyzerAbstract EqNa
        {
            get { return _EqNa; }
            set { _EqNa = value; }
        }

        public override bool Initialize(bool finalScript)
        {
            _EqNa.TriggerSingle(ChannelNumber);

            tmpSparamRaw[TriggerID] = new EqLib.NA.SParam

            {
                Freq = _EqNa.GetFreqList(ChannelNumber)
            };
            _EqNa.GrabSParamRiData(ChannelNumber);
            //_EqNa.TriggerMode(naEnum.ETriggerMode.Cont);
            _Result.Enable = false;

            return true;

        }

        public override int RunTest()
        {

            tmpSparamRaw[TriggerID] = new EqLib.NA.SParam();
            
            _EqNa.TriggerSingle(ChannelNumber);
            tmpSparamRaw[TriggerID] = _EqNa.GrabSParamRiData(ChannelNumber);
            return 0;

        }
    }
}
