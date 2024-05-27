using System.Collections.Generic;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// TCF Data Object.
    /// </summary>
    public class SParaTestConditionFactory
    {
        public string EnaStateFile { get; set; }
        public string SnpFileIteration { get; set; }
        public bool EnaStateFileEnable { get; set; }
        public string SnpFileMaxCount { get; set; }
        public string TraceFileMaxCount { get; set; }

        /// <summary>
        /// TraceFile_Enable.
        /// </summary>
        public int TraceFileOutput_Count { get; set; }
        /// <summary>
        /// TraceFile_Enable.
        /// </summary>
        public int SNPFileOutput_Count { get; set; }
        /// <summary>
        /// True if TraceFile_Enable is not zero.
        /// </summary>
        public bool TraceFileEnable { get; set; }

        /// <summary>
        /// TraceFile_Sampling.
        /// </summary>
        public string SnpFileSampling { get; set; }

        /// <summary>
        /// TraceFile_Sampling
        /// </summary>
        public int TraceFileOutput_Count_Sampling { get; set; }

        /// <summary>
        /// True if TraceFile_Sampling is not zero.
        /// </summary>
        public bool SamplingTraceFileEnable { get; set; }

        /// <summary>
        /// SnpFileSampling
        /// </summary>
        public int SnpFileOutput_Count_Sampling { get; set; }
        public bool SamplingSnpFileEnable { get; set; }
        /// <summary>
        /// ENA_Cal_Enable
        /// </summary>
        public string Cal_Enable { get; set; }
        /// <summary>
        /// True if ENA_Cal_Enable is not zero.
        /// </summary>
        public bool ENA_Cal_Enable { get; set; }
        /// <summary>
        /// Is a production feature not SPara.
        /// </summary>
        public bool PauseTestOnDuplicate { get; set; }
        /// <summary>
        /// Is a production feature not SPara.
        /// </summary>
        public bool DPAT_Flag { get; set; }

        /// <summary>
        /// Condition_FBAR Sheet: Column Names and row values. All including DC.
        /// </summary>
        public List<Dictionary<string, string>> DicTestCondTempNA { get; }
        /// <summary>
        /// Condition_FBAR Sheet: Column Names and row values. Filtered by Test Mode column is DC Only.
        /// </summary>
        public List<Dictionary<string, string>> DicTestCondMipi { get; }

        public SParaTestConditionFactory()
        {
            DicTestCondTempNA = new List<Dictionary<string, string>>();
            DicTestCondMipi = new List<Dictionary<string, string>>();
        }
    }

}