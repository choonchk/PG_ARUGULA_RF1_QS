using System;
using System.Threading;
using Ivi.Visa.Interop;

namespace ClothoLibStandard
{
    public class EquipSA
    {
        FormattedIO488 myVisaSa = null;

        public EquipSA(FormattedIO488 _myVisaSa)
        {
            myVisaSa = _myVisaSa;
        }
        ~EquipSA() { }

        public static FormattedIO488 OpenIO(string aliasName)
        {
            ResourceManager grm = new ResourceManager();
            FormattedIO488 myVisaSa = new FormattedIO488();
            myVisaSa.IO = (IMessage)grm.Open(aliasName, AccessMode.NO_LOCK, 1000, "");
            myVisaSa.IO.Timeout = 10000;
            return myVisaSa;
        }


        public void INITIALIZATION(int EquipId)
        {
            //myVisaSa.WriteString("*CLS", true);
            myVisaSa.WriteString(":INST SA", true);
            //myVisaSa.WriteString("*RST", true);
            RESET();
            if (EquipId == 1)   // PSA
            {
                myVisaSa.WriteString("CORR:CSET1 OFF", true);
                myVisaSa.WriteString("CORR:CSET2 OFF", true);
                myVisaSa.WriteString("CORR:CSET3 OFF", true);
                myVisaSa.WriteString("CORR:CSET4 OFF", true);
                myVisaSa.WriteString("CORR:CSET:ALL OFF", true);
            }
            else if (EquipId == 3) // MXA
                myVisaSa.WriteString(":CORR:SA:GAIN 0", true);

            myVisaSa.WriteString(":FORM:DATA REAL,32", true);
            myVisaSa.WriteString(":DET AVER", true);
            myVisaSa.WriteString(":AVER:TYPE RMS", true);
            myVisaSa.WriteString(":INIT:CONT 1", true);
            myVisaSa.WriteString(":BWID:VID:RAT " + "10", true);
            myVisaSa.WriteString("SWE:POIN 101", true);

            // Alignment
            if (EquipId == 1)   // PSA
            {

            }
        }
        public void RESET()
        {
            try
            {
                myVisaSa.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("EquipSA: RESET -> " + ex.Message);
            }
        }

        public void PRESET()
        {
            myVisaSa.WriteString("SYST:PRES", true);
        }

        public void SELECT_INSTRUMENT(N9020A_INSTRUMENT_MODE _MODE)
        {
            switch (_MODE)
            {
                case N9020A_INSTRUMENT_MODE.SpectrumAnalyzer: myVisaSa.WriteString("INST:SEL SA", true); break;
                case N9020A_INSTRUMENT_MODE.Basic: myVisaSa.WriteString("INST:SEL BASIC", true); break;
                case N9020A_INSTRUMENT_MODE.Wcdma: myVisaSa.WriteString("INST:SEL WCDMA", true); break;
                case N9020A_INSTRUMENT_MODE.WIMAX: myVisaSa.WriteString("INST:SEL WIMAXOFDMA", true); break;
                case N9020A_INSTRUMENT_MODE.EDGE_GSM: myVisaSa.WriteString("INST:SEL EDGEGSM", true); break;
                default: throw new Exception("Not such a intrument mode!");
            }
        }
        public void SELECT_TRIGGERING(N9020A_TRIGGERING_TYPE _TYPE)
        {
            switch (_TYPE)
            {
                ///******************************************
                /// SweptSA mode trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.RF_Ext1: myVisaSa.WriteString("TRIG:RF:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.RF_Ext2: myVisaSa.WriteString("TRIG:RF:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.RF_RFBurst: myVisaSa.WriteString("TRIG:RF:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.RF_Video: myVisaSa.WriteString("TRIG:RF:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.RF_FreeRun: myVisaSa.WriteString("TRIG:RF:SOUR IMM", true); break;

                ///******************************************
                /// EDGEGSM Transmit power type trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.TXP_Ext1: myVisaSa.WriteString("TRIG:TXP:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.TXP_Ext2: myVisaSa.WriteString("TRIG:TXP:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.TXP_RFBurst: myVisaSa.WriteString("TRIG:TXP:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.TXP_Video: myVisaSa.WriteString("TRIG:TXP:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.TXP_FreeRun: myVisaSa.WriteString("TRIG:TXP:SOUR IMM", true); break;

                ///******************************************
                /// EDGEGSM Power Vs Time type trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.PVT_Ext1: myVisaSa.WriteString("TRIG:PVT:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.PVT_Ext2: myVisaSa.WriteString("TRIG:PVT:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.PVT_RFBurst: myVisaSa.WriteString("TRIG:PVT:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.PVT_Video: myVisaSa.WriteString("TRIG:PVT:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.PVT_FreeRun: myVisaSa.WriteString("TRIG:PVT:SOUR IMM", true); break;

                ///******************************************
                /// EDGEGSM Power Vs Time type trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.EPVT_Ext1: myVisaSa.WriteString("TRIG:EPVT:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.EPVT_Ext2: myVisaSa.WriteString("TRIG:EPVT:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.EPVT_RFBurst: myVisaSa.WriteString("TRIG:EPVT:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.EPVT_Video: myVisaSa.WriteString("TRIG:EPVT:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.EPVT_FreeRun: myVisaSa.WriteString("TRIG:EPVT:SOUR IMM", true); break;

                ///******************************************
                /// EDGEGSM Edge Power Vs Time type trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.EORFS_Ext1: myVisaSa.WriteString("TRIG:EORF:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.EORFS_Ext2: myVisaSa.WriteString("TRIG:EORF:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.EORFS_RFBurst: myVisaSa.WriteString("TRIG:EORF:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.EORFS_Video: myVisaSa.WriteString("TRIG:EORF:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.EORFS_FreeRun: myVisaSa.WriteString("TRIG:EORF:SOUR IMM", true); break;

                ///******************************************
                /// EDGEGSM Edge EVM type trigerring
                ///******************************************
                case N9020A_TRIGGERING_TYPE.EEVM_Ext1: myVisaSa.WriteString("TRIG:EEVM:SOUR EXT1", true); break;
                case N9020A_TRIGGERING_TYPE.EEVM_Ext2: myVisaSa.WriteString("TRIG:EEVM:SOUR EXT2", true); break;
                case N9020A_TRIGGERING_TYPE.EEVM_RFBurst: myVisaSa.WriteString("TRIG:EEVM:SOUR RFB", true); break;
                case N9020A_TRIGGERING_TYPE.EEVM_Video: myVisaSa.WriteString("TRIG:EEVM:SOUR VID", true); break;
                case N9020A_TRIGGERING_TYPE.EEVM_FreeRun: myVisaSa.WriteString("TRIG:EEVM:SOUR IMM", true); break;
                default: throw new Exception("Not such a Trigger Mode!");
            }

        }
        public void SELECT_RADIO_STD_BAND(N9020A_RADIO_STD_BAND _BAND)
        {
            switch (_BAND)
            {
                case N9020A_RADIO_STD_BAND.EGSM: myVisaSa.WriteString(":RAD:STAN:BAND EGSM", true); break;
                case N9020A_RADIO_STD_BAND.PGSM: myVisaSa.WriteString(":RAD:STAN:BAND PGSM", true); break;
                case N9020A_RADIO_STD_BAND.RGSM: myVisaSa.WriteString(":RAD:STAN:BAND RGSM", true); break;
                case N9020A_RADIO_STD_BAND.DCS1800: myVisaSa.WriteString(":RAD:STAN:BAND DCS1800", true); break;
                case N9020A_RADIO_STD_BAND.PCS1900: myVisaSa.WriteString(":RAD:STAN:BAND PCS1900", true); break;
                case N9020A_RADIO_STD_BAND.GSM450: myVisaSa.WriteString(":RAD:STAN:BAND GSM450", true); break;
                case N9020A_RADIO_STD_BAND.GSM480: myVisaSa.WriteString(":RAD:STAN:BAND GSM480", true); break;
                case N9020A_RADIO_STD_BAND.GSM700: myVisaSa.WriteString(":RAD:STAN:BAND GSM700", true); break;
                case N9020A_RADIO_STD_BAND.GSM850: myVisaSa.WriteString(":RAD:STAN:BAND GSM850", true); break;
                default: throw new Exception("Not such a RADIO_STD_BAND!");
            }

        }
        public void SELECT_RADIO_STANDARD(N9020A_RAD_STD _STANDARD)
        {
            switch (_STANDARD)
            {
                case N9020A_RAD_STD.NONE: myVisaSa.WriteString(":RAD:STAN:SEL NONE", true); break;
                case N9020A_RAD_STD.IS95A: myVisaSa.WriteString(":RAD:STAN:SEL IS95a", true); break;
                case N9020A_RAD_STD.JSTD: myVisaSa.WriteString(":RAD:STAN:SEL JSTD", true); break;
                case N9020A_RAD_STD.IS97D: myVisaSa.WriteString(":RAD:STAN:SEL IS97D", true); break;
                case N9020A_RAD_STD.GSM: myVisaSa.WriteString(":RAD:STAN:SEL GSM", true); break;
                case N9020A_RAD_STD.W3GPP: myVisaSa.WriteString(":RAD:STAN:SEL W3GPP", true);
                    myVisaSa.WriteString(":RAD:STAN:DEV MS", true); break;
                case N9020A_RAD_STD.C2000MC1: myVisaSa.WriteString(":RAD:STAN:SEL C2000MC1", true); break;
                case N9020A_RAD_STD.C20001X: myVisaSa.WriteString(":RAD:STAN:SEL C20001X", true); break;
                case N9020A_RAD_STD.Nadc: myVisaSa.WriteString(":RAD:STAN:SEL NADC", true); break;
                case N9020A_RAD_STD.Pdc: myVisaSa.WriteString(":RAD:STAN:SEL PDC", true); break;
                case N9020A_RAD_STD.BLUEtooth: myVisaSa.WriteString(":RAD:STAN:SEL BLUEtooth", true); break;
                case N9020A_RAD_STD.TETRa: myVisaSa.WriteString(":RAD:STAN:SEL TETRa", true); break;
                case N9020A_RAD_STD.WL802DOT11A: myVisaSa.WriteString(":RAD:STAN:SEL WL802DOT11A", true); break;
                case N9020A_RAD_STD.WL802DOT11B: myVisaSa.WriteString(":RAD:STAN:SEL WL802DOT11B", true); break;
                case N9020A_RAD_STD.WL802DOT11G: myVisaSa.WriteString(":RAD:STAN:SEL WL802DOT11G", true); break;
                case N9020A_RAD_STD.HIPERLAN2: myVisaSa.WriteString(":RAD:STAN:SEL HIPERLAN2", true); break;
                case N9020A_RAD_STD.DVBTLSN: myVisaSa.WriteString(":RAD:STAN:SEL DVBTLSN", true); break;
                case N9020A_RAD_STD.DVBTGPN: myVisaSa.WriteString(":RAD:STAN:SEL DVBTGPN", true); break;
                case N9020A_RAD_STD.DVBTIPN: myVisaSa.WriteString(":RAD:STAN:SEL DVBTIPN", true); break;
                case N9020A_RAD_STD.FCC15: myVisaSa.WriteString(":RAD:STAN:SEL FCC15", true); break;
                case N9020A_RAD_STD.SDMBSE: myVisaSa.WriteString(":RAD:STAN:SEL SDMBSE", true); break;
                case N9020A_RAD_STD.UWBINDOOR: myVisaSa.WriteString(":RAD:STAN:SEL UWBINDOOR", true); break;
                default: throw new Exception("Not such a Radio Standard!");
            }
        }
        public void ACP_METHOD(N9020A_ACP_METHOD _METHOD)
        {
            switch (_METHOD)
            {
                case N9020A_ACP_METHOD.IBW: myVisaSa.WriteString(":ACP:METH IBW", true); break;
                case N9020A_ACP_METHOD.IBWRange: myVisaSa.WriteString(":ACP:METH IBWRange", true); break;
                case N9020A_ACP_METHOD.FAST: myVisaSa.WriteString(":ACP:METH FAST", true); break;
                case N9020A_ACP_METHOD.RBW: myVisaSa.WriteString(":ACP:METH RBW", true); break;
                default: throw new Exception("Not such a ACP Method!");
            }

        }
        public void MEASURE_SETUP(N9020A_MEAS_TYPE _TYPE)
        {
            switch (_TYPE)
            {
                case N9020A_MEAS_TYPE.SweptSA: myVisaSa.WriteString(":INIT:SAN", true); break;
                case N9020A_MEAS_TYPE.ChanPower: myVisaSa.WriteString(":INIT:CHP", true); break;
                case N9020A_MEAS_TYPE.ACP: myVisaSa.WriteString(":CONF:ACP:NDEF", true); break;
                case N9020A_MEAS_TYPE.BTxPow: myVisaSa.WriteString(":INIT:TXP", true); break;
                case N9020A_MEAS_TYPE.GPowVTM: myVisaSa.WriteString(":INIT:PVT", true); break;
                case N9020A_MEAS_TYPE.GPHaseFreq: myVisaSa.WriteString(":INIT:PFER", true); break;
                case N9020A_MEAS_TYPE.GOutRFSpec: myVisaSa.WriteString(":INIT:ORFS", true); break;
                case N9020A_MEAS_TYPE.GTxSpur: myVisaSa.WriteString(":INIT:TSP", true); break;
                case N9020A_MEAS_TYPE.EPowVTM: myVisaSa.WriteString(":INIT:EPVT", true); break;
                case N9020A_MEAS_TYPE.EEVM: myVisaSa.WriteString(":INIT:EEVM", true); break;
                case N9020A_MEAS_TYPE.EOutRFSpec: myVisaSa.WriteString(":INIT:EORF", true); break;
                case N9020A_MEAS_TYPE.ETxSpur: myVisaSa.WriteString(":INIT:ETSP", true); break;
                case N9020A_MEAS_TYPE.MonitorSpec: break;
                default: throw new Exception("Not such a Measure setup type!");
            }

        }

        public void AUTO_COUPLE(N9020A_AUTO_COUPLE _ALLNONE)
        {
            myVisaSa.WriteString(":COUP " + _ALLNONE, true);
        }
        public void DEVICE_RADIO_SETUP(N9020A_STANDARD_DEVICE _DEVICE)
        {
            myVisaSa.WriteString(":RAD:DEV " + _DEVICE, true);
        }
        public void TIME_SLOT(double _timeslot)
        {
            myVisaSa.WriteString(":CHAN:SLOT " + _timeslot.ToString(), true);
            myVisaSa.WriteString(":CHAN:SLOT:AUTO ON", true);
        }
        public void EORF_FREQ_LIST_MODE(N9020A_FREQUENCY_LIST_MODE _FREQLISTMODE)
        {
            switch (_FREQLISTMODE)
            {
                case N9020A_FREQUENCY_LIST_MODE.STANDARD: myVisaSa.WriteString("EORF:LIST:SEL STAN", true); break;
                case N9020A_FREQUENCY_LIST_MODE.SHORT: myVisaSa.WriteString("EORF:LIST:SEL SHOR", true); break;
                case N9020A_FREQUENCY_LIST_MODE.CUSTOM: myVisaSa.WriteString("EORF:LIST:SEL CUST", true); break;
                default: throw new Exception("Not such a frequecy list mode!");
            }
        }
        public void ENABLE_DISPLAY(N9020A_DISPLAY _ONOFF)
        {
            myVisaSa.WriteString(":DISP:ENAB " + _ONOFF, true);
        }
        public void VBW_RATIO(double _ratio)
        {
            myVisaSa.WriteString("BAND:VID:RAT " + _ratio.ToString(), true);
        }
        public void SPAN(double _freq_MHz)
        {
            myVisaSa.WriteString("FREQ:SPAN " + _freq_MHz.ToString() + " MHz", true);
        }

        public void MARKER_TURN_ON_NORMAL(int _markerNum, string _MarkerFreq_MHz, string _BandSpan_MHz)
        {
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":MODE POS", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":X " + _MarkerFreq_MHz.ToString() + " MHz", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":FUNC BPOW", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":FUNC:BAND:SPAN " + _BandSpan_MHz.ToString() + " MHz", true);

        }
        public void MARKER_TURN_ON_DELTA(int _markerNum, string _MarkerFreq_MHz, string _BandSpan_MHz)
        {
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":MODE DELT", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":X " + _MarkerFreq_MHz.ToString() + " MHz", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":REF 1", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":FUNC BPOW", true);
            myVisaSa.WriteString("CALC:MARK" + _markerNum.ToString() + ":FUNC:BAND:SPAN " + _BandSpan_MHz.ToString() + " MHz", true);

        }

        public void TURN_ON_INTERNAL_PREAMP()
        {
            myVisaSa.WriteString("POW:GAIN ON", true);
            myVisaSa.WriteString("POW:GAIN:BAND FULL", true);
        }
        public void TURN_OFF_INTERNAL_PREAMP()
        {
            myVisaSa.WriteString("POW:GAIN OFF", true);
        }
        public void TURN_OFF_MARKER()
        {
            myVisaSa.WriteString(":CALC:MARK:AOFF", true);
        }

        public double READ_MARKER(int _markerNum)
        {
            return WRITE_READ_DOUBLE("CALC:MARK" + _markerNum.ToString() + ":Y?");
        }

        public void ACP_AVERAGE_ON(int _AvgCount)
        {
            myVisaSa.WriteString(":ACP:AVER:COUN " + _AvgCount.ToString(), true);
            myVisaSa.WriteString(":ACP:AVER ON", true);
        }
        public void ACP_AVERAGE_OFF()
        {
            myVisaSa.WriteString(":ACP:AVER OFF", true);
        }

        public void ACP_SWEEP_TIMES(int _sweeptime_ms)
        {
            myVisaSa.WriteString(":ACP:SWE:TIME " + _sweeptime_ms.ToString() + " ms", true);
        }
        public void ACP_SWEEP_POINTS(int _sweepPoints)
        {
            myVisaSa.WriteString(":ACP:SWE:POIN " + _sweepPoints.ToString(), true);
        }

        public void CHP_IBW(double _CHP_IBWMHz)
        {
            myVisaSa.WriteString("CHP:BAND:INT " + _CHP_IBWMHz.ToString() + "MHz", true);
        }
        public void CHP_AMP_REF_LEVEL(double _RefLvl)
        {
            myVisaSa.WriteString("DISP:CHP:VIEW:WIND:TRAC:Y:RLEV " + _RefLvl.ToString(), true);
        }
        public void CHP_SWEEP_TIMES(int _sweeptime_ms)
        {
            myVisaSa.WriteString(":CHP:SWE:TIME " + _sweeptime_ms.ToString() + " ms", true);
        }
        public void CHP_SWEEP_POINTS(int _sweepPoints)
        {
            myVisaSa.WriteString(":CHP:SWE:POIN " + _sweepPoints.ToString(), true);
        }

        public void FUNC_MARK_BAND_POWER_ON()
        {
            myVisaSa.WriteString("CALC:MARK1:FUNC BPOW", true);
        }
        public void FUNC_MARK_SPAN(double _IntervalSpanMHz)
        {
            myVisaSa.WriteString(":CALC:MARK:FUNC:BAND:SPAN " + _IntervalSpanMHz.ToString() + "MHz", true);
        }

        public void ACP_INIT()
        {
            //myVisaSa.WriteString("FORM ASCII", true);
            myVisaSa.WriteString("ACP:DET AVER", true);
            //myVisaSa.WriteString("INIT:CONT OFF", true);
            //myVisaSa.WriteString("ACP:AVER OFF", true);
            //myVisaSa.WriteString("ACP:SWE:TIME 20ms", true);
            //myVisaSa.WriteString("ACP:METH FAST", true);
        }

        public void SWEEP_TIMES(int _sweeptime_ms)
        {
            myVisaSa.WriteString(":SWE:TIME " + _sweeptime_ms.ToString() + " ms", true);
        }
        public void SWEEP_POINTS(int _sweepPoints)
        {
            myVisaSa.WriteString(":SWE:POIN " + _sweepPoints.ToString(), true);
        }

        public void CONTINUOUS_MEASUREMENT_ON()
        {
            myVisaSa.WriteString("INIT:CONT 1", true);
        }
        public void CONTINUOUS_MEASUREMENT_OFF()
        {
            myVisaSa.WriteString("INIT:CONT 0", true);
        }

        public void RESOLUTION_BW(double _BW)
        {
            myVisaSa.WriteString(":BAND " + _BW.ToString(), true);
        }
        public void RESOLUTION_TBW(double _TBW)
        {
            myVisaSa.WriteString(":TXP:BAND " + _TBW.ToString(), true);
        }
        //public void Recall_State(N5182A_WAVEFORM_MODE _TYPE)
        //{

        //    myVisaSa.WriteString("", true);
        //}
        
        public void AVERAGE_TYPE(N9020A_AVE_TYPE _TYPE)
        {
            switch (_TYPE)
            {
                case N9020A_AVE_TYPE.RMS: myVisaSa.WriteString(":AVER:TYPE RMS", true); break;
                case N9020A_AVE_TYPE.LOGARITHM: myVisaSa.WriteString(":AVER:TYPE LOG", true); break;
                case N9020A_AVE_TYPE.SCALAR: myVisaSa.WriteString(":AVER:TYPE SCAL", true); break;
                case N9020A_AVE_TYPE.TXPRMS: myVisaSa.WriteString(":TXP:AVER:TYPE RMS", true); break;
                case N9020A_AVE_TYPE.TXPLOGARITHM: myVisaSa.WriteString(":TXP:AVER:TYPE LOG", true); break;
                case N9020A_AVE_TYPE.PVTRMS: myVisaSa.WriteString(":PVT:AVER:TYPE RMS", true); break;
                case N9020A_AVE_TYPE.PVTLOGARITHM: myVisaSa.WriteString(":PVT:AVER:TYPE LOG", true); break;
                case N9020A_AVE_TYPE.EPVTRMS: throw new Exception(_TYPE + " is not ready!");
                case N9020A_AVE_TYPE.EPVTLOGRITHM: throw new Exception(_TYPE + " is not ready!");
                case N9020A_AVE_TYPE.EORFRMS: throw new Exception(_TYPE + " is not ready!");
                case N9020A_AVE_TYPE.EORFLOGRITHM: throw new Exception(_TYPE + " is not ready!");
                default: throw new Exception(_TYPE + " is not ready!");
            }
        }

        public void CHP_AVERAGE_ON(int _AvgCount)
        {
            myVisaSa.WriteString(":CHP:AVER:COUN " + _AvgCount.ToString(), true);
            myVisaSa.WriteString(":CHP:AVER ON", true);
        }
        public void CHP_AVERAGE_OFF()
        {
            myVisaSa.WriteString(":CHP:AVER OFF", true);
        }

        public void NOISE_CORRECTION_OFF()
        {
            myVisaSa.WriteString(":ACP:CORR:NOIS OFF", true);
        }
        public void NOISE_CORRECTION_ON()
        {
            myVisaSa.WriteString(":ACP:CORR:NOIS ON", true);
        }

        public double MEASURE_PEAK_POINT()
        {
            myVisaSa.WriteString("CALC:MARK:MAX", true);
            return WRITE_READ_DOUBLE("CALC:MARK:Y?");
        }
        double _meas = 0;
        public double MEASURE_MEAN_POINT()
        {
            //WRITE(":FORM ASC");
            _meas = WRITE_READ_DOUBLE("CALC:DATA:COMP? DME");
            //WRITE(":FORM REAL,32");
            return _meas;
        }

        public void CENTER_FREQUENCY(double _Center_FreqMHz)
        {
            myVisaSa.WriteString(":FREQ:CENT " + _Center_FreqMHz + " MHz", true);
        }

        public void CENTER_FREQUENCY(float _Center_FreqMHz)
        {
            myVisaSa.WriteString(":FREQ:CENT " + _Center_FreqMHz + " MHz", true);
        }
        public void VIDEO_BW(double _VBW_MHz)
        {
            myVisaSa.WriteString(":BAND:VID " + _VBW_MHz + " MHz", true);
        }

        public void TRIGGER_CONTINUOUS()
        {
            myVisaSa.WriteString("INIT:CONT ON", true);
        }
        public void TRIGGER_SINGLE()
        {
            myVisaSa.WriteString("INIT:CONT OFF", true);
        }
        public void TRIGGER_IMM()
        {
            myVisaSa.WriteString("INIT:IMM", true);
        }

        public void TRACE_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":AVERage:COUN " + _AVG.ToString(), true);
            myVisaSa.WriteString(":TRAC:TYPE AVER", true);
        }
        public void GSM_AVERAGE_NUM(N9020A_GSM_AVERAGE _measurement, int _AVG)
        {
            switch (_measurement)
            {
                case N9020A_GSM_AVERAGE.TXP: myVisaSa.WriteString(":TXPower:AVERage:COUN " + _AVG.ToString(), true); break;
                case N9020A_GSM_AVERAGE.PVT: myVisaSa.WriteString(":PVT:AVERage:COUN " + _AVG.ToString(), true); break;
                case N9020A_GSM_AVERAGE.EPVT: myVisaSa.WriteString(":EPVT:AVERage:COUN " + _AVG.ToString(), true); break;
                case N9020A_GSM_AVERAGE.EEVM: myVisaSa.WriteString(":EEVM:AVERage:COUN " + _AVG.ToString(), true); break;
                default: throw new Exception("GAM_AVERAGE_NUM --> " + _measurement + " not such a GAM_AVERAGE");
            }
        }
        public void GSM_AVERAGE_STATUS(N9020A_GSM_AVERAGE _measurement, bool _on)
        {
            if (_on)
            {
                switch (_measurement)
                {
                    case N9020A_GSM_AVERAGE.TXP: myVisaSa.WriteString(":TXPower:AVER ON", true); break;
                    case N9020A_GSM_AVERAGE.PVT: myVisaSa.WriteString(":PVT:AVER ON", true); break;
                    case N9020A_GSM_AVERAGE.EPVT: myVisaSa.WriteString(":EPVT:AVER ON", true); break;
                    case N9020A_GSM_AVERAGE.EEVM: myVisaSa.WriteString(":EEVM:AVER ON", true); break;
                    case N9020A_GSM_AVERAGE.EORF: myVisaSa.WriteString(":EORF:AVER ON", true); break;
                    default: throw new Exception("GAM_AVERAGE_NUM --> " + _measurement + " not such a GAM_AVERAGE");
                }
            }
            else
            {
                switch (_measurement)
                {
                    case N9020A_GSM_AVERAGE.TXP: myVisaSa.WriteString(":TXPower:AVER OFF", true); break;
                    case N9020A_GSM_AVERAGE.PVT: myVisaSa.WriteString(":PVT:AVER OFF", true); break;
                    case N9020A_GSM_AVERAGE.EPVT: myVisaSa.WriteString(":EPVT:AVER OFF", true); break;
                    case N9020A_GSM_AVERAGE.EEVM: myVisaSa.WriteString(":EEVM:AVER OFF", true); break;
                    case N9020A_GSM_AVERAGE.EORF: myVisaSa.WriteString(":EORF:AVER OFF", true); break;
                    default: throw new Exception("GAM_AVERAGE_NUM --> " + _measurement + " not such a GAM_AVERAGE");
                }
            }

        }

        public void BURST_Tx_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":TXP:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }
        public void GPowVTM_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":PVT:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }
        public void GORFSpect_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":ORFS:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }
        public void EEVM_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":EEVM:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }
        public void EORFSpect_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":EORF:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }
        public void ACP_Trace_AVERAGE(int _AVG)
        {
            myVisaSa.WriteString(":ACP:AVERage:COUN " + _AVG, true);
            //myVisaSa.WriteString(":TRAC:TYPE AVER");
        }

        public void TRACE_AVERAGE_TYPE(int _TraceNum)
        {
            myVisaSa.WriteString("DET:TRAC" + _TraceNum + " AVER", true);
        }

        public void CLEAR_WRITE()
        {
            myVisaSa.WriteString(":TRAC:TYPE WRIT", true);
        }
        /// <summary>
        /// set BWID 5MHz
        /// set BWID:VID 5MHz
        /// </summary>
        public void BWID()
        {
            myVisaSa.WriteString("BWID 5MHz", true);
            myVisaSa.WriteString("BWID:VID 5MHz", true);
        }


        public void AMPLITUDE_REF_LEVEL_OFFSET(double _RefLvlOffset)
        {
            myVisaSa.WriteString("DISP:WIND:TRAC:Y:RLEV:OFFS " + _RefLvlOffset, true);
        }
        public void AMPLITUDE_REF_LEVEL(double _RefLvl)
        {
            myVisaSa.WriteString("DISP:WIND:TRAC:Y:RLEV " + _RefLvl, true);
        }
        public void AMPLITUDE_INPUT_ATTENUATION(double _Input_Attenuation)
        {
            myVisaSa.WriteString("POW:ATT " + _Input_Attenuation, true);
        }

        public void ELEC_ATTENUATION(float _Input_Attenuation)
        {
            myVisaSa.WriteString("POW:EATT " + _Input_Attenuation, true);
        }

        public void ELEC_ATTEN_ENABLE(bool _Input_Stat)
        {
            if (_Input_Stat)
                myVisaSa.WriteString("POW:EATT:STAT ON", true);
            else
                myVisaSa.WriteString("POW:EATT:STAT OFF", true);
        }

        public bool CHECK_MXA_OPTION(string OPT)
        {
            bool available = false;
            string temp = WRITE_READ_STRING("SYST:OPT?");
            if (temp.ToUpper().Contains(OPT))
            {
                available = true;
            }
            else
            {
                available = false;
            }
            return available;

        }

        public void ACP_OFFSET2(float Offset1_MHz, float Offset2_MHz)
        {
            myVisaSa.WriteString("ACP:OFFS1:LIST " + Offset1_MHz + " MHz, " + Offset2_MHz + " MHz, " + " 0 MHz, 0 MHz, 0 MHz, 0 MHz", true);

        }
        public void ACP_OFFSET(double Offset1_MHz, double Offset2_MHz, double Offset3_MHz, double BW1_MHz, double BW2_MHz, double BW3_MHz)
        {
            if (BW3_MHz == 0)
            {
                myVisaSa.WriteString("ACP:OFFS1:LIST:STAT ON, ON, OFF, OFF, OFF, OFF", true);
                myVisaSa.WriteString("ACP:OFFS1:LIST " + Offset1_MHz + " MHz, " + Offset2_MHz + " MHz, " + "0 MHz, 0 MHz", true);
                myVisaSa.WriteString("ACP:OFFS1:LIST:BAND " + BW1_MHz + " MHz, " + BW2_MHz + " MHz, " + "10 kHz, 10 kHz", true);


            }
            else
            {
                myVisaSa.WriteString("ACP:OFFS1:LIST:STAT ON, ON, ON, OFF, OFF, OFF", true);
                myVisaSa.WriteString("ACP:OFFS1:LIST " + Offset1_MHz + " MHz, " + Offset2_MHz + " MHz, " + Offset3_MHz + " MHz, 0 MHz, 0 MHz, 0 MHz", true);
                myVisaSa.WriteString("ACP:OFFS1:LIST:BAND " + BW1_MHz + " MHz, " + BW2_MHz + " MHz, " + BW3_MHz + " MHz, " + "10 kHz, 10 kHz, 10 kHz", true);

            }
        }
        public void ACP_CARRIER(double NoiseBW_MHz, double CarrSpacing_MHz)
        {
            myVisaSa.WriteString("ACP:CARR1:LIST:WIDT " + CarrSpacing_MHz + "MHz", true);
            myVisaSa.WriteString("ACP:CARR1:LIST:BAND " + NoiseBW_MHz.ToString() + "MHz", true);
        }

        public void EORF_OFFSET(string Offset1_MHz, string Offset2_MHz, string Offset3_MHz, string BW_kHz)
        {
            if (Offset3_MHz == "0")
            {
                myVisaSa.WriteString("EORF:LIST:MOD:FREQ 0.0," + Offset1_MHz + " MHz, " + Offset2_MHz + " MHz", true);
                myVisaSa.WriteString("EORF:LIST:MOD:BAND " + 60 + "kHz, " + BW_kHz + "kHz, " + BW_kHz + "kHz", true);
                myVisaSa.WriteString("EORF:LIST:MOD:STAT ON, ON, ON, OFF, OFF, OFF", true);
            }
            else
            {
                myVisaSa.WriteString("EORF:LIST:MOD:FREQ 0.0," + Offset1_MHz + " MHz, " + Offset2_MHz + " MHz, " + Offset3_MHz + " MHz", true);
                myVisaSa.WriteString("EORF:LIST:MOD:BAND " + 60 + " kHz, " + BW_kHz + " kHz," + BW_kHz + " kHz," + BW_kHz + " kHz", true);
                myVisaSa.WriteString("EORF:LIST:MOD:STAT ON, ON, ON, ON, OFF, OFF", true);
            }
        }

        public double READ_CHANPOWER()
        {
            return WRITE_READ_DOUBLE("FETC:CHP:CHP?");
            //return Write_Read_Double("MEAS:CHP:CHP?", true);
            //return Write_Read_Double("READ:CHP:CHP?", true);
        }
        public void ALLIGN_PARTIAL()
        {
            myVisaSa.WriteString(":CAL:AUTO PART", true);
        }
        public void CAL()
        {
            myVisaSa.WriteString(":CAL", true);
        }
        public bool OPERATION_COMPLETE()
        {
            bool _complete = false;
            double _dummy = -9;
            do
            {
                //Thread.Sleep(2);
                _dummy = WRITE_READ_DOUBLE("*OPC?");
            } while (_dummy == 0);
            _complete = true;
            return _complete;
        }

        public void FREQ_CENT(int EquipId, string strSaFreq, string strUnit)
        {
            //SAFREQCENT:SA01:%02_FREQ:MHz:@	# SA center frequency in MHz -> ":FREQ:CENT ", f, "MHz"
            //SAFREQCENT:SA01:01_TEST_VARIABLE:MHz:@	# SA center frequency

            //Checking whether the passing value is number or variable
            //split[2] = funcCheckPassValue(2);

            if (false)
            {
                // Debug.WriteLine("\nSA center frequency changed to " + split[2] + split[3]);
            }
            else
            {
                //int varUnit = 0;

                //if (strUnit == "kHz")
                //    varUnit = 1000;
                //else if (strUnit == "MHz")
                //    varUnit = 1000000;
                //else if (strUnit == "GHz")
                //    varUnit = 1000000000;

                if (EquipId == 1 || EquipId == 3)
                    myVisaSa.WriteString(":FREQ:CENT " + strSaFreq + strUnit, true);


                //switch (EquipId)
                //{
                //    case "SA01":

                //        varSystemSa01FreqCent = Convert.ToDouble(strSaFreq) * varUnit;

                //        if (varIdSa01 == 2) // PXI digitizer
                //        {


                //            if (Convert.ToSingle(strSaFreq) >= 5922.24)
                //            {
                //                myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopBelow;
                //                varSystemSa01LoPositionHigh = false;
                //            }
                //            else if (Convert.ToSingle(strSaFreq) <= 452.76)
                //            {
                //                myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopAbove;
                //                varSystemSa01LoPositionHigh = true;
                //            }
                //            else if (!varSystemSa01LoPositionHigh)
                //            {
                //                try
                //                {
                //                    myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopAbove;
                //                    varSystemSa01LoPositionHigh = true;
                //                }
                //                catch (Exception e)
                //                {
                //                    myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopBelow;
                //                    varSystemSa01LoPositionHigh = false;
                //                }
                //            }

                //            try
                //            {
                //                myAfDig.RF.CentreFrequency = Convert.ToDouble(strSaFreq) * varUnit;
                //            }
                //            catch (Exception e)
                //            {
                //                if (varSystemSa01LoPositionHigh)
                //                    myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopBelow;
                //                else
                //                    myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopAbove;
                //            }

                //        }
                //        else
                //        {
                //            mySa01.WriteLine(":FREQ:CENT " + strSaFreq + strUnit);
                //            varSaCentFreq = Convert.ToSingle(strSaFreq);
                //            funcSaVariableCal();
                //        }
                //        break;

                //    case "SA02":

                //        varSystemSa02FreqCent = Convert.ToDouble(strSaFreq) * varUnit;

                //        if (varIdSa02 == 2) // PXI digitizer
                //        {
                //            myAfDig.RF.CentreFrequency = Convert.ToDouble(strSaFreq) * varUnit;

                //            if (Convert.ToSingle(strSaFreq) >= 5922.24)
                //            {
                //                myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopBelow;
                //                varSystemSa02LoPositionHigh = false;
                //            }
                //            else if (Convert.ToSingle(strSaFreq) <= 452.76)
                //            {
                //                myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopAbove;
                //                varSystemSa02LoPositionHigh = true;
                //            }
                //            else if (!varSystemSa01LoPositionHigh)
                //            {
                //                myAfDig.RF.LOPosition = AFCOMDIGITIZERLib.afDigitizerDll_lopLOPosition_t.afDigitizerDll_lopAbove;
                //                varSystemSa02LoPositionHigh = true;
                //            }
                //        }
                //        else
                //        {
                //            mySa02.WriteLine(":FREQ:CENT " + strSaFreq + strUnit);
                //            varSaCentFreq = Convert.ToSingle(strSaFreq);
                //            funcSaVariableCal();
                //        }
                //        break;
                //}
            }
        }
        public void MEAS_SPAN_ZERO(int EquipId, float CalDataSa, ref float MeasuredPout)
        {
            int i = 0;
            float varAverageTraceData = 0;
            float[] arrSaTraceData = null;

            if (EquipId == 1)    // PSA
            {
                bool result = OPERATION_COMPLETE();

                string varTestResult = null;


                //varTestResult = mySa01.Read();

                //try
                //{
                //    varTestResult = mySa01.Read();
                //}
                //catch (Exception ex)
                //{
                //    Debug.WriteLine(ex.ToString());
                //}
                //while (varTestResult == null)
                //{

                //    Thread.Sleep(100);

                //    try
                //    {
                //        varTestResult = mySa01.Read();
                //    }
                //    catch (Exception ex)
                //    {
                //        Debug.WriteLine(ex.ToString());
                //    }
                //}

                //mySa01.WriteLine(":TRAC? TRACE1");
                //myVisaSa.WriteString(":TRAC? TRACE1", true);
                //arrSaTraceData = mySa01.ReadIeeeBlockAsSingleArray();
                //arrSaTraceData = myVisaSa01.ReadIEEEBlock(IEEEBinaryType.BinaryType_R4, true, true);

                //object varTempObject = myVisaSa01.ReadIEEEBlock(IEEEBinaryType.BinaryType_R4, true, true);
                //arrSaTraceData = (float[])varTempObject;
                //arrSaTraceData = (float[])myVisaSa.ReadIEEEBlock(IEEEBinaryType.BinaryType_R4, true, true);


            }

            arrSaTraceData = WRITE_READ_IEEEBlock(":TRAC? TRACE1", IEEEBinaryType.BinaryType_R4);

            for (i = 0; i < arrSaTraceData.Length; i++)
                varAverageTraceData = varAverageTraceData + arrSaTraceData[i];

            varAverageTraceData = varAverageTraceData / arrSaTraceData.Length; // +varOutputSgCal;
            MeasuredPout = varAverageTraceData + CalDataSa;

            //MeasuredPout = (float)MEASURE_MEAN_POINT();


            //strCurrentPout = Convert.ToString(varAverageTraceData);
            //varCurrentPout = Convert.ToSingle(strCurrentPout);
        }
        public void MEAS_SPAN_ASC(ref float MeasuredPout)
        {
            //myVisaSa.WriteString(":FORM ASC", true);
            MeasuredPout = WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME");
            //WRITE(":FORM REAL,32");
        }

        public void MEAS_SPAN_REAL(ref float MeasuredPout)
        {
            float[] Temp_MeasPower;
            //myVisaSa.WriteString(":FORM ASC", true);
            Temp_MeasPower = WRITE_READ_IEEEBlock(":CALC:DATA1:COMP? DME", IEEEBinaryType.BinaryType_R4);
            MeasuredPout = Temp_MeasPower[0];

            //WRITE(":FORM REAL,32");
        }
        public void MEAS_SPAN_ZERO_ASC(int EquipId, ref float MeasuredPout)
        {
            int i = 0;
            float varAverageTraceData = 0;
            float[] arrSaTraceData = null;

            //if (EquipId == 1)    // PSA
            //{
            //    bool result = OPERATION_COMPLETE();

            //    string varTestResult = null;


            //}


            //myVisaSa.WriteString(":FORM ASC",true );
            varAverageTraceData = WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME");
            //myVisaSa.WriteString(":FORM REAL,32",true );
            MeasuredPout = varAverageTraceData;

            //arrSaTraceData = WRITE_READ_IEEEBlock(":TRAC? TRACE1", IEEEBinaryType.BinaryType_R4);

            //for (i = 0; i < arrSaTraceData.Length; i++)
            //    varAverageTraceData = varAverageTraceData + arrSaTraceData[i];

            //varAverageTraceData = varAverageTraceData / arrSaTraceData.Length; // +varOutputSgCal;
            //MeasuredPout = varAverageTraceData + CalDataSa;

            //MeasuredPout = (float)MEASURE_MEAN_POINT();


            //strCurrentPout = Convert.ToString(varAverageTraceData);
            //varCurrentPout = Convert.ToSingle(strCurrentPout);
        }

        public void WRITE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
        }
        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaSa.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
            return myVisaSa.ReadString();
        }
        public float WRITE_READ_SINGLE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
            return Convert.ToSingle(myVisaSa.ReadString());
        
        }
        public float[] READ_IEEEBlock(IEEEBinaryType _type)
        {
            return (float[])myVisaSa.ReadIEEEBlock(_type, true, true);
        }
        public float[] WRITE_READ_IEEEBlock(string _cmd, IEEEBinaryType _type)
        {
            myVisaSa.WriteString(_cmd, true);
            return (float[])myVisaSa.ReadIEEEBlock(_type, true, true);
        }
    }
}
