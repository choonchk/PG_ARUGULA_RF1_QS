using System;
using System.Windows.Forms;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;

namespace LibFBAR_TOPAZ
{
    public class TestCaseBase
    {
        public int ChannelNumber;
        public string SParameters;
        protected int SParam;

        protected s_Result SaveResult;
        /// <summary>
        /// Used in InitSettings to calculate points.
        /// </summary>
        protected s_SegmentTable[] SegmentParam
        {
            get { return m_spModel.SegmentParam; }
        }

        protected S_Param[] SParamData
        {
            get { return m_spModel.SParamData; }
        }
        protected s_TraceMatching[] TraceMatch
        {
            get { return m_spModel.TraceMatch; }
        }
        protected SParameterMeasurementDataModel m_spModel;

        public TestCaseBase()
        {
            SaveResult = new s_Result();
            m_model1 = new WaveformInterpolation();
        }

        public void Initialize(SParameterMeasurementDataModel spDataModel)
        {
            m_spModel = spDataModel;
        }

        public s_Result GetResult()
        {
            return SaveResult;
        }

        public string Header
        {
            get { return SaveResult.Result_Header;}
            set { SaveResult.Result_Header = value; }
        }

        public virtual void Clear_Results()
        {
            SaveResult.Clear_Results();


        }

        public TestCaseBase Previous_Test { get; set; }

        public virtual void InitSettings()
        { }

        public virtual void RunTest()
        {
        }

        protected virtual void SetResult(double result)
        {
            SaveResult.Result_Data = result;
        }

        protected void ShowError(object source, string msg)
        {
            string title = source.GetType().Name;
            PromptManager.Instance.ShowError(msg, title);
            SaveResult.Result_Data = -999;
        }

        protected void InitSparam()
        {
            try
            {
                e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                SParam = TraceMatch[ChannelNumber - 1].TraceNumber[SParamDef.GetHashCode()];
                m_currentSp = SParamData[ChannelNumber - 1].sParam_Data[SParam];
            }
            catch (Exception e)
            {
                MessageBox.Show("InitSparam Error:" + "\r\n" + "ChannelNumber-1 is " + (ChannelNumber - 1).ToString() + "\r\n" +
                                "SParam is: " + SParam.ToString());
                string dis = "";
                //throw;
            }
        }

        public double Frequency_At;

        /// <summary>
        /// Prev test here means an arbitrary test number, not the previous.
        /// </summary>
        protected bool IsPreviousTestDefined
        {
            get { return Previous_Test != null; }
        }

        public string Interpolation
        {
            get { return m_model1.Interpolation; }
            set
            {
                m_model1.Interpolation = value;
            }
        }

        protected bool b_Interpolation
        {
            get { return m_model1.b_Interpolation; }
        }


        protected WaveformInterpolation m_model1;

        protected S_ParamData m_currentSp;

        protected void InitWaveformInterpolation()
        {
            InitSparam();
            //ChannelNumber = ChannelNumber - 1;
            //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
            if (IsPreviousTestDefined) return;

            try
            {
                m_model1.Init(m_currentSp, SegmentParam[ChannelNumber - 1], Frequency_At);
            }
            catch (Exception)
            {
                int x = 0;
                //throw;
            }
            if (m_model1.IsError)
            {
                ShowError(this, m_model1.ErrorMessage);
                SaveResult.Result_Data = -999;
            }
        }
    }

    public class TestCaseCalcBase
    {
        protected s_Result SaveResult2;

        public string Header
        {
            get { return SaveResult2.Result_Header; }
            set
            {
                SaveResult2 = new s_Result();
                SaveResult2.Result_Header = value;
            }
        }

        public virtual s_Result Get_Result()
        {
            return SaveResult2;
        }

        public virtual void Clear_Results()
        {
            SaveResult2.Clear_Results();
        }

        public virtual void RunTest()
        {
        }

        protected void SetResult(double rtnResult)
        {
            SaveResult2.Result_Data = rtnResult;
        }
    }
    }