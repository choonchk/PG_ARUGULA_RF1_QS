using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.Windows.Forms;
using System.Text.RegularExpressions;


using Avago.ATF.Shares;
using Avago.ATF.StandardLibrary;
using Avago.ATF.LogService;
using Avago.ATF.Logger;
using System.IO;
using System.Threading;

using EqLib;
using ClothoLibAlgo;
using SnP_BuddyFileBuilder;


namespace TestLib
{
    public class Legacy_FbarTest : iTest
    {
        public string TestMode;
        public string TestParaName;
        public string Band;

        public int chan;
        public string Trace_Name;
        public int trace;
        public double startFreq;
        public double endFreq;
        public string PowerMode;
        public string SearchType;
        public string RippleBW;
        public double SearchTarget;
        public bool Trigger;
        public string MipiDacBitQ2;
        public string MipiDacBitQ1;
        public string TestNote;
        public string MathVariable;

        public List<double> holdit = new List<double>();
        public List<double> zMag = new List<double>();
        public List<double> zAng = new List<double>();
        public static double[] FullTrace;  
        private static SnPFileBuilder SecResHolder;
        public static string Port_Impedance;

        public static bool updateflag;

        public Dictionary<string, DcSetting> SmuSettingsDictNA = new Dictionary<string, DcSetting>();

        public double ENAresult;
        public double ENAresultFreq;

        public static Dictionary.DoubleKey<int, int, double[]> traceData = new Dictionary.DoubleKey<int, int, double[]>();
        public static Dictionary<int, List<double>> freqList = new Dictionary<int, List<double>>();
        public static Dictionary<string, double> MathCalcLegacy = new Dictionary<string, double>();

        SnPFileBuilder SecResFile;

        //declarations from Topaz_FbarTest.cs
        public static byte ActiveSite;
        public static byte PassiveSite;
        public static Dictionary<byte, byte> Sync_Group = new Dictionary<byte, byte>();
        public byte Site;
        public static bool SynchronizaionFlag = false;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public string ParaFunc;
        //public static Dictionary<string, double> MathCalc0 = new Dictionary<string, double>();
        //public static Dictionary<string, double> MathCalc1 = new Dictionary<string, double>();
        public List<Operation> ActivePath = new List<Operation>();
        public int RowNum;
        public string Sparam;
        public string formula;

        public static class TestModes
        {
            //public const string NA = "NA";
            //public const string CONFIG = "CONFIG";
            //public const string COMMON = "COMMON";
            public const string FBAR = "FBAR";
            public const string CONFIG = "CONFIG";
            public const string DC = "DC";
            public const string COMMON = "COMMON";
        }

        public static class SearchTypes
        {
            public const string LEFT = "LEFT";
            public const string RIGHT = "RIGHT";
            public const string MAX = "MAX";
            public const string MIN = "MIN";
            public const string AVG = "AVG";
            public const string RIPPLE = "RIPPLE";
        }

        public void PresetSynSite()
        {
            if (Sync_Group.Keys.Contains(Site)) ActiveSite = Site;
            if (Sync_Group.Values.Contains(Site)) PassiveSite = Site;
        }

        public bool Initialize(bool FinalScript)
        {
            return true;
        }

        public int RunTest()
        {

            try
            {
                SecResHolder = SecResFile;
                DataFiles.SNP.snpFlag.WaitOne(); // finish all snp acquisitions before test
                PresetSynSite();

                #region If TestMode = CONFIG or DC

                if (Site == ActiveSite)
                {
                    if (Sync_Group[Site] == Site) SynchronizaionFlag = false;
                    else SynchronizaionFlag = true;
                }
                if (TestMode == "CONFIG") return 0;

                if (TestMode == "DC" || Trigger)
                {

                    this.ConfigureVoltageAndCurrent();

                   
                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands);
                 }
                #endregion If TestMode = CONFIG or DC


                #region Trigger?
                if (TestMode != "CONFIG"&&(this.ParaFunc.ToUpper() == "TRIGGER"||Trigger))
                {
                    try
                    {
                        //if (Sync_Group.Keys.Contains(Site)) MathCalc0.Clear();
                        //if (Sync_Group.Values.Contains(Site)) MathCalc1.Clear();
                        traceData[chan] = new Dictionary<int, double[]>(); // Clear previous trace data  KH 3/20/17

                        #region Setup and trigger measurement

                        ////Set bias
                        //this.ConfigureVoltageAndCurrent();

                        ////take care of mipi vectors
                        //foreach (string MipiWaveform in MipiWaveformNames)
                        //{
                        //    Eq.Site[Site].HSDIO.SendVector(MipiWaveform);
                        //}

                        //Take care of switching
                        foreach (Operation SwitchPath in ActivePath)
                        {
                            Eq.Site[Site].SwMatrix.ActivatePath(this.Band, SwitchPath);
                        }

                        //Do a single trigger capture of the traces
                        Eq.Site[Site].EqNetAn.SendTrigger(false, chan);

                        //Get the frequency list
                        freqList[chan] = Eq.Site[Site].EqNetAn.ReadFreqList_Chan(chan);

                        #endregion Setup and trigger measurement

                        DataFiles.SNP.SaveToENA(chan, trace, TestNote, "", 4);
                        
                        //Legacy_FbarTest.DataFiles.SNP.AcquireAll(); // save S2p file as it is created


                       // DataFiles.SNP.SaveToENA(chan, trace, Band + "_" + PowerMode, NaSetup.Chan[chan].NaPortsString, NaSetup.Chan[chan].NaPortCombos.Count);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
                    }

                } //End of if (Trigger) 
                #endregion Trigger?

                switch (TestMode)
                {
                    case TestModes.FBAR:
                        Search(SecResFile);
                        break;
                }

                // BuildResults(ref ResultBuilder.results);  This will be called after all FBAR tests are run at testplan_custom level  KH 3/20/17
                return 0;
            }

            catch (Exception e)
            {
                MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
                return 0;
            }
        }

        //public int RunTest(ref ATFReturnResult sb, SnPFileBuilder SecResFile)
        //{

        //    try
        //    {
        //        SecResHolder = SecResFile;
        //        DataFiles.SNP.snpFlag.WaitOne(); // finish all snp acquisitions before test
        //        PresetSynSite();

        //        #region If TestMode = CONFIG or DC
        //        if (TestMode == "CONFIG")
        //        {
        //            return 0;  //Do nothing
        //        }

        //        if (Site == ActiveSite)
        //        {
        //            if (Sync_Group[Site] == Site) SynchronizaionFlag = false;
        //            else SynchronizaionFlag = true;
        //        }


        //        if (TestMode == "DC")
        //        {
        //            MessageBox.Show("Please remove all parameters in the FBAR tab with TestMode = DC" + "\r\n" +
        //                "Consolidate the settings with Test Parameter = TRIGGER");
        //            //this.ConfigureVoltageAndCurrent();

        //            //// new, replaces non-generic code
        //            //foreach (string MipiWaveform in MipiWaveformNames)
        //            //{
        //            //    Eq.Site[Site].HSDIO.SendVector(MipiWaveform);
        //            //}
        //            //return 0;
        //        }
        //        #endregion If TestMode = CONFIG or DC


        //        #region Trigger?
        //        if (this.ParaFunc.ToUpper() == "TRIGGER")
        //        {
        //            try
        //            {
        //                if (Sync_Group.Keys.Contains(Site)) MathCalc0.Clear();
        //                if (Sync_Group.Values.Contains(Site)) MathCalc1.Clear();

        //                #region Setup and trigger measurement

        //                //Set bias
        //                this.ConfigureVoltageAndCurrent();

        //                //take care of mipi vectors
        //                foreach (string MipiWaveform in MipiWaveformNames)
        //                {
        //                    Eq.Site[Site].HSDIO.SendVector(MipiWaveform);
        //                }

        //                //Take care of switching
        //                foreach (Operation SwitchPath in ActivePath)
        //                {
        //                    Eq.Site[Site].SwMatrix.ActivatePath(this.Band, SwitchPath);
        //                }

        //                //Do a single trigger capture of the traces
        //                Eq.Site[Site].EqNetAn.SendTrigger(false, chan, 0);

        //                #endregion Setup and trigger measurement

        //                string ztitlenote = Band + "_" + PowerMode + "_" + "DACQ2_" + MipiDacBitQ2 + "_" + "DACQ1_" + MipiDacBitQ1;
        //                DataFiles.SNP.SaveToENA(chan, trace, ztitlenote, NaSetup.Chan[chan].NaPortsString, NaSetup.Chan[chan].NaPortCombos.Count);
        //                //DataFiles.SNP.SaveToENA(chan, trace, Band + "_" + PowerMode, NaSetup.Chan[chan].NaPortsString, NaSetup.Chan[chan].NaPortCombos.Count);
        //            }
        //            catch (Exception e)
        //            {
        //                MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
        //            }

        //        } //End of if (Trigger) 
        //        #endregion Trigger?

        //        switch (TestMode)
        //        {
        //            case TestModes.FBAR:
        //                Search(SecResFile);
        //                break;
        //        }

        //        BuildResults(ref sb);
        //        return 0;
        //    }

        //    catch (Exception e)
        //    {
        //        MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
        //        return 0;
        //    }
        //}

        private void ConfigureVoltageAndCurrent()
        {
            try
            {
                foreach (string pinName in SmuSettingsDictNA.Keys)
                {
                    if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue; // don't force voltage on MIPI pins

                    Eq.Site[Site].DC[pinName].ForceVoltage(SmuSettingsDictNA[pinName].Volts, SmuSettingsDictNA[pinName].Current);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            }
        }





        public static int VerifyTraceLimits(byte site, int chan, int trace, double minLimit, double maxLimit, double minLimit_HighFreq, double maxLimit_HighFreq)
        {
            SortedList<double, double> subTrace = new SortedList<double, double>();
            SortedList<double, double> subTrace_HighFreq = new SortedList<double, double>();

            Eq.Site[site].EqNetAn.MaximizeActiveWindow(true);
            Eq.Site[site].EqNetAn.SetActiveTrace(chan, trace);

            Eq.Site[site].EqNetAn.SendTrigger(false, chan, trace);



            List<double> freqData = Eq.Site[site].EqNetAn.ReadFreqList_Chan(chan);
            double[] traceData = Eq.Site[site].EqNetAn.ReadENATrace(chan, trace);

            int point = 0;
            foreach (double freq in freqData)
            {
                if (freqData[point] < 6000)
                {
                    if (!subTrace.ContainsKey(freq))
                        subTrace.Add(freq, traceData[point]); // eliminates equal key names. Sometime array has same freq point returned
                }
                else
                {
                    if (!subTrace_HighFreq.ContainsKey(freq))
                        subTrace_HighFreq.Add(freq, traceData[point]); // eliminates equal key names. Sometime array has same freq point returned
                }
                point++;
            }

            double minValue = subTrace.Values.Min();
            double maxValue = subTrace.Values.Max();
            double minValue_HighFreq = subTrace_HighFreq.Values.Min();
            double maxValue_HighFreq = subTrace_HighFreq.Values.Max();

            if (minValue < minLimit)
            {
                DialogResult res = MessageBox.Show("Ch " + chan.ToString() + "Trace " + trace.ToString() + " Data: " + minValue.ToString() + "  is beyond min limit:" + minLimit.ToString(), "Verify Limits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            if (maxValue > maxLimit)
            {
                DialogResult res = MessageBox.Show("Ch " + chan.ToString() + "Trace " + trace.ToString() + " Data: " + maxValue.ToString() + "  is beyond max limit:" + maxLimit.ToString(), "Verify Limits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            if (minValue_HighFreq < minLimit_HighFreq)
            {
                DialogResult res = MessageBox.Show("Ch " + chan.ToString() + "Trace " + trace.ToString() + " Data: " + minValue_HighFreq.ToString() + "  is beyond min limit:" + minLimit.ToString(), "Verify Limits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            if (maxValue_HighFreq > maxLimit_HighFreq)
            {
                DialogResult res = MessageBox.Show("Ch " + chan.ToString() + "Trace " + trace.ToString() + " Data: " + maxValue_HighFreq.ToString() + "  is beyond max limit:" + maxLimit.ToString(), "Verify Limits", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            return 1;
        }



        public void Search(SnPFileBuilder zFileHan)
        {
            try
            {
                if (traceData[chan, trace] == null)
                {
                    FullTrace = Eq.Site[0].EqNetAn.ReadENATrace(chan, trace);
                    traceData[chan, trace] = FullTrace;


                    //if (Eq.Site[0].EqNetAn.CheckTraceFormat(chan, trace) != "MLOG\n")
                    //{
                    //    FullTrace = Eq.Site[0].EqNetAn.ReadENATrace(chan, trace);
                    //    traceData[chan, trace] = FullTrace;
                    //    //updateflag = false;


                    //}
                    //else
                    //{
                    //    FullTrace = Eq.Site[0].EqNetAn.ReadENATrace2(chan, trace);

                    //    #region Get the full trace data and extract Mag and Ang from it

                    //    for (int k = 0; k < FullTrace.Length - 1; k += 2) //Magnitude and Angle convertion
                    //    {
                    //        zMag.Add(20 * (Math.Log10(Math.Sqrt(Math.Pow(FullTrace[k], 2) + Math.Pow(FullTrace[k + 1], 2)))));
                    //        //zMag[k] = 20 * (Math.Log10(Math.Sqrt(Math.Pow(FullTrace[k * 2], 2) + Math.Pow(FullTrace[(k * 2) + 1], 2))));

                    //        //zAng[k] = Math.Atan2(FullTrace[(k * 2) + 1], FullTrace[k * 2]);
                    //        zAng.Add(Math.Atan2(FullTrace[k + 1], FullTrace[k]));
                    //    }

                    //    #endregion

                    //    traceData[chan, trace] = zMag.ToArray();
                    //    DataFiles.Trace.UpdateContents(this);
                    //    zMag.Clear();
                    //    zAng.Clear();
                    //    //updateflag = true;
                    //}


                } //Closing bracket if for "if (traceData[chan, trace] == null)"




                ENAresult = 0; ENAresultFreq = 0;

                #region make new list with only frequencies of interest, and interpolated end points

                SortedList<double, double> subTrace = new SortedList<double, double>();

                
                
                int startFreqI = freqList[chan].BinarySearch(startFreq);
                if (startFreqI < 0)  // start frequency not found, must interpolate
                {
                    startFreqI = ~startFreqI;   // index just after target freq
                    subTrace[startFreq] = InterpolateLinear(freqList[chan][startFreqI - 1], freqList[chan][startFreqI], traceData[chan, trace][startFreqI - 1], traceData[chan, trace][startFreqI], startFreq);
                }

                int endFreqI = freqList[chan].BinarySearch(endFreq);
                if (endFreqI < 0)  // end frequency not found, must interpolate
                {
                    endFreqI = ~endFreqI - 1;   // index just before target freq
                    subTrace[endFreq] = InterpolateLinear(freqList[chan][endFreqI], freqList[chan][endFreqI + 1], traceData[chan, trace][endFreqI], traceData[chan, trace][endFreqI + 1], endFreq);
                }

                for (int i = startFreqI; i <= endFreqI; i++) subTrace[freqList[chan][i]] = traceData[chan, trace][i];

                #endregion make new list with only frequencies of interest, and interpolated end points

                #region SearchType switch
                switch (SearchType)
                {
                    case SearchTypes.MIN:

                        #region Search Min
                        try
                        {
                            
                            ENAresult = subTrace.Values.Min();
                            ENAresultFreq = subTrace.Keys[subTrace.IndexOfValue(ENAresult)];
                            ENAresult = subTrace.Values.Min();

                            break;

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Error with MIN search type" + "\r\n" + e.ToString());
                            break;
                        }
                        #endregion

                    case SearchTypes.MAX:

                        #region Search Max
                        try
                        {
                            ENAresult = subTrace.Values.Max();
                            ENAresultFreq = subTrace.Keys[subTrace.IndexOfValue(ENAresult)];

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                            MessageBox.Show("Error with MAX search type" + "\r\n" + e.ToString());

                        }
                        break;
                        #endregion

                    case SearchTypes.AVG:

                        #region Search Average
                        try
                        {
                            //double[] valuesWatts = new double[] {3,10,25}; //Test Case
                            //for (int x = 0; x < 3; x++)
                            //{
                            //    valuesWatts[x] = dBmToWatts(valuesWatts[x]);
                            //}
                            //ENAresult = (10 * Math.Log10(valuesWatts.Average()*1000));
                            //Console.WriteLine(ENAresult.ToString());

                            double[] valuesWatts = new double[subTrace.Values.Count];
                            for (int x = 0; x < subTrace.Values.Count; x++)
                            {
                                valuesWatts[x] = dBmToWatts(subTrace.Values[x]);
                            }
                            ENAresult = (10 * Math.Log10(valuesWatts.Average() * 1000));
                            //ENAresult = subTrace.Values.Average();                                                        
                            ENAresultFreq = subTrace.Keys[subTrace.IndexOfValue(subTrace.Values.Max())];
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                            MessageBox.Show("Error with AVG search type" + "\r\n" + e.ToString());
                        }
                        break;
                        #endregion

                    case SearchTypes.LEFT:

                        #region Search Left
                        try
                        {

                            for (int i = subTrace.Count() - 2; i >= 0; i--)
                            {
                                if (subTrace.ElementAt(i).Value < SearchTarget)
                                {

                                    ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchTarget);
                                    ENAresult = SearchTarget;
                                    break;

                                }
                            }


                        }
                        catch (Exception e)
                        {
                            try
                            {
                                for (int i = 1; i < subTrace.Count(); i++)
                                {
                                    if (subTrace.ElementAt(i).Value > SearchTarget)
                                    {
                                        ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchTarget);
                                        ENAresult = SearchTarget;
                                        break;
                                    }
                                }

                            }
                            catch
                            {

                                ENAresultFreq = 0;
                                ENAresult = 0;


                                //MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                                //MessageBox.Show("Error with LEFT search type" + "\r\n" + "subtrace.Count() = " + subTrace.Count().ToString() + "\r\n" + e.ToString());

                            }




                        }

                        break;
                        #endregion

                    case SearchTypes.RIGHT:

                        #region Search Right
                        try
                        {

                            for (int i = 0; i <= subTrace.Count() - 2; i++)
                            {
                                if (subTrace.ElementAt(i).Value < SearchTarget)
                                {
                                    ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchTarget);
                                    ENAresult = SearchTarget;
                                    break;

                                }

                            }


                        }
                        catch (Exception e)
                        {
                            try
                            {
                                for (int i = subTrace.Count() - 2; i >= 0; i--)
                                {
                                    if (subTrace.ElementAt(i).Value > SearchTarget)
                                    {
                                        ENAresultFreq = InterpolateLinear(subTrace.Values[i], subTrace.Values[i + 1], subTrace.Keys[i], subTrace.Keys[i + 1], SearchTarget);
                                        ENAresult = SearchTarget;
                                        break;
                                    }
                                }

                            }
                            catch
                            {
                                ENAresultFreq = 0;
                                ENAresult = 0;

                            }

                        }

                        break;
                        #endregion

                    case SearchTypes.RIPPLE:

                        #region Search Ripple
                        List<double> fstartlist = new List<double>();
                        List<double> ripplelist = new List<double>();
                        double BW = Convert.ToDouble(RippleBW);
                        double fstart = startFreq;
                        //double fstop = fstart + 5;
                        double fstop = fstart + BW;


                        try
                        {
                            for (int ripx = 0; fstart <= endFreq - BW; ripx++)
                            {

                                double minval, maxval;
                                List<double> templist = new List<double>();

                                startFreqI = freqList[chan].BinarySearch(fstart);
                                if (startFreqI < 0)  // start frequency not found, must interpolate
                                {
                                    startFreqI = ~startFreqI;   // index just after target freq
                                    subTrace[startFreq] = InterpolateLinear(freqList[chan][startFreqI - 1], freqList[chan][startFreqI], traceData[chan, trace][startFreqI - 1], traceData[chan, trace][startFreqI], fstart);
                                }

                                endFreqI = freqList[chan].BinarySearch(fstop);
                                if (endFreqI < 0)  // end frequency not found, must interpolate
                                {
                                    endFreqI = ~endFreqI - 1;   // index just before target freq
                                    subTrace[fstop] = InterpolateLinear(freqList[chan][endFreqI], freqList[chan][endFreqI + 1], traceData[chan, trace][endFreqI], traceData[chan, trace][endFreqI + 1], fstop);
                                }

                                for (int i = startFreqI; i <= endFreqI; i++) templist.Add(traceData[chan, trace][i]);  //This gets the trace data of interest

                                //Interpolate for the value based on interpolated frequency
                                double fstartval = InterpolateLinear(freqList[chan][startFreqI - 1], freqList[chan][endFreqI - 1], traceData[chan, trace][startFreqI - 1], traceData[chan, trace][endFreqI - 1], fstart);
                                double fstopval = InterpolateLinear(freqList[chan][startFreqI - 1], freqList[chan][endFreqI - 1], traceData[chan, trace][startFreqI - 1], traceData[chan, trace][endFreqI - 1], fstop);

                                templist.Add(fstartval);
                                templist.Add(fstopval);

                                minval = templist.Min();
                                maxval = templist.Max();

                                ripplelist.Add(maxval - minval);
                                fstartlist.Add(fstart + (BW / 2));


                                fstart = fstart + 0.5; //500 kHz increment step size for frequency
                                fstop = fstart + BW;
                            }

                            ENAresult = ripplelist.Max();
                            ENAresultFreq = fstartlist[ripplelist.IndexOf(ENAresult)];

                            //MessageBox.Show("Passed Search Type Ripple, Line 323()");

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                            MessageBox.Show("Error with Ripple search type" + "\r\n" + e.ToString());

                        }

                        break;
                        #endregion

                }
                #endregion SearchType switch

            }
            catch (Exception e)
            {

                MessageBox.Show("During " + TestParaName + "\r\n" + "Error while searching for " + SearchType + " value in ENA trace for " + SearchTarget.ToString() + "\r\n" + e.ToString(), "NetANTest.Search", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        public double dBmToWatts(double dBmvalue)
        {
            double retval = -999;

            try
            {
                //retval = ((Math.Pow((dBmvalue / 10), 10))/1000);
                retval = ((Math.Pow(10, (dBmvalue / 10))) / 1000);

            }
            catch (Exception e)
            {
                MessageBox.Show("Problem during dBmToWatts conversion: " + e.ToString());

            }

            return retval;

        }

        public void BuildResults(ref ATFReturnResult sb)
        {
            try
            {
                switch (TestMode)
                {
                    case TestModes.FBAR:
                        if (TestParaName.Contains("GD")) //Group delay measurements need to be normalized to 1 ns
                        {
                            ResultBuilder.AddResult(ActiveSite,  TestParaName, "", Math.Round((ENAresult / 1E-9), 4));
                            ResultBuilder.AddResult(ActiveSite,  TestParaName + "_FREQ", "", ENAresultFreq); break;
                        }
                        if (!(String.Equals(this.MathVariable, null) || String.Equals(this.MathVariable, "")))
                        {
                                //if (Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CheckTraceFormat(chan, trace) == "MLOG\n")
                                if (true)
                                {
                                try
                                {
                                    if (MathCalcLegacy.ContainsKey(this.MathVariable))   //kh
                                        MathCalcLegacy[this.MathVariable] = ENAresult;
                                    else
                                        MathCalcLegacy.Add(this.MathVariable, ENAresult);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Duplicated condition detect in adding variable: " + this.MathVariable);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Trace format not supported. Please check the unit of variable " + this.MathVariable + " for Test paramter name" + this.TestParaName);
                            }
                        }
                        if (TestParaName.ToUpper().Contains("VSWR") & TestParaName.ToUpper().Contains("RL"))
                        {
                                //if ((Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CheckTraceFormat(chan, trace) == "MLOG\n"))
                                if (true)
                                {
                                double VSWR = 0;
                                if (ENAresult >= 0)
                                {
                                    ATFLogControl.Instance.Log(LogLevel.Warn, "Error in converting RL to VSWR. The RL in dB is an non-negative number as:" + ENAresult.ToString() + " Please check the test parameter,port impedance matching or ENA cal of the system.");
                                    VSWR = 999;
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName, "", Math.Round(VSWR, 4));
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName.Replace("VSWR", "dB"), "", Math.Round(ENAresult, 4));
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName.Replace("_VSWR", "") + "_FREQ", "", ENAresultFreq);
                                }
                                else
                                {
                                    VSWR = (1 + Math.Pow(10, ENAresult / 20)) / (1 - Math.Pow(10, ENAresult / 20));
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName, "", Math.Round(VSWR, 4));
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName.Replace("VSWR", "dB"), "", Math.Round(ENAresult, 4));
                                    ResultBuilder.AddResult(ActiveSite,  TestParaName.Replace("_VSWR", "") + "_FREQ", "", ENAresultFreq);
                                }
                            }
                            break;
                        }
                        else
                        {
                            if (ENAresult < -100 || ENAresult > 100)
                            {
                                ENAresult = 50;
                            }
                            ResultBuilder.AddResult(ActiveSite,  TestParaName, "", Math.Round(ENAresult, 4));
                            ResultBuilder.AddResult(ActiveSite,  TestParaName + "_FREQ", "", ENAresultFreq); break;
                        }
                    case TestModes.COMMON:
                        string formula = this.MathVariable;
                        string formula_org = this.MathVariable;
                        if (!(string.Equals(formula, "") || string.Equals(formula, null)))
                        {
                            try
                            {
                                foreach (KeyValuePair<string, double> kp in MathCalcLegacy)
                                {
                                    string KPvalue = kp.Value.ToString();
                                    if (kp.Value < 0) KPvalue = "(0" + kp.Value.ToString() + ")";
                                    //if (formula.Contains(kp.Key)) formula = formula.Replace(kp.Key, KPvalue);
                                   if (Regex.IsMatch(formula, "\\b" + kp.Key + "\\b", RegexOptions.IgnoreCase))  // change in legacyFbar if this solves problem of partiale varible names being found
                                       formula = formula.Replace(kp.Key, KPvalue);
                                }
                                StringToFormula stf = new StringToFormula();
                                double result = stf.Eval(formula, formula_org);
                                ResultBuilder.AddResult(ActiveSite,  TestParaName, "", Math.Round(result, 4));
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
                        break;
                    default: break;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Exception happened during BuildResults in NetAn.cs" + "\r\n" + e.ToString());
            }
        }

        private double InterpolateLinear(double lowerX, double upperX, double lowerY, double upperY, double xTarget)
        {
            try
            {
                return (((upperY - lowerY) * (xTarget - lowerX)) / (upperX - lowerX)) + lowerY;
            }
            catch (Exception e)
            {
                return -99999;
            }
        }

        public static class DataFiles
        {
            public static int LogEveryNthDut;  //Snp file will be saved every LogEveryNthDut
            public static int MaxNumDutsToLog;
            public static int dutSN = 0;
            public static string currResultFile;
            public static void PreTest()
            {
                string SN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");

                if (SN != "")
                    dutSN = Convert.ToInt32(SN);
                else
                    dutSN++;   // Lite Driver

                currResultFile = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, "");
                if (currResultFile.Trim() == "") currResultFile = "LiteDriver";   // for LiteDriver
                
                // Clear math varible
                Legacy_FbarTest.MathCalcLegacy.Clear();
            }

            public static void WriteAll()
            {
                Legacy_FbarTest.DataFiles.SNP.AcquireAll();
                //Legacy_FbarTest.DataFiles.Trace.Write();
            }

            public static class SNP
            {
                public static bool Enable;
                private static string snpFolder;
                private static List<string> snpFiles = new List<string>();
                public static ManualResetEvent snpFlag = new ManualResetEvent(true);

                public static void SaveToENA(int chan, int trace, string titleNote, string NAPorts, int numports)
                {
                    if (!Enable) return;

                    try
                    {
                        
                        string snpFileName = currResultFile + "_S4P_PID-" + dutSN.ToString() + "_Channel_" + chan.ToString() + "_" + titleNote + ".s4p";

                        if (dutSN % LogEveryNthDut != 0 || dutSN / LogEveryNthDut > MaxNumDutsToLog) return;

                        if (snpFiles.Contains(snpFileName))
                        {
                            MessageBox.Show("Error, SNP File is being created twice:\n\n" + snpFileName + "\n\nPlease check TCF to ensure you are not triggering excessively,\nand each test has unique naming.", "SNP File Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetTimeout(10000);

                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":DISP:WIND" + chan.ToString() + ":ACT");
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":CALC" + chan.ToString() + ":PAR" + trace.ToString() + ":SEL");
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":MMEM:STOR:SNP:FORM DB");
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":MMEM:STOR:SNP:TYPE:S4P 1,2,3,4");
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":MMEM:STOR:SNP \"D:\\" + snpFileName + "\"");

                        snpFiles.Add(snpFileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error happened during SNP Acquire in NetAn.cs" + "\r\n" + e.ToString());
                    }
                }

                public static void AcquireAll()
                {
                    if (!Enable) return;
                    if (!String.Equals(currResultFile, "LiteDriver"))
                    {
                        #region elimate the date and Ip information from folder name
                        char[] delimiterChars = { '_' };
                        string[] words = currResultFile.Split(delimiterChars);
                        int Index = words.Count();
                        int Indexofdot = words[Index - 1].IndexOf(".");
                        string FileNameOnly = words[Index - 1].Remove(Indexofdot);
                        string shortcurrResultFile = String.Join("_", words[0], FileNameOnly);
                        #endregion
                        snpFolder = @"C:\Avago.ATF.Common\SNP_Results\" + shortcurrResultFile + "_SNP\\";
                    }
                    else
                    {
                        snpFolder = @"C:\Avago.ATF.Common\SNP_Results\" + currResultFile + "_SNP\\";
                    }
                    if (!Directory.Exists(snpFolder)) Directory.CreateDirectory(snpFolder);

                    snpFlag.WaitOne();
                    snpFlag.Reset();
                    ThreadPool.QueueUserWorkItem(AcquireAll, null);
                }

                private static void Acquire(string snpFileName)
                {
                    try
                    {

                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write("*CLS");
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetTimeout(10000);
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":MMEM:TRAN? \"D:\\" + snpFileName + "\"");

                        byte[] tempData = (byte[])Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Read_BiBlock();

                        using (BinaryWriter w = new BinaryWriter(new FileStream(snpFolder + snpFileName, FileMode.Create)))
                        {
                            w.Write(tempData);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("SNP File transfer error\n\n" + ex.ToString(), "Error");
                    }

                    Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":MMEM:DEL \"D:\\" + snpFileName + "\""); //deletes the file from the NA
                }

                public static void AcquireAll(object o)
                {
                    foreach (string snpFile in snpFiles)
                    {
                        Acquire(snpFile);
                    }
                    snpFiles.Clear();
                    snpFlag.Set();
                }
            }

            public static class Trace
            {
                public static bool Enable;
                public static List<string> SPAR_HEAD = new List<string>();
                public static List<string> SPAR_BODY = new List<string>();

                public static bool headerWritten = false;

                public static void UpdateContents(Legacy_FbarTest naTest)
                {
                    try
                    {
                        //SecResHolder.flag.WaitOne(5000);
                        if (!Enable) return;
                        dutSN = Convert.ToInt32(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "10"));
                        if (dutSN % LogEveryNthDut != 0 || dutSN / LogEveryNthDut > MaxNumDutsToLog) return;

                        SecResHolder.SnPParHeader(naTest.Trace_Name, freqList[naTest.chan].ToArray(), Convert.ToSingle(Port_Impedance), naTest.Band, naTest.PowerMode);
                        SecResHolder.SnPParBody(FullTrace);


                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error happened during updatetracefile in NetAn.cs" + "\r\n" + e.ToString());
                    }

                }

                public static void Write()
                {
                    if (!Enable) return;

                    if (true)   //NATestParams.Count > 0
                    {
                        if (dutSN % LogEveryNthDut != 0 || dutSN / LogEveryNthDut > MaxNumDutsToLog) return;

                        //This is where the SPAR should be written because testing for one part is finished
                        //Have a flag here that determines is the header needs to be written or not

                        using (StreamWriter sr = new StreamWriter(@"C:\Avago.ATF.Common\Results\" + currResultFile + "_SPAR.csv", true))
                        {

                            #region Write the _SPAR header

                            if (!headerWritten)
                            {
                                sr.Write("Parameter,");

                                foreach (string headerelement in SPAR_HEAD)
                                {
                                    sr.Write(headerelement + ",");
                                }

                                sr.WriteLine();

                                headerWritten = true;
                            }

                            #endregion Write the _SPAR Header

                            #region Write the _SPAR Body
                            sr.Write("PID-" + dutSN + ",");

                            foreach (string element in SPAR_BODY)
                            {
                                sr.Write(element + ",");
                            }

                            sr.WriteLine();

                            #endregion Write the _SPAR Body

                        } //using closing brace

                        SPAR_HEAD.Clear();
                        SPAR_BODY.Clear();
                    }

                }
            }
        }

    }

    public class NaSetup
    {
        public static Dictionary<int, NaSetup> Chan = new Dictionary<int, NaSetup>();

        public List<EqSwitchMatrix.PortCombo> NaPortCombos = new List<EqSwitchMatrix.PortCombo>();
        public static Dictionary<EqSwitchMatrix.InstrPort, int> SwitchMatrixConnections = new Dictionary<EqSwitchMatrix.InstrPort, int>();
        public List<int> NaPorts = new List<int>();
        public string Band = "";
        public string NaPortsString = "";
        public string DutPortsString = "";
        public static Dictionary<string, CalStd> CalStds = new Dictionary<string, CalStd>();
        public static Dictionary<string, int> NaChansPerBand = new Dictionary<string, int>();

        public static void DefineSwitchMatrixConnection(EqSwitchMatrix.InstrPort instrPort, int naPort)
        {
            SwitchMatrixConnections[instrPort] = naPort;
        }

        public static void DefineChannel(int NaChanNum, EqSwitchMatrix.PortCombo PortCombo1 = null, EqSwitchMatrix.PortCombo PortCombo2 = null, EqSwitchMatrix.PortCombo PortCombo3 = null, EqSwitchMatrix.PortCombo PortCombo4 = null)
        {
            Chan[NaChanNum] = new NaSetup(); ;

            if (PortCombo1 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo1);
            if (PortCombo2 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo2);
            if (PortCombo3 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo3);
            if (PortCombo4 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo4);

            Chan[NaChanNum].NaPorts = Chan[NaChanNum].NaPortCombos.Select(x => SwitchMatrixConnections[x.instrPort]).OrderBy(x => x).ToList();
            Chan[NaChanNum].NaPortsString = string.Join(",", Chan[NaChanNum].NaPorts);
            Chan[NaChanNum].DutPortsString = string.Join(", ", Chan[NaChanNum].NaPortCombos.Select(x => x.dutPort).OrderBy(x => x));
        }

        public static void DefineChannel(int NaChanNum, string band, Operation op1 = Operation.MeasureH3_PS, Operation op2 = Operation.MeasureH3_PS, Operation op3 = Operation.MeasureH3_PS, Operation op4 = Operation.MeasureH3_PS)
        {
            EqSwitchMatrix.PortCombo PortCombo1 = op1 != Operation.MeasureH3_PS ? Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op1) : null;
            EqSwitchMatrix.PortCombo PortCombo2 = op2 != Operation.MeasureH3_PS ? Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op2) : null;
            EqSwitchMatrix.PortCombo PortCombo3 = op3 != Operation.MeasureH3_PS ? Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op3) : null;
            EqSwitchMatrix.PortCombo PortCombo4 = op4 != Operation.MeasureH3_PS ? Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op4) : null;

            DefineChannel(NaChanNum, PortCombo1, PortCombo2, PortCombo3, PortCombo4);
            Chan[NaChanNum].Band = band;
            NaChansPerBand[band] = NaChanNum;
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType)
        {
            foreach (CalStd calStd in CalStds.Values)
            {
                if (calStd.StdType == calStdType)
                {
                    return calStd.xyTrayCoord;
                }
            }

            throw new Exception("Cal Standard not defined for " + calStdType);
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType, string band, Operation op1, Operation op2)
        {
            return GetTrayCoordinates(calStdType, Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op1), Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op2));
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType, EqSwitchMatrix.PortCombo PortComboA, EqSwitchMatrix.PortCombo PortComboB)
        {
            CalStd.ThruCombo tempThru = new CalStd.ThruCombo(PortComboA, PortComboB);

            foreach (CalStd calStd in CalStds.Values)
            {
                if (calStd.StdType == calStdType && calStd.Thrus != null)
                {
                    foreach (CalStd.ThruCombo thru in calStd.Thrus)
                    {
                        if (thru == tempThru)
                        {
                            return calStd.xyTrayCoord;
                        }
                    }
                }
            }

            throw new Exception("Cal Standard not defined for " + calStdType);
        }

        public class CalStd
        {
            public string ID;
            public string xyTrayCoord;
            public Type StdType;

            public List<ThruCombo> Thrus;

            public enum Type
            {
                Short, Open, Load, Thru
            }

            public class ThruCombo
            {
                public EqSwitchMatrix.PortCombo PortComboA, PortComboB;

                public ThruCombo(EqSwitchMatrix.PortCombo PortComboA, EqSwitchMatrix.PortCombo PortComboB)
                {
                    if (SwitchMatrixConnections[PortComboA.instrPort] < SwitchMatrixConnections[PortComboB.instrPort])
                    {
                        this.PortComboA = PortComboA;
                        this.PortComboB = PortComboB;
                    }
                    else
                    {
                        this.PortComboA = PortComboB;
                        this.PortComboB = PortComboA;
                    }
                }

                public override bool Equals(Object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    ThruCombo p = (ThruCombo)obj;
                    return (PortComboA == p.PortComboA) && (PortComboB == p.PortComboB);
                }
                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = hash * 29 + PortComboA.GetHashCode();
                        hash = hash * 29 + PortComboB.GetHashCode();
                        return hash;
                    }
                }
                public static bool operator ==(ThruCombo x, ThruCombo y)
                {
                    return Object.Equals(x, y);
                }
                public static bool operator !=(ThruCombo x, ThruCombo y)
                {
                    return !(x == y);
                }
            }

            public CalStd(string ID, string xyTrayCoord, Type StdType)
            {
                this.ID = ID;
                this.xyTrayCoord = xyTrayCoord;
                this.StdType = StdType;

                Thrus = null;
            }

            public static void Define(string CalStdID, string xyTrayCoord, Type calStandard)
            {
                CalStds.Add(CalStdID, new CalStd(CalStdID, xyTrayCoord, calStandard));
            }

            public static void AddThru(string CalStdID, string band, Operation op1, Operation op2)
            {
                if (CalStds[CalStdID].Thrus == null) CalStds[CalStdID].Thrus = new List<ThruCombo>();

                CalStds[CalStdID].Thrus.Add(new CalStd.ThruCombo(
                        Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op1),
                        Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.GetPath(band, op2)));


            }

        }
    }

    public static class NaCal
    {
        public static EqLib.EqHandler.iEqHandler zHandler;

        public static void PerformEcal(int NaChan)
        {
            try
            {
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.enaStateRecallFlag.WaitOne(20000);
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":DISP:ENAB 1");   // 0 off, 1 on
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":TRIG:SOUR BUS");    // db - is this necessary?

                foreach (EqSwitchMatrix.PortCombo portCombo in NaSetup.Chan[NaChan].NaPortCombos)
                {
                    Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.ActivatePath(portCombo);
                }

                MessageBox.Show("Connect cables " + NaSetup.Chan[NaChan].DutPortsString + " to ECAL Module.  Click OK when finished.", "Channel " + NaChan + " ENA Cal");

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetTimeout(30000);

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetActiveWindow(NaChan);
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.PerformEcal(NaChan, NaSetup.Chan[NaChan].NaPortCombos.Count, NaSetup.Chan[NaChan].NaPortsString);

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.funcSerialPoll();
            }
            catch (Exception e)
            {

            }
        }

        public static void PerformTrayMapSOLTcal(int calKitNum, List<int> NaChans = null)
        {
            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":DISP:ENAB 1");

            string trayCoord = "";

            if (NaChans == null) NaChans = NaSetup.Chan.Keys.ToList();

            foreach (int NaChan in NaChans)
            {
                foreach (EqSwitchMatrix.PortCombo portCombo in NaSetup.Chan[NaChan].NaPortCombos) Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.ActivatePath(portCombo);

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetActiveWindow(NaChan);
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetCalibrationType(NaChan, NaSetup.Chan[NaChan].NaPortCombos.Count, NaSetup.Chan[NaChan].NaPortsString);
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SelectCalKit(NaChan, calKitNum);
                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.Write(":SENS" + NaChan + ":CORR:TRIG:FREE ON");  //Set to internal trigger
                zHandler.CheckSRQStatusByte(72);

                trayCoord = NaSetup.GetTrayCoordinates(NaSetup.CalStd.Type.Open);
                zHandler.TrayMapCoord(trayCoord);
                foreach (int naPort in NaSetup.Chan[NaChan].NaPorts) Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalibrateOpen(NaChan, naPort);
                zHandler.TrayMapEOT("1");
                zHandler.CheckSRQStatusByte(72);

                trayCoord = NaSetup.GetTrayCoordinates(NaSetup.CalStd.Type.Short);
                zHandler.TrayMapCoord(trayCoord);
                foreach (int naPort in NaSetup.Chan[NaChan].NaPorts) Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalibrateShort(NaChan, naPort);
                zHandler.TrayMapEOT("1");
                zHandler.CheckSRQStatusByte(72);

                trayCoord = NaSetup.GetTrayCoordinates(NaSetup.CalStd.Type.Load);
                zHandler.TrayMapCoord(trayCoord);
                foreach (int naPort in NaSetup.Chan[NaChan].NaPorts) Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalibrateLoad(NaChan, naPort);
                zHandler.TrayMapEOT("1");


                for (int i1 = 0; i1 < NaSetup.Chan[NaChan].NaPortCombos.Count - 1; i1++)
                {
                    for (int i2 = i1 + 1; i2 < NaSetup.Chan[NaChan].NaPortCombos.Count; i2++)
                    {
                        EqSwitchMatrix.PortCombo portComboA = NaSetup.Chan[NaChan].NaPortCombos[i1];
                        EqSwitchMatrix.PortCombo portComboB = NaSetup.Chan[NaChan].NaPortCombos[i2];

                        int naPortA = NaSetup.SwitchMatrixConnections[portComboA.instrPort];
                        int naPortB = NaSetup.SwitchMatrixConnections[portComboB.instrPort];

                        trayCoord = NaSetup.GetTrayCoordinates(NaSetup.CalStd.Type.Thru, portComboA, portComboB);
                        zHandler.CheckSRQStatusByte(72);
                        zHandler.TrayMapCoord(trayCoord);
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalibrateThruWithSubclass(NaChan, naPortA, naPortB);
                        Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalibrateThruWithSubclass(NaChan, naPortB, naPortA);
                        zHandler.TrayMapEOT("1");
                    }
                }

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.CalcCalCoeffs(NaChan); //Save the cal for the channel 
            }

        }

        public static void ManualVerifyTrayMapThru(List<int> NaChans = null)   // if no argument, performs on all chans
        {
            if (NaChans == null) NaChans = NaSetup.Chan.Keys.ToList();

            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetTrigInternal();
            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.MaximizeActiveWindow(true);

            foreach (NaSetup.CalStd calStd in NaSetup.CalStds.Values)
            {
                if (calStd.StdType != NaSetup.CalStd.Type.Thru) continue;

                NaCal.zHandler.CheckSRQStatusByte(72);
                NaCal.zHandler.TrayMapCoord(calStd.xyTrayCoord);

                foreach (NaSetup.CalStd.ThruCombo tc in calStd.Thrus)
                {
                    #region determine which channel the thru belongs to

                    int NaChan = -1;

                    foreach (int chan in NaSetup.Chan.Keys)
                    {
                        if (NaSetup.Chan[chan].NaPortCombos.Contains(tc.PortComboA) && NaSetup.Chan[chan].NaPortCombos.Contains(tc.PortComboB))
                        {
                            NaChan = chan;
                            break;
                        }
                    }

                    if (!NaChans.Contains(NaChan)) continue;

                    #endregion

                    foreach (EqSwitchMatrix.PortCombo portCombo in NaSetup.Chan[NaChan].NaPortCombos) Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.ActivatePath(portCombo);

                    Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetActiveWindow(NaChan);

                    int naPortA = NaSetup.SwitchMatrixConnections[tc.PortComboA.instrPort];
                    int naPortB = NaSetup.SwitchMatrixConnections[tc.PortComboB.instrPort];

                    string traceName = "S" + naPortA.ToString() + naPortB.ToString();

                    MessageBox.Show("Verify " + traceName + " for NA chan " + NaChan + ".  Click OK when finished.", "NA Cal Manual Verification");
                }

                NaCal.zHandler.TrayMapEOT("1");
            }

            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.MaximizeActiveWindow(false);
        }

        public static void ManualVerifyTrayMapLoad(List<int> NaChans = null)  // if no argument, performs on all chans
        {
            if (NaChans == null) NaChans = NaSetup.Chan.Keys.ToList();

            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.MaximizeActiveWindow(true);

            string trayCoord = NaSetup.GetTrayCoordinates(NaSetup.CalStd.Type.Load);
            zHandler.CheckSRQStatusByte(72);
            zHandler.TrayMapCoord(trayCoord);

            foreach (int NaChan in NaChans)
            {
                foreach (EqSwitchMatrix.PortCombo portCombo in NaSetup.Chan[NaChan].NaPortCombos) Eq.Site[Legacy_FbarTest.ActiveSite].SwMatrix.ActivatePath(portCombo);

                Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.SetActiveWindow(NaChan);

                string traceName = "";
                foreach (int naPort in NaSetup.Chan[NaChan].NaPorts) traceName += "S" + naPort + naPort + ", ";

                MessageBox.Show("Verify " + traceName + "for NA chan " + NaChan + ".  Click OK when finished.", "NA Cal Manual Verification");
            }

            Eq.Site[Legacy_FbarTest.ActiveSite].EqNetAn.MaximizeActiveWindow(true);

            zHandler.TrayMapEOT("1");
        }

    }

    public class StringToFormula
    {
        private string[] _operators = { "-", "+", "/", "*", "^" };
        private Func<double, double, double>[] _operations = 
        {
		    (a1, a2) => a1 - a2,
		    (a1, a2) => a1 + a2,
		    (a1, a2) => a1 / a2,
		    (a1, a2) => a1 * a2,
		    (a1, a2) => Math.Pow(a1, a2)
	    };

        public double Eval(string expression, string formula)
        {
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Eval(subExpr, formula));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_operators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    try
                    {
                        operandStack.Push(double.Parse(token));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Missing operand information of" + token + " in calculating" + formula);
                    }
                }
                tokenIndex += 1;
            }
            if (operatorStack.Count == 0)
            {

            }
            while (operatorStack.Count > 0)
            {
                try
                {
                    string op = operatorStack.Pop();
                    double arg2 = operandStack.Pop();
                    double arg1 = operandStack.Pop();
                    operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error in calculating expression" + formula);
                }
            }
            return operandStack.Pop();
        }

        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in expression.Replace(" ", string.Empty))
            {
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
    }
} //NALib Namespace closing brace
