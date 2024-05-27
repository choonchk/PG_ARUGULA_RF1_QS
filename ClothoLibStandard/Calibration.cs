using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Ivi.Visa.Interop;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;

namespace ClothoLibStandard
{
    public class Calibration : Lib_Var
    {
        bool
            varCalDataAvailableSource,
            varCalDataAvailableMeas;
        int
            varIdSg01 = 0,
            varIdSa01 = 0,
            varIdPm01 = 0,
            varIdPm02 = 0,
            varIdPm03 = 0;
        string
            strSourceEquip = "",
            strMeasEquip = "",
            tempStr = "",
            tempFreq = "",
            tempResult = "";

        float[] arrSaTraceData = null;

        FileInfo
            fCalDataFile = null,
            fCalEquipSource = null,
            fCalFreqList = null;
        StreamWriter
            swCalDataFile;
        StreamReader
            srCalEquipSource,
            srCalEquipMeas,
            srCalFreqList;

        public Calibration() { }
        ~Calibration() { }

        public void DeviceCalFixedPower(string strSourceEquipModel, int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string strTargetCalDataFile, string strSourceEquipCalFactor, string strMeasEquipCalFactor, string strCalFreqList)
        {
            string
                varTestResult = "";

            int
                varNumOfCalFreqList = 0,
                varSgInitPowLev = -10;

            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;

            string[,]
                arrCalDataSource = new string[300, 2],
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300];

            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);

            Assign_Cal_File(".\\" + strTargetCalDataFile);

            Load_Cal_Data_for_Source_Equip(strSourceEquipCalFactor, ref arrCalDataSource, ref varCalDataAvailableSource, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);
            Load_Cal_Data_Freq_List(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);


            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {

                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);
                ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);
                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }

            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            //string strPsNumber = strMeasEquip.Substring(7);

            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {

                        ºmyLibSa.INITIALIZATION(varIdSa01);
                        ºmyLibSa.SPAN(1);                                    // Spam to 1 MHz
                        ºmyLibSa.AUTO_COUPLE(N9020A_AUTO_COUPLE.ALL);        // Auto Coupling
                        ºmyLibSa.AMPLITUDE_REF_LEVEL(varExpectedGain + 10);  // Reference level    
                    }
                    else
                        throw new Exception("There is no available SA model.");
                    break;

                case "PM01PS01":
                    if (varIdPm01 == 1) // E4419B
                    {

                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // E4419B
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else
                        throw new Exception("There is no available PM01PS01 model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else
                        throw new Exception("There is no available PM02PS01 model.");
                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        ºmyLibSg.SET_OUTPUT_POWER(varSgInitPowLev);
                    else
                        throw new Exception("There is no available SG01 model.");
                    break;
            }


            for (int i = 0; i < varNumOfCalFreqList; i++)
            {
                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                        {
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        }

                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 5)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        break;
                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {

                    string varSourceEquipPowOffset = null;

                    if (arrCalFreqList[i] == arrCalDataSource[i, 0])
                        varSourceEquipPowOffset = arrCalDataSource[i, 1];
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex, 0] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex, 0] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex, 1];
                                break;
                            }
                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 - 10);
                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(400);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        break;
                }

                // SA Init
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3) // MXA
                            ºmyLibSa.OPERATION_COMPLETE();
                        break;


                }

                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3) // MXA
                        {
                            // Placing a marker
                            ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            ºmyLibSa.OPERATION_COMPLETE();

                            // varTestResult
                            varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 5) // P-Series
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        break;
                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }

                        break;
                }

                varTestResult = (Convert.ToSingle(varTestResult) + varSgInitPowLev * -1).ToString();
                // Writing cal data file
                swCalDataFile.Write(arrCalFreqList[i] + "," + varTestResult + "\n");
            }

            swCalDataFile.Close();

            // Turn off SG01

            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }

        }

        public int DeviceCalFixedPowerWithModulationSource(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName,
            string strTargetCalDataFile, string strSourceEquipCalFactor, string strMeasEquipCalFactor, string strCalFreqList)
        {
            try
            {
                string varTestResult = "";

                float varSgInitPowLev = -10;
                int varNumOfCalFreqList = 0;

                string[,] arrCalDataSource = new string[300, 2];
                string[,] arrCalDataMeas = new string[300, 2];

                string[] arrCalFreqList = new string[300];

                Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
                Assign_Cal_File(".\\" + strTargetCalDataFile);
                Load_Cal_Data_for_Source_Equip(strSourceEquipCalFactor, ref arrCalDataSource, ref varCalDataAvailableSource, ref varNumOfCalFreqList);
                Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);
                Load_Cal_Data_Freq_List(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);

                // Resetting Source Equipment

                if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                {
                    ºmyLibSg.RESET();

                    if (ModulationFormat != "CW")
                        ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);
                    if (ModulationFormat == "CW")
                        ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                    ºmyLibSg.SET_OUTPUT_POWER(-100);
                    ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);

                }

                string strPsNumber = strMeasEquip.Substring(7);
                // Resetting Meas equipment
                switch (strMeasEquip)
                {
                    case "SA01":
                        string RBW;
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        if (ModulationFormat == "CW") RBW = "1";
                        else if (ModulationFormat == "CDMA") RBW = "1.3";
                        else if (ModulationFormat == "WCDMA") RBW = "5";
                        else throw new Exception("Calibration: DeviceCalFixedPowerWithModulationSource --> No modulation type is specified.");

                        ºmyLibSa.WRITE(
                            ":FREQ:SPAN " + "0" + "MHz;" +
                            ":BWID:RES " + RBW + "MHz;" +
                            ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                        break;

                    case "PM01PS01":

                        ºmyLibPM.RESET();
                        if (varIdPm01 == 1) // E4419B
                        {
                            ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                        }
                        else if (varIdPm01 == 4) // E4419B
                        {
                            ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        }
                        else if (varIdPm01 == 5) // U2000A, U2001A
                        {
                            ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        }
                        else
                            throw new Exception("There is no available PM model.");

                        break;

                    case "PM02PS01":
                        ºmyLibPM.RESET();

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                            ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                        }

                        break;
                }

                // Setting the initial power            
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        {
                            ºmyLibSg.SET_OUTPUT_POWER(varSgInitPowLev);
                        }

                        break;
                }


                for (int i = 0; i < varNumOfCalFreqList; i++)
                {
                    // Setting Source Equip Freq
                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                            break;
                    }

                    // Setting Meas Equip Freq
                    switch (strMeasEquip)
                    {
                        case "SA01":
                            if (varIdSa01 == 1 || varIdSa01 == 3)
                                ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));

                            break;

                        case "PM01PS01":

                            if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 5)) // E4419B
                                ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                            break;

                        case "PM02PS01":

                            if (varIdPm02 == 1) // E4419B
                                ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                            break;
                    }

                    // Setting the source power level
                    if (varCalDataAvailableSource)
                    {

                        string varSourceEquipPowOffset = null;

                        if (arrCalFreqList[i] == arrCalDataSource[i, 0])
                            varSourceEquipPowOffset = arrCalDataSource[i, 1];
                        else
                        {
                            int varCurrentIndex = 0;
                            while (arrCalDataSource[varCurrentIndex, 0] != null)
                            {
                                if (arrCalDataSource[varCurrentIndex, 0] == arrCalFreqList[i])
                                {
                                    varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex, 1];
                                    break;
                                }

                                varCurrentIndex++;
                            }
                        }

                        switch (strSourceEquip)
                        {
                            case "SG01":

                                if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                    ºmyLibSg.SET_OUTPUT_POWER((Convert.ToSingle(varSourceEquipPowOffset) * -1 - 10));
                                break;
                        }
                    }

                    // Settling time for SG
                    switch (strSourceEquip)
                    {
                        case "SG01":
                            if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                                Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                            else if (varIdSg01 == 7) // MXG
                                Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                            if (strMeasEquip == "PM01PS01")
                                Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                            break;
                    }


                    // Power measurement
                    switch (strMeasEquip)
                    {
                        case "SA01":

                            if (varIdSa01 == 1) // PSA
                            {
                                Thread.Sleep(100);
                                float MeasuredPout = 0;
                                ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                                varTestResult = MeasuredPout.ToString();
                            }
                            else if (varIdSa01 == 3) // MXA
                            {
                                ºmyLibSa.OPERATION_COMPLETE();
                                // Placing a marker
                                ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                                ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");


                                // Setting frequency

                                ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                                ºmyLibSa.OPERATION_COMPLETE();

                                // varTestResult
                                varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                            }

                            break;

                        case "PM01PS01":

                            if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 5) // P-Series
                            {
                                ºmyLibPM.OPERATION_COMPLETE();
                                varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                                break;
                            }

                            break;

                        case "PM02PS01":

                            if (varIdPm02 == 1) // E4419B
                            {
                                ºmyLibPM.OPERATION_COMPLETE();
                                varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                                break;
                            }

                            break;
                    }

                    varTestResult = (Convert.ToSingle(varTestResult) + varSgInitPowLev * -1).ToString();
                    // Writing cal data file
                    swCalDataFile.Write(arrCalFreqList[i] + "," + varTestResult + "\n");
                }

                swCalDataFile.Close();

                // Turn off SG01

                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                        {
                            ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                            ºmyLibSg.SET_OUTPUT_POWER(-100);
                        }

                        break;
                }

                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }


        }

        public int DeviceCalFixedPowerWithModulationSource_DualBand(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName, string strTargetCalDataFile, string strSourceEquipCalFactor,
            string strMeasEquipCalFactor, string strCalFreqList, float CalLimitLow, float CalLimitHigh, string SwitchBand)
        {
            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;
            int
                varNumOfCalFreqList = 0;
            float
                CalResultMin = 0,
                CalResultMax = -999,
                varSgInitPowLev = 8;
            string
                varTestResult = "";
            string[,]
                arrCalDataSource = new string[300, 2],
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300];


            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
            Assign_Cal_File(strTargetCalDataFile);

            Load_Cal_Data_for_Source_Equip(strSourceEquipCalFactor, ref arrCalDataSource, ref varCalDataAvailableSource, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);
            Load_Cal_Data_Freq_List(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);

            // Resetting Source Equipment

            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {
                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);

                if (ModulationFormat != "CW")
                    ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);

                if (ModulationFormat == "CW")
                    ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }


            string RBW;
            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        // Measurement setting
                        if (ModulationFormat == "GSM")
                        {
                            ºmyLibSa.SELECT_INSTRUMENT(N9020A_INSTRUMENT_MODE.EDGE_GSM);
                            ºmyLibSa.MEASURE_SETUP(N9020A_MEAS_TYPE.BTxPow);
                            ºmyLibSa.SELECT_TRIGGERING(N9020A_TRIGGERING_TYPE.TXP_Ext1);
                            ºmyLibSa.CONTINUOUS_MEASUREMENT_ON();
                            ºmyLibSa.WRITE("TXP:AVER:COUN 2");
                            ºmyLibSa.AMPLITUDE_INPUT_ATTENUATION(10);
                            ºmyLibSa.WRITE("DISP:TXP:VIEW:WIND:TRAC:Y:RLEV 0dbm");

                        }
                        else if (ModulationFormat == "CW") ;
                        else if (ModulationFormat == "CDMA") ;
                        else if (ModulationFormat == "WCDMA")
                        {
                            ºmyLibSa.SELECT_INSTRUMENT(N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                            RBW = "8";
                            ºmyLibSa.WRITE(":FREQ:SPAN " + "0" + "MHz;" +
                                ":BWID:RES " + RBW + "MHz;" + ":BAND:VID 50MHz;" +
                                ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                            ºmyLibSa.SWEEP_TIMES(1);
                        }

                        else
                        {
                            throw new Exception("No modulation type is specified.");
                        }

                    }

                    break;

                case "PM01PS01":


                    if (varIdPm01 == 1 || varIdPm01 == 6) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // N1912A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT:CONT 0");
                        ºmyLibPM.WRITE("SENS:MRATE DOUB");
                        ºmyLibPM.WRITE("SENS:AVER:COUN 2");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER 0");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 1");
                    }
                    else if (varIdPm01 == 7) // U2001AH_2, U2001AH16
                    {
                        if (ModulationFormat == "GSM")
                        {
                            ºmyLibUSB2_PS.PRESET();
                            Thread.Sleep(30);
                            ºmyLibUSB2_PS.WRITE("*CLS");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("TRIG:SOUR EXT");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:SWE:TIME " + 0.0009);
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:SWE:OFFS:TIME " + 0.0002);
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("CALC:FEED " + "'" + "POW:AVER ON SWEEP" + "'");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("INIT:CONT ON");
                            Thread.Sleep(5);
                            //para.myLibUSBPS2.WRITE("SENS:AVER:COUNT 1");
                            //Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:MRATE FAST");
                        }
                        else
                        {
                            ºmyLibUSB2_PS.PRESET();

                            ºmyLibUSB2_PS.WRITE("INIT:CONT 0");
                            ºmyLibUSB2_PS.WRITE("SENS:MRATE FAST");
                        }

                    }
                    else
                        throw new Exception("There is no available PM model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm02 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM02.RESET();

                        ºmyLibPM02.WRITE("INIT:CONT 0");
                        ºmyLibPM02.WRITE("SENS:MRATE DOUB");
                        ºmyLibPM02.WRITE("SENS:AVER:COUN 2");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER 0");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 1");
                    }
                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        ºmyLibSg.SET_OUTPUT_POWER((float)varSgInitPowLev);

                    break;
            }

            for (int i = 0; i < varNumOfCalFreqList; i++)
            {

                if (ModulationFormat == "GSM")
                {
                    if (SwitchBand.ToUpper() == "HIGH")
                        ºmyLibSW.WRITE(ºSwGSM_Hb);
                    else
                        ºmyLibSW.WRITE(ºSwGSM_Lb);
                }
                else
                {
                    if (SwitchBand.ToUpper() == "HIGH")
                        ºmyLibSW.WRITE(ºSwUMTS_Hb);
                    else
                        ºmyLibSW.WRITE(ºSwUMTS_Lb);

                }


                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 6)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 7) // U2001AH16
                            ºmyLibUSB2_PS.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 5) // U2001AH

                            ºmyLibPM.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {

                    string varSourceEquipPowOffset = null;

                    if (arrCalFreqList[i] == arrCalDataSource[i, 0])
                        varSourceEquipPowOffset = arrCalDataSource[i, 1];
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex, 0] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex, 0] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex, 1];
                                break;
                            }

                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 + 8);
                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                        break;
                }


                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":

                        if (varIdSa01 == 1) // PSA
                        {
                            float MeasuredPout = 0;
                            ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                            varTestResult = MeasuredPout.ToString();
                        }
                        else if (varIdSa01 == 3) // MXA
                        {
                            ºmyLibSa.OPERATION_COMPLETE();


                            // Placing a marker
                            //ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            //ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            //ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            if (ModulationFormat == "GSM")
                            {
                                arrSaTraceData = ºmyLibSa.WRITE_READ_IEEEBlock("FETC:TXP1?", IEEEBinaryType.BinaryType_R4);
                                varTestResult = Convert.ToString(arrSaTraceData[1]);
                            }
                            else
                            {
                                //ºmyLibSa.WRITE(":FORM ASC");
                                varTestResult = Convert.ToString(ºmyLibSa.WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME"));
                                //ºmyLibSa.WRITE(":FORM REAL,32");
                            }
                            ºmyLibSa.OPERATION_COMPLETE();

                            // varTestResult
                            //varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 6) // P-Series
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":READ?");
                            break;
                        }

                        if (varIdPm01 == 7) //U2001AH16 OR U2001A
                        {

                            Thread.Sleep(100);
                            //ºmyLibUSB2_PS.OPERATION_COMPLETE();
                            if (ModulationFormat == "GSM")
                                varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("FETC?");
                            else
                                varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("READ?");
                            break;
                        }
                        if (varIdPm01 == 5) //U2001AH16 OR U2001A
                        {

                            //ºmyLibUSB2_PS.OPERATION_COMPLETE();
                            Thread.Sleep(30);
                            varTestResult = ºmyLibPM.WRITE_READ_STRING("READ?");
                            break;
                        }

                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }

                        break;
                }

                //double vartestresult2 = varTestResult - varSgInitPowLev;
                varTestResult = (Convert.ToDouble(varTestResult) + varSgInitPowLev * -1).ToString();
                // Writing cal data file
                swCalDataFile.Write(arrCalFreqList[i] + "," + varTestResult + "\n");

                if (Convert.ToSingle(varTestResult) < CalResultMin)
                    CalResultMin = Convert.ToSingle(varTestResult);
                if (Convert.ToSingle(varTestResult) > CalResultMax)
                    CalResultMax = Convert.ToSingle(varTestResult);
            }

            swCalDataFile.Close();

            // Turn off SG01

            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }
            if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                return 0;
            else
                throw new Exception("Calibration results are out of limits.");
        }

        public int DeviceCalFixedPowerWithModulationSourceWolfer_DualBand(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName, string strTargetCalDataFile, string strSourceEquipCalFactor,
            string strMeasEquipCalFactor, string strCalFreqList, float CalLimitLow, float CalLimitHigh, string SwitchBand, string _Mode)
        {
            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;
            int
                varNumOfCalFreqList = 0;
            float
                CalResultMin = 0,
                CalResultMax = -999,
                varSgInitPowLev = 12;        //ORI = 8
            string
                varTestResult = "";
            string[,]
                arrCalDataSource = new string[300, 2],
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300];


            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
            Assign_Cal_File(strTargetCalDataFile);

            Load_Cal_Data_for_Source_Equip(strSourceEquipCalFactor, ref arrCalDataSource, ref varCalDataAvailableSource, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);
            Load_Cal_Data_Freq_List(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);

            // Resetting Source Equipment

            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {
                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);

                if (ModulationFormat != "CW")
                    ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);

                if (ModulationFormat == "CW")
                    ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }


            string RBW;
            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        // Measurement setting
                        if (ModulationFormat == "GSM")
                        {
                            ºmyLibSa.SELECT_INSTRUMENT(N9020A_INSTRUMENT_MODE.EDGE_GSM);
                            ºmyLibSa.MEASURE_SETUP(N9020A_MEAS_TYPE.BTxPow);
                            ºmyLibSa.SELECT_TRIGGERING(N9020A_TRIGGERING_TYPE.TXP_Ext1);
                            ºmyLibSa.CONTINUOUS_MEASUREMENT_ON();
                            ºmyLibSa.WRITE("TXP:AVER:COUN 2");
                            ºmyLibSa.AMPLITUDE_INPUT_ATTENUATION(10);
                            ºmyLibSa.WRITE("DISP:TXP:VIEW:WIND:TRAC:Y:RLEV 0dbm");

                        }
                        else if (ModulationFormat == "CW") ;
                        else if (ModulationFormat == "CDMA") ;
                        else if (ModulationFormat == "WCDMA")
                        {
                            ºmyLibSa.SELECT_INSTRUMENT(N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                            RBW = "8";
                            ºmyLibSa.WRITE(":FREQ:SPAN " + "0" + "MHz;" +
                                ":BWID:RES " + RBW + "MHz;" + ":BAND:VID 50MHz;" +
                                ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                            ºmyLibSa.SWEEP_TIMES(1);
                        }

                        else
                        {
                            throw new Exception("No modulation type is specified.");
                        }

                    }

                    break;

                case "PM01PS01":


                    if (varIdPm01 == 1 || varIdPm01 == 6) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // N1912A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT:CONT 0");
                        ºmyLibPM.WRITE("SENS:MRATE DOUB");
                        ºmyLibPM.WRITE("SENS:AVER:COUN 2");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER 0");
                        //ºmyLibUSB2_PS.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 1");
                    }
                    else if (varIdPm01 == 7) // U2001AH_2, U2001AH16
                    {
                        if (ModulationFormat == "GSM")
                        {
                            ºmyLibUSB2_PS.PRESET();
                            Thread.Sleep(30);
                            ºmyLibUSB2_PS.WRITE("*CLS");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("TRIG:SOUR EXT");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:SWE:TIME " + 0.0009);
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:SWE:OFFS:TIME " + 0.0002);
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("CALC:FEED " + "'" + "POW:AVER ON SWEEP" + "'");
                            Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("INIT:CONT ON");
                            Thread.Sleep(5);
                            //para.myLibUSBPS2.WRITE("SENS:AVER:COUNT 1");
                            //Thread.Sleep(5);
                            ºmyLibUSB2_PS.WRITE("SENS:MRATE FAST");
                        }
                        else
                        {
                            ºmyLibUSB2_PS.PRESET();

                            ºmyLibUSB2_PS.WRITE("INIT:CONT 0");
                            ºmyLibUSB2_PS.WRITE("SENS:MRATE FAST");
                        }

                    }
                    else if (varIdPm01 == 8) // R&S NRPZ11
                    {
                        if (ModulationFormat != "CW")
                            RS_BurstSetting();

                        if (ModulationFormat == "CW")
                            RS_NonBurstSetting();
                    }
                    else
                        throw new Exception("There is no available PM model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        ºmyLibSg.SET_OUTPUT_POWER((float)varSgInitPowLev);

                    break;
            }

            for (int i = 0; i < varNumOfCalFreqList; i++)
            {

                if (_Mode == "GSM")
                {
                    if (SwitchBand.ToUpper() == "HIGH")
                    {
                        ºmyLibSW.WRITE(ºSwGSM_Hb);
                        //ºmyLibAM330.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwGSM_Hb));
                    }
                    else
                    {
                        ºmyLibSW.WRITE(ºSwGSM_Lb);
                        //ºmyLibAM330.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwGSM_Lb));
                    }
                }
                else
                {
                    if (SwitchBand.ToUpper() == "HIGH")
                    {
                        ºmyLibSW.WRITE(ºSwUMTS_Hb);
                        //ºmyLibAM330.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwUMTS_Hb));
                    }
                    else
                    {
                        ºmyLibSW.WRITE(ºSwUMTS_Lb);
                        //ºmyLibAM330.AMB1340C_DRIVEPORT(Convert.ToInt32(ºSwUMTS_Lb));
                    }
                }

                Thread.Sleep(100);


                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                        {
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                            if (ModulationFormat == "CW")
                            {
                                ºmyLibSa.SPAN(0);
                            }
                        }
                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 6)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 7) // U2001AH16
                            ºmyLibUSB2_PS.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 5) // U2001AH
                            ºmyLibPM.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        else if (varIdPm01 == 8)
                        {
                            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Convert.ToDouble(arrCalFreqList[i]) * 1000000.0); // Set corr frequency
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {

                    string varSourceEquipPowOffset = null;

                    if (arrCalFreqList[i] == arrCalDataSource[i, 0])
                        varSourceEquipPowOffset = arrCalDataSource[i, 1];
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex, 0] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex, 0] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex, 1];
                                break;
                            }

                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 + 12); //ORI 8
                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7 || varIdSg01 == 8) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                        break;
                }


                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":

                        if (varIdSa01 == 1) // PSA
                        {
                            float MeasuredPout = 0;
                            ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                            varTestResult = MeasuredPout.ToString();
                        }
                        else if (varIdSa01 == 3) // MXA
                        {
                            ºmyLibSa.OPERATION_COMPLETE();


                            // Placing a marker
                            //ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            //ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            //ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            if (ModulationFormat == "GSM")
                            {
                                arrSaTraceData = ºmyLibSa.WRITE_READ_IEEEBlock("FETC:TXP1?", IEEEBinaryType.BinaryType_R4);
                                varTestResult = Convert.ToString(arrSaTraceData[1]);
                            }
                            else
                            {
                                //ºmyLibSa.WRITE(":FORM ASC");
                                varTestResult = Convert.ToString(ºmyLibSa.WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME"));
                                //ºmyLibSa.WRITE(":FORM REAL,32");
                            }
                            ºmyLibSa.OPERATION_COMPLETE();

                            // varTestResult
                            //varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 6) // P-Series
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":READ?");
                            break;
                        }
                        if (varIdPm01 == 8)
                        {
                            Thread.Sleep(100);
                            double meas_value = -999;
                            if (ModulationFormat != "CW")
                                meas_value = RSPM_Meas(ºmyLibRS_PS);

                            else if (ModulationFormat == "CW")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);
                            varTestResult = meas_value.ToString();

                            break;
                        }
                        if (varIdPm01 == 7) //U2001AH16 OR U2001A
                        {

                            Thread.Sleep(100);
                            //ºmyLibUSB2_PS.OPERATION_COMPLETE();
                            if (ModulationFormat == "GSM")
                                varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("FETC?");
                            else
                                varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("READ?");
                            break;
                        }
                        if (varIdPm01 == 5) //U2001AH16 OR U2001A
                        {

                            //ºmyLibUSB2_PS.OPERATION_COMPLETE();
                            Thread.Sleep(30);
                            varTestResult = ºmyLibPM.WRITE_READ_STRING("READ?");
                            break;
                        }

                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }

                        break;
                }

                //double vartestresult2 = varTestResult - varSgInitPowLev;
                varTestResult = (Convert.ToDouble(varTestResult) + varSgInitPowLev * -1).ToString();
                // Writing cal data file
                swCalDataFile.Write(arrCalFreqList[i] + "," + varTestResult + "\n");

                if (Convert.ToSingle(varTestResult) < CalResultMin)
                    CalResultMin = Convert.ToSingle(varTestResult);
                if (Convert.ToSingle(varTestResult) > CalResultMax)
                    CalResultMax = Convert.ToSingle(varTestResult);
            }

            swCalDataFile.Close();

            // Turn off SG01

            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7 || varIdSg01 == 8) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }
            if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                return 0;
            else
                throw new Exception("Calibration results are out of limits.");
        }

        //LEX/FEM
        public int DeviceCalFixedPowerWithModulationSource02(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName, string strTargetCalDataFile, string strSourceEquipCalFactor,
            string strMeasEquipCalFactor, string strCalFreqList, float CalLimitLow, float CalLimitHigh)
        {
            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;
            int
                varNumOfCalFreqList = 0;
            float
                CalResultMin = 0,
                CalResultMax = -999,
                varSgInitPowLev = 0;
            string
                varTestResult = "";
            string[,]
                arrCalDataSource = new string[300, 2],
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300];

            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
            Assign_Cal_File(strTargetCalDataFile);

            Load_Cal_Data_for_Source_Equip(strSourceEquipCalFactor, ref arrCalDataSource, ref varCalDataAvailableSource, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);
            Load_Cal_Data_Freq_List(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);

            // Resetting Source Equipment

            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {
                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);

                if (ModulationFormat != "CW")
                    ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);

                if (ModulationFormat == "CW")
                    ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }


            string RBW;
            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        // Measurement setting
                        if (ModulationFormat == "CW") RBW = "5";
                        else if (ModulationFormat == "CDMA") RBW = "1.3";
                        else if (ModulationFormat == "WCDMA") RBW = "5";
                        else
                        {
                            throw new Exception("No modulation type is specified.");
                        }
                        ºmyLibSa.WRITE(":FREQ:SPAN " + "0" + "MHz;" +
                            ":BWID:RES " + RBW + "MHz;" +
                            ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                    }

                    break;

                case "PM01PS01":


                    if (varIdPm01 == 1 || varIdPm01 == 6) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 1");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // N1912A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 7) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT:CONT ON");
                        ºmyLibPM.WRITE("SENS:AVER:COUN 2");
                    }

                    else if (varIdPm01 == 8) // R&S NRPZ11
                    {
                        if (ModulationFormat == "GSM")
                            RS_BurstSetting();

                        else if (ModulationFormat == "CW")
                            RS_NonBurstSetting();
                        else if (ModulationFormat == "WCDMA")
                            RS_NonBurstSetting();
                    }

                    else
                        throw new Exception("There is no available PM model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm02 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM02.RESET();
                        ºmyLibPM02.WRITE("INIT:CONT 0");
                        ºmyLibPM02.WRITE("SENS:AVER:COUN 2");
                    }

                    break;

                case "PM03PS01":

                    if (varIdPm03 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm03 == 5) // U2000A3, U2001A3
                    {
                        ºmyLibPM03.RESET();
                        ºmyLibPM03.WRITE("INIT:CONT 0");
                        ºmyLibPM03.WRITE("SENS:AVER:COUN 2");
                    }

                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        ºmyLibSg.SET_OUTPUT_POWER((float)varSgInitPowLev);

                    break;
            }


            for (int i = 0; i < varNumOfCalFreqList; i++)
            {
                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 5) || (varIdPm01 == 6)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 7) // U2001AH16
                            ºmyLibPM.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 8)
                        {
                            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Convert.ToDouble(arrCalFreqList[i]) * 1000000.0); // Set corr frequency
                        }


                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm02 == 1) || (varIdPm02 == 4) || (varIdPm02 == 5) || (varIdPm02 == 6)) // E4419B
                            ºmyLibPM02.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm03 == 1) || (varIdPm03 == 4) || (varIdPm03 == 5) || (varIdPm03 == 6)) // E4419B
                            ºmyLibPM03.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {

                    string varSourceEquipPowOffset = null;

                    if (arrCalFreqList[i] == arrCalDataSource[i, 0])
                        varSourceEquipPowOffset = arrCalDataSource[i, 1];
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex, 0] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex, 0] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex, 1];
                                break;
                            }

                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 + 0);
                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                        break;
                }


                Thread.Sleep(100);

                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":

                        if (varIdSa01 == 1) // PSA
                        {
                            float MeasuredPout = 0;
                            Thread.Sleep(100);
                            ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                            varTestResult = MeasuredPout.ToString();
                        }
                        else if (varIdSa01 == 3) // MXA
                        {
                            //ºmyLibSa.OPERATION_COMPLETE();


                            // Placing a marker
                            ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            //ºmyLibSa.OPERATION_COMPLETE();
                            Thread.Sleep(300);
                            // varTestResult
                            varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 5 || varIdPm01 == 6) // P-Series
                        {
                            ºmyLibPM.WRITE("INIT1:CONT 0");
                            //ºmyLibPM.WRITE("*WAI");
                            Thread.Sleep(1000);
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":READ?");
                            break;
                        }

                        if (varIdPm01 == 7) //U2001AH16
                        {

                            //ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("FETC?");
                            break;
                        }

                        if (varIdPm01 == 8)
                        {
                            Thread.Sleep(100);
                            double meas_value = -999;
                            if (ModulationFormat == "GSM")
                                meas_value = RSPM_Meas(ºmyLibRS_PS);

                            else if (ModulationFormat == "CW")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);

                            else if (ModulationFormat == "WCDMA")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);

                            varTestResult = meas_value.ToString();

                            break;
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm02 == 1 || varIdPm02 == 4 || varIdPm02 == 5 || varIdPm02 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM02.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;
                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm03 == 1 || varIdPm03 == 4 || varIdPm03 == 5 || varIdPm03 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM03.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;

                }

                varTestResult = (Convert.ToSingle(varTestResult) + varSgInitPowLev * -1).ToString();
                // Writing cal data file
                swCalDataFile.Write(arrCalFreqList[i] + "," + varTestResult + "\n");

                if (Convert.ToSingle(varTestResult) < CalResultMin)
                    CalResultMin = Convert.ToSingle(varTestResult);
                if (Convert.ToSingle(varTestResult) > CalResultMax)
                    CalResultMax = Convert.ToSingle(varTestResult);
            }

            swCalDataFile.Close();

            // Turn off SG01

            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }
            if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                return 0;
            else
                throw new Exception("Calibration results are out of limits.");
        }

        //KCC - For single cal file
        public int DeviceCalFixedPowerWithModulationSource02SingleCalFile(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName, string strTargetCalDataFile, string strCalFreqList,
            string strSourceEquipCalFactor, string strTargetCalSegmentName, string strMeasEquipCalFactor,
            float CalLimitLow, float CalLimitHigh)
        {
            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;
            int
                varNumOfCalFreqList = 0;
            float
                CalResultMin = 0,
                CalResultMax = -999,
                varSgInitPowLev = 0;
            string
                varTestResult = "";
            string[,]
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300],
                arrCalDataSource = new string[300];


            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
            Assign_Cal_File_Combined(strTargetCalDataFile);
            Load_Cal_Data_Freq_List_Combined(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Source_Equip_Combined(strTargetCalDataFile, strSourceEquipCalFactor, arrCalFreqList, ref arrCalDataSource, ref varCalDataAvailableSource);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);

            // Resetting Source Equipment
            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {
                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);

                if (ModulationFormat != "CW")
                    ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);

                if (ModulationFormat == "CW")
                    ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }

            string RBW;
            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        // Measurement setting
                        if (ModulationFormat == "CW") RBW = "5";
                        else if (ModulationFormat == "CDMA") RBW = "1.3";
                        else if (ModulationFormat == "WCDMA") RBW = "5";
                        else
                        {
                            throw new Exception("No modulation type is specified.");
                        }
                        ºmyLibSa.WRITE(":FREQ:SPAN " + "0" + "MHz;" +
                            ":BWID:RES " + RBW + "MHz;" +
                            ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                    }

                    break;

                case "PM01PS01":

                    if (varIdPm01 == 1 || varIdPm01 == 6) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 1");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // N1912A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 7) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT:CONT ON");
                        ºmyLibPM.WRITE("SENS:AVER:COUN 2");
                    }

                    else if (varIdPm01 == 8) // R&S NRPZ11
                    {
                        if (ModulationFormat == "GSM")
                            RS_BurstSetting();

                        else if (ModulationFormat == "CW")
                            RS_NonBurstSetting();
                        else if (ModulationFormat == "WCDMA")
                            RS_NonBurstSetting();
                    }

                    else
                        throw new Exception("There is no available PM model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm02 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM02.RESET();
                        ºmyLibPM02.WRITE("INIT:CONT 0");
                        ºmyLibPM02.WRITE("SENS:AVER:COUN 2");
                    }

                    break;

                case "PM03PS01":

                    if (varIdPm03 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm03 == 5) // U2000A3, U2001A3
                    {
                        ºmyLibPM03.RESET();
                        ºmyLibPM03.WRITE("INIT:CONT 0");
                        ºmyLibPM03.WRITE("SENS:AVER:COUN 2");
                    }

                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        
                        ºmyLibSg.SET_OUTPUT_POWER((float)varSgInitPowLev - varExpectedGain );
                    break;
            }

            for (int i = 0; i < varNumOfCalFreqList; i++)
            {
                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));
                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 5) || (varIdPm01 == 6)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 7) // U2001AH16
                            ºmyLibPM.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 8)
                        {
                            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Convert.ToDouble(arrCalFreqList[i]) * 1000000.0); // Set corr frequency
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm02 == 1) || (varIdPm02 == 4) || (varIdPm02 == 5) || (varIdPm02 == 6)) // E4419B
                            ºmyLibPM02.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm03 == 1) || (varIdPm03 == 4) || (varIdPm03 == 5) || (varIdPm03 == 6)) // E4419B
                            ºmyLibPM03.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {
                    string varSourceEquipPowOffset = null;

                    if (arrCalDataSource[i] != null)
                        varSourceEquipPowOffset = arrCalDataSource[i].ToString();
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex].ToString();
                                break;
                            }

                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 + 0);
                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                        break;
                }

                Thread.Sleep(100);

                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":

                        if (varIdSa01 == 1) // PSA
                        {
                            float MeasuredPout = 0;
                            Thread.Sleep(100);
                            ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                            varTestResult = MeasuredPout.ToString();
                        }
                        else if (varIdSa01 == 3) // MXA
                        {
                            //ºmyLibSa.OPERATION_COMPLETE();


                            // Placing a marker
                            ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            //ºmyLibSa.OPERATION_COMPLETE();
                            Thread.Sleep(300);
                            // varTestResult
                            int ReadPort = 0;
                            //ºmyLibSW_AemWLF.AMB1340C_READPORT(out ReadPort);
                            varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                            if (i == 80 || i == 0)
                            {
                                Thread.Sleep(5);
                            }
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 5 || varIdPm01 == 6) // P-Series
                        {
                            ºmyLibPM.WRITE("INIT1:CONT 0");
                            //ºmyLibPM.WRITE("*WAI");
                            Thread.Sleep(1000);
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":READ?");
                            break;
                        }

                        if (varIdPm01 == 7) //U2001AH16
                        {

                            //ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("FETC?");
                            break;
                        }

                        if (varIdPm01 == 8)
                        {
                            Thread.Sleep(100);
                            double meas_value = -999;
                            if (ModulationFormat == "GSM")
                                meas_value = RSPM_Meas(ºmyLibRS_PS);

                            else if (ModulationFormat == "CW")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);

                            else if (ModulationFormat == "WCDMA")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);
                            
                            varTestResult = meas_value.ToString();
                            //int ReadPort = 0;
                            //ºmyLibSW_AemWLF.AMB1340C_READPORT(out ReadPort);
                            if (i == 80 || i == 0)
                            {
                                Thread.Sleep(5);
                            }
                            break;
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm02 == 1 || varIdPm02 == 4 || varIdPm02 == 5 || varIdPm02 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM02.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;
                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm03 == 1 || varIdPm03 == 4 || varIdPm03 == 5 || varIdPm03 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM03.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;

                }

                //Str construction
                varTestResult = (Convert.ToSingle(varTestResult) + varSgInitPowLev + varExpectedGain).ToString();
                tempFreq = tempFreq + arrCalFreqList[i] + ",";
                tempResult = tempResult + varTestResult + ",";

                if (Convert.ToSingle(varTestResult) < CalResultMin)
                    CalResultMin = Convert.ToSingle(varTestResult);
                if (Convert.ToSingle(varTestResult) > CalResultMax)
                    CalResultMax = Convert.ToSingle(varTestResult);
            }

            //Write Cal
            int tempInt = tempFreq.Length;
            tempFreq = tempFreq.Remove(tempInt - 1);
            tempInt = tempResult.Length;
            tempResult = tempResult.Remove(tempInt - 1);
            swCalDataFile.WriteLine("");
            swCalDataFile.WriteLine(strTargetCalSegmentName + "," + tempFreq);
            swCalDataFile.WriteLine("," + tempResult);
            tempFreq = "";
            tempResult = "";
            swCalDataFile.Close();

            // Turn off SG01
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }
            if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                return 0;
            else
                throw new Exception("Calibration results are out of limits.");
        }


        //KCC- For PXI
        public int DeviceCalFixedPowerWithModulationSource02SingleCalFile_WithPXI(string strSourceEquipModel,
            int strSourceEquipCh, string strMeasEquipModel, int strMeasEquipCh, float varExpectedGain,
            string ModulationFormat, string SgWaveformName, string strTargetCalDataFile, string strCalFreqList,
            string strSourceEquipCalFactor, string strTargetCalSegmentName, string strMeasEquipCalFactor,
            float CalLimitLow, float CalLimitHigh)
        {
            bool
                varCalDataAvailableSource = false,
                varCalDataAvailableMeas = false;
            int
                varNumOfCalFreqList = 0;
            float
                CalResultMin = 0,
                CalResultMax = -999,
                varSgInitPowLev = 0;
            string
                varTestResult = "";
            string[,]
                arrCalDataMeas = new string[300, 2];

            string[]
                arrCalFreqList = new string[300],
                arrCalDataSource = new string[300];

            //KCC - New for PXI
            Assign_Equip_ID(strSourceEquipModel, strSourceEquipCh, strMeasEquipModel, strMeasEquipCh);
            Assign_Cal_File_Combined(strTargetCalDataFile);
            Load_Cal_Data_Freq_List_Combined(strCalFreqList, ref arrCalFreqList, ref varNumOfCalFreqList);
            Load_Cal_Data_for_Source_Equip_Combined(strTargetCalDataFile, strSourceEquipCalFactor, arrCalFreqList, ref arrCalDataSource, ref varCalDataAvailableSource);
            Load_Cal_Data_for_Meas_Equip(strMeasEquipCalFactor, ref varCalDataAvailableMeas);

            // Resetting Source Equipment
            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
            {
                ºmyLibSg.RESET();
                ºmyLibSg.SET_OUTPUT_POWER(-100);

                if (ModulationFormat != "CW")
                    ºmyLibSg.MOD_FORMAT_WITH_LOADING_CHECK(SgWaveformName, false);

                if (ModulationFormat == "CW")
                    ºmyLibSg.OUTPUT_MODULATION(INSTR_OUTPUT.OFF);

                ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.ON);
            }

            //PXI - Spurce equipment
            if (varIdSg01 == 8) // PXI
            {
                ºmyLibSgPxi.CloseGsmEdge();
                ºmyLibSgPxi.InitializeGsmEdgeCw();
                if (ModulationFormat != "CW")
                {
                    if (ModulationFormat == "EDGE")
                        ºmyLibSgPxi.ModulationChange("GSM", "EDGE");
                }
                else
                {
                    ºmyLibSgPxi.ModulationChange("", ModulationFormat);
                }
                ºmyLibSgPxi.PoweLevel(-100, ModulationFormat, SgWaveformName);
                ºmyLibSgPxi.OutputEnable(true);
            }

            string RBW;
            string strPsNumber = strMeasEquip.Substring(strMeasEquip.Length - 1);
            // Resetting Meas equipment
            switch (strMeasEquip)
            {
                case "SA01":
                    if (varIdSa01 == 1 || varIdSa01 == 3)
                    {
                        ºmyLibSa.INITIALIZATION(varIdSa01);

                        // Measurement setting
                        if (ModulationFormat == "CW") RBW = "5";
                        else if (ModulationFormat == "CDMA") RBW = "1.3";
                        else if (ModulationFormat == "WCDMA") RBW = "5";
                        else
                        {
                            throw new Exception("No modulation type is specified.");
                        }
                        ºmyLibSa.WRITE(":FREQ:SPAN " + "0" + "MHz;" +
                            ":BWID:RES " + RBW + "MHz;" +
                            ":DISP:WIND:TRAC:Y:RLEV " + (varExpectedGain + 10).ToString());
                    }

                    //PXI - Meas equipment
                    else if (varIdSa01 == 4)
                    {
                        if (ModulationFormat == "CW")
                        {
                            ºmyLibSaPxi.InitializeCw();
                        }
                        else
                        {
                            // mySa.InitializeGsm();
                            ºmyLibSaPxi.InitializeGsmEdge();
                        }

                        ºmyLibSaPxi.RefLevel(0 + varExpectedGain + 10);


                        if (ModulationFormat != "CW")
                            ºmyLibSaPxi.IqTriggerLevel(0 + varExpectedGain + 10 - 20);
                    }

                    break;

                case "PM01PS01":

                    if (varIdPm01 == 1 || varIdPm01 == 6) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 1");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }
                    else if (varIdPm01 == 4) // N1912A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":MRAT DOUB");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                    }
                    else if (varIdPm01 == 7) // U2000A, U2001A
                    {
                        ºmyLibPM.RESET();
                        ºmyLibPM.WRITE("INIT:CONT ON");
                        ºmyLibPM.WRITE("SENS:AVER:COUN 2");
                    }

                    else if (varIdPm01 == 8) // R&S NRPZ11
                    {
                        if (ModulationFormat == "GSM")
                            RS_BurstSetting();

                        else if (ModulationFormat == "CW")
                            RS_NonBurstSetting();
                        else if (ModulationFormat == "WCDMA")
                            RS_NonBurstSetting();
                    }

                    else
                        throw new Exception("There is no available PM model.");

                    break;

                case "PM02PS01":

                    if (varIdPm02 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm02 == 5) // U2000A, U2001A
                    {
                        ºmyLibPM02.RESET();
                        ºmyLibPM02.WRITE("INIT:CONT 0");
                        ºmyLibPM02.WRITE("SENS:AVER:COUN 2");
                    }

                    break;

                case "PM03PS01":

                    if (varIdPm03 == 1) // E4419B
                    {
                        ºmyLibPM.RESET();

                        ºmyLibPM.WRITE("INIT" + strPsNumber + ":CONT 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":SPE 40");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":AVER:COUN:AUTO 0");
                        ºmyLibPM.WRITE("SENS" + strPsNumber + ":POW:AC:RANG:AUTO 1");
                    }

                    else if (varIdPm03 == 5) // U2000A3, U2001A3
                    {
                        ºmyLibPM03.RESET();
                        ºmyLibPM03.WRITE("INIT:CONT 0");
                        ºmyLibPM03.WRITE("SENS:AVER:COUN 2");
                    }

                    break;
            }

            // Setting the initial power            
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 7) // ESG, MXG
                        ºmyLibSg.SET_OUTPUT_POWER((float)varSgInitPowLev);

                    else if (varIdSg01 == 8) //PXI 
                        ºmyLibSgPxi.PoweLevel(varSgInitPowLev, ModulationFormat, SgWaveformName);

                    break;
            }

            for (int i = 0; i < varNumOfCalFreqList; i++)
            {
                // Setting Source Equip Freq
                switch (strSourceEquip)
                {
                    case "SG01":

                        if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                            ºmyLibSg.SET_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));

                        else if (varIdSg01 == 8) //PXI
                            ºmyLibSgPxi.Freq(Convert.ToSingle(arrCalFreqList[i]) * 1000000);

                        break;
                }

                // Setting Meas Equip Freq
                switch (strMeasEquip)
                {
                    case "SA01":
                        if (varIdSa01 == 1 || varIdSa01 == 3)
                            ºmyLibSa.CENTER_FREQUENCY(Convert.ToDouble(arrCalFreqList[i]));

                        else if (varIdSa01 == 4) //PXI
                        {
                            if (ModulationFormat == "CW")
                            {
                                ºmyLibSaPxi.ConfigureSpectrumFrequencyCenterSpan(Convert.ToDouble(arrCalFreqList[i]) * 1000000, 1000000);

                            }
                            else
                                ºmyLibSaPxi.Freq(Convert.ToSingle(arrCalFreqList[i]) * 1000000);
                        }

                        break;

                    case "PM01PS01":

                        if ((varIdPm01 == 1) || (varIdPm01 == 4) || (varIdPm01 == 5) || (varIdPm01 == 6)) // E4419B
                            ºmyLibPM.WRITE("SENS1:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 7) // U2001AH16
                            ºmyLibPM.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");

                        else if (varIdPm01 == 8)
                        {
                            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Convert.ToDouble(arrCalFreqList[i]) * 1000000.0); // Set corr frequency
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm02 == 1) || (varIdPm02 == 4) || (varIdPm02 == 5) || (varIdPm02 == 6)) // E4419B
                            ºmyLibPM02.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                            ºmyLibPM.WRITE("SENS2:FREQ " + arrCalFreqList[i] + "MHz");
                        else if ((varIdPm03 == 1) || (varIdPm03 == 4) || (varIdPm03 == 5) || (varIdPm03 == 6)) // E4419B
                            ºmyLibPM03.WRITE("SENS:FREQ " + arrCalFreqList[i] + "MHz");
                        break;

                }

                // Setting the source power level
                if (varCalDataAvailableSource)
                {
                    string varSourceEquipPowOffset = null;

                    if (arrCalDataSource[i] != null)
                        varSourceEquipPowOffset = arrCalDataSource[i].ToString();
                    else
                    {
                        int varCurrentIndex = 0;
                        while (arrCalDataSource[varCurrentIndex] != null)
                        {
                            if (arrCalDataSource[varCurrentIndex] == arrCalFreqList[i])
                            {
                                varSourceEquipPowOffset = arrCalDataSource[varCurrentIndex].ToString();
                                break;
                            }

                            varCurrentIndex++;
                        }
                    }

                    switch (strSourceEquip)
                    {
                        case "SG01":

                            if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                                ºmyLibSg.SET_OUTPUT_POWER(Convert.ToSingle(varSourceEquipPowOffset) * -1 + 0);

                            else if (varIdSg01 == 8)
                            {
                                float newPowerLevel = Convert.ToSingle(varSourceEquipPowOffset) * -1 + 0;
                                ºmyLibSgPxi.PoweLevel(newPowerLevel, ModulationFormat, SgWaveformName);
                            }

                            break;
                    }
                }

                // Settling time for SG
                switch (strSourceEquip)
                {
                    case "SG01":
                        if (varIdSg01 == 1) // ESG --> Freq Mod on 57 msec, Mod off 17 msec,  Level 21 msec
                            Thread.Sleep(100);   // 10 msec for Cal, Generally 5 msec
                        else if (varIdSg01 == 7) // MXG
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec
                        if (strMeasEquip == "PM01PS01")
                            Thread.Sleep(10);   // 10 msec for Cal, Generally 5 msec

                        break;
                }

                Thread.Sleep(100);

                // Power measurement
                switch (strMeasEquip)
                {
                    case "SA01":

                        if (varIdSa01 == 1) // PSA
                        {
                            float MeasuredPout = 0;
                            Thread.Sleep(100);
                            ºmyLibSa.MEAS_SPAN_ZERO(varIdSa01, 0, ref MeasuredPout);
                            varTestResult = MeasuredPout.ToString();
                        }
                        else if (varIdSa01 == 3) // MXA
                        {
                            //ºmyLibSa.OPERATION_COMPLETE();


                            // Placing a marker
                            ºmyLibSa.WRITE(":CALC:MARK1:STAT 1");
                            ºmyLibSa.WRITE(":CALC:MARK1:MODE POS");

                            // Setting frequency
                            ºmyLibSa.WRITE(":CALC:MARK1:X " + arrCalFreqList[i] + " MHz");
                            //ºmyLibSa.OPERATION_COMPLETE();
                            Thread.Sleep(300);
                            // varTestResult
                            varTestResult = ºmyLibSa.WRITE_READ_STRING(":CALC:MARK1:Y?");
                        }
                        else if (varIdSa01 == 4) //PXI
                        {
                            float resultFloat = 0;
                            if (ModulationFormat == "EDGE")
                            {
                                bool maskTestResult = false;
                                ºmyLibSaPxi.MeasPoutEdgePvt(ref resultFloat, ref maskTestResult);
                            }
                            else if (ModulationFormat == "GSM")
                                ºmyLibSaPxi.MeasPoutGsm(ref resultFloat);
                            else if (ModulationFormat == "CW")
                            {
                                ºmyLibSaPxi.MeasPoutCw(ref resultFloat);
                            }
                            varTestResult = resultFloat.ToString();
                        }

                        break;

                    case "PM01PS01":

                        if (varIdPm01 == 1 || varIdPm01 == 4 || varIdPm01 == 5 || varIdPm01 == 6) // P-Series
                        {
                            ºmyLibPM.WRITE("INIT1:CONT 0");
                            //ºmyLibPM.WRITE("*WAI");
                            Thread.Sleep(1000);
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":READ?");
                            break;
                        }

                        if (varIdPm01 == 7) //U2001AH16
                        {

                            //ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibUSB2_PS.WRITE_READ_STRING("FETC?");
                            break;
                        }

                        if (varIdPm01 == 8)
                        {
                            Thread.Sleep(100);
                            double meas_value = -999;
                            if (ModulationFormat == "GSM")
                                meas_value = RSPM_Meas(ºmyLibRS_PS);

                            else if (ModulationFormat == "CW")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);

                            else if (ModulationFormat == "WCDMA")
                                meas_value = RSPM_NoBurstMeas(ºmyLibRS_PS);

                            varTestResult = meas_value.ToString();

                            break;
                        }
                        break;

                    case "PM02PS01":

                        if (varIdPm02 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm02 == 1 || varIdPm02 == 4 || varIdPm02 == 5 || varIdPm02 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM02.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;
                    case "PM03PS01":

                        if (varIdPm03 == 1) // E4419B
                        {
                            ºmyLibPM.OPERATION_COMPLETE();
                            varTestResult = ºmyLibPM.WRITE_READ_STRING(":FETC1?");
                        }
                        else if (varIdPm03 == 1 || varIdPm03 == 4 || varIdPm03 == 5 || varIdPm03 == 6) // P-Series
                        {
                            //ºmyLibPM.OPERATION_COMPLETE();
                            Thread.Sleep(50);
                            varTestResult = ºmyLibPM03.WRITE_READ_STRING("READ?");
                            break;
                        }
                        break;

                }

                //Str construction
                varTestResult = (Convert.ToSingle(varTestResult) + varSgInitPowLev * -1).ToString();
                tempFreq = tempFreq + arrCalFreqList[i] + ",";
                tempResult = tempResult + varTestResult + ",";

                if (Convert.ToSingle(varTestResult) < CalResultMin)
                    CalResultMin = Convert.ToSingle(varTestResult);
                if (Convert.ToSingle(varTestResult) > CalResultMax)
                    CalResultMax = Convert.ToSingle(varTestResult);
            }

            //Write Cal
            int tempInt = tempFreq.Length;
            tempFreq = tempFreq.Remove(tempInt - 1);
            tempInt = tempResult.Length;
            tempResult = tempResult.Remove(tempInt - 1);
            swCalDataFile.WriteLine("");
            swCalDataFile.WriteLine(strTargetCalSegmentName + "," + tempFreq);
            swCalDataFile.WriteLine("," + tempResult);
            tempFreq = "";
            tempResult = "";
            swCalDataFile.Close();

            // Turn off SG01
            switch (strSourceEquip)
            {
                case "SG01":

                    if (varIdSg01 == 1 || varIdSg01 == 2 || varIdSg01 == 3 || varIdSg01 == 7) // ESG
                    {
                        ºmyLibSg.OUTPUT_STATE(INSTR_OUTPUT.OFF);
                        ºmyLibSg.SET_OUTPUT_POWER(-100);
                    }
                    break;
            }
            if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                return 0;
            else
                throw new Exception("Calibration results are out of limits.");
        }

        private void Assign_Cal_File(string _strTargetCalDataFile)
        {
            // Checking and creating a new calibration data file
            fCalDataFile = new FileInfo(_strTargetCalDataFile);

            if (fCalDataFile.Exists)
                MessageBox.Show("The Cal file, " + _strTargetCalDataFile + ", exists. Please, make a backup and click ok.", "Debug Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);

            swCalDataFile = fCalDataFile.CreateText();
        }

        //Added PXI, SgId =8, SaId =4
        private void Assign_Equip_ID(string _strSourceEquipModel, int _strSourceEquipCh, string _strMeasEquipModel, int _strMeasEquipCh)
        {
            // Equipment ID
            varIdSg01 = 0;
            varIdSa01 = 0;
            varIdPm01 = 0;
            varIdPm02 = 0;
            strSourceEquip = "";
            strMeasEquip = "";

            // SG
            if (_strSourceEquipModel == "E4438C")
            {
                varIdSg01 = 1;
                strSourceEquip = "SG01";
            }
            else if (_strSourceEquipModel == "N5182A")
            {
                varIdSg01 = 7;
                strSourceEquip = "SG01";
            }
            else if (_strSourceEquipModel == "VXG") //PXI
            {
                varIdSg01 = 8;
                strSourceEquip = "SG01";
            }
            else
                MessageBox.Show("The source equipment model is not available in the library. Please, check the library and start the progrma again.");

            // Meas Equip
            if (_strMeasEquipModel == "E4440A" && _strMeasEquipCh == 1)
            {
                varIdSa01 = 1;
                strMeasEquip = "SA01";
            }
            else if (_strMeasEquipModel == "N9020A" && _strMeasEquipCh == 1)
            {
                varIdSa01 = 3;
                strMeasEquip = "SA01";
            }
            else if (_strMeasEquipModel == "VXA") //PXI
            {
                varIdSa01 = 4;
                strMeasEquip = "SA01";
            }
            else if (_strMeasEquipModel == "E4417A" && _strMeasEquipCh == 1)
            {
                varIdPm01 = 6;
                strMeasEquip = "PM01PS01";
            }
            else if (_strMeasEquipModel == "E4419B" && _strMeasEquipCh == 1)
            {
                varIdPm01 = 1;
                strMeasEquip = "PM01PS01";
            }
            else if ((_strMeasEquipModel == "U2000A" && _strMeasEquipCh == 1)
                || (_strMeasEquipModel == "U2001A" && _strMeasEquipCh == 1))
            {
                varIdPm01 = 5;
                strMeasEquip = "PM01PS01";
            }
            else if ((_strMeasEquipModel == "U2000A2" && _strMeasEquipCh == 1)
                || (_strMeasEquipModel == "U2001A2" && _strMeasEquipCh == 1))
            {
                varIdPm02 = 5;
                strMeasEquip = "PM02PS01";
            }
            else if ((_strMeasEquipModel == "U2000A3" && _strMeasEquipCh == 1)
                || (_strMeasEquipModel == "U2001A3" && _strMeasEquipCh == 1))
            {
                varIdPm03 = 5;
                strMeasEquip = "PM03PS01";
            }
            else if ((_strMeasEquipModel == "U2000AH16" && _strMeasEquipCh == 1)
                || (_strMeasEquipModel == "U2001AH16" && _strMeasEquipCh == 1))
            {
                varIdPm01 = 7;
                strMeasEquip = "PM01PS01";
            }
            else if ((_strMeasEquipModel == "NRPZ11" && _strMeasEquipCh == 1)
                || (_strMeasEquipModel == "NRPZ11" && _strMeasEquipCh == 1))
            {
                varIdPm01 = 8;
                strMeasEquip = "PM01PS01";
            }
            else
                MessageBox.Show("The meausrement equipment model is not available in the library. Please, check the library and start the progrma again.");

        }

        private void Load_Cal_Data_for_Source_Equip(string strSourceEquipCalFactor, ref string[,] arrCalDataSource, ref bool varCalDataAvailableSource, ref int varNumOfCalFreqList)
        {
            // Loading the calibration data for the source equipment
            if (strSourceEquipCalFactor.ToUpper().Trim() == "NONE")
                varCalDataAvailableSource = false;
            else
            {
                varCalDataAvailableSource = true;

                fCalEquipSource = new FileInfo(strSourceEquipCalFactor);
                srCalEquipSource = new StreamReader(fCalEquipSource.ToString());

                varNumOfCalFreqList = 0;
                while ((tempStr = srCalEquipSource.ReadLine()) != null)
                {
                    string[] arrSttTemp = tempStr.Trim().Split(',');
                    arrCalDataSource[varNumOfCalFreqList, 0] = arrSttTemp[0];
                    arrCalDataSource[varNumOfCalFreqList, 1] = arrSttTemp[1];

                    varNumOfCalFreqList++;
                }
                srCalEquipSource.Close();
            }
        }

        private void Load_Cal_Data_for_Meas_Equip(string strMeasEquipCalFactor, ref bool varCalDataAvailableMeas)
        {
            // Loading the calibration data for the measurement equipment
            if (strMeasEquipCalFactor.ToUpper().Trim() == "NONE")
                varCalDataAvailableMeas = false;
            else
                varCalDataAvailableMeas = true;
        }

        private void Load_Cal_Data_Freq_List(string strCalFreqList, ref string[] arrCalFreqList, ref int varNumOfCalFreqList)
        {
            // Loading the calibration freq list
            fCalFreqList = new FileInfo(strCalFreqList);
            srCalFreqList = new StreamReader(fCalFreqList.ToString());

            varNumOfCalFreqList = 0;
            while ((tempStr = srCalFreqList.ReadLine()) != null)
            {
                arrCalFreqList[varNumOfCalFreqList] = tempStr.Trim();
                varNumOfCalFreqList++;
            }
            srCalFreqList.Close();
        }

        private void RS_BurstSetting()
        {
            ºmyLibRS_PS.chan_reset(1);
            ºmyLibRS_PS.chan_mode(1, rsnrpzConstants.SensorModeTimeslot);
            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Decimal.ToDouble(915) * 1000000.0);
            ºmyLibRS_PS.trigger_setSource(1, rsnrpzConstants.TriggerSourceInternal);
            ºmyLibRS_PS.tslot_configureTimeSlot(1, 2, Decimal.ToDouble(500) / 1000000.0);
            ºmyLibRS_PS.timing_configureExclude(1, Decimal.ToDouble(100) / 1000000.0, Decimal.ToDouble(100) / 1000000.0);
            double triggerLevel_W = Math.Pow(10, Decimal.ToDouble(-35) / 10.0) / 1000.0;
            ºmyLibRS_PS.trigger_setLevel(1, triggerLevel_W);
            ºmyLibRS_PS.avg_configureAvgManual(1, 1);
            ºmyLibRS_PS.errorCheckState(false);

            ºRS_Enable_Burst = true;
        }

        private void RS_NonBurstSetting()
        {
            ºmyLibRS_PS.chan_reset(1);
            ºmyLibRS_PS.avg_setAutoEnabled(1, false);// Deactivates automatic determination of filter bandwidth
            ºmyLibRS_PS.avg_setCount(1, 1); // Number of averages
            ºmyLibRS_PS.chan_setCorrectionFrequency(1, Decimal.ToDouble(915) * 1000000.0); // Set corr frequency
            ºmyLibRS_PS.trigger_setSource(1, rsnrpzConstants.TriggerSourceImmediate);// Set trigger source to immediate
            ºmyLibRS_PS.chan_setContAvAperture(1, 0.001);
            ºmyLibRS_PS.avg_configureAvgManual(1, 1);   // Averaging Manual 1, increase number for better repeatability
            ºmyLibRS_PS.chan_setContAvSmoothingEnabled(1, false);// Smoothing off
            ºmyLibRS_PS.errorCheckState(false);

            ºRS_Enable_Burst = false;
        }

        public float RSPM_Meas(EquipRSNRPZ _EquipRSPS)
        {
            bool meas_complete;
            #region timeslot
            System.DateTime begin_time = System.DateTime.Now;

            for (int i = 0; i < 5; i++)
            {

                ºmyLibRS_PS.chans_initiate();
                System.DateTime tout = System.DateTime.Now.AddSeconds(1);
                do
                {
                    ºmyLibRS_PS.chan_isMeasurementComplete(1, out meas_complete);
                    System.Threading.Thread.Sleep(0);
                } while (!meas_complete && (System.DateTime.Now.CompareTo(tout) < 0));

                if (meas_complete)
                {
                    double[] result = new double[5];
                    int count = 0;

                    _EquipRSPS.meass_fetchBufferMeasurement(1, 5, result, out count);
                    if (count > 0)
                    {
                        double meas_value;
                        double Avg_result;
                        if (count == 2)
                        {
                            Avg_result = ((result[0] + result[1]) / 2);
                        }
                        else
                        {
                            Avg_result = (result[0]);
                        }
                        //meas_value = (10 * Math.Log(Math.Abs(result[0])) / Math.Log(10)) + 30.0;
                        meas_value = (10 * Math.Log(Math.Abs(Avg_result)) / Math.Log(10)) + 30.0;
                        //return  meas_value.ToString();
                        //Update();
                        //measResults[i] = meas_value;
                        return (float)meas_value;

                    }
                    else
                    {
                        throw new System.Runtime.InteropServices.ExternalException("Measurement Error Occured", 0);
                    }
                }
                else
                {
                    break;
                    throw new System.Runtime.InteropServices.ExternalException("Measurement Timeout Occured", 0);
                }
            }


            #endregion
            return -999;
        }

        public float RSPM_NoBurstMeas(EquipRSNRPZ _EquipRSPS)
        {
            bool meas_complete;
            ºmyLibRS_PS.chans_initiate();
            System.DateTime tout = System.DateTime.Now.AddSeconds(5);
            do
            {
                ºmyLibRS_PS.chan_isMeasurementComplete(1, out meas_complete);

            } while (!meas_complete && (System.DateTime.Now.CompareTo(tout) < 2));

            if (meas_complete)
            {
                double result, meas_value;
                ºmyLibRS_PS.meass_fetchMeasurement(1, out result);
                meas_value = 10 * Math.Log(Math.Abs(result)) / Math.Log(10) + 30.0;
                return (float)meas_value;
            }
            else
            {

                //throw new System.Runtime.InteropServices.ExternalException("NON BURST RSPM_Meas Measurement Timeout Occured", 0);
            }

            return -999;
        }

        //KCC - For single cal file
        private void Assign_Cal_File_Combined(string _strTargetCalDataFile)
        {
            // Checking and creating a new calibration data file
            fCalDataFile = new FileInfo(_strTargetCalDataFile);
            swCalDataFile = fCalDataFile.AppendText();
        }

        private void Load_Cal_Data_Freq_List_Combined(string strCalFreqList, ref string[] arrCalFreqList, ref int varNumOfCalFreqList)
        {
            // Loading the calibration freq list
            fCalFreqList = new FileInfo(strCalFreqList);
            srCalFreqList = new StreamReader(fCalFreqList.ToString());

            varNumOfCalFreqList = 0;
            while ((tempStr = srCalFreqList.ReadLine()) != null)
            {
                arrCalFreqList[varNumOfCalFreqList] = tempStr.Trim();    //tempStr.Trim();
                varNumOfCalFreqList++;
            }
            srCalFreqList.Close();
        }

        private void Load_Cal_Data_for_Source_Equip_Combined(string _strTargetCalDataFile, string strSourceEquipCalFactor, string[] arrFreqList, ref string[] arrCalDataSource, ref bool varCalDataAvailableSource)
        {
            string errInformation = "";
            float cal_factor = 0f;
            int varNumOfCalFreqList = 0;

            // Loading the calibration data for the source equipment
            if (strSourceEquipCalFactor.ToUpper().Trim() == "NONE")
                varCalDataAvailableSource = false;
            else
            {
                varCalDataAvailableSource = true;
                varNumOfCalFreqList = 0;
                try
                {
                    swCalDataFile.Close();
                }
                catch { }

                ATFCrossDomainWrapper.Cal_LoadCalData("CalData1D_", _strTargetCalDataFile);

                try
                {
                    Assign_Cal_File_Combined(_strTargetCalDataFile);
                }
                catch { }

                ATFCrossDomainWrapper.Cal_GetCalData1DCombined("CalData1D_", strSourceEquipCalFactor, Convert.ToSingle(arrFreqList[varNumOfCalFreqList]), ref cal_factor, ref errInformation);
                while (arrFreqList[varNumOfCalFreqList] != null)
                {
                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined("CalData1D_", strSourceEquipCalFactor, Convert.ToSingle(arrFreqList[varNumOfCalFreqList]), ref cal_factor, ref errInformation);
                    arrCalDataSource[varNumOfCalFreqList] = cal_factor.ToString(); ;
                    varNumOfCalFreqList++;
                }
                try
                {
                    ATFCrossDomainWrapper.Cal_ResetAll();
                }
                catch { }

            }
        }
    }
}
