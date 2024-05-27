using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace IqWaveform
{
    public static class IQ
    {
        public static Dictionary<string, Waveform> Mem = new Dictionary<string, Waveform>();
        public static string BasePath;

        public static bool Load(string ModStd, string WvfrmName, bool Model)
        {
            lock (Mem)
            {
                if ((ModStd == null | WvfrmName == null) || (ModStd == "" & WvfrmName == "")) return true;

                ModStd = ModStd.ToUpper();
                WvfrmName = WvfrmName.ToUpper();

                if (IQ.Mem.ContainsKey(ModStd + WvfrmName))
                {
                    return true;
                }

                string FullPath = IQ.BasePath + ModStd.ToUpper() + "\\" + WvfrmName.ToUpper() + "\\";
                string Ifile = "";
                string Qfile = "";
                List<double> Markers = new List<double>();
                double SubsetStartSeconds = 0;
                double SubsetLengthSeconds = 0;
                decimal VsgIQrate = 0;
                bool IsBursted = false;
                double ServoWindowLengthSec = 0;
                double IntialServoMeasTime = 0;
                double FinalServoMeasTime = 0;
                double RefChBW = 0;
                double AcqRate = 0;
                double AcqDur = 0;
                double WvfrmStartTime = 0;
                double WvfrmTimetoLoad = 0;
                double Rbw = 0;
                string EVM_Type = "";
                ACLRsettings AdjChans = new ACLRsettings();

                switch (ModStd)
                {
                    case "CW":
                        RefChBW = 1e6;
                        AdjChans.Add("ACLR1", 0, 1e3);

                        Ifile = "none";
                        Qfile = "none";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0;
                        SubsetLengthSeconds = 0;
                        VsgIQrate = 7.68e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.050e-3;
                        FinalServoMeasTime = 0.250e-3;
                        AcqRate = 31.25e6;
                        AcqDur = 5e-5;
                        Rbw = 100e3;
                        break;

                    case "CW_SWITCHINGTIME_ON":
                        RefChBW = 100e3;
                        AdjChans.Add("ACLR1", 0, 1e3);

                        Ifile = "none";
                        Qfile = "none";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0;
                        SubsetLengthSeconds = 0;
                        VsgIQrate = 4e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.050e-3;
                        FinalServoMeasTime = 10e-3;
                        Rbw = 0;
                        break;

                    case "CW_SWITCHINGTIME_OFF":
                        RefChBW = 100e3;
                        AdjChans.Add("ACLR1", 0, 1e3);

                        Ifile = "none";
                        Qfile = "none";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0;
                        SubsetLengthSeconds = 0;
                        VsgIQrate = 4e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.050e-3;
                        FinalServoMeasTime = 0.5e-3;
                        Rbw = 0;
                        break;


                    case "PINSWEEP":
                        RefChBW = 0.1e6;

                        AdjChans.Add("ACLR1", 1e6, 1e6);
                        AdjChans.Add("ACLR2", 1e6, 1e6);

                        Ifile = "none";
                        Qfile = "none";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0;
                        SubsetLengthSeconds = 0;
                        VsgIQrate = 10e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.050e-3;
                        FinalServoMeasTime = 0.5e-3;   //0.5e-3;
                        Rbw = 0;
                        break;

                    case "TWOTONE":

                        RefChBW = 100e3;
                        AdjChans.Add("ACLR1", 1e6, 1e6);
                        AdjChans.Add("ACLR2", 1e6, 1e6);

                        Ifile = "TWOTONE_IDATA.txt";
                        Qfile = "TWOTONE_QDATA.txt";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0;
                        SubsetLengthSeconds = 0;
                        VsgIQrate = 10e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.000004;
                        FinalServoMeasTime = 0.000005;
                        Rbw = 0;
                        break;

                    case "LFUQ":
                        #region LFUQ
                        switch (WvfrmName)
                        {
                            case "5M1RB0S": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFUQ_5M1R0S5M_REF.txt";
                                Qfile = "Q_LFUQ_5M1R0S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0.372161458333333e-3;
                                SubsetLengthSeconds = 0.38984375e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.040e-3;
                                FinalServoMeasTime = 0.38984375e-3;
                                Rbw = 10e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M1RB24S": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFUQ_5M1R24S5M_REF.txt";
                                Qfile = "Q_LFUQ_5M1R24S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0.201588541666667e-3;
                                SubsetLengthSeconds = 0.38984375e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.040e-3;
                                FinalServoMeasTime = 0.38984375e-3;
                                Rbw = 10e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M8RB0S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFUQ_5M8R0S6M_REF.txt";
                                Qfile = "Q_LFUQ_5M8R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M8RB17S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFUQ_5M8R17S6M_REF.txt";
                                Qfile = "Q_LFUQ_5M8R17S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M25RB":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFUQ_5M25R0S5M_REF.txt";
                                Qfile = "Q_LFUQ_5M25R0S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB": //CCDF
                            case "10M12RB0S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 7.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 12.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LFUQ_10M12R0S6M_REF.txt";
                                Qfile = "Q_LFUQ_10M12R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 9.47541666666667e-3;
                                SubsetLengthSeconds = 0.269921875000001e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.269921875000001e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB38S": //CCDF
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 7.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 12.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LFUQ_10M12R38S6M_REF.txt";
                                Qfile = "Q_LFUQ_10M12R38S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 9.47541666666667e-3;
                                SubsetLengthSeconds = 0.269921875000001e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.269921875000001e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M50RB": //CCDF
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 7.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 12.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LFUQ_10M50R0S6M_REF.txt";
                                Qfile = "Q_LFUQ_10M50R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0.497161458333333e-3;
                                SubsetLengthSeconds = 0.269921875e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.02e-3;
                                FinalServoMeasTime = 0.269921875e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M16RB0S":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 10e6, 3.84e6);
                                AdjChans.Add("ACLR2", 15e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LFUQ_15M16R0S5M_REF.txt";
                                Qfile = "Q_LFUQ_15M16R0S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M16RB59S":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 10e6, 3.84e6);
                                AdjChans.Add("ACLR2", 15e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LFUQ_15M16R59S5M_REF.txt";
                                Qfile = "Q_LFUQ_15M16R59S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M75RB":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 10e6, 3.84e6);
                                AdjChans.Add("ACLR2", 15e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LFUQ_15M75R0S3M_REF.txt";
                                Qfile = "Q_LFUQ_15M75R0S3M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M16R0R":
                                RefChBW = 30e6;
                                AdjChans.Add("ACLR1", 17.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 22.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.5e6);

                                Ifile = "I_LFUQ_30M16R0R_REF.txt";
                                Qfile = "Q_LFUQ_30M16R0R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.500e-3;
                                VsgIQrate = 46.08e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.500e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M100R50R":
                                RefChBW = 29.9e6;
                                AdjChans.Add("ACLR1", 17.45e6, 3.84e6);
                                AdjChans.Add("ACLR2", 22.45e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 29.9e6, 27.9e6);

                                Ifile = "I_LFUQ_30M100R50R_REF.txt";
                                Qfile = "Q_LFUQ_30M100R50R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.500e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.500e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M75R75R":
                                RefChBW = 30e6;
                                AdjChans.Add("ACLR1", 17.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 22.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.5e6);

                                Ifile = "I_LFUQ_30M75R75R_REF.txt";
                                Qfile = "Q_LFUQ_30M75R75R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.500e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.500e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "40M100RB100RB":
                            case "40M200RB": //CCDF
                                RefChBW = 39.8e6;
                                AdjChans.Add("ACLR1", 22.4e6, 3.84e6);
                                AdjChans.Add("ACLR2", 27.4e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 39.8e6, 37.8e6);

                                Ifile = "I_LFUQ_40M100R100R_REF.txt";
                                Qfile = "Q_LFUQ_40M100R100R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 5.55015625e-3;
                                SubsetLengthSeconds = 0.499983723958333e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.010e-3;
                                FinalServoMeasTime = 0.499983723958333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;

                    #endregion
                    case "LFU1":
                        #region LFU1
                        switch (WvfrmName)
                        {
                            case "5M25RB": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFU1_5M25R0S12M_REF.txt";
                                Qfile = "Q_LFU1_5M25R0S12M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 5.75979166666667e-3;
                                SubsetLengthSeconds = 0.299869791666666e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.299869791666666e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M75RB":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 10e6, 3.84e6);
                                AdjChans.Add("ACLR2", 15e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LFU1_15M75R0S15M_REF.txt";
                                Qfile = "Q_LFU1_15M75R0S15M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;
                    #endregion LFU1
                    case "LFU6":
                        #region LFU6
                        switch (WvfrmName)
                        {
                            case "5M25RB":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LFU6_5M25R0S21M_REF.txt";
                                Qfile = "Q_LFU6_5M25R0S21M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M75RB":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 10e6, 3.84e6);
                                AdjChans.Add("ACLR2", 15e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LFU6_15M75R0S27M_REF.txt";
                                Qfile = "Q_LFU6_15M75R0S27M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.020e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;
                    #endregion LFU6
                    case "LTUQ":
                        #region LTUQ
                        switch (WvfrmName)
                        {
                            case "5M1RB0S": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTUQ_5M1R0S5M_REF.txt";
                                Qfile = "Q_LTUQ_5M1R0S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 7.83348958333333e-3;
                                SubsetLengthSeconds = 0.999869791666666e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.999869791666666e-3;
                                IntialServoMeasTime = 0.010e-3;
                                FinalServoMeasTime = 0.999869791666666e-3;
                                Rbw = 10e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M1RB24S": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTUQ_5M1R24S5M_REF.txt";
                                Qfile = "Q_LTUQ_5M1R24S5M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 7.83348958333333e-3;
                                SubsetLengthSeconds = 0.999869791666666e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.999869791666666e-3;
                                IntialServoMeasTime = 0.010e-3;
                                FinalServoMeasTime = 0.999869791666666e-3;
                                Rbw = 10e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M8RB0S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTUQ_5M8R0S6M_REF.txt";
                                Qfile = "Q_LTUQ_5M8R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M8RB17S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTUQ_5M8R17S6M_REF.txt";
                                Qfile = "Q_LTUQ_5M8R17S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB": //CCDF
                            case "10M1RB0S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTETU_QPSK_10M1RB.txt";
                                Qfile = "Q_LTETU_QPSK_10M1RB.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.825546875e-3;
                                SubsetLengthSeconds = 0.499934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.499934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M1RB49S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "LTUQ_10M1R49S_Idata.txt";
                                Qfile = "LTUQ_10M1R49S_Qdata.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.825546875e-3;
                                SubsetLengthSeconds = 0.499934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.499934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB0S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTUQ_10M12R0S6M_REF.txt";
                                Qfile = "Q_LTUQ_10M12R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.825546875e-3;        //org 2.825546875e-3;
                                SubsetLengthSeconds = 1.2e-3; //org 0.499934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499934895833333e-3; //org 0.499934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 1.2e-3; //org 0.499934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB19S": //CCDF added by HZ
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "LTUQ_10M12R19S_Idata.txt";
                                Qfile = "LTUQ_10M12R19S_Qdata.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 3.03036458333333e-3;
                                SubsetLengthSeconds = 0.499934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.499934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                                
                            case "10M12RB38S": //CCDF
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTUQ_10M12R38S6M_REF.txt";
                                Qfile = "Q_LTUQ_10M12R38S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 3.03036458333333e-3;
                                SubsetLengthSeconds = 0.499934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.499934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M50RB": //CCDF
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTUQ_10M50R0S6M_REF.txt";
                                Qfile = "Q_LTUQ_10M50R0S6M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 3.112265625e-3;
                                SubsetLengthSeconds = 0.399934895833333e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.399934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.399934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "20M100RB": //CCDF
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 12.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 17.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 20e6, 18e6);

                                Ifile = "LTUQ_20M100R2M_Idata.txt";
                                Qfile = "LTUQ_20M100R2M_Qdata.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 3.112265625e-3;
                                SubsetLengthSeconds = 0.399934895833333e-3;
                                VsgIQrate = 30.72e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.399934895833333e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.399934895833333e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "20M100R-20M100R": //CCDF
                                RefChBW = 37.8e6;
                                AdjChans.Add("ACLR1", 20.7e6, 1.28e6);
                                AdjChans.Add("ACLR2", 22.3e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 39.8e6, 37.8e6);

                                Ifile = "I_LTUQ_20M100R_20M100R_V2.txt";
                                Qfile = "Q_LTUQ_20M100R_20M100R_V2.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.5e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 1.50e-3;
                                FinalServoMeasTime = 2.5e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M16R0R":
                                RefChBW = 30e6;
                                AdjChans.Add("ACLR1", 15.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 17.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.5e6);

                                Ifile = "I_LTUQ_30M16R0R_REF.txt";
                                Qfile = "Q_LTUQ_30M16R0R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.5e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M100R50R":
                                RefChBW = 29.9e6;
                                AdjChans.Add("ACLR1", 15.75e6, 1.28e6);
                                AdjChans.Add("ACLR2", 17.35e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 29.9e6, 27.9e6);

                                Ifile = "I_LTUQ_30M100R50R_REF.txt";
                                Qfile = "Q_LTUQ_30M100R50R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.5e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "30M75R75R":
                                RefChBW = 30e6;
                                AdjChans.Add("ACLR1", 15.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 17.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.5e6);

                                Ifile = "I_LTUQ_30M75R75R_REF.txt";
                                Qfile = "Q_LTUQ_30M75R75R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.5e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "35M100RB75RB":
                            case "35M175RB":
                                RefChBW = 34.85e6;
                                AdjChans.Add("ACLR1", 18.225e6, 1.28e6);
                                AdjChans.Add("ACLR2", 19.825e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 34.85e6, 32.85e6);

                                Ifile = "I_LTUQ_35M100R75R_REF.txt";
                                Qfile = "Q_LTUQ_35M100R75R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.50e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.50e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "40M100RB100RB": //CCDF
                            case "40M200RB":
                                RefChBW = 39.8e6;
                                AdjChans.Add("ACLR1", 20.7e6, 1.28e6);
                                AdjChans.Add("ACLR2", 22.3e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 39.8e6, 37.8e6);

                                Ifile = "I_LTUQ_40M100R100R_REF.txt";
                                Qfile = "Q_LTUQ_40M100R100R_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 7.789609375e-3;
                                SubsetLengthSeconds = 0.249983723958334e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.249983723958334e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.249983723958334e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "60M300RB":
                                RefChBW = 59.6e6;
                                AdjChans.Add("ACLR1", 30.6e6, 1.28e6);
                                AdjChans.Add("ACLR2", 32.2e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 59.6e6, 57.6e6);

                                Ifile = "LTUQ_60M300RB_Idata.txt";
                                Qfile = "LTUQ_60M300RB_Qdata.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.25e-3;
                                VsgIQrate = 92.16e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.250e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.250e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;
                    #endregion LTUQ
                    case "LTU1":
                        #region LTU1
                        switch (WvfrmName)
                        {
                            case "5M8RB0S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTU1_5M8R0S20M_REF.txt";
                                Qfile = "Q_LTU1_5M8R0S20M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M8RB17S":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTU1_5M8R17S20M_REF.txt";
                                Qfile = "Q_LTU1_5M8R17S20M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "5M25RB": //CCDF
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTU1_5M25R0S12M_REF.txt";
                                Qfile = "Q_LTU1_5M25R0S12M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 8.49729166666667e-3; //8.49729166666667e-3
                                SubsetLengthSeconds = 0.499869791666667e-3; //0.499869791666667e-3
                                VsgIQrate = 7.68e6m; //7.68e6m
                                IsBursted = true;
                                ServoWindowLengthSec = 0.499869791666667e-3; //0.499869791666667e-3;
                                IntialServoMeasTime = 0.050e-3; //0.050e-3
                                FinalServoMeasTime = 0.499869791666667e-3; //0.499869791666667e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB0S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTU1_10M12R0S20M_REF.txt";
                                Qfile = "Q_LTU1_10M12R0S20M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "10M12RB38S":
                                RefChBW = 9e6;
                                AdjChans.Add("ACLR1", 5.8e6, 1.28e6);
                                AdjChans.Add("ACLR2", 7.4e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 10e6, 9e6);

                                Ifile = "I_LTU1_10M12R38S20M_REF.txt";
                                Qfile = "Q_LTU1_10M12R38S20M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 15.36e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;

                            case "15M75RB":
                                RefChBW = 13.5e6;
                                AdjChans.Add("ACLR1", 8.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 9.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 15e6, 13.5e6);

                                Ifile = "I_LTU1_15M75R0S15M_REF.txt";
                                Qfile = "Q_LTU1_15M75R0S15M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 23.04e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;
                    #endregion LTU1
                    case "LTU6":
                        #region LTU6
                        switch (WvfrmName)
                        {
                            case "5M25RB":
                                RefChBW = 4.5e6;
                                AdjChans.Add("ACLR1", 3.3e6, 1.28e6);
                                AdjChans.Add("ACLR2", 4.9e6, 1.28e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.5e6);

                                Ifile = "I_LTU6_5M25R0S21M_REF.txt";
                                Qfile = "Q_LTU6_5M25R0S21M_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 2.50e-3;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.3e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                EVM_Type = "LTE";
                                break;
                        }
                        break;
                    #endregion LTU6
                    case "WCDMA":
                        #region WCDMA
                        switch (WvfrmName)
                        {
                            case "GTC1": //CCDF
                            case "R99":
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "I_WCDMA_GTC1_REF.txt";
                                Qfile = "Q_WCDMA_GTC1_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 7.39950520833333e-3;
                                SubsetLengthSeconds = 0.269921875e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.269921875e-3;
                                AcqRate = 33.75e6;
                                AcqDur = 1e-4;
                                WvfrmStartTime = 4.0910222222E-03;
                                WvfrmTimetoLoad = 0.9e-3;
                                break;

                            case "GTC1-NEW":
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "I_WCDMA_GTC1_New_20101111.txt";
                                Qfile = "Q_WCDMA_GTC1_New_20101111.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.270e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.250e-3;
                                AcqRate = 33.75e6;
                                AcqDur = 1e-4;
                                WvfrmStartTime = 4.0910222222E-03;
                                WvfrmTimetoLoad = 0.9e-3;
                                Rbw = 100e3;
                                break;
                        }
                        break;
                    #endregion WCDMA
                    case "HSUPA":
                        #region HSUPA
                        switch (WvfrmName)
                        {
                            case "TC1":
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "WCDMA_HSUPA_ST1_100408A_Idata.txt";
                                Qfile = "WCDMA_HSUPA_ST1_100408A_Qdata.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 1e-3; //Modified
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.1e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                break;

                            case "TC2":
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "I_WCDMA_HSUPA_ST2_REF.txt";
                                Qfile = "Q_WCDMA_HSUPA_ST2_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 0.3e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.1e-3;
                                FinalServoMeasTime = 0.3e-3;
                                Rbw = 100e3;
                                break;

                            case "TC3": //CCDF
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "I_WCDMA_HSUPA_ST3_REF.txt";
                                Qfile = "Q_WCDMA_HSUPA_ST3_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 6.504453125e-3;
                                SubsetLengthSeconds = 0.299869791666667e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.1e-3;
                                FinalServoMeasTime = 0.299869791666667e-3;
                                Rbw = 100e3;
                                break;

                            case "TC4": //CCDF
                                RefChBW = 3.84e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);

                                Ifile = "I_WCDMA_HSUPA_ST4_REF.txt";
                                Qfile = "Q_WCDMA_HSUPA_ST4_REF.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0.180885416666667e-3;
                                SubsetLengthSeconds = 0.299869791666667e-3;
                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.1e-3;
                                FinalServoMeasTime = 0.299869791666667e-3;
                                Rbw = 100e3;
                                break;
                        }
                        break;
                    #endregion HSUPA
                    case "TDSCDMA":
                        #region TDSCDMA
                        //RefChBW = 1.28e6;

                        //switch (WvfrmName)
                        //{
                        //    case "TS1": //CCDF
                        RefChBW = 1.28e6;
                        AdjChans.Add("ACLR1", 1.6e6, 1.28e6);
                        AdjChans.Add("ACLR2", 3.2e6, 1.28e6);

                        Ifile = "I_TDSCDMA_TS1_1P28MHZ_REF.txt";
                        Qfile = "Q_TDSCDMA_TS1_1P28MHZ_REF.txt";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 0.909775390625e-3;
                        SubsetLengthSeconds = 0.499951171875e-3;
                        VsgIQrate = 20.48e6m;
                        IsBursted = true;
                        ServoWindowLengthSec = 0.499951171875e-3;
                        IntialServoMeasTime = 0.1e-3;
                        FinalServoMeasTime = 0.499951171875e-3;
                        Rbw = 100e3;
                        //        break;
                        //}
                        break;
                    #endregion TDSCDMA
                    case "GSM":
                        #region GSM
                        switch (WvfrmName)
                        {
                            case "TS01":
                                RefChBW = 300e3;
                                AdjChans.Add("ORFS1", 200e3, 30e3);
                                AdjChans.Add("ORFS2", 250e3, 30e3);
                                AdjChans.Add("ORFS3", 400e3, 30e3);
                                AdjChans.Add("ORFS4", 600e3, 30e3);
                                AdjChans.Add("ORFS5", 1.2e6, 30e3);
                                AdjChans.Add("ORFS6", 1.8e6, 100e3);
                                AdjChans.Add("ORFS7", 6.0e6, 100e3);

                                Ifile = "I_GSM_TIMESLOT01_20100107.txt";
                                Qfile = "Q_GSM_TIMESLOT01_20100107.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 2e-3;
                                VsgIQrate = 1.083333e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.657e-3;
                                Rbw = 6.7e3;
                                break;
                        }
                        break;
                    #endregion
                    case "CDMA2K":
                        #region CDMA2K
                        RefChBW = 1.28e6;

                        switch (WvfrmName)
                        {
                            case "RC1":
                                RefChBW = 1e6;
                                AdjChans.Add("ACLR1", 1.25e6, 30e3);
                                AdjChans.Add("ACLR2", 1.98e6, 30e3);

                                Ifile = "I_CDMA2K_RC1_20100316.txt";
                                Qfile = "Q_CDMA2K_RC1_20100316.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 1.422e-3;
                                SubsetLengthSeconds = 0.35e-3;
                                VsgIQrate = 4.9152e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.162e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.162e-3;
                                Rbw = 3e3;
                                break;
                        }
                        break;
                    #endregion CDMA2K
                    case "EVDO":
                        #region EVDO
                        //RefChBW = 1.23e6;

                        //switch (WvfrmName)
                        //{
                        //    case "TC6": //CCDF
                        RefChBW = 1.23e6;
                        AdjChans.Add("ACLR1", 1.25e6, 30e3);
                        AdjChans.Add("ACLR2", 1.98e6, 30e3);

                        Ifile = "I_1XEVDO_REVA_TR4096_REF.txt";
                        Qfile = "Q_1XEVDO_REVA_TR4096_REF.txt";
                        Markers = new List<double> { 0 };
                        SubsetStartSeconds = 46.8121028645833e-3;
                        SubsetLengthSeconds = 1.5997314453125e-3;
                        VsgIQrate = 4.9152e6m;
                        IsBursted = false;
                        ServoWindowLengthSec = 0;
                        IntialServoMeasTime = 0.01e-3;
                        FinalServoMeasTime = 1.5997314453125e-3;
                        Rbw = 3e3;
                        //        break;
                        //}
                        break;
                    #endregion
                    case "EDGE":
                        #region EDGE
                        switch (WvfrmName)
                        {
                            case "TS01":
                                RefChBW = 300e3;
                                AdjChans.Add("ORFS1", 200e3, 30e3);
                                AdjChans.Add("ORFS2", 250e3, 30e3);
                                AdjChans.Add("ORFS3", 400e3, 30e3);
                                AdjChans.Add("ORFS4", 600e3, 30e3);
                                AdjChans.Add("ORFS5", 1.2e6, 30e3);
                                AdjChans.Add("ORFS6", 1.8e6, 100e3);
                                AdjChans.Add("ORFS7", 6.0e6, 100e3);

                                Ifile = "I_EDGE_TS01_20100107.txt";
                                Qfile = "Q_EDGE_TS01_20100107.txt";
                                Markers = new List<double> { 0 };
                                SubsetStartSeconds = 0;
                                SubsetLengthSeconds = 2e-3;
                                VsgIQrate = 1.083333e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.5e-3;
                                IntialServoMeasTime = 0.050e-3;
                                FinalServoMeasTime = 0.657e-3;
                                Rbw = 6.7e3;
                                break;
                        }
                        break;
                    #endregion EDGE

                    case "NR":
                        #region NR //for debugging
                        switch (WvfrmName)
                        {
                            case "NCFU2_SC30B100M273R0S1X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);


                                Ifile = "I_NCFU2_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 4325.2197e-6;
                                SubsetLengthSeconds = 589.9902e-6;

                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 589.9902e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFU2_SC30B100M273R0S1X_EVM":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);


                                Ifile = "I_NCFU2_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.01;// 589.9902e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 589.9902e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFUQ_SC30B100M273R0S1X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);


                                Ifile = "I_NCFUQ_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 2213.1591e-6;
                                SubsetLengthSeconds = 574.9919e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 574.9919e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFU2_SC30B80M217R0S1X":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);

                                Ifile = "I_NCFU2_SC30B80M217R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B80M217R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 6243.0908e-6;
                                SubsetLengthSeconds = 379.9886e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 379.9886e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFU2_SC30B80M217R0S1X_EVM":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);

                                Ifile = "I_NCFU2_SC30B80M217R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B80M217R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 6243.0908e-6;
                                SubsetLengthSeconds = 0.01;// 379.9886e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 379.9886e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFUQ_SC30B80M217R0S1X":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);

                                Ifile = "I_NCFUQ_SC30B80M217R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC30B80M217R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 3224.1455e-6;
                                SubsetLengthSeconds = 474.9918e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 474.9918e-6;
                                EVM_Type = "NR";
                                break;
                            case "NCFUQ_SC15B40M216R0S1X":
                                RefChBW = 38.895e6;
                                AdjChans.Add("ACLR1", 22.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 27.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 40e6, 38.895e6);

                                Ifile = "I_NCFUQ_SC15B40M216R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B40M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.0003;
                                FinalServoMeasTime = 0.0003;
                                EVM_Type = "NR";
                                break;
                            case "NDFU2_SC30B100M270R0S1X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDFU2_SC30B100M270R0S1X.txt";
                                Qfile = "Q_NDFU2_SC30B100M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 4735.6201e-6;
                                SubsetLengthSeconds = 519.9951e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 519.9951e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFU2_SC30B100M270R0S1X_EVM":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDFU2_SC30B100M270R0S1X.txt";
                                Qfile = "Q_NDFU2_SC30B100M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 4735.6201e-6;
                                SubsetLengthSeconds = 0.01;// 519.9951e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 519.9951e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFUQ_SC30B100M270R0S1X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDFUQ_SC30B100M270R0S1X.txt";
                                Qfile = "Q_NDFUQ_SC30B100M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1978.9307e-6;
                                SubsetLengthSeconds = 529.9886e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 529.9886e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFU2_SC30B80M216R0S1X":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);


                                Ifile = "I_NDFU2_SC30B80M216R0S1X.txt";
                                Qfile = "Q_NDFU2_SC30B80M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 4802.6855e-6;
                                SubsetLengthSeconds = 549.9919e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 549.9919e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFU2_SC30B80M216R0S1X_EVM":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);


                                Ifile = "I_NDFU2_SC30B80M216R0S1X.txt";
                                Qfile = "Q_NDFU2_SC30B80M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 4802.6855e-6;
                                SubsetLengthSeconds = 0.01;// 549.9919e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 549.9919e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFUQ_SC30B80M216R0S1X":
                                RefChBW = 78.15e6;
                                AdjChans.Add("ACLR1", 42.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 47.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 80e6, 78.15e6);
                                else AdjChans.Add("E-ACLR", 0, 78.15e6);


                                Ifile = "I_NDFUQ_SC30B80M216R0S1X.txt";
                                Qfile = "Q_NDFUQ_SC30B80M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 7857.6660e-6;
                                SubsetLengthSeconds = 134.9935e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 134.9935e-6;
                                EVM_Type = "NR";
                                break;
                            case "NDFUQ_SC15B40M216R0S1X":
                                RefChBW = 38.895e6;
                                AdjChans.Add("ACLR1", 22.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 27.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 40e6, 38.895e6);

                                Ifile = "I_NDFUQ_SC15B40M216R0S1X.txt";
                                Qfile = "Q_NDFUQ_SC15B40M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0e-3;
                                SubsetLengthSeconds = 0.5e-3;
                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.0003;
                                FinalServoMeasTime = 0.0003;
                                EVM_Type = "NR";
                                break;

                        }
                        break;
                    #endregion

                    case "NCFUQ":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B5M13R6S1X":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFUQ_SC15B5M13R6S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B5M13R6S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 5023.046875e-6;
                                SubsetLengthSeconds = 0.294921875e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.294921875e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.294921875e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B5M13R6S1XEVM":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFUQ_SC15B5M13R6S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B5M13R6S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 5023.046875e-6;
                                SubsetLengthSeconds = 0.002;// 0.294921875e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.002;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.002;
                                EVM_Type = "NR";
                                break;


                            case "SC15B20M53R27S1X":
                                RefChBW = 19.095e6;
                                AdjChans.Add("ACLR1", 12.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 17.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 20e6, 19.095e6);

                                Ifile = "I_NCFUQ_SC15B20M53R27S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B20M53R27S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 5963.574219e-6;
                                SubsetLengthSeconds = 0.554980469e-3;

                                VsgIQrate = 30.72e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.554980469e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.554980469e-3;
                                EVM_Type = "NR";
                                break;
                            case "SC15B20M53R27S1XEVM":
                                RefChBW = 19.095e6;
                                AdjChans.Add("ACLR1", 12.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 17.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 20e6, 19.095e6);

                                Ifile = "I_NCFUQ_SC15B20M53R27S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B20M53R27S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 5963.574219e-6;
                                SubsetLengthSeconds = 0.002;// 0.554980469e-3;

                                VsgIQrate = 30.72e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.554980469e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.554980469e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B50M270R0S1X":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NCFUQ_SC15B50M270R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B50M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 9503.808594e-6;
                                SubsetLengthSeconds = 0.399983724e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.399983724e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B50M270R0S1XEVM":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NCFUQ_SC15B50M270R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B50M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 9503.808594e-6;
                                SubsetLengthSeconds = 0.002;// 0.399983724e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.399983724e-3;
                                EVM_Type = "NR";
                                break;


                        }
                        #endregion
                        break;

                    case "NCFU1":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B5M25R0S1X":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFU1_SC15B5M25R0S1X.txt";
                                Qfile = "Q_NCFU1_SC15B5M25R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 288.5416667e-6;
                                SubsetLengthSeconds = 0.534895833e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.534895833e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.534895833e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B5M25R0S1XEVM":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFU1_SC15B5M25R0S1X.txt";
                                Qfile = "Q_NCFU1_SC15B5M25R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 288.5416667e-6;
                                SubsetLengthSeconds = 0.002;// 0.534895833e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.002;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.002;
                                EVM_Type = "NR";
                                break;

                                #endregion
                        }

                        break;
                    case "NCFU2":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B20M106R0S1X":
                                RefChBW = 19.095e6;
                                AdjChans.Add("ACLR1", 12.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 17.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 20e6, 19.095e6);

                                Ifile = "I_NCFU2_SC15B20M106R0S1X.txt";
                                Qfile = "Q_NCFU2_SC15B20M106R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 8526.464844e-6;
                                SubsetLengthSeconds = 0.569954427e-3;

                                VsgIQrate = 30.72e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.569954427e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.569954427e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B20M106R0S1XEVM":
                                RefChBW = 19.095e6;
                                AdjChans.Add("ACLR1", 12.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 17.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 20e6, 19.095e6);

                                Ifile = "I_NCFU2_SC15B20M106R0S1X.txt";
                                Qfile = "Q_NCFU2_SC15B20M106R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 8526.464844e-6;
                                SubsetLengthSeconds = 0.002;// 0.569954427e-3;

                                VsgIQrate = 30.72e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.569954427e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.569954427e-3;// 0.569954427e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B40M216R0S1X":
                                RefChBW = 38.895e6;
                                AdjChans.Add("ACLR1", 22.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 27.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 40e6, 38.895e6);

                                Ifile = "I_NCFU2_SC15B40M216R0S1X.txt";
                                Qfile = "Q_NCFU2_SC15B40M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 8129.703776e-6;
                                SubsetLengthSeconds = 0.329980469e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.329980469e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.329980469e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B40M216R0S1XEVM":
                                RefChBW = 38.895e6;
                                AdjChans.Add("ACLR1", 22.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 27.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 40e6, 38.895e6);

                                Ifile = "I_NCFU2_SC15B40M216R0S1X.txt";
                                Qfile = "Q_NCFU2_SC15B40M216R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 8129.703776e-6;
                                SubsetLengthSeconds = 0.002;// 0.329980469e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.329980469e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.329980469e-3;// 0.329980469e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M273R0S1X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCFU2_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1109.277344e-6;
                                SubsetLengthSeconds = 0.409993489e-3;

                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.409993489e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.409993489e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M273R0S1XEVM":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCFU2_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCFU2_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 1109.277344e-6;
                                SubsetLengthSeconds = 0.001;// 0.409993489e-3;

                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.409993489e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.409993489e-3;// 0.409993489e-3;
                                EVM_Type = "NR";
                                break;
                        }
                        break;
                    #endregion
                    case "NCFU6":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B5M25R0S1X":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFU6_SC15B5M25R0S1X.txt";
                                Qfile = "Q_NCFU6_SC15B5M25R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 3210.15625e-6;
                                SubsetLengthSeconds = 0.524869792e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.524869792e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.524869792e-3;
                                EVM_Type = "NR";
                                break;


                            case "SC15B5M25R0S1XEVM":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NCFU6_SC15B5M25R0S1X.txt";
                                Qfile = "Q_NCFU6_SC15B5M25R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 3210.15625e-6;
                                SubsetLengthSeconds = 0.002;// 0.524869792e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.002;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.002;// 0.524869792e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B30M78R0S1X":
                                RefChBW = 28.515e6;
                                AdjChans.Add("ACLR1", 17.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 22.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.515e6);

                                Ifile = "I_NCFU6_SC30B30M78R0S1X.txt";
                                Qfile = "Q_NCFU6_SC30B30M78R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 7135.742188e-6;
                                SubsetLengthSeconds = 0.594986979e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.594986979e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.594986979e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B30M78R0S1XEVM":
                                RefChBW = 28.515e6;
                                AdjChans.Add("ACLR1", 17.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 22.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 30e6, 28.515e6);

                                Ifile = "I_NCFU6_SC30B30M78R0S1X.txt";
                                Qfile = "Q_NCFU6_SC30B30M78R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0;// 7135.742188e-6;
                                SubsetLengthSeconds = 0.001;// 0.594986979e-3;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.594986979e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.594986979e-3;// 0.594986979e-3;
                                EVM_Type = "NR";
                                break;


                        }

                        break;
                    #endregion

                    case "NCTUQ":
                        #region
                        switch (WvfrmName)
                        {
                           
                            case "SC30B100M273R0S3X50CFRPAPR6DB3X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S3X_50_CFR6_3x.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S3X_50_CFR6_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 8.272473e-3;
                                SubsetLengthSeconds = 0.224997e-3;

                                VsgIQrate = 368.64e6m; //61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.224997e-3;
                                IntialServoMeasTime = 8.272473e-3;// 0.001; //0.01e-3
                                FinalServoMeasTime = 0.224997e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M273R0S3XV7CFR7DT10":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S3X_V7_CFR7_DT10.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S3X_V7_CFR7_DT10.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;
                                SubsetLengthSeconds = 0.5e-3;

                                VsgIQrate = 368.64e6m; //61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.224997e-3;
                                IntialServoMeasTime = 8.272473e-3;// 0.001; //0.01e-3
                                FinalServoMeasTime = 0.224997e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M273R0S3XV7CFR7":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S3X_V7_CFR7.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S3X_V7_CFR7.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;
                                SubsetLengthSeconds = 2.49999e-3;

                                VsgIQrate = 368.64e6m; //61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.224997e-3;
                                IntialServoMeasTime = 8.272473e-3;// 0.001; //0.01e-3
                                FinalServoMeasTime = 0.224997e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M273R0S3X50CFRPAPR7DB3X":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S3X_50_CFRPAPR-7dB_3x.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S3X_50_CFRPAPR-7dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 8.272473e-3;
                                SubsetLengthSeconds = 0.224997e-3;

                                VsgIQrate = 368.64e6m; //61.44e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.224997e-3;//0.25e-3;
                                IntialServoMeasTime = 8.272473e-3;//8.25e-3;// 0.0001; //0.01e-3
                                FinalServoMeasTime = 0.224997e-3; //0.1e-3;//0.22497e-3;
                                EVM_Type = "NR";
                                break;

                                case "SC30B100M273R0S3X50CFRPAPR7DB3XEVM":
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S3X_50_CFRPAPR-7dB_3x.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S3X_50_CFRPAPR-7dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.001;
                                SubsetLengthSeconds = 0.0025;

                                VsgIQrate = 368.64e6m; //61.44e6m;				
                                IsBursted = true;
                                ServoWindowLengthSec = 0.0025;
                                IntialServoMeasTime = 0.001; //0.01e-3				
                                FinalServoMeasTime = 0.0025;
                                EVM_Type = "NR";
                                break;



                            case "SC15B5M12R6S1X_50_org":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.002;
                                SubsetLengthSeconds = 0.001;

                                VsgIQrate = 7.68e6m; //61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.00015; //0.01e-3
                                FinalServoMeasTime = 0.00025;
                                EVM_Type = "NR";
                                break;

                            case "SC15B5M12R6S1X_50":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR", 5e6, 4.515e6);
                                //AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                //AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.002;
                                SubsetLengthSeconds = 0.001;

                                VsgIQrate = 7.68e6m; //61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.00015; //0.01e-3
                                FinalServoMeasTime = 0.00025;
                                EVM_Type = "NR";
                                break;

                            case "SC15B5M12R6S13X_50":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S13X_50.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S13X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.002;
                                SubsetLengthSeconds = 0.001;

                                VsgIQrate = 7.68e6m; //61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.00015; //0.01e-3
                                FinalServoMeasTime = 0.00025;
                                EVM_Type = "NR";
                                break;


                            case "SC30B100M273R0S1X": //Mario Debug
                                
                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 4325.2197e-6;// 4325.2197e-6; 0.0025
                                SubsetLengthSeconds = 589.9902e-6;// 589.9902e-6; 0.0035
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 589.9902e-6; // 589.9902e-6 0.0035
                                EVM_Type = "NR";

                                break;


                            case "SC30B100M273R0S1X_50": //Mario Debug

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);  ///AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTUQ_SC30B100M273R0S1X_50.txt";
                                Qfile = "Q_NCTUQ_SC30B100M273R0S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0015; ;// 4325.2197e-6; 0.0015;
                                SubsetLengthSeconds = 0.002; ;// 589.9902e-6; 0.0025;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.0015; //0.01e-3 0.0015;
                                FinalServoMeasTime = 0.002; // 589.9902e-6 0.0025;
                                EVM_Type = "NR";

                                break;
                        }

                        break;
                    #endregion

                    case "NCTU6": // Hz
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B50M270R0S1X":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NCFUQ_SC15B50M270R0S1X.txt";
                                Qfile = "Q_NCFUQ_SC15B50M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0025;
                                SubsetLengthSeconds = 0.0035;

                                VsgIQrate = 61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.0035;
                                EVM_Type = "NR";
                                break;

                        }
                        break;
                        #endregion

                    case "NCTU2":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC30B100M273R0S3X50CFRPAPR95DB3X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTU2_SC30B100M273R0S3X_50_CFRPAPR-9.5dB_3x.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S3X_50_CFRPAPR-9.5dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 8.112299e-3;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.234998e-3;// 589.9902e-6; //0.0045 //0.0025
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.234998e-3; //0.0025
                                IntialServoMeasTime = 0.001; //0.01e-3
                                FinalServoMeasTime = 0.234998e-3; // 589.9902e-6 //0.0035 //0.0025
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M273R0S3XV7CFR95":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTU2_SC30B100M273R0S3X_V7_CFR9.5.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S3X_V7_CFR9.5.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;// 4325.2197e-6;
                                SubsetLengthSeconds = 2.49999e-3;// 589.9902e-6; //0.0045 //0.0025
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.234998e-3; //0.0025
                                IntialServoMeasTime = 0.001; //0.01e-3
                                FinalServoMeasTime = 0.234998e-3; // 589.9902e-6 //0.0035 //0.0025
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M273R0S3XV7CFR95DT10":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTU2_SC30B100M273R0S3X_V7_CFR9.5_DT10.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S3X_V7_CFR9.5_DT10.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.5e-3;// 589.9902e-6; //0.0045 //0.0025
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.234998e-3; //0.0025
                                IntialServoMeasTime = 0.001; //0.01e-3
                                FinalServoMeasTime = 0.234998e-3; // 589.9902e-6 //0.0035 //0.0025
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M273R0S3X50CFRPAPR95DB3XEVM":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);		

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);		
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);		

                                Ifile = "I_NCTU2_SC30B100M273R0S3X_50_CFRPAPR-9.5dB_3x.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S3X_50_CFRPAPR-9.5dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.001;// 4325.2197e-6;		
                                SubsetLengthSeconds = 0.0012;// 589.9902e-6; //0.0045 //0.0025 //org 0.0012
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.0025; //0.0012; //0.0025		//hosein 10192020
                                IntialServoMeasTime = 0.001; //0.01e-3		
                                FinalServoMeasTime = 0.002;//0.0012 // 589.9902e-6 //0.0035 //0.0025 //org 0.0012	
                                EVM_Type = "NR";

                                break;


                            case "SC30B100M273R0S1X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTU2_SC30B100M273R0S1X.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0025;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.0055;// 589.9902e-6; //0.0045
                                VsgIQrate = 122.88e6m; 
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.0025; //0.01e-3
                                FinalServoMeasTime = 0.0055; // 589.9902e-6 //0.0045
                                EVM_Type = "NR";

                                break;


                            case "SC30B100M273R0S1X_50":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);  //AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NCTU2_SC30B100M273R0S1X_50.txt";
                                Qfile = "Q_NCTU2_SC30B100M273R0S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0015;// 4325.2197e-6; //org 0.0015;
                                SubsetLengthSeconds = 0.0018;// 589.9902e-6; //org 0.0025;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime =0.0015; //0.01e-3 //org 0.0015
                                FinalServoMeasTime = 0.0018; // 589.9902e-6 //org 0.0025;
                                EVM_Type = "NR";

                                break;

                        }
                        break;
                    #endregion

                    case "NDTUQ":
                        #region
                        switch (WvfrmName)
                        {
                            case "SC15B5M12R6S3X50CFRPAPR32DB3X":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S3X_50_CFRPAPR-3.2dB_3x.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S3X_50_CFRPAPR-3.2dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 3.663949e-3; //3210.15625e-6;
                                SubsetLengthSeconds = 0.294977e-3;    // 0.524869792e-3;

                                VsgIQrate = 23.04e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.294977e-3;
                                IntialServoMeasTime = 0.002;
                                FinalServoMeasTime = 0.294977e-3;
                                EVM_Type = "NR";
                                break;

                            case "SC15B5M12R6S3X50CFRPAPR32DB3XEVM":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S3X_50_CFRPAPR-3.2dB_3x.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S3X_50_CFRPAPR-3.2dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0025; //3210.15625e-6;
                                SubsetLengthSeconds = 0.0035;    // 0.524869792e-3;

                                VsgIQrate = 23.04e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.0035;
                                IntialServoMeasTime = 0.001;
                                FinalServoMeasTime = 0.0035;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M135R67S3XV5CFR3P2":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M135R67S3X_V5_CFR3.2.txt";
                                Qfile = "Q_NDTUQ_SC30B100M135R67S3X_V5_CFR3.2.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1.5e-3;// 2.603126e-3; 2e-3; 0.9e-3; 
                                SubsetLengthSeconds = 0.001;// 0.144996e-3;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.001; //0.144996e-3;  //hosein 10192020
                                IntialServoMeasTime = 0.0015;//0.001;
                                FinalServoMeasTime = 0.0002; // 0.144996e-3;
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M135R67S3XV7CFR45DT10":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M135R67S3X_V7_CFR4.5_DT10.txt";
                                Qfile = "Q_NDTUQ_SC30B100M135R67S3X_V7_CFR4.5_DT10.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;// 2.603126e-3; 2e-3; 0.9e-3; 
                                SubsetLengthSeconds = 0.0005;// 0.144996e-3;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.001; //0.144996e-3;  //hosein 10192020
                                IntialServoMeasTime = 0.0015;//0.001;
                                FinalServoMeasTime = 0.0002; // 0.144996e-3;
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M135R67S3XV7CFR4P5":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M135R67S3X_V7_CFR4.5.txt";
                                Qfile = "Q_NDTUQ_SC30B100M135R67S3X_V7_CFR4.5.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1e-3;// 2.603126e-3; 2e-3; 0.9e-3; 
                                SubsetLengthSeconds = 2.49999E-3;// 0.144996e-3;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.001; //0.144996e-3;  //hosein 10192020
                                IntialServoMeasTime = 0.0015;//0.001;
                                FinalServoMeasTime = 0.0002; // 0.144996e-3;
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M135R71S3XV5CFR3P2":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M135R71S3X_V5_CFR3.2.txt";
                                Qfile = "Q_NDTUQ_SC30B100M135R71S3X_V5_CFR3.2.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 1.5e-3; //1e-3;// 2.603126e-3; 2e-3; 0.9e-3;
                                SubsetLengthSeconds = 0.001; //0.002;// 0.144996e-3;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = .001;// 0.002;   //hosein 10192020
                                IntialServoMeasTime = .0015;// 0.001;//0.001;
                                FinalServoMeasTime = 0.0002; // 0.144996e-3
                                EVM_Type = "NR";

                                break;

                            case "SC15B5M12R6S1X_50":
                                RefChBW = 4.515e6;
                                AdjChans.Add("ACLR1", 5e6, 3.84e6);
                                //AdjChans.Add("ACLR2", 10e6, 3.84e6);
                                //AdjChans.Add("E-ACLR", 5e6, 4.515e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 3210.15625e-6;
                                SubsetLengthSeconds = 0.524869792e-3;

                                VsgIQrate = 7.68e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.524869792e-3;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.524869792e-3;
                                EVM_Type = "NR";
                                break;

                            // 2020/03/12 Mario Debug for NR ACLR
                            //case "SC15B5M12R6S1X_50":
                            //    RefChBW = 48.615e6;
                            //    AdjChans.Add("ACLR1", 15e6, 14.235e6);  //AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                            //    //AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                            //    //AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                            //    Ifile = "I_NDTUQ_SC15B5M12R6S1X_50.txt";
                            //    Qfile = "Q_NDTUQ_SC15B5M12R6S1X_50.txt";
                            //    Markers = new List<double> { 0 };

                            //    SubsetStartSeconds = 0.002; 
                            //    SubsetLengthSeconds = 0.001;

                            //    VsgIQrate = 7.68e6m; //61.44e6m;
                            //    IsBursted = false;
                            //    ServoWindowLengthSec = 0.399983724e-3;
                            //    IntialServoMeasTime = 0.00015; //0.01e-3
                            //    FinalServoMeasTime = 0.00025;
                            //    EVM_Type = "NR";
                            //    break;

                            case "SC15B5M12R6S13X_50":
                                RefChBW = 48.615e6;
                                AdjChans.Add("ACLR1", 27.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 32.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 50e6, 48.615e6);

                                Ifile = "I_NDTUQ_SC15B5M12R6S13X_50.txt";
                                Qfile = "Q_NDTUQ_SC15B5M12R6S13X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.002;
                                SubsetLengthSeconds = 0.001;

                                VsgIQrate = 7.68e6m; //61.44e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.399983724e-3;
                                IntialServoMeasTime = 0.00015; //0.01e-3
                                FinalServoMeasTime = 0.00025;
                                EVM_Type = "NR";
                                break;

                            case "SC30B100M270R0S1X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M270R0S1X.txt";
                                Qfile = "Q_NDTUQ_SC30B100M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0025;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.0035;// 589.9902e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.01e-3;
                                FinalServoMeasTime = 0.0035; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M270R0S1X_50":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);  //AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M270R0S1X_50.txt";
                                Qfile = "Q_NDTUQ_SC30B100M270R0S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0015;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.002;// 589.9902e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.0015;//0.01e-3
                                FinalServoMeasTime = 0.002; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M270R0S3X50CFRPAPR42DB3X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M270R0S3X_50_CFRPAPR-4.2dB_3x.txt";
                                Qfile = "Q_NDTUQ_SC30B100M270R0S3X_50_CFRPAPR-4.2dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 2.603126e-3;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.144996e-3;// 589.9902e-6;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.144996e-3; ;
                                IntialServoMeasTime = 0.001;//0.01e-3
                                FinalServoMeasTime = 0.144996e-3; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M270R0S3X50CFRPAPR42DB3XEVM":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTUQ_SC30B100M270R0S3X_50_CFRPAPR-4.2dB_3x.txt";
                                Qfile = "Q_NDTUQ_SC30B100M270R0S3X_50_CFRPAPR-4.2dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.001;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.0025;// 589.9902e-6;
                                VsgIQrate = 368.64e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0.0025;
                                IntialServoMeasTime = 0.001;//0.01e-3
                                FinalServoMeasTime = 0.0025; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                        }
                        break;
                    #endregion

                    case "NDTU2":
                        #region
                        switch (WvfrmName)
                        {


                            case "SC30B100M270R0S3X50CFRPAPR71DB3X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTU2_SC30B100M270R0S3X_50_CFRPAPR-7.1dB_3x.txt";
                                Qfile = "Q_NDTU2_SC30B100M270R0S3X_50_CFRPAPR-7.1dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 7.806438e-3;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.284998e-3;// 589.9902e-6;
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = 0.284998e-3;
                                IntialServoMeasTime = 0.001;  //0.01e-3
                                FinalServoMeasTime = 0.284998e-3; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M270R0S3X50CFRPAPR71DB3XEVM":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);
                                AdjChans.Add("E-ACLR", 100e6, 98.31e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTU2_SC30B100M270R0S3X_50_CFRPAPR-7.1dB_3x.txt";
                                Qfile = "Q_NDTU2_SC30B100M270R0S3X_50_CFRPAPR-7.1dB_3x.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.001;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.0012;// 589.9902e-6; 0.0025; //org 0.0012
                                VsgIQrate = 368.64e6m;
                                IsBursted = true;
                                ServoWindowLengthSec = .0015;//0.0012; // 0.0025;
                                IntialServoMeasTime = 0.001;  //0.01e-3
                                FinalServoMeasTime = 0.002; // 589.9902e-6 0.0025; //org 0.0012
                                EVM_Type = "NR";

                                break;


                            case "SC30B100M270R0S1X":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTU2_SC30B100M270R0S1X.txt";
                                Qfile = "Q_NDTU2_SC30B100M270R0S1X.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0025;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.002;// 589.9902e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.0025;  //0.01e-3
                                FinalServoMeasTime = 0.002; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                            case "SC30B100M270R0S1X_50":

                                RefChBW = 98.31e6;
                                AdjChans.Add("ACLR1", 52.5e6, 3.84e6);  //AdjChans.Add("ACLR1", 52.5e6, 3.84e6);
                                //AdjChans.Add("ACLR2", 57.5e6, 3.84e6);

                                //if (!Model) AdjChans.Add("E-ACLR", 100e6, 98.31e6);
                                //else AdjChans.Add("E-ACLR", 0, 98.31e6);

                                Ifile = "I_NDTU2_SC30B100M270R0S1X_50.txt";
                                Qfile = "Q_NDTU2_SC30B100M270R0S1X_50.txt";
                                Markers = new List<double> { 0 };

                                SubsetStartSeconds = 0.0015;// 4325.2197e-6;
                                SubsetLengthSeconds = 0.0011;// 589.9902e-6;
                                VsgIQrate = 122.88e6m;
                                IsBursted = false;
                                ServoWindowLengthSec = 0;
                                IntialServoMeasTime = 0.0015; //0.01e-3
                                FinalServoMeasTime = 0.0011; // 589.9902e-6
                                EVM_Type = "NR";

                                break;

                        }
                        break;
                        #endregion
                }
                        if (Ifile == "")
                        {
                            MessageBox.Show("Requested waveform:\n" + ModStd + ", " + WvfrmName + "\n\nIs not yet supported.",
                                "Waveform Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        Mem.Add(ModStd + WvfrmName, new Waveform(
                            ModStd,
                            WvfrmName,
                            Ifile,
                            Qfile,
                            FullPath,
                            Markers,
                            SubsetStartSeconds,
                            SubsetLengthSeconds,
                            VsgIQrate,
                            IsBursted,
                            ServoWindowLengthSec,
                            IntialServoMeasTime,
                            FinalServoMeasTime,
                            RefChBW,
                            AdjChans,
                            AcqRate,
                            AcqDur,
                            WvfrmStartTime,
                            WvfrmTimetoLoad,
                            Rbw,
                            EVM_Type
                            ));

                        return true;
                }
            }
        

        public class Waveform
        {
            #region Parameters
            // Input Parameters, VSG side
            public string ModulationStd;
            public string WaveformName;
            public string Ifile;
            public string Qfile;
            public string BasePath;
            public List<double> Markers;
            double SubsetStartSeconds;
            //double SubsetLengthSeconds; //Origlnal
            public double SubsetLengthSeconds;
            public decimal VsgIQrate;
            public bool IsBursted;
            public double ServoWindowLengthSec;
            public double IntialServoMeasTime;
            public double FinalServoMeasTime;
            public double  AcqRate;
            public double AcqDur;
            public double WvfrmStartTime;
            public double WvfrmTimetoLoad;
            public double Rbw;
            public string EVM_Type;
            // Input Parameters, VSA side
            public double RefChBW;
            public ACLRsettings AclrSettings = new ACLRsettings();

            // Auto Calculated
            public double[] Idata, IdataServo;
            public double[] Qdata, QdataServo;
            public double PAR;
            public string FullName, FullNameServo;
            public string Script, ScriptServo;
            public string ScriptName, ScriptNameServo;
            public string TestNamePrefix;
            public decimal VsaIQrate;
            public long SamplesPerRecord;
            public decimal UTP;

            public double[] fftWindow;
            public double[] PowerBackoff;

            #endregion


            public Waveform(string ModStd, string WvfrmName, string Ifile, string Qfile, string BasePath, List<double> Markers, double SubsetStartSeconds, double SubsetLengthSeconds, decimal IQsampleRate, bool IsBursted, double BurstLengthSec, double IntialServoMeasTime, double FinalServoMeasTime,
                double RefChBW, ACLRsettings AdjChans, double AcqRate, double AcqDur, double WvfrmStartTime, double WvfrmTimetoLoad)
            {
                #region Save Parameters
                this.ModulationStd = ModStd;
                this.WaveformName = WvfrmName;
                this.Ifile = Ifile;
                this.Qfile = Qfile;
                this.BasePath = BasePath;
                this.Markers = Markers;
                this.SubsetStartSeconds = SubsetStartSeconds;
                this.SubsetLengthSeconds = SubsetLengthSeconds;
                this.AcqDur = AcqDur;
                this.AcqRate = AcqRate;
                this.WvfrmStartTime = WvfrmStartTime;
                this.WvfrmTimetoLoad = WvfrmTimetoLoad;
                this.Rbw = Rbw;

                this.VsgIQrate = IQsampleRate;
                this.IsBursted = IsBursted;
                this.ServoWindowLengthSec = BurstLengthSec;
                this.IntialServoMeasTime = IntialServoMeasTime;
                this.FinalServoMeasTime = FinalServoMeasTime;

                this.RefChBW = RefChBW;
                this.AclrSettings = AdjChans;

                this.FullName = (this.ModulationStd + this.WaveformName).Replace("_", "").Replace("-", "") ;
                this.FullNameServo = this.FullName + "servo";
                this.ScriptName = "Gen" + this.FullName;
                this.ScriptNameServo = "Gen" + this.FullNameServo;

                string Subset = "";
                int SubsetStartSample = 0;
                int SubsetStopSample = 0;

                if (SubsetLengthSeconds != 0)
                {
                    SubsetStartSample = (int)(SubsetStartSeconds * (double)VsgIQrate / 4.0) * 4;
                    SubsetStopSample = (int)(SubsetLengthSeconds * (double)VsgIQrate / 4.0) * 4;
                    Subset = SubsetStartSample + "," + SubsetStopSample;
                }
                else
                {
                    Subset = "";
                }

                #endregion

                #region Download / Create Waveforms

                //double[] Idata = new double[8];
                //double[] Qdata = new double[8];

                int startPARcalc = 0;
                int stopPARcalc = 0;

                if (ModStd.Contains("CW"))
                {
                    CreateCW(ref Idata, ref Qdata, this.PAR = 3.0103, 0);
                }
                else if (ModStd == "PINSWEEP")
                {
                    CreateCharWave(ref Idata, ref Qdata);
                }
                else
                {
                    Ifile = BasePath + Ifile;
                    Qfile = BasePath + Qfile;
                    string[] IdataStr = File.ReadAllLines(Ifile);
                    string[] QdataStr = File.ReadAllLines(Qfile);
                    Idata = new double[IdataStr.Length];
                    Qdata = new double[QdataStr.Length];

                    double SumSquares = 0;
                    startPARcalc = (int)(this.Markers[0] * (double)VsgIQrate) + SubsetStartSample;
                    stopPARcalc = startPARcalc + (int)(this.FinalServoMeasTime * (double)this.VsgIQrate) - 1;

                    for (int i = 0; i < IdataStr.Length; i++)
                    {
                        Idata[i] = Convert.ToDouble(IdataStr[i]);
                        Qdata[i] = Convert.ToDouble(QdataStr[i]);

                        if (i >= startPARcalc & i <= stopPARcalc)
                        {
                            double magSquared = Math.Pow(Idata[i], 2.0) + Math.Pow(Qdata[i], 2.0);
                            SumSquares += magSquared;
                        }
                    }
                    this.PAR = -20.0 * Math.Log10(Math.Sqrt(SumSquares / (stopPARcalc - startPARcalc + 1)));  // this equation should not include max magnitude, since max magnitude is always 1.0 (full-scale of baseband bits is 1.0)
                }

                #endregion

                #region CW servo waveforms

                CreateCW(ref IdataServo, ref QdataServo, this.PAR, 0);

                #endregion

                #region Write Waveforms

                string markersStr = "";
                foreach (double marker in Markers)
                {
                    markersStr += (int)(marker * (double)VsgIQrate / 4) * 4 + ",";
                }
                markersStr = markersStr.TrimEnd(',');


                this.Script =
                    "script " + ScriptName +
                        " repeat forever" +
                                " generate " + FullName +
                                (Subset != "" ? (" subset(" + Subset + ")") : "") +
                                " marker0(" + markersStr + ")" +
                                " marker1(" + markersStr + ")" +
                        " end repeat" +
                    " end script";

                this.ScriptServo =
                    "script " + ScriptNameServo +
                        " clear scripttrigger0" +
                        " repeat until scripttrigger0" +
                            " generate " + FullNameServo +
                        " end repeat" +
                        " repeat forever" +
                                " generate " + FullName +
                                (Subset != "" ? (" subset(" + Subset + ")") : "") +
                                " marker0(" + markersStr + ")" +
                                " marker1(" + markersStr + ")" +
                        " end repeat" +
                    " end script";

                #endregion

                #region VSA settings
                SetDsp();

                #endregion
            }

            public Waveform(string ModStd, string WvfrmName, string Ifile, string Qfile, string BasePath, List<double> Markers, double SubsetStartSeconds, double SubsetLengthSeconds, decimal IQsampleRate, bool IsBursted, double BurstLengthSec, double IntialServoMeasTime, double FinalServoMeasTime,
                double RefChBW, ACLRsettings AdjChans, double AcqRate, double AcqDur, double WvfrmStartTime, double WvfrmTimetoLoad, double Rbw, string EVM_Type)
            {
                #region Save Parameters
                this.ModulationStd = ModStd;
                this.WaveformName = WvfrmName;
                this.Ifile = Ifile;
                this.Qfile = Qfile;
                this.BasePath = BasePath;
                this.Markers = Markers;
                this.SubsetStartSeconds = SubsetStartSeconds;
                this.SubsetLengthSeconds = SubsetLengthSeconds;
                this.AcqDur = AcqDur;
                this.AcqRate = AcqRate;
                this.WvfrmStartTime = WvfrmStartTime;
                this.WvfrmTimetoLoad = WvfrmTimetoLoad;
                this.Rbw = Rbw;

                this.EVM_Type = EVM_Type; //for EVM

                this.VsgIQrate = IQsampleRate;
                this.IsBursted = IsBursted;
                this.ServoWindowLengthSec = BurstLengthSec;
                this.IntialServoMeasTime = IntialServoMeasTime;
                this.FinalServoMeasTime = FinalServoMeasTime;

                this.RefChBW = RefChBW;
                this.AclrSettings = AdjChans;

                this.FullName = (this.ModulationStd + this.WaveformName).Replace("_", "").Replace("-", "");
                this.FullNameServo = this.FullName + "servo";
                this.ScriptName = "Gen" + this.FullName;
                this.ScriptNameServo = "Gen" + this.FullNameServo;

                string Subset = "";
                int SubsetStartSample = 0;
                int SubsetStopSample = 0;

                if (SubsetLengthSeconds != 0)
                {
                    SubsetStartSample = (int)(SubsetStartSeconds * (double)VsgIQrate / 4.0) * 4;
                    SubsetStopSample = (int)(SubsetLengthSeconds * (double)VsgIQrate / 4.0) * 4;
                    Subset = SubsetStartSample + "," + SubsetStopSample;
                }
                else
                {
                    Subset = "";
                }

                #endregion

                #region Download / Create Waveforms

                //double[] Idata = new double[8];
                //double[] Qdata = new double[8];

                int startPARcalc = 0;
                int stopPARcalc = 0;

                if (ModStd.Contains("CW"))
                {
                    CreateCW(ref Idata, ref Qdata, this.PAR = 3.0103, 0);
                }
                else if (ModStd == "PINSWEEP")
                {
                    CreateCharWave(ref Idata, ref Qdata);
                }
                else
                {
                    Ifile = BasePath + Ifile;
                    Qfile = BasePath + Qfile;
                    string[] IdataStr = File.ReadAllLines(Ifile);
                    string[] QdataStr = File.ReadAllLines(Qfile);
                    Idata = new double[IdataStr.Length];
                    Qdata = new double[QdataStr.Length];

                    double SumSquares = 0;
                    startPARcalc = (int)(this.Markers[0] * (double)VsgIQrate) + SubsetStartSample;
                    stopPARcalc = startPARcalc + (int)(this.FinalServoMeasTime * (double)this.VsgIQrate) - 1;

                    for (int i = 0; i < IdataStr.Length; i++)
                    {
                        Idata[i] = Convert.ToDouble(IdataStr[i]);
                        Qdata[i] = Convert.ToDouble(QdataStr[i]);

                        if (i >= startPARcalc & i <= stopPARcalc)
                        {
                            double magSquared = Math.Pow(Idata[i], 2.0) + Math.Pow(Qdata[i], 2.0);
                            SumSquares += magSquared;
                        }
                    }
                    this.PAR = -20.0 * Math.Log10(Math.Sqrt(SumSquares / (stopPARcalc - startPARcalc + 1)));  // this equation should not include max magnitude, since max magnitude is always 1.0 (full-scale of baseband bits is 1.0)  //hosein 10192020
                }

                #endregion

                #region CW servo waveforms

                CreateCW(ref IdataServo, ref QdataServo, this.PAR, 0);

                #endregion
                
                #region Write Waveforms

                string markersStr = "";
                foreach (double marker in Markers)
                {
                    markersStr += (int)(marker * (double)VsgIQrate / 4) * 4 + ",";
                }
                markersStr = markersStr.TrimEnd(',');


                this.Script =
                    "script " + ScriptName +
                        " repeat forever" +
                                " generate " + FullName +
                                (Subset != "" ? (" subset(" + Subset + ")") : "") +
                                " marker0(" + markersStr + ")" +
                                " marker1(" + markersStr + ")" +
                        " end repeat" +
                    " end script";

                this.ScriptServo =
                    "script " + ScriptNameServo +
                        " clear scripttrigger0" +
                        " repeat until scripttrigger0" +
                            " generate " + FullNameServo +
                        " end repeat" +
                        " repeat forever" +
                                " generate " + FullName +
                                (Subset != "" ? (" subset(" + Subset + ")") : "") +
                                " marker0(" + markersStr + ")" +
                                " marker1(" + markersStr + ")" +
                        " end repeat" +
                    " end script";

                #endregion

                #region VSA settings
                SetDsp();

                #endregion
            }

            private void CreateCW(ref double[] Idata, ref double[] Qdata, double par, double lengthS)
            {
                int numPoints = (int)Math.Max((lengthS * (double)this.VsgIQrate), 8);
                Idata = new double[numPoints];
                Qdata = new double[numPoints];

                double val = Math.Pow(2.0, -0.5) * Math.Pow(10.0, -par / 20.0);

                for (int i = 0; i < numPoints; i++)
                {
                    Idata[i] = val;
                    Qdata[i] = val;
                }
            }

            private void CreateCharWave(ref double[] Idata, ref double[] Qdata)
            {
                bool isLogSignal = false;
                if (isLogSignal) // Previous Signal Log
                {
                    double UTP = FinalServoMeasTime * 2.0;   // *2 because we only capture 1st half of generated waveform
                    int N = (int)((double)VsgIQrate * UTP);
                    Idata = new double[N];
                    Qdata = new double[N];
                    PowerBackoff = new double[N];

                    double SumSquares = 0;

                    double maxPowerBackoff = -30.0;
                    double maxIQval = Math.Pow(2.0, -0.5);  // this yields a power backoff of 0dB
                    double minIQval = maxIQval * Math.Pow(10.0, maxPowerBackoff / 20.0);  // this yields a power backoff of maxPowerBackoff

                    for (int i = 0; i < N; i++)
                    {
                        //iArrDataForAwg[i] = (-Math.Cos((double)i * 2.0 * Math.PI / (double)pwrPointsPerVccInAwg + Math.PI / 4.0) + 1.0) / 2.0;
                        //qArrDataForAwg[i] = (-Math.Sin((double)i * 2.0 * Math.PI / (double)pwrPointsPerVccInAwg + Math.PI / 4.0) + 1.0) / 2.0;     // ensure we have single tone, not two-tone                
                        //Idata[i] = (-Math.Cos((double)i * 2.0 * Math.PI / (double)N) + 1.0) / 2.0 * 0.5;
                        Idata[i] = (-Math.Cos((double)i / (double)N * 2.0 * Math.PI) - 1) / 2.0 * (maxIQval - minIQval) + maxIQval;
                        Qdata[i] = Idata[i];

                        double magSquared = Math.Pow(Idata[i], 2.0) + Math.Pow(Qdata[i], 2.0);
                        SumSquares += magSquared;

                        PowerBackoff[i] = 20.0 * Math.Log10(Math.Sqrt(2.0) * Idata[i]);

                    }

                    this.PAR = -20.0 * Math.Log10(Math.Sqrt(SumSquares / N));  // this equation should not include max magnitude, since max magnitude is always 1.0 (full-scale of baseband bits is 1.0)

                    this.VsaIQrate = this.VsgIQrate;
                    SamplesPerRecord = N / 2;
                }
                else  //New Signal Linear 
                {
                    double UTP = FinalServoMeasTime * 2.0;   // *2 because we only capture 1st half of generated waveform
                    int N = (int)((double)VsgIQrate * UTP);
                    int Halflength = (int)((double)N / 2);
                    double point = Convert.ToDouble(Halflength);


                    double[] Rising_region = new double[Halflength];
                    double[] Falling_region = new double[Halflength];
                    Idata = new double[N];
                    Qdata = new double[N];

                    double step_point = (1 / point);
                    double scale = 40;
                    double I = Math.Sqrt(Math.Pow(10, scale / 10));

                    PowerBackoff = new double[N];

                    for (int i = 0; i < Halflength; i++)
                    {
                        Rising_region[i] = Math.Pow(I, step_point * (i + 1)) * (1 / I);
                    }

                    Array.Reverse(Rising_region);

                    for (int i = 0; i < Halflength; i++)
                    {
                        Falling_region[i] = Rising_region[i];
                    }

                    Array.Reverse(Rising_region);

                    for (int i = 0; i < N; i++)
                    {
                        Qdata[i] = 0;
                        if (i < Halflength)
                        {
                            Idata[i] = Rising_region[i];
                        }
                        else if (Halflength <= i)
                        {
                            Idata[i] = Falling_region[i - Halflength];
                        }
                        PowerBackoff[i] = 20.0 * Math.Log10(Idata[i]);
                    }

                    double Averege = Idata.Average();
                    double Max = Idata.Max();
                    double RMS = Max - Averege;

                    this.PAR = -20.0 * Math.Log10(Math.Pow(RMS, 2.0));

                    this.VsaIQrate = this.VsgIQrate;
                    SamplesPerRecord = N / 2;
                }
            }

            private void SetDsp()
            {
                if (ModulationStd == "PINSWEEP") return;

                if (ModulationStd.Contains("CW"))
                {
                    VsaIQrate = VsgIQrate;
                    SamplesPerRecord = (int)((double)VsaIQrate * this.FinalServoMeasTime);
                    fftWindow = CreateBlackmanHarrisWindow((int)SamplesPerRecord);
                    return;
                }

                List<double> maxSpan = new List<double>();

                maxSpan.Add(RefChBW * 3.0);   // in case of 3rd harmonic

                for (int i = 0; i < AclrSettings.OffsetHz.Count; i++)
                {
                    maxSpan.Add(AclrSettings.OffsetHz[i] * 2.0 + AclrSettings.BwHz[i]);
                }

                decimal minVsaIQrate = (decimal)maxSpan.Max() + 0.1e6m;

                for (int i = 8; i < 25; i++)
                {
                    SamplesPerRecord = (int)Math.Pow(2, i);
                    VsaIQrate = (decimal)SamplesPerRecord / (decimal)this.FinalServoMeasTime;
                    if (VsaIQrate >= minVsaIQrate) break;
                }

                if (VsaIQrate > 200e6m)
                {
                    VsaIQrate = 200e6m;
                    SamplesPerRecord = (int)((double)VsaIQrate * this.FinalServoMeasTime);
                }

                fftWindow = CreateBlackmanHarrisWindow((int)SamplesPerRecord);

            }

            private double[] CreateBlackmanHarrisWindow(int length)
            {
                const double a0 = 0.35875f;
                const double a1 = 0.48829f;
                const double a2 = 0.14128f;
                const double a3 = 0.01168f;

                double[] window = new double[length];
                double sum1 = 0;

                for (int idx = 0; idx < length; idx++)
                {
                    window[idx] = a0
                        - (a1 * Math.Cos((2.0f * Math.PI * idx) / (length - 1)))
                        + (a2 * Math.Cos((4.0f * Math.PI * idx) / (length - 1)))
                        - (a3 * Math.Cos((6.0f * Math.PI * idx) / (length - 1)));

                    sum1 += (window[idx] * window[idx]);
                }

                sum1 = Math.Sqrt(sum1 / length);

                for (int idx = 0; idx < length; idx++)
                {
                    window[idx] /= sum1;
                }

                return window;
            }

            internal static double ScaleIQto1V(ref double[] iArray, ref double[] qArray)
            {
                // find the max for scaling I and Q to 1.0;
                double maxIQ = 0;
                for (int i = 0; i < iArray.Length; i++)
                {
                    maxIQ = Math.Max(maxIQ, Math.Max(Math.Abs(iArray[i]), Math.Abs(qArray[i])));
                }

                // scale I and Q to 1.0
                for (int i = 0; i < iArray.Length; i++)
                {
                    iArray[i] /= maxIQ;
                    qArray[i] /= maxIQ;
                }

                return maxIQ;
            }

        }
    }

    public class AdjCh
    {
        public string Name;
        public double upperDbc;
        public double lowerDbc;
    }

    public class ACLRsettings
    {
        public List<string> Name = new List<string>();
        public List<double> OffsetHz = new List<double>();
        public List<double> BwHz = new List<double>();

        public void Add(string Name, double OffsetHz, double BwHz)
        {
            this.Name.Add(Name);
            this.OffsetHz.Add(OffsetHz);
            this.BwHz.Add(BwHz);
        }
    }
}
