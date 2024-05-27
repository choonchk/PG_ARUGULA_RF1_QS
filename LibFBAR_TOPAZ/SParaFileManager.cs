using System;
using System.Collections.Generic;
using Avago.ATF.StandardLibrary;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// Manage output file Trace and Tiger from S-Param measurement.
    /// </summary>
    public class SParaFileManager
    {
        public bool SNP_Sampling_Enabled;
        public int SNP_Sampling_Interval;
        public int tmpUnit_No;
        /// <summary>
        /// False on first run. True on 2nd run onwards. Used on the first run of a lot to save SNP file.
        /// </summary>
        public bool m_isNotFirstRun;
        //For new OQA bin trace saving
        public bool CnTracerEnable = false;

        //Tiger file
        public bool TigerTraceEnable
        {
            get { return TigerModel.TigerTraceEnable; }
        }

        /// <summary>
        /// CNTrace. For new OQA bin trace saving. Read by Test Plan to send the trace data to Clotho.
        /// </summary>
        public List<string> TraceCollection;

        //Tiger file. Read by Test Plan to save the trace data to Clotho.
        public s_Result cAlgoTigerTraceString;
        public TigerFileManager TigerModel;
        private s_SNPFile SNPFile;

        public SParaFileManager()
        {
            tmpUnit_No = 1;
            SNP_Sampling_Interval = 10;
            SNP_Sampling_Enabled = false;
            TraceCollection = new List<string>();
            cAlgoTigerTraceString = new s_Result();
            cAlgoTigerTraceString.TraceFile = new List<string>();
            m_isNotFirstRun = false;

            TigerModel = new TigerFileManager();

        }

        public void DisableSnpFile()
        {
            m_isNotFirstRun = true;
        }

        public void SetTraceResult()
        {
            TraceCollection = TigerModel.TraceSaveResult;
            cAlgoTigerTraceString = TigerModel.TigerTraceResult;
        }

        public void Clear_Results()
        {
            TigerModel.Clear_Results();
        }

        // Called in ATF Init to init default values.
        public void InitializeSnpFile(s_SNPFile initialSnpFile)
        {
            SNPFile = initialSnpFile;
            //LibFbar.SNPFile.ENASNPFileOutput_Enable = tcfData.EnaStateFileEnable;
            //LibFbar.SNPFile.FileOutput_Enable = tcfData.TraceFileEnable;
            //LibFbar.SNPFile.FileOutput_Path = m_modelTpProd.ActiveDirectory;
            //LibFbar.SNPFile.SNPFileOutput_Path = m_modelTpProd.SNP_Files_Dir;
            //LibFbar.SNPFile.FileOutput_FileName = "FEM";
            //LibFbar.SNPFile.FileOuuput_Count = tcfData.TraceFileOutput_Count;
        }

        // Called in DoAtfTest - Pretest.
        public void SetSnpFile(string fn, string filePath, List<string> headerName)
        {
            SNPFile.FileOutput_FileName = fn;
            SNPFile.FileOutput_Path = filePath;
            SNPFile.FileOutput_HeaderName = headerName;
            //LibFbar.SNPFile.FileOutput_FileName = snpFile.FileOutput_FileName;

            ////Parse information to LibFbar
            //LibFbar.SNPFile.FileOutput_Path = snpFile.FileOutput_Path;

            ////Generate SNP Header file
            //LibFbar.SNPFile.FileOutput_HeaderName = snpFile.FileOutput_HeaderName;
        }

        public void SetSnpFile(string fn, string filePath, List<string> headerName, string fnSnp)
        {
            SNPFile.FileOutput_FileName = fn;
            SNPFile.FileOutput_Path = filePath;
            SNPFile.FileOutput_HeaderName = headerName;
            SNPFile.SNPFileOutput_Path = fnSnp;
            //LibFbar.SNPFile.FileOutput_FileName = snpFile.FileOutput_FileName;

            ////Parse information to LibFbar
            //LibFbar.SNPFile.FileOutput_Path = snpFile.FileOutput_Path;

            ////Generate SNP Header file
            //LibFbar.SNPFile.FileOutput_HeaderName = snpFile.FileOutput_HeaderName;
        }

        // Initialize output file path before each Trigger.RunTest().
        public void SetTriggerOnFirstRun(int testIndex, SparamTrigger testTrig)
        {
            if (m_isNotFirstRun) return;

            //testTrig.FileOutput_Enable = SNPFile.FileOutput_Enable;
            if (!SNPFile.FileOutput_Enable) return;
            string SNPpath;
            if (testTrig.FileOutput_Mode != "")
            {
                ////This is for the trace data
                ////Modified by CheeOn 22-June-2018 **************************************
                //if ((testTrig.TunableBand.Contains("MIMO")) || (testTrig.TunableBand.Contains("HBAUX")))   //Modified by CheeOn 22-June-2018
                //    NewPath = System.IO.Path.Combine(SNPFile.FileOutput_Path, Test + "_" + testTrig.TunableBand + "_" + testTrig.Select_RX + "_" + testTrig.FileOutput_Mode + "_CH" + testTrig.ChannelNumber + "\\");
                //else
                //    NewPath = System.IO.Path.Combine(SNPFile.FileOutput_Path, Test + "_" + testTrig.TunableBand + "_" + testTrig.Switch_Ant_val + "_" + testTrig.Select_RX + "_" + testTrig.FileOutput_Mode + "_CH" + testTrig.ChannelNumber + "\\");
                ////***


                SNPpath = SNPFile
                    .SNPFileOutput_Path; // System.IO.Path.Combine(SNPFile.SNPFileOutput_Path, string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now) + "\\");
            }
            else
            {
                SNPpath = SNPFile.SNPFileOutput_Path;
            }

            SNPFile.FileOutput_FileName =
                ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "LOT-ID");
            testTrig.SNPFileOutput_Path = SNPpath;
            //testTrig.FileOutput_Mode = "";

            testTrig.ENASNPFileOutput_Enable = SNPFile.ENASNPFileOutput_Enable;
            //testTrig.FileOutput_Sampling_Count = 0; //Added by MM to reset the sampling count
            //testTrig.SNPFileOutput_Sampling_Count = 0;
            //testTrig.FileOutput_Counting = 0;
            SetTriggerOnFirstRunTiger(TigerModel, SNPFile, testTrig, testIndex);
            
            

        }

        private void SetTriggerOnFirstRunTiger(TigerFileManager model, s_SNPFile snpFile,
            SparamTrigger testTrig, int Test)
        {
            string newPath;
            if (testTrig.FileOutput_Mode != "")
            {
                ////This is for the trace data
                ////Modified by CheeOn 22-June-2018 **************************************
                //if ((testTrig.TunableBand.Contains("MIMO")) || (testTrig.TunableBand.Contains("HBAUX")))   //Modified by CheeOn 22-June-2018
                //    NewPath = System.IO.Path.Combine(SNPFile.FileOutput_Path, Test + "_" + testTrig.TunableBand + "_" + testTrig.Select_RX + "_" + testTrig.FileOutput_Mode + "_CH" + testTrig.ChannelNumber + "\\");
                //else
                //    NewPath = System.IO.Path.Combine(SNPFile.FileOutput_Path, Test + "_" + testTrig.TunableBand + "_" + testTrig.Switch_Ant_val + "_" + testTrig.Select_RX + "_" + testTrig.FileOutput_Mode + "_CH" + testTrig.ChannelNumber + "\\");
                ////***


                string fp = string.Format("{0}_{1}_CH{2}\\", Test, testTrig.FileOutput_Mode,
                    testTrig.ChannelNumber);
                newPath = System.IO.Path.Combine(snpFile.FileOutput_Path, fp);
            }
            else
            {
                // Case Joker.
                //NewPath = SNPFile.FileOutput_Path;
                // Case HLS2 : use the Lot ID.
                newPath = string.Format("{0}{1}_{2}\\", snpFile.SNPFileOutput_Path, ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "LOT-ID"),
                    ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "SUBLOT-ID"));
            }

            bool testbool = System.IO.Directory.Exists(newPath);
            model.FileOutput_FileName = string.Format("{0}_{1}_", snpFile.FileOutput_FileName,
                string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now));
            //testTrig.FileOutput_FileName2 = SNPFile.FileOutput_FileName + "2_" + string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now) + "_";
            //SNPFile.FileOutput_FileName = testTrig.FileOutput_FileName;

            ////ChoonChin - Disable for tiger file
            //if (!System.IO.Directory.Exists(NewPath))
            //{
            //    System.IO.Directory.CreateDirectory(NewPath);
            //    Generate_SNP_Header(NewPath + SNPFile.FileOutput_FileName + ".txt", SNPFile.FileOutput_HeaderName);
            //    //SNPFile.FileOutput_Path = NewPath;
            //    SNPFile.FileOutput_HeaderCount++;
            //    TempFolderName.Add(NewPath);
            //}
            model.FileOutput_Unit = tmpUnit_No;
            model.Initialize(snpFile.FileOutput_Enable, snpFile.FileOuuput_Count, snpFile.SNPFileOuuput_Count, snpFile.FileOutput_Path); //ChoonChin (20200114) - Change output folder
            model.CnTracerEnable = CnTracerEnable;            
        }

        public void IncrementFileCount(SparamTrigger testTrig)
        {
            //ChoonChin - Add sampling trace file
            if (SNP_Sampling_Enabled && SNPFile.FileOutput_Enable)
            {
                if (++testTrig.FileOutput_Sampling_Count >= SNP_Sampling_Interval)
                {
                    testTrig.FileOutput_Sampling_Count = 0;
                    testTrig.FileOutput_Counting++;
                }
                //testTrig.FileOutput_Counting++;
            }
            else
            {
                testTrig.FileOutput_Sampling_Count = 0;
                testTrig.FileOutput_Counting++;
            }

            if ((SNP_Sampling_Interval > 0) && SNPFile.ENASNPFileOutput_Enable) //ChoonCHin (20191216) -Fix sampling not working issue.
            {
                if (++testTrig.SNPFileOutput_Sampling_Count >= SNP_Sampling_Interval)
                {
                    testTrig.SNPFileOutput_Sampling_Count = 0;
                    testTrig.SNPFileOutput_Counting++;
                }
                //testTrig.FileOutput_Counting++;
            }
            else
            {
                testTrig.SNPFileOutput_Sampling_Count = 0;
                testTrig.SNPFileOutput_Counting++;
            }

            //if (++testTrig.SNPFileOutput_Sampling_Count >= SNP_Sampling_Interval) //Jerome Trace_Sampling_Interval
            //{
            //    testTrig.SNPFileOutput_Sampling_Count = 0;
            //    testTrig.SNPFileOutput_Counting++;
            //}
        }

        public void IncrementFileCount2(SparamTrigger testTrig)
        {
            testTrig.FileOutput_Counting++;
        }

        public List<string> GetContentTiger()
        {
            if (TigerTraceEnable) return cAlgoTigerTraceString.TigerTraceFile;
            return new List<string>();
        }

        public List<string> GetContentCnTrace()
        {
            if (CnTracerEnable) return TraceCollection;
            return new List<string>();
        }

        public void GenerateWaveData(s_SNPFile snpFile, bool isSamplingEnabled, int samplingInterval)
        {
            if (String.IsNullOrEmpty(snpFile.FileOutput_FileName)) return;

            SetSnpFile(snpFile.FileOutput_FileName,
                snpFile.FileOutput_Path, snpFile.FileOutput_HeaderName);
            SNP_Sampling_Enabled = isSamplingEnabled;
            SNP_Sampling_Interval = samplingInterval;
        }

    }

    public class TigerFileManager
    {
        public string FileOutput_Path;
        public string FileOutput_FileName;
        public int FileOutput_Unit;
        public bool FileOutput_Enable;
        /// <summary>
        /// Max number of file to generate.
        /// </summary>
        public int FileOutput_Count;
        public int SNPFileOutput_Count;
        //For new OQA bin trace saving
        public bool CnTracerEnable = false;

        //Tiger file
        public bool TigerTraceEnable;

        //For new OQA bin trace saving. Read by Test Plan to save the trace data to Clotho.
        public List<string> TraceSaveResult;

        //Tiger file. Read by Test Plan to save the trace data to Clotho.
        public s_Result TigerTraceResult;

        public TigerFileManager()
        {
            //For new OQA bin trace saving
            TraceSaveResult = new List<string>();

            //Tiger file
            TigerTraceResult = new s_Result();
            TigerTraceResult.TigerTraceFile = new List<string>();
            TigerTraceEnable = false;
        }

        // SNPFile.FileOutput_Enable, SNPFile.FileOuuput_Count, NewPath
        public void Initialize(bool isEnabled, int foCount,int foSNPCount, string foPath)
        {
            FileOutput_Enable = isEnabled;
            FileOutput_Count = foCount;
            SNPFileOutput_Count = foSNPCount;
            FileOutput_Path = foPath;
        }

        public void Clear_Results()
        {
            TraceSaveResult.Clear();
            TigerTraceResult.TigerTraceFile.Clear();
        }

        private bool IsHasNotReachedMaxFileGenerationCount(int currentFileCount)
        {
            bool isReachedMaxFileCount = FileOutput_Count == 999 || currentFileCount >= FileOutput_Count; //ChoonChin - 20191204 - Do not need to minus 1
            return !isReachedMaxFileCount;
        }

        private bool IsToGenerateFile(int currentFileSamplingCount, int currentFileCount)
        {
            bool isNotMaxCount = IsHasNotReachedMaxFileGenerationCount(currentFileCount);
            bool isToGenerateFile = FileOutput_Enable && (currentFileSamplingCount == 0) && isNotMaxCount;
            return isToGenerateFile;
        }

        public void SaveSnpFile(int foSamplingCount, int foCounting, string tcfPowerMode,
            int iChn,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            bool isGenerate = IsToGenerateFile(foSamplingCount, foCounting);
            if (isGenerate)
                //if (FileOutput_Enable && ((FileOutput_Count == 999) || (FileOutput_Counting <= FileOutput_Count - 1)))
            {
                if (tcfPowerMode != "")
                {
                    SaveFile2SNPDPort(FileOutput_Path, FileOutput_FileName,
                        tcfPowerMode, FileOutput_Unit.ToString(), iChn,
                        currentTd, sp, tm);
                }
                else
                {
                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName,
                        FileOutput_Unit.ToString(), iChn,
                        currentTd, sp, tm);
                }
            }
        }

        // Called by Trigger.MeasureResult to save SNP.
        public void SaveSnpFile2(int foSamplingCount, int foCounting, string tcfPowerMode,
            int iChn, s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            int currentSn = 0;
            string current_pid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0");
            Int32.TryParse(current_pid, out currentSn);

            string sn = FileOutput_Unit.ToString();
            if (currentSn != 0)
            {
                sn = currentSn.ToString();
            }
            bool isGenerate = IsToGenerateFile(foSamplingCount, foCounting);
            if (isGenerate)
                //if (FileOutput_Enable && ((FileOutput_Count == 999) || (FileOutput_Counting <= FileOutput_Count - 1)))
            {
                if (tcfPowerMode != "")
                {
                    SaveFile2SNPDPort(FileOutput_Path, FileOutput_FileName,
                        tcfPowerMode, sn, (iChn + 1),
                        currentTd, sp, tm); //file generated by this method has syntax errors for sonnet lite.  

                }
                else
                {
                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName,
                        sn, (iChn + 1),
                        currentTd, sp, tm); //file generated by this method as well has syntax errors for sonnet lite.
                }
            }
        }

        public void StoreCnTrace(string tcfBand,
            string tcfSwitchOut, string tcfPowerMode,
            int iChnZeroIndexed, int testNb,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)

        {
            if (CnTracerEnable)
            {
                SaveFile2SNP_CnTracer(FileOutput_Path, FileOutput_FileName,
                    tcfPowerMode, iChnZeroIndexed, testNb,
                    tcfBand, tcfSwitchOut, currentTd, sp, tm);
            }
        }

        // Called by Trigger.MeasureResult2 to save Tiger.
        public void StoreTigerContent(int foSamplingCount, int foCounting, string tcfBand,
            string tcfSwitchOut, string tcfPowerMode, int iChnZeroIndexed, int testNb,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)

        {
            TigerTraceEnable = false;
            bool isGenerate = IsToGenerateFile(foSamplingCount, foCounting);
            if (!isGenerate) return;

            bool isNotReachedMaxFileCount = IsHasNotReachedMaxFileGenerationCount(foCounting);
            if ((!FileOutput_Enable) || !isNotReachedMaxFileCount) return;

            

            if (tcfPowerMode != "")
            {
                string headerPowerMode = GetMode(tcfPowerMode);
                string header = string.Format("CH{0}_{1}_{2}_OUT-{3}_TN{4}", (iChnZeroIndexed + 1).ToString(), tcfBand, headerPowerMode, tcfSwitchOut, testNb); //Choon Chin - 20191204 - Chn cannot start with 0

                ////KCC - Changed unit to +1 for Clotho
                //SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber, TestNo);

                //ChoonChin - Changed to cntrace format (.tiger)
                TigerTraceEnable = true;
                List<string> content = GenerateTigerFileContent(header, currentTd, sp, tm);
                TigerTraceResult.TigerTraceFile.AddRange(content);
            }
            else
            {
                //KCC - Changed unit to +1 for Clotho
                //   SaveFile2SNP(FileOutput_Path, FileOutput_FileName, CurrentSN.ToString(), ChannelNumber);
            }
            //T3 = Speed.Elapsed.TotalMilliseconds;
            //if (FileOutput_Mode != "")
            //{
            //    if(!SecondTest) //KCC - Changed unit to +1 for Clotho
            //        SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber);
            //    else
            //        SaveFile2SNP(FileOutput_Path, FileOutput_FileName2, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber);
            //}
            //else
            //{
            //    //KCC - Changed unit to +1 for Clotho
            //    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, (FileOutput_Unit + 1).ToString(), ChannelNumber);
            //}
        }

        public void StoreTigerContent(int foSamplingCount, int foCounting, string tcfBand,
            string tcfSwitchOut, string tcfPowerMode, int iChnZeroIndexed, int testNb,
            s_SParam_Grab currentTd, S_Param sp, int[] tm, string snpfilepath)

        {
            TigerTraceEnable = false;
            bool isGenerate = IsToGenerateFile(foSamplingCount, foCounting);
            if (!isGenerate) return;

            bool isNotReachedMaxFileCount = IsHasNotReachedMaxFileGenerationCount(foCounting);
            if ((!FileOutput_Enable) || !isNotReachedMaxFileCount) return;

            if (tcfPowerMode != "")
            {
                string headerPowerMode = GetMode(tcfPowerMode);
                string header = string.Format("CH{0}_{1}_{2}_OUT-{3}_TN{4}", (iChnZeroIndexed + 1).ToString(), tcfBand, headerPowerMode, tcfSwitchOut, testNb); //Choon Chin - 20191204 - Chn cannot start with 0

                ////KCC - Changed unit to +1 for Clotho
                //SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber, TestNo);

                //ChoonChin - Changed to cntrace format (.tiger)
                TigerTraceEnable = true;
                List<string> content = GenerateTigerFileContent(header, currentTd, sp, tm);
                TigerTraceResult.TigerTraceFile.AddRange(content);
            }
            else
            {
                //KCC - Changed unit to +1 for Clotho
                //   SaveFile2SNP(FileOutput_Path, FileOutput_FileName, CurrentSN.ToString(), ChannelNumber);
            }
            //T3 = Speed.Elapsed.TotalMilliseconds;
            //if (FileOutput_Mode != "")
            //{
            //    if(!SecondTest) //KCC - Changed unit to +1 for Clotho
            //        SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber);
            //    else
            //        SaveFile2SNP(FileOutput_Path, FileOutput_FileName2, FileOutput_Mode, CurrentSN.ToString(), ChannelNumber);
            //}
            //else
            //{
            //    //KCC - Changed unit to +1 for Clotho
            //    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, (FileOutput_Unit + 1).ToString(), ChannelNumber);
            //}
        }

        public void SaveSnpFile(string tcfPowerMode, int iChn,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            if (FileOutput_Enable)
            {
                if (tcfPowerMode != "")
                {
                    SaveFile2SNPDPort(FileOutput_Path, FileOutput_FileName,
                        tcfPowerMode, FileOutput_Unit.ToString(), (iChn + 1),
                        currentTd, sp, tm);
                }
                else
                {
                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName,
                        FileOutput_Unit.ToString(), (iChn + 1),
                        currentTd, sp, tm);
                }
            }
        }

        public bool IsSaveSnp1(int foSamplingCount, int foCounting, string tcfSearchMethod)
        {
            bool isGenerate = IsToGenerateFile(foSamplingCount, foCounting);
            if (isGenerate)
            {
                if (tcfSearchMethod != "")
                {
                    return true;
                }
            }

            return false;
        }

        public string FormSnpOutputFileName(string fnPostFix)
        {
            int CurrentSN = 0;
            string current_pid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0");
            int.TryParse(current_pid, out CurrentSN);
            //ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "LOT-ID");
            //ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "SUBLOT-ID");
            //FileName = TestNo + "_" + FileOutput_FileName + "SN" + CurrentSN + "_" + FileOutput_Mode + "_CH" + ChannelNumber;


            System.IO.Directory.CreateDirectory(FileOutput_Path);

            //FileName = znowonder
            //    + "CH" + ChannelNumber + "_" + TunableBand + "_" + Select_RX+"_"+ FileOutput_Mode +"_"+ FileOutput_FileName + "SN" + CurrentSN;

            string FileName = string.Format("{0}{1}_{2}SN{3}", FileOutput_Path, fnPostFix, FileOutput_FileName, CurrentSN);
            //FileName = DataTrigger_i
            //    + "_CH" + ChannelNumber + "_" + Band + "_" + FileOutput_Mode + "_" + SwitchIn + "_" + SwitchAnt + "_" + SwitchOut + "_SN" + CurrentSN;

            //FileName = SNP_FormedFileName+ "SN" + CurrentSN + "_" + FileOutput_Mode; 
            //FileOutput_FileName is "LOT-ID_20180324_0250_"
            //FileOutput_Mode is the Rx Power Mode like G0
            //You add the directory to teh FileName
            //CALC:MEAS2:DATA:SNP:PORTs:Save '1,2,4','D:\MyData.s3p';*OPC?
            return FileName;
        }

        private void SaveFile2SNP(string FolderName, string FileName, string Unit, int iChn,
s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            string[] OutputData;
            string OutputFileName;
            string tmpStr;
            e_SParametersDef tmpSParamDef;

            OutputFileName = FolderName + FileName;
            OutputData = new string[sp.Freq.Length + 3];
            OutputData[0] = "#\tHZ\tS\tMagPhase\tR50.0";
            OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();
            switch (sp.NoPorts)
            {
                case 2:
                    OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S21\t\t" + "S22\t\t";
                    break;
                case 3:
                    OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t";
                    break;
                case 4:
                    OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S14\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S24\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t" + "S34\t\t" + "S41\t\t" + "S42\t\t" + "S43\t\t" + "S44\t\t";
                    break;
            }
            for (int iPts = 0; iPts < sp.Freq.Length; iPts++)
            {
                OutputData[iPts + 3] = sp.Freq[iPts].ToString();
                for (int X = 1; X < (sp.NoPorts + 1); X++)
                {
                    for (int Y = 1; Y < (sp.NoPorts + 1); Y++)
                    {
                        tmpStr = "S" + X.ToString() + Y.ToString();
                        tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);

                        if (currentTd.SParam_Grab[tmpSParamDef.GetHashCode()])
                        {
                            int tm2 = tm[tmpSParamDef.GetHashCode()];
                            OutputData[iPts + 3] += "\t" + sp.sParam_Data[tm2].sParam[iPts].dBAng.dB
                                                    + "\t" + sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle;
                        }
                        else
                        {
                            OutputData[iPts + 3] += "\t0\t0";
                        }
                    }
                }
            }

            string fn = string.Format("{0}_Channel{1}_Unit{2}.s{3}p", OutputFileName, (iChn + 1), Unit, sp.NoPorts);
            System.IO.File.WriteAllLines(fn, OutputData);
        }

        //KCC: Added d port
        private void SaveFile2SNPDPort(string FolderName, string FileName, string Mode, string Unit, int iChn,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            string[] OutputData;
            string OutputFileName;
            string tmpStr;
            e_SParametersDef tmpSParamDef;

            OutputFileName = FolderName + FileName;
            OutputData = new string[sp.Freq.Length + 3];
            OutputData[0] = "#\tHZ\tS\tMagPhase\tR50.0";
            OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();

            e_SParametersDef[] GetSpara = (e_SParametersDef[])Enum.GetValues(typeof(e_SParametersDef));
            for (int i = 0; i < GetSpara.Length - 1; i++)
            {
                if (i == 0) { OutputData[2] = "Freq\t"; }
                OutputData[2] += GetSpara[i] + "\t\t";
            }

            for (int iPts = 0; iPts < sp.Freq.Length; iPts++)
            {

                OutputData[iPts + 3] = (Math.Round(sp.Freq[iPts])).ToString();

                for (int i = 0; i < GetSpara.Length - 1; i++)
                {

                    tmpStr = Convert.ToString(GetSpara[i]);

                    try
                    {
                        tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);

                        bool isDef = currentTd.IsDef(tmpSParamDef);
                        if (isDef)
                        {
                            int tm2 = tm[tmpSParamDef.GetHashCode()];
                            //ChoonChin - To reduce number of decimal point
                            if (tmpStr.Contains("GDEL")) //Except for GDEL meas
                            {
                                OutputData[iPts + 3] += "\t" + sp.sParam_Data[tm2].sParam[iPts].dBAng.dB
                                                        + "\t" + sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle;
                            }
                            else
                            {
                                OutputData[iPts + 3] += "\t" + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.dB, 3)
                                                        + "\t" + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle, 3);
                            }

                            //Original code
                            //OutputData[iPts + 3] += "\t" + sp.sParam_Data[DataInfo].sParam[iPts].dBAng.dB
                            //                        + "\t" + sp.sParam_Data[DataInfo].sParam[iPts].dBAng.Angle;
                        }
                        else
                        {
                            OutputData[iPts + 3] += "\t0\t0";
                        }
                    }
                    catch { }
                }

            }
            //   int a = TestNo;
            int actualChannelNo = iChn + 1;
            string fn = string.Format("{0}_CHAN{1}_{2}_Unit{3}.s4pd", OutputFileName, actualChannelNo, Mode, Unit);
            System.IO.File.WriteAllLines(fn, OutputData);
        }

        // Called by Trigger.MeasureResult to save CN Trace.
        //ChoonChin - For new OQA bin trace saving
        private void SaveFile2SNP_CnTracer(string FolderName, string FileName, string Mode,
            int iChn, int Testnb, string Band, string SelectedPort,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            string[] OutputData;
            string OutputFileName;
            string tmpStr;
            e_SParametersDef tmpSParamDef;
            int DataInfo;

            OutputFileName = FolderName + FileName;
            OutputData = new string[sp.Freq.Length + 3];
            OutputData[0] = "#,HZ,S,MagPhase,R50.0";
            OutputData[1] = "!," + DateTime.Now.ToShortDateString() + "," + DateTime.Now.ToLongTimeString();

            e_SParametersDef[] GetSpara = (e_SParametersDef[])Enum.GetValues(typeof(e_SParametersDef));
            for (int i = 0; i < GetSpara.Length - 1; i++)
            {
                if (i == 0) { OutputData[2] = "Freq,"; }

                if (GetSpara[i].ToString().Contains("GDEL"))
                {
                    OutputData[2] += GetSpara[i] + "_S," + GetSpara[i] + "_None,";
                }
                else
                {
                    OutputData[2] += GetSpara[i] + "_Mag," + GetSpara[i] + "_Phase,";
                }
            }

            //For new OQA bin trace saving
            //Modify for each product
            List<string> ModeList = new List<string>();
            ModeList.Add("G0-H");
            ModeList.Add("G0-L");
            ModeList.Add("G1");
            ModeList.Add("G2");
            ModeList.Add("G3");
            ModeList.Add("G4");
            ModeList.Add("G5");
            ModeList.Add("HPHA");
            ModeList.Add("x");

            foreach (string PowMode in ModeList)
            {
                if (Mode.Contains(PowMode))
                {
                    Mode = PowMode;
                    break;
                }
            }

            if (Mode.Contains("HPHA"))
            {
                Mode = "HP";
            }

            if (Mode.Contains("X"))
            {
                Mode = "HPM";
            }

            string Header = string.Format("CH{0}_{1}_{2}_OUT-{3}_TN{4}", (iChn + 1).ToString(), Band, Mode.Replace(" ", string.Empty), SelectedPort, Testnb); //ChoonChin - 20191205 - Channel plus 1
            TraceSaveResult.Add(Header);
            TraceSaveResult.Add(OutputData[2]);

            for (int iPts = 0; iPts < sp.Freq.Length; iPts++)
            {
                OutputData[iPts + 3] = (Math.Round(sp.Freq[iPts])).ToString();

                for (int i = 0; i < GetSpara.Length - 1; i++)
                {
                    tmpStr = Convert.ToString(GetSpara[i]);
                    try
                    {
                        tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                        bool isDef = currentTd.IsDef(tmpSParamDef);
                        if (isDef)
                        {
                            int tm2 = tm[tmpSParamDef.GetHashCode()];

                            //ChoonChin - To reduce number of decimal point
                            if (tmpStr.Contains("GDEL")) //Except for GDEL meas
                            {
                                OutputData[iPts + 3] += "," + sp.sParam_Data[tm2].sParam[iPts].dBAng.dB
                                                        + "," + sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle;
                            }
                            else
                            {
                                OutputData[iPts + 3] += "," + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.dB, 3)
                                                        + "," + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle, 3);
                            }
                        }
                        else
                        {
                            OutputData[iPts + 3] += ",0,0";
                        }
                    }
                    catch { }
                }

                //For new OQA bin trace saving
                TraceSaveResult.Add(OutputData[iPts + 3]);
            }
        }

        //ChoonChin - Replaced .actraceo -> Tiger file
        private List<string> GenerateTigerFileContent(string fileHeader,
            s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            List<string> content = new List<string>();
            List<string> OutputData = GenerateTigerHeader();
            content.Add(fileHeader);
            content.Add(OutputData[2]);

            List<string> spContent = GenerateTigerContent(currentTd, sp, tm);
            content.AddRange(spContent);
            return content;
        }

        private static List<string> GenerateTigerHeader()
        {
            List<string> header = new List<string>();
            string h1 = "#,HZ,S,MagPhase,R50.0";
            string h2 = "!," + DateTime.Now.ToShortDateString() + "," + DateTime.Now.ToLongTimeString();
            string h3 = "";

            e_SParametersDef[] GetSpara = (e_SParametersDef[]) Enum.GetValues(typeof(e_SParametersDef));
            for (int i = 0; i < GetSpara.Length - 1; i++)
            {
                if (i == 0)
                {
                    h3 = "Freq,";
                }

                if (GetSpara[i].ToString().Contains("GDEL"))
                {
                    h3 += GetSpara[i] + "_S," + GetSpara[i] + "_None,";
                }
                else
                {
                    h3 += GetSpara[i] + "_Mag," + GetSpara[i] + "_Phase,";
                }
            }

            header.Add(h1);
            header.Add(h2);
            header.Add(h3);

            return header;
        }

        private List<string> GenerateTigerContent(s_SParam_Grab currentTd, S_Param sp, int[] tm)
        {
            e_SParametersDef[] GetSpara = (e_SParametersDef[])Enum.GetValues(typeof(e_SParametersDef));
            List<string> tigerContent = new List<string>();

            for (int iPts = 0; iPts < sp.Freq.Length; iPts++)
            {
                string tigerLine = (Math.Round(sp.Freq[iPts])).ToString();

                for (int i = 0; i < GetSpara.Length - 1; i++)
                {
                    var tmpStr = Convert.ToString(GetSpara[i]);
                    try
                    {
                        var tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                        bool isDef = currentTd.IsDef(tmpSParamDef);

                        if (isDef)
                        {
                            int tm2 = tm[tmpSParamDef.GetHashCode()];

                            //ChoonChin - To reduce number of decimal point
                            if (tmpStr.Contains("GDEL")) //Except for GDEL meas
                            {
                                tigerLine += "," + sp.sParam_Data[tm2].sParam[iPts].dBAng.dB
                                                            + "," + sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle;
                            }
                            else
                            {
                                tigerLine += "," + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.dB, 3)
                                                            + "," + Math.Round(sp.sParam_Data[tm2].sParam[iPts].dBAng.Angle, 3);
                            }
                        }
                        else
                        {
                            tigerLine += ",0,0";
                        }
                    }
                    catch { }
                }

                //For new OQA bin trace saving
                tigerContent.Add(tigerLine);
            }

            return tigerContent;
        }

        private static string GetMode(string Mode)
        {
            List<string> ModeList = new List<string>();
            ModeList.Add("G0-H");
            ModeList.Add("G0-L");
            ModeList.Add("G1");
            ModeList.Add("G2");
            ModeList.Add("G3");
            ModeList.Add("G4");
            ModeList.Add("G5");
            ModeList.Add("HPHA");
            ModeList.Add("x");

            foreach (string PowMode in ModeList)
            {
                if (Mode.Contains(PowMode))
                {
                    Mode = PowMode;
                    break;
                }
            }

            if (Mode.Contains("HPHA"))
            {
                Mode = "HP";
            }

            if (Mode.Contains("X"))
            {
                Mode = "HPM";
            }

            Mode = Mode.Replace(" ", string.Empty);
            return Mode;
        }
    }
}