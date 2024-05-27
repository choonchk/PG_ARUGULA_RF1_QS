using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;
using SParamTestCommon;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// Temporary module to speed up TCF loading.
    /// </summary>
    public class TcfSpeedLoader
    {
        public S_Param[] SParamData;
        private S_CMRRnBal_Param[] SBalanceParamData;
        public s_TraceMatching[] TraceMatch;
        public s_SegmentTable[] SegmentParam;

        private TcfSheetTrace m_sheetTrace;
        private TcfSheetSegmentTable m_sheetSegment;

        public void SetTraceData(TcfSheetTrace sheetTrace, TcfSheetSegmentTable sheetSegment) //seoul
        {
            m_sheetTrace = sheetTrace;
            m_sheetSegment = sheetSegment;
        }

        public void Init_Channel()
        {
            SetTraceMatching(m_sheetTrace.testPlan.Count);
        }
        public void Init_SegmentParam(bool[] cprojectPortEnable, int CalColmnIndexNFset,
            int totalChannel, bool isDiva = false)
        {
            SegmentParam = Init_SegmentParam2(cprojectPortEnable, CalColmnIndexNFset, totalChannel, isDiva);
        }

        /// <summary>
        /// Speed up repetitive call to get the same value. By cache.
        /// </summary>
        /// <param name="Test_Parameters"></param>
        /// <returns></returns>
        public List<string> GetRowAboveTestParameterRow(Dictionary<string, int> Test_Parameters)
        {
            if (m_cacheRowAboveTp != null) return m_cacheRowAboveTp;

            m_cacheRowAboveTp = new List<string>();

            for (int channel = 1; channel < 10; channel++)  // arbitrary max number 10.
            {
                string searchName = "V_CH" + channel;
                bool isChannelExist = Test_Parameters.ContainsKey(searchName);
                if (isChannelExist)
                {
                    string rowAboveHeader = cExtract.Get_Data("Condition_FBAR", 1, Test_Parameters[searchName]);
                    m_cacheRowAboveTp.Add(rowAboveHeader);
                }
            }

            return m_cacheRowAboveTp;
        }

        private void SetTraceMatching(int totalChannel)
        {
            int ChannelNumber;
            int PortNumbers;
            string TraceSetting;
            TraceMatch = new s_TraceMatching[totalChannel]; //This has TraceNumber and S_param_Def_number
            SParamData = new S_Param[totalChannel]; //Freq, num of points, num of ports, sparam enable, total trace count, s param data - this line is levels deep with structs
            SBalanceParamData = new S_CMRRnBal_Param[totalChannel];
            CreateArray(totalChannel);

            for (int iChn = 0; iChn < totalChannel; iChn++)
            {
                //Modified by KCC
                //TraceMatch[iChn].TraceNumber = new int[24];
                TraceMatch[iChn].TraceNumber = new int[Enum.GetValues(typeof(e_SParametersDef)).Length];  //This line sets the TraceNumber array size to be the length of the number or enums of type e_SParametersDef
                TraceMatch[iChn].SParam_Def_Number = new int[Enum.GetValues(typeof(e_SParametersDef)).Length]; //This is the line that sets up the array size for SParam_Def_Number
            }
            for (int iRow = 0; iRow < totalChannel; iRow++)
            {
                //ChannelNumber = int.Parse(cExtract.Get_Data(TraceTabName, (iRow + 2), 1));      // seoul
                //PortNumbers = int.Parse(cExtract.Get_Data(TraceTabName, (iRow + 2), 2));        //Excel Data
                //SParamData[iRow].NoPorts = PortNumbers;
                ////   SParamData[iRow].NoPoints 
                //TraceSetting = cExtract.Get_Data(TraceTabName, (iRow + 2), 3);                  //Excel Data
                m_sheetTrace.SetCurrentRow(iRow);
                // CAse HLS2
                //ChannelNumber = m_sheetTrace.GetDataInt("Channel (new segment tablel highlighted)");
                ChannelNumber = m_sheetTrace.GetDataInt("Channel");

                PortNumbers = m_sheetTrace.GetDataInt("Ports");
                SParamData[iRow].NoPorts = PortNumbers;
                //   SParamData[iRow].NoPoints 
                TraceSetting = m_sheetTrace.GetData("Trace Number Settings");
                Init_TraceMatch((ChannelNumber - 1), PortNumbers, convertAutoStr2Bool(TraceSetting)); //This is the line that sets up the number of traces per channel based on the TCF entry++
            }
        }

        public List<StateFileDataObject> GetStateFileGenerationInput(int totalChannel)
        {
            List< StateFileDataObject > sfList = new List<StateFileDataObject>();
            int TopazWindows;            //added chee on 11-July-2017

            for (int iRow = 0; iRow < totalChannel; iRow++)
            {
                //seoul
                m_sheetTrace.SetCurrentRow(iRow);

                StateFileDataObject sf = new StateFileDataObject();
                sf.ChannelNumber = m_sheetTrace.GetDataInt("Channel") - 1;
                sf.FilteredTraceNameList = SetSParamDefNumber(TraceMatch[sf.ChannelNumber]);
                sf.SegmentTable = SegmentParam[sf.ChannelNumber];
                sf.NF_source_P_string = m_sheetTrace.GetData("NF_src");
                sf.NF_rcv_P_string = m_sheetTrace.GetData("NF_rcv");
                sf.NF_sweep_Time_string = m_sheetTrace.GetData("NF_sweep[ms]");
                sfList.Add(sf);
            }

            return sfList;
        }

        private List<string> SetSParamDefNumber(s_TraceMatching tm)
        {
            List<string> traceNameList = new List<string>();
            int tmp_SParam_Def = 0;

            for (int iDef = 0; iDef < Enum.GetValues(typeof(e_SParametersDef)).Length; iDef++)
            {
                if (tm.TraceNumber[iDef] >= 0)
                {
                    tm.SParam_Def_Number[tmp_SParam_Def] = iDef;
                    tmp_SParam_Def++;
                    string traceName = Enum.GetName(typeof(e_SParametersDef), iDef);
                    traceNameList.Add(traceName);
                }


            }

            return traceNameList;
        }

        private void Init_TraceMatch(int ChannelNumber, int PortNumber, bool AutoSet)
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
                        break;
                    case 2:
                        //SParamData[ChannelNumber].sParam_Data = new S_ParamData[4];
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        break;
                    case 3:
                        //SParamData[ChannelNumber].sParam_Data = new S_ParamData[9];
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S33.GetHashCode()] = 4;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S13.GetHashCode()] = 5;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S23.GetHashCode()] = 6;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S31.GetHashCode()] = 7;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S32.GetHashCode()] = 8;
                        break;
                    case 4:
                        //SParamData[ChannelNumber].sParam_Data = new S_ParamData[16];
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S11.GetHashCode()] = 0;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S22.GetHashCode()] = 1;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S12.GetHashCode()] = 2;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S21.GetHashCode()] = 3;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S33.GetHashCode()] = 4;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S13.GetHashCode()] = 5;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S23.GetHashCode()] = 6;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S31.GetHashCode()] = 7;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S32.GetHashCode()] = 8;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S44.GetHashCode()] = 9;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S14.GetHashCode()] = 10;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S24.GetHashCode()] = 11;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S34.GetHashCode()] = 12;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S41.GetHashCode()] = 13;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S42.GetHashCode()] = 14;
                        //TraceMatch[ChannelNumber].TraceNumber[e_SParametersDef.S43.GetHashCode()] = 15;
                        break;
                }
            }
            else
            {
                //Modified by KCC
                //SParamData[ChannelNumber].sParam_Data = new S_ParamData[27];
                int size = Enum.GetValues(typeof(e_SParametersDef)).Length;
                SParamData[ChannelNumber].sParam_Data = CreateArray2(size);

                for (int iDef = 0; iDef < Enum.GetValues(typeof(e_SParametersDef)).Length; iDef++) //so the purpose of this loop is to come up with TotalTraceCount and populate the TraceNumber array for each channel
                {
                    string ka = m_sheetTrace.allContents.Item3[ChannelNumber + 1, iDef + 3];
                    int ka2 = GetDataInt(ka);
                    if (ka2 - 1 != -1)  //The -1 is, I think, to make it zero based
                    {
                        SParamData[ChannelNumber].TotalTraceCount++;
                        //TraceMatch[ChannelNumber].TraceNumber[iDef] = cExtract.Get_Data_Int(TraceTabName, ChannelNumber + 2, iDef + 4) - 1;
                        string ka3 = m_sheetTrace.allContents.Item3[ChannelNumber + 1, iDef + 3];
                        int ka4 = Convert.ToInt16(ka3 == "" ? "0" : ka3) - 1;
                        TraceMatch[ChannelNumber].TraceNumber[iDef] = ka4; //seoul
                    }

                }

                ////
            }
            int tmp_SParam_Def = 0;

            for (int iTrace = 0; iTrace < Enum.GetValues(typeof(e_SParametersDef)).Length; iTrace++)
            {
                if (TraceMatch[ChannelNumber].TraceNumber[iTrace] >= 0)
                {
                    TraceMatch[ChannelNumber].SParam_Def_Number[tmp_SParam_Def] = iTrace;
                    tmp_SParam_Def++;
                }
            }
        }

        private int GetDataInt(string num)
        {
            bool isZero = String.IsNullOrEmpty(num);
            if (isZero) return 0;
            return int.Parse(num);
        }

        private bool convertAutoStr2Bool(string input)
        {
            if (input.ToUpper() == "AUTO")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<string> m_cacheRowAboveTp;
        private s_SegmentTable[] Init_SegmentParam2(bool[] cprojectPortEnable, int CalColmnIndexNFset,
            int totalChannel, bool isDiva = false)
        {
            string tmpStr;

            var RowNo = 0;
            var SegmentSettings = false;
            var SegmentTableSettings = false;
            var TotalPoints = 0;
            var TotalSegment = 0;
            var ChannelNumber = 0;

            // CCT: reset the indexing for compatibility.
            CalColmnIndexNFset = CalColmnIndexNFset - 1;

            s_SegmentTable[] st = new s_SegmentTable[totalChannel];

            try
            {
                do
                {
                    //tmpStr = cExtract.Get_Data(SegmentTabName, RowNo, 1);
                    tmpStr = m_sheetSegment.allContents.Item3[RowNo, 0];

                    if (SegmentSettings == true)
                    {
                        ChannelNumber = (m_sheetSegment.GetDataInt(RowNo, 1)) - 1;
                        //int AA = cExtract.Get_Data_Int(SegmentTabName, RowNo + 1, 2);
                        st[ChannelNumber].mode = (e_ModeSetting)m_sheetSegment.GetDataInt(RowNo + 1, 1);
                        st[ChannelNumber].ifbw = (e_OnOff)m_sheetSegment.GetDataInt(RowNo + 2, 1);
                        st[ChannelNumber].pow = (e_OnOff)m_sheetSegment.GetDataInt(RowNo + 3, 1);
                        st[ChannelNumber].del = (e_OnOff)m_sheetSegment.GetDataInt(RowNo + 4, 1);
                        st[ChannelNumber].swp = (e_OnOff)m_sheetSegment.GetDataInt(RowNo + 5, 1);
                        st[ChannelNumber].time = (e_OnOff)m_sheetSegment.GetDataInt(RowNo + 6, 1);
                        st[ChannelNumber].segm = m_sheetSegment.GetDataInt(RowNo + 7, 1);

                        SegmentSettings = false;
                    }
                    if (SegmentTableSettings == true)
                    {
                        if (TotalPoints == 0) st[ChannelNumber].SegmentData = new s_SegmentData[st[ChannelNumber].segm];
                        s_SegmentData stTotal = st[ChannelNumber].SegmentData[TotalSegment];
                        stTotal.Start = m_sheetSegment.GetFrequency(RowNo, 1);
                        stTotal.Stop = m_sheetSegment.GetFrequency(RowNo, 2);
                        stTotal.Points = m_sheetSegment.GetDataInt(RowNo, 3);
                        TotalPoints += stTotal.Points;
                        stTotal.ifbw_value = m_sheetSegment.GetFrequency2(isDiva, RowNo, 4);

                        if (cprojectPortEnable[0]) stTotal.pow_n1_value = m_sheetSegment.GetDataDouble(RowNo, 5);
                        if (cprojectPortEnable[1]) stTotal.pow_n2_value = m_sheetSegment.GetDataDouble(RowNo, 6);
                        if (cprojectPortEnable[2]) stTotal.pow_n3_value = m_sheetSegment.GetDataDouble(RowNo, 7);
                        if (cprojectPortEnable[3]) stTotal.pow_n4_value = m_sheetSegment.GetDataDouble(RowNo, 8);
                        if (cprojectPortEnable[4]) stTotal.pow_n5_value = m_sheetSegment.GetDataDouble(RowNo, 9);
                        if (cprojectPortEnable[5]) stTotal.pow_n6_value = m_sheetSegment.GetDataDouble(RowNo, 10);
                        if (cprojectPortEnable[6]) stTotal.pow_n7_value = m_sheetSegment.GetDataDouble(RowNo, 11);
                        if (cprojectPortEnable[7]) stTotal.pow_n8_value = m_sheetSegment.GetDataDouble(RowNo, 12);
                        if (cprojectPortEnable[8]) stTotal.pow_n9_value = m_sheetSegment.GetDataDouble(RowNo, 13);

                        //@10ports StartIndexNFset = 15  //@6ports StartIndexNFset = 12
                        stTotal.del_value = m_sheetSegment.GetDataDouble(RowNo, CalColmnIndexNFset);
                        stTotal.swp_value = (e_SweepMode)m_sheetSegment.GetDataInt(RowNo, CalColmnIndexNFset + 1);
                        stTotal.time_value = m_sheetSegment.GetDataDouble(RowNo, CalColmnIndexNFset + 2);
                        st[ChannelNumber].SegmentData[TotalSegment] = stTotal;
                        TotalSegment++;
                        if (TotalSegment == st[ChannelNumber].segm) SegmentTableSettings = false;
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
            }
            catch (Exception e)
            {
                MessageBox.Show("channel number is " + ChannelNumber.ToString());
                //throw;
            }

            ////////////////////////////////////////////Seoul

            //SParamData[ChannelNumber].NoPoints = TotalPoints;
            return st;
        }

        private void CreateArray(int totalChannel)
        {
            TraceMatch = new s_TraceMatching[totalChannel];
            SParamData = new S_Param[totalChannel]; //Freq, num of points, num of ports, sparam enable, total trace count, s param data - this line is levels deep with structs
            SBalanceParamData = new S_CMRRnBal_Param[totalChannel];
            for (int i = 0; i < totalChannel; i++)
            {
                TraceMatch[i] = new s_TraceMatching();
                SParamData[i] = new S_Param();
                SBalanceParamData[i] = new S_CMRRnBal_Param();
            }
        }

        private S_ParamData[] CreateArray2(int size)
        {
            S_ParamData[] spData = new S_ParamData[size];
            for (int i = 0; i < size; i++)
            {
                spData[i] = new S_ParamData();
            }
            return spData;
        }


    }


}
