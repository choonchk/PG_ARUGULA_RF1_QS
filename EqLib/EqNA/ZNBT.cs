using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa.Interop;

namespace EqLib.NA
{
    public class Znbt:NetworkAnalyzerAbstract
    {
        readonly int _intTimeOut = 10000;
        public Znbt(string address)
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
                DisplayError("ZNBT address mismatch -" + address, ex);
            }
        }

        public override void LoadState(string filename)
        {
            SendCommand("FORM REAL, 64");
            SendCommand("FORM:BORD NORM");
            filename = filename + ".znx";
            filename = String.Format("{0}{1}", "C:\\Users\\Public\\Documents\\Rohde-Schwarz\\Vna\\RecallSets\\", filename);
            SendCommand("MMEM:LOAD:STAT 1,\"" + filename.Trim() + "\"");
            base.Operation_Complete();
        }
        public override void SaveState(string filename)
        {
            SendCommand("FORM REAL, 64");
            SendCommand("FORM:BORD NORM");
            filename = filename + ".znx";
            filename = String.Format("{0}{1}", "C:\\Users\\Public\\Documents\\Rohde-Schwarz\\Vna\\RecallSets\\", filename);
            SendCommand("MMEM:STOR:STAT 1,\"" + filename.Trim() + "\"");
            base.Operation_Complete();
        }
        public override void DisplayOn(bool state)
        {
            switch (state)
            {
                case true:
                    SendCommand(":SYST:DISP:UPD ON");
                    break;
                case false:
                    SendCommand(":SYST:DISP:UPD OFF");
                    break;
            }
        }

        public override void Averaging(int channelNumber, naEnum.EOnOff val)
        {
            SendCommand("SENS" + channelNumber + ":AVER:STAT " + val);
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
            SendCommand("CONF:CHAN" + channelNumber.ToString() + ":STAT ON");
            SendCommand("INST:NSEL " + channelNumber.ToString());
        }

        public override void SetCorrProperty(int channelNumber, bool enable)
        {
            switch (enable)
            {
                case true:
                    SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:ACQ:RSAV:DEF ON");
                    break;
                case false:
                    SendCommand("SENS" + channelNumber.ToString() + ":CORR:COLL:ACQ:RSAV:DEF OFF");
                    break;
            }
        }
        public override void SetCorrMethod(int channelNumber, List<int> numberPorts, List<string> ConType)
        {
            string portStr = "";
            string type = "UOSM";

            for (int i = 0; i < numberPorts.Count; i++)
            {
                portStr = string.Format("{0}, {1}", portStr, numberPorts[i]);
            }

            SendCommand("SENS" + channelNumber + ":CORR:COLL:METH:DEF 'SOLT" + numberPorts.Count.ToString() + "', " + type + " " + portStr);
        }
        public override void MeasCorr1PortStd(int channelNumber, int portNumber, int stdNumber, string stdType)
        {
            if (stdType == "LOAD") stdType = "MATCH";
            SendCommand(":SENS" + channelNumber + ":CORR:COLL:ACQ:SEL " + stdType + ", " + portNumber);
        }
        public override void MeasCorr2PortStd(int channelNumber, int port1, int port2, int stdNumber, string stdType)
        {
            if (stdType == "THRU") stdType = "THROUGH";
            SendCommand(":SENS" + channelNumber + ":CORR:COLL:ACQ:SEL " + stdType + ", " + port1 + "," + port2);
        }
        public override void SaveCorr(int channelNumber)
        {
            SendCommand("SENS" + channelNumber + ":CORR:COLL:SAVE:SEL");
        }

        public override void TriggerMode(naEnum.ETriggerMode trigMode)
        {
            if (trigMode == naEnum.ETriggerMode.Cont)
            {
                SendCommand("INIT:CONT:ALL");
                SendCommand("INIT:IMM:ALL");
            }
        }

        public override void TriggerSingle(int channelNumber)
        {
            ActiveChannel(channelNumber);
            SendCommand("INIT" + channelNumber.ToString() + ":CONT OFF");
            SendCommand("INIT" + channelNumber.ToString());
            SendCommand("INIT" + channelNumber.ToString() + ":IMM");
            base.Operation_Complete();
        }

        public override void SetPower(float level)
        {
            SendCommand("SOUR:POW " + level.ToString());
        }

        public override string GetTraceInfo(int channelNumber)
        {
            return ReadCommand("CALC" + channelNumber.ToString() + ":PAR:CAT?");
        }
        public override naEnum.ESFormat GetTraceFormat(int channelNumber, int traceNumber)
        {
            SendCommand("CALC" + channelNumber.ToString() + ":PAR:SEL 'Trc" + traceNumber.ToString() + "'");
            string tmp = ReadCommand("CALC" + channelNumber.ToString() + ":FORM?");
            tmp = tmp.Replace("'", "").Replace("\n", "").Trim();
            naEnum.ESFormat format = (naEnum.ESFormat)Enum.Parse(typeof(naEnum.ESFormat), tmp);
            return format;
        }
        public override double[] GetFreqList(int channelNumber)
        {  
            return (ReadIeeeBlock("CALC" + channelNumber.ToString() + ":DATA:STIM?"));
        }

        public override double[] GrabRealImagData(int channelNumber)
        {
            return ReadIeeeBlock("CALC" + channelNumber.ToString() + ":DATA:CHAN:ALL? MDATA");
        }

        public override void GetSegmentTable(out SSegmentTable segmentTable, int channelNumber)
        {
            int count;
            string DataFormat;
            segmentTable = new SSegmentTable();

            try
            {

                SendCommand("FORM:DATA ASC");
                count = int.Parse(ReadCommand("SENS" + channelNumber + ":SEGM:COUN?"));
                segmentTable.Mode = naEnum.EModeSetting.StartStop;

                segmentTable.Ifbw = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), ReadCommand(String.Format("SENS{0}:SEGM:BWID:CONT?", channelNumber)));
                segmentTable.Pow = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), ReadCommand(String.Format("SENS{0}:SEGM:POW:CONT?", channelNumber)));
                segmentTable.Del = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), ReadCommand(String.Format("SENS{0}:SEGM:SWE:DWEL:CONT?", channelNumber)));
                segmentTable.Time = (naEnum.EOnOff)Enum.Parse(typeof(naEnum.EOnOff), ReadCommand(String.Format("SENS{0}:SEGM:SWE:TIME:CONT?", channelNumber)));

                segmentTable.Segm = count;
                segmentTable.SegmentData = new SSegmentData[segmentTable.Segm];
                for (int i = 0; i < count; i++)
                {
                    segmentTable.SegmentData[i].Start = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:FREQ:STAR?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].Stop = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:FREQ:STOP?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].Points = int.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:POIN?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].IfbwValue = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:BWID?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].PowValue = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:POW?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].TimeValue = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:TIME?", channelNumber.ToString(), (i + 1).ToString())));
                    segmentTable.SegmentData[i].DelValue = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:DWEL?", channelNumber.ToString(), (i + 1).ToString())));
                }

                SendCommand("FORM REAL, 64");
                SendCommand("FORM:BORD NORM");
            }
            catch(Exception ex)
            {
                DisplayError("Mistake in Segment Table", ex);
            }
        }

        internal override void InsertTrace(int channelNumber, SortedList<int, string> Traces)
        {
            string read;
            foreach (KeyValuePair<int, string> trace in Traces)
            {
                read = ReadCommand("CALC" + channelNumber + ":PAR:CAT?");
                if (read.Contains("Trc" + trace.Key))
                {
                    SendCommand("CONF:CHAN" + channelNumber + ":TRAC:REN " + "'CH" + channelNumber + "Trc" + trace.Key + "'");
                    SendCommand("CALC" + channelNumber + ":PAR:MEAS 'CH" + channelNumber + "Trc" + trace.Key  + "','" + trace.Value  + "'");
                    SendCommand("CALC" + channelNumber + ":FORM " + naEnum.ESFormat.SMIT.ToString());
                }
                else
                {
                    SendCommand("DISPlay:WINDow" + channelNumber + ":STATe ON");
                    SendCommand("CALC" + channelNumber + ":PAR:SDEF 'CH" + channelNumber + "Trc" + trace.Key + "','" + trace.Value + "'");
                    SendCommand("DISP:WIND" + channelNumber + ":TRAC" + trace.Key + ":FEED 'CH" + channelNumber + "Trc" + trace.Key + "'");
                    SendCommand("CALC" + channelNumber + ":PAR:SEL CH" + channelNumber +"Trc" + trace.Key);
                    SendCommand("CALC" + channelNumber + ":FORM " + naEnum.ESFormat.SMIT.ToString());        
                }
                base.Operation_Complete();
            }
            
        }

        internal override void InsertCalKitStd(SCalStdTable stdTable)
        {
            SetCalKitLabel(stdTable.channelNo, stdTable.calkit_label);
   
            AveragingMode(stdTable.channelNo, stdTable.Avg_Mode);
            AveragingFactor(stdTable.channelNo, stdTable.Avg_Factor);

            for (int i = 0; i < stdTable.CalStdData.Length; i++)
            {
                switch (stdTable.CalStdData[i].StdType)
                {
                    case naEnum.ECalibrationStandard.OPEN: // "OPEN":
                        //tempStandard = "OPEN";
                        WriteSingleStandard(stdTable, i, "FOPEN");
                        WriteSingleStandard(stdTable, i, "MOPEN");
                        break;
                    case naEnum.ECalibrationStandard.SHORT: //"SHORT":
                        //tempStandard = "SHOR";
                        WriteSingleStandard(stdTable, i, "FSHORT");
                        WriteSingleStandard(stdTable, i, "MSHORT");
                        break;
                    case naEnum.ECalibrationStandard.LOAD: //"LOAD":
                        //tempStandard = "MATC";
                        WriteSingleStandard(stdTable, i, "MMTCh");
                        WriteSingleStandard(stdTable, i, "FMTCh");
                        break;
                    case naEnum.ECalibrationStandard.THRU: //"THRU":
                        WriteSingleStandard(stdTable, i, "MMTHrough");
                        WriteSingleStandard(stdTable, i, "MFTHrough");
                        WriteSingleStandard(stdTable, i, "FFTHrough");
                        break;
                }
                base.Operation_Complete();
            }
        }
        internal override void SetCalKitLabel(int channelNumber, string label)
        {
            SendCommand("SENS" + channelNumber + ":CORR:COLL:CONN:PORT ALL");
            SendCommand("SENS" + channelNumber + ":CORR:COLL:SCON 'Package'");
            SendCommand("SENS" + channelNumber + ":CORRection:CKIT:DMODe 'Package', '" + label + "', '', DEL");

            SendCommand("SENS" + channelNumber + ":CORR:CKIT:SEL 'Package'," + "'" + label + "'");
        }
        internal override void InsertSegmentTableData(int channelNumber, SSegmentTable segmentTable, bool UseCalPow = false)
        {
            //Delete all segment first
            SendCommand("SENS" + channelNumber.ToString() + ":SEGM:DEL:ALL");
            for (int Seg = segmentTable.SegmentData.Length - 1; Seg >= 0; Seg--)
            {
                //[SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>,
                //<SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>,
                //<Selectivity>]
                SendCommand(String.Format("SENS{0}:SEGM:INS {1}, {2}, {3}, {4}, {5}, 0, {6}",
                    channelNumber.ToString(), segmentTable.SegmentData[Seg].Start.ToString(),
                    segmentTable.SegmentData[Seg].Stop.ToString(), segmentTable.SegmentData[Seg].Points.ToString(),
                    segmentTable.SegmentData[Seg].PowValue.ToString(), segmentTable.SegmentData[Seg].TimeValue.ToString(),
                    segmentTable.SegmentData[Seg].IfbwValue.ToString()));
            }

            SendCommand(String.Format("SENS{0}:SEGM:POW:CONT {1}", channelNumber.ToString(), (segmentTable.Pow == naEnum.EOnOff.On ? "1" : "0")));

            SendCommand(String.Format("SENS{0}:SEGM:BWID:CONT {1}", channelNumber.ToString(), (segmentTable.Ifbw == naEnum.EOnOff.On ? "1" : "0")));

            SendCommand(String.Format("SENS{0}:SEGM:SWE:DWEL:CONT {1}", channelNumber.ToString(), (segmentTable.Del == naEnum.EOnOff.On ? "1" : "0")));

            SendCommand(String.Format("SENS{0}:SEGM:SWE:TIME:CONT {1}", channelNumber.ToString(), (segmentTable.Time == naEnum.EOnOff.On ? "1" : "0")));

            switch (segmentTable.Swp)
            {
                case naEnum.EOnOff.On:
                    SendCommand("SENS" + channelNumber.ToString() + ":SEGM:STAT ON");
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 6," + Convert_SegmentTable2String(SegmentData, sweepmode));
                    break;
                case  naEnum.EOnOff.Off:
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:STAT OFF");
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 5," + Convert_SegmentTable2String(SegmentData, sweepmode));
                    break;
            }
            base.Operation_Complete();
        }

        internal override string GetCalKitLabel(int channelNumber)
        {
            string label;
            label = ReadCommand(":SENS:CORR:COLL:CKIT:PORT?");
            string[] labelArr;
            try
            {
                labelArr = label.Split(',');
                return labelArr[1].Substring(1, labelArr[1].Length - 2);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }


        //private function section, unique to ZNBT
        private void WriteSingleStandard(SCalStdTable stdTable, int stdNumber, string rSStandard)
        {
            
            string cmd = "";
            cmd = cmd + ":SENS:CORR:CKIT:";
            cmd = cmd + rSStandard;
            cmd = cmd + " 'Package','" + stdTable.calkit_label; //calkitLabel;
            cmd = cmd + "','" + stdTable.CalStdData[stdNumber].StdLabel + "'"; //stdLabel
            cmd = cmd + "," + stdTable.CalStdData[stdNumber].MinFreq; //TEXT(FMIN.ToString());
            cmd = cmd + "," + stdTable.CalStdData[stdNumber].MaxFreq; //TEXT(FMAX.ToString());
            cmd = cmd + "," + stdTable.CalStdData[stdNumber].OffsetDelay; //TEXT(DEL.ToString());
            cmd = cmd + "," + stdTable.CalStdData[stdNumber].OffsetLoss / 1000000000; //TEXT(LOSS.ToString());

            cmd = cmd + "," + stdTable.CalStdData[stdNumber].OffsetZ0; //TEXT(Z0.ToString());
            if (!rSStandard.Contains("THrough"))
            {
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C0_L0; //TEXT(C0.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C1_L1; //TEXT(C1.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C2_L2; //TEXT(C2.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C3_L3; //TEXT(C3.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C0_L0; //TEXT(L0.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C1_L1; //TEXT(L1.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C2_L2; //TEXT(L2.ToString());
                cmd = cmd + "," + stdTable.CalStdData[stdNumber].C3_L3; //TEXT(L3.ToString());
                if (stdTable.CalStdData[stdNumber].Port1 != 0 & stdTable.CalStdData[stdNumber].Port1 != -1)
                {
                    cmd = cmd + "," + stdTable.CalStdData[stdNumber].StdType.ToString();//tempStandard
                    cmd = cmd + "," + stdTable.CalStdData[stdNumber].Port1; //TEXT(Port1.ToString());
                }
            }
            else
            {
                if (stdTable.CalStdData[stdNumber].Port1 != 0 && stdTable.CalStdData[stdNumber].Port2 != 0)
                {
                    cmd = cmd + "," + stdTable.CalStdData[stdNumber].Port1; //TEXT(Port1.ToString());
                    cmd = cmd + "," + stdTable.CalStdData[stdNumber].Port2; //TEXT(Port2.ToString());
                }
            }
            SendCommand(cmd);
            string rc = ReadCommand(":SYST:ERR?");
            rc = rc.ToUpper();
            if (!rc.Contains("NO ERROR"))
            {
                string s = cmd;
            }
        }
    }
}
