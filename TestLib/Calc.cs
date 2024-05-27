using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace TestLib
{

    public class Calculate : TimingBase, iTest
    {
        public byte Site;
        public string TestParaName;
        public string MathVariable;
        public double result;
        public double[] result_ACLR1 = new double[] { 0, 0 };
        public double[] result_ACLR2 = new double[] { 0, 0 };
        public double[] result_EACLR = new double[] { 0, 0 };
        public static bool MathCalcDicInit = false;
        public static Dictionary<string, double>[] MathCalc = new Dictionary<string, double>[Eq.NumSites];
        public static Dictionary<string, double[]>[] MathCalc_ACLR1 = new Dictionary<string, double[]>[Eq.NumSites];
        public static Dictionary<string, double[]>[] MathCalc_ACLR2 = new Dictionary<string, double[]>[Eq.NumSites];
        public static Dictionary<string, double[]>[] MathCalc_EACLR = new Dictionary<string, double[]>[Eq.NumSites];
        public Dictionary.Ordered<string, DcSetting>DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public static Dictionary<string, ConcurrentDictionary<string, double>>[] MathCalcCurrent = new Dictionary<string, ConcurrentDictionary<string, double>>[Eq.NumSites];
        public DcCalcTestResults TestResults;
        public bool TestGain;
        public bool TestAcp1;
        public bool TestAcp2; 
        public bool TestEUTRA;
        public string CktID;

        public bool Initialize(bool finalScript)
        {
            InitializeTiming(this.TestParaName);

            if(MathCalcDicInit == false)
            {
                //Initialized MathCalc dictionaries
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    MathCalc[site] = new Dictionary<string, double>();
                    MathCalc_ACLR1[site] = new Dictionary<string, double[]>();
                    MathCalc_ACLR2[site] = new Dictionary<string, double[]>();
                    MathCalc_EACLR[site] = new Dictionary<string, double[]>();
                    MathCalcCurrent[site] = new Dictionary<string, ConcurrentDictionary<string, double>>();

                    if (site == Eq.NumSites-1)
                    {
                        MathCalcDicInit = true;
                    }
                }
            }

            return true;
        }
        public int RunTest()
        {
            SwBeginRun(Site);

            try
            {

                string formula = this.MathVariable;
                string formula_org = this.MathVariable;
                string[] Split = formula.Split('-');
                if (!(string.Equals(formula, "") || string.Equals(formula, null)))
                {
                    try
                    {
                        if (TestEUTRA)
                        {
                            double Init_L = MathCalc_EACLR[Site][Split[0]][0];
                            double Post_L = MathCalc_EACLR[Site][Split[1]][0];
                            result_EACLR[0] = Post_L - Init_L;

                            double Init_H = MathCalc_EACLR[Site][Split[0]][1];
                            double Post_H = MathCalc_EACLR[Site][Split[1]][1];
                            result_EACLR[1] = Post_H - Init_H;
                        }
                        if (TestAcp1)
                        {
                            double Init_L = MathCalc_ACLR1[Site][Split[0]][0];
                            double Post_L = MathCalc_ACLR1[Site][Split[1]][0];
                            result_ACLR1[0] = Post_L - Init_L;

                            double Init_H = MathCalc_ACLR1[Site][Split[0]][1];
                            double Post_H = MathCalc_ACLR1[Site][Split[1]][1];
                            result_ACLR1[1] = Post_H - Init_H;
                        }
                        if (TestAcp2)
                        {
                            double Init_L = MathCalc_ACLR2[Site][Split[0]][0];
                            double Post_L = MathCalc_ACLR2[Site][Split[1]][0];
                            result_ACLR2[0] = Post_L - Init_L;

                            double Init_H = MathCalc_ACLR2[Site][Split[0]][1];
                            double Post_H = MathCalc_ACLR2[Site][Split[1]][1];
                            result_ACLR2[1] = Post_H - Init_H;
                        }
                        if (TestGain)
                        {
                            double Init = MathCalc[Site][Split[0]];
                            double Post = MathCalc[Site][Split[1]];

                            result = Post - Init;
                        }

                        CalcCurrentDelta(Split, formula);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No formula calculation expression for TestParaName" + TestParaName);
                }
            }
            catch (Exception e)
            {
            }

            SwEndRun(Site);

            return 0;
        }

        private void CalcCurrentDelta(string[] var, string formula)
        {
            double value1 = 0;
            double value2 = 0;
            TestResults = new DcCalcTestResults();
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (DcSettings[pinName].Test)
                {
                    value1 = 0;
                    value2 = 0;
                    if (!(string.Equals(formula, "") || string.Equals(formula, null)))
                    {
                        try
                        {
                            foreach (KeyValuePair<string, ConcurrentDictionary<string, double>> kp in MathCalcCurrent[Site])
                            {
                                if (kp.Key == var[0])
                                {
                                    value1 = kp.Value[pinName];
                                }

                                if (kp.Key == var[1])
                                {
                                    value2 = kp.Value[pinName];
                                }
                            }
                            TestResults.Imeas[pinName] = value2 - value1;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("No formula calculation expression for TestParaName" + TestParaName);
                    }
                }
            }
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            if (ResultBuilder.headerFileMode) return;

            if (TestEUTRA)
            {
                string ParamHeaderTemp = TestParaName.Replace(CktID, CktID + "E-ACLR-Delta_");
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Lower", "", Math.Round(result_EACLR[0], 4));
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Upper", "", Math.Round(result_EACLR[1], 4));
            }

            if (TestAcp1)
            {
                string ParamHeaderTemp = TestParaName.Replace(CktID, CktID + "ACLR1-Delta_");
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Lower", "", Math.Round(result_ACLR1[0], 4));
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Upper", "", Math.Round(result_ACLR1[1], 4));
            }

            if (TestAcp2)
            {
                string ParamHeaderTemp = TestParaName.Replace(CktID, CktID + "ACLR2-Delta_");
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Lower", "", Math.Round(result_ACLR2[0], 4));
                ResultBuilder.AddResult(Site, ParamHeaderTemp + "Upper", "", Math.Round(result_ACLR2[1], 4));
            }

            if (TestGain)
            {
                string ParamHeaderTemp = TestParaName.Replace(CktID, CktID + "Gain_Delta_");
                ResultBuilder.AddResult(Site, ParamHeaderTemp, "", Math.Round(result, 4));
            }

            //For DC

            foreach (string pinName in DcSettings.Keys)
            {
                if (DcSettings[pinName].Test)
                {
                    string ParamHeaderTempDC = TestParaName.Replace(CktID, CktID + DcSettings[pinName].iParaName + "_Delta_");
                    ResultBuilder.AddResult(Site, ParamHeaderTempDC, "A", TestResults.Imeas[pinName], 9);
                }
            }

        }

        public class DcCalcTestResults
        {
            public double Itotal = 0;
            public ConcurrentDictionary<string, double> Imeas = new ConcurrentDictionary<string, double>();
        }

    }
}
