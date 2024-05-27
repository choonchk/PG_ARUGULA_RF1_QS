using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Ivi.Visa.Interop;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using System.IO;
using System.Threading;
using System.IO.MemoryMappedFiles;
using ClothoLibAlgo;
using Agilent.AgNA.Interop;

namespace EqLib
{
    public partial class Eq_ENA
    {
       // public static FormattedIO488 NetAn;
        public static Dictionary<byte, byte> Sync_Group = new Dictionary<byte, byte>();
        public static ManualResetEvent enaStateRecallFlag = new ManualResetEvent(false);
        public static bool InitFlag;
        //public static Agilent.AgNA.Interop.IAgNA NA_IVI_2 = new Agilent.AgNA.Interop.AgNA();//site1 // need to modify if more than 2 site
        //public static Dictionary<byte, Agilent.AgNA.Interop.IAgNA> NA_IVI = new Dictionary<byte, Agilent.AgNA.Interop.IAgNA>();
        public static MemoryMappedFile mappedFile;
        public static MemoryMappedViewAccessor mappedFileView;
        public static AgNAChannel ch1;
        public static int[] memOffsets;
        //public static Dictionary<byte,int[]> memOffsets= new Dictionary<byte,int[]>();
        public static int numPoints;
        public static List<int> NumofPoints=new List<int>();
        public static int para_count;
        public static int total_chan;
        public static bool Read_Freq=false;
        public static float[][] measData;
        public static int total_trace=0;
        public static int[] chanNums;
        public static string statefile;
        public static bool parallel;
        public static Dictionary<int, int> Chan_Trace_set = new Dictionary<int, int>();
        public static Dictionary<int, int> Chan_TraceNum = new Dictionary<int, int>();
        //public static Dictionary<byte, float[][]> measData = new Dictionary<byte, float[][]>();
        public static Dictionary<int, List<double>> FreqList = new Dictionary<int, List<double>>();
        public static ENA_base Get(byte site)
        {
            ENA_base EqENA;
            if (Sync_Group.Keys.Contains(site))
            {
                EqENA = new Eq_ENA.Active_ENA();
            }
            else if (Sync_Group.Values.Contains(site))
            {
                EqENA = new Eq_ENA.None();
            }
            else
            {
                throw new Exception("ENA site is not recognized, please check site number!");
            }
            return EqENA;
        }
        public abstract class ENA_base
        {
            public byte Site { get; set; }
            public abstract void initArray();
            public abstract void set_Memory_map();
            public abstract int[] trace_offset(int Chan);
            public abstract void InitializeENA(object InstrAddress);
            public abstract int openIviSession(string vnaIviAddr);
            public abstract string CheckTraceFormat(int channel, int tracenum);
            public abstract void Write(string cmd);
            public abstract void ReadData(int ChanNum,bool Synchronization);
            public abstract double[] ReadENATrace(int channel, int trace_OneBased);
            public abstract double[] ReadENATrace2(int channel, int trace_OneBased, byte site);
            public abstract void Display();
            public abstract List<double> ReadFreqList_Chan(int channel);
            public abstract void ReadFreqList(int total_chan);
            public abstract string ReadString();
            public abstract void ManualCal();
            public abstract void SendTrigger(bool Sync, int Chan, int Trace);
        }
        /*
 * 
 * Initialize meas data arrays
 * 
 */
        public class Active_ENA : ENA_base
        {
            public static Agilent.AgNA.Interop.IAgNA NA_IVI = new Agilent.AgNA.Interop.AgNA();//site0
            //public static Dictionary<byte, Agilent.AgNA.Interop.IAgNA> NA_IVI = new Dictionary<byte, Agilent.AgNA.Interop.IAgNA>();
            public override void initArray()
            {
                // Allocate buffers to hold the output data
                measData = new float[total_trace][];

                for (int i = 0; i < measData.Length; i++)
                {
                    measData[i] = new float[NumofPoints[i]];
                }
                //NA_IVI.Trigger.Source = Agilent.AgNA.Interop.AgNATriggerSourceEnum.AgNATriggerSourceInternal;
                //NA_IVI.Display.Enabled = true;
                ReadFreqList(total_chan);
                InitFlag = true;
                HideTraces(NA_IVI);
                //freqs = new double[numChans, numPoints];
            }
            // Function that hides (by unfeeding) all traces from the display.
            // The channels and measurement numbers are still available for measurements and remote
            // usage
            public void HideTraces(Agilent.AgNA.Interop.IAgNA vna)
            {
                int[] Meas;
                foreach (int Chan in chanNums)
                {
                    Meas = NA_IVI.Channels.get_Item(Chan.ToString()).Measurements.GetActiveMeasurements();
                    int MeasNums = Meas.Length;
                    for (int i = 1; i < MeasNums; i++)
                    {
                        NA_IVI.Channels.get_Item(Chan.ToString()).Measurements.get_Item(Meas[i].ToString());
                        Write("DISP:MEAS" + Meas[i].ToString() + ":DEL");
                    }
                }
                // At the end, remove all the windows (saves a little bit of memory, not as much as removing the traces)
               // vna.Windows.DeleteAll();
            }
            // Use memory mapped data between code and VNA. Allows for fastest transfer
            public override void set_Memory_map()
            {
                // initialize memory mapped structures in VNA
                //Eq_ENA.NA_IVI[site].Display.Enabled = true;
                string Chan_List = "";
                Write("SYST:PRES");
                Write("*OPC?");
                ReadString();
                total_trace = 0;
                Write("MMEM:LOAD \"" + statefile.Trim() + "\"");//Load State file for Memory map use
                chanNums = NA_IVI.Channels.GetActiveChannels();
                for (int i = 1; i < chanNums.Count(); i++)
                {
                    Chan_List = Chan_List + i.ToString() + ",";
                }
                Chan_List = Chan_List + chanNums.Count().ToString();
                int[] Meas;
                NumofPoints.Clear();
                Chan_Trace_set.Clear();
                #region change trace format from Smith to Log
                foreach (int Chan in chanNums)
                {
                    
                    Write("SENS" + Chan.ToString() + ":SWE:SPE FAST");
                    Meas = NA_IVI.Channels.get_Item(Chan.ToString()).Measurements.GetActiveMeasurements();
                    int MeasNums = Meas.Length;
                    if (Chan <= 12 || (Chan >= 23 && Chan <= 34)) MeasNums = Meas.Length - 1;
                    if ((Chan >= 49 && Chan <= 60) || (Chan >= 71 && Chan <= 82)) MeasNums = Meas.Length - 1;
                    for (int i = 0; i < MeasNums; i++)
                    {
                        NA_IVI.Channels.get_Item(Chan.ToString()).Measurements.get_Item(Meas[i].ToString());
                        Write("CALC" + Chan.ToString() + ":MEAS" + Meas[i].ToString() + ":FORM MLOG");
                    }
                    foreach (int meas in Meas)
                    {
                        Chan_Trace_set.Add(meas, Chan);
                    }
                }
                #endregion
                int CoupleNum = chanNums.Count() / 2;
                if (parallel)
                {
                    NA_IVI.System.IO.WriteString("SYST:CHAN:COUP:GROUP " + CoupleNum.ToString() + "," + Chan_List);
                    NA_IVI.System.IO.WriteString("SYST:CHAN:COUP:STAT 1");  // Turn coupling on
                    NA_IVI.System.IO.WriteString("SYST:CHAN:COUP:PAR:ENAB 1"); // turn on parallel  
                    NA_IVI.System.IO.WriteString("TRIG:SOUR IMM");
                    NA_IVI.System.IO.WriteString("TRIG:SCOP ALL");
                    foreach (int Chan in chanNums)
                    {
                        NA_IVI.System.IO.WriteString("INIT" + Chan.ToString() + ":CONT ON");
                    }
                }
                else
                {
                    foreach (int Chan in chanNums)
                    {
                        NA_IVI.System.IO.WriteString("Sens"+ Chan.ToString()+":Sweep:Mode HOLD" );
                    }
                }

                NA_IVI.System.WaitForOperationComplete(5000);
                Write("SYST:DATA:MEM:INIT\n");
                //NA_IVI.Channels.get_Item("1").Measurements.get_Item("1");
                //Write("CALC1:PAR:CAT:EXT?");//read trace type for channel 1//if diff channel has diff trace type need to change here

                //string temp = NA_IVI.System.IO.ReadString();
                //string[] Par_list = temp.Split(',');
                //para_count = Par_list.Length;//double size parameter count

                total_chan = chanNums.Count();
                Dictionary<int, int[]> Offset_for_chan_dic = new Dictionary<int, int[]>();
                int Total_channel_length = 0;
                // setup segment sweeps and grab offsets for each measurement in memory map

                foreach (int chan in chanNums)
                {

                    int[] offsets_for_chan_data = trace_offset(chan);
                    Offset_for_chan_dic.Add(Total_channel_length, offsets_for_chan_data);
                    Total_channel_length = Total_channel_length + offsets_for_chan_data.Length;
                }
                //int[] offsets_for_chan1_data = setupSweep(1);

                //int[] offsets_for_chan2_data = setupSweep(2);
                // combine memory offsets for measurements both channels into single array
                memOffsets = new int[total_trace];
                foreach (int channel_length in Offset_for_chan_dic.Keys)
                {
                    Offset_for_chan_dic[channel_length].CopyTo(memOffsets, channel_length);
                }

                //Tell the VNA to allocate the memory map. Name it "VNA_MemoryMap"
                Write("SYST:DATA:MEM:COMM 'VNA_MemoryMap'");

                // Query the size of the memory map
                Write("SYST:DATA:MEM:SIZE?");
                int size = int.Parse(NA_IVI.System.IO.ReadString());

                // Create the memory map in C#. This requires .NET 4.5 framework
                mappedFile = MemoryMappedFile.CreateOrOpen("VNA_MemoryMap", size);
                mappedFileView = mappedFile.CreateViewAccessor();
                initArray();
            }
            public override int[] trace_offset(int Chan)
            {             
                // setup S11, S21, S22, S33, S31
                int[] measnums;
                numPoints = NA_IVI.Channels.get_Item(Chan.ToString()).StandardStimulus.Points;
                measnums = NA_IVI.Channels.get_Item(Chan.ToString()).Measurements.GetActiveMeasurements();
                if (!Chan_TraceNum.Keys.Contains(Chan)) Chan_TraceNum.Add(Chan, measnums.Length);
                total_trace += measnums.Length;
                int[] offsets_for_complex_data = new int[measnums.Length];
                for (int i = 0; i < measnums.Length; i++)
                {
                    NumofPoints.Add(numPoints);
                    // Configure a new section of the memory map to monitor the complex data of this parameter
                    Write("SYST:DATA:MEM:ADD '" + Chan.ToString() + ":" + measnums[i].ToString() + ":FDATA:" + numPoints.ToString() + "'"); // add parameter to memory mapped
                    Write("SYST:DATA:MEM:OFFSet?");
                    //Thread.Sleep(5);
                    offsets_for_complex_data[i] = int.Parse(NA_IVI.System.IO.ReadString());
                    //offsets_for_complex_data[i] = int.Parse(NA_IVI.System.IO.ReadString());
                    //Write("*OPC?");
                    //string readback = NA_IVI.System.IO.ReadString();
                }
                return offsets_for_complex_data;
            }
            public override void InitializeENA(object InstrAddress)
            {
                try
                {
                    KillTopaz();// Kill remaining Topaz process before init
                    int pass = openIviSession(InstrAddress.ToString());
                    foreach (byte key in Sync_Group.Keys)
                    {
                        if (Sync_Group[key] != key) parallel = true;// check if we need to configure Topaz as parallel
                    }
                    set_Memory_map();//load state file in set Memory map
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }
            }
            public void KillTopaz()
            {
                Process[] processes = Process.GetProcessesByName("835x");
                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            public override int openIviSession(string vnaIviAddr)
            {
                Cursor.Current = Cursors.WaitCursor;
                // try opening the session
                try
                {

                    NA_IVI.Initialize(vnaIviAddr, false, false, "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MessageBox.Show("Connection Timeout");
                    return -1;
                }
                Cursor.Current = Cursors.Default;
                return 1;
            }
            public override string CheckTraceFormat(int channel, int tracenum)
            {
                Write("CALC" + channel.ToString() + ":MEAS" + tracenum.ToString() + ":FORM? ");
                string formatt_trace = NA_IVI.System.IO.ReadString();
                return formatt_trace;
            }
            public override void Write(string cmd)
            {
                try
                {
                    NA_IVI.System.IO.WriteString(cmd);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    MessageBox.Show("Topaz process is abnormally terminated. Please reload the program!", "",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Hand,
                    MessageBoxDefaultButton.Button2); 
                }
            }
            public override void ReadData(int ChanNum,bool synchronization)
            {        
                int Startpoint = 0;
                bool StartFound = false;
                int Endpoint=0;
                bool EndFound=false;
                foreach (int trace in Chan_Trace_set.Keys)
                {
                    if (Chan_Trace_set[trace] == ChanNum && !StartFound)
                    {
                        Startpoint = trace;
                        StartFound = true;
                    }
                    if (Chan_Trace_set[trace] == ChanNum + 1 && !EndFound)
                    {
                        Endpoint = trace - 1;
                        EndFound = true;
                    }
                    if (StartFound && EndFound) break;
                }
                //int Startpoint = (ChanNum - 1) * para_count / 2;
                //int Endpoint = ChanNum * para_count / 2;
                //if (ChanNum == 5) Endpoint = ChanNum * para_count / 2-1;
                for (int i = Startpoint-1; i < Endpoint; i++)
                {
                    ReadBytes(mappedFileView, memOffsets[i], NumofPoints[i], measData[i]);
                }
                if (synchronization)
                {
                    int Startpoint_site1 = Endpoint + 1;
                    int Endpoint_site1 = Startpoint_site1 + Endpoint - Startpoint;
                    for (int i = Startpoint_site1 - 1; i < Endpoint_site1; i++)
                    {
                        ReadBytes(mappedFileView, memOffsets[i], NumofPoints[i], measData[i]);
                    }
                }               
            }

            // [Burhan] Add for communication with Topaz
            static public unsafe void Write_Topaz(string cmd)
            {
               NA_IVI.System.IO.WriteString(cmd);
            }

            // [Burhan] Add for communication with Topaz
            static public unsafe string Read_Topaz(string cmd)
            {
                NA_IVI.System.IO.WriteString(cmd);
                return NA_IVI.System.IO.ReadString();
            }

            static public unsafe double[] ReadIEEEBlock(string cmd)
            {
                double[] data = { 0 };
                // M9485.System.IO.WriteString("SENS1:CORR:CSET:STIM 1", true);
                try
                {
                    NA_IVI.System.IO.WriteString(cmd);
                    return ((double[])NA_IVI.System.ScpiPassThrough.ReadIEEEBlockR8());
                    //NA_IVI.System.IO.WriteString(cmd, true);
                    //return ((double[])NA_IVI.System.IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
                    //return ((double[])IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
                }
                catch
                {
                    return ((double[])NA_IVI.System.ScpiPassThrough.ReadIEEEBlockR8());
                    //return ((double[])NA_IVI.System.IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
                    //return ((double[])IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
                }
                return data;
            }
            static public unsafe void ReadBytes(MemoryMappedViewAccessor mappedFileView,
            int offset, int num, float[] arr)
            {
                //This is equivalent to: 
                //m_mappedFileView.ReadArray<float>(m_sharedMemoryOffsets[i-1], complexArray, 0, points*2);
                //But, using this "unsafe" code is 30 times faster. 100usec versus 3ms
                try
                {
                    byte* ptr = (byte*)0;
                    mappedFileView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                    System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(new IntPtr(ptr), offset), arr, 0, num);
                    mappedFileView.SafeMemoryMappedViewHandle.ReleasePointer();
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString()); 
                }
            }
            public override double[] ReadENATrace(int channel, int trace_OneBased)
            {
                try
                {
                    float[] fullDATA_X = measData[trace_OneBased - 1];

                    double[] data = new double[fullDATA_X.Length / 2];

                    for (int k = 0; k < data.Length; k++)
                    {
                        data[k] = fullDATA_X[2 * k];
                    }
                    return data;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    return new double[4];
                }
            }
            public override double[] ReadENATrace2(int channel, int trace_OneBased, byte site)
            {
                try
                {
                    double[] fullDATA_X = new double[measData[trace_OneBased - 1].Count()];
                    int i=0;
                    foreach (float value in measData[trace_OneBased - 1])
                    {
                        fullDATA_X[i] = Convert.ToDouble(value);
                        i++;
                    }
                    return fullDATA_X;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    return new double[4];
                }
            }
            public override void Display()
            {
                //Please call this function to display /close the window.
                if (NA_IVI.Display.Enabled)
                {
                    NA_IVI.Display.Enabled = false;
                }
                else
                {
                    NA_IVI.Display.Enabled = true;
                }
            }
            public override List<double> ReadFreqList_Chan(int channel)
            {
                try
                {
                    return FreqList[channel];
                }
                catch (Exception e)
                {
                    List<double> freqs = new List<double>();
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    return freqs;
                }
            }
            public override void ReadFreqList(int total_chan)
            {
                try
                {
                        FreqList.Clear();
                        //for (int channel = 1; channel <= 22; channel = channel + 2)
                        for (int channel = 1; channel <= total_chan; channel++)
                        {
                            int traceNum =0;
                            foreach (int trace in Chan_Trace_set.Keys)
                            {
                                if (Chan_Trace_set[trace] == channel)
                                {
                                    traceNum = trace;//Any trace num on the channel 
                                    break;
                                } 
                            }
                            AgNAMeasurement meas = NA_IVI.Channels.get_Item(channel.ToString()).Measurements.get_Item(traceNum.ToString());
                            //double[] retval = NA_IVI[site].Channels.get_Item(channel.ToString()).Measurements.get_Item(traceNum.ToString()).FetchX();
                            double[] retval = meas.FetchX();                      
                            List<double> freqs = retval.ToList();
                            for (int i = 0; i < freqs.Count(); i++) freqs[i] *= 1e-6;
                            FreqList.Add(channel, freqs);
                        }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }
            }
            public override string ReadString()
            {
                string temp=null;
                try
                {
                  temp = NA_IVI.System.IO.ReadString();
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString ());
                    MessageBox.Show("Topaz process is abnormally terminated. Please reload the program!", "",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Hand,
                                    MessageBoxDefaultButton.Button2); 
                }
                return temp;
            }
            public override void SendTrigger(bool Sync, int Chan, int Trace)
            {
                if (Sync)
                {
                    Write("SENS" + Chan + ":SWE:MODE SING");
                    Write("*OPC?");
                }
                else
                {
                    Write("TRIG:SCOP CURRent");
                    Write("Init" + Chan + ":Imm");
                    Write("*OPC?");
                }
            }
            public override void ManualCal() 
            {
                //NA_IVI.GuidedCalibration.Reset();
                //int[] channels = { 1, 2};
                int[] ports1 = { 1, 2, 3,4,5,6 };
                int[] ports2 = { 7, 8, 9, 10, 11, 12 };
                // Setup convenient pointers to GuidedCalibration
                IAgNAGuidedCalibration pGuidedCal = NA_IVI.GuidedCalibration;
                string calKit = "";
                calKit = "85033D/E";
                // set channels and ports to be calibrated
                pGuidedCal.SetChannels(ref chanNums);
                for (int i = 0; i < chanNums.Length; i++)
                {
                    if (i % 2 == 0) pGuidedCal.SetPorts(chanNums[i], ref ports1);
                    else pGuidedCal.SetPorts(chanNums[i], ref ports2);
                }
                for (int i = 0; i < ports1.Length; i++)
                {
                    pGuidedCal.SetPortConnector(ports1[i], "APC 3.5 female");
                    pGuidedCal.SetCalKit(ports1[i], calKit);
                }
                for (int i = 0; i < ports2.Length; i++)
                {
                    pGuidedCal.SetPortConnector(ports2[i], "APC 3.5 female");
                    pGuidedCal.SetCalKit(ports2[i], calKit);
                }
                pGuidedCal.Advanced.IFBandWidth = 1000;

                // check for special calibrations
                // for standard measurements this will be power cal
                string ans = pGuidedCal.Advanced.GetParameterCatalog();
                string propVals = pGuidedCal.Advanced.GetParameterValueCatalog(ans);
                MessageBox.Show("prop vals: " + propVals);
                DialogResult dr = MessageBox.Show(ans + "?",
                          "Guided Calibration", MessageBoxButtons.YesNo);

                if (dr == DialogResult.Yes)
                {
                    NA_IVI.System.IO.WriteString("SYST:CAL:ALL:MCL:PROP:VAL '" + ans + "','true'");
                }
                else if (dr == DialogResult.No)
                {
                    NA_IVI.System.IO.WriteString("SYST:CAL:ALL:MCL:PROP:VAL '" + ans + "','false'");

                }

                pGuidedCal.Reset();
                pGuidedCal.GenerateSteps();
                int numSteps = pGuidedCal.StepCount;

                //Measure the standards
                AgNACalibrationStep pCalStep = null;

                for (int i = 1; i < numSteps + 1; i++)
                {
                    pCalStep = pGuidedCal.GetStep(i);
                    string step = "Step " + i.ToString() + " of " + numSteps;
                    string strPrompt = pCalStep.Description;

                    dr = MessageBox.Show(step + "\n" + strPrompt,
                          "Guided Calibration", MessageBoxButtons.OKCancel);

                    if (dr == DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        pCalStep.Acquire(i);
                        while (pCalStep.IsCompleted == false) ;
                        Cursor.Current = Cursors.Default;
                    }
                    else if (dr == DialogResult.Cancel)
                    {
                        return;
                    }

                }

                // save cal with hardcoded name
                pGuidedCal.Save("Demo_Cal");
                Write("MMEM:STOR:CSAR 'myState'");
                pGuidedCal.ApplyCalibration();

                MessageBox.Show("Calibration Complete");
          
            }
        }

        public class None : ENA_base
        {
            public override void initArray(){ }
            public override void set_Memory_map() { }
            public override int[] trace_offset(int Chan)
            {
                int[] arr= {1};
                return arr;
            }
            public override void InitializeENA(object InstrAddress)
            {
            }
            public override int openIviSession(string vnaIviAddr) { return -1; }
            public override string CheckTraceFormat(int channel, int tracenum) { return ""; }
            public override void Write(string cmd) { }
            public override void ReadData(int ChanNum,bool synchronization) { }
            public override double[] ReadENATrace(int channel, int trace_OneBased)
            {
                double[] arr = { 0.01 };
                return arr;
            }
            public override double[] ReadENATrace2(int channel, int trace_OneBased, byte site)
            {
                try
                {
                    double[] fullDATA_X = new double[measData[trace_OneBased - 1].Count()];
                    int i = 0;
                    foreach (float value in measData[trace_OneBased - 1])
                    {
                        fullDATA_X[i] = Convert.ToDouble(value);
                        i++;
                    }
                    return fullDATA_X;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    return new double[4];
                }
            }
            public override void Display() { }
            public override List<double> ReadFreqList_Chan(int channel)
            {
                try
                {
                    return FreqList[channel];
                }
                catch (Exception e)
                {
                    List<double> freqs = new List<double>();
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    return freqs;
                }
            }
            public override void ReadFreqList(int total_chan) { ; }
            public override string ReadString()
            {
                return "";
            }
            public override void ManualCal(){}
            public override void SendTrigger(bool Sync, int Chan, int Trace) { }
        }

    }
}
