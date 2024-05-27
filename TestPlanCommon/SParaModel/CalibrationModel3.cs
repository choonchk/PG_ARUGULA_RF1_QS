using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Avago.ATF.Logger;
using Avago.ATF.StandardLibrary;
using EqLib;
using LibFBAR_TOPAZ;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.DataType;
using LibPXI;
using MPAD_TestTimer;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// Main model for calibration. Still contains project specific switching codes.
    /// </summary>
    public class CalibrationModel3
    {
        //TODO Product specific switching in this class. Redesign Calibration classes.
        public bool b_Mode;
        private s_CalibrationProcedure[] Cal_Prod;

        private bool Flag = true;
        private int lastChannel = 1;
        //public int[] iPortMethod = new int[TotalChannel];
        private S_Param[] SParamData;
        private s_TraceMatching[] TraceMatch;
        private SParameterMeasurementDataModel m_model1;
        private int TotalChannel;
        private s_SegmentTable[] SegmentParam;
        private cENA ENA;
        private TopazEquipmentDriver m_equipmentNa;
        private EqSwitchMatrix.EqSwitchMatrixBase m_eqsm;
        private static string CalsetnameA; //Seoul
        private static string CalsetnameB;
        private static string AdapterSnP;
        private static string FixtureSnP;
        private static List<string> verificationTxt = new List<string>(); //Yoonchun for Sub cal verification
        private static List<string> verificationTxtAll = new List<string>(); //Yoonchun for Sub cal verification
        public List<string> ListOfENRfiles = new List<string>();

        public CalibrationInputDataObject DataObject { get; set; }

        /// <summary>
        /// To be improved - removing project specific.
        /// </summary>
        public TestPlanCommon.SParaModel.ProjectSpecificFactor.Projectbase ProjectSpecificSwitching
        {
            get
            {
                return ProjectSpecificFactor.cProject;
            }
        }

        public void Initialize(SParameterMeasurementDataModel model1, TopazEquipmentDriver equipment,
            EqSwitchMatrix.EqSwitchMatrixBase eqsm)
        {
            m_model1 = model1;
            SParamData = m_model1.SParamData;
            TraceMatch = m_model1.TraceMatch;
            TotalChannel = m_model1.SParamData.Length;
            SegmentParam = m_model1.SegmentParam;
            ENA = equipment.ENA;
            m_equipmentNa = equipment;
            m_eqsm = eqsm;
        }

        public s_CalibrationProcedure[] parse_Procedure
        {
            set
            {
                Cal_Prod = value;
            }
        }

        public void SetTrigger()
        {
            m_equipmentNa.SetTrigger(TotalChannel);
        }

        public void Verify_ECAL_procedure(string Channel_Num)
        {
            m_equipmentNa.Verify_ECAL_procedure(Channel_Num);
        }

        public void Save_StateFile(string Filename)
        {
            m_equipmentNa.Save_StateFile(Filename);
        }

        public void Save_StateFile_TotalChannel(string Filename)
        {
            m_equipmentNa.Save_StateFile(Filename, TotalChannel);
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

            for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
            {
                #region "Switch"



                #endregion

                //For switch
                Thread.Sleep(100);

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
                        DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
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
                            DisplayError(Cal_Prod[iCal]);
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
                            DisplayMessage2(Cal_Prod[iCal].CalType, tmpStr);
                            DisplayMessage2(Cal_Prod[iCal].CalType, tmpStr);
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            DisplayError(Cal_Prod[iCal]);
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

            DisplayMessageCalComplete();
        }

        public void Calibrate_Auto(CalibrationInputDataObject calDo)
        {
            string tmpStr;
            string handStr;
            bool bNext;
            int ChannelNo = 0;
            e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            bool[] AnalysisEnable = new bool[TotalChannel];
            int[] step = new int[TotalChannel];
            long currentSN = 0;


                

            try
            {
                currentSN = ATFCrossDomainWrapper.GetClothoCurrentSN();
            }
            catch
            { }

            if (calDo.iCalCount == 0 || currentSN == 1)
            {
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                        ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                    }

                    if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12))
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                        }
                    }
                }

                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);
                    ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                    //step[iChn] = int.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                }
            }

            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {
                #region "Switch"
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N1toTx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N2toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N3toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N4toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N5toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N6toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N7toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N8toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N9toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N10toRx);
                #endregion

                //For switch
                Thread.Sleep(100);

                #region "Calibration Message"
                if (Cal_Prod[iCal].Message.Trim() != "")
                {
                    tmpStr = Cal_Prod[iCal].Message;
                    // b_Mode = false;

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
                        case e_CalibrationType.UnknownTHRU:
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
                    //  b_Mode = true;
                }

                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                        {
                            // DisplayMessage2(Cal_Prod[iCal].CalType, tmpStr);                               

                            calDo.iCalCount = iCal;
                            calDo.CalContinue = true;
                            calDo.Fbar_cal = false;
                            break;
                        }
                        else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            calDo.CalContinue = false;
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    #region "Cal Kit Procedure"
                    switch (Cal_Prod[iCal].CalType)
                    {
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(100);
                            ENA.BasicCommand.System.Operation_Complete();
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
                        case e_CalibrationType.ISOLATION:
                            bool verifyPass = true;
                            int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                            double[] compData = new double[NOP];
                            int loopCount = 0;
                            string TrFormat = "";
                            double VerLimitLow = 0,
                                VerLimitHigh = 0,
                                maxVal = 0,
                                minVal = 0;

                            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                            {
                                verifyPass = false;
                                #region Math->Normalize
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.BasicCommand.System.Operation_Complete();
                                #endregion

                                #region resopnse cal using normalize
                                ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(100);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region response cal using average value of cal kit
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region varification response cal
                                Thread.Sleep(100);
                                ENA.Format.DATA(e_FormatData.REAL);
                                TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);

                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                VerLimitLow = -0.1;
                                VerLimitHigh = 0.1;
                                // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                // Array.Resize(ref FData, NOP);

                                compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                maxVal = compData.Max();
                                minVal = compData.Min();

                                //for (int j = 0; j < FData.Length; ++j)
                                //{
                                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                {
                                    verifyPass = true;
                                }
                                // }                                    
                                loopCount++;
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                #endregion
                            }
                            if (loopCount == 3)
                            {
                                string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}", 
                                    Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                DisplayCalProcedureError(errDesc);
                            }

                            break;
                        default:
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    Thread.Sleep(100);
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        //KCC - Autocal
                        if (Cal_Prod[iCal].Message.Trim() != "")
                        {
                            DisplayMessage2(Cal_Prod[iCal].CalType, tmpStr);
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                        {
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn],
                                ProjectSpecificSwitching.PortEnable);
                        }
                    }

                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;
                }
            }

            // DisplayMessageCalComplete();
        }

        // [Burhan] : Added for PCB CalSub AutoCal with CalKit Method
        public void Initialize_NA_Channel_Before_Autocal_For_CalKit_Method()
        {
            //e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            //bool[] AnalysisEnable = new bool[TotalChannel];
            //int[] step = new int[TotalChannel];

            for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
            {
                if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                {
                    ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                }
                if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12))
                {
                    ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                    if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                        ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                        ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                    }
                }
                else if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7))
                {
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                        ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                }
            }

            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
            }

        }

        // [Burhan] : Added for PCB CalSub AutoCal with CalKit Method
        public void Finalize_NA_Channel_After_Autocal_For_CalKit_Method()
        {
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                    ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
            }
        }

#if (false) // Subcal + NF cal using Fixed ENR old Method
            public void Calibrate_TOPAZ()
            {
                string tmpStr;
                string handStr;
                bool bNext;
                int ChannelNo = 0;
                string ENR_Path = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346C_15dB.enr";
                e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
                bool[] AnalysisEnable = new bool[TotalChannel];


        #region Topaz_Init

                if (Flag)
                {
                    ENA.Sense.Correction.Collect.GuidedCal.Delete_AllCalData();

                    int[] Cal_Check = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    // int[] Cal_Check = new int[] {0,0,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,0,0,1,1,1,0,0,1,1,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

                    int[] step = new int[TotalChannel];

                    for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                    {
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                        }
                        if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12))
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                            }
                        }
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                        {

                            string dummy;
                            int NFchNum = Cal_Prod[iCal].ChannelNumber; // Channel number for NF
                            int NFsrcPortNum = 2; // VNA Source port (DUT input port) for NF
                            int NFrcvPortNum = Cal_Prod[iCal].Port_1; // VNA Receiver port (DUT output port) for NF
                            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346C_15dB.enr"; // ENR file location
                            string NS_CalKitName = "85033D/E"; // Cal kit name (e.g Mechanical Cal kit 85033D/E), check if it matches DUT port connector type
                            string NS_DUTinPortConn = "APC 3.5 female"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            string NS_DUToutPortConn = "APC 3.5 female"; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            string CalKitName = Cal_Prod[iCal].CalKit_Name; //"Anakin"; // Define Cal kit name (e.g Ecal N4431B S/N 03605), check if it matches DUT port connector type
                            string DUTinPortConn = "ANT"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            string DUToutPortConn = Cal_Prod[iCal].Type; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);
                            dummy = ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                            dummy = dummy.Trim('\"', '\n');
                            string[] tr = dummy.Split(',');
                            ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);

                            // Setup for Guilded Calibration
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:ENR:FILENAME '" + ENRfile + "'"); // Load Noise Source ENR file Create Noise Figure class
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConn + "'"); // Define DUT input port connector type
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFrcvPortNum + " '" + DUToutPortConn + "'"); // Define DUT output port connector type
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT input port
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT output port
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CONN '" + DUToutPortConn + "'"); // Define Noise Source connector type
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CKIT '" + CalKitName + "'"); // Define Cal kit that will be used for the Noise Source adapter

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:METHOD 'Scalar'"); // Define the method for performing a calibration on a noise channel
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:RMEThod 'NoiseSource'"); // Define  the method used to characterize the noise receivers

                            ////ChoonChin - Added internal trigger command as requested by Takeshi
                            //ENA.BasicCommand.System.SendCommand(":TRIG:SOUR IMM");

                            // Start Guilde Calibration
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:INIT"); // Initiates a guided calibration
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?");  // Ask the Number of Calibration setps
                            int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());

                            string aa1 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 1);
                            string aa2 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 2);
                            string aa3 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 3);
                            string aa4 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 4);
                            string aa5 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 5);
                            string aa6 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 6);
                            string aa7 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 7);
                            string aa8 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 8);

                            if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " setting fail");

                        }
                    }

                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = int.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    Flag = false;
                }
        #endregion


                for (int iCal = iCalCount; iCal < Cal_Prod.Length; iCal++)
                {
        #region "Switch"

                    //if (Cal_Prod[iCal].ChannelNumber == 91)   // check point
                    //{
                    //    int calkit_numa =  Cal_Prod[iCal].CalKit;

                    //string read_value = ENA.Sense.ReadCommand( "SENS" +Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:STEP?");

                    //}

                    // if (Cal_Prod[iCal].ChannelNumber > 85) Thread.Sleep(200);
                    Thread.Sleep(200);

                    ProductionLib.SwitchMatrix2.Maps2.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(),  ProductionLib.Operation2.ENAtoRFIN);
                    ProductionLib.SwitchMatrix2.Maps2.Activate(Cal_Prod[iCal].Switch_Ant,  ProductionLib.Operation2.ENAtoRFOUT);
                    ProductionLib.SwitchMatrix2.Maps2.Activate(Cal_Prod[iCal].Switch_Rx,  ProductionLib.Operation2.ENAtoRX);
                    // Cal_Prod[iCal].ChannelNumber
                    if (Cal_Prod[iCal].ChannelNumber > 85)
                    {

                        //string aswitch_ANT ="B39";
                        //string aswitch_Rx = "MB2";
                        //InstrLib.SwitchMatrix.Maps.Activate(aswitch_ANT, InstrLib.Operation.ENAtoRFOUT);
                        //InstrLib.SwitchMatrix.Maps.Activate(aswitch_Rx, InstrLib.Operation.ENAtoRX);

                    }
        #endregion

                    //For switch
                    Thread.Sleep(200);

                    string Isolation_trace1 = ENA.Sense.Correction.Collect.GuidedCal.Select_Trace(Cal_Prod[iCal].ChannelNumber);
                    Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                    string[] tr = Isolation_trace1.Split(',');
                    int Isolation_trace = Convert.ToInt16(tr[0]);

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
                            case e_CalibrationType.UnknownTHRU:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                                if (Cal_Prod[iCal].ChannelNumber > 85)
                                {
                                    Thread.Sleep(200);
                                }
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
                            case e_CalibrationType.NS_POWER:
                                tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                                b_Mode = false;
                                break;
                        }
                        //  b_Mode = true;
                    }

        #endregion

                    if (Cal_Prod[iCal].b_CalKit)
                    {
                        if (!b_Mode)
                        {
                            if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                                {
                                    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                }
                            }
                            else
                            {
                                if (Cal_Prod[iCal].Message.Trim() != "" && !CalContinue)
                                {

                                    iCalCount = iCal;
                                    CalContinue = true;
                                    Fbar_cal = false;

                                    break;
                                }
                                if (Cal_Prod[iCal].Message.Trim() != "" && CalContinue)
                                {
                                    if (Cal_Prod[iCal].CalType.ToString() == "OPEN")
                                    {
                                        DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                    }
                                    else if (Cal_Prod[iCal].CalType.ToString() == "SHORT")
                                    {
                                        //DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                    }
                                    else if (Cal_Prod[iCal].CalType.ToString() == "LOAD")
                                    {
                                        //DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                    }
                                    else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                    {
                                        //DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                    }
                                    else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                    {
                                        //DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                    }
                                    CalContinue = false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
                            }

                        }
        #region "Cal Kit Procedure"

                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.OPEN:
                            case e_CalibrationType.SHORT:
                            case e_CalibrationType.LOAD:
                            case e_CalibrationType.THRU:
                            case e_CalibrationType.NS_POWER:
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);

                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;

                            case e_CalibrationType.UnknownTHRU:
                                if (Cal_Prod[iCal].ChannelNumber >= 85)
                                {
                                    int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());
                                    if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " thru cal fail \n\n" + tmpStr);
                                }
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);


                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();

                                break;

                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.ISOLATION:
                                bool verifyPass = true;
                                int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                                double[] compData = new double[NOP];
                                int loopCount = 0;
                                string TrFormat = "";
                                double VerLimitLow = 0,
                                    VerLimitHigh = 0,
                                    maxVal = 0,
                                    minVal = 0;

                                while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                                {
                                    verifyPass = false;
        #region Math->Normalize
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.BasicCommand.System.Operation_Complete();
        #endregion

        #region resopnse cal using normalize
                                    ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    Thread.Sleep(100);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
        #endregion

        #region response cal using average value of cal kit
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    //ENA.BasicCommand.System.Operation_Complete();
                                    //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
        #endregion

        #region varification response cal
                                    Thread.Sleep(100);
                                    ENA.Format.DATA(e_FormatData.REAL);
                                    TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                    //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                    Thread.Sleep(100);
                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    VerLimitLow = -0.1;
                                    VerLimitHigh = 0.1;
                                    // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                    // Array.Resize(ref FData, NOP);

                                    compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                    maxVal = compData.Max();
                                    minVal = compData.Min();

                                    //for (int j = 0; j < FData.Length; ++j)
                                    //{
                                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                    {
                                        verifyPass = true;
                                    }
                                    // }                                    
                                    loopCount++;
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
        #endregion
                                }
                                if (loopCount == 3)
                                {
                                    string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}", 
                                        Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                    DisplayCalProcedureError(errDesc);
                                }
  
                                break;
                            default:
                                DisplayError(Cal_Prod[iCal]);
                                break;
                        }
                        //Thread.Sleep(100);
                        ENA.BasicCommand.System.Operation_Complete();
        #endregion
                    }
                    else
                    {
                        if (!b_Mode)
                        {
                            if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                                {
                                    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                }
                            }
                            else
                            {
                                if (Cal_Prod[iCal].Message.Trim() != "" && !CalContinue)
                                {
                                    iCalCount = iCal;
                                    CalContinue = true;
                                    Fbar_cal = false;

                                    break;
                                }
                                else if (Cal_Prod[iCal].Message.Trim() != "" && CalContinue)
                                {
                                    CalContinue = false;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
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
                                    if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                        ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                    else
                                        ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                    break;
                                case 4:
                                    ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                    break;

                            }
                            //Thread.Sleep(500);
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
                                    Thread.Sleep(4000);
                                }
                                Thread.Sleep(500);
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
                                Thread.Sleep(300);
                                ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.THRU:
                                ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                Thread.Sleep(300);
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
                                DisplayError(Cal_Prod[iCal]);
                                break;
                        }
        #endregion
                        Thread.Sleep(200);
                    }

                    if (iCal == Cal_Prod.Length - 1 && !CalContinue)
                    {
                        for (int iChn = 0; iChn < TotalChannel; iChn++)
                        {
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                        }

                        CalDone = true;
                        Fbar_cal = false;

                 //       DisplayMessageCalComplete();
                    }

                }


                //for (int iChn = 0; iChn < TotalChannel; iChn++)
                //{
                //    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
                //        ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                //}



            }

#endif
        public void Calibrate_TOPAZ(CalibrationInputDataObject calDo, int topazCalPower, string ENRfile)
        {
            string tmpStr;
            string handStr;
            bool bNext;
            int ChannelNo = 0;
            e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            bool[] AnalysisEnable = new bool[TotalChannel];

            ENA.BasicCommand.System.Clear_Status(); //MM - clearing the status of the event registers to isolate errors from calibration


            /////////////
            string eCalKit = "";
            string[] listnum = ENA.BasicCommand.ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?").Split(',');
            if (int.Parse(listnum[0]) > 0)
            {
                string[] EcalList = new string[listnum.Length];

                for (int i = 0; i < listnum.Length; ++i)
                {
                    listnum[i] = int.Parse(listnum[i]).ToString();
                    string[] EcalInfo = ENA.BasicCommand.ReadCommand("SENS:CORR:CKIT:ECAL" + listnum[i] + ":INF?").Split(',', ':');
                    EcalList[i] = EcalInfo[1].Trim() + " ECal " + EcalInfo[3].Trim();
                    // Example of return value: 
                    // "ModelNumber: N4431B, SerialNumber: 03605, ConnectorType: 35F 35F 35F 35F, PortAConnector: APC 3.5 female, PortBConnector: APC 3.5 female, PortCConnector: APC 3.5 female, PortDConnector: APC 3.5 female, MinFreq: 9000, MaxFreq: 13510000000, NumberOfPoints: 336, Calibrated: 19/Dec/2007, CharacterizedBy: 0000099999, NetworkAnalyzer: US44240045"
                    eCalKit = EcalList[0];
                }
            }
            else
            {
                //If Ecalkit not existed, message will be shown   
            }
            /////////////


            #region Topaz_Init

            if (Flag)
            {
                //Used to be commented out by MM (06/22/2017) - ECAL is being used in combination with subcal.  This line keeps blowing the ecal away
                ENA.Sense.Correction.Collect.GuidedCal.Delete_AllCalData();

                // int[] Cal_Check = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                // int[] Cal_Check = new int[] {0,0,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,0,0,1,1,1,0,0,1,1,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

                int[] step = new int[TotalChannel];

                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {

                    if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, 
                        Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                    if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                    {
                        if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5))) //This is asking if the calkit number is NOT that of a SHORT
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                            ENA.BasicCommand.System.Operation_Complete();
                        }
                    }
                    if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12)) //This is asking if the calkit is that of an UNKNOWN THRU for a 3 port (Thru for 2 -3)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                            ENA.BasicCommand.System.Operation_Complete();
                        }
                    }
                    if ((Cal_Prod[iCal].No_Ports == 4) && (Cal_Prod[iCal].CalKit == 18)) //This is asking if the calkit is that of an UNKNOWN THRU for a 4 port 
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                        {
                            //For better accuracy purposes for define thru cal
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_4);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_4);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                            ENA.BasicCommand.System.Operation_Complete();
                        }
                    }
                    if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7)) //Is the calkit that of an UNKNOWN THRU for a 2 port
                    {
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                        ENA.BasicCommand.System.Operation_Complete();
                    }//Newrly added

                    #region If e_CalibrationType.NS_POWER
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                    {

                        string dummy;
                        int NFchNum = Cal_Prod[iCal].ChannelNumber; // Channel number for NF
                        int NFsrcPortNum = Cal_Prod[iCal].NF_SrcPortNum; ; // 2; // VNA Source port (DUT input port) for NF //Need to change next time
                        int NFrcvPortNum = Cal_Prod[iCal].Port_1; // VNA Receiver port (DUT output port) for NF
                                                                  //string NS_CalKitName = "85033D/E"; // Cal kit name (e.g Mechanical Cal kit 85033D/E), check if it matches DUT port connector type
                                                                  //string NS_DUTinPortConn = "APC 3.5 female"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                                                                  //string NS_DUToutPortConn = "APC 3.5 female"; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                        string CalKitName = Cal_Prod[iCal].CalKit_Name; //"Anakin"; // Define Cal kit name (e.g Ecal N4431B S/N 03605), check if it matches DUT port connector type
                        string DUTinPortConn = Cal_Prod[iCal].DUT_NF_InPort_Conn; // "ANT1"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type //Need to Change
                        string DUToutPortConn = Cal_Prod[iCal].DUT_NF_OutPort_Conn; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                                                                                    //string DUTinPortConn = "ANT1"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type //Need to Change
                                                                                    //string DUToutPortConn = Cal_Prod[iCal].Type; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);
                        dummy = ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                        dummy = dummy.Trim('\"', '\n');
                        string[] tr = dummy.Split(',');
                        ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER:COUN 20");
                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER ON");
                        string AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":AVER?");

                        // Setup for ENR load ---------------------------------
                        //switch (NFrcvPortNum)
                        //{
                        //    case 3:
                        //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_MB1.enr"; // ENR file location
                        //        break;
                        //    case 4:
                        //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_MB2.enr"; // ENR file location
                        //        break;
                        //    case 5:
                        //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_HB1.enr"; // ENR file location
                        //        break;
                        //    case 6:
                        //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_HB2.enr"; // ENR file location
                        //        break;
                        //}


                        // Setup for Guilded Calibration ---------------------------------


                        // Setup for Guilded Calibration
                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConn + "'"); // Define DUT input port connector type
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFrcvPortNum + " '" + DUToutPortConn + "'"); // Define DUT output port connector type
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT input port
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT output port
                        ENA.BasicCommand.System.Operation_Complete();


                        ENA.BasicCommand.System.SendCommand(":SENS" + NFchNum + ":CORR:COLL:GUID:PATH:TMET " + NFsrcPortNum + "," + NFrcvPortNum + "," + " \"Undefined Thru\""); // Define Cal kit for DUT output port
                        ENA.BasicCommand.System.Operation_Complete();


                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CONN '" + DUToutPortConn + "'"); // Define Noise Source connector type
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CKIT '" + CalKitName + "'"); // Define Cal kit that will be used for the Noise Source adapter
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:ENR:FILENAME '" + ENRfile + "'"); // Load Noise Source ENR file Create Noise Figure class
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:METHOD 'Scalar'"); // Define the method for performing a calibration on a noise channel
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:RMEThod 'NoiseSource'"); // Define  the method used to characterize the noise receivers
                        ENA.BasicCommand.System.Operation_Complete();



                        #region Commented Out
                        // Start Guilde Calibration
                        //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:INIT"); // Initiates a guided calibration
                        ////ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?");  // Ask the Number of Calibration setps
                        //int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());

                        //string aa1 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 1);
                        //string aa2 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 2);
                        //string aa3 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 3);
                        //string aa4 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 4);
                        //string aa5 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 5);
                        //string aa6 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 6);
                        //string aa7 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 7);
                        //string aa8 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 8);

                        //if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " setting fail"); 
                        #endregion Commented Out

                    }
                    #endregion If e_CalibrationType.NS_POWER


                    // New for Setup full 1-port cal for noise source ENR characterization
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN)
                    {
                        ENA.BasicCommand.System.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:CONN:PORT" + (Cal_Prod[iCal].Port_1) + ":SEL " + "\"" + Cal_Prod[iCal].Type + "\"");
                        ENA.BasicCommand.System.Operation_Complete();
                        ENA.BasicCommand.System.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:CKIT:PORT" + (Cal_Prod[iCal].Port_1) + ":SEL " + "\"" + Cal_Prod[iCal].CalKit_Name + "\""); // Use Cal Kit "Anakin"
                    }
                    // End New for Setup full 1-port cal for noise source ENR characterization

                } // for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)

                ////// Create int array Cal_Check[] for guided calibration channel check (20161206)

                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN))
                    {
                        Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    Cal_Check[ch - 1] = 1;
                }
                ///////////////////////////////////////////////////////////////////////////////////////

                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);

                    ////// Send InitGuidedCal() only if Cal_Check[iChn] == 1 (20161206)
                    if (Cal_Check[iChn] == 1)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = int.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    else
                    {
                        step[iChn] = 0;
                    }
                    //
                }
                Flag = false;
            } //if (Flag)
            #endregion Topaz_Init


            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {

                try
                {
                    #region "Switch"
                    if (Cal_Prod[iCal].ParameterType != "NF")
                    {
                        Thread.Sleep(200);

                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), EqLib.Operation.N1toTx);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Ant, EqLib.Operation.N2toAnt);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.N3toRx);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.N4toRx);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.N5toRx);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.N6toRx);
                    }
                    else
                    {
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), EqLib.Operation.ENAtoRFIN);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Ant, EqLib.Operation.ENAtoRFOUT);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.ENAtoRX);
                    }
                    //Thread.Sleep(200);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), clsSwitchMatrix.Operation2.N1toTx, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Ant, clsSwitchMatrix.Operation2.N2toAnt, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Rx, clsSwitchMatrix.Operation2.N3toRx, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Rx2, clsSwitchMatrix.Operation2.N4toRx, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Rx3, clsSwitchMatrix.Operation2.N5toRx, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Rx4, clsSwitchMatrix.Operation2.N6toRx, SwitchesDictionary_Global);

                    //Revisit this idea after getting the switch mapping right
                    //zProcessSwitchSettings(Cal_Prod[iCal].Switch_Rx);

                    //Previous implement - commented out and saved just for reference
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), clsSwitchMatrix.Operation2.ENAtoRFIN, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Ant, clsSwitchMatrix.Operation2.ENAtoRFOUT, SwitchesDictionary_Global);
                    //clsSwitchMatrix.Maps2.Activate(Cal_Prod[iCal].Switch_Rx, clsSwitchMatrix.Operation2.ENAtoRX, SwitchesDictionary_Global);

                    Thread.Sleep(200);


                    #endregion "Switch"
                }
                catch (Exception e)
                {

                    MessageBox.Show("The Parameter Type is" + Cal_Prod[iCal].ParameterType + "and the Band is " + Cal_Prod[iCal].Switch_Input.ToUpper()
                        + "\r\n");
                }

                //For switch

                string Isolation_trace1 = ENA.Sense.Correction.Collect.GuidedCal.Select_Trace(Cal_Prod[iCal].ChannelNumber);
                Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                string[] tr = Isolation_trace1.Split(',');
                int Isolation_trace = Convert.ToInt16(tr[0]);

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
                        case e_CalibrationType.ECAL_LOAD:
                        case e_CalibrationType.ECAL_SAVE_A:
                        case e_CalibrationType.ECAL_SAVE_B:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();

                            break;
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.ECAL_SHORT:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.SUBCLASS:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                            if (Cal_Prod[iCal].ChannelNumber > 85)
                            {
                                Thread.Sleep(200);
                            }
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
                        case e_CalibrationType.NS_POWER:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            b_Mode = false;
                            break;
                    }
                    //  b_Mode = true;
                }

                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {

                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                //Commented all this out to remove not get the prompts between loads - MM - 08262017
                                //if (Cal_Prod[iCal].CalType.ToString() == "OPEN" || Cal_Prod[iCal].CalType.ToString() == "ECAL_OPEN")
                                //{
                                //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "SHORT" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SHORT")
                                //{
                                //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SAVE")
                                //{
                                //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                //{
                                //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                //{
                                //    General.DisplayMessage(ClassName + " --> " + SubClass, Cal_Prod[iCal].CalType.ToString() + " Calibration (Cal Kit)", tmpStr);
                                //}
                                Thread.Sleep(3000);
                                calDo.CalContinue = false;
                            }



                            else
                            {
                                Thread.Sleep(100);
                            }
                        } //else

                    } // if (!b_Mode)

                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);

                    #region "Cal Kit Procedure"

                    switch (Cal_Prod[iCal].CalType)
                    {
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.ECAL_OPEN:
                        case e_CalibrationType.ECAL_SHORT:
                        case e_CalibrationType.ECAL_LOAD:
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(200);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;

                        case e_CalibrationType.ECAL_SAVE_A:
                            //////////// This is new ///////////////
                            // Save full 1-port guided calibration for noise source ENR characterization, save “CalSet_A”                             
                            string CalsetnameA = "";
                            CalsetnameA = "NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber;
                            string cSetA = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");

                            if (cSetA.TrimEnd('\n') == "1")
                            {
                                ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                            }
                            ENA.BasicCommand.SendCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                            ENA.BasicCommand.System.Operation_Complete();
                            break;

                        case e_CalibrationType.ECAL_SAVE_B:

                            string eCalCH = "";
                            string CalsetnameB = "";
                            string NFrcvPortNum = Cal_Prod[iCal].Port_1.ToString(); // VNA Receiver port (DUT output port) for NF

                            // Perform full 1-port calibration at Ecal side using the same frequency points, save “CalSet_B”
                            //switch (NFrcvPortNum)
                            //{
                            //    case "3":
                            //        eCalCH = "71";
                            //        break;
                            //    case "4":
                            //        eCalCH = "72";
                            //        break;
                            //    case "5":
                            //        eCalCH = "73";
                            //        break;
                            //    case "6":
                            //        eCalCH = "74";
                            //        break;
                            //}


                            for (int x = 0; x < 3; x++)
                            {
                                // Specifies the Ecal Kit for Ecal Calibration
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                                ENA.BasicCommand.System.Operation_Complete();
                                // Acquire Ecal Oopn/Short/Load standards
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                                ENA.BasicCommand.System.Operation_Complete();
                                string dummy = ENA.BasicCommand.ReadCommand("*OPC?");

                                string Ecalstat = ENA.BasicCommand.System.Event_Register(false);

                                if (Ecalstat != "No Error") //Non-zero return means an error(s) happened
                                {
                                    MessageBox.Show("An error occurred during Calibrate_Topaz(): " + "\r\n" + Ecalstat + "\r\n" +
                                        "Error happened during Ecal of CH" + Cal_Prod[iCal].ChannelNumber + " for Port" + NFrcvPortNum + "\r\n");
                                    if (x > 2)
                                    {
                                        throw new Exception("Calibrate_Topaz() error " + "\r\n" + Ecalstat);
                                    }
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    break;
                                }
                                //Added this line here to isolate individual channel errors from performing ecal
                                ENA.BasicCommand.System.Clear_Status(); //MM - clearing the status of the event registers to isolate errors from calibration

                            }
                            // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                            CalsetnameB = "NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber;
                            string cSetB = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                            if (cSetB.TrimEnd('\n') == "1")
                            {
                                ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                            }
                            ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");


                            // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                            // portpair: Ecal thru port pair
                            // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                            // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                            //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original

                            //This was the active line when I got it from Seoul - MM
                            //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");


                            ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\"");

                            // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                            ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber + "\" , \"NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");

                            //// Calculate adapter data from "CalSet_B_CH*" and "CalSet_A_CH*", save "Adapter_CH*.s2p"
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber + "\" , \"NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");


                            // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                            ENA.BasicCommand.SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");


                            // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                            ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr\"");
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr\"");
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + zz + "_" + NFrcvPortNum.Trim() + ".enr\""); // original
                            ENA.BasicCommand.System.Operation_Complete();

                            ListOfENRfiles.Add("NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr");


                            break;



                        case e_CalibrationType.NS_POWER:
                            // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                            int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                            ENA.BasicCommand.SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");


                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(2000);
                            ENA.BasicCommand.System.Operation_Complete();

                            break;


                        case e_CalibrationType.UnknownTHRU:
                            //if (Cal_Prod[iCal].ChannelNumber >= 85)
                            //{
                            //    int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());
                            //    if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " thru cal fail \n\n" + tmpStr);
                            //}
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(200);
                            ENA.BasicCommand.System.Operation_Complete();

                            break;

                        case e_CalibrationType.TRLLINE:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.TRLREFLECT:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.TRLTHRU:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.ISOLATION:
                            bool verifyPass = true;
                            int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                            double[] compData = new double[NOP];
                            int loopCount = 0;
                            string TrFormat = "";
                            double VerLimitLow = 0,
                                VerLimitHigh = 0,
                                maxVal = 0,
                                minVal = 0;

                            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                            {
                                verifyPass = false;
                                #region Math->Normalize
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.BasicCommand.System.Operation_Complete();
                                #endregion

                                #region resopnse cal using normalize
                                ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(100);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region response cal using average value of cal kit
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region varification response cal
                                Thread.Sleep(100);
                                ENA.Format.DATA(e_FormatData.REAL);
                                TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                Thread.Sleep(100);
                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                VerLimitLow = -0.1;
                                VerLimitHigh = 0.1;
                                // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                // Array.Resize(ref FData, NOP);

                                compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                maxVal = compData.Max();
                                minVal = compData.Min();

                                //for (int j = 0; j < FData.Length; ++j)
                                //{
                                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                {
                                    verifyPass = true;
                                }
                                // }                                    
                                loopCount++;
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                #endregion
                            }
                            if (loopCount == 3)
                            {
                                string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                    Cal_Prod[iCal].ChannelNumber, Isolation_trace, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                DisplayCalProcedureError(errDesc);
                            }

                            break;
                        default:
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    //Thread.Sleep(100);
                    ENA.BasicCommand.System.Operation_Complete();
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        //Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            Thread.Sleep(300);
                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.THRU:
                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(300);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {

                    ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                    int[] Cal_Check = new int[TotalChannel];
                    List<int> Cal_Check_list = new List<int>();
                    Cal_Check_list.Clear();
                    for (int i = 0; i < Cal_Prod.Length; i++)
                    {
                        if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                        {
                            Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                        }
                    }
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        Cal_Check[iChn] = 0;
                    }

                    foreach (int ch in Cal_Check_list)
                    {
                        Cal_Check[ch - 1] = 1;
                    }
                    //////////////////////////////


                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                        if (Cal_Check[iChn] == 1)
                        {
                            string newENRfileName = "";
                            string chName = ENA.BasicCommand.ReadCommand("SENS" + (iChn + 1) + ":CLAS:NAME?");

                            //chName = Cal_Prod[iChn].ParameterType;


                            //The disconnect here is that the determination if a given channel is a Noise figure channel and the indexing for the NF input and output port do not come from the same place.  Have to find the index of the channel to get the appropriate INput and Output port designation.
                            //The question is, where do we get this information
                            //if (chName.Contains("Noise"))
                            if (chName.Contains("Noise"))
                            {
                                string NFrcvPortNum = ENA.BasicCommand.ReadCommand(":SENS" + (iChn + 1) + ":NOIS:PMAP:OUTP?").TrimEnd('\n').Replace("+", ""); // NF Receiver port (DUT output port)
                                string RxPath = "";
                                string zBandInfo = Cal_Prod[iCal].Switch_Input;

                                int zindex = 0;
                                for (int i = 0; i < Cal_Prod.Length; i++)
                                {
                                    //Get the Channel Number, Port_1 value, DUT NF Input Port Conn and DUT NF Output Port Conn
                                    if (Cal_Prod[i].Port_1.ToString() == NFrcvPortNum && Cal_Prod[i].ChannelNumber == iChn + 1)
                                    {
                                        zindex = i;
                                        break;

                                    }
                                }




                                switch (NFrcvPortNum)
                                {
                                    case "3":
                                        RxPath = "MB1";
                                        break;

                                    case "4":
                                        RxPath = "HB2";
                                        break;

                                    case "5":
                                        RxPath = "HB1";
                                        break;

                                    case "6":
                                        RxPath = "HB2";
                                        break;
                                }

                                ENA.BasicCommand.System.SendCommand("SENS" + (iChn + 1) + ":AVER OFF");
                                string AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + (iChn + 1) + ":AVER?");

                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath +".enr";
                                newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[zindex].DUT_NF_InPort_Conn + "_" + Cal_Prod[zindex].DUT_NF_OutPort_Conn + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + "_" + NFrcvPortNum.Trim() + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + zBandInfo + "_" + NFrcvPortNum.Trim() + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + ".enr";
                                ENA.BasicCommand.SendCommand(":SENS" + (iChn + 1) + ":NOIS:ENR:FILENAME '" + newENRfileName + "'"); // Load Characterized Noise Source ENR file
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.BasicCommand.SendCommand("SENS" + (iChn + 1) + ":CORR:COLL:GUID:SAVE");
                                ENA.BasicCommand.System.Operation_Complete();
                            }

                            else
                                ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            {
                                Thread.Sleep(500);
                                //Set segment power back to -20 dBm - modified by CheeOn 17-July-2017
                                if (Cal_Prod[iCal].ParameterType != "NF")
                                {
                                    ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], ProjectSpecificSwitching.PortEnable);
                                }

                            }
                        }

                        //
                    }
                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;

                    //       General.DisplayMessage(ClassName + " --> " + SubClass, "Calibration Completed", "Calibration Complete");
                } //if (iCal == Cal_Prod.Length - 1 && !CalContinue)

            } //for (int iCal = iCalCount; iCal < Cal_Prod.Length; iCal++)

            #region Commented Out
            //for (int iChn = 0; iChn < TotalChannel; iChn++)
            //{
            //    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
            //        ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
            //} 
            #endregion Commented Out

        } //Calibrate_Topaz

        public void Calibrate_TOPAZ(CalibrationInputDataObject calDo, string ENR_file, int topazCalPower) //SEOUL
        {
            string tmpStr;
            //string handStr;
            //bool bNext;
            //int ChannelNo = 0;
            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/" + ENR_file; //ENR_NC346D_22dB_AE236.enr";//ENR_NC346C_15dB.enr"; // ENR file location

            e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            bool[] AnalysisEnable = new bool[TotalChannel];

            ///////////////Seoul
            string eCalKit = "";
            string[] listnum = ENA.Sense.Correction.Collect.ECAL.ModuleList().Split(','); //ENA.BasicCommand.ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?").Split(',');
            if (Int32.Parse(listnum[0]) > 0)
            {
                string[] EcalList = new string[listnum.Length];

                for (int i = 0; i < listnum.Length; ++i)
                {
                    listnum[i] = Int32.Parse(listnum[i]).ToString();
                    string[] EcalInfo = ENA.Sense.Correction.Collect.ECAL.ModuleInfo(listnum[i]).Split(',', ':'); // ENA.BasicCommand.ReadCommand("SENS:CORR:CKIT:ECAL" + listnum[i] + ":INF?").Split(',', ':');
                    EcalList[i] = EcalInfo[1].Trim() + " ECal " + EcalInfo[3].Trim();
                    // Example of return value: 
                    // "ModelNumber: N4431B, SerialNumber: 03605, ConnectorType: 35F 35F 35F 35F, PortAConnector: APC 3.5 female, PortBConnector: APC 3.5 female, PortCConnector: APC 3.5 female, PortDConnector: APC 3.5 female, MinFreq: 9000, MaxFreq: 13510000000, NumberOfPoints: 336, Calibrated: 19/Dec/2007, CharacterizedBy: 0000099999, NetworkAnalyzer: US44240045"
                    eCalKit = EcalList[0];
                }
            }
            else
            {//Seoul
                MessageBox.Show("Please check Ecal Module");//If Ecalkit not existed, message will be shown   
            }
            /////////////


            #region Topaz_Init

            if (Flag)
            {
                //Used to be commented out by MM (06/22/2017) - ECAL is being used in combination with subcal.  This line keeps blowing the ecal away
                ENA.Sense.Correction.Collect.GuidedCal.Delete_AllCalData();
                ENA.BasicCommand.System.Operation_Complete();

                int[] step = new int[TotalChannel];

                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {//Seoul
                    switch (Cal_Prod[iCal].No_Ports)
                    {
                        case 3:
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                            break;
                        case 4:
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                            break;
                        case 5:
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4, Cal_Prod[iCal].Port_5);
                            break;
                        case 6:
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4, Cal_Prod[iCal].Port_5, Cal_Prod[iCal].Port_6);
                            break;
                        default:
                            break;
                    }

                    if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                    switch (Cal_Prod[iCal].CalType) //Seoul
                    {
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                            //if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5)))
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                        }
                            break;

                        case e_CalibrationType.NS_POWER:
                            string dummy;
                            //string tempport;
                            //if (Cal_Prod[iCal].ChannelNumber == 38 || Cal_Prod[iCal].ChannelNumber == 42)
                            //    tempport = "HP_TDD_ANT";
                            //else tempport = "ANT";

                            Cal_Prod[iCal].Port_1 = Convert.ToInt16(ENA.Sense.Noise.InputPort(Cal_Prod[iCal].ChannelNumber));
                            Cal_Prod[iCal].Port_2 = Convert.ToInt16(ENA.Sense.Noise.OutputPort(Cal_Prod[iCal].ChannelNumber));
                            ENA.Sense.Noise.PortMapping(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2); //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);                            

                            //string antConnectorType = "";
                            //switch (Cal_Prod[iCal].Port_1.ToString())
                            //{
                            //    case "3": antConnectorType = "ANT1"; break;
                            //    case "4": antConnectorType = "ANT2"; break;
                            //    case "5": antConnectorType = "ANT3"; break;
                            //}

                            dummy = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber); //ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                            dummy = dummy.Trim('\"', '\n');
                            string[] tr = dummy.Split(',');

                            ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Convert.ToInt32(tr[0])); //ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);
                            ENA.Sense.Average.Count(Cal_Prod[iCal].ChannelNumber, 10);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER:COUN 20");
                            ENA.Sense.Average.State(Cal_Prod[iCal].ChannelNumber, e_OnOff.On);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER ON");
                            ENA.BasicCommand.System.Operation_Complete();

                            //SEOUL
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit_Name);

                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].DUT_NF_InPort_Conn);
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Type);
                            ENA.BasicCommand.System.Operation_Complete();
                                                                

                            //ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, tempport);
                            //ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Type);

                            //    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);                                    
                            //    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit_Name);
                            //    ENA.BasicCommand.System.Operation_Complete();
                            //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2); 


                            ENA.Sense.Noise.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Type);
                            ENA.Sense.Noise.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit_Name);
                            ENA.Sense.Noise.LoadENRfile(Cal_Prod[iCal].ChannelNumber, ENRfile);
                            ENA.Sense.Noise.SelectCalMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.Scalar);
                            ENA.Sense.Noise.SelectRecMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.NoiseSource);
                            ENA.Sense.Noise.Temperature(Cal_Prod[iCal].ChannelNumber, 302.8f); //Lindsay

                            //ENA.BasicCommand.System.Operation_Complete();
                            // MessageBox.Show("");

                            break;
                        case e_CalibrationType.THRU:

                            break;
                        case e_CalibrationType.UnknownTHRU:
                            ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            break;

                    }

                    ENA.BasicCommand.System.Operation_Complete();
#if false //SEOUL
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                        {
                            //if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5)))
                            //{
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                            //}
                        }
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                        {

                            string dummy;
                            //int NFchNum = Cal_Prod[iCal].ChannelNumber; // Channel number for NF
                            //int NFsrcPortNum = 2; // VNA Source port (DUT input port) for NF
                            //int NFrcvPortNum = Cal_Prod[iCal].Port_1; // VNA Receiver port (DUT output port) for NF
                            //string NS_CalKitName = "85033D/E"; // Cal kit name (e.g Mechanical Cal kit 85033D/E), check if it matches DUT port connector type
                            //string NS_DUTinPortConn = "APC 3.5 female"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            //string NS_DUToutPortConn = "APC 3.5 female"; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            //string CalKitName = Cal_Prod[iCal].CalKit_Name; //"Anakin"; // Define Cal kit name (e.g Ecal N4431B S/N 03605), check if it matches DUT port connector type
                            //string DUTinPortConn = "ANT"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            //string DUToutPortConn = Cal_Prod[iCal].Type; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type


                            Cal_Prod[iCal].Port_1 = Convert.ToInt16(ENA.Sense.Noise.InputPort(Cal_Prod[iCal].ChannelNumber));
                            Cal_Prod[iCal].Port_2 = Convert.ToInt16(ENA.Sense.Noise.OutputPort(Cal_Prod[iCal].ChannelNumber));
                            ENA.Sense.Noise.PortMapping(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2); //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);                            
                            
                            dummy = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber); //ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                            dummy = dummy.Trim('\"', '\n');
                            string[] tr = dummy.Split(',');

                            ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Convert.ToInt32(tr[0])); //ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);
                            ENA.Sense.Average.Count(Cal_Prod[iCal].ChannelNumber, 20);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER:COUN 20");
                            ENA.Sense.Average.State(Cal_Prod[iCal].ChannelNumber, e_OnOff.On);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER ON");
                            //string AVGstatus = ENA.Sense.Average.State(Cal_Prod[iCal].ChannelNumber); //ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":AVER?");

                            // Setup for Guilded Calibration
                            //ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            //ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Type);
                            //ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                            //ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit_Name);

                            //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);

                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConn + "'"); // Define DUT input port connector type
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFrcvPortNum + " '" + DUToutPortConn + "'"); // Define DUT output port connector type
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT input port
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT output port

                            //ENA.BasicCommand.System.SendCommand(":SENS" + NFchNum + ":CORR:COLL:GUID:PATH:TMET "+ NFsrcPortNum+","+NFrcvPortNum+","+" \"Undefined Thru\""); // Define Cal kit for DUT output port

                            ENA.Sense.Noise.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Type);
                            ENA.Sense.Noise.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit_Name);
                            ENA.Sense.Noise.LoadENRfile(Cal_Prod[iCal].ChannelNumber, ENRfile);
                            ENA.Sense.Noise.SelectCalMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.Scalar);
                            ENA.Sense.Noise.SelectRecMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.NoiseSource);
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CONN '" + DUToutPortConn + "'"); // Define Noise Source connector type
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CKIT '" + CalKitName + "'"); // Define Cal kit that will be used for the Noise Source adapter
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:ENR:FILENAME '" + ENRfile + "'"); // Load Noise Source ENR file Create Noise Figure class
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:METHOD 'Scalar'"); // Define the method for performing a calibration on a noise channel
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:RMEThod 'NoiseSource'"); // Define  the method used to characterize the noise receivers


                            // Start Guilde Calibration
                            //ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(Cal_Prod[iCal].ChannelNumber);
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:INIT"); // Initiates a guided calibration
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?");  // Ask the Number of Calibration setps
                            //int CalStep = Convert.ToInt16(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(Cal_Prod[iCal].ChannelNumber)); //Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());
                            
                            //string aa1 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 1);
                            //string aa2 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 2);
                            //string aa3 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 3);
                            //string aa4 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 4);
                            //string aa5 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 5);
                            //string aa6 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 6);
                            //string aa7 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 7);
                            //string aa8 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 8);

                            //if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " setting fail");
                        }


                        // New for Setup full 1-port cal for noise source ENR characterization
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                            ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);                            
                        }
                        // End New for Setup full 1-port cal for noise source ENR characterization
#endif
                }

                ////// Create int array Cal_Check[] for guided calibration channel check (20161206)

                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN))
                    {
                        Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    Cal_Check[ch - 1] = 1;
                }
                ///////////////////////////////////////////////////////////////////////////////////////

                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);

                    ////// Send InitGuidedCal() only if Cal_Check[iChn] == 1 (20161206)
                    if (Cal_Check[iChn] == 1)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = Int32.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    else
                    {
                        step[iChn] = 0;
                    }
                    //
                }
                Flag = false;
            } //if (Flag)
            #endregion Topaz_Init


            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {
                try
                {
                    #region "Switch" //SEOUL                       
                        
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), ProjectSpecificSwitching.dicSwitchConfig[Cal_Prod[iCal].Switch_Input.ToUpper()]);
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Ant, ProjectSpecificSwitching.dicSwitchConfig[Cal_Prod[iCal].Switch_Ant]); 
                                               
                    if ((Cal_Prod[iCal].Switch_Rx.Contains("OUT")) && !(Cal_Prod[iCal].Switch_Rx.Contains("DRX")) && !(Cal_Prod[iCal].Switch_Rx.Contains("MIMO")))
                    {
                        Eq.Site[0].SwMatrix.ActivatePath("OUT1", ProjectSpecificSwitching.dicSwitchConfig["OUT1"]);
                        Eq.Site[0].SwMatrix.ActivatePath("OUT2", ProjectSpecificSwitching.dicSwitchConfig["OUT2"]);
                        Eq.Site[0].SwMatrix.ActivatePath("OUT3", ProjectSpecificSwitching.dicSwitchConfig["OUT3"]);
                        Eq.Site[0].SwMatrix.ActivatePath("OUT4", ProjectSpecificSwitching.dicSwitchConfig["OUT4"]);
                    }
                    else
                    {
                        Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", ProjectSpecificSwitching.dicSwitchConfig["OUT1"]);
                        Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", ProjectSpecificSwitching.dicSwitchConfig["OUT2"]);
                        Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", ProjectSpecificSwitching.dicSwitchConfig["OUT3"]);
                        Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", ProjectSpecificSwitching.dicSwitchConfig["OUT4"]);
                    }
                    Thread.Sleep(200);


                    #endregion "Switch"
                }
                catch (Exception e)
                {

                    MessageBox.Show("The Parameter Type is" + Cal_Prod[iCal].ParameterType + "and the Band is " + Cal_Prod[iCal].Switch_Input.ToUpper()
                                    + "\r\n");
                }

                //For switch

                //string Isolation_trace1 = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber); //seoul
                //Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                //string[] tr = Isolation_trace1.Split(',');
                //int Isolation_trace = Convert.ToInt16(tr[0]);

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
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.ECAL_SHORT:
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.ECAL_LOAD:
                        case e_CalibrationType.ECAL_SAVE_A:
                        case e_CalibrationType.ECAL_SAVE_B:
                        case e_CalibrationType.SUBCLASS:
                        case e_CalibrationType.TRLREFLECT:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                        case e_CalibrationType.TRLLINE:
                        case e_CalibrationType.TRLTHRU:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                            break;
                        case e_CalibrationType.NS_POWER:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            b_Mode = false;
                            break;
                    }
                    //  b_Mode = true;
                }

                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                //DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                //Commented all this out to remove not get the prompts between loads - MM - 08262017
                                //if (Cal_Prod[iCal].CalType.ToString() == "OPEN" || Cal_Prod[iCal].CalType.ToString() == "ECAL_OPEN")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "SHORT" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SHORT")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SAVE")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //Thread.Sleep(3000);
                                calDo.CalContinue = false;
                            }



                            else
                            {
                                Thread.Sleep(100);
                            }
                        } //else

                    } // if (!b_Mode)

                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);

                    #region "Cal Kit Procedure"

                    switch (Cal_Prod[iCal].CalType)
                    {
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.ECAL_OPEN:
                        case e_CalibrationType.ECAL_SHORT:
                        case e_CalibrationType.ECAL_LOAD:
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(200);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;

                        case e_CalibrationType.ECAL_SAVE_A:
                            //////////// This is new /////////////// seoul
                            // Save full 1-port guided calibration for noise source ENR characterization, save “CalSet_A”           
                            string cSetA = "";
                            CalsetnameA = "";
                            CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type;
                            //CalsetnameA = Cal_Prod[iCal].Switch_Ant +"_" + Cal_Prod[iCal].Type + "_CalSet_A";// + Cal_Prod[iCal].Type;
                            //CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type; //Original
                            cSetA = ENA.Sense.Correction.Collect.Cset.Exist(CalsetnameA);
                            //cSetA = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");
                            if (cSetA.TrimEnd('\n') == "1")
                            {
                                ENA.Sense.Correction.Collect.Cset.Delete(CalsetnameA);
                                ENA.BasicCommand.System.Operation_Complete();
                                //ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                            }
                            ENA.Sense.Correction.Collect.Cset.Save(Cal_Prod[iCal].ChannelNumber, CalsetnameA);
                            //ENA.BasicCommand.SendCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                            ENA.BasicCommand.System.Operation_Complete();
                            break;

                        case e_CalibrationType.ECAL_SAVE_B:
                            //seoul
                            //string eCalCH = "";
                            CalsetnameB = "";
                            //string NFrcvPortNum = Cal_Prod[iCal].Port_1.ToString(); // VNA Receiver port (DUT output port) for NF

                            // Perform full 1-port calibration at Ecal side using the same frequency points, save “CalSet_B”
                            //switch (NFrcvPortNum)
                            //{
                            //    case "3":
                            //        eCalCH = "71";
                            //        break;
                            //    case "4":
                            //        eCalCH = "72";
                            //        break;
                            //    case "5":
                            //        eCalCH = "73";
                            //        break;
                            //    case "6":
                            //        eCalCH = "74";
                            //        break;
                            //}

                            // Specifies the Ecal Kit for Ecal Calibration
                            //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                            ENA.Sense.Correction.Collect.ECAL.SelectEcal(Cal_Prod[iCal].ChannelNumber, eCalKit);
                            // Acquire Ecal Oopn/Short/Load standards
                            ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                            //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                            ENA.BasicCommand.System.Operation_Complete();

                            // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                            CalsetnameB = Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + "_CalSet_B";
                            CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type;

                            string cSetB = ENA.Sense.Correction.Collect.Cset.Exist(CalsetnameB); //ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                            if (cSetB.TrimEnd('\n') == "1")
                            {
                                ENA.Sense.Correction.Collect.Cset.Delete(CalsetnameB);
                                ENA.BasicCommand.System.Operation_Complete();
                                //ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                            }
                            //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");
                            ENA.Sense.Correction.Collect.Cset.Copy(Cal_Prod[iCal].ChannelNumber, CalsetnameB);
                            //ENA.Sense.Correction.Collect.Cset.Save(Cal_Prod[iCal].ChannelNumber, CalsetnameB);
                            ENA.BasicCommand.System.Operation_Complete();


                            AdapterSnP = "Adapter_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".s2p";
                            FixtureSnP = "Fixture_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".s2p";
                            // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                            // portpair: Ecal thru port pair
                            // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                            // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                            //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original
                            ENA.Sense.Correction.Collect.ECAL.saveEcalSnp(eCalKit, "AB", "Ecal_thru.s2p");
                            //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");
                            ENA.BasicCommand.System.Operation_Complete();

                            // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                            ENA.Sense.Correction.Collect.Cset.CharacterizedFixture(CalsetnameA, CalsetnameB, Cal_Prod[iCal].Port_1, AdapterSnP, e_SweepType.LOG);
                            ENA.BasicCommand.System.Operation_Complete();
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + eCalCH + "\" , \"NS_CalSet_B_CH" + eCalCH + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + eCalCH + ".s2p\" , LOG");
                            // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + eCalCH + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + eCalCH + ".s2p\" , LOG");
                            ENA.Sense.Correction.Collect.Cset.Combine2Snp(AdapterSnP, "Ecal_thru.s2p", FixtureSnP, e_SweepType.LOG);
                            ENA.BasicCommand.System.Operation_Complete();

                            // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                            //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + eCalCH + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr\"");
                            //ENA.Sense.Correction.Collect.Cset.newENR(ENRfile, FixtureSnP, Cal_Prod[iCal].Switch_Ant + "_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr"); //Original
                            ENA.Sense.Correction.Collect.Cset.newENR(ENRfile, FixtureSnP, "New_CharacterizedENR_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".enr");
                            ENA.BasicCommand.System.Operation_Complete();
                            break;



                        case e_CalibrationType.NS_POWER:
                            // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                            int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                            //ENA.BasicCommand.SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");
                            ENA.Sense.Correction.Collect.ECAL.PathState_2Port(EcalModuleNum);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            Thread.Sleep(200);
                            ENA.BasicCommand.System.Operation_Complete();

                            break;


                        case e_CalibrationType.UnknownTHRU:
                            //Thread.Sleep(100);
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            //Thread.Sleep(200);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(100);
                            break;

                        case e_CalibrationType.TRLLINE:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.TRLREFLECT:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.TRLTHRU:
                            ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.ISOLATION:
                            bool verifyPass = true;
                            int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                            double[] compData = new double[NOP];
                            int loopCount = 0;
                            string TrFormat = "";
                            double VerLimitLow = 0,
                                VerLimitHigh = 0,
                                maxVal = 0,
                                minVal = 0;

                            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                            {
                                verifyPass = false;
                                #region Math->Normalize
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.BasicCommand.System.Operation_Complete();
                                #endregion

                                #region resopnse cal using normalize
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber); //seoul
                                ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(100);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region response cal using average value of cal kit
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region varification response cal
                                Thread.Sleep(100);
                                ENA.Format.DATA(e_FormatData.REAL);
                                //seoul
                                //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                Thread.Sleep(100);
                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                VerLimitLow = -0.1;
                                VerLimitHigh = 0.1;
                                // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                // Array.Resize(ref FData, NOP);

                                //seoul
                                //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                maxVal = compData.Max();
                                minVal = compData.Min();

                                //for (int j = 0; j < FData.Length; ++j)
                                //{
                                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                {
                                    verifyPass = true;
                                }
                                // }                                    
                                loopCount++;
                                //seoul
                                //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                #endregion
                            }

                            if (loopCount == 3)
                            {
                                string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                    Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                DisplayCalProcedureError(errDesc);
                            }

                            break;
                        default:
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    //Thread.Sleep(100);
                    ENA.BasicCommand.System.Operation_Complete();
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        //Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            Thread.Sleep(300);
                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.THRU:
                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(300);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {

                    ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                    int[] Cal_Check = new int[TotalChannel];
                    List<int> Cal_Check_list = new List<int>();
                    Cal_Check_list.Clear();
                    for (int i = 0; i < Cal_Prod.Length; i++)
                    {
                        if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                        {
                            Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                        }
                    }
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        Cal_Check[iChn] = 0;
                    }

                    foreach (int ch in Cal_Check_list)
                    {
                        Cal_Check[ch - 1] = 1;
                    }
                    //////////////////////////////


                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                        if (Cal_Check[iChn] == 1)
                        {
                            string newENRfileName = "";
                            string chName = ENA.Sense.Class((iChn + 1));// ENA.BasicCommand.ReadCommand("SENS" + (iChn + 1) + ":CLAS:NAME?");

                            //chName = Cal_Prod[iChn].ParameterType;


                            //The disconnect here is that the determination if a given channel is a Noise figure channel and the indexing for the NF input and output port do not come from the same place.  Have to find the index of the channel to get the appropriate INput and Output port designation.
                            //The question is, where do we get this information
                            //if (chName.Contains("Noise"))
                            if (chName.Contains("Noise"))
                            {
                                string NFrcvPortNum = ENA.Sense.Noise.OutputPort((iChn + 1)).TrimEnd('\n').Replace("+", ""); //ENA.BasicCommand.ReadCommand(":SENS" + (iChn + 1) + ":NOIS:PMAP:OUTP?").TrimEnd('\n').Replace("+", ""); // NF Receiver port (DUT output port)
                                string RxPath = "";
                                string zBandInfo = Cal_Prod[iCal].Switch_Input;

                                int zindex = 0;
                                for (int i = 0; i < Cal_Prod.Length; i++)
                                {
                                    //Get the Channel Number, Port_1 value, DUT NF Input Port Conn and DUT NF Output Port Conn
                                    if (Cal_Prod[i].Port_1.ToString() == NFrcvPortNum 
                                        && Cal_Prod[i].ChannelNumber == iChn + 1
                                        && Cal_Prod[i].CalKit == 1 )
                                    {
                                        zindex = i;
                                        break;

                                    }
                                }
                                    
                                //switch (NFrcvPortNum) //seoul
                                //{
                                //    case "6"://@10ports  6  @6ports  3
                                //        RxPath = "OUT1";
                                //        break;

                                //    case "7"://@10ports  7  @6ports  4
                                //        RxPath = "OUT2";
                                //        break;

                                //    case "8"://@10ports  8  @6ports  5
                                //        RxPath = "OUT3";
                                //        break;

                                //    case "9"://@10ports  9  @6ports  6
                                //        RxPath = "OUT4";
                                //        break;
                                //}

                                RxPath = ProjectSpecificSwitching.dicRxOutNamemapping[Convert.ToInt16(NFrcvPortNum)];

                                ENA.Sense.Average.State((iChn + 1), e_OnOff.Off);
                                //ENA.BasicCommand.System.SendCommand("SENS" + (iChn + 1) + ":AVER OFF");
                                string AVGstatus = ENA.Sense.Average.State((iChn + 1)); //ENA.BasicCommand.System.ReadCommand("SENS" + (iChn + 1) + ":AVER?");

                                //string nS = "";
                                string antPort = ENA.Sense.Noise.InputPort(iChn + 1).Trim().Remove(0, 1);

                                //switch (antPort) //seoul
                                //{
                                //    case "3": nS = "NS1"; break;
                                //    case "4": nS = "NS2"; break;
                                //    case "5": nS = "NS3"; break;

                                //}
                                newENRfileName = "C:/Users/Public/Documents/Network Analyzer/New_CharacterizedENR_" + Cal_Prod[zindex].Switch_Ant + "_" + RxPath + ".enr";


                                //if ((iChn + 1) == 62 | (iChn + 1) == 66 | (iChn + 1) == 75 | (iChn + 1) == 79) //if ((iChn + 1) == 38 | (iChn + 1) == 42)
                                //    newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS2_CharacterizedENR_" + RxPath + ".enr";

                                //else newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + ".enr";


                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + ".enr";
                                ENA.Sense.Noise.LoadENRfile((iChn + 1), newENRfileName);
                                ENA.BasicCommand.System.Operation_Complete();
                                //ENA.BasicCommand.SendCommand(":SENS" + (iChn + 1) + ":NOIS:ENR:FILENAME '" + newENRfileName + "'"); // Load Characterized Noise Source ENR file
                                ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                                ENA.BasicCommand.System.Operation_Complete();
                            }

                            else
                            {
                                ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                                ENA.BasicCommand.System.Operation_Complete();
                                //Set segment power back to -20 dBm - modified by CheeOn 17-July-2017
                                //if (Cal_Prod[iCal].ParameterType != "NF")
                                ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], ProjectSpecificSwitching.PortEnable);

                            }
                        }
                        ENA.Display.Update(false);
                        //
                    }

                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;

                    //       DisplayMessageCalComplete();
                } //if (iCal == Cal_Prod.Length - 1 && !CalContinue)

            } //for (int iCal = iCalCount; iCal < Cal_Prod.Length; iCal++)

            #region Commented Out
            //for (int iChn = 0; iChn < TotalChannel; iChn++)
            //{
            //    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
            //        ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
            //} 
            #endregion Commented Out

        } //Calibrate_Topaz

        public void Calibrate_SPARA_PARTIAL(CalibrationInputDataObject calDo, int topazCalPower)
        {
            string tmpStr;
            int[] step = new int[TotalChannel];
            bool TempCalSelFlag;

            List<bool> CalSelection = new List<bool>(); //To store cal GUI user selection

            #region Topaz_Init

            if (Flag)
            {
                calDo.cCalSelection = new Dictionary<string, bool>();

                ////List<string> CalSegment_sorted = new List<string>() ;
                ////CalSegment.Sort();
                ////foreach (var name in CalSegment)
                ////    CalSegment_sorted.Add(name);
                // Get partial calibration from Calibration MENU GUI  Modified cheeon 29-July-2017
                CalibrationGUI CalGui = new CalibrationGUI(calDo.CalSegment);

                DialogResult NewForm = CalGui.ShowDialog();
                CalSelection = CalGui.UserSelection;
                int countSelection = 0;
                foreach (var w in calDo.CalSegment)
                {
                    calDo.cCalSelection.Add(w, CalSelection.ElementAt(countSelection));
                    countSelection++;
                }
                ///////////////////////////////

                //Delete CAL SET of Sparameter Channel
                int[] ch_check = new int[TotalChannel];
                List<int> ch_check_list = new List<int>();
                ch_check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ISOLATION))
                        {                               
                            ch_check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    ch_check[iChn] = 0;
                }

                foreach (int ch in ch_check_list)
                {
                    calDo.cCalSelection.TryGetValue("Channel#" + Convert.ToString(ch), out TempCalSelFlag);
                    if (TempCalSelFlag)
                    {
                        ch_check[ch - 1] = 1;
                    }
                }
                for (int i = 0; i < ch_check.Length; i++)
                {
                    if (ch_check[i] == 1)
                    {
                        ENA.BasicCommand.SendCommand(":CSET:DEL \"CH" + (i + 1) + "_CALREG" + "\"");
                    }
                }


                //Initialize for S-parameter
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                        {
                            if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5))) //This is asking if the calkit number is NOT that of a SHORT
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12)) //This is asking if the calkit is that of an UNKNOWN THRU for a 3 port (Thru for 2 -3)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                            ENA.BasicCommand.System.Operation_Complete();
                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 4) && (Cal_Prod[iCal].CalKit == 18)) //This is asking if the calkit is that of an UNKNOWN THRU for a 4 port 
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);

                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                //ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7)) //Is the calkit that of an UNKNOWN THRU for a 2 port
                        {
                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }//Newrly added
                    }
                }


                //Check Channel of Sparameter
                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN))
                        {
                            Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    calDo.cCalSelection.TryGetValue("Channel#" + Convert.ToString(ch), out TempCalSelFlag);
                    if (TempCalSelFlag)
                    {
                        Cal_Check[ch - 1] = 1;
                    }

                }


                // Init GuidedCalibration only for 2/3port Full port calibration
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    if (Cal_Check[iChn] == 1)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = Int32.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    else
                    {
                        step[iChn] = 0;
                    }
                }
                Flag = false;
            }
            #endregion


            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {
                if (Cal_Prod[iCal].ParameterType == "SPARA")
                {
                    #region "Switch"
                    Thread.Sleep(200);

                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), Operation.N1toTx);
                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Ant, Operation.N2toAnt);
                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N3toRx);
                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N4toRx);
                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N5toRx);
                    m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N6toRx);
                    #endregion
                }
                string Isolation_trace1 = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber);  //ENA.Sense.Correction.Collect.GuidedCal.Select_Trace(Cal_Prod[iCal].ChannelNumber); //seoul
                Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                string[] tr = Isolation_trace1.Split(',');
                int Isolation_trace = Convert.ToInt16(tr[0]);

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
                        case e_CalibrationType.ECAL_LOAD:
                        case e_CalibrationType.ECAL_SAVE_A:
                        case e_CalibrationType.ECAL_SAVE_B:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();

                            break;
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.ECAL_SHORT:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.SUBCLASS:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                            if (Cal_Prod[iCal].ChannelNumber > 85)
                            {
                                Thread.Sleep(200);
                            }
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
                        case e_CalibrationType.NS_POWER:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            b_Mode = false;
                            break;
                    }
                    //  b_Mode = true;
                }
                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if ((Cal_Prod[iCal].Message.Trim() != "" ) && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }

                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                //Commented all this out to remove not get the prompts between loads - MM - 08262017
                                //if (Cal_Prod[iCal].CalType.ToString() == "OPEN" || Cal_Prod[iCal].CalType.ToString() == "ECAL_OPEN")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "SHORT" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SHORT")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SAVE")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                Thread.Sleep(3000);
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                                
                        }
                    }

                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);
                    #region "Cal Kit Procedure"

                    calDo.cCalSelection.TryGetValue("Channel#" + Convert.ToString(Cal_Prod[iCal].ChannelNumber), out TempCalSelFlag);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel#" + Convert.ToString(Cal_Prod[iCal].ChannelNumber) + ":" + TempCalSelFlag );
                    if (TempCalSelFlag)
                    {
                        if (Cal_Prod[iCal].ParameterType == "SPARA")
                        {
                            switch (Cal_Prod[iCal].CalType)
                            {
                                case e_CalibrationType.OPEN:
                                case e_CalibrationType.SHORT:
                                case e_CalibrationType.LOAD:
                                case e_CalibrationType.THRU:
                                    if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    Thread.Sleep(200);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    break;

                                case e_CalibrationType.UnknownTHRU:
                                    if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    Thread.Sleep(200);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    break;

                                case e_CalibrationType.TRLLINE:
                                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    break;
                                case e_CalibrationType.TRLREFLECT:
                                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    break;
                                case e_CalibrationType.TRLTHRU:
                                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    break;
                                case e_CalibrationType.ISOLATION:
                                    bool verifyPass = true;
                                    int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                                    double[] compData = new double[NOP];
                                    int loopCount = 0;
                                    string TrFormat = "";
                                    double VerLimitLow = 0,
                                        VerLimitHigh = 0,
                                        maxVal = 0,
                                        minVal = 0;

                                    while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                                    {
                                        verifyPass = false;
                                        #region Math->Normalize
                                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        //ENA.BasicCommand.System.Operation_Complete();
                                        #endregion

                                        #region resopnse cal using normalize
                                        ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                        ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                        Thread.Sleep(100);
                                        ENA.BasicCommand.System.Operation_Complete();
                                        ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                        #endregion

                                        #region response cal using average value of cal kit
                                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                        //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                        //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                        //ENA.BasicCommand.System.Operation_Complete();
                                        //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                        #endregion

                                        #region varification response cal
                                        Thread.Sleep(100);
                                        ENA.Format.DATA(e_FormatData.REAL);
                                        TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                        ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                        //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                        //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                        //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                        //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                        Thread.Sleep(100);
                                        ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                        ENA.BasicCommand.System.Operation_Complete();

                                        ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                        ENA.BasicCommand.System.Operation_Complete();

                                        VerLimitLow = -0.1;
                                        VerLimitHigh = 0.1;
                                        // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                        // Array.Resize(ref FData, NOP);

                                        compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                        //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                        maxVal = compData.Max();
                                        minVal = compData.Min();

                                        //for (int j = 0; j < FData.Length; ++j)
                                        //{
                                        if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                        {
                                            verifyPass = true;
                                        }
                                        // }                                    
                                        loopCount++;
                                        ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                        //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                        #endregion
                                    }
                                    if (loopCount == 3)
                                    {
                                        string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                            Cal_Prod[iCal].ChannelNumber, Isolation_trace, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                        DisplayCalProcedureError(errDesc);
                                    }
                                    break;
                                default:
                                    DisplayError(Cal_Prod[iCal]);
                                    break;
                            }
                            //Thread.Sleep(100);
                            ENA.BasicCommand.System.Operation_Complete();
                            #endregion
                        }
                    }
                }
                else
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        //Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            Thread.Sleep(300);
                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.THRU:
                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(300);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }
                    

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {

                    ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                    int[] Cal_Check = new int[TotalChannel];
                    List<int> Cal_Check_list = new List<int>();
                    Cal_Check_list.Clear();

                    for (int i = 0; i < Cal_Prod.Length; i++)
                    {
                        if (Cal_Prod[i].ParameterType == "SPARA")
                        {
                            if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                            {
                                Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                            }
                        }
                    }
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        Cal_Check[iChn] = 0;
                    }

                    foreach (int ch in Cal_Check_list)
                    {
                        calDo.cCalSelection.TryGetValue("Channel#" + Convert.ToString(ch), out TempCalSelFlag);
                        if (TempCalSelFlag)
                        {
                            Cal_Check[ch - 1] = 1;
                        }
                        else
                        {
                            Cal_Check[ch - 1] = 0;
                        }
                    }


                    // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {

                        if (Cal_Check[iChn] == 1)
                        {
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            Thread.Sleep(500);
                        }

                        if (Cal_Prod[iCal].ParameterType != "NF")
                        {
                            ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], ProjectSpecificSwitching.PortEnable);
                        }

                    }

                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;
                }

            }

        }

        public void Calibrate_SPARA(CalibrationInputDataObject calDo, int topazCalPower, string ENRfile)
        {
            string tmpStr;
            int[] step = new int[TotalChannel];
            string eCalKit = "";
            #region Topaz_Init

            if (Flag)
            {

                //Delete CAL SET of Sparameter Channel
                int[] ch_check = new int[TotalChannel];
                List<int> ch_check_list = new List<int>();
                ch_check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ISOLATION))
                        {
                            if ((Cal_Prod[iCal].ChannelNumber == 52))
                            {
                                int a = 0;
                            }

                            ch_check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    ch_check[iChn] = 0;
                }

                foreach (int ch in ch_check_list)
                {
                    ch_check[ch - 1] = 1;
                }
                for (int i = 0; i < ch_check.Length; i++)
                {
                    if (ch_check[i] == 1)
                    {
                        ENA.BasicCommand.SendCommand(":CSET:DEL \"CH" + (i + 1) + "_CALREG" + "\"");
                    }
                }


                //Initialize for S-parameter
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                        {
                            if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5))) //This is asking if the calkit number is NOT that of a SHORT
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12)) //This is asking if the calkit is that of an UNKNOWN THRU for a 3 port (Thru for 2 -3)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 4) && (Cal_Prod[iCal].CalKit == 18)) //This is asking if the calkit is that of an UNKNOWN THRU for a 4 port 
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);

                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                //ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7)) //Is the calkit that of an UNKNOWN THRU for a 2 port
                        {
                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }//Newrly added
                    }
                }


                //Check Channel of Sparameter
                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN))
                        {
                            Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    Cal_Check[ch - 1] = 1;
                }


                // Init GuidedCalibration only for 2/3port Full port calibration
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    ////// Send InitGuidedCal() only if Cal_Check[iChn] == 1 (20161206)
                    if (Cal_Check[iChn] == 1)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = Int32.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    else
                    {
                        step[iChn] = 0;
                    }
                    //
                }
                Flag = false;
            }
            #endregion


            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {

                if (Cal_Prod[iCal].ParameterType == "SPARA")
                {
                    #region "Switch"
                    if (Cal_Prod[iCal].ParameterType != "NF")
                    {
                        Thread.Sleep(200);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), Operation.N1toTx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Ant, Operation.N2toAnt);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N3toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N4toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N5toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N6toRx);
                    }
                    else
                    {
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), Operation.ENAtoRFIN);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Ant, Operation.ENAtoRFOUT);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.ENAtoRX);
                    }
   
                    Thread.Sleep(200);
                    #endregion
                }
                string Isolation_trace1 = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber);  //ENA.Sense.Correction.Collect.GuidedCal.Select_Trace(Cal_Prod[iCal].ChannelNumber); //seoul
                Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                string[] tr = Isolation_trace1.Split(',');
                int Isolation_trace = Convert.ToInt16(tr[0]);

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
                        case e_CalibrationType.ECAL_LOAD:
                        case e_CalibrationType.ECAL_SAVE_A:
                        case e_CalibrationType.ECAL_SAVE_B:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();

                            break;
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.ECAL_SHORT:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.SUBCLASS:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                            if (Cal_Prod[iCal].ChannelNumber > 85)
                            {
                                Thread.Sleep(200);
                            }
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
                        case e_CalibrationType.NS_POWER:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            b_Mode = false;
                            break;
                    }
                    //  b_Mode = true;
                }
                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                //if (Cal_Prod[iCal].CalType.ToString() == "OPEN" || Cal_Prod[iCal].CalType.ToString() == "ECAL_OPEN")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                ////else if (Cal_Prod[iCal].CalType.ToString() == "SHORT" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SHORT")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SAVE")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                Thread.Sleep(3000);
                                calDo.CalContinue = false;
                            }

                            else
                            {
                                Thread.Sleep(100);
                            }
                        } //else

                    } // if (!b_Mode)
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);

                    #region "Cal Kit Procedure"

                    if (Cal_Prod[iCal].ParameterType == "SPARA")
                    {
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.OPEN:
                            case e_CalibrationType.SHORT:
                            case e_CalibrationType.LOAD:
                            case e_CalibrationType.THRU:
                            case e_CalibrationType.ECAL_OPEN:
                            case e_CalibrationType.ECAL_SHORT:
                            case e_CalibrationType.ECAL_LOAD:
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;

                            case e_CalibrationType.ECAL_SAVE_A:
                                //////////// This is new ///////////////
                                // Save full 1-port guided calibration for noise source ENR characterization, save “CalSet_A”                             
                                string CalsetnameA = "";
                                CalsetnameA = "NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber;
                                string cSetA = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");

                                if (cSetA.TrimEnd('\n') == "1")
                                {
                                    ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                                }
                                ENA.BasicCommand.SendCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                                ENA.BasicCommand.System.Operation_Complete();
                                break;

                            case e_CalibrationType.ECAL_SAVE_B:

                                string eCalCH = "";
                                string CalsetnameB = "";
                                string NFrcvPortNum = Cal_Prod[iCal].Port_1.ToString(); // VNA Receiver port (DUT output port) for NF

                                // Perform full 1-port calibration at Ecal side using the same frequency points, save “CalSet_B”
                                //switch (NFrcvPortNum)
                                //{
                                //    case "3":
                                //        eCalCH = "71";
                                //        break;
                                //    case "4":
                                //        eCalCH = "72";
                                //        break;
                                //    case "5":
                                //        eCalCH = "73";
                                //        break;
                                //    case "6":
                                //        eCalCH = "74";
                                //        break;
                                //}

                                // Specifies the Ecal Kit for Ecal Calibration
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                                ENA.BasicCommand.System.Operation_Complete();
                                // Acquire Ecal Oopn/Short/Load standards
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                                ENA.BasicCommand.System.Operation_Complete();
                                string dummy = ENA.BasicCommand.ReadCommand("*OPC?");

                                // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                                CalsetnameB = "NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber;
                                string cSetB = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                                if (cSetB.TrimEnd('\n') == "1")
                                {
                                    ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                                }
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");


                                // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                                // portpair: Ecal thru port pair
                                // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                                // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                                //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original

                                //This was the active line when I got it from Seoul - MM
                                //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");


                                ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\"");

                                // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber + "\" , \"NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");
                                // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");


                                // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr\"");
                                //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr\"");
                                //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + zz + "_" + NFrcvPortNum.Trim() + ".enr\""); // original
                                ENA.BasicCommand.System.Operation_Complete();


                                break;



                            case e_CalibrationType.NS_POWER:
                                // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                                int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                                ENA.BasicCommand.SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");


                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(2000);
                                ENA.BasicCommand.System.Operation_Complete();

                                break;


                            case e_CalibrationType.UnknownTHRU:
                                //if (Cal_Prod[iCal].ChannelNumber >= 85)
                                //{
                                //    int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());
                                //    if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " thru cal fail \n\n" + tmpStr);
                                //}
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();

                                break;

                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.ISOLATION:
                                bool verifyPass = true;
                                int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                                double[] compData = new double[NOP];
                                int loopCount = 0;
                                string TrFormat = "";
                                double VerLimitLow = 0,
                                    VerLimitHigh = 0,
                                    maxVal = 0,
                                    minVal = 0;

                                while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                                {
                                    verifyPass = false;
                                    #region Math->Normalize
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.BasicCommand.System.Operation_Complete();
                                    #endregion

                                    #region resopnse cal using normalize
                                    ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    Thread.Sleep(100);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                    #endregion

                                    #region response cal using average value of cal kit
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    //ENA.BasicCommand.System.Operation_Complete();
                                    //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                    #endregion

                                    #region varification response cal
                                    Thread.Sleep(100);
                                    ENA.Format.DATA(e_FormatData.REAL);
                                    TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                    //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                    Thread.Sleep(100);
                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    VerLimitLow = -0.1;
                                    VerLimitHigh = 0.1;
                                    // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                    // Array.Resize(ref FData, NOP);

                                    compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                    maxVal = compData.Max();
                                    minVal = compData.Min();

                                    //for (int j = 0; j < FData.Length; ++j)
                                    //{
                                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                    {
                                        verifyPass = true;
                                    }
                                    // }                                    
                                    loopCount++;
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                    #endregion
                                }
                                if (loopCount == 3)
                                {
                                    string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                        Cal_Prod[iCal].ChannelNumber, Isolation_trace, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                    DisplayCalProcedureError(errDesc);
                                }
                                break;
                            default:
                                DisplayError(Cal_Prod[iCal]);
                                break;
                        }
                        //Thread.Sleep(100);
                        ENA.BasicCommand.System.Operation_Complete();
                    }
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        //Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            Thread.Sleep(300);
                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.THRU:
                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(300);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {

                    ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                    int[] Cal_Check = new int[TotalChannel];
                    List<int> Cal_Check_list = new List<int>();
                    Cal_Check_list.Clear();

                    for (int i = 0; i < Cal_Prod.Length; i++)
                    {
                        if (Cal_Prod[i].ParameterType == "SPARA")
                        {
                            if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                            {
                                Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                            }
                        }
                    }
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        Cal_Check[iChn] = 0;
                    }

                    foreach (int ch in Cal_Check_list)
                    {
                        Cal_Check[ch - 1] = 1;
                    }


                    // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {

                        if (Cal_Check[iChn] == 1)
                        {
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            Thread.Sleep(500);
                            ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], ProjectSpecificSwitching.PortEnable);
                        }

                    }

                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;
                }

            }
        }

        public void Calibrate_NF(CalibrationInputDataObject calDo, int topazCalPower, string ENRfile)
        {
            string tmpStr;
            string handStr;
            bool bNext;
            int ChannelNo = 0;

            e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            bool[] AnalysisEnable = new bool[TotalChannel];

            /////////////
            string eCalKit = "";
            string[] listnum = ENA.BasicCommand.ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?").Split(',');
            if (Int32.Parse(listnum[0]) > 0)
            {
                string[] EcalList = new string[listnum.Length];

                for (int i = 0; i < listnum.Length; ++i)
                {
                    listnum[i] = Int32.Parse(listnum[i]).ToString();
                    string[] EcalInfo = ENA.BasicCommand.ReadCommand("SENS:CORR:CKIT:ECAL" + listnum[i] + ":INF?").Split(',', ':');
                    EcalList[i] = EcalInfo[1].Trim() + " ECal " + EcalInfo[3].Trim();
                    // Example of return value: 
                    // "ModelNumber: N4431B, SerialNumber: 03605, ConnectorType: 35F 35F 35F 35F, PortAConnector: APC 3.5 female, PortBConnector: APC 3.5 female, PortCConnector: APC 3.5 female, PortDConnector: APC 3.5 female, MinFreq: 9000, MaxFreq: 13510000000, NumberOfPoints: 336, Calibrated: 19/Dec/2007, CharacterizedBy: 0000099999, NetworkAnalyzer: US44240045"
                    eCalKit = EcalList[0];
                }
            }
            else
            {
                //If Ecalkit not existed, message will be shown   
            }
            /////////////


            #region Topaz_Init

            if (Flag)
            {
                //Delete CAL SET of NF Channel
                int[] ch_check = new int[TotalChannel];
                List<int> ch_check_list = new List<int>();
                ch_check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "NF")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN))
                        {
                            ch_check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    ch_check[iChn] = 0;
                }

                foreach (int ch in ch_check_list)
                {
                    ch_check[ch - 1] = 1;
                }
                for (int i = 0; i < ch_check.Length; i++)
                {
                    if (ch_check[i] == 1)
                    {
                        ENA.BasicCommand.SendCommand(":CSET:DEL \"CH" + (i + 1) + "_CALREG" + "\"");
                    }
                }

                int[] step = new int[TotalChannel];

                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "NF")
                    {

                        if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                        if (Cal_Prod[iCal].CalType == e_CalibrationType.OPEN)
                        {
                            if (!((Cal_Prod[iCal].CalKit == 2) || (Cal_Prod[iCal].CalKit == 5))) //This is asking if the calkit number is NOT that of a SHORT
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 3) && (Cal_Prod[iCal].CalKit == 12)) //This is asking if the calkit is that of an UNKNOWN THRU for a 3 port (Thru for 2 -3)
                        {
                            ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);

                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }
                        if ((Cal_Prod[iCal].No_Ports == 2) && (Cal_Prod[iCal].CalKit == 7)) //Is the calkit that of an UNKNOWN THRU for a 2 port
                        {
                            if (Cal_Prod[iCal].CalType == e_CalibrationType.UnknownTHRU)
                            {
                                ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                                ENA.BasicCommand.System.Operation_Complete();
                            }
                        }//Newrly added

                        #region If e_CalibrationType.NS_POWER
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                        {

                            string dummy;
                            int NFchNum = Cal_Prod[iCal].ChannelNumber; // Channel number for NF
                            int NFsrcPortNum = Cal_Prod[iCal].NF_SrcPortNum; ; // 2; // VNA Source port (DUT input port) for NF //Need to change next time
                            int NFrcvPortNum = Cal_Prod[iCal].Port_1; // VNA Receiver port (DUT output port) for NF
                            //string NS_CalKitName = "85033D/E"; // Cal kit name (e.g Mechanical Cal kit 85033D/E), check if it matches DUT port connector type
                            //string NS_DUTinPortConn = "APC 3.5 female"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            //string NS_DUToutPortConn = "APC 3.5 female"; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            string CalKitName = Cal_Prod[iCal].CalKit_Name; //"Anakin"; // Define Cal kit name (e.g Ecal N4431B S/N 03605), check if it matches DUT port connector type
                            string DUTinPortConn = Cal_Prod[iCal].DUT_NF_InPort_Conn; // "ANT1"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type //Need to Change
                            string DUToutPortConn = Cal_Prod[iCal].DUT_NF_OutPort_Conn; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type
                            //string DUTinPortConn = "ANT1"; // DUT input Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type //Need to Change
                            //string DUToutPortConn = Cal_Prod[iCal].Type; // DUT output Port connector type (e.g APC 3.5 female), check if it matches Cak kit connector type

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);
                            dummy = ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                            dummy = dummy.Trim('\"', '\n');
                            string[] tr = dummy.Split(',');
                            ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER:COUN 20");
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER ON");
                            string AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":AVER?");

                            // Setup for ENR load ---------------------------------
                            //switch (NFrcvPortNum)
                            //{
                            //    case 3:
                            //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_MB1.enr"; // ENR file location
                            //        break;
                            //    case 4:
                            //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_MB2.enr"; // ENR file location
                            //        break;
                            //    case 5:
                            //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_HB1.enr"; // ENR file location
                            //        break;
                            //    case 6:
                            //        ENRfile = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_PRX_HB2.enr"; // ENR file location
                            //        break;
                            //}


                            // Setup for Guilded Calibration ---------------------------------


                            // Setup for Guilded Calibration
                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConn + "'"); // Define DUT input port connector type
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CONN:PORT" + NFrcvPortNum + " '" + DUToutPortConn + "'"); // Define DUT output port connector type
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT input port
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT output port
                            ENA.BasicCommand.System.Operation_Complete();


                            ENA.BasicCommand.System.SendCommand(":SENS" + NFchNum + ":CORR:COLL:GUID:PATH:TMET " + NFsrcPortNum + "," + NFrcvPortNum + "," + " \"Undefined Thru\""); // Define Cal kit for DUT output port
                            ENA.BasicCommand.System.Operation_Complete();


                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CONN '" + DUToutPortConn + "'"); // Define Noise Source connector type
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:SOUR:CKIT '" + CalKitName + "'"); // Define Cal kit that will be used for the Noise Source adapter
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:ENR:FILENAME '" + ENRfile + "'"); // Load Noise Source ENR file Create Noise Figure class
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:METHOD 'Scalar'"); // Define the method for performing a calibration on a noise channel
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOISE:CAL:RMEThod 'NoiseSource'"); // Define  the method used to characterize the noise receivers
                            ENA.BasicCommand.System.Operation_Complete();

                            ENA.BasicCommand.System.SendCommand("SENS" + NFchNum.ToString() + ":CORR:COLL:GUID:INIT");
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum.ToString() + ":CORR:COLL:GUID:STEP?");

                            #region Commented Out
                            // Start Guilde Calibration
                            //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:INIT"); // Initiates a guided calibration
                            ////ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?");  // Ask the Number of Calibration setps
                            //int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + NFchNum + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());

                            //string aa1 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 1);
                            //string aa2 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 2);
                            //string aa3 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 3);
                            //string aa4 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 4);
                            //string aa5 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 5);
                            //string aa6 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 6);
                            //string aa7 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 7);
                            //string aa8 = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:DESC? " + 8);

                            //if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " setting fail"); 
                            #endregion Commented Out

                        }
                        #endregion If e_CalibrationType.NS_POWER


                        // New for Setup full 1-port cal for noise source ENR characterization
                        if (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN)
                        {
                            ENA.BasicCommand.System.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:CONN:PORT" + (Cal_Prod[iCal].Port_1) + ":SEL " + "\"" + Cal_Prod[iCal].Type + "\"");
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.BasicCommand.System.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:CKIT:PORT" + (Cal_Prod[iCal].Port_1) + ":SEL " + "\"" + Cal_Prod[iCal].CalKit_Name + "\""); // Use Cal Kit "Anakin"
                        }
                        // End New for Setup full 1-port cal for noise source ENR characterization
                    }
                } // for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)

                ////// Create int array Cal_Check[] for guided calibration channel check (20161206)

                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (Cal_Prod[iCal].ParameterType == "NF")
                    {
                        if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN))
                        {
                            Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                        }
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    Cal_Check[ch - 1] = 1;
                }
                ///////////////////////////////////////////////////////////////////////////////////////

                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);

                    ////// Send InitGuidedCal() only if Cal_Check[iChn] == 1 (20161206)
                    if (Cal_Check[iChn] == 1)
                    {
                        ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                        ENA.BasicCommand.System.Operation_Complete();
                        if (iChn == 0) Thread.Sleep(100);
                        step[iChn] = Int32.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));
                        Thread.Sleep(10);
                    }
                    else
                    {
                        step[iChn] = 0;
                    }
                    //
                }
                Flag = false;
            } //if (Flag)
            #endregion Topaz_Init


            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {

                try
                {
                    #region "Switch"
                    if (Cal_Prod[iCal].ParameterType != "NF")
                    {
                        Thread.Sleep(200);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), Operation.N1toTx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Ant, Operation.N2toAnt);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N3toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N4toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N5toRx);
                        m_eqsm.ActivatePath(Cal_Prod[iCal].Switch_Rx, Operation.N6toRx);
                    }
                    else
                    {
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Input.ToUpper(), EqLib.Operation.ENAtoRFIN);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Ant, EqLib.Operation.ENAtoRFOUT);
                        Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[iCal].Switch_Rx, EqLib.Operation.ENAtoRX);
                    }

                    Thread.Sleep(200);
                    #endregion "Switch"
                }
                catch (Exception e)
                {

                    MessageBox.Show("The Parameter Type is" + Cal_Prod[iCal].ParameterType + "and the Band is " + Cal_Prod[iCal].Switch_Input.ToUpper()
                                    + "\r\n");
                }

                //For switch

                string Isolation_trace1 = ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber);  //ENA.Sense.Correction.Collect.GuidedCal.Select_Trace(Cal_Prod[iCal].ChannelNumber); //seoul
                Isolation_trace1 = Isolation_trace1.Trim('\"', '\n');
                string[] tr = Isolation_trace1.Split(',');
                int Isolation_trace = Convert.ToInt16(tr[0]);

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
                        case e_CalibrationType.ECAL_LOAD:
                        case e_CalibrationType.ECAL_SAVE_A:
                        case e_CalibrationType.ECAL_SAVE_B:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.ECAL_OPEN:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();

                            break;
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.ECAL_SHORT:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.SUBCLASS:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            break;
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                            if (Cal_Prod[iCal].ChannelNumber > 85)
                            {
                                Thread.Sleep(200);
                            }
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
                        case e_CalibrationType.NS_POWER:
                            tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                            b_Mode = false;
                            break;
                    }
                    //  b_Mode = true;
                }

                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;
                                Thread.Sleep(100);
                                break;
                            }
                            if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                //Commented all this out to remove not get the prompts between loads - MM - 08262017
                                //if (Cal_Prod[iCal].CalType.ToString() == "OPEN" || Cal_Prod[iCal].CalType.ToString() == "ECAL_OPEN")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "SHORT" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SHORT")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_LOAD" || Cal_Prod[iCal].CalType.ToString() == "ECAL_SAVE")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "UnknownTHRU")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                //else if (Cal_Prod[iCal].CalType.ToString() == "ISOLATION")
                                //{
                                //    DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                                //}
                                Thread.Sleep(3000);
                                calDo.CalContinue = false;
                            }



                            else
                            {
                                Thread.Sleep(100);
                            }
                        } //else

                    } // if (!b_Mode)

                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                    ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);

                    #region "Cal Kit Procedure"
                    if (Cal_Prod[iCal].ParameterType == "NF")
                    {
                        switch (Cal_Prod[iCal].CalType)
                        {
                            case e_CalibrationType.OPEN:
                            case e_CalibrationType.SHORT:
                            case e_CalibrationType.LOAD:
                            case e_CalibrationType.THRU:
                            case e_CalibrationType.ECAL_OPEN:
                            case e_CalibrationType.ECAL_SHORT:
                            case e_CalibrationType.ECAL_LOAD:
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;

                            case e_CalibrationType.ECAL_SAVE_A:
                                //////////// This is new ///////////////
                                // Save full 1-port guided calibration for noise source ENR characterization, save “CalSet_A”                             
                                string CalsetnameA = "";
                                CalsetnameA = "NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber;
                                string cSetA = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");

                                if (cSetA.TrimEnd('\n') == "1")
                                {
                                    ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                                }
                                ENA.BasicCommand.SendCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                                ENA.BasicCommand.System.Operation_Complete();
                                break;

                            case e_CalibrationType.ECAL_SAVE_B:

                                string eCalCH = "";
                                string CalsetnameB = "";
                                string NFrcvPortNum = Cal_Prod[iCal].Port_1.ToString(); // VNA Receiver port (DUT output port) for NF

                                // Perform full 1-port calibration at Ecal side using the same frequency points, save “CalSet_B”
                                //switch (NFrcvPortNum)
                                //{
                                //    case "3":
                                //        eCalCH = "71";
                                //        break;
                                //    case "4":
                                //        eCalCH = "72";
                                //        break;
                                //    case "5":
                                //        eCalCH = "73";
                                //        break;
                                //    case "6":
                                //        eCalCH = "74";
                                //        break;
                                //}

                                // Specifies the Ecal Kit for Ecal Calibration
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                                ENA.BasicCommand.System.Operation_Complete();
                                // Acquire Ecal Oopn/Short/Load standards
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                                ENA.BasicCommand.System.Operation_Complete();
                                string dummy = ENA.BasicCommand.ReadCommand("*OPC?");

                                // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                                CalsetnameB = "NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber;
                                string cSetB = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                                if (cSetB.TrimEnd('\n') == "1")
                                {
                                    ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                                }
                                ENA.BasicCommand.SendCommand(":SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");


                                // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                                // portpair: Ecal thru port pair
                                // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                                // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                                //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original

                                //This was the active line when I got it from Seoul - MM
                                //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");


                                ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\"");

                                // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + Cal_Prod[iCal].ChannelNumber + "\" , \"NS_CalSet_B_CH" + Cal_Prod[iCal].ChannelNumber + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");
                                // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , LOG");


                                // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                                ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr\"");
                                //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr\"");
                                //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + Cal_Prod[iCal].ChannelNumber + ".s2p\" , \"NS_CharacterizedENR_" + zz + "_" + NFrcvPortNum.Trim() + ".enr\""); // original
                                ENA.BasicCommand.System.Operation_Complete();


                                break;



                            case e_CalibrationType.NS_POWER:
                                // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                                int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                                ENA.BasicCommand.SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");


                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(2000);
                                ENA.BasicCommand.System.Operation_Complete();

                                break;


                            case e_CalibrationType.UnknownTHRU:
                                //if (Cal_Prod[iCal].ChannelNumber >= 85)
                                //{
                                //    int CalStep = Convert.ToInt16(ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:STEP?"));// int.Parse(instrument.ReadString());
                                //    if (CalStep != 8) MessageBox.Show("NF channel " + Cal_Prod[iCal].ChannelNumber + " thru cal fail \n\n" + tmpStr);
                                //}
                                if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(200);
                                ENA.BasicCommand.System.Operation_Complete();

                                break;

                            case e_CalibrationType.TRLLINE:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLREFLECT:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.TRLTHRU:
                                ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                                ENA.BasicCommand.System.Operation_Complete();
                                break;
                            case e_CalibrationType.ISOLATION:
                                bool verifyPass = true;
                                int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                                double[] compData = new double[NOP];
                                int loopCount = 0;
                                string TrFormat = "";
                                double VerLimitLow = 0,
                                    VerLimitHigh = 0,
                                    maxVal = 0,
                                    minVal = 0;

                                while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                                {
                                    verifyPass = false;
                                    #region Math->Normalize
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.BasicCommand.System.Operation_Complete();
                                    #endregion

                                    #region resopnse cal using normalize
                                    ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                    ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    Thread.Sleep(100);
                                    ENA.BasicCommand.System.Operation_Complete();
                                    ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                    #endregion

                                    #region response cal using average value of cal kit
                                    //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                    //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                    //ENA.BasicCommand.System.Operation_Complete();
                                    //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                    #endregion

                                    #region varification response cal
                                    Thread.Sleep(100);
                                    ENA.Format.DATA(e_FormatData.REAL);
                                    TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                                    //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                                    //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                                    Thread.Sleep(100);
                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                    ENA.BasicCommand.System.Operation_Complete();

                                    VerLimitLow = -0.1;
                                    VerLimitHigh = 0.1;
                                    // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                    // Array.Resize(ref FData, NOP);

                                    compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                                    //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                    maxVal = compData.Max();
                                    minVal = compData.Min();

                                    //for (int j = 0; j < FData.Length; ++j)
                                    //{
                                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                    {
                                        verifyPass = true;
                                    }
                                    // }                                    
                                    loopCount++;
                                    ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                                    //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                    #endregion
                                }
                                if (loopCount == 3)
                                {
                                    string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                        Cal_Prod[iCal].ChannelNumber, Isolation_trace, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                    DisplayCalProcedureError(errDesc);
                                }
                                break;
                            default:
                                DisplayError(Cal_Prod[iCal]);
                                break;

                        }
                    }
                    //Thread.Sleep(100);
                    ENA.BasicCommand.System.Operation_Complete();
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        if (Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && Cal_Prod[iCal].CalType.ToString() == "NS_POWER")
                            {
                                DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                            }
                        }
                        else
                        {
                            if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                            {
                                calDo.iCalCount = iCal;
                                calDo.CalContinue = true;
                                calDo.Fbar_cal = false;

                                break;
                            }
                            else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                            {
                                calDo.CalContinue = false;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        //Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            Thread.Sleep(300);
                            ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                            ENA.BasicCommand.System.Operation_Complete();
                            break;
                        case e_CalibrationType.THRU:
                            ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(300);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }

                if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
                {

                    ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                    int[] Cal_Check = new int[TotalChannel];
                    List<int> Cal_Check_list = new List<int>();
                    Cal_Check_list.Clear();
                    for (int i = 0; i < Cal_Prod.Length; i++)
                    {
                        if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                        {
                            if (Cal_Prod[i].ParameterType == "NF") Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                        }
                    }
                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        Cal_Check[iChn] = 0;
                    }

                    foreach (int ch in Cal_Check_list)
                    {
                        Cal_Check[ch - 1] = 1;
                    }
                    //////////////////////////////


                    for (int iChn = 0; iChn < TotalChannel; iChn++)
                    {
                        // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                        if (Cal_Check[iChn] == 1)
                        {
                            string newENRfileName = "";
                            string chName = ENA.BasicCommand.ReadCommand("SENS" + (iChn + 1) + ":CLAS:NAME?");

                            //chName = Cal_Prod[iChn].ParameterType;


                            //The disconnect here is that the determination if a given channel is a Noise figure channel and the indexing for the NF input and output port do not come from the same place.  Have to find the index of the channel to get the appropriate INput and Output port designation.
                            //The question is, where do we get this information
                            //if (chName.Contains("Noise"))
                            if (chName.Contains("Noise"))
                            {
                                string NFrcvPortNum = ENA.BasicCommand.ReadCommand(":SENS" + (iChn + 1) + ":NOIS:PMAP:OUTP?").TrimEnd('\n').Replace("+", ""); // NF Receiver port (DUT output port)
                                string RxPath = "";
                                string zBandInfo = Cal_Prod[iCal].Switch_Input;

                                int zindex = 0;
                                for (int i = 0; i < Cal_Prod.Length; i++)
                                {
                                    //Get the Channel Number, Port_1 value, DUT NF Input Port Conn and DUT NF Output Port Conn
                                    if (Cal_Prod[i].Port_1.ToString() == NFrcvPortNum && Cal_Prod[i].ChannelNumber == iChn + 1)
                                    {
                                        zindex = i;
                                        break;

                                    }
                                }




                                switch (NFrcvPortNum)
                                {
                                    case "3":
                                        RxPath = "MB1";
                                        break;

                                    case "4":
                                        RxPath = "HB2";
                                        break;

                                    case "5":
                                        RxPath = "HB1";
                                        break;

                                    case "6":
                                        RxPath = "HB2";
                                        break;
                                }

                                ENA.BasicCommand.System.SendCommand("SENS" + (iChn + 1) + ":AVER OFF");
                                string AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + (iChn + 1) + ":AVER?");

                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath +".enr";
                                newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[zindex].DUT_NF_InPort_Conn + "_" + Cal_Prod[zindex].DUT_NF_OutPort_Conn + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[iCal].DUT_NF_InPort_Conn + "_" + Cal_Prod[iCal].DUT_NF_OutPort_Conn + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + "_" + NFrcvPortNum.Trim() + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + zBandInfo + "_" + NFrcvPortNum.Trim() + ".enr";
                                //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/NS_CharacterizedENR_" + RxPath + ".enr";
                                ENA.BasicCommand.SendCommand(":SENS" + (iChn + 1) + ":NOIS:ENR:FILENAME '" + newENRfileName + "'"); // Load Characterized Noise Source ENR file
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.BasicCommand.SendCommand("SENS" + (iChn + 1) + ":CORR:COLL:GUID:SAVE");
                                ENA.BasicCommand.System.Operation_Complete();
                            }

                            else
                            {
                                ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                                Thread.Sleep(500);
                                //Set segment power back to -20 dBm - modified by CheeOn 17-July-2017
                                if (Cal_Prod[iCal].ParameterType != "NF")
                                {
                                    ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn],
                                        ProjectSpecificSwitching.PortEnable);
                                }

                            }
                        }

                        //
                    }

                    calDo.CalDone = true;
                    calDo.Fbar_cal = false;

                    //       DisplayMessageCalComplete();
                } //if (iCal == Cal_Prod.Length - 1 && !CalContinue)

            } //for (int iCal = iCalCount; iCal < Cal_Prod.Length; iCal++)

            #region Commented Out
            //for (int iChn = 0; iChn < TotalChannel; iChn++)
            //{
            //    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
            //        ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
            //} 
            #endregion Commented Out

        } //Calibrate_NF
            
        public void Calibrate_ResponseCal()
        {
            string tmpStr;
            string handStr;
            bool bNext;
            int ChannelNo = 0;
            e_SFormat[,] DisplayFormat = new e_SFormat[TotalChannel, 2];
            bool[] AnalysisEnable = new bool[TotalChannel];
            int[] step = new int[TotalChannel];

            for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
            {
                #region "Switch"

                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N1toTx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N2toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N3toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N4toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N5toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N6toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N7toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N8toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N9toRx);
                //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch.ToUpper(), InstrLib.Operation.N10toRx);

                #endregion

                //For switch
                Thread.Sleep(100);

                #region "Calibration Message"
                if (Cal_Prod[iCal].Message.Trim() != "")
                {
                    tmpStr = Cal_Prod[iCal].Message;
                    b_Mode = false;

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
                        case e_CalibrationType.UnknownTHRU:
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
                    b_Mode = true;
                }

                #endregion

                if (Cal_Prod[iCal].b_CalKit)
                {
                    if (!b_Mode)
                    {
                        DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                    }
                    #region "Cal Kit Procedure"
                    switch (Cal_Prod[iCal].CalType)
                    {
                        case e_CalibrationType.OPEN:
                        case e_CalibrationType.SHORT:
                        case e_CalibrationType.LOAD:
                        case e_CalibrationType.THRU:
                        case e_CalibrationType.UnknownTHRU:
                            if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                            ENA.BasicCommand.System.Operation_Complete();
                            Thread.Sleep(100);
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
                        case e_CalibrationType.ISOLATION:
                            bool verifyPass = true;
                            int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                            double[] compData = new double[NOP];
                            int loopCount = 0;
                            string TrFormat = "";
                            double VerLimitLow = 0,
                                VerLimitHigh = 0,
                                maxVal = 0,
                                minVal = 0;

                            while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                            {
                                verifyPass = false;
                                #region Math->Normalize
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.BasicCommand.System.Operation_Complete();
                                #endregion

                                #region resopnse cal using normalize
                                ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                                ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                Thread.Sleep(100);
                                ENA.BasicCommand.System.Operation_Complete();
                                ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region response cal using average value of cal kit
                                //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                                //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                                //ENA.BasicCommand.System.Operation_Complete();
                                //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                                #endregion

                                #region varification response cal
                                Thread.Sleep(100);
                                ENA.Format.DATA(e_FormatData.REAL);
                                TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);

                                ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                                ENA.BasicCommand.System.Operation_Complete();

                                VerLimitLow = -0.1;
                                VerLimitHigh = 0.1;
                                // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                                // Array.Resize(ref FData, NOP);

                                compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                                maxVal = compData.Max();
                                minVal = compData.Min();

                                //for (int j = 0; j < FData.Length; ++j)
                                //{
                                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                                {
                                    verifyPass = true;
                                }
                                // }                                    
                                loopCount++;
                                ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                                #endregion
                            }
                            if (loopCount == 3)
                            {
                                string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                                    Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, VerLimitLow, VerLimitHigh, maxVal, minVal);
                                DisplayCalProcedureError(errDesc);
                            }
                            break;
                        default:
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    Thread.Sleep(100);
                    #endregion
                }
                else
                {
                    if (!b_Mode)
                    {
                        //KCC - Autocal
                        if (Cal_Prod[iCal].Message.Trim() != "")
                        {
                            DisplayMessage2(Cal_Prod[iCal].CalType, tmpStr);
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
                                if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                                else
                                    ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 3);
                                break;
                            case 4:
                                ENA.Sense.Correction.Collect.Method.SOLT4(Cal_Prod[iCal].ChannelNumber, 1, 2, 3, 4);
                                break;

                        }
                        Thread.Sleep(500);
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
                                Thread.Sleep(4000);
                            }
                            Thread.Sleep(500);
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
                            DisplayError(Cal_Prod[iCal]);
                            break;
                    }
                    #endregion
                    Thread.Sleep(200);
                }
            }


            //for (int iChn = 0; iChn < TotalChannel; iChn++)
            //{
            //    if (Cal_Prod[iChn].CalType != e_CalibrationType.ECAL)
            //        ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
            //}

            DisplayMessageCalComplete();
        }

        public void PortExtension(string Channel_Num) //seoul
        {
            ENA.BasicCommand.System.SendCommand("DISP:WIND" + Channel_Num + ":SIZ MAX");
            ENA.BasicCommand.System.Operation_Complete();
            ENA.BasicCommand.System.SendCommand("SENS:CORR:COLL:GUID:CHAN:MODE 1");
            ENA.BasicCommand.System.SendCommand("SENS" + Channel_Num + ":SWE:MODE CONT");
            ENA.BasicCommand.System.Operation_Complete();
            ENA.BasicCommand.System.SendCommand("SENS" + Channel_Num + ":CORR:EXT:AUTO:RES");
            ENA.BasicCommand.System.Operation_Complete();
            ENA.BasicCommand.System.SendCommand("SENS" + Channel_Num + ":CORR:EXT:AUTO:LOSS 1");
            ENA.BasicCommand.System.SendCommand("SENS" + Channel_Num + ":CORR:EXT:AUTO:MEAS OPEN");
            ENA.BasicCommand.System.Operation_Complete();
        }

        public void VerificationAll()
        {
            bool SparaverifyPass = true;
            bool NFverifyPass = true;

            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            string[] S_paracheck = new string[Cal_Prod.Length];
            string SubCalResultPath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
            string SubCalResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_VerificationAll.txt";

            DirectoryInfo di = new DirectoryInfo(SubCalResultPath);
            if (di.Exists == false) di.Create();

            StreamWriter SubCalResult = new StreamWriter(SubCalResultName, true);
            verificationTxt.Clear();

            for (int i = 0; i < Cal_Prod.Length; i++)
            {
                if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                {
                    switch (Cal_Prod[i].CalKit.ToString())
                    {
                        case "1":
                            S_paracheck[i] = "SPARA";
                            break;

                        case "2":
                            S_paracheck[i] = "NF";
                            break;

                        default:
                            S_paracheck[i] = "NONE";
                            break;
                    }
                }
                else
                {
                    S_paracheck[i] = "NONE";
                }
            }
                
            for (int i = 0; i < S_paracheck.Length; i++)
            {
                Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Input.ToUpper(), EqLib.Operation.N1toTx);
                Dictionary<string, Operation> swDict = ProjectSpecificSwitching.dicSwitchConfig;
                if (Cal_Prod[i].Switch_Ant.ToUpper().Contains("NS-"))
                {
                    string temp = Cal_Prod[i].Switch_Ant.ToUpper().Split('-')[1];
                    Eq.Site[0].SwMatrix.ActivatePath(temp, swDict[temp]);                       
                }
                else
                {
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Ant.ToUpper(), swDict[Cal_Prod[i].Switch_Ant.ToUpper()]);                        
                }
                    
                Eq.Site[0].SwMatrix.ActivatePath("OUT1", swDict["OUT1"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT2", swDict["OUT2"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT3", swDict["OUT3"]);
                Eq.Site[0].SwMatrix.ActivatePath("OUT4", swDict["OUT4"]);

                if ((Cal_Prod[i].Switch_Rx.Contains("MLB")) || (Cal_Prod[i].Switch_Rx.Contains("GSM")) || (Cal_Prod[i].Switch_Rx.Contains("DRX")) || (Cal_Prod[i].Switch_Rx.Contains("MIMO")))
                {
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", swDict["OUT-DRX"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", swDict["IN-MLB"]);
                    Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", swDict["IN-GSM"]);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", swDict["OUT-MIMO"]);
                }

                if (S_paracheck[i].ToUpper() == "SPARA")
                {
                    //var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                    //               where
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S11") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S22") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S33") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S44") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S55") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S66") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S77") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S88") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S99")  //@10ports
                    //               select ((e_SParametersDef)trace).ToString()).Distinct();

                    var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                        where ProjectSpecificSwitching.dicTraceIndexTable.ContainsKey(trace)
                        select ((e_SParametersDef)trace).ToString()).Distinct();

                        
                    double d_start_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Start > 1E9 && segment.Start < 3E9 select segment.Start).Min();
                    double d_stop_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Stop > 1E9 && segment.Stop < 4E9 select segment.Stop).Max();

                    int start_freq = (int)(d_start_freq / (double)1e6);
                    int stop_freq = (int)(d_stop_freq / (double)1e6);

                    double channelStopFreq = SParamData[Cal_Prod[i].ChannelNumber - 1].Freq.Max() / (double)1e6;

                    foreach (var sparam in sparams)
                    {
                        if (channelStopFreq > 4000)
                        {
                            SparaverifyPass &= VerifyCalData_split_Freq(
                                Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                sparam, -120, -20, 4000 * 1e6, -18);
                        }
                        else
                        {
                            SparaverifyPass &= VerifyCalData_new(
                                Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                sparam, -120, -20);
                        }
                    }
                }
                else if (S_paracheck[i].ToUpper() == "NF")
                {

                    NFverifyPass &= VerifyCalData_NF(
                        Convert.ToInt32(Cal_Prod[i].ChannelNumber), -1, 1);

                }
            }

            ENA.Display.Update(false);

            MessageBox.Show("Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "Spara Cal Substrate Verification");
            MessageBox.Show("Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            SubCalResult.WriteLine("Spara Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "SPara Cal Substrate Verification");
            SubCalResult.WriteLine("NF Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            for (int i = 0; i < verificationTxt.Count; i++)
            {
                SubCalResult.WriteLine(verificationTxt[i]);
            }
            SubCalResult.Close();

        }

        public void SparaVerification(StreamWriter sparaResult, StreamWriter sparaResultAll)
        {
            bool verifyPass = true;
            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            string[] S_paracheck = new string[Cal_Prod.Length];
            verificationTxt.Clear();
            verificationTxtAll.Clear();

            for (int i = 0; i < Cal_Prod.Length; i++)
            {
                if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                {
                    switch (Cal_Prod[i].CalKit.ToString())
                    {
                        case "1":
                            S_paracheck[i] = "SPARA";
                            break;

                        case "2":
                            S_paracheck[i] = "NF";
                            break;

                        default:
                            S_paracheck[i] = "NONE";
                            break;
                    }
                }
                else
                {
                    S_paracheck[i] = "NONE";
                }
            }

            for (int i = 0; i < S_paracheck.Length; i++)
            {
                if (S_paracheck[i].ToUpper() == "SPARA")
                {                       
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Input.ToUpper(), EqLib.Operation.N1toTx);
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Ant.ToUpper(), EqLib.Operation.N2toAnt);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT1", EqLib.Operation.N3toRx);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT2", EqLib.Operation.N4toRx);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT3", EqLib.Operation.N5toRx);
                    Eq.Site[0].SwMatrix.ActivatePath("OUT4", EqLib.Operation.N6toRx);

                    if ((Cal_Prod[i].Switch_Rx.Contains("MLB")) || (Cal_Prod[i].Switch_Rx.Contains("GSM")) || (Cal_Prod[i].Switch_Rx.Contains("DRX")) || (Cal_Prod[i].Switch_Rx.Contains("MIMO")))
                    {
                        Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", EqLib.Operation.N3toRx);
                        Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", EqLib.Operation.N4toRx);
                        Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", EqLib.Operation.N5toRx);
                        Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", EqLib.Operation.N6toRx);
                    }



                    Thread.Sleep(100);

                    //var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                    //               where
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S11") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S22") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S33") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S44") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S55") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S66") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S77") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S88") ||
                    //               trace == (int)Enum.Parse(typeof(e_SParametersDef), "S99")  //@10ports
                    //               select ((e_SParametersDef)trace).ToString()).Distinct();


                    var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                        where ProjectSpecificSwitching.dicTraceIndexTable.ContainsKey(trace)
                        select   ((e_SParametersDef)trace).ToString()).Distinct();


                    double d_start_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Start >= 1E9 && segment.Start <= 7E9 select segment.Start).Min();
                    double d_stop_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Stop >= 1E9 && segment.Stop <= 7E9 select segment.Stop).Max();

                    int start_freq = (int)(d_start_freq / (double)1e6);
                    int stop_freq = (int)(d_stop_freq / (double)1e6);

                    foreach (var sparam in sparams)
                    {
                        verifyPass &= VerifyCalData_new(
                            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                            sparam, -100, -20); // LOAD Check
                    }
                }
            }
            MessageBox.Show("Cal Substrate Verification " + (verifyPass == true ? "PASSED" : "FAILED"), "Cal Substrate Verification");

            sparaResult.WriteLine("Cal Substrate Verification " + (verifyPass == true ? "PASSED" : "FAILED"), "Cal Substrate Verification");
            for (int i = 0; i < verificationTxt.Count; i++)
            {
                sparaResult.WriteLine(verificationTxt[i]);
            }
            sparaResult.Close();

            for (int i = 0; i < verificationTxtAll.Count; i++)
            {
                sparaResultAll.WriteLine(verificationTxtAll[i]);
            }
            sparaResultAll.Close();

        }

        public void NFVerification(StreamWriter NFResult)
        {
            string[] S_paracheck = new string[Cal_Prod.Length];
            double VerLimitLow = -1;
            double VerLimitHigh = 1;
            int nTry = 3;

            bool verifyPass = true;
            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            string[] NF_paracheck = new string[Cal_Prod.Length];
            verificationTxt.Clear();

            //Distinguish Channel of Sparameter and NF parameter 
            for (int i = 0; i < Cal_Prod.Length; i++)
            {
                if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                {
                    switch (Cal_Prod[i].CalKit.ToString())
                    {
                        case "1":
                            S_paracheck[i] = "SPARA";
                            break;

                        case "2":
                            S_paracheck[i] = "NF";
                            break;

                        default:
                            S_paracheck[i] = "NONE";
                            break;
                    }
                }
                else
                {
                    S_paracheck[i] = "NONE";
                }
            }

            //NF verification
            for (int i = 0; i < S_paracheck.Length; i++)
            {
                if (S_paracheck[i].ToUpper() == "NF")
                {

                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Input.ToUpper(), EqLib.Operation.N1toTx);
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Ant.ToUpper(), EqLib.Operation.N2toAnt);
                    // Case HLS2.
                    Eq.Site[0].SwMatrix.ActivatePath(Cal_Prod[i].Switch_Rx.ToUpper(), EqLib.Operation.N2toAnt);

                    // Case Joker.
                    //Eq.Site[0].SwMatrix.ActivatePath("OUT1", EqLib.Operation.N3toRx);
                    //Eq.Site[0].SwMatrix.ActivatePath("OUT2", EqLib.Operation.N4toRx);
                    //Eq.Site[0].SwMatrix.ActivatePath("OUT3", EqLib.Operation.N5toRx);
                    //Eq.Site[0].SwMatrix.ActivatePath("OUT4", EqLib.Operation.N6toRx);

                    //if ((Cal_Prod[i].Switch_Rx.Contains("MLB")) || (Cal_Prod[i].Switch_Rx.Contains("GSM")) || (Cal_Prod[i].Switch_Rx.Contains("DRX")) || (Cal_Prod[i].Switch_Rx.Contains("MIMO")))
                    //{
                    //    Eq.Site[0].SwMatrix.ActivatePath("OUT-DRX", EqLib.Operation.N3toRx);
                    //    Eq.Site[0].SwMatrix.ActivatePath("IN-MLB", EqLib.Operation.N4toRx);
                    //    Eq.Site[0].SwMatrix.ActivatePath("IN-GSM", EqLib.Operation.N5toRx);
                    //    Eq.Site[0].SwMatrix.ActivatePath("OUT-MIMO", EqLib.Operation.N6toRx);
                    //}


                    Thread.Sleep(100);

                    double[] compData = new double[SParamData[Cal_Prod[i].ChannelNumber - 1].NoPoints];

                    ENA.Format.DATA(e_FormatData.ASC);
                    //ENA.BasicCommand.System.SendCommand("FORM:DATA ASC");

                    //Find the Trace from NF CH
                    string traceNum = ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + Cal_Prod[i].ChannelNumber);
                    traceNum = traceNum.Trim('\"', '\n');
                    string[] tr = traceNum.Split(',');

                    //Incident DUT Relative NPwr mode set
                    ENA.BasicCommand.System.SendCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":PAR DUTRNPI");

                    //Avg Count On
                    ENA.BasicCommand.System.SendCommand("SENS" + Cal_Prod[i].ChannelNumber + ":AVER:COUN 20");
                    ENA.BasicCommand.System.SendCommand("SENS" + Cal_Prod[i].ChannelNumber + ":AVER ON");
                    string AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[i].ChannelNumber + ":AVER?");

                    ENA.Trigger.Single(Convert.ToInt32(Cal_Prod[i].ChannelNumber));
                    ENA.BasicCommand.System.Operation_Complete();

                    string nF_Data = ENA.BasicCommand.System.ReadCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":DATA:FDAT?");
                    string nF_Freq = ENA.BasicCommand.System.ReadCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":X?");
                    ENA.BasicCommand.System.Operation_Complete();

                    compData = Array.ConvertAll(nF_Data.Trim('\n').Split(','), new Converter<string, double>(Convert.ToDouble));

                    double maxVal = compData.Max();
                    double minVal = compData.Min();

                    try
                    {
                        if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                            verifyPass &= true;
                        else
                        {
                            for (int j = 0; j < nTry; j++)
                            {
                                ENA.Trigger.Single(Convert.ToInt32(Cal_Prod[i].ChannelNumber));
                                ENA.BasicCommand.System.Operation_Complete();

                                nF_Data = ENA.BasicCommand.System.ReadCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":DATA:FDAT?");
                                nF_Freq = ENA.BasicCommand.System.ReadCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":X?");
                                ENA.BasicCommand.System.Operation_Complete();

                                compData = Array.ConvertAll(nF_Data.Trim('\n').Split(','), new Converter<string, double>(Convert.ToDouble));

                                maxVal = compData.Max();
                                minVal = compData.Min();

                                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                                {
                                    verifyPass &= true;
                                    break;
                                }
                                else if (j == (nTry - 1))
                                {
                                    verifyPass &= false;
                                    verificationTxt.Add("Verification results are out of limits. Channel: " + Cal_Prod[i].ChannelNumber.ToString() + "  Parameter: " + "  IncidentDUT Relative NPwr" + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                    throw new Exception("Verification results are out of limits. \n\nChannel: " + Cal_Prod[i].ChannelNumber.ToString() + "\nParameter: " + "IncidentDUT Relative NPwr" + "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        DisplayError("Sub Cal Verification Error", ex);
                    }
                    ENA.BasicCommand.System.SendCommand("CALC" + Cal_Prod[i].ChannelNumber + ":MEAS" + tr[0] + ":PAR NF");
                    ENA.BasicCommand.System.SendCommand("SENS" + Cal_Prod[i].ChannelNumber + ":AVER OFF");
                    AVGstatus = ENA.BasicCommand.System.ReadCommand("SENS" + Cal_Prod[i].ChannelNumber + ":AVER?");
                }
            }

            //Validate the enr files
            //string[] EnrFiles = Directory.GetFiles(@"C:\Users\Public\Documents\Network Analyzer", "*.enr", SearchOption.TopDirectoryOnly);
            string mm = "";
            List<string> ENRtable = new List<string>();
            List<double> xyzzz = new List<double>();
            List<double> xyzzz1 = new List<double>();
            List<double> xyzzz2 = new List<double>();
            foreach (string x in ListOfENRfiles)
            {
                #region Slope of the ENR file

                int skipheader = 0;
                foreach (string y in File.ReadLines(@"C:\Users\Public\Documents\Network Analyzer\" + x))
                {
                    try
                    {
                        if (skipheader > 5)
                        {
                            mm = y.Substring(18, 5);
                            ENRtable.Add(mm);
                            xyzzz.Add(Convert.ToDouble(mm));
                        }
                        skipheader++;
                    }
                    catch (Exception e)
                    {
                        string nonon = y;
                    }
                }
                xyzzz1 = xyzzz.GetRange(0, (xyzzz.Count() / 4));
                int asdf = xyzzz1.Count();
                double zfirst = xyzzz1.Average();
                xyzzz2 = xyzzz.GetRange((xyzzz.Count() / 4) * 3, (xyzzz.Count() / 4));
                int dfglg = xyzzz2.Count();
                double zsec = xyzzz2.Average();
                double myslope = (zsec - zfirst) / 3;

                #endregion Slope of the ENR file

                //The slope has to be negative and steeper than -0.05
                if (myslope > -0.05)
                {
                    verificationTxt.Add("ENR file: " + x + " failed verification due to slope issue. Slope needs to be steeper than -0.5 and measured slope is " + myslope.ToString());
                    verifyPass = false;
                }


            }

            NFResult.WriteLine("Cal Substrate Verification " + (verifyPass == true ? "PASSED" : "FAILED"), "Cal Substrate Verification");
            for (int i = 0; i < verificationTxt.Count; i++)
            {
                NFResult.WriteLine(verificationTxt[i]);
            }
            NFResult.Close();

            MessageBox.Show("Cal Substrate Verification " + (verifyPass == true ? "PASSED" : "FAILED"), "Cal Substrate Verification");
        }

        public void SubCalVerification() //seoul
        {
            bool verifyPass = true;

            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            var bands = (from calkit in Cal_Prod select calkit.Switch_Input).Distinct();
            var RxPort = (from calkit in Cal_Prod select calkit.Switch).Distinct();


            foreach (var band in RxPort)
            {

                #region "Switch"

                m_eqsm.ActivatePath(band[0].ToUpper(), Operation.ENAtoRFIN);
                m_eqsm.ActivatePath(band[1].ToUpper(), Operation.ENAtoRFOUT);
                m_eqsm.ActivatePath(band[2].ToUpper(), Operation.ENAtoRX);

                //InstrLib.SwitchMatrix.Maps.Activate(band[0].ToString().ToUpper(), InstrLib.Operation.ENAtoRFIN);
                //InstrLib.SwitchMatrix.Maps.Activate(band[1].ToString().ToUpper(), InstrLib.Operation.ENAtoRFOUT);
                //InstrLib.SwitchMatrix.Maps.Activate(band[2].ToString().ToUpper(), InstrLib.Operation.ENAtoRX);

                //InstrLib.SwitchMatrix.Maps.Activate(band[0].ToString().ToUpper(), InstrLib.Operation.N1toTx);
                //InstrLib.SwitchMatrix.Maps.Activate(band[1].ToString().ToUpper(), InstrLib.Operation.N2toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate("MB1", InstrLib.Operation.N3toAnt);
                //InstrLib.SwitchMatrix.Maps.Activate("MB2", InstrLib.Operation.N4toRx);
                //InstrLib.SwitchMatrix.Maps.Activate("HB1", InstrLib.Operation.N5toRx);
                //InstrLib.SwitchMatrix.Maps.Activate("HB2", InstrLib.Operation.N6toRx);



                #endregion

                //For switch
                Thread.Sleep(100);

                var SWBand = band[1];
                var channels = (from calkit in Cal_Prod where calkit.Switch_Ant == SWBand select calkit.ChannelNumber).Distinct();

                foreach (var channel in channels)
                {
                    if (channel < 86)
                    {
                        var sparams = (from trace in TraceMatch[Convert.ToInt32(channel) - 1].SParam_Def_Number
                            where
                                trace == (int)Enum.Parse(typeof(e_SParametersDef), "S11") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S22") ||
                                trace == (int)Enum.Parse(typeof(e_SParametersDef), "S33") || trace == (int)Enum.Parse(typeof(e_SParametersDef), "S44")
                            select ((e_SParametersDef)trace).ToString()).Distinct();

                        double d_start_freq = (from segment in SegmentParam[Convert.ToInt32(channel) - 1].SegmentData where segment.Start > 1E9 && segment.Start < 3E9 select segment.Start).Min();
                        double d_stop_freq = (from segment in SegmentParam[Convert.ToInt32(channel) - 1].SegmentData where segment.Stop > 1E9 && segment.Stop < 3E9 select segment.Stop).Max();

                        int start_freq = (int)(d_start_freq / (double)1e6);
                        int stop_freq = (int)(d_stop_freq / (double)1e6);

                        foreach (var sparam in sparams)
                        {
                            verifyPass &= VerifyCalData(
                                Convert.ToInt32(channel),
                                sparam, -100, -25);

                            //verifyPass &= VerifyCalData(
                            //        Convert.ToInt32(channel),
                            //        sparam, -1.2, 1.2);

                            //verifyPass &= VerifyCalData(
                            //Convert.ToInt32(channel),
                            //sparam, -100, -30);

                            //verifyPass &= VerifyCalData(
                            //Convert.ToInt32(channel),
                            //sparam,
                            //start_freq, stop_freq, -70, -30);
                        }
                    }
                }
            }
            MessageBox.Show("Cal Substrate Verification " + (verifyPass == true ? "PASSED" : "FAILED"), "Cal Substrate Verification");

            //DialogResult verifySubCal = MessageBox.Show("Do you want to verify FBAR sub-cal with LOAD?", "WSD NPI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //switch (verifySubCal)
            //{
            //    case DialogResult.Yes:
            //        CalDone = true;
            //        doubleCheck = true;
            //        break;
            //    case DialogResult.No:
            //        iCalCount = 0;
            //        CalDone = false;
            //        doubleCheck = false;
            //        break;
            //    default:
            //        break;
            //}
            ENA.Display.Update(false);

            //return doubleCheck;
        }

        public bool VerifyCalData(int channel, string sparam, double VerLimitLow, double VerLimitHigh) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;

            try
            {
                List<string> allTraces, traceParam;
                //double[] freqList;                    
                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                // double[] tmpFreqlist;
                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                double[] compData = new double[SParamData[channel - 1].NoPoints];

                // StartIdx = StopIdx = 0;

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();
                //trace = trace.Replace("ch" + channel, "").Replace("tr", "").Replace("_", "").Replace("\"","").Trim();                    
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //freqList = ENA.Sense.Frequency.FreqList(channel, traceNumber);

                // for (int i = 1; i < freqList.Length; i++)
                // {
                //     if (freqList[i] > freqStartMhz * 1E6)
                //     {
                //         StartIdx = i;
                //         break;
                //     }

                // }
                // for (int i = freqList.Length - 1; i >= 0; i--)
                // {
                //     if (freqList[i] <= freqStopMhz * 1E6)
                //     {
                //         StopIdx = i;
                //         break;
                //     }

                // }


                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    // Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxVal = compData.Max();
                    double minVal = compData.Min();

                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                        status = true;
                    else
                        throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                                            "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                    //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\nStartFreq(MHz): " + freqStartMhz.ToString() + "\nStopFreq(MHz): " + freqStopMhz.ToString() +
                    //    "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal);

                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                    //throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\nStartFreq(MHz): " + freqStartMhz.ToString() + "\nStopFreq(MHz): " + freqStopMhz.ToString() +
                    //   "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }

        public bool VerifyCalData_new(int channel, string sparam, double VerLimitLow, double VerLimitHigh) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                double[] compData = new double[SParamData[channel - 1].NoPoints];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    // Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxVal = compData.Max();
                    double minVal = compData.Min();

                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                    }
                    else
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();
                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            maxVal = compData.Max();
                            minVal = compData.Min();

                            if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh)) //pass
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                            }       
                        }
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }

        //split verifyCal data under freq and ovr freq 
        public bool VerifyCalData_split_Freq(int channel, string sparam, double VerLimitLow, double VerLimitHigh, double VerFreq, double VerLimitHighFromFreq) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                int freqIndex = Array.FindIndex(SParamData[channel - 1].Freq, x => x > VerFreq);
                double[] compData = new double[SParamData[channel - 1].NoPoints];
                double[] compDataUnderFreq = new double[freqIndex];
                double[] compDataOverFreq = new double[SParamData[channel - 1].NoPoints- freqIndex];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);
                    compDataUnderFreq = compData.Take(freqIndex).ToArray();
                    compDataOverFreq = compData.Skip(freqIndex).ToArray();
                    ENA.BasicCommand.System.Operation_Complete();
                    // Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxValUnderFreq = compDataUnderFreq.Max();
                    double minValUnderFreq = compDataUnderFreq.Min();

                    double maxValOverFreq = compDataOverFreq.Max();
                    double minValOverFreq = compDataOverFreq.Min();


                    if (((minValUnderFreq > VerLimitLow) && (maxValUnderFreq < VerLimitHigh)) && 
                        ((minValOverFreq > VerLimitLow) && (maxValOverFreq < VerLimitHighFromFreq)))
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000,2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                                               " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                    }
                    else
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();

                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            compDataUnderFreq = compData.Take(freqIndex).ToArray();
                            compDataOverFreq = compData.Skip(freqIndex).ToArray();

                            ENA.BasicCommand.System.Operation_Complete();

                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            maxValUnderFreq = compDataUnderFreq.Max();
                            minValUnderFreq = compDataUnderFreq.Min();

                            maxValOverFreq = compDataOverFreq.Max();
                            minValOverFreq = compDataOverFreq.Min();

                            if (((minValUnderFreq > VerLimitLow) && (maxValUnderFreq < VerLimitHigh)) &&
                                ((minValOverFreq > VerLimitLow) && (maxValOverFreq < VerLimitHighFromFreq)))
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "Under Freq : " + VerFreq / 1000000 + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                                                       " Over Freq " + VerFreq / 1000000 + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                                                       " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                                                    " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\nOOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxValUnderFreq + "\nMeasured Value (Min): " + minValUnderFreq +
                                //"\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHighFromFreq + "\nMeasured Value (Max): " + maxValOverFreq + "\nMeasured Value (Min): " + minValOverFreq);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex); return false;
            }
            return status;
        }

        public bool VerifyCalData_NF(int channel, double VerLimitLow, double VerLimitHigh) //seoul
        {
            bool status = false;
            int nTry = 3;

            try
            {
                double[] compData = new double[SParamData[channel - 1].NoPoints];

                ENA.Format.DATA(e_FormatData.REAL);
                //ENA.Format.DATA(e_FormatData.ASC);
                //ENA.BasicCommand.System.SendCommand("FORM:DATA ASC");

                //Find the Trace from NF CH
                string traceNum = ENA.Calculate.Par.GetTraceCategory(channel); //ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + channel);
                traceNum = traceNum.Trim('\"', '\n');
                string[] tr = traceNum.Split(',');

                //Incident DUT Relative NPwr mode set
                ENA.Calculate.Par.Define_Trace(channel, Convert.ToInt16(tr[0]), "DUTRNPI");
                ENA.BasicCommand.System.Operation_Complete();
                //ENA.BasicCommand.System.SendCommand("CALC" + channel + ":MEAS" + tr[0] + ":PAR DUTRNPI");

                //Avg Count On
                ENA.Sense.Average.Count(channel, 20);
                ENA.Sense.Average.State(channel, e_OnOff.On);
                //ENA.BasicCommand.System.SendCommand("SENS" + channel + ":AVER:COUN 20");
                //ENA.BasicCommand.System.SendCommand("SENS" + channel + ":AVER ON");
                //string AVGstatus = ENA.Sense.Average.State(channel);// ENA.BasicCommand.System.ReadCommand("SENS" + channel + ":AVER?");

                ENA.Trigger.Single(Convert.ToInt32(channel));
                ENA.BasicCommand.System.Operation_Complete();

                compData = ENA.Calculate.Data.FData(channel, Convert.ToInt16(tr[0]));
                ENA.BasicCommand.System.Operation_Complete();
                //string nF_Freq = ENA.Sense.Frequency.FreqList(channel, Convert.ToInt16(tr[0]));
                //string nF_Data = ENA.BasicCommand.System.ReadCommand("CALC" + channel + ":MEAS" + tr[0] + ":DATA:FDAT?"); //ENA.Calculate.Data.FData(Cal_Prod[i].ChannelNumber, Convert.ToInt16(tr[0])); 
                //string nF_Freq = ENA.BasicCommand.System.ReadCommand("CALC" + channel + ":MEAS" + tr[0] + ":X?"); //ENA.Sense.Frequency.FreqList(Cal_Prod[i].ChannelNumber, Convert.ToInt16(tr[0]));
                //ENA.BasicCommand.System.Operation_Complete();

                //compData = Array.ConvertAll(nF_Data.Trim('\n').Split(','), new Converter<string, double>(Convert.ToDouble));

                ENA.Calculate.Par.Define_Trace(channel, Convert.ToInt16(tr[0]), "NF");
                ENA.BasicCommand.System.Operation_Complete();
                //ENA.BasicCommand.System.SendCommand("CALC" + channel + ":MEAS" + tr[0] + ":PAR NF");
                ENA.Sense.Average.State(channel, e_OnOff.Off);
                ENA.BasicCommand.System.Operation_Complete();
                //ENA.BasicCommand.System.SendCommand("SENS" + channel + ":AVER OFF");
                //AVGstatus = ENA.Sense.Average.State(channel);//ENA.BasicCommand.System.ReadCommand("SENS" + channel + ":AVER?");


                double maxVal = compData.Max();
                double minVal = compData.Min();


                if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                    status = true;
                else
                {
                    for (int j = 0; j < nTry; j++)
                    {
                        ENA.Calculate.Par.Define_Trace(channel, Convert.ToInt16(tr[0]), "DUTRNPI");
                        ENA.BasicCommand.System.Operation_Complete();

                        //Avg Count On
                        ENA.Sense.Average.Count(channel, 20);
                        ENA.Sense.Average.State(channel, e_OnOff.On);
                        //AVGstatus = ENA.Sense.Average.State(channel);// ENA.BasicCommand.System.ReadCommand("SENS" + channel + ":AVER?");

                        ENA.Trigger.Single(channel);
                        ENA.BasicCommand.System.Operation_Complete();

                        compData = ENA.Calculate.Data.FData(channel, Convert.ToInt16(tr[0]));
                        //nF_Data = ENA.BasicCommand.System.ReadCommand("CALC" + channel + ":MEAS" + tr[0] + ":DATA:FDAT?");
                        //nF_Freq = ENA.BasicCommand.System.ReadCommand("CALC" + channel + ":MEAS" + tr[0] + ":X?");
                        ENA.BasicCommand.System.Operation_Complete();

                        //compData = Array.ConvertAll(nF_Data.Trim('\n').Split(','), new Converter<string, double>(Convert.ToDouble));

                        ENA.Calculate.Par.Define_Trace(channel, Convert.ToInt16(tr[0]), "NF");
                        ENA.BasicCommand.System.Operation_Complete();
                        ENA.Sense.Average.State(channel, e_OnOff.Off);
                        ENA.BasicCommand.System.Operation_Complete();

                        maxVal = compData.Max();
                        minVal = compData.Min();

                        if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                        {
                            status = true;
                            verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: Incident DUT Relative NPwr  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                            break;
                        }
                        else if (j == (nTry - 1))
                        {
                            verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: Incident DUT Relative NPwr  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                            verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: Incident DUT Relative NPwr  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                            //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: Incident DUT Relative NPwr\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
            }

            return status;
        }

        //ChoonChin - To save all traces during subcal verification
        public bool VerifyCalData_new_SaveTrace(int channel, string sparam, double VerLimitLow, double VerLimitHigh, ref List<double> TraceData) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                double[] compData = new double[SParamData[channel - 1].NoPoints];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);

                    foreach (double tdata in compData)
                    {
                        TraceData.Add(tdata);
                    }

                    ENA.BasicCommand.System.Operation_Complete();
                    // Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxVal = compData.Max();
                    double minVal = compData.Min();

                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                    }
                    else
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();
                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            maxVal = compData.Max();
                            minVal = compData.Min();

                            if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh)) //pass
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }
        public bool VerifyCalData_split_Freq_SaveTrace(int channel, string sparam, double VerLimitLow, double VerLimitHigh, double VerFreq, double VerLimitHighFromFreq, ref List<double> TraceData) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                int freqIndex = Array.FindIndex(SParamData[channel - 1].Freq, x => x > VerFreq);
                double[] compData = new double[SParamData[channel - 1].NoPoints];
                double[] compDataUnderFreq = new double[freqIndex];
                double[] compDataOverFreq = new double[SParamData[channel - 1].NoPoints - freqIndex];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);

                    foreach (double tdata in compData)
                    {
                        TraceData.Add(tdata);
                    }

                    compDataUnderFreq = compData.Take(freqIndex).ToArray();
                    compDataOverFreq = compData.Skip(freqIndex).ToArray();
                    ENA.BasicCommand.System.Operation_Complete();
                    // Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxValUnderFreq = compDataUnderFreq.Max();
                    double minValUnderFreq = compDataUnderFreq.Min();

                    double maxValOverFreq = compDataOverFreq.Max();
                    double minValOverFreq = compDataOverFreq.Min();


                    if (((minValUnderFreq > VerLimitLow) && (maxValUnderFreq < VerLimitHigh)) &&
                        ((minValOverFreq > VerLimitLow) && (maxValOverFreq < VerLimitHighFromFreq)))
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                            " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                    }
                    else
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();

                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            compDataUnderFreq = compData.Take(freqIndex).ToArray();
                            compDataOverFreq = compData.Skip(freqIndex).ToArray();

                            ENA.BasicCommand.System.Operation_Complete();

                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            maxValUnderFreq = compDataUnderFreq.Max();
                            minValUnderFreq = compDataUnderFreq.Min();

                            maxValOverFreq = compDataOverFreq.Max();
                            minValOverFreq = compDataOverFreq.Min();

                            if (((minValUnderFreq > VerLimitLow) && (maxValUnderFreq < VerLimitHigh)) &&
                                ((minValOverFreq > VerLimitLow) && (maxValOverFreq < VerLimitHighFromFreq)))
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "Under Freq : " + VerFreq / 1000000 + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                            " Over Freq " + VerFreq / 1000000 + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                            " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + " OOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxValUnderFreq + "  Measured Value (Min): " + minValUnderFreq +
                            " Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHighFromFreq + "  Measured Value (Max): " + maxValOverFreq + "  Measured Value (Min): " + minValOverFreq);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\nOOB Verify Freq : " + Math.Round(SParamData[channel - 1].Freq[freqIndex] / 1000000, 2) + " Mhz\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\nMeasured Value (Max): " + maxValUnderFreq + "\nMeasured Value (Min): " + minValUnderFreq +
                                //"\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHighFromFreq + "\nMeasured Value (Max): " + maxValOverFreq + "\nMeasured Value (Min): " + minValOverFreq);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }

        //ChoonChin - 20191120 - For spec setting until 2.8 GHz only - Pinot
        public bool VerifyCalData_new_SaveTrace_FreqSpec(int channel, string sparam, double VerLimitLow, double VerLimitHigh, int FreqLimitCount, ref List<double> TraceData) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                double[] compData = new double[SParamData[channel - 1].NoPoints];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);

                    int loop = 0;

                    foreach (double tdata in compData)
                    {
                        if (loop == FreqLimitCount)
                        {
                            break;
                        }

                        TraceData.Add(tdata);
                        loop++;
                    }

                    ENA.BasicCommand.System.Operation_Complete();
                    //Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    //double maxVal = compData.Max();
                    //double minVal = compData.Min();

                    double maxVal = TraceData.Max();
                    double minVal = TraceData.Min();

                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                    }
                    else
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();
                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            loop = 0;
                            TraceData.Clear();
                            foreach (double tdata in compData)
                            {
                                if (loop == FreqLimitCount)
                                {
                                    break;
                                }

                                TraceData.Add(tdata);
                                loop++;
                            }

                            maxVal = TraceData.Max();
                            minVal = TraceData.Min();

                            if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh)) //pass
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }//

        //ChoonChin - 20191121 - For spec setting until 2.8 GHz only and full trace saving - Pinot
        public bool VerifyCalData_new_SaveTrace_FreqSpec(int channel, string sparam, double VerLimitLow, double VerLimitHigh, int FreqLimitCount, bool VerifyNeeded, ref List<double> TraceData) //double freqStartMhz, double freqStopMhz, 
        {
            bool status = false;
            int nTry = 3;

            try
            {
                List<string> allTraces, traceParam;

                int trcIdx, StartIdx, StopIdx;
                int points = SParamData[channel - 1].NoPoints;

                List<double> VerifyTraceData = new List<double>();

                string trace;
                string[] traces;
                int traceNumber = 1;

                string TrFormat = "";

                double[] compData = new double[SParamData[channel - 1].NoPoints];

                ENA.Trigger.Single(channel);
                ENA.BasicCommand.System.Operation_Complete();

                ENA.Format.DATA(e_FormatData.REAL);
                trace = ENA.Calculate.Par.GetAllCategory(channel);
                trace = trace.Replace("_", "").Replace("\n", "").Replace("\"", "").Trim();

                //Spara Trace check
                allTraces = trace.Split(new char[] { ',' }).ToList();
                traceParam = allTraces.Where((item, index) => index % 2 != 0).ToList();

                //Trace Number check
                trace = ENA.Calculate.Par.GetTraceCategory(channel);
                trace = trace.Replace("\n", "").Replace("\"", "").Trim();
                traces = trace.Split(new char[] { ',' }).ToArray();

                //Get Index of sParam
                if (sparam == "S10_10") sparam = "S1010";
                trcIdx = traceParam.IndexOf(sparam);

                if (trcIdx >= 0)
                {
                    traceNumber = Convert.ToInt32(traces[trcIdx]);
                    TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    compData = ENA.Calculate.Data.FData(channel, traceNumber);

                    int loop = 0;
                    bool stopAdding = false;

                    foreach (double tdata in compData)
                    {
                        if (VerifyNeeded && !stopAdding)
                        {
                            if (loop == FreqLimitCount - 1)
                            {
                                stopAdding = true;
                            }
                            VerifyTraceData.Add(tdata);
                        }

                        TraceData.Add(tdata);
                        loop++;
                    }

                    ENA.BasicCommand.System.Operation_Complete();
                    //Array.Resize(ref FData, SParamData[channel].NoPoints);
                    ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                    double maxVal = 1;
                    double minVal = 0;

                    if (VerifyNeeded)
                    {
                        maxVal = VerifyTraceData.Max();
                        minVal = VerifyTraceData.Min();
                    }

                    if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh) && VerifyNeeded)
                    {
                        status = true;
                        verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                    }
                    else if (VerifyNeeded)
                    {
                        for (int i = 0; i < nTry; i++)
                        {
                            traceNumber = Convert.ToInt32(traces[trcIdx]);
                            TrFormat = ENA.Calculate.Format.returnFormat(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            ENA.Calculate.Format.setFormat(channel, traceNumber, e_SFormat.MLOG);
                            ENA.BasicCommand.System.Operation_Complete();
                            compData = ENA.Calculate.Data.FData(channel, traceNumber);
                            ENA.BasicCommand.System.Operation_Complete();
                            // Array.Resize(ref FData, SParamData[channel].NoPoints);
                            ENA.Calculate.Format.setFormat(channel, traceNumber, TrFormat);

                            loop = 0;
                            VerifyTraceData.Clear();
                            foreach (double tdata in compData)
                            {
                                if (loop == FreqLimitCount)
                                {
                                    break;
                                }

                                VerifyTraceData.Add(tdata);
                                loop++;
                            }

                            maxVal = VerifyTraceData.Max();
                            minVal = VerifyTraceData.Min();

                            if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh)) //pass
                            {
                                status = true;
                                verificationTxtAll.Add("Verification results are within limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                break;
                            }
                            else if (i == (nTry - 1))
                            {
                                verificationTxtAll.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                verificationTxt.Add("Verification results are out of limits. Channel: " + channel.ToString() + "  Parameter: " + sparam + "  Low Limit: " + VerLimitLow + "  High Limit: " + VerLimitHigh + "  Measured Value (Max): " + maxVal + "  Measured Value (Min): " + minVal);
                                //throw new Exception("Verification results are out of limits. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam + "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh + "\n\nMeasured Value (Max): " + maxVal + "\n\nMeasured Value (Min): " + minVal);
                            }
                        }
                    }
                    else
                    {
                        //Nothing yet
                    }
                }
                else
                {
                    throw new Exception("Trace " + trcIdx.ToString() + " cannot be found. \n\nChannel: " + channel.ToString() + "\nParameter: " + sparam +
                        "\n\nLow Limit: " + VerLimitLow + "\nHigh Limit: " + VerLimitHigh);
                }
            }
            catch (Exception ex)
            {
                DisplayError("Sub Cal Verification Error", ex);
                return false;
            }
            return status;
        }//

        #region Re arrange Auto Calibration Function (**Same work as before) 

        public void Auto_VerificationAll(StreamWriter SubCalResult, bool isUsing28Ohm)
        {            
            bool SparaverifyPass = true;
            bool NFverifyPass = true;

            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            string[] S_paracheck = new string[Cal_Prod.Length];

            verificationTxt.Clear();
            verificationTxtAll.Clear();

            //ChoonChin - To save all traces during subcal verification
            double[] FreqPoint;
            List<double> MagData = new List<double>();
            List<double> FreqData = new List<double>();
            List<string> TraceFile = new List<string>();
            string foldertime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Defined Verify 
            for (int i = 0; i < Cal_Prod.Length; i++)
            {
                S_paracheck[i] = isRequiredVerify(i);
            }

            for (int i = 0; i < S_paracheck.Length; i++)
            {
                try
                {
                    //Set Switch Path for Verify
                    ProjectSpecificSwitching.SetSwitchMatrixPaths(Cal_Prod[i].Switch_Input.ToUpper(), Cal_Prod[i].Switch_Ant.ToUpper(), Cal_Prod[i].Switch_Rx.ToUpper());

                    //for debugging purpose - messagebox to manually check the cal once switched in
                    //MessageBox.Show("ANT port is " + Cal_Prod[i].Switch_Ant.ToUpper() + "\r\n" + "Rx port is " + Cal_Prod[i].Switch_Rx.ToUpper(), "");

                    Thread.Sleep(5); //@need to time to setting for switch at least 1ms

                    if (S_paracheck[i].ToUpper() == "SPARA")
                    {
                        var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                                       where ProjectSpecificSwitching.dicTraceIndexTable.ContainsKey(trace)
                                       select ((e_SParametersDef)trace).ToString()).Distinct();


                        double d_start_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Start > 1E9 && segment.Start < 6E9 select segment.Start).Min();
                        double d_stop_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Stop > 1E9 && segment.Stop < 8E9 select segment.Stop).Max();

                        int start_freq = (int)(d_start_freq / (double)1e6);
                        int stop_freq = (int)(d_stop_freq / (double)1e6);

                        double channelStopFreq = SParamData[Cal_Prod[i].ChannelNumber - 1].Freq.Max() / (double)1e6;

                        //ChoonChin - To save all traces during subcal verification
                        FreqPoint = SParamData[Cal_Prod[i].ChannelNumber - 1].Freq;
                        FreqData.Clear();

                        //ChoonChin - 20191120 - For spec setting until 2.8 GHz only.
                        int FreqPointCount = 0;
                        int LoopCount = 0;
                        double FreqSpec = 2800000000; //3GHz
                        bool VerifyNeeded = true;
                        List<double> VerifyFreqData = new List<double>();
                        //

                        foreach (double fdata in FreqPoint)
                        {
                            //ChoonChin - 20191111 - For spec setting until 3 GHz only.
                            if (fdata <= FreqSpec)
                            {
                                FreqPointCount = LoopCount;
                                VerifyFreqData.Add(fdata);
                                LoopCount++;
                            }
                            //

                            FreqData.Add(fdata); //Full FreqData                            
                        }

                        //ChoonChin - 20191120 - To handle < 2.8 GHz segment
                        if (LoopCount == FreqPoint.Length)
                        {
                            FreqPointCount = LoopCount;
                        }

                        //ChoonChin - 20191120 - To handle >2.8 Ghz segment, skip verify
                        if (FreqPointCount == 0)
                        {
                            VerifyNeeded = false;
                        }

                        foreach (var sparam in sparams)
                        {
                            MagData.Clear();
                            if (!isUsing28Ohm)
                            {
                                if (channelStopFreq > 4000)
                                {
                                    //SparaverifyPass &= VerifyCalData_split_Freq_SaveTrace
                                    SparaverifyPass &= VerifyCalData_split_Freq_SaveTrace(
                                            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                            sparam, -120, -20, 4000 * 1e6, -18, ref MagData); //28 Ohm from -9 to -12
                                }
                                else
                                {
                                    //SparaverifyPass &= VerifyCalData_new_SaveTrace
                                    SparaverifyPass &= VerifyCalData_new_SaveTrace(
                                            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                            sparam, -120, -20, ref MagData);
                                }
                            }
                            else
                            {
                                ////ChoonChin - For 28Ohm verification
                                SparaverifyPass &= VerifyCalData_new_SaveTrace_FreqSpec(
                                    Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                    sparam, -11.5, -10.5, FreqPointCount, VerifyNeeded, ref MagData);

                                //{
                                //    SparaverifyPass &= VerifyCalData_new_SaveTrace(
                                //            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                //            sparam, -11.5, -10.5, ref MagData);
                                //}
                            }

                            //ChoonChin - To save all traces during subcal verification
                            //Combine 2 lists into dictionary
                            Dictionary<double, double> TraceTable = new Dictionary<double, double>();
                            string fileName = "";

                            //ChoonChin - 20191111 - To handle >3Ghz segment, skip verify
                            TraceTable = FreqData.ToDictionary(x => x, x => MagData[FreqData.IndexOf(x)]);
                            fileName = "CH" + Cal_Prod[i].ChannelNumber + "_" + sparam.ToString();
                            TraceFile.Add("Freq," + sparam.ToString());
                            foreach (KeyValuePair<double, double> kvp in TraceTable)
                            {
                                TraceFile.Add(kvp.Key + "," + kvp.Value);
                            }

                            //Create csv
                            if (true)
                            {
                                string newFileFolder = "C:\\Avago.ATF.Common.x64\\Input\\Verification\\" + foldertime + "_AllTraces";
                                if (!System.IO.Directory.Exists(newFileFolder))
                                {
                                    System.IO.Directory.CreateDirectory(newFileFolder);
                                }
                                System.IO.StreamWriter SW;
                                string newFilePath = newFileFolder + "\\" + fileName + ".csv";
                                SW = System.IO.File.CreateText(newFilePath);

                                foreach (string line in TraceFile)
                                {
                                    SW.WriteLine(line);
                                }
                                SW.Close();
                            }
                            TraceFile.Clear();
                        }
                    }
                    else if (S_paracheck[i].ToUpper() == "NF")
                    {

                        NFverifyPass &= VerifyCalData_NF(
                            Convert.ToInt32(Cal_Prod[i].ChannelNumber), -1, 1);

                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Error occured CH" + (i + 1), e.ToString()); - Temp disable for Pinot proto2
                }

            }

            ENA.Display.Update(false);

            //ChoonChin - Change port power to segment 28Ohm
            if (isUsing28Ohm)
            {
                ChangePortPowerToSegmentPower(ProjectSpecificSwitching.PortEnable);
            }
            MessageBox.Show("Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "Spara Cal Substrate Verification");
            //MessageBox.Show("Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            SubCalResult.WriteLine("Spara Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "SPara Cal Substrate Verification");
            //SubCalResult.WriteLine("NF Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            for (int i = 0; i < verificationTxt.Count; i++)
            {
                SubCalResult.WriteLine(verificationTxt[i]);
            }

            ////ChoonChin - Save all
            //for (int i = 0; i < verificationTxtAll.Count; i++)
            //{
            //    SubCalResult.WriteLine(verificationTxtAll[i]);
            //}

            SubCalResult.Close();
        }

        public void Auto_VerificationOnly(StreamWriter SubCalResult, bool isUsing28Ohm, int portPower)
        {
            //Change port power to cal power:
            ChangePortPowerToDefinedPower(ProjectSpecificSwitching.PortEnable, portPower);

            bool SparaverifyPass = true;
            bool NFverifyPass = true;

            ENA.BasicCommand.System.Operation_Complete();
            ENA.Display.Update(true);

            string[] S_paracheck = new string[Cal_Prod.Length];

            verificationTxt.Clear();
            verificationTxtAll.Clear();

            //ChoonChin - To save all traces during subcal verification
            double[] FreqPoint;
            List<double> MagData = new List<double>();
            List<double> FreqData = new List<double>();
            List<string> TraceFile = new List<string>();
            string foldertime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Defined Verify 
            for (int i = 0; i < Cal_Prod.Length; i++)
            {
                S_paracheck[i] = isRequiredVerify(i);
            }

            for (int i = 0; i < S_paracheck.Length; i++)
            {
                try
                {
                    //Set Switch Path for Verify
                    ProjectSpecificSwitching.SetSwitchMatrixPaths(Cal_Prod[i].Switch_Input.ToUpper(), Cal_Prod[i].Switch_Ant.ToUpper(), Cal_Prod[i].Switch_Rx.ToUpper());

                    Thread.Sleep(5); //@need to time to setting for switch at least 1ms

                    if (S_paracheck[i].ToUpper() == "SPARA")
                    {
                        var sparams = (from trace in TraceMatch[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SParam_Def_Number
                                       where ProjectSpecificSwitching.dicTraceIndexTable.ContainsKey(trace)
                                       select ((e_SParametersDef)trace).ToString()).Distinct();


                        double d_start_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Start > 1E9 && segment.Start < 6E9 select segment.Start).Min();
                        double d_stop_freq = (from segment in SegmentParam[Convert.ToInt32(Cal_Prod[i].ChannelNumber) - 1].SegmentData where segment.Stop > 1E9 && segment.Stop < 8E9 select segment.Stop).Max();

                        int start_freq = (int)(d_start_freq / (double)1e6);
                        int stop_freq = (int)(d_stop_freq / (double)1e6);

                        double channelStopFreq = SParamData[Cal_Prod[i].ChannelNumber - 1].Freq.Max() / (double)1e6;

                        //ChoonChin - To save all traces during subcal verification
                        FreqPoint = SParamData[Cal_Prod[i].ChannelNumber - 1].Freq;
                        FreqData.Clear();

                        //ChoonChin - 20191120 - For spec setting until 2.8 GHz only.
                        int FreqPointCount = 0;
                        int LoopCount = 0;
                        double FreqSpec = 2800000000; //3GHz
                        bool VerifyNeeded = true;
                        List<double> VerifyFreqData = new List<double>();
                        //

                        foreach (double fdata in FreqPoint)
                        {
                            //ChoonChin - 20191111 - For spec setting until 3 GHz only.
                            if (fdata <= FreqSpec)
                            {
                                FreqPointCount = LoopCount;
                                VerifyFreqData.Add(fdata);
                                LoopCount++;
                            }
                            //

                            FreqData.Add(fdata); //Full FreqData                            
                        }

                        //ChoonChin - 20191120 - To handle < 2.8 GHz segment
                        if (LoopCount == FreqPoint.Length)
                        {
                            FreqPointCount = LoopCount;
                        }

                        //ChoonChin - 20191120 - To handle >2.8 Ghz segment, skip verify
                        if (FreqPointCount == 0)
                        {
                            VerifyNeeded = false;
                        }

                        foreach (var sparam in sparams)
                        {
                            MagData.Clear();
                            if (!isUsing28Ohm)
                            {
                                if (channelStopFreq > 4000)
                                {
                                    //SparaverifyPass &= VerifyCalData_split_Freq_SaveTrace
                                    SparaverifyPass &= VerifyCalData_split_Freq_SaveTrace(
                                            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                            sparam, -120, -20, 4000 * 1e6, -18, ref MagData); //28 Ohm from -9 to -12
                                }
                                else
                                {
                                    //SparaverifyPass &= VerifyCalData_new_SaveTrace
                                    SparaverifyPass &= VerifyCalData_new_SaveTrace(
                                            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                            sparam, -120, -20, ref MagData);
                                }
                            }
                            else
                            {
                                ////ChoonChin - For 28Ohm verification
                                SparaverifyPass &= VerifyCalData_new_SaveTrace_FreqSpec(
                                    Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                    sparam, -11.5, -10.5, FreqPointCount, VerifyNeeded, ref MagData);

                                //{
                                //    SparaverifyPass &= VerifyCalData_new_SaveTrace(
                                //            Convert.ToInt32(Cal_Prod[i].ChannelNumber),
                                //            sparam, -11.5, -10.5, ref MagData);
                                //}
                            }

                            //ChoonChin - To save all traces during subcal verification
                            //Combine 2 lists into dictionary
                            Dictionary<double, double> TraceTable = new Dictionary<double, double>();
                            string fileName = "";

                            //ChoonChin - 20191111 - To handle >3Ghz segment, skip verify
                            TraceTable = FreqData.ToDictionary(x => x, x => MagData[FreqData.IndexOf(x)]);
                            fileName = "CH" + Cal_Prod[i].ChannelNumber + "_" + sparam.ToString();
                            TraceFile.Add("Freq," + sparam.ToString());
                            foreach (KeyValuePair<double, double> kvp in TraceTable)
                            {
                                TraceFile.Add(kvp.Key + "," + kvp.Value);
                            }

                            //Create csv
                            if (true)
                            {
                                string newFileFolder = "C:\\Avago.ATF.Common.x64\\Input\\Verification\\" + foldertime + "_AllTraces";
                                if (!System.IO.Directory.Exists(newFileFolder))
                                {
                                    System.IO.Directory.CreateDirectory(newFileFolder);
                                }
                                System.IO.StreamWriter SW;
                                string newFilePath = newFileFolder + "\\" + fileName + ".csv";
                                SW = System.IO.File.CreateText(newFilePath);

                                foreach (string line in TraceFile)
                                {
                                    SW.WriteLine(line);
                                }
                                SW.Close();
                            }
                            TraceFile.Clear();
                        }
                    }
                    else if (S_paracheck[i].ToUpper() == "NF")
                    {

                        NFverifyPass &= VerifyCalData_NF(
                            Convert.ToInt32(Cal_Prod[i].ChannelNumber), -1, 1);

                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Error occured CH" + (i + 1), e.ToString()); - Temp disable for Pinot proto2
                }

            }

            ENA.Display.Update(false);

            //ChoonChin - Change port power to segment 28Ohm
            if (isUsing28Ohm)
            {
                ChangePortPowerToSegmentPower(ProjectSpecificSwitching.PortEnable);
            }
            MessageBox.Show("Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "Spara Cal Substrate Verification");
            //MessageBox.Show("Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            SubCalResult.WriteLine("Spara Cal Substrate Verification " + (SparaverifyPass == true ? "PASSED" : "FAILED"), "SPara Cal Substrate Verification");
            //SubCalResult.WriteLine("NF Cal Substrate Verification " + (NFverifyPass == true ? "PASSED" : "FAILED"), "NF Cal Substrate Verification");

            for (int i = 0; i < verificationTxt.Count; i++)
            {
                SubCalResult.WriteLine(verificationTxt[i]);
            }

            ////ChoonChin - Save all
            //for (int i = 0; i < verificationTxtAll.Count; i++)
            //{
            //    SubCalResult.WriteLine(verificationTxtAll[i]);
            //}

            SubCalResult.Close();
        }

        public void Auto_CalibrateAll(CalibrationInputDataObject calDo, string ENR_file, int topazCalPower, int[] DoCalCh = null, bool Diva = false) 
        {                
            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/" + ENR_file; 
                                
            string eCalKit = DetectEcalKit();
            bool IsPartialCal = DoCalCh == null ? false : true;
                                
            //Init Setting for Calibration 
            TopazCal_Init(Flag, ENRfile, IsPartialCal, topazCalPower, DoCalCh, Diva);                
                
            for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++)
            {
                if (IsPartialCal && Array.IndexOf(DoCalCh, Cal_Prod[iCal].ChannelNumber) < 0 && Cal_Prod[iCal].ChannelNumber != lastChannel)
                {
                    if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                    {
                        calDo.iCalCount = iCal;
                        calDo.CalContinue = true;
                        calDo.Fbar_cal = false;
                        break;
                    }
                    if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                    {
                        calDo.CalContinue = false;
                    }
                    continue; //91 NF cal channel 
                }

                ////Set Switch config
                //if (!ProjectSpecificSwitching.SetSwitchMatrixPaths(Cal_Prod[iCal].Switch_Input.ToUpper(), Cal_Prod[iCal].Switch_Ant.ToUpper(), Cal_Prod[iCal].Switch_Rx.ToUpper(), false))
                //{
                //    MessageBox.Show("The Parameter Type is" + Cal_Prod[iCal].ParameterType + "and the Band is " + Cal_Prod[iCal].Switch_Input.ToUpper() + "\r\n");
                //}

                //Set Switch config
                if (!ProjectSpecificSwitching.SetSwitchMatrixPaths(Cal_Prod[iCal].Switch_Input.ToUpper(), Cal_Prod[iCal].Switch_Ant.ToUpper(), Cal_Prod[iCal].Switch_Rx.ToUpper())) 
                {
                    MessageBox.Show("The Parameter Type is" + Cal_Prod[iCal].ParameterType + "and the Band is " + Cal_Prod[iCal].Switch_Input.ToUpper() + "\r\n");
                }

                //To be obsolated
                //Thread.Sleep(100); 

                //Calibration Message
                string tmpStr = strDisplayLog(iCal);

                //b_Mode Setting
                if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER) b_Mode = false;

                //Cal Kit Procedure
                if (AllCalkitProcedure(calDo, iCal, ENRfile, eCalKit, tmpStr, Diva)) break;

                SaveCalibrationStatus(calDo, iCal, IsPartialCal, DoCalCh);

            } //for (int iCal = calDo.iCalCount; iCal < Cal_Prod.Length; iCal++) 

        } //public void Auto_CalibrateAll(CalibrationInputDataObject calDo, string ENR_file, int topazCalPower, int[] DoCalCh = null, bool Diva = false)  

        private string isRequiredVerify(int _CalIndex)
        {
            string S_paracheck = "";

            if (Cal_Prod[_CalIndex].CalType == e_CalibrationType.OPEN)
            {
                switch (Cal_Prod[_CalIndex].CalKit.ToString())
                {
                    case "1":
                        S_paracheck = "SPARA";
                        break;

                    case "2":
                        S_paracheck = "NF";
                        break;

                    default:
                        S_paracheck = "NONE";
                        break;
                }
            }
            else
            {
                S_paracheck = "NONE";
            }

            return S_paracheck;
        } 
        private string DetectEcalKit()
        {
            string eCalKit = "";

            string[] listnum = ENA.Sense.Correction.Collect.ECAL.ModuleList().Split(','); //ENA.BasicCommand.ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?").Split(',');
            if (Int32.Parse(listnum[0]) > 0)
            {
                string[] EcalList = new string[listnum.Length];

                for (int i = 0; i < listnum.Length; ++i)
                {
                    listnum[i] = Int32.Parse(listnum[i]).ToString();
                    string[] EcalInfo = ENA.Sense.Correction.Collect.ECAL.ModuleInfo(listnum[i]).Split(',', ':'); // ENA.BasicCommand.ReadCommand("SENS:CORR:CKIT:ECAL" + listnum[i] + ":INF?").Split(',', ':');
                    EcalList[i] = EcalInfo[1].Trim() + " ECal " + EcalInfo[3].Trim();
                    // Example of return value: 
                    // "ModelNumber: N4431B, SerialNumber: 03605, ConnectorType: 35F 35F 35F 35F, PortAConnector: APC 3.5 female, PortBConnector: APC 3.5 female, PortCConnector: APC 3.5 female, PortDConnector: APC 3.5 female, MinFreq: 9000, MaxFreq: 13510000000, NumberOfPoints: 336, Calibrated: 19/Dec/2007, CharacterizedBy: 0000099999, NetworkAnalyzer: US44240045"
                    eCalKit = EcalList[0];
                }
            }
            else
            {
                //MessageBox.Show("Please check Ecal Module");//If Ecalkit not existed, message will be shown   
            }

            return eCalKit;
        }

        private void TopazCal_Init(bool _flag, string ENRfile, bool _IsPartialCal, int topazCalPower, int[] _DoCalCh, bool isDiva)
        {
            if (_flag)
            {
                //ChoonChin - 20191120 - Turn off display during Sub calibration 
                ENA.BasicCommand.SendCommand("DISP:ENAB 1");

                //Used to be commented out by MM (06/22/2017) - ECAL is being used in combination with subcal.  This line keeps blowing the ecal away
                if (!_IsPartialCal)
                {
                    //ENA.Sense.Correction.Collect.GuidedCal.Delete_AllCalData();
                }

                ENA.BasicCommand.System.Operation_Complete();

                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++) //To set last channel in ENA state
                {
                    if (Cal_Prod[iCal].ChannelNumber > lastChannel) lastChannel = Cal_Prod[iCal].ChannelNumber;
                }

                for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
                {
                    if (_IsPartialCal && Array.IndexOf(_DoCalCh, Cal_Prod[iCal].ChannelNumber) < 0 && Cal_Prod[iCal].ChannelNumber != lastChannel)
                    {
                        continue;
                    }

                    GuidedCalDefinedThru(iCal);

                    //For DIVA
                    if (isDiva)
                    {
                        if (Cal_Prod[iCal].ParameterType == "NF") //during NF cal, regular thru is used instead of starthru. 10 for switch, 1 for thru
                        {
                            ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:HLC ON");
                            ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:MIL 10"); 
                        }
                        else //During spara cal, star thru is used.  10 for switch, 23.5 for starthru
                        {
                            ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:HLC ON");
                            ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:MIL 33.5");
                            //ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:MIL 11");
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= 90; i++)
                            ENA.BasicCommand.SendCommand($":SENS" + i + ":AVER:state ON");
                    }

                    //Change Source Power due to StarThru loss..
                    if (Cal_Prod[iCal].ParameterType != "NF") ENA.Sense.Segment.ChangeSegmentPower(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, topazCalPower, SegmentParam[Cal_Prod[iCal].ChannelNumber - 1]);

                    DefineCalConfig(iCal, ENRfile); 

                    ENA.BasicCommand.System.Operation_Complete();
                }

                CreatCalCheckListAndStepList(_IsPartialCal, _DoCalCh); //to be opsolated..
                                        
                Flag = false;
            }
        }
        private void GuidedCalDefinedThru(int iCal)
        {
            switch (Cal_Prod[iCal].No_Ports)
            {
                case 3:
                    ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3);
                    break;
                case 4:
                    ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4);
                    break;
                case 5:
                    ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4, Cal_Prod[iCal].Port_5);
                    break;
                case 6:
                    ENA.Sense.Correction.Collect.GuidedCal.DefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_3, Cal_Prod[iCal].Port_4, Cal_Prod[iCal].Port_5, Cal_Prod[iCal].Port_6);
                    break;
                default://Use Defualt Thru Setting
                    break;
            }
        }
        private void DefineCalConfig(int iCal, string ENRfile)
        {
            switch (Cal_Prod[iCal].CalType) //Seoul - This sets the connector type and calkit
            {
                case e_CalibrationType.OPEN:
                case e_CalibrationType.ECAL_OPEN:
                        
                    ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Type);
                    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);                        
                    break;

                case e_CalibrationType.NS_POWER:
                        
                    Cal_Prod[iCal].Port_1 = Convert.ToInt16(ENA.Sense.Noise.InputPort(Cal_Prod[iCal].ChannelNumber));
                    Cal_Prod[iCal].Port_2 = Convert.ToInt16(ENA.Sense.Noise.OutputPort(Cal_Prod[iCal].ChannelNumber));
                    ENA.Sense.Noise.PortMapping(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2); //ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":NOIS:PMAP " + NFsrcPortNum + "," + NFrcvPortNum);                            

                    string dummy= ENA.Calculate.Par.GetTraceCategory(Cal_Prod[iCal].ChannelNumber); //ENA.BasicCommand.System.ReadCommand("SYST:MEAS:CAT? " + NFchNum);    // instrument.ReadString();
                    dummy = dummy.Trim('\"', '\n');
                    string[] tr = dummy.Split(',');

                    ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Convert.ToInt32(tr[0])); //ENA.BasicCommand.System.SendCommand("CALC" + NFchNum + ":PAR:MNUM " + tr[0]);
                    ENA.Sense.Average.Count(Cal_Prod[iCal].ChannelNumber, 10);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER:COUN 20");
                    ENA.Sense.Average.State(Cal_Prod[iCal].ChannelNumber, e_OnOff.On);// ENA.BasicCommand.System.SendCommand("SENS" + NFchNum + ":AVER ON");
                    ENA.BasicCommand.System.Operation_Complete();
                        
                    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                    ENA.Sense.Correction.Collect.GuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit_Name);

                    ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].DUT_NF_InPort_Conn);
                    ENA.Sense.Correction.Collect.GuidedCal.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Type);
                    ENA.BasicCommand.System.Operation_Complete();
                        
                    ENA.Sense.Noise.DefineConnectorType(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Type);
                    ENA.Sense.Noise.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit_Name);
                    ENA.Sense.Noise.LoadENRfile(Cal_Prod[iCal].ChannelNumber, ENRfile);
                    ENA.Sense.Noise.SelectCalMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.Scalar);
                    ENA.Sense.Noise.SelectRecMethod(Cal_Prod[iCal].ChannelNumber, e_NoiseCalMethod.NoiseSource);
                    ENA.Sense.Noise.Temperature(Cal_Prod[iCal].ChannelNumber, 302.8f); //Lindsay
                    break;

                case e_CalibrationType.THRU:
                    break;

                case e_CalibrationType.UnknownTHRU:
                    ENA.Sense.Correction.Collect.GuidedCal.unDefineThru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                    break;

            }
        }
        private void CreatCalCheckListAndStepList(bool _IsPartialCal, int[] _DoCalCh)
        { 
            ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
            int[] step = new int[TotalChannel];
            int[] Cal_Check = new int[TotalChannel];
            List<int> Cal_Check_list = new List<int>();

            Cal_Check_list.Clear();

            for (int iCal = 0; iCal < Cal_Prod.Length; iCal++)
            {
                if ((Cal_Prod[iCal].CalType == e_CalibrationType.OPEN) || (Cal_Prod[iCal].CalType == e_CalibrationType.ECAL_OPEN))
                {
                    if (_IsPartialCal && Array.IndexOf(_DoCalCh, Cal_Prod[iCal].ChannelNumber) < 0 && Cal_Prod[iCal].ChannelNumber != lastChannel)
                    {
                        continue;
                    }
                    Cal_Check_list.Add(Cal_Prod[iCal].ChannelNumber);
                }
            }
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                Cal_Check[iChn] = 0;
            }

            foreach (int ch in Cal_Check_list)
            {
                Cal_Check[ch - 1] = 1;
            }
            ///////////////////////////////////////////////////////////////////////////////////////

            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                // ENA.Sense.Correction.Collect.GuidedCal.DefineThru(iChn + 1, 2);
                ////// Send InitGuidedCal() only if Cal_Check[iChn] == 1 (20161206)
                if (Cal_Check[iChn] == 1)
                {
                    ENA.Sense.Correction.Collect.GuidedCal.InitGuidedCal(iChn + 1);
                    ENA.BasicCommand.System.Operation_Complete();
                    if (iChn == 0) Thread.Sleep(100);
                    step[iChn] = Int32.Parse(ENA.Sense.Correction.Collect.GuidedCal.CalibrationStep(iChn + 1));

                    //here for debugging purpose - MM
                    //==============================
                    List<string> stepdescriptions = new List<string>();
                    string stepdesc = "";
                    for (int i = 0; i < step[iChn]; i++)
                    {
                        stepdesc = ENA.Sense.Correction.Collect.GuidedCal.CalibrationStepDesc((iChn + 1), i+1);
                        stepdescriptions.Add(stepdesc);
                    }
                    //==================================
                    Thread.Sleep(10);
                    stepdescriptions.Clear();
                }
                else
                {
                    step[iChn] = 0;
                }
            }
        }

        private string strDisplayLog(int iCal)
        {               
            string tmpStr = "";

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
                    case e_CalibrationType.OPEN:
                    case e_CalibrationType.ECAL_OPEN:
                    case e_CalibrationType.SHORT:
                    case e_CalibrationType.ECAL_SHORT:
                    case e_CalibrationType.LOAD:
                    case e_CalibrationType.ECAL_LOAD:
                    case e_CalibrationType.ECAL_SAVE_A:
                    case e_CalibrationType.ECAL_SAVE_B:
                    case e_CalibrationType.SUBCLASS:
                    case e_CalibrationType.TRLREFLECT:
                        tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();
                        break;
                    case e_CalibrationType.THRU:
                    case e_CalibrationType.UnknownTHRU:
                    case e_CalibrationType.TRLLINE:
                    case e_CalibrationType.TRLTHRU:
                        tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString() + "," + Cal_Prod[iCal].Port_2.ToString();
                        break;
                    case e_CalibrationType.NS_POWER:
                        tmpStr += "Port " + Cal_Prod[iCal].Port_1.ToString();                           
                        break;
                }
            }

            return "";
        }
            
        private bool AllCalkitProcedure(CalibrationInputDataObject calDo, int iCal, string ENRfile, 
            string eCalKit,string tmpStr, bool isDivaInstrument)
        {
            if (Cal_Prod[iCal].b_CalKit)
            {
                if (!b_Mode)
                {
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                    {
                        if (Cal_Prod[iCal].Message.Trim() != "")
                        {
                            DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                        }
                    }
                    else
                    {
                        if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                        {
                            calDo.iCalCount = iCal;
                            calDo.CalContinue = true;
                            calDo.Fbar_cal = false;
                            return true; 
                        }
                        if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                        {
                            calDo.CalContinue = false;
                        }

                        //to be obsolated..
                        //Thread.Sleep(100); //100
                    }
                }


                ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Calibration Type: " + Cal_Prod[iCal].CalType);
                ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Error, "Channel: " + Cal_Prod[iCal].ChannelNumber);

                CalkitProcedure(iCal, ENRfile, eCalKit, isDivaInstrument);
            }
            else
            {
                if (!b_Mode)
                {
                    if (Cal_Prod[iCal].CalType == e_CalibrationType.NS_POWER)
                    {
                        if (Cal_Prod[iCal].Message.Trim() != "")
                        {
                            DisplayMessage(Cal_Prod[iCal].CalType, tmpStr);
                        }
                    }
                    else
                    {
                        if (Cal_Prod[iCal].Message.Trim() != "" && !calDo.CalContinue)
                        {
                            calDo.iCalCount = iCal;
                            calDo.CalContinue = true;
                            calDo.Fbar_cal = false;
                            return true;
                        }
                        else if (Cal_Prod[iCal].Message.Trim() != "" && calDo.CalContinue)
                        {
                            calDo.CalContinue = false;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }

                NonCalkitProcedure(iCal);

                //to be obsolated..
                Thread.Sleep(100); //100
            }

            return false;
        }

        private void CalkitProcedure(int iCal, string ENRfile, string eCalKit, bool isDivaInstrument)
        {
            switch (Cal_Prod[iCal].CalType)
            {
                case e_CalibrationType.OPEN:
                case e_CalibrationType.SHORT:
                case e_CalibrationType.LOAD:
                case e_CalibrationType.THRU:
                case e_CalibrationType.ECAL_OPEN:
                case e_CalibrationType.ECAL_SHORT:
                case e_CalibrationType.ECAL_LOAD:
                    if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    Thread.Sleep(200);
                    ENA.BasicCommand.System.Operation_Complete();
                    break;

                case e_CalibrationType.ECAL_SAVE_A:
                    //////////// This is new /////////////// seoul
                    // Save full 1-port guided calibration for noise source ENR characterization, save “CalSet_A”           
                    string cSetA = "";
                    CalsetnameA = "";
                    CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type;
                    //CalsetnameA = Cal_Prod[iCal].Switch_Ant +"_" + Cal_Prod[iCal].Type + "_CalSet_A";// + Cal_Prod[iCal].Type;
                    //CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type; //Original
                    cSetA = ENA.Sense.Correction.Collect.Cset.Exist(CalsetnameA);
                    //cSetA = ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameA + "\"");
                    if (cSetA.TrimEnd('\n') == "1")
                    {
                        ENA.Sense.Correction.Collect.Cset.Delete(CalsetnameA);
                        ENA.BasicCommand.System.Operation_Complete();
                        //ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameA + "\"");
                    }
                    ENA.Sense.Correction.Collect.Cset.Save(Cal_Prod[iCal].ChannelNumber, CalsetnameA);
                    //ENA.BasicCommand.SendCommand("SENS" + Cal_Prod[iCal].ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalsetnameA + "\"");
                    ENA.BasicCommand.System.Operation_Complete();
                    break;

                case e_CalibrationType.ECAL_SAVE_B:

                    CalsetnameB = "";

                    ENA.BasicCommand.System.Clear_Status();
                    for (int i = 0; i < 3; i++)
                    {
                        // Specifies the Ecal Kit for Ecal Calibration
                        //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:COLL:GUID:ECAL:SEL \"" + eCalKit + "\"");
                        ENA.Sense.Correction.Collect.ECAL.SelectEcal(Cal_Prod[iCal].ChannelNumber, eCalKit);
                        // Acquire Ecal Oopn/Short/Load standards
                        System.Threading.Thread.Sleep(2000);
                        ENA.Sense.Correction.Collect.ECAL.SOLT1(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1);
                        //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + NFrcvPortNum);
                        System.Threading.Thread.Sleep(2000);
                        ENA.BasicCommand.System.Operation_Complete();

                        string Ecalstat = ENA.BasicCommand.System.Event_Register(false);

                        if (Ecalstat != "No Error") //Non-zero return means an error(s) happened
                        {
                            MessageBox.Show("An error occurred during Calibrate_Topaz(): " + "\r\n" + Ecalstat + "\r\n" +
                                            "Error happened during Ecal of CH" + Cal_Prod[iCal].ChannelNumber + " for Port" + Cal_Prod[iCal].Port_1 + "\r\n");
                            if (i > 2)
                            {
                                throw new Exception("Calibrate_Topaz() error " + "\r\n" + Ecalstat);
                            }
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            break;
                        }
                        //Added this line here to isolate individual channel errors from performing ecal
                        ENA.BasicCommand.System.Clear_Status(); //MM - clearing the status of the event registers to isolate errors from calibration 

                    } //for (int i = 0; i < 3; i++)


                    // Finish 1-port calibrtion and store CalSet as "NS_CalSet_B_CH*"
                    CalsetnameB = Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + "_CalSet_B";
                    CalsetnameA = Cal_Prod[iCal].Switch_Ant + "_CalSet_A";// + Cal_Prod[iCal].Type;

                    string cSetB = ENA.Sense.Correction.Collect.Cset.Exist(CalsetnameB); //ENA.BasicCommand.ReadCommand(":CSET:EXIS? \"" + CalsetnameB + "\"");
                    if (cSetB.TrimEnd('\n') == "1")
                    {
                        ENA.Sense.Correction.Collect.Cset.Delete(CalsetnameB);
                        ENA.BasicCommand.System.Operation_Complete();
                        //ENA.BasicCommand.SendCommand(":CSET:DEL \"" + CalsetnameB + "\"");
                    }
                    //ENA.BasicCommand.SendCommand(":SENS" + eCalCH + ":CORR:CSET:COPY \"" + CalsetnameB + "\"");
                    ENA.Sense.Correction.Collect.Cset.Copy(Cal_Prod[iCal].ChannelNumber, CalsetnameB);
                    //ENA.Sense.Correction.Collect.Cset.Save(Cal_Prod[iCal].ChannelNumber, CalsetnameB);
                    ENA.BasicCommand.System.Operation_Complete();


                    AdapterSnP = "Adapter_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".s2p";
                    FixtureSnP = "Fixture_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".s2p";
                    // Read 2-port Ecal thru data, save “Ecal_thru.s2p”
                    // portpair: Ecal thru port pair
                    // AB: Ecal port A is connected to VNA side, port B is connected to  noise source side
                    // BA: Ecal port A is connected to noise source side, port B is connected to VNA side
                    //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "BA" + "\", \"Ecal_thru.s2p\""); // Original
                    ENA.Sense.Correction.Collect.ECAL.saveEcalSnp(eCalKit, "AB", "Ecal_thru.s2p");
                    //ENA.BasicCommand.SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + eCalKit + "\", \"" + "AB" + "\", \"Ecal_thru.s2p\"");
                    ENA.BasicCommand.System.Operation_Complete();

                    // Calculate adapter data from "CalSet_A_CH*" and "CalSet_B_CH*", save "Adapter_CH*.s2p"
                    ENA.Sense.Correction.Collect.Cset.CharacterizedFixture(CalsetnameA, CalsetnameB, Cal_Prod[iCal].Port_1, AdapterSnP, e_SweepType.LOG);
                    ENA.BasicCommand.System.Operation_Complete();
                    //ENA.BasicCommand.SendCommand(":CSET:FIXT:CHAR \"NS_CalSet_A_CH" + eCalCH + "\" , \"NS_CalSet_B_CH" + eCalCH + "\" , " + NFrcvPortNum + " , \"Adapter_CH" + eCalCH + ".s2p\" , LOG");
                    // Calculate cascaded S-parameter from "Adapter_CH*.s2p" and "Ecal_thru.s2p", save "Fixture_CH*.s2p" 
                    //ENA.BasicCommand.SendCommand(":CSET:FIXT:CASC \"Adapter_CH" + eCalCH + ".s2p\" , \"Ecal_thru.s2p\" , \"Fixture_CH" + eCalCH + ".s2p\" , LOG");
                    ENA.Sense.Correction.Collect.Cset.Combine2Snp(AdapterSnP, "Ecal_thru.s2p", FixtureSnP, e_SweepType.LOG);
                    ENA.BasicCommand.System.Operation_Complete();

                    // Calculate noise source ENR from "Fixture.s2p" and original ENR data ("Original.enr"), save "New_Port_*.enr"
                    //ENA.BasicCommand.SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\" , \"Fixture_CH" + eCalCH + ".s2p\" , \"NS_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr\"");
                    //ENA.Sense.Correction.Collect.Cset.newENR(ENRfile, FixtureSnP, Cal_Prod[iCal].Switch_Ant + "_CharacterizedENR_" + Cal_Prod[iCal].Type + ".enr"); //Original
                    ENA.Sense.Correction.Collect.Cset.newENR(ENRfile, FixtureSnP, "New_CharacterizedENR_" + Cal_Prod[iCal].Switch_Ant + "_" + Cal_Prod[iCal].Type + ".enr");
                    ENA.BasicCommand.System.Operation_Complete();
                    break;



                case e_CalibrationType.NS_POWER:
                    // Set Ecal state to “Thru” explicitly for hot/cold noise power measurement from noise source (optional, Topaz firmware automatically set this normaly)
                    int EcalModuleNum = 1; // In the case of using connected Ecal #1                                                                            
                    //ENA.BasicCommand.SendCommand(":CONT:ECAL:MOD" + EcalModuleNum + ":PATH:STAT AB,1");
                    ENA.Sense.Correction.Collect.ECAL.PathState_2Port(EcalModuleNum);
                    ENA.BasicCommand.System.Operation_Complete();
                    ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    Thread.Sleep(200);
                    ENA.BasicCommand.System.Operation_Complete();

                    break;


                case e_CalibrationType.UnknownTHRU:
                    //Thread.Sleep(100);

                    if (isDivaInstrument) ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:CAV 20");

                    if (iCal == 0) ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    ENA.Sense.Correction.Collect.GuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    //Thread.Sleep(200);
                    ENA.BasicCommand.System.Operation_Complete();
                    Thread.Sleep(500);

                    if (isDivaInstrument) ENA.BasicCommand.SendCommand($":DIAG:VVNA:CAP:SWG:CAV 0");

                    break;

                case e_CalibrationType.TRLLINE:
                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Line(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                    ENA.BasicCommand.System.Operation_Complete();
                    break;
                case e_CalibrationType.TRLREFLECT:
                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Reflect(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                    ENA.BasicCommand.System.Operation_Complete();
                    break;
                case e_CalibrationType.TRLTHRU:
                    ENA.Sense.Correction.Collect.Cal_Kit.TRL_Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].CalKit);
                    ENA.BasicCommand.System.Operation_Complete();
                    break;
                case e_CalibrationType.ISOLATION:
                    bool verifyPass = true;
                    int NOP = SParamData[Cal_Prod[iCal].ChannelNumber - 1].NoPoints;
                    double[] compData = new double[NOP];
                    int loopCount = 0;
                    string TrFormat = "";
                    double VerLimitLow = 0,
                        VerLimitHigh = 0,
                        maxVal = 0,
                        minVal = 0;

                    while ((!verifyPass && loopCount < 3) || (loopCount == 0))
                    {
                        verifyPass = false;
                        #region Math->Normalize
                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                        //ENA.Calculate.Math.SetMath(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                        //ENA.Calculate.Math.MathOperation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                        //ENA.BasicCommand.System.Operation_Complete();
                        #endregion

                        #region resopnse cal using normalize
                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                        ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber); //seoul
                        ENA.Sense.Correction.Collect.UnGuidedCal.ResponseCal(Cal_Prod[iCal].ChannelNumber);
                        ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                        Thread.Sleep(100);
                        ENA.BasicCommand.System.Operation_Complete();
                        ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                        #endregion

                        #region response cal using average value of cal kit
                        //ENA.Calculate.Par.SelectTrace(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                        //ENA.Sense.Correction.Collect.UnGuidedCal.setCalKit(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].CalKit_Name);
                        //ENA.Sense.Correction.Collect.UnGuidedCal.ResponseTHRU(Cal_Prod[iCal].ChannelNumber);
                        //ENA.Sense.Correction.Collect.UnGuidedCal.AcquireStandNum(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].CalKit);
                        //ENA.BasicCommand.System.Operation_Complete();
                        //ENA.Sense.Correction.Collect.Save(Cal_Prod[iCal].ChannelNumber);
                        #endregion

                        #region varification response cal
                        Thread.Sleep(100);
                        ENA.Format.DATA(e_FormatData.REAL);
                        //seoul
                        //TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                        //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, e_SFormat.MLOG);
                        TrFormat = ENA.Calculate.Format.returnFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);
                        ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, e_SFormat.MLOG);


                        //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Input.ToUpper(), InstrLib.Operation.ENAtoRFIN);

                        //InstrLib.SwitchMatrix.Maps.Activate(Cal_Prod[iCal].Switch_Rx, InstrLib.Operation.ENAtoRX);
                        Thread.Sleep(100);
                        ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                        ENA.BasicCommand.System.Operation_Complete();

                        ENA.Trigger.Single(Cal_Prod[iCal].ChannelNumber);
                        ENA.BasicCommand.System.Operation_Complete();

                        VerLimitLow = -0.1;
                        VerLimitHigh = 0.1;
                        // NOP = SParamData[Cal_Prod[iCal].ChannelNumber].NoPoints;
                        // Array.Resize(ref FData, NOP);

                        //seoul
                        //compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Isolation_trace);
                        compData = ENA.Calculate.Data.FData(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber);

                        maxVal = compData.Max();
                        minVal = compData.Min();

                        //for (int j = 0; j < FData.Length; ++j)
                        //{
                        if ((minVal > VerLimitLow) && (maxVal < VerLimitHigh))//(compData[j] < -0.1 || compData[j] > 0.1)
                        {
                            verifyPass = true;
                        }
                        // }                                    
                        loopCount++;
                        //seoul
                        //ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Isolation_trace, TrFormat);
                        ENA.Calculate.Format.setFormat(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, TrFormat);
                        #endregion
                    }
                    if (loopCount == 3)
                    {
                        string errDesc = string.Format("Verification results are out of limits. \n\nChannel: {0}\nParameter: {1}\n\nLow Limit: {2}\nHigh Limit: {3}\n\nMeasured Value (Max): {4}\n\nMeasured Value (Min): {5}",
                            Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].TraceNumber, VerLimitLow, VerLimitHigh, maxVal, minVal);
                        DisplayCalProcedureError(errDesc);
                    }
                    break;
                default:
                    DisplayError(Cal_Prod[iCal]);
                    break;
            }
            //Thread.Sleep(100);
            ENA.BasicCommand.System.Operation_Complete();
        }
        private void NonCalkitProcedure(int iCal)
        {
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
                        if (Cal_Prod[iCal].ChannelNumber == 4 || Cal_Prod[iCal].ChannelNumber == 10)
                            ENA.Sense.Correction.Collect.Method.SOLT3(Cal_Prod[iCal].ChannelNumber, 1, 2, 4);
                        else
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
                        Thread.Sleep(4000);
                    }
                    Thread.Sleep(500);
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
                    Thread.Sleep(300);
                    ENA.Sense.Correction.Collect.Acquire.Isolation(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_2, Cal_Prod[iCal].Port_1);
                    ENA.BasicCommand.System.Operation_Complete();
                    break;
                case e_CalibrationType.THRU:
                    ENA.Sense.Correction.Collect.Acquire.Thru(Cal_Prod[iCal].ChannelNumber, Cal_Prod[iCal].Port_1, Cal_Prod[iCal].Port_2);
                    ENA.BasicCommand.System.Operation_Complete();
                    Thread.Sleep(300);
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
                    DisplayError(Cal_Prod[iCal]);
                    break;
            }
        }

        private void SaveCalibrationStatus(CalibrationInputDataObject calDo, int iCal, bool _IsPartialCal, int[] _DoCalCh)
        {
            if (iCal == Cal_Prod.Length - 1 && !calDo.CalContinue)
            {
                ////// Create int array Cal_Check[] for guided calibration channel check (20161206)
                int[] Cal_Check = new int[TotalChannel];
                List<int> Cal_Check_list = new List<int>();
                Cal_Check_list.Clear();
                for (int i = 0; i < Cal_Prod.Length; i++)
                {
                    if (Cal_Prod[i].CalType == e_CalibrationType.OPEN)
                    {
                        if (_IsPartialCal && Array.IndexOf(_DoCalCh, Cal_Prod[i].ChannelNumber) < 0 && Cal_Prod[i].ChannelNumber != lastChannel)
                        {
                            continue;
                        }
                        Cal_Check_list.Add(Cal_Prod[i].ChannelNumber);
                    }
                }
                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    Cal_Check[iChn] = 0;
                }

                foreach (int ch in Cal_Check_list)
                {
                    Cal_Check[ch - 1] = 1;
                }
                //////////////////////////////

                for (int iChn = 0; iChn < TotalChannel; iChn++)
                {
                    // Send Save_SmartCal() only if Cal_Check[iChn] == 1 (20161206)
                    if (Cal_Check[iChn] == 1)
                    {
                        string newENRfileName = "";
                        string chName = ENA.Sense.Class((iChn + 1));// ENA.BasicCommand.ReadCommand("SENS" + (iChn + 1) + ":CLAS:NAME?");

                        //The disconnect here is that the determination if a given channel is a Noise figure channel and the indexing for the NF input and output port do not come from the same place.  Have to find the index of the channel to get the appropriate INput and Output port designation.
                        //The question is, where do we get this information
                        if (chName.Contains("Noise"))
                        {
                            string NFrcvPortNum = ENA.Sense.Noise.OutputPort((iChn + 1)).TrimEnd('\n').Replace("+", ""); //ENA.BasicCommand.ReadCommand(":SENS" + (iChn + 1) + ":NOIS:PMAP:OUTP?").TrimEnd('\n').Replace("+", ""); // NF Receiver port (DUT output port)
                            string RxPath = "";

                            //zindex is used to as the index for the correct Switch.Ant to be used in forming the name of the ENR file
                            //The loop sets zindex to the index value of the correct channels NS_POWER index based on its place in Cal_Prod
                            int zindex = 0;
                            for (int i = 0; i < Cal_Prod.Length; i++)
                            {
                                //Get the Channel Number, Port_1 value, DUT NF Input Port Conn and DUT NF Output Port Conn
                                if (Cal_Prod[i].ChannelNumber == iChn + 1 && Cal_Prod[i].CalType == e_CalibrationType.NS_POWER) //Cal_Prod[i].Port_1.ToString() == NFrcvPortNum && 
                                {
                                    zindex = i;
                                    break;
                                }
                            }

                            RxPath = ProjectSpecificSwitching.dicRxOutNamemapping[Convert.ToInt16(NFrcvPortNum)];

                            ENA.Sense.Average.State((iChn + 1), e_OnOff.Off);
                            //ENA.BasicCommand.System.SendCommand("SENS" + (iChn + 1) + ":AVER OFF");
                            string AVGstatus = ENA.Sense.Average.State((iChn + 1)); //ENA.BasicCommand.System.ReadCommand("SENS" + (iChn + 1) + ":AVER?");

                            //string nS = "";
                            string antPort = Cal_Prod[zindex].Switch_Ant; //ENA.Sense.Noise.InputPort(iChn + 1).Trim().Remove(0, 1);
                            if (!Cal_Prod[zindex].Switch_Ant.Contains("NS-"))
                            {
                                antPort = "NS-" + antPort; // Cal_Prod[zindex].Switch_Ant;
                            }

                            newENRfileName = "C:/Users/Public/Documents/Network Analyzer/New_CharacterizedENR_" + Cal_Prod[zindex].Switch_Ant + "_" + RxPath + ".enr";

                            //MessageBox.Show("The Channel is " + (iChn+1).ToString() + "\r\n" + "The ENR file name is: " + newENRfileName, "Make sure file name is correct");

                            //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/New_CharacterizedENR_" + antPort + "_" + RxPath + ".enr";

                            //newENRfileName = "C:/Users/Public/Documents/Network Analyzer/exampleNew_CharacterizedENR_NS-ANT1_N77_OUT1-N77.enr";

                            //For NF cal debug purpose only
                            //Forcing the ENR file to be the valid type to avoid ENR loading problem
                            //if (iChn == 17) newENRfileName = "C:/Users/Public/Documents/Network Analyzer/exampleNew_CharacterizedENR_NS-ANT1_N77_OUT1-N77.enr";
                            //if (iChn == 18) newENRfileName = "C:/Users/Public/Documents/Network Analyzer/exampleNew_CharacterizedENR_NS-ANT2_N77_OUT3-N77.enr";
                            //if (iChn == 19) newENRfileName = "C:/Users/Public/Documents/Network Analyzer/exampleNew_CharacterizedENR_NS-ANT1_N79_OUT1-N79.enr";
                            //if (iChn == 20) newENRfileName = "C:/Users/Public/Documents/Network Analyzer/exampleNew_CharacterizedENR_NS-ANT2_N79_OUT2-N79.enr";


                            ENA.Sense.Noise.LoadENRfile((iChn + 1), newENRfileName);
                            //Thread.Sleep(10000);
                            ENA.BasicCommand.System.Operation_Complete();
                            //ENA.BasicCommand.SendCommand(":SENS" + (iChn + 1) + ":NOIS:ENR:FILENAME '" + newENRfileName + "'"); // Load Characterized Noise Source ENR file
                            ENA.Display.Update(true);
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            //Thread.Sleep(10000);
                            ENA.BasicCommand.System.Operation_Complete();
                        }
                        else
                        {
                            ENA.Sense.Correction.Collect.Save_SmartCal(iChn + 1);
                            ENA.BasicCommand.System.Operation_Complete();
                            //Set segment power back to -20 dBm - modified by CheeOn 17-July-2017
                            //if (Cal_Prod[iCal].ParameterType != "NF")
                            if (!calDo.Using28Ohm)
                            {
                                ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], ProjectSpecificSwitching.PortEnable);
                            }
                        }
                    }
                    ENA.Display.Update(false);
                }

                calDo.CalDone = true;
                calDo.Fbar_cal = false;

                    

                //DisplayMessageCalComplete();
            }

               

        }

        private void ChangePortPowerToSegmentPower(bool[] portList)
        {
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                ENA.Sense.Segment.ChangeSegmPower_Allport(iChn + 1, SegmentParam[iChn], portList);
            }
        }

        private void ChangePortPowerToDefinedPower (bool[] portList, int portPower)
        {
            for (int iChn = 0; iChn < TotalChannel; iChn++)
            {
                ENA.Sense.Segment.ChangeSegmentPowerVerifyOnly(iChn + 1, portPower, SegmentParam[iChn]);
            }
        }


        #endregion New Auto Calibration Function (**Same function as before) 

        private void DisplayMessage(e_CalibrationType msg1, string msg2)
        {
            PromptManager.Instance.ShowInfo(msg2, msg1 + " Calibration (Cal Kit)");
        }

        private void DisplayMessage2(e_CalibrationType msg1, string msg2)
        {
            PromptManager.Instance.ShowInfo(msg2, msg1 + " Calibration");
        }

        private void DisplayMessageCalComplete()
        {
            PromptManager.Instance.ShowInfo("Calibration Completed", "Calibration Complete");
        }

        private void DisplayError(string msg, Exception ex)
        {
            PromptManager.Instance.ShowError(msg, ex);
        }

        private void DisplayError(s_CalibrationProcedure cp)
        {
            string errDesc = string.Format("Unrecognize Calibration Procedure = {0} for Cal Kit Standard {1}", cp.CalType, cp.CalKit);
            PromptManager.Instance.ShowError("Error in Normal Calibration Procedure", errDesc);
        }

        private void DisplayCalProcedureError(string errDesc)
        {
            PromptManager.Instance.ShowError("Error in Normal Calibration Procedure", errDesc);
        }

    }
}