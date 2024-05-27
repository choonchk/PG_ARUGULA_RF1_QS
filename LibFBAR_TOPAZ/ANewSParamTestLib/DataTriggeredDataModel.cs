using System;
using System.Collections.Generic;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ.ANewSParamTestLib
{
    /// <summary>
    /// Store a boolean list of active SParam measurement to make for each trigger, inferred from test conditions.
    /// </summary>
    public class DataTriggeredDataModel
    {
        /// <summary>
        /// Active SParameter measurement for each trigger.
        /// </summary>
        private List<s_SParam_Grab> ActiveSParameterList { get; set; }
        private s_SParam_Grab m_currentDataTrigger;
        private int m_currentTriggerNo;

        public DataTriggeredDataModel()
        {
            m_currentDataTrigger = new s_SParam_Grab();
            m_currentDataTrigger.SParam_Grab = new bool[118];
            ActiveSParameterList = new List<s_SParam_Grab>();
        }

        /// <summary>
        /// Called when a new trigger test condition is loaded.
        /// </summary>
        public void AddTrigger()
        {
            ActiveSParameterList.Add(m_currentDataTrigger);

            m_currentDataTrigger = new s_SParam_Grab();
            m_currentDataTrigger.SParam_Grab = new bool[118];
        }

        /// <summary>
        /// Called for each test conditions loaded after the trigger test condition.
        /// </summary>
        /// <param name="tcSParameter"></param>
        public void SetSParameterNumber(string tcSParameter)
        {
            bool isTcSparameterColumnDefined = tcSParameter != "";
            if (!isTcSparameterColumnDefined) return;

            e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tcSParameter);
            m_currentDataTrigger.SParam_Grab[SParamDef.GetHashCode()] = true;
        }

        public void IncrementTrigger()
        {
            m_currentTriggerNo++;
        }

        public s_SParam_Grab GetCurrentDataTrigger()
        {
            return ActiveSParameterList[m_currentTriggerNo];
        }

        public int GetCurrentDataTriggerNumber()
        {
            return m_currentTriggerNo;
        }

        public bool IsCurrentSParameterActive(int Select_SParam_Def)
        {
            return ActiveSParameterList[m_currentTriggerNo].SParam_Grab[Select_SParam_Def];
        }

        /// <summary>
        /// Called before a RunTest().
        /// </summary>
        public void Reset()
        {
            m_currentTriggerNo = 0;
        }
    }
}