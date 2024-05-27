using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibFBAR.DC;


namespace LibFBAR
{
    public class cDC_PowerSupply : cDC_common 
    {
        #region "Structure"

        #endregion
        public static string ClassName = "DC Power Supply Test Class";
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  29/12/2011       KKL             New and example for DC Power Supplies class new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }
        public void Set_IO(int PS_Set, int hSys)
        {
            Parse_IO(PS_Set, hSys);
        }
        public void Set_IO(int PS_Set, Ivi.Visa.Interop.FormattedIO488 IO)
        {
            Parse_IO(PS_Set, IO);
        }

        //static cExcel.cExcel_Lib Excel;
        //public cExcel.cExcel_Lib parseExcel
        //{
        //    set
        //    {
        //        Excel = value;
        //    }
        //}

        #region "Equipment Declarations"
        //public string DC_EquipmentName;
        
        public void InitEquipment()
        {
            Init();
        }
        #endregion

        #region "Result Declarations"
        private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        public void Init(int Tests)
        {
            SaveResult = new s_Result[Tests];
        }
        public s_Result[] Result_setting
        {
            get
            {
                return SaveResult;
            }
        }     // Getting the internal data to public
        public void Clear_Results()
        {
            for (int iClear = 0; iClear < SaveResult.Length; iClear++)
            {
                if (SaveResult[iClear].b_MultiResult)
                {
                    for (int iSubClear = 0; iSubClear < SaveResult[iClear].Multi_Results.Length; iSubClear++)
                    {
                        SaveResult[iClear].Multi_Results[iSubClear].Result_Data = 0;
                    }
                }
                SaveResult[iClear].Result_Data = 0;
            }
        }
        #endregion

        private static s_DC_Match[] DC_matching;

        public void Load_DC_ChannelSettings()
        {
            string tmpDCStr = "DC_Channel_Setting";
            string tmpStr;
            string tmpChnStr;
            int RowNo;
            int DC_Set;
            int DC_Chn_Cnt;
            bool b_Found;

            RowNo = 1;
            DC_Chn_Cnt = 1;
            b_Found = false;

            DC_matching = new s_DC_Match[1];
            DC_matching[0].Ch_Match = new int[1];

            do
            {
                tmpStr = LibFBAR.cExtract.Get_Data(tmpDCStr, RowNo, 1);
                if (tmpStr.ToUpper() == ("#End").ToUpper())
                {
                    b_Found = false;
                }
                if (b_Found)
                {
                    if (int.TryParse(tmpStr, out DC_Set))
                    {
                        if (DC_Set > 0)
                        {
                            
                            if (DC_Set > 1)
                            {
                                Array.Resize(ref DC_matching, DC_Set);
                                //CM WONG
                                //DC_matching[DC_Set].Ch_Match = new int[1];
                                DC_matching[DC_Set - 1].Ch_Match = new int[1];
                                DC_Chn_Cnt = 1;
                            }

                            DC_matching[DC_Set - 1].PS_Name = Set_PS_Equipment(LibFBAR.cExtract.Get_Data(tmpDCStr, RowNo, 2));
                            DC_matching[DC_Set - 1].Address = LibFBAR.cExtract.Get_Data(tmpDCStr, RowNo, 3);
                            do
                            {
                                
                                tmpChnStr = LibFBAR.cExtract.Get_Data(tmpDCStr, RowNo, (DC_Chn_Cnt + 3));
                                if (tmpChnStr != "")
                                {
                                    if (DC_Chn_Cnt > 1)
                                    {
                                        Array.Resize(ref DC_matching[DC_Set - 1].Ch_Match, DC_Chn_Cnt);
                                    }
                                    DC_matching[DC_Set - 1].Ch_Match[DC_Chn_Cnt - 1] = int.Parse(tmpChnStr);
                                }
                                DC_Chn_Cnt++;
                            } while (tmpChnStr.Trim() != "");
                        }
                    }
                    
                }
                if (tmpStr.ToUpper() == ("#Start").ToUpper())
                {
                    b_Found = true;
                }
                RowNo++;
            } while (tmpStr.ToUpper() != ("#End").ToUpper());

            parse_DC_Matching = DC_matching;
        }

        public cTestClasses[] TestClass;
        public class cTestClasses
        {
            public cSMU_DC_Setting SMU_DC_Setting;

            public interface iTestFunction
            {
                void InitSettings();
                void RunTest();
                void MeasureResult();
            }
            public class cSMU_DC_Setting : cDC_common, iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "DC Setting Function Class";    // Sub Class Naming
                
                // External Variables
                public ManualResetEvent _mre;
                public int DC_Set;
                //public int TotalChannel;
                //public s_DC_Set[] DC_Setting;
                public int Sleep_ms;
                public int TestNo;
                public bool Ignore_Read;
                public int NPLC;

                public string PowerMode;
                public string[] MIPI_DAC;

                public float LimitIcc2;
                public float LimitIcc1;

                public float Vcc2;
                public float Vcc1;

                // Internal Variables
                //Stopwatch watch = new Stopwatch();
                //private int[] Bias;
                //private int[] Read;
                //private int[] RsltData;
                private s_mRslt[] rtnResult;

                #endregion
                public void InitArray()
                {
                    TotalChannel = DC_matching[DC_Set - 1].Ch_Match.Length;
                    DC_Setting = new s_DC_Set[TotalChannel];
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    Init_Channel();
                    Init_Bias_Array();
                    Init_Read_Array();
                    if (!Ignore_Read)
                    {
                        SaveResult[TestNo].Enable = true;
                        SaveResult[TestNo].b_MultiResult = true;
                        //SaveResult[TestNo].Multi_Results = new s_mRslt[TotalChannel];

                        rtnResult = new s_mRslt[TotalChannel];

                        for (int iSet = 0; iSet < TotalChannel; iSet++)
                        {
                            rtnResult[iSet].Enable = DC_Setting[iSet].b_Enable;
                            rtnResult[iSet].Result_Header = DC_Setting[iSet].Header;
                        }
                        Init_Read_Array();
                    }
                    else
                    {
                        SaveResult[TestNo].Enable = false;
                        SaveResult[TestNo].b_MultiResult = false;
                    }
                    if (NPLC == 0) NPLC = 1;
                }
                public void InitSettings_Pxi()
                {
                    
                          
                }

                public void RunTest()
                {
                    //Init_DC_Settings(DC_Set, NPLC); //by hng
                    Set_Bias(DC_Set);
                    //ºmyLibDM482.VIO_ON();

                    //ºMipi_ReaBack = ºmyLibDM482.Resistor_Change(PowerMode, MIPI_DACbit);

                    //ºmyLibAM400.ClampCurrent(ClothoLibStandard.Aemulus_SMU.SMUPin.VCC, (double)DC_Setting[3].Current );
                    //ºmyLibAM400.DriveVoltage(ClothoLibStandard.Aemulus_SMU.SMUPin.VCC, (double)DC_Setting[3].Voltage);

                    //ºmyLibAM400.ClampCurrent(ClothoLibStandard.Aemulus_SMU.SMUPin.VBATT, (double)DC_Setting[2].Current);
                    //ºmyLibAM400.DriveVoltage(ClothoLibStandard.Aemulus_SMU.SMUPin.VBATT, (double)DC_Setting[2].Voltage);


                    //double Check_Idle_current = ºmyLibAM400.ReadCurrent(ClothoLibStandard.Aemulus_SMU.SMUPin.VCC);
                    //double Check_voltage = ºmyLibAM400.ReadVoltage(ClothoLibStandard.Aemulus_SMU.SMUPin.VCC);

                    Thread.Sleep(Sleep_ms);
                    if (!Ignore_Read)
                    {
                        MeasureResult();
                        SetResult();
                    }
                }
                public void MeasureResult()
                {
                    Read_Bias(DC_Set);
                    for (int iRslt = 0; iRslt < TotalChannel; iRslt++)
                    {
                        SaveResult[TestNo].Multi_Results[iRslt].Result_Data = Get_Result_Array[iRslt];
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Multi_Results = rtnResult;
                }
            }
        }
    }
}
