using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TestLib;

namespace LibFBAR.DC
{
    #region "Enumeration Declaration"
    public enum e_DC_PS_Equipment
    {
        PS4142 = 0,
        AEMULUS,
        PS6626,
        NI_SMU
    }
    #endregion
    #region "Structure"
    public struct s_DC_Set
    {
        public int Channel;
        public double Voltage;
        public double Current;
        public bool b_Enable;
        public string Header;
        public int Mode;
        public int Pos_Neg;
        public int On_Off;
        public int Range;
    }
    public struct s_DC_Match
    {
        public e_DC_PS_Equipment PS_Name;
        public int[] Ch_Match;
        public string Address;
        public int Aemulus_SubChannel;
        public int AM371_ChannelIndex;
    }
    #endregion
    public class cDC_common
    {
        public static string ClassName = "DC Common Class";
        //public e_DC_PS_Equipment PS_Equipment;   // Do not Static, if multiple of the same equipment is used
        LibFBAR.cGeneral general = new LibFBAR.cGeneral();

        //public string Address;

        private static DC.cAemulus_AM330[] AM330;
        private static DC.cDC4142[] DC4142;
           

        public int TotalChannel;
        //public int DC_Set;
        private static s_DC_Match[] DC_matching; 
        public s_DC_Set[] DC_Setting;
        //public int Sleep_ms;
        public int[] Bias;
        public int[] Read;
        private int[] RsltData;
        private double[] Results_Array;
        private int[] NoBias;
        //private int Aemulus_SubChannel;
        //private int AM371_ChannelIndex;

        public float NPLC;

        public e_DC_PS_Equipment Set_PS_Equipment(string Equipment_Str)
        {
            e_DC_PS_Equipment tmpPS;
            switch (Equipment_Str.ToUpper())
            {
                case "PS4142":
                case "AEMULUS":
                case "PS6626":
                case "NI_SMU":
                    tmpPS = (e_DC_PS_Equipment)Enum.Parse(typeof(e_DC_PS_Equipment), Equipment_Str, true);
                    break;
                default:
                    general.DisplayError(ClassName, "Unable to Set Power Supply Equipment Correctly",
                                                    "Please Check the Power Supply Defination Settings \r\nDefined Setting : " + Equipment_Str);
                    tmpPS = e_DC_PS_Equipment.AEMULUS;
                    break;
            }
            return (tmpPS);
        }
        public void Parse_IO(int PS_Set, int hSys)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {
                AM330[PS_Set - 1].Parse_hSys = hSys;
            }
            else
            {
                general.DisplayError(ClassName, "Power Supply Settings Mismatch", "Power Supply Set : " + PS_Set.ToString() + "\r\nExpected Power Supply Model : " + DC_matching[PS_Set - 1].PS_Name.ToString());
            }
        }
        public void Parse_IO(int PS_Set, Ivi.Visa.Interop.FormattedIO488 IO)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.PS4142)
            {
                DC4142[PS_Set - 1].parseIO = IO;
            }
            else
            {
                general.DisplayError(ClassName, "Power Supply Settings Mismatch", "Power Supply Set : " + PS_Set.ToString() + "\r\nExpected Power Supply Model : " + DC_matching[PS_Set - 1].PS_Name.ToString());
            }
        }
        public void Init()
        {
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                switch (DC_matching[PS_Set].PS_Name)
                {
                    case e_DC_PS_Equipment.AEMULUS:
                        AM330[PS_Set] = new LibFBAR.DC.cAemulus_AM330();
                        AM330[PS_Set].CREATESESSION(null);
                        AM330[PS_Set].AMB1340C_CREATEINSTANCE(Convert.ToInt32(DC_matching[PS_Set].Address), 0);
                        AM330[PS_Set].System.INITIALIZE();

                        break;
                    case e_DC_PS_Equipment.PS4142:
                        DC4142[PS_Set] = new LibFBAR.DC.cDC4142();
                        DC4142[PS_Set].Address = DC_matching[PS_Set].Address;
                        DC4142[PS_Set].OpenIO();
                        break;
                }
            }
        }
        public void Init_Pxi()
        {
                         
        }
        public void UnInit()
        {
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                switch (DC_matching[PS_Set].PS_Name)
                {
                    case e_DC_PS_Equipment.AEMULUS:
                        AM330[PS_Set].System.INITIALIZE();
                        AM330[PS_Set].CLOSESESSION();
                        AM330[PS_Set].AMB1340C_DELETEINSTANCE();
                        break;
                }
            }
            
        }
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.0.01";        //  15/1/2011       KKL             Driver for Power Supply Common Class

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        public void Init_Bias_Array()
        {
            int ChnSet;
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                ChnSet = 0;
                //DC_Setting = new s_DC_Set[TotalChannel];
                if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.AEMULUS)
                {
                    Bias = new int[DC_matching[PS_Set].Aemulus_SubChannel * 6];

                    for (int iSet = 0; iSet < DC_matching[PS_Set].Ch_Match.Length; iSet++)
                    {
                        if (DC_matching[PS_Set].Ch_Match[DC_Setting[iSet].Channel] < 8)
                        {
                            DC_Setting[ChnSet].On_Off = 1;
                            DC_Setting[ChnSet].Pos_Neg = 1;

                            Bias[(ChnSet * 6)] = DC_matching[PS_Set].Ch_Match[DC_Setting[iSet].Channel];        // Channel Number
                            Bias[(ChnSet * 6) + 1] = DC_Setting[iSet].Mode;       // mode 0 => clampI->driveV
                            Bias[(ChnSet * 6) + 2] = (int)(DC_Setting[iSet].Current * 1000000000);    // Current nA
                            Bias[(ChnSet * 6) + 3] = (int)(DC_Setting[iSet].Voltage * 1000);          // Voltage mV
                            Bias[(ChnSet * 6) + 4] = DC_Setting[iSet].Pos_Neg;    // Pos_Neg
                            Bias[(ChnSet * 6) + 5] = DC_Setting[iSet].On_Off;     // On_Off
                            ChnSet++;
                        }
                    }
                    Read = new int[DC_matching[PS_Set].Aemulus_SubChannel * 2];
                    RsltData = new int[DC_matching[PS_Set].Ch_Match.Length];
                    Results_Array = new double[DC_matching[PS_Set].Ch_Match.Length];
                }
                else if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.PS4142)
                {
                    Bias = new int[DC_matching[PS_Set].Ch_Match.Length];
                    for (int iSet = 0; iSet < DC_matching[PS_Set].Ch_Match.Length; iSet++)
                    {
                        Bias[iSet] = DC_matching[PS_Set].Ch_Match[DC_Setting[iSet].Channel];
                    }
                }
            }

        }

        public void Init_Read_Array()
        {
            int ChnSet;
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                ChnSet = 0;
                if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.AEMULUS)
                {
                    for (int iRead = 0; iRead < DC_matching[PS_Set].Ch_Match.Length; iRead++)
                    {
                        if (DC_matching[PS_Set].Ch_Match[DC_Setting[iRead].Channel] < 8)
                        {
                            Read[(ChnSet * 2)] = DC_matching[PS_Set].Ch_Match[DC_Setting[iRead].Channel];
                            Read[(ChnSet * 2) + 1] = 1;
                            ChnSet++;
                        }

                    }
                }
            }
        }
        public void Init_DC_Settings(int PS_Set,float NPLC)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {
                for (int iChn = 0; iChn < DC_matching[PS_Set - 1].Ch_Match.Length; iChn++)
                {
                    if (DC_matching[PS_Set - 1].Ch_Match[iChn] < 8)
                    {
                        AM330[PS_Set - 1].Settings.SETANAPINBANDWIDTH(DC_matching[PS_Set - 1].Ch_Match[iChn], 2);
                        AM330[PS_Set - 1].Settings.SETNPLC(DC_matching[PS_Set - 1].Ch_Match[iChn], NPLC);

                        //AM330[PS_Set - 1].Settings.SETNPLC(DC_matching[PS_Set - 1].Ch_Match[iChn], NPLC);
                    }
                    else
                    {
                        AM330[PS_Set - 1].Settings.SETNPLC(DC_matching[PS_Set - 1].Ch_Match[iChn], NPLC);
                        DC_matching[PS_Set - 1].AM371_ChannelIndex = iChn;
                    }
                }
            }
        }
        public void Init_Channel()
        {
            int subChn;
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                subChn = 0;
                if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.AEMULUS)
                {
                    for (int iChn = 0; iChn < DC_matching[PS_Set].Ch_Match.Length; iChn++)
                    {
                        if (DC_matching[PS_Set].Ch_Match[iChn] < 8)
                        {
                            subChn++;
                        }
                    }
                    DC_matching[PS_Set].Aemulus_SubChannel = subChn;
                }
            }
        }

        public s_DC_Match[] parse_DC_Matching
        {
            set
            {
                DC_matching = value;
                TotalChannel = value.Length;
                AM330 = new cAemulus_AM330[DC_matching.Length];
                DC4142 = new cDC4142[DC_matching.Length];
            }
        }

        public double[] Get_Result_Array
        {
            get
            {
                return Results_Array;
            }
        }

        public void Set_Bias(int PS_Set)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {
                AM330[PS_Set - 1].SMU.BIASSMUPIN(DC_matching[PS_Set - 1].Aemulus_SubChannel, out Bias[0]);
                if (DC_matching[PS_Set - 1].Ch_Match.Length != DC_matching[PS_Set - 1].Aemulus_SubChannel)
                {
                    AM330[PS_Set - 1].Clamp.AM371_CLAMPCURRENT(8, (float)2);
                    AM330[PS_Set - 1].SMU.AM371_ONSMUPIN(8, 0);
                    Thread.Sleep(1);
                    AM330[PS_Set - 1].Drive.AM371_DRIVEVOLTAGESETVRANGE(8, (float)DC_Setting[PS_Set - 1].Voltage, 2);
                }
            }
            else if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.PS4142)
            {
                for (int iSet = 0; iSet < DC_matching[PS_Set - 1].Ch_Match.Length; iSet++)
                {
                    if (general.CInt2Bool(DC_Setting[iSet].On_Off))
                    {
                        DC4142[PS_Set - 1].Output.ON(Bias[iSet]);
                    }
                    else
                    {
                        DC4142[PS_Set - 1].Output.OFF(Bias[iSet]);
                    }
                    DC4142[PS_Set - 1].Output.Set_Voltage(Bias[iSet], DC_Setting[iSet].Range, DC_Setting[iSet].Voltage, DC_Setting[iSet].Current);
                }
            }
            else if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.NI_SMU)
            {
                
                for (int iSet = 0; iSet < DC_matching[PS_Set - 1].Ch_Match.Length; iSet++)
                {
                    TestLib.PaTest.SmuResources[DC_matching[iSet].Address].ForceVoltage(DC_Setting[iSet].Voltage, DC_Setting[iSet].Current);
                
                //    TestLib.ClsTestLib.SmuResources[DC_matching[iSet].Address].ForceVoltage(DC_Setting[iSet].Voltage, DC_Setting[iSet].Current);
                }
            }   
            
        }
        public void No_Bias()
        {
            for (int PS_Set = 0; PS_Set < DC_matching.Length; PS_Set++)
            {
                if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.AEMULUS)
                {
                    NoBias = new int[DC_matching[PS_Set].Ch_Match.Length * 6];
                    for (int iSet = 0; iSet < DC_matching[PS_Set].Ch_Match.Length; iSet++)
                    {
                        if (DC_matching[PS_Set].Ch_Match[iSet] < 8)
                        {

                            NoBias[(iSet * 6)] = DC_matching[PS_Set].Ch_Match[iSet];        // Channel Number
                            NoBias[(iSet * 6) + 1] = 0;       // mode 0 => clampI->driveV
                            NoBias[(iSet * 6) + 2] = 0;    // Current nA
                            NoBias[(iSet * 6) + 3] = 0;          // Voltage mV
                            NoBias[(iSet * 6) + 4] = 1;    // Pos_Neg
                            NoBias[(iSet * 6) + 5] = 0;     // On_Off
                        }
                    }
                    AM330[PS_Set].SMU.BIASSMUPIN(DC_matching[PS_Set].Aemulus_SubChannel, out NoBias[0]);
                    if (DC_matching[PS_Set].Ch_Match.Length != DC_matching[PS_Set].Aemulus_SubChannel)
                    {
                        AM330[PS_Set].Drive.AM371_DRIVEVOLTAGESETVRANGE(8, 0, 0);
                        Thread.Sleep(1); 
                        AM330[PS_Set].Clamp.AM371_CLAMPCURRENT(8, 0);
                        AM330[PS_Set].SMU.AM371_OFFSMUPIN(8);
                    }
                }
                else if (DC_matching[PS_Set].PS_Name == e_DC_PS_Equipment.PS4142)
                {
                    for (int iSet = 0; iSet < DC_matching[PS_Set].Ch_Match.Length; iSet++)
                    {
                        DC4142[PS_Set].Output.Set_Voltage(DC_matching[PS_Set].Ch_Match[DC_Setting[iSet].Channel],0,0,0);
                        DC4142[PS_Set].Output.OFF(Bias[iSet]);
                    }
                }
            }
        }

        //public void No_Bias_Pxi()
        //{
            //ºmyLibAM400.DriveVoltage(ClothoLibStandard.Aemulus_SMU.SMUPin.VCC, 0);
            //ºmyLibAM400.DriveVoltage(ClothoLibStandard.Aemulus_SMU.SMUPin.VBATT, 0);           
        //}
        public void Read_Bias(int PS_Set)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {

                AM330[PS_Set - 1].SMU.READSMUPIN(DC_matching[PS_Set - 1].Aemulus_SubChannel, out Read[0], out RsltData[0]);
                //Thread.Sleep(Sleep_ms);
                for (int iRslt = 0; iRslt < DC_matching[PS_Set - 1].Ch_Match.Length; iRslt++)
                {
                    Results_Array[iRslt] = Convert.ToDouble(RsltData[iRslt]);
                }
            }
            else if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.PS4142)
            {
                for (int iRead = 0; iRead < DC_matching[PS_Set - 1].Ch_Match.Length; iRead++)
                {
                    Results_Array[iRead] = Convert.ToDouble(DC4142[PS_Set - 1].Measurement.IMeasurement(DC_matching[PS_Set - 1].Ch_Match[DC_Setting[iRead].Channel]));
                }
            }
            
        }

        public void Set_Integration(int PS_Set, int set_Value)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {
                AM330[PS_Set - 1].Settings.SETINTEGRATION(set_Value);
            }
        }
        public void ClampCurrent(int PS_Set, int Channel, double current_A)
        {
            if (DC_matching[PS_Set - 1].PS_Name == e_DC_PS_Equipment.AEMULUS)
            {
                if (DC_matching[PS_Set - 1].Ch_Match[Channel] < 8)
                {
                    AM330[PS_Set - 1].Clamp.CLAMPCURRENT(DC_matching[PS_Set - 1].Ch_Match[Channel], (int)(current_A * 1000000000));
                }
                else
                {
                    AM330[PS_Set - 1].Clamp.AM371_CLAMPCURRENT(DC_matching[PS_Set - 1].Ch_Match[Channel], (float)(current_A));
                }

            }
        }
    }
}
