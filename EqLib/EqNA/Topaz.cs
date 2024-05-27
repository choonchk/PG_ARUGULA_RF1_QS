using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace EqLib.NA
{
    public class Topaz : NetworkAnalyzerAbstract
    {
        readonly int _intTimeOut = 90000;
        string CalKitName { get; set; }

        public static int[] MemorySize;
        public static int[][] offsets_for_ReadData;
        public static float[][] ReadData;
        public static MemoryMappedFile[] mappedFile;
        public static MemoryMappedViewAccessor[] mappedFileView;

        public Topaz(string address = "TCPIP0::localhost::hislip0::instr")
        {
            try
            {
                base.OpenIo(address, _intTimeOut);
                base.Operation_Complete();
                SendCommand("FORM REAL, 64");
                SendCommand("FORM:BORD NORM");
            }
            catch (Exception ex)
            {
                DisplayError("TOPAZ address mismatch -" + address, ex);
            }
        }
        public override void LoadState(string filename)
        {
            DisplayOn(false);
            SendCommand("FORM:DATA REAL, 64");
            SendCommand("FORM:BORD NORM");
            base.Operation_Complete();
            SendCommand("MMEM:LOAD \"" + filename.Trim() + "\"");
            System.Threading.Thread.Sleep(7000);           
            base.Operation_Complete();
            DisplayOn(true);
        }

        public override void SaveState(string filename)
        {
            SendCommand("FORM:DATA REAL, 64");
            SendCommand("FORM:BORD NORM");
            SendCommand("MMEM:STOR \"" + filename.Trim() + ".csa\"");
            base.Operation_Complete();
        }
        public override void DisplayOn(bool state)
        {
            switch (state)
            {
                case true:
                    SendCommand("DISP:UPD ON");
                    //SendCommand("DISP:ENAB ON");
                    //SendCommand("DISP:VIS ON");
                    break;
                case false:
                    SendCommand("DISP:UPD OFF");
                    //SendCommand("DISP:ENAB OFF");
                    //SendCommand("DISP:VIS OFF");
                    break;
            }
        }

        public override void Averaging(int channelNumber, naEnum.EOnOff val)
        {
            SendCommand("SENS" + channelNumber + ":AVER " + val);
        }
        public override void AveragingMode(int channelNumber, string val)
        {
            SendCommand("SENS" + channelNumber + ":AVER:MODE " + val);
        }
        public override void AveragingFactor(int channelNumber, int val)
        {
            SendCommand("SENS" + channelNumber + ":AVER:COUN " + val);
        }

        public override void ActiveChannel(int channelNumber)
        {
            string tmpStr;


            tmpStr = ReadCommand("CALC" + channelNumber.ToString() + ":PAR:CAT?");
            if (tmpStr.Contains("NO CATALOG")) return;
            tmpStr = tmpStr.Replace("ch" + channelNumber + "_tr", "");
            tmpStr = tmpStr.Replace("\"", "");
            tmpStr = tmpStr.Replace("'", "").Replace("\n", "").Trim();

            
            string[] arrRet = tmpStr.Split(',');

            SendCommand("CALC" + channelNumber + ":PAR:SEL '" + arrRet[0] +"'");
        }

        public override void ChannelMax(int channelNumber, bool state)
        {
            switch (state)
            {
                case true:
                    SendCommand("DISP:WIND:SIZE MAX");
                    break;
                case false:
                    SendCommand("DISP:WIND:SIZE NORM");
                    break;
            }
        }

        public override void TriggerSource(naEnum.ETriggerSource trigSource)
        {
            String val = trigSource.ToString();
            if (trigSource == naEnum.ETriggerSource.INT | trigSource == naEnum.ETriggerSource.BUS)
                val = "MAN";
            
            SendCommand("TRIG:SEQ:SOUR " + val);
            //SendCommand("TRIG:SEQ:SCOP CURR");
        }
        public override void TriggerMode(naEnum.ETriggerMode trigMode)
        {
            SendCommand("SYST:CHAN:HOLD");
            //SendCommand("SENS:SWE:MODE " + trigMode.ToString().ToUpper());
        }
        public override void TriggerSingle(int channelNumber)
        {
            //ActiveChannel(channelNumber);
            //SendCommand("TRIG:SCOP ACT");
            SendCommand("INIT" + channelNumber + ":IMM");
            Operation_Complete();
        }

       

        public override string GetTraceInfo(int channelNumber)
        {
            string ret = "";
            string tmpStr = "";
            tmpStr = ReadCommand("CALC" + channelNumber.ToString() + ":PAR:CAT?");

            tmpStr = tmpStr.Replace("ch" + channelNumber + "_tr", "");
            tmpStr  = tmpStr.Replace("\"","");
            tmpStr = tmpStr.Replace("'", "").Replace("\n", "").Trim();
            string [] arrTrace = tmpStr.Split(new char[] { ',' });
            tmpStr = ReadCommand("SYST:MEAS:CAT? " + channelNumber.ToString());
            tmpStr = tmpStr.Replace("\"", "");
            tmpStr = tmpStr.Replace("'", "").Replace("\n", "").Trim();
            string[] arrMnum = tmpStr.Split(new char[] { ',' });
            tmpStr = "";
            for (int i = 0; i < arrMnum.Length;i++)
            {
                arrTrace[i*2] = arrMnum[i];
                tmpStr +=  arrTrace[i * 2] + "," + arrTrace[(i*2)+1] + ",";
            }
            
            ret = tmpStr.Remove(tmpStr.Length - 1, 1);

            return ret;
        }
        public override naEnum.ESFormat GetTraceFormat(int channelNumber, int traceNumber)
        {
            string tmp = ReadCommand("CALC" + channelNumber.ToString() + ":MEAS" + traceNumber.ToString() + ":FORM?").Replace("\n", "");
            naEnum.ESFormat format = (naEnum.ESFormat)Enum.Parse(typeof(naEnum.ESFormat), tmp);
            return format;
        }
        public override double[] GetFreqList(int channelNumber)
        {
            string ret = GetTraceInfo(channelNumber);
            string[] arrRet = ret.Split(',');
            SendCommand("FORM REAL, 64");
            SendCommand("FORM:BORD NORM");
            return ReadIeeeBlock("CALC" + channelNumber + ":MEAS" + arrRet[0] +":X?");
        }

        public override double[] GrabRealImagData(int channelNumber)

        {
            
            double[] tmpDat;
            ReadBytes(mappedFileView[channelNumber], offsets_for_ReadData[channelNumber][0], (MemorySize[channelNumber] / sizeof(float)), ReadData[channelNumber]);
  

            tmpDat = Array.ConvertAll(ReadData[channelNumber],x => (double)x);
            return tmpDat;

            SendCommand("FORM REAL, 64");
            SendCommand("FORM:BORD NORM");

            string traceNumber = "";
            string tmp = GetTraceInfo(channelNumber);
            tmp = tmp.Replace("'", "").Replace("\n", "").Trim();
            string[] arrTrace = tmp.Split(new char[] { ',' });
            for (int i = 0; i < arrTrace.Length; i++)
            {
                traceNumber = traceNumber + arrTrace[i].ToString() + ",";
                i++;
            }
            traceNumber = traceNumber.Remove(traceNumber.Length - 1, 1);
            tmpDat = (ReadIeeeBlock("CALC" + channelNumber.ToString() + ":DATA:MFD? \"" + traceNumber + "\""));

            return tmpDat;



        }

        private unsafe void ReadBytes(MemoryMappedViewAccessor mappedFileView,
            int offset, int num, float[] arr)
        {
            byte* ptr = (byte*)0;
            mappedFileView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(new IntPtr(ptr), offset), arr, 0, num);
            mappedFileView.SafeMemoryMappedViewHandle.ReleasePointer();
        }

        public override void setMemoryMap()
        {
            int intChnCount = 0;
            int TotalChannel = 0;
            try
            {
                string trace;
                string[] allTraces, traces;
                int traceNumber = 1;
                int traceCount;
                int nop_mem;
                //offsets_for_ReadData = new int[TotalChannel][];
                //ReadData = new float[TotalChannel][];
                //MemorySize = new int[TotalChannel];
                //mappedFile = new MemoryMappedFile[TotalChannel];
                //mappedFileView = new MemoryMappedViewAccessor[TotalChannel];

                int Select_SParam_Def;
                int Select_SParam_Def_Arr;
                int traceOffset = 0;

                // ENA.Memory.Store.InitMemory();

                string[] ListFbar_channel;
                string strlist_Chan;
                

                strlist_Chan = ReadCommand("SYST:CHAN:CAT?").Replace("\"", "").Replace("\n", "");            //List Channel list available in statefile
                ListFbar_channel = strlist_Chan.Split(',');         //List Channel list available in statefile
                int ListFbar_chanLenght = ListFbar_channel.Length;      //List Channel list available in statefile
                //TotalChannel = ListFbar_chanLenght;
                int.TryParse(ListFbar_channel[ListFbar_chanLenght-1],out TotalChannel);

                offsets_for_ReadData = new int[TotalChannel + 1][];
                ReadData = new float[TotalChannel +1 ][];
                MemorySize = new int[TotalChannel + 1];
                mappedFile = new MemoryMappedFile[TotalChannel + 1];
                mappedFileView = new MemoryMappedViewAccessor[TotalChannel + 1];


                for (int chn = 0; chn < ListFbar_chanLenght; chn++)
                {
                    int ChnNo;
                    int.TryParse(ListFbar_channel[chn],out ChnNo);

                    if (chn == 195)
                    {
                        //debug purpose
                        int dasdsa = 0;
                        //intChnCount++;
                        //continue;
                    }
                
                        //if (intChnCount >= ListFbar_chanLenght) break;

                        //if (Convert.ToInt32(ListFbar_channel[intChnCount]) == chn + 1)
                        //{


                            trace = ReadCommand("SYST:MEAS:CAT? " + (ChnNo));//ENA.Calculate.Par.GetTraceCategory(chn + 1);
                            trace = trace.Substring(1, trace.Length - 2).Trim();
                    trace = trace.Replace("_", "").Replace("\"", "").Trim();
                    allTraces = trace.Split(new char[] { ',', '\\' });
                            // int nop_mem;

                            //Get only odd number
                            //traces = allTraces.Where((item, index) => index % 2 != 0).ToArray();
                            traceCount = allTraces.Length;

                            double[] fList = GetFreqList(ChnNo);
                            nop_mem = fList.Length;//SParamData[chn].NoPoints;

                            offsets_for_ReadData[ChnNo] = new int[traceCount];
                            ReadData[ChnNo] = new float [2];

                            //****************************************/
                            //ENA.Format.DATA(e_FormatData.REAL32);
                            //ENA.Memory.Store.InitMemory();

                            SendCommand("FORM:DATA REAL, 64");
                            SendCommand("FORM:BORD NORM");
                            SendCommand("SYST:DATA:MEM:INIT\n");
                        //****************************************/

                        for (int i = 1; i <= traceCount; i++)
                        {
                        int.TryParse(allTraces[i-1], out traceNumber);
                            naEnum.ESFormat Format = GetTraceFormat(ChnNo, traceNumber);
                            //e_SFormat Format = SParamData[chn].sParam_Data[i - 1].Format = ENA.Calculate.Format.Format(chn + 1, traceNumber);   // SiteNumber

                            if (Format == naEnum.ESFormat.SCOM || Format == naEnum.ESFormat.SMIT)
                                //ENA.Memory.Store.AddParameterSDATA(chn + 1, traceNumber, nop_mem);
                                SendCommand("SYST:DATA:MEM:ADD '" + ChnNo.ToString() + ":" + traceNumber.ToString() + ":SDATA:" + nop_mem + "'");
                            else
                                //ENA.Memory.Store.AddParameterFDATA(chn + 1, traceNumber, nop_mem);
                                SendCommand("SYST:DATA:MEM:ADD '" + ChnNo.ToString() + ":" + traceNumber.ToString() + ":FDATA:" + nop_mem + "'");

                            Operation_Complete();

                            offsets_for_ReadData[ChnNo][i-1] = int.Parse(ReadCommand("SYST:DATA:MEM:OFFSet?"));//ENA.Memory.Store.ReturnOffset();
                            Operation_Complete();
                            //traceNumber++;
                        }

                            //ENA.Memory.Store.AllocateMemory("VNA_MemoryMap", chn + 1);
                            SendCommand("SYST:DATA:MEM:COMM '" + "VNA_MemoryMap" + ChnNo.ToString() + "'");
                            Operation_Complete();
                            MemorySize[ChnNo] = int.Parse(ReadCommand("SYST:DATA:MEM:SIZE?"));//ENA.Memory.Store.SizeOfMemory();
                            Operation_Complete();
                            mappedFile[ChnNo] = MemoryMappedFile.CreateOrOpen("VNA_MemoryMap" + ChnNo.ToString(), MemorySize[ChnNo]);
                            mappedFileView[ChnNo] = mappedFile[ChnNo].CreateViewAccessor();
                            Array.Resize(ref ReadData[ChnNo], MemorySize[ChnNo] / sizeof(float));

                            intChnCount++;
                        //}
                        //else
                        //{
                        //    traceNumber = traceNumber;// + SParamData[chn].TotalTraceCount;
                        //intChnCount++;
                        //}
                    
                }
            }
            catch
            {
                int a = intChnCount;
            }
            //  string aaa= ENA.Memory.Store.MemoryCatalog();
        }

        public override void GetSegmentTable(out SSegmentTable segmentTable, int channelNumber)
        {
            String tmpStr = "";
            int SegCount = 0, i = 0;
            int SegDataCount = 0;
            segmentTable = new SSegmentTable();

            SendCommand("FORM:DATA ASC");

            tmpStr = ReadCommand("SENS" + channelNumber.ToString() + ":SEGM:BWID:CONT?");
            segmentTable.Ifbw = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), tmpStr);


            segmentTable.Mode = (naEnum.EModeSetting)Enum.Parse(typeof(naEnum.EModeSetting), "0");

            tmpStr = ReadCommand("SENS" + channelNumber.ToString() + ":SEGM:SWE:DWEL:CONT?");
            segmentTable.Del = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), tmpStr);

            tmpStr = ReadCommand("SENS" + channelNumber.ToString() + ":SEGM:POW:CONT?");
            segmentTable.Pow = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), tmpStr);
            

            tmpStr = ReadCommand("SENS" + channelNumber.ToString() + ":SEGM:COUN?");
            SegCount = int.Parse(tmpStr);
            segmentTable.SegmentData = new SSegmentData[SegCount];

            tmpStr = ReadCommand("SENS" + channelNumber.ToString() + ":SEGM:LIST?").Replace("\n", "").Trim();
            string[] tmpSegData = tmpStr.Split(',');
            
            segmentTable.Segm = SegCount;
            SegDataCount = tmpSegData.Length / SegCount;

            i = 0;
            for (int iSeg = 0; iSeg < SegCount; iSeg++)
            {
                segmentTable.SegmentData[iSeg].Points = int.Parse(double.Parse(tmpSegData[i+1]).ToString());
                segmentTable.SegmentData[iSeg].Start = double.Parse(tmpSegData[i + 2]);
                segmentTable.SegmentData[iSeg].Stop = double.Parse(tmpSegData[i + 3]);
                segmentTable.SegmentData[iSeg].IfbwValue = double.Parse(tmpSegData[i + 4]);
                segmentTable.SegmentData[iSeg].DelValue = double.Parse(tmpSegData[i + 5]);
                segmentTable.SegmentData[iSeg].PowValue = double.Parse(tmpSegData[i + 6]);
                i += SegDataCount;
            }
            SendCommand("FORM:DATA REAL, 64");
        }

        public override void SetupNFChannel(int channelNumber, STraceMatching TraceMatching)
        {
            double ScrPwr = -20, RcvPwr = -20;
            double SwpTime = TraceMatching.NFSwpTime[0] * 10;

            string ScrPort = TraceMatching.NFSrcPort[0].ToString();
            string RcvPort = TraceMatching.NFRcvPort[0].ToString();


            SendCommand("SENS" + channelNumber + ":NOIS:PMAP " + ScrPort + "," + RcvPort);
            SendCommand("SOUR" + channelNumber + ":POW" + ScrPort + " " + ScrPwr);
            SendCommand("SOUR" + channelNumber + ":POW" + RcvPort + " " + RcvPwr);
            SendCommand("SOUR" + channelNumber + ":POW" + ScrPort + ":ALC:MODE OPEN");
            SendCommand("SOUR" + channelNumber + ":POW" + RcvPort + ":ALC:MODE OPEN");
            SendCommand("SOUR" + channelNumber + ":POW" + ScrPort + ":ATT:REC:REF 0");
            SendCommand("SOUR" + channelNumber + ":POW" + ScrPort + ":ATT:REC:TEST 0");
            SendCommand("SOUR" + channelNumber + ":POW" + RcvPort + ":ATT:REC:REF 0");
            SendCommand("SOUR" + channelNumber + ":POW" + RcvPort + ":ATT:REC:TEST 0");

            SendCommand("SENS" + channelNumber + ":NOIS:REC NOIS");
            SendCommand("SENS" + channelNumber + ":NOIS:BWID:RES 4e6");
            SendCommand("SENS" + channelNumber + ":NOIS:AVER:COUN " + SwpTime);
            SendCommand("SENS" + channelNumber + ":NOIS:AVER:STAT 1");
        }

        internal override void InsertTrace(int channelNumber, SortedList<int, string> Traces, SortedList<int, string> Balun, SortedList<int, int> TopazWind)
        {
            const string GD = "GD";

            int PreWind = -1;
            int NxtTr = -1;
            int CurrChan = -1;


 
            
            base.Operation_Complete();
            foreach (KeyValuePair<int, string> trace in Traces)
            {
                string paraDef = trace.Value.Replace("GD", "").ToLower();
                paraDef = "S" + paraDef.Remove(0, 1);

                //cuurent method
                ////string CurrWindNo = ReadCommand("DISP:CAT?").Replace("\"", "");
                ////if (CurrWindNo.Replace("\n","") != TopazWind[trace.Key].ToString())
                //    SendCommand("DISP:WIND" + TopazWind[trace.Key] + ":STATE ON");
                //if (channelNumber != CurrChan)
                //{
                //    SendCommand("CALC" + channelNumber + ":PAR:COUN " + Traces.Count);
                //    CurrChan = channelNumber;
                //}
                //Oon method
                if (trace.Key != 1)
                {
                    SendCommand("DISP:WIND" + TopazWind[trace.Key] + ":STATE ON");
                    if (_NFChn)
                        SendCommand("CALC" + channelNumber + ":CUST:DEF 'TR" + trace.Key + "','Noise Figure Cold Source','" + paraDef + "'");
                    else
                        SendCommand("CALC" + channelNumber + ":CUST:DEF 'TR" + trace.Key + "','STANDARD','" + paraDef + "'");

                    NxtTr = Convert.ToUInt16(ReadCommand("DISP:WIND" + TopazWind[trace.Key] + ":TRAC:NEXT?"));

                    SendCommand("DISP:WIND" + TopazWind[trace.Key] + ":TRAC" + NxtTr + ":FEED TR" + trace.Key);

                    SendCommand("CALC" + channelNumber + ":PAR:SEL TR" + trace.Key + ",fast");

                }
                //if (Balun[trace.Key].ToUpper() != naEnum.EBalunTopology.NONE.ToString().ToUpper())
                //    SendCommand("CALC" + channelNumber + ":FSIM:BAL:DEV " + Balun[trace.Key].ToString());


                //SendCommand("CALC" + channelNumber + ":MEAS" + trace.Key + ":PAR '" + paraDef + "'");

                base.Operation_Complete();
                if (trace.Value.Contains(GD))
                    SendCommand("CALC" + channelNumber + ":MEAS" + trace.Key + ":FORM " + naEnum.ESFormat.GDEL.ToString());
                else 
                    SendCommand("CALC" + channelNumber + ":MEAS" + trace.Key + ":FORM " + naEnum.ESFormat.SMIT.ToString());
                base.Operation_Complete();
            }
        }
        internal override void InsertCalKitStd(SCalStdTable stdTable)
        {
            int channelNo = stdTable.channelNo;
            int stdNo;
            int nextKitNo;
            string ret;
            string[] Std2Cls = new string[8]; //refer TCF for class number

            #region Create Kit
            //CREATE KIT
            SendCommand("SENS:CORR:CKIT:CLE '" + stdTable.calkit_label + "'");
            int.TryParse(ReadCommand("SENS:CORR:CKIT:COUN?"), out nextKitNo);
            Operation_Complete();
            nextKitNo++;
            SendCommand("SENS:CORR:COLL:CKIT " + nextKitNo);
            SendCommand("SENS:CORR:COLL:CKIT:NAME '" + stdTable.calkit_label + "'");
            SendCommand("SENS:CORR:COLL:CKIT:DESC '" + stdTable.calkit_label + "'");

            //ADD CONNECTOR TYPE
            SendCommand("SENS:CORR:COLL:CKIT:CONN:DEL");
            Operation_Complete();
            for (int iConn = 1; iConn < stdTable.Total_Port + 1; iConn++)
            {
                SendCommand("SENS:CORR:COLL:CKIT:CONN:ADD 'P" + iConn + "',0HZ,999GHZ,50.0,NONE,COAX,0.0");
                Operation_Complete();
            }
            #endregion

            #region Create Std
            //CREATE STD
            int StdCount = 1;
            int LoopCount;

            for (int i = 0; i < stdTable.CalStdData.Length; i++)
            {
                if (stdTable.CalStdData[i].Port1 == -1)
                    LoopCount = stdTable.Total_Port;
                else
                    LoopCount = 1;

                for (int iLoop = 1; iLoop < LoopCount + 1; iLoop++)
                {
                    SendCommand("SENS:CORR:COLL:CKIT:STAN " + (StdCount));
                    if (stdTable.CalStdData[i].StdType == naEnum.ECalibrationStandard.UTHRU)
                        SendCommand("SENS:CORR:COLL:CKIT:STAN:TYPE THRU");
                    else
                        SendCommand("SENS:CORR:COLL:CKIT:STAN:TYPE " + stdTable.CalStdData[i].StdType);

                    SendCommand("SENS:CORR:COLL:CKIT:STAN:CHAR COAX");
                    Operation_Complete();
                    SendCommand("SENS:CORR:COLL:CKIT:STAN:LABEL '" + stdTable.CalStdData[i].StdLabel + "'");
                    Operation_Complete();
                    SendCommand("SENS:CORR:COLL:CKIT:STAN:FMIN 0 Hz");
                    Operation_Complete();
                    SendCommand("SENS:CORR:COLL:CKIT:STAN:FMAX 999 GHz");
                    Operation_Complete();

                    SendCommand("SENS:CORR:COLL:CKIT:STAN:LOSS " + stdTable.CalStdData[i].OffsetLoss);
                    Operation_Complete();
                    SendCommand("SENS:CORR:COLL:CKIT:STAN:DELAY " + stdTable.CalStdData[i].OffsetDelay);
                    Operation_Complete();
                    SendCommand("SENS:CORR:COLL:CKIT:STAN:IMP " + stdTable.CalStdData[i].OffsetZ0);
                    Operation_Complete();

                    if (stdTable.CalStdData[i].StdType == naEnum.ECalibrationStandard.OPEN |
                        stdTable.CalStdData[i].StdType == naEnum.ECalibrationStandard.SHORT )
                    {
                        string cl = "";
                        if (stdTable.CalStdData[i].StdType == naEnum.ECalibrationStandard.OPEN) cl = "C";
                        if (stdTable.CalStdData[i].StdType == naEnum.ECalibrationStandard.SHORT) cl = "L";

                        SendCommand("SENS:CORR:COLL:CKIT:STAN:" + cl + "0 " + stdTable.CalStdData[i].C0_L0);
                        Operation_Complete();
                        SendCommand("SENS:CORR:COLL:CKIT:STAN:" + cl + "1 " + stdTable.CalStdData[i].C1_L1);
                        Operation_Complete();
                        SendCommand("SENS:CORR:COLL:CKIT:STAN:" + cl + "2 " + stdTable.CalStdData[i].C2_L2);
                        Operation_Complete();
                        SendCommand("SENS:CORR:COLL:CKIT:STAN:" + cl + "3 " + stdTable.CalStdData[i].C3_L3);
                        Operation_Complete();
                    }

                    Operation_Complete();
                    //Add Cls list
                    // pass in std no and cts type
                    AddCalOrder(StdCount, stdTable.CalStdData[i].StdType, ref Std2Cls);
                    StdCount++;
                }
            }
            #endregion

            #region Define Std
            //DEFINE STD
            StdCount = 1;
            for (int i = 0; i < stdTable.CalStdData.Length; i++)
            {
                if (stdTable.CalStdData[i].Port1 == -1)
                    LoopCount = stdTable.Total_Port;
                else
                    LoopCount = 1;

                for (int iLoop = 1; iLoop < LoopCount + 1; iLoop++)
                {
                    SendCommand("SENS:CORR:COLL:CKIT:STAN " + (StdCount));
                    Operation_Complete();

                    if (stdTable.CalStdData[i].Port1 == -1 && stdTable.CalStdData[i].Port2 == -1)
                            SendCommand("SENS:CORR:COLL:CKIT:CONN:SNAM 'P" + (iLoop) + "',NONE,1");
                    else
                        SendCommand("SENS:CORR:COLL:CKIT:CONN:SNAM 'P" + stdTable.CalStdData[i].Port1 + "',NONE,1");

                    if (stdTable.CalStdData[i].Port2 != -1)
                        SendCommand("SENS:CORR:COLL:CKIT:CONN:SNAM 'P" + stdTable.CalStdData[i].Port2 + "',NONE,2");
                    Operation_Complete();
                    StdCount++;
                }
            }
            #endregion

            #region Define Cal Class
            const int TotalCalcls = 8;
            //not yet complete, pending keysight replay on unknow thru
            for (int i = 0; i< TotalCalcls;i++)
            {
                if (!string.IsNullOrEmpty(Std2Cls[i]))
                {
                    SendCommand("SENS:CORR:COLL:CKIT:order" + (i+1) + " " + Std2Cls[i].TrimEnd (','));
//                    SendCommand("SENS:CORR:COLL:CKIT:olabel" + (i+1) + " " + Std2Cls[i].TrimEnd(','));
                }
            }

            #endregion

        }
        internal override void SetCalKitLabel(int channelNumber, string label)
        {
            string kitNo = "1";
            string tmp;
            
           tmp = ReadCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:CKIT:CAT?").Replace("\"", "").Replace("\n", "").Trim(); ;
            Operation_Complete();
            string [] KitList = tmp.Split(',');
            for(int i = 0; i < KitList.Count(); i++)
            {
                if (KitList[i].ToUpper().Trim() == label.ToUpper())
                {
                    kitNo = (i + 1).ToString();
                    i = KitList.Count();
                }
            }
            SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:CKIT " + kitNo);
            CalKitName = label;
        }
        public override void SetCorrMethod(int channelNumber, List<int> numberPorts, List<string> ConType)
        {
            //Must call SetCalkitLabel first
            string ChNo = channelNumber.ToString();

            string msg = String.Format("Topaz.SetCorrMethod\t Channel {0}, Num of port count:{1}, ConType count:{2}",
                ChNo, numberPorts.Count, ConType.Count);
            MPAD_TestTimer.LoggingManager.Instance.LogInfoTestPlan(msg);
            for (int i = 0; i < ConType.Count; i++)
            {
                SendCommand("SENS" + ChNo + ":CORR:COLL:GUID:CONN:PORT" + numberPorts [i] + " '" + ConType[i] + "'");
                SendCommand("SENS" + ChNo + ":CORR:COLL:GUID:CKIT:PORT" + numberPorts[i] + " '" + CalKitName + "'");
            }

            //foreach (string t in ConType)
            //{
            //    SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:CONN:PORT" + t + " 'P" + t +"'");
            //    SendCommand ("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + t + " '" + CalKitName + "'");
            //}
            if (numberPorts.Count > 1)
            {
                string portString = "";
                for (int i = 0; i < numberPorts.Count; i++)
                {
                    for (int j = i + 1; j < numberPorts.Count; j++)
                    {
                        portString += numberPorts[i] + "," + numberPorts[j] + ",";
                    }
                }

                portString += portString.TrimEnd(',');

                SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + portString);

                portString = "";
                for (int i = 0; i < numberPorts.Count; i++)
                {
                    for (int j = i + 1; j < numberPorts.Count; j++)
                    {
                        portString = numberPorts[i] + "," + numberPorts[j] + ",";
                        SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:PATH:TMET " + portString + " 'Undefined Thru'");
                    }
                }
            }
            SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:INIT");
            SendCommand("SENS" + channelNumber.ToString() + ":SWE:SPE FAST");
        }
        public override void MeasCorr1PortStd(int channelNumber, int portNumber, int stdNumber, string stdType)
        {
            SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:ACQ STAN" + stdNumber);
        }
        public override void MeasCorr2PortStd(int channelNumber, int port1, int port2, int stdNumber, string stdType)
        {
            SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:ACQ STAN" + stdNumber);
        }
        internal override void InitNFCalChannel(int ChNo, int srcPortNum, int rcvPortNum,string ConnType, string KitName, string EnrFile)
        {
            SendCommand("SENS" + ChNo + ":NOIS:PMAP " + srcPortNum + "," + rcvPortNum);
            Operation_Complete();
            ActiveChannel(ChNo);
            Operation_Complete();
            AveragingFactor(ChNo, 20);
            Operation_Complete();
            Averaging(ChNo, naEnum.EOnOff.On);
            Operation_Complete();
            SendCommand("SENS" + ChNo + ":NOIS:SOUR:CONN '" + ConnType +"'");
            Operation_Complete();
            SendCommand("SENS" + ChNo + ":NOIS:SOUR:CKIT '" + KitName + "'");
            Operation_Complete();
            SendCommand("SENS" + ChNo + ":NOIS:ENR:FILENAME '" + EnrFile + "'");
            Operation_Complete();
            SendCommand("SENS" + ChNo + ":NOIS:CAL:METHOD 'SCALAR'");
            Operation_Complete();
            SendCommand("SENS" + ChNo + ":NOIS:CAL:RMET 'NoiseSource'");
            Operation_Complete();
        }
        public override void SetCorrProperty(int channelNumber, bool enable)
        {
            if (enable)
                SendCommand(":SENS" + channelNumber.ToString() + ":CORR ON" );
            else
                SendCommand(":SENS" + channelNumber.ToString() + ":CORR OFF");

        }

        public override void SaveCorr(int channelNumber)
        {
            SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:GUID:SAVE");
        }

        internal override void InsertSegmentTableData(int channelNumber, SSegmentTable segmentTable, bool UseCalPow = false)
        {
            String tmpStr = "";
            SendCommand("FORM:DATA ASC");
            _NFChn = segmentTable.NFChannel;

            for (int i = 0; i < segmentTable.SegmentData.Length;i++)
            {
                tmpStr += "1,";
                tmpStr += segmentTable.SegmentData[i].Points + ",";
                tmpStr += segmentTable.SegmentData[i].Start + ",";
                tmpStr += segmentTable.SegmentData[i].Stop + ",";
                if (UseCalPow)
                    tmpStr += segmentTable.SegmentData[i].CalIfbwValue + ",";
                else
                    tmpStr += segmentTable.SegmentData[i].IfbwValue + ",";

                tmpStr += segmentTable.SegmentData[i].DelValue + ",";

                //for (int iPow = 0; iPow < segmentTable.SegmentData[i].PowList.Length; iPow++)
                //{
                    if (UseCalPow)
                        tmpStr += segmentTable.SegmentData[i].CalPowValue + ",";
                    else
                        //tmpStr += segmentTable.SegmentData[i].PowList[iPow] + ",";
                        tmpStr += segmentTable.SegmentData[i].PowValue + ",";
                //}
            }

            tmpStr = tmpStr.Remove(tmpStr.Length - 1, 1);
            if (!_NFChn)
            {
                SendCommand("SENS" + channelNumber.ToString() + ":SEGM:BWID:CONT " + segmentTable.Ifbw);
                base.Operation_Complete();
                SendCommand("SENS" + channelNumber.ToString() + ":SEGM:SWE:DWEL:CONT " + segmentTable.Del);
                base.Operation_Complete();
                SendCommand("SENS" + channelNumber.ToString() + ":SEGM:POW:CONT " + segmentTable.Pow);
                base.Operation_Complete();
            }
            SendCommand("SENS" + channelNumber.ToString() + ":SEGM:LIST SSTOP," + segmentTable.Segm + "," + tmpStr);

            if (!_NFChn)
            {
                SendCommand("SENS" + channelNumber.ToString() + ":SEGM:POW:CONT ON");
                SendCommand("SOUR" + channelNumber.ToString() + ":POW:COUP OFF");
                for (int i = 0; i < segmentTable.SegmentData.Length; i++)
                {
                    for (int iPow = 0; iPow < segmentTable.SegmentData[i].PowList.Length; iPow++)
                    {
                        if (UseCalPow)
                            SendCommand("SENS" + channelNumber.ToString() + ":SEGM" + (i + 1) +
                            ":POW" + (iPow + 1) + " " + segmentTable.SegmentData[i].CalPowValue);
                        else
                            SendCommand("SENS" + channelNumber.ToString() + ":SEGM" + (i + 1) +
                            ":POW" + (iPow + 1) + " " + segmentTable.SegmentData[i].PowList[iPow]);

                    }
                }
            }
                SendCommand("FORM:DATA REAL, 64");
            //SendCommand("SENS" + channelNumber.ToString() + ":SWE:SPE FAST");
            System.Threading.Thread.Sleep(300);
            base.Operation_Complete();
            
        }

        internal override string GetECalKitLabel()
        {
            string eCalKit = "";
            string[] listnum = ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?").Split(',');
            if (int.Parse(listnum[0]) > 0)
            {
                string[] EcalList = new string[listnum.Length];

                for (int i = 0; i < listnum.Length; ++i)
                {
                    listnum[i] = int.Parse(listnum[i]).ToString();
                    string[] EcalInfo = ReadCommand("SENS:CORR:CKIT:ECAL" + listnum[i] + ":INF?").Split(',', ':');
                    EcalList[i] = EcalInfo[1].Trim() + " ECal " + EcalInfo[3].Trim();
                    // Example of return value: 
                    // "ModelNumber: N4431B, SerialNumber: 03605, ConnectorType: 35F 35F 35F 35F, PortAConnector: APC 3.5 female, PortBConnector: APC 3.5 female, PortCConnector: APC 3.5 female, PortDConnector: APC 3.5 female, MinFreq: 9000, MaxFreq: 13510000000, NumberOfPoints: 336, Calibrated: 19/Dec/2007, CharacterizedBy: 0000099999, NetworkAnalyzer: US44240045"
                    eCalKit = EcalList[0];
                }
            }
            else
            {
                //If Ecalkit not existed, message will be shown   
            }
            return eCalKit;
        }

        private void AddCalOrder(int stdNo, naEnum.ECalibrationStandard stdType, ref string[] std2cls)
        {
            switch(stdType)
            {
                case naEnum.ECalibrationStandard.OPEN:
                    if (string.IsNullOrEmpty(std2cls[0])) std2cls[0] = "";
                    if (string.IsNullOrEmpty(std2cls[4])) std2cls[4] = "";
                    std2cls[0] = std2cls[0]  + stdNo + ",";
                    std2cls[4] = std2cls[4]  +  stdNo + ",";
                    break;
                case naEnum.ECalibrationStandard.SHORT:
                    if (string.IsNullOrEmpty(std2cls[1])) std2cls[1] = "";
                    if (string.IsNullOrEmpty(std2cls[5])) std2cls[5] = "";
                    std2cls[1] = std2cls[1] +  stdNo + ",";
                    std2cls[5] = std2cls[5]  +  stdNo + ",";
                    break;
                case naEnum.ECalibrationStandard.LOAD:
                    if (string.IsNullOrEmpty(std2cls[2])) std2cls[2] = "";
                    if (string.IsNullOrEmpty(std2cls[6])) std2cls[6] = "";
                    std2cls[2] = std2cls[2]  +  stdNo + ",";
                    std2cls[6] = std2cls[6] +  stdNo + ",";
                    break;
                case naEnum.ECalibrationStandard.THRU:
                    if (string.IsNullOrEmpty(std2cls[3])) std2cls[3] = "";
                    if (string.IsNullOrEmpty(std2cls[8])) std2cls[8] = "";
                    std2cls[3] = std2cls[3] +  stdNo + ",";
                    std2cls[8] = std2cls[8]  +  stdNo + ",";
                    break;
                default:
                    break;
            }
        }
    }
}
