using System;
using System.Collections.Generic;
using EqLib;
using MPAD_TestTimer;
using TestPlanCommon.CommonModel;
using System.Windows.Forms;
using LibFBAR_TOPAZ.ANewEqLib;
using TestPlanCommon.SParaModel;

namespace TestPlan_NathanFull
{
    /// <summary>
    /// S-Para project specific configuration.
    /// </summary>
    public class PinotRF2 : ProjectSpecificFactor.Projectbase
    {
        int NPort = 6;
        SwitchCondtion cTemp = new SwitchCondtion();

        public PinotRF2()
        {
            ProjectSpecificFactor.TopazCalPower = 4; //10;
            //THIS IS THE PORT POWER
            int index = 0;
            foreach (bool _port in PortEnable)
            {
                if (index < NPort) PortEnable[index] = true;
                index++;
            }

            //Defined LNA Reg List 
            listLNAReg = new string[] { "RXREG0B", "RXREG0D", "RXREG0F", "RXREG11", "RXREG13" };

            FirstTrigOfRaw = 1;
            CalColmnIndexNFset = 12;
            TraceColmnIndexNFset = 44;

            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S11"), "S11");
            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S22"), "S22");
            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S33"), "S33");
            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S44"), "S44");
            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S55"), "S55");
            dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S66"), "S66");

            dicRxOutNamemapping.Add(3, "OUT1-N77");
            dicRxOutNamemapping.Add(6, "OUT3-N77");
            dicRxOutNamemapping.Add(5, "OUT1-N79");
            dicRxOutNamemapping.Add(4, "OUT2-N79");

            //Switch Ch1 
            //dicSwitchConfig.Add("OUT-DRX", EqLib.Operation.N3toRx);
            //dicSwitchConfig.Add("IN-MLB", EqLib.Operation.N4toRx);
            //dicSwitchConfig.Add("IN-GSM", EqLib.Operation.N6toRx);
            //dicSwitchConfig.Add("OUT-MIMO", EqLib.Operation.N5toRx);

            //dicSwitchConfig.Add("OUT1", EqLib.Operation.N3toRx);
            //dicSwitchConfig.Add("OUT2", EqLib.Operation.N4toRx);
            //dicSwitchConfig.Add("OUT3", EqLib.Operation.N5toRx);
            //dicSwitchConfig.Add("OUT4", EqLib.Operation.N6toRx);

            //dicSwitchConfig.Add("IN-MB1", EqLib.Operation.N1toTx);
            //dicSwitchConfig.Add("IN-HB1", EqLib.Operation.N1toTx);

            //dicSwitchConfig.Add("ANT1", EqLib.Operation.N2toAnt);
            //dicSwitchConfig.Add("ANT2", EqLib.Operation.N2toAnt);
            //dicSwitchConfig.Add("ANT-UAT", EqLib.Operation.N2toAnt);
            //dicSwitchConfig.Add("ANT-21", EqLib.Operation.N2toAnt);

            dicSwitchConfig.Add("IN1-N77", Operation.N1toTx); 
            dicSwitchConfig.Add("IN2-N79", Operation.N1toTx); 
            dicSwitchConfig.Add("ANT1", Operation.N2toAnt);
            dicSwitchConfig.Add("ANT1_N77", Operation.N2toAnt);
            dicSwitchConfig.Add("ANT1_N79", Operation.N2toAnt);
            dicSwitchConfig.Add("OUT1-N77", Operation.N3toRx); 
            dicSwitchConfig.Add("OUT2-N79", Operation.N4toRx); 
            dicSwitchConfig.Add("OUT1-N79", Operation.N5toRx); 
            dicSwitchConfig.Add("OUT-FBRX", Operation.N5toRx); 
            dicSwitchConfig.Add("OUT3-N77", Operation.N6toRx); 
            dicSwitchConfig.Add("ANT2", Operation.N2toAnt);
            dicSwitchConfig.Add("ANT2_N77", Operation.N2toAnt);
            dicSwitchConfig.Add("ANT2_N79", Operation.N2toAnt);
            dicSwitchConfig.Add("OUT-N77N79", Operation.N2toAnt); 
            dicSwitchConfig.Add("IN-FBRX", Operation.N2toAnt);
            dicSwitchConfig.Add("NS-ANT1_N77", Operation.NS1toAnt1);
            dicSwitchConfig.Add("NS-ANT1_N79", Operation.NS1toAnt1);
            dicSwitchConfig.Add("NS-ANT2_N77", Operation.NS2toAnt2);
            dicSwitchConfig.Add("NS-ANT2_N79", Operation.NS2toAnt2);
            dicSwitchConfig.Add("NS-ANT1", Operation.NS1toAnt1);
            dicSwitchConfig.Add("NS-ANT2", Operation.NS2toAnt2);
        }

        public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string RxPort = null)
        {
            bool Sw_Test = true;

            try
            {
                if (TxPort != "") m_eqsm.ActivatePath(TxPort, dicSwitchConfig[TxPort]);
                if (AntPort != "") m_eqsm.ActivatePath(AntPort, dicSwitchConfig[AntPort]);
                if (RxPort != "") m_eqsm.ActivatePath(RxPort, dicSwitchConfig[RxPort]);

                //if ((RxPort.Contains("MLB")) || (RxPort.Contains("GSM")) || (RxPort.Contains("DRX")) || (RxPort.Contains("MIMO")))
                //{
                //    if (TxPort.ToUpper() != "X") m_eqsm.ActivatePath(TxPort, dicSwitchConfig[TxPort]);

                //    m_eqsm.ActivatePath(AntPort, dicSwitchConfig[AntPort]);
                //    m_eqsm.ActivatePath("OUT-DRX", dicSwitchConfig["OUT-DRX"]);
                //    m_eqsm.ActivatePath("IN-MLB", dicSwitchConfig["IN-MLB"]);
                //    m_eqsm.ActivatePath("IN-GSM", dicSwitchConfig["IN-GSM"]);
                //    m_eqsm.ActivatePath("OUT-MIMO", dicSwitchConfig["OUT-MIMO"]);
                //}
                //else
                //{
                //    if (TxPort.ToUpper() != "X") m_eqsm.ActivatePath(TxPort, dicSwitchConfig[TxPort]);
                //    m_eqsm.ActivatePath(AntPort, dicSwitchConfig[AntPort]);
                //    m_eqsm.ActivatePath("OUT1", dicSwitchConfig["OUT1"]);
                //    m_eqsm.ActivatePath("OUT2", dicSwitchConfig["OUT2"]);
                //    m_eqsm.ActivatePath("OUT3", dicSwitchConfig["OUT3"]);
                //    m_eqsm.ActivatePath("OUT4", dicSwitchConfig["OUT4"]);
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show("SetSwitchMarixPaths Error" + "\r\n" + e.ToString());
                return Sw_Test = false;
            }


            return Sw_Test;
        }
        public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string RxPort, bool isNormalPath = true)
        {
            bool Sw_Test = true;

            try
            {
                m_eqsm.ActivatePath(TxPort, dicSwitchConfig[TxPort]);

                if (AntPort.Contains("NS-") && isNormalPath)
                {
                    string temp = AntPort.Split('-')[1];
                    m_eqsm.ActivatePath(temp, dicSwitchConfig[temp]);
                }
                else
                {
                    m_eqsm.ActivatePath(AntPort.ToUpper(), dicSwitchConfig[AntPort]);
                }

                m_eqsm.ActivatePath("OUT1", dicSwitchConfig["OUT1"]);
                m_eqsm.ActivatePath("OUT2", dicSwitchConfig["OUT2"]);
                m_eqsm.ActivatePath("OUT3", dicSwitchConfig["OUT3"]);
                m_eqsm.ActivatePath("OUT4", dicSwitchConfig["OUT4"]);

                if ((RxPort.Contains("MLB")) || (RxPort.Contains("GSM")) || (RxPort.Contains("DRX")) || (RxPort.Contains("MIMO")))
                {
                    m_eqsm.ActivatePath("OUT-DRX", dicSwitchConfig["OUT-DRX"]);
                    m_eqsm.ActivatePath("IN-MLB", dicSwitchConfig["IN-MLB"]);
                    m_eqsm.ActivatePath("IN-GSM", dicSwitchConfig["IN-GSM"]);
                    m_eqsm.ActivatePath("OUT-MIMO", dicSwitchConfig["OUT-MIMO"]);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("Please Check Switch Path" + "\r\n" + e.ToString());
                return Sw_Test = false;
            }

            return Sw_Test;
        }
    }

    public class SparaManualCalSwitchingModel
    {
        private EqSwitchMatrix.EqSwitchMatrixBase m_eqsm;

        public SparaManualCalSwitchingModel(EqSwitchMatrix.EqSwitchMatrixBase equipment)
        {
            m_eqsm = equipment;
        }

        public bool Run_Manual_SW(string Sw_Band)
        {

            bool Sw_Test = true;
            m_eqsm.ActivatePath(Sw_Band, Operation.ENAtoRFIN);
            m_eqsm.ActivatePath(Sw_Band, Operation.ENAtoRFOUT);
            m_eqsm.ActivatePath(Sw_Band, Operation.ENAtoRX);

            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N1toTx, zswitches);
            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N2toAnt, zswitches);
            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N3toRx, zswitches);
            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N4toRx, zswitches);
            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N5toRx, zswitches);
            //clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.N6toRx, zswitches);


            return Sw_Test;
        }

        public bool Run_Manual_SW2(string BandPort)
        {

            bool Sw_Test = true;
            PinotRF2 sm = new PinotRF2();
            try
            {
                if (BandPort != "") m_eqsm.ActivatePath(BandPort, sm.dicSwitchConfig[BandPort]);

                return Sw_Test;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Please Check Switch Path" + "\r\n" + e.ToString());
                return Sw_Test = false;
            }
        }
    }

    public class SParaEquipmentInitializer : IEquipmentInitializer
    {
        private ITesterSite m_modelTester;
        private HsdioModel m_modelEqHsdio_spara;

        public void SetTester(ITesterSite tester)
        {
            m_modelEqHsdio_spara = new HsdioModel();
            m_modelTester = tester;
        }

        public void InitializeSwitchMatrix(bool isMagicBox)
        {
            SwitchMatrixModel sw = new SwitchMatrixModel();
            sw.InitializeSwitchMatrix2(m_modelTester);
        }

        public void InitializeSwitchMatrix(bool isMagicBox, string Instr_Alias="")
        {
            if (Instr_Alias == "") Instr_Alias = GlobalVariables.DIOAlias;
            SwitchMatrixModel sw = new SwitchMatrixModel();
            sw.InitializeSwitchMatrix2(m_modelTester, Instr_Alias);
        }

        public Dictionary<string, string> Digital_Definitions_Part_Specific 
        {
            get
            {
                return m_modelEqHsdio_spara.Digital_Definitions_Part_Specific;
            }
        }
        public bool InitializeHSDIO()
        {
            return m_modelEqHsdio_spara.InitializeHSDIO2(m_modelTester);
        }

        public bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion,
           Dictionary<string, string> TXQC, Dictionary<string, string> RXQC, TestLib.MipiTestConditions testConditions)
        {
            return true;
        }

        public bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion)
        {
            return m_modelEqHsdio_spara.LoadVector(clothoRootDir, tcfCmosDieType, sampleVersion);
            // do nothing for S-Para.
            //return true;
        }      

        public void InitializeSmu()
        {
            InitializeSmuJoker();
        }

        public void InitializeRF()
        {
            throw new NotImplementedException();
        }

        public ValidationDataObject InitializeDC(ClothoLibAlgo.Dictionary.Ordered<string, string[]> DcResourceTempList)
        {
            throw new NotImplementedException();
        }

        public void InitializeChassis()
        {
            throw new NotImplementedException();
        }

        public void InitializeHandler(string handlerType, string visaAlias)
        {
            Eq.Handler = EqHandler.Get(!TestLib.ResultBuilder.LiteDriver, handlerType);
            Eq.Handler.Initialize(visaAlias);
        }

        private void InitializeSmuHls2()
        {
            // Tester specific Vcc Vbatt Vdd.
            List<KeyValuePair<string, string>> testerSpecificList = m_modelTester.GetSmuSetting();
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata1", "NI6570.5"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk1", "NI6570.7"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio1", "NI6570.4"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata2", "NI6570.9"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk2", "NI6570.11"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio2", "NI6570.10"));
            testerSpecificList.AddRange(DCpinSmuResourceTempList);

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                foreach (KeyValuePair<string, string> kv in testerSpecificList)
                {
                    string DcPinName = kv.Key;
                    string pinName = DcPinName.Remove(0, 2);
                    string VisaAlias = kv.Value.Split('.')[0];
                    string Chan = kv.Value.Split('.')[1];

                    DcPinName = DcPinName.Replace("V.", "").Trim();
                    //EqDC.iEqDC dcValue = EqDC.Get(VisaAlias, Chan, DcPinName, site, true);
                    // Case HLS2, differences in NI6570 which has not been merged.
                    EqDC.iEqDC dcValue;
                    if (VisaAlias == "NI6570")
                    {
                        dcValue = new EqLib.EqDC.NI6570Pmu(VisaAlias, Chan, DcPinName, site);
                    }
                    else
                    {
                        dcValue = EqDC.Get(VisaAlias, Chan, DcPinName, site, true);
                    }

                    Eq.Site[site].DC.Add(DcPinName, dcValue); //This is what feeds InitializeHSDIO
                }
            }
        }

        /// <summary>
        /// Joker variant.
        /// </summary>
        private void InitializeSmuJoker()
        {
            //Night Hawk Demo Board settings - MM 01/05/2020
            //// Tester specific Vcc Vbatt Vdd.
            //List<KeyValuePair<string, string>> testerSpecificList = m_modelTester.GetSmuSetting();
            //List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk1", "NI6570.7"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata1", "NI6570.5"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio1", "NI6570.4"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk2", "NI6570.11"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata2", "NI6570.9"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio2", "NI6570.3"));
            //testerSpecificList.AddRange(DCpinSmuResourceTempList);

            //Night Hawk Load Board settings - MM 01/05/2020
            List<KeyValuePair<string, string>> testerSpecificList = m_modelTester.GetSmuSetting();
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk1", "NI6570.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata1", "NI6570.1"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio1", "NI6570.2"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk2", "NI6570.4"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata2", "NI6570.5"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio2", "NI6570.3"));
            testerSpecificList.AddRange(DCpinSmuResourceTempList);

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                foreach (KeyValuePair<string, string> kv in testerSpecificList)
                {
                    string DcPinName = kv.Key;
                    string pinName = DcPinName.Remove(0, 2);
                    string VisaAlias = kv.Value.Split('.')[0];
                    string Chan = kv.Value.Split('.')[1];

                    DcPinName = DcPinName.Replace("V.", "").Trim();
                    //EqDC.iEqDC dcValue = EqDC.Get(VisaAlias, Chan, DcPinName, site, true);
                    // Case HLS2, differences in NI6570 which has not been merged.
                    EqDC.iEqDC dcValue;
                    if (VisaAlias == "NI6570")
                    {
                        dcValue = new EqLib.EqDC.NI6570Pmu(VisaAlias, Chan, DcPinName, site);
                    }
                    else
                    {
                        dcValue = EqDC.Get(VisaAlias, Chan, DcPinName, site, true);
                    }

                    Eq.Site[site].DC.Add(DcPinName, dcValue); //This is what feeds InitializeHSDIO
                }
            }
        }
    }
}

