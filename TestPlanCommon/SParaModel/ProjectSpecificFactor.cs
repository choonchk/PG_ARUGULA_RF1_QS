using System;
using System.Collections.Generic;
using EqLib;
using LibFBAR_TOPAZ.ANewEqLib;

namespace TestPlanCommon.SParaModel
{
    public class ProjectSpecificFactor // Actually used 
    {
        public static bool CalContinue { get; set; }
        public static bool CalDone { get; set; }
        public static List<string> CalSegment { get; set; }
        public static Dictionary<string, bool> cCalSelection { get; set; }
        public static bool Fbar_cal { get; set; }
        public static int iCalCount { get; set; }
        public static bool Using28Ohm { get; set; }
        /// <summary>
        /// No longer there. Relocated.
        /// </summary>
        public static int TopazCalPower { get; set; }
        
        public static Projectbase cProject;

        public static void SetProject(Projectbase projectAndType)
        {
            cProject = projectAndType;
            cProject.SetEquipment(Eq.Site[0].SwMatrix);
        }

        public abstract class Projectbase
        {
            public bool[] PortEnable = new bool[10];
            public bool threadLockActivatePath;

            public int FirstTrigOfRaw;
            public int CalColmnIndexNFset;
            public int TraceColmnIndexNFset;
            public string[] listLNAReg;
            protected EqSwitchMatrix.EqSwitchMatrixBase m_eqsm;

            public Dictionary<int, string> dicTraceIndexTable = new Dictionary<int, string>();
            public Dictionary<int, string> dicRxOutNamemapping = new Dictionary<int, string>();
            public Dictionary<string, EqLib.Operation> dicSwitchConfig = new Dictionary<string, EqLib.Operation>();


            public class SwitchCondtion
            {
                public EqLib.Operation[] TempPor = new EqLib.Operation[10];
                public bool[] isEnabel = new bool[10];
                public string[] strDefined = new string[10];
            }

            public abstract bool SetSwitchMatrixPaths(string TxPort, string AntPort,string Rxport = null );
            public abstract bool SetSwitchMatrixPaths(string TxPort, string AntPort,string RxPort, bool isNormalPath = true);

            public virtual void SetEquipment(EqSwitchMatrix.EqSwitchMatrixBase equipment)
            {
                m_eqsm = equipment;
            }

        }

        public class Example : ProjectSpecificFactor.Projectbase
        {
            int NPort = 9; //Number of Ports
            SwitchCondtion cTemp = new SwitchCondtion();

            public Example()
            {
                ProjectSpecificFactor.TopazCalPower = 10;

                int index = 0;
                foreach (bool _port in PortEnable)
                {
                    if (index < NPort) PortEnable[index] = true;
                    index++;
                }

                FirstTrigOfRaw = 1; //Tcf Trigger Start Index
                CalColmnIndexNFset = 15; // Cal proccesure NF settting Colmn start Index
                TraceColmnIndexNFset = 91; // Trace NF settting Colmn start Index

                //Trace Setting 
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S11"), "S11");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S22"), "S22");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S33"), "S33");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S44"), "S44");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S55"), "S55");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S66"), "S66");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S77"), "S77");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S88"), "S88");
                dicTraceIndexTable.Add((int)Enum.Parse(typeof(e_SParametersDef), "S99"), "S99");

                //RxOUT Port Define
                dicRxOutNamemapping.Add(6, "OUT1");
                dicRxOutNamemapping.Add(7, "OUT2");
                dicRxOutNamemapping.Add(8, "OUT3");
                dicRxOutNamemapping.Add(9, "OUT4");

                //Switch Define
                dicSwitchConfig.Add("OUT-DRX", EqLib.Operation.N6toRx);
                dicSwitchConfig.Add("IN-MLB", EqLib.Operation.N7toRx);
                dicSwitchConfig.Add("IN-GSM", EqLib.Operation.N8toRx);
                dicSwitchConfig.Add("OUT-MIMO", EqLib.Operation.N9toRx);

                dicSwitchConfig.Add("OUT1", EqLib.Operation.N6toRx);
                dicSwitchConfig.Add("OUT2", EqLib.Operation.N7toRx);
                dicSwitchConfig.Add("OUT3", EqLib.Operation.N8toRx);
                dicSwitchConfig.Add("OUT4", EqLib.Operation.N9toRx);

                dicSwitchConfig.Add("IN-MB1", EqLib.Operation.N1toTx);
                dicSwitchConfig.Add("IN-MB2", EqLib.Operation.N1toTx);
                dicSwitchConfig.Add("IN-HB1", EqLib.Operation.N2toTx);
                dicSwitchConfig.Add("IN-HB2", EqLib.Operation.N2toTx);

                dicSwitchConfig.Add("ANT1", EqLib.Operation.N3toAnt);
                dicSwitchConfig.Add("ANT2", EqLib.Operation.N4toAnt);
                dicSwitchConfig.Add("ANT3", EqLib.Operation.N5toAnt);

            }
            
            public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string Rxport = null)
            {
                return true;
            }
            public override bool SetSwitchMatrixPaths(string TxPort, string AntPort, string Rxport, bool isNormalPath = true)
            {
                return true;
            }
        }

    }

    public class ProjectSpecificFactorDataObject
    {
        public int FirstTrigOfRaw { get; set; }
        public string[] listLNAReg;
        public bool[] PortEnable { get; set; }
        public int CalColmnIndexNFset { get; set; }
        public CalibrationInputDataObject CalibrationInputDataObject { get; set; }

        public ProjectSpecificFactorDataObject()
        {
            PortEnable = new bool[10];
        }
    }


}
