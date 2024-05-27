using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;


namespace EqLib.NA
{
    public struct SCalibrationProcedure
    {
        public string CalType;
        public naEnum.ECalibrationStandard CalStandard;
        public naEnum.EOnOff AvgState;
        public int ChannelNumber;
        public int CKitLocNum;
        public string CKitLabel;
        public int NoPorts;
        public int CalKit;
        public bool BCalKit;
        public string Message;
        public int Sleep;
        public string Switch;
        public string MoveStep;
        public string ParameterType;
        public string Switch_Input;
        public string Switch_Ant;
        public string Switch_Rx;
        public int TraceNumber;
        public string ConnectorType;
        public string NF_Input_Port;
        public string NF_Output_Port;
        public int NF_SrcPortNum;
        public int Port1;
        public int Port2;
        public int Port3;
        public int Port4;
        public int Port5;
        public int Port6;
        public int Port7;
        public int Port8;
        public int Port9;
        public int Port10;
        public int Port11;
        public int Port12;
        public int Port13;
        public int Port14;
        public int Port15;
        public int Port16;
        public int Port17;
        public int Port18;
        public int Port19;
        public int Port20;
        public int Port21;
        public int Port22;
        public int Port23;
        public int Port24;      
    }
    public struct SCalStdTable
    {
        public int channelNo;
        public naEnum.EOnOff enable;
        public int calkit_locnum;
        public string calkit_label;
        public int total_calstd;
        public SCalStdData[] CalStdData;
        public naEnum.EOnOff Avg_Enable;
        public string Avg_Mode;
        public int Avg_Factor;
        public int Total_Port;
    }
    public struct SCalStdData
    {
        public naEnum.ECalibrationStandard StdType;
        public string StdLabel;
        public int StdNo;
        public double C0_L0;
        public double C1_L1;
        public double C2_L2;
        public double C3_L3;
        public double OffsetDelay;
        public double OffsetZ0;
        public double OffsetLoss;
        public double ArbImp;
        public double MinFreq;
        public double MaxFreq;
        public naEnum.ECalStdMedia Media;
        public naEnum.ECalStdLengthType LengthType;
        public int Port1;
        public int Port2;
    }

    public struct STraceMatching
    {
        public int[] TraceNumber;
        public int[] SParamDefNumber;
        public int[] BalunTopology;
        public int[] TopazWindNo;
        public int[] NFSrcPort;
        public int[] NFRcvPort;
        public int[] NFSwpTime;

        public bool NFChannel;
        public int NoPorts;
    }

    public struct SParam
    {
        public SParamData[] SParamData;
        public double[] Freq;
        public int NoPoints;
        public bool[] SParamEnable;
    }
    public struct SParamData
    {
        public ComplexNumber[] SParam;
        public naEnum.ESFormat Format;
        public naEnum.ESParametersDef SParamDef;
    }

    public struct SSegmentTable
    {
        public naEnum.EModeSetting Mode;
        public naEnum.EOnOff Ifbw;
        public naEnum.EOnOff Pow;
        public naEnum.EOnOff Del;
        public naEnum.EOnOff Swp;
        public naEnum.EOnOff Time;
        public int Segm;
        public bool NFChannel;
        public SSegmentData[] SegmentData;

    }
    public struct SSegmentData
    {
        public double Start;
        public double Stop;
        public int Points;
        public double IfbwValue;
        public double PowValue;
        public double DelValue;
        public naEnum.ESweepMode SwpValue;
        public double TimeValue;
        public double CalIfbwValue;
        public double CalPowValue;
        public double[] PowList;
        public double[] CalPowList;
    }

    public struct SStateFile
    {
        public string StateFile;
        public bool LoadState;
    }

    public class ComplexNumber
    {
        public double Real;
        public double Imag;
        public double DB;
        public double Mag;
        public double Phase;
        public double Impedance;

        public void conv_RealImag_to_dBAngle()
        {
            try
            {
                conv_RealImag_to_MagAngle();
                DB = 20 * (Math.Log10(Mag));
            }
            catch
            {
                DB = -999;
            }
        }

        private void conv_RealImag_to_MagAngle()
        {
            try
            {
                Mag = Math.Sqrt((Real * Real) + (Imag * Imag));
            }
            catch
            {
                Mag = -999;
            }

            calc_Angle();
        }

        private void calc_Angle()
        {
            try
            {
                double radian;
                radian = 0;
                if ((Real == 0) && (Imag == 0))
                    radian = 0;
                else if (Real > 0)
                    radian = Math.Atan(Imag / Real);
                else if (Real == 0)
                    radian = Math.Sign(Imag) * Math.PI / 2;
                else if (Real < 0)
                {
                    if (Imag == 0)
                        radian = Math.Atan(Imag / Real) + Math.PI;
                    else
                        radian = Math.Atan(Imag / Real) + (Math.Sign(Imag) * Math.PI);
                }

                Phase = radian * (180 / Math.PI);
            }
            catch
            {
                Phase = -999;
            }
        }

        public void conv_SParam_to_Impedance(double Z0)
        {
            // This conversion only correct for Reflection Measurement (ie only S11,S22,S33 or S44)
            // Equation base on Application Note 2866 (AN2866)
            // Zin = Z0 * ((1 + S11)/(1 - S11))
            // Return will be impedence 
            try
            {
                Impedance = Z0 * ((1 - Math.Pow(Real, 2) - Math.Pow(Imag, 2)) /
                                      (Math.Pow((1 - Real), 2) + Math.Pow(Imag, 2)));
            }
            catch
            {
                Impedance = -999;
            }
        }
    }

}
