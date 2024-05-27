using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using NationalInstruments.ModularInstruments.Interop;


namespace EqLib
{
    public partial class EqDC
    {
        public static EqDC.iEqDC Get(string VisaAlias, string ChanNumber, string PinName, byte Site, bool Reset)
        {
            iEqDC dc;

            if (VisaAlias.Contains("4154"))
            {
                dc = new NI4154(VisaAlias, ChanNumber, PinName, Site, Reset);
            }
            else if (VisaAlias.Contains("4139"))
            {
                dc = new NI4139(VisaAlias, ChanNumber, PinName, Site, Reset);
            }
            else if (VisaAlias.Contains("4143") || VisaAlias.Contains("4141"))
            {
                dc = new NI4143(VisaAlias, ChanNumber, PinName, Site, Reset);
            }
            else if (VisaAlias.ToUpper().Contains("9195"))
            {
                dc = new Ks9195Pmu(VisaAlias, ChanNumber, PinName, Site);
            }
            else if (VisaAlias.ToUpper().Contains("6570"))
            {
                dc = new NI6570Pmu(VisaAlias, ChanNumber, PinName, Site);
            }
            else if (VisaAlias.ToUpper().Contains("471E"))
            {
                dc = new Aemulus471e(VisaAlias, ChanNumber, PinName, Site);
            }
            else if (VisaAlias.ToUpper().Contains("430E"))
            {
                dc = new Aemulus430e(VisaAlias, ChanNumber, PinName, Site);
            }
            else
            {
                throw new Exception("Visa Alias \"" + VisaAlias + "\" is not in a recognized format.\nValid SMU Visa Aliases must include one of the following:\n"
                    + "\n\"4154\""
                    + "\n\"4139\""
                    + "\n\"4143\""
                    + "\n\"4141\""
                    + "\n\"6556\""
                    + "\n\"9195\""
                    + "\n\"6570\""
                    + "\n\nFor example, Visa Alias \"SMU_NI4143_02\" will be recognized as an NI 4143 module.");
            }

            return dc;
        }

        private static string GetNiInstrumentInfo(iEqDC smu, nidcpower SMUsession, byte Site)
        {
            return smu.PinName + Site + " = " + SMUsession.GetString(nidcpowerProperties.InstrumentModel) + " r" + SMUsession.GetString(nidcpowerProperties.SpecificDriverRevision) + "*" + smu.SerialNumber + "; ";
        }

        private static string TranslateNiTriggerLine(TriggerLine trigLine)
        {
            switch (trigLine)
            {
                case TriggerLine.None:
                    return "";

                case TriggerLine.FrontPanel0:
                    return "PFI0";

                case TriggerLine.FrontPanel1:
                    return "PFI1";

                case TriggerLine.FrontPanel2:
                    return "PFI2";

                case TriggerLine.FrontPanel3:
                    return "PFI3";

                case TriggerLine.PxiTrig0:
                    return "PXI_Trig0";

                case TriggerLine.PxiTrig1:
                    return "PXI_Trig1";

                case TriggerLine.PxiTrig2:
                    return "PXI_Trig2";

                case TriggerLine.PxiTrig3:
                    return "PXI_Trig3";

                case TriggerLine.PxiTrig4:
                    return "PXI_Trig4";

                case TriggerLine.PxiTrig5:
                    return "PXI_Trig5";

                case TriggerLine.PxiTrig6:
                    return "PXI_Trig6";

                case TriggerLine.PxiTrig7:
                    return "PXI_Trig7";

                default:
                    throw new Exception("NI DC trigger line not supported");
            }
        }

        public interface iEqDC
        {
            string VisaAlias { get; set; }
            string SerialNumber { get; }
            string ChanNumber { get; set; }
            string PinName { get; set; }
            byte Site { get; set; }
            double priorVoltage { get; set; }
            double priorCurrentLim { get; set; }

            void ForceVoltage(double voltsForce, double currentLimit);
            void SetupCurrentMeasure(double aperture, TriggerLine trigLine);
            double MeasureCurrent(int NumAverages);
            void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, TriggerLine trigLine);
            double[] MeasureCurrentTrace();
            void SetupVoltageMeasure();
            double MeasureVoltage(int NumAverages);
            void TransientResponse_Fast(ClothoLibAlgo.DcSetting settings);
            void TransientResponse_Normal(ClothoLibAlgo.DcSetting settings);
            void PreLeakageTest(ClothoLibAlgo.DcSetting settings);            
            void PostLeakageTest();
            void SetupContinuity(double currentForce);
            double MeasureContinuity(int avgs);
            double ReadTemp(double Temp);
            void DeviceSelfCal();
        }
    }
}
