using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LibFBAR.DC
{
    public class cAemulus
    {
        public static string ClassName = "Power Supply Aemulus Class";
        protected static int err;
        protected static Int32 returnVal;

        #region "Class Initialization"
        public cSystem System;
        public cRead Read;
        public cClamp Clamp;
        public cSetting Settings;
        public cSMU SMU;

        private static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        public cAemulus()
        {
            Init();
        }
        public void Init()
        {
            System = new cSystem();
            Read = new cRead();
            Clamp = new cClamp();
            Settings = new cSetting();
            SMU = new cSMU();

        }
        #endregion

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  18/10/2010       KKL             VISA Driver for DC4142

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }
        public class cSystem
        {
            public void Initialize()
            {
                err = pInitialize();
                RaiseError(err);
            }
            public void ResetBoards()
            {
                err = pResetBoards();
                RaiseError(err);
            }
        }
        public class cDrive
        {
            public void DriveVoltage(int Channel, int mVolt, int Sign)
            {
                err = pdriveVoltage(Channel, mVolt, Sign);
                RaiseError(err);
            }
            public void DriveCurrent(int Channel, int nAmp, int Sign)
            {
                err = pdriveCurrent(Channel, nAmp, Sign);
                RaiseError(err);
            }
        }
        public class cRead
        {
            public Int32 Voltage(int Channel)
            {
                err = psenseVoltage(Channel, ref returnVal);
                RaiseError(err);
                if (err == 0)
                {
                    return (returnVal);
                }
                else
                {
                    return (-999);
                }
            }
            public Int32 Current(int Channel)
            {
                err = psenseCurrent(Channel, ref returnVal);
                RaiseError(err);
                if (err == 0)
                {
                    return (returnVal);
                }
                else
                {
                    return (-999);
                }
            }
            public Int32 CurrentWithAverage(int Channel, Int32 Average)
            {
                err = psenseCurrentWithAverage(Channel, Average, ref returnVal);
                RaiseError(err);
                if (err == 0)
                {
                    return (returnVal);
                }
                else
                {
                    return (-999);
                }
            }
            public Int32 CurrentWithAutoRange(int Channel)
            {
                err = psenseCurrentAutoRange(Channel, ref returnVal);
                RaiseError(err);
                if (err == 0)
                {
                    return (returnVal);
                }
                else
                {
                    return (-999);
                }
            }
            public Int32 CurrentFromRange(int Channel)
            {
                err = psenseCurrentFromRange(Channel, ref returnVal);
                RaiseError(err);
                if (err == 0)
                {
                    return (returnVal);
                }
                else
                {
                    return (-999);
                }
            }
        }
        public class cClamp
        {
            public void ClampVoltage(int Channel, Int32 mVolt)
            {
                err = pclampVoltage(Channel, mVolt);
                RaiseError(err);
            }
            public void ClampCurrent(int Channel, Int32 mCurrent)
            {
                err = pclampVoltage(Channel, mCurrent);
                RaiseError(err);
            }
        }
        public class cSetting
        {
            public void onSMU()
            {
                err = ponSmuCh();
                RaiseError(err);
            }
            public void offSMU()
            {
                err = poffSmuCh();
                RaiseError(err);
            }
            public void SetAnalogePinBandwidth(Int32 Pin, Int32 Bandwidth)
            {
                err = psetAnaPinBW(Pin, Bandwidth);
                RaiseError(err);
            }
            public void SetIntergration(int Set)
            {
                err = pSetIntegration(Set);
                RaiseError(err);
            }
            public void SetIntegrationPowerCycle(int Set, int Cycles)
            {
                err = pSetIntegrationNPLC(Set, Cycles);
                RaiseError(err);
            }
            public void SetNPLC(int Channel, float Settings)
            {
                err = pSetNPLC(Channel, Settings);
                RaiseError(err);
            }
        }
        public class cSMU
        {
            public void BiasSMUPin(int ChannelSet, ref Int32 ChannelData)
            {
                err = pBiasSmuPin(ChannelSet, ref ChannelData);
                RaiseError(err);
            }
            public void ReadSMUPin(int ChannelSet, ref Int32 ChannelData, ref Int32 Result)
            {
                err = pReadSmuPin(ChannelSet, ref ChannelData, ref Result);
                RaiseError(err);
            }
            public void OnOffSMUPin(int ChannelSet, ref Int32 ChannelData)
            {
                err = pOnOffSmuPin(ChannelSet, ref ChannelData);
                RaiseError(err);
            }
        }



        #region "Direct API calls"
        [DllImport("AM330.dll", EntryPoint = "INITIALIZE")]
        static extern System.Int32 pInitialize();

        [DllImport("AM330.dll", EntryPoint = "RESETBOARDS")]
        static extern System.Int32 pResetBoards();

        [DllImport("AM330.dll", EntryPoint = "DRIVEVOLTAGE")]
        static extern System.Int32 pdriveVoltage(System.Int32 Ch, System.Int32 mVvalue, System.Int32 sign);
        [DllImport("AM330.dll", EntryPoint = "DRIVECURRENT")]
        static extern System.Int32 pdriveCurrent(System.Int32 Ch, System.Int32 nAvalue, System.Int32 sign);

        [DllImport("AM330.dll", EntryPoint = "READVOLTAGE")]
        static extern System.Int32 psenseVoltage(System.Int32 Ch, ref System.Int32 mVvalue);
        [DllImport("AM330.dll", EntryPoint = "READCURRENT")]
        static extern System.Int32 psenseCurrent(System.Int32 Ch, ref System.Int32 nAvalue);
        [DllImport("AM330.dll", EntryPoint = "READCURRENTWITHAVERAGE")]
        static extern System.Int32 psenseCurrentWithAverage(System.Int32 Ch, System.Int32 Average, ref System.Int32 nAvalue);
        [DllImport("AM330.dll", EntryPoint = "READCURRENTAUTORANGE")]
        static extern System.Int32 psenseCurrentAutoRange(System.Int32 Ch, ref System.Int32 nAvalue);
        [DllImport("AM330.dll", EntryPoint = "READCURRENTFROMRANGE")]
        static extern System.Int32 psenseCurrentFromRange(System.Int32 Ch, ref System.Int32 nAvalue);

        [DllImport("AM330.dll", EntryPoint = "CLAMPVOLTAGE")]
        static extern System.Int32 pclampVoltage(System.Int32 Ch, System.Int32 mVvalue);
        [DllImport("AM330.dll", EntryPoint = "CLAMPCURRENT")]
        static extern System.Int32 pclampCurrent(System.Int32 Ch, System.Int32 nAvalue);

        [DllImport("AM330.dll", EntryPoint = "ONSMUPIN")]
        static extern System.Int32 ponSmuCh();
        [DllImport("AM330.dll", EntryPoint = "OFFSMUPIN")]
        static extern System.Int32 poffSmuCh();
        [DllImport("AM330.dll", EntryPoint = "SETANAPINBANDWIDTH")]
        static extern System.Int32 psetAnaPinBW(System.Int32 pin, System.Int32 setting);
        [DllImport("AM330.dll", EntryPoint = "SETINTEGRATION")]
        static extern System.Int32 pSetIntegration(System.Int32 Set);
        [DllImport("AM330.dll", EntryPoint = "SETINTEGRATIONPOWERCYCLES")]
        static extern System.Int32 pSetIntegrationNPLC(System.Int32 setting, System.Int32 cycle);
        [DllImport("AM330.dll", EntryPoint = "SETNPLC")]
        static extern System.Int32 pSetNPLC(System.Int32 Ch, System.Single setting);

        [DllImport("AM330.dll", EntryPoint = "BIASSMUPIN")]
        static extern System.Int32 pBiasSmuPin(System.Int32 chset, ref System.Int32 chdat);
        [DllImport("AM330.dll", EntryPoint = "READSMUPIN")]
        static extern System.Int32 pReadSmuPin(System.Int32 chset, ref System.Int32 chdat, ref System.Int32 chRead);
        [DllImport("AM330.dll", EntryPoint = "ONOFFSMUPIN")]
        static extern System.Int32 pOnOffSmuPin(System.Int32 chset, ref System.Int32 chdat);

        [DllImport("AM330.dll", EntryPoint = "AM371_DRIVEVOLTAGE")]
        static extern System.Int32 am371_driveVoltage(System.Int32 Ch, System.Single Vvalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_DRIVEVOLTAGESETVRANGE")]
        static extern System.Int32 am371_driveVoltageSetVrange(System.Int32 Ch, System.Single Vvalue, System.Int32 Vrange);
        [DllImport("AM330.dll", EntryPoint = "AM371_DRIVECURRENT")]
        static extern System.Int32 am371_driveCurrent(System.Int32 Ch, System.Single Avalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_DRIVECURRENTSETIRANGE")]
        static extern System.Int32 am371_driveCurrentSetIrange(System.Int32 Ch, System.Single Avalue, System.Int32 Irange);

        [DllImport("AM330.dll", EntryPoint = "AM371_READVOLTAGE")]
        static extern System.Int32 am371_senseVoltage(System.Int32 Ch, ref System.Single Vvalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_READCURRENT")]
        static extern System.Int32 am371_senseCurrent(System.Int32 Ch, ref System.Single Avalue);

        [DllImport("AM330.dll", EntryPoint = "AM371_CLAMPVOLTAGE")]
        static extern System.Int32 am371_clampVoltage(System.Int32 Ch, System.Single Vvalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_CLAMPVOLTAGESETVRANGE")]
        static extern System.Int32 am371_clampVoltageSetVrange(System.Int32 Ch, System.Single Vvalue, System.Int32 Vrange);
        [DllImport("AM330.dll", EntryPoint = "AM371_CLAMPCURRENT")]
        static extern System.Int32 am371_clampCurrent(System.Int32 Ch, System.Single Avalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_CLAMPCURRENTSETIRANGE")]
        static extern System.Int32 am371_clampCurrentSetIrange(System.Int32 Ch, System.Single Avalue, System.Int32 Irange);

        [DllImport("AM330.dll", EntryPoint = "AM371_ONSMUPIN")]
        static extern System.Int32 am371_onSmuCh(System.Int32 Ch, System.Int32 remote_sense_h);
        [DllImport("AM330.dll", EntryPoint = "AM371_OFFSMUPIN")]
        static extern System.Int32 am371_offSmuCh();

        [DllImport("AM330.dll", EntryPoint = "AM371_READVOLTAGEARRAY")]
        static extern System.Int32 am371_senseVoltageArray(System.Int32 Ch, System.Int32 nSamples, System.Single sample_delay_s, ref System.Single Vvalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_READCURRENTARRAY")]
        static extern System.Int32 am371_senseCurrentArray(System.Int32 Ch, System.Int32 nSamples, System.Single sample_delay_s, ref System.Single Avalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGARM_READCURRENTARRAY")]
        static extern System.Int32 am371_ExtTrigArm_senseCurrentArray(System.Int32 Ch, System.Int32 posedge_h, System.Int32 delay_s, System.Int32 nSamples, System.Single sample_delay_s);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGARM_READVOLTAGEARRAY")]
        static extern System.Int32 am371_ExtTrigArm_senseVoltageArray(System.Int32 Ch, System.Int32 posedge_h, System.Int32 delay_s, System.Int32 nSamples, System.Single sample_delay_s);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGGET_READCURRENTARRAY")]
        static extern System.Int32 am371_ExtTrigGet_senseCurrentArray(System.Int32 Ch, ref System.Int32 nSamples, ref System.Single Aval);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGGET_READVOLTAGEARRAY")]
        static extern System.Int32 am371_ExtTrigGet_senseVoltageArray(System.Int32 Ch, ref System.Int32 nSamples, ref System.Single Vval);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX")]
        static extern System.Int32 am371_ExtTrigGet_senseCurrentArray_minmax(System.Int32 Ch, ref System.Int32 nSamples, ref System.Single Aval, ref System.Single minAval, ref System.Single maxAval);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGGET_READVOLTAGEARRAY_WITH_MINMAX")]
        static extern System.Int32 am371_ExtTrigGet_senseVoltageArray_minmax(System.Int32 Ch, ref System.Int32 nSamples, ref System.Single Vval, ref System.Single minVval, ref System.Single maxVval);

        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGARM_GETSTATUS")]
        static extern System.Int32 am371_ExtTrigArm_getStatus(System.Int32 Ch, ref System.Int32 status, ref System.Int32 Mode);
        [DllImport("AM330.dll", EntryPoint = "AM371_EXTTRIGARM_RELEASE")]
        static extern System.Int32 am371_ExtTrigArm_release();

        [DllImport("AM330.dll", EntryPoint = "AM371_USERBWSEL")]
        static extern System.Int32 am371_UserBwSel(System.Int32 Ch, System.Int32 drCoarseBw, System.Int32 drBoostEn, System.Int32 clCoarseBw, System.Int32 clBoostEn);
        [DllImport("AM330.dll", EntryPoint = "AM371_READCURRENT10X")]
        static extern System.Int32 am371_ReadCurrent10x(System.Int32 Ch, ref System.Single Avalue);
        [DllImport("AM330.dll", EntryPoint = "AM371_CLAMPSUMCONTROL")]
        static extern System.Int32 am371_SetClampSumControl(System.Int32 Ch, System.Int32 setting);
        #endregion

        public static void RaiseError(int ErrorCode)
        {
            switch (ErrorCode)
            {
                case 0: // Successful
                    break;
                case 1: //Cannot write to device
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Unable to write to device");
                    break;
                case 2: //Cannot read from device
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Unable to read to device");
                    break;
                case 3: //Out of memory
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Device out of memory");
                    break;
                case 4: //Invalid argument(s)
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Invalid argument(s)");
                    break;
                case 5: //Current overdrive
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Current overdrive");
                    break;
                case 6: //Timed out
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Device Timed Out");
                    break;
                case 7: //DirectAPI does not support this tester
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Unsupportrf Direct API call");
                    break;
                case 8: //Temperature overheat
                    General.DisplayError(ClassName, "Error on Aemulus Device", "Device Temperature overheat");
                    break;
                default:

                    break;
            }
        }
    }
    public class cAemulus_AM330
    {
        public static string ClassName = "Power Supply Aemulus Class";

        private static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        int session, hSys;
        static string API_Name = "";

        #region "Class Initialization"
        public cSystem System;
        public cPort Port;
        public cRead Read;
        public cDrive Drive;
        public cClamp Clamp;
        public cSetting Settings;
        public cSMU SMU;
        public cExtTrig ExtTrig;
        
        public void Init(int parse_hSys)
        {
            System = new cSystem(parse_hSys);
            Port = new cPort(parse_hSys);
            Read = new cRead(parse_hSys);
            Drive = new cDrive(parse_hSys);
            Clamp = new cClamp(parse_hSys);
            Settings = new cSetting(parse_hSys);
            SMU = new cSMU(parse_hSys);
            ExtTrig = new cExtTrig(parse_hSys);
        }
        #endregion

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  18/10/2010       KKL             VISA Driver for DC4142

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }
        
        public void CREATESESSION(string hostname)
        {
            API_Name = "CREATESESSION"; retVal = aMB1340C.CREATESESSION(hostname, out session);
        }
        public void CLOSESESSION()
        {
            API_Name = "CLOSESESSION"; retVal = aMB1340C.CLOSESESSION(session);
        }
        public void AMB1340C_CREATEINSTANCE(int sysId, int offlinemode)
        {
            API_Name = "AMB1340C_CREATEINSTANCE"; retVal = aMB1340C.AMB1340C_CREATEINSTANCE(session, sysId, offlinemode, out hSys);
            Init(hSys);
        }
        public void AMB1340C_DELETEINSTANCE()
        {
            API_Name = "AMB1340C_DELETEINSTANCE"; retVal = aMB1340C.AMB1340C_DELETEINSTANCE(hSys);
        }
        public int Parse_hSys
        {
            get
            {
                return (hSys);
            }
            set
            {
                hSys = value;
                Init(value);
            }
        }
        public class cSystem
        {
            int hSys;
            public cSystem(int parse)
            {
                hSys = parse;
            }
            public void INITIALIZE()
            {
                API_Name = "INITIALIZE"; retVal = aMB1340C.INITIALIZE(hSys);
            }
            public void RESETBOARDS()
            {
                API_Name = "RESETBOARDS"; retVal = aMB1340C.RESETBOARDS();
            }
        }
        public class cPort
        {
            int hSys;
            public cPort(int parse)
            {
                hSys = parse;
            }
            public void AMB1340C_DRIVEPORT(int value)
            {
                API_Name = "AMB1340C_DRIVEPORT"; retVal = aMB1340C.DRIVEPORT(hSys, value);
            }
            public void AMB1340C_DRIVEPIN(int pin, int value)
            {
                API_Name = "AMB1340C_DRIVEPIN"; retVal = aMB1340C.DRIVEPIN(hSys, pin, value);
            }
            public void AMB1340C_READPORT(out int value)
            {
                API_Name = "AMB1340C_READPORT"; retVal = aMB1340C.READPORT(hSys, out value);
            }
            public void AMB1340C_READPIN(int pin, out int value)
            {
                API_Name = "AMB1340C_READPIN"; retVal = aMB1340C.READPIN(hSys, pin, out value);
            }
            public void AMB1340C_SETPORTDIRECTION(int value)
            {
                API_Name = "AMB1340C_SETPORTDIRECTION"; retVal = aMB1340C.SETPORTDIRECTION(hSys, value);
            }
            public void AMB1340C_SETPINDIRECTION(int pin, int value)
            {
                API_Name = "AMB1340C_SETPINDIRECTION"; retVal = aMB1340C.SETPINDIRECTION(hSys, pin, value);
            }
            public void AMB1340C_GETPORTDIRECTION(out int value)
            {
                API_Name = "AMB1340C_GETPORTDIRECTION"; retVal = aMB1340C.GETPORTDIRECTION(hSys, out value);
            }
            public void AMB1340C_GETPINDIRECTION(int pin, out int value)
            {
                API_Name = "AMB1340C_GETPINDIRECTION"; retVal = aMB1340C.GETPINDIRECTION(hSys, pin, out value);
            }

        }
        public class cDrive
        {
            int hSys;
            public cDrive(int parse)
            {
                hSys = parse;
            }
            public void DRIVEVOLTAGE(int chset, int mVvalue, int sign)
            {
                API_Name = "DRIVEVOLTAGE"; retVal = aMB1340C.DRIVEVOLTAGE(hSys, chset, mVvalue, sign);
            }
            public void DRIVECURRENT(int chset, int nAvalue, int sign)
            {
                API_Name = "DRIVECURRENT"; retVal = aMB1340C.DRIVECURRENT(hSys, chset, nAvalue, sign);
            }

            public void AM371_DRIVEVOLTAGE(int pin, float volt)
            {
                API_Name = "AM371_DRIVEVOLTAGE"; retVal = aMB1340C.AM371_DRIVEVOLTAGE(hSys, pin, volt);
            }
            public void AM371_DRIVECURRENT(int pin, float ampere)
            {
                API_Name = "AM371_DRIVECURRENT"; retVal = aMB1340C.AM371_DRIVECURRENT(hSys, pin, ampere);
            }
            public void AM371_DRIVEVOLTAGESETVRANGE(int pin, float volt, int vrange)
            {
                API_Name = "AM371_DRIVEVOLTAGESETVRANGE"; retVal = aMB1340C.AM371_DRIVEVOLTAGESETVRANGE(hSys, pin, volt, vrange);
            }
            public void AM371_DRIVECURRENTSETIRANGE(int pin, float ampere, int irange)
            {
                API_Name = "AM371_DRIVECURRENTSETIRANGE"; retVal = aMB1340C.AM371_DRIVECURRENTSETIRANGE(hSys, pin, ampere, irange);
            }

            public void AM330_DRIVEPULSEVOLTAGE(int pin, float _base, float pulse, float pulse_s, float hold_s,
            int dr_vrange, int cycles, int meas_ch, int meas_sel, int meas_vrange, int trig_percentage,
            int arm_ext_trigin_h, float timeout_s)
            {
                API_Name = "AM330_DRIVEPULSEVOLTAGE"; retVal = aMB1340C.AM330_DRIVEPULSEVOLTAGE(hSys, pin, _base, pulse, pulse_s, hold_s, dr_vrange, cycles, meas_ch, meas_sel, meas_vrange, trig_percentage, arm_ext_trigin_h, timeout_s);
            }
        }
        public class cRead
        {
            int hSys;
            public cRead(int parse)
            {
                hSys = parse;
            }
            public void READVOLTAGE(int chset, out int mVvalue)
            {
                API_Name = "READVOLTAGE"; retVal = aMB1340C.READVOLTAGE(hSys, chset, out mVvalue);
            }
            public void READCURRENT(int chset, out int nAvalue)
            {
                API_Name = "READCURRENT"; retVal = aMB1340C.READCURRENT(hSys, chset, out nAvalue);
            }
            public void READCURRENTRATE(int chset, out int nAvalue)
            {
                API_Name = "READCURRENTRATE"; retVal = aMB1340C.READCURRENTRATE(hSys, chset, out nAvalue);
            }
            public void READVOLTAGEVOLT(int chset, out float volt)
            {
                API_Name = "READVOLTAGEVOLT"; retVal = aMB1340C.READVOLTAGEVOLT(hSys, chset, out volt);
            }
            public void READCURRENTAMP(int chset, out float ampere)
            {
                API_Name = "READCURRENTAMP"; retVal = aMB1340C.READCURRENTAMP(hSys, chset, out ampere);
            }
            public void READCURRENTAMPRATE(int chset, out float ampere)
            {
                API_Name = "READCURRENTAMPRATE"; retVal = aMB1340C.READCURRENTAMPRATE(hSys, chset, out ampere);
            }
            public void READVOLTAGEWITHAVERAGE(int chset, int average, out int average_mV, out int every_mV)
            {
                API_Name = "READVOLTAGEWITHAVERAGE"; retVal = aMB1340C.READVOLTAGEWITHAVERAGE(hSys, chset, average, out average_mV, out every_mV);
            }
            public void READCURRENTWITHAVERAGE(int chset, int average, out int average_nA, out int every_nA)
            {
                API_Name = "READCURRENTWITHAVERAGE"; retVal = aMB1340C.READCURRENTWITHAVERAGE(hSys, chset, average, out average_nA, out every_nA);
            }
            public void READCURRENTAUTORANGE(int chset, out int nAvalue)
            {
                API_Name = "READCURRENTAUTORANGE"; retVal = aMB1340C.READCURRENTAUTORANGE(hSys, chset, out nAvalue);
            }
            public void READCURRENTFROMRANGE(int chset, out int nAvalue)
            {
                API_Name = "READCURRENTFROMRANGE"; retVal = aMB1340C.READCURRENTFROMRANGE(hSys, chset, out nAvalue);
            }

            public void AM371_READVOLTAGE(int pin, out float volt)
            {
                API_Name = "AM371_READVOLTAGE"; retVal = aMB1340C.AM371_READVOLTAGE(hSys, pin, out volt);
            }
            public void AM371_READVOLTAGEGETVRANGE(int pin, out float volt, out int vrange)
            {
                API_Name = "AM371_READVOLTAGEGETVRANGE"; retVal = aMB1340C.AM371_READVOLTAGEGETVRANGE(hSys, pin, out volt, out vrange);
            }
            public void AM371_READCURRENT(int pin, out float ampere)
            {
                API_Name = "AM371_READCURRENT"; retVal = aMB1340C.AM371_READCURRENT(hSys, pin, out ampere);
            }
            public void AM371_READCURRENTRATE(int pin, out float ampere)
            {
                API_Name = "AM371_READCURRENTRATE"; retVal = aMB1340C.AM371_READCURRENTRATE(hSys, pin, out ampere);
            }
            public void AM371_READCURRENTGETIRANGE(int pin, out float ampere, out int irange)
            {
                API_Name = "AM371_READCURRENTGETIRANGE"; retVal = aMB1340C.AM371_READCURRENTGETIRANGE(hSys, pin, out ampere, out irange);
            }

            public void AM371_READCURRENT10X(int pin, out float Avalue)
            {
                API_Name = "AM371_READCURRENT10X"; retVal = aMB1340C.AM371_READCURRENT10X(hSys, pin, out Avalue);
            }
            public void AM371_READCURRENT10XRATE(int pin, out float Avalue)
            {
                API_Name = "AM371_READCURRENT10XRATE"; retVal = aMB1340C.AM371_READCURRENT10XRATE(hSys, pin, out Avalue);
            }
        }
        public class cSMU
        {
            int hSys;
            public cSMU(int parse)
            {
                hSys = parse;
            }
            public void BIASSMUPIN(int chset, out int chdat)
            {
                API_Name = "BIASSMUPIN"; retVal = aMB1340C.BIASSMUPIN(hSys, chset, out chdat);
            }
            public void READSMUPIN(int chset, out int chdat, out int chRead)
            {
                API_Name = "READSMUPIN"; retVal = aMB1340C.READSMUPIN(hSys, chset, out chdat, out chRead);
            }
            public void READSMUPINRATE(int chset, out int chdat, out int chRead)
            {
                API_Name = "READSMUPINRATE"; retVal = aMB1340C.READSMUPINRATE(hSys, chset, out chdat, out chRead);
            }
            public void ONSMUPIN(int pin)
            {
                API_Name = "ONSMUPIN"; retVal = aMB1340C.ONSMUPIN(hSys, pin);
            }
            public void OFFSMUPIN(int pin)
            {
                API_Name = "OFFSMUPIN"; retVal = aMB1340C.OFFSMUPIN(hSys, pin);
            }
            public void ONOFFSMUPIN(int chset, out int chdat)
            {
                API_Name = "ONOFFSMUPIN"; retVal = aMB1340C.ONOFFSMUPIN(hSys, chset, out chdat);
            }

            public void ARMREADSMUPIN(int measset, out int chdat)
            {
                API_Name = "ARMREADSMUPIN"; retVal = aMB1340C.ARMREADSMUPIN(hSys, measset, out chdat);
            }
            public void RETRIEVEREADSMUPIN(int measset, out int chdat, out int chRead)
            {
                API_Name = "RETRIEVEREADSMUPIN"; retVal = aMB1340C.RETRIEVEREADSMUPIN(hSys, measset, out chdat, out chRead);
            }

            public void SOURCEDELAYMEASURESMUPIN(int chset, out int chdat, out int chRead, int sequence)
            {
                API_Name = "SOURCEDELAYMEASURESMUPIN";
                retVal = aMB1340C.SOURCEDELAYMEASURESMUPIN(hSys, chset, out chdat, out chRead, sequence);
            }

            public void AMB1340C_SOURCEDELAYMEASURESMUPIN(int pinset, out float pindat, out int measset, out float pinRead, int sequence)
            {
                API_Name = "AMB1340C_SOURCEDELAYMEASURESMUPIN";
                retVal = aMB1340C.AMB1340C_SOURCEDELAYMEASURESMUPIN(hSys, pinset, out pindat, out measset, out pinRead, sequence);
            }

            public void AM371_ONSMUPIN(int pin, int remote_sense_h)
            {
                API_Name = "AM371_ONSMUPIN"; retVal = aMB1340C.AM371_ONSMUPIN(hSys, pin, remote_sense_h);
            }
            public void AM371_OFFSMUPIN(int pin)
            {
                API_Name = "AM371_OFFSMUPIN"; retVal = aMB1340C.AM371_OFFSMUPIN(hSys, pin);
            }
        }

        public class cSetting
        {
            int hSys;
            public cSetting(int parse)
            {
                hSys = parse;
            }

            public void SETANAPINBANDWIDTH(int pin, int setting)
            {
                API_Name = "SETANAPINBANDWIDTH"; retVal = aMB1340C.SETANAPINBANDWIDTH(hSys, pin, setting);
            }
            public void SETINTEGRATION(int chdat)
            {
                API_Name = "SETINTEGRATION"; retVal = aMB1340C.SETINTEGRATION(hSys, chdat);
            }
            public void SETINTEGRATIONPOWERCYCLES(int setting, int power_cycles)
            {
                API_Name = "SETINTEGRATIONPOWERCYCLES"; retVal = aMB1340C.SETINTEGRATIONPOWERCYCLES(hSys, setting, power_cycles);
            }
            public void SETNPLC(int pin, float nplc)
            {
                API_Name = "SETNPLC"; retVal = aMB1340C.SETNPLC(hSys, pin, nplc);
            }

            public void AM371_USERBWSEL(int pin, int drvCoarseBw, int drvBoostEn, int clmpCoarseBw, int clmpBoostEn)
            {
                API_Name = "AM371_USERBWSEL"; retVal = aMB1340C.AM371_USERBWSEL(hSys, pin, drvCoarseBw, drvBoostEn, clmpCoarseBw, clmpBoostEn);
            }
        }
        public class cClamp
        {
            int hSys;
            public cClamp(int parse)
            {
                hSys = parse;
            }
            public void CLAMPVOLTAGE(int chset, int mVvalue)
            {
                API_Name = "CLAMPVOLTAGE"; retVal = aMB1340C.CLAMPVOLTAGE(hSys, chset, mVvalue);
            }
            public void CLAMPCURRENT(int chset, int nAvalue)
            {
                API_Name = "CLAMPCURRENT"; retVal = aMB1340C.CLAMPCURRENT(hSys, chset, nAvalue);
            }

            public void AM371_CLAMPVOLTAGE(int pin, float volt)
            {
                API_Name = "AM371_CLAMPVOLTAGE"; retVal = aMB1340C.AM371_CLAMPVOLTAGE(hSys, pin, volt);
            }
            public void AM371_CLAMPCURRENT(int pin, float ampere)
            {
                API_Name = "AM371_CLAMPCURRENT"; retVal = aMB1340C.AM371_CLAMPCURRENT(hSys, pin, ampere);
            }
            public void AM371_CLAMPVOLTAGESETVRANGE(int pin, float volt, int vrange)
            {
                API_Name = "AM371_CLAMPVOLTAGESETVRANGE"; retVal = aMB1340C.AM371_CLAMPVOLTAGESETVRANGE(hSys, pin, volt, vrange);
            }
            public void AM371_CLAMPCURRENTSETIRANGE(int pin, float ampere, int irange)
            {
                API_Name = "AM371_CLAMPCURRENTSETIRANGE"; retVal = aMB1340C.AM371_CLAMPCURRENTSETIRANGE(hSys, pin, ampere, irange);
            }
        }
        public class cExtTrig
        {
            int hSys;
            public cExtTrig(int parse)
            {
                hSys = parse;
            }
            public void AM371_EXTTRIGARM_RELEASE(int pin)
            {
                API_Name = "AM371_EXTTRIGARM_RELEASE"; retVal = aMB1340C.AM371_EXTTRIGARM_RELEASE(hSys, pin);
            }
            public int AM371_EXTTRIGARM_READCURRENTARRAY(int pin, int posedge_h, float delay_s, int nsample, float sample_delay_s)
            {
                API_Name = "AM371_EXTTRIGARM_READCURRENTARRAY";
                return aMB1340C.AM371_EXTTRIGARM_READCURRENTARRAY(hSys, pin, posedge_h, delay_s, nsample, sample_delay_s);
            }
            public int AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX(int pin, out int nsample, float[] iarray, out float min, out float max, out float average)
            {
                API_Name = "AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX";
                return aMB1340C.AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX(hSys, pin, out nsample, iarray, out min, out max, out average);
            }
            public void AM330_EXTTRIGARM_READSMUPIN(int measset, out int chdat, int trig_mode, float delay_after_trig_s)
            {
                API_Name = "AM330_EXTTRIGARM_READSMUPIN"; retVal = aMB1340C.AM330_EXTTRIGARM_READSMUPIN(hSys, measset, out chdat, trig_mode, delay_after_trig_s);
            }
            public void AM330_EXTTRIG_RETRIEVEREADSMUPIN(int measset, out int chdat, out int chRead)
            {
                API_Name = "AM330_EXTTRIG_RETRIEVEREADSMUPIN"; retVal = aMB1340C.AM330_EXTTRIG_RETRIEVEREADSMUPIN(hSys, measset, out chdat, out chRead);
            }
            public void AM330_EXTTRIGARM_GETSTATUS(out int armed_h, out int triggered_h, out int timeout_h)
            {
                API_Name = "AM330_EXTTRIGARM_GETSTATUS"; retVal = aMB1340C.AM330_EXTTRIGARM_GETSTATUS(hSys, out armed_h, out triggered_h, out timeout_h);
            }
            public void AM330_EXTTRIGARM_RELEASE()
            {
                API_Name = "AM330_EXTTRIGARM_RELEASE"; retVal = aMB1340C.AM330_EXTTRIGARM_RELEASE(hSys);
            }
        }
        
        private static int retVal
        {
            set
            {
                int mRetVal;
                try
                {
                    mRetVal = value;
                    if (mRetVal != 0)
                        throw new Exception("AM1340c " + API_Name + " \r\nError: " + String.Format("{0:x8}", mRetVal).ToUpper() + "\r\n Detail: " + RaiseError(mRetVal));
                }
                catch (Exception ex)
                {
                    throw new System.Exception(ex.Message);
                }


            }
        }
        static string RaiseError(int ErrorCode)
        {
            string tmpStr;
            tmpStr = "";
            switch (ErrorCode)
            {
                case 0: // Successful
                    break;
                case 1: //Cannot write to device
                    tmpStr = ("Unable to write to device");
                    break;
                case 2: //Cannot read from device
                    tmpStr = ("Unable to read to device");
                    break;
                case 3: //Out of memory
                    tmpStr = ("Device out of memory");
                    break;
                case 4: //Invalid argument(s)
                    tmpStr = ("Invalid argument(s)");
                    break;
                case 5: //Current overdrive
                    tmpStr = ("Current overdrive");
                    break;
                case 6: //Timed out
                    tmpStr = ("Device Timed Out");
                    break;
                case 7: //DirectAPI does not support this tester
                    tmpStr = ("Unsupportrf Direct API call");
                    break;
                case 8: //Temperature overheat
                    tmpStr = ("Device Temperature overheat");
                    break;
                default:
                    break;
            }
            return (tmpStr);
        }
    }

    abstract class aMB1340C
    {
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_READSMUPIN")]
        public static extern int AM330_EXTTRIGARM_READSMUPIN(int hSys, int measset, out int chdat, int trig_mode, float delay_after_trig_s);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIG_RETRIEVEREADSMUPIN")]
        public static extern int AM330_EXTTRIG_RETRIEVEREADSMUPIN(int hSys, int measset, out int chdat, out int chRead);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_GETSTATUS")]
        public static extern int AM330_EXTTRIGARM_GETSTATUS(int hSys, out int armed_h, out int triggered_h, out int timeout_h);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_RELEASE")]
        public static extern int AM330_EXTTRIGARM_RELEASE(int hSys);


        [DllImport("AMB1340C.dll", EntryPoint = "INITIALIZE")]
        public static extern int INITIALIZE(int hSys);
        [DllImport("AMB1340C.dll", EntryPoint = "RESETBOARDS")]
        public static extern int RESETBOARDS();

        [DllImport("AMB1340C.dll", EntryPoint = "CREATESESSION")]
        public static extern int CREATESESSION(string hostname, out int session);
        [DllImport("AMB1340C.dll", EntryPoint = "CLOSESESSION")]
        public static extern int CLOSESESSION(int session);
        [DllImport("AMB1340C.dll", EntryPoint = "AMB1340C_CREATEINSTANCE")]
        public static extern int AMB1340C_CREATEINSTANCE(int session, int sysId, int offlinemode, out int hSys);
        [DllImport("AMB1340C.dll", EntryPoint = "AMB1340C_DELETEINSTANCE")]
        public static extern int AMB1340C_DELETEINSTANCE(int hSys);

        [DllImport("AMB1340C.dll", EntryPoint = "DRIVEPORT")]
        public static extern int DRIVEPORT(int hSys, int value);
        [DllImport("AMB1340C.dll", EntryPoint = "DRIVEPIN")]
        public static extern int DRIVEPIN(int hSys, int pin, int value);
        [DllImport("AMB1340C.dll", EntryPoint = "READPORT")]
        public static extern int READPORT(int hSys, out int value);
        [DllImport("AMB1340C.dll", EntryPoint = "READPIN")]
        public static extern int READPIN(int hSys, int pin, out int value);
        [DllImport("AMB1340C.dll", EntryPoint = "SETPORTDIRECTION")]
        public static extern int SETPORTDIRECTION(int hSys, int value);
        [DllImport("AMB1340C.dll", EntryPoint = "SETPINDIRECTION")]
        public static extern int SETPINDIRECTION(int hSys, int pin, int value);
        [DllImport("AMB1340C.dll", EntryPoint = "GETPORTDIRECTION")]
        public static extern int GETPORTDIRECTION(int hSys, out int value);
        [DllImport("AMB1340C.dll", EntryPoint = "GETPINDIRECTION")]
        public static extern int GETPINDIRECTION(int hSys, int pin, out int value);

        [DllImport("AMB1340C.dll", EntryPoint = "DRIVEVOLTAGE")]
        public static extern int DRIVEVOLTAGE(int hSys, int pin, int mVvalue, int sign);
        [DllImport("AMB1340C.dll", EntryPoint = "DRIVECURRENT")]
        public static extern int DRIVECURRENT(int hSys, int pin, int nAvalue, int sign);
        [DllImport("AMB1340C.dll", EntryPoint = "CLAMPVOLTAGE")]
        public static extern int CLAMPVOLTAGE(int hSys, int pin, int mVvalue);
        [DllImport("AMB1340C.dll", EntryPoint = "CLAMPCURRENT")]
        public static extern int CLAMPCURRENT(int hSys, int pin, int nAvalue);

        [DllImport("AMB1340C.dll", EntryPoint = "READVOLTAGE")]
        public static extern int READVOLTAGE(int hSys, int pin, out int mVvalue);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENT")]
        public static extern int READCURRENT(int hSys, int pin, out int nAvalue);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTABS")]
        public static extern int READCURRENTRATE(int hSys, int pin, out int nAvalue);
        [DllImport("AMB1340C.dll", EntryPoint = "READVOLTAGEVOLT")]
        public static extern int READVOLTAGEVOLT(int hSys, int pin, out float volt);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTAMP")]
        public static extern int READCURRENTAMP(int hSys, int pin, out float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTAMPABS")]
        public static extern int READCURRENTAMPRATE(int hSys, int pin, out float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "READVOLTAGEWITHAVERAGE")]
        public static extern int READVOLTAGEWITHAVERAGE(int hSys, int pin, int average, out int average_mV, out int every_mV);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTWITHAVERAGE")]
        public static extern int READCURRENTWITHAVERAGE(int hSys, int pin, int average, out int average_nA, out int every_nA);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTAUTORANGE")]
        public static extern int READCURRENTAUTORANGE(int hSys, int pin, out int nAvalue);
        [DllImport("AMB1340C.dll", EntryPoint = "READCURRENTFROMRANGE")]
        public static extern int READCURRENTFROMRANGE(int hSys, int channel, out int nAvalue);

        [DllImport("AMB1340C.dll", EntryPoint = "ONSMUPIN")]
        public static extern int ONSMUPIN(int hSys, int pin);
        [DllImport("AMB1340C.dll", EntryPoint = "OFFSMUPIN")]
        public static extern int OFFSMUPIN(int hSys, int pin);
        [DllImport("AMB1340C.dll", EntryPoint = "SETANAPINBANDWIDTH")]
        public static extern int SETANAPINBANDWIDTH(int hSys, int pin, int setting);
        [DllImport("AMB1340C.dll", EntryPoint = "SETINTEGRATION")]
        public static extern int SETINTEGRATION(int hSys, int chdat);
        [DllImport("AMB1340C.dll", EntryPoint = "SETINTEGRATIONPOWERCYCLES")]
        public static extern int SETINTEGRATIONPOWERCYCLES(int hSys, int setting, int power_cycles);
        [DllImport("AMB1340C.dll", EntryPoint = "SETNPLC")]
        public static extern int SETNPLC(int hSys, int pin, float nplc);    // 0.0009 ~ 60

        [DllImport("AMB1340C.dll", EntryPoint = "BIASSMUPIN")]
        public static extern int BIASSMUPIN(int hSys, int chset, out int chdat);
        [DllImport("AMB1340C.dll", EntryPoint = "READSMUPIN")]
        public static extern int READSMUPIN(int hSys, int chset, out int chdat, out int chRead);
        [DllImport("AMB1340C.dll", EntryPoint = "READSMUPINABS")]
        public static extern int READSMUPINRATE(int hSys, int chset, out int chdat, out int chRead);
        [DllImport("AMB1340C.dll", EntryPoint = "ONOFFSMUPIN")]
        public static extern int ONOFFSMUPIN(int hSys, int chset, out int chdat);

        [DllImport("AMB1340C.dll", EntryPoint = "ARMREADSMUPIN")]
        public static extern int ARMREADSMUPIN(int hSys, int measset, out int chdat);
        [DllImport("AMB1340C.dll", EntryPoint = "RETRIEVEREADSMUPIN")]
        public static extern int RETRIEVEREADSMUPIN(int hSys, int measset, out int chdat, out int chRead);


        [DllImport("AMB1340C.dll", EntryPoint = "SOURCEDELAYMEASURESMUPIN")]
        public static extern int SOURCEDELAYMEASURESMUPIN(int hSys, int chset, out int chdat, out int chRead, int sequence);
        [DllImport("AMB1340C.dll", EntryPoint = "AMB1340C_SOURCEDELAYMEASURESMUPIN")]
        public static extern int AMB1340C_SOURCEDELAYMEASURESMUPIN(int hSys, int pinset, out float pindat, out int measset, out float pinRead, int sequence);

        [DllImport("AMB1340C.dll", EntryPoint = "AM330_DRIVEPULSEVOLTAGE")]
        public static extern int AM330_DRIVEPULSEVOLTAGE(int hSys, int pin, float _base, float pulse, float pulse_s, float hold_s, int dr_vrange, int cycles, int meas_ch, int meas_sel, int meas_vrange, int trig_percentage, int arm_ext_trigin_h, float timeout_s);

        [DllImport("AMB1340C.dll", EntryPoint = "AM371_DRIVEVOLTAGE")]
        public static extern int AM371_DRIVEVOLTAGE(int hSys, int pin, float volt);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_DRIVEVOLTAGESETVRANGE")]
        public static extern int AM371_DRIVEVOLTAGESETVRANGE(int hSys, int pin, float volt, int vrange);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_DRIVECURRENT")]
        public static extern int AM371_DRIVECURRENT(int hSys, int pin, float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_DRIVECURRENTSETIRANGE")]
        public static extern int AM371_DRIVECURRENTSETIRANGE(int hSys, int pin, float ampere, int irange);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_CLAMPVOLTAGE")]
        public static extern int AM371_CLAMPVOLTAGE(int hSys, int pin, float volt);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_CLAMPVOLTAGESETVRANGE")]
        public static extern int AM371_CLAMPVOLTAGESETVRANGE(int hSys, int pin, float volt, int vrange);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_CLAMPCURRENT")]
        public static extern int AM371_CLAMPCURRENT(int hSys, int pin, float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_CLAMPCURRENTSETIRANGE")]
        public static extern int AM371_CLAMPCURRENTSETIRANGE(int hSys, int pin, float ampere, int irange);

        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READVOLTAGE")]
        public static extern int AM371_READVOLTAGE(int hSys, int pin, out float volt);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READVOLTAGEGETVRANGE")]
        public static extern int AM371_READVOLTAGEGETVRANGE(int hSys, int pin, out float volt, out int vrange);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READCURRENT")]
        public static extern int AM371_READCURRENT(int hSys, int pin, out float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READCURRENTABS")]
        public static extern int AM371_READCURRENTRATE(int hSys, int pin, out float ampere);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READCURRENTGETIRANGE")]
        public static extern int AM371_READCURRENTGETIRANGE(int hSys, int pin, out float ampere, out int irange);

        [DllImport("AMB1340C.dll", EntryPoint = "AM371_ONSMUPIN")]
        public static extern int AM371_ONSMUPIN(int hSys, int pin, int remote_sense_h);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_OFFSMUPIN")]
        public static extern int AM371_OFFSMUPIN(int hSys, int pin);

        [DllImport("AMB1340C.dll", EntryPoint = "AM371_EXTTRIGARM_READCURRENTARRAY")]
        public static extern int AM371_EXTTRIGARM_READCURRENTARRAY(int hSys, int pin, int posedge_h, float delay_s, int nsample, float sample_delay_s);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX")]
        public static extern int AM371_EXTTRIGGET_READCURRENTARRAY_WITH_MINMAX(int hSys, int pin, out int nsample, [MarshalAs(UnmanagedType.LPArray)] float[] iarray, out float min, out float max, out float average);

        [DllImport("AMB1340C.dll", EntryPoint = "AM371_EXTTRIGARM_RELEASE")]
        public static extern int AM371_EXTTRIGARM_RELEASE(int hSys, int pin);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_USERBWSEL")]
        public static extern int AM371_USERBWSEL(int hSys, int pin, int drvCoarseBw, int drvBoostEn, int clmpCoarseBw, int clmpBoostEn);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READCURRENT10X")]
        public static extern int AM371_READCURRENT10X(int hSys, int pin, out float Avalue);
        [DllImport("AMB1340C.dll", EntryPoint = "AM371_READCURRENT10XABS")]
        public static extern int AM371_READCURRENT10XRATE(int hSys, int pin, out float Avalue);

        [DllImport("AMB1340C.dll", EntryPoint = "WLF_SETVOLTAGELEVEL")]
        public static extern int WLF_SETVOLTAGELEVEL(int hSys, int switch_num, int setting);
        [DllImport("AMB1340C.dll", EntryPoint = "WLF_DRIVESINGLESWITCH")]
        public static extern int WLF_DRIVESINGLESWITCH(int hSys, int switch_num, int val);
        [DllImport("AMB1340C.dll", EntryPoint = "WLF_DRIVEALLSWITCH")]
        public static extern int WLF_DRIVEALLSWITCH(int hSys, int val);

    }
}
