using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LibFBAR_TOPAZ.ANewEqLib;
using SParamTestCommon;

namespace TestPlanCommon.SParaModel
{
    public class SParaCalibrationDataObject
    {
        public int TopazCalPower { get; set; }
        public string NA_StateFile { get; set; }
        public string ENR_file { get; set; }

        public SParaCalibrationDataObject()
        {
            ENR_file = "ENR_NC346D_22dB_AF963.enr";
        }
    }

    /// <summary>
    /// This is like ENACalibrationModel, it wraps CalibrationModel3.
    /// </summary>
    public class CalibrationModel
    {
        private bool RaiseError_Calibration;
        private bool isDivaInstrument = false;
        private CalibrationModel3 m_model;

        public SParaCalibrationDataObject DataObject { get; set; }

        public void Initialize(CalibrationModel3 model)
        {
            m_model = model;
        }

        public void SetInstrumentDiva(bool isDiva)
        {
            isDivaInstrument = isDiva;
            //For DIVA
            if (isDiva)
            {
                DataObject.TopazCalPower = 5;
            }
            else
            {
                DataObject.TopazCalPower = 10;  //Change the Topaz output power level to 7dBm for high power calibration, added by Cheeon 28-July-2017
            }
        }

        public List<s_CalibrationProcedure> Load_Calibration(TcfSheetCalProcedure sheetCalProc)
        {
            List<s_CalibrationProcedure> listCalProc = new List<s_CalibrationProcedure>();

            for (int i = 0; i < sheetCalProc.testPlan.Count; i++)
            {
                sheetCalProc.SetCurrentRow(i);
                s_CalibrationProcedure proc = FillCalProcedure(sheetCalProc);
                listCalProc.Add(proc);
            }

            RaiseError_Calibration = listCalProc.Count == 0;

            m_model.parse_Procedure = listCalProc.ToArray();

            return listCalProc;
        }

        private s_CalibrationProcedure FillCalProcedure(TcfSheetCalProcedure sheet)
        {
            s_CalibrationProcedure proc = new s_CalibrationProcedure();
            var CalKit_No = sheet.GetDataInt("Standard Number");
            if (CalKit_No > 0)
            {
                proc.CalKit = CalKit_No;
                proc.b_CalKit = true;
            }

            proc.ChannelNumber = sheet.GetDataInt("Channel");
            proc.Message = sheet.GetData("Message Remarks");
            //KCC
            proc.Switch_Input = sheet.GetData("Switch_Input");
            proc.Switch_Ant = sheet.GetData("Switch_Ant");
            proc.Switch_Rx = sheet.GetData("Switch_Rx");
            //MM
            proc.DUT_NF_InPort_Conn = sheet.GetData("NF_Input_Port");
            proc.DUT_NF_OutPort_Conn = sheet.GetData("NF_Output_Port");
            proc.NF_SrcPortNum = sheet.GetDataInt("NF_SrcPortNum");

            List<string> CalSegment = new List<string>();
            string channel = "Channel#" + Convert.ToString(proc.ChannelNumber);
            if (!CalSegment.Contains(channel))
            {
                CalSegment.Add(channel);
            }

            proc.Switch = new string[3];

            proc.Switch[0] = sheet.GetData("Switch_Input");
            proc.Switch[1] = sheet.GetData("Switch_Ant");
            proc.Switch[2] = sheet.GetData("Switch_Rx");

            proc.No_Ports = sheet.GetDataInt("Number of Ports");

            proc.CalKit_Name = sheet.GetData("CALKIT Label");
            proc.Type = sheet.GetData("Type");
            proc.TraceNumber = sheet.GetDataInt("TraceNumber");

            proc.ParameterType = sheet.GetData("ParameterType"); // Yoonchun for distinguish NF & Spara

            var Cal_Type =
                (e_CalibrationType)Enum.Parse(typeof(e_CalibrationType), sheet.GetData("Calibration Type"));
            proc.CalType = Cal_Type;

            switch (Cal_Type)
            {
                case e_CalibrationType.ECAL:

                    proc.No_Ports = sheet.GetDataInt("Number of Ports");

                    for (int iPort = 0; iPort < proc.No_Ports; iPort++)
                    {
                        switch (iPort)
                        {
                            case 0:
                                proc.Port_1 = sheet.GetDataInt("Port 1");
                                break;

                            case 1:
                                proc.Port_2 = sheet.GetDataInt("Port 2");
                                break;

                            case 2:
                                proc.Port_3 = sheet.GetDataInt("Port 3");
                                break;

                            case 3:
                                proc.Port_4 = sheet.GetDataInt("Port 4");
                                break;

                            case 4:
                                proc.Port_5 = sheet.GetDataInt("Port 5");
                                break;

                            case 5:
                                proc.Port_6 = sheet.GetDataInt("Port 6");
                                break;
                            //case 6:
                            //    Procedure.Port_7 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 7"]);
                            //    break;
                            //case 7:
                            //    Procedure.Port_8 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 8"]);
                            //    break;
                            //case 8:
                            //    Procedure.Port_9 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 9"]);
                            //    break;
                        }
                    }

                    break;

                case e_CalibrationType.SUBCLASS:
                case e_CalibrationType.THRU:
                case e_CalibrationType.TRLLINE:
                case e_CalibrationType.TRLTHRU:
                case e_CalibrationType.ISOLATION:
                case e_CalibrationType.UnknownTHRU:
                    proc.Port_1 = sheet.GetDataInt("Port 1");
                    proc.Port_2 = sheet.GetDataInt("Port 2");
                    proc.Port_3 = sheet.GetDataInt("Port 3");
                    proc.Port_4 = sheet.GetDataInt("Port 4");
                    proc.Port_5 = sheet.GetDataInt("Port 5");
                    proc.Port_6 = sheet.GetDataInt("Port 6");
                    //Procedure.Port_7 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 7"]);
                    //Procedure.Port_8 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 8"]);
                    //Procedure.Port_9 = cExtract.Get_Data_Int(CalibrationStr, RowNo, Cal_Header["Port 9"]);
                    break;

                case e_CalibrationType.LOAD:
                case e_CalibrationType.OPEN:
                case e_CalibrationType.SHORT:
                case e_CalibrationType.TRLREFLECT:
                case e_CalibrationType.NS_POWER:
                case e_CalibrationType.ECAL_OPEN: // for 2pot Ecal for NF
                case e_CalibrationType.ECAL_SHORT:
                case e_CalibrationType.ECAL_LOAD:
                case e_CalibrationType.ECAL_SAVE_A:
                case e_CalibrationType.ECAL_SAVE_B:
                    proc.Port_1 = sheet.GetDataInt("Port 1");
                    break;

                default:
                    break;
            }

            return proc;
        }

        public void Run_CalibrationProcedure_Auto(CalibrationInputDataObject calDo)
        {
            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }
            //ENR_file = "ENR_NC346D_22dB_AF963.enr";
            //FBAR.Calibration_Class.Calibrate_TOPAZ(ENR_file);
            // Case HLS2.
            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346D_22dB_AF963.enr"; // ENR file location
            
            //m_model.Calibrate_TOPAZ(10, ENRfile);
            // Case Joker.
            m_model.Auto_CalibrateAll(calDo, DataObject.ENR_file, DataObject.TopazCalPower, null, isDivaInstrument); //For DIVA 
        }
        public void Run_CalibrationProcedure_SPARA(CalibrationInputDataObject calDo)
        {
            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }
            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346D_22dB_AE329.enr"; // ENR file location

            m_model.Calibrate_SPARA(calDo, DataObject.TopazCalPower, ENRfile);
        }

        public void Run_CalibrationProcedure_NF(CalibrationInputDataObject calDo)
        {
            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }
            string ENRfile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346D_22dB_AE329.enr"; // ENR file location
            //string ENRfile = "C:/Users/Public/Documents/Network Analyzer/ENR_NC346D_22dB_AE329.enr"; // ENR file location

            m_model.Calibrate_NF(calDo, DataObject.TopazCalPower, ENRfile);
        }

        public void Run_CalibrationProcedure_SpraramPartial(CalibrationInputDataObject calDo, int[] DoCalCh = null)
        {

            if (RaiseError_Calibration)
            {
                DialogResult chkStatus = MessageBox.Show("Error in Calibration Procedure\r\nDo you want to continue?", "Cautious: Calibration Procedure Error!!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (chkStatus == DialogResult.No)
                {
                    return;
                }
            }
            //FBAR.Calibration_Class.Calibrate_SPARA_PARTIAL();
            //ENR_file = "ENR_NC346D_22dB_AF963.enr";
            //m_model.Auto_CalibrateAll(ENR_file, TopazCalPower, DoCalCh);
            m_model.Auto_CalibrateAll(calDo, DataObject.ENR_file, DataObject.TopazCalPower, DoCalCh, isDivaInstrument); //For DIVA 

        }

        public void VerificationAll(bool isUsing28Ohm)
        {
            string SubCalResultPath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
            string SubCalResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_VerificationAll.txt";

            DirectoryInfo di = new DirectoryInfo(SubCalResultPath);
            if (di.Exists == false) di.Create();

            StreamWriter SubCalResult = new StreamWriter(SubCalResultName, true);
            //FBAR.Calibration_Class.VerificationAll();
            m_model.Auto_VerificationAll(SubCalResult, isUsing28Ohm);
        }

        public void VerificationOnly(bool isUsing28Ohm, int PortPower)
        {
            string SubCalResultPath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
            string SubCalResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_VerificationAll.txt";

            DirectoryInfo di = new DirectoryInfo(SubCalResultPath);
            if (di.Exists == false) di.Create();

            StreamWriter SubCalResult = new StreamWriter(SubCalResultName, true);
            //FBAR.Calibration_Class.VerificationAll();
            m_model.Auto_VerificationOnly(SubCalResult, isUsing28Ohm, PortPower);
        }

        public void Verification_Spara()
        {
            string sparaResultPath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
            string sparaResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_SparaVerification.txt";
            string sparaAllResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_SparaVerification_Detail.txt";

            DirectoryInfo di = new DirectoryInfo(sparaResultPath);
            if (di.Exists == false) di.Create();

            StreamWriter sparaResult = new StreamWriter(sparaResultName, true);
            StreamWriter sparaResultAll = new StreamWriter(sparaAllResultName, true);
            m_model.SparaVerification(sparaResult, sparaResultAll);
            //FBAR.Calibration_Class.NFVerification();
        }

        public void Verification_NF()
        {
            string NFResultPath = @"C:\Avago.ATF.Common.x64\Input\Verification\";
            string NFResultName = @"C:\Avago.ATF.Common.x64\Input\Verification\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_NFVerification.txt";

            DirectoryInfo di = new DirectoryInfo(NFResultPath);
            if (di.Exists == false) di.Create();

            StreamWriter NFResult = new StreamWriter(NFResultName, true);
            m_model.NFVerification(NFResult);
        }

        public void SetTrigger()
        {
            m_model.SetTrigger();
        }

        public void Verify_ECAL_SCPI(string Channel_Num)
        {
            m_model.Verify_ECAL_procedure(Channel_Num);
        }

        public void Save_StateFile()
        {
            if (string.IsNullOrEmpty(DataObject.NA_StateFile)) return;
            m_model.Save_StateFile_TotalChannel(DataObject.NA_StateFile); //ChoonChin - 20191205 - No hard code on total channel
        }
    }
}