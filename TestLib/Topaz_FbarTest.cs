using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using Agilent.AgNA.Interop;
using System.Diagnostics;

using System.Windows.Forms;

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
    public class Topaz_FbarTest : iTest
    {
        public static Dictionary<byte, byte> Sync_Group = new Dictionary<byte, byte>();
        public List<Operation> ActivePath = new List<Operation>();
        public string TestMode;
        public string TestParaName;
        public string Band;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public string ParaFunc;
        public string formula;
        public byte Site;
        public int chan;
        public int RowNum;
        public string Sparam;
        public string Trace_Name;
        public int trace;
        public double startFreq;
        public double endFreq;
        public string PowerMode;
        public string SearchType;
        public string RippleBW;
        public double SearchTarget;
        public bool Trigger;
        public string TestNote;
        public int tracenum = 0;
        public List<double> holdit = new List<double>();
        public List<double> zMag = new List<double>();
        public List<double> zAng = new List<double>();
        public double[] FullTrace;
        public static double[] FullTrace_ang;
        public static string Port_Impedance;
        public static double[] Traces_Chan;
        public static bool updateflag;
        public Dictionary<byte, SortedList<double, double>> subTrace = new Dictionary<byte, SortedList<double, double>>();
        public Dictionary<string, DcSetting> SmuSettingsDictNA = new Dictionary<string, DcSetting>();
        public double ENAresult;
        public double ENAresultFreq;
        public double[] dataSize3;
        public static bool SynchronizaionFlag = false;
        readonly static object locker = new object();
        public static Dictionary.DoubleKey<int, int, double[]>[] traceData=new Dictionary.DoubleKey<int,int,double[]>[4];
        //public static Dictionary.TripleKey<byte, int, int, double[]> traceData = new Dictionary.TripleKey<byte, int, int, double[]>();
        //public static Dictionary.DoubleKey<int, int, double[]> traceData = new Dictionary.DoubleKey<int, int, double[]>();
        public static Dictionary<int, List<double>> freqList = new Dictionary<int, List<double>>();
        public static Dictionary<string, double> MathCalc0 = new Dictionary<string, double>();
        public static Dictionary<string, double> MathCalc1 = new Dictionary<string, double>();
        public static class TestModes
        {
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

        public bool Initialize(bool finalScript)
        {
            return true;
        }

        public int RunTest()
        {

            try
            {
                PresetSynSite();
                if (TestMode == "CONFIG")
                {
                    return 0;  //Do nothing
                }

                if (Site == ActiveSite)
                {
                    if (Sync_Group[Site] == Site) SynchronizaionFlag = false;
                    else SynchronizaionFlag = true;
                }
                DataFiles.SNP.snpFlag.WaitOne(); // finish all snp acquisitions before test
                if(TestMode=="DC")
                {
                    this.ConfigureVoltageAndCurrent();

                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands);
                    return 0;
                }
                if (this.ParaFunc.ToUpper() == "TRIGGER")
                {
                    try
                    {
                        if (Sync_Group.Keys.Contains(Site)) MathCalc0.Clear();
                        if (Sync_Group.Values.Contains(Site)) MathCalc1.Clear();
                        #region Setup and trigger measurement
                        foreach (Operation SwitchPath in ActivePath)
                        {
                            Eq.Site[Site].SwMatrix.ActivatePath(this.Band, SwitchPath);
                        }
                        // Need to sync here.  Need to wait all threads finsihed then only continue to next section.
                        //Console.WriteLine("[{0}.1] Wait sync", Site);                 
                       
                        if (SynchronizaionFlag)
                        {
                            lock (objWaitSync)
                            {
                                //Console.WriteLine("[{0}.2] Entered objWaitSync", Site);
                                if (!WaitSync(this.RowNum)) // Check whether need to wait other thread or can continue.
                                {
                                    //Console.WriteLine("[{0}.3] Wait other thread", Site);
                                    Monitor.Wait(objWaitSync);  // Wait other thread to complete.
                                }
                            }
                        }
                            
                        //Monitor.Exit(objWaitSync);
                        // Eq_ENA.Active_ENA.NA_IVI.Channels.get_Item(chan.ToString()).TriggerSweep(10000);
                        //Console.WriteLine("[{0}.3a] Site ", Site);
                        bool SlaveFlag = false;//Check if only Slave site is enabled.
                        if (!ResultBuilder.ValidSites.Contains(0) && !ResultBuilder.ValidSites.Contains(1))
                        {
                            SlaveFlag = true;
                            MessageBox.Show("Only slave site 2 & site 3 are enabled. Cannot run Fbar test without enable master site 0 & site 1");
                        }
                        else if (!ResultBuilder.ValidSites.Contains(0) && ResultBuilder.ValidSites.Contains(2))
                        {
                            SlaveFlag = true;
                            MessageBox.Show("Cannot run site2 Fbar test without enable master site 0 ");
                        }
                        if (!ResultBuilder.ValidSites.Contains(1) && ResultBuilder.ValidSites.Contains(3))
                        {
                            SlaveFlag = true;
                            MessageBox.Show("Cannot run site3 Fbar test without enable master site1 ");
                        }
                        if (!SlaveFlag)
                        {
                            #region save all traces data in the channel
                            //Stopwatch myWatch_chan = new Stopwatch();
                            //myWatch_chan.Start();

                           // Console.WriteLine("[{0}.3b] Site ", site);

                            if (Sync_Group.Keys.Contains(Site))//if it is Active site
                            {
                                Dictionary.DoubleKey<int, int, double[]> dataSize1 = new Dictionary.DoubleKey<int, int, double[]>();
                                if (traceData[Site] != null) traceData[Site].Clear();
                                else traceData[Site] = new Dictionary.DoubleKey<int, int, double[]>();
                                //traceData[Site].Add(chan, dataSize2);
                                traceData[Site].Add(chan, new Dictionary<int, double[]>());
                                byte Slavesite = Sync_Group[Site];
                                if (SynchronizaionFlag)
                                {
                                    int TraceOff = Eq_ENA.Chan_TraceNum[chan];                                   
                                    dataSize1.Add(chan + 1, new Dictionary<int, double[]>());// To avoid exception.
                                    traceData[Slavesite]=dataSize1;
                                }
                                if (!freqList.ContainsKey(chan))
                                {
                                    freqList[chan] = Eq.Site[Site].EqENA.ReadFreqList_Chan(chan);
                                    if (SynchronizaionFlag) freqList[chan + 1] = Eq.Site[Slavesite].EqENA.ReadFreqList_Chan(chan + 1);
                                }
                                //Console.WriteLine("[{0}.3c] Site ", site);
                                foreach (int traceNum in Eq_ENA.Chan_Trace_set.Keys)
                                {
                                    if (Eq_ENA.Chan_Trace_set[traceNum] == chan)
                                    {
                                        tracenum = traceNum;
                                        break;
                                    }
                                }
                                //if (Chan == null) Console.WriteLine("[{0}.xx] Site Chan: {1};Trace: {2}", site, chan, trace);
                                Eq.Site[Site].EqENA.SendTrigger(SynchronizaionFlag, chan, tracenum);
                                string readback = Eq.Site[Site].EqENA.ReadString();
                                Eq.Site[Site].EqENA.ReadData(chan,SynchronizaionFlag);
                                DataFiles.SNP.SaveSNP(chan, tracenum.ToString(), TestParaName + PowerMode, 6, ResultBuilder.SitesAndPhases);//need to save all site once  
                                //else DataFiles.SNP.SaveSNP(chan, tracenum, PowerMode, 6, ResultBuilder.ValidSites, this.AntSw);
                                //Console.WriteLine("[{0}.3h] Site ", Site);
                            }
                            //Console.WriteLine("[{0}.5] Wait sync#2 site " + site.ToString(), site);
                            if (SynchronizaionFlag)
                            {
                                lock (objWaitSync)
                                {
                                    //Console.WriteLine("[{0}.6] Entered objWaitSync", site);
                                    if (!WaitSync(this.RowNum))
                                    {
                                        //Console.WriteLine("[{0}.7] Wait other thread", site);
                                        Monitor.Wait(objWaitSync);
                                    }
                                }
                            }

                            #endregion
 
                        }

                                               
                        #endregion
                       
                        
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
                    }
                    return 0;
                }

                switch (TestMode)
                {
                    case TestModes.FBAR:
                        if (this.ParaFunc != "TRIGGER") Search();
                        break;
                }
                CalcResults();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception happened during NA Runtest" + "\r\n" + e.ToString(), "Try/Catch Exception");
            }

            return 0;
        }
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
        public void Search()
        {
            try
            {
                if (!traceData[Site][chan].Keys.Contains(trace))
                {
                    {
                        //Console.WriteLine("[{0}.readdata] Entered Chan= {1}, trace = {2}", Site, chan, trace);
                        FullTrace = Eq.Site[Site].EqENA.ReadENATrace2(chan, trace, Site);
                        //Console.WriteLine("[{0}.readdata] done Chan= {1}, trace = {2}", Site, chan, trace);
                        #region Get the full trace data and extract Mag and Ang from it

                        for (int k = 0; k < FullTrace.Length; k += 1) //Magnitude and Angle convertion
                        {
                            //zMag.Add(20 * (Math.Log10(Math.Sqrt(Math.Pow(FullTrace[k], 2) + Math.Pow(FullTrace[k + 1], 2)))));
                            //zMag.Add(20 * (Math.Log10(Math.Sqrt(Math.Pow(FullTrace[k], 2) + Math.Pow(FullTrace[k + 1], 2)))));
                            //zMag[k] = 20 * (Math.Log10(Math.Sqrt(Math.Pow(FullTrace[k * 2], 2) + Math.Pow(FullTrace[(k * 2) + 1], 2))));
                            zMag.Add(FullTrace[k]);
                            //zAng[k] = Math.Atan2(FullTrace[(k * 2) + 1], FullTrace[k * 2]);
                            //zAng.Add(Math.Atan2(FullTrace[k + 1], FullTrace[k]));
                        }

                        #endregion
                        traceData[Site][chan].Add(trace, zMag.ToArray());
                        zMag.Clear();
                        zAng.Clear();
                        //updateflag = true;
                        //Console.WriteLine("[{0}.readdata] traceData updated Chan= {1}, trace = {2}", Site, chan, trace);
                    }

                } //Closing bracket if for "if (traceData[chan, trace] == null)"

               
                ENAresult=0; ENAresultFreq = 0;
                //Console.WriteLine("[{0}.ENA result] Entered Chan= {1}, trace = {2}", Site, chan, trace);
                #region make new list with only frequencies of interest, and interpolated end points
                SortedList<double,double> datasize=new SortedList<double,double>();
                if (subTrace.Keys.Contains(Site)) subTrace[Site].Clear();
                else subTrace.Add(Site, datasize);

                int[] startFreqI = new int[4];
                int[] endFreqI = new int[4];

                startFreqI[Site] = freqList[chan].BinarySearch(startFreq);
                //Console.WriteLine("[{0}.FreqList] Entered Chan= {1}, trace = {2}", Site, chan, trace);
                if (startFreqI[Site] < 0)  // start frequency not found, must interpolate
                {
                    startFreqI[Site] = ~startFreqI[Site];   // index just after target freq
                    subTrace[Site].Add(startFreq, InterpolateLinear(freqList[chan][startFreqI[Site] - 1], freqList[chan][startFreqI[Site]], traceData[Site][chan, trace][startFreqI[Site] - 1], traceData[Site][chan, trace][startFreqI[Site]], startFreq));
                }
                double BW = freqList[chan][startFreqI[Site]+1] - freqList[chan][startFreqI[Site]];  
                endFreqI[Site] = freqList[chan].BinarySearch(endFreq);
                if (endFreqI[Site] < 0)  // end frequency not found, must interpolate
                {
                    endFreqI[Site] = ~endFreqI[Site] - 1;   // index just before target freq
                    if (endFreq != startFreq) subTrace[Site].Add(endFreq, InterpolateLinear(freqList[chan][endFreqI[Site]], freqList[chan][endFreqI[Site] + 1], traceData[Site][chan, trace][endFreqI[Site]], traceData[Site][chan, trace][endFreqI[Site] + 1], endFreq));
                }
                // Console.WriteLine("[{0}.after Interpolate] Entered Chan= {1}, trace = {2}", Site, chan, trace);
                for (int i = startFreqI[Site]; i <= endFreqI[Site]; i++) subTrace[Site][freqList[chan][i]] = traceData[Site][chan, trace][i];

                
                #endregion
               //Console.WriteLine("[{0}.aa] Site ", Site);

                    switch (SearchType)
                    {
                        case SearchTypes.MIN:

                            #region Search Min
                            try
                            {
                                ENAresult = subTrace[Site].Values.Min();
                                ENAresultFreq = subTrace[Site].Keys[subTrace[Site].IndexOfValue(ENAresult)];
                                ENAresult = subTrace[Site].Values.Min();

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
                                ENAresult = subTrace[Site].Values.Max();
                                ENAresultFreq = subTrace[Site].Keys[subTrace[Site].IndexOfValue(ENAresult)];
                                // Console.WriteLine("[{0}.aa] Site Result ", Site, ENAresult);
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
                                double[] valuesWatts1 = new double[subTrace[Site].Values.Count];
                                double[] valuesWatts2 = new double[subTrace[Site].Values.Count];
                                if (ActiveSite == Site)
                                {
                                    for (int x = 0; x < subTrace.Values.Count; x++)
                                    {
                                        valuesWatts1[x] = dBmToWatts(subTrace[Site].Values[x]);
                                    }
                                    ENAresult = (10 * Math.Log10(valuesWatts1.Average() * 1000));
                                }
                                if (PassiveSite == Site)
                                {
                                    for (int x = 0; x < subTrace.Values.Count; x++)
                                    {
                                        valuesWatts2[x] = dBmToWatts(subTrace[Site].Values[x]);
                                    }
                                    ENAresult = (10 * Math.Log10(valuesWatts2.Average() * 1000));
                                }           
                                
                                ENAresultFreq = subTrace[Site].Keys[subTrace[Site].IndexOfValue(subTrace[Site].Values.Max())];
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
                                    if (subTrace[Site].ElementAt(i).Value < SearchTarget)
                                    {

                                        ENAresultFreq = InterpolateLinear(subTrace[Site].Values[i - 1], subTrace[Site].Values[i], subTrace[Site].Keys[i - 1], subTrace[Site].Keys[i], SearchTarget);
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
                                        if (subTrace[Site].ElementAt(i).Value > SearchTarget)
                                        {
                                            ENAresultFreq = InterpolateLinear(subTrace[Site].Values[i - 1], subTrace[Site].Values[i], subTrace[Site].Keys[i - 1], subTrace[Site].Keys[i], SearchTarget);
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
                                    if (subTrace[Site].ElementAt(i).Value < SearchTarget)
                                    {
                                        ENAresultFreq = InterpolateLinear(subTrace[Site].Values[i - 1], subTrace[Site].Values[i], subTrace[Site].Keys[i - 1], subTrace[Site].Keys[i], SearchTarget);
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
                                        if (subTrace[Site].ElementAt(i).Value > SearchTarget)
                                        {
                                            ENAresultFreq = InterpolateLinear(subTrace[Site].Values[i], subTrace[Site].Values[i + 1], subTrace[Site].Keys[i], subTrace[Site].Keys[i + 1], SearchTarget);
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
                            
                            double fstart = startFreq;
                            //double fstop = fstart + 5;
                            double fstop = endFreq;
                            //
                            try
                            {
                                for (int ripx = 0; fstart <= endFreq; ripx++)
                                {

                                    double minval, maxval;
                                    List<double> templist = new List<double>();

                                    startFreqI[Site] = freqList[chan].BinarySearch(fstart);
                                    if (startFreqI[Site] < 0)  // start frequency not found, must interpolate
                                    {
                                        startFreqI[Site] = ~startFreqI[Site];   // index just after target freq
                                        subTrace[Site][startFreq] = InterpolateLinear(freqList[chan][startFreqI[Site] - 1], freqList[chan][startFreqI[Site]], traceData[Site][chan, trace][startFreqI[Site] - 1], traceData[Site][chan, trace][startFreqI[Site]], fstart);
                                    }

                                    endFreqI[Site] = freqList[chan].BinarySearch(fstop);
                                    if (endFreqI[Site] < 0)  // end frequency not found, must interpolate
                                    {
                                        endFreqI[Site] = ~endFreqI[Site] - 1;   // index just before target freq
                                        subTrace[Site][fstop] = InterpolateLinear(freqList[chan][endFreqI[Site]], freqList[chan][endFreqI[Site] + 1], traceData[Site][chan, trace][endFreqI[Site]], traceData[Site][chan, trace][endFreqI[Site] + 1], fstop);
                                    }

                                    for (int i = startFreqI[Site]; i <= endFreqI[Site]; i++) templist.Add(traceData[Site][chan, trace][i]);  //This gets the trace data of interest

                                    //Interpolate for the value based on interpolated frequency
                                    double fstartval = InterpolateLinear(freqList[chan][startFreqI[Site] - 1], freqList[chan][endFreqI[Site] - 1], traceData[Site][chan, trace][startFreqI[Site] - 1], traceData[Site][chan, trace][endFreqI[Site] - 1], fstart);
                                    double fstopval = InterpolateLinear(freqList[chan][startFreqI[Site] - 1], freqList[chan][endFreqI[Site] - 1], traceData[Site][chan, trace][startFreqI[Site] - 1], traceData[Site][chan, trace][endFreqI[Site] - 1], fstop);

                                    templist.Add(fstartval);
                                    templist.Add(fstopval);

                                    minval = templist.Min();
                                    maxval = templist.Max();

                                    ripplelist.Add(maxval - minval);
                                    fstartlist.Add(fstart + BW);


                                    fstart = fstart +  BW; //500 kHz increment step size for frequency
                                    //fstop = fstart + BW;
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
                  
            }
            catch (Exception e)
            {

                MessageBox.Show("During " + TestParaName + "\r\n" + "Error while searching for " + SearchType + " value in ENA trace for " + SearchTarget.ToString() + "\r\n" + e.ToString(), "NetANTest.Search", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }
       
        public void CalcResults()
        {
            try
            {
                switch (TestMode)
                {
                    case TestModes.FBAR:
                        if (TestParaName.Contains("GD")) //Group delay measurements 
                        {
                        }
                        else
                        {
                            if (ENAresult < -100 || ENAresult > 100)
                            {
                                ENAresult = 50;
                            }
                        }

                        if (Sync_Group.Keys.Contains(Site)) MathCalc0.Add(this.RowNum.ToString() + "_" + Site.ToString(), ENAresult);
                        if (Sync_Group.Values.Contains(Site)) MathCalc1.Add(this.RowNum.ToString() + "_" + Site.ToString(), ENAresult);

                        break;
                    case TestModes.COMMON:
                        try
                        {
                            string[] Formula = this.formula.Split(',');
                            string arg1 = Formula[0]+"_"+Site.ToString();
                            string arg2 = Formula[1] + "_" + Site.ToString();
                            if (Sync_Group.Keys.Contains(Site)) ENAresult = MathCalc0[arg1] - MathCalc0[arg2];
                            if (Sync_Group.Values.Contains(Site)) ENAresult = MathCalc1[arg1] - MathCalc1[arg2];

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Catch exception in formula calculation :"+e.ToString());
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
        public void BuildResults(ref ATFReturnResult results)
        {
            try
            {
                if (this.ParaFunc.ToUpper() == "TRIGGER") return;

                switch (TestMode)
                {
                    case TestModes.FBAR:
                        if (TestParaName.Contains("GD")) //Group delay measurements 
                        {
                            ResultBuilder.AddResult(Site, TestParaName, "", (ENAresult));
                            // ResultBuilder.AddResult(Site, TestParaName + "_FREQ", "", ENAresultFreq);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, TestParaName, "", Math.Round(ENAresult, 4));
                            // ResultBuilder.AddResult(Site, TestParaName + "_FREQ", "", ENAresultFreq); 
                        }

                        break;
                    case TestModes.COMMON:

                        ResultBuilder.AddResult(Site, TestParaName, "", Math.Round(ENAresult, 4));
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
                    return (((upperY - lowerY) * (Convert.ToSingle(xTarget) - Convert.ToSingle(lowerX))) / (Convert.ToSingle(upperX) - Convert.ToSingle(lowerX))) + lowerY;
                }
                catch (Exception e)
                {
                    return -99999;
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
        public static class DataFiles
        {
            public static int LogEveryNthDut;  //Snp file will be saved every LogEveryNthDut
            public static int MaxNumDutsToLog;
            public static int dutSN = 0;
            public static string currResultFile;

            public static void PreTest()
            {
                string SN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");

                if (!string.IsNullOrEmpty(SN))
                    GetSN(SN);
                else
                    dutSN++;   // Lite Driver

                currResultFile = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, "");
                if (currResultFile.Trim() == "") currResultFile = "LiteDriver";   // for LiteDriver
            }
            public static void GetSN(string SN)
            {
                dutSN = Convert.ToInt32(SN);
                if (ResultBuilder.SitesAndPhases[0] == 1 && ResultBuilder.SitesAndPhases[1]!=0) dutSN += 2;
                if (ResultBuilder.SitesAndPhases[1] == 1) dutSN += 2;

            }
            public static void WriteAll()
            {
               Topaz_FbarTest.DataFiles.SNP.AcquireAll();

            }

            public static class SNP
            {
                public static bool Enable;
                private static string snpFolder;
                private static List<string> snpFiles = new List<string>();
                public static ManualResetEvent snpFlag = new ManualResetEvent(true);
                public static void AcquireAll()
                {
                    if (!Enable) return;

                    snpFolder = @"C:\Avago.ATF.Common\Results\" + currResultFile + "_SNP\\";

                    if (!Directory.Exists(snpFolder)) Directory.CreateDirectory(snpFolder);

                    snpFlag.WaitOne();
                    snpFlag.Reset();
                    //ThreadPool.QueueUserWorkItem(AcquireAll, null);
                }
                public static void SaveSNP(int chan, string trace, string titleNote, int numports, List<int> Numsite)
                {
                    if (!Enable) return;
                    try
                    {
                        if ((chan > 34 && chan <= 48) ||( chan >= 83 && chan <= 96) && titleNote.Contains("HA"))
                        {
                            string[] Title = titleNote.Split('H');
                            titleNote = Title[0] + "PAOFF";
                        }
                        string PIDName0 = "P_PID-" + (dutSN - 1).ToString();
                        string PIDName1 = "P_PID-" + (dutSN).ToString();
                        string snpFileName0 = currResultFile + "_S" + numports + PIDName0 + "_" + titleNote + ".s" + numports + "p";
                        string snpFileName1 = currResultFile + "_S" + numports + PIDName1 + "_" + titleNote + ".s" + numports + "p";

                        if (dutSN % LogEveryNthDut != 0 || dutSN / LogEveryNthDut > MaxNumDutsToLog) return;

                        if (snpFiles.Contains(snpFileName0))
                        {
                            MessageBox.Show("Error, SNP File is being created twice:\n\n" + snpFileName0 + "\n\nPlease check TCF to ensure you are not triggering excessively,\nand each test has unique naming.", "SNP File Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else if(snpFiles.Contains(snpFileName1))
                        {
                            MessageBox.Show("Error, SNP File is being created twice:\n\n" + snpFileName1 + "\n\nPlease check TCF to ensure you are not triggering excessively,\nand each test has unique naming.", "SNP File Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        int[] ports = new int[numports];
                        string path = "C:\\Avago.ATF.Common.x64\\SNP\\" + PIDName0 + "\\";
                        string path1 = "C:\\Avago.ATF.Common.x64\\SNP\\" + PIDName1 + "\\";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        if (!Directory.Exists(path1))
                        {
                            Directory.CreateDirectory(path1);
                        }
                        for (int site = 0; site < Numsite.Count(); site++ )
                        {
                            if (site == 0 || site == 1)
                            {
                                if (Numsite[site] == 1)
                                {
                                    for (int i = 1; i <= numports; i++)
                                    {
                                        ports[i - 1] = i;
                                    }
                                    //Eq_ENA.NA_IVI[site].Channels.get_Item(chan.ToString()).Measurements.get_Item(trace.ToString()).SaveSNP(ports,snpFileName);
                                    Eq_ENA.Active_ENA.NA_IVI.Channels.get_Item(chan.ToString()).Measurements.get_Item(trace).SaveSNP(ports, path + "Site" + site.ToString() + "_" + snpFileName0);
                                    //Eq_ENA.NA_IVI[site].Channels.get_Item("2").Measurements.get_Item("12").SaveSNP(ports, "C:\\Avago.ATF.2.2.5\\Data\\OrcaB40A.s3p");
                                    Eq_ENA.Active_ENA.NA_IVI.System.WaitForOperationComplete(10000);
                                    snpFiles.Add(snpFileName0);
                                }

                            }
                            else if (site == 2 || site == 3)
                            {
                                if (Numsite[site] == 1)
                                {
                                    for (int i = numports + 1; i <= numports * 2; i++)
                                    {
                                        ports[i - numports - 1] = i;
                                    }
                                    //Eq_ENA.NA_IVI[site].Channels.get_Item(chan.ToString()).Measurements.get_Item(trace.ToString()).SaveSNP(ports,snpFileName);
                                    chan++;
                                    trace += Eq_ENA.Chan_TraceNum[chan];
                                    Eq_ENA.Active_ENA.NA_IVI.Channels.get_Item(chan.ToString()).Measurements.get_Item(trace).SaveSNP(ports, path1 + "Site" + site.ToString() + "_" + snpFileName1);
                                    //Eq_ENA.NA_IVI[site].Channels.get_Item("2").Measurements.get_Item("12").SaveSNP(ports, "C:\\Avago.ATF.2.2.5\\Data\\OrcaB40A.s3p");
                                    Eq_ENA.Active_ENA.NA_IVI.System.WaitForOperationComplete(10000);
                                    snpFiles.Add(snpFileName1);
                                }

                            }

                        }
                        
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error happened during SNP Acquire in NetAn.cs" + "\r\n" + e.ToString());
                    }
                }


            }

            
        }
        public static Boolean canProceed = false;
        private static byte ActiveSite;
        private static byte PassiveSite;
        private static int thread_sync_count0 =9999;
        private static int thread_sync_count1 = 8888; 
        private const Int32 MAX_THREAD_COUNT = 2;
        private Object objLock = new Object();
        private static Object objWaitSync = new Object();
        public void PresetSynSite()
        {
            if (Sync_Group.Keys.Contains(Site)) ActiveSite = Site;
            if (Sync_Group.Values.Contains(Site)) PassiveSite = Site;
        }
        private Boolean WaitSync(int RowNum)
        {
            canProceed = false;
           //Console.WriteLine("[{0}.a] WaitSync - enter, objWaitSync = {1}", site, objWaitSync.GetHashCode());
            if (ActiveSite==Site) thread_sync_count0 = RowNum;
            if (PassiveSite==Site) thread_sync_count1 = RowNum;
            if (thread_sync_count0 == thread_sync_count1)  // If all threads have reported done.
            {
                //Console.WriteLine("[{0}.c] WaitSync - PulseAll", Site);
                //Console.WriteLine("thread_sync_count0:{0}", thread_sync_count0);
                Monitor.PulseAll(objWaitSync);  // Pulse all to notify other threads to continue.
                thread_sync_count0=9999;  // Reset the count.
                thread_sync_count1=8888;
                canProceed = true;
               // Console.WriteLine("[{0}.d] WaitSync - exit {1}", Site, canProceed);                 
            }
            return canProceed;
        }
    }

    public static class Topaz_PCBSub_AutoCal
    {
        public static bool[] Topaz_Trace_Still_Available = new bool[744]; // Added to support Multi-Site Split Test setup

        // [Burhan1] : 06/11/15
        // Data Type to capture VNA Calibration Coefficient
        public struct Spyro_AutoCal_Dataset
        {

            // SOLT done measurement flag
            // Raw
            public bool Flag_Open_Raw_Measurement_Port1;
            public bool Flag_Short_Raw_Measurement_Port1;
            public bool Flag_Load_Raw_Measurement_Port1;
            public bool Flag_Open_Raw_Measurement_Port2;
            public bool Flag_Short_Raw_Measurement_Port2;
            public bool Flag_Load_Raw_Measurement_Port2;
            public bool Flag_Open_Raw_Measurement_Port3;
            public bool Flag_Short_Raw_Measurement_Port3;
            public bool Flag_Load_Raw_Measurement_Port3;
            public bool Flag_Thru_P1P2_Raw_Measurement;
            public bool Flag_Thru_P1P3_Raw_Measurement;
            public bool Flag_Thru_P2P3_Raw_Measurement;
            // Actual
            public bool Flag_Open_Actual_Readin_Port1;
            public bool Flag_Short_Actual_Readin_Port1;
            public bool Flag_Load_Actual_Readin_Port1;
            public bool Flag_Open_Actual_Readin_Port2;
            public bool Flag_Short_Actual_Readin_Port2;
            public bool Flag_Load_Actual_Readin_Port2;
            public bool Flag_Open_Actual_Readin_Port3;
            public bool Flag_Short_Actual_Readin_Port3;
            public bool Flag_Load_Actual_Readin_Port3;
            public bool Flag_Thru_P1P2_Actual_Readin;
            public bool Flag_Thru_P1P3_Actual_Readin;
            public bool Flag_Thru_P2P3_Actual_Readin;
            //
            // Switch Setting
            public string Switch_Name;
            //
            // Data List
            //public double[] Freq_List;
            public double Freq_Hz;

            // Capture number of points
            public int Number_Of_Point;

            public string Channel_Band_Name;

        }

        public class Substarte_ID_Extract_Information
        {
            public string Header_String;
            public string Substrate_Type; // Either SOLT_CAL, GU_CAL, SOLT_VAL, GU_VAL
            public string[] Remaining_File_Content;
        }

        public static bool FLAG_Use_ID_Filter_For_SOLTCal = false;
        public static bool FLAG_Use_ID_Filter_For_GUCal = false;
        public static bool FLAG_Use_ID_Filter_For_SOLTValidation = false;
        public static bool FLAG_Use_ID_Filter_For_GUValidation = false;

        public static bool FLAG_CalibrationValidation;
        public static bool FLAG_CalibrationIncomplete;

        public static bool Flag_SOLT_Calibration_Enable;
        public static bool Flag_GU_Calibration_Enable;
        public static bool Flag_SOLT_Validation_Enable;
        public static bool Flag_GU_Validation_Enable;

        public static bool FLAG_Calibration_Set_Completed;
        public static bool FLAG_GU_Calibration_Completed;
        public static bool FLAG_Still_Have_Unit_In_Handler;

        public static bool Flag_Handler_Response_Still_Have_Unit_In_Handler;
        public static string SOT_MultiSite_Return_String; // Added to support MultiSite / Split Test

        public static bool Flag_Validation_Result_Pass;
        public static bool Flag_ValidValidation_Unit;
        public static bool FLAG_SOLT_Validation_Process_Completed;
        public static bool FLAG_GU_Validation_Process_Completed;

        public static bool FLAG_User_NotQuiting;

        public static int SOLT_Pass_Validation_Counter;
        public static int SOLT_Pass_Counter_User_Input;
        public static int GU_Pass_Validation_Counter;
        public static int GU_Pass_Counter_User_Input;

        public static bool Flag_RePunch_Cycle;
        public static bool FLAG_User_Wish_To_Move_To_Next_Unit;

        public static bool Flag_ValidValidation_Unit_SOLT;
        public static bool Flag_ValidValidation_Unit_GU;

        public static int SOLT_RePunch_Validation_Counter;
        public static int SOLT_RePunch_Counter_User_Input;
        public static int GU_RePunch_Validation_Counter;
        public static int GU_RePunch_Counter_User_Input;

        public static int Handler_Retry_SOT_Query;
        public static int Hanlder_Retry_SOT_Sleep_mSec;

        public static Substarte_ID_Extract_Information Extracted_Info_From_ID = new Substarte_ID_Extract_Information();

        public static int SOLT_CAL_Bin;
        public static int SOLT_VAL_Bin;
        public static int GU_VAL_Bin;
        public static int GU_CAL_Bin;
        public static int UnKnown_Bin;

        public static int Debug_CalSub_Counter; // Purely for debugging purpose only

        // Added to support MultiSite/Split Testing
        public static int Calibration_Substrate_Site1_X_Coor;
        public static int Calibration_Substrate_Site1_Y_Coor;
        public static int Calibration_Substrate_Site2_X_Coor;
        public static int Calibration_Substrate_Site2_Y_Coor;
        public static int Calibration_Substrate_Site3_X_Coor;
        public static int Calibration_Substrate_Site3_Y_Coor;
        public static int Calibration_Substrate_Site4_X_Coor;
        public static int Calibration_Substrate_Site4_Y_Coor;
        public static int Validation_Substrate_Site1_X_Coor;
        public static int Validation_Substrate_Site1_Y_Coor;
        public static int Validation_Substrate_Site2_X_Coor;
        public static int Validation_Substrate_Site2_Y_Coor;
        public static int Validation_Substrate_Site3_X_Coor;
        public static int Validation_Substrate_Site3_Y_Coor;
        public static int Validation_Substrate_Site4_X_Coor;
        public static int Validation_Substrate_Site4_Y_Coor;

        // ***************************************************************************
        // Make sure Debug_Without_Handler = False for actual run with handler !!!!!!!
        // Only set to true for debugging without handler
        public static bool Debug_Without_Handler = false; // true;
        // ***************************************************************************

        // [Burhan] : Add in for AutoCal
        //public static void Extract_Eterm_With_AutoHandler_ForAutoCal(string PCB_CALType, string SiteToCalibrate, MfgLotNum myMfgLotNum)
            public static void Extract_Eterm_With_AutoHandler_ForAutoCal(string PCB_CALType, string SiteToCalibrate)
        {

            ATFLogControl logger = ATFLogControl.Instance;  // for writing to Clotho Logger

            string Calibration_Validation_Substrate_ID;

            int Sub_Channel_Number = 0; // To store current Channel Number for existing Substrate in contactor

            bool SOLT_CAL_Complete_Flag = false;

            int Number_OfTestSite = 2; //  4;
            int Start_TesSiteNumber = 0;

            int Number_OfTestSite_Total = 4;

            Spyro_AutoCal_Dataset[,] Channel_Dataset_Storage = new Spyro_AutoCal_Dataset[24, Number_OfTestSite_Total]; // Note that all Flags already default to False

            int[,] Port1_Assignment = new int[24, Number_OfTestSite_Total];
            int[,] Port2_Assignment = new int[24, Number_OfTestSite_Total];
            int[,] Port3_Assignment = new int[24, Number_OfTestSite_Total];

            bool[,] Response_Cal_Flag = new bool[200, Number_OfTestSite_Total];
            bool[,] PCB_Variant_Cal_Flag = new bool[100, Number_OfTestSite_Total];

            bool[] Device_In_TestSite_Exist = new bool[Number_OfTestSite_Total]; // Added for MultiSite environment
            int[] Device_In_TestSite_Bin = new int[Number_OfTestSite_Total]; // Added for MultiSite environment

            bool Validation_Unit_Found = false;
            bool Validation_Unit_Pass = false;

            // Assign Site numbering
            if (SiteToCalibrate == "1-2")
            {
                Number_OfTestSite = 2;
                Start_TesSiteNumber = 0;
            }
            else // Just treat it as site to calibrate as "3-4"
            {
                Number_OfTestSite = 4;
                Start_TesSiteNumber = 2;
            }

            FLAG_User_NotQuiting = true;  // Initialize the FLAG to true first

            Debug_CalSub_Counter = 0; // For debugging purpose only

            // [T001] : Input Validation Detail
            SOLT_RePunch_Counter_User_Input = 0; // Allowable max re-punch for SOLT validation
            GU_RePunch_Counter_User_Input = 0; // Allowable max re-punch for GU validation
            SOLT_Pass_Counter_User_Input = 0; // Min number of SOLT Validation unit to pass SOLT calibration validation process
            GU_Pass_Counter_User_Input = 0; // Min number of GU Validation unit to pass SOLT calibration validation process

            // [T002] : Input Handler Detail
            Handler_Retry_SOT_Query = 15;
            Hanlder_Retry_SOT_Sleep_mSec = 1000;
            // Handler Bin assignment
            SOLT_CAL_Bin = 3;
            GU_CAL_Bin = 3;
            SOLT_VAL_Bin = 4;
            GU_VAL_Bin = 4;
            UnKnown_Bin = 5;

            // [T003] : Initialize and Setup NA
            // Load_FBAR_ForAutoCal(); // Remark as for Topaz will use same setup as production

            // [T004] : Input specific SOLT/GU/Validation units ID filter
            FLAG_Use_ID_Filter_For_SOLTCal = true;
            FLAG_Use_ID_Filter_For_GUCal = true;
            FLAG_Use_ID_Filter_For_SOLTValidation = true;
            FLAG_Use_ID_Filter_For_GUValidation = true;

            // [T005] : Set Calibration Valid Flag to False ( indicating NO production can start until valid calibration completed )
            FLAG_CalibrationValidation = false;
            FLAG_CalibrationIncomplete = false;

            FLAG_Calibration_Set_Completed = false;
            FLAG_GU_Calibration_Completed = false;

            int Handler_Test_Counter = 0;
            int OTP_ID_Test_Counter = 0; // For debug only

            // Read in ID first time as seems the first read in always giving wrong ID number
            // Can remove this if the above problem already resolve
            //for (byte k = Convert.ToByte(Start_TesSiteNumber); k < Number_OfTestSite; k++)
            //{
            //    // Just keep reading a few times eventhough the issue only happen on the first read
            //    Calibration_Validation_Substrate_ID = myMfgLotNum.Read(k).ToString();
            //    Calibration_Validation_Substrate_ID = myMfgLotNum.Read(k).ToString();
            //    Calibration_Validation_Substrate_ID = myMfgLotNum.Read(k).ToString();
            //}

            // Reset Topaz and Display and Update Off
            Eq_ENA.Active_ENA.Write_Topaz("SYST:PRES"); // Preset Topaz
            Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

            // Load in fresh state with all the traces available
            Eq_ENA.Active_ENA.Write_Topaz("MMEM:LOAD \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_burhan.csa".Trim() + "\"");
            SetSegmentPower(6);// set segment power level to 6dBm
                // Start deleting un-use Traces for the other two un-use Test Sites, this is needed ro reduce memory usage
                if (SiteToCalibrate == "1-2")
                {
                    for (int k = 1; k < 187; k++) // Run thru Trace #1 till #186
                    {
                        // Delete Traces from TestSite#2 and TestSite#3
                        Eq_ENA.Active_ENA.Write_Topaz("DISP:MEAS" + Actual_Trace_Number(2, k).ToString() + ":DEL");
                        Eq_ENA.Active_ENA.Write_Topaz("DISP:MEAS" + Actual_Trace_Number(3, k).ToString() + ":DEL");
                    }
                }
                else // Just treat it as site to calibrate as "3-4"
                {
                    for (int k = 1; k < 187; k++) // Run thru Trace #1 till #186
                    {
                        // Delete Traces from TestSite#0 and TestSite#1
                        Eq_ENA.Active_ENA.Write_Topaz("DISP:MEAS" + Actual_Trace_Number(0, k).ToString() + ":DEL");
                        Eq_ENA.Active_ENA.Write_Topaz("DISP:MEAS" + Actual_Trace_Number(1, k).ToString() + ":DEL");
                    }
                }

            // Display off the window
            Eq_ENA.Active_ENA.Write_Topaz("DISP:ENABle OFF"); // Off Display
            Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

            // For Handler communication debugging only
            if (1 == 0)
            {
                #region Handler Comunication debug

                Handler_Test_Counter = 0;

                while (Handler_Test_Counter <= 50000)
                {
                    // Delay
                    Thread.Sleep(500);

                    // Read SOT
                    //Flag_Handler_Response_Still_Have_Unit_In_Handler = bHandler.Check_Handler_SOT();
                    Flag_Handler_Response_Still_Have_Unit_In_Handler = Eq.Handler.CheckSRQStatusByte(72); 

                    if (Flag_Handler_Response_Still_Have_Unit_In_Handler == false) // False meaning SOT Received ???                    
                    {
                        // Delay
                        Thread.Sleep(500);

                        // Read ID Number
                        //Calibration_Validation_Substrate_ID = myMfgLotNum.Read(TestSiteNumber).ToString();
                        //Calibration_Validation_Substrate_ID = myMfgLotNum.Read(1).ToString();
                        
                        // Send SOT With Bin
                        Eq.Handler.TrayMapEOT(SOLT_CAL_Bin.ToString());
                    }

                    Handler_Test_Counter = Handler_Test_Counter + 1;

                }

                #endregion
            }

            if (1 == 1) // Initialize setting only allowing SOLT Calibration only
            {
                Flag_SOLT_Calibration_Enable = true;
                Flag_SOLT_Validation_Enable = false;
                Flag_GU_Calibration_Enable = false;
                Flag_GU_Validation_Enable = false;
            }

            if (PCB_CALType.ToUpper() == "1") // Perform CalKit Table calibration method
            {
                Load_Calibration(); // Load CakKit Calibration Table
            }

            // [T006] : Ask operator if this is for Full SOLT/GU or just cal validation process
            // if (MessageBox.Show("Proceed with Full Calibration and/or Validation?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
            if(1==1) // Purposely remove the messagebox in this case as it pretty annoying sometime
            {
                if (1 == 0) // Disable user prompt for now
                {
                    #region Query for SOLT and GU mode operation
                    if (MessageBox.Show("Perform SOLT Calibration and Validation?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Flag_SOLT_Calibration_Enable = true;
                        Flag_SOLT_Validation_Enable = true;
                    }
                    else
                    {
                        Flag_SOLT_Calibration_Enable = false;
                        Flag_SOLT_Validation_Enable = false;
                    }

                    if (MessageBox.Show("Perform GU Calibration and Validation?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Flag_GU_Calibration_Enable = true;
                        Flag_GU_Validation_Enable = true;
                    }
                    else
                    {
                        Flag_GU_Calibration_Enable = false;
                        Flag_GU_Validation_Enable = false;
                    }
                    #endregion
                }

                if (Flag_SOLT_Calibration_Enable == true)
                {
                    // Prepare NA Channel flag system to make sure all the needed path and ports have their own Flag
                    // These flags will be used to check if all the neccesary path / port already have the raw and actual data for eterm calculation later
                    #region Initialize variables

                    // Initialize all the flags for SOLT Calibration
                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {
                        Channel_Dataset_Storage[0, k].Switch_Name = "B1";
                        Channel_Dataset_Storage[1, k].Switch_Name = "B2";
                        Channel_Dataset_Storage[2, k].Switch_Name = "B3";
                        Channel_Dataset_Storage[3, k].Switch_Name = "B4";
                        Channel_Dataset_Storage[4, k].Switch_Name = "B7";
                        Channel_Dataset_Storage[5, k].Switch_Name = "B30";
                        Channel_Dataset_Storage[6, k].Switch_Name = "2GMB";
                        Channel_Dataset_Storage[7, k].Switch_Name = "GSMRX";
                        Channel_Dataset_Storage[8, k].Switch_Name = "TRX3";
                        Channel_Dataset_Storage[9, k].Switch_Name = "TRX2";
                        Channel_Dataset_Storage[10, k].Switch_Name = "DRX";
                        Channel_Dataset_Storage[11, k].Switch_Name = "B1";
                        Channel_Dataset_Storage[12, k].Switch_Name = "B2";
                        Channel_Dataset_Storage[13, k].Switch_Name = "B3";
                        Channel_Dataset_Storage[14, k].Switch_Name = "B4";
                        Channel_Dataset_Storage[15, k].Switch_Name = "B7";
                        Channel_Dataset_Storage[16, k].Switch_Name = "B30";
                        Channel_Dataset_Storage[17, k].Switch_Name = "2GMB";
                        Channel_Dataset_Storage[18, k].Switch_Name = "GSMRX";
                        Channel_Dataset_Storage[19, k].Switch_Name = "TRX3";
                        Channel_Dataset_Storage[20, k].Switch_Name = "TRX2";
                        Channel_Dataset_Storage[21, k].Switch_Name = "DRX";
                        Channel_Dataset_Storage[22, k].Switch_Name = "B1";
                        Channel_Dataset_Storage[23, k].Switch_Name = "B7";

                        Channel_Dataset_Storage[0, k].Channel_Band_Name = "B1";
                        Channel_Dataset_Storage[1, k].Channel_Band_Name = "B2";
                        Channel_Dataset_Storage[2, k].Channel_Band_Name = "B3";
                        Channel_Dataset_Storage[3, k].Channel_Band_Name = "B4";
                        Channel_Dataset_Storage[4, k].Channel_Band_Name = "B7";
                        Channel_Dataset_Storage[5, k].Channel_Band_Name = "B30";
                        Channel_Dataset_Storage[6, k].Channel_Band_Name = "2GMB";
                        Channel_Dataset_Storage[7, k].Channel_Band_Name = "GSMRX";
                        Channel_Dataset_Storage[8, k].Channel_Band_Name = "TRX3";
                        Channel_Dataset_Storage[9, k].Channel_Band_Name = "TRX2";
                        Channel_Dataset_Storage[10, k].Channel_Band_Name = "DRX";
                        Channel_Dataset_Storage[11, k].Channel_Band_Name = "B1";
                        Channel_Dataset_Storage[12, k].Channel_Band_Name = "B2";
                        Channel_Dataset_Storage[13, k].Channel_Band_Name = "B3";
                        Channel_Dataset_Storage[14, k].Channel_Band_Name = "B4";
                        Channel_Dataset_Storage[15, k].Channel_Band_Name = "B7";
                        Channel_Dataset_Storage[16, k].Channel_Band_Name = "B30";
                        Channel_Dataset_Storage[17, k].Channel_Band_Name = "2GMB";
                        Channel_Dataset_Storage[18, k].Channel_Band_Name = "GSMRX";
                        Channel_Dataset_Storage[19, k].Channel_Band_Name = "TRX3";
                        Channel_Dataset_Storage[20, k].Channel_Band_Name = "TRX2";
                        Channel_Dataset_Storage[21, k].Channel_Band_Name = "DRX";
                        Channel_Dataset_Storage[22, k].Channel_Band_Name = "B1";
                        Channel_Dataset_Storage[23, k].Channel_Band_Name = "B7";
                    }

                    // Initialize all the flags for SOLT Calibration
                    for (int j = 0; j < 24; j++)
                    {
                        for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                        {
                            // Initialize measurement flags
                            Channel_Dataset_Storage[j, k].Flag_Open_Raw_Measurement_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Raw_Measurement_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Raw_Measurement_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Open_Raw_Measurement_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Raw_Measurement_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Raw_Measurement_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Open_Raw_Measurement_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Raw_Measurement_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Raw_Measurement_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P1P2_Raw_Measurement = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P1P3_Raw_Measurement = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P2P3_Raw_Measurement = false;
                            //
                            Channel_Dataset_Storage[j, k].Number_Of_Point = 0;
                            //
                            // Initialize Readin flags
                            Channel_Dataset_Storage[j, k].Flag_Open_Actual_Readin_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Actual_Readin_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Actual_Readin_Port1 = false;
                            Channel_Dataset_Storage[j, k].Flag_Open_Actual_Readin_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Actual_Readin_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Actual_Readin_Port2 = false;
                            Channel_Dataset_Storage[j, k].Flag_Open_Actual_Readin_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Short_Actual_Readin_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Load_Actual_Readin_Port3 = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P1P2_Actual_Readin = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P1P3_Actual_Readin = false;
                            Channel_Dataset_Storage[j, k].Flag_Thru_P2P3_Actual_Readin = false;
                            //
                        }
                    }

                    for (int k = 0; k < 744; k++)
                    {
                        Topaz_Trace_Still_Available[k] = true;
                    }

                    // Variant Number Flag
                    // To indicate PCB Cal Variant number which is still missing for easy debugging
                    for (int j = 0; j < 100; j++)
                    {
                        for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++) PCB_Variant_Cal_Flag[j, k] = true;
                    }

                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {
                        // Reset the relevent Flags to false
                        PCB_Variant_Cal_Flag[1, k] = false;
                        PCB_Variant_Cal_Flag[2, k] = false;
                        PCB_Variant_Cal_Flag[8, k] = false;
                        PCB_Variant_Cal_Flag[10, k] = false;
                        PCB_Variant_Cal_Flag[11, k] = false;
                        PCB_Variant_Cal_Flag[12, k] = false;
                        PCB_Variant_Cal_Flag[14, k] = false;
                        PCB_Variant_Cal_Flag[15, k] = false;
                        PCB_Variant_Cal_Flag[17, k] = false;
                        PCB_Variant_Cal_Flag[19, k] = false;
                        PCB_Variant_Cal_Flag[21, k] = false;
                        PCB_Variant_Cal_Flag[22, k] = false;
                        PCB_Variant_Cal_Flag[23, k] = false;
                        PCB_Variant_Cal_Flag[25, k] = false;
                        PCB_Variant_Cal_Flag[26, k] = false;
                        PCB_Variant_Cal_Flag[28, k] = false;
                        PCB_Variant_Cal_Flag[30, k] = false;
                        PCB_Variant_Cal_Flag[31, k] = false;
                        PCB_Variant_Cal_Flag[34, k] = false;
                        PCB_Variant_Cal_Flag[35, k] = false;
                        PCB_Variant_Cal_Flag[38, k] = false;
                        PCB_Variant_Cal_Flag[40, k] = false;
                        PCB_Variant_Cal_Flag[41, k] = false;
                        PCB_Variant_Cal_Flag[43, k] = false;
                        PCB_Variant_Cal_Flag[48, k] = false;
                        PCB_Variant_Cal_Flag[49, k] = false;
                        PCB_Variant_Cal_Flag[50, k] = false;
                        PCB_Variant_Cal_Flag[52, k] = false;
                        PCB_Variant_Cal_Flag[54, k] = false;
                        PCB_Variant_Cal_Flag[55, k] = false;
                        PCB_Variant_Cal_Flag[57, k] = false;
                        PCB_Variant_Cal_Flag[59, k] = false;
                        PCB_Variant_Cal_Flag[61, k] = false;
                        PCB_Variant_Cal_Flag[62, k] = false;
                        PCB_Variant_Cal_Flag[63, k] = false;
                        PCB_Variant_Cal_Flag[64, k] = false;
                        PCB_Variant_Cal_Flag[68, k] = false;
                        PCB_Variant_Cal_Flag[69, k] = false;
                        PCB_Variant_Cal_Flag[71, k] = false;
                        PCB_Variant_Cal_Flag[73, k] = false;
                        PCB_Variant_Cal_Flag[74, k] = false;
                        PCB_Variant_Cal_Flag[76, k] = false;
                        PCB_Variant_Cal_Flag[77, k] = false;
                        PCB_Variant_Cal_Flag[80, k] = false;
                        PCB_Variant_Cal_Flag[81, k] = false;
                        PCB_Variant_Cal_Flag[82, k] = false;
                        PCB_Variant_Cal_Flag[86, k] = false;
                        PCB_Variant_Cal_Flag[87, k] = false;
                        PCB_Variant_Cal_Flag[88, k] = false;
                    }

                    // Initialize Flag for Response Trace calibration
                    // Initialize all the array content to true first
                    for (int j = 0; j < 200; j++)
                    {
                        for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++) Response_Cal_Flag[j, k] = true;
                    }

                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {
                        // Reset the relevent Flags to false
                        Response_Cal_Flag[4,k] = false;
                        Response_Cal_Flag[8, k] = false;
                        Response_Cal_Flag[9, k] = false;
                        Response_Cal_Flag[14, k] = false;
                        Response_Cal_Flag[18, k] = false;
                        Response_Cal_Flag[19, k] = false;
                        Response_Cal_Flag[24, k] = false;
                        Response_Cal_Flag[25, k] = false;
                        Response_Cal_Flag[29, k] = false;
                        Response_Cal_Flag[34, k] = false;
                        Response_Cal_Flag[35, k] = false;
                        Response_Cal_Flag[36, k] = false;
                        Response_Cal_Flag[44, k] = false;
                        Response_Cal_Flag[45, k] = false;
                        Response_Cal_Flag[46, k] = false;
                        Response_Cal_Flag[54, k] = false;
                        Response_Cal_Flag[55, k] = false;
                        Response_Cal_Flag[59, k] = false;
                        Response_Cal_Flag[62, k] = false;
                        Response_Cal_Flag[63, k] = false;
                        Response_Cal_Flag[64, k] = false;
                        Response_Cal_Flag[68, k] = false;
                        Response_Cal_Flag[69, k] = false;
                        Response_Cal_Flag[70, k] = false;
                        Response_Cal_Flag[74, k] = false;
                        Response_Cal_Flag[77, k] = false;
                        Response_Cal_Flag[78, k] = false;
                        Response_Cal_Flag[80, k] = false;
                        Response_Cal_Flag[83, k] = false;
                        Response_Cal_Flag[84, k] = false;
                        Response_Cal_Flag[86, k] = false;
                        Response_Cal_Flag[87, k] = false;
                        Response_Cal_Flag[90, k] = false;
                        Response_Cal_Flag[92, k] = false;
                        Response_Cal_Flag[98, k] = false;
                        Response_Cal_Flag[99, k] = false;
                        Response_Cal_Flag[102, k] = false;
                        Response_Cal_Flag[108, k] = false;
                        Response_Cal_Flag[109, k] = false;
                        Response_Cal_Flag[112, k] = false;
                        Response_Cal_Flag[115, k] = false;
                        Response_Cal_Flag[119, k] = false;
                        Response_Cal_Flag[122, k] = false;
                        Response_Cal_Flag[125, k] = false;
                        Response_Cal_Flag[126, k] = false;
                        Response_Cal_Flag[132, k] = false;
                        Response_Cal_Flag[135, k] = false;
                        Response_Cal_Flag[136, k] = false;
                        Response_Cal_Flag[142, k] = false;
                        Response_Cal_Flag[145, k] = false;
                        Response_Cal_Flag[149, k] = false;
                        Response_Cal_Flag[151, k] = false;
                        Response_Cal_Flag[153, k] = false;
                        Response_Cal_Flag[154, k] = false;
                        Response_Cal_Flag[157, k] = false;
                        Response_Cal_Flag[159, k] = false;
                        Response_Cal_Flag[160, k] = false;
                        Response_Cal_Flag[163, k] = false;
                        Response_Cal_Flag[167, k] = false;
                        Response_Cal_Flag[168, k] = false;
                        Response_Cal_Flag[169, k] = false;
                        Response_Cal_Flag[173, k] = false;
                        Response_Cal_Flag[174, k] = false;
                        Response_Cal_Flag[175, k] = false;
                        Response_Cal_Flag[177, k] = false;
                        Response_Cal_Flag[180, k] = false;
                    }

                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {

                        #region Site0_Site1_Port_Setting

                        if (k < 2) // Site#0 and Site#1 Port setting
                        {
                            // SOLT Channel Port assignment
                            // Port#1
                            Port1_Assignment[0, k] = 1;
                            Port2_Assignment[0, k] = 2;
                            Port3_Assignment[0, k] = 4;
                            // Port#2
                            Port1_Assignment[1, k] = 1;
                            Port2_Assignment[1, k] = 2;
                            Port3_Assignment[1, k] = 4;
                            // Port#3
                            Port1_Assignment[2, k] = 1;
                            Port2_Assignment[2, k] = 2;
                            Port3_Assignment[2, k] = 5;
                            // Port#4
                            Port1_Assignment[3, k] = 1;
                            Port2_Assignment[3, k] = 2;
                            Port3_Assignment[3, k] = 6;
                            // Port#5
                            Port1_Assignment[4, k] = 1;
                            Port2_Assignment[4, k] = 2;
                            Port3_Assignment[4, k] = 6;
                            // Port#6
                            Port1_Assignment[5, k] = 1;
                            Port2_Assignment[5, k] = 2;
                            Port3_Assignment[5, k] = 5;
                            // Port#7
                            Port1_Assignment[6, k] = 2;
                            Port2_Assignment[6, k] = 6;
                            Port3_Assignment[6, k] = 0;
                            // Port#8
                            Port1_Assignment[7, k] = 2;
                            Port2_Assignment[7, k] = 6;
                            Port3_Assignment[7, k] = 0;
                            // Port#9
                            Port1_Assignment[8, k] = 2;
                            Port2_Assignment[8, k] = 4;
                            Port3_Assignment[8, k] = 0;
                            // Port#10
                            Port1_Assignment[9, k] = 2;
                            Port2_Assignment[9, k] = 4;
                            Port3_Assignment[9, k] = 0;
                            // Port#11
                            Port1_Assignment[10, k] = 2;
                            Port2_Assignment[10, k] = 5;
                            Port3_Assignment[10, k] = 0;
                            // Port#12
                            Port1_Assignment[11, k] = 1;
                            Port2_Assignment[11, k] = 3;
                            Port3_Assignment[11, k] = 4;
                            // Port#13
                            Port1_Assignment[12, k] = 1;
                            Port2_Assignment[12, k] = 3;
                            Port3_Assignment[12, k] = 4;
                            // Port#14
                            Port1_Assignment[13, k] = 1;
                            Port2_Assignment[13, k] = 3;
                            Port3_Assignment[13, k] = 5;
                            // Port#15
                            Port1_Assignment[14, k] = 1;
                            Port2_Assignment[14, k] = 3;
                            Port3_Assignment[14, k] = 6;
                            // Port#16
                            Port1_Assignment[15, k] = 1;
                            Port2_Assignment[15, k] = 3;
                            Port3_Assignment[15, k] = 6;
                            // Port#17
                            Port1_Assignment[16, k] = 1;
                            Port2_Assignment[16, k] = 3;
                            Port3_Assignment[16, k] = 5;
                            // Port#18
                            Port1_Assignment[17, k] = 3;
                            Port2_Assignment[17, k] = 6;
                            Port3_Assignment[17, k] = 0;
                            // Port#19
                            Port1_Assignment[18, k] = 3;
                            Port2_Assignment[18, k] = 6;
                            Port3_Assignment[18, k] = 0;
                            // Port#20
                            Port1_Assignment[19, k] = 3;
                            Port2_Assignment[19, k] = 4;
                            Port3_Assignment[19, k] = 0;
                            // Port#21
                            Port1_Assignment[20, k] = 3;
                            Port2_Assignment[20, k] = 4;
                            Port3_Assignment[20, k] = 0;
                            // Port#22
                            Port1_Assignment[21, k] = 3;
                            Port2_Assignment[21, k] = 5;
                            Port3_Assignment[21, k] = 0;
                            // Port#23
                            Port1_Assignment[22, k] = 1;
                            Port2_Assignment[22, k] = 2;
                            Port3_Assignment[22, k] = 3;
                            // Port#24
                            Port1_Assignment[23, k] = 1;
                            Port2_Assignment[23, k] = 2;
                            Port3_Assignment[23, k] = 3;
                        }

                        #endregion Site0_Site1_Port_Setting

                        #region Site2_Site3_Port_Setting

                        if (k > 1 ) // Site#2 and Site#3 Port setting , Note step by 6 as start from Port 7 till port 12
                        {
                            // SOLT Channel Port assignment
                            // Port#1
                            Port1_Assignment[0, k] = 1 + 6;
                            Port2_Assignment[0, k] = 2 + 6;
                            Port3_Assignment[0, k] = 4 + 6;
                            // Port#2
                            Port1_Assignment[1, k] = 1 + 6;
                            Port2_Assignment[1, k] = 2 + 6;
                            Port3_Assignment[1, k] = 4 + 6;
                            // Port#3
                            Port1_Assignment[2, k] = 1 + 6;
                            Port2_Assignment[2, k] = 2 + 6;
                            Port3_Assignment[2, k] = 5 + 6;
                            // Port#4
                            Port1_Assignment[3, k] = 1 + 6;
                            Port2_Assignment[3, k] = 2 + 6;
                            Port3_Assignment[3, k] = 6 + 6;
                            // Port#5
                            Port1_Assignment[4, k] = 1 + 6;
                            Port2_Assignment[4, k] = 2 + 6;
                            Port3_Assignment[4, k] = 6 + 6;
                            // Port#6
                            Port1_Assignment[5, k] = 1 + 6;
                            Port2_Assignment[5, k] = 2 + 6;
                            Port3_Assignment[5, k] = 5 + 6;
                            // Port#7
                            Port1_Assignment[6, k] = 2 + 6;
                            Port2_Assignment[6, k] = 6 + 6;
                            Port3_Assignment[6, k] = 0; // Keep as Zero as just 2 port
                            // Port#8
                            Port1_Assignment[7, k] = 2 + 6;
                            Port2_Assignment[7, k] = 6 + 6;
                            Port3_Assignment[7, k] = 0; // Keep as Zero as just 2 port
                            // Port#9
                            Port1_Assignment[8, k] = 2 + 6;
                            Port2_Assignment[8, k] = 4 + 6;
                            Port3_Assignment[8, k] = 0; // Keep as Zero as just 2 port
                            // Port#10
                            Port1_Assignment[9, k] = 2 + 6;
                            Port2_Assignment[9, k] = 4 + 6;
                            Port3_Assignment[9, k] = 0; // Keep as Zero as just 2 port
                            // Port#11
                            Port1_Assignment[10, k] = 2 + 6;
                            Port2_Assignment[10, k] = 5 + 6;
                            Port3_Assignment[10, k] = 0; // Keep as Zero as just 2 port
                            // Port#12
                            Port1_Assignment[11, k] = 1 + 6;
                            Port2_Assignment[11, k] = 3 + 6;
                            Port3_Assignment[11, k] = 4 + 6;
                            // Port#13
                            Port1_Assignment[12, k] = 1 + 6;
                            Port2_Assignment[12, k] = 3 + 6;
                            Port3_Assignment[12, k] = 4 + 6;
                            // Port#14
                            Port1_Assignment[13, k] = 1 + 6;
                            Port2_Assignment[13, k] = 3 + 6;
                            Port3_Assignment[13, k] = 5 + 6;
                            // Port#15
                            Port1_Assignment[14, k] = 1 + 6;
                            Port2_Assignment[14, k] = 3 + 6;
                            Port3_Assignment[14, k] = 6 + 6;
                            // Port#16
                            Port1_Assignment[15, k] = 1 + 6;
                            Port2_Assignment[15, k] = 3 + 6;
                            Port3_Assignment[15, k] = 6 + 6;
                            // Port#17
                            Port1_Assignment[16, k] = 1 + 6;
                            Port2_Assignment[16, k] = 3 + 6;
                            Port3_Assignment[16, k] = 5 + 6;
                            // Port#18
                            Port1_Assignment[17, k] = 3 + 6;
                            Port2_Assignment[17, k] = 6 + 6;
                            Port3_Assignment[17, k] = 0; // Keep as Zero as just 2 port
                            // Port#19
                            Port1_Assignment[18, k] = 3 + 6;
                            Port2_Assignment[18, k] = 6 + 6;
                            Port3_Assignment[18, k] = 0; // Keep as Zero as just 2 port
                            // Port#20
                            Port1_Assignment[19, k] = 3 + 6;
                            Port2_Assignment[19, k] = 4 + 6;
                            Port3_Assignment[19, k] = 0; // Keep as Zero as just 2 port
                            // Port#21
                            Port1_Assignment[20, k] = 3 + 6;
                            Port2_Assignment[20, k] = 4 + 6;
                            Port3_Assignment[20, k] = 0; // Keep as Zero as just 2 port
                            // Port#22
                            Port1_Assignment[21, k] = 3 + 6;
                            Port2_Assignment[21, k] = 5 + 6;
                            Port3_Assignment[21, k] = 0; // Keep as Zero as just 2 port
                            // Port#23
                            Port1_Assignment[22, k] = 1 + 6;
                            Port2_Assignment[22, k] = 2 + 6;
                            Port3_Assignment[22, k] = 3 + 6;
                            // Port#24
                            Port1_Assignment[23, k] = 1 + 6;
                            Port2_Assignment[23, k] = 2 + 6;
                            Port3_Assignment[23, k] = 3 + 6;
                        }

                        #endregion Site2_Site3_Port_Setting

                    }

                    if (PCB_CALType.ToUpper() == "1") // Perform CalKit Table calibration method
                    {
                        Initialize_NA_Channel_Before_Autocal_For_CalKit_Method();
                    }
                    else // Perform Database calibration method
                    {
                        // Initialize all the NA channel setting for Calibration use
                        for (int i = 1; i < 24 + 1; i++)
                        {
                            for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                            {
                                Initialize_NA_Channel_Before_Autocal(Actual_Channel_Number(k, i), 1, Port1_Assignment[i - 1, k], Port2_Assignment[i - 1, k], Port3_Assignment[i - 1, k]);
                            }
                        }
                    }

                    #endregion
                }

                // Scan for SOT with in the timeout time frame
                if (Debug_Without_Handler == false)
                {
                    Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);

                    // Trigger handler to pickup Standard units for all the 4 contactor sites
                    Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray(true, true, false, SiteToCalibrate); // Reset all 4 sites tray coodinates, it will be standard units pick
                }
                else
                {
                    Flag_Handler_Response_Still_Have_Unit_In_Handler = true;
                } 
                FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;

                // Notify user to put in units if there is no unit in the handler
                while (FLAG_Still_Have_Unit_In_Handler == false && FLAG_User_NotQuiting == true)
                          #region Main SOLT and GU Calibration Loop
      {
                    // Check for missing Cal Variant for all sites
                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {
                        string Missing_CalVariant = "";
                        for (int j = 0; j < 100; j++)
                        {
                            if (PCB_Variant_Cal_Flag[j,k] == false)
                            {
                                Missing_CalVariant = Missing_CalVariant + "[" + j.ToString().Trim() + "]";
                            }
                        }
                        if (Missing_CalVariant != "") logger.Log(Avago.ATF.LogService.LogLevel.HighLight, "Missing PCB Variant Number for Site#" + k.ToString() + ": " + Missing_CalVariant + "\n");
                    }
                    if (MessageBox.Show("Please put in units into handler and press OK button, else to quit press Cancel button?", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                        FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;
                    }
                    else
                    {
                        FLAG_User_NotQuiting = false; // Set Flag to quit the process
                    }
                }

                // !!!! Turn all on for now, please update later once handler SOT process ready
                SOT_MultiSite_Return_String = "1111"; // literally follow ReleaseNOTE_Clotho_V2.2.5 page 11 format
                if (SOT_MultiSite_Return_String.Length == 4)
                {
                    Device_In_TestSite_Exist[0] = (SOT_MultiSite_Return_String.Substring(0, 1) == "1") ? true : false;
                    Device_In_TestSite_Exist[1] = (SOT_MultiSite_Return_String.Substring(1, 1) == "1") ? true : false;
                    Device_In_TestSite_Exist[2] = (SOT_MultiSite_Return_String.Substring(2, 1) == "1") ? true : false;
                    Device_In_TestSite_Exist[3] = (SOT_MultiSite_Return_String.Substring(3, 1) == "1") ? true : false;
                }
                else // If wrong string length then consider it is error and set all the site existance to false
                {
                    Device_In_TestSite_Exist[0] = false;
                    Device_In_TestSite_Exist[1] = false;
                    Device_In_TestSite_Exist[2] = false;
                    Device_In_TestSite_Exist[3] = false;
                }


                while (FLAG_Still_Have_Unit_In_Handler == true && FLAG_User_NotQuiting == true)
                {
                    for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                    {
                        if (Device_In_TestSite_Exist[k] == true)
                        {
                            byte TestSiteNumber = Convert.ToByte(k);

                            // [T007] : Start Loading in Calibration/Validation Substrate Unit
                            // Note that by this time the substarte already in the right placement as already query SOT earlier...

                            // [T008] : Read in Calibration / Validation Substarte ID

                            //Calibration_Validation_Substrate_ID = myMfgLotNum.Read(TestSiteNumber).ToString();

                            // Format to 6 charaters format
                            //Calibration_Validation_Substrate_ID = Int32.Parse(Calibration_Validation_Substrate_ID).ToString("000000");

                            // For full debug Calibration run mode without handler
                            if (Debug_Without_Handler==true)
                            {
                                #region Debug Full calibration without handler mode

                                if (k == 0) OTP_ID_Test_Counter = OTP_ID_Test_Counter + 1;
                                if (k == 2) OTP_ID_Test_Counter = OTP_ID_Test_Counter + 1;

                                //if (OTP_ID_Test_Counter == 190)
                                //{
                                //    Calibration_Validation_Substrate_ID = "0310";
                                //}

                               // Calibration_Validation_Substrate_ID = "0310" + OTP_ID_Test_Counter.ToString("00");

                                #endregion
                            }

                           // Extracted_Info_From_ID = Extract_Information_From_Substarte_ID(Calibration_Validation_Substrate_ID);

                            //Console.WriteLine("Calibration ID = " + Calibration_Validation_Substrate_ID);

                            // Set PCB_Variant_Cal_Flag to true indicating this PCB variant found and exist for Calibration use
                           // PCB_Variant_Cal_Flag[Convert.ToInt32(Calibration_Validation_Substrate_ID.Substring(Calibration_Validation_Substrate_ID.Length - 2)),k] = true;

                            // For MultiSite : Bin the Unit based on the Substrate Type
                            if (Extracted_Info_From_ID.Substrate_Type == "SOLT_CAL") Device_In_TestSite_Bin[k] = SOLT_CAL_Bin;
                            if (Extracted_Info_From_ID.Substrate_Type == "GU_CAL") Device_In_TestSite_Bin[k] = GU_CAL_Bin;
                            if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION") Device_In_TestSite_Bin[k] = SOLT_VAL_Bin;
                            if (Extracted_Info_From_ID.Substrate_Type == "GU_VALIDATION") Device_In_TestSite_Bin[k] = GU_VAL_Bin;
                            if (Extracted_Info_From_ID.Substrate_Type != "SOLT_CAL" && Extracted_Info_From_ID.Substrate_Type != "SOLT_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_CAL") Device_In_TestSite_Bin[k] = UnKnown_Bin;

                            // [T009] : If it is SOLT Calibration Substrate and SOLT Calibration mode is TRUE
                            if (Extracted_Info_From_ID.Substrate_Type == "SOLT_CAL" && Flag_SOLT_Calibration_Enable == true)
                            {
                                // [T011] : Check if the measurement for SOLT Calibration Set completed
                                if (FLAG_Calibration_Set_Completed == true)
                                {
                                    // Do nothing
                                }
                                else
                                {
                                    string[] tmp_split;

                                    // [T013] : Set Switch Matrix Path / Extract Raw Measurement / Extract Actual Data from File
                                    // Loop through all the combination available in the validation file // based on the header string provided in the database file ...
                                    tmp_split = Extracted_Info_From_ID.Header_String.Split('_').ToArray();
                                    for (int j = 0; j < tmp_split.Length; j++)
                                    {

                                        // Set the switch matrix to the correct Channel path
                                        if ((tmp_split[j].ToUpper().Substring(0, 1) == "C") && (Convert.ToInt32(tmp_split[j].ToUpper().Substring(1, tmp_split[j].Length - 1)) < 25) && (Convert.ToInt32(tmp_split[j].ToUpper().Substring(1, tmp_split[j].Length - 1)) > 0))
                                        {

                                            #region Switch Matrix Channel Path connection

                                            Sub_Channel_Number = 0;

                                            // Extract out channel number
                                            if (tmp_split[j].ToUpper().Contains("1")) Sub_Channel_Number = 1;
                                            if (tmp_split[j].ToUpper().Contains("2")) Sub_Channel_Number = 2;
                                            if (tmp_split[j].ToUpper().Contains("3")) Sub_Channel_Number = 3;
                                            if (tmp_split[j].ToUpper().Contains("4")) Sub_Channel_Number = 4;
                                            if (tmp_split[j].ToUpper().Contains("5")) Sub_Channel_Number = 5;
                                            if (tmp_split[j].ToUpper().Contains("6")) Sub_Channel_Number = 6;
                                            if (tmp_split[j].ToUpper().Contains("7")) Sub_Channel_Number = 7;
                                            if (tmp_split[j].ToUpper().Contains("8")) Sub_Channel_Number = 8;
                                            if (tmp_split[j].ToUpper().Contains("9")) Sub_Channel_Number = 9;
                                            if (tmp_split[j].ToUpper().Contains("10")) Sub_Channel_Number = 10;
                                            if (tmp_split[j].ToUpper().Contains("11")) Sub_Channel_Number = 11;
                                            if (tmp_split[j].ToUpper().Contains("12")) Sub_Channel_Number = 12;
                                            if (tmp_split[j].ToUpper().Contains("13")) Sub_Channel_Number = 13;
                                            if (tmp_split[j].ToUpper().Contains("14")) Sub_Channel_Number = 14;
                                            if (tmp_split[j].ToUpper().Contains("15")) Sub_Channel_Number = 15;
                                            if (tmp_split[j].ToUpper().Contains("16")) Sub_Channel_Number = 16;
                                            if (tmp_split[j].ToUpper().Contains("17")) Sub_Channel_Number = 17;
                                            if (tmp_split[j].ToUpper().Contains("18")) Sub_Channel_Number = 18;
                                            if (tmp_split[j].ToUpper().Contains("19")) Sub_Channel_Number = 19;
                                            if (tmp_split[j].ToUpper().Contains("20")) Sub_Channel_Number = 20;
                                            if (tmp_split[j].ToUpper().Contains("21")) Sub_Channel_Number = 21;
                                            if (tmp_split[j].ToUpper().Contains("22")) Sub_Channel_Number = 22;
                                            if (tmp_split[j].ToUpper().Contains("23")) Sub_Channel_Number = 23;
                                            if (tmp_split[j].ToUpper().Contains("24")) Sub_Channel_Number = 24;

                                            // * Set Switch Matrix Path based on the channel and port info provided from ID tag
                                            #region Switch settings

                                            string Band_Current;

                                            Band_Current = Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Channel_Band_Name.ToUpper();

                                            Console.WriteLine("Band_Current = " + Band_Current);
                                            Console.WriteLine("TestSite# = " + TestSiteNumber.ToString());
                                            Console.WriteLine("tmp_split[j] = " + tmp_split[j]);
                                            Console.WriteLine(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt"));

                                            if (TestSiteNumber % 2 == 0) Eq.Site[TestSiteNumber].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_1);
                                            else Eq.Site[TestSiteNumber].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_2);   // make this generic

                                            if (!(Band_Current.Contains("X") || Band_Current.Contains("M")))
                                            {
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N1toTx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N2toAnt);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N3toAnt);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N4toRx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N5toRx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N6toRx);
                                            }
                                            else
                                            {
                                                // Note that no N1 path needed ...
                                                //Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N1toTx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N2toAnt);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N3toAnt);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N4toRx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N5toRx);
                                                Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N6toRx);

                                            }

                                            //For switch
                                            Thread.Sleep(100);

                                            #endregion Switch settings

                                            Thread.Sleep(300); // To be adjusted or remove later

                                            #endregion Switch Matrix Channel Path connection

                                        }

                                        // * Perform raw data measurement for the specified channel / ports
                                        // * Mark the path / port flag to done once the above process completed successfully...
                                        if (tmp_split[j].ToUpper().Contains("OP") || tmp_split[j].ToUpper().Contains("SP") || tmp_split[j].ToUpper().Contains("LP") || tmp_split[j].ToUpper().Contains("TP") || tmp_split[j].ToUpper().Contains("RT")) // When the SOLT selected
                                        {
                                            // Make sure valid current channel for Spyro which is 1 till 7 only
                                            if (Sub_Channel_Number >= 0 && Sub_Channel_Number < 25)
                                            {

                                                Console.WriteLine("SOLT tmp_split[j] = " + tmp_split[j]);

                                                // Start manipulate the data into Eterm friendly format
                                                if (tmp_split[j].ToUpper().Contains("OP")) // Open Standard
                                                {
                                                    #region Measure Open Standard

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port1_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port1_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Open measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "OPEN", "P1", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Raw_Measurement_Port1 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Actual_Readin_Port1 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port2_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port2_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Open measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "OPEN", "P2", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Raw_Measurement_Port2 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Actual_Readin_Port2 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port3_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port3_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Open measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "OPEN", "P3", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Raw_Measurement_Port3 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Open_Actual_Readin_Port3 = true;
                                                    }

                                                    #endregion Measure Open Standard
                                                }
                                                if (tmp_split[j].ToUpper().Contains("SP")) // Short Standard
                                                {
                                                    #region Measure Short Standard

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port1_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port1_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Short measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "SHORT", "P1", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Raw_Measurement_Port1 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Actual_Readin_Port1 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port2_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port2_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Short measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "SHORT", "P2", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Raw_Measurement_Port2 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Actual_Readin_Port2 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port3_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port3_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Short measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "SHORT", "P3", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Raw_Measurement_Port3 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Short_Actual_Readin_Port3 = true;
                                                    }

                                                    #endregion Measure Short Standard
                                                }

                                                if (tmp_split[j].ToUpper().Contains("LP")) // Load Standard
                                                {
                                                    #region Measure Load Standard

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port1_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port1_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Load measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "LOAD", "P1", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Raw_Measurement_Port1 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Actual_Readin_Port1 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port2_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port2_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Load measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "LOAD", "P2", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Raw_Measurement_Port2 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Actual_Readin_Port2 = true;
                                                    }

                                                    if ((Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2)) == Port3_Assignment[Sub_Channel_Number - 1, k]) || (Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2))+6 == Port3_Assignment[Sub_Channel_Number - 1, k]))
                                                    {
                                                        // Trigger Load measurement for P1
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "LOAD", "P3", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Raw_Measurement_Port3 = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Load_Actual_Readin_Port3 = true;
                                                    }

                                                    #endregion Measure Load Standard
                                                }

                                                if (tmp_split[j].ToUpper().Contains("TP")) // Thru Standard
                                                {
                                                    #region Measure Thru Standard

                                                    int Start_ThruChar;
                                                    int Stop_ThruChar;
                                                    int PortA_Value = 0;
                                                    int PortB_Value = 0;
                                                    int PortA_AssignmentNumber = 0;
                                                    int PortB_AssignmentNumber = 0;

                                                    Start_ThruChar = tmp_split[j].ToUpper().IndexOf("P");
                                                    Stop_ThruChar = tmp_split[j].ToUpper().IndexOf("P", tmp_split[j].ToUpper().IndexOf("P") + 1);
                                                    PortA_Value = Convert.ToInt32(tmp_split[j].ToUpper().Substring(Start_ThruChar + 1, Stop_ThruChar - Start_ThruChar - 1));
                                                    PortB_Value = Convert.ToInt32(tmp_split[j].ToUpper().Substring(Stop_ThruChar + 1, tmp_split[j].Length - (Stop_ThruChar + 1)));

                                                    if ((PortA_Value == Port1_Assignment[Sub_Channel_Number - 1, k]) || (PortA_Value + 6 == Port1_Assignment[Sub_Channel_Number - 1, k])) PortA_AssignmentNumber = 1;
                                                    if ((PortA_Value == Port2_Assignment[Sub_Channel_Number - 1, k]) || (PortA_Value + 6 == Port2_Assignment[Sub_Channel_Number - 1, k])) PortA_AssignmentNumber = 2;
                                                    if ((PortA_Value == Port3_Assignment[Sub_Channel_Number - 1, k]) || (PortA_Value + 6 == Port3_Assignment[Sub_Channel_Number - 1, k])) PortA_AssignmentNumber = 3;

                                                    if ((PortB_Value == Port1_Assignment[Sub_Channel_Number - 1, k]) || (PortB_Value + 6 == Port1_Assignment[Sub_Channel_Number - 1, k])) PortB_AssignmentNumber = 1;
                                                    if ((PortB_Value == Port2_Assignment[Sub_Channel_Number - 1, k]) || (PortB_Value + 6 == Port2_Assignment[Sub_Channel_Number - 1, k])) PortB_AssignmentNumber = 2;
                                                    if ((PortB_Value == Port3_Assignment[Sub_Channel_Number - 1, k]) || (PortB_Value + 6 == Port3_Assignment[Sub_Channel_Number - 1, k])) PortB_AssignmentNumber = 3;

                                                    if (PortA_AssignmentNumber == 1 && PortB_AssignmentNumber == 2)
                                                    {
                                                        // Trigger Thru measurement for P1P2
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "THRU", "P1P2", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P1P2_Raw_Measurement = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P1P2_Actual_Readin = true;
                                                    }

                                                    if (PortA_AssignmentNumber == 1 && PortB_AssignmentNumber == 3)
                                                    {
                                                        // Trigger Thru measurement for P1P3
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "THRU", "P1P3", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P1P3_Raw_Measurement = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P1P3_Actual_Readin = true;
                                                    }

                                                    if (PortA_AssignmentNumber == 2 && PortB_AssignmentNumber == 3)
                                                    {
                                                        // Trigger Thru measurement for P2P3
                                                        Trigger_Acquire_Data(Actual_Channel_Number(k, Sub_Channel_Number), "THRU", "P2P3", Port3_Assignment[Sub_Channel_Number - 1, k]);

                                                        // Generate CITI File

                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P2P3_Raw_Measurement = true;
                                                        Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Flag_Thru_P2P3_Actual_Readin = true;
                                                    }

                                                    #endregion Measure Thru Standard
                                                }

                                                if (tmp_split[j].ToUpper().Contains("RT")) // Response Standard
                                                {
                                                    #region Measure Response

                                                    int TraceNumber = 0;

                                                    TraceNumber = Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2));

                                                    // Trigger Response measurement base on the TraceNumber
                                                    Trigger_Acquire_Data_Response(Actual_Channel_Number(k, Sub_Channel_Number), Actual_Trace_Number(k, TraceNumber), Actual_SingleTraceInChannel_Number(k, TraceNumber));

                                                    // Set the Flag to true indicating the Trace already went thru response cal
                                                    Response_Cal_Flag[TraceNumber, k] = true;

                                                    #endregion Measure Response
                                                }

                                            }
                                        }

                                        // Check if all the neccesary SOLT standards done
                                        #region Check if all the needed Cal Substrate measurement already done

                                        SOLT_CAL_Complete_Flag = true;
                                        for (int x = 0; x < 24; x++)
                                        {
                                            for (int q = Start_TesSiteNumber; q < Number_OfTestSite; q++)
                                            {
                                                if (Channel_Dataset_Storage[x, q].Flag_Open_Raw_Measurement_Port1 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Open_Raw_Measurement_Port2 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Short_Raw_Measurement_Port1 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Short_Raw_Measurement_Port2 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Load_Raw_Measurement_Port1 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Load_Raw_Measurement_Port2 == false) SOLT_CAL_Complete_Flag = false;
                                                if (Channel_Dataset_Storage[x, q].Flag_Thru_P1P2_Raw_Measurement == false) SOLT_CAL_Complete_Flag = false;
                                                if (Port3_Assignment[x, q] != 0) // only if the channel is 3-port SOLT setup
                                                {
                                                    if (Channel_Dataset_Storage[x, q].Flag_Open_Raw_Measurement_Port3 == false) SOLT_CAL_Complete_Flag = false;
                                                    if (Channel_Dataset_Storage[x, q].Flag_Short_Raw_Measurement_Port3 == false) SOLT_CAL_Complete_Flag = false;
                                                    if (Channel_Dataset_Storage[x, q].Flag_Load_Raw_Measurement_Port3 == false) SOLT_CAL_Complete_Flag = false;
                                                    if (Channel_Dataset_Storage[x, q].Flag_Thru_P1P3_Raw_Measurement == false) SOLT_CAL_Complete_Flag = false;
                                                    if (Channel_Dataset_Storage[x, q].Flag_Thru_P2P3_Raw_Measurement == false) SOLT_CAL_Complete_Flag = false;
                                                }
                                            }
                                            if (SOLT_CAL_Complete_Flag == false) break;
                                        }

                                        // Check if all the neccesary Response standards done
                                        if (SOLT_CAL_Complete_Flag == true)
                                        {
                                            for (int m = 0; m < 200; m++)
                                            {
                                                for (int q = Start_TesSiteNumber; q < Number_OfTestSite; q++)
                                                {
                                                    if (Response_Cal_Flag[m,q] == false) SOLT_CAL_Complete_Flag = false;
                                                }
                                                if (SOLT_CAL_Complete_Flag == false) break;
                                            }
                                        }

                                        // Perform Error Term Recalculation for all the SOLT standards done for all channels
                                        FLAG_Calibration_Set_Completed = SOLT_CAL_Complete_Flag;
                                        if (FLAG_Calibration_Set_Completed == true)
                                        {

                                            if (PCB_CALType.ToUpper() == "1") // Perform CalKit Table calibration method
                                            {
                                                Finalize_NA_Channel_After_Autocal_For_CalKit_Method();
                                            }
                                            else // Perform Database calibration method
                                            {
                                                for (int i = 1; i < 24 + 1; i++)
                                                {
                                                    for (int q = Start_TesSiteNumber; q < Number_OfTestSite; q++)
                                                    {
                                                        if (Port3_Assignment[i - 1, q] == 0)
                                                        {
                                                            Compute_Error_Term(i, Actual_Channel_Number(q, i), 2); // 2 Port Channel Cal
                                                        }
                                                        else
                                                        {
                                                            Compute_Error_Term(i, Actual_Channel_Number(q, i), 3); // 3 Port Channel Cal
                                                        }
                                                    }
                                                }
                                            }

                                            // Re-apply Cal registers for all the channels / Kind of refresh as Eterm Recalculation function causing some unwanted issue which can be fix by re-apply Cal Register
                                            // Seems recall Cal_Reg required for both Database and conventional methods
                                            for (int i = 1; i < 24 + 1; i++)
                                            {
                                                for (int q = Start_TesSiteNumber; q < Number_OfTestSite; q++)
                                                {
                                                    ReApply_CalReg_To_Channel(Actual_Channel_Number(q, i));
                                                    Console.WriteLine("q = " + q);
                                                    Console.WriteLine("i = " + i);
                                                    Console.WriteLine("Actual_Channel_Number = " + Actual_Channel_Number(q, i));
                                                }
                                            }

                                            break;
                                        }

                                        #endregion Check if all the needed Cal Substrate measurement already done

                                    }
                                }

                                // [T020] : Bin Back to "SOLT Unit" Bin
                                // bHandler.SendEOTCommand(SOLT_CAL_Bin);
                            }

                            // [T012] : If it is GU Calibration Substrate and GU Calibration mode is TRUE
                            if (Extracted_Info_From_ID.Substrate_Type == "GU_CAL" && Flag_GU_Calibration_Enable == true)
                            {
                                // [T014] : Check if GU Calibration completed
                                if (FLAG_GU_Calibration_Completed == true)
                                {
                                    // Do nothing
                                    logger.Log(Avago.ATF.LogService.LogLevel.Info, "Calibration is COMPLETED"); //CM Edited
                                }
                                else
                                {
                                    // [T015] : Perform GU Calibration process
                                    // * if GU Cal done then set FLAG_GU_Calibration_Completed == true stright away
                                    FLAG_GU_Calibration_Completed = true; // For now keep to true, needed to be changed once ready to port in GU Cal codes
                                }

                                // [T023] : Bin Back to "GU Unit" Bin
                                // bHandler.SendEOTCommand(GU_CAL_Bin);
                            }
                        }
                    }

                    #region Perform neccesary Handler Binning

                    string binResultSite1 = (Device_In_TestSite_Exist[0] == true) ? Device_In_TestSite_Bin[0].ToString().Trim() : "-1";
                    string binResultSite2 = (Device_In_TestSite_Exist[1] == true) ? Device_In_TestSite_Bin[1].ToString().Trim() : "-1";
                    string binResultSite3 = (Device_In_TestSite_Exist[2] == true) ? Device_In_TestSite_Bin[2].ToString().Trim() : "-1";
                    string binResultSite4 = (Device_In_TestSite_Exist[3] == true) ? Device_In_TestSite_Bin[3].ToString().Trim() : "-1";
                    string Final_Bin_String = binResultSite1 + "," + binResultSite2 + "," + binResultSite3 + "," + binResultSite4; // literally follow ReleaseNOTE_Clotho_V2.2.5 page 11

                    if (Debug_Without_Handler == false)
                    {
                        Final_Bin_String = "1,2,3,4"; // Force dummy string as per current understanding from Yanan/David
                        Eq.Handler.TrayMapEOT(Final_Bin_String); // !!! for MultiSite Handler configuration ready
                        Console.WriteLine("Running => Eq.Handler.TrayMapEOT(Final_Bin_String)");
                    }

                    /*
                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_CAL")
                    {
                        // [T020] : Bin Back to "SOLT Unit" Bin
                        bHandler.SendEOTCommand(SOLT_CAL_Bin);
                    }
                    if (Extracted_Info_From_ID.Substrate_Type == "GU_CAL")
                    {
                        // [T023] : Bin Back to "GU Unit" Bin
                        bHandler.SendEOTCommand(GU_CAL_Bin);
                    }

                    // [T010] : If it is Validation unit, SOLT_VAL and GU_VAL can be binned to different bin if neeeded...
                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION")
                    {
                        // [T021] : Bin Back to "Validation Unit" Bin
                        bHandler.SendEOTCommand(SOLT_VAL_Bin);
                    }
                    if (Extracted_Info_From_ID.Substrate_Type == "GU_VALIDATION")
                    {
                        // [T021] : Bin Back to "Validation Unit" Bin
                        bHandler.SendEOTCommand(GU_VAL_Bin);
                    }

                    // [T012] : If in is unknown Calibration Substrate
                    if (Extracted_Info_From_ID.Substrate_Type != "SOLT_CAL" && Extracted_Info_From_ID.Substrate_Type != "SOLT_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_CAL")
                    {
                        // [T022] : Bin Back to "Unknown Unit" Bin
                        bHandler.SendEOTCommand(UnKnown_Bin);
                    }
                    */

                    #endregion Perform neccesary Handler Binning

                    // [T016] : Check if the measurement for SOLT Calibration Set completed and GU Calibration completed
                    if (FLAG_Calibration_Set_Completed == true && (FLAG_GU_Calibration_Completed || (Flag_GU_Calibration_Enable == false)))
                    {
                        // Move to Eterm calculation and Validation process step [Obseleted]

                        // [T024] : Calculate Error Terms for all available channels ( consider interpolation ) [Obseleted]

                        // [T025] : Download Error Terms into Network Analyzer [Obseleted]

                        if (Validation_Unit_Found == false)
                        {
                            // [T026] : Save Network Analyzer "State" file (as backup) with latest calibration data
                            // Save_FBAR_State();
                            if (SiteToCalibrate == "1-2")
                            {
                                // Save as backup only, can be removed once system stable
                                Eq_ENA.Active_ENA.Write_Topaz("MMEM:STOR \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_1_2_CalOn_burhan.csa".Trim() + "\"");
                            }
                            else
                            {
                                // Save as backup only, can be removed once system stable
                                Eq_ENA.Active_ENA.Write_Topaz("MMEM:STOR \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_3_4_CalOn_burhan.csa".Trim() + "\"");

                                // Preset to get a fresh State file for file State file save with all the 96 channels cal on
                                // Reset Topaz and Display and Update ON
                                Eq_ENA.Active_ENA.Write_Topaz("SYST:PRES"); // Preset Topaz
                                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                                // Load in fresh state with all the traces available (Note : make sure al 744 tracse available!!!!)
                                Eq_ENA.Active_ENA.Write_Topaz("MMEM:LOAD \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_burhan.csa".Trim() + "\"");
                                Eq_ENA.Active_ENA.Write_Topaz("DISP:ENABle ON"); // On back Update
                                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                                // Load all the CalReg file for all the 96 Channels
                                for (int i = 1; i < 24 + 1; i++)
                                {
                                    for (int q = 0; q < Number_OfTestSite_Total; q++)
                                    {
                                        ReApply_CalReg_To_Channel(Actual_Channel_Number(q, i));
                                        Console.WriteLine("q = " + q);
                                        Console.WriteLine("i = " + i);
                                        Console.WriteLine("Actual_Channel_Number = " + Actual_Channel_Number(q, i));
                                    }
                                }
                                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                                // Save as final state file with all the 96 channels Cal On
                                Eq_ENA.Active_ENA.Write_Topaz("MMEM:STOR \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_All_CalOn.csa".Trim() + "\"");
                                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");
                            }
                        }

                        // [T027] : Save Error Terms to datalog file for history tracking [Obseleted]

                        // Set Flag to quit the calibration loop
                        FLAG_Still_Have_Unit_In_Handler = false;

                        // Next to move into validation process steps

                        // **********************************************************
                        // Note : Keep Validation with in this block for this version
                        // **********************************************************
                        #region Reflection (Open) Validation block

                        //bool Validation_Unit_Found = false;
                        bool Open_Validation_Fail_Flag = false;

                        // Added to support MultiSite environment
                        // bool[] Validation_Unit_In_TestSite_Found = new bool[4] { false, false, false, false };

                        if (Validation_Unit_Found == false)
                        {
                            if (MessageBox.Show("To perform Calibration Validation please press OK button, else press Cancel button to Quit", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                            {
                                Validation_Unit_Found = false;
                            }
                            else
                            {
                                Validation_Unit_Found = true; // Set to true just to skip the calibration validation step
                            }
                        }

                        bool First_Validation_Unit_Run = true;

                        // Added to support MultiSite environment
                        bool[] Validation_Unit_In_TestSite_Found = new bool[4] { false, false, false, false };
                        bool[] Validation_Unit_In_TestSite_Pass = new bool[4] { false, false, false, false };

                        while (Validation_Unit_Found == false) // Enter Validation loop
                        {
                            // Added to support MultiSite environment
                            //bool[] Validation_Unit_In_TestSite_Found = new bool[4] { false, false, false, false };
                            //bool[] Validation_Unit_In_TestSite_Pass = new bool[4] { false, false, false, false };

                            Open_Validation_Fail_Flag = false;

                            // Scan for SOT with in the timeout time frame
                            if (Debug_Without_Handler == false)
                            {
                                Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);

                                // Trigger handler to pickup Standard units for all the 4 contactor sites
                                Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray(First_Validation_Unit_Run, false, true, SiteToCalibrate); // Reset all 4 sites tray coodinates if it is the first validation unit, will be validation units pick
                                First_Validation_Unit_Run = false; // Once the first unit reset done, then the remaining will be in 1 step increment
                            }
                            else
                            {
                                Flag_Handler_Response_Still_Have_Unit_In_Handler = true;
                            }
                            FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;

                            // Notify user to put in units if there is no unit in the handler
                            while (FLAG_Still_Have_Unit_In_Handler == false && FLAG_User_NotQuiting == true)
                            {
                                if (MessageBox.Show("Please put in units into handler and press OK button, else to quit press Cancel button?", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                    Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                                    FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;
                                }
                                else
                                {
                                    Validation_Unit_Found = true; // Set Flag to quit the process
                                }
                            }

                            // !!!! Turn all on for now, please update later once handler SOT process ready
                            SOT_MultiSite_Return_String = "1111"; // literally follow ReleaseNOTE_Clotho_V2.2.5 page 11 format
                            if (SOT_MultiSite_Return_String.Length == 4)
                            {
                                Device_In_TestSite_Exist[0] = (SOT_MultiSite_Return_String.Substring(0, 1) == "1") ? true : false;
                                Device_In_TestSite_Exist[1] = (SOT_MultiSite_Return_String.Substring(1, 1) == "1") ? true : false;
                                Device_In_TestSite_Exist[2] = (SOT_MultiSite_Return_String.Substring(2, 1) == "1") ? true : false;
                                Device_In_TestSite_Exist[3] = (SOT_MultiSite_Return_String.Substring(3, 1) == "1") ? true : false;
                            }
                            else // If wrong string length then consider it is error and set all the site existance to false
                            {
                                Device_In_TestSite_Exist[0] = false;
                                Device_In_TestSite_Exist[1] = false;
                                Device_In_TestSite_Exist[2] = false;
                                Device_In_TestSite_Exist[3] = false;
                            }

                            for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                            {
                                if (Device_In_TestSite_Exist[k] == true)
                                {
                                    byte TestSiteNumber = Convert.ToByte(k);

                                    // Read ID
                                    //Calibration_Validation_Substrate_ID = myMfgLotNum.Read(TestSiteNumber).ToString();
                                    if (Debug_Without_Handler==true) Calibration_Validation_Substrate_ID = "030007"; // Only when debugging Flag on

                                    // Format to 6 charaters format
                                  //  Calibration_Validation_Substrate_ID = Int32.Parse(Calibration_Validation_Substrate_ID).ToString("000000");

                                  //  Extracted_Info_From_ID = Extract_Information_From_Substarte_ID(Calibration_Validation_Substrate_ID);

                                    // For MultiSite : Bin the Unit based on the Substrate Type
                                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_CAL") Device_In_TestSite_Bin[k] = SOLT_CAL_Bin;
                                    if (Extracted_Info_From_ID.Substrate_Type == "GU_CAL") Device_In_TestSite_Bin[k] = GU_CAL_Bin;
                                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION") Device_In_TestSite_Bin[k] = SOLT_VAL_Bin;
                                    if (Extracted_Info_From_ID.Substrate_Type == "GU_VALIDATION") Device_In_TestSite_Bin[k] = GU_VAL_Bin;
                                    if (Extracted_Info_From_ID.Substrate_Type != "SOLT_CAL" && Extracted_Info_From_ID.Substrate_Type != "SOLT_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_CAL") Device_In_TestSite_Bin[k] = UnKnown_Bin;

                                    // Check if this is validation unit
                                    // Perform validation here
                                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION")
                                    {
                                        string[] tmp_split;

                                        Open_Validation_Fail_Flag = false;

                                        // Loop through all the combination available in the validation file // based on the header string provided in the database file ...
                                        tmp_split = Extracted_Info_From_ID.Header_String.Split('_').ToArray();
                                        for (int j = 0; j < tmp_split.Length; j++)
                                        {

                                            // Set the switch matrix to the correct Channel path
                                            if ((tmp_split[j].ToUpper().Substring(0, 1) == "C") && (Convert.ToInt32(tmp_split[j].ToUpper().Substring(1, tmp_split[j].Length - 1)) < 25) && (Convert.ToInt32(tmp_split[j].ToUpper().Substring(1, tmp_split[j].Length - 1)) > 0))
                                            {
                                                #region Switch Matrix Channel Path connection

                                                Sub_Channel_Number = 0;

                                                // Extract out channel number
                                                if (tmp_split[j].ToUpper().Contains("1")) Sub_Channel_Number = 1;
                                                if (tmp_split[j].ToUpper().Contains("2")) Sub_Channel_Number = 2;
                                                if (tmp_split[j].ToUpper().Contains("3")) Sub_Channel_Number = 3;
                                                if (tmp_split[j].ToUpper().Contains("4")) Sub_Channel_Number = 4;
                                                if (tmp_split[j].ToUpper().Contains("5")) Sub_Channel_Number = 5;
                                                if (tmp_split[j].ToUpper().Contains("6")) Sub_Channel_Number = 6;
                                                if (tmp_split[j].ToUpper().Contains("7")) Sub_Channel_Number = 7;
                                                if (tmp_split[j].ToUpper().Contains("8")) Sub_Channel_Number = 8;
                                                if (tmp_split[j].ToUpper().Contains("9")) Sub_Channel_Number = 9;
                                                if (tmp_split[j].ToUpper().Contains("10")) Sub_Channel_Number = 10;
                                                if (tmp_split[j].ToUpper().Contains("11")) Sub_Channel_Number = 11;
                                                if (tmp_split[j].ToUpper().Contains("12")) Sub_Channel_Number = 12;
                                                if (tmp_split[j].ToUpper().Contains("13")) Sub_Channel_Number = 13;
                                                if (tmp_split[j].ToUpper().Contains("14")) Sub_Channel_Number = 14;
                                                if (tmp_split[j].ToUpper().Contains("15")) Sub_Channel_Number = 15;
                                                if (tmp_split[j].ToUpper().Contains("16")) Sub_Channel_Number = 16;
                                                if (tmp_split[j].ToUpper().Contains("17")) Sub_Channel_Number = 17;
                                                if (tmp_split[j].ToUpper().Contains("18")) Sub_Channel_Number = 18;
                                                if (tmp_split[j].ToUpper().Contains("19")) Sub_Channel_Number = 19;
                                                if (tmp_split[j].ToUpper().Contains("20")) Sub_Channel_Number = 20;
                                                if (tmp_split[j].ToUpper().Contains("21")) Sub_Channel_Number = 21;
                                                if (tmp_split[j].ToUpper().Contains("22")) Sub_Channel_Number = 22;
                                                if (tmp_split[j].ToUpper().Contains("23")) Sub_Channel_Number = 23;
                                                if (tmp_split[j].ToUpper().Contains("24")) Sub_Channel_Number = 24;

                                                // * Set Switch Matrix Path based on the channel and port info provided from ID tag
                                                #region Switch settings

                                                string Band_Current;

                                                Band_Current = Channel_Dataset_Storage[Sub_Channel_Number - 1, k].Channel_Band_Name.ToUpper();

                                                Console.WriteLine("Band_Current = " + Band_Current);
                                                Console.WriteLine("TestSite# = " + TestSiteNumber.ToString());
                                                Console.WriteLine("tmp_split[j] = " + tmp_split[j]);
                                                Console.WriteLine(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt"));

                                                if (TestSiteNumber % 2 == 0) Eq.Site[TestSiteNumber].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_1);
                                                else Eq.Site[TestSiteNumber].SwMatrixSplit.ActivatePath("Split", Operation.P_FBARtoFBAR_2);   // make this generic

                                                if (!(Band_Current.Contains("X") || Band_Current.Contains("M")))
                                                {
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N1toTx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N2toAnt);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N3toAnt);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N4toRx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N5toRx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N6toRx);
                                                }
                                                else
                                                {
                                                    // Note that no N1 path needed ...
                                                    //Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N1toTx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N2toAnt);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N3toAnt);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N4toRx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N5toRx);
                                                    Eq.Site[TestSiteNumber].SwMatrix.ActivatePath(Band_Current, Operation.N6toRx);

                                                }

                                                //For switch
                                                Thread.Sleep(100);

                                                #endregion Switch settings

                                                Thread.Sleep(300); // To be adjusted or remove later

                                                #endregion Switch Matrix Channel Path connection
                                            }

                                            if (tmp_split[j].ToUpper().Contains("RT")) // Open Standard Validation
                                            {
                                                // Make sure valid current channel for Maximator which is 1 till 24 only
                                                if (Sub_Channel_Number >= 0 && Sub_Channel_Number < 25)
                                                {
                                                    #region Measure and Validate Open Reflection

                                                    int TraceNumber = 0;

                                                    TraceNumber = Convert.ToInt32(tmp_split[j].ToUpper().Substring(2, tmp_split[j].Length - 2));

                                                    // Only do this if unit not found yet before or already found by failed
                                                    if ((Validation_Unit_In_TestSite_Found[k] == true && Validation_Unit_In_TestSite_Pass[k] == false)||(Validation_Unit_In_TestSite_Found[k] == false))
                                                    {
                                                        // Trigger Validate Response measurement base on the TraceNumber
                                                        if (Debug_Without_Handler == false)
                                                        {
                                                            if (Validate_Trace_Response_Data(Actual_Channel_Number(k, Sub_Channel_Number), Actual_Trace_Number(k, TraceNumber), -0.2, 0.2) == false)
                                                            {
                                                                // If the validation trace fail
                                                                Open_Validation_Fail_Flag = true;
                                                                //Validation_Unit_In_TestSite_Pass[k] = false;
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                //Open_Validation_Fail_Flag = false;
                                                                //Validation_Unit_In_TestSite_Pass[k] = true;
                                                                //break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (Validate_Trace_Response_Data(Actual_Channel_Number(k, Sub_Channel_Number), Actual_Trace_Number(k, TraceNumber), -2000, 2000) == false) // For debugging, please change back to +/- 0.3dB later
                                                            {
                                                                // If the validation trace fail
                                                                Open_Validation_Fail_Flag = true;
                                                                //Validation_Unit_In_TestSite_Pass[k] = false;
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                //Open_Validation_Fail_Flag = false;
                                                                //Validation_Unit_In_TestSite_Pass[k] = true;
                                                                //break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        break;
                                                    }

                                                    #endregion Measure and Validate Open Reflection
                                                }
                                            }
                                        }
                                        
                                        // Set the Flag to true indicating the validation unit found
                                        Validation_Unit_In_TestSite_Found[k] = true;

                                        // Check Pass flag
                                        if (Open_Validation_Fail_Flag == false) Validation_Unit_In_TestSite_Pass[k] = true;

                                    }
                                }
                            }

                            #region Binning process

                            //string binResultSiteA = (Device_In_TestSite_Exist[0] == true) ? Device_In_TestSite_Bin[0].ToString().Trim() : "-1";
                            //string binResultSiteB = (Device_In_TestSite_Exist[1] == true) ? Device_In_TestSite_Bin[1].ToString().Trim() : "-1";
                            //string binResultSiteC = (Device_In_TestSite_Exist[2] == true) ? Device_In_TestSite_Bin[2].ToString().Trim() : "-1";
                            //string binResultSiteD = (Device_In_TestSite_Exist[3] == true) ? Device_In_TestSite_Bin[3].ToString().Trim() : "-1";
                            //string Final_BinValidate_String = binResultSiteA + "," + binResultSiteB + "," + binResultSiteC + "," + binResultSiteD; // literally follow ReleaseNOTE_Clotho_V2.2.5 page 11

                            if (Debug_Without_Handler == false)
                            {
                                Final_Bin_String = "1,2,3,4"; // Force dummy string as per current understanding from Yanan/David
                                Eq.Handler.TrayMapEOT(Final_Bin_String); // !!! for MultiSite Handler configuration ready
                                Console.WriteLine("Running => Eq.Handler.TrayMapEOT(Final_Bin_String)");
                            }

                            #endregion Binning process

                            // Only if validation substrates found in all the test sites, set the flag to true
                            Validation_Unit_Found = true; // Reset to true before check the actual status below
                            Validation_Unit_Pass = true;
                            for (int z = Start_TesSiteNumber; z < Number_OfTestSite; z++)
                            {
                                if (Validation_Unit_In_TestSite_Found[z] == false) Validation_Unit_Found = false;
                                if (Validation_Unit_In_TestSite_Pass[z] == false) Validation_Unit_Pass = false;

                            }

                            if (Validation_Unit_Found == true && Validation_Unit_Pass == false) //Open_Validation_Fail_Flag == true)
                            {
                                if (MessageBox.Show("Validation FAILED, ReRun Calibration Validation again please press OK button, else press Cancel button to Quit", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                    Validation_Unit_Found = false; // Set back the flag to false if user wish to re-run validation again
                                    // Reset the fail site only
                                    for (int z = Start_TesSiteNumber; z < Number_OfTestSite; z++)
                                    {
                                        //if (Validation_Unit_In_TestSite_Found[z] == false) Validation_Unit_Found = false;
                                        if (Validation_Unit_In_TestSite_Pass[z] == false)
                                        {
                                            //Validation_Unit_In_TestSite_Pass[z] = true;
                                            Validation_Unit_In_TestSite_Found[z] = false;
                                        }

                                    }
                                }
                            }

                            // Notify user that the Cal Validation pass
                            if (Validation_Unit_Found == true && Validation_Unit_Pass == true) //Open_Validation_Fail_Flag == false)
                            {
                                MessageBox.Show("Passed Calibration Standard !!!!", "Notifcation", MessageBoxButtons.OK); 
                                FLAG_Still_Have_Unit_In_Handler = false; // Added to make sure to program exit the main while loop : [Burhan], Added to solve "handler communication crash" observed by Yanan
                                FLAG_User_NotQuiting = false; // Added to make sure to program exit the main while loop : [Burhan], Added to solve "handler communication crash" observed by Yanan
                                break;
                            }

                        }

                        #endregion Reflection (Open) Validation block

                    }
                    else  // If not yet complete the SOLT / GU Cal then
                    {
                        #region Check for SOT ( indication avaiability of new unit in the contactor )

                        // Check for missing Cal Variant for all sites [Burhan 9June2016]
                        for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                        {
                            string Missing_CalVariant = "";
                            for (int j = 0; j < 100; j++)
                            {
                                if (PCB_Variant_Cal_Flag[j, k] == false)
                                {
                                    Missing_CalVariant = Missing_CalVariant + "[" + j.ToString().Trim() + "]";
                                }
                            }
                            if (Missing_CalVariant != "") Console.WriteLine("Missing PCB Variant Number for Site#" + k.ToString() + ": " + Missing_CalVariant + "\n");
                            if (Missing_CalVariant != "") logger.Log(Avago.ATF.LogService.LogLevel.Info, "Missing PCB Variant Number for Site#" + k.ToString() + ": " + Missing_CalVariant + "\n");
                            if (Missing_CalVariant != "") logger.Log(Avago.ATF.LogService.LogLevel.Error, "Missing PCB Variant Number for Site#" + k.ToString() + ": " + Missing_CalVariant + "\n");
                            logger.Log(Avago.ATF.LogService.LogLevel.Info, "- Missing Variant Marker -" + "\n");
                            logger.Log(Avago.ATF.LogService.LogLevel.Error, "- Missing Variant Marker -" + "\n");
                        
                        }
                        // Scan for SOT with in the timeout time frame
                        if (Debug_Without_Handler == false)
                        {
                            Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);

                            // Trigger handler to pickup Standard units for all the 4 contactor sites
                            Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray(false, true, false, SiteToCalibrate); // pick next units in the tray, will be standard units pick
                        
                            // Added purely for debugging issue
                            //logger.Log(Avago.ATF.LogService.LogLevel.Info, "Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray executed" + "\n");
                            //logger.Log(Avago.ATF.LogService.LogLevel.Error, "Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray executed" + "\n");

                        }
                        else
                        {
                            Flag_Handler_Response_Still_Have_Unit_In_Handler = true;
                        } 
                        FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;

                        // Notify user to put in units if there is no unit in the handler
                        while (FLAG_Still_Have_Unit_In_Handler == false && FLAG_User_NotQuiting == true)
                        {
                            // Check for missing Cal Variant for all sites
                            for (int k = Start_TesSiteNumber; k < Number_OfTestSite; k++)
                            {
                                string Missing_CalVariant = "";
                                for (int j = 0; j < 100; j++)
                                {
                                    if (PCB_Variant_Cal_Flag[j, k] == false)
                                    {
                                        Missing_CalVariant = Missing_CalVariant + "[" + j.ToString().Trim() + "]";
                                    }
                                }
                                if (Missing_CalVariant != "") logger.Log(Avago.ATF.LogService.LogLevel.HighLight, "Missing PCB Variant Number for Site#" + k.ToString() + ": " + Missing_CalVariant + "\n");
                            }

                            // [T019] : Notify Operator for incomplete operation due to lack of units in the handler
                            // [T017] : Check if the operator loading in new fresh units
                            if (MessageBox.Show("Please put in units into handler and press OK button, else to quit press Cancel button?", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                            {
                                // [T018] : Any more unit in the handler?
                                Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                                FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;
                            }
                            else
                            {
                                // If operator press cancel than that mean operator wish to terminate remaining process steps
                                // [T045] : Set Calibration Valid Flag to incomplete ( no production can start until valid calibration completed)
                                FLAG_CalibrationIncomplete = true;

                                FLAG_Still_Have_Unit_In_Handler = false; // Set Flag to quit the calibration loop
                                FLAG_User_NotQuiting = false; // Set Flag to quit the process
                            }
                        }

                        // !!!! Turn all on for now, please update later once handler SOT process ready
                        SOT_MultiSite_Return_String = "1111"; // literally follow ReleaseNOTE_Clotho_V2.2.5 page 11 format
                        if (SOT_MultiSite_Return_String.Length == 4)
                        {
                            Device_In_TestSite_Exist[0] = (SOT_MultiSite_Return_String.Substring(0, 1) == "1") ? true : false;
                            Device_In_TestSite_Exist[1] = (SOT_MultiSite_Return_String.Substring(1, 1) == "1") ? true : false;
                            Device_In_TestSite_Exist[2] = (SOT_MultiSite_Return_String.Substring(2, 1) == "1") ? true : false;
                            Device_In_TestSite_Exist[3] = (SOT_MultiSite_Return_String.Substring(3, 1) == "1") ? true : false;
                        }
                        else // If wrong string length then consider it is error and set all the site existance to false
                        {
                            Device_In_TestSite_Exist[0] = false;
                            Device_In_TestSite_Exist[1] = false;
                            Device_In_TestSite_Exist[2] = false;
                            Device_In_TestSite_Exist[3] = false;
                        }

                        #endregion Check for SOT ( indication avaiability of new unit in the contactor )

                        #region Nothing

                        //// [T018] : Any more unit in the handler?
                        //if (FLAG_Still_Have_Unit_In_Handler == true)
                        //{
                        //    // Back to [T007] node
                        //    // Will go back thru the loop
                        //}
                        //else
                        //{
                        //    // [T019] : Notify Operator for incomplete operation due to lack of units in the handler
                        //    if (MessageBox.Show("Issue : No more units to continue the process, please load in more units into bowl/tray", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        //    {
                        //        // [T017] : Check if the operator loading in new fresh units
                        //        // Check from Handler that the new units already loaded in
                        //        Flag_Handler_Response_Still_Have_Unit_In_Handler = true; // must put in handler communication later
                        //    }
                        //    else
                        //    {
                        //        // If operator press cancel than that mean operator wish to terminate remaining process steps
                        //        // [T045] : Set Calibration Valid Flag to incomplete ( no production can start until valid calibration completed)
                        //        FLAG_CalibrationIncomplete = true;
                        //    }
                        //}

                        #endregion Nothing
                    }
                }

                #endregion Main SOLT and GU Calibration Loop
            }
            else
            {
                #region Query for SOLT and GU mode validation
                if (MessageBox.Show("Perform SOLT Validation?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Flag_SOLT_Validation_Enable = true;
                }
                else
                {
                    Flag_SOLT_Validation_Enable = false;
                }

                if (MessageBox.Show("Perform GU Validation?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Flag_GU_Validation_Enable = true;
                }
                else
                {
                    Flag_GU_Validation_Enable = false;
                }

                if (Flag_SOLT_Validation_Enable == false && Flag_GU_Validation_Enable == false)
                {
                    MessageBox.Show("Calibration and Validation process incomplete !!!!", "Warning", MessageBoxButtons.OK);

                    FLAG_User_NotQuiting = true;

                    //return;
                }
                #endregion  Query for SOLT and GU mode validation
            }

            // ************************************************
            // Any thing below will be validation process steps
            // ************************************************

            // Initialize counters
            SOLT_Pass_Validation_Counter = 0;
            GU_Pass_Validation_Counter = 0;

            FLAG_User_NotQuiting = false; // Remark for debuging, bypass validation process step below at this time

            if (FLAG_User_NotQuiting == true)
            {
                #region Check for SOT ( indication avaiability of new unit in the contactor )

                // Scan for SOT with in the timeout time frame
                Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;

                // Notify user to put in units if there is no unit in the handler
                while (FLAG_Still_Have_Unit_In_Handler == false && FLAG_User_NotQuiting == true)
                {
                    if (MessageBox.Show("Please put in units into handler and press OK button, else to quit press Cancel button?", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                        FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;
                    }
                    else
                    {
                        // [T045] : Set Calibration Valid Flag to incomplete ( no production can start until valid calibration completed)
                        FLAG_CalibrationIncomplete = true;
                        FLAG_User_NotQuiting = false; // Set Flag to quit the whole process
                    }
                }

                #endregion
            }

            #region Main SOLT and GU Validation loop
            while (FLAG_Still_Have_Unit_In_Handler == true && FLAG_User_NotQuiting == true)
            {
                if (Flag_RePunch_Cycle == false) // Do not perform new unit loading if re-punch cycle still in place
                {
                    // [T028] : Load in next Calibration Substrate
                    // Do nothing here as the unit already in the contactor

                    byte TestSiteNumber = Convert.ToByte(1); // Temp value, will need to change later

                    // [T008??] : Read in Calibration / Validation Substarte ID
                    //Calibration_Validation_Substrate_ID = myMfgLotNum.Read(TestSiteNumber).ToString();

                    // Extract out more information regarding the substrate in the contactor base on its ID
                 //   Extracted_Info_From_ID = Extract_Information_From_Substarte_ID(Calibration_Validation_Substrate_ID);
                    // Flag_ValidValidation_Unit_SOLT??
                    if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION")
                    {
                        Flag_ValidValidation_Unit_SOLT = true;
                    }
                    else
                    {
                        Flag_ValidValidation_Unit_SOLT = false;
                    }
                    // Flag_ValidValidation_Unit_GU??
                    if (Extracted_Info_From_ID.Substrate_Type == "GU_VALIDATION")
                    {
                        Flag_ValidValidation_Unit_GU = true;
                    }
                    else
                    {
                        Flag_ValidValidation_Unit_GU = false;
                    }
                    // Initialize repunch counter once new unit is in
                    SOLT_RePunch_Validation_Counter = 0;
                    GU_RePunch_Validation_Counter = 0;
                }

                Flag_RePunch_Cycle = false; // Reset Flag to false

                // [T029] : Is this a valid validation unit ( based on what in the database and based on filter list )
                if (Flag_ValidValidation_Unit_SOLT == true || Flag_ValidValidation_Unit_GU == true)
                {
                    Flag_ValidValidation_Unit = true;
                }
                else
                {
                    Flag_ValidValidation_Unit = false;
                }
                if (Flag_ValidValidation_Unit == true)
                {
                    // [T030] : Set Switch Matrix Box Path base on channel setting
                    // Loop through all the combination available in the validation file

                    // [T031] : Compare measured after correction with actual data (both SOLT and GU), save comparison results to datalog file

                    // [T032] : Is it within the set limit ? Pass or Fail the limit
                    if (Flag_Validation_Result_Pass == true) // Pass validation
                    {
                        // [T037] : Is it already the Nth for SOLT and Yth for GU valication units?
                        if (SOLT_Pass_Validation_Counter >= SOLT_Pass_Counter_User_Input)
                        {
                            FLAG_SOLT_Validation_Process_Completed = true;
                        }
                        if (GU_Pass_Validation_Counter >= GU_Pass_Counter_User_Input)
                        {
                            FLAG_GU_Validation_Process_Completed = true;
                        }
                        if (FLAG_SOLT_Validation_Process_Completed == true && FLAG_GU_Validation_Process_Completed == true)
                        {
                            // [T038] : Save remaining details to datalog file

                            // [T039] : Set Calibration Valid FLAG to True to allow program to start production run

                            FLAG_User_NotQuiting = false;

                            Save_FBAR_State(); // *** Note to save the calibartion state with the new uploaded Eterm back to the NA State

                            // Should stop here ...
                        }
                        else
                        {
                            // Increment the counter for SOLT or GU validation unit
                            if (Flag_ValidValidation_Unit_SOLT == true)
                            {
                                SOLT_Pass_Validation_Counter++;
                            }
                            if (Flag_ValidValidation_Unit_GU == true)
                            {
                                GU_Pass_Validation_Counter++;
                            }

                            // Do nothing as after this will flow back to load in another units
                        }
                    }
                    else  // Fail validation
                    {
                        // [T033] : Is the unit already exceeded X# for SOLT and K# for GU of punch?
                        if (SOLT_RePunch_Validation_Counter < SOLT_RePunch_Counter_User_Input || GU_RePunch_Validation_Counter < GU_RePunch_Counter_User_Input)
                        {
                            // [T036] : Re-punch validation unit
                            // Send command to handler for re-punch unit
                            Flag_RePunch_Cycle = true;

                            // Increment the counter for SOLT or GU repunch validation unit
                            if (Flag_ValidValidation_Unit_SOLT == true)
                            {
                                SOLT_RePunch_Validation_Counter++;
                            }
                            if (Flag_ValidValidation_Unit_GU == true)
                            {
                                GU_RePunch_Validation_Counter++;
                            }
                        }
                        else
                        {
                            FLAG_User_Wish_To_Move_To_Next_Unit = false; // Initialize the Flag to false first

                            // [T034] : Notify operator for next action
                            if (MessageBox.Show("Validation unit still failing after N punches, move to next unit?", "Action needed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                FLAG_User_Wish_To_Move_To_Next_Unit = true;
                            }

                            // [T035] : Operator decide to move to next unit? Yes or No
                            if (FLAG_User_Wish_To_Move_To_Next_Unit == true)
                            {
                                Flag_RePunch_Cycle = false; // Make sure no more re-punch but instead move to new unit
                            }
                            else
                            {
                                // [T045] : Set Calibration Valid Flag to incomplete ( no production can start until valid calibration completed)
                                FLAG_CalibrationIncomplete = true;
                                FLAG_User_NotQuiting = false; // Quit the while process loop
                            }
                        }
                    }
                }
                else // If not the valid validation unit in the contactor
                {
                    // Do nothing
                }

                #region Perform Handler binning

                // EOT and do the binning, put under no repunch loop once re-punch code ready...
                if (Extracted_Info_From_ID.Substrate_Type == "SOLT_CAL")
                {
                    //Eq.Handler.TrayMapEOT(SOLT_CAL_Bin.ToString());
                }
                if (Extracted_Info_From_ID.Substrate_Type == "GU_CAL")
                {
                    //Eq.Handler.TrayMapEOT(GU_CAL_Bin.ToString());
                }
                if (Extracted_Info_From_ID.Substrate_Type == "SOLT_VALIDATION")
                {
                    //Eq.Handler.TrayMapEOT(SOLT_VAL_Bin.ToString());
                }
                if (Extracted_Info_From_ID.Substrate_Type == "GU_VALIDATION")
                {
                    //Eq.Handler.TrayMapEOT(GU_VAL_Bin.ToString());
                }
                if (Extracted_Info_From_ID.Substrate_Type != "SOLT_CAL" && Extracted_Info_From_ID.Substrate_Type != "SOLT_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_VALIDATION" && Extracted_Info_From_ID.Substrate_Type != "GU_CAL")
                {
                    //Eq.Handler.TrayMapEOT(UnKnown_Bin.ToString());
                }

                #endregion

                // Only perform if not unit re-punch cycle
                if (Flag_RePunch_Cycle == false)
                {

                    #region Check for SOT ( indication avaiability of new unit in the contactor )

                    // Scan for SOT with in the timeout time frame
                    Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                    FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;

                    // Notify user to put in units if there is no unit in the handler
                    while (FLAG_Still_Have_Unit_In_Handler == false && FLAG_User_NotQuiting == true)
                    {
                        if (MessageBox.Show("Please put in units into handler and press OK button, else to quit press Cancel button?", "Action needed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            Flag_Handler_Response_Still_Have_Unit_In_Handler = Check_For_SOT_Signal(20);
                            FLAG_Still_Have_Unit_In_Handler = Flag_Handler_Response_Still_Have_Unit_In_Handler;
                        }
                        else
                        {
                            // If operator press cancel than that mean operator wish to terminate remaining process steps
                            // [T045] : Set Calibration Valid Flag to incomplete ( no production can start until valid calibration completed)
                            FLAG_CalibrationIncomplete = true;
                            FLAG_User_NotQuiting = false; // Set Flag to quit the process
                        }
                    }

                    #endregion

                }
                else
                {
                    // Send re-punch command to Handler
                }
            }
            #endregion

            // [T043] : Purge all remaining DUTs

        }
        
        private static bool Trigger_Next_Standard_Or_Validation_Units_In_XYCoord_Tray(bool Reset_Coordinate, bool Standard_Unit_Pick, bool Validation_Unit_Pick, string SiteActive)
        {
            // Notes :
            // 1. For Maximator total PCB Cal Sub use will be 49 pcs including Open, Short, Load
            // 2. Please position the 49 cal set with in the 7 by 7 matrix as below in the tray
            // 3. As the actual PCB identification will be done by reading in OTP ID, then the end user do not have to position the units in specific sequence
            //
            //    [E] => Empty slot in tray
            //    [C] => PCB Cal exist in the tray slot
            //    [V] => Validation unit (Please use Open Unit Variant#7)
            //
            //           1   2   3   4   5   6   7   8   9   10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28
            //        14 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //        13 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //        12 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //        11 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //        10 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         9 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         8 [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         7 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         6 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         5 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         4 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         3 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         2 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]
            //         1 [C] [C] [C] [C] [C] [C] [C] [V] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E] [E]

            // 4. Once the tray move to the last possible position, if no reset flag send then the next will go back the first pick position as if it already auto-reset
            //    This hopefully help to reduce human intervention if the pick up head fail to pick the unit on the previous try...

            int Calibration_Substrate_SiteA_Temp_X_Coor=0;
            int Calibration_Substrate_SiteA_Temp_Y_Coor=0;
            int Calibration_Substrate_SiteB_Temp_X_Coor=0;
            int Calibration_Substrate_SiteB_Temp_Y_Coor=0;

            bool Status_Return = false;

            if (Reset_Coordinate == true) // Reset all the Coordinate
            {
                if (Standard_Unit_Pick == true)
                {
                    Calibration_Substrate_Site1_X_Coor = 1; // Reset position for standard at "1,1;2,1;3,1;4,1"
                    Calibration_Substrate_Site1_Y_Coor = 1;
                    Calibration_Substrate_Site2_X_Coor = 2;
                    Calibration_Substrate_Site2_Y_Coor = 1;
                    Calibration_Substrate_Site3_X_Coor = 3;
                    Calibration_Substrate_Site3_Y_Coor = 1;
                    Calibration_Substrate_Site4_X_Coor = 4;
                    Calibration_Substrate_Site4_Y_Coor = 1;
                    Status_Return = true;
                }
                if (Validation_Unit_Pick == true)
                {
                    Validation_Substrate_Site1_X_Coor = 1; // Reset position for standard at "1,8;2,8;3,8;4,8"
                    Validation_Substrate_Site1_Y_Coor = 8;
                    Validation_Substrate_Site2_X_Coor = 2;
                    Validation_Substrate_Site2_Y_Coor = 8;
                    Validation_Substrate_Site3_X_Coor = 3;
                    Validation_Substrate_Site3_Y_Coor = 8;
                    Validation_Substrate_Site4_X_Coor = 4;
                    Validation_Substrate_Site4_Y_Coor = 8;
                    Status_Return = true;
                }
            }
            else // Increment by 1 step for the next pick in the list
            {
                if (Standard_Unit_Pick == true)
                {
                    // TestSite #1
                    #region TestSite1 move
                    Calibration_Substrate_Site1_X_Coor = Calibration_Substrate_Site1_X_Coor + 1; // Move by one step
                    if (Calibration_Substrate_Site1_X_Coor > 7)
                    {
                        Calibration_Substrate_Site1_X_Coor = 1;
                        Calibration_Substrate_Site1_Y_Coor = Calibration_Substrate_Site1_Y_Coor + 1;
                    }
                    if (Calibration_Substrate_Site1_Y_Coor > 7) // Reset beck to first position
                    {
                        Calibration_Substrate_Site1_X_Coor = 1;
                        Calibration_Substrate_Site1_Y_Coor = 1;
                    }
                    #endregion TestSite1 move

                    // TestSite #2
                    #region TestSite2 move
                    Calibration_Substrate_Site2_X_Coor = Calibration_Substrate_Site2_X_Coor + 1; // Move by one step
                    if (Calibration_Substrate_Site2_X_Coor > 7)
                    {
                        Calibration_Substrate_Site2_X_Coor = 1;
                        Calibration_Substrate_Site2_Y_Coor = Calibration_Substrate_Site2_Y_Coor + 1;
                    }
                    if (Calibration_Substrate_Site2_Y_Coor > 7) // Reset beck to first position
                    {
                        Calibration_Substrate_Site2_X_Coor = 1;
                        Calibration_Substrate_Site2_Y_Coor = 1;
                    }
                    #endregion TestSite2 move

                    // TestSite #3
                    #region TestSite3 move
                    Calibration_Substrate_Site3_X_Coor = Calibration_Substrate_Site3_X_Coor + 1; // Move by one step
                    if (Calibration_Substrate_Site3_X_Coor > 7)
                    {
                        Calibration_Substrate_Site3_X_Coor = 1;
                        Calibration_Substrate_Site3_Y_Coor = Calibration_Substrate_Site3_Y_Coor + 1;
                    }
                    if (Calibration_Substrate_Site3_Y_Coor > 7) // Reset beck to first position
                    {
                        Calibration_Substrate_Site3_X_Coor = 1;
                        Calibration_Substrate_Site3_Y_Coor = 1;
                    }
                    #endregion TestSite3 move

                    // TestSite #4
                    #region TestSite4 move
                    Calibration_Substrate_Site4_X_Coor = Calibration_Substrate_Site4_X_Coor + 1; // Move by one step
                    if (Calibration_Substrate_Site4_X_Coor > 7)
                    {
                        Calibration_Substrate_Site4_X_Coor = 1;
                        Calibration_Substrate_Site4_Y_Coor = Calibration_Substrate_Site4_Y_Coor + 1;
                    }
                    if (Calibration_Substrate_Site4_Y_Coor > 7) // Reset beck to first position
                    {
                        Calibration_Substrate_Site4_X_Coor = 1;
                        Calibration_Substrate_Site4_Y_Coor = 1;
                    }
                    #endregion TestSite4 move

                    Status_Return = true;
                }
                if (Validation_Unit_Pick == true)
                {
                    // TestSite #1
                    #region TestSite1 move
                    Validation_Substrate_Site1_X_Coor = Validation_Substrate_Site1_X_Coor + 1; // Move by one step
                    if (Validation_Substrate_Site1_X_Coor > 7) Validation_Substrate_Site1_X_Coor = 1; // Reset back to one if exceed the end limit
                    #endregion TestSite1 move

                    // TestSite #2
                    #region TestSite2 move
                    Validation_Substrate_Site2_X_Coor = Validation_Substrate_Site2_X_Coor + 1; // Move by one step
                    if (Validation_Substrate_Site2_X_Coor > 7) Validation_Substrate_Site2_X_Coor = 1; // Reset back to one if exceed the end limit
                    #endregion TestSite2 move

                    // TestSite #3
                    #region TestSite3 move
                    Validation_Substrate_Site3_X_Coor = Validation_Substrate_Site3_X_Coor + 1; // Move by one step
                    if (Validation_Substrate_Site3_X_Coor > 7) Validation_Substrate_Site3_X_Coor = 1; // Reset back to one if exceed the end limit
                    #endregion TestSite3 move

                    // TestSite #4
                    #region TestSite4 move
                    Validation_Substrate_Site4_X_Coor = Validation_Substrate_Site4_X_Coor + 1; // Move by one step
                    if (Validation_Substrate_Site4_X_Coor > 7) Validation_Substrate_Site4_X_Coor = 1; // Reset back to one if exceed the end limit
                    #endregion TestSite4 move

                    Status_Return = true;
                }
            }

            string TrayMapCoordinateToSend = "";

            // Note that if SiteActive not "1-2" and "3-4", then this will be consider as full quad site calibration
            if (SiteActive == "1-2")
            {
                if (Standard_Unit_Pick == true)
                {
                    Calibration_Substrate_SiteA_Temp_X_Coor = Calibration_Substrate_Site3_X_Coor;
                    Calibration_Substrate_SiteA_Temp_Y_Coor = Calibration_Substrate_Site3_Y_Coor;
                    Calibration_Substrate_SiteB_Temp_X_Coor = Calibration_Substrate_Site4_X_Coor;
                    Calibration_Substrate_SiteB_Temp_Y_Coor = Calibration_Substrate_Site4_Y_Coor;

                    Calibration_Substrate_Site3_X_Coor = -1;
                    Calibration_Substrate_Site3_Y_Coor = -1;
                    Calibration_Substrate_Site4_X_Coor = -1;
                    Calibration_Substrate_Site4_Y_Coor = -1;
                }
                if (Validation_Unit_Pick == true)
                {
                    Calibration_Substrate_SiteA_Temp_X_Coor = Validation_Substrate_Site3_X_Coor;
                    Calibration_Substrate_SiteA_Temp_Y_Coor = Validation_Substrate_Site3_Y_Coor;
                    Calibration_Substrate_SiteB_Temp_X_Coor = Validation_Substrate_Site4_X_Coor;
                    Calibration_Substrate_SiteB_Temp_Y_Coor = Validation_Substrate_Site4_Y_Coor;

                    Validation_Substrate_Site3_X_Coor = -1;
                    Validation_Substrate_Site3_Y_Coor = -1;
                    Validation_Substrate_Site4_X_Coor = -1;
                    Validation_Substrate_Site4_Y_Coor = -1;
                }

            }
            if (SiteActive == "3-4")
            {
                if (Standard_Unit_Pick == true)
                {
                    Calibration_Substrate_SiteA_Temp_X_Coor = Calibration_Substrate_Site1_X_Coor;
                    Calibration_Substrate_SiteA_Temp_Y_Coor = Calibration_Substrate_Site1_Y_Coor;
                    Calibration_Substrate_SiteB_Temp_X_Coor = Calibration_Substrate_Site2_X_Coor;
                    Calibration_Substrate_SiteB_Temp_Y_Coor = Calibration_Substrate_Site2_Y_Coor;

                    Calibration_Substrate_Site1_X_Coor = -1;
                    Calibration_Substrate_Site1_Y_Coor = -1;
                    Calibration_Substrate_Site2_X_Coor = -1;
                    Calibration_Substrate_Site2_Y_Coor = -1;
                }
                if (Validation_Unit_Pick == true)
                {
                    Calibration_Substrate_SiteA_Temp_X_Coor = Validation_Substrate_Site1_X_Coor;
                    Calibration_Substrate_SiteA_Temp_Y_Coor = Validation_Substrate_Site1_Y_Coor;
                    Calibration_Substrate_SiteB_Temp_X_Coor = Validation_Substrate_Site2_X_Coor;
                    Calibration_Substrate_SiteB_Temp_Y_Coor = Validation_Substrate_Site2_Y_Coor;

                    Validation_Substrate_Site1_X_Coor = -1;
                    Validation_Substrate_Site1_Y_Coor = -1;
                    Validation_Substrate_Site2_X_Coor = -1;
                    Validation_Substrate_Site2_Y_Coor = -1;
                }
            }

            // Send pick from tray coordinate command base on the above coordinate process results
            if (Standard_Unit_Pick == true)
            {
                TrayMapCoordinateToSend = Calibration_Substrate_Site1_X_Coor.ToString() + "," + Calibration_Substrate_Site1_Y_Coor.ToString() + ";" +
                          Calibration_Substrate_Site2_X_Coor.ToString() + "," + Calibration_Substrate_Site2_Y_Coor.ToString() + ";" +
                          Calibration_Substrate_Site3_X_Coor.ToString() + "," + Calibration_Substrate_Site3_Y_Coor.ToString() + ";" +
                          Calibration_Substrate_Site4_X_Coor.ToString() + "," + Calibration_Substrate_Site4_Y_Coor.ToString() + ";";
            }
            if (Validation_Unit_Pick == true)
            {
                TrayMapCoordinateToSend = Validation_Substrate_Site1_X_Coor.ToString() + "," + Validation_Substrate_Site1_Y_Coor.ToString() + ";" +
                          Validation_Substrate_Site2_X_Coor.ToString() + "," + Validation_Substrate_Site2_Y_Coor.ToString() + ";" +
                          Validation_Substrate_Site3_X_Coor.ToString() + "," + Validation_Substrate_Site3_Y_Coor.ToString() + ";" +
                          Validation_Substrate_Site4_X_Coor.ToString() + "," + Validation_Substrate_Site4_Y_Coor.ToString() + ";";
            }

            if (SiteActive == "1-2")
            {
                if (Standard_Unit_Pick == true)
                {
                    Calibration_Substrate_Site3_X_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                    Calibration_Substrate_Site3_Y_Coor = Calibration_Substrate_SiteA_Temp_Y_Coor;
                    Calibration_Substrate_Site4_X_Coor = Calibration_Substrate_SiteB_Temp_X_Coor;
                    Calibration_Substrate_Site4_Y_Coor = Calibration_Substrate_SiteB_Temp_Y_Coor;
                }
                if (Validation_Unit_Pick == true)
                {
                    Validation_Substrate_Site3_X_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                    Validation_Substrate_Site3_Y_Coor = Calibration_Substrate_SiteA_Temp_Y_Coor;
                    Validation_Substrate_Site4_X_Coor = Calibration_Substrate_SiteB_Temp_X_Coor;
                    Validation_Substrate_Site4_Y_Coor = Calibration_Substrate_SiteB_Temp_Y_Coor;
                }

            }
            if (SiteActive == "3-4")
            {
                if (Standard_Unit_Pick == true)
                {
                    Calibration_Substrate_Site1_X_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                    Calibration_Substrate_Site1_Y_Coor = Calibration_Substrate_SiteA_Temp_Y_Coor;
                    Calibration_Substrate_Site2_X_Coor = Calibration_Substrate_SiteB_Temp_X_Coor;
                    Calibration_Substrate_Site2_Y_Coor = Calibration_Substrate_SiteB_Temp_Y_Coor;
                }
                if (Validation_Unit_Pick == true)
                {
                    Validation_Substrate_Site1_X_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                    Validation_Substrate_Site1_Y_Coor = Calibration_Substrate_SiteA_Temp_Y_Coor;
                    Validation_Substrate_Site2_X_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                    Validation_Substrate_Site2_Y_Coor = Calibration_Substrate_SiteA_Temp_X_Coor;
                }

            }

            Eq.Handler.TrayMapCoord(TrayMapCoordinateToSend); // Sample string => "1,1;2,1;3,1;4,1;"

            Console.WriteLine("Running => Eq.Handler.TrayMapCoord(TrayMapCoordinateToSend)");
            Console.WriteLine("["+TrayMapCoordinateToSend+"]");

            return (Status_Return);
        }

        //private static bool Check_For_SOT_Signal(EqLib.HandlerExistechNI6509SwitchMatrixSmpadV2 bHandler_Temp, int CounterLoop_Value)
        private static bool Check_For_SOT_Signal(int CounterLoop_Value)
        {
            /*
            bool Handler_Result;
            
            Handler_Result = false;

            // Scan for SOT with in the timeout time frame
            int Handler_Test_Counter = 0;
            while (Handler_Test_Counter <= CounterLoop_Value)
            {
                Thread.Sleep(500); // Delay
                //Handler_Result = bHandler_Temp.Check_Handler_SOT(); // Read SOT
                Handler_Result = Eq.Handler.CheckSRQStatusByte(72); // Read Synax SOT
                if (Handler_Result == false) break; // False indicating SOT received ???
                Handler_Test_Counter = Handler_Test_Counter + 1;
            }
            
            return (Handler_Result);
            */

            if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
            Console.WriteLine("Running => Eq.Handler.CheckSRQStatusByte(72)");
            return (true);

        }

        private static Substarte_ID_Extract_Information Extract_Information_From_Substarte_ID(string ID_Code_String)
        {
            string tempFileName;

            Substarte_ID_Extract_Information tempReturn = new Substarte_ID_Extract_Information();

            string[] tmp_string;

            // Note : For now will only capture the last 2 digit which represent variant number
            tempFileName = "C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\SubCode_Maximator_0310" + ID_Code_String.Substring(ID_Code_String.Length - 2) + ".txt"; // Just capture the the laest 2 digits which is the Variant#

            tempReturn.Substrate_Type = "NONE";

            if (File.Exists(tempFileName))
            {
                tmp_string = ReadFromFile(tempFileName);

                // Read in the header which contain the detail information of the substrate
                tempReturn.Header_String = tmp_string[0].ToUpper().Trim();

                // Initial value set to "NONE"
                tempReturn.Substrate_Type = "NONE";

                // Read in and interprate header
                if (tmp_string[0].ToUpper().Contains("SOLTCAL"))
                {
                    tempReturn.Substrate_Type = "SOLT_CAL";
                }
                if (tmp_string[0].ToUpper().Contains("GUCAL"))
                {
                    tempReturn.Substrate_Type = "GU_CAL";
                }
                if (tmp_string[0].ToUpper().Contains("SOLTVAL"))
                {
                    tempReturn.Substrate_Type = "SOLT_VALIDATION";
                }
                if (tmp_string[0].ToUpper().Contains("GUVAL"))
                {
                    tempReturn.Substrate_Type = "GU_VALIDATION";
                }

                // Transfer remaining file content
                tempReturn.Remaining_File_Content = tmp_string;

            }

            return (tempReturn);
        }

        private static string[] ReadFromFile(string FilenamePath)
        {
            string[] tmp_read_string = File.ReadAllLines(FilenamePath);
            return (tmp_read_string);
        }

        // [Burhan] : Added for Topaz
        #region AutoCal Codes

        private static void Initialize_NA_Channel_Before_Autocal(int Channel_Number, int CalKit_Number, int Port1, int Port2, int Port3)
        {
            //Eq_ENA.Active_ENA.Write_Topaz("MMEM:LOAD \"" + "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site.csa".Trim() + "\"");

            if (Port3 == 0) CalKit_Number = CalKit_Number + 1; // 2 Port Cal Setting at 1 step increment number

            Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:SEL " + CalKit_Number.ToString());

            if (Port3 == 0) // Manage 2Port correction
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 1");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Open.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 2");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Open.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 3");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Short.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 4");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Short.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 5");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Load.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 6");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Load.cti'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL 7");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Thru.cti'");

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port1.ToString() + " 'A'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port1.ToString() + " 'AutoCal 2Port'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port2.ToString() + " 'B'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port2.ToString() + " 'AutoCal 2Port'");
            }
            else // Manage 3 port correction
            {
                for (int i = 1; i < 3 + 1; i++)
                {
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Open.cti'");
                }

                for (int i = 4; i < 6 + 1; i++)
                {
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Short.cti'");
                }

                for (int i = 7; i < 9 + 1; i++)
                {
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Load.cti'");
                }

                for (int i = 10; i < 12 + 1; i++)
                {
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\Dummy_Thru.cti'");
                }

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port1.ToString() + " 'A'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port1.ToString() + " 'AutoCal 3Port'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port2.ToString() + " 'B'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port2.ToString() + " 'AutoCal 3Port'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port3.ToString() + " 'C'");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port3.ToString() + " 'AutoCal 3Port'");
            }

            if (Port3 != 0)
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString() + "," + Port1.ToString() + "," + Port3.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
            }
            else
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString());
            }

            Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:PATH:TMET " + Port1.ToString() + "," + Port2.ToString() + ", \"Undefined Thru\"");
            if (Port3 != 0)
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:PATH:TMET " + Port1.ToString() + "," + Port3.ToString() + ", \"Undefined Thru\"");
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:PATH:TMET " + Port2.ToString() + "," + Port3.ToString() + ", \"Undefined Thru\"");
            }

            Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:INIT");

            //int CalStep = Convert.ToInt32(ENA.BasicCommand.ReadCommand("SENS:CORR:COLL:GUID:STEPS?"));
            int CalStep = Convert.ToInt32(Eq_ENA.Active_ENA.Read_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:STEPS?"));

            if (Port3 == 0) // 2 Port
            {
                if (!(CalStep == 7))
                {
                    System.Windows.Forms.MessageBox.Show("Please check Channel setting, possible issue!!!");
                }
            }
            else // 3 Port
            {
                if (!(CalStep == 12))
                {
                    System.Windows.Forms.MessageBox.Show("Please check Channel setting, possible issue!!!");
                }
            }
        }

        private static void Trigger_Acquire_Data_Response(int Channel_Number, int TraceNumber, int TopazSingleTraceNumber)
        {
            //return;  // For debugging only

            ATFLogControl logger = ATFLogControl.Instance;  // for writing to Clotho Logger

            bool verifyPass = true;
            int NOP = Convert.ToInt32(Eq_ENA.Active_ENA.Read_Topaz("SENSe" + Channel_Number .ToString()+ ":SWEep:POINts?"));
            double[] compData = new double[NOP]; // ??
            int loopCount = 0;
            string TrFormat = "";
            double VerLimitLow = 0,
                   VerLimitHigh = 0,
                   maxVal = 0,
                   minVal = 0;

            //SENSe<cnum>:SEGMent<snum>:SWEep:POINts?
            //SENSe<cnum>:SWEep:POINts?

            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
            {
                verifyPass = false;

                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":PAR:MNUM:SEL " + TraceNumber.ToString());

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:METH RESP");

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:ACQ RESP"); // Updated to fix the Response cal bug

                Thread.Sleep(100);

                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:SAVE");

                #region verification response cal

                Thread.Sleep(100);

                //ENA.Format.DATA(e_FormatData.REAL);
                Eq_ENA.Active_ENA.Write_Topaz("FORM:DATA " + "REAL,64");

                //TrFormat = ENA.Calculate.Format.returnFormat(Channel_Number, TraceNumber);
                TrFormat = Eq_ENA.Active_ENA.Read_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM?");     

                //ENA.Calculate.Format.setFormat(Channel_Number, TraceNumber, e_SFormat.MLOG);
                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + "MLOG"); // e_SFormat.MLOG = 0

                Eq_ENA.Active_ENA.Write_Topaz("INIT" + Channel_Number.ToString() + ":IMM");
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");
                
                //compData = ENA.Calculate.Data.FData(Channel_Number, TraceNumber);
                compData = Eq_ENA.Active_ENA.ReadIEEEBlock("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":DATA:FDATA?");
                
                //ENA.BasicCommand.System.Operation_Complete();
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");
                
                Thread.Sleep(10);

                VerLimitLow = -0.1;
                VerLimitHigh = 0.1;

                maxVal = compData.Max();
                minVal = compData.Min();

                //Console.WriteLine(maxVal.ToString() + "," + minVal.ToString() + "," + compData.Average().ToString());
                Console.WriteLine(maxVal.ToString());
                Console.WriteLine(minVal.ToString());
                Console.WriteLine(compData.Average().ToString());

                // Added for debugging only, please remove later
                if (Debug_Without_Handler == true)
                {
                    minVal = 0;
                    maxVal = 0;
                }

                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                {
                    verifyPass = true;
                    
                    //Eq_ENA.Active_ENA.Write_Topaz("DISP:MEAS" + TraceNumber.ToString() + ":DEL");
                    
                    Topaz_Trace_Still_Available[TraceNumber-1] = false;
                }

                loopCount++;

                //ENA.Calculate.Format.setFormat(Channel_Number, TraceNumber, TrFormat);
                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + TrFormat.Trim());

                #endregion verification response cal

            }

            //if (loopCount == 3) General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Verification results are out of limits. \n\nChannel: " + Channel_Number.ToString() + "\nParameter: " + TraceNumber.ToString() +
            //   "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);            
            
            // [Burhan 9June2016]
            if ((loopCount == 3) && (verifyPass == false)) Console.WriteLine("Error : Verification results for Thru Response Calibration are out of limits. \nChannel: " + Channel_Number.ToString() + " , Parameter: " + TraceNumber.ToString() + "\nLow Limit: " + VerLimitLow + " , High Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxVal + " , Measured Value (Min): " + minVal + "\n");
            if ((loopCount == 3) && (verifyPass == false)) logger.Log(Avago.ATF.LogService.LogLevel.Error, "Error : Verification results for Thru Response Calibration are out of limits. \nChannel: " + Channel_Number.ToString() + " , Parameter: " + TraceNumber.ToString() + "\nLow Limit: " + VerLimitLow + " , High Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxVal + " , Measured Value (Min): " + minVal + "\n");
            //if (loopCount == 3) logger.Log(Avago.ATF.LogService.LogLevel.Error, "Error : Verification results for Thru Response Calibration are out of limits. \nChannel: " + Channel_Number.ToString() + " , Parameter: " + TraceNumber.ToString() + "\nLow Limit: " + VerLimitLow + " , High Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxVal + " , Measured Value (Min): " + minVal + "\n");

        }

        private static bool Validate_Trace_Response_Data(int Channel_Number, int TraceNumber, double VerLimitLow, double VerLimitHigh)
        {
            //return (true); // for debugging only

            ATFLogControl logger = ATFLogControl.Instance;  // for writing to Clotho Logger

            bool verifyPass = true;
            int NOP = Convert.ToInt32(Eq_ENA.Active_ENA.Read_Topaz("SENSe" + Channel_Number.ToString() + ":SWEep:POINts?"));
            double[] compData = new double[NOP]; // ??
            int loopCount = 0;
            string TrFormat = "";
            //double VerLimitLow = 0,
            //       VerLimitHigh = 0,
            double maxVal = 0,
                   minVal = 0;

            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
            {
                verifyPass = false;
                
                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":PAR:MNUM:SEL " + TraceNumber.ToString());

                Thread.Sleep(100);

                //ENA.Format.DATA(e_FormatData.REAL);
                //Eq_ENA.Active_ENA.Write_Topaz("FORM:DATA " + "REAL,64");
                Eq_ENA.Active_ENA.Write_Topaz("FORM:DATA " + "REAL,64");

                //TrFormat = ENA.Calculate.Format.returnFormat(Channel_Number, TraceNumber);
                TrFormat = Eq_ENA.Active_ENA.Read_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM?");     

                //ENA.Calculate.Format.setFormat(Channel_Number, TraceNumber, e_SFormat.MLOG);
                //Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + "0"); // e_SFormat.MLOG = 0
                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + "MLOG"); // e_SFormat.MLOG = 0

                //ENA.Trigger.Single(Channel_Number);
                Eq_ENA.Active_ENA.Write_Topaz("INIT" + Channel_Number.ToString() + ":IMM");

                //ENA.BasicCommand.System.Operation_Complete();
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                //compData = ENA.Calculate.Data.FData(Channel_Number, TraceNumber);
                compData = Eq_ENA.Active_ENA.ReadIEEEBlock("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":DATA:FDATA?");

                //ENA.BasicCommand.System.Operation_Complete();
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");

                Thread.Sleep(10);

                //VerLimitLow = -0.3;
                //VerLimitHigh = 0.3;
                maxVal = compData.Max();
                minVal = compData.Min();

                Console.WriteLine(maxVal.ToString());
                Console.WriteLine(minVal.ToString());
                Console.WriteLine(compData.Average().ToString());

                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                {
                    verifyPass = true;
                }

                loopCount++;
                
                //ENA.Calculate.Format.setFormat(Channel_Number, TraceNumber, TrFormat);
                Eq_ENA.Active_ENA.Write_Topaz("CALC" + Channel_Number.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + TrFormat.Trim());

            }

            //if (loopCount == 3) General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Verification results are out of limits. \n\nChannel: " + Channel_Number.ToString() + "\nParameter: " + TraceNumber.ToString() +
            //   "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);

            if (loopCount == 3) logger.Log(Avago.ATF.LogService.LogLevel.Error, "Error : Verification results for Open Reflection Calibration are out of limits. \nChannel: " + Channel_Number.ToString() + " , Parameter: " + TraceNumber.ToString() + "\nLow Limit: " + VerLimitLow + " , High Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxVal + " , Measured Value (Min): " + minVal + "\n");

            return (verifyPass);
        }

        private static void Trigger_Acquire_Data(int Channel_Number, string Standard_Type, string PortConnection, int TwoPortFlag)
        {
            //return; // For debugging only

            int i = 0;

            if (TwoPortFlag == 0) // 2 Port Calibration Setting
            {
                switch (Standard_Type.ToUpper().Trim())
                {
                    case "OPEN":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 1;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 4;
                        break;
                    case "SHORT":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 2;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 5;
                        break;
                    case "LOAD":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 3;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 6;
                        break;
                    case "THRU":
                        if (PortConnection.ToUpper().Trim() == "P1P2") i = 7;
                        break;
                }
            }
            else // 2 Port Calibration Setting
            {
                switch (Standard_Type.ToUpper().Trim())
                {
                    case "OPEN":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 1;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 4;
                        if (PortConnection.ToUpper().Trim() == "P3") i = 8;
                        break;
                    case "SHORT":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 2;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 5;
                        if (PortConnection.ToUpper().Trim() == "P3") i = 9;
                        break;
                    case "LOAD":
                        if (PortConnection.ToUpper().Trim() == "P1") i = 3;
                        if (PortConnection.ToUpper().Trim() == "P2") i = 6;
                        if (PortConnection.ToUpper().Trim() == "P3") i = 10;
                        break;
                    case "THRU":
                        if (PortConnection.ToUpper().Trim() == "P1P2") i = 7;
                        if (PortConnection.ToUpper().Trim() == "P1P3") i = 11;
                        if (PortConnection.ToUpper().Trim() == "P2P3") i = 12;
                        break;
                }
            }

            if (i >= 1 && i <= 12)
            {
                //Thread.Sleep(1000);
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:COLL:GUID:ACQ STAN" + i.ToString());
                //ENA.BasicCommand.System.Operation_Complete();
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");
                //Thread.Sleep(2000);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Error during calibration, please check back your codes...");
            }
        }

        private static void ReApply_CalReg_To_Channel(int Channel_Number)
        {

            //return; // for debugging only

            //string CalSet_List = ENA.BasicCommand.ReadCommand("CSET:CAT?");

            //string Response_Input = ENA.BasicCommand.ReadCommand("CSET:EXISts? 'CH" + Channel_Number + "_CALREG'");
            string Response_Input = "1"; // ENA.BasicCommand.ReadCommand("CSET:EXISts? 'CH" + Channel_Number + "_CALREG'");

            //ENA.BasicCommand.ReadCommand("CSET:EXISts? 'CH20_CALREG'")
            int Response_Input_Int = Convert.ToInt32(Response_Input);

            if(Response_Input_Int==1)
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number.ToString() + ":CORR:CSET:ACT 'CH" + Channel_Number.ToString() + "_CALREG',1");
                Eq_ENA.Active_ENA.Read_Topaz("*OPC?");
            }
        }

        private static void Compute_Error_Term(int Channel_Number_CITI_Code,int Channel_Number_Actual, int NumberOfCalPort)
        {
            //return; // For debuging only

            // Replace CITI file with new files
            int i = 0;

            if (NumberOfCalPort == 2) // 2 Port Channel SCal setting
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:SEL 2");
                //Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:SEL 39");

                // Open
                i = 1;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Open_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 2;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Open_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Short
                i = 3;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Short_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 4;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Short_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Load
                i = 5;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Load_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 6;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Load_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Thru P1P2
                i = 7;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_THRU_P1P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

            }

            if (NumberOfCalPort == 3) // 3 Port Channel SCal setting
            {

                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:SEL 1");
                //Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:SEL 38");

                // Open
                i = 1;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Open_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 2;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Open_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 3;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Open_P3_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Short
                i = 4;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Short_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 5;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Short_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 6;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Short_P3_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Load
                i = 7;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Load_P1_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 8;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Load_P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 9;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_Load_P3_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");

                // Thru P1P2
                i = 10;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_THRU_P1P2_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 11;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_THRU_P1P3_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
                i = 12;
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:SEL " + i.ToString());
                Eq_ENA.Active_ENA.Write_Topaz("SENS" + Channel_Number_Actual.ToString() + ":CORR:COLL:CKIT:STAN:DATA:LOAD 'C:\\Avago.ATF.Common\\PCB_Base_AutoCal\\Maximator\\CITI\\Maximator_THRU_P2P3_CH" + Channel_Number_CITI_Code.ToString() + ".cti'");
            }

            // Re-compute Eterm
            Eq_ENA.Active_ENA.Write_Topaz("SENSe" + Channel_Number_Actual.ToString() + ":CORRection:COLLect:GUIDed:ETERms:COMPute");
        }

        // [Burhan] : Added for PCB CalSub AutoCal with CalKit Method
        private static void Initialize_NA_Channel_Before_Autocal_For_CalKit_Method()
        {
                 //e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                 //bool[] AnalysisEnable = new bool[TotalChannel];
                 //int[] step = new int[TotalChannel];

                 // Remark for now as the input variables not exist in the current environment // Values reside in TCF File
                 /*
                 for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                 {
                     if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                     {
                         ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                         ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                     }
                     if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12))
                     {
                         ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                         if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                         {
                             ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                             ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                             ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                         }
                     }
                     else if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7))
                     {
                         if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                             ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                     }
                 }

                 for (int iChn = 0; iChn < TotalChannel; iChn++)
                 {
                     ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                 }
                 */
        }

        // [Burhan] : Added for PCB CalSub AutoCal with CalKit Method
        private static void Finalize_NA_Channel_After_Autocal_For_CalKit_Method()
        {
            // Remark for now as the input variables not exist in the current environment // Values reside in TCF File
            /*
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                    Eq_ENA.Active_ENA.Write_Topaz("SENS" + (iChn + 1).ToString() + ":CORR:COLL:GUID:SAVE:IMM");
            }
            */
        }

        private static void Load_Calibration()
        {
            // ??? Dummy // Values reside in TCF File
        }

        private static void Save_FBAR_State()
        {
            // !!!! Please link to the actual Topaz state file later in actual implementation
            string NA_StateFile = "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site_burhan_CalOnA.csa";
            //string NA_StateFile = "C:\\Users\\Public\\Documents\\Network Analyzer\\Maximator_Temp_State_File.csa";

            // "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_quad_site.csa"

            //NA_StateFile = "C:\\Users\\Public\\Documents\\Network Analyzer\\MAXIMATOR_REV02p1_6P_dual_site.csa".Trim(); // ??

            if (NA_StateFile != "" && NA_StateFile != null)
            {
                Eq_ENA.Active_ENA.Write_Topaz("MMEM:STOR \"" + NA_StateFile.Trim() + "\"");
            }
        }

        // [Burhan] End of codes

        // Burhan [Added] for SJ setup
        private static int Actual_Trace_Number(int Site_Number, int Primary_Trace_Number)
        {
            int Final_Result = 0;

            // Best and most universal now is to use table form for all setting
            if (Site_Number == 0)
            {
                #region Site_0_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 1;
                else if (Primary_Trace_Number == 2) Final_Result = 2;
                else if (Primary_Trace_Number == 3) Final_Result = 3;
                else if (Primary_Trace_Number == 4) Final_Result = 4;
                else if (Primary_Trace_Number == 5) Final_Result = 5;
                else if (Primary_Trace_Number == 6) Final_Result = 6;
                else if (Primary_Trace_Number == 7) Final_Result = 7;
                else if (Primary_Trace_Number == 8) Final_Result = 8;
                else if (Primary_Trace_Number == 9) Final_Result = 9;
                else if (Primary_Trace_Number == 10) Final_Result = 10;
                else if (Primary_Trace_Number == 11) Final_Result = 21;
                else if (Primary_Trace_Number == 12) Final_Result = 22;
                else if (Primary_Trace_Number == 13) Final_Result = 23;
                else if (Primary_Trace_Number == 14) Final_Result = 24;
                else if (Primary_Trace_Number == 15) Final_Result = 25;
                else if (Primary_Trace_Number == 16) Final_Result = 26;
                else if (Primary_Trace_Number == 17) Final_Result = 27;
                else if (Primary_Trace_Number == 18) Final_Result = 28;
                else if (Primary_Trace_Number == 19) Final_Result = 29;
                else if (Primary_Trace_Number == 20) Final_Result = 30;
                else if (Primary_Trace_Number == 21) Final_Result = 41;
                else if (Primary_Trace_Number == 22) Final_Result = 42;
                else if (Primary_Trace_Number == 23) Final_Result = 43;
                else if (Primary_Trace_Number == 24) Final_Result = 44;
                else if (Primary_Trace_Number == 25) Final_Result = 45;
                else if (Primary_Trace_Number == 26) Final_Result = 46;
                else if (Primary_Trace_Number == 27) Final_Result = 47;
                else if (Primary_Trace_Number == 28) Final_Result = 48;
                else if (Primary_Trace_Number == 29) Final_Result = 49;
                else if (Primary_Trace_Number == 30) Final_Result = 50;
                else if (Primary_Trace_Number == 31) Final_Result = 61;
                else if (Primary_Trace_Number == 32) Final_Result = 62;
                else if (Primary_Trace_Number == 33) Final_Result = 63;
                else if (Primary_Trace_Number == 34) Final_Result = 64;
                else if (Primary_Trace_Number == 35) Final_Result = 65;
                else if (Primary_Trace_Number == 36) Final_Result = 66;
                else if (Primary_Trace_Number == 37) Final_Result = 67;
                else if (Primary_Trace_Number == 38) Final_Result = 68;
                else if (Primary_Trace_Number == 39) Final_Result = 69;
                else if (Primary_Trace_Number == 40) Final_Result = 70;
                else if (Primary_Trace_Number == 41) Final_Result = 81;
                else if (Primary_Trace_Number == 42) Final_Result = 82;
                else if (Primary_Trace_Number == 43) Final_Result = 83;
                else if (Primary_Trace_Number == 44) Final_Result = 84;
                else if (Primary_Trace_Number == 45) Final_Result = 85;
                else if (Primary_Trace_Number == 46) Final_Result = 86;
                else if (Primary_Trace_Number == 47) Final_Result = 87;
                else if (Primary_Trace_Number == 48) Final_Result = 88;
                else if (Primary_Trace_Number == 49) Final_Result = 89;
                else if (Primary_Trace_Number == 50) Final_Result = 90;
                else if (Primary_Trace_Number == 51) Final_Result = 101;
                else if (Primary_Trace_Number == 52) Final_Result = 102;
                else if (Primary_Trace_Number == 53) Final_Result = 103;
                else if (Primary_Trace_Number == 54) Final_Result = 104;
                else if (Primary_Trace_Number == 55) Final_Result = 105;
                else if (Primary_Trace_Number == 56) Final_Result = 106;
                else if (Primary_Trace_Number == 57) Final_Result = 107;
                else if (Primary_Trace_Number == 58) Final_Result = 108;
                else if (Primary_Trace_Number == 59) Final_Result = 109;
                else if (Primary_Trace_Number == 60) Final_Result = 110;
                else if (Primary_Trace_Number == 61) Final_Result = 121;
                else if (Primary_Trace_Number == 62) Final_Result = 122;
                else if (Primary_Trace_Number == 63) Final_Result = 123;
                else if (Primary_Trace_Number == 64) Final_Result = 124;
                else if (Primary_Trace_Number == 65) Final_Result = 125;
                else if (Primary_Trace_Number == 66) Final_Result = 126;
                else if (Primary_Trace_Number == 67) Final_Result = 133;
                else if (Primary_Trace_Number == 68) Final_Result = 134;
                else if (Primary_Trace_Number == 69) Final_Result = 135;
                else if (Primary_Trace_Number == 70) Final_Result = 136;
                else if (Primary_Trace_Number == 71) Final_Result = 137;
                else if (Primary_Trace_Number == 72) Final_Result = 138;
                else if (Primary_Trace_Number == 73) Final_Result = 145;
                else if (Primary_Trace_Number == 74) Final_Result = 146;
                else if (Primary_Trace_Number == 75) Final_Result = 147;
                else if (Primary_Trace_Number == 76) Final_Result = 148;
                else if (Primary_Trace_Number == 77) Final_Result = 149;
                else if (Primary_Trace_Number == 78) Final_Result = 150;
                else if (Primary_Trace_Number == 79) Final_Result = 157;
                else if (Primary_Trace_Number == 80) Final_Result = 158;
                else if (Primary_Trace_Number == 81) Final_Result = 159;
                else if (Primary_Trace_Number == 82) Final_Result = 160;
                else if (Primary_Trace_Number == 83) Final_Result = 161;
                else if (Primary_Trace_Number == 84) Final_Result = 162;
                else if (Primary_Trace_Number == 85) Final_Result = 169;
                else if (Primary_Trace_Number == 86) Final_Result = 170;
                else if (Primary_Trace_Number == 87) Final_Result = 171;
                else if (Primary_Trace_Number == 88) Final_Result = 172;
                else if (Primary_Trace_Number == 89) Final_Result = 173;
                else if (Primary_Trace_Number == 90) Final_Result = 174;
                else if (Primary_Trace_Number == 91) Final_Result = 181;
                else if (Primary_Trace_Number == 92) Final_Result = 182;
                else if (Primary_Trace_Number == 93) Final_Result = 183;
                else if (Primary_Trace_Number == 94) Final_Result = 184;
                else if (Primary_Trace_Number == 95) Final_Result = 185;
                else if (Primary_Trace_Number == 96) Final_Result = 186;
                else if (Primary_Trace_Number == 97) Final_Result = 187;
                else if (Primary_Trace_Number == 98) Final_Result = 188;
                else if (Primary_Trace_Number == 99) Final_Result = 189;
                else if (Primary_Trace_Number == 100) Final_Result = 190;
                else if (Primary_Trace_Number == 101) Final_Result = 201;
                else if (Primary_Trace_Number == 102) Final_Result = 202;
                else if (Primary_Trace_Number == 103) Final_Result = 203;
                else if (Primary_Trace_Number == 104) Final_Result = 204;
                else if (Primary_Trace_Number == 105) Final_Result = 205;
                else if (Primary_Trace_Number == 106) Final_Result = 206;
                else if (Primary_Trace_Number == 107) Final_Result = 207;
                else if (Primary_Trace_Number == 108) Final_Result = 208;
                else if (Primary_Trace_Number == 109) Final_Result = 209;
                else if (Primary_Trace_Number == 110) Final_Result = 210;
                else if (Primary_Trace_Number == 111) Final_Result = 221;
                else if (Primary_Trace_Number == 112) Final_Result = 222;
                else if (Primary_Trace_Number == 113) Final_Result = 223;
                else if (Primary_Trace_Number == 114) Final_Result = 224;
                else if (Primary_Trace_Number == 115) Final_Result = 225;
                else if (Primary_Trace_Number == 116) Final_Result = 226;
                else if (Primary_Trace_Number == 117) Final_Result = 227;
                else if (Primary_Trace_Number == 118) Final_Result = 228;
                else if (Primary_Trace_Number == 119) Final_Result = 229;
                else if (Primary_Trace_Number == 120) Final_Result = 230;
                else if (Primary_Trace_Number == 121) Final_Result = 241;
                else if (Primary_Trace_Number == 122) Final_Result = 242;
                else if (Primary_Trace_Number == 123) Final_Result = 243;
                else if (Primary_Trace_Number == 124) Final_Result = 244;
                else if (Primary_Trace_Number == 125) Final_Result = 245;
                else if (Primary_Trace_Number == 126) Final_Result = 246;
                else if (Primary_Trace_Number == 127) Final_Result = 247;
                else if (Primary_Trace_Number == 128) Final_Result = 248;
                else if (Primary_Trace_Number == 129) Final_Result = 249;
                else if (Primary_Trace_Number == 130) Final_Result = 250;
                else if (Primary_Trace_Number == 131) Final_Result = 261;
                else if (Primary_Trace_Number == 132) Final_Result = 262;
                else if (Primary_Trace_Number == 133) Final_Result = 263;
                else if (Primary_Trace_Number == 134) Final_Result = 264;
                else if (Primary_Trace_Number == 135) Final_Result = 265;
                else if (Primary_Trace_Number == 136) Final_Result = 266;
                else if (Primary_Trace_Number == 137) Final_Result = 267;
                else if (Primary_Trace_Number == 138) Final_Result = 268;
                else if (Primary_Trace_Number == 139) Final_Result = 269;
                else if (Primary_Trace_Number == 140) Final_Result = 270;
                else if (Primary_Trace_Number == 141) Final_Result = 281;
                else if (Primary_Trace_Number == 142) Final_Result = 282;
                else if (Primary_Trace_Number == 143) Final_Result = 283;
                else if (Primary_Trace_Number == 144) Final_Result = 284;
                else if (Primary_Trace_Number == 145) Final_Result = 285;
                else if (Primary_Trace_Number == 146) Final_Result = 286;
                else if (Primary_Trace_Number == 147) Final_Result = 287;
                else if (Primary_Trace_Number == 148) Final_Result = 288;
                else if (Primary_Trace_Number == 149) Final_Result = 289;
                else if (Primary_Trace_Number == 150) Final_Result = 290;
                else if (Primary_Trace_Number == 151) Final_Result = 301;
                else if (Primary_Trace_Number == 152) Final_Result = 302;
                else if (Primary_Trace_Number == 153) Final_Result = 303;
                else if (Primary_Trace_Number == 154) Final_Result = 304;
                else if (Primary_Trace_Number == 155) Final_Result = 305;
                else if (Primary_Trace_Number == 156) Final_Result = 306;
                else if (Primary_Trace_Number == 157) Final_Result = 313;
                else if (Primary_Trace_Number == 158) Final_Result = 314;
                else if (Primary_Trace_Number == 159) Final_Result = 315;
                else if (Primary_Trace_Number == 160) Final_Result = 316;
                else if (Primary_Trace_Number == 161) Final_Result = 317;
                else if (Primary_Trace_Number == 162) Final_Result = 318;
                else if (Primary_Trace_Number == 163) Final_Result = 325;
                else if (Primary_Trace_Number == 164) Final_Result = 326;
                else if (Primary_Trace_Number == 165) Final_Result = 327;
                else if (Primary_Trace_Number == 166) Final_Result = 328;
                else if (Primary_Trace_Number == 167) Final_Result = 329;
                else if (Primary_Trace_Number == 168) Final_Result = 330;
                else if (Primary_Trace_Number == 169) Final_Result = 337;
                else if (Primary_Trace_Number == 170) Final_Result = 338;
                else if (Primary_Trace_Number == 171) Final_Result = 339;
                else if (Primary_Trace_Number == 172) Final_Result = 340;
                else if (Primary_Trace_Number == 173) Final_Result = 341;
                else if (Primary_Trace_Number == 174) Final_Result = 342;
                else if (Primary_Trace_Number == 175) Final_Result = 349;
                else if (Primary_Trace_Number == 176) Final_Result = 350;
                else if (Primary_Trace_Number == 177) Final_Result = 351;
                else if (Primary_Trace_Number == 178) Final_Result = 352;
                else if (Primary_Trace_Number == 179) Final_Result = 353;
                else if (Primary_Trace_Number == 180) Final_Result = 354;
                else if (Primary_Trace_Number == 181) Final_Result = 361;
                else if (Primary_Trace_Number == 182) Final_Result = 362;
                else if (Primary_Trace_Number == 183) Final_Result = 363;
                else if (Primary_Trace_Number == 184) Final_Result = 367;
                else if (Primary_Trace_Number == 185) Final_Result = 368;
                else if (Primary_Trace_Number == 186) Final_Result = 369;
                else Final_Result = 0;

                #endregion Site_0_Trace_Number
            }
            else if (Site_Number == 1)
            {
                #region Site_1_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 373;
                else if (Primary_Trace_Number == 2) Final_Result = 374;
                else if (Primary_Trace_Number == 3) Final_Result = 375;
                else if (Primary_Trace_Number == 4) Final_Result = 376;
                else if (Primary_Trace_Number == 5) Final_Result = 377;
                else if (Primary_Trace_Number == 6) Final_Result = 378;
                else if (Primary_Trace_Number == 7) Final_Result = 379;
                else if (Primary_Trace_Number == 8) Final_Result = 380;
                else if (Primary_Trace_Number == 9) Final_Result = 381;
                else if (Primary_Trace_Number == 10) Final_Result = 382;
                else if (Primary_Trace_Number == 11) Final_Result = 393;
                else if (Primary_Trace_Number == 12) Final_Result = 394;
                else if (Primary_Trace_Number == 13) Final_Result = 395;
                else if (Primary_Trace_Number == 14) Final_Result = 396;
                else if (Primary_Trace_Number == 15) Final_Result = 397;
                else if (Primary_Trace_Number == 16) Final_Result = 398;
                else if (Primary_Trace_Number == 17) Final_Result = 399;
                else if (Primary_Trace_Number == 18) Final_Result = 400;
                else if (Primary_Trace_Number == 19) Final_Result = 401;
                else if (Primary_Trace_Number == 20) Final_Result = 402;
                else if (Primary_Trace_Number == 21) Final_Result = 413;
                else if (Primary_Trace_Number == 22) Final_Result = 414;
                else if (Primary_Trace_Number == 23) Final_Result = 415;
                else if (Primary_Trace_Number == 24) Final_Result = 416;
                else if (Primary_Trace_Number == 25) Final_Result = 417;
                else if (Primary_Trace_Number == 26) Final_Result = 418;
                else if (Primary_Trace_Number == 27) Final_Result = 419;
                else if (Primary_Trace_Number == 28) Final_Result = 420;
                else if (Primary_Trace_Number == 29) Final_Result = 421;
                else if (Primary_Trace_Number == 30) Final_Result = 422;
                else if (Primary_Trace_Number == 31) Final_Result = 433;
                else if (Primary_Trace_Number == 32) Final_Result = 434;
                else if (Primary_Trace_Number == 33) Final_Result = 435;
                else if (Primary_Trace_Number == 34) Final_Result = 436;
                else if (Primary_Trace_Number == 35) Final_Result = 437;
                else if (Primary_Trace_Number == 36) Final_Result = 438;
                else if (Primary_Trace_Number == 37) Final_Result = 439;
                else if (Primary_Trace_Number == 38) Final_Result = 440;
                else if (Primary_Trace_Number == 39) Final_Result = 441;
                else if (Primary_Trace_Number == 40) Final_Result = 442;
                else if (Primary_Trace_Number == 41) Final_Result = 453;
                else if (Primary_Trace_Number == 42) Final_Result = 454;
                else if (Primary_Trace_Number == 43) Final_Result = 455;
                else if (Primary_Trace_Number == 44) Final_Result = 456;
                else if (Primary_Trace_Number == 45) Final_Result = 457;
                else if (Primary_Trace_Number == 46) Final_Result = 458;
                else if (Primary_Trace_Number == 47) Final_Result = 459;
                else if (Primary_Trace_Number == 48) Final_Result = 460;
                else if (Primary_Trace_Number == 49) Final_Result = 461;
                else if (Primary_Trace_Number == 50) Final_Result = 462;
                else if (Primary_Trace_Number == 51) Final_Result = 473;
                else if (Primary_Trace_Number == 52) Final_Result = 474;
                else if (Primary_Trace_Number == 53) Final_Result = 475;
                else if (Primary_Trace_Number == 54) Final_Result = 476;
                else if (Primary_Trace_Number == 55) Final_Result = 477;
                else if (Primary_Trace_Number == 56) Final_Result = 478;
                else if (Primary_Trace_Number == 57) Final_Result = 479;
                else if (Primary_Trace_Number == 58) Final_Result = 480;
                else if (Primary_Trace_Number == 59) Final_Result = 481;
                else if (Primary_Trace_Number == 60) Final_Result = 482;
                else if (Primary_Trace_Number == 61) Final_Result = 493;
                else if (Primary_Trace_Number == 62) Final_Result = 494;
                else if (Primary_Trace_Number == 63) Final_Result = 495;
                else if (Primary_Trace_Number == 64) Final_Result = 496;
                else if (Primary_Trace_Number == 65) Final_Result = 497;
                else if (Primary_Trace_Number == 66) Final_Result = 498;
                else if (Primary_Trace_Number == 67) Final_Result = 505;
                else if (Primary_Trace_Number == 68) Final_Result = 506;
                else if (Primary_Trace_Number == 69) Final_Result = 507;
                else if (Primary_Trace_Number == 70) Final_Result = 508;
                else if (Primary_Trace_Number == 71) Final_Result = 509;
                else if (Primary_Trace_Number == 72) Final_Result = 510;
                else if (Primary_Trace_Number == 73) Final_Result = 517;
                else if (Primary_Trace_Number == 74) Final_Result = 518;
                else if (Primary_Trace_Number == 75) Final_Result = 519;
                else if (Primary_Trace_Number == 76) Final_Result = 520;
                else if (Primary_Trace_Number == 77) Final_Result = 521;
                else if (Primary_Trace_Number == 78) Final_Result = 522;
                else if (Primary_Trace_Number == 79) Final_Result = 529;
                else if (Primary_Trace_Number == 80) Final_Result = 530;
                else if (Primary_Trace_Number == 81) Final_Result = 531;
                else if (Primary_Trace_Number == 82) Final_Result = 532;
                else if (Primary_Trace_Number == 83) Final_Result = 533;
                else if (Primary_Trace_Number == 84) Final_Result = 534;
                else if (Primary_Trace_Number == 85) Final_Result = 541;
                else if (Primary_Trace_Number == 86) Final_Result = 542;
                else if (Primary_Trace_Number == 87) Final_Result = 543;
                else if (Primary_Trace_Number == 88) Final_Result = 544;
                else if (Primary_Trace_Number == 89) Final_Result = 545;
                else if (Primary_Trace_Number == 90) Final_Result = 546;
                else if (Primary_Trace_Number == 91) Final_Result = 553;
                else if (Primary_Trace_Number == 92) Final_Result = 554;
                else if (Primary_Trace_Number == 93) Final_Result = 555;
                else if (Primary_Trace_Number == 94) Final_Result = 556;
                else if (Primary_Trace_Number == 95) Final_Result = 557;
                else if (Primary_Trace_Number == 96) Final_Result = 558;
                else if (Primary_Trace_Number == 97) Final_Result = 559;
                else if (Primary_Trace_Number == 98) Final_Result = 560;
                else if (Primary_Trace_Number == 99) Final_Result = 561;
                else if (Primary_Trace_Number == 100) Final_Result = 562;
                else if (Primary_Trace_Number == 101) Final_Result = 573;
                else if (Primary_Trace_Number == 102) Final_Result = 574;
                else if (Primary_Trace_Number == 103) Final_Result = 575;
                else if (Primary_Trace_Number == 104) Final_Result = 576;
                else if (Primary_Trace_Number == 105) Final_Result = 577;
                else if (Primary_Trace_Number == 106) Final_Result = 578;
                else if (Primary_Trace_Number == 107) Final_Result = 579;
                else if (Primary_Trace_Number == 108) Final_Result = 580;
                else if (Primary_Trace_Number == 109) Final_Result = 581;
                else if (Primary_Trace_Number == 110) Final_Result = 582;
                else if (Primary_Trace_Number == 111) Final_Result = 593;
                else if (Primary_Trace_Number == 112) Final_Result = 594;
                else if (Primary_Trace_Number == 113) Final_Result = 595;
                else if (Primary_Trace_Number == 114) Final_Result = 596;
                else if (Primary_Trace_Number == 115) Final_Result = 597;
                else if (Primary_Trace_Number == 116) Final_Result = 598;
                else if (Primary_Trace_Number == 117) Final_Result = 599;
                else if (Primary_Trace_Number == 118) Final_Result = 600;
                else if (Primary_Trace_Number == 119) Final_Result = 601;
                else if (Primary_Trace_Number == 120) Final_Result = 602;
                else if (Primary_Trace_Number == 121) Final_Result = 613;
                else if (Primary_Trace_Number == 122) Final_Result = 614;
                else if (Primary_Trace_Number == 123) Final_Result = 615;
                else if (Primary_Trace_Number == 124) Final_Result = 616;
                else if (Primary_Trace_Number == 125) Final_Result = 617;
                else if (Primary_Trace_Number == 126) Final_Result = 618;
                else if (Primary_Trace_Number == 127) Final_Result = 619;
                else if (Primary_Trace_Number == 128) Final_Result = 620;
                else if (Primary_Trace_Number == 129) Final_Result = 621;
                else if (Primary_Trace_Number == 130) Final_Result = 622;
                else if (Primary_Trace_Number == 131) Final_Result = 633;
                else if (Primary_Trace_Number == 132) Final_Result = 634;
                else if (Primary_Trace_Number == 133) Final_Result = 635;
                else if (Primary_Trace_Number == 134) Final_Result = 636;
                else if (Primary_Trace_Number == 135) Final_Result = 637;
                else if (Primary_Trace_Number == 136) Final_Result = 638;
                else if (Primary_Trace_Number == 137) Final_Result = 639;
                else if (Primary_Trace_Number == 138) Final_Result = 640;
                else if (Primary_Trace_Number == 139) Final_Result = 641;
                else if (Primary_Trace_Number == 140) Final_Result = 642;
                else if (Primary_Trace_Number == 141) Final_Result = 653;
                else if (Primary_Trace_Number == 142) Final_Result = 654;
                else if (Primary_Trace_Number == 143) Final_Result = 655;
                else if (Primary_Trace_Number == 144) Final_Result = 656;
                else if (Primary_Trace_Number == 145) Final_Result = 657;
                else if (Primary_Trace_Number == 146) Final_Result = 658;
                else if (Primary_Trace_Number == 147) Final_Result = 659;
                else if (Primary_Trace_Number == 148) Final_Result = 660;
                else if (Primary_Trace_Number == 149) Final_Result = 661;
                else if (Primary_Trace_Number == 150) Final_Result = 662;
                else if (Primary_Trace_Number == 151) Final_Result = 673;
                else if (Primary_Trace_Number == 152) Final_Result = 674;
                else if (Primary_Trace_Number == 153) Final_Result = 675;
                else if (Primary_Trace_Number == 154) Final_Result = 676;
                else if (Primary_Trace_Number == 155) Final_Result = 677;
                else if (Primary_Trace_Number == 156) Final_Result = 678;
                else if (Primary_Trace_Number == 157) Final_Result = 685;
                else if (Primary_Trace_Number == 158) Final_Result = 686;
                else if (Primary_Trace_Number == 159) Final_Result = 687;
                else if (Primary_Trace_Number == 160) Final_Result = 688;
                else if (Primary_Trace_Number == 161) Final_Result = 689;
                else if (Primary_Trace_Number == 162) Final_Result = 690;
                else if (Primary_Trace_Number == 163) Final_Result = 697;
                else if (Primary_Trace_Number == 164) Final_Result = 698;
                else if (Primary_Trace_Number == 165) Final_Result = 699;
                else if (Primary_Trace_Number == 166) Final_Result = 700;
                else if (Primary_Trace_Number == 167) Final_Result = 701;
                else if (Primary_Trace_Number == 168) Final_Result = 702;
                else if (Primary_Trace_Number == 169) Final_Result = 709;
                else if (Primary_Trace_Number == 170) Final_Result = 710;
                else if (Primary_Trace_Number == 171) Final_Result = 711;
                else if (Primary_Trace_Number == 172) Final_Result = 712;
                else if (Primary_Trace_Number == 173) Final_Result = 713;
                else if (Primary_Trace_Number == 174) Final_Result = 714;
                else if (Primary_Trace_Number == 175) Final_Result = 721;
                else if (Primary_Trace_Number == 176) Final_Result = 722;
                else if (Primary_Trace_Number == 177) Final_Result = 723;
                else if (Primary_Trace_Number == 178) Final_Result = 724;
                else if (Primary_Trace_Number == 179) Final_Result = 725;
                else if (Primary_Trace_Number == 180) Final_Result = 726;
                else if (Primary_Trace_Number == 181) Final_Result = 733;
                else if (Primary_Trace_Number == 182) Final_Result = 734;
                else if (Primary_Trace_Number == 183) Final_Result = 735;
                else if (Primary_Trace_Number == 184) Final_Result = 739;
                else if (Primary_Trace_Number == 185) Final_Result = 740;
                else if (Primary_Trace_Number == 186) Final_Result = 741;
                else Final_Result = 0;

                #endregion Site_1_Trace_Number
            }
            else if (Site_Number == 2)
            {
                #region Site_2_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 11;
                else if (Primary_Trace_Number == 2) Final_Result = 12;
                else if (Primary_Trace_Number == 3) Final_Result = 13;
                else if (Primary_Trace_Number == 4) Final_Result = 14;
                else if (Primary_Trace_Number == 5) Final_Result = 15;
                else if (Primary_Trace_Number == 6) Final_Result = 16;
                else if (Primary_Trace_Number == 7) Final_Result = 17;
                else if (Primary_Trace_Number == 8) Final_Result = 18;
                else if (Primary_Trace_Number == 9) Final_Result = 19;
                else if (Primary_Trace_Number == 10) Final_Result = 20;
                else if (Primary_Trace_Number == 11) Final_Result = 31;
                else if (Primary_Trace_Number == 12) Final_Result = 32;
                else if (Primary_Trace_Number == 13) Final_Result = 33;
                else if (Primary_Trace_Number == 14) Final_Result = 34;
                else if (Primary_Trace_Number == 15) Final_Result = 35;
                else if (Primary_Trace_Number == 16) Final_Result = 36;
                else if (Primary_Trace_Number == 17) Final_Result = 37;
                else if (Primary_Trace_Number == 18) Final_Result = 38;
                else if (Primary_Trace_Number == 19) Final_Result = 39;
                else if (Primary_Trace_Number == 20) Final_Result = 40;
                else if (Primary_Trace_Number == 21) Final_Result = 51;
                else if (Primary_Trace_Number == 22) Final_Result = 52;
                else if (Primary_Trace_Number == 23) Final_Result = 53;
                else if (Primary_Trace_Number == 24) Final_Result = 54;
                else if (Primary_Trace_Number == 25) Final_Result = 55;
                else if (Primary_Trace_Number == 26) Final_Result = 56;
                else if (Primary_Trace_Number == 27) Final_Result = 57;
                else if (Primary_Trace_Number == 28) Final_Result = 58;
                else if (Primary_Trace_Number == 29) Final_Result = 59;
                else if (Primary_Trace_Number == 30) Final_Result = 60;
                else if (Primary_Trace_Number == 31) Final_Result = 71;
                else if (Primary_Trace_Number == 32) Final_Result = 72;
                else if (Primary_Trace_Number == 33) Final_Result = 73;
                else if (Primary_Trace_Number == 34) Final_Result = 74;
                else if (Primary_Trace_Number == 35) Final_Result = 75;
                else if (Primary_Trace_Number == 36) Final_Result = 76;
                else if (Primary_Trace_Number == 37) Final_Result = 77;
                else if (Primary_Trace_Number == 38) Final_Result = 78;
                else if (Primary_Trace_Number == 39) Final_Result = 79;
                else if (Primary_Trace_Number == 40) Final_Result = 80;
                else if (Primary_Trace_Number == 41) Final_Result = 91;
                else if (Primary_Trace_Number == 42) Final_Result = 92;
                else if (Primary_Trace_Number == 43) Final_Result = 93;
                else if (Primary_Trace_Number == 44) Final_Result = 94;
                else if (Primary_Trace_Number == 45) Final_Result = 95;
                else if (Primary_Trace_Number == 46) Final_Result = 96;
                else if (Primary_Trace_Number == 47) Final_Result = 97;
                else if (Primary_Trace_Number == 48) Final_Result = 98;
                else if (Primary_Trace_Number == 49) Final_Result = 99;
                else if (Primary_Trace_Number == 50) Final_Result = 100;
                else if (Primary_Trace_Number == 51) Final_Result = 111;
                else if (Primary_Trace_Number == 52) Final_Result = 112;
                else if (Primary_Trace_Number == 53) Final_Result = 113;
                else if (Primary_Trace_Number == 54) Final_Result = 114;
                else if (Primary_Trace_Number == 55) Final_Result = 115;
                else if (Primary_Trace_Number == 56) Final_Result = 116;
                else if (Primary_Trace_Number == 57) Final_Result = 117;
                else if (Primary_Trace_Number == 58) Final_Result = 118;
                else if (Primary_Trace_Number == 59) Final_Result = 119;
                else if (Primary_Trace_Number == 60) Final_Result = 120;
                else if (Primary_Trace_Number == 61) Final_Result = 127;
                else if (Primary_Trace_Number == 62) Final_Result = 128;
                else if (Primary_Trace_Number == 63) Final_Result = 129;
                else if (Primary_Trace_Number == 64) Final_Result = 130;
                else if (Primary_Trace_Number == 65) Final_Result = 131;
                else if (Primary_Trace_Number == 66) Final_Result = 132;
                else if (Primary_Trace_Number == 67) Final_Result = 139;
                else if (Primary_Trace_Number == 68) Final_Result = 140;
                else if (Primary_Trace_Number == 69) Final_Result = 141;
                else if (Primary_Trace_Number == 70) Final_Result = 142;
                else if (Primary_Trace_Number == 71) Final_Result = 143;
                else if (Primary_Trace_Number == 72) Final_Result = 144;
                else if (Primary_Trace_Number == 73) Final_Result = 151;
                else if (Primary_Trace_Number == 74) Final_Result = 152;
                else if (Primary_Trace_Number == 75) Final_Result = 153;
                else if (Primary_Trace_Number == 76) Final_Result = 154;
                else if (Primary_Trace_Number == 77) Final_Result = 155;
                else if (Primary_Trace_Number == 78) Final_Result = 156;
                else if (Primary_Trace_Number == 79) Final_Result = 163;
                else if (Primary_Trace_Number == 80) Final_Result = 164;
                else if (Primary_Trace_Number == 81) Final_Result = 165;
                else if (Primary_Trace_Number == 82) Final_Result = 166;
                else if (Primary_Trace_Number == 83) Final_Result = 167;
                else if (Primary_Trace_Number == 84) Final_Result = 168;
                else if (Primary_Trace_Number == 85) Final_Result = 175;
                else if (Primary_Trace_Number == 86) Final_Result = 176;
                else if (Primary_Trace_Number == 87) Final_Result = 177;
                else if (Primary_Trace_Number == 88) Final_Result = 178;
                else if (Primary_Trace_Number == 89) Final_Result = 179;
                else if (Primary_Trace_Number == 90) Final_Result = 180;
                else if (Primary_Trace_Number == 91) Final_Result = 191;
                else if (Primary_Trace_Number == 92) Final_Result = 192;
                else if (Primary_Trace_Number == 93) Final_Result = 193;
                else if (Primary_Trace_Number == 94) Final_Result = 194;
                else if (Primary_Trace_Number == 95) Final_Result = 195;
                else if (Primary_Trace_Number == 96) Final_Result = 196;
                else if (Primary_Trace_Number == 97) Final_Result = 197;
                else if (Primary_Trace_Number == 98) Final_Result = 198;
                else if (Primary_Trace_Number == 99) Final_Result = 199;
                else if (Primary_Trace_Number == 100) Final_Result = 200;
                else if (Primary_Trace_Number == 101) Final_Result = 211;
                else if (Primary_Trace_Number == 102) Final_Result = 212;
                else if (Primary_Trace_Number == 103) Final_Result = 213;
                else if (Primary_Trace_Number == 104) Final_Result = 214;
                else if (Primary_Trace_Number == 105) Final_Result = 215;
                else if (Primary_Trace_Number == 106) Final_Result = 216;
                else if (Primary_Trace_Number == 107) Final_Result = 217;
                else if (Primary_Trace_Number == 108) Final_Result = 218;
                else if (Primary_Trace_Number == 109) Final_Result = 219;
                else if (Primary_Trace_Number == 110) Final_Result = 220;
                else if (Primary_Trace_Number == 111) Final_Result = 231;
                else if (Primary_Trace_Number == 112) Final_Result = 232;
                else if (Primary_Trace_Number == 113) Final_Result = 233;
                else if (Primary_Trace_Number == 114) Final_Result = 234;
                else if (Primary_Trace_Number == 115) Final_Result = 235;
                else if (Primary_Trace_Number == 116) Final_Result = 236;
                else if (Primary_Trace_Number == 117) Final_Result = 237;
                else if (Primary_Trace_Number == 118) Final_Result = 238;
                else if (Primary_Trace_Number == 119) Final_Result = 239;
                else if (Primary_Trace_Number == 120) Final_Result = 240;
                else if (Primary_Trace_Number == 121) Final_Result = 251;
                else if (Primary_Trace_Number == 122) Final_Result = 252;
                else if (Primary_Trace_Number == 123) Final_Result = 253;
                else if (Primary_Trace_Number == 124) Final_Result = 254;
                else if (Primary_Trace_Number == 125) Final_Result = 255;
                else if (Primary_Trace_Number == 126) Final_Result = 256;
                else if (Primary_Trace_Number == 127) Final_Result = 257;
                else if (Primary_Trace_Number == 128) Final_Result = 258;
                else if (Primary_Trace_Number == 129) Final_Result = 259;
                else if (Primary_Trace_Number == 130) Final_Result = 260;
                else if (Primary_Trace_Number == 131) Final_Result = 271;
                else if (Primary_Trace_Number == 132) Final_Result = 272;
                else if (Primary_Trace_Number == 133) Final_Result = 273;
                else if (Primary_Trace_Number == 134) Final_Result = 274;
                else if (Primary_Trace_Number == 135) Final_Result = 275;
                else if (Primary_Trace_Number == 136) Final_Result = 276;
                else if (Primary_Trace_Number == 137) Final_Result = 277;
                else if (Primary_Trace_Number == 138) Final_Result = 278;
                else if (Primary_Trace_Number == 139) Final_Result = 279;
                else if (Primary_Trace_Number == 140) Final_Result = 280;
                else if (Primary_Trace_Number == 141) Final_Result = 291;
                else if (Primary_Trace_Number == 142) Final_Result = 292;
                else if (Primary_Trace_Number == 143) Final_Result = 293;
                else if (Primary_Trace_Number == 144) Final_Result = 294;
                else if (Primary_Trace_Number == 145) Final_Result = 295;
                else if (Primary_Trace_Number == 146) Final_Result = 296;
                else if (Primary_Trace_Number == 147) Final_Result = 297;
                else if (Primary_Trace_Number == 148) Final_Result = 298;
                else if (Primary_Trace_Number == 149) Final_Result = 299;
                else if (Primary_Trace_Number == 150) Final_Result = 300;
                else if (Primary_Trace_Number == 151) Final_Result = 307;
                else if (Primary_Trace_Number == 152) Final_Result = 308;
                else if (Primary_Trace_Number == 153) Final_Result = 309;
                else if (Primary_Trace_Number == 154) Final_Result = 310;
                else if (Primary_Trace_Number == 155) Final_Result = 311;
                else if (Primary_Trace_Number == 156) Final_Result = 312;
                else if (Primary_Trace_Number == 157) Final_Result = 319;
                else if (Primary_Trace_Number == 158) Final_Result = 320;
                else if (Primary_Trace_Number == 159) Final_Result = 321;
                else if (Primary_Trace_Number == 160) Final_Result = 322;
                else if (Primary_Trace_Number == 161) Final_Result = 323;
                else if (Primary_Trace_Number == 162) Final_Result = 324;
                else if (Primary_Trace_Number == 163) Final_Result = 331;
                else if (Primary_Trace_Number == 164) Final_Result = 332;
                else if (Primary_Trace_Number == 165) Final_Result = 333;
                else if (Primary_Trace_Number == 166) Final_Result = 334;
                else if (Primary_Trace_Number == 167) Final_Result = 335;
                else if (Primary_Trace_Number == 168) Final_Result = 336;
                else if (Primary_Trace_Number == 169) Final_Result = 343;
                else if (Primary_Trace_Number == 170) Final_Result = 344;
                else if (Primary_Trace_Number == 171) Final_Result = 345;
                else if (Primary_Trace_Number == 172) Final_Result = 346;
                else if (Primary_Trace_Number == 173) Final_Result = 347;
                else if (Primary_Trace_Number == 174) Final_Result = 348;
                else if (Primary_Trace_Number == 175) Final_Result = 355;
                else if (Primary_Trace_Number == 176) Final_Result = 356;
                else if (Primary_Trace_Number == 177) Final_Result = 357;
                else if (Primary_Trace_Number == 178) Final_Result = 358;
                else if (Primary_Trace_Number == 179) Final_Result = 359;
                else if (Primary_Trace_Number == 180) Final_Result = 360;
                else if (Primary_Trace_Number == 181) Final_Result = 364;
                else if (Primary_Trace_Number == 182) Final_Result = 365;
                else if (Primary_Trace_Number == 183) Final_Result = 366;
                else if (Primary_Trace_Number == 184) Final_Result = 370;
                else if (Primary_Trace_Number == 185) Final_Result = 371;
                else if (Primary_Trace_Number == 186) Final_Result = 372;
                else Final_Result = 0;

                #endregion Site_2_Trace_Number
            }
            else if (Site_Number == 3)
            {
                #region Site_3_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 383;
                else if (Primary_Trace_Number == 2) Final_Result = 384;
                else if (Primary_Trace_Number == 3) Final_Result = 385;
                else if (Primary_Trace_Number == 4) Final_Result = 386;
                else if (Primary_Trace_Number == 5) Final_Result = 387;
                else if (Primary_Trace_Number == 6) Final_Result = 388;
                else if (Primary_Trace_Number == 7) Final_Result = 389;
                else if (Primary_Trace_Number == 8) Final_Result = 390;
                else if (Primary_Trace_Number == 9) Final_Result = 391;
                else if (Primary_Trace_Number == 10) Final_Result = 392;
                else if (Primary_Trace_Number == 11) Final_Result = 403;
                else if (Primary_Trace_Number == 12) Final_Result = 404;
                else if (Primary_Trace_Number == 13) Final_Result = 405;
                else if (Primary_Trace_Number == 14) Final_Result = 406;
                else if (Primary_Trace_Number == 15) Final_Result = 407;
                else if (Primary_Trace_Number == 16) Final_Result = 408;
                else if (Primary_Trace_Number == 17) Final_Result = 409;
                else if (Primary_Trace_Number == 18) Final_Result = 410;
                else if (Primary_Trace_Number == 19) Final_Result = 411;
                else if (Primary_Trace_Number == 20) Final_Result = 412;
                else if (Primary_Trace_Number == 21) Final_Result = 423;
                else if (Primary_Trace_Number == 22) Final_Result = 424;
                else if (Primary_Trace_Number == 23) Final_Result = 425;
                else if (Primary_Trace_Number == 24) Final_Result = 426;
                else if (Primary_Trace_Number == 25) Final_Result = 427;
                else if (Primary_Trace_Number == 26) Final_Result = 428;
                else if (Primary_Trace_Number == 27) Final_Result = 429;
                else if (Primary_Trace_Number == 28) Final_Result = 430;
                else if (Primary_Trace_Number == 29) Final_Result = 431;
                else if (Primary_Trace_Number == 30) Final_Result = 432;
                else if (Primary_Trace_Number == 31) Final_Result = 443;
                else if (Primary_Trace_Number == 32) Final_Result = 444;
                else if (Primary_Trace_Number == 33) Final_Result = 445;
                else if (Primary_Trace_Number == 34) Final_Result = 446;
                else if (Primary_Trace_Number == 35) Final_Result = 447;
                else if (Primary_Trace_Number == 36) Final_Result = 448;
                else if (Primary_Trace_Number == 37) Final_Result = 449;
                else if (Primary_Trace_Number == 38) Final_Result = 450;
                else if (Primary_Trace_Number == 39) Final_Result = 451;
                else if (Primary_Trace_Number == 40) Final_Result = 452;
                else if (Primary_Trace_Number == 41) Final_Result = 463;
                else if (Primary_Trace_Number == 42) Final_Result = 464;
                else if (Primary_Trace_Number == 43) Final_Result = 465;
                else if (Primary_Trace_Number == 44) Final_Result = 466;
                else if (Primary_Trace_Number == 45) Final_Result = 467;
                else if (Primary_Trace_Number == 46) Final_Result = 468;
                else if (Primary_Trace_Number == 47) Final_Result = 469;
                else if (Primary_Trace_Number == 48) Final_Result = 470;
                else if (Primary_Trace_Number == 49) Final_Result = 471;
                else if (Primary_Trace_Number == 50) Final_Result = 472;
                else if (Primary_Trace_Number == 51) Final_Result = 483;
                else if (Primary_Trace_Number == 52) Final_Result = 484;
                else if (Primary_Trace_Number == 53) Final_Result = 485;
                else if (Primary_Trace_Number == 54) Final_Result = 486;
                else if (Primary_Trace_Number == 55) Final_Result = 487;
                else if (Primary_Trace_Number == 56) Final_Result = 488;
                else if (Primary_Trace_Number == 57) Final_Result = 489;
                else if (Primary_Trace_Number == 58) Final_Result = 490;
                else if (Primary_Trace_Number == 59) Final_Result = 491;
                else if (Primary_Trace_Number == 60) Final_Result = 492;
                else if (Primary_Trace_Number == 61) Final_Result = 499;
                else if (Primary_Trace_Number == 62) Final_Result = 500;
                else if (Primary_Trace_Number == 63) Final_Result = 501;
                else if (Primary_Trace_Number == 64) Final_Result = 502;
                else if (Primary_Trace_Number == 65) Final_Result = 503;
                else if (Primary_Trace_Number == 66) Final_Result = 504;
                else if (Primary_Trace_Number == 67) Final_Result = 511;
                else if (Primary_Trace_Number == 68) Final_Result = 512;
                else if (Primary_Trace_Number == 69) Final_Result = 513;
                else if (Primary_Trace_Number == 70) Final_Result = 514;
                else if (Primary_Trace_Number == 71) Final_Result = 515;
                else if (Primary_Trace_Number == 72) Final_Result = 516;
                else if (Primary_Trace_Number == 73) Final_Result = 523;
                else if (Primary_Trace_Number == 74) Final_Result = 524;
                else if (Primary_Trace_Number == 75) Final_Result = 525;
                else if (Primary_Trace_Number == 76) Final_Result = 526;
                else if (Primary_Trace_Number == 77) Final_Result = 527;
                else if (Primary_Trace_Number == 78) Final_Result = 528;
                else if (Primary_Trace_Number == 79) Final_Result = 535;
                else if (Primary_Trace_Number == 80) Final_Result = 536;
                else if (Primary_Trace_Number == 81) Final_Result = 537;
                else if (Primary_Trace_Number == 82) Final_Result = 538;
                else if (Primary_Trace_Number == 83) Final_Result = 539;
                else if (Primary_Trace_Number == 84) Final_Result = 540;
                else if (Primary_Trace_Number == 85) Final_Result = 547;
                else if (Primary_Trace_Number == 86) Final_Result = 548;
                else if (Primary_Trace_Number == 87) Final_Result = 549;
                else if (Primary_Trace_Number == 88) Final_Result = 550;
                else if (Primary_Trace_Number == 89) Final_Result = 551;
                else if (Primary_Trace_Number == 90) Final_Result = 552;
                else if (Primary_Trace_Number == 91) Final_Result = 563;
                else if (Primary_Trace_Number == 92) Final_Result = 564;
                else if (Primary_Trace_Number == 93) Final_Result = 565;
                else if (Primary_Trace_Number == 94) Final_Result = 566;
                else if (Primary_Trace_Number == 95) Final_Result = 567;
                else if (Primary_Trace_Number == 96) Final_Result = 568;
                else if (Primary_Trace_Number == 97) Final_Result = 569;
                else if (Primary_Trace_Number == 98) Final_Result = 570;
                else if (Primary_Trace_Number == 99) Final_Result = 571;
                else if (Primary_Trace_Number == 100) Final_Result = 572;
                else if (Primary_Trace_Number == 101) Final_Result = 583;
                else if (Primary_Trace_Number == 102) Final_Result = 584;
                else if (Primary_Trace_Number == 103) Final_Result = 585;
                else if (Primary_Trace_Number == 104) Final_Result = 586;
                else if (Primary_Trace_Number == 105) Final_Result = 587;
                else if (Primary_Trace_Number == 106) Final_Result = 588;
                else if (Primary_Trace_Number == 107) Final_Result = 589;
                else if (Primary_Trace_Number == 108) Final_Result = 590;
                else if (Primary_Trace_Number == 109) Final_Result = 591;
                else if (Primary_Trace_Number == 110) Final_Result = 592;
                else if (Primary_Trace_Number == 111) Final_Result = 603;
                else if (Primary_Trace_Number == 112) Final_Result = 604;
                else if (Primary_Trace_Number == 113) Final_Result = 605;
                else if (Primary_Trace_Number == 114) Final_Result = 606;
                else if (Primary_Trace_Number == 115) Final_Result = 607;
                else if (Primary_Trace_Number == 116) Final_Result = 608;
                else if (Primary_Trace_Number == 117) Final_Result = 609;
                else if (Primary_Trace_Number == 118) Final_Result = 610;
                else if (Primary_Trace_Number == 119) Final_Result = 611;
                else if (Primary_Trace_Number == 120) Final_Result = 612;
                else if (Primary_Trace_Number == 121) Final_Result = 623;
                else if (Primary_Trace_Number == 122) Final_Result = 624;
                else if (Primary_Trace_Number == 123) Final_Result = 625;
                else if (Primary_Trace_Number == 124) Final_Result = 626;
                else if (Primary_Trace_Number == 125) Final_Result = 627;
                else if (Primary_Trace_Number == 126) Final_Result = 628;
                else if (Primary_Trace_Number == 127) Final_Result = 629;
                else if (Primary_Trace_Number == 128) Final_Result = 630;
                else if (Primary_Trace_Number == 129) Final_Result = 631;
                else if (Primary_Trace_Number == 130) Final_Result = 632;
                else if (Primary_Trace_Number == 131) Final_Result = 643;
                else if (Primary_Trace_Number == 132) Final_Result = 644;
                else if (Primary_Trace_Number == 133) Final_Result = 645;
                else if (Primary_Trace_Number == 134) Final_Result = 646;
                else if (Primary_Trace_Number == 135) Final_Result = 647;
                else if (Primary_Trace_Number == 136) Final_Result = 648;
                else if (Primary_Trace_Number == 137) Final_Result = 649;
                else if (Primary_Trace_Number == 138) Final_Result = 650;
                else if (Primary_Trace_Number == 139) Final_Result = 651;
                else if (Primary_Trace_Number == 140) Final_Result = 652;
                else if (Primary_Trace_Number == 141) Final_Result = 663;
                else if (Primary_Trace_Number == 142) Final_Result = 664;
                else if (Primary_Trace_Number == 143) Final_Result = 665;
                else if (Primary_Trace_Number == 144) Final_Result = 666;
                else if (Primary_Trace_Number == 145) Final_Result = 667;
                else if (Primary_Trace_Number == 146) Final_Result = 668;
                else if (Primary_Trace_Number == 147) Final_Result = 669;
                else if (Primary_Trace_Number == 148) Final_Result = 670;
                else if (Primary_Trace_Number == 149) Final_Result = 671;
                else if (Primary_Trace_Number == 150) Final_Result = 672;
                else if (Primary_Trace_Number == 151) Final_Result = 679;
                else if (Primary_Trace_Number == 152) Final_Result = 680;
                else if (Primary_Trace_Number == 153) Final_Result = 681;
                else if (Primary_Trace_Number == 154) Final_Result = 682;
                else if (Primary_Trace_Number == 155) Final_Result = 683;
                else if (Primary_Trace_Number == 156) Final_Result = 684;
                else if (Primary_Trace_Number == 157) Final_Result = 691;
                else if (Primary_Trace_Number == 158) Final_Result = 692;
                else if (Primary_Trace_Number == 159) Final_Result = 693;
                else if (Primary_Trace_Number == 160) Final_Result = 694;
                else if (Primary_Trace_Number == 161) Final_Result = 695;
                else if (Primary_Trace_Number == 162) Final_Result = 696;
                else if (Primary_Trace_Number == 163) Final_Result = 703;
                else if (Primary_Trace_Number == 164) Final_Result = 704;
                else if (Primary_Trace_Number == 165) Final_Result = 705;
                else if (Primary_Trace_Number == 166) Final_Result = 706;
                else if (Primary_Trace_Number == 167) Final_Result = 707;
                else if (Primary_Trace_Number == 168) Final_Result = 708;
                else if (Primary_Trace_Number == 169) Final_Result = 715;
                else if (Primary_Trace_Number == 170) Final_Result = 716;
                else if (Primary_Trace_Number == 171) Final_Result = 717;
                else if (Primary_Trace_Number == 172) Final_Result = 718;
                else if (Primary_Trace_Number == 173) Final_Result = 719;
                else if (Primary_Trace_Number == 174) Final_Result = 720;
                else if (Primary_Trace_Number == 175) Final_Result = 727;
                else if (Primary_Trace_Number == 176) Final_Result = 728;
                else if (Primary_Trace_Number == 177) Final_Result = 729;
                else if (Primary_Trace_Number == 178) Final_Result = 730;
                else if (Primary_Trace_Number == 179) Final_Result = 731;
                else if (Primary_Trace_Number == 180) Final_Result = 732;
                else if (Primary_Trace_Number == 181) Final_Result = 736;
                else if (Primary_Trace_Number == 182) Final_Result = 737;
                else if (Primary_Trace_Number == 183) Final_Result = 738;
                else if (Primary_Trace_Number == 184) Final_Result = 742;
                else if (Primary_Trace_Number == 185) Final_Result = 743;
                else if (Primary_Trace_Number == 186) Final_Result = 744;
                else Final_Result = 0;

                #endregion Site_3_Trace_Number
            }
            else
            {
                Final_Result = 0;
            }
            return Final_Result;
        }

        private static int Actual_Channel_Number(int Site_Number, int Primary_Channel_Number)
        {
            int Final_Result = 0;

            // Best and most universal now is to use table form for all setting
            if (Site_Number == 0)
            {
                #region Site_0_Channel_Number

                if (Primary_Channel_Number == 1) Final_Result = 1;
                else if (Primary_Channel_Number == 2) Final_Result = 3;
                else if (Primary_Channel_Number == 3) Final_Result = 5;
                else if (Primary_Channel_Number == 4) Final_Result = 7;
                else if (Primary_Channel_Number == 5) Final_Result = 9;
                else if (Primary_Channel_Number == 6) Final_Result = 11;
                else if (Primary_Channel_Number == 7) Final_Result = 13;
                else if (Primary_Channel_Number == 8) Final_Result = 15;
                else if (Primary_Channel_Number == 9) Final_Result = 17;
                else if (Primary_Channel_Number == 10) Final_Result = 19;
                else if (Primary_Channel_Number == 11) Final_Result = 21;
                else if (Primary_Channel_Number == 12) Final_Result = 23;
                else if (Primary_Channel_Number == 13) Final_Result = 25;
                else if (Primary_Channel_Number == 14) Final_Result = 27;
                else if (Primary_Channel_Number == 15) Final_Result = 29;
                else if (Primary_Channel_Number == 16) Final_Result = 31;
                else if (Primary_Channel_Number == 17) Final_Result = 33;
                else if (Primary_Channel_Number == 18) Final_Result = 35;
                else if (Primary_Channel_Number == 19) Final_Result = 37;
                else if (Primary_Channel_Number == 20) Final_Result = 39;
                else if (Primary_Channel_Number == 21) Final_Result = 41;
                else if (Primary_Channel_Number == 22) Final_Result = 43;
                else if (Primary_Channel_Number == 23) Final_Result = 45;
                else if (Primary_Channel_Number == 24) Final_Result = 47;
                else Final_Result = 0;

                #endregion Site_0_Channel_Number
            }
            else if (Site_Number == 1)
            {
                #region Site_1_Channel_Number

                if (Primary_Channel_Number == 1) Final_Result = 49;
                else if (Primary_Channel_Number == 2) Final_Result = 51;
                else if (Primary_Channel_Number == 3) Final_Result = 53;
                else if (Primary_Channel_Number == 4) Final_Result = 55;
                else if (Primary_Channel_Number == 5) Final_Result = 57;
                else if (Primary_Channel_Number == 6) Final_Result = 59;
                else if (Primary_Channel_Number == 7) Final_Result = 61;
                else if (Primary_Channel_Number == 8) Final_Result = 63;
                else if (Primary_Channel_Number == 9) Final_Result = 65;
                else if (Primary_Channel_Number == 10) Final_Result = 67;
                else if (Primary_Channel_Number == 11) Final_Result = 69;
                else if (Primary_Channel_Number == 12) Final_Result = 71;
                else if (Primary_Channel_Number == 13) Final_Result = 73;
                else if (Primary_Channel_Number == 14) Final_Result = 75;
                else if (Primary_Channel_Number == 15) Final_Result = 77;
                else if (Primary_Channel_Number == 16) Final_Result = 79;
                else if (Primary_Channel_Number == 17) Final_Result = 81;
                else if (Primary_Channel_Number == 18) Final_Result = 83;
                else if (Primary_Channel_Number == 19) Final_Result = 85;
                else if (Primary_Channel_Number == 20) Final_Result = 87;
                else if (Primary_Channel_Number == 21) Final_Result = 89;
                else if (Primary_Channel_Number == 22) Final_Result = 91;
                else if (Primary_Channel_Number == 23) Final_Result = 93;
                else if (Primary_Channel_Number == 24) Final_Result = 95;
                else Final_Result = 0;

                #endregion Site_1_Channel_Number
            }
            else if (Site_Number == 2)
            {
                #region Site_2_Channel_Number

                if (Primary_Channel_Number == 1) Final_Result = 2;
                else if (Primary_Channel_Number == 2) Final_Result = 4;
                else if (Primary_Channel_Number == 3) Final_Result = 6;
                else if (Primary_Channel_Number == 4) Final_Result = 8;
                else if (Primary_Channel_Number == 5) Final_Result = 10;
                else if (Primary_Channel_Number == 6) Final_Result = 12;
                else if (Primary_Channel_Number == 7) Final_Result = 14;
                else if (Primary_Channel_Number == 8) Final_Result = 16;
                else if (Primary_Channel_Number == 9) Final_Result = 18;
                else if (Primary_Channel_Number == 10) Final_Result = 20;
                else if (Primary_Channel_Number == 11) Final_Result = 22;
                else if (Primary_Channel_Number == 12) Final_Result = 24;
                else if (Primary_Channel_Number == 13) Final_Result = 26;
                else if (Primary_Channel_Number == 14) Final_Result = 28;
                else if (Primary_Channel_Number == 15) Final_Result = 30;
                else if (Primary_Channel_Number == 16) Final_Result = 32;
                else if (Primary_Channel_Number == 17) Final_Result = 34;
                else if (Primary_Channel_Number == 18) Final_Result = 36;
                else if (Primary_Channel_Number == 19) Final_Result = 38;
                else if (Primary_Channel_Number == 20) Final_Result = 40;
                else if (Primary_Channel_Number == 21) Final_Result = 42;
                else if (Primary_Channel_Number == 22) Final_Result = 44;
                else if (Primary_Channel_Number == 23) Final_Result = 46;
                else if (Primary_Channel_Number == 24) Final_Result = 48;
                else Final_Result = 0;

                #endregion Site_2_Channel_Number
            }
            else if (Site_Number == 3)
            {
                #region Site_3_Channel_Number

                if (Primary_Channel_Number == 1) Final_Result = 50;
                else if (Primary_Channel_Number == 2) Final_Result = 52;
                else if (Primary_Channel_Number == 3) Final_Result = 54;
                else if (Primary_Channel_Number == 4) Final_Result = 56;
                else if (Primary_Channel_Number == 5) Final_Result = 58;
                else if (Primary_Channel_Number == 6) Final_Result = 60;
                else if (Primary_Channel_Number == 7) Final_Result = 62;
                else if (Primary_Channel_Number == 8) Final_Result = 64;
                else if (Primary_Channel_Number == 9) Final_Result = 66;
                else if (Primary_Channel_Number == 10) Final_Result = 68;
                else if (Primary_Channel_Number == 11) Final_Result = 70;
                else if (Primary_Channel_Number == 12) Final_Result = 72;
                else if (Primary_Channel_Number == 13) Final_Result = 74;
                else if (Primary_Channel_Number == 14) Final_Result = 76;
                else if (Primary_Channel_Number == 15) Final_Result = 78;
                else if (Primary_Channel_Number == 16) Final_Result = 80;
                else if (Primary_Channel_Number == 17) Final_Result = 82;
                else if (Primary_Channel_Number == 18) Final_Result = 84;
                else if (Primary_Channel_Number == 19) Final_Result = 86;
                else if (Primary_Channel_Number == 20) Final_Result = 88;
                else if (Primary_Channel_Number == 21) Final_Result = 90;
                else if (Primary_Channel_Number == 22) Final_Result = 92;
                else if (Primary_Channel_Number == 23) Final_Result = 94;
                else if (Primary_Channel_Number == 24) Final_Result = 96;
                else Final_Result = 0;

                #endregion Site_3_Channel_Number
            }
            else
            {
                Final_Result = 0;
            }
            return Final_Result;
        }

        private static int Actual_SingleTraceInChannel_Number(int Site_Number, int Primary_Trace_Number)
        {
            int Final_Result = 0;

            // Best and most universal now is to use table form for all setting
            if (Site_Number == 0)
            {
                #region Site_0_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 1;
                else if (Primary_Trace_Number == 2) Final_Result = 1;
                else if (Primary_Trace_Number == 3) Final_Result = 1;
                else if (Primary_Trace_Number == 4) Final_Result = 1;
                else if (Primary_Trace_Number == 5) Final_Result = 1;
                else if (Primary_Trace_Number == 6) Final_Result = 1;
                else if (Primary_Trace_Number == 7) Final_Result = 1;
                else if (Primary_Trace_Number == 8) Final_Result = 1;
                else if (Primary_Trace_Number == 9) Final_Result = 1;
                else if (Primary_Trace_Number == 10) Final_Result = 1;
                else if (Primary_Trace_Number == 11) Final_Result = 21;
                else if (Primary_Trace_Number == 12) Final_Result = 21;
                else if (Primary_Trace_Number == 13) Final_Result = 21;
                else if (Primary_Trace_Number == 14) Final_Result = 21;
                else if (Primary_Trace_Number == 15) Final_Result = 21;
                else if (Primary_Trace_Number == 16) Final_Result = 21;
                else if (Primary_Trace_Number == 17) Final_Result = 21;
                else if (Primary_Trace_Number == 18) Final_Result = 21;
                else if (Primary_Trace_Number == 19) Final_Result = 21;
                else if (Primary_Trace_Number == 20) Final_Result = 21;
                else if (Primary_Trace_Number == 21) Final_Result = 41;
                else if (Primary_Trace_Number == 22) Final_Result = 41;
                else if (Primary_Trace_Number == 23) Final_Result = 41;
                else if (Primary_Trace_Number == 24) Final_Result = 41;
                else if (Primary_Trace_Number == 25) Final_Result = 41;
                else if (Primary_Trace_Number == 26) Final_Result = 41;
                else if (Primary_Trace_Number == 27) Final_Result = 41;
                else if (Primary_Trace_Number == 28) Final_Result = 41;
                else if (Primary_Trace_Number == 29) Final_Result = 41;
                else if (Primary_Trace_Number == 30) Final_Result = 41;
                else if (Primary_Trace_Number == 31) Final_Result = 61;
                else if (Primary_Trace_Number == 32) Final_Result = 61;
                else if (Primary_Trace_Number == 33) Final_Result = 61;
                else if (Primary_Trace_Number == 34) Final_Result = 61;
                else if (Primary_Trace_Number == 35) Final_Result = 61;
                else if (Primary_Trace_Number == 36) Final_Result = 61;
                else if (Primary_Trace_Number == 37) Final_Result = 61;
                else if (Primary_Trace_Number == 38) Final_Result = 61;
                else if (Primary_Trace_Number == 39) Final_Result = 61;
                else if (Primary_Trace_Number == 40) Final_Result = 61;
                else if (Primary_Trace_Number == 41) Final_Result = 81;
                else if (Primary_Trace_Number == 42) Final_Result = 81;
                else if (Primary_Trace_Number == 43) Final_Result = 81;
                else if (Primary_Trace_Number == 44) Final_Result = 81;
                else if (Primary_Trace_Number == 45) Final_Result = 81;
                else if (Primary_Trace_Number == 46) Final_Result = 81;
                else if (Primary_Trace_Number == 47) Final_Result = 81;
                else if (Primary_Trace_Number == 48) Final_Result = 81;
                else if (Primary_Trace_Number == 49) Final_Result = 81;
                else if (Primary_Trace_Number == 50) Final_Result = 81;
                else if (Primary_Trace_Number == 51) Final_Result = 101;
                else if (Primary_Trace_Number == 52) Final_Result = 101;
                else if (Primary_Trace_Number == 53) Final_Result = 101;
                else if (Primary_Trace_Number == 54) Final_Result = 101;
                else if (Primary_Trace_Number == 55) Final_Result = 101;
                else if (Primary_Trace_Number == 56) Final_Result = 101;
                else if (Primary_Trace_Number == 57) Final_Result = 101;
                else if (Primary_Trace_Number == 58) Final_Result = 101;
                else if (Primary_Trace_Number == 59) Final_Result = 101;
                else if (Primary_Trace_Number == 60) Final_Result = 101;
                else if (Primary_Trace_Number == 61) Final_Result = 121;
                else if (Primary_Trace_Number == 62) Final_Result = 122;
                else if (Primary_Trace_Number == 63) Final_Result = 123;
                else if (Primary_Trace_Number == 64) Final_Result = 124;
                else if (Primary_Trace_Number == 65) Final_Result = 125;
                else if (Primary_Trace_Number == 66) Final_Result = 126;
                else if (Primary_Trace_Number == 67) Final_Result = 133;
                else if (Primary_Trace_Number == 68) Final_Result = 133;
                else if (Primary_Trace_Number == 69) Final_Result = 133;
                else if (Primary_Trace_Number == 70) Final_Result = 133;
                else if (Primary_Trace_Number == 71) Final_Result = 133;
                else if (Primary_Trace_Number == 72) Final_Result = 133;
                else if (Primary_Trace_Number == 73) Final_Result = 145;
                else if (Primary_Trace_Number == 74) Final_Result = 145;
                else if (Primary_Trace_Number == 75) Final_Result = 145;
                else if (Primary_Trace_Number == 76) Final_Result = 145;
                else if (Primary_Trace_Number == 77) Final_Result = 145;
                else if (Primary_Trace_Number == 78) Final_Result = 145;
                else if (Primary_Trace_Number == 79) Final_Result = 157;
                else if (Primary_Trace_Number == 80) Final_Result = 157;
                else if (Primary_Trace_Number == 81) Final_Result = 157;
                else if (Primary_Trace_Number == 82) Final_Result = 157;
                else if (Primary_Trace_Number == 83) Final_Result = 157;
                else if (Primary_Trace_Number == 84) Final_Result = 157;
                else if (Primary_Trace_Number == 85) Final_Result = 169;
                else if (Primary_Trace_Number == 86) Final_Result = 169;
                else if (Primary_Trace_Number == 87) Final_Result = 169;
                else if (Primary_Trace_Number == 88) Final_Result = 169;
                else if (Primary_Trace_Number == 89) Final_Result = 169;
                else if (Primary_Trace_Number == 90) Final_Result = 169;
                else if (Primary_Trace_Number == 91) Final_Result = 181;
                else if (Primary_Trace_Number == 92) Final_Result = 181;
                else if (Primary_Trace_Number == 93) Final_Result = 181;
                else if (Primary_Trace_Number == 94) Final_Result = 181;
                else if (Primary_Trace_Number == 95) Final_Result = 181;
                else if (Primary_Trace_Number == 96) Final_Result = 181;
                else if (Primary_Trace_Number == 97) Final_Result = 181;
                else if (Primary_Trace_Number == 98) Final_Result = 181;
                else if (Primary_Trace_Number == 99) Final_Result = 181;
                else if (Primary_Trace_Number == 100) Final_Result = 181;
                else if (Primary_Trace_Number == 101) Final_Result = 201;
                else if (Primary_Trace_Number == 102) Final_Result = 201;
                else if (Primary_Trace_Number == 103) Final_Result = 201;
                else if (Primary_Trace_Number == 104) Final_Result = 201;
                else if (Primary_Trace_Number == 105) Final_Result = 201;
                else if (Primary_Trace_Number == 106) Final_Result = 201;
                else if (Primary_Trace_Number == 107) Final_Result = 201;
                else if (Primary_Trace_Number == 108) Final_Result = 201;
                else if (Primary_Trace_Number == 109) Final_Result = 201;
                else if (Primary_Trace_Number == 110) Final_Result = 201;
                else if (Primary_Trace_Number == 111) Final_Result = 221;
                else if (Primary_Trace_Number == 112) Final_Result = 221;
                else if (Primary_Trace_Number == 113) Final_Result = 221;
                else if (Primary_Trace_Number == 114) Final_Result = 221;
                else if (Primary_Trace_Number == 115) Final_Result = 221;
                else if (Primary_Trace_Number == 116) Final_Result = 221;
                else if (Primary_Trace_Number == 117) Final_Result = 221;
                else if (Primary_Trace_Number == 118) Final_Result = 221;
                else if (Primary_Trace_Number == 119) Final_Result = 221;
                else if (Primary_Trace_Number == 120) Final_Result = 221;
                else if (Primary_Trace_Number == 121) Final_Result = 241;
                else if (Primary_Trace_Number == 122) Final_Result = 241;
                else if (Primary_Trace_Number == 123) Final_Result = 241;
                else if (Primary_Trace_Number == 124) Final_Result = 241;
                else if (Primary_Trace_Number == 125) Final_Result = 241;
                else if (Primary_Trace_Number == 126) Final_Result = 241;
                else if (Primary_Trace_Number == 127) Final_Result = 241;
                else if (Primary_Trace_Number == 128) Final_Result = 241;
                else if (Primary_Trace_Number == 129) Final_Result = 241;
                else if (Primary_Trace_Number == 130) Final_Result = 241;
                else if (Primary_Trace_Number == 131) Final_Result = 261;
                else if (Primary_Trace_Number == 132) Final_Result = 261;
                else if (Primary_Trace_Number == 133) Final_Result = 261;
                else if (Primary_Trace_Number == 134) Final_Result = 261;
                else if (Primary_Trace_Number == 135) Final_Result = 261;
                else if (Primary_Trace_Number == 136) Final_Result = 261;
                else if (Primary_Trace_Number == 137) Final_Result = 261;
                else if (Primary_Trace_Number == 138) Final_Result = 261;
                else if (Primary_Trace_Number == 139) Final_Result = 261;
                else if (Primary_Trace_Number == 140) Final_Result = 261;
                else if (Primary_Trace_Number == 141) Final_Result = 281;
                else if (Primary_Trace_Number == 142) Final_Result = 281;
                else if (Primary_Trace_Number == 143) Final_Result = 281;
                else if (Primary_Trace_Number == 144) Final_Result = 281;
                else if (Primary_Trace_Number == 145) Final_Result = 281;
                else if (Primary_Trace_Number == 146) Final_Result = 281;
                else if (Primary_Trace_Number == 147) Final_Result = 281;
                else if (Primary_Trace_Number == 148) Final_Result = 281;
                else if (Primary_Trace_Number == 149) Final_Result = 281;
                else if (Primary_Trace_Number == 150) Final_Result = 281;
                else if (Primary_Trace_Number == 151) Final_Result = 301;
                else if (Primary_Trace_Number == 152) Final_Result = 301;
                else if (Primary_Trace_Number == 153) Final_Result = 301;
                else if (Primary_Trace_Number == 154) Final_Result = 301;
                else if (Primary_Trace_Number == 155) Final_Result = 301;
                else if (Primary_Trace_Number == 156) Final_Result = 301;
                else if (Primary_Trace_Number == 157) Final_Result = 313;
                else if (Primary_Trace_Number == 158) Final_Result = 313;
                else if (Primary_Trace_Number == 159) Final_Result = 313;
                else if (Primary_Trace_Number == 160) Final_Result = 313;
                else if (Primary_Trace_Number == 161) Final_Result = 313;
                else if (Primary_Trace_Number == 162) Final_Result = 313;
                else if (Primary_Trace_Number == 163) Final_Result = 325;
                else if (Primary_Trace_Number == 164) Final_Result = 325;
                else if (Primary_Trace_Number == 165) Final_Result = 325;
                else if (Primary_Trace_Number == 166) Final_Result = 325;
                else if (Primary_Trace_Number == 167) Final_Result = 325;
                else if (Primary_Trace_Number == 168) Final_Result = 325;
                else if (Primary_Trace_Number == 169) Final_Result = 337;
                else if (Primary_Trace_Number == 170) Final_Result = 337;
                else if (Primary_Trace_Number == 171) Final_Result = 337;
                else if (Primary_Trace_Number == 172) Final_Result = 337;
                else if (Primary_Trace_Number == 173) Final_Result = 337;
                else if (Primary_Trace_Number == 174) Final_Result = 337;
                else if (Primary_Trace_Number == 175) Final_Result = 349;
                else if (Primary_Trace_Number == 176) Final_Result = 349;
                else if (Primary_Trace_Number == 177) Final_Result = 349;
                else if (Primary_Trace_Number == 178) Final_Result = 349;
                else if (Primary_Trace_Number == 179) Final_Result = 349;
                else if (Primary_Trace_Number == 180) Final_Result = 349;
                else if (Primary_Trace_Number == 181) Final_Result = 361;
                else if (Primary_Trace_Number == 182) Final_Result = 361;
                else if (Primary_Trace_Number == 183) Final_Result = 361;
                else if (Primary_Trace_Number == 184) Final_Result = 367;
                else if (Primary_Trace_Number == 185) Final_Result = 367;
                else if (Primary_Trace_Number == 186) Final_Result = 367;
                else Final_Result = 0;

                #endregion Site_0_Trace_Number
            }
            else if (Site_Number == 1)
            {
                #region Site_1_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 373;
                else if (Primary_Trace_Number == 2) Final_Result = 373;
                else if (Primary_Trace_Number == 3) Final_Result = 373;
                else if (Primary_Trace_Number == 4) Final_Result = 373;
                else if (Primary_Trace_Number == 5) Final_Result = 373;
                else if (Primary_Trace_Number == 6) Final_Result = 373;
                else if (Primary_Trace_Number == 7) Final_Result = 373;
                else if (Primary_Trace_Number == 8) Final_Result = 373;
                else if (Primary_Trace_Number == 9) Final_Result = 373;
                else if (Primary_Trace_Number == 10) Final_Result = 373;
                else if (Primary_Trace_Number == 11) Final_Result = 393;
                else if (Primary_Trace_Number == 12) Final_Result = 393;
                else if (Primary_Trace_Number == 13) Final_Result = 393;
                else if (Primary_Trace_Number == 14) Final_Result = 393;
                else if (Primary_Trace_Number == 15) Final_Result = 393;
                else if (Primary_Trace_Number == 16) Final_Result = 393;
                else if (Primary_Trace_Number == 17) Final_Result = 393;
                else if (Primary_Trace_Number == 18) Final_Result = 393;
                else if (Primary_Trace_Number == 19) Final_Result = 393;
                else if (Primary_Trace_Number == 20) Final_Result = 393;
                else if (Primary_Trace_Number == 21) Final_Result = 413;
                else if (Primary_Trace_Number == 22) Final_Result = 413;
                else if (Primary_Trace_Number == 23) Final_Result = 413;
                else if (Primary_Trace_Number == 24) Final_Result = 413;
                else if (Primary_Trace_Number == 25) Final_Result = 413;
                else if (Primary_Trace_Number == 26) Final_Result = 413;
                else if (Primary_Trace_Number == 27) Final_Result = 413;
                else if (Primary_Trace_Number == 28) Final_Result = 413;
                else if (Primary_Trace_Number == 29) Final_Result = 413;
                else if (Primary_Trace_Number == 30) Final_Result = 413;
                else if (Primary_Trace_Number == 31) Final_Result = 433;
                else if (Primary_Trace_Number == 32) Final_Result = 433;
                else if (Primary_Trace_Number == 33) Final_Result = 433;
                else if (Primary_Trace_Number == 34) Final_Result = 433;
                else if (Primary_Trace_Number == 35) Final_Result = 433;
                else if (Primary_Trace_Number == 36) Final_Result = 433;
                else if (Primary_Trace_Number == 37) Final_Result = 433;
                else if (Primary_Trace_Number == 38) Final_Result = 433;
                else if (Primary_Trace_Number == 39) Final_Result = 433;
                else if (Primary_Trace_Number == 40) Final_Result = 433;
                else if (Primary_Trace_Number == 41) Final_Result = 453;
                else if (Primary_Trace_Number == 42) Final_Result = 453;
                else if (Primary_Trace_Number == 43) Final_Result = 453;
                else if (Primary_Trace_Number == 44) Final_Result = 453;
                else if (Primary_Trace_Number == 45) Final_Result = 453;
                else if (Primary_Trace_Number == 46) Final_Result = 453;
                else if (Primary_Trace_Number == 47) Final_Result = 453;
                else if (Primary_Trace_Number == 48) Final_Result = 453;
                else if (Primary_Trace_Number == 49) Final_Result = 453;
                else if (Primary_Trace_Number == 50) Final_Result = 453;
                else if (Primary_Trace_Number == 51) Final_Result = 473;
                else if (Primary_Trace_Number == 52) Final_Result = 473;
                else if (Primary_Trace_Number == 53) Final_Result = 473;
                else if (Primary_Trace_Number == 54) Final_Result = 473;
                else if (Primary_Trace_Number == 55) Final_Result = 473;
                else if (Primary_Trace_Number == 56) Final_Result = 473;
                else if (Primary_Trace_Number == 57) Final_Result = 473;
                else if (Primary_Trace_Number == 58) Final_Result = 473;
                else if (Primary_Trace_Number == 59) Final_Result = 473;
                else if (Primary_Trace_Number == 60) Final_Result = 473;
                else if (Primary_Trace_Number == 61) Final_Result = 493;
                else if (Primary_Trace_Number == 62) Final_Result = 493;
                else if (Primary_Trace_Number == 63) Final_Result = 493;
                else if (Primary_Trace_Number == 64) Final_Result = 493;
                else if (Primary_Trace_Number == 65) Final_Result = 493;
                else if (Primary_Trace_Number == 66) Final_Result = 493;
                else if (Primary_Trace_Number == 67) Final_Result = 505;
                else if (Primary_Trace_Number == 68) Final_Result = 505;
                else if (Primary_Trace_Number == 69) Final_Result = 505;
                else if (Primary_Trace_Number == 70) Final_Result = 505;
                else if (Primary_Trace_Number == 71) Final_Result = 505;
                else if (Primary_Trace_Number == 72) Final_Result = 505;
                else if (Primary_Trace_Number == 73) Final_Result = 517;
                else if (Primary_Trace_Number == 74) Final_Result = 517;
                else if (Primary_Trace_Number == 75) Final_Result = 517;
                else if (Primary_Trace_Number == 76) Final_Result = 517;
                else if (Primary_Trace_Number == 77) Final_Result = 517;
                else if (Primary_Trace_Number == 78) Final_Result = 517;
                else if (Primary_Trace_Number == 79) Final_Result = 529;
                else if (Primary_Trace_Number == 80) Final_Result = 529;
                else if (Primary_Trace_Number == 81) Final_Result = 529;
                else if (Primary_Trace_Number == 82) Final_Result = 529;
                else if (Primary_Trace_Number == 83) Final_Result = 529;
                else if (Primary_Trace_Number == 84) Final_Result = 529;
                else if (Primary_Trace_Number == 85) Final_Result = 541;
                else if (Primary_Trace_Number == 86) Final_Result = 541;
                else if (Primary_Trace_Number == 87) Final_Result = 541;
                else if (Primary_Trace_Number == 88) Final_Result = 541;
                else if (Primary_Trace_Number == 89) Final_Result = 541;
                else if (Primary_Trace_Number == 90) Final_Result = 541;
                else if (Primary_Trace_Number == 91) Final_Result = 553;
                else if (Primary_Trace_Number == 92) Final_Result = 553;
                else if (Primary_Trace_Number == 93) Final_Result = 553;
                else if (Primary_Trace_Number == 94) Final_Result = 553;
                else if (Primary_Trace_Number == 95) Final_Result = 553;
                else if (Primary_Trace_Number == 96) Final_Result = 553;
                else if (Primary_Trace_Number == 97) Final_Result = 553;
                else if (Primary_Trace_Number == 98) Final_Result = 553;
                else if (Primary_Trace_Number == 99) Final_Result = 553;
                else if (Primary_Trace_Number == 100) Final_Result = 553;
                else if (Primary_Trace_Number == 101) Final_Result = 573;
                else if (Primary_Trace_Number == 102) Final_Result = 573;
                else if (Primary_Trace_Number == 103) Final_Result = 573;
                else if (Primary_Trace_Number == 104) Final_Result = 573;
                else if (Primary_Trace_Number == 105) Final_Result = 573;
                else if (Primary_Trace_Number == 106) Final_Result = 573;
                else if (Primary_Trace_Number == 107) Final_Result = 573;
                else if (Primary_Trace_Number == 108) Final_Result = 573;
                else if (Primary_Trace_Number == 109) Final_Result = 573;
                else if (Primary_Trace_Number == 110) Final_Result = 573;
                else if (Primary_Trace_Number == 111) Final_Result = 593;
                else if (Primary_Trace_Number == 112) Final_Result = 593;
                else if (Primary_Trace_Number == 113) Final_Result = 593;
                else if (Primary_Trace_Number == 114) Final_Result = 593;
                else if (Primary_Trace_Number == 115) Final_Result = 593;
                else if (Primary_Trace_Number == 116) Final_Result = 593;
                else if (Primary_Trace_Number == 117) Final_Result = 593;
                else if (Primary_Trace_Number == 118) Final_Result = 593;
                else if (Primary_Trace_Number == 119) Final_Result = 593;
                else if (Primary_Trace_Number == 120) Final_Result = 593;
                else if (Primary_Trace_Number == 121) Final_Result = 613;
                else if (Primary_Trace_Number == 122) Final_Result = 613;
                else if (Primary_Trace_Number == 123) Final_Result = 613;
                else if (Primary_Trace_Number == 124) Final_Result = 613;
                else if (Primary_Trace_Number == 125) Final_Result = 613;
                else if (Primary_Trace_Number == 126) Final_Result = 613;
                else if (Primary_Trace_Number == 127) Final_Result = 613;
                else if (Primary_Trace_Number == 128) Final_Result = 613;
                else if (Primary_Trace_Number == 129) Final_Result = 613;
                else if (Primary_Trace_Number == 130) Final_Result = 613;
                else if (Primary_Trace_Number == 131) Final_Result = 633;
                else if (Primary_Trace_Number == 132) Final_Result = 633;
                else if (Primary_Trace_Number == 133) Final_Result = 633;
                else if (Primary_Trace_Number == 134) Final_Result = 633;
                else if (Primary_Trace_Number == 135) Final_Result = 633;
                else if (Primary_Trace_Number == 136) Final_Result = 633;
                else if (Primary_Trace_Number == 137) Final_Result = 633;
                else if (Primary_Trace_Number == 138) Final_Result = 633;
                else if (Primary_Trace_Number == 139) Final_Result = 633;
                else if (Primary_Trace_Number == 140) Final_Result = 633;
                else if (Primary_Trace_Number == 141) Final_Result = 653;
                else if (Primary_Trace_Number == 142) Final_Result = 653;
                else if (Primary_Trace_Number == 143) Final_Result = 653;
                else if (Primary_Trace_Number == 144) Final_Result = 653;
                else if (Primary_Trace_Number == 145) Final_Result = 653;
                else if (Primary_Trace_Number == 146) Final_Result = 653;
                else if (Primary_Trace_Number == 147) Final_Result = 653;
                else if (Primary_Trace_Number == 148) Final_Result = 653;
                else if (Primary_Trace_Number == 149) Final_Result = 653;
                else if (Primary_Trace_Number == 150) Final_Result = 653;
                else if (Primary_Trace_Number == 151) Final_Result = 673;
                else if (Primary_Trace_Number == 152) Final_Result = 673;
                else if (Primary_Trace_Number == 153) Final_Result = 673;
                else if (Primary_Trace_Number == 154) Final_Result = 673;
                else if (Primary_Trace_Number == 155) Final_Result = 673;
                else if (Primary_Trace_Number == 156) Final_Result = 673;
                else if (Primary_Trace_Number == 157) Final_Result = 685;
                else if (Primary_Trace_Number == 158) Final_Result = 685;
                else if (Primary_Trace_Number == 159) Final_Result = 685;
                else if (Primary_Trace_Number == 160) Final_Result = 685;
                else if (Primary_Trace_Number == 161) Final_Result = 685;
                else if (Primary_Trace_Number == 162) Final_Result = 685;
                else if (Primary_Trace_Number == 163) Final_Result = 697;
                else if (Primary_Trace_Number == 164) Final_Result = 697;
                else if (Primary_Trace_Number == 165) Final_Result = 697;
                else if (Primary_Trace_Number == 166) Final_Result = 697;
                else if (Primary_Trace_Number == 167) Final_Result = 697;
                else if (Primary_Trace_Number == 168) Final_Result = 697;
                else if (Primary_Trace_Number == 169) Final_Result = 709;
                else if (Primary_Trace_Number == 170) Final_Result = 709;
                else if (Primary_Trace_Number == 171) Final_Result = 709;
                else if (Primary_Trace_Number == 172) Final_Result = 709;
                else if (Primary_Trace_Number == 173) Final_Result = 709;
                else if (Primary_Trace_Number == 174) Final_Result = 709;
                else if (Primary_Trace_Number == 175) Final_Result = 721;
                else if (Primary_Trace_Number == 176) Final_Result = 721;
                else if (Primary_Trace_Number == 177) Final_Result = 721;
                else if (Primary_Trace_Number == 178) Final_Result = 721;
                else if (Primary_Trace_Number == 179) Final_Result = 721;
                else if (Primary_Trace_Number == 180) Final_Result = 721;
                else if (Primary_Trace_Number == 181) Final_Result = 733;
                else if (Primary_Trace_Number == 182) Final_Result = 733;
                else if (Primary_Trace_Number == 183) Final_Result = 733;
                else if (Primary_Trace_Number == 184) Final_Result = 739;
                else if (Primary_Trace_Number == 185) Final_Result = 739;
                else if (Primary_Trace_Number == 186) Final_Result = 739;
                else Final_Result = 0;

                #endregion Site_1_Trace_Number
            }
            else if (Site_Number == 2)
            {
                #region Site_2_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 11;
                else if (Primary_Trace_Number == 2) Final_Result = 11;
                else if (Primary_Trace_Number == 3) Final_Result = 11;
                else if (Primary_Trace_Number == 4) Final_Result = 11;
                else if (Primary_Trace_Number == 5) Final_Result = 11;
                else if (Primary_Trace_Number == 6) Final_Result = 11;
                else if (Primary_Trace_Number == 7) Final_Result = 11;
                else if (Primary_Trace_Number == 8) Final_Result = 11;
                else if (Primary_Trace_Number == 9) Final_Result = 11;
                else if (Primary_Trace_Number == 10) Final_Result = 11;
                else if (Primary_Trace_Number == 11) Final_Result = 31;
                else if (Primary_Trace_Number == 12) Final_Result = 31;
                else if (Primary_Trace_Number == 13) Final_Result = 31;
                else if (Primary_Trace_Number == 14) Final_Result = 31;
                else if (Primary_Trace_Number == 15) Final_Result = 31;
                else if (Primary_Trace_Number == 16) Final_Result = 31;
                else if (Primary_Trace_Number == 17) Final_Result = 31;
                else if (Primary_Trace_Number == 18) Final_Result = 31;
                else if (Primary_Trace_Number == 19) Final_Result = 31;
                else if (Primary_Trace_Number == 20) Final_Result = 31;
                else if (Primary_Trace_Number == 21) Final_Result = 51;
                else if (Primary_Trace_Number == 22) Final_Result = 51;
                else if (Primary_Trace_Number == 23) Final_Result = 51;
                else if (Primary_Trace_Number == 24) Final_Result = 51;
                else if (Primary_Trace_Number == 25) Final_Result = 51;
                else if (Primary_Trace_Number == 26) Final_Result = 51;
                else if (Primary_Trace_Number == 27) Final_Result = 51;
                else if (Primary_Trace_Number == 28) Final_Result = 51;
                else if (Primary_Trace_Number == 29) Final_Result = 51;
                else if (Primary_Trace_Number == 30) Final_Result = 51;
                else if (Primary_Trace_Number == 31) Final_Result = 71;
                else if (Primary_Trace_Number == 32) Final_Result = 71;
                else if (Primary_Trace_Number == 33) Final_Result = 71;
                else if (Primary_Trace_Number == 34) Final_Result = 71;
                else if (Primary_Trace_Number == 35) Final_Result = 71;
                else if (Primary_Trace_Number == 36) Final_Result = 71;
                else if (Primary_Trace_Number == 37) Final_Result = 71;
                else if (Primary_Trace_Number == 38) Final_Result = 71;
                else if (Primary_Trace_Number == 39) Final_Result = 71;
                else if (Primary_Trace_Number == 40) Final_Result = 71;
                else if (Primary_Trace_Number == 41) Final_Result = 91;
                else if (Primary_Trace_Number == 42) Final_Result = 91;
                else if (Primary_Trace_Number == 43) Final_Result = 91;
                else if (Primary_Trace_Number == 44) Final_Result = 91;
                else if (Primary_Trace_Number == 45) Final_Result = 91;
                else if (Primary_Trace_Number == 46) Final_Result = 91;
                else if (Primary_Trace_Number == 47) Final_Result = 91;
                else if (Primary_Trace_Number == 48) Final_Result = 91;
                else if (Primary_Trace_Number == 49) Final_Result = 91;
                else if (Primary_Trace_Number == 50) Final_Result = 91;
                else if (Primary_Trace_Number == 51) Final_Result = 111;
                else if (Primary_Trace_Number == 52) Final_Result = 111;
                else if (Primary_Trace_Number == 53) Final_Result = 111;
                else if (Primary_Trace_Number == 54) Final_Result = 111;
                else if (Primary_Trace_Number == 55) Final_Result = 111;
                else if (Primary_Trace_Number == 56) Final_Result = 111;
                else if (Primary_Trace_Number == 57) Final_Result = 111;
                else if (Primary_Trace_Number == 58) Final_Result = 111;
                else if (Primary_Trace_Number == 59) Final_Result = 111;
                else if (Primary_Trace_Number == 60) Final_Result = 111;
                else if (Primary_Trace_Number == 61) Final_Result = 127;
                else if (Primary_Trace_Number == 62) Final_Result = 127;
                else if (Primary_Trace_Number == 63) Final_Result = 127;
                else if (Primary_Trace_Number == 64) Final_Result = 127;
                else if (Primary_Trace_Number == 65) Final_Result = 127;
                else if (Primary_Trace_Number == 66) Final_Result = 127;
                else if (Primary_Trace_Number == 67) Final_Result = 139;
                else if (Primary_Trace_Number == 68) Final_Result = 139;
                else if (Primary_Trace_Number == 69) Final_Result = 139;
                else if (Primary_Trace_Number == 70) Final_Result = 139;
                else if (Primary_Trace_Number == 71) Final_Result = 139;
                else if (Primary_Trace_Number == 72) Final_Result = 139;
                else if (Primary_Trace_Number == 73) Final_Result = 151;
                else if (Primary_Trace_Number == 74) Final_Result = 151;
                else if (Primary_Trace_Number == 75) Final_Result = 151;
                else if (Primary_Trace_Number == 76) Final_Result = 151;
                else if (Primary_Trace_Number == 77) Final_Result = 151;
                else if (Primary_Trace_Number == 78) Final_Result = 151;
                else if (Primary_Trace_Number == 79) Final_Result = 163;
                else if (Primary_Trace_Number == 80) Final_Result = 163;
                else if (Primary_Trace_Number == 81) Final_Result = 163;
                else if (Primary_Trace_Number == 82) Final_Result = 163;
                else if (Primary_Trace_Number == 83) Final_Result = 163;
                else if (Primary_Trace_Number == 84) Final_Result = 163;
                else if (Primary_Trace_Number == 85) Final_Result = 175;
                else if (Primary_Trace_Number == 86) Final_Result = 175;
                else if (Primary_Trace_Number == 87) Final_Result = 175;
                else if (Primary_Trace_Number == 88) Final_Result = 175;
                else if (Primary_Trace_Number == 89) Final_Result = 175;
                else if (Primary_Trace_Number == 90) Final_Result = 175;
                else if (Primary_Trace_Number == 91) Final_Result = 191;
                else if (Primary_Trace_Number == 92) Final_Result = 191;
                else if (Primary_Trace_Number == 93) Final_Result = 191;
                else if (Primary_Trace_Number == 94) Final_Result = 191;
                else if (Primary_Trace_Number == 95) Final_Result = 191;
                else if (Primary_Trace_Number == 96) Final_Result = 191;
                else if (Primary_Trace_Number == 97) Final_Result = 191;
                else if (Primary_Trace_Number == 98) Final_Result = 191;
                else if (Primary_Trace_Number == 99) Final_Result = 191;
                else if (Primary_Trace_Number == 100) Final_Result = 191;
                else if (Primary_Trace_Number == 101) Final_Result = 211;
                else if (Primary_Trace_Number == 102) Final_Result = 211;
                else if (Primary_Trace_Number == 103) Final_Result = 211;
                else if (Primary_Trace_Number == 104) Final_Result = 211;
                else if (Primary_Trace_Number == 105) Final_Result = 211;
                else if (Primary_Trace_Number == 106) Final_Result = 211;
                else if (Primary_Trace_Number == 107) Final_Result = 211;
                else if (Primary_Trace_Number == 108) Final_Result = 211;
                else if (Primary_Trace_Number == 109) Final_Result = 211;
                else if (Primary_Trace_Number == 110) Final_Result = 211;
                else if (Primary_Trace_Number == 111) Final_Result = 231;
                else if (Primary_Trace_Number == 112) Final_Result = 231;
                else if (Primary_Trace_Number == 113) Final_Result = 231;
                else if (Primary_Trace_Number == 114) Final_Result = 231;
                else if (Primary_Trace_Number == 115) Final_Result = 231;
                else if (Primary_Trace_Number == 116) Final_Result = 231;
                else if (Primary_Trace_Number == 117) Final_Result = 231;
                else if (Primary_Trace_Number == 118) Final_Result = 231;
                else if (Primary_Trace_Number == 119) Final_Result = 231;
                else if (Primary_Trace_Number == 120) Final_Result = 231;
                else if (Primary_Trace_Number == 121) Final_Result = 251;
                else if (Primary_Trace_Number == 122) Final_Result = 251;
                else if (Primary_Trace_Number == 123) Final_Result = 251;
                else if (Primary_Trace_Number == 124) Final_Result = 251;
                else if (Primary_Trace_Number == 125) Final_Result = 251;
                else if (Primary_Trace_Number == 126) Final_Result = 251;
                else if (Primary_Trace_Number == 127) Final_Result = 251;
                else if (Primary_Trace_Number == 128) Final_Result = 251;
                else if (Primary_Trace_Number == 129) Final_Result = 251;
                else if (Primary_Trace_Number == 130) Final_Result = 251;
                else if (Primary_Trace_Number == 131) Final_Result = 271;
                else if (Primary_Trace_Number == 132) Final_Result = 271;
                else if (Primary_Trace_Number == 133) Final_Result = 271;
                else if (Primary_Trace_Number == 134) Final_Result = 271;
                else if (Primary_Trace_Number == 135) Final_Result = 271;
                else if (Primary_Trace_Number == 136) Final_Result = 271;
                else if (Primary_Trace_Number == 137) Final_Result = 271;
                else if (Primary_Trace_Number == 138) Final_Result = 271;
                else if (Primary_Trace_Number == 139) Final_Result = 271;
                else if (Primary_Trace_Number == 140) Final_Result = 271;
                else if (Primary_Trace_Number == 141) Final_Result = 291;
                else if (Primary_Trace_Number == 142) Final_Result = 291;
                else if (Primary_Trace_Number == 143) Final_Result = 291;
                else if (Primary_Trace_Number == 144) Final_Result = 291;
                else if (Primary_Trace_Number == 145) Final_Result = 291;
                else if (Primary_Trace_Number == 146) Final_Result = 291;
                else if (Primary_Trace_Number == 147) Final_Result = 291;
                else if (Primary_Trace_Number == 148) Final_Result = 291;
                else if (Primary_Trace_Number == 149) Final_Result = 291;
                else if (Primary_Trace_Number == 150) Final_Result = 291;
                else if (Primary_Trace_Number == 151) Final_Result = 307;
                else if (Primary_Trace_Number == 152) Final_Result = 307;
                else if (Primary_Trace_Number == 153) Final_Result = 307;
                else if (Primary_Trace_Number == 154) Final_Result = 307;
                else if (Primary_Trace_Number == 155) Final_Result = 307;
                else if (Primary_Trace_Number == 156) Final_Result = 307;
                else if (Primary_Trace_Number == 157) Final_Result = 319;
                else if (Primary_Trace_Number == 158) Final_Result = 319;
                else if (Primary_Trace_Number == 159) Final_Result = 319;
                else if (Primary_Trace_Number == 160) Final_Result = 319;
                else if (Primary_Trace_Number == 161) Final_Result = 319;
                else if (Primary_Trace_Number == 162) Final_Result = 319;
                else if (Primary_Trace_Number == 163) Final_Result = 331;
                else if (Primary_Trace_Number == 164) Final_Result = 331;
                else if (Primary_Trace_Number == 165) Final_Result = 331;
                else if (Primary_Trace_Number == 166) Final_Result = 331;
                else if (Primary_Trace_Number == 167) Final_Result = 331;
                else if (Primary_Trace_Number == 168) Final_Result = 331;
                else if (Primary_Trace_Number == 169) Final_Result = 343;
                else if (Primary_Trace_Number == 170) Final_Result = 343;
                else if (Primary_Trace_Number == 171) Final_Result = 343;
                else if (Primary_Trace_Number == 172) Final_Result = 343;
                else if (Primary_Trace_Number == 173) Final_Result = 343;
                else if (Primary_Trace_Number == 174) Final_Result = 343;
                else if (Primary_Trace_Number == 175) Final_Result = 355;
                else if (Primary_Trace_Number == 176) Final_Result = 355;
                else if (Primary_Trace_Number == 177) Final_Result = 355;
                else if (Primary_Trace_Number == 178) Final_Result = 355;
                else if (Primary_Trace_Number == 179) Final_Result = 355;
                else if (Primary_Trace_Number == 180) Final_Result = 355;
                else if (Primary_Trace_Number == 181) Final_Result = 364;
                else if (Primary_Trace_Number == 182) Final_Result = 364;
                else if (Primary_Trace_Number == 183) Final_Result = 364;
                else if (Primary_Trace_Number == 184) Final_Result = 370;
                else if (Primary_Trace_Number == 185) Final_Result = 370;
                else if (Primary_Trace_Number == 186) Final_Result = 370;
                else Final_Result = 0;

                #endregion Site_2_Trace_Number
            }
            else if (Site_Number == 3)
            {
                #region Site_3_Trace_Number

                if (Primary_Trace_Number == 1) Final_Result = 383;
                else if (Primary_Trace_Number == 2) Final_Result = 383;
                else if (Primary_Trace_Number == 3) Final_Result = 383;
                else if (Primary_Trace_Number == 4) Final_Result = 383;
                else if (Primary_Trace_Number == 5) Final_Result = 383;
                else if (Primary_Trace_Number == 6) Final_Result = 383;
                else if (Primary_Trace_Number == 7) Final_Result = 383;
                else if (Primary_Trace_Number == 8) Final_Result = 383;
                else if (Primary_Trace_Number == 9) Final_Result = 383;
                else if (Primary_Trace_Number == 10) Final_Result = 383;
                else if (Primary_Trace_Number == 11) Final_Result = 403;
                else if (Primary_Trace_Number == 12) Final_Result = 403;
                else if (Primary_Trace_Number == 13) Final_Result = 403;
                else if (Primary_Trace_Number == 14) Final_Result = 403;
                else if (Primary_Trace_Number == 15) Final_Result = 403;
                else if (Primary_Trace_Number == 16) Final_Result = 403;
                else if (Primary_Trace_Number == 17) Final_Result = 403;
                else if (Primary_Trace_Number == 18) Final_Result = 403;
                else if (Primary_Trace_Number == 19) Final_Result = 403;
                else if (Primary_Trace_Number == 20) Final_Result = 403;
                else if (Primary_Trace_Number == 21) Final_Result = 423;
                else if (Primary_Trace_Number == 22) Final_Result = 423;
                else if (Primary_Trace_Number == 23) Final_Result = 423;
                else if (Primary_Trace_Number == 24) Final_Result = 423;
                else if (Primary_Trace_Number == 25) Final_Result = 423;
                else if (Primary_Trace_Number == 26) Final_Result = 423;
                else if (Primary_Trace_Number == 27) Final_Result = 423;
                else if (Primary_Trace_Number == 28) Final_Result = 423;
                else if (Primary_Trace_Number == 29) Final_Result = 423;
                else if (Primary_Trace_Number == 30) Final_Result = 423;
                else if (Primary_Trace_Number == 31) Final_Result = 443;
                else if (Primary_Trace_Number == 32) Final_Result = 443;
                else if (Primary_Trace_Number == 33) Final_Result = 443;
                else if (Primary_Trace_Number == 34) Final_Result = 443;
                else if (Primary_Trace_Number == 35) Final_Result = 443;
                else if (Primary_Trace_Number == 36) Final_Result = 443;
                else if (Primary_Trace_Number == 37) Final_Result = 443;
                else if (Primary_Trace_Number == 38) Final_Result = 443;
                else if (Primary_Trace_Number == 39) Final_Result = 443;
                else if (Primary_Trace_Number == 40) Final_Result = 443;
                else if (Primary_Trace_Number == 41) Final_Result = 463;
                else if (Primary_Trace_Number == 42) Final_Result = 463;
                else if (Primary_Trace_Number == 43) Final_Result = 463;
                else if (Primary_Trace_Number == 44) Final_Result = 463;
                else if (Primary_Trace_Number == 45) Final_Result = 463;
                else if (Primary_Trace_Number == 46) Final_Result = 463;
                else if (Primary_Trace_Number == 47) Final_Result = 463;
                else if (Primary_Trace_Number == 48) Final_Result = 463;
                else if (Primary_Trace_Number == 49) Final_Result = 463;
                else if (Primary_Trace_Number == 50) Final_Result = 463;
                else if (Primary_Trace_Number == 51) Final_Result = 483;
                else if (Primary_Trace_Number == 52) Final_Result = 483;
                else if (Primary_Trace_Number == 53) Final_Result = 483;
                else if (Primary_Trace_Number == 54) Final_Result = 483;
                else if (Primary_Trace_Number == 55) Final_Result = 483;
                else if (Primary_Trace_Number == 56) Final_Result = 483;
                else if (Primary_Trace_Number == 57) Final_Result = 483;
                else if (Primary_Trace_Number == 58) Final_Result = 483;
                else if (Primary_Trace_Number == 59) Final_Result = 483;
                else if (Primary_Trace_Number == 60) Final_Result = 483;
                else if (Primary_Trace_Number == 61) Final_Result = 499;
                else if (Primary_Trace_Number == 62) Final_Result = 499;
                else if (Primary_Trace_Number == 63) Final_Result = 499;
                else if (Primary_Trace_Number == 64) Final_Result = 499;
                else if (Primary_Trace_Number == 65) Final_Result = 499;
                else if (Primary_Trace_Number == 66) Final_Result = 499;
                else if (Primary_Trace_Number == 67) Final_Result = 511;
                else if (Primary_Trace_Number == 68) Final_Result = 511;
                else if (Primary_Trace_Number == 69) Final_Result = 511;
                else if (Primary_Trace_Number == 70) Final_Result = 511;
                else if (Primary_Trace_Number == 71) Final_Result = 511;
                else if (Primary_Trace_Number == 72) Final_Result = 511;
                else if (Primary_Trace_Number == 73) Final_Result = 523;
                else if (Primary_Trace_Number == 74) Final_Result = 523;
                else if (Primary_Trace_Number == 75) Final_Result = 523;
                else if (Primary_Trace_Number == 76) Final_Result = 523;
                else if (Primary_Trace_Number == 77) Final_Result = 523;
                else if (Primary_Trace_Number == 78) Final_Result = 523;
                else if (Primary_Trace_Number == 79) Final_Result = 535;
                else if (Primary_Trace_Number == 80) Final_Result = 535;
                else if (Primary_Trace_Number == 81) Final_Result = 535;
                else if (Primary_Trace_Number == 82) Final_Result = 535;
                else if (Primary_Trace_Number == 83) Final_Result = 535;
                else if (Primary_Trace_Number == 84) Final_Result = 535;
                else if (Primary_Trace_Number == 85) Final_Result = 547;
                else if (Primary_Trace_Number == 86) Final_Result = 547;
                else if (Primary_Trace_Number == 87) Final_Result = 547;
                else if (Primary_Trace_Number == 88) Final_Result = 547;
                else if (Primary_Trace_Number == 89) Final_Result = 547;
                else if (Primary_Trace_Number == 90) Final_Result = 547;
                else if (Primary_Trace_Number == 91) Final_Result = 563;
                else if (Primary_Trace_Number == 92) Final_Result = 563;
                else if (Primary_Trace_Number == 93) Final_Result = 563;
                else if (Primary_Trace_Number == 94) Final_Result = 563;
                else if (Primary_Trace_Number == 95) Final_Result = 563;
                else if (Primary_Trace_Number == 96) Final_Result = 563;
                else if (Primary_Trace_Number == 97) Final_Result = 563;
                else if (Primary_Trace_Number == 98) Final_Result = 563;
                else if (Primary_Trace_Number == 99) Final_Result = 563;
                else if (Primary_Trace_Number == 100) Final_Result = 563;
                else if (Primary_Trace_Number == 101) Final_Result = 583;
                else if (Primary_Trace_Number == 102) Final_Result = 583;
                else if (Primary_Trace_Number == 103) Final_Result = 583;
                else if (Primary_Trace_Number == 104) Final_Result = 583;
                else if (Primary_Trace_Number == 105) Final_Result = 583;
                else if (Primary_Trace_Number == 106) Final_Result = 583;
                else if (Primary_Trace_Number == 107) Final_Result = 583;
                else if (Primary_Trace_Number == 108) Final_Result = 583;
                else if (Primary_Trace_Number == 109) Final_Result = 583;
                else if (Primary_Trace_Number == 110) Final_Result = 583;
                else if (Primary_Trace_Number == 111) Final_Result = 603;
                else if (Primary_Trace_Number == 112) Final_Result = 603;
                else if (Primary_Trace_Number == 113) Final_Result = 603;
                else if (Primary_Trace_Number == 114) Final_Result = 603;
                else if (Primary_Trace_Number == 115) Final_Result = 603;
                else if (Primary_Trace_Number == 116) Final_Result = 603;
                else if (Primary_Trace_Number == 117) Final_Result = 603;
                else if (Primary_Trace_Number == 118) Final_Result = 603;
                else if (Primary_Trace_Number == 119) Final_Result = 603;
                else if (Primary_Trace_Number == 120) Final_Result = 603;
                else if (Primary_Trace_Number == 121) Final_Result = 623;
                else if (Primary_Trace_Number == 122) Final_Result = 623;
                else if (Primary_Trace_Number == 123) Final_Result = 623;
                else if (Primary_Trace_Number == 124) Final_Result = 623;
                else if (Primary_Trace_Number == 125) Final_Result = 623;
                else if (Primary_Trace_Number == 126) Final_Result = 623;
                else if (Primary_Trace_Number == 127) Final_Result = 623;
                else if (Primary_Trace_Number == 128) Final_Result = 623;
                else if (Primary_Trace_Number == 129) Final_Result = 623;
                else if (Primary_Trace_Number == 130) Final_Result = 623;
                else if (Primary_Trace_Number == 131) Final_Result = 643;
                else if (Primary_Trace_Number == 132) Final_Result = 643;
                else if (Primary_Trace_Number == 133) Final_Result = 643;
                else if (Primary_Trace_Number == 134) Final_Result = 643;
                else if (Primary_Trace_Number == 135) Final_Result = 643;
                else if (Primary_Trace_Number == 136) Final_Result = 643;
                else if (Primary_Trace_Number == 137) Final_Result = 643;
                else if (Primary_Trace_Number == 138) Final_Result = 643;
                else if (Primary_Trace_Number == 139) Final_Result = 643;
                else if (Primary_Trace_Number == 140) Final_Result = 643;
                else if (Primary_Trace_Number == 141) Final_Result = 663;
                else if (Primary_Trace_Number == 142) Final_Result = 663;
                else if (Primary_Trace_Number == 143) Final_Result = 663;
                else if (Primary_Trace_Number == 144) Final_Result = 663;
                else if (Primary_Trace_Number == 145) Final_Result = 663;
                else if (Primary_Trace_Number == 146) Final_Result = 663;
                else if (Primary_Trace_Number == 147) Final_Result = 663;
                else if (Primary_Trace_Number == 148) Final_Result = 663;
                else if (Primary_Trace_Number == 149) Final_Result = 663;
                else if (Primary_Trace_Number == 150) Final_Result = 663;
                else if (Primary_Trace_Number == 151) Final_Result = 679;
                else if (Primary_Trace_Number == 152) Final_Result = 679;
                else if (Primary_Trace_Number == 153) Final_Result = 679;
                else if (Primary_Trace_Number == 154) Final_Result = 679;
                else if (Primary_Trace_Number == 155) Final_Result = 679;
                else if (Primary_Trace_Number == 156) Final_Result = 679;
                else if (Primary_Trace_Number == 157) Final_Result = 691;
                else if (Primary_Trace_Number == 158) Final_Result = 691;
                else if (Primary_Trace_Number == 159) Final_Result = 691;
                else if (Primary_Trace_Number == 160) Final_Result = 691;
                else if (Primary_Trace_Number == 161) Final_Result = 691;
                else if (Primary_Trace_Number == 162) Final_Result = 691;
                else if (Primary_Trace_Number == 163) Final_Result = 703;
                else if (Primary_Trace_Number == 164) Final_Result = 703;
                else if (Primary_Trace_Number == 165) Final_Result = 703;
                else if (Primary_Trace_Number == 166) Final_Result = 703;
                else if (Primary_Trace_Number == 167) Final_Result = 703;
                else if (Primary_Trace_Number == 168) Final_Result = 703;
                else if (Primary_Trace_Number == 169) Final_Result = 715;
                else if (Primary_Trace_Number == 170) Final_Result = 715;
                else if (Primary_Trace_Number == 171) Final_Result = 715;
                else if (Primary_Trace_Number == 172) Final_Result = 715;
                else if (Primary_Trace_Number == 173) Final_Result = 715;
                else if (Primary_Trace_Number == 174) Final_Result = 715;
                else if (Primary_Trace_Number == 175) Final_Result = 727;
                else if (Primary_Trace_Number == 176) Final_Result = 727;
                else if (Primary_Trace_Number == 177) Final_Result = 727;
                else if (Primary_Trace_Number == 178) Final_Result = 727;
                else if (Primary_Trace_Number == 179) Final_Result = 727;
                else if (Primary_Trace_Number == 180) Final_Result = 727;
                else if (Primary_Trace_Number == 181) Final_Result = 736;
                else if (Primary_Trace_Number == 182) Final_Result = 736;
                else if (Primary_Trace_Number == 183) Final_Result = 736;
                else if (Primary_Trace_Number == 184) Final_Result = 742;
                else if (Primary_Trace_Number == 185) Final_Result = 742;
                else if (Primary_Trace_Number == 186) Final_Result = 742;
                else Final_Result = 0;

                #endregion Site_3_Trace_Number
            }
            else
            {
                Final_Result = 0;
            }
            return Final_Result;
        }

        #endregion AutoCal Codes

        private static void SetSegmentPower(int Power=0)
        {
            int[] ChanNum = Eq_ENA.Active_ENA.NA_IVI.Channels.GetActiveChannels();//query channel number
            int TotalChanNum = ChanNum.Count();// get total channel number
            for (int Chan = 1; Chan <= TotalChanNum; Chan++)
            {
                Eq_ENA.Active_ENA.Write_Topaz("SENSE" + Chan.ToString() + ":segment:count?");//query segment number for each channel
                int SegNumPerChan = Convert.ToInt16(Eq_ENA.Active_ENA.NA_IVI.System.ScpiPassThrough.ReadString());
                for (int SegNum = 1; SegNum <= SegNumPerChan; SegNum++)
                {
                    Eq_ENA.Active_ENA.Write_Topaz("sense" + Chan.ToString() + ":segment" + SegNum.ToString() + ":power1:level " + Power.ToString());//change power level for each segment
                }
            }
        }
    }
}
