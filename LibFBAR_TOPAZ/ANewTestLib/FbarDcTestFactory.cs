using System;
using System.Collections.Generic;
using EqLib;
using MPAD_TestTimer;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;
using SParamTestCommon;

namespace LibFBAR_TOPAZ.ANewTestLib
{
    /// <summary>
    /// Store, initialize, run DC test. Run MIPI, OTP Test. Load DC Channel Settings TCF sheet.
    /// </summary>
    public class FbarDcTestFactory : IDcMipiOtpTestCase
    {
        private DCTest m_modelDc;
        public Dictionary<int, DCMipiOtpTestController> m_dcTestCollection { get; set; }

        ///// <summary>
        ///// Get result after LoadTestCondition().
        ///// </summary>
        //public s_Result[] GetResult()
        //{
        //    foreach (KeyValuePair<int, DCMipiOtpTestController> t in m_dcTestCollection)
        //    {
        //        SaveResult[t.Value.TestNo] = t.Value.SaveResult3;
        //    }

        //    return SaveResult;
        //}
        public FbarDcTestFactory()
        {
            m_dcTestCollection = new Dictionary<int, DCMipiOtpTestController>();
        }

        /// <summary>
        /// Load DC Setting from TCF DC_Channel_Setting sheet. Fill DC_matching.
        /// </summary>
        public void Load_DC_ChannelSettings()
        {
            List<PowerSupplyDCMatchDataType> dcMatchList = Load_DC_ChannelSettings2();
            m_modelDc = new DCTest();
            m_modelDc.DC_matching = dcMatchList;
        }

        /// <summary>
        /// Create a DC test.
        /// </summary>
        /// <param name="testItem"></param>
        public void Create(DCMipiOtpTestController testItem)
        {
            if (testItem == null) return;
            testItem.Initialize(m_modelDc.DC_matching, Eq.Site[0].DC,
                Eq.Site[0].HSDIO);
            m_dcTestCollection.Add(testItem.TestNo, testItem);
        }

        public s_Result RunTest(int testCnt)
        {
            DCMipiOtpTestController t = m_dcTestCollection[testCnt];
            t.RunTest();
            return t.SaveResult;
        }
        public s_Result RunTest2(int testCnt)
        {
            DCMipiOtpTestController testItem = m_dcTestCollection[testCnt];
            testItem.InitializeTest();
            testItem.RunTest2();
            testItem.UnInitTest();
            return testItem.SaveResult;
        }

        public void RunTest3(DCMipiOtpTestController testObject)
        {
            testObject.InitializeTest();
            testObject.RunTest2();
            testObject.UnInitTest();
        }

        /// <summary>
        /// Get result after LoadTestCondition().
        /// </summary>
        public s_Result GetResult(int testIndex)
        {
            return m_dcTestCollection[testIndex].SaveResult;
        }

        // Getting the internal data to public
        public void Clear_Results()
        {
            foreach (KeyValuePair<int, DCMipiOtpTestController> t in m_dcTestCollection)
            {
                t.Value.ClearResult();
            }
        }
        /// <summary>
        /// Load DC Setting from TCF. Fill DC_matching.
        /// </summary>
        public List<PowerSupplyDCMatchDataType> Load_DC_ChannelSettings2()
        {
            List<PowerSupplyDCMatchDataType> listDc =
                new List<PowerSupplyDCMatchDataType>();
            TcfSheetDcChannelSetting sheet = new TcfSheetDcChannelSetting("DC_Channel_Setting", 20, 20);

            for (int i = 0; i < sheet.testPlan.Count; i++)
            {
                sheet.SetCurrentRow(i);

                PowerSupplyDCMatchDataType proc = FillDC_ChannelSettings(sheet);
                listDc.Add(proc);
            }

            return listDc;
        }

        private PowerSupplyDCMatchDataType FillDC_ChannelSettings(TcfSheetDcChannelSetting sheet)
        {
            PowerSupplyDCMatchDataType m = new PowerSupplyDCMatchDataType();
            string ps = sheet.GetData("EquipmentName");
            m.PS_Name = GetPSEquipment(ps);
            m.Address = sheet.GetData("Address/SMU Pin Names");

            // Load channel numbers. These are the remaining columns Ch1,2,3,4.......
            for (int i = 3; i < sheet.Header.Count; i++)
            {
                string channelNo = sheet.GetData(sheet.Header[i]);
                if (channelNo != "")
                {
                    m.Channel_Match.Add(int.Parse(channelNo));
                }
            }
            return m;
        }

        private PowerSupplyEquipmentType GetPSEquipment(string equipment_str)
        {
            PowerSupplyEquipmentType tmpPS;
            switch (equipment_str.ToUpper())
            {
                case "NI_SMU":
                    tmpPS = (PowerSupplyEquipmentType)Enum.Parse(typeof(PowerSupplyEquipmentType), equipment_str, true);
                    break;
                case "PS4142":
                    tmpPS = (PowerSupplyEquipmentType)Enum.Parse(typeof(PowerSupplyEquipmentType), equipment_str, true);
                    break;
                case "AEMULUS":
                case "PS6626":
                default:
                    string msg =
                        String.Format("Please Check the Power Supply Defination Settings \r\nDefined Setting : {0}",
                            equipment_str);
                    PromptManager.Instance.ShowError(msg, "Unable to Set Power Supply Equipment Correctly");
                    tmpPS = PowerSupplyEquipmentType.AEMULUS;
                    break;
            }
            return tmpPS;
        }

        // below are unused.
        public void No_Bias()
        {
            m_modelDc.No_Bias();
        }

        /// <summary>
        /// Not called.
        /// </summary>
        public void InitEquipment()
        {
            m_modelDc.Init();
        }

        public void InitSettings_Pxi(int testCnt,string tp)
        {
            switch (tp)
            {
                case "DC_SETTINGS":
                case "DC_SETTING":
                    m_dcTestCollection[testCnt].InitSettings_Pxi();
                    break;
            }
        }
    }
}
