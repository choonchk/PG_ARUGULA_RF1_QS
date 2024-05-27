using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToBeObsoleted
{
    class ObsoleteTestPlan1
    {
        //OTP read function
        //public static void ReadOtpPA(ref string MSB_Hex, ref string LSB_Hex)
        //{
        //    PaTest.SmuResources["Vbatt"].ForceVoltage(3.8, 0.01);
        //    //PaTest.SmuResources["Vcc"].ForceVoltage(3.4, 0.01);

        //    HiTimer.wait(100);

        //    MSB_Hex = HSDIO.Instrument.RegRead("D");
        //    if (MSB_Hex == "0")
        //    {
        //        HiTimer.wait(100);
        //        MSB_Hex = HSDIO.Instrument.RegRead("D");
        //    }

        //    if (MSB_Hex.Length == 1)
        //        MSB_Hex = "0" + string.Format("{0:x}", MSB_Hex).ToUpper();

        //    HiTimer.wait(100);

        //    LSB_Hex = HSDIO.Instrument.RegRead("E");
        //    if (LSB_Hex.Length == 1)
        //        LSB_Hex = "0" + string.Format("{0:x}", LSB_Hex).ToUpper();

        //    PaTest.SmuResources["Vbatt"].ForceVoltage(0.0, 0.01);
        //    //PaTest.SmuResources["Vcc"].ForceVoltage(0.0, 0.01);
        //}
        //public static void ReadOtpFbar(ref string MSB_Hex, ref string LSB_Hex)
        //{
        //    string LSB_Dec = "";
        //    string MSB_Dec = "";
        //    Aemulus_PXI.SMU_ClampCurrent(Aemulus_AM471_PinAlias.SMUPin.VBATT, 0.01);
        //    Aemulus_PXI.SMU_DriveVoltage(Aemulus_AM471_PinAlias.SMUPin.VBATT, 3.8);
        //    Aemulus_PXI.HSDIO_SwitchVecToMIPI(0);

        //    HiTimer.wait(50);

        //    MSB_Dec = Aemulus_PXI.HSDIO_Read_Register_SplitTest(0xD); //decimal output

        //    HiTimer.wait(50);

        //    LSB_Dec = Aemulus_PXI.HSDIO_Read_Register_SplitTest(0xE);

        //    int number = int.Parse(MSB_Dec);
        //    MSB_Hex = number.ToString("x");
        //    if (MSB_Hex.Length == 1)
        //        MSB_Hex = "0" + string.Format("{0:x}", MSB_Hex).ToUpper();

        //    number = int.Parse(LSB_Dec);
        //    LSB_Hex = number.ToString("x");
        //    if (LSB_Hex.Length == 1)
        //        LSB_Hex = "0" + string.Format("{0:x}", LSB_Hex).ToUpper();

        //    //MSB_Hex = MSB_Hex.ToUpper();
        //    //LSB_Hex = LSB_Hex.ToUpper();

        //    Aemulus_PXI.HSDIO_SwitchMIPIToVec(0);
        //    Aemulus_PXI.HSDIO_VIO_Off();
        //    Aemulus_PXI.SMU_Off();
        //}

        //public void VST_SMUSelfCal(bool checkTempFile)
        //{
        //    try
        //    {
        //        SMU_Vcc_temperature = 0;
        //        SMU_Vbatt_temperature = 0;
        //        SMU_Vdd_temperature = 0;
        //        SMU_Vlna_temperature = 0;
        //        SA_temperature = 0;
        //        SG_temperature = 0;
        //        AlignFlag = 0;

        //        PaTest.SmuResources["Vcc"].GetDeviceTemperature(out SMU_Vcc_temperature);
        //        PaTest.SmuResources["Vbatt"].GetDeviceTemperature(out SMU_Vbatt_temperature);
        //        PaTest.SmuResources["Vdd"].GetDeviceTemperature(out SMU_Vdd_temperature);
        //        PaTest.SmuResources["Vlna"].GetDeviceTemperature(out SMU_Vlna_temperature);

        //        PxiVst.SA.GetDeviceTemperature(out SA_temperature);
        //        PxiVst.SG.GetDeviceTemperature(out SG_temperature);

        //        if (checkTempFile)
        //        {
        //            if (ProductionFunctions.TemperatureAlignFlag(SMU_Vcc_temperature) == true)
        //            {
        //                PxiVst.SA.SelfCalRange(1700e6, 6000e6, -60, 15);   //1920e6, 1980e6, -30, 30
        //                PxiVst.SG.SelfCalRange(1700e6, 2600e6, -40, 0);     //1920e6, 1980e6, -20, 8
        //                                                                    //PaTest.SmuResources["Vcc"].DeviceSelfCal();
        //                                                                    //PaTest.SmuResources["Vbatt"].DeviceSelfCal(); //CM Edited: removed due to leakage -ve value
        //                                                                    //PaTest.SmuResources["Vdd"].DeviceSelfCal();
        //                                                                    //PaTest.SmuResources["Vlna"].DeviceSelfCal(); //CM Edited: removed due to leakage -ve value
        //                LogToLogServiceAndFile(LogLevel.HighLight, "Check Temp File, Equipment temperature file not existed & delta > 1 degree. VST self-cal ran");
        //                AlignFlag = 1;
        //            }
        //        }
        //        else
        //        {
        //            if (logTemperature)
        //            {
        //                previousTemperature = SMU_Vcc_temperature;

        //                if (File.Exists(TempLogLocation))
        //                    File.Delete(TempLogLocation);
        //                TempFile = new FileInfo(TempLogLocation);
        //                swTempFile = TempFile.CreateText();
        //                swTempFile.Close();
        //                swTempFile = TempFile.AppendText();
        //                swTempFile.WriteLine(previousTemperature);
        //                swTempFile.Close();
        //            }
        //        }
        //    }

        //    //double current_vstTemp;
        //    //double delta_vstTemp;

        //    //PxiVst.SA.GetDeviceTemperature("", out current_vstTemp);

        //    //delta_vstTemp = Math.Abs(current_vstTemp - CAL_vstTemp);

        //    //if (delta_vstTemp > 1)
        //    //{
        //    //    PxiVst.SA.SelfCalRange(2300e6, -20300e6, -60, 35);   //1920e6, 1980e6, -30, 30
        //    //    PxiVst.SG.SelfCalRange(2300e6, 2690e6, -28, 10);     //1920e6, 1980e6, -20, 8
        //    //    CAL_vstTemp = current_vstTemp;
        //    //}

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        programLoadSuccess = false;
        //    }
        //}

        //public bool Run_Manual_SW(string Channel)
        //{
        //    string Sw_Band = "";
        //    bool Sw_Test = true;
        //    switch (Channel.ToUpper())
        //    {
        //        case "1":
        //            Sw_Band = "B7";
        //            break;
        //        case "2":
        //            Sw_Band = "B30";
        //            break;
        //        case "3":
        //            Sw_Band = "B40A";
        //            break;
        //        case "4":
        //            Sw_Band = "B40B";
        //            break;
        //        case "5":
        //            Sw_Band = "B41A";
        //            break;
        //        case "6":
        //            Sw_Band = "B41B";
        //            break;
        //        case "7":
        //            Sw_Band = "B41C";
        //            break;
        //        default:
        //            Sw_Test = false;
        //            break;
        //    }

        //    if (Sw_Test)
        //    {
        //        MessageBox.Show("Click OK after configure " + Sw_Band + " for calibration.", "Procedures");

        //        if (SplitTestVariable.SplitTestEnable)
        //        {
        //            clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.ENAtoRFIN);
        //            clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.ENAtoRFOUT);
        //            clsSwitchMatrix.Maps2.Activate(Sw_Band, clsSwitchMatrix.Operation2.ENAtoRX);
        //        }
        //        else
        //        {
        //            SwitchMatrix.Maps.Activate(Sw_Band, InstrLib.Operation.ENAtoRFIN);
        //            SwitchMatrix.Maps.Activate(Sw_Band, InstrLib.Operation.ENAtoRFOUT);
        //            SwitchMatrix.Maps.Activate(Sw_Band, InstrLib.Operation.ENAtoRX);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Invalid Channel Number", "Procedures");
        //    }

        //    return Sw_Test;
        //}

        public static int CompareMSB_LSB(int Fbar_MSB, int Fbar_LSB, int Pa_MSB, int Pa_LSB)
        {
            //Match
            // 0 = fail
            // 1 = pass
            // 2 = skip due to contact
            int match = 0;
            if ((Fbar_MSB != 0) && (Fbar_LSB != 0))
            {
                if ((Pa_MSB != 0) && (Pa_LSB != 0))
                {
                    if ((Fbar_MSB == Pa_MSB) && (Fbar_LSB == Pa_LSB))
                    {
                        match = 1;
                    }
                    else
                    {
                        match = 0;
                    }
                }
                else //(Fbar_MSB == Pa_MSB && Fbar_LSB == Pa_LSB)
                {
                    match = 2; //skip
                }
            }
            else //(Fbar_MSB != 0 && Fbar_LSB != 0)
            {
                match = 2; //skip
            }


            return match;
        }

        public void makecalfile(string[] FreqList, ref int FreqCount, string Freq)
        {
            if (Freq == "1710")
            {
                int adas = 0;
            }
            if (!FreqList.Contains(Freq))
            {
                FreqList[FreqCount] = Freq;
                FreqCount++;
            }
            if (!FreqList.Contains(Convert.ToString(Convert.ToDouble(Freq) * 2)))
            {
                FreqList[FreqCount] = Convert.ToString(Convert.ToDouble(Freq) * 2);
                FreqCount++;
            }
            if (!FreqList.Contains(Convert.ToString(Convert.ToDouble(Freq) * 3)))
            {
                FreqList[FreqCount] = Convert.ToString(Convert.ToDouble(Freq) * 3);
                FreqCount++;
            }

        }

    }
}
