using EqLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestLib
{
    public static class DPAT
    {
        public static Dictionary<string, DPATCondition>[] DPAT_Dic = new Dictionary<string, DPATCondition>[Eq.NumSites];

        public static bool IsExisting(string Paraname, int site)
        {
            return DPAT_Dic[site].ContainsKey(Paraname);
        }

        public static void Clear(int site)
        {
            DPAT_Dic[site]?.Clear();
        }

        public static int AddPara(string Paraname, string Formula, string SpecCondition, string SetVlaue, double LSL, double USL, int site)
        {
            DPATCondition Conditon = new DPATCondition();

            Conditon.Formulas = Formula;
            Conditon.SetSpecCondition = SpecCondition;
            Conditon.Y = Convert.ToDouble(SetVlaue);
            Conditon.Data = new List<double>();
            Conditon.Count = 0;
            Conditon.IsEnabledPAT_USL = false;
            Conditon.Hard_LSL = LSL;
            Conditon.Hard_USL = USL;

            if(DPAT_Dic[site] == null) { DPAT_Dic[site] = new Dictionary<string, DPATCondition>(); }
            DPAT_Dic[site].Add(Paraname.Replace("V." + Conditon.Formulas + "." + Conditon.SetSpecCondition, "x"), Conditon);

            DPAT_Dic[site][Paraname].IsActualTesting = true;
            return 1;
        }

        public static bool AddData(string Paraname, double Value, bool Simulation, int site)
        {
            if (DPAT_Dic[site].ContainsKey(Paraname))
            {
                if (DPAT_Dic[site][Paraname].IsActualTesting)
                {
                    if (!Simulation) DPAT_Dic[site][Paraname].Count++;

                    int a = DPAT_Dic[site][Paraname].Xint[DPAT_Dic[site][Paraname].XintCount];
                    if (DPAT_Dic[site][Paraname].Count - 1 == DPAT_Dic[site][Paraname].Xint[DPAT_Dic[site][Paraname].XintCount])
                    {
                        if (DPAT_Dic[site][Paraname].Formulas.ToUpper() == "DPAT") Calculate_DPAT(Paraname, site);
                        else Calculate_AECDPAT(Paraname, site);

                        if (DPAT_Dic[site][Paraname].SetSpecCondition.ToUpper() == "L")
                        {
                            //DPAT_Dic[Paraname].USL = 999;
                        }
                        else if (DPAT_Dic[site][Paraname].SetSpecCondition.ToUpper() == "U")
                        {
                            //DPAT_Dic[Paraname].LSL = -999;
                        }
                        else if (DPAT_Dic[site][Paraname].SetSpecCondition.ToUpper() == "B")
                        {
                        }
                        DPAT_Dic[site][Paraname].Count = 1;
                        DPAT_Dic[site][Paraname].XintCount++;
                        DPAT_Dic[site][Paraname].Data.Add(Math.Round(Value, 12));
                        return true;
                    }
                    else
                    {
                        if (!Simulation)
                            DPAT_Dic[site][Paraname].Data.Add(Math.Round(Value, 12));
                    }
                }
            }

            return false;
        }

        public static void Calculate_DPAT(string Paraname, int site)
        {
            double[] data = DPAT_Dic[site][Paraname].Data.ToArray();

            Array.Sort(data);
            double P25 = 25f;
            double P50 = 50f;
            double P75 = 75f;

            double P75_rank = (P75 / 100) * (data.Length + 1);

            int P75_rank_int = (int)P75_rank;
            double P75_rank_dec = P75_rank - P75_rank_int;

            double rank_dec_comp = 1 - P75_rank_dec;
            double data_n = data[P75_rank_int - 1];
            double data_n_1 = data[P75_rank_int];

            DPAT_Dic[site][Paraname].P75 = (data_n * rank_dec_comp) + (data_n_1 * P75_rank_dec);

            double P25_rank = (P25 / 100) * (data.Length + 1);

            int P25_rank_int = (int)P25_rank;
            double P25_rank_dec = P25_rank - P25_rank_int;

            rank_dec_comp = 1 - P25_rank_dec;
            data_n = data[P25_rank_int - 1];
            data_n_1 = data[P25_rank_int];

            DPAT_Dic[site][Paraname].P25 = (data_n * rank_dec_comp) + (data_n_1 * P25_rank_dec);

            double P50_rank = (P50 / 100) * (data.Length + 1);

            int P50_rank_int = (int)P50_rank;
            double P50_rank_dec = P50_rank - P50_rank_int;

            rank_dec_comp = 1 - P50_rank_dec;
            data_n = data[P50_rank_int - 1];
            data_n_1 = data[P50_rank_int];

            double P50data = (data_n * rank_dec_comp) + (data_n_1 * P50_rank_dec);

            DPAT_Dic[site][Paraname].Median = P50data;

            double LowQ = DPAT_Dic[site][Paraname].P25;
            double HowQ = DPAT_Dic[site][Paraname].P75;

            DPAT_Dic[site][Paraname].Q1 = DPAT_Dic[site][Paraname].P25;
            DPAT_Dic[site][Paraname].Q3 = DPAT_Dic[site][Paraname].P75;

            DPAT_Dic[site][Paraname].IQR = HowQ - LowQ;

            DPAT_Dic[site][Paraname].LSL = DPAT_Dic[site][Paraname].Median - (DPAT_Dic[site][Paraname].Y * (DPAT_Dic[site][Paraname].IQR / 1.35));
            DPAT_Dic[site][Paraname].USL = DPAT_Dic[site][Paraname].Median + (DPAT_Dic[site][Paraname].Y * (DPAT_Dic[site][Paraname].IQR / 1.35));

            DPAT_Dic[site][Paraname].IsEnabledPAT_USL = true;

            //double[] data = DPAT_Dic[Paraname].Data.ToArray();

            //Array.Sort(data);

            ////get the media
            //int size = data.Length;
            //int mid = size / 2;

            //int middle = data.Length / 2;
            //int Lowermiddle = middle / 2;
            //int Uppermiddle = data.Length - Lowermiddle - 1;

            //DPAT_Dic[Paraname].Median = (size % 2 != 0) ? data[mid] : (data[mid] + data[mid - 1]) / 2;

            //if (data.Length % 2 == 0)
            //{
            //    DPAT_Dic[Paraname].Q1 = (data[Lowermiddle] + data[Lowermiddle - 1]) / 2;
            //    DPAT_Dic[Paraname].Q3 = (data[Uppermiddle] + data[Uppermiddle + 1]) / 2;
            //}
            //else
            //{
            //    DPAT_Dic[Paraname].Q1 = data[Lowermiddle];
            //    DPAT_Dic[Paraname].Q3 = data[Uppermiddle];
            //}

            //double LowQ = DPAT_Dic[Paraname].Q1;
            //double HowQ = DPAT_Dic[Paraname].Q3;

            //DPAT_Dic[Paraname].IQR = HowQ - LowQ;

            //DPAT_Dic[Paraname].LSL = DPAT_Dic[Paraname].Median - (DPAT_Dic[Paraname].Y * (DPAT_Dic[Paraname].IQR / 1.35));
            //DPAT_Dic[Paraname].USL = DPAT_Dic[Paraname].Median + (DPAT_Dic[Paraname].Y * (DPAT_Dic[Paraname].IQR / 1.35));

            //DPAT_Dic[Paraname].IsEnabledPAT_USL = true;
        }

        public static void Calculate_AECDPAT(string Paraname, int site)
        {
            Stopwatch S = new Stopwatch();
            S.Start();

            double[] data = DPAT_Dic[site][Paraname].Data.ToArray();

            Array.Sort(data);
            double P01 = 1f;
            double P50 = 50f;
            double P99 = 99f;

            double P99_rank = (P99 / 100) * (data.Length + 1);

            int P99_rank_int = (int)P99_rank;
            double P99_rank_dec = P99_rank - P99_rank_int;

            double rank_dec_comp = 1 - P99_rank_dec;
            double data_n = data[P99_rank_int - 1];
            double data_n_1 = data[P99_rank_int];

            DPAT_Dic[site][Paraname].P99 = (data_n * rank_dec_comp) + (data_n_1 * P99_rank_dec);

            double P01_rank = (P01 / 100) * (data.Length + 1);

            int P01_rank_int = (int)P01_rank;
            double P01_rank_dec = P01_rank - P01_rank_int;

            rank_dec_comp = 1 - P01_rank_dec;
            data_n = data[P01_rank_int - 1];
            data_n_1 = data[P01_rank_int];

            DPAT_Dic[site][Paraname].P01 = (data_n * rank_dec_comp) + (data_n_1 * P01_rank_dec);

            double P50_rank = (P50 / 100) * (data.Length + 1);

            int P50_rank_int = (int)P50_rank;
            double P50_rank_dec = P50_rank - P50_rank_int;

            rank_dec_comp = 1 - P50_rank_dec;
            data_n = data[P50_rank_int - 1];
            data_n_1 = data[P50_rank_int];

            double P50data = (data_n * rank_dec_comp) + (data_n_1 * P50_rank_dec);

            DPAT_Dic[site][Paraname].Median = P50data;

            DPAT_Dic[site][Paraname].LSL = P50data - (DPAT_Dic[site][Paraname].Y * (P50data - DPAT_Dic[site][Paraname].P01) * 0.43);
            DPAT_Dic[site][Paraname].USL = P50data + (DPAT_Dic[site][Paraname].Y * (DPAT_Dic[site][Paraname].P99 - P50data) * 0.43);

            DPAT_Dic[site][Paraname].IsEnabledPAT_USL = true;

            S.Stop();

            double t = S.ElapsedMilliseconds;
        }

        public static void Initiate(byte Site, string Paraname, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
        {
            bool Flag = false;
            bool RF1_Outlier_Flag = false;
            string _DPATString = "_MORDOR";

            if (OTP_Procedure.OTP_Read_RF1_Outlier_Bit(Site)) //(OtpTestEngine.OTP_Read_RF1_Outlier_Bit(Site)) - Attention
            {
                RF1_Outlier_Flag = true;
            }
            else if (ResultBuilder.FailedTests[Site].Count == 0)
            {
                if (DPAT.DPAT_Dic[Site][Paraname].IsitFirsttesting == true)
                {
                    DPAT.DPAT_Dic[Site][Paraname].IsitFirsttesting = false;
                    Flag = DPAT.AddData(Paraname, ResultBuilder.ParametersDict[Site][Paraname], ResultBuilder.Isfirststep, Site);
                }
                else if (DPAT.DPAT_Dic[Site][Paraname].P_LSL < ResultBuilder.ParametersDict[Site][Paraname] && ResultBuilder.ParametersDict[Site][Paraname] < DPAT.DPAT_Dic[Site][Paraname].P_USL)
                {
                    Flag = DPAT.AddData(Paraname, ResultBuilder.ParametersDict[Site][Paraname], ResultBuilder.Isfirststep, Site);
                }
            }

            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Q1", "A", DPAT.DPAT_Dic[Site][Paraname].Q1, 12);
            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Q3", "A", DPAT.DPAT_Dic[Site][Paraname].Q3, 12);
            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Median", "A", DPAT.DPAT_Dic[Site][Paraname].Median, 12);
            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_IQR", "A", DPAT.DPAT_Dic[Site][Paraname].IQR, 12);

            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_P01", "A", DPAT.DPAT_Dic[Site][Paraname].P01, 12);
            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_P99", "A", DPAT.DPAT_Dic[Site][Paraname].P99, 12);

            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Lspec", "A", DPAT.DPAT_Dic[Site][Paraname].LSL, 12);
            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Hspec", "A", DPAT.DPAT_Dic[Site][Paraname].USL, 12);

            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_Xint", "A", DPAT.DPAT_Dic[Site][Paraname].Count, 12);

            if (DPAT.DPAT_Dic[Site][Paraname].IsEnabledPAT_USL && Flag)
            {
                if (DPAT.DPAT_Dic[Site][Paraname].SetSpecCondition.ToUpper() == "L")
                {
                    if (DPAT.DPAT_Dic[Site][Paraname].Hard_LSL > DPAT.DPAT_Dic[Site][Paraname].LSL)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_LSL, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_USL, 12);

                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].Hard_LSL;
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].Hard_USL;
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].LSL, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_USL, 12);

                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].LSL;
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].Hard_USL;
                    }
                }
                else if (DPAT.DPAT_Dic[Site][Paraname].SetSpecCondition.ToUpper() == "U")
                {
                    if (DPAT.DPAT_Dic[Site][Paraname].Hard_USL > DPAT.DPAT_Dic[Site][Paraname].USL)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_LSL, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].USL, 12);

                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].Hard_LSL;
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].USL;
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_LSL, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].Hard_USL, 12);

                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].Hard_LSL;
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].Hard_USL;
                    }
                }
                else if (DPAT.DPAT_Dic[Site][Paraname].SetSpecCondition.ToUpper() == "B")
                {
                    if (DPAT.DPAT_Dic[Site][Paraname].Hard_LSL > DPAT.DPAT_Dic[Site][Paraname].LSL)
                    {
                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].Hard_LSL;
                    }
                    else
                    {
                        DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].LSL;
                    }

                    if (DPAT.DPAT_Dic[Site][Paraname].Hard_USL > DPAT.DPAT_Dic[Site][Paraname].USL)
                    {
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].USL;
                    }
                    else
                    {
                        DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].Hard_USL;
                    }

                    ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_LSL, 12);
                    ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_USL, 12);
                }

                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_FailedCount", "A", ResultBuilder.FailedTests[Site].Count, 12);

                if (DPAT.DPAT_Dic[Site][Paraname].P_LSL < ResultBuilder.ParametersDict[Site][Paraname] && ResultBuilder.ParametersDict[Site][Paraname] < DPAT.DPAT_Dic[Site][Paraname].P_USL)
                {
                    if (RF1_Outlier_Flag == true)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 0, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                    }
                }
                else
                {
                    if (ResultBuilder.CheckPass(Paraname, ResultBuilder.ParametersDict[Site][Paraname]))
                    {
                        // OtpTestEngine.OTP_Burn_RF1_Outlier_Flag(Site, MipiCommands); - Attention
                        OTP_Procedure.OTP_Burn_Custom(Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_OUTLIER_EFUSE"), "80");

                        if (RF1_Outlier_Flag == true)
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 1, 12);
                        }
                    }
                    else
                    {
                        if (RF1_Outlier_Flag == true)
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                        }
                    }
                }
            }
            else if (DPAT.DPAT_Dic[Site][Paraname].IsEnabledPAT_USL && !Flag)
            {
                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_LSL, 12);
                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_USL, 12);

                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_FailedCount", "A", ResultBuilder.FailedTests[Site].Count, 12);

                if (DPAT.DPAT_Dic[Site][Paraname].P_LSL < ResultBuilder.ParametersDict[Site][Paraname] && ResultBuilder.ParametersDict[Site][Paraname] < DPAT.DPAT_Dic[Site][Paraname].P_USL)
                {
                    if (RF1_Outlier_Flag == true)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 0, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                    }
                }
                else
                {
                    if (ResultBuilder.CheckPass(Paraname, ResultBuilder.ParametersDict[Site][Paraname]))
                    {
                        //OtpTestEngine.OTP_Burn_RF1_Outlier_Flag(Site, MipiCommands); - Attention
                        OTP_Procedure.OTP_Burn_Custom(Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_OUTLIER_EFUSE"), "80");

                        if (RF1_Outlier_Flag == true)
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 1, 12);
                        }
                    }
                    else
                    {
                        if (RF1_Outlier_Flag == true)
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                            ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                        }
                    }
                }
            }
            else
            {
                DPAT.DPAT_Dic[Site][Paraname].P_LSL = DPAT.DPAT_Dic[Site][Paraname].Hard_LSL;
                DPAT.DPAT_Dic[Site][Paraname].P_USL = DPAT.DPAT_Dic[Site][Paraname].Hard_USL;

                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_LErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_LSL, 12);
                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_UErosLimit", "A", DPAT.DPAT_Dic[Site][Paraname].P_USL, 12);

                ResultBuilder.AddResult(Site, Paraname + _DPATString + "_FailedCount", "A", ResultBuilder.FailedTests[Site].Count, 12);

                if (ResultBuilder.CheckPass(Paraname, ResultBuilder.ParametersDict[Site][Paraname]))
                {
                    if (RF1_Outlier_Flag == true)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 0, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                    }
                }
                else
                {
                    if (RF1_Outlier_Flag == true)
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 2, 12);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_ErosFlag", "A", 1, 12);
                        ResultBuilder.AddResult(Site, Paraname + _DPATString + "_OTPBurnd", "A", 0, 12);
                    }
                }
            }
        }
    }

    public class DPATCondition
    {
        public string Formulas { get; set; }
        public List<double> Data { get; set; }
        public double Median = 999;
        public double Q1 = 999;
        public double Q3 = 999;
        public double P01 = 999;
        public double P25 = 999;
        public double P75 = 999;
        public double P99 = 999;
        public double IQR = 999;
        public string SetSpecCondition { get; set; }
        public double Y = 999;
        public double P_LSL = 999;
        public double P_USL = 999;
        public double LSL = 999;
        public double USL = 999;
        public double Hard_LSL = 999;
        public double Hard_USL = 999;
        public bool IsEnabledPAT_USL { get; set; }
        public int Count = 999999;

        public int[] Xint = (int[])Enum.GetValues(typeof(Interval));
        public int XintCount = 0;
        public bool IsitFirsttesting = true;

        public bool IsActualTesting;
    }

    public class DPAT_Variable
    {
        public string Fomula;
        public string SpecCondition;
        public string SetValue;
    }

    public enum Interval : int
    {
        //X0 = 10,
        //X1 = 10,
        //X2 = 20,
        //X5 = 50,
        //X8 = 80,
        //X10 = 100,
        //X20 = 200,
        //X50 = 500,
        //X100 = 1000,
        //X200 = 2000,
        //X300 = 3000,
        //X400 = 4000,
        //X500 = 5000,
        //X600 = 6000,
        //X700 = 7000,

        X0 = 100,
        X1 = 100,
        X2 = 200,
        X5 = 500,
        X8 = 800,
        X10 = 1000,
        X20 = 2000,
        X50 = 5000,
        X100 = 10000,
        X200 = 20000,
    }

    public enum SpecCondition
    {
        L,
        H,
        BOTH
    }

    public enum Formulas
    {
        DPAT,
        AECDPAT
    }
}