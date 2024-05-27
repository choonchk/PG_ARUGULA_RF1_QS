using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibFBAR
{
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
            public double conv_Mag_to_dB(double mag)
            {
                double rtn_dB;

                rtn_dB = 20 * (Math.Log10(mag));
                return (rtn_dB);
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
                return(radian * (180/Math.PI));
            }
        }
    }
}
