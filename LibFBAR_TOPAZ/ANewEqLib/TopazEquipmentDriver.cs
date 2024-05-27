using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;

namespace LibFBAR_TOPAZ.ANewEqLib
{
    public class TopazEquipmentDriver
    {
        /// <summary>
        /// legacy ENA driver.
        /// </summary>
        public cENA ENA { get; set; }
        private S_Param[] m_sp;
        private List<string> ListNFCH_Freq { get; set; }
        private Dictionary<double, double> DicNF_Freq_Data { get; set; }
        private Dictionary<double, double> DicNFgain_Freq_Data { get; set; }

        public void Initialize(S_Param[] sp)
        {
            m_sp = sp;
            DicNF_Freq_Data = new Dictionary<double, double>();
            DicNFgain_Freq_Data = new Dictionary<double, double>();
        }

        public void InitEquipment(string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            ENA = new cENA();
            ENA.Address = address;
            ENA.OpenIO();
        }

        //For DIVA
        public void Init_DIVA(string port)
        {
            if (port == "6")
            {
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB3:FUNC \"NF_SOURCE\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB4:FUNC \"NF_RECEIVER3\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB5:FUNC \"NF_RECEIVER4\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB6:FUNC \"NF_RECEIVER5\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:MAIN4:FUNC \"NF_RECEIVER6\"");

            }
            else if (port == "10")
            {
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB3:FUNC \"NF_SOURCE\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB4:FUNC \"NF_SOURCE\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB5:FUNC \"NF_SOURCE\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:SUB6:FUNC \"NF_RECEIVER3\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:MAIN1:FUNC \"NF_RECEIVER4\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:MAIN2:FUNC \"NF_RECEIVER5\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:MAIN3:FUNC \"NF_RECEIVER6\"");
                ENA.BasicCommand.SendCommand("CONT:SIGN:KDMI:MAIN4:FUNC \"NF_RECEIVER9\"");
            }
            else
            {
                MessageBox.Show("Please check the port information for DIVA!");
            }
        }

        public void SetAvgCount(int ChannelNumber,int AvgCount)
        {
            for (int i = 1; i <= ChannelNumber; i++)
                ENA.BasicCommand.SendCommand($":sense" + i + ":average:count " + AvgCount);
        }

        public void SetAvgFunction(Boolean onoff, int ChannelNumber)
        {
            if (onoff == true)
            {
                for (int i = 1; i <= ChannelNumber; i++)
                    ENA.BasicCommand.SendCommand($":sense" + i + ":average:state ON");
            }
            else
            {
                for (int i = 1; i <= ChannelNumber; i++)
                    ENA.BasicCommand.SendCommand($":sense" + i + ":average:state OFF");
            }
        }

        public string[] GetChannelList()
        {
            string strlist_Chan = ENA.Sense.ReadCommand("SYST:CHAN:CAT?").Replace("\"", "").Replace("\n", "");            //List Channel list available in statefile
            string[] ListFbar_channel = strlist_Chan.Split(',');     //List Channel list available in statefile
            return ListFbar_channel;
        }

        /// <summary>
        /// Initialize SParamData.Freq
        /// </summary>
        public void InitializeFrequencyList(string[] ListFbar_channel, int totalChannel)
        {
            int traceNumber = 1;

            int intChnCount = 0;

            int ListFbar_chanLenght = ListFbar_channel.Length;      //List Channel list available in statefile
            for (int chn = 0; chn < totalChannel; chn++)
            {
                if (intChnCount >= ListFbar_chanLenght) break;
                if (Convert.ToInt32(ListFbar_channel[intChnCount]) == chn + 1)
                {
                    ENA.Format.DATA(e_FormatData.REAL);
                    var trace = ENA.Calculate.Par.GetAllCategory(chn + 1);
                    trace = trace.Replace("_", "").Replace("\n", "").Trim();
                    var allTraces = trace.Split(new char[] { ',' });

                    //Get only odd number
                    var traces = allTraces.Where((item, index) => index % 2 != 0).ToArray();
                    var traceCount = traces.Length;
                    var tmpFreqlist = ENA.Sense.Frequency.FreqList(chn + 1, traceNumber);
                    if (tmpFreqlist.Length > 0)
                    {
                        m_sp[chn].Freq = new double[tmpFreqlist.Length];
                        m_sp[chn].Freq = tmpFreqlist;
                    }

                    traceNumber += traceCount;
                    intChnCount++;
                }
                else
                {
                    traceNumber = traceNumber + m_sp[chn].TotalTraceCount;
                }

            }

        }

        public void MeasureResult_NF(int channelNumber)
        {
            DicNF_Freq_Data.Clear();
            DicNFgain_Freq_Data.Clear();

            string nfMode = "";
            //double[] readData = new double[ReadData[ChannelNumber - 1].Length];

            string[] nfFreqCount_array = ListNFCH_Freq[channelNumber - 1].Split(',');
            int nfFreqCount = nfFreqCount_array.Length;

            TopazEquipmentDriver.ReadBytes(channelNumber - 1);
            double[] readData = GetBytes(channelNumber - 1);
            //TriggerModel.ReadBytes(mappedFileView[ChannelNumber - 1], offsets_for_ReadData[ChannelNumber - 1][0], (MemorySize[ChannelNumber - 1] / sizeof(float)), ReadData[ChannelNumber - 1]);
            int actual_nfFreqCount = readData.Length;

            if (nfFreqCount == actual_nfFreqCount) { nfMode = "onlyNF"; }
            if (nfFreqCount == actual_nfFreqCount / 2) { nfMode = "NFandGain"; }

            switch (nfMode)
            {
                case "onlyNF":

                    string OnlyNFFreq = ListNFCH_Freq[channelNumber - 1];
                    string[] OnlyNFFreq_array = OnlyNFFreq.Split(',');

                    //for (int i = 0; i < ReadData[ChannelNumber - 1].Length; i++)
                    //{
                    //    readData[i] = (double)ReadData[ChannelNumber - 1][i];
                    //}

                    for (int i = 0; i < actual_nfFreqCount; i++)
                    {
                        DicNF_Freq_Data.Add(Convert.ToDouble(OnlyNFFreq_array[i]), Convert.ToDouble(readData[i]));
                    }
                    break;

                case "NFandGain":

                    string nfandGainFreq = ListNFCH_Freq[channelNumber - 1] + ',' + ListNFCH_Freq[channelNumber - 1];
                    string[] nfandGainFreq_array = nfandGainFreq.Split(',');

                    //for (int i = 0; i < ReadData[ChannelNumber - 1].Length; i++)
                    //{
                    //    readData[i] = (double)ReadData[ChannelNumber - 1][i];
                    //}

                    for (int i = 0; i < actual_nfFreqCount; i++)
                    {
                        if (i < (actual_nfFreqCount / 2))
                        {
                            DicNF_Freq_Data.Add(Convert.ToDouble(nfandGainFreq_array[i]), Convert.ToDouble(readData[i]));
                        }
                        else
                        {
                            DicNFgain_Freq_Data.Add(Convert.ToDouble(nfandGainFreq_array[i]), Convert.ToDouble(readData[i]));
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Need to check NF CH setting");
                    break;
            }
        }

        public void MeasureResult_NF_DUAL(int ChannelNumber, int MasterChannel, int SlaveChannel)
        {
            DicNF_Freq_Data.Clear();
            DicNFgain_Freq_Data.Clear();

            string master_nfMode = "";

            //Actual NF data count from memory map
            int actual_MasterNFCount = ReadData[MasterChannel - 1].Length;

            //NF Feq count from segment
            string[] masterNFfreq_array = ListNFCH_Freq[MasterChannel - 1].Split(',');
            int masterNFfreqCount = masterNFfreq_array.Count();

            //MasterCH NF data fetch
            int cn = ChannelNumber - 1;
            TopazEquipmentDriver.ReadBytes(cn);
            double[] masterReadData = GetBytes(cn);

            if (masterNFfreqCount == actual_MasterNFCount) { master_nfMode = "onlyNF"; }
            if (masterNFfreqCount == actual_MasterNFCount / 2) { master_nfMode = "NFandGain"; }

            switch (master_nfMode)
            {
                case "onlyNF":
                    for (int i = 0; i < actual_MasterNFCount; i++)
                    {
                        DicNF_Freq_Data.Add(Convert.ToDouble(masterNFfreq_array[i]), Convert.ToDouble(masterReadData[i]));
                    }
                    break;

                case "NFandGAin":
                    string masterNF_GainFreq = ListNFCH_Freq[MasterChannel - 1] + ',' + ListNFCH_Freq[MasterChannel - 1];
                    string[] masterNF_GainFreq_array = masterNF_GainFreq.Split(',');

                    for (int i = 0; i < actual_MasterNFCount; i++)
                    {
                        if (i < (actual_MasterNFCount / 2))
                        {
                            DicNF_Freq_Data.Add(Convert.ToDouble(masterNF_GainFreq_array[i]), Convert.ToDouble(masterReadData[i]));
                        }
                        else
                        {
                            DicNFgain_Freq_Data.Add(Convert.ToDouble(masterNF_GainFreq_array[i]), Convert.ToDouble(masterReadData[i]));
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Need to check NF CH setting");
                    break;
            }

            string slave_nfMode = "";
            //Actual NF data count from memory map
            int actual_SlaveNFCount = ReadData[SlaveChannel - 1].Length;

            //NF Freq count from segment
            string[] slaveNFfreq_array = ListNFCH_Freq[SlaveChannel - 1].Split(',');
            int slaveNFfreqCount = slaveNFfreq_array.Count();

            //SlaveCH NF data fetch
            TopazEquipmentDriver.ReadBytes(cn);
            double[] slaveReadData = GetBytes(cn);
            //double[] slaveReadData = new double[ReadData[SlaveChannel - 1].Length];
            //TriggerModel.ReadBytes(mappedFileView[SlaveChannel - 1], offsets_for_ReadData[SlaveChannel - 1][0], (MemorySize[SlaveChannel - 1] / sizeof(float)), ReadData[SlaveChannel - 1]);

            //for (int i = 0; i < ReadData[SlaveChannel - 1].Length; i++)
            //{
            //    slaveReadData[i] = (double)ReadData[SlaveChannel - 1][i];
            //}

            if (slaveNFfreqCount == actual_SlaveNFCount) { slave_nfMode = "onlyNF"; }
            if (slaveNFfreqCount == actual_SlaveNFCount / 2) { slave_nfMode = "NFandGain"; }

            switch (slave_nfMode)
            {
                case "onlyNF":
                    for (int i = 0; i < actual_SlaveNFCount; i++)
                    {
                        DicNF_Freq_Data.Add(Convert.ToDouble(slaveNFfreq_array[i]), Convert.ToDouble(slaveReadData[i]));
                    }
                    break;

                case "NFandGAin":
                    string slaveNF_GainFreq = ListNFCH_Freq[MasterChannel - 1] + ',' + ListNFCH_Freq[MasterChannel - 1];
                    string[] slaveNF_GainFreq_array = slaveNF_GainFreq.Split(',');

                    for (int i = 0; i < actual_SlaveNFCount; i++)
                    {
                        if (i < (actual_SlaveNFCount / 2))
                        {
                            DicNF_Freq_Data.Add(Convert.ToDouble(slaveNF_GainFreq_array[i]), Convert.ToDouble(slaveReadData[i]));
                        }
                        else
                        {
                            DicNFgain_Freq_Data.Add(Convert.ToDouble(slaveNF_GainFreq_array[i]), Convert.ToDouble(slaveReadData[i]));
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Need to check NF CH setting");
                    break;
            }
        }

        public double GetNfResult(double freq, int ChannelNumber)
        {
            double NFResult = -999;

            if (DicNF_Freq_Data.TryGetValue(freq, out NFResult))
            {
                if (double.IsNaN(NFResult) || double.IsInfinity(NFResult)) NFResult = -999; //Seoul
            }
            else
            {
                MessageBox.Show("Need to Check NF Target Frequency between TCF & Segment table at CH" + ChannelNumber);
            }

            return NFResult;
        }

        public double GetNfGainResult(double freq, int ChannelNumber)
        {
            double NFResult = -999;

            if (DicNFgain_Freq_Data.TryGetValue(freq, out NFResult))
            {
            }
            else
            {
                MessageBox.Show("Need to Check NF Target Frequency between TCF & Segment table at CH" + ChannelNumber);
            }

            return NFResult;
        }

        /// <summary>
        /// NF Initialization.
        /// </summary>
        public void NFCH_Freq(string[] ListFbar_channel, int totalChannel)
        {
            ListNFCH_Freq = new List<string>();

            for (int i = 0; i < totalChannel; i++)
            {
                bool isExist = IsChannelExist(ListFbar_channel, i + 1);
                if (!isExist)
                {
                    ListNFCH_Freq.Add("NA");
                    continue;
                }

                string chName = ENA.Sense.Class((i + 1));// ENA.BasicCommand.ReadCommand("SENS" + (i + 1) + ":CLAS:NAME?");


                if (chName.Contains("Noise"))
                {
                    ENA.Format.DATA(e_FormatData.ASC);
                    string active_trace = ENA.Calculate.Par.GetTraceCategory((i + 1));
                    ENA.BasicCommand.System.Operation_Complete();
                    active_trace = active_trace.Trim('\"', '\n');
                    string[] tr = active_trace.Split(',');
                    string nfFreq = ENA.Sense.Frequency.FreqList((i + 1), tr[0]);

                    ListNFCH_Freq.Add(nfFreq.Trim('\"', '\n'));
                }

                else
                {
                    ListNFCH_Freq.Add("Spara");
                }
            }
        }

        private bool IsChannelExist(string[] channelList, int channelNumberSearch)
        {
            foreach (string channel in channelList)
            {
                int chno = Convert.ToInt32(channel);
                if (chno == channelNumberSearch) return true;
            }
            return false;
        }

        // For NF Testing -End

        #region  additional code for NF cal & test 20160615

        public void Verify_ECAL_procedure(string Channel_Num)
        {
            ENA.Display.Window.Channel_Max(Convert.ToInt16(Channel_Num));
            ENA.BasicCommand.System.Operation_Complete();
            ENA.Sense.Correction.Collect.GuidedCal.ChannelMode(true);
            ENA.Trigger.Mode(e_TriggerMode.CONT, Convert.ToInt16(Channel_Num));
            ENA.BasicCommand.System.Operation_Complete();
        }

        public double Temp_Topaz()
        {
            //Seoul
            double readTemp = ENA.Sense.ReadTemp(e_TempUnit.CELS, 7); // Convert.ToDouble(ENA.BasicCommand.ReadCommand("SENS:TEMP? CELS,7"));
            return readTemp;
        }

        #endregion  --------------------  additional code for NF cal & test 20160615  ---------------------------
        public void Load_StateFile(string Filename)
        {
            if (string.IsNullOrEmpty(Filename))
            {
                return;
            }

            MPAD_TestTimer.LoggingManager.Instance.LogInfo("Equipment:Topaz: Reset.");
            ENA.BasicCommand.System.Reset();
            Thread.Sleep(10000);
            ENA.BasicCommand.System.Operation_Complete();
            MPAD_TestTimer.LoggingManager.Instance.LogInfo("Equipment:Topaz: Loading State File.");
            ENA.Memory.Load.State(Filename);
            Thread.Sleep(10000);
            ENA.BasicCommand.System.Operation_Complete();
            MPAD_TestTimer.LoggingManager.Instance.LogInfo("Equipment:Topaz: State File loaded.");
            //Original
            //ENA.BasicCommand.System.Reset();
            //Thread.Sleep(10000);
            //ENA.Memory.Load.State(Filename);
            //Thread.Sleep(5000);
            //ENA.Memory.Load.State(Filename);
            //Thread.Sleep(90000);

            // ENA.Display.Update(false);
            //ENA.Display.Visible(false);
        }

        [Obsolete] //ChoonChin - 20191205 - Due to hard coded total of channel
        public void Save_StateFile(string Filename)
        {
            // ChoonChin added for DIVA.
            //Turn off Averaging
            for (int iChn = 1; iChn < 92; iChn++) //Pinot proto2
            {
                ENA.BasicCommand.SendCommand($":SENS" + iChn + ":AVER OFF");
                ENA.BasicCommand.System.Operation_Complete();
            }

            //ChoonChin - 20191120 - Turn on display before saving
            ENA.BasicCommand.SendCommand("DISP:ENAB 1");

            ENA.Memory.Store.State(Filename);
        }
        public void Save_StateFile(string Filename, int TotalChannel)
        {
            // ChoonChin added for DIVA.
            //Turn off Averaging
            for (int iChn = 1; iChn < (TotalChannel + 1); iChn++)
            {
                ENA.BasicCommand.SendCommand($":SENS" + iChn + ":AVER OFF");
                ENA.BasicCommand.System.Operation_Complete();
            }

            //ChoonChin - 20191120 - Turn on display before saving
            ENA.BasicCommand.SendCommand("DISP:ENAB 1");

            ENA.Memory.Store.State(Filename);
        }

        public void SetTrigger(int totalChannel)
        {
            SetTriggerMode(e_TriggerMode.HOLD, totalChannel);
            SetTriggerSingle(e_TriggerSource.MAN);
            SetTrigger(e_TriggerScope.CURR);
            PreSingleTriggering(totalChannel);
        }
        public void SetTrigger(e_TriggerScope Scope)
        {
            ENA.Trigger.Scope(Scope);
        }
        public void SetTriggerMode(e_TriggerMode Trig, int totalChannel)
        {
            string[] ListFbar_channel = GetChannelList();

            for (int iChn = 0; iChn < totalChannel; iChn++)
            {
                bool isExist = IsChannelExist(ListFbar_channel, iChn + 1);
                if (!isExist) continue;
                ENA.Trigger.Mode(Trig, iChn + 1);
            }
        }
        public void SetTriggerSingle(e_TriggerSource Trig)
        {
            ENA.Trigger.Source(Trig);
            //for (int iChn = 0; iChn < TotalChannel; iChn++)
            //{
            //    ENA.Initiate.Continuous(iChn + 1, false);
            //}
        }
        public void PreSingleTriggering(int totalChannel)
        {
            string[] ListFbar_channel = GetChannelList();

            for (int iChn = 0; iChn < totalChannel; iChn++)
            {
                bool isExist = IsChannelExist(ListFbar_channel, iChn);
                if (!isExist) continue;
                ENA.Trigger.Single(iChn);
                ENA.BasicCommand.System.Operation_Complete();
            }
        }

        //ChoonChin - For Topaz temperature read back
        public string ReadTopazTemp(int Module)
        {
            return (ENA.Trigger.ReadVnaTemp(Module));
        }

        public string Identify_NA_Type()
        {
            return (ENA.BasicCommand.System.Equipment_ID());
        }

        public void SetDCUSB(string[] ListFbar_channel, int totalChannel)
        {
            //Disabled
            #region 28V DC with E36XX
            //string noiseSource = "NoiseSource1";
            //string gpibAddress = "GPIB1::6::INSTR";
            //string dcTiming = "E36XX_for_NoiseSource.xml";
            //string qq = "\"";

            //ENA.BasicCommand.SendCommand("SYST:CONF:EDEV:ADD " + qq + noiseSource + qq);
            //ENA.BasicCommand.SendCommand("SYST:CONF:EDEV:DTYPE " + qq + noiseSource + qq + "," + qq + "DC Source" + qq);
            //ENA.BasicCommand.SendCommand("SYST:CONF:EDEV:DRIV " + qq + noiseSource + qq + "," + qq + "DCSource" + qq);
            //ENA.BasicCommand.SendCommand("SYST:CONF:EDEV:IOC " + qq + noiseSource + qq + "," + qq + gpibAddress + qq);
            //ENA.BasicCommand.SendCommand("SYST:CONF:EDEV:LOAD " + qq + dcTiming + qq + "," + qq + noiseSource + qq);

            //for (int i = 85; i < 101; i++)
            //{
            //    ENA.BasicCommand.SendCommand("SENS" + i.ToString() + ":NOIS:EXDC:NAME " + qq + noiseSource + qq);
            //}
            #endregion 28V DC with E36XX 

            #region 28V USB DC Source
            string noiseSource = "346CH08_NoiseSource";
            string driverAddress = "TCPIP0::LOCALHOST::8025::SOCKET";
            //string driverAddress = "TCPIP0::CXK4Y12::8025::SOCKET";
            string dcTiming = "346CH08.xml";
            string qq = "\"";


            //Seoul
            ENA.ExternalDevice.Add(noiseSource);
            ENA.ExternalDevice.Type(noiseSource, "DC Source");
            ENA.ExternalDevice.Driver(noiseSource, e_EDeviceDriver.DCSource);
            ENA.ExternalDevice.Load(noiseSource, dcTiming);
            ENA.ExternalDevice.IOConfig(noiseSource, driverAddress);
            ENA.ExternalDevice.State(noiseSource, e_OnOff.Off);
            ENA.ExternalDevice.State(noiseSource, e_OnOff.On);

            for (int i = 0; i < totalChannel; i++)
            {
                bool isExist = IsChannelExist(ListFbar_channel, i + 1);
                if (!isExist) continue;
                string chName = ENA.Sense.Class((i + 1)); //ENA.BasicCommand.ReadCommand("SENS" + (i + 1) + ":CLAS:NAME?"); 
                ENA.BasicCommand.System.Operation_Complete();

                if (chName.Contains("Noise"))
                {
                    ENA.Sense.Noise.ExDCName(noiseSource, (i + 1));
                    //ENA.BasicCommand.SendCommand("SENS" + (i + 1) + ":NOIS:EXDC:NAME " + qq + noiseSource + qq);
                }
            }
            ENA.BasicCommand.System.Operation_Complete();
            #endregion 28V USB DC Source
        }

        public void Sweep_Speed(e_Format speed, string[] ListFbar_channel, int totalChannel)
        {
            int intChnCount = 0;
            for (int iChn = 0; iChn < totalChannel; iChn++)
            {
                if (intChnCount >= ListFbar_channel.Length) break;
                if (Convert.ToInt32(ListFbar_channel[intChnCount]) == iChn + 1)
                {
                    ENA.Sense.Sweep.SweepSpeed(iChn + 1, speed);
                    //    ENA.Initiate.Continuous(iChn + 1, false);
                    intChnCount++;
                }
            }
            if (speed == e_Format.FAST)
            {
                double time = 0;
                for (int iChn = 0; iChn < totalChannel; iChn++)
                {
                    if (intChnCount >= ListFbar_channel.Length) break;
                    if (Convert.ToInt32(ListFbar_channel[intChnCount]) == iChn + 1)
                    {
                        ENA.Sense.Sweep.DwellTime(time, iChn + 1);
                        intChnCount++;
                    }
                }
            }

        }

        private static int[] MemorySize;
        private static int[][] offsets_for_ReadData;
        private static float[][] ReadData;
        static MemoryMappedFile[] mappedFile;
        static MemoryMappedViewAccessor[] mappedFileView;

        public void SetMemoryMap(string[] ListFbar_channel, S_Param[] sp, int totalChannel)
        {
            string trace;
            string[] allTraces, traces;
            int traceNumber = 1;
            int traceCount;
            int nop_mem;
            offsets_for_ReadData = new int[totalChannel][];
            ReadData = new float[totalChannel][];
            MemorySize = new int[totalChannel];
            mappedFile = new MemoryMappedFile[totalChannel];
            mappedFileView = new MemoryMappedViewAccessor[totalChannel];

            int traceOffset = 0;

            // ENA.Memory.Store.InitMemory();
            int intChnCount = 0;

            for (int chn = 0; chn < totalChannel; chn++)
            {
                if (intChnCount >= ListFbar_channel.Length) break;
                if (Convert.ToInt32(ListFbar_channel[intChnCount]) != chn + 1)
                {
                    traceNumber = traceNumber + sp[chn].TotalTraceCount;
                    continue;
                }

                // ENA.Format.DATA(e_FormatData.REAL32);
                trace = ENA.Calculate.Par.GetTraceCategory(chn + 1);
                trace = trace.Substring(1, trace.Length - 2).Trim();
                // trace = trace.Replace("_", "").Replace("\n", "").Trim();
                allTraces = trace.Split(new char[] { ',', '\\' });
                // int nop_mem;

                //Get only odd number
                //traces = allTraces.Where((item, index) => index % 2 != 0).ToArray();
                traceCount = allTraces.Length;
                nop_mem = sp[chn].NoPoints;

                offsets_for_ReadData[chn] = new int[traceCount];
                ReadData[chn] = new float[2];

                ENA.Format.DATA(e_FormatData.REAL32);
                ENA.Memory.Store.InitMemory();

                // traceNumber = Convert.ToInt16(allTraces[0]);

                // StringBuilder TraceNumberStr = new StringBuilder();
                // StringBuilder TraceNumberStrOffset = new StringBuilder();
                // int i = 1;
                // for (int iParam = 0; iParam < (SParamData[chn].NoPorts * SParamData[chn].NoPorts); iParam++)
                //{
                //    Select_SParam_Def = TraceMatch[chn].SParam_Def_Number[iParam];
                //    if (Select_SParam_Def >= 0)
                //    {
                //        Select_SParam_Def_Arr = TraceMatch[chn].TraceNumber[Select_SParam_Def];
                //        //if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                //        {
                //         //   TraceNumberStr.AppendFormat("{0},", Select_SParam_Def_Arr);
                //          //  TraceNumberStrOffset.AppendFormat("{0},", Select_SParam_Def_Arr + 1 + topazTraceOffset);
                //            //SParamData[chn - 1].sParam_Data[Select_SParam_Def_Arr].Format = ENA.Calculate.Format.Format(chn + 1, traceNumber);   // SiteNumber

                //            // iTraceCount++;
                //            e_SFormat Format = SParamData[chn].sParam_Data[i - 1].Format = ENA.Calculate.Format.Format(chn + 1, traceNumber);   // SiteNumber
                //            if (Format == e_SFormat.SCOM || Format == e_SFormat.SMIT) ENA.Memory.Store.AddParameterSDATA(chn + 1, traceNumber, nop_mem);
                //            else ENA.Memory.Store.AddParameterFDATA(chn + 1, traceNumber, nop_mem);

                //            offsets_for_ReadData[chn][i - 1] = ENA.Memory.Store.ReturnOffset();
                //            //if (i == traceNumber + traceCount - 1 && chn==TotalChannel-1)
                //            //{
                //            //    offsets_for_complex_data[i] = offsets_for_complex_data[i- 1] + nop_mem * 4;
                //            //}
                //            traceNumber++;
                //            i++;
                //        }
                //         //traceOffset++;

                //    }
                //}
                //Array.Resize(ref offsets_for_ReadData[chn], i-1);
                // incr_DataTrigger();
                for (int i = 1; i <= traceCount; i++)
                {

                    e_SFormat Format = sp[chn].sParam_Data[i - 1].Format = ENA.Calculate.Format.Format(chn + 1, traceNumber);   // SiteNumber
                    if (Format == e_SFormat.SCOM || Format == e_SFormat.SMIT) ENA.Memory.Store.AddParameterSDATA(chn + 1, traceNumber, nop_mem);
                    else ENA.Memory.Store.AddParameterFDATA(chn + 1, traceNumber, nop_mem);

                    offsets_for_ReadData[chn][i - 1] = ENA.Memory.Store.ReturnOffset();
                    //if (i == traceNumber + traceCount - 1 && chn==TotalChannel-1)
                    //{
                    //    offsets_for_complex_data[i] = offsets_for_complex_data[i- 1] + nop_mem * 4;
                    //}
                    traceNumber++;
                }

                // traceNumber += traceCount;

                ENA.Memory.Store.AllocateMemory("VNA_MemoryMap", chn + 1);
                // M9485.System.IO.WriteString("SYST:DATA:MEM:COMM 'VNA_MemoryMap'");
                // ENA.Memory.Store.AllocateMemory("MX_MemoryMap");
                MemorySize[chn] = ENA.Memory.Store.SizeOfMemory();

                mappedFile[chn] = MemoryMappedFile.CreateOrOpen("VNA_MemoryMap" + (chn + 1).ToString(), MemorySize[chn]);
                mappedFileView[chn] = mappedFile[chn].CreateViewAccessor();
                Array.Resize(ref ReadData[chn], MemorySize[chn] / sizeof(float));

                intChnCount++;
            }
            //  string aaa= ENA.Memory.Store.MemoryCatalog();
        }

        public static unsafe void ReadBytes(int channelNumber)
        {
            ReadBytes(mappedFileView[channelNumber], offsets_for_ReadData[channelNumber][0],
                (MemorySize[channelNumber] / sizeof(float)), ReadData[channelNumber]);
        }

        private static unsafe void ReadBytes(MemoryMappedViewAccessor mappedFileView,
            int offset, int num, float[] arr)
        {
            // This is equivalent to: 
            //         //m_mappedFileView.ReadArray<float>(...);
            // But, using this "unsafe" code is 30 times faster. 100usec versus 3ms
            byte* ptr = (byte*)0;
            mappedFileView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(new IntPtr(ptr), offset), arr, 0, num);
            mappedFileView.SafeMemoryMappedViewHandle.ReleasePointer();
        }

        public double[] GetBytes(int channelNumber)
        {
            double[] readData = new double[ReadData[channelNumber].Length];
            for (int i = 0; i < ReadData[channelNumber].Length; i++)
            {
                readData[i] = (double)ReadData[channelNumber][i];
            }
            return readData;
        }

        #region set new state file

        private int TraceNumberZNB;

        /// <summary>
        /// Set new state file.
        /// </summary>
        public void SetNewStateFile(List<StateFileDataObject> sfList, int totalChannel, 
            bool[] cprojectPortEnable)
        {
            ReadTraceMatching1(totalChannel);
            TraceNumberZNB = 1;
            foreach (StateFileDataObject sf in sfList)
            {
                string msg = String.Format("Set new state file - Channel {0}", sf.ChannelNumber + 1);
                LoggingManager.Instance.LogInfo(msg);
                TraceNumberZNB = ReadTraceMatching2(sf, cprojectPortEnable, TraceNumberZNB);
            }
        }

        private void ReadTraceMatching1(int totalChannel)
        {
            ENA.Sense.SendCommand("SYST:PRES");
            ENA.BasicCommand.System.Operation_Complete();

            ENA.Display.Window.Window_Layout(totalChannel);
            ENA.BasicCommand.System.Operation_Complete();
            ENA.Sense.SendCommand("DISP:UPD On");
            ENA.Sense.SendCommand("CALC:PAR:DEL:ALL");
            ENA.BasicCommand.System.Operation_Complete();
            //int NextTrNo = Convert.ToUInt16(ENA.Sense.ReadCommand("DISP:WIND" + 1 + ":TRAC:NEXT?"));     //Added by cheeon 12-July-2017 // (Optimal) Return the next unused trace number in Window #2. Assume 2 as trace number 1 already exist  (=Tr 2)
            //ENA.BasicCommand.System.Operation_Complete();
        }

        private int ReadTraceMatching2(StateFileDataObject sf, bool[] cprojectPortEnable, int startingTraceNumber)
        {
            // Trace maker and NF setting
            int createdTraceCount = Set_ZNBChannel(sf, cprojectPortEnable, startingTraceNumber); 
            int real_channel = sf.ChannelNumber + 1;
            ENA.Sense.SendCommand("TRIG:SOUR MAN");
            ENA.Sense.SendCommand("SENS" + real_channel + ":SWE:MODE HOLD");
            ENA.Sense.SendCommand("INIT" + real_channel + ":IMM");
            ENA.BasicCommand.System.Operation_Complete();

            return createdTraceCount;
        }

        private int Set_ZNBChannel(StateFileDataObject sf, bool[] cprojectPortEnable, int startingTraceNumber) //seoul
        {
            string NFmeasName = "'noiseFig'";
            //ENA.Display.Window.Activate(ChannelNumber + 1);
            //ENA.Display.Window.Channel_Max(true);
            int real_channel = sf.ChannelNumber + 1;
            TraceNumberZNB = startingTraceNumber;

            if (sf.NF_source_P_string != "")
            {
                double sweep_T1 = Convert.ToDouble(sf.NF_sweep_Time_string) * 10;   // sweep time = Topaz setting time / 10

                // NF setting on Display
                NFmeasName = "'noiseFig" + real_channel + "'";
                ENA.Sense.SendCommand("CALC" + real_channel + ":CUST:DEF " + NFmeasName + ", 'Noise Figure Cold Source', 'NF'");
                ENA.Sense.SendCommand("DISP:WIND" + real_channel + ":TRAC:FEED " + NFmeasName);
                ENA.Sense.SendCommand("CALC" + real_channel + ":PAR:SEL " + NFmeasName + ",fast");

                // Set measurement parameters (Power)
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:PMAP " + sf.NF_source_P_string + "," + sf.NF_rcv_P_string); // Assing VNA ports for NF (DUT input port, DUT output port)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_source_P_string + " -20"); // Set VNA source power to -20dBm for S21 measurement (DUT input port)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_rcv_P_string + " -20"); // Set VNA source power to -20dBm for S21 measurement (DUT output port)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_source_P_string + ":ALC:MODE OPEN"); // Set VNA port ALC mode to OPEN (DUT input port)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_rcv_P_string + ":ALC:MODE OPEN"); // Set VNA port ALC mode to OPEN (DUT output port)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_source_P_string + ":ATT:REC:REF 0"); // Set VNA port reference attenuator to 0dB (DUT input port, only for DRA)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_source_P_string + ":ATT:REC:TEST 0"); // Set VNA port test attenuator to 0dB (DUT input port, only for DRA)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_rcv_P_string + ":ATT:REC:REF 0"); // Set VNA port reference attenuator to 0dB (DUT input port, only for DRA)
                ENA.Sense.SendCommand("SOUR" + real_channel + ":POW" + sf.NF_rcv_P_string + ":ATT:REC:TEST 0"); // Set VNA port test attenuator to 0dB (DUT input port, only for DRA)

                // Set measurement parameters (Noise Figure)
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:REC NOISe"); // Set Noise Receiver mode
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:BWID:RES 4e6"); // Set Noise Bandwidth to 4MHz
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:AVER:COUN " + sweep_T1); // Set Noise Average count to 200 (20 msec)
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:AVER:STAT 1"); // Enable Noise Average
                ENA.Sense.SendCommand("SENS" + real_channel + ":NOIS:TEMP:AMB 302.8"); // Lindsay
                //SENS:SEGM:FREQ:STAR 1MHZ
                TraceNumberZNB = TraceNumberZNB + 1;

            }

            if (sf.NF_source_P_string == "") ENA.Calculate.Par.Count(real_channel, sf.FilteredTraceNameList.Count);

            // ENA.Calculate.Par.Define_Channel(ChannelNumber + 1);

            Set_SegmentParam(sf.ChannelNumber, sf.SegmentTable, cprojectPortEnable);

            foreach (string traceName in sf.FilteredTraceNameList)
            {
                if (sf.NF_source_P_string == "")
                {
                    //TraceNumber = TraceMatch[ChannelNumber].TraceNumber[iDef] + 1;
                    ENA.Calculate.Par.Define_Trace(real_channel, TraceNumberZNB, traceName);
                    ENA.BasicCommand.System.Operation_Complete();
                    //  Thread.Sleep(200);
                    if (traceName.Contains("GDEL"))
                    {

                        ENA.Calculate.Format.Format(real_channel, TraceNumberZNB, e_SFormat.GDEL);
                    }
                    else ENA.Calculate.Format.Format(real_channel, TraceNumberZNB, e_SFormat.SMIT); //ENA.Calculate.Format.Format(ChannelNumber + 1, TraceNumber, e_SFormat.SCOM);

                    ENA.BasicCommand.System.Operation_Complete();
                    TraceNumberZNB = TraceNumberZNB + 1;

                }
            }

            return TraceNumberZNB;
        }

        private void Set_SegmentParam(int iChn, s_SegmentTable SegmentTable, bool[] enabledPortList)
        {
            ENA.Sense.Segment.Add_SegmentTable2String(iChn + 1, SegmentTable, enabledPortList);
            ENA.Sense.Sweep.Type(iChn + 1, e_SweepType.SEGM);
            ENA.BasicCommand.System.Operation_Complete();
            Thread.Sleep(500);
        }
        #endregion
    }

    /// <summary>
    /// Fill SParamData with measurement from instrument.
    /// </summary>
    public class TriggerModel
    {
        static cFunction Math_Func = new cFunction();

        /// <summary>
        /// Copy Inputdata to sp.
        /// </summary>
        /// <param name="InputData"></param>
        /// <param name="sp"></param>
        /// <param name="tm"></param>
        /// <param name="TraceNumber"></param>
        public void TransferEnhanceData(double[] InputData, S_Param sp, s_TraceMatching tm,
            string TraceNumber)
        {
            string[] TraceArr = TraceNumber.Split(',');
            int TraceCount = TraceArr.Length;
            int BasePoint = sp.Freq.Length;
            int GDoffset = 0;

            for (int iTrace = 0; iTrace < TraceCount; iTrace++)
            {
                int SParamDef = int.Parse(TraceArr[iTrace]);
                S_ParamData sp2 = sp.sParam_Data[SParamDef];
                int Points; // = InputData.Length / 2 / TraceCount
                int Offset;
                if (sp2.Format == e_SFormat.GDEL)// && (SParamDef > 0))
                {
                    Points = BasePoint;
                    GDoffset += BasePoint;
                    Offset = SParamDef * Points;
                }
                else
                {
                    Points = BasePoint * 2;
                    Offset = SParamDef * Points - GDoffset;
                }

                //if (TraceCount == 1 || SParamDef == 0) Offset = 0; // Original Source code

                if ((TraceCount == 1) && (SParamDef != 0)) { }
                else if (TraceCount == 1 || SParamDef == 0) Offset = 0;

                sp2.sParam = CreateArray(Points);

                ExpectedGain ExpectedGain = new ExpectedGain();
                ExpectedGain.dB = new double[BasePoint];
                ExpectedGain.Freq = new double[BasePoint];

                for (int iPts = 0; iPts < BasePoint; iPts++)
                {
                    s_DataType spDt = sp.sParam_Data[SParamDef].sParam[iPts];

                    switch (sp2.Format)
                    {
                        case e_SFormat.MLOG:
                        case e_SFormat.SLIN:
                            spDt.dBAng.dB = InputData[Offset + (iPts * 2)];
                            spDt.dBAng.Angle = InputData[Offset + (iPts * 2) + 1];
                            break;
                        case e_SFormat.GDEL:
                        case e_SFormat.GDEL2:
                            spDt.dBAng.dB = InputData[iPts + (Offset * 2)];
                            spDt.dBAng.Angle = 0;
                            break;
                        case e_SFormat.SCOM:
                        case e_SFormat.SMIT:
                            int currentPoint = Offset + (iPts * 2);
                            FillReImAng(InputData, spDt, currentPoint);
                            TransferExpectedGain(sp, spDt, tm, SParamDef, iPts, ExpectedGain);
                            break;
                    }
                }

            }

            //yoonchun no longer use
            //try
            //{
            //    DicExpectedGain.Add(FileOutput_Mode.ToString(), ExpectedGain);
            //}
            //catch
            //{
            //    int dsadsad = 0;
            //}


        }

        private static void FillReImAng(double[] InputData, s_DataType spDt, int currentPoint)
        {
            spDt.ReIm.Real = InputData[currentPoint];
            spDt.ReIm.Imag = InputData[currentPoint + 1];
            dB_Angle tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(spDt.ReIm);
            spDt.dBAng = tmp_dBAng;
        }

        private static void TransferExpectedGain(S_Param sp, s_DataType spDt, s_TraceMatching tm, int SParamDef, int iPts,
            ExpectedGain ExpectedGain)
        {
            double db = Convert.ToDouble(spDt.dBAng.dB);

            int EnumValue = (int)Enum.Parse(typeof(e_SParametersDef), "S32");
            var tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S32");
            if (tm.TraceNumber[tmpSParamDef.GetHashCode()] != -1 && tm.TraceNumber[EnumValue] == SParamDef)
            {
                ExpectedGain.dB[iPts] = db;
                ExpectedGain.Freq[iPts] = sp.Freq[iPts];
            }

            EnumValue = (int)Enum.Parse(typeof(e_SParametersDef), "S42");
            tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S42");
            if (tm.TraceNumber[tmpSParamDef.GetHashCode()] != -1 && tm.TraceNumber[EnumValue] == SParamDef)
            {
                ExpectedGain.dB[iPts] = db;
                ExpectedGain.Freq[iPts] = sp.Freq[iPts];
            }

            EnumValue = (int)Enum.Parse(typeof(e_SParametersDef), "S52");
            tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S52");
            if (tm.TraceNumber[tmpSParamDef.GetHashCode()] != -1 && tm.TraceNumber[EnumValue] == SParamDef)
            {
                ExpectedGain.dB[iPts] = db;
                ExpectedGain.Freq[iPts] = sp.Freq[iPts];
            }

            EnumValue = (int)Enum.Parse(typeof(e_SParametersDef), "S62");
            tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S62");
            if (tm.TraceNumber[tmpSParamDef.GetHashCode()] != -1 && tm.TraceNumber[EnumValue] == SParamDef)
            {
                ExpectedGain.dB[iPts] = db;
                ExpectedGain.Freq[iPts] = sp.Freq[iPts];
            }
        }

        /// <summary>
        /// Fill SParam.SParamData.sParam data with result from NA.
        /// </summary>
        public void TransferData(double[] InputData, S_ParamData sp, e_SFormat Format)
        {
            int Points;
            //KCC - Added Lin Mag

            Points = InputData.Length / 2;

            sp.sParam = new s_DataType[Points];

            for (int iPts = 0; iPts < Points; iPts++)
            {
                if (Format == e_SFormat.SCOM || Format == e_SFormat.SMIT)
                {
                    sp.sParam[iPts].ReIm.Real = InputData[iPts * 2];
                    sp.sParam[iPts].ReIm.Imag = InputData[(iPts * 2) + 1];
                    var tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(sp.sParam[iPts].ReIm);
                    sp.sParam[iPts].dBAng = tmp_dBAng;

                    //KCC - Lin Mag
                    var tmp_MagAng = Math_Func.Conversion.conv_RealImag_to_MagAngle(sp.sParam[iPts].ReIm);
                    sp.sParam[iPts].MagAng = tmp_MagAng;
                }
                else if (Format == e_SFormat.MLOG)
                {
                    sp.sParam[iPts].dBAng.dB = InputData[iPts * 2];
                    sp.sParam[iPts].dBAng.Angle = InputData[(iPts * 2) + 1];
                }
                else if (Format == e_SFormat.GDEL || Format == e_SFormat.GDEL2)
                {
                    sp.sParam[iPts].dBAng.dB = InputData[iPts * 2];
                    sp.sParam[iPts].dBAng.Angle = 0;
                }
            }
        }

        
        public int SParam_Def2Value(s_TraceMatching[] tm, int SParamDef, int Channel_Number)
        {
            //int rtnRslt;
            //rtnRslt = 0;
            //for (int iTrace = 0; iTrace < TraceMatch[ChannelNumber].TraceNumber.Length; iTrace++)
            //{
            //    if (TraceMatch[ChannelNumber].TraceNumber[iTrace] == SParamDef)
            //    {
            //        rtnRslt = iTrace;
            //        break;
            //    }
            //}
            //return (rtnRslt);
            return (tm[Channel_Number].SParam_Def_Number[SParamDef]);
        }

        private s_DataType[] CreateArray(int size)
        {
            s_DataType[] dt = new s_DataType[size];
            for (int i = 0; i < size; i++)
            {
                dt[i] = new s_DataType();
            }
            return dt;
        }

    }

    public class ExpectedGain
    {
        public double[] Freq;
        public double[] dB;
    }

    public class StateFileDataObject
    {
        /// <summary>
        /// zero-indexed number, not real channel number.
        /// </summary>
        public int ChannelNumber { get; set; }

        public List<string> FilteredTraceNameList { get; set; }

        /// <summary>
        /// Segment table of this channel.
        /// </summary>
        public s_SegmentTable SegmentTable { get; set; }

        /// <summary>
        /// NF Settings.
        /// </summary>
        public string NF_source_P_string { get; set; }
        public string NF_rcv_P_string { get; set; }
        public string NF_sweep_Time_string { get; set; }

        public StateFileDataObject()
        {
            FilteredTraceNameList = new List<string>();
        }
    }
}