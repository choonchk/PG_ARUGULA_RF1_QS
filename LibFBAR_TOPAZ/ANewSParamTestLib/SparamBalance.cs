using System;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    public class SparamBalance : TestCaseBase
    {
        private enum e_BalanceType
        {
            CMRR = 0,
            AMPLITUDE,
            PHASE
        }

        #region "Declarations"

        private string SubClassName = "Balance";    // Sub Class Naming

        // External Variables
        public int TestNo;

        public double StartFreq;
        public double StopFreq;
        public string Search_Type;
        public int Channel_Number;
        public string SParameters_1;
        public string SParameters_2;
        public string BalanceType;
        public bool b_Absolute;

        // Internal Variables
        private e_SearchType SearchType;

        private int StartFreqCnt;
        private int StopFreqCnt;
        private int tmpCnt;
        private int SParam_1;
        private int SParam_2;
        private e_BalanceType eBalanceType;

        private double rtnResult;

        private bool Found;
        private static LibFBAR_TOPAZ.cFunction Math_Func;
        /// <summary>
        /// Value not assigned. Obsolete unused?
        /// </summary>
        private static S_CMRRnBal_Param[] SBalanceParamData;


        #endregion "Declarations"

        public void InitSettings()
        {
            Math_Func = new cFunction();
            //ChannelNumber--;
            e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters_1.ToUpper());
            SParam_1 = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];
            SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters_2.ToUpper());
            SParam_2 = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

            SearchType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_Type.ToUpper());

            #region "Start Point"

            tmpCnt = 0;
            Found = false;
            for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
            {
                s_SegmentData sd = SegmentParam[Channel_Number - 1].SegmentData[seg];

                if (StartFreq >= sd.Start && StartFreq <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }
                if (Found == true)
                {
                    StartFreqCnt = Convert.ToInt32(((StartFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt);
                    break;
                }
            }
            if (Found == false)
            {
                string msg = String.Format("Unable to find Start Point : Start Frequency = {0}", StartFreq);
                ShowError(this, msg);
            }

            #endregion "Start Point"

            #region "End Point"

            tmpCnt = 0;
            Found = false;
            for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
            {
                s_SegmentData sd = SegmentParam[Channel_Number - 1].SegmentData[seg];
                if (StopFreq >= sd.Start && StopFreq <= sd.Stop)
                {
                    Found = true;
                }
                else
                {
                    tmpCnt += sd.Points;
                }
                if (Found == true)
                {
                    StopFreqCnt = Convert.ToInt32(((StopFreq - sd.Start) / (sd.Stop - sd.Start) * (sd.Points - 1)) + tmpCnt);
                    break;
                }
            }
            if (Found == false)
            {
                string msg = String.Format("Unable to find Stop Point : Stop Frequency = {0}", StopFreq);
                ShowError(this, msg);
            }

            #endregion "End Point"

            eBalanceType = (e_BalanceType)Enum.Parse(typeof(e_BalanceType), BalanceType.ToUpper());
        }

        public void RunTest()
        {
            MeasureResult();
            SetResult();
        }   //Function to call and run test. Not in used for FBAR

        public void MeasureResult()
        {
            int NoPoints = SParamData[Channel_Number - 1].NoPoints;
            double tmpVal = 0;
            s_DataType tmp_Data_1 = new s_DataType();
            s_DataType tmp_Data_2 = new s_DataType();

            if (eBalanceType == e_BalanceType.CMRR)
            {
                #region "CMRR"

                S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];
                if (!blc.CMRR_Enable)
                {
                    blc.CMRR.sParam = new s_DataType[NoPoints];
                }
                for (int iPts = 0; iPts < NoPoints; iPts++)
                {
                    S_ParamData sp1 = SParamData[Channel_Number - 1].sParam_Data[SParam_1];
                    S_ParamData sp2 = SParamData[Channel_Number - 1].sParam_Data[SParam_2];

                    tmp_Data_1.ReIm = Math_Func.Complex_Number.Minus(sp1.sParam[iPts].ReIm, sp2.sParam[iPts].ReIm);
                    tmp_Data_2.ReIm = Math_Func.Complex_Number.Sum(sp1.sParam[iPts].ReIm, sp2.sParam[iPts].ReIm);
                    tmp_Data_1.dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(tmp_Data_1.ReIm);
                    tmp_Data_2.dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(tmp_Data_2.ReIm);
                    blc.CMRR.sParam[iPts].dBAng.dB = tmp_Data_1.dBAng.dB - tmp_Data_2.dBAng.dB;
                    blc.CMRR.sParam[iPts].dBAng.Angle = 0;
                }
                blc.CMRR_Enable = true;

                #endregion "CMRR"
            }
            else if (eBalanceType == e_BalanceType.AMPLITUDE || eBalanceType == e_BalanceType.PHASE)
            {
                #region "Amplitude & Phase"
                S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                if (!blc.Balance_Enable)
                {
                    blc.Balance.sParam = new s_DataType[NoPoints];
                }
                for (int iPts = 0; iPts < NoPoints; iPts++)
                {
                    dB_Angle sp1 = SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].dBAng;
                    dB_Angle sp2 = SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].dBAng;
                    if (b_Absolute)
                    {
                        blc.Balance.sParam[iPts].dBAng.dB = Math.Abs(sp1.dB - sp2.dB);
                        blc.Balance.sParam[iPts].dBAng.Angle = Math.Abs(sp1.Angle - sp2.Angle);

                        //KCC - Phase
                        blc.Balance.sParam[iPts].dBAng.Angle = blc.Balance.sParam[iPts].dBAng.Angle + 180;
                        if (blc.Balance.sParam[iPts].dBAng.Angle >= 180)
                        {
                            blc.Balance.sParam[iPts].dBAng.Angle = blc.Balance.sParam[iPts].dBAng.Angle - 360;
                        }
                    }
                    else
                    {
                        blc.Balance.sParam[iPts].dBAng.dB = (sp1.dB - sp2.dB);
                        blc.Balance.sParam[iPts].dBAng.Angle = (sp1.Angle - sp2.Angle);

                        //KCC - Phase
                        blc.Balance.sParam[iPts].dBAng.Angle = blc.Balance.sParam[iPts].dBAng.Angle + 180;
                        if (blc.Balance.sParam[iPts].dBAng.Angle >= 180)
                        {
                            blc.Balance.sParam[iPts].dBAng.Angle = blc.Balance.sParam[iPts].dBAng.Angle - 360;
                        }
                    }
                }
                blc.Balance_Enable = true;

                #endregion "Amplitude & Phase"
            }

            if (eBalanceType == e_BalanceType.CMRR)
            {
                if (SearchType == e_SearchType.MAX)
                {
                    tmpVal = -999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal < blc.CMRR.sParam[iCnt].dBAng.dB)
                        {
                            tmpVal = blc.CMRR.sParam[iCnt].dBAng.dB;
                            tmpCnt = iCnt;
                        }
                    }
                }
                else if (SearchType == e_SearchType.MIN)
                {
                    tmpVal = 999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal > blc.CMRR.sParam[iCnt].dBAng.dB)
                        {
                            tmpVal = blc.CMRR.sParam[iCnt].dBAng.dB;
                            tmpCnt = iCnt;
                        }
                    }
                }
            }
            else if (eBalanceType == e_BalanceType.AMPLITUDE)
            {
                if (SearchType == e_SearchType.MAX)
                {
                    tmpVal = -999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal < blc.Balance.sParam[iCnt].dBAng.dB)
                        {
                            tmpVal = blc.Balance.sParam[iCnt].dBAng.dB;
                            tmpCnt = iCnt;
                        }
                    }
                }
                else if (SearchType == e_SearchType.MIN)
                {
                    tmpVal = 999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal > blc.Balance.sParam[iCnt].dBAng.dB)
                        {
                            tmpVal = blc.Balance.sParam[iCnt].dBAng.dB;
                            tmpCnt = iCnt;
                        }
                    }
                }
            }
            else if (eBalanceType == e_BalanceType.PHASE)
            {
                if (SearchType == e_SearchType.MAX)
                {
                    tmpVal = -999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal < blc.Balance.sParam[iCnt].dBAng.Angle)
                        {
                            tmpVal = blc.Balance.sParam[iCnt].dBAng.Angle;
                            tmpCnt = iCnt;
                        }
                    }
                }
                else if (SearchType == e_SearchType.MIN)
                {
                    tmpVal = 999999;
                    S_CMRRnBal_Param blc = SBalanceParamData[Channel_Number - 1];

                    for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                    {
                        if (tmpVal > blc.Balance.sParam[iCnt].dBAng.Angle)
                        {
                            tmpVal = blc.Balance.sParam[iCnt].dBAng.Angle;
                            tmpCnt = iCnt;
                        }
                    }
                }
            }
            rtnResult = tmpVal;
        }

        private void SetResult()
        {
            SaveResult.Result_Data = rtnResult;
            SaveResult.Misc = tmpCnt;
        }
    }

    public struct Real_Imag
    {
        public double Real;
        public double Imag;
    }
    public struct dB_Angle
    {
        public double dB;
        public double Angle;
    }
    public struct Mag_Angle
    {
        public double Mag;
        public double Angle;
    }

    public class cFunction
    {
        public cComplex_Number Complex_Number = new cComplex_Number();
        public cMatrics Matrics = new cMatrics();
        public cConversion Conversion = new cConversion();
        public class cComplex_Number
        {
            public Real_Imag Sum(Real_Imag Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real + Z2.Real;
                tmp.Imag = Z1.Imag + Z2.Imag;
                return (tmp);
            }
            public Real_Imag Sum(double Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1 + Z2.Real;
                tmp.Imag = Z2.Imag;
                return (tmp);
            }
            public Real_Imag Sum(Real_Imag Z1, double Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real + Z2;
                tmp.Imag = Z1.Imag;
                return (tmp);
            }
            public Real_Imag Sum(Real_Imag Z1, Real_Imag Z2, Real_Imag Z3)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real + Z2.Real + Z3.Real;
                tmp.Imag = Z1.Imag + Z2.Imag + Z3.Imag;
                return (tmp);
            }
            public Real_Imag Sum(Real_Imag Z1, Real_Imag Z2, Real_Imag Z3, Real_Imag Z4)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real + Z2.Real + Z3.Real + Z4.Real;
                tmp.Imag = Z1.Imag + Z2.Imag + Z3.Imag + Z4.Imag;
                return (tmp);
            }

            public Real_Imag Minus(Real_Imag Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real - Z2.Real;
                tmp.Imag = Z1.Imag - Z2.Imag;
                return (tmp);
            }
            public Real_Imag Minus(double Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1 - Z2.Real;
                tmp.Imag = -Z2.Imag;
                return (tmp);
            }
            public Real_Imag Minus(Real_Imag Z1, double Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real - Z2;
                tmp.Imag = Z1.Imag;
                return (tmp);
            }
            public Real_Imag Minus(Real_Imag Z1, Real_Imag Z2, Real_Imag Z3)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real - Z2.Real - Z3.Real;
                tmp.Imag = Z1.Imag - Z2.Imag - Z3.Imag;
                return (tmp);
            }
            public Real_Imag Minus(Real_Imag Z1, Real_Imag Z2, Real_Imag Z3, Real_Imag Z4)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real - Z2.Real - Z3.Real - Z4.Real;
                tmp.Imag = Z1.Imag - Z2.Imag - Z3.Imag - Z4.Imag;
                return (tmp);
            }

            public Real_Imag Multiply(Real_Imag Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = (Z1.Real * Z2.Real) - (Z1.Imag * Z2.Imag);
                tmp.Imag = (Z1.Imag * Z2.Real) + (Z1.Real * Z2.Imag);
                return (tmp);
            }
            public Real_Imag Multiply(Real_Imag Z1, double Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real * Z2;
                tmp.Imag = Z1.Imag * Z2;
                return (tmp);
            }
            public Real_Imag Multiply(double Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z2.Real * Z1;
                tmp.Imag = Z2.Imag * Z1;
                return (tmp);
            }

            public Real_Imag Divide(Real_Imag Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                double Denominator = Math.Pow(Z2.Real, 2) + Math.Pow(Z2.Imag, 2);
                tmp.Real = ((Z1.Real * Z2.Real) + (Z1.Imag * Z2.Imag)) / Denominator;
                tmp.Imag = ((Z2.Real * Z1.Imag) - (Z1.Real * Z2.Imag)) / Denominator;
                return (tmp);
            }
            public Real_Imag Divide(double Z1, Real_Imag Z2)
            {
                Real_Imag tmp = new Real_Imag();
                double Denominator = Math.Pow(Z2.Real, 2) + Math.Pow(Z2.Imag, 2);
                tmp.Real = (Z1 * Z2.Real) / Denominator;
                tmp.Imag = (-(Z1 * Z2.Imag)) / Denominator;
                return (tmp);
            }
            public Real_Imag Divide(Real_Imag Z1, double Z2)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Real / Z2;
                tmp.Imag = Z1.Imag / Z2;
                return (tmp);
            }

            public Real_Imag Negative(Real_Imag Z1)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = -Z1.Real;
                tmp.Imag = -Z1.Imag;
                return (tmp);
            }

            public Real_Imag Convert2Real_Imag(double Real, double Imag)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Real;
                tmp.Imag = Imag;
                return (tmp);
            }

        }
        public class cMatrics : cComplex_Number
        {
            public Real_Imag[,] Sum(Real_Imag[,] Z1, Real_Imag[,] Z2)
            {
                int N;
                Real_Imag[,] tmp;

                if (Z1.Length != Z2.Length)
                {
                    tmp = new Real_Imag[0, 0];
                    return (tmp);
                }

                N = (int)Math.Sqrt(Z1.Length);
                tmp = new Real_Imag[N, N];

                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        tmp[x, y] = Sum(Z1[x, y], Z2[x, y]);
                    }
                }
                return (tmp);
            }
            public Real_Imag[,] Minus(Real_Imag[,] Z1, Real_Imag[,] Z2)
            {
                int N;
                Real_Imag[,] tmp;

                if (Z1.Length != Z2.Length)
                {
                    tmp = new Real_Imag[0, 0];
                    return (tmp);
                }

                N = (int)Math.Sqrt(Z1.Length);
                tmp = new Real_Imag[N, N];

                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        tmp[x, y] = Minus(Z1[x, y], Z2[x, y]);
                    }
                }
                return (tmp);
            }
            public Real_Imag[,] Multiply(Real_Imag[,] Z1, Real_Imag[,] Z2)
            {
                int N;
                Real_Imag[,] tmp;

                if (Z1.Length != Z2.Length)
                {
                    tmp = new Real_Imag[0, 0];
                    return (tmp);
                }

                N = (int)Math.Sqrt(Z1.Length);
                tmp = new Real_Imag[N, N];

                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        for (int z = 0; z < N; z++)
                        {
                            tmp[x, y] = Sum(Z1[x, y], Multiply(Z1[x, z], Z2[z, y]));
                        }
                    }
                }
                return (tmp);
            }
            public Real_Imag[,] Inverse(Real_Imag[,] Z1)
            {
                int N = (int)Math.Sqrt(Z1.Length);
                Real_Imag[,] tmp = new Real_Imag[N, N];
                Real_Imag[,] Z = new Real_Imag[N, N];
                Real_Imag div = new Real_Imag();
                Real_Imag mun = new Real_Imag();
                int Fix;
                for (int A = 0; A < N; A++)
                {
                    tmp[A, A].Real = 1;
                }
                Z = Z1;
                Fix = 0;
                for (int x = 0; x < N; x++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        if (y == 0)
                        {
                            Fix = x;
                            if ((Z[x, x].Real != 0) || (Z[x, x].Imag != 0))
                            {
                                div = Z[x, x];
                                for (int k = 0; k < N; k++)
                                {
                                    Z[Fix, k] = Divide(Z[Fix, k], div);
                                    tmp[Fix, k] = Divide(tmp[Fix, k], div);
                                }
                            }
                        }
                        if (y != x)
                        {
                            mun = Z[y, x];
                            for (int l = 0; l < N; l++)
                            {
                                Z[y, l] = Minus(Z[y, l], Multiply(mun, Z[Fix, l]));
                                tmp[y, l] = Minus(tmp[y, l], Multiply(mun, tmp[Fix, l]));
                            }
                        }
                    }
                }
                return (tmp);

            }
            public Real_Imag[,] Divide(Real_Imag[,] Z1, Real_Imag[,] Z2)
            {
                int N;
                Real_Imag[,] tmp;

                if (Z1.Length != Z2.Length)
                {
                    tmp = new Real_Imag[0, 0];
                    return (tmp);
                }

                N = (int)Math.Sqrt(Z1.Length);
                tmp = new Real_Imag[N, N];

                tmp = Multiply(Inverse(Z2), Z1);
                return (tmp);
            }
        }
        public class cConversion : cComplex_Number
        {
            public Real_Imag conv_MagAngle_to_RealImag(Mag_Angle Z1)
            {
                Real_Imag tmp = new Real_Imag();
                tmp.Real = Z1.Mag * Math.Cos(Z1.Angle);
                tmp.Imag = Z1.Mag * Math.Sin(Z1.Angle);
                return (tmp);
            }
            public dB_Angle conv_RealImag_to_dBAngle(Real_Imag Z1)
            {
                Mag_Angle tmp = new Mag_Angle();
                dB_Angle rtn = new dB_Angle();

                tmp = conv_RealImag_to_MagAngle(Z1);
                rtn.dB = 20 * (Math.Log10(tmp.Mag));
                rtn.Angle = tmp.Angle;
                return (rtn);
            }
            public Mag_Angle conv_RealImag_to_MagAngle(Real_Imag Z1)
            {
                Mag_Angle tmp = new Mag_Angle();
                tmp.Mag = Math.Sqrt((Z1.Real * Z1.Real) + (Z1.Imag * Z1.Imag));
                tmp.Angle = calc_Angle(Z1);
                return (tmp);
            }
            public double calc_Angle(Real_Imag Z1)
            {
                double radian;
                radian = 0;
                if ((Z1.Real == 0) && (Z1.Imag == 0))
                    radian = 0;
                else if (Z1.Real > 0)
                    radian = Math.Atan(Z1.Imag / Z1.Real);
                else if (Z1.Real == 0)
                    radian = Math.Sign(Z1.Imag) * Math.PI / 2;
                else if (Z1.Real < 0)
                {
                    if (Z1.Imag == 0)
                        radian = Math.Atan(Z1.Imag / Z1.Real) + Math.PI;
                    else
                        radian = Math.Atan(Z1.Imag / Z1.Real) + (Math.Sign(Z1.Imag) * Math.PI);
                }
                return (radian * (180 / Math.PI));
            }
        }
    }
}