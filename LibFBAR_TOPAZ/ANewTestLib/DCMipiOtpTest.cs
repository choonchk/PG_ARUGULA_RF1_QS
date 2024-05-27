using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EqLib;
using InstrLib;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;
using TestLib;

namespace LibFBAR_TOPAZ.ANewTestLib
{
    // CCThai : can be improved. To restructure DCMipiOtp.
    public interface IDcMipiOtpTestCase
    {
        // To be obsoleted.
        void InitSettings_Pxi(int testCnt, string tp);
        /// <summary>
        /// testIndex is needed because it is a collection of test instead of just one test.
        /// </summary>
        /// <param name="testIndex"></param>
        /// <returns></returns>
        s_Result GetResult(int testIndex);
        // Others are clearing 1 result only but this is clearing all, since it's a collection.
        void Clear_Results();
    }

    /// <summary>
    /// DCTest - Set and read bias.
    /// </summary>
    public class DCTest
    {
        public int TotalChannel;
        /// <summary>
        /// TCF DC_Channel_Setting sheet.
        /// </summary>
        public List<PowerSupplyDCMatchDataType> DC_matching;
        public List<PowerSupplyDCSettingDataType> DC_Setting; //ChoonChin (20191206) - Set to public for header param sync. with RF1
        private DC4142Equipment m_equipmentDc4142;
        private Dictionary<string, EqDC.iEqDC> m_equipmentDcNiSmuList;

        public void Initialize(int totalChannel, List<PowerSupplyDCMatchDataType> fixedDcMatching,
            List<PowerSupplyDCSettingDataType> dc_Setting, Dictionary<string, EqDC.iEqDC> equipmentDcNiSmuList)
        {
            TotalChannel = totalChannel;
            DC_Setting = dc_Setting;

            // Fixed conditions.
            m_equipmentDcNiSmuList = equipmentDcNiSmuList;
            m_equipmentDc4142 = new DC4142Equipment();
            DC_matching = fixedDcMatching;
            if (m_equipmentDc4142 != null)
            {
                m_equipmentDc4142.parse_DC_Matching = fixedDcMatching;
            }
        }

        public void ForceVoltage(string resultHeader)
        {
            bool isHls2 = GetProductSpecificProduct() == "HLS2";
            bool isJoker = GetProductSpecificProduct() == "Joker";

            if (isHls2)
            {
                ForceVoltage("Vbatt", 0, 0.1);
                ForceVoltage("Vcc", 0, 0.1);
                ForceVoltage("Vlna", 0, 0.1);
            }

            if (isJoker)
            {
                if (resultHeader.ToUpper() == "READ_CURRENT_LEAKAGE")
                {
                    ForceVoltage("Vbatt", 3, 0.1);
                    ForceVoltage("Vcc", 5, 0.5);
                    ForceVoltage("Vdd", 1.3, 0.1);
                }
                else if (!resultHeader.ToUpper().Contains("POWER_MODE"))
                {
                    ForceVoltage("Vbatt", 0, 0.1);
                    ForceVoltage("Vcc", 0, 0.5);
                    ForceVoltage("Vdd", 0, 0.1);
                }

            }

            Thread.Sleep(1);
        }

        public void ForceVoltage(string dcName, double voltage, double currentLimit)
        {
            m_equipmentDcNiSmuList[dcName].ForceVoltage(voltage, currentLimit);
        }

        public void ForceVoltageLegacy(int dcIndex, double voltage, double currentLimit)
        {
            string address = DC_matching[dcIndex].Address;
            m_equipmentDcNiSmuList[address].ForceVoltage(voltage, currentLimit);
        }

        public void Set_Bias(int PS_Set)
        {
            Set_Bias(PS_Set, DC_Setting);
        }

        private void Set_Bias(int PS_Set, List<PowerSupplyDCSettingDataType> dcSettingList)
        {
            switch (DC_matching[PS_Set - 1].PS_Name)
            {
                case PowerSupplyEquipmentType.NI_SMU:
                    for (int iSet = 0; iSet < DC_matching.Count; iSet++)
                    {
                        string address = DC_matching[iSet].Address;
                        PowerSupplyDCSettingDataType dcs = dcSettingList[iSet];
                        m_equipmentDcNiSmuList[address]
                            .ForceVoltage(dcs.Voltage, dcs.Current);
                    }

                    //  }
                    break;
                case PowerSupplyEquipmentType.PS4142:
                {
                    if (m_equipmentDc4142 != null) m_equipmentDc4142.Set_Bias(PS_Set);
                    break;
                }

           }
        }
        /// <summary>
        /// Measure current, ForceVoltage, Measure current, PostLeakage test.
        /// </summary>
        /// <param name="PS_Set"></param>
        /// <param name="headerstring"></param>
        public List<s_mRslt> Read_Bias(int PS_Set, string headerstring)
        {
            List<s_mRslt> rList = new List<s_mRslt>();

            switch (DC_matching[PS_Set - 1].PS_Name)
            {
                case PowerSupplyEquipmentType.NI_SMU:
                {
                    rList = Read_BiasNiSmu(headerstring);
                    break;
                }

                case PowerSupplyEquipmentType.PS4142:
                {
                    m_equipmentDc4142.ReadBias(PS_Set);
                    break;
                }
            }

            return rList;
        }

        private List<s_mRslt> Read_BiasNiSmu(string headerstring)
        {
            List<s_mRslt> rList = new List<s_mRslt>();
            Thread.Sleep(200);   /// back

            foreach (PowerSupplyDCMatchDataType dcMatch in DC_matching)
            {
                string dcAddress = dcMatch.Address;
                double data = m_equipmentDcNiSmuList[dcAddress].MeasureCurrent(1);
                string rHeader = String.Empty;
                switch (dcAddress)
                {
                    case "Vbatt":
                    case "Vlna":
                    case "Vcc":
                    case "Vdd":
                        rHeader = headerstring.Replace("LEAKAGE",
                            "LEAKAGE_" + dcAddress.ToUpper()); // _4.5V"; //example is VBATT_LEAKAGE_4.5
                        SetResult(rHeader, Math.Abs(data), rList);
                        break;
                }
            }

            Thread.Sleep(2);

            foreach (KeyValuePair<string, EqDC.iEqDC> dcPin in m_equipmentDcNiSmuList)
            {
                switch (dcPin.Key)
                {
                    case "Sdata1":
                    case "Sclk1":
                    case "Vio1":
                    case "Sdata2":
                    case "Sclk2":
                    case "Vio2":
                        m_equipmentDcNiSmuList[dcPin.Key].ForceVoltage(1.95, 0.0000019);
                        break;
                }
            }

            foreach (KeyValuePair<string, EqDC.iEqDC> dcPin in m_equipmentDcNiSmuList)
            {
                switch (dcPin.Key)
                {
                    case "Sdata1":
                    case "Sclk1":

                    case "Sdata2":
                    case "Sclk2":
                    case "Vio2":
                        double data = m_equipmentDcNiSmuList[dcPin.Key].MeasureCurrent(1);
                        string rHeader = headerstring.Replace("LEAKAGE",
                            "LEAKAGE_" + dcPin.Key.ToUpper().Replace("_", ""));
                        SetResult(rHeader, Math.Abs(data), rList);
                        break;

                    case "Vio1":
                        data = m_equipmentDcNiSmuList[dcPin.Key].MeasureCurrent(1);
                        string rHeaderVio1 = headerstring.Replace("LEAKAGE",
                            "LEAKAGE_" + dcPin.Key.ToUpper().Replace("_", ""));
                        SetResult(rHeaderVio1, Math.Abs(data), rList);
                        break;
                }

            }

            foreach (KeyValuePair<string, EqDC.iEqDC> dcPin in m_equipmentDcNiSmuList)
            {
                switch (dcPin.Key)
                {
                    case "Sdata1":
                    case "Sclk1":
                    case "Vio1":
                    case "Sdata2":
                    case "Sclk2":
                    case "Vio2":
                        m_equipmentDcNiSmuList[dcPin.Key].PostLeakageTest();
                        break;
                }

            }

            return rList;

        }

        //Seoul
        public s_mRslt Read_Bias(int PS_Set, int Channel,string DCTestCore)
        {
            if (DC_matching[PS_Set - 1].PS_Name != PowerSupplyEquipmentType.NI_SMU) return new s_mRslt();


            string currAddress = DC_matching[Channel].Address;
            EqDC.iEqDC currEquipmentDc = m_equipmentDcNiSmuList[currAddress];

            if (!DCTestCore.ToUpper().Contains("LEAKAGE"))
            {
                currEquipmentDc.SetupCurrentMeasure(0.0003, TriggerLine.None);

                Thread.Sleep(1); //follows PA test               

                double data = currEquipmentDc.MeasureCurrent(1);
                string header = "I" + currAddress.Remove(0, 1) + "-Q";
                s_mRslt r = SetResult(header, data);
                return r;
            }


            // Leakage test.
            int mDelay = 50;
            int CurrentAvg = 16;

            Thread.Sleep(mDelay);

            currEquipmentDc.PreLeakageTest(new ClothoLibAlgo.DcSetting());
            double mt = mDelay / 1000.0 + (double) CurrentAvg * 0.001;
            currEquipmentDc.SetupCurrentTraceMeasurement(mt, 500e-6, TriggerLine.None);

            double[] dcLeakageTrace = currEquipmentDc.MeasureCurrentTrace();

            int skipPoints = (int) (dcLeakageTrace.Length * (double) mDelay / 1000.0 /
                                    ((double) mDelay / 1000.0 + (double) CurrentAvg * 0.001));
            skipPoints = Math.Min(skipPoints, dcLeakageTrace.Length - 1);

            double result = dcLeakageTrace.Skip(skipPoints).Average();
            string headerLeakage = "I" + currAddress.Remove(0, 1) + "-Q";
            s_mRslt r2 = SetResult(headerLeakage, result);

            currEquipmentDc.PostLeakageTest();

            return r2;

        }

        //ChoonChin - For Icq test
        public s_mRslt Read_Bias_Icq(int PS_Set, string Band)
        {
            bool isHls2 = GetProductSpecificProduct() == "HLS2";
            if (isHls2)
            {
                return Read_Bias_Icq(PS_Set, Band, "Vlna", m_equipmentDcNiSmuList["Vlna"]);
            }
            bool isJoker = GetProductSpecificProduct() == "Joker";
            if (isJoker)
            {
                return Read_Bias_Icq(PS_Set, Band, "Vbatt", m_equipmentDcNiSmuList["Vbatt"]);
            }

            return new s_mRslt();
        }

        //ChoonChin - For Icq test
        private s_mRslt Read_Bias_Icq(int PS_Set, string Band, string dcAddress, EqDC.iEqDC dcToRead)
        {
            s_mRslt r = new s_mRslt();

            switch (DC_matching[PS_Set - 1].PS_Name)
            {
                case PowerSupplyEquipmentType.NI_SMU:
                {
                    dcToRead.SetupCurrentMeasure(0.0003, TriggerLine.None);
                    Thread.Sleep(1);  //follows PA test               
                    double data = dcToRead.MeasureCurrent(1);
                    string header = "I" + dcAddress.Remove(0, 1) + "-Q";
                    r = SetResult(header, data);
                    break;
                }

                case PowerSupplyEquipmentType.PS4142:
                {
                    m_equipmentDc4142.ReadBias(PS_Set);
                    break;
                }
            }

            return r;
        }
        //CheeOn - For IbiasT test
        public s_mRslt Read_BiasT_Icq(int PS_Set, string Band)
        {
            s_mRslt r = new s_mRslt();
            switch (DC_matching[PS_Set - 1].PS_Name)
            {
                case PowerSupplyEquipmentType.NI_SMU:
                {
                    EqDC.iEqDC equipmentDc = m_equipmentDcNiSmuList[DC_matching[2].Address];

                    equipmentDc.SetupCurrentMeasure(0.0003, TriggerLine.None);
                    Thread.Sleep(50);  //follows PA test               

                    double data = equipmentDc.MeasureCurrent(1);
                    string header = "I" + DC_matching[2].Address.Remove(0, 1);
                    r = SetResult(header, Math.Abs(data));
                    break;
                }

                case PowerSupplyEquipmentType.PS4142:
                {
                    m_equipmentDc4142.ReadBias(PS_Set);
                    break;
                }
            }
            return r;

        }

        //CheeOn - For ICPL test
        public s_mRslt Read_CPL_Icq(int PS_Set)
        {
            s_mRslt r = new s_mRslt();

            switch (DC_matching[PS_Set - 1].PS_Name)
            {
                case PowerSupplyEquipmentType.NI_SMU:
                {
                    EqDC.iEqDC equipmentDc = m_equipmentDcNiSmuList[DC_matching[4].Address];

                    equipmentDc.SetupCurrentMeasure(0.0003, TriggerLine.None);
                    Thread.Sleep(2);  //follows PA test               

                    double data = equipmentDc.MeasureCurrent(1);
                    string header = "I" + DC_matching[4].Address.Remove(0, 1);
                    r = SetResult(header, data);
                    break;
                }

                case PowerSupplyEquipmentType.PS4142:
                {
                    m_equipmentDc4142.ReadBias(PS_Set);
                    break;
                }
            }

            return r;

        }

        private void SetResult(string name, double data, List<s_mRslt> rList)
        {
            s_mRslt r = new s_mRslt { Enable = true, Result_Data = data };
            r.Result_Header = name;
            rList.Add(r);
        }

        private s_mRslt SetResult(string name, double data)
        {
            s_mRslt r = new s_mRslt { Enable = true, Result_Data = data };
            r.Result_Header = name;
            return r;
        }

        private string GetProductSpecificProduct()
        {
            if (m_equipmentDcNiSmuList.ContainsKey("Vlna"))
            {
                return "HLS2";
            }
            if (m_equipmentDcNiSmuList.ContainsKey("Vdd"))
            {
                return "Joker";
            }
            return String.Empty;
        }

        public void Init()
        {
            if (m_equipmentDc4142 != null) m_equipmentDc4142.Init();
        }

        public void Init_Bias_Array()
        {
            if (m_equipmentDc4142 != null) m_equipmentDc4142.Init_Bias_Array();
        }

        public s_mRslt[] Init_Bias_Array2(bool isIgnoreRead)
        {
            s_mRslt[] rtnResult = null;
            if (m_equipmentDc4142 != null) m_equipmentDc4142.Init_Bias_Array();

            if (!isIgnoreRead)
            {
                rtnResult = new s_mRslt[TotalChannel];

                for (int iSet = 0; iSet < TotalChannel; iSet++)
                {
                    //rtnResult[iSet].Enable = DC_Setting[iSet].b_Enable;
                    //rtnResult[iSet].Result_Header = DC_Setting[iSet].Header;
                }
            }

            return rtnResult;
        }

        public void No_Bias()
        {
            if (m_equipmentDc4142!=null) m_equipmentDc4142.No_Bias();
        }

        public double[] Get_Result_Array
        {
            get
            {
                if (m_equipmentDc4142 != null) return m_equipmentDc4142.Get_Result_Array;
                return new double[10];
            }
        }
    }

    public class MipiTest
    {
        private s_Result SaveResult;

        public s_Result RunTest(string nParamClass, EqHSDIO.EqHSDIObase eqHsdio, string powerMode)
        {
            SaveResult = new s_Result();

            string TempMipiRead;
            int MipiRead;
            //EqHSDIO.EqHSDIObase eqHsdio = Eq.Site[0].HSDIO;
            float RsltData;

            switch (nParamClass)
            {
                case "TEMP":
                    SetResult("MIPI_TEMP1", HsdioSendVectorTemp());
                    break;
                case "MODULE_ID":
                    //TestLib.OTP_Procedure.OTP_Read_Bytes(1, 0);
                    SendVectorMipi1(eqHsdio);
                    //(20-Nov-2017)
                    TempMipiRead = eqHsdio.RegRead("E4");
                    MipiRead = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    // bit7 is CM ID
                    //MipiNumBitErrors = (((MipiRead & 0x80) >> 7));
                    //MipiRead = (MipiRead & 0x7F) << 8;
                    //MipiRead = MipiRead << 8;
                    MipiRead = (MipiRead & 0x7F) << 8; //Mask out cm id bit
                    TempMipiRead = eqHsdio.RegRead("E5");
                    RsltData = MipiRead + (int.Parse(TempMipiRead,
                                   System.Globalization.NumberStyles.HexNumber));
                    SetResult("OTP_MODULE_ID", RsltData);
                    break;
                case "MFG_ID":
                    SendVectorMipi1(eqHsdio);
                    //(20-Nov-2017)
                    TempMipiRead = eqHsdio.RegRead("E0");
                    MipiRead = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    MipiRead = (MipiRead) << 8;
                    TempMipiRead = eqHsdio.RegRead("E1");
                    RsltData = MipiRead + (int.Parse(TempMipiRead,
                                   System.Globalization.NumberStyles.HexNumber));
                    SetResult("MFG_ID", RsltData);
                    break;
                case "QC":
                    Eq.Site[0].HSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                        ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                        : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    eqHsdio.SendVector(powerMode);
                    RsltData = eqHsdio.GetNumExecErrors(powerMode);
                    SetResult("M_QCErrorBits_" + powerMode, RsltData);
                    break;
                case "CM_ID":
                    SendVectorMipi1(eqHsdio);
                    //(20-Nov-2017)
                    TempMipiRead = eqHsdio.RegRead("E4");
                    MipiRead = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    // bit7 is CM ID
                    int cm_ID = (((MipiRead & 0x80) >>
                                  7)); //bit7 is CM ID                                                                  
                    //(MipiRead & 0x8000) == 0x8000? 1 : 0;
                    RsltData = cm_ID;
                    SetResult("M_MIPI_CM-ID", RsltData);
                    break;
                case "READREG":
                    Eq.Site[0].HSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                        ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                        : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    Eq.Site[0].HSDIO.dutSlavePairIndex = (powerMode.Contains("TX") ? 1 : 2);
                    TempMipiRead = eqHsdio.RegRead(powerMode.Substring(2));

                    int result = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    string resultHeader = "M_MIPI_ReadTxReg-" + powerMode.Replace("TX", "");
                    if (powerMode.Contains("RX"))
                    {
                        resultHeader = "M_MIPI_ReadRxReg-" + powerMode.Replace("RX", "");
                    }
                    SetResult(resultHeader, result);
                    break;

                //ChoonChin - 20191125 - Add Tx and Rx wafer info
                case "M_MIPI_CMOS-TX-X":
                    SendVectorMipi1(eqHsdio); 
                    MipiRead = OTP_Procedure.OTP_Read_TX_X(0);
                    SetResult("M_MIPI_CMOS-TX-X", MipiRead);
                    break;
                case "M_MIPI_CMOS-TX-Y":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_TX_Y(0);
                    SetResult("M_MIPI_CMOS-TX-Y", MipiRead);
                    break;
                case "M_MIPI_CMOS-TX-WAFER-ID":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_WAFER_ID(0);
                    SetResult("M_MIPI_CMOS-TX-WAFER-ID", MipiRead);
                    break;
                case "M_MIPI_CMOS-TX-WAFER-LOT":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_WAFER_LOT(0);
                    SetResult("M_MIPI_CMOS-TX-WAFER-LOT", MipiRead);
                    break;
                case "M_MIPI_LNA-X":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_X(0);
                    SetResult("M_MIPI_LNA-X", MipiRead);
                    break;
                case "M_MIPI_LNA-Y":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_Y(0);
                    SetResult("M_MIPI_LNA-Y", MipiRead);
                    break;
                case "M_MIPI_LNA-WAFER-ID":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_ID(0);
                    SetResult("M_MIPI_LNA-WAFER-ID", MipiRead);
                    break;
                case "M_MIPI_LNA-WAFER-LOT":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_LOT(0);
                    SetResult("M_MIPI_LNA-WAFER-LOT", MipiRead);
                    break;
                case "M_FLAG_LOCKBIT_RX":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_Rx_Lockbit(0);
                    SetResult("M_FLAG_LOCKBIT_RX", MipiRead);
                    break;

                default:
                    break;
            }

            return SaveResult;
        }

        public s_Result RunTest(string nParamClass, EqHSDIO.EqHSDIObase eqHsdio, string powerMode, List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands)
        {
            SaveResult = new s_Result();

            string TempMipiRead;
            int MipiRead;
            //EqHSDIO.EqHSDIObase eqHsdio = Eq.Site[0].HSDIO;
            float RsltData;

            switch (nParamClass)
            {
                case "TEMP":
                    SetResult("MIPI_TEMP1", HsdioSendVectorTemp());
                    break;

                case "MODULE_ID":
                case "MFG_ID":
                case "OTP_STATUS_REV_ID_TX":
                case "OTP_STATUS_MODULE_ID_TX":
                case "OTP_STATUS_REV_ID_RX":
                case "OTP_STATUS_LNA_RX":
                case "OTP_MODULE_ID":
                case "OTP_STATUS_TX-X":
                case "OTP_STATUS_TX-Y":
                case "OTP_STATUS_WAFER-LOT":
                case "OTP_STATUS_WAFER-ID":
                case "OTP_STATUS_LNA_X":
                case "OTP_STATUS_LNA_Y":
                case "OTP_STATUS_LNA_WAFER-LOT":
                case "OTP_STATUS_LNA_WAFER-ID":
                case "M_MIPI_CMOS-TX-X":
                case "M_MIPI_CMOS-TX-Y":
                case "M_MIPI_CMOS-TX-WAFER-ID":
                case "M_MIPI_CMOS-TX-WAFER-LOT":
                case "M_MIPI_LNA-X":
                case "M_MIPI_LNA-Y":
                case "M_MIPI_LNA-WAFER-ID":
                case "M_MIPI_LNA-WAFER-LOT":

                    SetResult(nParamClass, TestLib.OTP_Procedure.OTP_Read_Bytes(0, zMIPI_Commands));
                    break;

                case "OTP_LOCKBIT_RX":
                case "OTP_STATUS_CM-ID":
                case "OTP_READ-NFR-PASS-FLAG":
                case "OTP_READ-RF1-PASS-FLAG":
                case "OTP_READ-RF2-PASS-FLAG":
                case "OTP_LOCKBIT_TX":
                case "M_FLAG_LOCKBIT_RX":
                    SetResult(nParamClass, (TestLib.OTP_Procedure.OTP_Check_Bit(0, zMIPI_Commands)==true? 1:0));
                    break;

                case "TXQC":
                case "RXQC01":
                case "RXQC02":
                case "RXQC03":
                    //EqHSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                    //    ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                    //    : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    //eqHsdio.SendVector(powerMode);
                    //RsltData = eqHsdio.GetNumExecErrors(powerMode);
                    //SetResult("M_QCErrorBits_" + powerMode, RsltData);

                    if (nParamClass.Contains("TX"))
                    {
                        Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                        Eq.Site[0].HSDIO.dutSlavePairIndex = 1;
                        eqHsdio.RegWrite("1C", "40");
                    }
                    else
                    {
                        Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                        Eq.Site[0].HSDIO.dutSlavePairIndex = 2;
                        eqHsdio.RegWrite("1C", "40");
                    }

                    eqHsdio.SendVector(nParamClass.ToUpper());
                    RsltData = eqHsdio.GetNumExecErrors(nParamClass);
                    SetResult("M_ErrorBits_" + nParamClass, RsltData);

                    Thread.Sleep(2);
                    eqHsdio.SendVector("VIOOFF");
                    Thread.Sleep(1);
                    eqHsdio.SendVector("VIOON");
                    Thread.Sleep(1);
                    break;

                case "QC":
                    Eq.Site[0].HSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                        ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                        : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    eqHsdio.SendVector(powerMode);
                    RsltData = eqHsdio.GetNumExecErrors(powerMode);
                    SetResult("M_QCErrorBits_" + powerMode, RsltData);
                    break;
                case "CM_ID":
                    SendVectorMipi1(eqHsdio);
                    //(20-Nov-2017)
                    TempMipiRead = eqHsdio.RegRead("E4");
                    MipiRead = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    // bit7 is CM ID
                    int cm_ID = (((MipiRead & 0x80) >>
                                  7)); //bit7 is CM ID                                                                  
                    //(MipiRead & 0x8000) == 0x8000? 1 : 0;
                    RsltData = cm_ID;
                    SetResult("M_MIPI_CM-ID", RsltData);
                    break;
                case "READREG":
                    Eq.Site[0].HSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                        ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                        : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    Eq.Site[0].HSDIO.dutSlavePairIndex = (powerMode.Contains("TX") ? 1 : 2);
                    TempMipiRead = eqHsdio.RegRead(powerMode.Substring(2));

                    int result = (int.Parse(TempMipiRead, System.Globalization.NumberStyles.HexNumber));
                    string resultHeader = "M_MIPI_ReadTxReg-" + powerMode.Replace("TX", "");
                    if (powerMode.Contains("RX"))
                    {
                        resultHeader = "M_MIPI_ReadRxReg-" + powerMode.Replace("RX", "");
                    }
                    SetResult(resultHeader, result);
                    break;
                case "OTP_BURN_REV_ID_TX":
                case "OTP_BURN_REV_ID_RX":
                    bool doBurn = ResultBuilder.FailedTests[0].Count == 0;
                    Eq.Site[0].HSDIO.dutSlaveAddress = (powerMode.Contains("TX")
                        ? eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"]
                        : eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"]);
                    Eq.Site[0].HSDIO.SendVector(EqHSDIO.Reset);
                    result = 0;

                    if (doBurn)
                    {
                      result = OTP_Procedure.OTP_Burn_Custom(0, zMIPI_Commands);
                    }

                    SetResult(nParamClass, result);
                    break;

                ////ChoonChin - 20191125 - Add Tx and Rx wafer info
                //case "M_MIPI_CMOS-TX-X":
                //    SendVectorMipi1(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_TX_X(0);
                //    SetResult("M_MIPI_CMOS-TX-X", MipiRead);
                //    break;
                //case "M_MIPI_CMOS-TX-Y":
                //    SendVectorMipi1(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_TX_Y(0);
                //    SetResult("M_MIPI_CMOS-TX-Y", MipiRead);
                //    break;
                //case "M_MIPI_CMOS-TX-WAFER-ID":
                //    SendVectorMipi1(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_WAFER_ID(0);
                //    SetResult("M_MIPI_CMOS-TX-WAFER-ID", MipiRead);
                //    break;
                //case "M_MIPI_CMOS-TX-WAFER-LOT":
                //    SendVectorMipi1(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_WAFER_LOT(0);
                //    SetResult("M_MIPI_CMOS-TX-WAFER-LOT", MipiRead);
                //    break;
                //case "M_MIPI_LNA-X":
                //    SendVectorMipi2(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_LNA_X(0);
                //    SetResult("M_MIPI_LNA-X", MipiRead);
                //    break;
                //case "M_MIPI_LNA-Y":
                //    SendVectorMipi2(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_LNA_Y(0);
                //    SetResult("M_MIPI_LNA-Y", MipiRead);
                //    break;
                //case "M_MIPI_LNA-WAFER-ID":
                //    SendVectorMipi2(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_ID(0);
                //    SetResult("M_MIPI_LNA-WAFER-ID", MipiRead);
                //    break;
                //case "M_MIPI_LNA-WAFER-LOT":
                //    SendVectorMipi2(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_LOT(0);
                //    SetResult("M_MIPI_LNA-WAFER-LOT", MipiRead);
                //    break;
                //case "M_FLAG_LOCKBIT_RX":
                //    SendVectorMipi2(eqHsdio);
                //    MipiRead = OTP_Procedure.OTP_Read_Rx_Lockbit(0);
                //    SetResult("M_FLAG_LOCKBIT_RX", MipiRead);
                //    break;

                default:
                    break;
            }

            return SaveResult;
        }

        public void SendMipiCommandTest0(int testNo, EqHSDIO.EqHSDIObase eqHsdio, List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands)
        {
            bool isAvailableEquipmentHsdio = Eq.Site[0].HSDIO != null;
            if (!isAvailableEquipmentHsdio)
            {
                MPAD_TestTimer.LoggingManager.Instance.LogWarningTestPlan("Warning, HSDIO MIPI Command not executed.");
                return;
            }

            MPAD_TestTimer.StopWatchManager.Instance.Start("SendMIPICommands",0);
            if (testNo == 0) eqHsdio.SendMipiCommands(zMIPI_Commands);
            eqHsdio.SendMipiCommands(zMIPI_Commands);
            MPAD_TestTimer.StopWatchManager.Instance.Stop("SendMIPICommands",0);
        }

        private void SetResult(string header, double resultValue)
        {
            SaveResult.SetValue(header, resultValue);
        }

        private float HsdioSendVectorTemp()
        {
            int Temp = 0;
            HSDIO.Instrument.SendVectorTEMP(ref Temp);
            string Hex = Convert.ToString(Convert.ToInt32(Temp), 16).ToUpper();
            int TempCount = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
            float RsltData = (TempCount - 1) * (Convert.ToSingle(150f / 254f)) - 20;
            return RsltData;
        }

        private void SendVectorMipi1(EqHSDIO.EqHSDIObase eqHsdio)
        {
            Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.dutSlavePairIndex = 1;

            eqHsdio.RegWrite("1C", "40"); // Mipi Reset

            eqHsdio.SendVector("VIOOFF");
            Thread.Sleep(1);
            eqHsdio.SendVector("VIOON");
            Thread.Sleep(1);

            eqHsdio.RegWrite("2B", "0F"); // Sclk Cap tunning                }
        }

        private void SendVectorMipi2(EqHSDIO.EqHSDIObase eqHsdio)
        {
            Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.dutSlavePairIndex = 2;

            eqHsdio.RegWrite("1C", "40"); // Mipi Reset

            eqHsdio.SendVector("VIOOFF");
            Thread.Sleep(1);
            eqHsdio.SendVector("VIOON");
            Thread.Sleep(1);

            eqHsdio.RegWrite("2B", "0F"); // Sclk Cap tunning                }
        }

    }

    public class OtpTest
    {
        private s_Result SaveResult;

        public s_Result RunTest(string nParamClass, EqHSDIO.EqHSDIObase eqHsdio)
        {
            SaveResult = new s_Result();

            int MipiRead;

            switch (nParamClass)
            {
                case "CMOS_TX_X":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_TX_X(0);
                    SetResult("M_MIPI_CMOS-TX-X", MipiRead);
                    break;
                case "CMOS_TX_Y":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_TX_Y(0);
                    SetResult("M_MIPI_CMOS-TX-Y", MipiRead);
                    break;
                case "CMOS_TX_WAFERLOT":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_WAFER_LOT(0);
                    SetResult("M_MIPI_CMOS-TX-WAFER-LOT", MipiRead);
                    break;
                case "CMOS_TX_WAFERID":
                    SendVectorMipi1(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_WAFER_ID(0);
                    SetResult("M_MIPI_CMOS-TX-WAFER-ID", MipiRead);
                    break;
                case "LNA_RX_X":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_X(0);
                    SetResult("M_MIPI_LNA-X", MipiRead);
                    break;
                case "LNA_RX_Y":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_Y(0);
                    SetResult("M_MIPI_LNA-Y", MipiRead);
                    break;
                case "LNA_RX_WAFERLOT":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_LOT(0);
                    SetResult("M_MIPI_LNA-WAFER-LOT", MipiRead);
                    break;
                case "LNA_RX_WAFERID":
                    SendVectorMipi2(eqHsdio);
                    MipiRead = OTP_Procedure.OTP_Read_LNA_WAFER_ID(0);
                    //ChoonChin - Mask out lockbit 1101 1111
                    MipiRead = MipiRead & 223;
                    SetResult("M_MIPI_LNA-WAFER-ID", MipiRead);
                    break;
                default:
                    break;
            }

            return SaveResult;
        }

        private void SetResult(string header, double resultValue)
        {
            SaveResult.SetValue(header, resultValue);
        }

        private void SendVectorMipi1(EqHSDIO.EqHSDIObase eqHsdio)
        {
            Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI1_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.dutSlavePairIndex = 1;
            eqHsdio.RegWrite("1C", "40"); // Mipi Reset

            eqHsdio.SendVector("VIOOFF");
            Thread.Sleep(1);
            eqHsdio.SendVector("VIOON");
            Thread.Sleep(1);

            eqHsdio.RegWrite("2B", "0F"); // Sclk Cap tunning                }
        }

        private void SendVectorMipi2(EqHSDIO.EqHSDIObase eqHsdio)
        {
            Eq.Site[0].HSDIO.dutSlaveAddress = eqHsdio.Digital_Definitions["MIPI2_SLAVE_ADDR"];
            Eq.Site[0].HSDIO.dutSlavePairIndex = 2;

            eqHsdio.RegWrite("1C", "40"); // Mipi Reset

            eqHsdio.SendVector("VIOOFF");
            Thread.Sleep(1);
            eqHsdio.SendVector("VIOON");
            Thread.Sleep(1);

            eqHsdio.RegWrite("2B", "0F"); // Sclk Cap tunning                }
        }
    }

}
