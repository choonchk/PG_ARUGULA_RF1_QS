using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Ivi.Visa.Interop;
using System.Windows.Forms;

using Avago.ATF.Shares;
using Avago.ATF.StandardLibrary;
using System.IO;
using System.Threading;

using ClothoLibAlgo;


namespace EqLib
{
    
    
    public static class ENA
    {
        public static FormattedIO488 NetAn;
        public static ManualResetEvent enaStateRecallFlag = new ManualResetEvent(false);

        public static void InitializeENA(string InstrAddress)
        {

            try
            {
                
                NetAn = GrabIO488DeviceByAliasName(InstrAddress);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }
            
        }

        public static FormattedIO488 GrabIO488DeviceByAliasName(string name)
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

        public static void Write(string _cmd)
        {
            try
            {
                NetAn.WriteString(_cmd, true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
                MessageBox.Show("error writing to ENA\n\n" + e.ToString());
            }
        }
        
        public static string Read()
        {
            try
            {
                return NetAn.ReadString();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
                MessageBox.Show("error reading from ENA\n\n" + e.ToString());
                return "";
            }

        }

        public static string WriteRead(string _cmd)
        {
            try
            {
                Write(_cmd);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }
            
            
            
            return Read();
        }

        public static double[] WriteReadIEEEBlock(string _cmd, IEEEBinaryType _type)
        {
            try
            {
                Write(_cmd);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }

            return NetAn.ReadIEEEBlock(_type, true, true);
        }

        public static object Read_BiBlock()
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

        public static void Write_IEEEBlock(string _cmd, object data)
        {
            try
            {
                NetAn.WriteIEEEBlock(_cmd, data, true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }
            
            

        }
        
        public static void SetTimeout(int tmout)
        {
            try
            {
                NetAn.IO.Timeout = tmout;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }
            
            
            
        }

        public static string funcSerialPoll()
        {
            string returnValue = null;

            try
            {
                while (true)
                {
                    ENA.Write("*OPC");

                    int bitAnd = 0;

                    Write("*ESR?");
                    returnValue = Read();

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
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }

            return returnValue;
            
            
        }

        public static void check_the_ena_catalog_for_stat_file_existence(string vFilename)
        {

            try
            {
                ENA.Write(":MMEM:CAT? \"D:\"");
                string file_list = ENA.Read();
                //zzzmyLibNa.CHECK_OPC();
                //bool file_exists = file_list.Contains("SUDARSH.STA");
                bool file_exists = file_list.Contains(vFilename.ToUpper());
                if (!file_exists)
                {
                    transfer_from_pc_to_ena(vFilename);
                }

                ENA.Write(":MMEM:LOAD \"D:\\" + vFilename + "\"");
                //Thread.Sleep(10000);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
            }
            
            
            
            
        }

        public static void transfer_from_pc_to_ena(string vFile2Xfer)
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
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine  + e.StackTrace);
                long newPOs = pos;
            }

            w.Close();
            //fs.Close();
            //string haha = ":MMEM:TRAN \"D:\\PikeB7.sta\",";  //#" + strlen_of_filesize.ToString() + length1;
            string haha = ":MMEM:TRAN \"D:\\" + vFile2Xfer + "\",";  //#" + strlen_of_filesize.ToString() + length1;

            ENA.Write_IEEEBlock(haha, stadata); //original cmd: ":MMEM:TRAN \"D:\\sudarsh.sta\",#6223986MSCF"
            //zzzmyLibNa.CHECK_OPC();

            //Thread.Sleep(1000);
        }

        public static void RecallENAState(object o)
        {
            ENA.Write(":MMEM:LOAD \"" + (string)o + "\"");
            Thread.Sleep(8000);
            ENA.Write(":DISP:ENAB 0");   // 0 off, 1 on
            ENA.Write(":TRIG:SOUR BUS");
            enaStateRecallFlag.Set();
        }

        public static List<double> ReadFreqList(int channel)
        {
            try
            {
                ENA.Write(":FORM:BORD NORM; :FORM:DATA REAL");

                List<double> freqs = ENA.WriteReadIEEEBlock(":SENS" + channel + ":FREQ:DATA?", IEEEBinaryType.BinaryType_R8).ToList();

                for (int i = 0; i < freqs.Count(); i++) freqs[i] *= 1e-6;

                return freqs;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
                return new List<double>();
            }
        }

        public static string CheckTraceFormat(int channel, int tracenum)
        {
            string traceformat = "";

            ENA.Write(":CALC" + channel.ToString() + ":TRAC" + tracenum.ToString() + ":FORM?");
            return traceformat = ENA.Read();

        }
        
        public static double[] ReadENATrace(int channel, int trace_OneBased)
        {
            try
            {
                ENA.Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                double[] fullDATA_X = ENA.WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:FDAT?", IEEEBinaryType.BinaryType_R8);

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

        public static double[] ReadENATrace2(int channel, int trace_OneBased)
        {
            try
            {
                ENA.Write(":CALC" + channel.ToString() + ":PAR" + trace_OneBased.ToString() + ":SEL");
                double[] fullDATA_X = ENA.WriteReadIEEEBlock(":CALC" + channel.ToString() + ":DATA:SDAT?", IEEEBinaryType.BinaryType_R8);

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

        public static void defineTracex(int ChannelNum, int TraceNumber, string MeasType)
        {
            try
            {
                ENA.Write(":DISP:WIND" + ChannelNum.ToString() + ":" + "TRAC" + TraceNumber.ToString() + ":STAT ON");
                string h = ":CALC" + ChannelNum.ToString() + ":PAR" + TraceNumber.ToString() + ":DEF " + MeasType;
                ENA.Write(h);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in NetAn.cs:" + e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            }
        }

        //Imported from old NALib.ENA_SystemCal >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

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

        public static double fstart
        {
            set
            {
                if (fstart < 100e3) fstart = 100e3;
            }
            get
            {
                return fstart;
            }
        }

        public static double fstop
        {
            set
            {
                if (fstop > 8.5e9) fstop = 8.5e9;
            }
            get
            {
                return fstop;
            }
        }

        public static int SweepSetup_Points
        {
            set
            {
                if (value < 201 || value > 1601) value = 801;
            }
            get
            {
                return SweepSetup_Points;
            }




        }

        public static void ENARecallState(string nameoffile)
        {

            try
            {
                ENA.Write(":MMEM:LOAD \"D:\\" + nameoffile + ".STA" + "\"");
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

        //public static void ENA_WriteDataToFile(string CalDir, string PortInfo, string BandInfo)
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

        public static void SetCalibrationType(int ChanNo, int numPorts, string PortDesignation)
        {
            try
            {
                //should really qualify numports and ChanNo for valid numbers
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:METH:SOLT" + numPorts + " " + PortDesignation.Trim()); //  " 1,2,3");

            }
            catch (Exception e)
            {


            }


        }

        public static void SelectCalKit(int ChanNo, int CalNum)
        {
            try
            {
                ENA.Write(":SENSE" + ChanNo + ":CORR:COLL:CKIT:SEL " + CalNum);
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }


        }

        public static void CalibrateOpen(int ChanNo, int PortNum)
        {
            try
            {
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:OPEN " + PortNum);

                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }


        }

        public static void CalibrateShort(int ChanNo, int PortNum)
        {
            try
            {
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:SHOR " + PortNum);
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }


        }

        public static void CalibrateLoad(int ChanNo, int PortNum)
        {
            try
            {
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:ACQ:LOAD " + PortNum);
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }


        }

        public static void SaveCalibration(string zfilenamewext, int sleepTime=0)
        {
            try
            {
                ENA.Write(":MMEM:STOR:STYP CDST"); //Save Type = All
                if (sleepTime != 0) Thread.Sleep(sleepTime);
                ENA.funcSerialPoll();
                //ENA.Write(":MMEM:STORE \"D:\\SPYRO_FBAR_AUTO.STA" + "\""); //File used to initially save the cal results
                ENA.Write(":MMEM:STORE \"D:\\" + zfilenamewext + ".STA" + "\"");
                if (sleepTime != 0) Thread.Sleep(sleepTime);
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }
        }

        public static void SetFixtureSimState(int ChanNo, int SimState)
        {
            try
            {
                ENA.Write(":CALC"+ChanNo.ToString()+":FSIM:STAT " + SimState.ToString());
                ENA.Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:STAT " + SimState.ToString()); //Turn off Port ZConversion
                ENA.funcSerialPoll();                
            }
            catch (Exception e)
            {

            }
        }

        public static void SetPortImpedance(int ChanNo, double ImpReal, double ImpIMG, string PortNo)
        {
            try
            {
                SetFixtureSimState( ChanNo, 1);
                ENA.funcSerialPoll();            
                ENA.Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT"+PortNo+":Z0 " + ImpReal.ToString()); //Set port impedance for Port 3 (Rx port) 
                ENA.funcSerialPoll();
                ENA.Write(":CALC" + ChanNo.ToString() + ":FSIM:SEND:ZCON:PORT"+PortNo+":IMAG " + ImpIMG.ToString()); //Set port impedance for Port 3 (Rx port) 
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }
        }



        public static void SetTrigInternal()
        {
            try
            {
                ENA.Write(":TRIG:SOUR INT");
            }
            catch (Exception e)
            {

            }
        }

        public static void SetTrigBus()
        {
            try
            {
                ENA.Write(":TRIG:SOUR BUS");
            }
            catch (Exception e)
            {

            }
        }

        public static void SetActiveWindow(int zwindow)
        {
            try
            {
                ENA.Write(":DISP:WIND" + zwindow.ToString().Trim() + ":ACT");
            }
            catch (Exception e)
            {

            }
        }

        public static void MaximizeActiveWindow(bool maximize)
        {
            try
            {
                ENA.Write(":DISP:MAX " + (maximize ? 1 : 0));
            }
            catch (Exception e)
            {

            }
        }

        public static void SetActiveTrace(int chanNum, int traceNum)
        {
            try
            {
                ENA.Write(":CALC" + chanNum + ":PAR" + traceNum + ":SEL");
            }
            catch (Exception e)
            {

            }
        }

        public static void MaximizeActiveTrace(int NaChan, bool maximize)
        {
            try
            {
                ENA.Write(":DISP:WIN" + NaChan + ":MAX " + (maximize ? 1 : 0));
            }
            catch (Exception e)
            {

            }
        }

        public static string GetTraceType(int chanNum, int traceNum)
        {
            ENA.Write(":CALC" + chanNum + ":PAR" + traceNum + ":DEF?");
            return ENA.Read().Trim();
        }

        public static int GetTraceCount(int chanNum)
        {
            ENA.Write(":CALC" + chanNum + ":PAR:COUN?");
            return Convert.ToInt32(ENA.Read().Trim());
        }

        public static void PerformEcal(int ChanNo, int numports, string Ports)
        {
            try
            {
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:ECAL:SOLT" + numports + " " + Ports);
            }
            catch(Exception e)
            {

            }
        }

        public static void CalibrateThruWithSubclass(int ChanNo, int StimPortNum, int RespPortNum)
        {
            try
            {
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:SUBC " + ChanNo);
                ENA.funcSerialPoll();

                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:THRU " + StimPortNum + "," + RespPortNum);
                ENA.funcSerialPoll();
            }
            catch (Exception e)
            {

            }


        }

        public static void CalcCalCoeffs(int ChanNo)
        {
            try
            {
                //Calculate the cal coefficients for a given channel
                //This command should only be used after all necessary combinations of port combinations have been done
                //otherwise the command is ignored
                ENA.Write(":SENS" + ChanNo + ":CORR:COLL:SAVE");

            }
            catch (Exception e)
            {


            }


        }




    } //public static class ENA closing brace


    

} //NALib Namespace closing brace
