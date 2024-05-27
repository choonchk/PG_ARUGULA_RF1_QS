using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqLib;
using System.Windows.Forms;

namespace LibFBAR_TOPAZ
{
    public class PinotRF2 : ProjectSpecificFactor.Projectbase
    {
        int NPort = 6;
        SwitchCondtion cTemp = new SwitchCondtion();

        public PinotRF2()
        {
            ProjectSpecificFactor.TopazCalPower = 10;

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

            dicRxOutNamemapping.Add(3, "PRX_OUT1-N");
            dicRxOutNamemapping.Add(4, "PRX_OUT2-N");
            dicRxOutNamemapping.Add(5, "PRX_OUT3-N");
            dicRxOutNamemapping.Add(6, "PRX_OUT4-N");

            //Switch Ch1 
            dicSwitchConfig.Add("OUT-DRX", EqLib.Operation.N3toRx);
            dicSwitchConfig.Add("IN-MLB", EqLib.Operation.N4toRx);
            dicSwitchConfig.Add("IN-GSM", EqLib.Operation.N6toRx);
            dicSwitchConfig.Add("OUT-MIMO", EqLib.Operation.N5toRx);

            dicSwitchConfig.Add("OUT1", EqLib.Operation.N3toRx);
            dicSwitchConfig.Add("OUT2", EqLib.Operation.N4toRx);
            dicSwitchConfig.Add("OUT3", EqLib.Operation.N5toRx);
            dicSwitchConfig.Add("OUT4", EqLib.Operation.N6toRx);

            dicSwitchConfig.Add("IN-MB1", EqLib.Operation.N1toTx);
            dicSwitchConfig.Add("IN-HB1", EqLib.Operation.N1toTx);

            dicSwitchConfig.Add("ANT1", EqLib.Operation.N2toAnt);
            dicSwitchConfig.Add("ANT2", EqLib.Operation.N2toAnt);
            dicSwitchConfig.Add("ANT-UAT", EqLib.Operation.N2toAnt);
            dicSwitchConfig.Add("ANT-21", EqLib.Operation.N2toAnt);
        }

        public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string RxPort = null)
        {
            bool Sw_Test = true;

            try
            {
                if ((RxPort.Contains("MLB")) || (RxPort.Contains("GSM")) || (RxPort.Contains("DRX")) || (RxPort.Contains("MIMO")))
                {
                    if (TxPort.ToUpper() != "X") Eq.Site[0].SwMatrix.ActivatePath(TxPort, dicSwitchConfig[TxPort]);

                    Eq.Site[0].SwMatrix.ActivatePath(AntPort, dicSwitchConfig[AntPort]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", dicSwitchConfig["OUT-DRX"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", dicSwitchConfig["IN-MLB"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", dicSwitchConfig["IN-GSM"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", dicSwitchConfig["OUT-MIMO"]);
                }
                else
                {
                    if (TxPort.ToUpper() != "X")  Eq.Site[0].SwMatrix.ActivatePath(TxPort, dicSwitchConfig[TxPort]);
                    Eq.Site[0].SwMatrix.ActivatePath(AntPort, dicSwitchConfig[AntPort]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT1", dicSwitchConfig["OUT1"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT2", dicSwitchConfig["OUT2"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT3", dicSwitchConfig["OUT3"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT4", dicSwitchConfig["OUT4"]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Please Check Switch Path" + "\r\n" + e.ToString());
                return Sw_Test = false;
            }


            return Sw_Test;
        }
        public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string RxPort, bool isNormalPath = true)
        {
            bool Sw_Test = true;

            try
            {
                Eq.Site[0].SwMatrix.ActivatePath(TxPort, dicSwitchConfig[TxPort]);

                if (AntPort.Contains("NS-") && isNormalPath)
                {
                    string temp = AntPort.Split('-')[1];
                    Eq.Site[0].SwMatrix.ActivatePath(temp, dicSwitchConfig[temp]);
                }
                else
                {
                    Eq.Site[0].SwMatrix.ActivatePath(AntPort.ToUpper(), dicSwitchConfig[AntPort]);
                }

                Eq.Site[0].SwMatrix.ActivatePath("OUT1", dicSwitchConfig["OUT1"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT2", dicSwitchConfig["OUT2"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT3", dicSwitchConfig["OUT3"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT4", dicSwitchConfig["OUT4"]);

                if ((RxPort.Contains("MLB")) || (RxPort.Contains("GSM")) || (RxPort.Contains("DRX")) || (RxPort.Contains("MIMO")))
                {
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", dicSwitchConfig["OUT-DRX"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", dicSwitchConfig["IN-MLB"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", dicSwitchConfig["IN-GSM"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", dicSwitchConfig["OUT-MIMO"]);
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
}
