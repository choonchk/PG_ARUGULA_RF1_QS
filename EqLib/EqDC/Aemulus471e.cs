using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqLib
{
    public partial class EqDC
    {
        class Aemulus471e : iEqDC
        {
            public string VisaAlias { get; set; }
            public string SerialNumber
            {
                get
                {
                    return "NA";
                }
            }
            public string ChanNumber { get; set; }
            public string PinName { get; set; }
            public byte Site { get; set; }
            public double priorVoltage { get; set; }
            public double priorCurrentLim { get; set; }

            public Aemulus471e(string VisaAlias, string ChanNumber, string PinName, byte Site)
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;
                this.Site = Site;
            }

            public void ForceVoltage(double voltsForce, double currentLimit)
            {
                throw new NotImplementedException();
            }

            public void SetupCurrentMeasure(double aperture, TriggerLine trigLine)
            {
                throw new NotImplementedException();
            }

            public double MeasureCurrent(int NumAverages)
            {
                throw new NotImplementedException();
            }

            public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, TriggerLine trigLine)
            {
                throw new NotImplementedException();
            }

            public double[] MeasureCurrentTrace()
            {
                throw new NotImplementedException();
            }

            public void SetupVoltageMeasure()
            {
                throw new NotImplementedException();
            }

            public double MeasureVoltage(int NumAverages)
            {
                throw new NotImplementedException();
            }

            public void TransientResponse_Fast(ClothoLibAlgo.DcSetting settings)
            {

            }

            public void TransientResponse_Normal(ClothoLibAlgo.DcSetting settings)
            {

            }

            public void PreLeakageTest(ClothoLibAlgo.DcSetting settings)
            {
                throw new NotImplementedException();
            }

            public void PostLeakageTest()
            {
                throw new NotImplementedException();
            }

            public void SetupContinuity(double currentForce)
            {
                throw new NotImplementedException();
            }

            public double MeasureContinuity(int avgs)
            {
                throw new NotImplementedException();
            }

            public double ReadTemp(double Temp)
            {
                throw new NotImplementedException();
            }

            public void DeviceSelfCal()
            {
                throw new NotImplementedException();
            }
        }
    }
}
