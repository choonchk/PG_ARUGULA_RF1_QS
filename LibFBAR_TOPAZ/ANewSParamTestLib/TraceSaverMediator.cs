using System;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;

namespace LibFBAR_TOPAZ.ANewSParamTestLib
{
    public class TraceSaverMediator
    {
        private SParaFileManager FileManager { get; }
        private SParameterMeasurementDataModel m_dm1;
        private DataTriggeredDataModel m_dm2;
        private SparamTrigger m_tt;

        private cENA VNA;

        public TraceSaverMediator(SparamTrigger tt, SParaFileManager fm,
            SParameterMeasurementDataModel dm1, DataTriggeredDataModel dm2)
        {
            m_tt = tt;
            FileManager = fm;
            m_dm1 = dm1;
            m_dm2 = dm2;
        }

        public string GetSnpOutputFilePath()
        {
            string snpOutputFilePath = String.Empty;

            //bool isSaveSnp = FileManager.TigerModel.IsSaveSnp1(m_tt.FileOutput_Sampling_Count,
            //    m_tt.FileOutput_Count, m_tt.SnPFile_Name);

            //ChoonChin - 20191122 - SNP is not tiger
            bool isSaveSnp = FileManager.SNP_Sampling_Enabled;
            //Jerome - 20191203 - To generate Snp separatley from Trace file
            isSaveSnp = m_tt.ENASNPFileOutput_Enable;
            isSaveSnp &= (m_tt.SNPFileOutput_Counting < FileManager.TigerModel.SNPFileOutput_Count ? true : false);
            isSaveSnp &= (m_tt.SNPFileOutput_Sampling_Count == 0 ? true : false);

            if (isSaveSnp)
            {
                string FileNamePartial = String.Format("CH{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                    m_tt.ChannelNumber, m_tt.Band, m_tt.SwitchIn, m_tt.SwitchAnt, m_tt.SwitchOut, m_tt.FileOutput_Mode, m_tt.ParameterNote);
                //string FileNamePartial = String.Format("{0}_CH{1}_{2}_{3}_{4}_{5}_{6}",
                //    m_modelTriggerDm.GetCurrentDataTriggerNumber(),
                //    ChannelNumber, Band, FileOutput_Mode, SwitchIn, SwitchAnt, SwitchOut);
                snpOutputFilePath = FileManager.TigerModel.FormSnpOutputFileName(FileNamePartial.Replace("__","_"));
                string x = m_tt.SNPFileOutput_Path;
            }

            return snpOutputFilePath;
        }

        public void SaveTrace(int channelNumber, int TestNo)
        {
            if (channelNumber == 0)
            {
                SaveSnpFileChannel0();
                return;
            }

            bool b_EnhanceDataFetch = true;
            if (b_EnhanceDataFetch)     // This is set to true always.
            {
                int cn = channelNumber - 1;
                string swParent = String.Format("Trigger_RunTest_{0}", TestNo);
                string sw2 = String.Format("Trigger_RunTest_{0}_MeasResult2_TTSaveTrace", TestNo);
                SaveTraceFileEnhanceDataFetch(cn);

                //VNA.Memory.Store.SNP.Data(ChannelNumber, noOfTrace,
                //    SParamData[ChannelNumber - 1].NoPorts, SnPFile_Name, snpFileName);

            }
            else
            {
                int cn = channelNumber - 1;
                int[] tm = m_dm1.TraceMatch[cn].TraceNumber;
                s_SParam_Grab spg = m_dm2.GetCurrentDataTrigger();
                FileManager.TigerModel.SaveSnpFile2(m_tt.FileOutput_Sampling_Count, m_tt.FileOutput_Counting,
                    m_tt.FileOutput_Mode, cn,
                    spg, m_dm1.SParamData[cn], tm);

            }
        }


        private void SaveSnpFileChannel0()
        {
            for (int iChn = 0; iChn < m_dm1.SParamData.Length; iChn++)
            {
                S_Param sp = m_dm1.SParamData[iChn];
                int actualCn = iChn + 1;
                int[] tm = m_dm1.TraceMatch[iChn].TraceNumber;
                s_SParam_Grab spg = m_dm2.GetCurrentDataTrigger();
                FileManager.TigerModel.SaveSnpFile(m_tt.FileOutput_Sampling_Count, m_tt.FileOutput_Counting,
                    m_tt.FileOutput_Mode, actualCn,
                    spg, sp, tm);
            }
        }

        private void SaveTraceFileEnhanceDataFetch(int cn)
        {
            s_SParam_Grab spg = m_dm2.GetCurrentDataTrigger();

            int[] tm = m_dm1.TraceMatch[cn].TraceNumber;
            FileManager.TigerModel.StoreCnTrace(m_tt.Band, m_tt.SwitchOut, m_tt.FileOutput_Mode, cn, m_tt.TestNo, spg, m_dm1.SParamData[cn], tm);
            FileManager.TigerModel.StoreTigerContent(m_tt.FileOutput_Sampling_Count, m_tt.FileOutput_Counting, m_tt.Band, m_tt.SwitchOut, m_tt.FileOutput_Mode, cn, m_tt.TestNo, spg, m_dm1.SParamData[cn], tm, GetSnpOutputFilePath());
        }
    }
}