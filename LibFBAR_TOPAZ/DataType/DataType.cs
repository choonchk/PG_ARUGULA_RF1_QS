using System;
using System.Collections.Generic;
using LibFBAR_TOPAZ.ANewEqLib;

namespace LibFBAR_TOPAZ.DataType
{
    public class S_Param
    {
        public S_ParamData[] sParam_Data;
        public double[] Freq;
        public int TotalTraceCount;
        public int NoPorts;
        public int NoPoints;
        public bool[] SParam_Enable;
    }

    public class S_ParamData
    {
        public s_DataType[] sParam;
        public e_SFormat Format;
    }

    public class s_DataType
    {
        public Real_Imag ReIm;
        public Mag_Angle MagAng;
        public dB_Angle dBAng;
    }

    /// <summary>
    /// Used by Balance function only.
    /// </summary>
    public class S_CMRRnBal_Param
    {
        public S_ParamData Balance;
        public S_ParamData CMRR;
        public bool Balance_Enable;
        public bool CMRR_Enable;
    }
    /// <summary>
    /// array size of 118 each representing S11,S21,S22,S31....
    /// </summary>
    public class s_SParam_Grab
    {
        /// <summary>
        /// array size of 118 each representing S11,S21,S22,S31....
        /// </summary>
        public bool[] SParam_Grab;

        public bool IsDef(string sparamDef)
        {
            e_SParametersDef tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), sparamDef);
            return SParam_Grab[tmpSParamDef.GetHashCode()];
        }

        public bool IsDef(e_SParametersDef sparamDef)
        {
            return SParam_Grab[sparamDef.GetHashCode()];
        }
    }
    public class s_TraceMatching
    {
        public int[] TraceNumber;
        public int[] SParam_Def_Number;
    }

    /// <summary>
    /// Store result, multi-result from DC test and Use_Previous result from Freq_At.
    /// </summary>
    public class s_Result
    {
        public int TestNumber;
        /// <summary>
        /// True by default. Set to false only for Trigger. Trigger does not have Result.
        /// </summary>
        public bool Enable;

        public s_Result()
        {
            m_multiResultList = new List<s_mRslt>();
            Enable = true;
        }

        private string result_header;
        public string Result_Header
        {
            get { return result_header; }
            set
            {
                string noSpace = value.Replace(" ", "_");
                result_header = noSpace;
            } //I need a masher here to put the header values in proper order
        }
        public double Result_Data;

        /// <summary>
        /// Set by Freq_At, read by Real_at, Imag_At, Mag_At for Use_Previous case.
        /// It is the frequency point found by Freq_At.
        /// </summary>
        public int Misc;
        //public string Result_Unit;  // if required

        //For new OQA bin trace saving
        public List<string> TraceFile;

        //Tiger file
        public List<string> TigerTraceFile;

        private List<s_mRslt> m_multiResultList;

        /// <summary>
        /// Is has multi-result. DC test will produce multi-result.
        /// </summary>
        public bool IsHasMultiResult
        {
            get { return m_multiResultList.Count > 0 && m_multiResultList[0].Enable; }
        }


        public void SetValue(string headerName, double resultData)
        {
            Result_Header = headerName;
            Result_Data = resultData;
        }

        public void AddMultiResult(List<s_mRslt> rsList)
        {
            m_multiResultList.AddRange(rsList);
        }

        public void AddMultiResult(s_mRslt rs)
        {
            m_multiResultList.Add(rs);
        }

        /// <summary>
        /// Filter out invalid result.
        /// </summary>
        /// <returns></returns>
        public List<s_mRslt> GetMultiResultList()
        {
            List < s_mRslt > result = new List<s_mRslt>();
            foreach (s_mRslt rs in m_multiResultList)
            {
                if (rs.Result_Header != null)
                {
                    result.Add(rs);
                }
            }
            return result;
        }

        public void Clear_Results()
        {
            Result_Data = 0;
            m_multiResultList.Clear();
        }

        public bool IsHeaderContains(string key)
        {
            if (String.IsNullOrEmpty(Result_Header))
            {
                return false;
            }
            bool isContain = Result_Header.ToUpper().Contains(key);
            return isContain;
        }
    }

    public class s_mRslt
    {
        public bool Enable;
        public string Result_Header;
        public double Result_Data;
    }

    public struct s_SNPFile
    {

        public string FileOutput_Path;
        public string SNPFileOutput_Path;
        public int FileOutput_HeaderCount;
        public string FileOutput_FileName;
        public string FileOutput_Mode;
        public bool FileOutput_Enable;
        public bool ENASNPFileOutput_Enable;
        /// <summary>
        /// TCF TraceFileOutput_Count
        /// </summary>
        public int FileOuuput_Count;
        public int SNPFileOuuput_Count;
        public List<string> FileOutput_HeaderName;

    }

}
