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

using ClothoLibAlgo;


namespace EqLib
{

    public partial class Eq_ENA
    {
        //Added by chee on 21-Feb-2018
        public struct S_SegmentTable
        {
            public e_ModeSetting Mode;
            public e_OnOff Ifbw;
            public e_OnOff Pow;
            public e_OnOff Del;
            public e_OnOff Swp;
            public e_OnOff Time;
            public int Segm;
            public s_SegmentData[] SegmentData;

        }

        public enum e_ModeSetting
        {
            StartStop = 0,
            CenterSpan
        }

        public enum e_OnOff
        {
            Off = 0,
            On
        }

        public enum e_SweepMode
        {
            Stepped = 0,
            Swept,
            FastStepped,
            FastSwept
        }

        public struct s_SegmentData
        {
            public double Start;
            public double Stop;
            public int Points;
            public double IfbwValue;
            public double PowValue;
            public double DelValue;
            public double pow_n1_value;
            public double pow_n2_value;
            public double pow_n3_value;
            public double pow_n4_value;
            public double pow_n5_value;
            public double pow_n6_value;
            public double pow_n7_value;
            public double pow_n8_value;
            public double pow_n9_value;
            public double pow_n10_value;
            public e_SweepMode SwpValue;
            public double TimeValue;
        }
        //Added by Chee On

        public class ENA_E5071C : NetAn_Base
        {
            public new FormattedIO488 NetAn;
            public new ManualResetEvent enaStateRecallFlag = new ManualResetEvent(false);

            public enum ENATrigSource
            {
                Internal, External, Manual, Bus

            }

            public enum ENANumSweepPoints
            {
                Points_201,
                Points_401,
                Points_801,
                Points_1601
            }

            public override void InitializeENA(string InstrAddress)
            {

                try
                {

                    NetAn = GrabIO488DeviceByAliasName(InstrAddress);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }

            }

            public override FormattedIO488 GrabIO488DeviceByAliasName(string name)
            {
                DialogResult resp = DialogResult.Retry;

                while (resp == DialogResult.Retry)
                {
                    try
                    {
                        FormattedIO488 meEquip = new FormattedIO488();
                        meEquip.IO = (IMessage)new Ivi.Visa.Interop.ResourceManager().Open(name, AccessMode.NO_LOCK, 3000, "");
                        return meEquip;
                    }
                    catch
                    {
                        resp = MessageBox.Show("Failed to open communication with " + name, "Instrument Communication Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                }

                return (null);
            }

            public override void Write(string _cmd)
            {
                try
                {
                    NetAn.WriteString(_cmd, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    MessageBox.Show("error writing to ENA\n\n" + e.ToString());
                }
            }

            public override string ReadString()
            {
                try
                {
                    return NetAn.ReadString();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    //MessageBox.Show("error reading from ENA\n\n" + e.ToString());
                    return "";
                }

            }

            public override void ReadData(int ChanNum, bool Synchronization)
            {
                //This is a dummy for the ENA because the ENA reads the data during ReadENATrace
            }

            public override string WriteRead(string _cmd)
            {
                try
                {
                    Write(_cmd);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }



                return ReadString();
            }

            public override double[] WriteReadIEEEBlock(string _cmd, IEEEBinaryType _type)
            {
                try
                {
                    Write(_cmd);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }
                return null;
                //return NetAn.ReadIEEEBlock(_type, true, true);
            }

            public override object Read_BiBlock()
            {
                try
                {
                    return NetAn.ReadIEEEBlock(IEEEBinaryType.BinaryType_UI1, true, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    return 0;
                }
            }

            public override void Write_IEEEBlock(string _cmd, object data)
            {
                try
                {
                    NetAn.WriteIEEEBlock(_cmd, data, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }



            }

            public override void SetTimeout(int tmout)
            {
                try
                {
                    NetAn.IO.Timeout = tmout;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }



            }

            public override string funcSerialPoll()
            {
                string returnValue = null;

                try
                {
                    while (true)
                    {
                        Write("*OPC?");

                        int bitAnd = 0;

                        //Write("*ESR?");
                        Thread.Sleep(1);
                        returnValue = ReadString();

                        if (returnValue.StartsWith("++"))
                        {
                            returnValue = returnValue.Substring(1);
                        }

                        bitAnd = Convert.ToInt16(returnValue) & 1;


                        if (bitAnd == 1)
                            break;

                        else if (Convert.ToInt16(returnValue) == 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        else
                        {

                            Thread.Sleep(1);
                        }


                    }


                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }

                return returnValue;


            }

            public override void check_the_ena_catalog_for_stat_file_existence(string vFilename)
            {

                try
                {
                    Write(":MMEM:CAT? \"D:\"");
                    string file_list = ReadString();
                    //zzzmyLibNa.CHECK_OPC();
                    //bool file_exists = file_list.Contains("SUDARSH.STA");
                    bool file_exists = file_list.Contains(vFilename.ToUpper());
                    if (!file_exists)
                    {
                        transfer_from_pc_to_ena(vFilename);
                    }

                    Write(":MMEM:LOAD \"D:\\" + vFilename + "\"");
                    //Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }




            }

            public override void transfer_from_pc_to_ena(string vFile2Xfer)
            {
                //
                //FileStream fs = new FileStream(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Pike\e5071b_state_file\sudarsh_pike_state01.sta", FileMode.Open );
                FileStream fs = new FileStream(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Pike\Pike_B7_OpFiles\E5071B_State_Files\" + vFile2Xfer, FileMode.Open);
                // Create the reader for data.
                BinaryReader w = new BinaryReader(fs);
                long length = w.BaseStream.Length;
                string length1 = Convert.ToString(length);
                int strlen_of_filesize = length1.Length;

                byte[] stadata = new byte[length];
                long pos = 0L;

                try
                {
                    while (pos < length)
                    {
                        stadata[pos] = w.ReadByte();
                        pos++;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    long newPOs = pos;
                }

                w.Close();
                //fs.Close();
                //string haha = ":MMEM:TRAN \"D:\\PikeB7.sta\",";  //#" + strlen_of_filesize.ToString() + length1;
                string haha = ":MMEM:TRAN \"D:\\" + vFile2Xfer + "\",";  //#" + strlen_of_filesize.ToString() + length1;

                Write_IEEEBlock(haha, stadata); //original cmd: ":MMEM:TRAN \"D:\\sudarsh.sta\",#6223986MSCF"
                                                //zzzmyLibNa.CHECK_OPC();

                //Thread.Sleep(1000);
            }

            public override void RecallENAState()
            {
                Write(":MMEM:LOAD \"" + statefile.Trim() + "\"");
                Thread.Sleep(8000);
                Write(":DISP:ENAB 0");   // 0 off, 1 on
                Write(":TRIG:SOUR BUS");
                enaStateRecallFlag.Set();
            }

            public override List<double> ReadFreqList_Chan(int channel)
            {
                try
                {
                    FreqList[channel] = ReadFreqList(channel);
                    return FreqList[channel];
                }
                catch (Exception e)
                {
                    List<double> freqs = new List<double>();
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    return freqs;
                }
            }

            public override List<double> ReadFreqList(int channel)
            {
                try
                {
                    Write(":FORM:BORD NORM; :FORM:DATA REAL");

                    List<double> freqs = WriteReadIEEEBlock(":SENS" + channel + ":FREQ:DATA?", IEEEBinaryType.BinaryType_R8).ToList();

                    for (int i = 0; i < freqs.Count(); i++) freqs[i] *= 1e-6;
                    return freqs;

                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                    return new List<double>();
                }
            }

            public override void SendTrigger(bool Sync, int Chan, int Trace)
            {
                Write("*CLS");
                Write(":TRIG:SEQ:SCOP ACT");
                Write(":DISP:WIND" + Chan + ":ACT");
                Write(":INIT" + Chan + ":CONT ON");
                Write(":TRIG:SOUR BUS");
                Write(":TRIG:SING;*OPC");
                funcSerialPoll();
            }


            public override string CheckTraceFormat(int channel, int tracenum)
            {
                string traceformat = "";

                Write(":CALC" + channel.ToString() + ":TRAC" + tracenum.ToString() + ":FORM?");
                return traceformat = ReadString();

            }

            public override double[] ReadENATrace(int channel, int trace_OneBased)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                    double[] fullDATA_X = WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:FDAT?", IEEEBinaryType.BinaryType_R8);

                    double[] data = new double[fullDATA_X.Length / 2];

                    for (int k = 0; k < data.Length; k++)
                    {
                        data[k] = fullDATA_X[2 * k];
                    }
                    return data;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    Write(":FORM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return new double[4];
                }
            }

            public override double[] ReadENATrace2(int channel, int trace_OneBased)
            {
                try
                {
                    Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                    double[] fullDATA_X = WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:SDAT?", IEEEBinaryType.BinaryType_R8);

                    //double[] data = new double[fullDATA_X.Length];

                    //for (int k = 0; k < data.Length; k++)
                    //{
                    //    data[k] = fullDATA_X[2 * k];
                    //}

                    return fullDATA_X;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    return new double[4];
                }
            }

            public override void defineTracex(int ChannelNum, int TraceNumber, string MeasType)
            {
                try
                {
                    Write(":DISP:WIND" + ChannelNum.ToString() + ":" + "TRAC" + TraceNumber.ToString() + ":STAT ON");
                    string h = ":CALC" + ChannelNum.ToString() + ":PAR" + TraceNumber.ToString() + ":DEF " + MeasType;
                    Write(h);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }
            }

            public override void ENARecallState(string nameoffile)
            {

                try
                {
                    Write(":MMEM:LOAD \"D:\\" + nameoffile + ".STA" + "\"");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }




            }

            //public void ENACaptureTrace(NetAnTest x, int ChanNo, int TraceNo)
            //{


            //    try
            //    {
            //        x.ReadFreqList(ChanNo);
            //        x.ReadENATrace();
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            //    }


            //}

            //public void defineTracex(int ChannelNum, int TraceNumber, string MeasType)
            //{
            //    try
            //    {
            //        ENA.Write(":DISP:WIND" + ChannelNum.ToString() + ":" + "TRAC" + TraceNumber.ToString() + ":STAT ON");
            //        string h = ":CALC" + ChannelNum.ToString() + ":PAR" + TraceNumber.ToString() + ":DEF " + MeasType;
            //        ENA.Write(h);

            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);

            //    }





            //}

            //public override void ENA_WriteDataToFile(string CalDir, string PortInfo, string BandInfo)
            //{

            //    try
            //    {
            //        StreamWriter CalFile = new StreamWriter(CalDir + "SMPAD_RevCEX_HarmonicPath_" + PortInfo + "_" + BandInfo + ".csv");

            //        int hello = 0;

            //        foreach (double val in NetAnTest.traceData[1, 1])
            //        {
            //            CalFile.Write(NetAnTest.freqList[1][hello] + "," + val + "\n");
            //            hello++;
            //        }

            //        CalFile.Close();

            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Exception during ENA_WriteDataToFile:" + Environment.NewLine + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);

            //    }

            //}

            public override void SetCalibrationType(int ChanNo, int numPorts, string PortDesignation)
            {
                try
                {
                    //should really qualify numports and ChanNo for valid numbers
                    Write(":SENS" + ChanNo + ":CORR:COLL:METH:SOLT" + numPorts + " " + PortDesignation.Trim()); //  " 1,2,3");
                    funcSerialPoll();
                }
                catch (Exception e)
                {


                }


            }

            public override void SelectCalKit(int ChanNo, int CalNum)
            {
                try
                {
                    Write(":SENSE" + ChanNo + ":CORR:COLL:CKIT:SEL " + CalNum);
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }


            }

            public override void CalibrateOpen(int ChanNo, int PortNum)
            {
                try
                {
                    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:OPEN " + PortNum);

                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }


            }

            public override void CalibrateShort(int ChanNo, int PortNum)
            {
                try
                {
                    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:SHOR " + PortNum);
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }


            }

            public override void CalibrateLoad(int ChanNo, int PortNum)
            {
                try
                {
                    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:LOAD " + PortNum);
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }


            }

            public override void SaveCalibration(string zfilenamewext, int sleepTime = 0)
            {
                try
                {
                    Write(":MMEM:STOR:STYP CDST"); //Save Type = All
                    if (sleepTime != 0) Thread.Sleep(sleepTime);
                    funcSerialPoll();
                    //ENA.Write(":MMEM:STORE \"D:\\SPYRO_FBAR_AUTO.STA" + "\""); //File used to initially save the cal results
                    Write(":MMEM:STORE \"D:\\" + zfilenamewext + "\"");
                    if (sleepTime != 0) Thread.Sleep(sleepTime);
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }
            }

            public override void SetFixtureSimState(int ChanNo, int SimState)
            {
                try
                {
                    Write(":CALC" + ChanNo.ToString() + ":FSIM:STAT " + SimState.ToString());
                    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:STAT " + SimState.ToString()); //Turn off Port ZConversion
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }
            }

            public override void SetPortImpedance(int ChanNo, double ImpReal, double ImpIMG, string PortNo)
            {
                try
                {
                    SetFixtureSimState(ChanNo, 1);
                    funcSerialPoll();
                    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNo + ":Z0 " + ImpReal.ToString()); //Set port impedance for Port 3 (Rx port) 
                    funcSerialPoll();
                    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNo + ":IMAG " + ImpIMG.ToString()); //Set port impedance for Port 3 (Rx port) 
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }
            }



            public override void SetTrigInternal()
            {
                try
                {
                    Write(":TRIG:SOUR INT");
                }
                catch (Exception e)
                {

                }
            }

            public override void SetTrigBus()
            {
                try
                {
                    Write(":TRIG:SOUR BUS");
                }
                catch (Exception e)
                {

                }
            }

            public override void SetActiveWindow(int zwindow)
            {
                try
                {
                    Write(":DISP:WIND" + zwindow.ToString().Trim() + ":ACT");
                    //funcSerialPoll();
                }
                catch (Exception e)
                {

                }
            }

            public override void MaximizeActiveWindow(bool maximize)
            {
                try
                {
                    Write(":DISP:MAX " + (maximize ? 1 : 0));
                }
                catch (Exception e)
                {

                }
            }

            public override void SetActiveTrace(int chanNum, int traceNum)
            {
                try
                {
                    Write(":CALC" + chanNum + ":PAR" + traceNum + ":SEL");
                }
                catch (Exception e)
                {

                }
            }

            public override void MaximizeActiveTrace(int NaChan, bool maximize)
            {
                try
                {
                    Write(":DISP:WIN" + NaChan + ":MAX " + (maximize ? 1 : 0));
                }
                catch (Exception e)
                {

                }
            }

            public override string GetTraceType(int chanNum, int traceNum)
            {
                Write(":CALC" + chanNum + ":PAR" + traceNum + ":DEF?");
                return ReadString().Trim();
            }

            public override int GetTraceCount(int chanNum)
            {
                Write(":CALC" + chanNum + ":PAR:COUN?");
                return Convert.ToInt32(ReadString().Trim());
            }

            public override void PerformEcal(int ChanNo, int numports, string Ports)
            {
                try
                {
                    Write(":SENS" + ChanNo + ":CORR:COLL:ECAL:SOLT" + numports + " " + Ports);
                }
                catch (Exception e)
                {

                }
            }

            public override void CalibrateThruWithSubclass(int ChanNo, int StimPortNum, int RespPortNum)
            {
                try
                {
                    Write(":SENS" + ChanNo + ":CORR:COLL:SUBC " + ChanNo);
                    funcSerialPoll();

                    Write(":SENS" + ChanNo + ":CORR:COLL:THRU " + StimPortNum + "," + RespPortNum);
                    funcSerialPoll();
                }
                catch (Exception e)
                {

                }


            }

            public override void CalcCalCoeffs(int ChanNo)
            {
                try
                {
                    //Calculate the cal coefficients for a given channel
                    //This command should only be used after all necessary combinations of port combinations have been done
                    //otherwise the command is ignored
                    Write(":SENS" + ChanNo + ":CORR:COLL:SAVE");
                    funcSerialPoll();
                }
                catch (Exception e)
                {


                }


            }


            public override void InsertSegmentTableData(int channelNumber, S_SegmentTable segmentTable)
            {

                Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");

                switch (segmentTable.Swp)
                {
                    case e_OnOff.On:
                        Eq.Site[0].EqNetAn.Write("SENS" + channelNumber.ToString() + ":SEGM:DATA 6," + Convert_SegmentTable2String(segmentTable));
                        break;
                    case e_OnOff.Off:
                        Eq.Site[0].EqNetAn.Write("SENS" + channelNumber.ToString() + ":SEGM:DATA 5," + Convert_SegmentTable2String(segmentTable));
                        break;
                }
                //funcSerialPoll();
                Eq.Site[0].EqNetAn.Write("OPC?");

                Eq.Site[0].EqNetAn.Write("FORM:DATA REAL");

                //base.Operation_Complete();
                //waitTimer.Wait(300);
            }

            private string Convert_SegmentTable2String(S_SegmentTable SegmentTable)
            {
                string tmpStr;
                tmpStr = "";
                e_OnOff sweepmode = SegmentTable.Swp;
                switch (sweepmode)
                {
                    case e_OnOff.On:
                        tmpStr = ((int)SegmentTable.Mode).ToString();
                        tmpStr += "," + ((int)SegmentTable.Ifbw).ToString();
                        tmpStr += "," + ((int)SegmentTable.Pow).ToString();
                        tmpStr += "," + ((int)SegmentTable.Del).ToString();
                        tmpStr += "," + ((int)SegmentTable.Swp).ToString();
                        tmpStr += "," + ((int)SegmentTable.Time).ToString();
                        tmpStr += "," + SegmentTable.Segm.ToString();
                        for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                        {
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                            if (SegmentTable.Ifbw == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].IfbwValue.ToString();
                            if (SegmentTable.Pow == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].PowValue.ToString();
                            if (SegmentTable.Del == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].DelValue.ToString();
                            if (SegmentTable.Swp == e_OnOff.On)
                                tmpStr += "," + ((int)SegmentTable.SegmentData[Seg].SwpValue).ToString(); //Stepped/0
                            if (SegmentTable.Time == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].TimeValue.ToString();
                        }
                        break;
                    case e_OnOff.Off:
                        tmpStr = ((int)SegmentTable.Mode).ToString();
                        tmpStr += "," + ((int)SegmentTable.Ifbw).ToString();
                        tmpStr += "," + ((int)SegmentTable.Pow).ToString();
                        tmpStr += "," + ((int)SegmentTable.Del).ToString();
                        tmpStr += "," + ((int)SegmentTable.Time).ToString();
                        tmpStr += "," + SegmentTable.Segm.ToString();
                        for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                        {
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                            if (SegmentTable.Ifbw == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].IfbwValue.ToString();
                            if (SegmentTable.Pow == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].PowValue.ToString();
                            if (SegmentTable.Del == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].DelValue.ToString();
                            if (SegmentTable.Time == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].TimeValue.ToString();
                        }
                        break;
                }
                return (tmpStr);
            }

            public override void GetSegmentTable(int channelNumber, out S_SegmentTable SegmentTable)
            {
                string DataFormat;
                string tmpStr;
                string[] tmpSegData;
                long tmpI;
                int iData = 3, iAdd = 0;

                SegmentTable = new S_SegmentTable();

                //segTable = new S_SegmentTable();
                DataFormat = Eq.Site[0].EqNetAn.ReadDataFormat("FORM:DATA?");
                if (DataFormat.ToUpper() == "REAL") Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");
                Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");
                tmpStr = Eq.Site[0].EqNetAn.ReadSegmentData(channelNumber);
                tmpSegData = tmpStr.Split(',');

                for (int s = 0; s < tmpSegData.Length; s++)
                {
                    tmpI = (long)(Convert.ToDouble(tmpSegData[s]));
                    tmpSegData[s] = tmpI.ToString();
                }

                SegmentTable.Mode = (e_ModeSetting)Enum.Parse(typeof(e_ModeSetting), tmpSegData[1]);
                SegmentTable.Ifbw = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[2]);
                SegmentTable.Pow = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[3]);
                SegmentTable.Del = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[4]);

                if (tmpSegData[0] == "5")
                {
                    SegmentTable.Swp = e_OnOff.Off;
                    SegmentTable.Time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                    SegmentTable.Segm = int.Parse(tmpSegData[6]);
                }
                else if (tmpSegData[0] == "6")
                {
                    SegmentTable.Swp = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                    SegmentTable.Time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[6]);
                    SegmentTable.Segm = int.Parse(tmpSegData[7]);
                    iAdd = 1;
                }

                SegmentTable.SegmentData = new s_SegmentData[SegmentTable.Segm];
                for (int iSeg = 0; iSeg < SegmentTable.Segm; iSeg++)
                {
                    SegmentTable.SegmentData[iSeg].Start = double.Parse(tmpSegData[(iSeg * iData) + 7 + iAdd]);
                    SegmentTable.SegmentData[iSeg].Stop = double.Parse(tmpSegData[(iSeg * iData) + 8 + iAdd]);
                    SegmentTable.SegmentData[iSeg].Points = int.Parse(tmpSegData[(iSeg * iData) + 9 + iAdd]);
                    tmpI = 10 + iAdd;
                    if (SegmentTable.Ifbw == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].IfbwValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Pow == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].PowValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Del == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].DelValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Swp == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].SwpValue = (e_SweepMode)Enum.Parse(typeof(e_SweepMode), tmpSegData[(iSeg * iData) + tmpI]);

                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Time == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].TimeValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                }

                Eq.Site[0].EqNetAn.Write("FORM:DATA " + DataFormat);
            }

            public override string ReadDataFormat(string _cmd)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write(":FORM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    Write(":FORM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
            }

            public override string ReadSegmentData(int Channel)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write("SENS" + Channel.ToString() + ":SEGM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    //Write(":FORM:DATA?");
                    //string binary_Type = NetAn.ReadString();
                    return "";
                }
            }

            public override string ReadSegmentCount(int Channel)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write("SENS" + Channel.ToString() + ":SEGM:COUNT?");
                    string Data = NetAn.ReadString();

                    return Data;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    //Write(":FORM:DATA?");
                    //string binary_Type = NetAn.ReadString();
                    return "";
                }
            }


        } //public class ENA_E5071C : NetAn_Base

        public class GenericTemplate : NetAn_Base
        {
            public new FormattedIO488 NetAn;
            public new ManualResetEvent enaStateRecallFlag = new ManualResetEvent(false);

            public enum ENATrigSource
            {
                Internal, External, Manual, Bus

            }

            public enum ENANumSweepPoints
            {
                Points_201,
                Points_401,
                Points_801,
                Points_1601
            }

            public override void InitializeENA(string InstrAddress)
            {

                try
                {

                    NetAn = GrabIO488DeviceByAliasName(InstrAddress);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }

            }

            public override FormattedIO488 GrabIO488DeviceByAliasName(string name)
            {
                //DialogResult resp = DialogResult.Retry;

                //while (resp == DialogResult.Retry)
                //{
                //    try
                //    {
                //        FormattedIO488 meEquip = new FormattedIO488();
                //        meEquip.IO = (IMessage)new Ivi.Visa.Interop.ResourceManager().Open(name, AccessMode.NO_LOCK, 3000, "");
                //        return meEquip;
                //    }
                //    catch
                //    {
                //        resp = MessageBox.Show("Failed to open communication with " + name, "Instrument Communication Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                //    }
                //}

                return (null);
            }

            public override void Write(string _cmd)
            {
                //try
                //{
                //    NetAn.WriteString(_cmd, true);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    MessageBox.Show("error writing to ENA\n\n" + e.ToString());
                //}
            }

            public override string ReadString()
            {
                //try
                //{
                //    return NetAn.ReadString();
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    //MessageBox.Show("error reading from ENA\n\n" + e.ToString());
                //    return "";
                //}
                return "";

            }

            public override void ReadData(int ChanNum, bool Synchronization)
            {
                //This is a dummy for the ENA because the ENA reads the data during ReadENATrace
            }

            public override string WriteRead(string _cmd)
            {
                //try
                //{
                //    Write(_cmd);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}



                return "";
            }

            public override double[] WriteReadIEEEBlock(string _cmd, IEEEBinaryType _type)
            {
                //try
                //{
                //    Write(_cmd);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}
                double[] dummy = { 0, 0, 0 };
                return dummy;
            }

            public override object Read_BiBlock()
            {
                //try
                //{
                //    return NetAn.ReadIEEEBlock(IEEEBinaryType.BinaryType_UI1, true, true);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    return 0;
                //}
                return 0;
            }

            public override void Write_IEEEBlock(string _cmd, object data)
            {
                try
                {
                    NetAn.WriteIEEEBlock(_cmd, data, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                }



            }

            public override void SetTimeout(int tmout)
            {
                //try
                //{
                //    NetAn.IO.Timeout = tmout;
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}



            }

            public override string funcSerialPoll()
            {
                string returnValue = null;

                //try
                //{
                //    while (true)
                //    {
                //        Write("*OPC");

                //        int bitAnd = 0;

                //        Write("*ESR?");
                //        returnValue = ReadString();

                //        if (returnValue.StartsWith("++"))
                //        {
                //            returnValue = returnValue.Substring(1);
                //        }

                //        bitAnd = Convert.ToInt16(returnValue) & 1;


                //        if (bitAnd == 1)
                //            break;

                //        else if (Convert.ToInt16(returnValue) == 0)
                //        {
                //            Thread.Sleep(1);
                //            continue;
                //        }
                //        else
                //        {

                //            Thread.Sleep(1);
                //        }


                //    }


                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}

                return returnValue;


            }

            public override void check_the_ena_catalog_for_stat_file_existence(string vFilename)
            {

                //try
                //{
                //    Write(":MMEM:CAT? \"D:\"");
                //    string file_list = ReadString();
                //    //zzzmyLibNa.CHECK_OPC();
                //    //bool file_exists = file_list.Contains("SUDARSH.STA");
                //    bool file_exists = file_list.Contains(vFilename.ToUpper());
                //    if (!file_exists)
                //    {
                //        transfer_from_pc_to_ena(vFilename);
                //    }

                //    Write(":MMEM:LOAD \"D:\\" + vFilename + "\"");
                //    //Thread.Sleep(10000);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}




            }

            public override void transfer_from_pc_to_ena(string vFile2Xfer)
            {
                ////
                ////FileStream fs = new FileStream(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Pike\e5071b_state_file\sudarsh_pike_state01.sta", FileMode.Open );
                //FileStream fs = new FileStream(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Pike\Pike_B7_OpFiles\E5071B_State_Files\" + vFile2Xfer, FileMode.Open);
                //// Create the reader for data.
                //BinaryReader w = new BinaryReader(fs);
                //long length = w.BaseStream.Length;
                //string length1 = Convert.ToString(length);
                //int strlen_of_filesize = length1.Length;

                //byte[] stadata = new byte[length];
                //long pos = 0L;

                //try
                //{
                //    while (pos < length)
                //    {
                //        stadata[pos] = w.ReadByte();
                //        pos++;
                //    }
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    long newPOs = pos;
                //}

                //w.Close();
                ////fs.Close();
                ////string haha = ":MMEM:TRAN \"D:\\PikeB7.sta\",";  //#" + strlen_of_filesize.ToString() + length1;
                //string haha = ":MMEM:TRAN \"D:\\" + vFile2Xfer + "\",";  //#" + strlen_of_filesize.ToString() + length1;

                //Write_IEEEBlock(haha, stadata); //original cmd: ":MMEM:TRAN \"D:\\sudarsh.sta\",#6223986MSCF"
                ////zzzmyLibNa.CHECK_OPC();

                ////Thread.Sleep(1000);
            }

            public override void RecallENAState()
            {
                //Write(":MMEM:LOAD \"" + (string)o + "\"");
                //Thread.Sleep(8000);
                //Write(":DISP:ENAB 0");   // 0 off, 1 on
                //Write(":TRIG:SOUR BUS");
                //enaStateRecallFlag.Set();
            }

            public override List<double> ReadFreqList_Chan(int channel)
            {
                //try
                //{
                //    return FreqList[channel];
                //}
                //catch (Exception e)
                //{
                //    List<double> freqs = new List<double>();
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    return freqs;
                //}
                return FreqList[channel];
            }

            public override List<double> ReadFreqList(int channel)
            {
                //try
                //{
                //    FreqList.Clear();

                //    for (channel = 1; channel <= total_chan; channel++)
                //    {
                //        int traceNum = 0;
                //        foreach (int trace in Chan_Trace_set.Keys)
                //        {
                //            if (Chan_Trace_set[trace] == channel)
                //            {
                //                traceNum = trace;//Any trace num on the channel 
                //                break;
                //            }
                //        }

                //        Write(":FORM:BORD NORM; :FORM:DATA REAL");
                //        List<double> freqs = WriteReadIEEEBlock(":SENS" + channel + ":FREQ:DATA?", IEEEBinaryType.BinaryType_R8).ToList();
                //        for (int i = 0; i < freqs.Count(); i++) freqs[i] *= 1e-6;

                //        FreqList.Add(channel, freqs);
                //    }
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //    //return new List<double>();
                //}
                return new List<double>();
            }

            public override void SendTrigger(bool Sync, int Chan, int Trace)
            {
                //Write("*CLS");
                //Write(":TRIG:SEQ:SCOP ACT");
                //Write(":DISP:WIND" + Chan + ":ACT");
                //Write(":INIT" + Chan + ":CONT ON");
                //Write(":TRIG:SOUR BUS");
                //Write(":TRIG:SING;*OPC");
                //funcSerialPoll();
            }


            public override string CheckTraceFormat(int channel, int tracenum)
            {
                //string traceformat = "";

                //Write(":CALC" + channel.ToString() + ":TRAC" + tracenum.ToString() + ":FORM?");
                //return traceformat = ReadString();
                return "";

            }

            public override double[] ReadENATrace(int channel, int trace_OneBased)
            {
                //try
                //{
                //    Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                //    double[] fullDATA_X = WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:FDAT?", IEEEBinaryType.BinaryType_R8);

                //    double[] data = new double[fullDATA_X.Length / 2];

                //    for (int k = 0; k < data.Length; k++)
                //    {
                //        data[k] = fullDATA_X[2 * k];
                //    }
                //    return data;
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                //    return new double[4];
                //}
                return new double[4];
            }

            public override double[] ReadENATrace2(int channel, int trace_OneBased)
            {
                //try
                //{
                //    Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                //    double[] fullDATA_X = WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:SDAT?", IEEEBinaryType.BinaryType_R8);

                //    //double[] data = new double[fullDATA_X.Length];

                //    //for (int k = 0; k < data.Length; k++)
                //    //{
                //    //    data[k] = fullDATA_X[2 * k];
                //    //}

                //    return fullDATA_X;
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                //    return new double[4];
                //}
                return new double[4];
            }

            public override void defineTracex(int ChannelNum, int TraceNumber, string MeasType)
            {
                //try
                //{
                //    Write(":DISP:WIND" + ChannelNum.ToString() + ":" + "TRAC" + TraceNumber.ToString() + ":STAT ON");
                //    string h = ":CALC" + ChannelNum.ToString() + ":PAR" + TraceNumber.ToString() + ":DEF " + MeasType;
                //    Write(h);
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}
            }

            public override void ENARecallState(string nameoffile)
            {

                //try
                //{
                //    Write(":MMEM:LOAD \"D:\\" + nameoffile + ".STA" + "\"");
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                //}
            }
                        
            public override void SetCalibrationType(int ChanNo, int numPorts, string PortDesignation)
            {
                //try
                //{
                //    //should really qualify numports and ChanNo for valid numbers
                //    Write(":SENS" + ChanNo + ":CORR:COLL:METH:SOLT" + numPorts + " " + PortDesignation.Trim()); //  " 1,2,3");

                //}
                //catch (Exception e)
                //{


                //}


            }

            public override void SelectCalKit(int ChanNo, int CalNum)
            {
                //try
                //{
                //    Write(":SENSE" + ChanNo + ":CORR:COLL:CKIT:SEL " + CalNum);
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}


            }

            public override void CalibrateOpen(int ChanNo, int PortNum)
            {
                //try
                //{
                //    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:OPEN " + PortNum);

                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}


            }

            public override void CalibrateShort(int ChanNo, int PortNum)
            {
                //try
                //{
                //    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:SHOR " + PortNum);
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}


            }

            public override void CalibrateLoad(int ChanNo, int PortNum)
            {
                //try
                //{
                //    Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:LOAD " + PortNum);
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}


            }

            public override void SaveCalibration(string zfilenamewext, int sleepTime = 0)
            {
                //try
                //{
                //    Write(":MMEM:STOR:STYP CDST"); //Save Type = All
                //    if (sleepTime != 0) Thread.Sleep(sleepTime);
                //    funcSerialPoll();
                //    //ENA.Write(":MMEM:STORE \"D:\\SPYRO_FBAR_AUTO.STA" + "\""); //File used to initially save the cal results
                //    Write(":MMEM:STORE \"D:\\" + zfilenamewext + ".STA" + "\"");
                //    if (sleepTime != 0) Thread.Sleep(sleepTime);
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void SetFixtureSimState(int ChanNo, int SimState)
            {
                //try
                //{
                //    Write(":CALC" + ChanNo.ToString() + ":FSIM:STAT " + SimState.ToString());
                //    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:STAT " + SimState.ToString()); //Turn off Port ZConversion
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void SetPortImpedance(int ChanNo, double ImpReal, double ImpIMG, string PortNo)
            {
                //try
                //{
                //    SetFixtureSimState(ChanNo, 1);
                //    funcSerialPoll();
                //    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNo + ":Z0 " + ImpReal.ToString()); //Set port impedance for Port 3 (Rx port) 
                //    funcSerialPoll();
                //    Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNo + ":IMAG " + ImpIMG.ToString()); //Set port impedance for Port 3 (Rx port) 
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}
            }



            public override void SetTrigInternal()
            {
                //try
                //{
                //    Write(":TRIG:SOUR INT");
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void SetTrigBus()
            {
                //try
                //{
                //    Write(":TRIG:SOUR BUS");
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void SetActiveWindow(int zwindow)
            {
                //try
                //{
                //    Write(":DISP:WIND" + zwindow.ToString().Trim() + ":ACT");
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void MaximizeActiveWindow(bool maximize)
            {
                //try
                //{
                //    Write(":DISP:MAX " + (maximize ? 1 : 0));
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void SetActiveTrace(int chanNum, int traceNum)
            {
                //try
                //{
                //    Write(":CALC" + chanNum + ":PAR" + traceNum + ":SEL");
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void MaximizeActiveTrace(int NaChan, bool maximize)
            {
                //try
                //{
                //    Write(":DISP:WIN" + NaChan + ":MAX " + (maximize ? 1 : 0));
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override string GetTraceType(int chanNum, int traceNum)
            {
                //Write(":CALC" + chanNum + ":PAR" + traceNum + ":DEF?");
                //return ReadString().Trim();
                return "";
            }

            public override int GetTraceCount(int chanNum)
            {
                //Write(":CALC" + chanNum + ":PAR:COUN?");
                //return Convert.ToInt32(ReadString().Trim());
                return 0;
            }

            public override void PerformEcal(int ChanNo, int numports, string Ports)
            {
                //try
                //{
                //    Write(":SENS" + ChanNo + ":CORR:COLL:ECAL:SOLT" + numports + " " + Ports);
                //}
                //catch (Exception e)
                //{

                //}
            }

            public override void CalibrateThruWithSubclass(int ChanNo, int StimPortNum, int RespPortNum)
            {
                //try
                //{
                //    Write(":SENS" + ChanNo + ":CORR:COLL:SUBC " + ChanNo);
                //    funcSerialPoll();

                //    Write(":SENS" + ChanNo + ":CORR:COLL:THRU " + StimPortNum + "," + RespPortNum);
                //    funcSerialPoll();
                //}
                //catch (Exception e)
                //{

                //}


            }

            public override void CalcCalCoeffs(int ChanNo)
            {
                //try
                //{
                //    //Calculate the cal coefficients for a given channel
                //    //This command should only be used after all necessary combinations of port combinations have been done
                //    //otherwise the command is ignored
                //    Write(":SENS" + ChanNo + ":CORR:COLL:SAVE");

                //}
                //catch (Exception e)
                //{


                //}


            }

            public override void InsertSegmentTableData(int channelNumber, S_SegmentTable segmentTable)
            {

                Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");

                switch (segmentTable.Swp)
                {
                    case e_OnOff.On:
                        Eq.Site[0].EqNetAn.Write("SENS" + channelNumber.ToString() + ":SEGM:DATA 6," + Convert_SegmentTable2String(segmentTable));
                        break;
                    case e_OnOff.Off:
                        Eq.Site[0].EqNetAn.Write("SENS" + channelNumber.ToString() + ":SEGM:DATA 5," + Convert_SegmentTable2String(segmentTable));
                        break;
                }
                //funcSerialPoll();
                Eq.Site[0].EqNetAn.Write("FORM:DATA REAL");

            }

            private string Convert_SegmentTable2String(S_SegmentTable SegmentTable)
            {
                string tmpStr;
                tmpStr = "";
                e_OnOff sweepmode = SegmentTable.Swp;
                switch (sweepmode)
                {
                    case e_OnOff.On:
                        tmpStr = ((int)SegmentTable.Mode).ToString();
                        tmpStr += "," + ((int)SegmentTable.Ifbw).ToString();
                        tmpStr += "," + ((int)SegmentTable.Pow).ToString();
                        tmpStr += "," + ((int)SegmentTable.Del).ToString();
                        tmpStr += "," + ((int)SegmentTable.Swp).ToString();
                        tmpStr += "," + ((int)SegmentTable.Time).ToString();
                        tmpStr += "," + SegmentTable.Segm.ToString();
                        for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                        {
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                            if (SegmentTable.Ifbw == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].IfbwValue.ToString();
                            if (SegmentTable.Pow == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].PowValue.ToString();
                            if (SegmentTable.Del == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].DelValue.ToString();
                            if (SegmentTable.Swp == e_OnOff.On)
                                tmpStr += "," + ((int)SegmentTable.SegmentData[Seg].SwpValue).ToString(); //Stepped/0
                            if (SegmentTable.Time == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].TimeValue.ToString();
                        }
                        break;
                    case e_OnOff.Off:
                        tmpStr = ((int)SegmentTable.Mode).ToString();
                        tmpStr += "," + ((int)SegmentTable.Ifbw).ToString();
                        tmpStr += "," + ((int)SegmentTable.Pow).ToString();
                        tmpStr += "," + ((int)SegmentTable.Del).ToString();
                        tmpStr += "," + ((int)SegmentTable.Time).ToString();
                        tmpStr += "," + SegmentTable.Segm.ToString();
                        for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                        {
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                            tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                            if (SegmentTable.Ifbw == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].IfbwValue.ToString();
                            if (SegmentTable.Pow == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].PowValue.ToString();
                            if (SegmentTable.Del == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].DelValue.ToString();
                            if (SegmentTable.Time == e_OnOff.On)
                                tmpStr += "," + SegmentTable.SegmentData[Seg].TimeValue.ToString();
                        }
                        break;
                }
                return (tmpStr);
            }

            public override void GetSegmentTable(int channelNumber, out S_SegmentTable SegmentTable)
            {
                string DataFormat;
                string tmpStr;
                string[] tmpSegData;
                long tmpI;
                int iData = 3, iAdd = 0;

                SegmentTable = new S_SegmentTable();

                //segTable = new S_SegmentTable();
                DataFormat = Eq.Site[0].EqNetAn.ReadDataFormat("FORM:DATA?");
                if (DataFormat.ToUpper() == "REAL") Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");
                Eq.Site[0].EqNetAn.Write("FORM:DATA ASC");
                tmpStr = Eq.Site[0].EqNetAn.ReadSegmentData(channelNumber);
                tmpSegData = tmpStr.Split(',');

                for (int s = 0; s < tmpSegData.Length; s++)
                {
                    tmpI = (long)(Convert.ToDouble(tmpSegData[s]));
                    tmpSegData[s] = tmpI.ToString();
                }

                SegmentTable.Mode = (e_ModeSetting)Enum.Parse(typeof(e_ModeSetting), tmpSegData[1]);
                SegmentTable.Ifbw = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[2]);
                SegmentTable.Pow = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[3]);
                SegmentTable.Del = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[4]);

                if (tmpSegData[0] == "5")
                {
                    SegmentTable.Swp = e_OnOff.Off;
                    SegmentTable.Time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                    SegmentTable.Segm = int.Parse(tmpSegData[6]);
                }
                else if (tmpSegData[0] == "6")
                {
                    SegmentTable.Swp = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                    SegmentTable.Time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[6]);
                    SegmentTable.Segm = int.Parse(tmpSegData[7]);
                    iAdd = 1;
                }

                SegmentTable.SegmentData = new s_SegmentData[SegmentTable.Segm];
                for (int iSeg = 0; iSeg < SegmentTable.Segm; iSeg++)
                {
                    SegmentTable.SegmentData[iSeg].Start = double.Parse(tmpSegData[(iSeg * iData) + 7 + iAdd]);
                    SegmentTable.SegmentData[iSeg].Stop = double.Parse(tmpSegData[(iSeg * iData) + 8 + iAdd]);
                    SegmentTable.SegmentData[iSeg].Points = int.Parse(tmpSegData[(iSeg * iData) + 9 + iAdd]);
                    tmpI = 10 + iAdd;
                    if (SegmentTable.Ifbw == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].IfbwValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Pow == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].PowValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Del == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].DelValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Swp == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].SwpValue = (e_SweepMode)Enum.Parse(typeof(e_SweepMode), tmpSegData[(iSeg * iData) + tmpI]);

                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                    if (SegmentTable.Time == e_OnOff.On)
                    {
                        SegmentTable.SegmentData[iSeg].TimeValue = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                        tmpI++;
                        if (iSeg == 0) iData++;
                    }
                }

                Eq.Site[0].EqNetAn.Write("FORM:DATA " + DataFormat);
            }

            public override string ReadDataFormat(string _cmd)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write(":FORM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    Write(":FORM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
            }

            public override string ReadSegmentData(int Channel)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write("SENS" + Channel.ToString() + ":SEGM:DATA?");
                    string binary_Type = NetAn.ReadString();
                    return binary_Type;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    //Write(":FORM:DATA?");
                    //string binary_Type = NetAn.ReadString();
                    return "";
                }
            }

            public override string ReadSegmentCount(int Channel)
            {
                try
                {
                    //Write("FORM:DATA REAL");
                    //string binary_Type = NetAn.ReadString();
                    Write("SENS" + Channel.ToString() + ":SEGM:COUNT?");
                    string Data = NetAn.ReadString();

                    return Data;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Exception happened during ReadENATrace" + "\r\n" + e.ToString());
                    //Write(":FORM:DATA?");
                    //string binary_Type = NetAn.ReadString();
                    return "";
                }
            }
        } //Template

        /// <summary>
        /// NetAn_Base will serve as the abstract class
        /// for Network Analyzers not similar to Topaz
        /// </summary>
        public abstract class NetAn_Base
        {
            public FormattedIO488 NetAn;
            public ManualResetEvent enaStateRecallFlag = new ManualResetEvent(false);

            public abstract void InitializeENA(string InstrAddress);


            public abstract FormattedIO488 GrabIO488DeviceByAliasName(string name);

            public abstract void Write(string _cmd);

            public abstract string ReadString();

            public abstract void ReadData(int ChanNum, bool Synchronization);

            public abstract string WriteRead(string _cmd);

            public abstract double[] WriteReadIEEEBlock(string _cmd, IEEEBinaryType _type);

            public abstract object Read_BiBlock();

            public abstract void Write_IEEEBlock(string _cmd, object data);

            public abstract void SetTimeout(int tmout);

            public abstract string funcSerialPoll();

            public abstract void check_the_ena_catalog_for_stat_file_existence(string vFilename);

            public abstract void transfer_from_pc_to_ena(string vFile2Xfer);

            public abstract void RecallENAState();

            public abstract List<double> ReadFreqList_Chan(int channel);
            public abstract List<double> ReadFreqList(int total_channel);

            public abstract void SendTrigger(bool Sync, int Chan, int Trace=0);

            public abstract string CheckTraceFormat(int channel, int tracenum);

            public abstract double[] ReadENATrace(int channel, int trace_OneBased);

            public abstract double[] ReadENATrace2(int channel, int trace_OneBased);

            public abstract void defineTracex(int ChannelNum, int TraceNumber, string MeasType);

            public abstract void ENARecallState(string nameoffile);

            public abstract void SetCalibrationType(int ChanNo, int numPorts, string PortDesignation);

            public abstract void SelectCalKit(int ChanNo, int CalNum);

            public abstract void CalibrateOpen(int ChanNo, int PortNum);

            public abstract void CalibrateShort(int ChanNo, int PortNum);

            public abstract void CalibrateLoad(int ChanNo, int PortNum);

            public abstract void SaveCalibration(string zfilenamewext, int sleepTime = 0);

            public abstract void SetFixtureSimState(int ChanNo, int SimState);

            public abstract void SetPortImpedance(int ChanNo, double ImpReal, double ImpIMG, string PortNo);

            public abstract void SetTrigInternal();

            public abstract void SetTrigBus();

            public abstract void SetActiveWindow(int zwindow);

            public abstract void MaximizeActiveWindow(bool maximize);

            public abstract void SetActiveTrace(int chanNum, int traceNum);

            public abstract void MaximizeActiveTrace(int NaChan, bool maximize);


            public abstract string GetTraceType(int chanNum, int traceNum);

            public abstract int GetTraceCount(int chanNum);

            public abstract void PerformEcal(int ChanNo, int numports, string Ports);

            public abstract void CalibrateThruWithSubclass(int ChanNo, int StimPortNum, int RespPortNum);

            public abstract void CalcCalCoeffs(int ChanNo);

            public abstract string ReadDataFormat(string _cmd);

            public abstract string ReadSegmentData(int Channel);

            public abstract string ReadSegmentCount(int Channel);

            public abstract void InsertSegmentTableData(int channelNumber, S_SegmentTable segmentTable);

            public abstract void GetSegmentTable(int channelNumber, out S_SegmentTable SegmentTable);
        }

    }




} //NALib Namespace closing brace
