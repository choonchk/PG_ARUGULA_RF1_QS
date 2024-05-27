using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using MPAD_TestTimer;

namespace EqLib.NA
{
    public abstract class NetworkAnalyzerAbstract : LibEqmtCommon
    {
        private static bool _swUse;
        private static bool _handlerUse;
        internal bool _NFChn;

        public bool switchUse
        {
            get { return _swUse; }
            set { _swUse = value; }
        }
        public bool pdLibSw { get; set; }

        // TODO Unused.
        public EqSwitchMatrix.EqSwitchMatrixBase EqSwitchMatrix { get; set; }
        public bool handlerUse
        {
            get { return _handlerUse; }
            set { _handlerUse = value; }
        }

        public void UnInit() { }
        public virtual void LoadState(string filename) { }
        public virtual void SaveState(string filename) { }

        public virtual void DisplayOn(bool state) { }

        public virtual void Averaging(int channelNumber, naEnum.EOnOff val) { }
        public virtual void AveragingMode(int channelNumber, string val) { }
        public virtual void AveragingFactor(int channelNumber, int val) { }
        public virtual void ActiveChannel(int channelNumber) { }
        public virtual void ChannelMax(int channelNumber, bool state) { }

        //Correction Method
        #region Correction

        public virtual void SetCorrProperty(int channelNumber, bool enable) { }
        public virtual void ClearCorr(int channelNumber) { }
        public virtual void SelectSubClass(int channelNumber) { }
        public virtual void SetCorrMethod(int channelNumber, List<int> numberPorts, List<string> ConType) { }
        public virtual void MeasCorr1PortStd(int channelNumber, int portNumber, int stdNumber, string stdType) { }
        public virtual void MeasCorr2PortStd(int channelNumber, int port1, int port2, int stdNumber, string stdType) { }
        public virtual void SaveCorr(int channelNumber) { }
        #endregion

        //Source Function
        public virtual void SetPower(float level){}

        //Sense function
        #region Sense Function
        public virtual void SetFreqStart(int channelNumber, double Freq)
        { 
            SendCommand("SENS" + channelNumber.ToString() + ":FREQ:STAR " + Freq.ToString()); 
        }
        public virtual void SetFreqStop(int channelNumber, double Freq) 
        {
            SendCommand("SENS" + channelNumber.ToString() + ":FREQ:STOP " + Freq.ToString());
        }
        public virtual void SetFreqBw(int channelNumber, double BW) 
        {
            SendCommand("SENS" + channelNumber.ToString() + ":BAND " + BW.ToString());
        }

        public virtual void SetSwePoint(int channelNumber, int points) 
        {
            SendCommand("SENS" + channelNumber.ToString() + ":SWE:POIN " + points.ToString());
        }
        public virtual void SetSweType(int channelNumber, naEnum.ESweepType sweepType) 
        {
            SendCommand("SENS" + channelNumber.ToString() + ":SWE:TYPE " + sweepType.ToString());
        }
        #endregion


        //Trigger Function
        public virtual void TriggerSingle(int channelNumber) { }
        public virtual void TriggerMode(naEnum.ETriggerMode trigMode) { }
        public virtual void TriggerSource(naEnum.ETriggerSource trigSource) { }

        public virtual string GetTraceInfo(int channelNumber)
        {
            return string.Empty;
        }
        public virtual naEnum.ESFormat GetTraceFormat(int channelNumber, int traceNumber)
        {
            return naEnum.ESFormat.MLOG;
        }
        public virtual double[] GetFreqList(int channelNumber)
        {
            return new double[] { };
        }

        //Grab Data

        public virtual double[] GrabRealImagData(int channelNumber)
        {
            return new double[] { };
        }
        public virtual void setMemoryMap() { }

        internal virtual void SetCalKitLabel(int channelNumber, string label) { }
        internal virtual string GetCalKitLabel(int channelNumber) { return string.Empty; }
        internal virtual void InsertTrace(int channelNumber, SortedList<int, String> Traces) { }
        internal virtual void InsertTrace(int channelNumber, SortedList<int, String> Traces, SortedList<int, String> Balun) { }
        internal virtual void InsertTrace(int channelNumber, SortedList<int, String> Traces, SortedList<int, String> Balun, SortedList<int,int> TopazWind) { }
        internal virtual void InsertSegmentTableData(int channelNumber, SSegmentTable segmentTable, bool UseCalPow = false) { }
        internal virtual void InsertCalKitStd(SCalStdTable stdTable){}
        internal virtual void SetDisplay(int TotalChNumber) { }
        internal virtual void SetFMState(int channelNumber, naEnum.EOnOff val) { }
        internal virtual naEnum.EOnOff GetFMState(int channelNumber) { return naEnum.EOnOff.Off;}

        #region abtract implementation

        public SParam GrabSParamRiData(int channelNumber)
        {
            int offSet = 0;
            int sParamDef;
            string tmpVar;
            string[] arrTrace, traces;
            string[] traceMap;
            double[] rawData;

            SParam sparamData = new SParam();
            naEnum.ESFormat traceFormat;

            rawData = GrabRealImagData(channelNumber);
            
            tmpVar = GetTraceInfo(channelNumber);
            tmpVar = tmpVar.Replace("'", "").Replace("\n", "").Trim();
            arrTrace = tmpVar.Split(new char[] { ',' });

            //Get only odd number
            traces = arrTrace.Where((item, index) => index % 2 != 0).ToArray();
            traceMap = arrTrace.Where((item, index) => index % 2 == 0).ToArray(); //new int[traces.Length];

            
            double[] fList = GetFreqList(channelNumber);
            sparamData.Freq = fList;
            sparamData.NoPoints = fList.Length;
            int Basepoint = fList.Length;
            sparamData.SParamData = new SParamData[traces.Length];

            for (int i = 0; i < traces.Length; i++)
            {
                tmpVar = traceMap[i].Replace("Trc", "");
                sParamDef = (int.Parse(tmpVar) - 1);
                traceFormat = GetTraceFormat(channelNumber, sParamDef + 1);
                offSet = i * Basepoint * 2;

                sparamData.SParamData[i].SParam = new ComplexNumber[Basepoint];
                sparamData.SParamData[i].Format = traceFormat;

                if (traceFormat == naEnum.ESFormat.GDEL)
                {
                    sparamData.SParamData[i].SParamDef = (naEnum.ESParametersDef)Enum.Parse(typeof(naEnum.ESParametersDef), "GD" + traces[i]);
                }
                else
                {
                    sparamData.SParamData[i].SParamDef = (naEnum.ESParametersDef)Enum.Parse(typeof(naEnum.ESParametersDef), traces[i]);
                }

                for (int iPts = 0; iPts < Basepoint; iPts++)
                {
                    try
                    {
                        ComplexNumber sp = new ComplexNumber();

                        if (traceFormat == naEnum.ESFormat.GDEL)
                        {

                            sp.DB = (rawData[offSet + (iPts)]) / 1e-9;
                            sp.Phase = 0;
                            //sparamData.SParamData[i].SParam[iPts].DB = 
                            //sparamData.SParamData[i].SParam[iPts].Phase = 0;
                        }
                        else if (sparamData.SParamData[i].SParamDef == naEnum.ESParametersDef.NF)
                        {
                            sp.DB = (rawData[offSet + (iPts)]);
                            sp.Phase = 0;
                        }
                        else
                        {

                            sp.Real = rawData[offSet + (iPts * 2)];
                            sp.Imag = rawData[offSet + (iPts * 2) + 1];
                            sp.conv_RealImag_to_dBAngle();

                            //objMathData.Real = rawData[offSet + (iPts * 2)];
                            //objMathData.Imag = rawData[offSet + (iPts * 2) + 1];

                            //objMath.conv_RealImag_to_dBAngle(ref objMathData);

                            //sparamData.SParamData[i].SParam[iPts].Real = objMathData.Real;
                            //sparamData.SParamData[i].SParam[iPts].Imag = objMathData.Imag;
                            //sparamData.SParamData[i].SParam[iPts].DB = objMathData.DB;
                            //sparamData.SParamData[i].SParam[iPts].Mag = objMathData.Mag;
                            //sparamData.SParamData[i].SParam[iPts].Phase = objMathData.Phase;
                        }
                        sparamData.SParamData[i].SParam[iPts] = sp;
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            }
            return sparamData;

        }

        //Setup Cal-kit
        public void SetupCalKit(SCalStdTable stdTable)
        {
            InsertCalKitStd(stdTable);
        }

        //Setup NA state file
        public void SetupTrace(int channelNumber, STraceMatching TraceMatching) 
        {
            SortedList<int, String> Traces = new SortedList<int, string>();
            SortedList<int, String> Balun = new SortedList<int, string>();
            SortedList<int, int> TopazWind = new SortedList<int, int>();

            ActiveChannel(channelNumber);
            //ChannelMax(channelNumber, true);
            
            for (int i = 0; i < TraceMatching.TraceNumber.Length;i++ )
            {
                if (TraceMatching.SParamDefNumber[i] > -1)
                {
                    Traces.Add(TraceMatching.TraceNumber[i], Enum.GetName(typeof(naEnum.ESParametersDef), TraceMatching.SParamDefNumber[i]));
                    Balun.Add(TraceMatching.TraceNumber[i], Enum.GetName(typeof(naEnum.EBalunTopology), TraceMatching.BalunTopology[i]));
                    TopazWind.Add(TraceMatching.TraceNumber[i], TraceMatching.TopazWindNo[i]);
                }
            }

            _NFChn = TraceMatching.NFChannel;
            InsertTrace(channelNumber, Traces,Balun, TopazWind);
            Thread.Sleep(600);
            //ChannelMax(channelNumber, false);
        }
        public virtual void SetupNFChannel(int channelNumber, STraceMatching TraceMatching) { }
       

        public void SetupSegmentTable(SSegmentTable[] segmentTable, bool UseCalPow = false)
        {  
            SetPower(0);//Set Power to 0 for ZNB
            SetDisplay(segmentTable.Length);
            Thread.Sleep(500);
            for (int iChn = 0; iChn < segmentTable.Length; iChn++)
            {
                InsertSegmentTableData(iChn + 1, segmentTable[iChn], UseCalPow);
                SetSweType(iChn + 1, naEnum.ESweepType.SEGM);
            }
        }
        public virtual void GetSegmentTable(out SSegmentTable segmentTable, int channelNumber)
        {
            segmentTable = new SSegmentTable();
        }
        public void ExportSnpFile(SParam sparamData, string FolderName, string LotNo, string Unit, string strChn)
        {
            #region FBAR 
            //string[] OutputData;
            //string OutputFileName;

            //if (!Directory.Exists(FolderName))
            //    Directory.CreateDirectory(FolderName);


            //OutputFileName = FolderName + "Unit" + Unit + "_Chan" + (iChn).ToString() + "_" + FileName;
            //OutputData = new string[sparamData.Freq.Length + 3];
            //OutputData[0] = "#\tHZ\tS\tdB\tR 50";
            //OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();
            //OutputData[2] = "!Freq\t";

            //for (int i = 0; i < sparamData.SParamData.Length; i++)
            //{
            //    OutputData[2] += sparamData.SParamData[i].SParamDef.ToString() + "\t\t";  
            //    for (int iPoint = 0; iPoint < sparamData.SParamData[i].SParam.Length; iPoint++)
            //    {
            //        if (i == 0)
            //            OutputData[iPoint + 3] += sparamData.Freq[iPoint].ToString();
            //        OutputData[iPoint + 3] += "\t" + sparamData.SParamData[i].SParam[iPoint].DB +
            //                                 "\t" + sparamData.SParamData[i].SParam[iPoint].Phase;
            //    }
            //}
            //System.IO.File.WriteAllLines(OutputFileName + ".snp", OutputData);
            #endregion
            string[] OutputData;
            string OutputFileName;
            string SubFolder;
            string tmpStr = strChn.Substring(strChn.LastIndexOf("_") + 1).Replace("Ch", "") + "_" +
                            strChn.Remove(strChn.LastIndexOf("_"));

            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);

            SubFolder = Path.Combine(FolderName, "CH" + tmpStr + "\\");
            if (!Directory.Exists(SubFolder))
                Directory.CreateDirectory(SubFolder);

            //OutputFileName = SubFolder + LotNo + "_" + DateTime.Now.ToString("yyyyMMdd" + "_" + "HHmm") + "_CH" + tmpStr + "_Unit" + Unit;
            OutputFileName = SubFolder + LotNo + "_" + DateTime.Now.ToString("yyyyMMdd" + "_" + "HHmm") + "_Unit" + Unit;
            OutputData = new string[sparamData.NoPoints + 3];
            OutputData[0] = "#\tHZ\tS\tdB\tR 50";
            OutputData[1] = "!\t" + DateTime.Now.ToShortDateString() + "\t" + DateTime.Now.ToLongTimeString();
            OutputData[2] = "!Freq\t";

            for (int i = 0; i < sparamData.SParamData.Length; i++)
            {
                OutputData[2] += sparamData.SParamData[i].SParamDef.ToString() + "\t\t";
                for (int iPoint = 0; iPoint < sparamData.NoPoints; iPoint++)
                {
                    if (i == 0)
                    {
                        OutputData[iPoint + 3] += sparamData.Freq[iPoint];// Legacy_FbarTest.freqList[iChn][iPoint];
                    }
                    OutputData[iPoint + 3] += "\t" +  sparamData.SParamData[i].SParam[iPoint].DB +//sparamData[Keys[i]][iPoint] +
                                             "\t" + sparamData.SParamData[i].SParam[iPoint].Phase;
                }



            }
            File.WriteAllLines(OutputFileName + ".snp", OutputData);
        }

        public virtual void Calibrate(SCalibrationProcedure[] CalProcedure, SSegmentTable[] segmentTables, string stateFile)
        {
            int tmpChannelNo = -1;
            int totalChannel = 0;
            string tmpLabel;
            bool CalKit_FailCheck = false;
            bool bNext;
            string EnrFile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346D_22dB_AE329.enr";
            string eCalKit = "eCal";
            naEnum.EOnOff FMState = naEnum.EOnOff.Off;
            List<string> InitList = new List<string>();

            string msg = String.Format("NAAbstract-Calibrate() - Step 1: Reset and loading state file.");
            LoggingManager.Instance.LogInfoTestPlan(msg);
            Reset();
            Thread.Sleep(500);
            LoadState(stateFile);
            Thread.Sleep(6000);
            DisplayOn(true);

            for (int i = 0; i < CalProcedure.Length; i++)
            {
                SCalibrationProcedure cp1 = CalProcedure[i];
                msg = String.Format("NAAbstract-Calibrate() - Step 2:Initialize {0} of {1}, Channel:{2},Port1:{3}, ConnType:{4}, CalStandard:{5}", 
                    i + 1, CalProcedure.Length, cp1.ChannelNumber, cp1.Port1, cp1.ConnectorType, cp1.CalStandard);
                LoggingManager.Instance.LogInfoTestPlan(msg);

                if (tmpChannelNo != CalProcedure[i].ChannelNumber)
                {
                    SetCalKitLabel(CalProcedure[i].ChannelNumber, CalProcedure[i].CKitLabel);
                    Operation_Complete();
                    Thread.Sleep(30);
                    //Only verify the 1st OPEN cal statement only. Must ensure that your Cal Kit Label define correctly for this row
                    if (naEnum.ECalibrationStandard.OPEN == CalProcedure[i].CalStandard)
                    {
                        if (CalProcedure[i].CKitLocNum != 0) //Only check if user define the cal kit location number, undefine will assume no check required
                        {
                            char[] trimChar = { '\"', '\n' };
                            tmpLabel = GetCalKitLabel(CalProcedure[i].ChannelNumber);
                            tmpLabel = tmpLabel.Trim(trimChar);
                            if (tmpLabel != CalProcedure[i].CKitLabel)
                            {
                                CalKit_FailCheck = true;  // set flag to true, cal program will not proceed if flag true
                                msg = string.Format("Unrecognize ENA CalKit Label = {0}{1}Define Cal Kit Label in config file = {2}{1}Please checked your configuration file !!!!!{1} ***** Calibration Procedure will STOP and EXIT *****", 
                                    tmpLabel, '\n', CalProcedure[i].CKitLabel);
                                DisplayError(msg, "Error Cal Kit Verification");
                            }
                        }
                        tmpChannelNo = CalProcedure[i].ChannelNumber;
                        totalChannel++;
                    }
                }
                #region NA Initial
                int ChannelNo = CalProcedure[i].ChannelNumber;
                if (!InitList.Contains(ChannelNo.ToString())| CalProcedure[i].CalStandard == naEnum.ECalibrationStandard.NS_POWER)
                {
                    if (CalProcedure[i].ParameterType == "NF" )
                        segmentTables[ChannelNo - 1].NFChannel = true;
                    InsertSegmentTableData(ChannelNo, segmentTables[ChannelNo - 1], true);//CHAMGE TO CHANGE CAL POWER - need new stuff
                    Operation_Complete();
                    SetCorrProperty(ChannelNo, false); //OFF CAL COOR - DONE
                    ChannelMax(ChannelNo, true);//ENA only
                    Thread.Sleep(100);
                    SelectSubClass(CalProcedure[i].ChannelNumber);
                    Operation_Complete();
                    SetCalKitLabel(CalProcedure[i].ChannelNumber, CalProcedure[i].CKitLabel); //TOPAZ DONE
                    Operation_Complete();

                    //*if (CalProcedure[iCal].ParameterType == "NF" & CalProcedure[iCal].CalStandard != naEnum.ECalibrationStandard.ECAL_OPEN*/)
                    if (CalProcedure[i].CalStandard == naEnum.ECalibrationStandard.NS_POWER)
                    {
                        InitNFCalChannel(CalProcedure[i].ChannelNumber, CalProcedure[i].NF_SrcPortNum, CalProcedure[i].Port1,
                                        CalProcedure[i].ConnectorType, CalProcedure[i].CKitLabel, EnrFile);
                        SetCorrMethod(CalProcedure[i].ChannelNumber, GetCalPorts(ChannelNo, CalProcedure)
                            , GetConType(ChannelNo, CalProcedure));
                        eCalKit = GetECalKitLabel();
                        InitList.Add(ChannelNo.ToString());
                    }
                    else if (CalProcedure[i].ParameterType != "NF" | CalProcedure[i].CalStandard == naEnum.ECalibrationStandard.ECAL_OPEN)
                    {
                        SetCorrMethod(CalProcedure[i].ChannelNumber, GetCalPorts(ChannelNo, CalProcedure)
                            , GetConType(ChannelNo, CalProcedure)); //TOPAZ - INIT CAL HERE
                        InitList.Add(ChannelNo.ToString());
                    }
                }
                #endregion
            }
            if (!CalKit_FailCheck)
            {
                string tempDef = GetTraceInfo(1);
                string[] parts = (tempDef.Split(','));

                DisplayMessage("Start Calibration", "Start Calibration");
                int ChannelNo = -1;
                int pStep = 0;
                for (int iCal = 0; iCal < CalProcedure.Length; iCal++)
                {
                    SCalibrationProcedure cp1 = CalProcedure[iCal];
                    msg = String.Format("NAAbstract-Calibrate() - Step 3:Initialize {0} of {1}, Channel:{2},Port1:{3}, ConnType:{4}, CalStandard:{5}",
                        iCal + 1, CalProcedure.Length, cp1.ChannelNumber, cp1.Port1, cp1.ConnectorType, cp1.CalStandard);
                    LoggingManager.Instance.LogInfoTestPlan(msg);

                    //on the ZNBT averaging
                    if (ChannelNo != -1 && ChannelNo == CalProcedure[iCal].ChannelNumber)
                    {
                        Averaging(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].AvgState);
                    }

                    string tmpStr;
                    do
                    {
                        bNext = false;


                        if (_handlerUse)
                        {
                            // put in PnP handler movement control code
                            Eq.Handler.WaitForDUT();
                        }
                        
                        #region "Calibration Message"
                        if (CalProcedure[iCal].Message.Trim() != "" & CalProcedure[iCal].Message.Trim() != "0")
                        {
                            if (pStep > 0 | iCal > 0)
                            {
                                // Disabled for handler.
                                //switch (MessageBox.Show("Do you want to re-measure STD?\n" + CalProcedure[pStep].Message, "Penang NPI", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                                //{
                                //    case DialogResult.Cancel:
                                //        bNext = true;
                                //        break;
                                //    case DialogResult.Yes:
                                //        bNext = false;
                                //        iCal = pStep;
                                //        break;
                                //}
                            }
                            tmpStr = CalProcedure[iCal].Message;
                            pStep = iCal;
                            DisplayMessage(tmpStr,"Calibration (Sub Cal Kit)");
                        }
                        else
                        {
                            tmpStr = string.Format("Calibrating (SubCal Kit) procedure for : " + "Channel Number : {0}\t", 
                                CalProcedure[iCal].ChannelNumber);
                            switch (CalProcedure[iCal].CalStandard)
                            {
                                case naEnum.ECalibrationStandard.ECAL:
                                    for (int iPort = 0; iPort < CalProcedure[iCal].NoPorts; iPort++)
                                    {
                                        switch (iPort)
                                        {
                                            case 0:
                                                tmpStr += "Port " + CalProcedure[iCal].Port1.ToString();
                                                break;
                                            case 1:
                                                tmpStr += "," + CalProcedure[iCal].Port2.ToString();
                                                break;
                                            case 2:
                                                tmpStr += "," + CalProcedure[iCal].Port3.ToString();
                                                break;
                                            case 3:
                                                tmpStr += "," + CalProcedure[iCal].Port4.ToString();
                                                break;

                                        }
                                    }
                                    break;
                                case naEnum.ECalibrationStandard.ISOLATION:
                                case naEnum.ECalibrationStandard.THRU:
                                case naEnum.ECalibrationStandard.TRLLINE:
                                case naEnum.ECalibrationStandard.TRLTHRU:
                                    tmpStr += "Port " + CalProcedure[iCal].Port1.ToString() + "," + CalProcedure[iCal].Port2.ToString();
                                    break;
                                case naEnum.ECalibrationStandard.LOAD:
                                case naEnum.ECalibrationStandard.OPEN:
                                case naEnum.ECalibrationStandard.SHORT:
                                case naEnum.ECalibrationStandard.SUBCLASS:
                                case naEnum.ECalibrationStandard.TRLREFLECT:
                                    tmpStr += "Port " + CalProcedure[iCal].Port1.ToString();
                                    break;
                            }
                        }

                        #endregion

                        Thread.Sleep(CalProcedure[iCal].Sleep);

                        if (CalProcedure[iCal].BCalKit)
                        {
                            #region "Cal Kit Procedure"

                            if (ChannelNo != CalProcedure[iCal].ChannelNumber)
                            {
                                if (ChannelNo != -1)
                                {
                                    //SaveCorr(ChannelNo);
                                    Thread.Sleep(100);
                                    SetFMState(ChannelNo, FMState);
                                }
                                ChannelNo = CalProcedure[iCal].ChannelNumber;

                                FMState = GetFMState(ChannelNo);

                                //_swUse = false;
                                if (_swUse)
                                {
                                    
                                    //do switching for PAD
                                    if(pdLibSw)
                                    {
                                        if (CalProcedure[iCal].ParameterType != "NF")
                                        {
                                            Thread.Sleep(200);
                                            // Case Pinot.
                                            SetSwitchMatrixPathsPinot(CalProcedure[iCal].Switch_Input.ToUpper(),
                                                CalProcedure[iCal].Switch_Ant, CalProcedure[iCal].Switch_Rx);
                                            // Case HLS2.
                                            /*Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Input.ToUpper(), Operation.N1toTx);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Ant, Operation.N2toAnt);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Rx, Operation.N3toRx);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Rx, Operation.N4toRx);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Rx, Operation.N5toRx);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Rx, Operation.N6toRx);*/
                                        }
                                        else
                                        {
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Input.ToUpper(), Operation.ENAtoRFIN);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Ant, Operation.ENAtoRFOUT);
                                            Eq.Site[0].SwMatrix.ActivatePath(CalProcedure[iCal].Switch_Rx, Operation.ENAtoRX);
                                        }
                                    }
                                    else
                                    {
                                        string chBand = string.Format("CH{0}_{1}", CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].Switch);
                                        EqSwitchMatrix.ActivatePath(chBand);
                                    }
                                }

                                //ActiveChannel(ChannelNo);

                                //#region NA Initial
                                //if (! InitList.Contains(ChannelNo.ToString()))
                                //{
                                //    if (CalProcedure[iCal].ParameterType == "NF")
                                //        segmentTables[ChannelNo - 1].NFChannel = true;
                                //    InsertSegmentTableData(ChannelNo, segmentTables[ChannelNo - 1], true);//CHAMGE TO CHANGE CAL POWER - need new stuff
                                //    Operation_Complete();
                                //    SetCorrProperty(ChannelNo, false); //OFF CAL COOR - DONE
                                //    ChannelMax(ChannelNo, true);//ENA only
                                //    Thread.Sleep(100);
                                //    SelectSubClass(CalProcedure[iCal].ChannelNumber);
                                //    Operation_Complete();
                                //    SetCalKitLabel(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].CKitLabel); //TOPAZ DONE
                                //    Operation_Complete();
                                    
                                //    //*if (CalProcedure[iCal].ParameterType == "NF" & CalProcedure[iCal].CalStandard != naEnum.ECalibrationStandard.ECAL_OPEN*/)
                                //    if(CalProcedure[iCal].CalStandard == naEnum.ECalibrationStandard.NS_POWER)
                                //    {
                                //        InitNFCalChannel(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].NF_SrcPortNum,
                                //                        CalProcedure[iCal].Port1, CalProcedure[iCal].CKitLabel, EnrFile);
                                //        eCalKit= GetECalKitLabel();
                                //    }
                                //    else
                                //    SetCorrMethod(CalProcedure[iCal].ChannelNumber, GetCalPorts(ChannelNo, CalProcedure)
                                //        , GetConType(ChannelNo, CalProcedure)); //TOPAZ - INIT CAL HERE
                                //    InitList.Add(ChannelNo.ToString());
                                //}
                                //    #endregion

                                Thread.Sleep(100);
                                Operation_Complete();

                            }

                            SetFMState(ChannelNo, naEnum.EOnOff.Off);

                            switch (CalProcedure[iCal].CalStandard)
                            {
                                case naEnum.ECalibrationStandard.OPEN:
                                case naEnum.ECalibrationStandard.SHORT:
                                case naEnum.ECalibrationStandard.LOAD:
                                case naEnum.ECalibrationStandard.ECAL_OPEN:
                                case naEnum.ECalibrationStandard.ECAL_SHORT:
                                case naEnum.ECalibrationStandard.ECAL_LOAD:

                                    MeasCorr1PortStd(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].Port1, CalProcedure[iCal].CalKit, CalProcedure[iCal].CalStandard.ToString());

                                    Thread.Sleep(CalProcedure[iCal].Sleep);
                                    Operation_Complete();

                                    break;

                                case naEnum.ECalibrationStandard.THRU:
                                case naEnum.ECalibrationStandard.TRLLINE:
                                case naEnum.ECalibrationStandard.TRLREFLECT:
                                case naEnum.ECalibrationStandard.TRLTHRU:
                                case naEnum.ECalibrationStandard.UTHRU:
                                case naEnum.ECalibrationStandard.UnknownTHRU:

                                    MeasCorr2PortStd(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].Port1, CalProcedure[iCal].Port2, CalProcedure[iCal].CalKit, CalProcedure[iCal].CalStandard.ToString());

                                    Thread.Sleep(CalProcedure[iCal].Sleep);
                                    Operation_Complete();

                                    break;

                                case naEnum.ECalibrationStandard.ECAL_SAVE_A:
                                    string CalsetnameA = "";
                                    CalsetnameA = "NS_CalSet_A_CH" + CalProcedure[iCal].ChannelNumber;
                                    string cSetA = ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");

                                    if (cSetA.TrimEnd('\n') == "1")
                                    {
                                        SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                                    }
                                    SendCommand("SENS" + CalProcedure[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                                    Operation_Complete();
                                    break;

                                case naEnum.ECalibrationStandard.ECAL_SAVE_B:
                                    string eCalCH = "";
                                    string CalsetnameB = "";
                                    string NFrcvPortNum = CalProcedure[iCal].Port1.ToString(); // VNA Receiver port (DUT output port) for NF                                  

                                    // Specifies the Ecal Kit for Ecal Calibration
                                    SendCommand(":SENS" + CalProcedure[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                                    Operation_Complete();
                                    // Acquire Ecal Oopn/Short/Load standards
                                    SendCommand(":SENS" + CalProcedure[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                                    Operation_Complete();
                                    string dummy = ReadCommand("*OPC?");

                                    // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                                    CalsetnameB = "NS_CalSet_B_CH" + CalProcedure[iCal].ChannelNumber;
                                    string cSetB = ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                                    if (cSetB.TrimEnd('\n') == "1")
                                    {
                                        SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                                    }
                                    SendCommand(":SENS" + CalProcedure[iCal].ChannelNumber + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");


                                    // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                                    // portpair: Ecal thru port pair
                                    // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                                    // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                                    //SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original

                                    //This was the active line when I got it from Seoul - MM
                                    //SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");


                                    SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\"");

                                    // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                                    SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + CalProcedure[iCal].ChannelNumber + "\" , \"NS_CalSet_B_CH" + CalProcedure[iCal].ChannelNumber + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , LOG");
                                    // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                                    SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , LOG");


                                    // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                                    SendCommand(":CSET:FIXT:ENR:EMB \"" + EnrFile + "\" , \"Fixture_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + CalProcedure[iCal].NF_Input_Port  + "_" + CalProcedure[iCal].NF_Output_Port + ".enr\"");
                                    //SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + CalProcedure[iCal].Type + ".enr\"");
                                    //SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + CalProcedure[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + zz + "_" + NFrcvPortNum.Trim() + ".enr\""); // original
                                    Operation_Complete();
                                    break;

                                case naEnum.ECalibrationStandard.NS_POWER:
                                    // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                                    int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                                    SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");


                                    MeasCorr1PortStd(CalProcedure[iCal].ChannelNumber, CalProcedure[iCal].Port1, CalProcedure[iCal].CalKit, CalProcedure[iCal].CalStandard.ToString());

                                    Thread.Sleep(CalProcedure[iCal].Sleep);
                                    Operation_Complete();

                                    break;

                                default:
                                    msg = string.Format("Unrecognize Calibration Procedure = {0} for Cal Kit Standard {1}", 
                                        CalProcedure[iCal].CalStandard, CalProcedure[iCal].CalKit);
                                    DisplayError(msg, "Error in Normal Calibration Procedure");
                                    break;
                            }
                            #endregion

                        }

                        Thread.Sleep(100);
                        bNext = true;

                        // Disabled for handler.
                        //if (CalProcedure.Length - 1 == iCal)
                        //{
                        //    switch (MessageBox.Show("Do you want to re-measure STD?\n" + CalProcedure[pStep].Message, "Penang NPI", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        //    {
                        //        case DialogResult.No:
                        //            bNext = true;
                        //            break;
                        //        case DialogResult.Yes:
                        //            bNext = false;
                        //            iCal = pStep;
                        //            break;
                        //    }
                        //}                   
                        

                    } while (!bNext);

                    Averaging(ChannelNo, naEnum.EOnOff.Off);
                    //SetCorrProperty(ChannelNo, true);
                }

                msg = "NAAbstract-Calibrate() - SetupSegmentTable()";
                LoggingManager.Instance.LogInfoTestPlan(msg);

                SetupSegmentTable(segmentTables);
                
                for (int i = 0; i < InitList.Count; i++)
                {
                    int chNo = int.Parse(InitList[i]);
                    FMState = GetFMState(chNo);
                    SaveCorr(chNo);
                    SetFMState(chNo, FMState);
                }
                //SaveCorr(ChannelNo);
                DisplayOn(true);
                ChannelMax(ChannelNo, false);

            }

            
            DisplayMessage("Calibration Completed", "Calibration Complete");
        }

        private bool SetSwitchMatrixPathsPinot(string TxPort, string AntPort, string RxPort = null)
        {
            bool Sw_Test = true;

            Dictionary<string, EqLib.Operation> dicSwitchConfig = new Dictionary<string, EqLib.Operation>();
            dicSwitchConfig.Add("OUT-DRX", Operation.N3toRx);
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
                    if (TxPort.ToUpper() != "X") Eq.Site[0].SwMatrix.ActivatePath(TxPort, dicSwitchConfig[TxPort]);
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

        public SParam ConvertTouchStone(int TotalPort, SParam sParamData)
        {
            SParam tsSparam = new SParam();

            int totalport = TotalPort;
            int totalsparam = 1;
            int iCount = 0, i, x;

            totalsparam = totalport * totalport + (4*4*9); //add GDex and 

            
            tsSparam.SParamData = new SParamData[totalsparam];
            tsSparam.Freq = sParamData.Freq;
            tsSparam.NoPoints = sParamData.NoPoints;

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "S" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            totalport = 4;
            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "GDS" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "SSD" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "GDSSD" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "SDS" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "GDSDS" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "SSD" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "GDSSD" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "SSS" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 1; i <= totalport; i++)
            {
                for (x = 1; x <= totalport; x++)
                {
                    tsSparam.SParamData[iCount].SParamDef = (naEnum.ESParametersDef)
                        Enum.Parse(typeof(naEnum.ESParametersDef), "GDSSS" + i + x);
                    tsSparam.SParamData[iCount].SParam = CreateComplexNumbers(tsSparam.Freq.Length);
                    iCount++;
                }
            }

            for (i = 0; i < sParamData.SParamData.Length;i++)
            {
                for (x = 0;x<tsSparam.SParamData.Length;x++)
                {
                    if (tsSparam.SParamData[x].SParamDef == sParamData.SParamData[i].SParamDef)
                    {
                        tsSparam.SParamData[x].SParam = sParamData.SParamData[i].SParam;
                        x = tsSparam.SParamData.Length;
                    }
                }
            }

            return tsSparam;
        }

        internal virtual void InitNFCalChannel(int ChNo, int srcPortNum, int rcvPortNum,string ConnType, string KitName, string EnrFile) {}
        internal virtual string GetECalKitLabel() { return "Not Supported"; }
        private List<int> GetCalPorts(int ChannelNo, SCalibrationProcedure[] calProd)
        {
            //int[] result = new int[calProd.Length];

            List<int> calports = new List<int>();

            for (int i = 0; i < calProd.Length; i++)
            {
                if (!calports.Contains(calProd[i].Port1) & calProd[i].ChannelNumber == ChannelNo)
                {
                    calports.Add(calProd[i].Port1);
                }
            }

            return calports;
        }

        private List<string > GetConType (int ChannelNo, SCalibrationProcedure[] calProd)
        {
            List<string> ConType = new List<string>();

            for (int i = 0; i < calProd.Length; i++)
            {
                if (!ConType.Contains(calProd[i].ConnectorType) & calProd[i].ConnectorType != "" &
                    calProd[i].ChannelNumber == ChannelNo)
                {
                    ConType.Add(calProd[i].ConnectorType);
                }
            }
            return ConType;
        }

        private ComplexNumber[] CreateComplexNumbers(int count)
        {
            ComplexNumber[] result = new ComplexNumber[count];
            for (int i=0;i<count;i++)
            {
                result[i] = new ComplexNumber();
            }
            return result;
        }

        #endregion
    }
}

