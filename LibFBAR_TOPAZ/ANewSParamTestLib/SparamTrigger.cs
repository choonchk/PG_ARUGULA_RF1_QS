using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;

namespace LibFBAR_TOPAZ
{
    public class SparamTrigger : TestCaseBase
    {
        #region "Declarations"

        // External Variables
        public int ChannelNumber;

        public int Sleep_ms;
        public int TestNo;

        public string Misc_Settings;
        public string SNPFileOutput_Path;
        /// <summary>
        /// TCF Power Mode column.
        /// </summary>
        public string FileOutput_Mode;

        public string FileOutput_FileName;
        public int FileOutput_Unit;
        public bool ENASNPFileOutput_Enable;

        //KCC - SNP File Count
        public int FileOutput_Count;
        /// <summary>
        /// how many files have been generated.
        /// </summary>
        public int FileOutput_Counting;
        public int SNPFileOutput_Counting;
        public int FileOutput_Sampling_Count = 0;
        public int SNPFileOutput_Sampling_Count = 0; //Jerome - 20191203 - To generate Snp separatley from Trace file
        public string SnPFile_Name;
        public bool SnPFile_Generate;

        // Internal Variables
        private e_SFormat tmp_SFormat;

        private bool b_Trigger_Pause;

        public bool b_AutoCheckFormat = false;
        private bool b_Captured = false;

        public string Select_RX;
        public string SwitchIn;
        public string SwitchAnt;
        public string SwitchOut;
        public string Band;
        public string TunableBand;
        public string Switch_Ant_val;
        public string ParameterNote;

        private Stopwatch watch = new Stopwatch();
        public bool SecondTest;

        public int Count;

        // NF Dual yoonchun
        public int MasterChannel;

        public int SlaveChannel;

        public string Master_RX;
        public string Slave_RX;

        public string Master_Band;
        public string Slave_Band;
        private bool b_EnhanceDataFetch = true;
        private string m_traceFileSnpSavePath;

        private int previousChannel;
        private int topazTraceOffset = 0;
        private int previous_topazTraceOffset = 0;
        public SParaFileManager m_modelTraceManager;
        private TriggerModel m_model2;
        private DataTriggeredDataModel m_modelTriggerDm;
        private TopazEquipmentDriver m_equipment;
        private cENA ENA;
        #endregion "Declarations"

        public TopazEquipmentDriver EquipmentENA
        {
            get
            {
                return m_equipment;
            }
            set
            {
                m_equipment = value;
                ENA = m_equipment.ENA;
            }
        }

        public string TraceFileSnpSavePath
        {
            get { return m_traceFileSnpSavePath; }
            set { m_traceFileSnpSavePath = value; }
        }

        public override void InitSettings()
        {
            SaveResult.Enable = false;
            if (Misc_Settings.Contains("P") || Misc_Settings.Contains("p"))
            {
                b_Trigger_Pause = true;
            }
            m_model2 = new TriggerModel();
        }

        public override void RunTest()
        {
            string swParent = String.Format("Trigger_RunTest_{0}", TestNo);

            if (ChannelNumber == 0)
            {
                TriggleSingle();
            }
            else
            {
                watch.Reset();
                watch.Start();

                if (ChannelNumber == 1) topazTraceOffset = 0;
                if (ChannelNumber == previousChannel) topazTraceOffset = previous_topazTraceOffset;
                //ENA.Initiate.Immediate(ChannelNumber);
                //ENA.Display.Window.Activate(ChannelNumber);

                TriggleSingle();

                long FbarTimeTemp = watch.ElapsedMilliseconds;

                SaveSnpFileToENA();
                
                // Save SNP.
                //bool isSaveSnp = m_modelTraceManager.TigerModel.IsSaveSnp1(FileOutput_Sampling_Count,
                //    FileOutput_Count, SnPFile_Name);
                //if (isSaveSnp)
                //{
                //   SaveSnpFileToENA(TraceFileSnpSavePath);
                //}
                
            }

            watch.Reset();
            watch.Start();

            Thread.Sleep(Sleep_ms);
            //  MeasureResult();
            MeasureResult2();
            //SaveTrace(ChannelNumber);     // Moved out.

            long FbarTimeTemp1 = watch.ElapsedMilliseconds;

            //  double AA = watch.Elapsed.TotalMilliseconds;
            previousChannel = ChannelNumber;
            if (b_Trigger_Pause)
            {
            }

            //DataTrigger_i++;
            //m_modelTriggerDm.IncrementTrigger();
        }

        // To Be Deleted.
        public void RunTest2()
        {
            if (ChannelNumber == 0)
            {
                TriggleSingle();
            }
            else
            {
                watch.Reset();
                watch.Start();

                if (ChannelNumber == 1) topazTraceOffset = 0;
                if (ChannelNumber == previousChannel) topazTraceOffset = previous_topazTraceOffset;
                //ENA.Initiate.Immediate(ChannelNumber);
                //ENA.Display.Window.Activate(ChannelNumber);

                TriggleSingle();

                long FbarTimeTemp = watch.ElapsedMilliseconds;

                // Save SNP.
                bool isSaveSnp = m_modelTraceManager.TigerModel.IsSaveSnp1(FileOutput_Sampling_Count,
                    FileOutput_Count, SnPFile_Name);
                if (isSaveSnp)
                {
                    string FileNamePartial = String.Format("CH{0}_{1}_{2}_{3}",
                        ChannelNumber, TunableBand, Select_RX, FileOutput_Mode);
                    //string FileNamePartial = String.Format("{0}_CH{1}_{2}_{3}_{4}_{5}_{6}",
                    //    m_modelTriggerDm.GetCurrentDataTriggerNumber(),
                    //    ChannelNumber, Band, FileOutput_Mode, SwitchIn, SwitchAnt, SwitchOut);
                    string snpOutputFilePath = m_modelTraceManager.TigerModel.FormSnpOutputFileName(FileNamePartial);

                    SaveSnpFileToENA(snpOutputFilePath);
                }
            }

            watch.Reset();
            watch.Start();

            Thread.Sleep(Sleep_ms);
            //  MeasureResult();
            MeasureResult2();
            SaveTrace(ChannelNumber);

            long FbarTimeTemp1 = watch.ElapsedMilliseconds;

            //  double AA = watch.Elapsed.TotalMilliseconds;
            previousChannel = ChannelNumber;
            if (b_Trigger_Pause)
            {
            }

            //DataTrigger_i++;
            m_modelTriggerDm.IncrementTrigger();
        }

        public void RunTest_NF()
        {
            double[] testtest = new double[50];

            double NF_trigger_TestTime = 0;


            watch.Reset();
            watch.Start();

            TriggleSingle();

            watch.Stop();
            NF_trigger_TestTime = watch.Elapsed.TotalMilliseconds;

            watch.Reset();
            watch.Start();

            Thread.Sleep(Sleep_ms);

            m_equipment.MeasureResult_NF(ChannelNumber);
            long FbarTimeTemp1 = watch.ElapsedMilliseconds;

            previousChannel = ChannelNumber;

            //DataTrigger_i++;
            m_modelTriggerDm.IncrementTrigger();

        }

        public void RunTest_NF_DUAL()
        {
            double[] testtest = new double[50];
            double NF_trigger_TestTime = 0;

            watch.Reset();
            watch.Start();

            //ENA.BasicCommand.SendCommand(NFGroup);
            //ENA.BasicCommand.SendCommand(":SYST:CHAN:NOIS:PAR:ENAB 1"); // NF Dual Band Grouping ON

            string trigger_dual = ENA.BasicCommand.System.ReadCommand(":INIT" + MasterChannel + ":IMM;*OPC? ; :INIT" + SlaveChannel + ":IMM;*OPC?");
            //ENA.BasicCommand.SendCommand(":SYST:CHAN:NOIS:PAR:ENAB 0"); // NF Dual Band Grouping OFF

            //string a = ENA.BasicCommand.ReadCommand(":SYST:CHAN:NOIS:PAR:STAT? 85");
            //string b = ENA.BasicCommand.ReadCommand(":SYST:CHAN:NOIS:PAR:STAT? 91");

            watch.Stop();
            NF_trigger_TestTime = watch.Elapsed.TotalMilliseconds;

            watch.Reset();
            watch.Start();

            Thread.Sleep(Sleep_ms);

            //FetchNFResult_DUAL(); //FetchResult by SCPI command by yoonchun
            m_equipment.MeasureResult_NF_DUAL(ChannelNumber, MasterChannel, SlaveChannel);

            long FbarTimeTemp1 = watch.ElapsedMilliseconds;

            previousChannel = ChannelNumber;

            //DataTrigger_i++;
            m_modelTriggerDm.IncrementTrigger();

        }
        public void InitializeTriggerData(DataTriggeredDataModel triggerDm)
        {
            m_modelTriggerDm = triggerDm;
        }

        public override void Clear_Results()
        {
            base.Clear_Results();
            m_modelTriggerDm.Reset();
        }

        private void MeasureResult2()
        {
            ENA.Format.DATA(e_FormatData.REAL32);
            ENA.Format.Border(e_Format.NORM);

            if (ChannelNumber == 0)
            {
                MeasureResult2Channel0();
                return;
            }

            int traceOffset = 0;
            if (b_EnhanceDataFetch)     // This is set to true always.
            {
                int cn = ChannelNumber - 1;
                MeasureResultEnhanceDataFetch(cn);
            }
            else
            {
                // This part will not execute.
                traceOffset = MeasureResult3();
            }
            previous_topazTraceOffset = topazTraceOffset;
            topazTraceOffset += traceOffset;
        }

        private void SaveTrace(int channelNumber)
        {
            if (channelNumber == 0)
            {
                SaveSnpFileChannel0();
                return;
            }

            if (b_EnhanceDataFetch)     // This is set to true always.
            {
                int cn = channelNumber - 1;
                SaveTraceFileEnhanceDataFetch(cn);
            }
            else
            {
                int cn = channelNumber - 1;
                int[] tm = TraceMatch[cn].TraceNumber;
                m_modelTraceManager.TigerModel.SaveSnpFile2(FileOutput_Sampling_Count, FileOutput_Counting,
                    FileOutput_Mode, cn,
                    m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[cn], tm);
            }
        }

        private void MeasureResultEnhanceDataFetch(int cn)
        {
            StringBuilder TraceNumberStr = new StringBuilder();
            StringBuilder TraceNumberStrOffset = new StringBuilder();

            // for (int iParam = 0; iParam < (SParamData[ChannelNumber - 1].NoPorts * SParamData[ChannelNumber - 1].NoPorts); iParam++)
            for (int iParam = 0; iParam < SParamData[cn].TotalTraceCount;
                iParam++) //The purpose of this loop is to go through all the traces of the channel
            {
                int Select_SParam_Def = TraceMatch[cn].SParam_Def_Number[iParam]; //SParam_Def_Number array contains the trace number of the Sxx as it was sucked in from the TCF
                if (Select_SParam_Def < 0) continue;
                int Select_SParam_Def_Arr = TraceMatch[cn].TraceNumber[Select_SParam_Def];
                bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                if (!isDefined) continue;

                TraceNumberStr.AppendFormat((string)"{0},", (object)Select_SParam_Def_Arr);
                TraceNumberStrOffset.AppendFormat("{0},",
                    Select_SParam_Def_Arr + 1 + topazTraceOffset); //What's with the +1+topazTraceOffset?
            }

            if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;

            try
            {
                //This line is dependent upon the Trace tab of the TCF.  If there is no entry in the trace tab for the channel being addressed, this will error out
                TopazEquipmentDriver.ReadBytes(cn);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "There is a discrepancy between the defined traces in the Trace tab of the TCF and conditions in the FBAR_Conditions tab of the TCF." +
                    "\r\n" +
                    "An Sxx is being called for Channel " + ChannelNumber + 1 +
                    " of the Network Analyzer that does not have a defined trace in the trace tab." + "\r\n" +
                    "Please inform the Product Engineer to have this fixed.");
                throw;
            }

            string traceNos = TraceNumberStr.ToString().Trim(',');
            try
            {
                if (traceNos.Length > 0) //TraceNumberStr is the trace number of interest for the measurement
                {
                    double[] readData = m_equipment.GetBytes(cn);
                    // readData = ENA.Calculate.Data.FMultiTrace_Data(ChannelNumber, TraceNumberStrOffset.ToString().Trim(','));   // SiteNumber
                    m_model2.TransferEnhanceData(readData, SParamData[cn], TraceMatch[cn], traceNos);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The error is " + ChannelNumber + " Test Number is " + TestNo.ToString());
                int x = 0;
                //throw;
            }

            if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;
        }

        private int MeasureResult3()
        {
            int traceOffset = 0;
            S_Param sp = SParamData[ChannelNumber - 1];

            for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
            {
                //Select_SParam_Def = SParam_Def2Value(iParam, ChannelNumber - 1);
                //Select_SParam_Def = SParam_Def2Value(iParam);
                int Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                if (Select_SParam_Def >= 0)
                {
                    int Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                    bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                    if (!isDefined) continue;

                    //watch.Reset();
                    //watch.Start();
                    int whatisthis = Select_SParam_Def_Arr + 1 + topazTraceOffset;
                    ENA.Calculate.Par.Select(ChannelNumber, whatisthis);
                    tmp_SFormat = ENA.Calculate.Format.Format(ChannelNumber, whatisthis);
                    int cn2 = ChannelNumber - 1;
                    TopazEquipmentDriver.ReadBytes(cn2);
                    double[] readData = m_equipment.GetBytes(cn2);

                    // readData = ENA.Calculate.Data.FData(ChannelNumber);
                    m_model2.TransferData(readData, sp.sParam_Data[Select_SParam_Def_Arr], tmp_SFormat);
                    //watch.Stop();
                    //General.DisplayError(ClassName, "Timming", "Elapsed Time = : " + watch.ElapsedMilliseconds.ToString());
                    traceOffset++;
                }
            }

            return traceOffset;
        }
        private void MeasureResult2Channel0()
        {
            for (int iChn = 0; iChn < SParamData.Length; iChn++)
            {
                S_Param sp = SParamData[iChn];
                int actualCn = iChn + 1;

                for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                {
                    //Select_SParam_Def = SParam_Def2Value(iParam, iChn);
                    //Select_SParam_Def = SParam_Def2Value(iParam);
                    int Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                    int Select_SParam_Def_Arr = TraceMatch[iChn].TraceNumber[Select_SParam_Def];
                    bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                    if (!isDefined) continue;

                    int spDefOffset = Select_SParam_Def_Arr + topazTraceOffset;
                    ENA.Calculate.Par.Select(actualCn, spDefOffset);
                    tmp_SFormat = ENA.Calculate.Format.Format(actualCn, spDefOffset);

                    int cn = ChannelNumber - 1;
                    TopazEquipmentDriver.ReadBytes(cn);
                    double[] readData = m_equipment.GetBytes(cn);
                    //TriggerModel.ReadBytes(mappedFileView[ChannelNumber - 1], offsets_for_ReadData[ChannelNumber - 1][0], 
                    //    (MemorySize[ChannelNumber - 1] / sizeof(float)), ReadData[ChannelNumber - 1]);
                    //readData = new double[ReadData[ChannelNumber - 1].Length];
                    //for (int i = 0; i < ReadData[ChannelNumber - 1].Length; i++)
                    //{
                    //    readData[i] = (double)ReadData[ChannelNumber - 1][i];
                    //}
                    //readData = ENA.Calculate.Data.FData(iChn + 1);
                    m_model2.TransferData(readData, sp.sParam_Data[Select_SParam_Def_Arr], tmp_SFormat);
                }
            }
        }
        public void MeasureResult()
        {
            double[] readData;
            int Select_SParam_Def;
            int Select_SParam_Def_Arr;
            int traceOffset = 0;
            ENA.Format.DATA(e_FormatData.REAL);
            ENA.Format.Border(e_Format.NORM);
            if (ChannelNumber == 0)
            {
                int ctTotalChannel = SParamData.Length;
                for (int iChn = 0; iChn < ctTotalChannel; iChn++)
                {
                    S_Param sp = SParamData[iChn];
                    for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                    {
                        //Select_SParam_Def = SParam_Def2Value(iParam, iChn);
                        //Select_SParam_Def = SParam_Def2Value(iParam);
                        Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                        Select_SParam_Def_Arr = TraceMatch[iChn].TraceNumber[Select_SParam_Def];
                        bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                        if (isDefined)
                        {
                            int spDefOffset = Select_SParam_Def_Arr + topazTraceOffset;
                            ENA.Calculate.Par.Select(iChn + 1, spDefOffset);
                            tmp_SFormat = ENA.Calculate.Format.Format(iChn + 1, spDefOffset);
                            readData = ENA.Calculate.Data.FData(iChn + 1);
                            m_model2.TransferData(readData,
                                sp.sParam_Data[Select_SParam_Def_Arr],
                                tmp_SFormat);
                            traceOffset++;
                        }
                    }
                    int[] tm = TraceMatch[iChn].TraceNumber;
                    m_modelTraceManager.TigerModel.SaveSnpFile(FileOutput_Sampling_Count, FileOutput_Counting,
                        FileOutput_Mode, (iChn),
                        m_modelTriggerDm.GetCurrentDataTrigger(), sp, tm);
                }
            }
            else
            {
                if (b_EnhanceDataFetch)
                {
                    #region "Enhance Data Transfer"

                    StringBuilder TraceNumberStr = new StringBuilder();
                    StringBuilder TraceNumberStrOffset = new StringBuilder();
                    //int iTraceCount = 0;
                    //if (!b_Captured)
                    //{
                    //TraceMatch[ChannelNumber].TotalTraces
                    S_Param sp = SParamData[ChannelNumber - 1];

                    for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                    {
                        Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];
                        if (Select_SParam_Def >= 0)
                        {
                            Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                            bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                            if (isDefined)
                            {
                                TraceNumberStr.AppendFormat("{0},", Select_SParam_Def_Arr + 1);
                                TraceNumberStrOffset.AppendFormat("{0},", Select_SParam_Def_Arr + 1 + topazTraceOffset);
                                if (!b_Captured) sp.sParam_Data[Select_SParam_Def_Arr].Format = 
                                    ENA.Calculate.Format.Format(ChannelNumber, Select_SParam_Def_Arr + 1 + topazTraceOffset);   // SiteNumber
                                //if (SParamData[ChannelNumber].sParam_Data[i].Format == e_SFormat.GDEL)
                                //{
                                //    if (traces[i] == "S21") traces[i] = "GDEL";
                                //    else traces[i] = "GDEL2";
                                //}
                                // iTraceCount++;
                            }
                            traceOffset++;
                        }
                    }
                    if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;
                    if (TraceNumberStr.ToString().Trim(',').Length > 0)
                    {
                        //  double a = ENA.Trigger.Read(ChannelNumber ,TraceNumberStr.ToString().Trim(','));
                        readData = ENA.Calculate.Data.FMultiTrace_Data(ChannelNumber, TraceNumberStrOffset.ToString().Trim(','));   // SiteNumber
                        m_model2.TransferEnhanceData(readData, SParamData[ChannelNumber - 1],
                            TraceMatch[ChannelNumber - 1], TraceNumberStr.ToString().Trim(','));
                    }

                    if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;

                    #endregion "Enhance Data Transfer"

                    int cn = ChannelNumber - 1;
                    int[] tm = TraceMatch[cn].TraceNumber;
                    m_modelTraceManager.TigerModel.SaveSnpFile2(FileOutput_Sampling_Count, FileOutput_Counting,
                        FileOutput_Mode, (cn),
                        m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[cn], tm);
                }
                else
                {
                    S_Param sp = SParamData[ChannelNumber - 1];

                    for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                    {
                        //Select_SParam_Def = SParam_Def2Value(iParam, ChannelNumber - 1);
                        //Select_SParam_Def = SParam_Def2Value(iParam);
                        Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                        if (Select_SParam_Def >= 0)
                        {
                            Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                            bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                            if (isDefined)
                            {
                                //watch.Reset();
                                //watch.Start();
                                ENA.Calculate.Par.Select(ChannelNumber, (Select_SParam_Def_Arr + 1) + topazTraceOffset);
                                tmp_SFormat = ENA.Calculate.Format.Format(ChannelNumber, (Select_SParam_Def_Arr + 1) + topazTraceOffset);
                                readData = ENA.Calculate.Data.FData(ChannelNumber);
                                m_model2.TransferData(readData,
                                    sp.sParam_Data[Select_SParam_Def_Arr],
                                    tmp_SFormat);
                                //watch.Stop();
                                //General.DisplayError(ClassName, "Timming", "Elapsed Time = : " + watch.ElapsedMilliseconds.ToString());
                                traceOffset++;
                            }
                        }
                    }

                    int cn = ChannelNumber - 1;
                    int[] tm = TraceMatch[cn].TraceNumber;
                    m_modelTraceManager.TigerModel.SaveSnpFile2(FileOutput_Sampling_Count, FileOutput_Counting,
                        FileOutput_Mode, cn,
                        m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[cn], tm);
                }
                previous_topazTraceOffset = topazTraceOffset;
                topazTraceOffset += traceOffset;
            }
        }

        private void TriggleSingle()
        {
//for (int i = 1; i < TotalChannel; i++)
            //{
            //    //ENA.Initiate.Immediate(i);
            //}
            ENA.Trigger.Single(ChannelNumber);

            ENA.BasicCommand.System.Operation_Complete();
        }

        private void SaveSnpFileChannel0()
        {
            for (int iChn = 0; iChn < SParamData.Length; iChn++)
            {
                S_Param sp = SParamData[iChn];
                int actualCn = iChn + 1;
                int[] tm = TraceMatch[iChn].TraceNumber;
                m_modelTraceManager.TigerModel.SaveSnpFile(FileOutput_Sampling_Count, FileOutput_Counting,
                    FileOutput_Mode, actualCn,
                    m_modelTriggerDm.GetCurrentDataTrigger(), sp, tm);
            }
        }

        private void SaveTraceFileEnhanceDataFetch(int cn)
        {
            int[] tm = TraceMatch[cn].TraceNumber;
            m_modelTraceManager.TigerModel.StoreCnTrace(Band, SwitchOut, FileOutput_Mode, cn, TestNo,
                m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[cn], tm);
            m_modelTraceManager.TigerModel.StoreTigerContent(FileOutput_Sampling_Count, FileOutput_Counting, Band,
                SwitchOut, FileOutput_Mode, cn, TestNo,
                m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[cn], tm);
        }

        //ChoonChin (20200114) - New void for save snp
        public void SaveSnpFileToENA()
        {
            bool isValidFilePath = !String.IsNullOrEmpty(TraceFileSnpSavePath);
            if (!isValidFilePath) return;

            int noOfTrace = QueryNumberOfTrace(ChannelNumber);

            ENA.Memory.Store.SNP.Data(ChannelNumber, noOfTrace,
                SParamData[ChannelNumber - 1].NoPorts, SnPFile_Name, TraceFileSnpSavePath);
            //ENA.Memory.Store.SNP.Data(ChannelNumber, topazTraceOffset + 1, SParamData[ChannelNumber - 1].NoPorts, SnPFile_Name, FileName); //Original
            //ENA.Memory.Store.SNP.Data(FileName);
            ENA.BasicCommand.System.Operation_Complete();
        }

        private void SaveSnpFileToENA(string snpFileName)
        {
            //TraceFileSnpSavePath
            bool isValidFilePath = !String.IsNullOrEmpty(snpFileName);
            if (!isValidFilePath) return;

            int noOfTrace = QueryNumberOfTrace(ChannelNumber);

            ENA.Memory.Store.SNP.Data(ChannelNumber, noOfTrace, 
                SParamData[ChannelNumber - 1].NoPorts, SnPFile_Name, snpFileName);
            //ENA.Memory.Store.SNP.Data(ChannelNumber, topazTraceOffset + 1, SParamData[ChannelNumber - 1].NoPorts, SnPFile_Name, FileName); //Original
            //ENA.Memory.Store.SNP.Data(FileName);
            ENA.BasicCommand.System.Operation_Complete();
        }

        private int QueryNumberOfTrace(int channelNumber)
        {
            string getChannelNo = string.Format("SYST:MEAS:CAT? {0}", channelNumber);
            string traceList = ENA.BasicCommand.ReadCommand(getChannelNo);
            string[] traceList_Array = traceList.Trim('\"', '\n').Split(',');
            int noOfTrace = Convert.ToInt32(traceList_Array[0]);
            return noOfTrace;
        }
    }

    public class cTrigger2 : TestCaseBase
    {
        #region "Declarations"

        private string SubClassName = "Trigger 2 Function Class";    // Sub Class Naming

        // External Variables
        private int ChannelNumber;

        private int Sleep_ms;
        private int TestNo;

        private string Misc_Settings;

        public string FileOutput_Path;
        public string FileOutput_FileName;
        private string FileOutput_Mode;
        public int FileOutput_Unit;
        public bool FileOutput_Enable;

        // Internal Variables
        private e_SFormat tmp_SFormat;

        private bool b_Trigger_Pause;
        public SParaFileManager m_model1;
        private TriggerModel m_model2;
        private DataTriggeredDataModel m_modelTriggerDm;
        private cENA ENA;

        #endregion "Declarations"

        public void InitSettings()
        {
            SaveResult.Enable = false;
            //ChannelNumber--;
            if (ChannelNumber == 0)
            {
                int ctTotalChannel = SParamData.Length;
                for (int i = 0; i < ctTotalChannel; i++)
                {
                    ENA.Initiate.Continuous(i, true);
                }
            }
            else
            {
                ENA.Initiate.Continuous(ChannelNumber, true);
            }
            if (Misc_Settings.Contains("P") || Misc_Settings.Contains("p"))
            {
                b_Trigger_Pause = true;
            }
            m_model2 = new TriggerModel();
        }

        public void RunTest()
        {
            ENA.Trigger.Single();
            ENA.BasicCommand.System.Operation_Complete();
            Thread.Sleep(Sleep_ms);
            MeasureResult();
            //DataTrigger_i++;

        }

        public void MeasureResult()
        {
            double[] readData;
            int Select_SParam_Def;
            int Select_SParam_Def_Arr;
            ENA.Format.DATA(e_FormatData.REAL);
            if (ChannelNumber == 0)
            {
                int ctTotalChannel = SParamData.Length;

                for (int iChn = 0; iChn < ctTotalChannel; iChn++)
                {
                    S_Param sp = SParamData[iChn];

                    for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                    {
                        Select_SParam_Def = m_model2.SParam_Def2Value(TraceMatch, iParam, iChn);
                        Select_SParam_Def_Arr = TraceMatch[iChn].TraceNumber[Select_SParam_Def];
                        bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                        if (isDefined)
                        {
                            ENA.Calculate.Par.Select(iChn + 1, Select_SParam_Def_Arr);
                            tmp_SFormat = ENA.Calculate.Format.Format(iChn + 1);
                            readData = ENA.Calculate.Data.FData(iChn + 1);
                            m_model2.TransferData(readData,
                                sp.sParam_Data[Select_SParam_Def_Arr],
                                tmp_SFormat);
                        }
                    }

                    int[] tm = TraceMatch[iChn].TraceNumber;
                    m_model1.TigerModel.SaveSnpFile(FileOutput_Mode, iChn + 1,
                        m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[iChn], tm);
                }
            }
            else
            {
                S_Param sp = SParamData[ChannelNumber - 1];

                for (int iParam = 0; iParam < (sp.NoPorts * sp.NoPorts); iParam++)
                {
                    Select_SParam_Def = m_model2.SParam_Def2Value(TraceMatch, iParam, ChannelNumber - 1);
                    Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                    bool isDefined = m_modelTriggerDm.IsCurrentSParameterActive(Select_SParam_Def);
                    if (isDefined)
                    {
                        ENA.Calculate.Par.Select(ChannelNumber, Select_SParam_Def_Arr);
                        tmp_SFormat = ENA.Calculate.Format.Format(ChannelNumber);
                        readData = ENA.Calculate.Data.FData(ChannelNumber);
                        m_model2.TransferData(readData,
                            sp.sParam_Data[Select_SParam_Def_Arr],
                            tmp_SFormat);
                    }
                }

                int[] tm = TraceMatch[ChannelNumber - 1].TraceNumber;
                m_model1.TigerModel.SaveSnpFile(FileOutput_Mode, ChannelNumber,
                    m_modelTriggerDm.GetCurrentDataTrigger(), SParamData[ChannelNumber], tm);
            }
        }

        public override void Clear_Results()
        {
            base.Clear_Results();
            m_modelTriggerDm.Reset();
        }
    }

}