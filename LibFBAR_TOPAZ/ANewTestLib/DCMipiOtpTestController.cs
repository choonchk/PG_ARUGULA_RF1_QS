using System;
using System.Collections.Generic;
using System.Threading;
using EqLib;
using InstrLib;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;

namespace LibFBAR_TOPAZ.ANewTestLib
{
    public class DCMipiOtpTestController
    {
        #region "Declarations"

        public int DC_PS_Set;
        public int ChannelNumber;
        /// <summary>
        /// Para.Spec.
        /// </summary>
        public string ParameterHeader;
        public string NParameterClass;

        public int Sleep_ms;
        public int TestNo;
        /// <summary>
        /// Misc. Use to toggle result enable.
        /// </summary>
        public bool Ignore_Read;
        public string PowerMode;
        public string Band;
        public int TotalChannel;
        public List<PowerSupplyDCSettingDataType> DC_Setting;
        public List<EqLib.MipiSyntaxParser.ClsMIPIFrame> MipiCommands_NA =
            new List<EqLib.MipiSyntaxParser.ClsMIPIFrame>();
        /// <summary>
        /// Header string. Can consist of N-Parameter Class: READ_CURRENT_LEAKAGE, POWER_MODE.
        /// </summary>
        public string Header;
        /// <summary>
        /// output of measured result.
        /// </summary>
        public s_Result SaveResult;

        public List<int> ChannelNumberList { get; set; }

        //ChoonChin - For Q current
        private DCTest m_modelDc;
        private EqHSDIO.EqHSDIObase m_eqHsdio;

        private ManualResetEvent _mre;

        //ChoonChin (20191206) - Added Switching port
        public string Switch_IN;
        public string Switch_ANT;
        public string Switch_OUT;

        #endregion

        public DCMipiOtpTestController()
        {
            ChannelNumberList = new List<int>();
        }

        public void Initialize(List<PowerSupplyDCMatchDataType> fixedDcMatching, Dictionary<string, EqDC.iEqDC> equipmentDcNiSmuList, 
            EqHSDIO.EqHSDIObase eqHsdio)
        {
            m_modelDc = new DCTest();
            // Make sure total channel count is DCMatching count and is listDcSet count.
            m_modelDc.Initialize(TotalChannel, fixedDcMatching, DC_Setting, equipmentDcNiSmuList);
            m_eqHsdio = eqHsdio;
            SaveResult = new s_Result();

        }

        public void InitializeTest()
        {
            m_modelDc.ForceVoltage(Header);

            m_modelDc.Set_Bias(DC_PS_Set);
        }

        /// <summary>
        /// Joker version.
        /// </summary>
        public void RunTest()
        {
            m_modelDc.ForceVoltage("Vbatt", 0, 0.1);
            m_modelDc.ForceVoltage("Vcc", 0, 0.1);
            m_modelDc.ForceVoltage("Vdd", 0, 0.1);

            Thread.Sleep(1);

            m_modelDc.Set_Bias(DC_PS_Set);


            string pHeader = ParameterHeader.ToUpper();
            if (pHeader.Contains("TEMP"))
            {
                int Temp = 0;
                HSDIO.Instrument.SendVectorTEMP(ref Temp);

                string Hex = Convert.ToString(Convert.ToInt32(Temp), 16).ToUpper();
                int TempCount = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
                float RsltData = (TempCount - 1) * (Convert.ToSingle(150f / 254f)) - 20;
                SetResult("MIMI_TEMP1", RsltData);
            }
            else if (pHeader.Contains("LEAKAGE"))
            {
                HsdioSendNextVectorsNA();
                List<s_mRslt> r = m_modelDc.Read_Bias(DC_PS_Set, Header);
                SetResult(r);
            }

            //ChoonChin - For Icq test
            else if (pHeader.Contains("IDLECURRENT_LNA"))
            {
                HsdioSendNextVectorsNA();
                //  Thread.Sleep(200);
                s_mRslt r = m_modelDc.Read_Bias_Icq(DC_PS_Set, Band);
                SetResult2(Header, r);
            }
            //Cheeon - For Ibiast test
            else if (pHeader.Contains("IDLECURRENT_BIAST"))
            {
                HsdioSendNextVectorsNA();
                s_mRslt r = m_modelDc.Read_BiasT_Icq(DC_PS_Set, PowerMode);
                SetResult2(Header, r);
            }
            //Cheeon - For Icpl test
            else if (pHeader.Contains("IDLECURRENT_CPL"))
            {
                HsdioSendNextVectorsNA();
                s_mRslt r = m_modelDc.Read_CPL_Icq(DC_PS_Set);
                SetResult2(Header, r);
            }
            else
            {
                HsdioSendNextVectorsNA();
            }

            if (!Ignore_Read)
            {
                // Reset to zero, then set new result. Actually no longer needed.
                MeasureResult();
                // Actually SetResult() no longer needed.
                //SetResult();
            }

            //Thread.Sleep(1000); //ADDED AS AN EXPERIMENT 05232017
        }

        /// <summary>
        /// HLS2 version. Run Dc Mipi Otp Test.
        /// </summary>
        public void RunTest2()
        {
            string nParamClass = NParameterClass.ToUpper();

            List<s_mRslt> resultArray = RunDcLeakageTest(nParamClass, m_eqHsdio, MipiCommands_NA);
            bool isExecuted = resultArray != null;
            if (isExecuted)
            {
                SetResult(resultArray);
                return;
            }
            s_mRslt resultMulti = RunDcTest(nParamClass, m_eqHsdio, MipiCommands_NA);
            isExecuted = !String.IsNullOrEmpty(resultMulti.Result_Header);
            if (isExecuted)
            {
                SetResult(resultMulti);
                return;
            }
            resultArray = RunReadCurrent(nParamClass, m_eqHsdio, MipiCommands_NA, ChannelNumberList);
            isExecuted = resultArray != null;
            if (isExecuted)
            {
                SetResult(resultArray);
                return;
            }

            MipiTest m_modelMipi = new MipiTest();
            s_Result rSingle = m_modelMipi.RunTest(nParamClass, m_eqHsdio, PowerMode, MipiCommands_NA);
            isExecuted = !String.IsNullOrEmpty(rSingle.Result_Header);
            if (isExecuted)
            {
                SetResult(rSingle);
                return;
            }

            OtpTest m_modelOtp = new OtpTest();
            rSingle = m_modelOtp.RunTest(nParamClass, m_eqHsdio);
            isExecuted = !String.IsNullOrEmpty(rSingle.Result_Header);
            if (isExecuted)
            {
                SetResult(rSingle);
                return;
            }

            // If not executed.
            m_modelMipi.SendMipiCommandTest0(TestNo, m_eqHsdio, MipiCommands_NA);
        }

        public void UnInitTest()
        {
            // Actually no longer needed. Supposed to execute this before returning.
            if (!Ignore_Read)
            {
                // Reset to zero, then set new result.
                MeasureResult();
                // Actually SetResult() no longer needed.
                //SetResult();
            }
        }
        private List<s_mRslt> RunDcLeakageTest(string nParamClass,
            EqHSDIO.EqHSDIObase eqHsdio,
            List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands)
        {
            List<s_mRslt> r = null;

            switch (nParamClass)
            {
                case "LEAKAGE":
                    SendMipiCommand(eqHsdio, zMIPI_Commands);
                    //  Thread.Sleep(200);
                    r = m_modelDc.Read_Bias(DC_PS_Set, Header);
                    break;
                default:
                    break;
            }

            return r;
        }

        private s_mRslt RunDcTest(string nParamClass, EqHSDIO.EqHSDIObase eqHsdio,
            List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands)
        {
            s_mRslt r2 = new s_mRslt();

            switch (nParamClass)
            {
                case "IDLECURRENT_LNA":
                    SendMipiCommand(eqHsdio, zMIPI_Commands);
                    r2 = m_modelDc.Read_Bias_Icq(DC_PS_Set, Band);
                    SetResult(Header, ref r2);
                    break;
                case "IDLECURRENT_BIAST":
                    SendMipiCommand(eqHsdio, zMIPI_Commands);
                    r2 = m_modelDc.Read_BiasT_Icq(DC_PS_Set, PowerMode);
                    SetResult(Header, ref r2);

                    break;
                case "IDLECURRENT_CPL":
                    SendMipiCommand(eqHsdio, zMIPI_Commands);
                    r2 = m_modelDc.Read_CPL_Icq(DC_PS_Set);
                    SetResult(Header, ref r2);
                    break;
                // Obsolete the single read read_Current.
                //case "READ_CURRENT":
                //    SendMipiCommand(eqHsdio, zMIPI_Commands);
                //    //  Thread.Sleep(200);
                //    r2 = m_modelDc.Read_Bias(DC_PS_Set, ChannelNumber, Header);
                //    string cnName = (m_modelDc.DC_matching[ChannelNumber].Address).Replace("V", "I");
                //    string headerrc =
                //        string.Format("F_{0}_x_{1}_x_x_x_x_{2}_x_x_x_x_x_x_x_x_{3}_NOTE_x", cnName, Band, PowerMode, ParameterHeader);
                //    SetResult(headerrc, ref r2);
                //    break;
                case "READ_CURRENT_LEAKAGE":
                    SendMipiCommand(eqHsdio, zMIPI_Commands);

                    Thread.Sleep(Sleep_ms);
                    r2 = m_modelDc.Read_Bias(DC_PS_Set, ChannelNumber, Header);
                    string cnName2 = (m_modelDc.DC_matching[ChannelNumber].Address).Replace("V", "I");
                    string headerrcl =
                        string.Format("F_{0}_x_{1}_x_x_x_x_{2}_x_x_x_x_x_x_x_x_{3}_NOTE_x", cnName2, Band, PowerMode, ParameterHeader);
                    SetResult(headerrcl, ref r2);

                    break;
                default:
                    break;
            }

            return r2;
        }

        private List<s_mRslt> RunReadCurrent(string nParamClass, EqHSDIO.EqHSDIObase eqHsdio, List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands, List<int> channelList)
        {
            bool isReadCurrent = nParamClass == "READ_CURRENT";
            string headerrc = "";
            if (!isReadCurrent) return null;

            List<s_mRslt> rList = new List<s_mRslt>();

            SendMipiCommand(eqHsdio, zMIPI_Commands);

            foreach (int channelNumber in channelList)
            {
                s_mRslt r = m_modelDc.Read_Bias(DC_PS_Set, channelNumber, Header);
                string cnName = (m_modelDc.DC_matching[channelNumber].Address).Replace("V", "I");
                headerrc = string.Format("F_{0}_Q_{1}_{4}_{2}_{3}", cnName, Band, PowerMode, ParameterHeader, Switch_ANT);
                //headerrc = string.Format("F_{0}_Q_{1}_{4}_x_x_x_x_{2}_x_x_x_x_x_x_x_x_{3}_NOTE_x", cnName, Band, PowerMode, ParameterHeader, Switch_ANT);
                

                //ChoonChin(20191206) - To modify q-current header as RF1, to be enable in Pinot EVT
                if (false)
                {                    
                    string VccValue, VddValue = "0.0";
                    VccValue = (m_modelDc.DC_Setting[0].Voltage).ToString();
                    VddValue = (m_modelDc.DC_Setting[2].Voltage).ToString();

                    string Port1 = "x";
                    string Port2 = "x";
                    string Mipi1 = "00";
                    string Mipi2 = "00";

                    if (PowerMode.Contains("HPM")) //TX
                    {
                        Mipi1 = zMIPI_Commands[8].Data_hex;
                        Mipi2 = zMIPI_Commands[9].Data_hex;
                        Port1 = Switch_IN;
                        Port2 = Switch_ANT;
                    }
                    if (PowerMode.Contains("G")) ///RX
                    {
                        int StartIndex = 0;
                        for (StartIndex = 0; StartIndex <= zMIPI_Commands.Count; StartIndex++)
                        {
                            if (zMIPI_Commands[StartIndex].Pair == 2 && (zMIPI_Commands[StartIndex].Register_hex == "0B" || zMIPI_Commands[StartIndex].Register_hex == "B"))
                            {
                                break;
                            }
                        }

                        for (int i = StartIndex; i < (StartIndex + 6); i++)
                        {
                            if (zMIPI_Commands[i].Data_hex != "0")
                            {
                                Mipi1 = zMIPI_Commands[i].Data_hex;
                                break;
                            }
                        }
                        Mipi2 = "00" ;
                        Port1 = Switch_ANT;
                        Port2 = Switch_OUT;
                    }

                    headerrc = 
                        string.Format("F_{0}_Q_{1}_{2}_x_x_x_x_x_{3}Vcc_{4}Vdd_0x{5}_0x{6}_x_{7}_{8}_{9}_NOTE_x", cnName, Band, PowerMode, 
                        VccValue, VddValue, Mipi1, Mipi2, Port1, Port2, ParameterHeader);
                }
                //
                
                SetResult(headerrc, ref r);
                rList.Add(r);
            }

            return rList;
        }

        private void SetResult(List<s_mRslt> resultList)
        {
            SaveResult.AddMultiResult(resultList);
        }

        private void SetResult(string header, double resultValue)
        {
            SaveResult.SetValue(header, resultValue);
        }

        private void SetResult(string header, ref s_mRslt res)
        {
            res.Result_Header = header;
        }

        /// <summary>
        /// Joker version.
        /// </summary>
        private void SetResult2(string header, s_mRslt res)
        {
            res.Result_Header = header;
            SaveResult.AddMultiResult(res);
        }

        private void SetResult(s_Result res)
        {
            SaveResult = res;
        }

        private void SetResult(s_mRslt res)
        {
            SaveResult.AddMultiResult(res);
        }

        public void ClearResult()
        {
            SaveResult.Clear_Results();
        }

        private void HsdioSendNextVectorsNA()
        {
            if (HSDIO.useScript == true)
            {
                if (TestNo == 0)
                {
                    HSDIO.Instrument.SendNextVectorsNA();
                }

                HSDIO.Instrument.SendNextVectorsNA();
            }
        }

        private void SendMipiCommand(EqHSDIO.EqHSDIObase eqHsdio,
            List<EqLib.MipiSyntaxParser.ClsMIPIFrame> zMIPI_Commands)
        {
            eqHsdio.SendMipiCommands(zMIPI_Commands);
            eqHsdio.SendMipiCommands(zMIPI_Commands);
        }

        /// <summary>
        /// Reset to zero. Unused.
        /// </summary>
        private void MeasureResult()
        {
            //   Read_Bias(DC_PS_Set);
            //for (int iRslt = 0; iRslt < m_modelDc.TotalChannel; iRslt++)
            //{
            //    SaveResult.Multi_Results[iRslt].Result_Data = m_modelDc.Get_Result_Array[iRslt];
            //}
        }

        /// <summary>
        /// Called by Test_Threading().
        /// </summary>
        /// <param name="State"></param>
        public void CallBack(object State)
        {
            RunTest();
            _mre.Set();
        }

        /// <summary>
        /// Unused.
        /// </summary>
        public void InitSettings()
        {
            //rtnResult = m_modelDc.Init_Bias_Array2(Ignore_Read);
            //SaveResult.Enable = !Ignore_Read;
            //SaveResult.b_MultiResult = !Ignore_Read;
        }

        public void InitSettings_Pxi()
        {
        }
    }
}