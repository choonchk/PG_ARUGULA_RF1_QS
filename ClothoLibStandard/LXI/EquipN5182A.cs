using System;
using System.Windows.Forms;
using Ivi.Visa.Interop;

namespace ClothoLibStandard
{
    public class EquipSG
    {

        FormattedIO488 myVisaSg = null;
        public EquipSG(FormattedIO488 _myVisaSg)
        {
            myVisaSg = _myVisaSg;
        }
        ~EquipSG() { }

        public void INITIALIZATION()
        {
            try
            {
                myVisaSg.WriteString("*IDN?", true);
                string result = myVisaSg.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception("EquipN5182A: Initialization -> " + ex.Message);
            }

        }
        public void RESET()
        {
            try
            {
                myVisaSg.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("EquipN5182A: RESET -> " + ex.Message);
            }
        }
        public void OUTPUT_STATE(INSTR_OUTPUT _ONOFF)
        {
            myVisaSg.WriteString("OUTP:STATE " + _ONOFF, true);

        }
        public void OUTPUT_MODULATION(INSTR_OUTPUT _ONOFF)
        {
            myVisaSg.WriteString("OUTP:MOD " + _ONOFF, true);
        }
        
        public bool OPERATION_COMPLETE()
        {
            try
            {
                bool _complete = false;
                double _dummy = -99;
                do
                {
                    _dummy = WRITE_READ_DOUBLE("*OPC?");
                } while (_dummy == 0);
                _complete = true;
                return _complete;

            }
            catch (Exception ex)
            {
                throw new Exception("EquipN5182A: OPERATION_COMPLETE -> " + ex.Message);
            }
        }

        public void SET_OUTPUT_POWER(float varOutputPower)
        {
            myVisaSg.WriteString(":POW " + varOutputPower.ToString(), true);
        }
        public void SET_AMPLITUDE_DBm(double _amplitude)
        {
            myVisaSg.WriteString("POWER " + _amplitude.ToString() + "DBm", true);
        }
        public void SET_FREQUENCY(double _MHz)
        {
            myVisaSg.WriteString("FREQ " + _MHz.ToString() + "MHz", true);
        }
        public void SET_FREQUENCY(float _MHz)
        {
            myVisaSg.WriteString("FREQ " + _MHz.ToString() + "MHz", true);
        }
        public void SET_AMPLITUDE_OFFSET_DB(double _offsetamp)
        {
            myVisaSg.WriteString("POW:OFFS " + _offsetamp.ToString() + "DB", true);
        }

        public void SET_POWER_MODE(N5182_POWER_MODE _mode)
        {
            myVisaSg.WriteString(":SOUR:POW:MODE " + _mode.ToString(), true);
        }
        public void SET_FREQUENCY_MODE(N5182_FREQUENCY_MODE _mode)
        {
            myVisaSg.WriteString(":SOUR:FREQ:MODE " + _mode.ToString(), true);
        }

        public string QUERY_ERROR()
        {
            string ErrMsg, TempErrMsg = "";
            int ErrNum;
            try
            {
                ErrMsg = WRITE_READ_STRING("SYST:ERR?");
                TempErrMsg = ErrMsg;
                // Remove the error number
                ErrNum = Convert.ToInt16(ErrMsg.Remove((ErrMsg.IndexOf(",")),
                    (ErrMsg.Length) - (ErrMsg.IndexOf(","))));
                if (ErrNum != 0)
                {
                    while (ErrNum != 0)
                    {
                        TempErrMsg = ErrMsg;

                        // Check for next error(s)
                        ErrMsg = WRITE_READ_STRING("SYST:ERR?");

                        // Remove the error number
                        ErrNum = Convert.ToInt16(ErrMsg.Remove((ErrMsg.IndexOf(",")),
                            (ErrMsg.Length) - (ErrMsg.IndexOf(","))));
                    }
                }
                return TempErrMsg;
            }
            catch (Exception ex)
            {
                throw new Exception("EquipN5182A: QUERY_ERROR --> " + ex.Message);
            }
        }

        public void MOD_FORMAT(string strWaveformName)
        {
            myVisaSg.WriteString(":MEM:COPY \"" + strWaveformName + "@NVWFM\",\"" + strWaveformName + "@WFM1\"" + ";*OPC", true);
            bool result = OPERATION_COMPLETE();

            myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:" + strWaveformName + "\"" + ";", true);
            result = OPERATION_COMPLETE();

            myVisaSg.WriteString(":RAD:ARB ON", true);

            myVisaSg.WriteString(":OUTP:MOD ON", true);
        }
        public void MOD_FORMAT_WITH_LOADING_CHECK(string strWaveformName, bool WaveformInitalLoad)
        {
            //Stopwatch Speedo = new Stopwatch();
            //float ElapsedTime = 0;
            int totalError = 0;

            while (true)
            {
                if (WaveformInitalLoad)
                {
                    

                    // myVisaSg.WriteString(":MEM:COPY \"NVWFM:" + split[3] + "\",\"WFM1:" + split[3] + "\"" + ";*OPC", true);
                    myVisaSg.WriteString(":MEM:COPY \"" + strWaveformName + "@NVWFM\",\"" + strWaveformName + "@WFM1\"", true);
                    
                    // string result = funcSerialPoll(myVisaSg);
                    //result = myLibMeas.funcSerialPoll(myVisaSg);
                }

                //Speedo.Reset();
                //Speedo.Start();

                myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:" + strWaveformName + "\"", true);
                // result = funcSerialPoll(myVisaSg);
                //result = myLibMeas.funcSerialPoll(myVisaSg);


                //Speedo.Stop();
                //ElapsedTime = Speedo.ElapsedMilliseconds;

                //Speedo.Reset();
                //Speedo.Start();

                myVisaSg.WriteString(":RAD:ARB ON;:OUTP:MOD ON", true);

                // Error check
                //myVisaSg.WriteString("SYST:ERR?", true);
                //string ret = myVisaSg.ReadString();
                //string[] arrEquipError = ret.Split(',');
                //if (arrEquipError.Length > 1)
                //{
                //    string strEquipErrorMessageNumber = arrEquipError[0].Trim();
                //    int ErrorNumber = Convert.ToInt32(strEquipErrorMessageNumber);
                //    //strEquipErrorMessage = arrEquipError[1].Trim();

                //    //if (strEquipErrorMessageNumber == "+0")
                //    //    return true;
                //    //else
                //    //{
                //    //    UpdateStatusText("Failure: #" + strEquipErrorMessageNumber + " : (" + strEquipErrorMessage + ")");
                //    //    return false;
                //    //}

                //    if (ErrorNumber != 0)
                //    {
                //        totalError++;
                //        if (totalError == 10)
                //            MessageBox.Show("SG generated an error during waveform loading.");
                //        break;
                //        // MessageBox.Show("SG generated an error during waveform loading.");
                //    }
                //    else
                //    {
                //        //Speedo.Stop();
                //        //ElapsedTime = Speedo.ElapsedMilliseconds;                    
                break;
                //    }
                //}


            }
        }
        public void MARKER_BLANKING(String _MarkerNum)
        {
            myVisaSg.WriteString(":RAD:ARB:MDES:PULS " + _MarkerNum, true);
        }

        public void SELECT_WAVEFORM(N5182A_WAVEFORM_MODE _MODE)
        {
            switch (_MODE)
            {
                case N5182A_WAVEFORM_MODE.CDMA2K:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:CDMA2K-WFM1\"", true);    break;
                case N5182A_WAVEFORM_MODE.CDMA2K_RC1:   myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:CDMA2K_RC1_20100316\"", true); break;
                case N5182A_WAVEFORM_MODE.GSM850:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK850\"", true);        break;
                case N5182A_WAVEFORM_MODE.GSM900:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK900\"", true);        break;
                case N5182A_WAVEFORM_MODE.GSM1800:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1800\"", true);       break;
                case N5182A_WAVEFORM_MODE.GSM1900:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1900\"", true);       break;
                case N5182A_WAVEFORM_MODE.GSM850A:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GSM850A\"", true);       break;
                case N5182A_WAVEFORM_MODE.GSM900A:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK900A\"", true);       break;
                case N5182A_WAVEFORM_MODE.GSM1800A:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1800A\"", true);      break;
                case N5182A_WAVEFORM_MODE.GSM1900A:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1900A\"", true);      break;
                case N5182A_WAVEFORM_MODE.HSDPA:        myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:HSDPA_UL\"", true);       break;
                case N5182A_WAVEFORM_MODE.HSUPA_TC3:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_UPLINK_HSUPA_TC3\"", true); break;                
                case N5182A_WAVEFORM_MODE.IS95A:        myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:IS95A_RE-WFM1\"", true);  break;
                case N5182A_WAVEFORM_MODE.IS98:         myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:IS98_WFM\"", true);       break;
                case N5182A_WAVEFORM_MODE.WCDMA:            myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_1DPCH_WFM\"", true);break;
                case N5182A_WAVEFORM_MODE.WCDMA_UL:         myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_UL\"", true); break;
                case N5182A_WAVEFORM_MODE.WCDMA_GTC1:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_GTC1_20100208A\"", true); break;
                case N5182A_WAVEFORM_MODE.WCDMA_GTC3:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_GTC3_20100726A\"", true); break;
                case N5182A_WAVEFORM_MODE.WCDMA_GTC4:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_UPLINK_GTC4\"", true); break;
                case N5182A_WAVEFORM_MODE.WCDMA_GTC1_NEW:   myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:WCDMA_GTC1_NEW_20101111\"", true); break;
                case N5182A_WAVEFORM_MODE.EDGE850:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE850\"", true);        break;
                case N5182A_WAVEFORM_MODE.EDGE900:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE900\"", true);        break;
                case N5182A_WAVEFORM_MODE.EDGE1800:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE1800\"", true);       break;
                case N5182A_WAVEFORM_MODE.EDGE1900:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE1900\"", true);       break;
                case N5182A_WAVEFORM_MODE.EDGE850A:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE850A\"", true);       break;
                case N5182A_WAVEFORM_MODE.EDGE900A:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE900A\"", true);       break;
                case N5182A_WAVEFORM_MODE.EDGE1800A:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE1800A\"", true);      break;
                case N5182A_WAVEFORM_MODE.EDGE1900A:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE1900A\"", true);      break;                
                case N5182A_WAVEFORM_MODE.LTE5M8RB:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_5M8RB_20091202\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M8RB17S:  myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_QPSK_5M8RB17S\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M25RB:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_5M25RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M1RB:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_QPSK_10M1RB\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M1RB49S: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_QPSK10M1RB49S\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M12RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_10M12RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M12RB19S:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK10M12RB19S_1220\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M12RB38S:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_10M12RB38S\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M48RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_10M48RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M50RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_10M50RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M20RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_QPSK_10M20RB\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE15M75RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_15M75RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE15M18RB57S:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK15M18RB57S_1025\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M100RB:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_20M100RB091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M18RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_20M18RB_100408\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M48RB:       myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_QPSK_20M48RB_091215\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M12RB_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_10M12RB_ST0_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE10M12RB38S_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_10M12RB_ST38_M6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M25RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_5M25RB_ST0_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M8RB17S_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_5M8RB_ST17_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE16QAM5M8RB17S: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_16QAM_5M8RB17S\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M25RB_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_5M25RB_ST0_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M1RB:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_QPSK_5M1RB\"", true); break;
                
                case N5182A_WAVEFORM_MODE.LTE10M50RB_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_10M50RB_ST0_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M18RB_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_20M18RB0S_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M18RB82S_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_20M18RB82S_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE20M100RB_MCS2: myVisaSg.WriteString(":RAD:ARB:WAV \"LTEFUQ_20M100RB0S_MCS2\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE15M16RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_15M16RB0S_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE15M16RB59S_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFUQ_15M16RB59S_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE15M75RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"LTEFUQ_15M75RB0S_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M8RB_MCS6: myVisaSg.WriteString(":RAD:ARB:WAV \"LTEFUQ_5M8RB_ST0_MCS6\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE5M8RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"LTEFUQ_5M8RB_ST0_MCS5\"", true); break;

                case N5182A_WAVEFORM_MODE.LTE1P4M5RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_1P4M5RB_ST0_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE1P4M5RB1S_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_1P4M5RB_ST1_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE3M4RB_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_3M4RB_ST0_MCS5\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE3M4RB11S_MCS5: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTEFU_3M4RB_ST11_MCS5\"", true); break;

                case N5182A_WAVEFORM_MODE.LTE16QAM5M25RB: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:LTE_16QAM_5M25RB\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE16QAM10M50RB: myVisaSg.WriteString(":RAD:ARB:WAV \"LTE_16QAM_10M50RB_0213\"", true); break;
                case N5182A_WAVEFORM_MODE.LTE16QAM5M8RB: myVisaSg.WriteString(":RAD:ARB:WAV \"LTE_16QAM_5M8RB\"", true); break;

                case N5182A_WAVEFORM_MODE.GMSK900:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK900\"", true); break;
                case N5182A_WAVEFORM_MODE.GMSK800:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK800\"", true); break;
                case N5182A_WAVEFORM_MODE.GMSK850:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK850\"", true); break;
                case N5182A_WAVEFORM_MODE.EDGE800:      myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE800\"", true); break;
                case N5182A_WAVEFORM_MODE.GMSK1700:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1700\"", true); break;
                case N5182A_WAVEFORM_MODE.GMSK1900:     myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GMSK1900\"", true); break;
                case N5182A_WAVEFORM_MODE.GMSK_TS01:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:GSM_TIMESLOT01_20100107\"", true); break;
                case N5182A_WAVEFORM_MODE.EDGE_TS01:    myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:EDGE_TS01_20100107\"", true); break;
                case N5182A_WAVEFORM_MODE.EVDO_4096: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:1XEVDO_REVA_TR4096_0816\"", true); break;
                case N5182A_WAVEFORM_MODE.EVDO_B: myVisaSg.WriteString(":RAD:ARB:WAV \"WFM1:1XEVDO_REVB_5MHZSEP_001\"", true); break;
                case N5182A_WAVEFORM_MODE.NONE:         break;
                default:                                        throw new Exception("Not suct a waveform!");
            }
        }

        public void WRITE(string _cmd)
        {
            myVisaSg.WriteString(_cmd, true);
        }
        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaSg.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaSg.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            myVisaSg.WriteString(_cmd, true);
            return myVisaSg.ReadString();
        }


    }
}
