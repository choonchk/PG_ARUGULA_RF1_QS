using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClothoLibStandard
{
    public class Aemulus_AM330
    {
        int mRetVal, session, hSys;
        string API_Name = "";
        public void INITIALIZE()
        {
            API_Name = "INITIALIZE"; retVal = aMB1340C.INITIALIZE(hSys);
        }
        public void RESETBOARDS()
        {
            API_Name = "RESETBOARDS"; retVal = aMB1340C.RESETBOARDS();
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
        }
        public void AMB1340C_DELETEINSTANCE()
        {
            API_Name = "AMB1340C_DELETEINSTANCE"; retVal = aMB1340C.AMB1340C_DELETEINSTANCE(hSys);
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

        public void DRIVEVOLTAGE(int chset, int mVvalue, int sign)
        {
            API_Name = "DRIVEVOLTAGE"; retVal = aMB1340C.DRIVEVOLTAGE(hSys, chset, mVvalue, sign);
        }
        public void DRIVECURRENT(int chset, int nAvalue, int sign)
        {
            API_Name = "DRIVECURRENT"; retVal = aMB1340C.DRIVECURRENT(hSys, chset, nAvalue, sign);
        }
        public void CLAMPVOLTAGE(int chset, int mVvalue)
        {
            API_Name = "CLAMPVOLTAGE"; retVal = aMB1340C.CLAMPVOLTAGE(hSys, chset, mVvalue);
        }
        public void CLAMPCURRENT(int chset, int nAvalue)
        {
            API_Name = "CLAMPCURRENT"; retVal = aMB1340C.CLAMPCURRENT(hSys, chset, nAvalue);
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

        public void ONSMUPIN(int pin)
        {
            API_Name = "ONSMUPIN"; retVal = aMB1340C.ONSMUPIN(hSys, pin);
        }
        public void OFFSMUPIN(int pin)
        {
            API_Name = "OFFSMUPIN"; retVal = aMB1340C.OFFSMUPIN(hSys, pin);
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

        public void BIASSMUPIN(int chset, out int chdat)
        {
            API_Name = "BIASSMUPIN"; retVal = aMB1340C.BIASSMUPIN(hSys, chset, out chdat);
        }
        public void READSMUPIN(int chset, out int chdat, out int chRead)
        {
            API_Name = "READSMUPIN"; retVal = aMB1340C.READSMUPIN(hSys, chset, out chdat, out chRead);
        }

        //Modified by ChoonChin for IccCal
        public int READSMUPIN_int(int chset, out int chdat, out int chRead)
        {
            API_Name = "READSMUPIN"; int a  = aMB1340C.READSMUPIN(hSys, chset, out chdat, out chRead);
            return a;
        }

        public void READSMUPINRATE(int chset, out int chdat, out int chRead)
        {
            API_Name = "READSMUPINRATE"; retVal = aMB1340C.READSMUPINRATE(hSys, chset, out chdat, out chRead);
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


        public void AM330_DRIVEPULSEVOLTAGE(int pin, float _base, float pulse, float pulse_s, float hold_s,
            int dr_vrange, int cycles, int meas_ch, int meas_sel, int meas_vrange, int trig_percentage,
            int arm_ext_trigin_h, float timeout_s)
        {
            API_Name = "AM330_DRIVEPULSEVOLTAGE"; retVal = aMB1340C.AM330_DRIVEPULSEVOLTAGE(hSys, pin, _base, pulse, pulse_s, hold_s, dr_vrange, cycles, meas_ch, meas_sel, meas_vrange, trig_percentage, arm_ext_trigin_h, timeout_s);
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

        public void AM371_ONSMUPIN(int pin, int remote_sense_h)
        {
            API_Name = "AM371_ONSMUPIN"; retVal = aMB1340C.AM371_ONSMUPIN(hSys, pin, remote_sense_h);
        }
        public void AM371_OFFSMUPIN(int pin)
        {
            API_Name = "AM371_OFFSMUPIN"; retVal = aMB1340C.AM371_OFFSMUPIN(hSys, pin);
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

        public void AM371_EXTTRIGARM_RELEASE(int pin)
        {
            API_Name = "AM371_EXTTRIGARM_RELEASE"; retVal = aMB1340C.AM371_EXTTRIGARM_RELEASE(hSys, pin);
        }

        public void AM371_USERBWSEL(int pin, int drvCoarseBw, int drvBoostEn, int clmpCoarseBw, int clmpBoostEn)
        {
            API_Name = "AM371_USERBWSEL"; retVal = aMB1340C.AM371_USERBWSEL(hSys, pin, drvCoarseBw, drvBoostEn, clmpCoarseBw, clmpBoostEn);
        }
        public void AM371_READCURRENT10X(int pin, out float Avalue)
        {
            API_Name = "AM371_READCURRENT10X"; retVal = aMB1340C.AM371_READCURRENT10X(hSys, pin, out Avalue);
        }
        public void AM371_READCURRENT10XRATE(int pin, out float Avalue)
        {
            API_Name = "AM371_READCURRENT10XRATE"; retVal = aMB1340C.AM371_READCURRENT10XRATE(hSys, pin, out Avalue);
        }

        public void AM330_EXTTRIGARM_READSMUPIN(int measset, out int chdat, int trig_mode, float delay_after_trig_s)
        {
            API_Name = "AM330_EXTTRIGARM_READSMUPIN"; retVal = aMB1340C.AM330_EXTTRIGARM_READSMUPIN(hSys, measset, out chdat, trig_mode, delay_after_trig_s);
        }
        public int AM330_EXTTRIGARM_RETRIEVEREADSMUPIN(int measset, out int chdat, out int chRead)
        {
            API_Name = "AM330_EXTTRIGARM_RETRIEVEREADSMUPIN";
            return aMB1340C.AM330_EXTTRIGARM_RETRIEVEREADSMUPIN(hSys, measset, out chdat, out chRead);
        }
        public void AM330_EXTTRIGARM_GETSTATUS(out int armed_h, out int triggered_h, out int timeout_h)
        {
            API_Name = "AM330_EXTTRIGARM_GETSTATUS"; retVal = aMB1340C.AM330_EXTTRIGARM_GETSTATUS(hSys, out armed_h, out triggered_h, out timeout_h);
        }
        public void AM330_EXTTRIGARM_RELEASE()
        {
            API_Name = "AM330_EXTTRIGARM_RELEASE"; retVal = aMB1340C.AM330_EXTTRIGARM_RELEASE(hSys);
        }
        public void AM330_EXTTRIGARM_SETTIMEOUTLIMIT(float timeout_s)
        {
            API_Name = "AM330_EXTTRIGARM_SETTIMEOUTLIMIT"; retVal = aMB1340C.AM330_EXTTRIGARM_SETTIMEOUTLIMIT(hSys, timeout_s);
        }

        private int retVal
        {
            set
            {
                try
                {
                    mRetVal = value;
                    if (mRetVal != 0)
                        throw new Exception("AM1340c " + API_Name + " Error: " + String.Format("{0:x8}", mRetVal).ToUpper());
                }
                catch (Exception ex)
                {
                    throw new System.Exception(ex.Message);
                }
                

            }
        }
    }

    abstract class aMB1340C
    {
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_READSMUPIN")]
        public static extern int AM330_EXTTRIGARM_READSMUPIN(int hSys, int measset, out int chdat, int trig_mode, float delay_after_trig_s);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_RETRIEVEREADSMUPIN")]
        public static extern int AM330_EXTTRIGARM_RETRIEVEREADSMUPIN(int hSys, int measset, out int chdat, out int chRead);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_GETSTATUS")]
        public static extern int AM330_EXTTRIGARM_GETSTATUS(int hSys, out int armed_h, out int triggered_h, out int timeout_h);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_RELEASE")]
        public static extern int AM330_EXTTRIGARM_RELEASE(int hSys);
        [DllImport("AMB1340C.dll", EntryPoint = "AM330_EXTTRIGARM_SETTIMEOUTLIMIT")]
        public static extern int AM330_EXTTRIGARM_SETTIMEOUTLIMIT(int hSys, float timeout_s);

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
