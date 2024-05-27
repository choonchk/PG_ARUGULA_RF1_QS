using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GuCal;
using EqLib;

namespace TestLib
{
    public class IccCal : GU.IccCalBase
    {
        private EqLib.EqDC.iEqDC mySmuVcc;
        private byte site;

        public IccCal(byte site, EqLib.EqDC.iEqDC _mySmuVcc, float _TargetPout, double _Frequency, float _InputPathGain, float _OutputPathGain, int _DelaySg, string _ModulationMode, string _Waveform, string _poutTestName, string _pinTestName, string _iccTestName, string _keyName, bool _applyIccTargetCorrection)
            : base(site, _TargetPout, _Frequency, _InputPathGain, _OutputPathGain, _DelaySg, _ModulationMode, _Waveform, _poutTestName, _pinTestName, _iccTestName, _keyName, _applyIccTargetCorrection)
        {
            this.mySmuVcc = _mySmuVcc;
            this.site = site;
        }

        public override float MeasurePout()
        {
            EqLib.Eq.Site[site].RF.SA.Abort(site);

            double pout = Eq.Site[site].RF.SA.MeasureChanPower();

            return (float)pout;
        }

        public override double MeasureIcc()
        {
            mySmuVcc.SetupCurrentMeasure(Eq.Site[site].RF.ActiveWaveform.FinalServoMeasTime, TriggerLine.PxiTrig0);
            double Icc = mySmuVcc.MeasureCurrent(1);

            return Icc;
        }

        public override void SetPowerLevel(double powerLevel)
        {
            EqLib.Eq.Site[site].RF.SG.Level = powerLevel;
        }
    }
}
