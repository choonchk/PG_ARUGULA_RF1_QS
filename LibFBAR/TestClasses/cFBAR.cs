using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;
using System.Collections;
//using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Avago.ATF.StandardLibrary;
//using NiVstCommonLib;
using InstrLib;
using TestLib;
using SnP_BuddyFileBuilder;
using ClothoLibAlgo;

namespace LibFBAR
{

    public class cFBAR
    {

        #region "Enumerations"
        public enum e_SearchDirection
        {
            NONE = 0,
            FROM_LEFT,
            FROM_RIGHT,
            FROM_MAX_LEFT,
            FROM_MAX_RIGHT,
            FROM_EXTREME_LEFT,
            FROM_EXTREME_RIGHT,
            TO_LEFT,
            TO_RIGHT,
        }
        public enum e_SearchType
        {
            MIN = 0,
            MAX,
            USER
        }
        public enum e_BalanceType
        {
            CMRR = 0,
            AMPLITUDE,
            PHASE
        }
        #endregion
        #region "Structure"
        public struct s_DataType
        {
            public Real_Imag ReIm;
            public Mag_Angle MagAng;
            public dB_Angle dBAng;
        }
        public struct S_ParamData
        {
            public s_DataType[] sParam;
            public e_SFormat Format;
        }
        public struct S_Param
        {
            public S_ParamData[] sParam_Data;
            public double[] Freq;
            public int NoPorts;
            public int NoPoints;
            public bool[] SParam_Enable;
        }
        public struct S_CMRRnBal_Param
        {
            public S_ParamData Balance;
            public S_ParamData CMRR;
            public bool Balance_Enable;
            public bool CMRR_Enable;
        }
        public struct s_SParam_Grab
        {
            public bool[] SParam_Grab;
        }
        public struct s_TraceMatching
        {
            public int TotalTraces;
            public int[] TraceNumber;
            public int[] SParam_Def_Number;
        }

        public struct s_ZImpedance
        {
            public double Z0;
            public double Real;
            public double Imag;
        }
        public struct s_ZConversion
        {
            public int ChannelNumber;
            public bool Enable;
            public s_ZImpedance[] PortConversion;
        }
        public bool b_Balance;
        #endregion
        #region "Flags"
        static bool b_CheckFlag;     // To Check for Errors
        public static bool b_ErrorFlag;
        #endregion
        public static string ClassName = "FBAR Class";
        public static string TraceTabName = "Trace";
        public static string SegmentTabName = "Segment";
        static LibFBAR.cFunction Math_Func = new cFunction();
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();
        public static bool b_ZnbInUse = false; //ZNB in use?
        public static double[] znbData;
        public static int znbDataOffset = 0;
        public static bool b_EnhanceDataFetch = false;
        public static bool b_ZnbDataReady = false;
        public static int znbTraceOffset = 0;

        public static Dictionary.DoubleKey<int, int, double[]> traceData = new Dictionary.DoubleKey<int, int, double[]>();

        public static string StateFile = ""; //For recover Calibration State use
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  10/10/2011       KKL             New and example for FBAR class new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
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
        static FormattedIO488 ioNA = new FormattedIO488();
        protected static cENA ENA = new cENA();
        protected static string NA_Name;
        protected static string NA_Addr;
        public FormattedIO488 parseNA_IO
        {
            set
            {
                ioNA = value;
                InitEquipment();
            }
        }
        public void InitEquipment()
        {
            ENA.parseIO = ioNA;
            ENA.Init(ioNA);
            NA_Name = "ENA";
            CheckENA_EnhanceFetch();
        }  //Modify this for new equipment 
        public void InitEq(string address)
        {
            if (address != "" && address != null)
            {
                NA_Addr = address;
                ENA.Address = address;
                ENA.OpenIO();
                CheckENA_EnhanceFetch();
            }
        }
        public void CheckENA_EnhanceFetch()
        {
            s_EquipmentInfo Info = ENA.BasicCommand.DeviceInfo();
            if (Info.ModelNumber.ToUpper().Contains("E5071C"))
            {
                string[] Version = Info.FirmwareVersion.Split('.');
                if (int.Parse(Version[1]) >= 11)
                {
                    if (int.Parse(Version[2]) >= 20)
                    {
                        b_EnhanceDataFetch = true;
                    }
                }
            }
            if (Info.ModelNumber.ToUpper().Contains("ZNB"))
            {
                ENA = null;
                ENA = new cZNB();
                if (NA_Addr != "" && NA_Addr != null)
                {
                    ENA.Address = NA_Addr;
                    ENA.OpenIO();
                }
                else
                {
                    ENA.parseIO = ioNA;
                    ENA.Init(ioNA);
                }
                NA_Name = "ZNB";
                b_EnhanceDataFetch = true; //Disable untill the data capturing solved
                b_ZnbInUse = true;
            }
        }
        #endregion

        #region "Results Declarations"
        private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        public s_Result[] Result_setting
        {
            get
            {
                return SaveResult;
            }
        }     // Getting the internal data to public
        public void Init(int Tests)
        {
            SaveResult = new s_Result[Tests + 1];
        }
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
            DataTrigger_i = 0;
        }
        #endregion

        #region "Channel Trace Settings"
        private static S_Param[] SParamData;
        private static S_CMRRnBal_Param[] SBalanceParamData;
        public static s_TraceMatching[] TraceMatch;
        public void Init_Channel()
        {
            int RowNo;
            int ChannelCnt;
            string tmpStr;
            ChannelCnt = 0;
            RowNo = 2;
            do
            {
                tmpStr = cExtract.Get_Data(TraceTabName, RowNo, 1);
                //tmpStr = Excel.Get_Data(TraceTabName, RowNo, 1);         //Excel Data
                RowNo++;
                ChannelCnt++;
                if (RowNo > 40) break;
            } while (tmpStr.ToUpper() != ("#EndTrace").ToUpper());

            TotalChannel = ChannelCnt - 1;
            //TotalChannel = 6;


            if (((TotalChannel < 1) || (RowNo > 40)) && b_CheckFlag == true)
            {
                General.DisplayError(ClassName, "Error Total Channel Number",
                    "Total Channel Number Error in Init_Channel() function \r\nTotal Channel = " + TotalChannel.ToString() + " (Less then 1!)");
            }

            SetTraceMatching(TotalChannel);  // Seoul
        }
        public void SetTraceMatching(int TotalChannel)
        {
            int ChannelNumber;
            int PortNumbers;
            string TraceSetting;
            TraceMatch = new s_TraceMatching[TotalChannel];
            SParamData = new cFBAR.S_Param[TotalChannel];
            SBalanceParamData = new S_CMRRnBal_Param[TotalChannel];
           
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                //Modified by KCC
                //TraceMatch[iChn].TraceNumber = new int[24];
                TraceMatch[iChn].TotalTraces = 0;
                TraceMatch[iChn].TraceNumber = new int[Enum.GetValues(typeof(e_SParametersDef)).Length];
                TraceMatch[iChn].SParam_Def_Number = new int[Enum.GetValues(typeof(e_SParametersDef)).Length];
            }
            for (int iRow = 0; iRow < TotalChannel; iRow++)
            {
                ChannelNumber = int.Parse(cExtract.Get_Data(TraceTabName, (iRow + 2), 1));      // seoul
                PortNumbers = int.Parse(cExtract.Get_Data(TraceTabName, (iRow + 2), 2));        //Excel Data
                SParamData[iRow].NoPorts = PortNumbers;
                TraceSetting = cExtract.Get_Data(TraceTabName, (iRow + 2), 3);                  //Excel Data
                Init_TraceMatch((ChannelNumber - 1), PortNumbers, General.convertAutoStr2Bool(TraceSetting));
            }
        }
        public void Init_TraceMatch(int ChannelNumber, int PortNumber, bool AutoSet)
        {
            for (int iArr = 0; iArr < Enum.GetValues(typeof(e_SParametersDef)).Length; iArr++)
            {
                TraceMatch[ChannelNumber].TraceNumber[iArr] = -1;
                TraceMatch[ChannelNumber].SParam_Def_Number[iArr] = -1;
            }
            if (AutoSet == true)
            {
                switch (PortNumber)
                {
                    case 1:
                        SParamData[ChannelNumber].sParam_Data = new S_ParamData[1];
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        TraceMatch[ChannelNumber].TotalTraces = 1;
                        break;
                    case 2:
                        SParamData[ChannelNumber].sParam_Data = new S_ParamData[4];
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        TraceMatch[ChannelNumber].TotalTraces = 4;
                        break;
                    case 3:
                        SParamData[ChannelNumber].sParam_Data = new S_ParamData[9];
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S33.GetHashCode()] = 4;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S13.GetHashCode()] = 5;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S23.GetHashCode()] = 6;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S31.GetHashCode()] = 7;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S32.GetHashCode()] = 8;
                        TraceMatch[ChannelNumber].TotalTraces = 9;
                        break;
                    case 4:
                        SParamData[ChannelNumber].sParam_Data = new S_ParamData[16];
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S33.GetHashCode()] = 4;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S13.GetHashCode()] = 5;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S23.GetHashCode()] = 6;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S31.GetHashCode()] = 7;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S32.GetHashCode()] = 8;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S44.GetHashCode()] = 9;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S14.GetHashCode()] = 10;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S24.GetHashCode()] = 11;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S34.GetHashCode()] = 12;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S41.GetHashCode()] = 13;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S42.GetHashCode()] = 14;
                        TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S43.GetHashCode()] = 15;
                        TraceMatch[ChannelNumber].TotalTraces = 15;
                        break;
                }
            }
            else
            {
                //Modified by KCC
                //SParamData[ChannelNumber].sParam_Data = new S_ParamData[27];
                SParamData[ChannelNumber].sParam_Data = new S_ParamData[Enum.GetValues(typeof(e_SParametersDef)).Length];
                //
                for (int iDef = 0; iDef < Enum.GetValues(typeof(e_SParametersDef)).Length; iDef++)
                {
                    TraceMatch[ChannelNumber].TraceNumber[iDef] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, iDef + 4) - 1;
                }
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 4) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 5) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S13.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 6) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S14.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 7) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 8) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 9) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S23.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 10) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S24.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 11) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S31.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 12) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S32.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 13) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S33.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 14) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S34.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 15) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S41.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 16) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S42.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 17) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S43.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 18) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S44.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 19) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.A.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 20) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.B.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 21) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.C.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 22) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.D.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 23) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.R1.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 24) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.R2.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 25) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.R3.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 26) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.R4.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 27) - 1;
                ////Added by KCC
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.SDS31.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 28) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.SCS31.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 29) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.SDS32.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 30) - 1;
                //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.SDD22.GetHashCode()] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, 31) - 1;
                ////
            }
            int tmp_SParam_Def = 0;

            for (int iTrace = 0; iTrace < Enum.GetValues(typeof(e_SParametersDef)).Length; iTrace++)
            {
                if (TraceMatch[ChannelNumber].TraceNumber[iTrace] >= 0)
                {
                    TraceMatch[ChannelNumber].SParam_Def_Number[tmp_SParam_Def] = iTrace;
                    TraceMatch[ChannelNumber].TotalTraces++; //Increase traces count
                    tmp_SParam_Def++;
                }
            }
        }
        public void Verify_TraceMatch(int ChannelNumber)
        {
            int TotalTrace;
            int iTmp;
            int iOffset = 0;
            bool bTmp;
            string sTmp;
            bool bCheckOffset = false;
            e_SParametersDef tmpDef;
            TotalTrace = ENA.Calculate.Par.Count(ChannelNumber);   //SiteNumber
            for (int iTrace = 0; iTrace < TotalTrace; iTrace++)
            {
                //bTmp = ENA.Calculate.FixtureSimulator.BALun.Parameter.Status(ChannelNumber, (iTrace + 1));   //SiteNumber
                //if (bTmp)
                //{
                //    sTmp = ENA.Calculate.FixtureSimulator.BALun.Parameter.Parameters(ChannelNumber, (iTrace + 1)).Trim(); //SiteNumber
                //    if (sTmp.Contains("CMRR"))
                //    {
                //        tmpDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), sTmp);
                //    }
                //    else
                //    {
                //        tmpDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S" + sTmp.Substring(sTmp.Length - 2, 2));
                //    }
                //}
                //else
                {
                    // DUALSITE: Dual site to modify the code
                    tmpDef = ENA.Calculate.Par.Define_Enum(ChannelNumber, (iTrace + 1));    //SiteNumber

                    {
                        if ((SParamData[ChannelNumber - 1].NoPorts == 2))
                        {
                            if (!bCheckOffset)
                            {
                                if (iTrace == 0)
                                {
                                    if ((tmpDef.ToString().Length == 3) && (tmpDef.ToString().Substring(0, 1) == "S"))
                                    {
                                        int FirstSPort = int.Parse(tmpDef.ToString().Substring(1, 1));
                                        int SecondSPort = int.Parse(tmpDef.ToString().Substring(2, 1));
                                        for (int ioff = 1; ioff < 3; ioff++)
                                        {
                                            if (((FirstSPort - ioff) <= 2) && ((FirstSPort - ioff) > 0) && ((SecondSPort - ioff) <= 2) && ((SecondSPort - ioff) > 0))
                                            {
                                                iOffset = ioff;
                                                bCheckOffset = true;
                                                FirstSPort -= ioff;
                                                SecondSPort -= ioff;
                                                tmpDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S" + FirstSPort.ToString() + SecondSPort.ToString());
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int FirstSPort = int.Parse(tmpDef.ToString().Substring(1, 1)) - iOffset;
                                int SecondSPort = int.Parse(tmpDef.ToString().Substring(2, 1)) - iOffset;

                                if ((tmpDef.ToString().Length == 3) && (tmpDef.ToString().Substring(0, 1) == "S"))
                                {
                                    tmpDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), "S" + FirstSPort.ToString() + SecondSPort.ToString());
                                }
                            }
                        }
                    }
                }
                iTmp = TraceMatch[ChannelNumber - 1].TraceNumber[tmpDef.GetHashCode()];
                if (iTmp != iTrace)
                {
                    General.DisplayError(ClassName, "Possible S Parameter Mismatch", "S Parameter for Trace Number " + (iTrace + 1).ToString() + " mismatch!!!");
                }

            }
        }
        #endregion
        #region "Channel Segment"
        public static int TotalChannel;
        public static s_SegmentTable[] SegmentParam;
        public void Init_SegmentParam()
        {
            int ChannelNumber;
            int TotalPoints;
            int TotalSegment;
            string tmpStr;
            int RowNo;
            bool SegmentSettings;
            bool SegmentTableSettings;

            RowNo = 1;
            SegmentSettings = false;
            SegmentTableSettings = false;
            TotalPoints = 0;
            TotalSegment = 0;
            ChannelNumber = 0;

            SegmentParam = new s_SegmentTable[TotalChannel];


            ////////////////////////////////Seoul

            do
            {
                tmpStr = cExtract.Get_Data(SegmentTabName, RowNo, 1);

                if (SegmentSettings == true)
                {
                    ChannelNumber = (cExtract.Get_Data_Int(SegmentTabName, RowNo, 2)) - 1;
                    //int AA = cExtract.Get_Data_Int(SegmentTabName, RowNo + 1, 2);
                    SegmentParam[ChannelNumber].mode = (e_ModeSetting)cExtract.Get_Data_Int(SegmentTabName, RowNo + 1, 2);
                    SegmentParam[ChannelNumber].ifbw = (e_OnOff)cExtract.Get_Data_Int(SegmentTabName, RowNo + 2, 2);
                    SegmentParam[ChannelNumber].pow = (e_OnOff)cExtract.Get_Data_Int(SegmentTabName, RowNo + 3, 2);
                    SegmentParam[ChannelNumber].del = (e_OnOff)cExtract.Get_Data_Int(SegmentTabName, RowNo + 4, 2);
                    SegmentParam[ChannelNumber].swp = (e_OnOff)cExtract.Get_Data_Int(SegmentTabName, RowNo + 5, 2);
                    SegmentParam[ChannelNumber].time = (e_OnOff)cExtract.Get_Data_Int(SegmentTabName, RowNo + 6, 2);
                    SegmentParam[ChannelNumber].segm = cExtract.Get_Data_Int(SegmentTabName, RowNo + 7, 2);

                    SegmentSettings = false;
                }
                if (SegmentTableSettings == true)
                {
                    if (TotalPoints == 0) SegmentParam[ChannelNumber].SegmentData = new s_SegmentData[SegmentParam[ChannelNumber].segm];
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].Start = General.convertStr2Val(cExtract.Get_Data(SegmentTabName, RowNo, 2));
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].Stop = General.convertStr2Val(cExtract.Get_Data(SegmentTabName, RowNo, 3));
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].Points = cExtract.Get_Data_Int(SegmentTabName, RowNo, 4);
                    TotalPoints += SegmentParam[ChannelNumber].SegmentData[TotalSegment].Points;
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].ifbw_value = General.convertStr2Val(cExtract.Get_Data(SegmentTabName, RowNo, 5));
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].pow_value = cExtract.Get_Data_Double(SegmentTabName, RowNo, 6);
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].del_value = cExtract.Get_Data_Double(SegmentTabName, RowNo, 7);
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].swp_value = (e_SweepMode)cExtract.Get_Data_Int(SegmentTabName, RowNo, 8);
                    SegmentParam[ChannelNumber].SegmentData[TotalSegment].time_value = cExtract.Get_Data_Double(SegmentTabName, RowNo, 9);
                    TotalSegment++;
                    if (TotalSegment == SegmentParam[ChannelNumber].segm) SegmentTableSettings = false;
                }

                if (tmpStr == "#Start")
                {
                    SegmentSettings = true;
                }

                if (tmpStr == "#End")
                {
                    SParamData[ChannelNumber].NoPoints = TotalPoints;
                    TotalPoints = 0;
                    TotalSegment = 0;
                    ChannelNumber++;
                }
                else if (tmpStr.ToUpper() == ("Segment No").ToUpper())
                {
                    SegmentTableSettings = true;
                }   
                RowNo++;
            }
            while (tmpStr.ToUpper() != ("#EndSegment").ToUpper());



            ////////////////////////////////////////////Seoul



            //SParamData[ChannelNumber].NoPoints = TotalPoints;
        }
        public void Verify_SegmentParam()
        {
            double tmpVal;
            string tmpStr;

            tmpStr = "";
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                e_SweepType Sweep_Type;
                Sweep_Type = ENA.Sense.Sweep.Type(iChn + 1);
                if ((SegmentParam[iChn].segm == 1) && (Sweep_Type == e_SweepType.LIN))
                {
                    if (Sweep_Type != e_SweepType.LIN)
                    {
                        General.DisplayError(ClassName, "Error in verifying Linear Table in Channel: " + (iChn+1).ToString(), "Please Check ENA setting again to avoid testing issue\nSweep Type = " + Sweep_Type.ToString());
                        b_ErrorFlag = true;
                    }
                    tmpVal = ENA.Sense.Frequency.Start();
                    if (tmpVal != SegmentParam[iChn].SegmentData[0].Start)
                    {
                        General.DisplayError(ClassName, "Error in verifying Start Frequency in Channel: " + (iChn + 1).ToString(), "Please Check ENA setting again to avoid testing issue\nENA Start Frequency = " + tmpVal.ToString() + ", Test Condition Start Frequency = " + SegmentParam[iChn].SegmentData[0].Start.ToString());
                        b_ErrorFlag = true;
                    }
                    tmpVal = ENA.Sense.Frequency.Stop();
                    if (tmpVal != SegmentParam[iChn].SegmentData[0].Stop)
                    {
                        General.DisplayError(ClassName, "Error in verifying Stop Frequency in Channel: " + (iChn + 1).ToString(), "Please Check ENA setting again to avoid testing issue\nENA Stop Frequency = " + tmpVal.ToString() + ", Test Condition Stop Frequency = " + SegmentParam[iChn].SegmentData[0].Stop.ToString());
                        b_ErrorFlag = true;
                    }
                    tmpVal = ENA.Sense.Sweep.Points();
                    if (tmpVal != SegmentParam[iChn].SegmentData[0].Points)
                    {
                        General.DisplayError(ClassName, "Error in verifying Number of Points in Channel: " + (iChn + 1).ToString(), "Please Check ENA setting again to avoid testing issue\nENA Number of Points = " + tmpVal.ToString() + ", Test Condition Number of Points = " + SegmentParam[iChn].SegmentData[0].Points.ToString());
                        b_ErrorFlag = true;
                    }
                }
                else
                {
                    s_SegmentTable ST = new s_SegmentTable();
                    ST = ENA.Sense.Segment.Data(iChn + 1);
                    tmpStr = "";

                    if (Sweep_Type == e_SweepType.LIN)
                    {
                        General.DisplayError(ClassName, "Error in verifying Segment Table for Channel " + (iChn + 1).ToString(), "Please Check ENA setting again to avoid testing issue\r\nSweep Type = " + Sweep_Type.ToString());
                        b_ErrorFlag = true;
                    }
                    if (ST.swp == e_OnOff.Off)
                    {
                        if (ST.mode != SegmentParam[iChn].mode)
                        {
                            tmpStr += "\r\nENA Segment Mode = " + ST.mode.ToString() + ", Segment Mode = " + SegmentParam[iChn].mode.ToString();
                        }
                        if (ST.ifbw != SegmentParam[iChn].ifbw)
                        {
                            tmpStr += "\r\nENA Segment IF BW = " + ST.ifbw.ToString() + ", Segment IF BW = " + SegmentParam[iChn].ifbw.ToString();
                        }
                        if (ST.pow != SegmentParam[iChn].pow)
                        {
                            tmpStr += "\r\nENA Segment Power = " + ST.pow.ToString() + ", Segment Power = " + SegmentParam[iChn].pow.ToString();
                        }
                        if (ST.del != SegmentParam[iChn].del)
                        {
                            tmpStr += "\r\nENA Segment Delay = " + ST.del.ToString() + ", Segment Delay = " + SegmentParam[iChn].del.ToString();
                        }
                        if (ST.time != SegmentParam[iChn].time)
                        {
                            tmpStr += "\r\nENA Segment Time = " + ST.time.ToString() + ", Segment Time = " + SegmentParam[iChn].time.ToString();
                        }
                        if (ST.segm != SegmentParam[iChn].segm)
                        {
                            tmpStr += "\r\nENA Segment Number = " + ST.segm.ToString() + ", Segment Number = " + SegmentParam[iChn].segm.ToString();
                        }
                        else
                        {
                            for (int iSegm = 0; iSegm < ST.segm; iSegm++)
                            {
                                if (ST.SegmentData[iSegm].Start != SegmentParam[iChn].SegmentData[iSegm].Start)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Start Frequency = " + ST.SegmentData[iSegm].Start.ToString() + ", Segment Start Frequency = " + SegmentParam[iChn].SegmentData[iSegm].Start.ToString();
                                }
                                if (ST.SegmentData[iSegm].Stop != SegmentParam[iChn].SegmentData[iSegm].Stop)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Stop Frequency = " + ST.SegmentData[iSegm].Stop.ToString() + ", Segment Stop Frequency = " + SegmentParam[iChn].SegmentData[iSegm].Stop.ToString();
                                }
                                if (ST.SegmentData[iSegm].Points != SegmentParam[iChn].SegmentData[iSegm].Points)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Points = " + ST.SegmentData[iSegm].Points.ToString() + ", Segment Points = " + SegmentParam[iChn].SegmentData[iSegm].Points.ToString();
                                }
                                if (ST.ifbw == e_OnOff.On && ST.SegmentData[iSegm].ifbw_value != SegmentParam[iChn].SegmentData[iSegm].ifbw_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment IF Bandwidth = " + ST.SegmentData[iSegm].ifbw_value.ToString() + ", Segment IF Bandwidth = " + SegmentParam[iChn].SegmentData[iSegm].ifbw_value.ToString();
                                }
                                if (ST.pow == e_OnOff.On && ST.SegmentData[iSegm].pow_value != SegmentParam[iChn].SegmentData[iSegm].pow_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Power = " + ST.SegmentData[iSegm].pow_value.ToString() + ", Segment Power = " + SegmentParam[iChn].SegmentData[iSegm].pow_value.ToString();
                                }
                                if (ST.del == e_OnOff.On && ST.SegmentData[iSegm].del_value != SegmentParam[iChn].SegmentData[iSegm].del_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Delay = " + ST.SegmentData[iSegm].del_value.ToString() + ", Segment Delay = " + SegmentParam[iChn].SegmentData[iSegm].del_value.ToString();
                                }
                                if (ST.time == e_OnOff.On && ST.SegmentData[iSegm].time_value != SegmentParam[iChn].SegmentData[iSegm].time_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Time = " + ST.SegmentData[iSegm].time_value.ToString() + ", Segment Time = " + SegmentParam[iChn].SegmentData[iSegm].time_value.ToString();
                                }
                            }
                        }
                        if (tmpStr != "")
                        {
                            b_ErrorFlag = true;
                            General.DisplayError(ClassName, "Error in verifying Segment Table for Channel " + (iChn + 1).ToString(), "Mistake in Segment Table \r\n" + tmpStr);
                        }
                    }
                    else
                    {
                        if (ST.mode != SegmentParam[iChn].mode)
                        {
                            tmpStr += "\r\nENA Segment Mode = " + ST.mode.ToString() + ", Segment Mode = " + SegmentParam[iChn].mode.ToString();
                        }
                        if (ST.ifbw != SegmentParam[iChn].ifbw)
                        {
                            tmpStr += "\r\nENA Segment IF BW = " + ST.ifbw.ToString() + ", Segment IF BW = " + SegmentParam[iChn].ifbw.ToString();
                        }
                        if (ST.pow != SegmentParam[iChn].pow)
                        {
                            tmpStr += "\r\nENA Segment Power = " + ST.pow.ToString() + ", Segment Power = " + SegmentParam[iChn].pow.ToString();
                        }
                        if (ST.del != SegmentParam[iChn].del)
                        {
                            tmpStr += "\r\nENA Segment Delay = " + ST.del.ToString() + ", Segment Delay = " + SegmentParam[iChn].del.ToString();
                        }
                        if (ST.swp != SegmentParam[iChn].swp)
                        {
                            tmpStr += "\r\nENA Segment Sweep Mode Setting = " + ST.del.ToString() + ", Segment Sweep Mode Setting = " + SegmentParam[iChn].del.ToString();
                        }
                        if (ST.time != SegmentParam[iChn].time)
                        {
                            tmpStr += "\r\nENA Segment Time = " + ST.time.ToString() + ", Segment Time = " + SegmentParam[iChn].time.ToString();
                        }
                        if (ST.segm != SegmentParam[iChn].segm)
                        {
                            tmpStr += "\r\nENA Segment Number = " + ST.segm.ToString() + ", Segment Number = " + SegmentParam[iChn].segm.ToString();
                        }
                        else
                        {
                            for (int iSegm = 0; iSegm < ST.segm; iSegm++)
                            {
                                if (ST.SegmentData[iSegm].Start != SegmentParam[iChn].SegmentData[iSegm].Start)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Start Frequency = " + ST.SegmentData[iSegm].Start.ToString() + ", Segment Start Frequency = " + SegmentParam[iChn].SegmentData[iSegm].Start.ToString();
                                }
                                if (ST.SegmentData[iSegm].Stop != SegmentParam[iChn].SegmentData[iSegm].Stop)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Stop Frequency = " + ST.SegmentData[iSegm].Stop.ToString() + ", Segment Stop Frequency = " + SegmentParam[iChn].SegmentData[iSegm].Stop.ToString();
                                }
                                if (ST.SegmentData[iSegm].Points != SegmentParam[iChn].SegmentData[iSegm].Points)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Points = " + ST.SegmentData[iSegm].Points.ToString() + ", Segment Points = " + SegmentParam[iChn].SegmentData[iSegm].Points.ToString();
                                }
                                if (ST.ifbw == e_OnOff.On && ST.SegmentData[iSegm].ifbw_value != SegmentParam[iChn].SegmentData[iSegm].ifbw_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment IF Bandwidth = " + ST.SegmentData[iSegm].ifbw_value.ToString() + ", Segment IF Bandwidth = " + SegmentParam[iChn].SegmentData[iSegm].ifbw_value.ToString();
                                }
                                if (ST.pow == e_OnOff.On && ST.SegmentData[iSegm].pow_value != SegmentParam[iChn].SegmentData[iSegm].pow_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Power = " + ST.SegmentData[iSegm].pow_value.ToString() + ", Segment Power = " + SegmentParam[iChn].SegmentData[iSegm].pow_value.ToString();
                                }
                                if (ST.del == e_OnOff.On && ST.SegmentData[iSegm].del_value != SegmentParam[iChn].SegmentData[iSegm].del_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Delay = " + ST.SegmentData[iSegm].del_value.ToString() + ", Segment Delay = " + SegmentParam[iChn].SegmentData[iSegm].del_value.ToString();
                                }
                                if (ST.swp == e_OnOff.On && ST.SegmentData[iSegm].swp_value != SegmentParam[iChn].SegmentData[iSegm].swp_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Sweep Mode = " + ST.SegmentData[iSegm].swp_value.ToString() + ", Segment Sweep Mode = " + SegmentParam[iChn].SegmentData[iSegm].swp_value.ToString();
                                }
                                if (ST.time == e_OnOff.On && ST.SegmentData[iSegm].time_value != SegmentParam[iChn].SegmentData[iSegm].time_value)
                                {
                                    tmpStr += "\r\nSegment Number : " + (iSegm + 1).ToString() + "--> ENA Segment Time = " + ST.SegmentData[iSegm].time_value.ToString() + ", Segment Time = " + SegmentParam[iChn].SegmentData[iSegm].time_value.ToString();
                                }
                            }
                        }
                        if (tmpStr != "")
                        {
                            b_ErrorFlag = true;
                            General.DisplayError(ClassName, "Error in verifying Segment Table for Channel " + (iChn + 1).ToString(), "Mistake in Segment Table \r\n" + tmpStr);
                        }
                    }
                }
            }

        }
        #endregion
        #region "Matching Circuits"
        public static s_PortMatchSetting[] PortMatching;
        public static bool PortMatchingState;
        public void Init_PortMatching()
        {
            int RowNo;
            int ChannelNumber;
            int ChannelCnt;
            string tmpStr;
            string FixtureStr = "Fixture Analysis";

            ChannelCnt = 0;
            PortMatchingState = General.convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, 1, 3));
            if (PortMatchingState == true)
            {
                PortMatching = new s_PortMatchSetting[1];
                RowNo = 2;
                do
                {
                    tmpStr = cExtract.Get_Data(FixtureStr, RowNo, 1);
                    if ((int.TryParse(tmpStr, out ChannelNumber)) && (tmpStr != ""))
                    {
                        if (ChannelNumber > 0)
                        {
                            ChannelCnt++;
                            if (ChannelCnt > 1)
                            {
                                Array.Resize(ref PortMatching, ChannelCnt);
                            }
                            PortMatching[ChannelCnt - 1].ChannelNumber = ChannelNumber;
                            PortMatching[ChannelCnt - 1].Enable = General.convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, RowNo, 2));
                            if (PortMatching[ChannelCnt - 1].Enable)
                            {
                                PortMatching[ChannelCnt - 1].Port = new s_PortMatchDetailSetting[SParamData[ChannelCnt - 1].NoPorts];
                                for (int iPort = 0; iPort < SParamData[ChannelCnt - 1].NoPorts; iPort++)
                                {
                                    PortMatching[ChannelCnt - 1].Port[iPort].MatchType = (e_PortMatchType)Enum.Parse(typeof(e_PortMatchType), cExtract.Get_Data(FixtureStr, RowNo, 4).ToUpper());
                                    switch (PortMatching[ChannelCnt - 1].Port[iPort].MatchType)
                                    {
                                        case e_PortMatchType.NONE:
                                            break;
                                        case e_PortMatchType.USER:
                                            PortMatching[ChannelCnt - 1].Port[iPort].UserFile = cExtract.Get_Data(FixtureStr, RowNo + iPort, 9);
                                            break;
                                        case e_PortMatchType.PCSL:
                                        case e_PortMatchType.PLPC:
                                        case e_PortMatchType.PLSC:
                                        case e_PortMatchType.SCPL:
                                        case e_PortMatchType.SLPC:
                                            PortMatching[ChannelCnt - 1].Port[iPort].R = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 5);
                                            PortMatching[ChannelCnt - 1].Port[iPort].L = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 6);
                                            PortMatching[ChannelCnt - 1].Port[iPort].C = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 7);
                                            PortMatching[ChannelCnt - 1].Port[iPort].G = cExtract.Get_Data_Double(FixtureStr, RowNo + iPort, 8);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    RowNo++;
                } while (tmpStr.ToUpper() != ("#EndMatching").ToUpper());

            }
        }
        public void Set_PortMatching()
        {
            bool b_Enable;
            b_Enable = false;
            for (int iChn = 0; iChn < PortMatching.Length; iChn++)
            {
                for (int iPort = 0; iPort < PortMatching[iChn].Port.Length; iPort++)
                {
                    if (PortMatching[iChn].Enable)
                    {
                        ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.Type(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].MatchType);
                        switch (PortMatching[iChn].Port[iPort].MatchType)
                        {
                            case e_PortMatchType.SLPC:
                            case e_PortMatchType.PCSL:
                            case e_PortMatchType.PLSC:
                            case e_PortMatchType.SCPL:
                            case e_PortMatchType.PLPC:
                                ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.R(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].R);
                                ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.L(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].L);
                                ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.C(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].C);
                                ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.G(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].G);
                                break;
                            case e_PortMatchType.USER:
                                ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.UserFilename(PortMatching[iChn].ChannelNumber, (iPort + 1), PortMatching[iChn].Port[iPort].UserFile);
                                break;
                        }
                        b_Enable = true;
                    }
                    else
                        ENA.Calculate.FixtureSimulator.SENDed.PMCircuit.State(PortMatching[iChn].ChannelNumber, PortMatching[iChn].Enable);

                }
            }
            if (b_Enable)
            {
                ENA.Calculate.FixtureSimulator.State(true);
            }
            else
            {
                ENA.Calculate.FixtureSimulator.State(false);
            }
        }
        #endregion
        #region "ZConversion Settings"
        public static s_ZConversion[] ZConversion;
        public static bool b_ZConversion;

        public void Init_ZConversion()
        {
            string FixtureStr = "Fixture Analysis";
            string tmpStr;
            int ChannelNumber;
            int ChannelCnt;
            int RowNo;
            bool b_ZCon;

            RowNo = 2;
            ChannelCnt = 0;
            b_ZCon = false;
            do
            {
                tmpStr = cExtract.Get_Data(FixtureStr, RowNo, 1);
                if (tmpStr.ToUpper() == "ZCONVERSION")
                {
                    b_ZConversion = General.convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, RowNo, 3));
                    ZConversion = new s_ZConversion[1];
                    b_ZCon = true;
                }

                if (b_ZCon)
                {
                    tmpStr = cExtract.Get_Data(FixtureStr, RowNo, 1);
                    if ((int.TryParse(tmpStr, out ChannelNumber)) && (tmpStr != ""))
                    {
                        if (ChannelNumber > 0)
                        {
                            ChannelCnt++;
                            if (ChannelCnt > 1)
                            {
                                Array.Resize(ref PortMatching, ChannelCnt);
                            }
                            ZConversion[ChannelCnt - 1].ChannelNumber = ChannelNumber;
                            ZConversion[ChannelCnt - 1].Enable = General.convertEnableDisable2Bool(cExtract.Get_Data(FixtureStr, RowNo, 2));
                            if (ZConversion[ChannelCnt - 1].Enable)
                            {
                                ZConversion[ChannelCnt - 1].PortConversion = new s_ZImpedance[SParamData[ChannelCnt - 1].NoPorts];
                                for (int iPort = 0; iPort < SParamData[ChannelCnt - 1].NoPorts; iPort++)
                                {
                                    ZConversion[ChannelCnt - 1].PortConversion[iPort].Z0 = cExtract.Get_Data_Double(FixtureStr, RowNo, 4);
                                    ZConversion[ChannelCnt - 1].PortConversion[iPort].Real = cExtract.Get_Data_Double(FixtureStr, RowNo, 5);
                                    ZConversion[ChannelCnt - 1].PortConversion[iPort].Imag = cExtract.Get_Data_Double(FixtureStr, RowNo, 6);
                                }
                            }
                        }
                    }
                }
                RowNo++;
            } while (tmpStr.ToUpper() != ("#EndZConversion").ToUpper());
        }
        public void Set_ZConversion()
        {
            bool b_FixtureState;  // Check for Fixture Analysis State
            bool b_Enable;

            b_Enable = false;
            b_FixtureState = ENA.Calculate.FixtureSimulator.State();

            for (int iChn = 0; iChn < ZConversion.Length; iChn++)
            {
                for (int iPort = 0; iPort < ZConversion[iChn].PortConversion.Length; iPort++)
                {
                    if (ZConversion[iChn].Enable)
                    {
                        ENA.Calculate.FixtureSimulator.SENDed.ZConversion.Z0(ZConversion[iChn].ChannelNumber, (iPort + 1), ZConversion[iChn].PortConversion[iPort].Z0);
                        ENA.Calculate.FixtureSimulator.SENDed.ZConversion.Real(ZConversion[iChn].ChannelNumber, (iPort + 1), ZConversion[iChn].PortConversion[iPort].Real);
                        ENA.Calculate.FixtureSimulator.SENDed.ZConversion.Imag(ZConversion[iChn].ChannelNumber, (iPort + 1), ZConversion[iChn].PortConversion[iPort].Imag);
                        b_Enable = true;
                    }
                    ENA.Calculate.FixtureSimulator.SENDed.ZConversion.State(ZConversion[iChn].Enable);
                }
            }
            if (!b_FixtureState)
            {
                if (b_Enable) ENA.Calculate.FixtureSimulator.State(b_Enable);
            }
        }
        #endregion
        #region "Grab Frequency list"
        public void GetFrequencyList()
        {
            double[] tmpFreqlist;
            for (int chn = 0; chn < TotalChannel; chn++)
            {
                ENA.Format.DATA(e_FormatData.REAL);
                tmpFreqlist = ENA.Sense.Frequency.FreqList(chn + 1);
                if (tmpFreqlist.Length > 0)
                {
                    SParamData[chn].Freq = new double[tmpFreqlist.Length];
                    SParamData[chn].Freq = tmpFreqlist;
                }
            }
        }
        
        public List<double> GetFrequencyList(int channel)
        {
            //ZNB
            ENA.Format.DATA(e_FormatData.REAL);
            List<double> freqs = ENA.Sense.Frequency.FreqList(channel).ToList();
            
            for (int i = 0; i < freqs.Count(); i++) freqs[i] *= 1e-6;

            return freqs;
        }

        #endregion
        #region Read Trace Data
        public double[] ReadENATrace(int channel, int trace_OneBased)
        {
            //ZNB
            ENA.Calculate.Par.Select(channel, trace_OneBased);
            
            ENA.Initiate.Immediate(channel); //InitSettings Immediate
            ENA.Trigger.Single(channel);
            ENA.BasicCommand.System.Operation_Complete();
            
            double[] fullDATA_X = ENA.Calculate.Data.FData(channel);
            
            return fullDATA_X;
        }
        #endregion Read Trace Data
        #region DefineTraceX
        public void defineTracex(int ChannelNum, int TraceNumber, e_SParametersDef Define)
        {
            //ZNB
            ENA.Calculate.Par.Define(ChannelNum, TraceNumber, Define);
        }
        #endregion DefineTraceX
        #region ThruResponseCal
        public void ThruReponseCal()
        {
            ENA.BasicCommand.SendCommand(":SYSTEM:DISPLAY:UPDATE ON");
            ENA.BasicCommand.SendCommand(":SENSE:CORRECTION:CKIT:N50:SELECT 'N 50 Ohm Ideal Kit'");
            ENA.BasicCommand.SendCommand(":SENSE1:CORRECTION:COLLECT:CONNECTION1 N50FEMALE");
            ENA.BasicCommand.SendCommand(":SENSE1:CORRECTION:COLLECT:CONNECTION2 N50FEMALE");
            ENA.BasicCommand.SendCommand(":SENSe:CORRection:COLLect:ACQuire:RSAVe:DEFault OFF");
            //// 2 port Bi-directional Trans Normalization
            //// Select cal procedure
            ENA.BasicCommand.SendCommand(":SENSe1:CORRection:COLLect:METHod:DEFine 'Test FTRans 12', FRTRans, 1, 2");
            //// Measure Standards
            ENA.BasicCommand.SendCommand(":SENSe1:CORRection:COLLect:ACQuire:SELected THROUGH, 1, 2");
            //// Apply calibration
            ENA.BasicCommand.SendCommand(":SENSe1:CORRection:COLLect:SAVE:SELected");
        }
        #endregion
        #region InterpolateLinear
        public static double InterpolateLinear(double lowerX, double upperX, double lowerY, double upperY, double xTarget)
        {
            try
            {
                return (((upperY - lowerY) * (xTarget - lowerX)) / (upperX - lowerX)) + lowerY;
            }
            catch (Exception e)
            {
                return -99999;
            }
        }
        #endregion InterpolateLinear
        public void Clear_TraceData()
        {
            traceData.Clear();
        }
        public void Load_StateFile(string Filename)
        {
            StateFile = Filename;
            ENA.BasicCommand.System.Reset();
            ENA.Memory.Load.State(Filename);
            string erroMsg = ENA.BasicCommand.System.QueryError();
            if (!(erroMsg.ToUpper().Contains("NO ERROR"))) 
            {
                b_ErrorFlag = true;
                General.DisplayError(ClassName, "Error in loading state file", erroMsg);
            }
            ENA.Format.Border(e_Format.NORM);
            Thread.Sleep(5000);
        }
        public void Save_StateFile(string Filename)
        {           
            ENA.Memory.Store.State(Filename);
        }
        public void SetTrigger(e_TriggerSource Trig)
        {
            ENA.Trigger.Source(Trig);
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                ENA.Initiate.Continuous(iChn + 1, true);
            }
        }
        public void SetTriggerSingle(e_TriggerSource Trig)
        {
            ENA.Trigger.Source(Trig);
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                ENA.Initiate.Continuous(iChn + 1, false);
            }
        }
        private static s_SParam_Grab[] DataTriggered;
        public s_SParam_Grab[] parse_SParamGrab
        {
            set
            {
                DataTriggered = value;
            }
        }
        static int DataTrigger_i;

        public cTestClasses[] TestClass;
        // Calibration Settings
        public cCalibrationClasses Calibration_Class;
        public void incr_DataTrigger()
        {
            DataTrigger_i++;
        }
        public class cTestClasses
        {
            public cTrigger Trigger;
            public cTrigger2 Trigger2;

            public cFreq_At Freq_At;
            public cMag_Between Mag_Between;
            public cCPL_Between CPL_Between;
            public cMag_At Mag_At;
            public cMag_At_Lin Mag_At_Lin;
            public cReal_At Real_At;
            public cImag_At Imag_At;
            public cRipple_Between Ripple_Between;
            public cPhase_At Phase_At;
            public cBalance Balance;
            public cChannel_Averaging Channel_Averaging;

            public interface iTestFunction
            {
                void RunTest();
                void InitSettings();
                void MeasureResult();
            }

            public class cTrigger : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Trigger Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int ChannelNumber;
                public int Sleep_ms;
                public int TestNo;

                public string Misc_Settings;

                public string FileOutput_Path;
                public string FileOutput_FileName;
                public string FileOutput_Mode;
                public int FileOutput_Unit;
                public bool FileOutput_Enable;

                //KCC - SNP File Count
                public int FileOutput_Count;
                public int FileOutput_Counting;
                public string SnPFile_Name;

                // Internal Variables
                e_SFormat tmp_SFormat;
                string tmpStr;
                private bool b_Trigger_Pause;

                public bool b_AutoCheckFormat = false;
                private bool b_Captured = false;

                Stopwatch watch = new Stopwatch();


                #endregion

                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    SaveResult[TestNo].Enable = false;
                    //ChannelNumber--;
                    if (Misc_Settings.Contains("P") || Misc_Settings.Contains("p"))
                    {
                        b_Trigger_Pause = true;
                    }
                }
                public void RunTest()
                {
                    if (ChannelNumber == 0)
                    {
                        //for (int i = 1; i < TotalChannel; i++)
                        //{
                        //    //ENA.Initiate.Immediate(i);
                        //}
                        ENA.Trigger.Single(ChannelNumber);
                        ENA.Initiate.Continuous(true); //ZNB Set Init continous on
                        ENA.BasicCommand.System.Operation_Complete();
                    }
                    else
                    {
                        ////    ENA.Display.Window.Activate(ChannelNumber);
                        if (ChannelNumber == 1) znbTraceOffset = 0;
                        //if (b_ZnbInUse && b_EnhanceDataFetch) 
                        //{
                        //    if (!b_ZnbDataReady)
                        //    {
                        //        ENA.Initiate.Immediate();
                        //        ENA.Trigger.Single(ChannelNumber);
                        //        ENA.BasicCommand.System.Operation_Complete();
                        //    }
                        //}
                        //else
                        //{
                        ENA.Initiate.Immediate(ChannelNumber); //InitSettings Immediate
                        ENA.Trigger.Single(ChannelNumber);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (SnPFile_Name != "")
                        {
                            long CurrentSN = 0;
                            try
                            {
                                CurrentSN = ATFCrossDomainWrapper.GetClothoCurrentSN();
                            }
                            catch
                            {
                                CurrentSN = 0;
                            }
                            string FileName;
                            if (b_ZnbInUse)
                            {
                                //FileName = FileOutput_FileName + "SN" + CurrentSN + "_" + FileOutput_Mode + "_CH" + ChannelNumber + "." + SnPFile_Name;
                                //ENA.Memory.Store.SNP.Data(ChannelNumber, FileName);
                            }
                            else
                            {
                                FileName = FileOutput_FileName + "SN" + CurrentSN + "_" + FileOutput_Mode + "." + SnPFile_Name;
                                ENA.Memory.Store.SNP.Data(FileName);
                            }
                            ENA.BasicCommand.System.Operation_Complete();
                        }
                    }

                    Thread.Sleep(Sleep_ms);
                    MeasureResult();

                    if (b_Trigger_Pause)
                    {
#if (DEBUG)
                        Debugger.Break();
#else
                        General.DisplayMessage(ClassName + " - " + SubClassName, "Pause Task", "Pause at Test " + TestNo.ToString());
#endif
                    }

                    //DataTrigger_i++;
                }
                public void MeasureResult()
                {
                    double[] readData;
                    int Select_SParam_Def;
                    int Select_SParam_Def_Arr;
                    int traceOffset = 0;
                    ENA.Format.DATA(e_FormatData.REAL);
                    ENA.Format.Border(e_Format.NORM);
                    if (ChannelNumber == 0)
                    {
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            for (int iParam = 0; iParam < (SParamData[iChn].NoPorts * SParamData[iChn].NoPorts); iParam++)
                            {
                                //Select_SParam_Def = SParam_Def2Value(iParam, iChn);
                                //Select_SParam_Def = SParam_Def2Value(iParam);
                                Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                                Select_SParam_Def_Arr = TraceMatch[iChn].TraceNumber[Select_SParam_Def];
                                if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                                {
                                    ENA.Calculate.Par.Select(iChn + 1, Select_SParam_Def_Arr + znbTraceOffset);
                                    tmp_SFormat = ENA.Calculate.Format.Format(iChn + 1, Select_SParam_Def_Arr + znbTraceOffset);
                                    readData = ENA.Calculate.Data.SData(iChn + 1);
                                    TransferData(readData, iChn, Select_SParam_Def_Arr, tmp_SFormat);
                                    traceOffset++;
                                }
                            }
                            if (FileOutput_Enable)
                            {
                                if (FileOutput_Mode != "")
                                {
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, FileOutput_Unit.ToString(), (iChn + 1));
                                }
                                else
                                {
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Unit.ToString(), (iChn + 1));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (b_EnhanceDataFetch)
                        {
                            #region "Enhance Data Transfer"
                            StringBuilder TraceNumberStr = new StringBuilder();
                            StringBuilder TraceNumberStrOffset = new StringBuilder();
                            //int iTraceCount = 0;

                            if (b_ZnbInUse) //ZNB
                            {

                                if (!b_Captured)
                                {
                                    

                                    //TraceMatch[ChannelNumber].TotalTraces
                                    string ZnbTraceParam;
                                    string[] allTraces, traces;
                                    int[] traceMap;

                                    #region Sorting Traces into correct order

                                    ZnbTraceParam = ENA.Calculate.Par.GetTraceCategory(ChannelNumber);
                                    ZnbTraceParam = ZnbTraceParam.Replace("'", "").Replace("\n", "").Trim();
                                    allTraces = ZnbTraceParam.Split(new char[] { ',' });
                                    //Get only odd number
                                    traces = allTraces.Where((item, index) => index % 2 != 0).ToArray();
                                    traceMap = new int[traces.Length];

                                    try
                                    {
                                        int trcStart = Convert.ToInt32(allTraces[0].Trim().Substring(3));
 
                                        for (int i = 0; i < traces.Length; i++)
                                        {
                                            traceMap[i] = i;
                                            //traceMap[i] = TraceMatch[ChannelNumber - 1].TraceNumber[((e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), traces[i])).GetHashCode()];
                                            SParamData[ChannelNumber - 1].sParam_Data[traceMap[i]].Format = ENA.Calculate.Format.Format(ChannelNumber, traceMap[i] + trcStart);   // SiteNumber
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string fault = ex.Message;
                                    }
                                    #endregion
                                }

                                znbData = ENA.Calculate.Data.AllFData(ChannelNumber);   // SiteNumber

                                //if (ChannelNumber > 1)

                                //{
                                //    readData = new double[znbData.Length - znbDataOffset];
                                //    //Buffer.BlockCopy(znbData, znbDataOffset, readData, 0, znbData.Length - znbDataOffset);
                                //    Array.Copy(znbData, znbDataOffset, readData, 0, znbData.Length - znbDataOffset);
                                //    TransferEnhanceData(readData, ChannelNumber - 1, TraceNumberStr.ToString().Trim(','));
                                //}
                                //else
                                //{
                                //readData = new double[znbData.Length];
                                //Buffer.BlockCopy(znbData, 0, readData, 0, znbData.Length);
                                //readData = znbData;
                                //znbDataOffset = 0;
                                TransferEnhanceDataZNB(znbData, ChannelNumber - 1);
                                //}

                                //if (!b_ZnbDataReady) //Only read once for ZNB
                                //{
                                //    znbData = ENA.Calculate.Data.AllFData(ChannelNumber);   // SiteNumber
                                //    readData = znbData;
                                //    b_ZnbDataReady = true;
                                //}
                                //else
                                //{
                                //    readData = new double[znbData.Length - znbDataOffset];
                                //    Array.Copy(znbData, znbDataOffset, readData, 0, znbData.Length - znbDataOffset);
                                //}
                            }
                            else //ENA Part
                            {

                                for (int iParam = 0; iParam < (SParamData[ChannelNumber - 1].NoPorts * SParamData[ChannelNumber - 1].NoPorts); iParam++)
                                {
                                    Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];
                                    if (Select_SParam_Def >= 0)
                                    {
                                        Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                                        if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                                        {
                                            TraceNumberStr.AppendFormat("{0},", Select_SParam_Def_Arr);
                                            TraceNumberStrOffset.AppendFormat("{0},", Select_SParam_Def_Arr + 1);
                                            if (!b_Captured && !b_ZnbInUse) SParamData[ChannelNumber - 1].sParam_Data[Select_SParam_Def_Arr].Format = ENA.Calculate.Format.Format(ChannelNumber, (Select_SParam_Def_Arr + 1) + znbTraceOffset);   // SiteNumber
                                            //iTraceCount++;
                                            traceOffset++;
                                        }

                                    }
                                }

                                //if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;
                                if (TraceNumberStr.ToString().Trim(',').Length > 0)
                                {

                                    readData = ENA.Calculate.Data.FMultiTrace_Data(ChannelNumber, TraceNumberStrOffset.ToString().Trim(','));   // SiteNumber
                                    TransferEnhanceData(readData, ChannelNumber - 1, TraceNumberStr.ToString().Trim(','));

                                }
                            }

                            if ((!b_AutoCheckFormat) && (!b_Captured)) b_Captured = true;

                            #endregion


                            if ((FileOutput_Enable) && ((FileOutput_Count == 999) || (FileOutput_Counting <= FileOutput_Count - 1)))
                            {
                                if (FileOutput_Mode != "")
                                {
                                    //KCC - Changed unit to +1 for Clotho
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, (FileOutput_Unit + 1).ToString(), ChannelNumber);
                                }
                                else
                                {
                                    //KCC - Changed unit to +1 for Clotho
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, (FileOutput_Unit + 1).ToString(), ChannelNumber);
                                }
                            }

                        }
                        else
                        {
                            for (int iParam = 0; iParam < (SParamData[ChannelNumber - 1].NoPorts * SParamData[ChannelNumber - 1].NoPorts); iParam++)
                            {

                                //Select_SParam_Def = SParam_Def2Value(iParam, ChannelNumber - 1);
                                //Select_SParam_Def = SParam_Def2Value(iParam);
                                Select_SParam_Def = TraceMatch[ChannelNumber - 1].SParam_Def_Number[iParam];

                                if (Select_SParam_Def >= 0)
                                {
                                    Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                                    if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                                    {
                                        //watch.Reset();
                                        //watch.Start();
                                        ENA.Calculate.Par.Select(ChannelNumber, (Select_SParam_Def_Arr + 1) + znbTraceOffset);
                                        tmp_SFormat = ENA.Calculate.Format.Format(ChannelNumber, (Select_SParam_Def_Arr + 1) + znbTraceOffset);
                                        readData = ENA.Calculate.Data.FData(ChannelNumber);
                                        TransferData(readData, ChannelNumber - 1, Select_SParam_Def_Arr, tmp_SFormat);
                                        //watch.Stop();
                                        //General.DisplayError(ClassName, "Timming", "Elapsed Time = : " + watch.ElapsedMilliseconds.ToString());
                                        traceOffset++;
                                    }
                                }
                            }
                        }
                        if (b_ZnbInUse) znbTraceOffset += traceOffset;
                        //KCC - Added file count
                        if ((FileOutput_Enable) && ((FileOutput_Count == 999) || (FileOutput_Counting <= FileOutput_Count - 1)))
                        {
                            if (FileOutput_Mode != "")
                            {
                                //KCC - Changed unit to +1 for Clotho
                                SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, (FileOutput_Unit + 1).ToString(), ChannelNumber);
                            }
                            else
                            {
                                //KCC - Changed unit to +1 for Clotho
                                SaveFile2SNP(FileOutput_Path, FileOutput_FileName, (FileOutput_Unit + 1).ToString(), ChannelNumber);
                            }
                        }
                    }
                }
            }

            public class cTrigger2 : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Trigger 2 Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int ChannelNumber;
                public int Sleep_ms;
                public int TestNo;

                public string Misc_Settings;

                public string FileOutput_Path;
                public string FileOutput_FileName;
                public string FileOutput_Mode;
                public int FileOutput_Unit;
                public bool FileOutput_Enable;

                // Internal Variables
                e_SFormat tmp_SFormat;
                private bool b_Trigger_Pause;
                #endregion

                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    SaveResult[TestNo].Enable = false;
                    //ChannelNumber--;
                    if (ChannelNumber == 0)
                    {
                        for (int i = 0; i < TotalChannel; i++)
                        {
                            ENA.Initiate.Continuous(i, true);
                        }
                    }
                    else
                    {
                        ENA.Initiate.Continuous(ChannelNumber, true);
                    }
                    if (Misc_Settings.Contains("P") || Misc_Settings.Contains("p"))
                    {
                        b_Trigger_Pause = true;
                    }
                }
                public void RunTest()
                {
                    ENA.Trigger.Single();
                    ENA.BasicCommand.System.Operation_Complete();
                    Thread.Sleep(Sleep_ms);
                    MeasureResult();
                    //DataTrigger_i++;
                    if (b_Trigger_Pause)
                    {
#if (DEBUG)
                        Debugger.Break();
#else
                        General.DisplayMessage(ClassName + " - " + SubClassName, "Pause Task", "Pause at Test " + TestNo.ToString());
#endif
                    }
                }
                public void MeasureResult()
                {
                    double[] readData;
                    int Select_SParam_Def;
                    int Select_SParam_Def_Arr;
                    ENA.Format.DATA(e_FormatData.REAL);
                    if (ChannelNumber == 0)
                    {
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            for (int iParam = 0; iParam < (SParamData[iChn].NoPorts * SParamData[iChn].NoPorts); iParam++)
                            {
                                Select_SParam_Def = SParam_Def2Value(iParam, iChn);
                                Select_SParam_Def_Arr = TraceMatch[iChn].TraceNumber[Select_SParam_Def];
                                if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                                {
                                    ENA.Calculate.Par.Select(iChn + 1, Select_SParam_Def_Arr * (iChn + 1));
                                    tmp_SFormat = ENA.Calculate.Format.Format(iChn + 1);
                                    readData = ENA.Calculate.Data.FData(iChn + 1);
                                    TransferData(readData, iChn, Select_SParam_Def_Arr, tmp_SFormat);
                                }
                            }
                            if (FileOutput_Enable)
                            {
                                if (FileOutput_Mode != "")
                                {
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, FileOutput_Unit.ToString(), (iChn + 1));
                                }
                                else
                                {
                                    SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Unit.ToString(), (iChn + 1));
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int iParam = 0; iParam < (SParamData[ChannelNumber - 1].NoPorts * SParamData[ChannelNumber - 1].NoPorts); iParam++)
                        {
                            Select_SParam_Def = SParam_Def2Value(iParam, ChannelNumber - 1);
                            Select_SParam_Def_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[Select_SParam_Def];
                            if (DataTriggered[DataTrigger_i].SParam_Grab[Select_SParam_Def])
                            {
                                ENA.Calculate.Par.Select(ChannelNumber, Select_SParam_Def_Arr * ChannelNumber);
                                tmp_SFormat = ENA.Calculate.Format.Format(ChannelNumber);
                                readData = ENA.Calculate.Data.FData(ChannelNumber);
                                TransferData(readData, ChannelNumber - 1, Select_SParam_Def_Arr, tmp_SFormat);
                            }
                        }
                        if (FileOutput_Enable)
                        {
                            if (FileOutput_Mode != "")
                            {
                                SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Mode, FileOutput_Unit.ToString(), ChannelNumber);
                            }
                            else
                            {
                                SaveFile2SNP(FileOutput_Path, FileOutput_FileName, FileOutput_Unit.ToString(), ChannelNumber);
                            }
                        }
                    }
                }
            }

            public class cFreq_At : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Freq At";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public double StartFreq_Point;
                public double StopFreq_Point;
                public string Search_DirectionMethod;
                public string Search_Type;
                public string Interpolation;
                public int Channel_Number;
                public string SParameters;
                public string Search_Value = "";
                public bool b_Invert_Search;
                public double Offset;
                //Below for Second Report File
                public string Band;
                public string PowerMode;
                public string Port_Impedance;
                
                // Internal Variables
                private e_SearchDirection SearchDirection;
                private e_SearchType SearchType;
                private bool b_Interpolation;
                private int StartFreqCnt;
                private int StopFreqCnt;
                private double tmpCnt;
                private int SParam;
                private double SearchValue;

                private double compareResult;
                private double rtnResult;
                private double rtnResultMag;
                private double rtnMiscResult;

                private bool Found;

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Multi_Results = new s_mRslt[2];
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[0].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[1].Result_Header = value.Replace(" ", "_") + "_Freq";
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings
                public void InitSettings()
                {
                    ////ChannelNumber--;
                    SearchDirection = (e_SearchDirection)Enum.Parse(typeof(e_SearchDirection), Search_DirectionMethod.ToUpper());
                   
                    if ((SearchDirection == e_SearchDirection.FROM_EXTREME_LEFT) || (SearchDirection == e_SearchDirection.FROM_EXTREME_RIGHT))
                    {
                        StartFreqCnt = 0;
                        StopFreqCnt = SParamData[Channel_Number - 1].NoPoints - 1;
                    }
                    else
                    {
                        #region "Start Point"

                        tmpCnt = 0;
                        Found = false;
                        for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                        {
                            if (StartFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StartFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                StartFreq_Point = ((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Start Point", "Start Frequency = " + StartFreq.ToString());
                        }
                        #endregion        // Calculate for Start Point
                        #region "End Point"

                        tmpCnt = 0;
                        Found = false;
                        for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
                        {
                            if (StopFreq > SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StopFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                StopFreq_Point = ((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                                if ((StopFreq_Point - StopFreqCnt) > 0)
                                {
                                    StopFreqCnt++;
                                }
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Stop Point", "Stop Frequency = " + StopFreq.ToString());
                        }
                        #endregion             // Calculate for Stop Point
                    }
                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                    if (Search_Value.Trim() == "")
                    {
                        SearchValue = -9999;
                        SearchType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_Type.ToUpper());
                    }
                    else
                    {
                        SearchValue = Convert.ToDouble(Search_Value);
                        SearchType = e_SearchType.USER;
                        if ((SearchValue < 0) && !((SearchDirection == e_SearchDirection.FROM_MAX_LEFT) || (SearchDirection == e_SearchDirection.FROM_MAX_RIGHT)))
                        {
                            b_Invert_Search = !b_Invert_Search;
                        }
                    }

                    b_Interpolation = General.CStr2Bool(Interpolation);
                    //SaveResult[TestNo].Enable = true;
                    //SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Enable = false;
                    SaveResult[TestNo].b_MultiResult = true;
                    SaveResult[TestNo].Multi_Results[0].Enable = true;
                    SaveResult[TestNo].Multi_Results[1].Enable = true;
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR

                public void GenerateSecondReportFile(SnP_BuddyFileBuilder.SnPFileBuilder zFileHan, int snpiterator, int MaxNumSnp)
                { 
                    try
                    {
                        if (traceData[Channel_Number - 1, SParam] == null)
                        {
                            //int snpiterator = 1;  //Snp file will be saved every snpiterator
                            //int MaxNumSnp = 200;

                            double[] forHertz = SParamData[Channel_Number - 1].Freq;
                            double[] holder = new double[SParamData[Channel_Number - 1].Freq.Length * 2];
                            double[] traceDataArr = new double[SParamData[Channel_Number - 1].Freq.Length];
                            
                            zFileHan.SnPParHeader(SParameters, forHertz, Convert.ToSingle(Port_Impedance), Band, PowerMode); //have to make sure this actually gets populated properly
                            
                            for (int iPts = 0; iPts < SParamData[Channel_Number - 1].Freq.Length; iPts++)
                            {
                                traceDataArr[iPts] = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iPts].dBAng.dB;
                                holder[iPts * 2] = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iPts].ReIm.Real;
                                holder[(iPts * 2) + 1] = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iPts].ReIm.Imag;
                            }

                            traceData[Channel_Number - 1, SParam] = traceDataArr;
                       
                            string currDUTSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0");
                            if (Convert.ToInt32(currDUTSN) % snpiterator != 0 || Convert.ToInt32(currDUTSN) / snpiterator > MaxNumSnp) return;
                            
                            zFileHan.SnPParBody(holder);
                        }
                    }

                    catch (Exception e)
                    {
                        General.DisplayMessage("cFBAR.cs", "GenerateSecondReportFile", "Error happened during updatetracefile" + "\r\n" + e.ToString());
                    }
                }

                public void MeasureResult()
                {
                    double Peak_Value;
                    //double tmpVal;
                    double Gradient;

                    int Peak_Pos_i;

                    int Start_i;
                    int Stop_i;
                    int Step_i;

                    int tmp_i;
                    int tmp_Peak_i;

                    int Remain_i;

                    tmp_i = 0;
                    Start_i = 0;
                    Stop_i = 0;
                    Step_i = 0;
                    tmp_Peak_i = 0;
                    Peak_Pos_i = 0;
                    try
                    {
                        #region "Search Peak Value and Position"
                        if ((SearchDirection == e_SearchDirection.FROM_MAX_LEFT) || (SearchDirection == e_SearchDirection.FROM_MAX_RIGHT))
                        {
                            // Look for Peak Value and Position First
                            Peak_Pos_i = 0;
                            Peak_Value = -999999;
                            for (int iArr = StartFreqCnt; iArr <= StopFreqCnt; iArr++)
                            {
                                if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iArr].dBAng.dB > Peak_Value)
                                {
                                    Peak_Value = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iArr].dBAng.dB;
                                    Peak_Pos_i = iArr;
                                }
                            }
                        }
                        #endregion

                        #region "Search Setting"
                        switch (SearchDirection)
                        {
                            case e_SearchDirection.FROM_LEFT:
                            case e_SearchDirection.FROM_EXTREME_LEFT:
                                Start_i = StartFreqCnt;
                                Stop_i = StopFreqCnt;
                                Step_i = 1;
                                if (Start_i == Stop_i) Step_i = 0;
                                break;
                            case e_SearchDirection.FROM_RIGHT:
                            case e_SearchDirection.FROM_EXTREME_RIGHT:
                                Start_i = StopFreqCnt;
                                Stop_i = StartFreqCnt;
                                Step_i = -1;
                                if (Start_i == Stop_i) Step_i = 0;
                                break;
                            case e_SearchDirection.FROM_MAX_LEFT:
                                Start_i = Peak_Pos_i;
                                Stop_i = StartFreqCnt;
                                Step_i = -1;
                                if (Start_i == Stop_i) Step_i = 0;  // f the Max Point at Extreme End
                                break;
                            case e_SearchDirection.FROM_MAX_RIGHT:
                                Start_i = Peak_Pos_i;
                                Stop_i = StopFreqCnt;
                                Step_i = 1;
                                if (Start_i == Stop_i) Step_i = 0;  // f the Max Point at Extreme End
                                break;
                        }
                        #endregion

                        if (SearchType == e_SearchType.MAX)
                        {
                            #region "Search Max Result"
                            if (Step_i == 0)
                            {
                                // Error pervention 
                                rtnResult = SParamData[Channel_Number - 1].Freq[Start_i];
                                rtnMiscResult = Start_i;
                            }
                            else
                            {
                                compareResult = -90000000;
                                rtnResult = -90000000;
                                tmp_i = Start_i;
                                //tmp_Peak_i = 0;
                                do
                                {
                                    if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB > compareResult)
                                    {
                                        rtnResult = SParamData[Channel_Number - 1].Freq[tmp_i];
                                        compareResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB;
                                        rtnMiscResult = tmp_i;
                                    }
                                    tmp_i += Step_i;
                                    //} while (tmp_i != Stop_i);  //Bug found - max loop will stop at Stop_i - 1 - Shaz 06/Dec/2012
                                } while (tmp_i != (Stop_i + Step_i));

                                if (b_Interpolation)
                                {
                                    if (rtnMiscResult < StartFreq_Point)
                                    {
                                        rtnResult = StartFreq;
                                        rtnMiscResult = StartFreq_Point; // (((StartFreq_Point - Start_i) / ((Start_i + Step_i) - (Start_i)))) + (Start_i);
                                    }
                                    else if (rtnMiscResult > StopFreq_Point)
                                    {
                                        rtnResult = StopFreq;
                                        rtnMiscResult = StopFreq_Point;// (((StopFreq_Point - (Stop_i - Step_i)) / ((Stop_i) - (Stop_i - Step_i)))) + (Stop_i + Step_i);
                                    }

                                }
                            }
                            #endregion
                        }
                        else if (SearchType == e_SearchType.MIN)
                        {
                            #region "Search Min Result"
                            if (Step_i == 0)
                            {
                                // Error pervention 
                                rtnResult = SParamData[Channel_Number - 1].Freq[Start_i];
                                rtnMiscResult = Start_i;
                            }
                            else
                            {
                                compareResult = 90000000;
                                rtnResult = 90000000;
                                tmp_i = Start_i;
                                //tmp_Peak_i = 0;
                                do
                                {
                                    if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB < compareResult)
                                    {
                                        rtnResult = SParamData[Channel_Number - 1].Freq[tmp_i];
                                        compareResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB;
                                        rtnMiscResult = tmp_i;
                                    }
                                    tmp_i += Step_i;
                                    //} while (tmp_i != Stop_i);  //Bug found - max loop will stop at Stop_i - 1 - Shaz 06/Dec/2012
                                } while (tmp_i != (Stop_i + Step_i));

                                if (b_Interpolation)
                                {
                                    if (rtnMiscResult < StartFreq_Point)
                                    {
                                        rtnResult = StartFreq;
                                        rtnMiscResult = StartFreq_Point; // (((StartFreq_Point - Start_i) / ((Start_i + Step_i) - (Start_i)))) + (Start_i);
                                    }
                                    else if (rtnMiscResult > StopFreq_Point)
                                    {
                                        rtnResult = StopFreq;
                                        rtnMiscResult = StopFreq_Point; // (((StopFreq_Point - (Stop_i - Step_i)) / ((Stop_i) - (Stop_i - Step_i)))) + (Stop_i + Step_i);
                                    }

                                }
                            }
                            #endregion
                        }
                        else
                        {
                            if (Step_i == 0)
                            {
                                // Error pervention 
                                rtnResult = SParamData[Channel_Number - 1].Freq[Start_i];
                                rtnMiscResult = Start_i;
                            }
                            else
                            {
                                if (false)
                                {
                                    #region SJ setup
                                    double ENAresultFreq = 0, ENAresult = 0;

                                    Dictionary<int, List<double>> dictFreq = new Dictionary<int, List<double>>();
                                    List<double> list = new List<double>();
                                    Dictionary.DoubleKey<int, int, double[]> traceDataDB = new Dictionary.DoubleKey<int, int, double[]>();
                                    double[] traceDataArr = new double[SParamData[Channel_Number - 1].Freq.Length];

                                    for (int iPts = 0; iPts < SParamData[Channel_Number - 1].Freq.Length; iPts++)
                                    {
                                        traceDataArr[iPts] = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iPts].dBAng.dB;
                                        list.Add(SParamData[Channel_Number - 1].Freq[iPts]);
                                    }

                                    dictFreq.Add(Channel_Number - 1, list);
                                    if (traceData[Channel_Number - 1, SParam] == null)
                                    {
                                        traceData[Channel_Number - 1, SParam] = traceDataArr;
                                    }

                                    SortedList<double, double> subTrace = new SortedList<double, double>();

                                    int startFreqI = dictFreq[Channel_Number - 1].BinarySearch(StartFreq);
                                    if (startFreqI < 0)  // start frequency not found, must interpolate
                                    {
                                        startFreqI = ~startFreqI;   // index just after target freq
                                        subTrace[StartFreq] = InterpolateLinear(dictFreq[Channel_Number - 1][startFreqI - 1], dictFreq[Channel_Number - 1][startFreqI], traceData[Channel_Number - 1, SParam][startFreqI - 1], traceData[Channel_Number - 1, SParam][startFreqI], StartFreq);
                                    }

                                    int endFreqI = dictFreq[Channel_Number - 1].BinarySearch(StopFreq);
                                    if (endFreqI < 0)  // end frequency not found, must interpolate
                                    {
                                        endFreqI = ~endFreqI - 1;   // index just before target freq
                                        subTrace[StopFreq] = InterpolateLinear(dictFreq[Channel_Number - 1][endFreqI], dictFreq[Channel_Number - 1][endFreqI + 1], traceData[Channel_Number - 1, SParam][endFreqI], traceData[Channel_Number - 1, SParam][endFreqI + 1], StopFreq);
                                    }

                                    for (int i = startFreqI; i <= endFreqI; i++) subTrace[dictFreq[Channel_Number - 1][i]] = traceData[Channel_Number - 1, SParam][i];


                                    switch (SearchDirection)
                                    {
                                        case e_SearchDirection.FROM_LEFT:

                                            #region Search Left

                                            try
                                            {

                                                for (int i = subTrace.Count() - 2; i >= 0; i--)
                                                {
                                                    if (subTrace.ElementAt(i).Value < SearchValue)
                                                    {
                                                        ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchValue);
                                                        ENAresult = SearchValue;
                                                        break;
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    for (int i = 1; i < subTrace.Count(); i++)
                                                    {
                                                        if (subTrace.ElementAt(i).Value > SearchValue)
                                                        {
                                                            ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchValue);
                                                            ENAresult = SearchValue;
                                                            break;
                                                        }
                                                    }

                                                }
                                                catch
                                                {
                                                    ENAresultFreq = 0;
                                                    ENAresult = 0;
                                                }
                                            }

                                            break;
                                            #endregion

                                        case e_SearchDirection.FROM_RIGHT:

                                            #region Search Right
                                            try
                                            {

                                                for (int i = 0; i <= subTrace.Count() - 2; i++)
                                                {
                                                    if (subTrace.ElementAt(i).Value < SearchValue)
                                                    {
                                                        ENAresultFreq = InterpolateLinear(subTrace.Values[i - 1], subTrace.Values[i], subTrace.Keys[i - 1], subTrace.Keys[i], SearchValue);
                                                        ENAresult = SearchValue;
                                                        break;
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    for (int i = subTrace.Count() - 2; i >= 0; i--)
                                                    {
                                                        if (subTrace.ElementAt(i).Value > SearchValue)
                                                        {
                                                            ENAresultFreq = InterpolateLinear(subTrace.Values[i], subTrace.Values[i + 1], subTrace.Keys[i], subTrace.Keys[i + 1], SearchValue);
                                                            ENAresult = SearchValue;
                                                            break;
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    ENAresultFreq = 0;
                                                    ENAresult = 0;
                                                }
                                            }

                                            break;
                                            #endregion
                                    }

                                    rtnResult = ENAresultFreq;
                                    rtnResultMag = ENAresult;

                                    #endregion SJ setup
                                }
                                if (true)
                                {
                                    #region "Search Result"
                                    bool positive_slope = false;
                                    tmp_i = Start_i;
                                    do
                                    {
                                        if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB < SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i + Step_i].dBAng.dB)
                                            positive_slope = true;
                                        else
                                            positive_slope = false;

                                        if (positive_slope)
                                        //if (b_Invert_Search) - bug found on 08/08/12, when search method = user, b_Invert_Search will always true. Add positive_slope flag to ensure we grab the right data
                                        {
                                            if ((SearchValue > SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB) &&
                                                (SearchValue < SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i + Step_i].dBAng.dB))
                                            {
                                                tmp_Peak_i = tmp_i;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if ((SearchValue < SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i].dBAng.dB) &&
                                                (SearchValue > SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_i + Step_i].dBAng.dB))
                                            {
                                                tmp_Peak_i = tmp_i;
                                                break;
                                            }
                                        }
                                        tmp_i = tmp_i + Step_i;
                                        Remain_i = Math.Abs(Stop_i - tmp_i);
                                    } while (Remain_i > 0);

                                    if (b_Interpolation)
                                    {
                                        if (tmp_Peak_i != 0)
                                        {
                                            Gradient = (SParamData[Channel_Number - 1].Freq[tmp_Peak_i + Step_i] - SParamData[Channel_Number - 1].Freq[tmp_Peak_i]) /
                                                            (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_Peak_i + Step_i].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_Peak_i].dBAng.dB);

                                            rtnResult = ((SearchValue - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_Peak_i].dBAng.dB) * Gradient) + SParamData[Channel_Number - 1].Freq[tmp_Peak_i];
                                        }
                                        else
                                        {
                                            rtnResult = SParamData[Channel_Number - 1].Freq[tmp_Peak_i];
                                        }
                                    }
                                    else
                                    {
                                        double Point1 = Math.Abs(SearchValue - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_Peak_i].dBAng.dB);
                                        double Point2 = Math.Abs(SearchValue - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[tmp_Peak_i + Step_i].dBAng.dB);
                                        if (Point1 < Point2)
                                        {
                                            rtnResult = SParamData[Channel_Number - 1].Freq[tmp_Peak_i];
                                        }
                                        else
                                        {
                                            rtnResult = SParamData[Channel_Number - 1].Freq[tmp_Peak_i + Step_i];
                                        }
                                    }
                                    rtnMiscResult = tmp_Peak_i;
                                    #endregion
                                }
                            }
                        }
                        if (true)
                        {
                            int i_TargetFreqCnt = (int)Math.Floor(rtnMiscResult);
                            if (rtnMiscResult - i_TargetFreqCnt == 0)
                            {
                                rtnResultMag = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                            }
                            else
                            {
                                rtnResultMag = ((rtnMiscResult - i_TargetFreqCnt) * (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt + 1].dBAng.dB
                                    - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB)) + SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                            }

                        }
                    }
                    catch
                    {
                        General.DisplayError(ClassName + "-->" + SubClassName, "Error",
                            "Test Number = " + TestNo.ToString() +
                            "Start = " + Start_i.ToString() +
                            "Stop = " + Stop_i.ToString() +
                            "Step = " + Step_i.ToString() +
                            "tmp_Peak = " + tmp_Peak_i.ToString() +
                            "Peak_Pos = " + Peak_Pos_i.ToString() +
                            "Tmp = " + tmp_i.ToString());
                    }
                }
                
                void SetResult()
                {
                    //SaveResult[TestNo].Result_Data = rtnResult;
                    SaveResult[TestNo].Multi_Results[0].Result_Data = rtnResultMag;
                    SaveResult[TestNo].Multi_Results[1].Result_Data = rtnResult * 1e-6;
                    SaveResult[TestNo].Misc = rtnMiscResult;
                }
            }

            public class cMag_At : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Magnitude At Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public int ChannelNumber;
                public string SParameters;
                public string Interpolation;
                public double Frequency_At;
                public int Previous_TestNo;

                public double Offset;

                // Internal Variables
                private int SParam;
                private int SParam_Arr;
                private bool b_Interpolation;
                private double tmpCnt;
                private double TargetFreqCnt;
                private int Point1;
                private int Point2;
                private double PartialGradient;
                private bool b_SinglePoint;
                private double rtnResult;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    //MeasureResult();
                    //SetResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    double StepFreq;
                    //ChannelNumber--;

                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    b_Interpolation = General.CStr2Bool(Interpolation);

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[ChannelNumber - 1].TraceNumber[SParamDef.GetHashCode()];
                    //ChannelNumber = ChannelNumber - 1;
                    //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
                    if (Previous_TestNo < 0)
                    {
                        #region "Target Point"


                        Found = false;
                        for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].segm; seg++)
                        {
                            if (Frequency_At >= SegmentParam[ChannelNumber - 1].SegmentData[seg].Start && Frequency_At <= SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[ChannelNumber - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                if (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop == SegmentParam[ChannelNumber - 1].SegmentData[seg].Start)
                                {
                                    TargetFreqCnt = tmpCnt;
                                    Point1 = (int)tmpCnt;
                                    b_SinglePoint = true;
                                }
                                else
                                {
                                    TargetFreqCnt = ((Frequency_At - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) * (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                    if (seg == (SegmentParam[ChannelNumber - 1].SegmentData.Length - 1))
                                    {
                                        if (Frequency_At == SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop)
                                        {
                                            Point1 = (int)Math.Floor(TargetFreqCnt);
                                            b_SinglePoint = true;
                                        }
                                        else
                                        {
                                            if (b_Interpolation == true)
                                            {
                                                Point1 = (int)Math.Floor(TargetFreqCnt);    //Remove the Decimal Point
                                                Point2 = Point1 + 1;
                                                StepFreq = (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1);
                                                PartialGradient = (Frequency_At - (((Point1 - tmpCnt) * StepFreq) + SegmentParam[ChannelNumber - 1].SegmentData[seg].Start)) / StepFreq;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Target Point for Test Number " + TestNo.ToString(), "Target Frequency = " + Frequency_At.ToString());
                            ErrorRaise = true;
                        }
                        #endregion        // Calculate for Start Point
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                    }
                    int i_TargetFreqCnt = 0;
                    if (Previous_TestNo > 0)
                    {
                        //i_TargetFreqCnt = SaveResult[Previous_TestNo].Misc;
                        //rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        i_TargetFreqCnt = (int)Math.Floor(SaveResult[Previous_TestNo].Misc);
                        if (SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt == 0)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                        else
                        {
                            rtnResult = ((SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt) * (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt + 1].dBAng.dB - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB)) + SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                    }
                    else
                    {
                        if (b_SinglePoint)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].dBAng.dB;
                        }
                        else
                        {
                            if (b_Interpolation)
                            {
                                rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].dBAng.dB +
                                    (PartialGradient *
                                    (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point2].dBAng.dB - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].dBAng.dB));
                            }
                            else
                            {
                                i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
                                rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                            }
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }

            }

            //KCC - Added for Apollo
            public class cMag_At_Lin : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Magnitude Linear At Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public int ChannelNumber;
                public string SParameters;
                public string Interpolation;
                public double Frequency_At;
                public int Previous_TestNo;

                public double Offset;

                // Internal Variables
                private int SParam;
                private int SParam_Arr;
                private bool b_Interpolation;
                private double tmpCnt;
                private double TargetFreqCnt;
                private int Point1;
                private int Point2;
                private double PartialGradient;

                private double rtnResult;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    //MeasureResult();
                    //SetResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    double StepFreq;
                    //ChannelNumber--;

                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    b_Interpolation = General.CStr2Bool(Interpolation);

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[ChannelNumber - 1].TraceNumber[SParamDef.GetHashCode()];
                    //ChannelNumber = ChannelNumber - 1;
                    //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
                    if (Previous_TestNo < 0)
                    {
                        #region "Target Point"


                        Found = false;
                        for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].segm; seg++)
                        {
                            if (Frequency_At >= SegmentParam[ChannelNumber - 1].SegmentData[seg].Start && Frequency_At <= SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[ChannelNumber - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                TargetFreqCnt = ((Frequency_At - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) * (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                if (b_Interpolation == true)
                                {
                                    Point1 = (int)Math.Floor(TargetFreqCnt);    //Remove the Decimal Point
                                    Point2 = Point1 + 1;
                                    StepFreq = (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1);
                                    PartialGradient = (Frequency_At - (((Point1 - tmpCnt) * StepFreq) + SegmentParam[ChannelNumber - 1].SegmentData[seg].Start)) / StepFreq;
                                }
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Target Point for Test Number " + TestNo.ToString(), "Target Frequency = " + Frequency_At.ToString());
                            ErrorRaise = true;
                        }
                        #endregion        // Calculate for Start Point
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                    }
                    int i_TargetFreqCnt = 0;
                    if (Previous_TestNo > 0)
                    {
                        //i_TargetFreqCnt = SaveResult[Previous_TestNo].Misc;
                        //rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        i_TargetFreqCnt = (int)Math.Floor(SaveResult[Previous_TestNo].Misc);
                        if (SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt == 0)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                        else
                        {
                            rtnResult = ((SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt) * (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt + 1].dBAng.dB - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB)) + SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                    }
                    else
                    {
                        if (b_Interpolation)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].MagAng.Mag +
                                (PartialGradient *
                                (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point2].MagAng.Mag - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].MagAng.Mag));
                        }
                        else
                        {
                            i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].MagAng.Mag;
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }

            }
            public class cReal_At : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Real At Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public int ChannelNumber;
                public string SParameters;
                public string Interpolation;
                public double Frequency_At;
                public int Previous_TestNo;

                public double Offset;

                // Internal Variables
                private int SParam;
                private int SParam_Arr;
                private bool b_Interpolation;
                private double tmpCnt;
                private double TargetFreqCnt;
                private int Point1;
                private int Point2;
                private double PartialGradient;

                private double rtnResult;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    //MeasureResult();
                    //SetResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    double StepFreq;
                    //ChannelNumber--;

                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    b_Interpolation = General.CStr2Bool(Interpolation);

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[ChannelNumber - 1].TraceNumber[SParamDef.GetHashCode()];
                    //ChannelNumber = ChannelNumber - 1;
                    //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
                    if (Previous_TestNo < 0)
                    {
                        #region "Target Point"


                        Found = false;
                        for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].segm; seg++)
                        {
                            if (Frequency_At >= SegmentParam[ChannelNumber - 1].SegmentData[seg].Start && Frequency_At <= SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[ChannelNumber - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                TargetFreqCnt = ((Frequency_At - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) * (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                if (b_Interpolation == true)
                                {
                                    Point1 = (int)Math.Floor(TargetFreqCnt);    //Remove the Decimal Point
                                    Point2 = Point1 + 1;
                                    StepFreq = (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1);
                                    PartialGradient = (Frequency_At - (((Point1 - tmpCnt) * StepFreq) + SegmentParam[ChannelNumber - 1].SegmentData[seg].Start)) / StepFreq;
                                }
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Target Point for Test Number " + TestNo.ToString(), "Target Frequency = " + Frequency_At.ToString());
                            ErrorRaise = true;
                        }
                        #endregion        // Calculate for Start Point
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                    }
                    int i_TargetFreqCnt = 0;
                    if (Previous_TestNo > 0)
                    {
                        //i_TargetFreqCnt = SaveResult[Previous_TestNo].Misc;
                        //rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        i_TargetFreqCnt = (int)Math.Floor(SaveResult[Previous_TestNo].Misc);
                        if (SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt == 0)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                        else
                        {
                            rtnResult = ((SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt) * (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt + 1].dBAng.dB - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB)) + SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                    }
                    else
                    {
                        if (b_Interpolation)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].ReIm.Real +
                                (PartialGradient *
                                (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point2].ReIm.Real - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].ReIm.Real));
                        }
                        else
                        {
                            i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].ReIm.Real;
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }

            }
            public class cImag_At : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Imaginary At Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public int ChannelNumber;
                public string SParameters;
                public string Interpolation;
                public double Frequency_At;
                public int Previous_TestNo;

                public double Offset;

                // Internal Variables
                private int SParam;
                private int SParam_Arr;
                private bool b_Interpolation;
                private double tmpCnt;
                private double TargetFreqCnt;
                private int Point1;
                private int Point2;
                private double PartialGradient;

                private double rtnResult;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Multi_Results = new s_mRslt[2];
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[0].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[1].Result_Header = value.Replace(" ", "_") + "_Freq";
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    //MeasureResult();
                    //SetResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    double StepFreq;
                    //ChannelNumber--;

                    //SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].Enable = false;
                    //SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].b_MultiResult = true;
                    SaveResult[TestNo].Multi_Results[0].Enable = true;
                    SaveResult[TestNo].Multi_Results[1].Enable = true;                    
                   
                    b_Interpolation = General.CStr2Bool(Interpolation);

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[ChannelNumber - 1].TraceNumber[SParamDef.GetHashCode()];
                    //ChannelNumber = ChannelNumber - 1;
                    //SParam_Arr = TraceMatch[ChannelNumber - 1].TraceNumber[SParam];
                    if (Previous_TestNo < 0)
                    {
                        #region "Target Point"


                        Found = false;
                        for (int seg = 0; seg < SegmentParam[ChannelNumber - 1].segm; seg++)
                        {
                            if (Frequency_At >= SegmentParam[ChannelNumber - 1].SegmentData[seg].Start && Frequency_At <= SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop)
                            {
                                Found = true;
                            }
                            else
                            {
                                tmpCnt += SegmentParam[ChannelNumber - 1].SegmentData[seg].Points;
                            }
                            if (Found == true)
                            {
                                TargetFreqCnt = ((Frequency_At - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) * (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                                if (b_Interpolation == true)
                                {
                                    Point1 = (int)Math.Floor(TargetFreqCnt);    //Remove the Decimal Point
                                    Point2 = Point1 + 1;
                                    StepFreq = (SegmentParam[ChannelNumber - 1].SegmentData[seg].Stop - SegmentParam[ChannelNumber - 1].SegmentData[seg].Start) / (SegmentParam[ChannelNumber - 1].SegmentData[seg].Points - 1);
                                    PartialGradient = (Frequency_At - (((Point1 - tmpCnt) * StepFreq) + SegmentParam[ChannelNumber - 1].SegmentData[seg].Start)) / StepFreq;
                                }
                                break;
                            }
                        }
                        if (Found == false)
                        {
                            General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Target Point for Test Number " + TestNo.ToString(), "Target Frequency = " + Frequency_At.ToString());
                            ErrorRaise = true;
                        }
                        #endregion        // Calculate for Start Point
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                    }
                    int i_TargetFreqCnt = 0;
                    if (Previous_TestNo > 0)
                    {
                        //i_TargetFreqCnt = SaveResult[Previous_TestNo].Misc;
                        //rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        i_TargetFreqCnt = (int)Math.Floor(SaveResult[Previous_TestNo].Misc);
                        if (SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt == 0)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                        else
                        {
                            rtnResult = ((SaveResult[Previous_TestNo].Misc - i_TargetFreqCnt) * (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt + 1].dBAng.dB - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB)) + SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].dBAng.dB;
                        }
                    }
                    else
                    {
                        if (b_Interpolation)
                        {
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].ReIm.Imag +
                                (PartialGradient *
                                (SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point2].ReIm.Imag - SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[Point1].ReIm.Imag));
                        }
                        else
                        {
                            i_TargetFreqCnt = Convert.ToInt32(TargetFreqCnt);
                            rtnResult = SParamData[ChannelNumber - 1].sParam_Data[SParam].sParam[i_TargetFreqCnt].ReIm.Imag;
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }

            }

            public class cMag_Between : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Mag Between Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public string Search_MethodType;
                public string Interpolation;
                public int Channel_Number;
                public string SParameters;
                public string Non_Inverted;
                public string Freq_Log;

                public double Offset;

                // Internal Variables
                private e_SearchType SearchMethodType;
                private bool b_NonInvert;
                private bool b_Interpolation;
                private bool b_Interpolation_High;
                private bool b_Interpolation_Low;

                private int StartFreqCnt;
                private int StopFreqCnt;

                private int tmpCnt;
                private int tmpCnt2;
                private double tmpChk;
                private int SParam;

                private double rtnResult;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.
                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }
                public void InitSettings()
                {

                    double Divider;

                    ////ChannelNumber--;

                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    b_Interpolation = General.CStr2Bool(Interpolation);

                    SearchMethodType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_MethodType.ToUpper());

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                    Divider = 0.000000001;
                    ErrorRaise = false;
                    if (b_Interpolation == true)
                    {
                        b_Interpolation_High = true;
                        b_Interpolation_Low = true;
                    }
                    else
                    {
                        b_Interpolation_High = false;
                        b_Interpolation_Low = false;
                    }
                    #region "Start Point"

                    tmpCnt = 0;
                    tmpCnt2 = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                    {
                        if (StartFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StartFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                            Divider = (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1);
                            tmpCnt2 = seg;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            //StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
                            tmpChk = (StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) % Divider;
                            if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
                            {
                                //StartFreqCnt = seg + tmpCnt;
                                StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                                b_Interpolation_Low = false;
                            }
                            else
                            {
                                StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt + 1);
                            }
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Start Point for Test Number " + TestNo.ToString(), "Start Frequency = " + StartFreq.ToString());
                        ErrorRaise = true;
                    }
                    #endregion        // Calculate for Start Point
                    #region "End Point"

                    tmpCnt = 0;
                    tmpCnt2 = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
                    {
                        if (StopFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StopFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                            tmpCnt2 = seg;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            //StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam.SegmentData[seg].Start) / (SegmentParam.SegmentData[seg].Stop - SegmentParam.SegmentData[seg].Start)) + tmpCnt);
                            tmpChk = (StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) % Divider;
                            if (tmpChk == 0 || tmpChk.ToString("#.####") == Divider.ToString("#.####"))
                            {
                                //StopFreqCnt = seg + 1 + tmpCnt;
                                //StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number-1].SegmentData[seg].Points -1)) + tmpCnt + 1);
                                //KCC: Without + 1
                                StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                                b_Interpolation_High = false;
                            }
                            else
                            {
                                StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                            }
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Stop Point for Test Number " + TestNo.ToString(), "Stop Frequency = " + StopFreq.ToString());
                        ErrorRaise = true;
                    }
                    #endregion             // Calculate for Stop Point

                    if (Divider == 0)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Divider Value equal 0 for Test Number " + TestNo.ToString(),
                            "Start Frequency = " + StartFreq.ToString() + "\nStop Frequency = " + StopFreq.ToString());
                        ErrorRaise = true;
                    }

                    if (Non_Inverted.ToUpper() == "V")
                    {
                        b_NonInvert = true;
                    }
                    else
                    {
                        b_NonInvert = false;
                    }
                    //Channel_Number = Channel_Number - 1;
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                    }

                    bool b_PositiveValue;
                    double tmpResult;
                    double Rslt1, Rslt2;

                    if (!b_NonInvert)
                    {
                        if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[StartFreqCnt].dBAng.dB > 0)
                        {
                            b_PositiveValue = true;
                        }
                        else
                        {
                            b_PositiveValue = false;
                        }
                    }
                    else
                    {
                        b_PositiveValue = true;
                    }

                    tmpResult = SearchData(SearchMethodType, b_PositiveValue);

                    //Modified by KCC (FreqCnt -1)
                    Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt - 1);
                    Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt - 1);
                    //Rslt1 = SearchInterpolatedData(b_Interpolation_Low, StartFreq, StartFreqCnt);
                    //Rslt2 = SearchInterpolatedData(b_Interpolation_High, StopFreq, StopFreqCnt);

                    rtnResult = ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
                    //SaveResult[TestNo].Result_Data = ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
                }
                void SetResult()
                {
                    if (TestNo == 16)
                    {
                        double yy;

                        yy = 11;
                    }
                    if (rtnResult < -200)
                        rtnResult = -200;

                    SaveResult[TestNo].Result_Data = rtnResult;
                }

                // Additional Function Codes
                double SearchData(e_SearchType Search, bool PositiveValue)
                {
                    double tmpResult;
                    tmpResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[StartFreqCnt].dBAng.dB;
                    switch (Search)
                    {
                        case e_SearchType.MAX:
                            if (PositiveValue)
                            {
                                for (int i = StartFreqCnt; i <= StopFreqCnt; i++)
                                {
                                    if (tmpResult < SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB)
                                    {
                                        tmpResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = StartFreqCnt; i <= StopFreqCnt; i++)
                                {
                                    if (tmpResult > SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB)
                                    {
                                        tmpResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB;
                                    }
                                }
                            }
                            break;
                        case e_SearchType.MIN:
                            if (PositiveValue)
                            {
                                for (int i = StartFreqCnt; i <= StopFreqCnt; i++)
                                {
                                    if (tmpResult > SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB)
                                    {
                                        tmpResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = StartFreqCnt; i <= StopFreqCnt; i++)
                                {
                                    if (tmpResult < SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB)
                                    {
                                        tmpResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[i].dBAng.dB;
                                    }
                                }
                            }
                            break;
                    }
                    return (tmpResult);

                }
                double SearchInterpolatedData(bool Interpolation, double Frequency, int FreqCnt)
                {
                    double tmpData;
                    if (Interpolation == false)
                    {
                        tmpData = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[StartFreqCnt].dBAng.dB;
                    }
                    else
                    {
                        double t1 = (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt + 1].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB);
                        double t2 = (SParamData[Channel_Number - 1].Freq[FreqCnt + 1] - SParamData[Channel_Number - 1].Freq[FreqCnt]);
                        double t3 = (Frequency - SParamData[Channel_Number - 1].Freq[FreqCnt]);
                        double t4 = t1 / t2 * t3;
                        double t5 = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB;
                        double t6 = t4 + t5;
                        double tt = ((SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt + 1].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB) /
                                    (SParamData[Channel_Number - 1].Freq[FreqCnt + 1] - SParamData[Channel_Number - 1].Freq[FreqCnt]) *
                                    (Frequency - SParamData[Channel_Number - 1].Freq[FreqCnt])) + SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB;
                        tmpData = ((SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt + 1].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB) /
                                    (SParamData[Channel_Number - 1].Freq[FreqCnt + 1] - SParamData[Channel_Number - 1].Freq[FreqCnt]) *
                                    (Frequency - SParamData[Channel_Number - 1].Freq[FreqCnt])) + SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[FreqCnt].dBAng.dB;
                    }
                    return tmpData;
                }
                double ProcessData(double Rslt, double Rslt1, double Rslt2, e_SearchType Search, bool PositiveValue)
                {
                    double rtnRslt;
                    //rtnRslt = -999;
                    rtnRslt = Rslt;
                    switch (Search)
                    {
                        case e_SearchType.MAX:
                            if (PositiveValue)
                            {
                                if (Rslt < Rslt1)
                                {
                                    rtnRslt = Rslt1;
                                }
                                if (Rslt < Rslt2)
                                {
                                    rtnRslt = Rslt2;
                                }
                            }
                            else
                            {
                                if (Rslt > Rslt1)
                                {
                                    rtnRslt = Rslt1;
                                }
                                if (Rslt > Rslt2)
                                {
                                    rtnRslt = Rslt2;
                                }
                            }
                            break;
                        case e_SearchType.MIN:
                            if (PositiveValue)
                            {
                                if (Rslt > Rslt1)
                                {
                                    rtnRslt = Rslt1;
                                }
                                if (Rslt > Rslt2)
                                {
                                    rtnRslt = Rslt2;
                                }
                            }
                            else
                            {
                                if (Rslt < Rslt1)
                                {
                                    rtnRslt = Rslt1;
                                }
                                if (Rslt < Rslt2)
                                {
                                    rtnRslt = Rslt2;
                                }
                            }
                            break;
                    }
                    return (rtnRslt);
                }
            }

            public class cCPL_Between : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "CPL Between Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public string Search_MethodType;
                public string Interpolation;
                public int Channel_Number;
                public string SParameters1;
                public string SParameters2;

                public double Offset;

                // Internal Variables
                private cMag_Between MagBetween1;
                private cMag_Between MagBetween2;
                double rtnResult;
                //private bool RaiseError;    //Error Checking to prevent error during measure or run test.

                #endregion
                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    //ChannelNumber--;
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;

                    MagBetween1 = new cMag_Between();
                    MagBetween1.TestNo = TestNo;
                    MagBetween1.Channel_Number = Channel_Number;
                    MagBetween1.Interpolation = Interpolation;
                    MagBetween1.Search_MethodType = Search_MethodType;
                    MagBetween1.SParameters = SParameters1;
                    MagBetween1.StartFreq = StartFreq;
                    MagBetween1.StopFreq = StopFreq;

                    MagBetween2 = new cMag_Between();
                    MagBetween2.TestNo = TestNo;
                    MagBetween2.Channel_Number = Channel_Number;
                    MagBetween2.Interpolation = Interpolation;
                    MagBetween2.Search_MethodType = Search_MethodType;
                    MagBetween2.SParameters = SParameters2;
                    MagBetween2.StartFreq = StartFreq;
                    MagBetween2.StopFreq = StopFreq;

                    MagBetween1.InitSettings();
                    MagBetween2.InitSettings();
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    MagBetween1.RunTest();
                    MagBetween2.RunTest();
                    rtnResult = MagBetween1.parseResult - MagBetween2.parseResult + Offset;
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }

            public class cRipple_Between : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Ripple Between Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public int Channel_Number;
                public string SParameters;
                public bool b_Absolute;

                public double Offset;

                //CM Wong - For sampling ripple measurement
                public string Sampling_Mode;
                public double Sampling_Interval;
                public double Sampling_BW;

                // Internal Variables
                private int StartFreqCnt;
                private int StopFreqCnt;

                private int tmpCnt;
                private int tmpCnt2;
                private double tmpChk;
                private int SParam_Arr;

                private double StepFreq;
                private double PartialGradient_Start;
                private double PartialGradient_Stop;
                
                private double rtnResult;
                private double rtnResultFreq;

                bool Found;
                private bool ErrorRaise;    //Error Checking to prevent error during measure or run test.
                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        //SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results = new s_mRslt[2];
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[0].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[1].Result_Header = value.Replace(" ", "_") + "_Freq";
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }
                public void InitSettings()
                {

                    double Divider;
                    //ChannelNumber--;
                    //SaveResult[TestNo].Enable = true;
                    //SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Enable = false;
                    SaveResult[TestNo].b_MultiResult = true;
                    SaveResult[TestNo].Multi_Results[0].Enable = true;
                    SaveResult[TestNo].Multi_Results[1].Enable = true;
                    
                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam_Arr = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                    Divider = 0.000000001;
                    ErrorRaise = false;

                    #region "Start Point"

                    tmpCnt = 0;
                    tmpCnt2 = 0;
                    Found = false;


                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                    {
                        if (StartFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StartFreq < SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                            Divider = (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1);
                            tmpCnt2 = seg;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            tmpChk = ((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                            StartFreqCnt = (int)Math.Floor(tmpChk);    //Remove the Decimal Point
                            StepFreq = (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1);
                            PartialGradient_Start = (StartFreq - (((StartFreqCnt - tmpCnt) * StepFreq) + SegmentParam[Channel_Number - 1].SegmentData[seg].Start)) / StepFreq;
                            StartFreqCnt++;
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Start Point for Test Number " + TestNo.ToString(), "Start Frequency = " + StartFreq.ToString());
                        ErrorRaise = true;
                    }
                    #endregion        // Calculate for Start Point
                    #region "End Point"

                    tmpCnt = 0;
                    tmpCnt2 = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
                    {
                        if (StopFreq > SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StopFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                            tmpCnt2 = seg;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {

                            tmpChk = ((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                            StopFreqCnt = (int)Math.Floor(tmpChk);    //Remove the Decimal Point
                            StepFreq = (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1);
                            PartialGradient_Stop = (StopFreq - (((StopFreqCnt - tmpCnt) * StepFreq) + SegmentParam[Channel_Number - 1].SegmentData[seg].Start)) / StepFreq;
                            StopFreqCnt++;
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Stop Point for Test Number " + TestNo.ToString(), "Stop Frequency = " + StopFreq.ToString());
                        ErrorRaise = true;
                    }
                    #endregion             // Calculate for Stop Point

                    if (Divider == 0)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Divider Value equal 0 for Test Number " + TestNo.ToString(),
                            "Start Frequency = " + StartFreq.ToString() + "\nStop Frequency = " + StopFreq.ToString());
                        ErrorRaise = true;
                    }

                    //Channel_Number = Channel_Number - 1;
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    double Rslt_Max;
                    double Rslt_Min;
                    double tmpRslt;

                    //Sampling Mode Declaration
                    bool b_SamplingMode = false;
                    List<double> ripplelist = new List<double>();
                    List<double> fstartlist = new List<double>();
                    int iNoPoint = SParamData[Channel_Number - 1].NoPoints;
                    double fstart = StartFreq;
                    double fstop = fstart + Sampling_BW;    
                    double endFreq = StopFreq;
                    Dictionary <int, List<double>> dictFreq = new Dictionary<int, List<double>>();
                    List<double> list = new List<double>();
                    int StartFreqCnt_Sampling = 0;
                    int StopFreqCnt_Sampling = 0;
                    SortedList<double, double> subTrace = new SortedList<double, double>();
                    //double startFreq = 0;

                    if (ErrorRaise == true)
                    {
                        SaveResult[TestNo].Result_Data = -999;
                        return;
                    }

                    if (Sampling_Mode.ToUpper() == "V") 
                        b_SamplingMode = true;

                    if (b_SamplingMode)
                    {
                        #region Sampling Mode
                        for (int iArr = 0 ; iArr < SParamData[Channel_Number - 1].Freq.Count(); iArr++)
                        {
                            list.Add(SParamData[Channel_Number - 1].Freq[iArr]);
                        }

                        dictFreq.Add(Channel_Number - 1, list);

                        for (int ripx = 0; fstart <= endFreq - Sampling_BW; ripx++)
                        {
                            List<double> templist = new List<double>();

                            StartFreqCnt_Sampling = dictFreq[Channel_Number - 1].BinarySearch(fstart);
                            
                            if (StartFreqCnt_Sampling < 0)  // start frequency not found, must interpolate
                            {
                                StartFreqCnt_Sampling = ~StartFreqCnt_Sampling;   // index just after target freq
                                //subTrace[startFreq] = InterpolateLinear(dictFreq[Channel_Number - 1][StartFreqCnt_Sampling - 1], dictFreq[Channel_Number - 1][StartFreqCnt_Sampling],
                                //    SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt_Sampling - 1].dBAng.dB, SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt_Sampling].dBAng.dB, fstart);
                            }

                            StopFreqCnt_Sampling = dictFreq[Channel_Number - 1].BinarySearch(fstop);
                            if (StopFreqCnt_Sampling < 0)  // end frequency not found, must interpolate
                            {
                                StopFreqCnt_Sampling = ~StopFreqCnt_Sampling - 1;   // index just before target freq
                                //subTrace[fstop] = InterpolateLinear(dictFreq[Channel_Number - 1][StopFreqCnt_Sampling], dictFreq[Channel_Number - 1][StopFreqCnt_Sampling + 1], 
                                //    SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt_Sampling].dBAng.dB, SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt_Sampling + 1].dBAng.dB, fstop);
                            }

                            for (int iArr = StartFreqCnt_Sampling; iArr < StopFreqCnt_Sampling; iArr++)
                            {
                                templist.Add(SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[iArr].dBAng.dB);
                            }

                            double fstartval, fstopval;

                            fstartval = InterpolateLinear(dictFreq[Channel_Number - 1][StartFreqCnt_Sampling - 1], dictFreq[Channel_Number - 1][StopFreqCnt_Sampling - 1],
                                SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt_Sampling - 1].dBAng.dB, SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt_Sampling - 1].dBAng.dB, fstart);

                            fstopval = InterpolateLinear(dictFreq[Channel_Number - 1][StartFreqCnt_Sampling - 1], dictFreq[Channel_Number - 1][StopFreqCnt_Sampling - 1],
                                SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt_Sampling - 1].dBAng.dB, SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt_Sampling - 1].dBAng.dB, fstop);
                            

                            templist.Add(fstartval);
                            templist.Add(fstopval);

                            Rslt_Min = templist.Min();
                            Rslt_Max = templist.Max();
                           
                            ripplelist.Add(Rslt_Max - Rslt_Min);
                            fstartlist.Add(fstart + (Sampling_BW/2));

                            fstart = fstart + Sampling_Interval;
                            fstop = fstart + Sampling_BW;
                            
                        }
                        rtnResult = ripplelist.Max();
                        rtnResultFreq = fstartlist[ripplelist.IndexOf(rtnResult)];
                    }    
                    #endregion Sampling Mode
                    else
                    {
                        Rslt_Max = SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt].dBAng.dB +
                                    (PartialGradient_Start *
                                    (SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt + 1].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StartFreqCnt].dBAng.dB));

                        Rslt_Min = Rslt_Max;

                        tmpRslt = SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt].dBAng.dB +
                                    (PartialGradient_Stop *
                                    (SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt + 1].dBAng.dB - SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[StopFreqCnt].dBAng.dB));

                        MaxMinComparator(ref Rslt_Min, ref Rslt_Max, tmpRslt);

                        for (int iArr = StartFreqCnt; iArr < StopFreqCnt; iArr++)
                        {
                            tmpRslt = SParamData[Channel_Number - 1].sParam_Data[SParam_Arr].sParam[iArr].dBAng.dB;
                            MaxMinComparator(ref Rslt_Min, ref Rslt_Max, tmpRslt);
                        }

                        if (b_Absolute)
                        {
                            rtnResult = Math.Abs(Rslt_Max - Rslt_Min);
                        }
                        else
                        {
                            rtnResult = Rslt_Max - Rslt_Min;
                        }
                    }
                    //SaveResult[TestNo].Result_Data = ProcessData(tmpResult, Rslt1, Rslt2, SearchMethodType, b_PositiveValue) + Offset;
                }
                public void SetResult()
                {
                    //SaveResult[TestNo].Result_Data = rtnResult;
                    SaveResult[TestNo].Multi_Results[0].Result_Data = rtnResult;
                    SaveResult[TestNo].Multi_Results[1].Result_Data = rtnResultFreq * 1e-6;
                }

                // Additional Function Codes
                public void MaxMinComparator(ref double MinValue, ref double MaxValue, double parseValue)
                {
                    if (parseValue > MaxValue)
                    {
                        MaxValue = parseValue;
                    }
                    if (parseValue < MinValue)
                    {
                        MinValue = parseValue;
                    }
                }
                private double InterpolateLinear(double lowerX, double upperX, double lowerY, double upperY, double xTarget)
                {
                    try
                    {
                        return (((upperY - lowerY) * (xTarget - lowerX)) / (upperX - lowerX)) + lowerY;
                    }
                    catch (Exception e)
                    {
                        return -99999;
                    }
                }
            }
           
            public class cPhase_At : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Phase At";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string Interpolation;
                public int Channel_Number;
                public string SParameters;
                public double Target_Frequency;

                // Internal Variables
                private bool b_Interpolation;
                private bool Found;
                private int tmpCnt;
                private int TargetFreqCnt;
                private int SParam;
                private double Phase1;
                private double Phase2;
                private bool ErrorRaise;

                private double rtnResult;


                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    //ChannelNumber--;
                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                    #region "Target Point"
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                    {
                        if (Target_Frequency >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && Target_Frequency <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            TargetFreqCnt = (int)Math.Floor((Target_Frequency - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt;
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Target Point for Test Number " + TestNo.ToString(), "Target Frequency = " + Target_Frequency.ToString());
                        ErrorRaise = true;
                    }
                    #endregion        // Calculate for Start Point

                    b_Interpolation = General.CStr2Bool(Interpolation);
                    SaveResult[TestNo].Enable = true;
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    if (b_Interpolation)
                    {
                        Phase1 = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[TargetFreqCnt].dBAng.Angle;
                        Phase2 = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[TargetFreqCnt + 1].dBAng.Angle;
                        if (((Phase1 + Phase2) / 2) > 180)
                        {
                            rtnResult = ((Phase1 + Phase2) / 2) - 360;
                        }
                        else
                        {
                            rtnResult = ((Phase1 + Phase2) / 2);
                        }
                    }
                    else
                    {
                        if (SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[TargetFreqCnt].dBAng.Angle > 180)
                        {
                            rtnResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[TargetFreqCnt].dBAng.Angle - 360;
                        }
                        else
                        {
                            rtnResult = SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[TargetFreqCnt].dBAng.Angle;
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }

            public class cBandwidth : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Bandwidth";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public string Search_DirectionMethod_1;
                public string Search_DirectionMethod_2;
                public string Search_Type;

                public string Interpolation;
                public int Channel_Number;
                public string SParameters;
                public string Search_Value_1;
                public string Search_Value_2;
                public bool b_Invert_Search;

                // Internal Variables
                private cFreq_At Freq_At_1;
                private cFreq_At Freq_At_2;

                private double rtnResult;
                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    //ChannelNumber--;
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;

                    Freq_At_1 = new cFreq_At();
                    Freq_At_2 = new cFreq_At();

                    Freq_At_1.TestNo = TestNo;
                    Freq_At_1.StartFreq = StartFreq;
                    Freq_At_1.StopFreq = StopFreq;
                    Freq_At_1.Channel_Number = Channel_Number;
                    Freq_At_1.SParameters = SParameters;
                    Freq_At_1.Search_DirectionMethod = Search_DirectionMethod_1;
                    Freq_At_1.Search_Type = Search_Type;
                    Freq_At_1.Search_Value = Search_Value_1;
                    Freq_At_1.Interpolation = Interpolation;
                    Freq_At_1.b_Invert_Search = b_Invert_Search;

                    Freq_At_2.TestNo = TestNo;
                    Freq_At_2.StartFreq = StartFreq;
                    Freq_At_2.StopFreq = StopFreq;
                    Freq_At_2.Channel_Number = Channel_Number;
                    Freq_At_2.SParameters = SParameters;
                    Freq_At_2.Search_DirectionMethod = Search_DirectionMethod_2;
                    Freq_At_2.Search_Type = Search_Type;
                    Freq_At_2.Search_Value = Search_Value_2;
                    Freq_At_2.Interpolation = Interpolation;
                    Freq_At_2.b_Invert_Search = b_Invert_Search;

                    Freq_At_1.InitSettings();
                    Freq_At_2.InitSettings();

                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    Freq_At_1.MeasureResult();
                    Freq_At_2.MeasureResult();

                    rtnResult = Freq_At_1.parseResult - Freq_At_2.parseResult;
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }

            public class cBalance : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Balance";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public double StartFreq;
                public double StopFreq;
                public string Search_Type;
                public int Channel_Number;
                public string SParameters_1;
                public string SParameters_2;
                public string BalanceType;
                public bool b_Absolute;

                // Internal Variables
                private e_SearchType SearchType;
                private int StartFreqCnt;
                private int StopFreqCnt;
                private int tmpCnt;
                private int SParam_1;
                private int SParam_2;
                private e_BalanceType eBalanceType;

                private double rtnResult;

                private bool Found;

                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    //ChannelNumber--;
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters_1.ToUpper());
                    SParam_1 = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];
                    SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters_2.ToUpper());
                    SParam_2 = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                    SearchType = (e_SearchType)Enum.Parse(typeof(e_SearchType), Search_Type.ToUpper());

                    #region "Start Point"

                    tmpCnt = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                    {
                        if (StartFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StartFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Start Point", "Start Frequency = " + StartFreq.ToString());
                    }
                    #endregion        // Calculate for Start Point
                    #region "End Point"

                    tmpCnt = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
                    {
                        if (StopFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StopFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Stop Point", "Stop Frequency = " + StopFreq.ToString());
                    }
                    #endregion             // Calculate for Stop Point

                    eBalanceType = (e_BalanceType)Enum.Parse(typeof(e_BalanceType), BalanceType.ToUpper());
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    int NoPoints = SParamData[Channel_Number - 1].NoPoints;
                    double tmpVal = 0;
                    s_DataType tmp_Data_1;
                    s_DataType tmp_Data_2;

                    if (eBalanceType == e_BalanceType.CMRR)
                    {
                        #region "CMRR"
                        if (!SBalanceParamData[Channel_Number - 1].CMRR_Enable)
                        {
                            SBalanceParamData[Channel_Number - 1].CMRR.sParam = new s_DataType[NoPoints];
                        }
                        for (int iPts = 0; iPts < NoPoints; iPts++)
                        {
                            tmp_Data_1.ReIm = Math_Func.Complex_Number.Minus(SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].ReIm, SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].ReIm);
                            tmp_Data_2.ReIm = Math_Func.Complex_Number.Sum(SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].ReIm, SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].ReIm);
                            tmp_Data_1.dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(tmp_Data_1.ReIm);
                            tmp_Data_2.dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(tmp_Data_2.ReIm);
                            SBalanceParamData[Channel_Number - 1].CMRR.sParam[iPts].dBAng.dB = tmp_Data_1.dBAng.dB - tmp_Data_2.dBAng.dB;
                            SBalanceParamData[Channel_Number - 1].CMRR.sParam[iPts].dBAng.Angle = 0;
                        }
                        SBalanceParamData[Channel_Number - 1].CMRR_Enable = true;
                        #endregion
                    }
                    else if (eBalanceType == e_BalanceType.AMPLITUDE || eBalanceType == e_BalanceType.PHASE)
                    {
                        #region "Amplitude & Phase"
                        if (!SBalanceParamData[Channel_Number - 1].Balance_Enable)
                        {
                            SBalanceParamData[Channel_Number - 1].Balance.sParam = new s_DataType[NoPoints];
                        }
                        for (int iPts = 0; iPts < NoPoints; iPts++)
                        {
                            if (b_Absolute)
                            {
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.dB = Math.Abs(SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].dBAng.dB
                                                                                                - SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].dBAng.dB);
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = Math.Abs(SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].dBAng.Angle
                                                                                                - SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].dBAng.Angle);

                                //KCC - Phase
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle + 180;
                                if (SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle >= 180)
                                {
                                    SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle - 360;
                                }
                            }
                            else
                            {
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.dB = (SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].dBAng.dB
                                                                                                - SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].dBAng.dB);
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = (SParamData[Channel_Number - 1].sParam_Data[SParam_1].sParam[iPts].dBAng.Angle
                                                                                                - SParamData[Channel_Number - 1].sParam_Data[SParam_2].sParam[iPts].dBAng.Angle);

                                //KCC - Phase
                                SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle + 180;
                                if (SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle >= 180)
                                {
                                    SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle = SBalanceParamData[Channel_Number - 1].Balance.sParam[iPts].dBAng.Angle - 360;
                                }
                            }
                        }
                        SBalanceParamData[Channel_Number - 1].Balance_Enable = true;
                        #endregion
                    }

                    if (eBalanceType == e_BalanceType.CMRR)
                    {
                        if (SearchType == e_SearchType.MAX)
                        {
                            tmpVal = -999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal < SBalanceParamData[Channel_Number - 1].CMRR.sParam[iCnt].dBAng.dB)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].CMRR.sParam[iCnt].dBAng.dB;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                        else if (SearchType == e_SearchType.MIN)
                        {
                            tmpVal = 999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal > SBalanceParamData[Channel_Number - 1].CMRR.sParam[iCnt].dBAng.dB)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].CMRR.sParam[iCnt].dBAng.dB;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                    }
                    else if (eBalanceType == e_BalanceType.AMPLITUDE)
                    {
                        if (SearchType == e_SearchType.MAX)
                        {
                            tmpVal = -999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal < SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.dB)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.dB;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                        else if (SearchType == e_SearchType.MIN)
                        {
                            tmpVal = 999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal > SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.dB)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.dB;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                    }
                    else if (eBalanceType == e_BalanceType.PHASE)
                    {
                        if (SearchType == e_SearchType.MAX)
                        {
                            tmpVal = -999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal < SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.Angle)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.Angle;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                        else if (SearchType == e_SearchType.MIN)
                        {
                            tmpVal = 999999;
                            for (int iCnt = StartFreqCnt; iCnt <= StopFreqCnt; iCnt++)
                            {
                                if (tmpVal > SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.Angle)
                                {
                                    tmpVal = SBalanceParamData[Channel_Number - 1].Balance.sParam[iCnt].dBAng.Angle;
                                    tmpCnt = iCnt;
                                }
                            }
                        }
                    }
                    rtnResult = tmpVal;
                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                    SaveResult[TestNo].Misc = tmpCnt;
                }
            }

            public class cChannel_Averaging : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Channel_Averaging";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public int SiteNumber;
                public bool b_DualSite = false;

                public double StartFreq;
                public double StopFreq;
                public int Channel_Number;
                public string SParameters;
                public double Offset;

                // Internal Variables
                private int StartFreqCnt;
                private int StopFreqCnt;
                private double tmpCnt;
                private int SParam;

                private double rtnResult;
                private double rtnResultFreq;

                private bool Found;

                #endregion

                public double parseResult
                {
                    get
                    {

                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Multi_Results = new s_mRslt[2];
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[0].Result_Header = value.Replace(" ", "_");
                        SaveResult[TestNo].Multi_Results[1].Result_Header = value.Replace(" ", "_") + "_Freq";
  
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    //To show the result data on screen
                    //SaveResult[TestNo].Enable = true;
                    //SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Enable = false;
                    SaveResult[TestNo].b_MultiResult = true;
                    SaveResult[TestNo].Multi_Results[0].Enable = true;
                    SaveResult[TestNo].Multi_Results[1].Enable = true;

                    #region "Start Point"
                    tmpCnt = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].segm; seg++)
                    {
                        if (StartFreq >= SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StartFreq < SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            StartFreqCnt = Convert.ToInt32(((StartFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Start Point", "Start Frequency = " + StartFreq.ToString());
                    }
                    #endregion        // Calculate for Start Point

                    #region "End Point"

                    tmpCnt = 0;
                    Found = false;
                    for (int seg = 0; seg < SegmentParam[Channel_Number - 1].SegmentData.Length; seg++)
                    {
                        if (StopFreq > SegmentParam[Channel_Number - 1].SegmentData[seg].Start && StopFreq <= SegmentParam[Channel_Number - 1].SegmentData[seg].Stop)
                        {
                            Found = true;
                        }
                        else
                        {
                            tmpCnt += SegmentParam[Channel_Number - 1].SegmentData[seg].Points;
                        }
                        if (Found == true)
                        {
                            StopFreqCnt = Convert.ToInt32(((StopFreq - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) / (SegmentParam[Channel_Number - 1].SegmentData[seg].Stop - SegmentParam[Channel_Number - 1].SegmentData[seg].Start) * (SegmentParam[Channel_Number - 1].SegmentData[seg].Points - 1)) + tmpCnt);
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        General.DisplayError(ClassName + " -> " + SubClassName, "Unable to find Stop Point", "Stop Frequency = " + StopFreq.ToString());
                    }
                    #endregion             // Calculate for Stop Point

                    e_SParametersDef SParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), SParameters.ToUpper());
                    SParam = TraceMatch[Channel_Number - 1].TraceNumber[SParamDef.GetHashCode()];

                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();

                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    s_DataType Mag_SParam;

                    double total_magnitude;
                    int noofpoints;
                    double mag_average;
                    List<double> freqList = new List<double>();

                    total_magnitude = 0;
                    noofpoints = 0;
                    mag_average = 0;
                
                    //Calculate the total number of point - to be use for averaging
                    noofpoints = (StopFreqCnt - StartFreqCnt) + 1;

                    //Start adding the magnitude base on the freq range specified earlier
                    for (int iArr = StartFreqCnt; iArr <= StopFreqCnt; iArr++)
                    {
                        Mag_SParam.MagAng = Math_Func.Conversion.conv_RealImag_to_MagAngle(SParamData[Channel_Number - 1].sParam_Data[SParam].sParam[iArr].ReIm);
                        total_magnitude = total_magnitude + Mag_SParam.MagAng.Mag;
                        freqList.Add(SParamData[Channel_Number - 1].Freq[iArr]);    //Follow SJ setup
                    }

                    //Calculate the channel averaging (in mag) then convert to dB
                    mag_average = total_magnitude / noofpoints;
                
                    rtnResult = Math_Func.Conversion.conv_Mag_to_dB(mag_average);
                    rtnResultFreq = freqList.Max();     //Follow SJ setup
                }
                void SetResult()
                {
                    //SaveResult[TestNo].Result_Data = rtnResult;
                    SaveResult[TestNo].Multi_Results[0].Result_Data = rtnResult;
                    SaveResult[TestNo].Multi_Results[1].Result_Data = rtnResultFreq * 1e-6;
                }
            }

            public class cTestExample : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Test Example Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                // Internal Variables
                private double rtnResult;

                #endregion

                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    cENA ENA_Driver = new cENA(); // (Example: Enable Driver)
                    FormattedIO488 ENA_IO = new FormattedIO488();   // (Example:  ENA IO )
                    ENA_Driver.Init(ENA_IO);    // (Example:  to initiate all classes in ENA_Driver

                    ENA_Driver.Sense.Frequency.Center(1000000000);
                    ENA_Driver.Sense.Frequency.Center("1.2 GHz");
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();

                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {


                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }

            static void SaveFile2SNP(string FolderName, string FileName, string Unit, int iChn)
            {
                string[] OutputData;
                string OutputFileName;
                string tmpStr;
                e_SParametersDef tmpSParamDef;
                int DataInfo;

                OutputFileName = FolderName + FileName;

                OutputData = new string[SParamData[iChn - 1].Freq.Length + 3];
                OutputData[0] = "#\tHZ\tS\tMagPhase\tR50.0";
                OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();
                switch (SParamData[iChn - 1].NoPorts)
                {
                    case 2:
                        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S21\t\t" + "S22\t\t";
                        break;
                    case 3:
                        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t";
                        break;
                    case 4:
                        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S14\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S24\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t" + "S34\t\t" + "S41\t\t" + "S42\t\t" + "S43\t\t" + "S44\t\t";
                        break;
                }
                for (int iPts = 0; iPts < SParamData[iChn - 1].Freq.Length; iPts++)
                {
                    OutputData[iPts + 3] = SParamData[iChn - 1].Freq[iPts].ToString();
                    for (int X = 1; X < (SParamData[iChn - 1].NoPorts + 1); X++)
                    {
                        for (int Y = 1; Y < (SParamData[iChn - 1].NoPorts + 1); Y++)
                        {
                            tmpStr = "S" + X.ToString() + Y.ToString();
                            tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                            DataInfo = TraceMatch[iChn - 1].TraceNumber[tmpSParamDef.GetHashCode()];

                            if (DataTriggered[DataTrigger_i].SParam_Grab[tmpSParamDef.GetHashCode()])
                            {
                                OutputData[iPts + 3] += "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.dB
                                                        + "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.Angle;
                            }
                            else
                            {
                                OutputData[iPts + 3] += "\t0\t0";
                            }
                        }
                    }
                }
                System.IO.File.WriteAllLines(OutputFileName + "_Channel" + (iChn + 1).ToString() + "_Unit" + Unit + ".s" + SParamData[iChn - 1].NoPorts.ToString() + "p", OutputData);
            }

            //KCC: Added d port
            static void SaveFile2SNP(string FolderName, string FileName, string Mode, string Unit, int iChn)
            {
                string[] OutputData;
                string OutputFileName;
                string tmpStr;
                e_SParametersDef tmpSParamDef;
                int DataInfo;

                OutputFileName = FolderName + FileName;
                OutputData = new string[SParamData[iChn - 1].Freq.Length + 3];
                OutputData[0] = "#\tHZ\tS\tMagPhase\tR50.0";
                OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();

                //KCC - Standardize format for SDI
                //switch (SParamData[iChn - 1].NoPorts)
                //{
                //    case 2:
                //        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S21\t\t" + "S22\t\t";
                //        break;
                //    case 3:
                        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t";
                //        break;
                //    case 4:
                //        OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S14\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S24\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t" + "S34\t\t" + "S41\t\t" + "S42\t\t" + "S43\t\t" + "S44\t\t"
                //            + "SDS32\t\t" + "SDD33\t\t" + "SDS31\t\t" + "SCS31\t\t" + "SDD11\t\t" + "SDD22\t\t" + "SDD44\t\t" + "SDS12\t\t";
                //        break;
                //}


                //OutputData[2] = "Freq\t" + "S11\t\t" + "S12\t\t" + "S13\t\t" + "S14\t\t" + "S21\t\t" + "S22\t\t" + "S23\t\t" + "S24\t\t" + "S31\t\t" + "S32\t\t" + "S33\t\t" + "S34\t\t" + "S41\t\t" + "S42\t\t" + "S43\t\t" + "S44\t\t"
                //           + "SDS32\t\t" + "SDD33\t\t" + "SDS31\t\t" + "SCS31\t\t" + "SDD11\t\t" + "SDD22\t\t" + "SDD44\t\t" + "SDS12\t\t";

                for (int iPts = 0; iPts < SParamData[iChn - 1].Freq.Length; iPts++)
                {
                    //KCC - Round decimal place
                    OutputData[iPts + 3] = (Math.Round(SParamData[iChn - 1].Freq[iPts])).ToString();

                    //KCC - Standardize format for SDI
                    //for (int X = 1; X < (SParamData[iChn - 1].NoPorts + 1); X++)                        
                    for (int X = 1; X < 4; X++)
                    {
                        //KCC - Standardize format for SDI
                        //for (int Y = 1; Y < (SParamData[iChn - 1].NoPorts + 1); Y++)
                        for (int Y = 1; Y < 4; Y++)
                        {
                            tmpStr = "S" + X.ToString() + Y.ToString();
                            tmpSParamDef = (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), tmpStr);
                            DataInfo = TraceMatch[iChn - 1].TraceNumber[tmpSParamDef.GetHashCode()];

                            if (DataTriggered[DataTrigger_i].SParam_Grab[tmpSParamDef.GetHashCode()])
                            {
                                OutputData[iPts + 3] += "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.dB
                                                        + "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.Angle;
                            }
                            else
                            {
                                OutputData[iPts + 3] += "\t0\t0";
                            }

                            //KCC - Differential port SDD, SDS, SCS...
                            if (tmpStr == "S44")
                            {
                                for (int Z = 1; Z < 9; Z++)
                                {
                                    DataInfo = TraceMatch[iChn - 1].TraceNumber[15 + Z];
                                    if (DataInfo != -1)
                                     {
                                        OutputData[iPts + 3] += "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.dB
                                                                + "\t" + SParamData[iChn - 1].sParam_Data[DataInfo].sParam[iPts].dBAng.Angle;
                                    }
                                    else
                                    {
                                        OutputData[iPts + 3] += "\t0\t0";
                                    }
                                }
                            }
                        }
                    }
                }
                ////KCC- For SDI format
                //string PName = "";
                //if (SParamData[iChn - 1].NoPorts == 3)
                //{
                //    PName = "p";
                //}
                //else if (SParamData[iChn - 1].NoPorts == 4)
                //{
                //    PName = "pd";
                //}
                //System.IO.File.WriteAllLines(OutputFileName + "_Channel" + (iChn + 1).ToString() + "_" + Mode + "_Unit" + Unit + ".s" + SParamData[iChn - 1].NoPorts.ToString() + "p", OutputData);

                //KCC - Standardize format for SDI
                System.IO.File.WriteAllLines(OutputFileName + "_CHAN" + (iChn).ToString() + "_" + Mode + "_Unit" + Unit + ".s4pd", OutputData);
            }
            static void TransferEnhanceDataZNB(double[] InputData, int ChannelNumber)
            {
                //string[] TraceArr = TraceNumber.Split(',');
                //int TraceCount = TraceArr.Length;
                int Points;
                int Offset = 0;
                int BasePoint = SParamData[ChannelNumber].Freq.Length;
                string ZnbTraceParam;
                string[] allTraces, traces;
                int[] traceMap;

                #region Sorting Traces into correct order

                ZnbTraceParam = ENA.Calculate.Par.GetTraceCategory(ChannelNumber + 1);
                ZnbTraceParam = ZnbTraceParam.Replace("'", "").Replace("\n", "").Trim();
                allTraces = ZnbTraceParam.Split(new char[] { ',' });
                //Get only odd number
                traces = allTraces.Where((item, index) => index % 2 != 0).ToArray();
                traceMap = new int[traces.Length];

                for (int i = 0; i < traces.Length; i++)
                {
                    traceMap[i] = i;
                    //traceMap[i] = TraceMatch[ChannelNumber].TraceNumber[((e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), traces[i])).GetHashCode()];
                }
                #endregion

                for (int iTrace = 0; iTrace < traces.Length; iTrace++)
                {

                    int SParamDef = traceMap[iTrace];
                    if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.GDEL)
                    {
                        Points = BasePoint;
                    }
                    else
                    {
                        Points = BasePoint * 2;
                    }
                    dB_Angle tmp_dBAng = new dB_Angle();

                    Offset = traceMap[iTrace] * Points;

                    SParamData[ChannelNumber].sParam_Data[SParamDef].sParam = new s_DataType[Points];
                    for (int iPts = 0; iPts < BasePoint; iPts++)
                    {
                        if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SCOM ||
                            SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SMIT)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Real = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Imag = InputData[Offset + (iPts * 2) + 1];
                            tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm);
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng = tmp_dBAng;
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.MLOG)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = InputData[Offset + (iPts * 2) + 1];
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.GDEL)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[(2 * Offset) + iPts];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = 0;
                        }
                    }

                }

            }

            static void TransferEnhanceData1(double[] InputData, int ChannelNumber, string TraceNumber)
            {
                string[] TraceArr = TraceNumber.Split(',');
                int TraceCount = TraceArr.Length;
                int Points;
                int Offset = 0;
                int BasePoint = SParamData[ChannelNumber].Freq.Length;

                for (int iTrace = 0; iTrace < TraceCount; iTrace++)
                {
                    int SParamDef = int.Parse(TraceArr[iTrace]);
                    if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.GDEL)
                    {
                        Points = BasePoint;
                    }
                    else
                    {
                        Points = BasePoint * 2;
                    }
                    dB_Angle tmp_dBAng = new dB_Angle();

                    SParamData[ChannelNumber].sParam_Data[SParamDef].sParam = new s_DataType[Points];
                    for (int iPts = 0; iPts < BasePoint; iPts++)
                    {
                        if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SCOM ||
                            SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SMIT)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Real = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Imag = InputData[Offset + (iPts * 2) + 1];
                            tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm);
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng = tmp_dBAng;
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[int.Parse(TraceArr[iTrace])].Format == e_SFormat.MLOG)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = InputData[Offset + (iPts * 2) + 1];
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[int.Parse(TraceArr[iTrace])].Format == e_SFormat.GDEL)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[Offset + iPts];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = 0;
                        }
                    }
                    Offset += Points;
                }
                //if (b_ZnbInUse)
                //    znbDataOffset = Offset;
            }
            static void TransferEnhanceData(double[] InputData, int ChannelNumber, string TraceNumber)
            {
                string[] TraceArr = TraceNumber.Split(',');
                int TraceCount = TraceArr.Length;
                int Points;
                for (int iTrace = 0; iTrace < TraceCount; iTrace++)
                {
                    int Offset;
                    int SParamDef = int.Parse(TraceArr[iTrace]);
                    if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format != e_SFormat.GDEL)
                    {
                        Points = InputData.Length / 2 / TraceCount;
                        Offset = iTrace * 2 * Points;
                    }
                    else
                    {
                        Points = InputData.Length / TraceCount;
                        Offset = iTrace * Points;
                    }
                    dB_Angle tmp_dBAng = new dB_Angle();
                    SParamData[ChannelNumber].sParam_Data[SParamDef].sParam = new s_DataType[Points];
                    for (int iPts = 0; iPts < Points; iPts++)
                    {
                        if (SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SCOM ||
                            SParamData[ChannelNumber].sParam_Data[SParamDef].Format == e_SFormat.SMIT)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Real = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm.Imag = InputData[Offset + (iPts * 2) + 1];
                            tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].ReIm);
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng = tmp_dBAng;
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[int.Parse(TraceArr[iTrace])].Format == e_SFormat.MLOG)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[Offset + (iPts * 2)];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = InputData[Offset + (iPts * 2) + 1];
                        }
                        else if (SParamData[ChannelNumber].sParam_Data[int.Parse(TraceArr[iTrace])].Format == e_SFormat.GDEL)
                        {
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.dB = InputData[Offset + iPts];
                            SParamData[ChannelNumber].sParam_Data[SParamDef].sParam[iPts].dBAng.Angle = 0;
                        }
                    }

                    //string aaa =Convert.ToString ( SParamData[ChannelNumber].sParam_Data[int.Parse(TraceArr[iTrace])].Format);

                }

            }
            static void TransferData(double[] InputData, int ChannelNumber, int SParam_Def)
            {
                int Points;

                Points = InputData.Length / 2;

                SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam = new s_DataType[Points];

                for (int iPts = 0; iPts < Points; iPts++)
                {
                    SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.dB = InputData[iPts * 2];
                    SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.Angle = InputData[(iPts * 2) + 1];
                }
            }
            static void TransferData(double[] InputData, int ChannelNumber, int SParam_Def, e_SFormat Format)
            {
                int Points;
                dB_Angle tmp_dBAng = new dB_Angle();
                //KCC - Added Lin Mag
                Mag_Angle tmp_MagAng = new Mag_Angle();
                if (Format != e_SFormat.GDEL)
                Points = InputData.Length / 2;
                else
                    Points = InputData.Length;

                SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam = new s_DataType[Points];
                SParamData[ChannelNumber].sParam_Data[SParam_Def].Format = Format;

                for (int iPts = 0; iPts < Points; iPts++)
                {
                    if (Format == e_SFormat.SCOM || Format == e_SFormat.SMIT)
                    {
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].ReIm.Real = InputData[iPts * 2];
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].ReIm.Imag = InputData[(iPts * 2) + 1];
                        tmp_dBAng = Math_Func.Conversion.conv_RealImag_to_dBAngle(SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].ReIm);
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng = tmp_dBAng;

                        //KCC - Lin Mag
                        tmp_MagAng = Math_Func.Conversion.conv_RealImag_to_MagAngle(SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].ReIm);
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].MagAng = tmp_MagAng;
                    }
                    else if (Format == e_SFormat.MLOG)
                    {
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.dB = InputData[iPts * 2];
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.Angle = InputData[(iPts * 2) + 1];
                    }
                    else if (Format == e_SFormat.GDEL)
                    {
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.dB = InputData[iPts];
                        SParamData[ChannelNumber].sParam_Data[SParam_Def].sParam[iPts].dBAng.Angle = 0;
                    }
                }
            }
            static int SParam_Def2Value(int SParamDef)
            {
                int rtnRslt;
                rtnRslt = 0;
                switch (SParamDef)
                {
                    case 1:
                        rtnRslt = e_SParametersDef.S11.GetHashCode();
                        break;
                    case 2:
                        rtnRslt = e_SParametersDef.S22.GetHashCode();
                        break;
                    case 3:
                        rtnRslt = e_SParametersDef.S12.GetHashCode();
                        break;
                    case 4:
                        rtnRslt = e_SParametersDef.S21.GetHashCode();
                        break;
                    case 5:
                        rtnRslt = e_SParametersDef.S33.GetHashCode();
                        break;
                    case 6:
                        rtnRslt = e_SParametersDef.S13.GetHashCode();
                        break;
                    case 7:
                        rtnRslt = e_SParametersDef.S23.GetHashCode();
                        break;
                    case 8:
                        rtnRslt = e_SParametersDef.S31.GetHashCode();
                        break;
                    case 9:
                        rtnRslt = e_SParametersDef.S32.GetHashCode();
                        break;
                    case 10:
                        rtnRslt = e_SParametersDef.S44.GetHashCode();
                        break;
                    case 11:
                        rtnRslt = e_SParametersDef.S14.GetHashCode();
                        break;
                    case 12:
                        rtnRslt = e_SParametersDef.S24.GetHashCode();
                        break;
                    case 13:
                        rtnRslt = e_SParametersDef.S34.GetHashCode();
                        break;
                    case 14:
                        rtnRslt = e_SParametersDef.S41.GetHashCode();
                        break;
                    case 15:
                        rtnRslt = e_SParametersDef.S42.GetHashCode();
                        break;
                    case 16:
                        rtnRslt = e_SParametersDef.S43.GetHashCode();
                        break;
                }
                return (rtnRslt);
            }
            static int SParam_Def2Value(int SParamDef, int Channel_Number)
            {
                //int rtnRslt;
                //rtnRslt = 0;
                //for (int iTrace = 0; iTrace < TraceMatch[Channel_Number].TraceNumber.Length; iTrace++)
                //{
                //    if (TraceMatch[Channel_Number].TraceNumber[iTrace] == SParamDef)
                //    {
                //        rtnRslt = iTrace;
                //        break;
                //    }
                //}
                //return (rtnRslt);
                return (TraceMatch[Channel_Number].SParam_Def_Number[SParamDef]);
            }
        }

        public class cCalibrationClasses : ClothoLibStandard.Lib_Var
        {
            //public EquipmentDrivers.cRasco Rasco = new cRasco();
            //public EquipmentDrivers.Handler.cHandler_Common Handler = new EquipmentDrivers.Handler.cHandler_Common();
            public static string SubClass = "Calibration Class";

            public struct s_CalibrationTotalPort
            {
                public int No_Ports;
                public int PortNo_1;
                public int PortNo_2;
                public int PortNo_3;
                public int PortNo_4;
            }
            private s_CalibrationTotalPort[] Cal_TotalPort;
            public s_CalibrationTotalPort[] parse_CalTotalPort
            {
                set
                {
                    Cal_TotalPort = value;
                }
            }
            public struct s_CalibrationProcedure
            {
                public e_CalibrationType CalType;
                public int ChannelNumber;
                public int CKit_LocNum;
                public string CKit_Label;
                public int No_Ports;
                public int Port_1;
                public int Port_2;
                public int Port_3;
                public int Port_4;
                public int CalKit;
                public bool b_CalKit;
                public string Message;
                public int Sleep;
                public bool Move;
                public int Move_Step;
                public string Switch;
            }
            public bool b_Mode;
            public bool CalKit_FailCheck;
            public int[] iPortMethod = new int[TotalChannel];
            public s_CalibrationProcedure[] Cal_Prod;
            //public int[] iPortMethod = new int[TotalChannel];

            public s_CalibrationProcedure[] parse_Procedure
            {
                set
                {
                    Cal_Prod = value;
                }
            }
                        
            public void Calibrate(InstrLib.HandlerS1 yada)
            {
                try
                {
                string tmpStr;
                string handStr;
                bool bNext;
                bool bDebug = false; //Set calibration debug on each steps
                int ChannelNo = 0;
                    string tmplabel;
                    int tmpChannelNo = 0;
                    CalKit_FailCheck = false;
                    tmplabel = "";

                    e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                    bool[] AnalysisEnable = new bool[TotalChannel];
                    bool[] PortExtEnable = new bool[TotalChannel];

                    if (b_Mode) ENA.Display.Update(false);  // Turn Off the ENA when is Auto Mode

                    //Reset ZNB
                    if (b_ZnbInUse)
                    {
                        ENA.BasicCommand.System.Reset();
                        if (StateFile != "") ENA.Memory.Load.State(StateFile);
                        ENA.Format.Border(e_Format.NORM);
                        ENA.BasicCommand.SendCommand("CORR:COLL:AVER MAN"); //Disable Auto Averaging to prevent Query Interrupted
                        Thread.Sleep(3000);
                    }

                    //Check CalKit - Ensure StateFile preloaded with correct cal coefficient
                    for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                    {
                        if (tmpChannelNo != Cal_Prod[iCal].ChannelNumber)
                        {

                            ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CKit_Label);

                            //Only verify the 1st OPEN cal statement only. Must ensure that your Cal Kit Label define correctly for this row
                            if (e_CalibrationType.OPEN == Cal_Prod[iCal].CalType)
                            {
                                if (Cal_Prod[iCal].CKit_LocNum != 0) //Only check if user define the cal kit location number, undefine will assume no check required
                                {
                                    char[] trimChar = { '\"', '\n' };
                                    tmplabel = ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber);
                                    tmplabel = tmplabel.Trim(trimChar);
                                    if (tmplabel != Cal_Prod[iCal].CKit_Label)
                                    {
                                        CalKit_FailCheck = true;  // set flag to true, cal program will not proceed if flag true
                                        General.DisplayError(ClassName, "Error Cal Kit Verification", "Unrecognize ENA CalKit Label = " + tmplabel + '\n' +
                                            "Define Cal Kit Label in config file = " + Cal_Prod[iCal].CKit_Label + '\n' +
                                            "Please checked your configuration file !!!!!" + '\n' +
                                            " ***** Calibration Procedure will STOP and EXIT *****");
                                    }
                                }
                                tmpChannelNo = Cal_Prod[iCal].ChannelNumber;
                            }
                        }
                    }
                    if (!CalKit_FailCheck) //Will only proceed calibration if CalKit Label match or if not define CalKit_LocationNumber in production.condition excel spreadsheet
                    {
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            AnalysisEnable[iChn] = ENA.Calculate.FixtureSimulator.State((iChn + 1));
                            PortExtEnable[iChn] = ENA.Sense.Correction.Collect.PortExt.State((iChn + 1));
                            ENA.Calculate.FixtureSimulator.State((iChn + 1), false);
                            ENA.Sense.Correction.Collect.PortExt.State((iChn + 1), false);
                            string tempDef = ENA.Calculate.Par.GetTraceCategory(iChn + 1);
                            string[] parts = (tempDef.Split(','));
                            try
                            {
                                int trc1 = Convert.ToInt32(parts[0].Trim().Substring(4));
                                int trc2 = Convert.ToInt32(parts[2].Trim().Substring(3));
                                DisplayFormat[iChn, 0] = ENA.Calculate.Format.Format((iChn + 1), trc1);
                                DisplayFormat[iChn, 1] = ENA.Calculate.Format.Format((iChn + 1), trc2);
                                ENA.Calculate.Format.Format((iChn + 1), trc1, e_SFormat.SCOM);
                                ENA.Calculate.Format.Format((iChn + 1), trc2, e_SFormat.SCOM);
                            }
                            catch
                            {
                            }

                        }
                        General.DisplayMessage(ClassName + " --> " + SubClass, "Start Calibration", "Start Calibration");
                        string currentStandard = "";

 
                        for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                        {
                            string erroMsg = ENA.BasicCommand.System.QueryError();
                     
                            if (b_ZnbInUse && !erroMsg.ToUpper().Contains("NO ERROR")) //Recover ZNB State
                            {
                                General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Error", "Calibration value not plausible, please check your calibration setup and retry!");
                                ENA.BasicCommand.System.Reset();
                                if (StateFile != "") ENA.Memory.Load.State(StateFile);
                                ENA.Format.Border(e_Format.NORM);
                                Thread.Sleep(3000);
                                return;
                            }

                            #region "Switch"
                            //Temporary Disabled Switches
                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRFIN);
                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRFOUT);
                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRX);

                            //switch (Cal_Prod[iCal].Switch.ToUpper())
                            //{
                            //    case "B1":
                            //        //ºmyLibSwitch.SetPath(ºSwFBAR_B1);
                            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B1));
                            //        break;
                            //    case "B2":
                            //        //ºmyLibSwitch.SetPath(ºSwFBAR_B2);
                            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B2));
                            //        break;
                            //    case "B3":
                            //        //ºmyLibSwitch.SetPath(ºSwFBAR_B3);
                            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B3));
                            //        break;
                            //    case "B4":
                            //        //ºmyLibSwitch.SetPath(ºSwFBAR_B4);
                            //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B4));
                            //        break;
                            //    case "NONE":
                            //        break;
                            //}

                            #endregion

                            do
                            {
                                bNext = false;

                                #region "Calibration Message"
                                if (Cal_Prod[iCal].Message.Trim() != "")
                                {
                                    tmpStr = Cal_Prod[iCal].Message;
                                    //General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);

                                    /*
                                     * Tray Map
                                     * 
                                     * OPEN, SHORT, LOAD
                                     * T1, T2, T3
                                     * T4, T5, T6
                                     * T7, T8, T9
                                     * 
                                     * OPEN coordinates is (0,0) 
                                     * SHORT = (1,0)
                                     * LOAD = (2,0)
                                     * 
                                     * 
                                     * */

                                    if (tmpStr.ToUpper().Contains("OPEN")) { yada.TrayMapCoord("0,0"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("SHORT")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("1,0"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("LOAD")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("2,0"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#1")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("0,1"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#2")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("1,1"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#3")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("2,1"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#4")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("0,2"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#5")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("1,2"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#6")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("2,2"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#7")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("0,3"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#8")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("1,3"); currentStandard = tmpStr; }
                                    if (tmpStr.ToUpper().Contains("#9")) { yada.SendEOTCommand(1); yada.CheckSRQStatusByte(72); yada.TrayMapCoord("2,3"); currentStandard = tmpStr; }

                                    

                                }
                                else
                                {
                                    //if (currentStandard.ToUpper().Contains("OPEN")) { yada.TrayMapCoord("0,0"); }
                                    //if (currentStandard.ToUpper().Contains("SHORT")) { yada.TrayMapCoord("1,0"); }
                                    //if (currentStandard.ToUpper().Contains("LOAD")) { yada.TrayMapCoord("2,0"); }
                                    //if (currentStandard.ToUpper().Contains("#1")) { yada.TrayMapCoord("0,1"); }
                                    //if (currentStandard.ToUpper().Contains("#2")) { yada.TrayMapCoord("1,1"); }
                                    //if (currentStandard.ToUpper().Contains("#3")) { yada.TrayMapCoord("2,1"); }
                                    //if (currentStandard.ToUpper().Contains("#4")) { yada.TrayMapCoord("0,2"); }
                                    //if (currentStandard.ToUpper().Contains("#5")) { yada.TrayMapCoord("1,2"); }
                                    //if (currentStandard.ToUpper().Contains("#6")) { yada.TrayMapCoord("2,2"); }
                                    //if (currentStandard.ToUpper().Contains("#7")) { yada.TrayMapCoord("0,3"); }
                                    //if (currentStandard.ToUpper().Contains("#8")) { yada.TrayMapCoord("1,3"); }
                                    //if (currentStandard.ToUpper().Contains("#9")) { yada.TrayMapCoord("2,3"); }
                                    
                                    tmpStr = "Calibrating " + Cal_Prod[iCal].CalType.ToString() + " (Cal Kit) procedure for : "
                                                    + "\r\n\tChannel Number : " + Cal_Prod[iCal].ChannelNumber.ToString() + "\r\n\t";
                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.ECAL:
                                            for (int iPort = 0; iPort < Cal_Prod[iCal].No_Ports; iPort++)
                                            {
                                                switch (iPort)
                                                {
                                                    case 0:
                                                        tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                                        break;
                                                    case 1:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_2.ToString();
                                                        break;
                                                    case 2:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_3.ToString();
                                                        break;
                                                    case 3:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_4.ToString();
                                                        break;
                                                }
                                            }
                                            break;
                                        case e_CalibrationType.ISOLATION:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.LOAD:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.OPEN:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.SHORT:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.SUBCLASS:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.THRU:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                    }
                                }

                                #endregion

                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                if (Cal_Prod[iCal].b_CalKit)
                                {
                                    //if (!b_Mode)
                                    //{
                                    //General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                    //}
                                    #region "Cal Kit Procedure"

                                    if (ChannelNo != Cal_Prod[iCal].ChannelNumber)
                                    {
                                        if (ChannelNo >= 1)
                                        {
                                            ENA.Sense.Correction.Collect.Save(ChannelNo);
                                            ENA.Sense.Correction.Property(ChannelNo, true);
                                        }
                                        ChannelNo = Cal_Prod[iCal].ChannelNumber;
                                        ENA.Display.Window.Activate(ChannelNo);
                                        ENA.Sense.Correction.Property(ChannelNo, false);
                                        ENA.Sense.Correction.Clear(ChannelNo);
                                        ENA.Display.Window.Channel_Max(true);
                                        Thread.Sleep(500);
                                        ENA.Sense.Correction.Collect.Cal_Kit.Select_SubClass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                        ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CKit_Label);
                                        switch (Cal_TotalPort[ChannelNo - 1].No_Ports)
                                        {
                                            case 1:
                                                ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1);
                                                break;
                                            case 2:
                                                ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2);
                                                break;
                                            case 3:
                                                ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2, Cal_TotalPort[ChannelNo - 1].PortNo_3);
                                                break;
                                            case 4:
                                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2, Cal_TotalPort[ChannelNo - 1].PortNo_3, Cal_TotalPort[ChannelNo - 1].PortNo_4);
                                                break;
                                        }
                                        Thread.Sleep(500);
                                        ENA.BasicCommand.System.Operation_Complete();
                                    }

                                    //if (!b_Mode && bDebug) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);

                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.OPEN:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.SHORT:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.LOAD:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.THRU:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            //yada.SendEOTCommand(1);
                                            //yada.CheckSRQStatusByte(72);
                                            break;
                                        default:
                                            General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString() + " for Cal Kit Standard " + Cal_Prod[iCal].CalKit.ToString());
                                            break;
                                    }
                                    #endregion

                                    if (bDebug)
                                    {
                                        //KCC - Autocal
                                        General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", "Done.");
                                    }
                                }
                                else
                                {
                                    //if (!b_Mode)
                                    //{
                                    //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                    //}
                                    #region "Non Cal Kit Procedure"
                                    if (ChannelNo != Cal_Prod[iCal].ChannelNumber)
                                    {
                                        if (ChannelNo >= 1)
                                        {
                                            ENA.Sense.Correction.Collect.Save(ChannelNo);
                                            ENA.Sense.Correction.Property(ChannelNo, true);
                                        }
                                        ChannelNo = Cal_Prod[iCal].ChannelNumber;
                                        ENA.Display.Window.Activate(ChannelNo);
                                        ENA.Sense.Correction.Property(ChannelNo, false);
                                        ENA.Sense.Correction.Clear(ChannelNo);
                                        ENA.Display.Window.Channel_Max(true);
                                        switch (Cal_TotalPort[ChannelNo - 1].No_Ports)
                                        {
                                            case 1:
                                                ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, 1);
                                                break;
                                            case 2:
                                                ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, 1, 2);
                                                break;
                                            case 3:
                                                ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                                break;
                                            case 4:
                                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                                break;

                                        }
                                        ENA.BasicCommand.System.Operation_Complete();
                                    }

                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.ECAL:
                                            if (!b_Mode)
                                            {
                                                General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                            }
                                            #region "ECAL"
                                            switch (Cal_Prod[iCal].No_Ports)
                                            {
                                                case 1:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                    break;
                                                case 2:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                    break;
                                                case 3:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                                    break;
                                                case 4:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                                                    break;
                                            }
                                            #endregion
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.OPEN:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();

                                            break;
                                        case e_CalibrationType.SHORT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.LOAD:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.ISOLATION:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.THRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                Thread.Sleep(3000);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.SUBCLASS:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            Thread.Sleep(200);
                                            ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            Thread.Sleep(200);
                                            ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        default:
                                            General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString());
                                            break;
                                    }
                                    #endregion
                                    if (!b_Mode)
                                    {
                                        General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", "Done.");
                                    }
                                }


                                bNext = true;

                                Thread.Sleep(500);
                            } while (!bNext);
                        }
                        ENA.Sense.Correction.Collect.Save(ChannelNo);
                        ENA.Sense.Correction.Property(ChannelNo, true);
                        ENA.Display.Update(true);
                        ENA.Display.Window.Channel_Max(false);
                        /*
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            ENA.Calculate.Format.Format((iChn + 1), 1, DisplayFormat[iChn, 0]);
                            ENA.Calculate.Format.Format((iChn + 1), 2, DisplayFormat[iChn, 1]);
                            ENA.Calculate.FixtureSimulator.State((iChn + 1), AnalysisEnable[iChn]);
                            ENA.Sense.Correction.Collect.PortExt.State((iChn + 1), PortExtEnable[iChn]);
                        }
                         */
                    }
                    General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Completed", "Calibration Complete");
                    //Handler.SendSingleBin(1);
                    // For Future checking mechanism
                }
                catch (Exception e)
                {
                    General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Error", e.Message);
                }
            }

            public void Calibrate()
            {
                try
                {
                    string tmpStr;
                    string handStr;
                    bool bNext;
                    bool bDebug = false; //Set calibration debug on each steps
                    int ChannelNo = 0;
                    string tmplabel;
                    int tmpChannelNo = 0;
                    CalKit_FailCheck = false;
                    tmplabel = "";

                    e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                    bool[] AnalysisEnable = new bool[TotalChannel];
                    bool[] PortExtEnable = new bool[TotalChannel];

                    if (b_Mode) ENA.Display.Update(false);  // Turn Off the ENA when is Auto Mode

                    //Reset ZNB
                    if (b_ZnbInUse)
                    {
                        ENA.BasicCommand.System.Reset();
                        if (StateFile != "") ENA.Memory.Load.State(StateFile);
                        ENA.Format.Border(e_Format.NORM);
                        ENA.BasicCommand.SendCommand("CORR:COLL:AVER MAN"); //Disable Auto Averaging to prevent Query Interrupted
                        Thread.Sleep(3000);
                    }

                    //Check CalKit - Ensure StateFile preloaded with correct cal coefficient
                    for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                    {
                        if (tmpChannelNo != Cal_Prod[iCal].ChannelNumber)
                        {

                            ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CKit_Label);

                            //Only verify the 1st OPEN cal statement only. Must ensure that your Cal Kit Label define correctly for this row
                            if (e_CalibrationType.OPEN == Cal_Prod[iCal].CalType)
                            {
                                if (Cal_Prod[iCal].CKit_LocNum != 0) //Only check if user define the cal kit location number, undefine will assume no check required
                                {
                                    char[] trimChar = { '\"', '\n' };
                                    tmplabel = ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber);
                                    tmplabel = tmplabel.Trim(trimChar);
                                    if (tmplabel != Cal_Prod[iCal].CKit_Label)
                                    {
                                        CalKit_FailCheck = true;  // set flag to true, cal program will not proceed if flag true
                                        General.DisplayError(ClassName, "Error Cal Kit Verification", "Unrecognize ENA CalKit Label = " + tmplabel + '\n' +
                                            "Define Cal Kit Label in config file = " + Cal_Prod[iCal].CKit_Label + '\n' +
                                            "Please checked your configuration file !!!!!" + '\n' +
                                            " ***** Calibration Procedure will STOP and EXIT *****");
                                    }
                                }
                                tmpChannelNo = Cal_Prod[iCal].ChannelNumber;
                            }
                        }
                    }
                    if (!CalKit_FailCheck) //Will only proceed calibration if CalKit Label match or if not define CalKit_LocationNumber in production.condition excel spreadsheet
                    {
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            AnalysisEnable[iChn] = ENA.Calculate.FixtureSimulator.State((iChn + 1));
                            PortExtEnable[iChn] = ENA.Sense.Correction.Collect.PortExt.State((iChn + 1));
                            ENA.Calculate.FixtureSimulator.State((iChn + 1), false);
                            ENA.Sense.Correction.Collect.PortExt.State((iChn + 1), false);
                            string tempDef = ENA.Calculate.Par.GetTraceCategory(iChn + 1);
                            string[] parts = (tempDef.Split(','));
                            try
                            {
                                int trc1 = Convert.ToInt32(parts[0].Trim().Substring(4));
                                int trc2 = Convert.ToInt32(parts[2].Trim().Substring(3));
                                DisplayFormat[iChn, 0] = ENA.Calculate.Format.Format((iChn + 1), trc1);
                                DisplayFormat[iChn, 1] = ENA.Calculate.Format.Format((iChn + 1), trc2);
                                ENA.Calculate.Format.Format((iChn + 1), trc1, e_SFormat.SCOM);
                                ENA.Calculate.Format.Format((iChn + 1), trc2, e_SFormat.SCOM);
                            }
                            catch
                            {
                            }

                        }
                        General.DisplayMessage(ClassName + " --> " + SubClass, "Start Calibration", "Start Calibration");

                        for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                        {
                            string erroMsg = ENA.BasicCommand.System.QueryError();
                            ////ChoonChin - Debug
                            //General.DisplayMessage("", "Channel = " + Cal_Prod[iCal].ChannelNumber + "\nCalType = " + Cal_Prod[iCal].CalType.ToString() + "\nPort = " + Cal_Prod[iCal].Port_1.ToString(), "");

                            if (b_ZnbInUse && !erroMsg.ToUpper().Contains("NO ERROR")) //Recover ZNB State
                            {
                                General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Error", "Calibration value not plausible, please check your calibration setup and retry!");
                                ENA.BasicCommand.System.Reset();
                                if (StateFile != "") ENA.Memory.Load.State(StateFile);
                                ENA.Format.Border(e_Format.NORM);
                                Thread.Sleep(3000);
                                return;
                            }

                            #region "Switch"

                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRFIN);
                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRFOUT);
                            SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), Operation.ENAtoRX);

                            #endregion

                            do
                            {
                                bNext = false;

                                #region "Calibration Message"
                                if (Cal_Prod[iCal].Message.Trim() != "")
                                {
                                    tmpStr = Cal_Prod[iCal].Message;
                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                }
                                else
                                {
                                    tmpStr = "Calibrating " + Cal_Prod[iCal].CalType.ToString() + " (Cal Kit) procedure for : "
                                                    + "\r\n\tChannel Number : " + Cal_Prod[iCal].ChannelNumber.ToString() + "\r\n\t";
                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.ECAL:
                                            for (int iPort = 0; iPort < Cal_Prod[iCal].No_Ports; iPort++)
                                            {
                                                switch (iPort)
                                                {
                                                    case 0:
                                                        tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                                        break;
                                                    case 1:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_2.ToString();
                                                        break;
                                                    case 2:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_3.ToString();
                                                        break;
                                                    case 3:
                                                        tmpStr += "," + Cal_Prod[iCal].Port_4.ToString();
                                                        break;
                                                }
                                            }
                                            break;
                                        case e_CalibrationType.ISOLATION:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.LOAD:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.OPEN:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.SHORT:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.SUBCLASS:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.THRU:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                    }
                                }

                                #endregion

                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                if (Cal_Prod[iCal].b_CalKit)
                                {
                                    #region "Cal Kit Procedure"

                                    if (ChannelNo != Cal_Prod[iCal].ChannelNumber)
                                    {
                                        if (ChannelNo >= 1)
                                        {
                                            ENA.Sense.Correction.Collect.Save(ChannelNo);
                                            ENA.Sense.Correction.Property(ChannelNo, true);
                                        }
                                        ChannelNo = Cal_Prod[iCal].ChannelNumber;
                                        ENA.Display.Window.Activate(ChannelNo);
                                        ENA.Sense.Correction.Property(ChannelNo, false);
                                        ENA.Sense.Correction.Clear(ChannelNo);
                                        ENA.Display.Window.Channel_Max(true);
                                        Thread.Sleep(500);
                                        ENA.Sense.Correction.Collect.Cal_Kit.Select_SubClass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                        ENA.Sense.Correction.Collect.Cal_Kit.Label(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CKit_Label);
                                        switch (Cal_TotalPort[ChannelNo - 1].No_Ports)
                                        {
                                            case 1:
                                                ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1);
                                                break;
                                            case 2:
                                                ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2);
                                                break;
                                            case 3:
                                                ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2, Cal_TotalPort[ChannelNo - 1].PortNo_3);
                                                break;
                                            case 4:
                                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_TotalPort[ChannelNo - 1].PortNo_1, Cal_TotalPort[ChannelNo - 1].PortNo_2, Cal_TotalPort[ChannelNo - 1].PortNo_3, Cal_TotalPort[ChannelNo - 1].PortNo_4);
                                                break;
                                        }
                                        Thread.Sleep(500);
                                        ENA.BasicCommand.System.Operation_Complete();
                                    }

                                    //if (!b_Mode && bDebug) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);

                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.OPEN:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.SHORT:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.LOAD:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.THRU:
                                            ENA.Sense.Correction.Collect.Cal_Kit.SubClass_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode) General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                            }
                                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        default:
                                            General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString() + " for Cal Kit Standard " + Cal_Prod[iCal].CalKit.ToString());
                                            break;
                                    }
                                    #endregion

                                    if (bDebug)
                                    {
                                        //KCC - Autocal
                                        General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", "Done.");
                                    }
                                }
                                else
                                {
                                    //if (!b_Mode)
                                    //{
                                    //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                    #region "Non Cal Kit Procedure"
                                    if (ChannelNo != Cal_Prod[iCal].ChannelNumber)
                                    {
                                        if (ChannelNo >= 1)
                                        {
                                            ENA.Sense.Correction.Collect.Save(ChannelNo);
                                            ENA.Sense.Correction.Property(ChannelNo, true);
                                        }
                                        ChannelNo = Cal_Prod[iCal].ChannelNumber;
                                        ENA.Display.Window.Activate(ChannelNo);
                                        ENA.Sense.Correction.Property(ChannelNo, false);
                                        ENA.Sense.Correction.Clear(ChannelNo);
                                        ENA.Display.Window.Channel_Max(true);
                                        switch (Cal_TotalPort[ChannelNo - 1].No_Ports)
                                        {
                                            case 1:
                                                ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, 1);
                                                break;
                                            case 2:
                                                ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, 1, 2);
                                                break;
                                            case 3:
                                                ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                                break;
                                            case 4:
                                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                                break;

                                        }
                                        ENA.BasicCommand.System.Operation_Complete();
                                    }

                                    switch (Cal_Prod[iCal].CalType)
                                    {
                                        case e_CalibrationType.ECAL:
                                            if (!b_Mode)
                                            {
                                                General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                            }
                                            #region "ECAL"
                                            switch (Cal_Prod[iCal].No_Ports)
                                            {
                                                case 1:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                    break;
                                                case 2:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                    break;
                                                case 3:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                                    break;
                                                case 4:
                                                    ENA.Sense.Correction.Collect.ECAL.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                                                    break;
                                            }
                                            #endregion
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.OPEN:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();

                                            break;
                                        case e_CalibrationType.SHORT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.LOAD:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.ISOLATION:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                Thread.Sleep(Cal_Prod[iCal].Sleep);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.THRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                Thread.Sleep(3000);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            Thread.Sleep(Cal_Prod[iCal].Sleep);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.SUBCLASS:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLLINE:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            Thread.Sleep(200);
                                            ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLREFLECT:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        case e_CalibrationType.TRLTHRU:
                                            if (bDebug)
                                            {
                                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                                ENA.BasicCommand.System.Operation_Complete();
                                                if (!b_Mode)
                                                {
                                                    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                                                }
                                            }
                                            ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            Thread.Sleep(200);
                                            ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                            ENA.BasicCommand.System.Operation_Complete();
                                            break;
                                        default:
                                            General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString());
                                            break;
                                    }
                                    #endregion
                                    if (!b_Mode)
                                    {
                                        General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", "Done.");
                                    }
                                }


                                bNext = true;

                                Thread.Sleep(500);
                            } while (!bNext);
                        }
                        ENA.Sense.Correction.Collect.Save(ChannelNo);
                        ENA.Sense.Correction.Property(ChannelNo, true);
                        ENA.Display.Update(true);
                        ENA.Display.Window.Channel_Max(false);
                        /*
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            ENA.Calculate.Format.Format((iChn + 1), 1, DisplayFormat[iChn, 0]);
                            ENA.Calculate.Format.Format((iChn + 1), 2, DisplayFormat[iChn, 1]);
                            ENA.Calculate.FixtureSimulator.State((iChn + 1), AnalysisEnable[iChn]);
                            ENA.Sense.Correction.Collect.PortExt.State((iChn + 1), PortExtEnable[iChn]);
                        }
                         */
                    }
                    General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Completed", "Calibration Complete");
                    //Handler.SendSingleBin(1);
                    // For Future checking mechanism
                }
                catch (Exception e)
                {
                    General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Error", e.Message);
                }
            }
            
        }
        
        /*
        public class cCalibrationClasses : ClothoLibStandard.Lib_Var //Added by KCC
        {
            public static string SubClass = "Calibration Class";
            public struct s_CalibrationProcedure
            {
                public e_CalibrationType CalType;
                public int ChannelNumber;
                public int No_Ports;
                public int Port_1;
                public int Port_2;
                public int Port_3;
                public int Port_4;
                public int CalKit;
                public bool b_CalKit;
                public string Message;
                //KCC
                public string Switch;
            }
            public bool b_Mode;
            private s_CalibrationProcedure[] Cal_Prod;
            //public int[] iPortMethod = new int[TotalChannel];

            public s_CalibrationProcedure[] parse_Procedure
            {
                set
                {
                    Cal_Prod = value;
                }
            }

             public void Calibrate()
            {
                string tmpStr;
                string handStr;
                bool bNext;
                int ChannelNo = 0;
                e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                bool[] AnalysisEnable = new bool[TotalChannel];
                //if (b_Mode) ENA.Display.Update(false);  // Turn Off the ENA when is Auto Mode

                //for (int iChn = 0; iChn < TotalChannel; iChn++)
                //{
                //    AnalysisEnable[iChn] = ENA.Calculate.FixtureSimulator.State();
                //    ENA.Calculate.FixtureSimulator.State(false);
                //    DisplayFormat[iChn, 0] = ENA.Calculate.Format.Format((iChn + 1), 1);
                //    DisplayFormat[iChn, 1] = ENA.Calculate.Format.Format((iChn + 1), 2);
                //    ENA.Calculate.Format.Format((iChn + 1), 1, e_SFormat.SCOM);
                //    ENA.Calculate.Format.Format((iChn + 1), 2, e_SFormat.SCOM);
                //}

                //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B34B39));
                //ºmyLibSwitch.SetPath(ºSwFBAR_B34B39);
                //General.DisplayMessage("Fbar-Ecal", "B34B49","Connect to A1-B1,and when you complete Ecal, Then Press OK");
                    
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    #region "Switch"

                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRFIN);
                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRFOUT);
                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRX);

                    //switch (Cal_Prod[iCal].Switch.ToUpper())
                    //{
                    //    case "B1":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B1);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B1));
                    //        break;
                    //    case "B2":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B2);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B2));
                    //        break;
                    //    case "B3":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B3);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B3));
                    //        break;
                    //    case "B4":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B4);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B4));
                    //        break;
                    //    case "NONE":
                    //        break;
                    //}

                    #endregion

                    //For switch
                    //Thread.Sleep(10);

                    #region "Calibration Message"
                    if (Cal_Prod[iCal].Message.Trim() != "")
                    {
                        tmpStr = Cal_Prod[iCal].Message;
                    }
                    else
                    {
                        tmpStr = "Calibrating " + Cal_Prod[iCal].CalType.ToString() + " (Cal Kit) procedure for : "
                                        + "\r\n\tChannel Number : " + Cal_Prod[iCal].ChannelNumber.ToString() + "\r\n\t";
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.ECAL:
                                for (int iPort = 0; iPort < Cal_Prod[iCal].No_Ports; iPort++)
                                {
                                    switch (iPort)
                                    {
                                        case 0:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case 1:
                                            tmpStr += "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case 2:
                                            tmpStr += "," + Cal_Prod[iCal].Port_3.ToString();
                                            break;
                                        case 3:
                                            tmpStr += "," + Cal_Prod[iCal].Port_4.ToString();
                                            break;
                                    }
                                }
                                break;
                            case e_CalibrationType.ISOLATION:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.LOAD:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.OPEN:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.SHORT:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.SUBCLASS:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.THRU:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.TRLLINE:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                        }
                    }

                    #endregion

                    if (Cal_Prod[iCal].b_CalKit)
                    {
                        if (!b_Mode)
                        {
                            General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                        }
                        #region "Cal Kit Procedure"
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.OPEN:
                                ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.SHORT:
                                ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.LOAD:
                                ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.THRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            default:
                                General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString() + " for Cal Kit Standard " + Cal_Prod[iCal].CalKit.ToString());
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        if (!b_Mode)
                        {
                            //KCC - Autocal
                            if (Cal_Prod[iCal].Message.Trim() != "")
                            {
                                General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                        #region "Non Cal Kit Procedure"

                        if (Cal_Prod[iCal].ChannelNumber >= 1)
                        {
                            switch (Cal_Prod[iCal].No_Ports)
                            {
                                case 1:
                                    ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, 1);
                                    break;
                                case 2:
                                    ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, 1, 2);
                                    break;
                                case 3:
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                    break;
                                case 4:
                                    ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                    break;

                            }
                            Thread.Sleep(300);
                            ENA.BasicCommand.System.Operation_Complete();
                        }

                        switch (Cal_Prod[iCal].CalType)
                        {  
                            case e_CalibrationType.ECAL:
                                #region "ECAL"
                                switch (Cal_Prod[iCal].No_Ports)
                                {
                                    case 1:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                        break;
                                    case 2:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                        break;
                                    case 3:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                        break;
                                    case 4:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                                        break;
                                }
                                #endregion
                                Thread.Sleep(12000);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.OPEN:
                                ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                //KCC - ENA error issue during Autocal
                                if (iCal == 0)
                                {
                                    Thread.Sleep(3000);
                                }
                                ENA.BasicCommand.System.Operation_Complete();                            
                                
                                break;
                            case e_CalibrationType.SHORT:
                                ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.LOAD:
                                ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.ISOLATION:
                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.THRU:
                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.SUBCLASS:
                                ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            default:
                                General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString());
                                break;
                        }
                        #endregion
                        Thread.Sleep(200);
                    }
                }


                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                    ENA.Sense.Correction.Collect.Save(iChn + 1);
                }

                General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Completed", "Calibration Complete");
            }
        }
        */
        /*
        public class cCalibrationClasses : ClothoLibStandard.Lib_Var //Added by KCC
        {
            public static string SubClass = "Calibration Class";
            public struct s_CalibrationProcedure
            {
                public e_CalibrationType CalType;
                public int ChannelNumber;
                public int No_Ports;
                public int Port_1;
                public int Port_2;
                public int Port_3;
                public int Port_4;
                public int CalKit;
                public bool b_CalKit;
                public string Message;
                //KCC
                public string Switch;
            }
            public bool b_Mode;
            private s_CalibrationProcedure[] Cal_Prod;
            //public int[] iPortMethod = new int[TotalChannel];

            public s_CalibrationProcedure[] parse_Procedure
            {
                set
                {
                    Cal_Prod = value;
                }
            }

             public void Calibrate()
            {
                string tmpStr;
                string handStr;
                bool bNext;
                int ChannelNo = 0;
                e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                bool[] AnalysisEnable = new bool[TotalChannel];
                //if (b_Mode) ENA.Display.Update(false);  // Turn Off the ENA when is Auto Mode

                //for (int iChn = 0; iChn < TotalChannel; iChn++)
                //{
                //    AnalysisEnable[iChn] = ENA.Calculate.FixtureSimulator.State();
                //    ENA.Calculate.FixtureSimulator.State(false);
                //    DisplayFormat[iChn, 0] = ENA.Calculate.Format.Format((iChn + 1), 1);
                //    DisplayFormat[iChn, 1] = ENA.Calculate.Format.Format((iChn + 1), 2);
                //    ENA.Calculate.Format.Format((iChn + 1), 1, e_SFormat.SCOM);
                //    ENA.Calculate.Format.Format((iChn + 1), 2, e_SFormat.SCOM);
                //}

                //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B34B39));
                //ºmyLibSwitch.SetPath(ºSwFBAR_B34B39);
                //General.DisplayMessage("Fbar-Ecal", "B34B49","Connect to A1-B1,and when you complete Ecal, Then Press OK");
                    
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    #region "Switch"

                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRFIN);
                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRFOUT);
                    SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper().Trim(), DutPaths.ENAtoRX);

                    //switch (Cal_Prod[iCal].Switch.ToUpper())
                    //{
                    //    case "B1":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B1);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B1));
                    //        break;
                    //    case "B2":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B2);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B2));
                    //        break;
                    //    case "B3":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B3);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B3));
                    //        break;
                    //    case "B4":
                    //        //ºmyLibSwitch.SetPath(ºSwFBAR_B4);
                    //        //ºmyLibSW_AemWLF.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwFBAR_B4));
                    //        break;
                    //    case "NONE":
                    //        break;
                    //}

                    #endregion

                    //For switch
                    //Thread.Sleep(10);

                    #region "Calibration Message"
                    if (Cal_Prod[iCal].Message.Trim() != "")
                    {
                        tmpStr = Cal_Prod[iCal].Message;
                    }
                    else
                    {
                        tmpStr = "Calibrating " + Cal_Prod[iCal].CalType.ToString() + " (Cal Kit) procedure for : "
                                        + "\r\n\tChannel Number : " + Cal_Prod[iCal].ChannelNumber.ToString() + "\r\n\t";
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.ECAL:
                                for (int iPort = 0; iPort < Cal_Prod[iCal].No_Ports; iPort++)
                                {
                                    switch (iPort)
                                    {
                                        case 0:
                                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                            break;
                                        case 1:
                                            tmpStr += "," + Cal_Prod[iCal].Port_2.ToString();
                                            break;
                                        case 2:
                                            tmpStr += "," + Cal_Prod[iCal].Port_3.ToString();
                                            break;
                                        case 3:
                                            tmpStr += "," + Cal_Prod[iCal].Port_4.ToString();
                                            break;
                                    }
                                }
                                break;
                            case e_CalibrationType.ISOLATION:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.LOAD:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.OPEN:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.SHORT:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.SUBCLASS:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.THRU:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.TRLLINE:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                break;
                        }
                    }

                    #endregion

                    if (Cal_Prod[iCal].b_CalKit)
                    {
                        if (!b_Mode)
                        {
                            General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                        }
                        #region "Cal Kit Procedure"
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.OPEN:
                                ENA.Sense.Correction.Collect.Cal_Kit.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.SHORT:
                                ENA.Sense.Correction.Collect.Cal_Kit.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.LOAD:
                                ENA.Sense.Correction.Collect.Cal_Kit.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.THRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                break;
                            default:
                                General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString() + " for Cal Kit Standard " + Cal_Prod[iCal].CalKit.ToString());
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        if (!b_Mode)
                        {
                            //KCC - Autocal
                            if (Cal_Prod[iCal].Message.Trim() != "")
                            {
                                General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration", tmpStr);
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                        #region "Non Cal Kit Procedure"

                        if (Cal_Prod[iCal].ChannelNumber >= 1)
                        {
                            switch (Cal_Prod[iCal].No_Ports)
                            {
                                case 1:
                                    ENA.Sense.Correction.Collect.Method.SOLT1(Cal_Prod[iCal].ChannelNumber, 1);
                                    break;
                                case 2:
                                    ENA.Sense.Correction.Collect.Method.SOLT2(Cal_Prod[iCal].ChannelNumber, 1, 2);
                                    break;
                                case 3:
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                    break;
                                case 4:
                                    ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                    break;

                            }
                            Thread.Sleep(300);
                            ENA.BasicCommand.System.Operation_Complete();
                        }

                        switch (Cal_Prod[iCal].CalType)
                        {  
                            case e_CalibrationType.ECAL:
                                #region "ECAL"
                                switch (Cal_Prod[iCal].No_Ports)
                                {
                                    case 1:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                        break;
                                    case 2:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT2(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                        break;
                                    case 3:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT3(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                        break;
                                    case 4:
                                        ENA.Sense.Correction.Collect.ECAL.SOLT4(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                                        break;
                                }
                                #endregion
                                Thread.Sleep(12000);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.OPEN:
                                ENA.Sense.Correction.Collect.Acquire.Open(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                //KCC - ENA error issue during Autocal
                                if (iCal == 0)
                                {
                                    Thread.Sleep(3000);
                                }
                                ENA.BasicCommand.System.Operation_Complete();                            
                                
                                break;
                            case e_CalibrationType.SHORT:
                                ENA.Sense.Correction.Collect.Acquire.Short(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.LOAD:
                                ENA.Sense.Correction.Collect.Acquire.Load(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.ISOLATION:
                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.THRU:
                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.SUBCLASS:
                                ENA.Sense.Correction.Collect.Acquire.Subclass(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.TRLLine(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Acquire.TRLReflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(200);
                                ENA.Sense.Correction.Collect.Acquire.TRLThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            default:
                                General.DisplayError(ClassName, "Error in Normal Calibration Procedure", "Unrecognize Calibration Procedure = " + Cal_Prod[iCal].CalType.ToString());
                                break;
                        }
                        #endregion
                        Thread.Sleep(200);
                    }
                }


                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                    ENA.Sense.Correction.Collect.Save(iChn + 1);
                }

                General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Completed", "Calibration Complete");
            }
        }
        */
    }
}
