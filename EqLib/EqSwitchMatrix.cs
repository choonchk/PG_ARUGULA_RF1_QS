﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ClothoLibAlgo;

using NationalInstruments.DAQmx;

namespace EqLib
{
    public class EqSwitchMatrix
    {
        public static EqSwitchMatrixBase Get(string VisaAlias, Rev Revision, bool threadLockActivatePath = false)
        {
            EqSwitchMatrixBase thisSwitchMatrix;

            switch (Revision)
            {
                case Rev.C:
                case Rev.CEX:
                case Rev.R:
                case Rev.F:
                case Rev.E:
                case Rev.O2:
                case Rev.SE:
                case Rev.Y2:
                case Rev.J1:
                case Rev.JM:
                case Rev.Y2D:
                case Rev.Y2DPN:
                case Rev.Y2DNightHawk:
                case Rev.MD_RF1:
                case Rev.EDAM_Modular_RF1:
                case Rev.Modular_RF1_QUADSITE:
                case Rev.NUWA_Modular_RF1_QUADSITE:
                    thisSwitchMatrix = new Avago();
                    break;
                default:
                    thisSwitchMatrix = new None();
                    break;
            }

            thisSwitchMatrix.Revision = Revision;
            thisSwitchMatrix.VisaAlias = VisaAlias;
            thisSwitchMatrix.threadLockActivatePath = threadLockActivatePath;

            thisSwitchMatrix.Initialize();

            return thisSwitchMatrix;
        }

        public abstract class EqSwitchMatrixBase
        {
            public Rev Revision { get; set; }
            public string VisaAlias { get; set; }
            public abstract int Initialize();
            public abstract bool IsDefined(DutPort dutPort, InstrPort instrPort);
            public Dictionary<string, List<Operation>> SwitchPathDic{ get; set; }
            private HashSet<DutPort> failedDevicePorts = new HashSet<DutPort>();
            private object locker_failedDevicePort = new object();
            internal bool threadLockActivatePath;
            private object locker_activatePath = new object();

            private Dictionary.DoubleKey<string, Operation, PortCombo> AllMaps = new Dictionary.DoubleKey<string, Operation, PortCombo>();

            private Dictionary<Operation, bool> MapIsBandSpecific = new Dictionary<Operation, bool>();

            public void DefinePath(Operation operation, DutPort port, InstrPort instrument)
            {
                DefinePath(null, operation, port, instrument);
            }

            public void DefinePath(string band, Operation operation, DutPort port, InstrPort instrument)
            {
                MapIsBandSpecific[operation] = band != null && band != "";

                PortCombo portCombo = new PortCombo(port, instrument);

                if (MapIsBandSpecific[operation])
                {
                    AllMaps[band, operation] = portCombo;
                }
                else
                {
                    AllMaps["", operation] = portCombo;
                }

                if (!IsDefined(port, instrument))
                {
                    MessageBox.Show("Switch Matrix path " + portCombo.instrPort + " to " + portCombo.dutPort + " is not defined");
                }

                //if (SwitchMatrixModel.SwitchPathSettingsDict[port, instrument] == default(Dictionary<int, int>))
                //{
                //    MessageBox.Show("Switch Matrix path " + portCombo.instrPort + " to " + portCombo.dutPort + " is not defined");
                //}
            }

            public int ActivatePath(DutPort dutPort, InstrPort instrPort)
            {
                if (threadLockActivatePath)
                {
                    lock (locker_activatePath)
                    {
                        return ActivatePath(new PortCombo(dutPort, instrPort));
                    }
                }
                else
                {
                    return ActivatePath(new PortCombo(dutPort, instrPort));
                }
            }

            public PortCombo ActivatePath(string band, Operation operation)
            {
                PortCombo SwitchPath = GetPath(band, operation);

                if (SwitchPath != null)
                {
                    if (threadLockActivatePath)
                    {
                        lock (locker_activatePath)
                        {
                            ActivatePath(SwitchPath);
                        }
                    }
                    else
                    {
                        ActivatePath(SwitchPath);
                    }
                }

                return SwitchPath;
            }

            public void ActivatePath(string Ch_band)
            {
                string[] Band = Ch_band.Split('_');
                List<Operation> SwPathLs = SwitchPathDic[Ch_band];

                foreach (Operation Path in SwPathLs)
                {
                    PortCombo SwitchPath = GetPath(Band[1], Path);

                    if (SwitchPath != null)
                    {
                        if (threadLockActivatePath)
                        {
                            lock (locker_activatePath)
                            {
                                ActivatePath(SwitchPath);
                            }
                        }
                        else
                        {
                            ActivatePath(SwitchPath);
                        }
                    }
                }
            }

            public abstract int ActivatePath(PortCombo SwitchPath);
            public abstract int ActivatePath_NonMSB(PortCombo SwitchPath);

            public bool IsFailedDevicePort(string band, Operation operation)
            {
                bool isFailedPort = false;
                PortCombo SwitchPath = GetPath(band, operation);

                if (SwitchPath != null)
                {
                    lock (locker_failedDevicePort)
                    {
                        if (failedDevicePorts.Count > 0 && SwitchPath.dutPort != DutPort.NONE)
                        {
                            if (failedDevicePorts.Contains(SwitchPath.dutPort))
                            {
                                isFailedPort = true;
                            }
                        }
                    }
                }

                return isFailedPort;
            }

            public void AddFailedDevicePort(string band, Operation operation)
            {
                PortCombo SwitchPath = GetPath(band, operation);

                if (SwitchPath != null)
                {
                    lock (locker_failedDevicePort)
                    {
                        if (!failedDevicePorts.Contains(SwitchPath.dutPort))
                        {
                            failedDevicePorts.Add(SwitchPath.dutPort);
                        }
                    }
                }
            }

            public void ClearFailedDevicePort()
            {
                lock (locker_failedDevicePort)
                {
                    failedDevicePorts.Clear();
                }
            }

            public PortCombo GetPath(string band, Operation operation)
            {
                PortCombo SwitchPath = null;

                if (MapIsBandSpecific[operation])
                {
                    SwitchPath = AllMaps[band, operation];
                }
                else
                {
                    SwitchPath = AllMaps["", operation];
                }

                if (SwitchPath == null)   // None is the default value
                {
                    throw new Exception("Switch Matrix Map not defined for " + band + " " + operation);
                }

                return SwitchPath;
            }
        }

        public class Avago : EqSwitchMatrixBase
        {
            private Task digitalWriteTaskP00;
            private Task digitalWriteTaskP01;
            private Task digitalWriteTaskP02;
            private Task digitalWriteTaskP03;
            private Task digitalWriteTaskP04;
            private Task digitalWriteTaskP05;
            private Task digitalWriteTaskP06;
            private Task digitalWriteTaskP07;
            private Task digitalWriteTaskP08;
            private Task digitalWriteTaskP09;
            private Task digitalWriteTaskP10;
            private Task digitalWriteTaskP11;

            // private DigitalSingleChannelWriter[] writerPort;

            private DigitalSingleChannelWriter writerP00;
            private DigitalSingleChannelWriter writerP01;
            private DigitalSingleChannelWriter writerP02;
            private DigitalSingleChannelWriter writerP03;
            private DigitalSingleChannelWriter writerP04;
            private DigitalSingleChannelWriter writerP05;
            private DigitalSingleChannelWriter writerP06;
            private DigitalSingleChannelWriter writerP07;
            private DigitalSingleChannelWriter writerP08;
            private DigitalSingleChannelWriter writerP09;
            private DigitalSingleChannelWriter writerP10;
            private DigitalSingleChannelWriter writerP11;

            private int[] ChannelValue = new int[12];

            public override int Initialize()
            {
                try
                {
                    //digitalWriteTaskPort = new Task();
                    //TaskList = new ArrayList();



                    //for (int i = 0; i < 12; i++)
                    //{
                    //    TaskList.Add(digitalWriteTaskPort);

                    //    //digitalWriteTaskP10.DOChannels.CreateChannel("DIO/port10", "port10",
                    //    //                ChannelLineGrouping.OneChannelForAllLines);
                    //    string tempString01 = "DIO/port" + i.ToString();
                    //    string tempString02 = "port" + i.ToString();
                    //    digitalWriteTaskPort.DOChannels.CreateChannel(tempString01, tempString02, ChannelLineGrouping.OneChannelForAllLines);

                    //    //writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);                    
                    //    writerPort[i] = new DigitalSingleChannelWriter(digitalWriteTaskPort.Stream);
                    //}               

                    digitalWriteTaskP00 = new Task();
                    digitalWriteTaskP01 = new Task();
                    digitalWriteTaskP02 = new Task();
                    digitalWriteTaskP03 = new Task();
                    digitalWriteTaskP04 = new Task();
                    digitalWriteTaskP05 = new Task();
                    digitalWriteTaskP06 = new Task();
                    digitalWriteTaskP07 = new Task();
                    digitalWriteTaskP08 = new Task();
                    digitalWriteTaskP09 = new Task();
                    digitalWriteTaskP10 = new Task();
                    digitalWriteTaskP11 = new Task();

                    digitalWriteTaskP00.DOChannels.CreateChannel(VisaAlias + "/port0", "port0",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP01.DOChannels.CreateChannel(VisaAlias + "/port1", "port1",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP02.DOChannels.CreateChannel(VisaAlias + "/port2", "port2",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP03.DOChannels.CreateChannel(VisaAlias + "/port3", "port3",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP04.DOChannels.CreateChannel(VisaAlias + "/port4", "port4",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP05.DOChannels.CreateChannel(VisaAlias + "/port5", "port5",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP06.DOChannels.CreateChannel(VisaAlias + "/port6", "port6",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP07.DOChannels.CreateChannel(VisaAlias + "/port7", "port7",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP08.DOChannels.CreateChannel(VisaAlias + "/port8", "port8",
                                  ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP09.DOChannels.CreateChannel(VisaAlias + "/port9", "port9",
                                    ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP10.DOChannels.CreateChannel(VisaAlias + "/port10", "port10",
                                  ChannelLineGrouping.OneChannelForAllLines);
                    digitalWriteTaskP11.DOChannels.CreateChannel(VisaAlias + "/port11", "port11",
                                  ChannelLineGrouping.OneChannelForAllLines);

                    writerP00 = new DigitalSingleChannelWriter(digitalWriteTaskP00.Stream);
                    writerP01 = new DigitalSingleChannelWriter(digitalWriteTaskP01.Stream);
                    writerP02 = new DigitalSingleChannelWriter(digitalWriteTaskP02.Stream);
                    writerP03 = new DigitalSingleChannelWriter(digitalWriteTaskP03.Stream);
                    writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);
                    writerP05 = new DigitalSingleChannelWriter(digitalWriteTaskP05.Stream);
                    writerP06 = new DigitalSingleChannelWriter(digitalWriteTaskP06.Stream);
                    writerP07 = new DigitalSingleChannelWriter(digitalWriteTaskP07.Stream);
                    writerP08 = new DigitalSingleChannelWriter(digitalWriteTaskP08.Stream);
                    writerP09 = new DigitalSingleChannelWriter(digitalWriteTaskP09.Stream);
                    writerP10 = new DigitalSingleChannelWriter(digitalWriteTaskP10.Stream);
                    writerP11 = new DigitalSingleChannelWriter(digitalWriteTaskP11.Stream);

                    writerP00.WriteSingleSamplePort(true, 0);
                    writerP01.WriteSingleSamplePort(true, 0);
                    writerP02.WriteSingleSamplePort(true, 0);
                    writerP03.WriteSingleSamplePort(true, 0);
                    writerP04.WriteSingleSamplePort(true, 0);
                    writerP05.WriteSingleSamplePort(true, 0);
                    writerP06.WriteSingleSamplePort(true, 0);
                    writerP07.WriteSingleSamplePort(true, 0);
                    writerP08.WriteSingleSamplePort(true, 0);
                    writerP09.WriteSingleSamplePort(true, 0);
                    writerP10.WriteSingleSamplePort(true, 0);
                    writerP11.WriteSingleSamplePort(true, 0);

                    DefineSwitchSettings();

                    return 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Initialize");
                    return -1;
                }
            }

            public override int ActivatePath(PortCombo SwitchPath)
            {
                try
                {
                    // Clear the channel write need status
                    bool[] ChannelWriteNeeded = new bool[12];
                    int PortNo = 0;
                    int ChNo = 0;

                    if (SwitchPathSettingsDict[SwitchPath.dutPort, SwitchPath.instrPort] == default(Dictionary<int, int>))
                    {
                        MessageBox.Show("Switch Matrix path " + SwitchPath.instrPort + " to " + SwitchPath.dutPort + " is not defined");
                    }

                    foreach (KeyValuePair<int, int> switchSetting in SwitchPathSettingsDict[SwitchPath.dutPort, SwitchPath.instrPort])
                    {
                        int SwitchNo = switchSetting.Key;
                        int SwitchStatus = switchSetting.Value;
                        
                        PortNo = Convert.ToInt32(Math.Truncate(SwitchNo / 8d));                        

                        ChNo = SwitchNo - PortNo * 8;
                        int tempChValue = Convert.ToInt32(Math.Pow(2, ChNo));
                        int tempBitAnd = (ChannelValue[PortNo] & tempChValue);

                        if ((tempBitAnd == tempChValue) && (SwitchStatus == 1))
                        {
                            // ChannelValue[PortNo] += Convert.ToInt32(Math.Pow(2, ChNo) * SwitchStatus[i]);
                        }
                        else if ((tempBitAnd == tempChValue) && (SwitchStatus == 0))
                        {
                            ChannelValue[PortNo] -= tempChValue;
                            ChannelWriteNeeded[PortNo] = true;
                        }
                        else if ((tempBitAnd != tempChValue) && (SwitchStatus == 1))
                        {
                            ChannelValue[PortNo] += tempChValue;
                            ChannelWriteNeeded[PortNo] = true;
                        }
                        else if ((tempBitAnd != tempChValue) && (SwitchStatus == 0))
                        {

                        }
                        else
                        {
                            MessageBox.Show("Error");
                        }
                    }

                    // Channel Write                
                    if (ChannelWriteNeeded[0])
                        writerP00.WriteSingleSamplePort(true, ChannelValue[0]);
                    if (ChannelWriteNeeded[1])
                        writerP01.WriteSingleSamplePort(true, ChannelValue[1]);
                    if (ChannelWriteNeeded[2])
                        writerP02.WriteSingleSamplePort(true, ChannelValue[2]);
                    if (ChannelWriteNeeded[3])
                        writerP03.WriteSingleSamplePort(true, ChannelValue[3]);
                    if (ChannelWriteNeeded[4])
                        writerP04.WriteSingleSamplePort(true, ChannelValue[4]);
                    if (ChannelWriteNeeded[5])
                        writerP05.WriteSingleSamplePort(true, ChannelValue[5]);
                    if (ChannelWriteNeeded[6])
                        writerP06.WriteSingleSamplePort(true, ChannelValue[6]);
                    if (ChannelWriteNeeded[7])
                        writerP07.WriteSingleSamplePort(true, ChannelValue[7]);
                    if (ChannelWriteNeeded[8])
                        writerP08.WriteSingleSamplePort(true, ChannelValue[8]);
                    if (ChannelWriteNeeded[9])
                        writerP09.WriteSingleSamplePort(true, ChannelValue[9]);
                    if (ChannelWriteNeeded[10])
                        writerP10.WriteSingleSamplePort(true, ChannelValue[10]);
                    if (ChannelWriteNeeded[11])
                        writerP11.WriteSingleSamplePort(true, ChannelValue[11]);

                    return 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SetPortPath");
                    return -1;
                }
            }

            public override int ActivatePath_NonMSB(PortCombo SwitchPath)
            {
                try
                {
                    // Clear the channel write need status
                    bool[] ChannelWriteNeeded = new bool[12];
                    int PortNo = 0;
                    int ChNo = 0;

                    if (SwitchPathSettingsDict[SwitchPath.dutPort, SwitchPath.instrPort] == default(Dictionary<int, int>))
                    {
                        MessageBox.Show("Switch Matrix path " + SwitchPath.instrPort + " to " + SwitchPath.dutPort + " is not defined");
                    }

                    foreach (KeyValuePair<int, int> switchSetting in SwitchPathSettingsDict[SwitchPath.dutPort, SwitchPath.instrPort])
                    {
                        int SwitchNo = switchSetting.Key;
                        int SwitchStatus = switchSetting.Value;

                        if (SwitchNo == 48)
                            PortNo = 9;
                        else
                        {
                            PortNo = Convert.ToInt32(Math.Truncate(SwitchNo / 8d));
                        }

                        ChNo = SwitchNo - PortNo * 8;
                        int tempChValue = Convert.ToInt32(Math.Pow(2, ChNo));
                        int tempBitAnd = (ChannelValue[PortNo] & tempChValue);

                        if ((tempBitAnd == tempChValue) && (SwitchStatus == 1))
                        {
                            // ChannelValue[PortNo] += Convert.ToInt32(Math.Pow(2, ChNo) * SwitchStatus[i]);
                        }
                        else if ((tempBitAnd == tempChValue) && (SwitchStatus == 0))
                        {
                            ChannelValue[PortNo] -= tempChValue;
                            ChannelWriteNeeded[PortNo] = true;
                        }
                        else if ((tempBitAnd != tempChValue) && (SwitchStatus == 1))
                        {
                            ChannelValue[PortNo] += tempChValue;
                            ChannelWriteNeeded[PortNo] = true;
                        }
                        else if ((tempBitAnd != tempChValue) && (SwitchStatus == 0))
                        {

                        }
                        else
                        {
                            MessageBox.Show("Error");
                        }




                        //switch (SwitchNo[i])
                        //{
                        //    case 0:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 0) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 1:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 1) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 2:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 2) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 3:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 3) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 4:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 4) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 5:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 5) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 6:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 6) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    case 7:
                        //        ChannelValue[0] += Convert.ToInt32(Math.Pow(2, 7) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[0] = true;
                        //        break;

                        //    // Port 1

                        //    case 8:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 0) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 9:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 1) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 10:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 2) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 11:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 3) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 12:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 4) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 13:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 5) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 14:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 6) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;

                        //    case 15:
                        //        ChannelValue[1] += Convert.ToInt32(Math.Pow(2, 7) * SwitchStatus[i]);
                        //        ChannelWriteNeeded[1] = true;
                        //        break;
                        //}
                    }


                    // Channel Write                
                    if (ChannelWriteNeeded[0])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP00.WriteSingleSamplePort(true, ChannelValue[0]);
                    if (ChannelWriteNeeded[1])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP01.WriteSingleSamplePort(true, ChannelValue[1]);
                    if (ChannelWriteNeeded[2])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP02.WriteSingleSamplePort(true, ChannelValue[2]);
                    if (ChannelWriteNeeded[3])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP03.WriteSingleSamplePort(true, ChannelValue[3]);
                    if (ChannelWriteNeeded[4])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP04.WriteSingleSamplePort(true, ChannelValue[4]);
                    if (ChannelWriteNeeded[5])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP05.WriteSingleSamplePort(true, ChannelValue[5]);

                    if (ChannelWriteNeeded[8])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP08.WriteSingleSamplePort(true, ChannelValue[8]);
                    if (ChannelWriteNeeded[9])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP09.WriteSingleSamplePort(true, ChannelValue[9]);
                    if (ChannelWriteNeeded[10])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP10.WriteSingleSamplePort(true, ChannelValue[10]);
                    if (ChannelWriteNeeded[11])
                        // writerP10.WriteSingleSamplePort(true, portValue);
                        writerP11.WriteSingleSamplePort(true, ChannelValue[11]);

                    return 0;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SetPortPath");
                    return -1;
                }
            }

            public override bool IsDefined(DutPort dutPort, InstrPort instrPort)
            {
                return SwitchPathSettingsDict[dutPort, instrPort] != default(Dictionary<int, int>);
            }

            public Dictionary.DoubleKey<DutPort, InstrPort, Dictionary<int, int>> SwitchPathSettingsDict = new Dictionary.DoubleKey<DutPort, InstrPort, Dictionary<int, int>>();

            private void DefineSwitchSettings()
            {
                Dictionary.DoubleKey<InstrPort, DutPort, string> settingsTemp = new Dictionary.DoubleKey<InstrPort, DutPort, string>();

                switch (Revision)
                {

                    case Rev.Y2D:

                        #region Rev Y2D
                        {
                            #region Switch Matrix Box Rev  Map

                            /////////////// For Joker RF2
                            
                            settingsTemp[InstrPort.N1, DutPort.A2] =  "00:0 02:0 03:0 04:1 05:1 06:0 07:0";                             //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A3] =  "00:0 02:0 03:0 04:1 05:1 06:1 07:0";                             //TX_HB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A12] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:0";                        //TX_MB2_NA
                            settingsTemp[InstrPort.N1, DutPort.A13] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:1";                        //TX_HB2_NA

                            settingsTemp[InstrPort.N2, DutPort.A4] = "08:0 09:1 10:0 12:0 13:0";                                       //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A14] = "08:0 09:1 10:0 12:0 13:1 65:0";                                  //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A15] = "08:0 09:1 10:0 12:0 13:1 65:1";                                  //ANT3_NA

                            settingsTemp[InstrPort.N3, DutPort.A16] = "17:0 66:0";                                                      //OUT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A24] = "15:0 70:0";                                                      //OUT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A18] = "21:0 67:0";                                                      //OUT3_NA
                            settingsTemp[InstrPort.N6, DutPort.A20] = "23:0 68:0";                                                      //OUT4_NA

                            settingsTemp[InstrPort.N3, DutPort.A17] = "17:0 66:1";                                                      //DRX_NA
                            settingsTemp[InstrPort.N4, DutPort.A25] = "15:0 70:1";                                                      //MLB_NA
                            settingsTemp[InstrPort.N5, DutPort.A19] = "21:0 67:1";                                                      //GSM_NA
                            settingsTemp[InstrPort.N6, DutPort.A21] = "23:0 68:1";                                                      //MIMO_NA

                            settingsTemp[InstrPort.NS, DutPort.A4] = "08:0 09:1 10:0 12:1 13:0";                                  //NS_ANT1
                            settingsTemp[InstrPort.NS, DutPort.A14] = "08:0 09:1 10:0 12:1 13:1 65:0";                             //NS_ANT2
                            settingsTemp[InstrPort.NS, DutPort.A15] = "08:0 09:1 10:0 12:1 13:1 65:1";                             //NS_ANT3


                            #endregion Switch Matrix Box Rev Y2D Map
                        }

                        #endregion

                        break;
                    case Rev.Y2DPN:

                        #region Rev Y2DPN
                        {
                            #region Switch Matrix Box Rev  Map

                            /////////////// For Pinot RF2

                            //settingsTemp[InstrPort.N1, DutPort.A2] = "00:0 02:0 03:0 04:1 05:1 06:0 07:0";                              //TX_MB1_NA
                            //settingsTemp[InstrPort.N1, DutPort.A3] = "00:0 02:0 03:0 04:1 05:1 06:1 07:0";                              //TX_HB1_NA
                            //settingsTemp[InstrPort.N1, DutPort.A12] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:0";                        //TX_MB2_NA
                            //settingsTemp[InstrPort.N1, DutPort.A13] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:1";                        //TX_HB2_NA

                            //settingsTemp[InstrPort.N2, DutPort.A4] = "08:0 09:1 10:0 12:0 13:0";                                        //ANT1_NA
                            //settingsTemp[InstrPort.N2, DutPort.A14] = "08:0 09:1 10:0 12:0 13:1 65:0";                                  //ANT2_NA
                            //settingsTemp[InstrPort.N2, DutPort.A15] = "08:0 09:1 10:0 12:0 13:1 65:1";                                  //ANT-UAT_NA
                            //settingsTemp[InstrPort.N2, DutPort.A22] = "08:0 09:1 10:1 12:0 69:0";                                       //LMB_OUT_NA

                            //settingsTemp[InstrPort.N3, DutPort.A16] = "17:0 66:0";                                                      //OUT1_NA
                            //settingsTemp[InstrPort.N4, DutPort.A24] = "15:0 70:0";                                                      //OUT2_NA
                            //settingsTemp[InstrPort.N5, DutPort.A18] = "21:0 67:0";                                                      //OUT3_NA
                            //settingsTemp[InstrPort.N6, DutPort.A20] = "23:0 68:0";                                                      //OUT4_NA

                            //settingsTemp[InstrPort.N3, DutPort.A17] = "17:0 66:1";                                                      //DRX_NA
                            //settingsTemp[InstrPort.N4, DutPort.A25] = "15:0 70:1";                                                      //MLB_NA
                            //settingsTemp[InstrPort.N5, DutPort.A19] = "21:0 67:1";                                                      //MIMO_NA
                            //settingsTemp[InstrPort.N6, DutPort.A21] = "23:0 68:1";                                                      //GSM_NA

                            //settingsTemp[InstrPort.NS, DutPort.A4] = "08:0 09:1 10:0 12:1 13:0";                                  //NS_ANT1
                            //settingsTemp[InstrPort.NS, DutPort.A14] = "08:0 09:1 10:0 12:1 13:1 65:0";                             //NS_ANT2
                            //settingsTemp[InstrPort.NS, DutPort.A15] = "08:0 09:1 10:0 12:1 13:1 65:1";                             //NS_ANT3

                            
                            settingsTemp[InstrPort.N1, DutPort.A12] = "64:0";                               //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A13] = "64:1";                               //TX_HB1_NA

                            settingsTemp[InstrPort.N2, DutPort.A23] = "69:1 71:0";                          //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A14] = "65:0 71:1";                          //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A15] = "65:1 71:1";                          //ANT-UAT_NA
                            settingsTemp[InstrPort.N2, DutPort.A22] = "69:0 71:0";                          //ANT-21

                            settingsTemp[InstrPort.N3, DutPort.A16] = "66:0";                               //OUT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A24] = "70:0";                               //OUT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A18] = "67:0";                               //OUT3_NA
                            settingsTemp[InstrPort.N6, DutPort.A20] = "68:0";                               //OUT4_NA

                            settingsTemp[InstrPort.N3, DutPort.A17] = "66:1";                                                      //DRX_NA
                            settingsTemp[InstrPort.N4, DutPort.A25] = "70:1";                                                      //MLB_NA
                            settingsTemp[InstrPort.N5, DutPort.A19] = "67:1";                                                      //MIMO_NA
                            settingsTemp[InstrPort.N6, DutPort.A21] = "68:1";                                                      //GSM_NA

                            #endregion Switch Matrix Box Rev Y2D Map
                        }

                        #endregion

                        break;
                    case Rev.Y2:

                        #region Rev Y2
                        {
                            // Joker RF1 Setup in Seoul Lab
                            #region Switch Matrix Box Rev  Map

                            const string VSG_Amp = "00:1 01:1 02:1 ";
                            const string VSG_Bypass = "00:0 01:0 02:0 ";
                            const string VSA_Bypass = "30:0 31:1 34:1 35:1 36:0";
                            const string VSA_HPF2p7GHz = "30:1 31:1 32:0 33:0 34:1 35:0 36:1";
                            const string VSA_HPF2p7GHz_Logout = "30:1 31:1 32:0 33:0 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Logout2 = "30:1 31:1 32:1 33:1 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Extout = "30:1 31:1 32:0 34:0 35:0 36:1"; //For H2 CPL method

                            //  settingsTemp[InstrPort.VSA, DutPort.NONE] = "12:1";
                            // MagicBox
                            settingsTemp[InstrPort.SGIn, DutPort.SGCplr] = "86:1";
                            settingsTemp[InstrPort.SAIn, DutPort.SAOut] = "80:0";
                            settingsTemp[InstrPort.PwrSensorIn_From_O2, DutPort.PwrSensor] = "88:1";

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "00:0"; // Terminate ANT1 WITH 50ohm at NF-T1 externally//
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "00:0 01:0 03:0 04:1 08:0"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSG, DutPort.A1] = VSG_Amp + "03:0 04:0 05:0 07:0";                                  //TX_MB1
                            settingsTemp[InstrPort.VSG, DutPort.A2] = VSG_Amp + "03:0 04:0 05:1 06:0 07:0";                             //TX_HB2
                            settingsTemp[InstrPort.VSG, DutPort.A3] = VSG_Amp + "03:0 04:0 05:1 06:1 07:0";                             //TX_MB2

                            settingsTemp[InstrPort.VSG, DutPort.A4] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:0";                   //ANT1
                            settingsTemp[InstrPort.VSG, DutPort.A5] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1";                   //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 08:1 09:0 10:1 11:1";                                  //ANT3

                            settingsTemp[InstrPort.VSA, DutPort.A4] = "08:0 09:0 10:0 11:0 12:0 13:0 18:1 19:1 24:0 27:0 29:0 " + VSA_Bypass;     //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 " + VSA_Bypass;     //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "08:0 09:0 10:1 11:1 18:1 19:1 24:0 27:0 29:0 " + VSA_Bypass;               //ANT3

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "08:1 09:1 10:0 11:1 12:0 13:0 24:1 27:0 29:0 30:0 31:1 33:0 34:1 35:0 36:1";   //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "08:1 09:1 10:0 11:1 12:0 13:1 24:1 27:0 29:0 30:0 31:1 33:1 34:1 35:0 36:1";   //ANT2
                            //settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "08:0 09:0 10:0 11:0 12:0 13:0 18:1 19:1 24:0 27:0 29:0 " + VSA_HPF2p7GHz;    //ANT1
                            //settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 " + VSA_HPF2p7GHz;    //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "08:0 09:0 10:1 18:1 19:1 24:0 27:0 29:0 " + VSA_HPF2p7GHz;                   //ANT3

                            settingsTemp[InstrPort.VSA, DutPort.A7] = "08:1 09:1 10:1 11:1 14:0 15:1 25:0 28:0 29:1 17:0 21:0 23:0 24:1 27:0 " + VSA_Bypass;            //OUT1
                            settingsTemp[InstrPort.VSA, DutPort.A8] = "08:1 09:1 10:1 11:1 16:0 17:1 25:1 28:0 29:1 15:0 21:0 23:0 24:1 27:0 18:1 19:1 " + VSA_Bypass;  //OUT2
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "08:1 09:1 10:1 11:1 20:0 21:1 26:0 28:1 29:1 15:0 17:0 23:0 24:1 27:0 " + VSA_Bypass;           //OUT3
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "08:1 09:1 10:1 11:1 22:0 23:1 26:1 28:1 29:1 15:0 17:0 21:0 24:1 27:0 " + VSA_Bypass;           //OUT4   
                            settingsTemp[InstrPort.VSA, DutPort.A9] = "08:1 09:1 10:1 11:1 19:0 24:0 27:1 29:0 15:0 17:0 21:0 23:0 " + VSA_Bypass;                      //MIMO


                            ////////////////Need to update for RF2 

                            //////////////// for Joker soka
                            settingsTemp[InstrPort.N1, DutPort.A1] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0";                              //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A2] = "00:0 02:0 03:0 04:1 05:1 06:0 07:0";                              //TX_HB2_NA
                            settingsTemp[InstrPort.N1, DutPort.A3] = "00:0 02:0 03:0 04:1 05:1 06:1 07:0";                              //TX_MB2_NA

                            settingsTemp[InstrPort.N2, DutPort.A4] = "08:0 09:1 10:0 12:0 13:0";                                        //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A5] = "08:0 09:1 10:0 12:0 13:1";                                        //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A6] = "08:0 09:1 10:1 12:0 13:0";                                        //ANT3_NA

                            settingsTemp[InstrPort.N3, DutPort.A7] = "15:0";                                                            //OUT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A8] = "17:0 18:0 19:0";                                                  //OUT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A10] = "21:0";                                                            //OUT3_NA
                            settingsTemp[InstrPort.N6, DutPort.A11] = "23:0";                                                            //OUT4_NA

                            settingsTemp[InstrPort.N4, DutPort.A9] = "17:0 18:1 19:1";                                                  //MIMO_OUT_NA

                            settingsTemp[InstrPort.NS1, DutPort.A5] = "13:1";                                                           //NS1_ANT1
                            settingsTemp[InstrPort.NS2, DutPort.A6] = "16:1";                                                           //NS2_ANT2
                            settingsTemp[InstrPort.NS3, DutPort.A7] = "19:1";                                                           //NS3_ANT3
                            //////////////////////////////

                            #endregion Switch Matrix Box Rev J1 Map
                        }

                        #endregion

                        break;

                    case Rev.J1:

                       
                        // Joker Full Setup in Seoul Lab
                        #region Rev J1
                        {
                            #region Switch Matrix Box Rev  Map

                            const string VSG_Amp = "00:1 01:1 02:1 ";
                            const string VSG_Bypass = "00:0 01:0 02:0 ";
                            const string VSA_Bypass = "38:0 41:00";
                            const string VSA_HPF2p7GHz = "38:1 39:1 40:1 41:1";


                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "00:0 01:0 03:0 04:0 06:0"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = "38:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = "38:1 39:1 40:1 41:1";

                            settingsTemp[InstrPort.VSG, DutPort.A1] = VSG_Amp + "03:0 04:0 05:0";                                       //TX_MB1
                            settingsTemp[InstrPort.VSG, DutPort.A2] = VSG_Amp + "03:0 04:0 05:1";                                       //TX_MB2
                            settingsTemp[InstrPort.VSG, DutPort.A3] = VSG_Amp + "03:1 07:0 08:0 09:0 10:0 11:0 12:0";                   //TX_HB1
                            settingsTemp[InstrPort.VSG, DutPort.A4] = VSG_Amp + "03:1 07:0 08:0 09:0 10:1 11:0 12:0";                   //TX_HB2

                            settingsTemp[InstrPort.VSG, DutPort.A5] = VSG_Amp + "03:1 07:0 08:1 09:0 13:0 14:0 15:1";                   //ANT1
                            settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 07:1 08:0 09:0 16:0 17:0 18:1";                   //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A7] = VSG_Amp + "03:1 07:1 08:0 09:1 19:0 20:0 21:1";                   //ANT3

                            settingsTemp[InstrPort.VSA, DutPort.A5] = "13:0 14:0 15:0 30:0 35:0 37:1 38:0 41:0";                        //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "16:0 17:0 18:0 30:1 35:0 37:1 38:0 41:0";                        //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A7] = "19:0 20:0 21:0 31:0 35:1 37:1 38:0 41:0";                        //ANT3

                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "13:0 14:0 15:0 30:0 35:0 37:1 38:1 39:1 40:1 41:1";    //ANT1 2ND HAR       
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "16:0 17:0 18:0 30:1 35:0 37:1 38:1 39:1 40:1 41:1";    //ANT2 2ND HAR        
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A7] = "19:0 20:0 21:0 31:0 35:1 37:1 38:1 39:1 40:1 41:1";    //ANT3 2ND HAR      

                            settingsTemp[InstrPort.VSA, DutPort.A8] = "22:0 23:1 31:1 35:1 37:1 38:0 41:0";                             //OUT1
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "24:0 25:1 32:0 36:0 37:0 38:0 41:0";                            //OUT2
                            settingsTemp[InstrPort.VSA, DutPort.A12] = "26:0 27:1 32:1 36:0 37:0 38:0 41:0";                            //OUT3
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "28:0 29:1 33:0 36:1 37:0 38:0 41:0";                            //OUT4

                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A8] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 22:0 23:1 31:1 35:1 37:1 38:0 41:0";       //OUT1 Tx Leakage
                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A10] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 24:0 25:1 32:0 35:1 36:0 37:0 38:0 41:0";      //OUT2 Tx Leakage
                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A12] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 26:0 27:1 32:1 35:1 36:0 37:0 38:0 41:0";      //OUT3 Tx Leakage              
                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A14] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 28:0 29:1 33:0 36:1 37:0 38:0 41:0";      //OUT4 Tx Leakage

                            settingsTemp[InstrPort.VSA, DutPort.A9] = "22:1 23:1 31:1 35:1 37:1 38:0 41:0";                             //DRX
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "24:1 25:1 32:0 36:0 37:0 38:0 41:0";                            //MLB
                            settingsTemp[InstrPort.VSA, DutPort.A13] = "26:1 27:1 32:1 36:0 37:0 38:0 41:0";                            //GSM
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "28:1 29:1 33:0 36:1 37:0 38:0 41:0";                            //MIMO

                            //TxLeakage
                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A9] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 22:1 23:1 31:1 35:1 37:1 38:0 41:0";                //DRX Tx Leakage
                            settingsTemp[InstrPort.Tx_Leakage, DutPort.A15] = "13:1 16:1 19:1 14:1 17:1 20:1 15:1 18:1 21:1 28:1 29:1 33:0 36:1 37:0 38:0 41:0";               //MIMO Tx Leakage

                            settingsTemp[InstrPort.N1, DutPort.A1] = "04:1 05:0 06:0";                                                  //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A2] = "04:1 05:1 06:0";                                                  //TX_MB2_NA
                            settingsTemp[InstrPort.N2, DutPort.A3] = "10:0 11:1 12:0";                                                  //TX_HB1_NA
                            settingsTemp[InstrPort.N2, DutPort.A4] = "10:1 11:1 12:0";                                                  //TX_HB2_NA

                            settingsTemp[InstrPort.N3, DutPort.A5] = "13:0 14:1";                                                       //ANT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A6] = "16:0 17:1";                                                       //ANT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A7] = "19:0 20:1";                                                       //ANT3_NA

                            settingsTemp[InstrPort.N6, DutPort.A8] = "22:0 23:0";                                                       //OUT1_NA
                            settingsTemp[InstrPort.N6, DutPort.A9] = "22:1 23:0";                                                       //DRX_OUT_NA
                            settingsTemp[InstrPort.N7, DutPort.A10] = "24:0 25:0";                                                      //OUT2_NA
                            settingsTemp[InstrPort.N7, DutPort.A11] = "24:1 25:0";                                                      //MLB_IN_NA
                            settingsTemp[InstrPort.N8, DutPort.A12] = "26:0 27:0";                                                      //OUT3_NA
                            settingsTemp[InstrPort.N8, DutPort.A13] = "26:1 27:0";                                                      //GSM_IN_NA
                            settingsTemp[InstrPort.N9, DutPort.A14] = "28:0 29:0";                                                      //OUT4_NA
                            settingsTemp[InstrPort.N9, DutPort.A15] = "28:1 29:0";                                                      //MIMO_OUT_NA

                            settingsTemp[InstrPort.NS1, DutPort.A5] = "13:1";                                                           //NS1_ANT1
                            settingsTemp[InstrPort.NS2, DutPort.A6] = "16:1";                                                           //NS2_ANT2
                            settingsTemp[InstrPort.NS3, DutPort.A7] = "19:1";                                                           //NS3_ANT3

                            #endregion Switch Matrix Box Rev J1 Map
                        }

                        #endregion
                       

                        break;

                    case Rev.O2:

                        #region Rev O2
                        {

                            #region Switch Matrix Box Rev O2 Map

                            //For Cpl Switching Time, terminate ANT1
                            settingsTemp[InstrPort.TERM, DutPort.A4] = "12:1 13:0 14:0"; // Terminate ANT1 WITH 50ohm at NF-T1 externally//

                            settingsTemp[InstrPort.SGIn, DutPort.SGCplr] = "86:1";
                            settingsTemp[InstrPort.SAIn, DutPort.SAOut] = "80:0";
                            settingsTemp[InstrPort.PwrSensorIn_From_O2, DutPort.PwrSensor] = "88:1";


                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "11:1 12:1"; // None

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A1] = "00:1 01:1 02:1 03:0 04:0 05:0"; // MB,2G IN //

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A2] = "00:1 01:1 02:1 03:0 04:0 05:1 06:0"; // MB IN //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A3] = "00:1 01:1 02:1 03:0 04:0 05:1 06:1"; // HB IN // 
                            settingsTemp[InstrPort.VSG, DutPort.A1] = "00:0 01:1 02:0 03:0 04:0 05:0"; // MB,2G IN ////No preAmp
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "00:0 01:1 02:0 03:0 04:0 05:1 06:0"; // MB IN //No preAmp
                            settingsTemp[InstrPort.VSG, DutPort.A3] = "00:0 01:1 02:0 03:0 04:0 05:1 06:1"; // HB IN //No preAmp

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A4] = "00:1 01:1 02:1 03:1 08:0 09:0 10:1 11:0 12:0 13:0 14:0"; //  VSG to ANT1 //

                            settingsTemp[InstrPort.VSA, DutPort.A4] = "10:0 11:0 12:0 13:0 14:0 39:0 43:0 44:0 66:0"; //  ANT1 //
                            settingsTemp[InstrPort.TERM, DutPort.A4] = "12:1 32:1 33:1 34:1 37:0 38:1"; //  ANT1 //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "10:0 11:0 12:0 13:0 14:0 39:0 43:0 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; // Ant 2ND HAR //       
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A4] = "10:0 11:0 12:0 13:0 14:0 39:0 43:0 44:1 45:1 46:0 47:0 64:1 65:1"; // Ant 3RD HAR //

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A5] = "00:1 01:1 02:1 03:1 08:0 09:0 10:1 11:0 12:0 13:0 14:1"; //  VSG to ANT1 //

                            settingsTemp[InstrPort.VSA, DutPort.A5] = "10:0 11:0 12:0 13:0 14:1 39:0 43:0 44:0 66:0"; //  ANT2 //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "10:0 11:0 12:0 13:0 14:1 39:0 43:0 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; // Ant 2ND HAR //       
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A5] = "10:0 11:0 12:0 13:0 14:1 39:0 43:0 44:1 45:1 46:0 47:0 64:1 65:1"; // Ant 3RD HAR //

                            settingsTemp[InstrPort.VSA, DutPort.A6] = "15:1 16:0 17:0 18:0 39:0 41:0 42:0 43:1 44:0 66:0"; //  HB1(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "15:1 16:0 17:0 18:0 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB1(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A6] = "15:1 16:0 17:0 18:0 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB1(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A7] = "15:1 16:0 17:1 18:0 39:0 41:0 42:0 43:1 44:0 66:0"; //  HBAUX1(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A7] = "15:1 16:0 17:1 18:0 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HBAUX1(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A7] = "15:1 16:0 17:1 18:0 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HBAUX1(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A8] = "15:1 16:0 17:0 18:1 39:0 41:0 42:0 43:1 44:0 66:0"; //  MB1(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A8] = "15:1 16:0 17:0 18:1 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  MB1(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A8] = "15:1 16:0 17:0 18:1 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // MB1(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A9] = "15:1 16:0 17:1 18:1 39:0 41:0 42:0 43:1 44:0 66:0"; //  MB4RX1(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A9] = "15:1 16:0 17:1 18:1 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  MB4RX1(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A9] = "15:1 16:0 17:1 18:1 39:0 41:0 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // MB4RX1(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A10] = "19:1 20:0 21:0 22:0 39:0 41:1 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A10] = "19:1 20:0 21:0 22:0 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A10] = "19:1 20:0 21:0 22:0 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A11] = "19:1 20:0 21:1 22:0 39:0 41:1 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A11] = "19:1 20:0 21:1 22:0 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A11] = "19:1 20:0 21:1 22:0 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A12] = "19:1 20:0 21:0 22:1 39:0 41:1 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A12] = "19:1 20:0 21:0 22:1 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A12] = "19:1 20:0 21:0 22:1 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A13] = "19:1 20:0 21:1 22:1 39:0 41:1 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A13] = "19:1 20:0 21:1 22:1 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A13] = "19:1 20:0 21:1 22:1 39:0 41:1 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A14] = "23:1 24:1 25:0 26:0 27:0 28:0 29:0 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = "23:1 24:1 25:0 26:0 27:0 28:0 29:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A14] = "23:1 24:1 25:0 26:0 27:0 28:0 29:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A15] = "23:1 24:1 25:0 26:0 27:0 28:1 29:0 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = "23:1 24:1 25:0 26:0 27:0 28:1 29:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A15] = "23:1 24:1 25:0 26:0 27:0 28:1 29:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A16] = "23:1 24:1 25:0 26:0 27:0 28:0 29:1 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A16] = "23:1 24:1 25:0 26:0 27:0 28:0 29:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A16] = "23:1 24:1 25:0 26:0 27:0 28:0 29:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A17] = "23:1 24:1 25:0 26:0 27:0 28:1 29:1 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A17] = "23:1 24:1 25:0 26:0 27:0 28:1 29:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A17] = "23:1 24:1 25:0 26:0 27:0 28:1 29:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A18] = "23:1 24:1 25:0 26:0 27:1 30:0 31:0 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A18] = "23:1 24:1 25:0 26:0 27:1 30:0 31:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A18] = "23:1 24:1 25:0 26:0 27:1 30:0 31:0 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A20] = "23:1 24:1 25:0 26:0 27:1 30:0 31:1 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A20] = "23:1 24:1 25:0 26:0 27:1 30:0 31:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A20] = "23:1 24:1 25:0 26:0 27:1 30:0 31:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A21] = "23:1 24:1 25:0 26:0 27:1 30:1 31:1 41:0 42:1 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A21] = "23:1 24:1 25:0 26:0 27:1 30:1 31:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A21] = "23:1 24:1 25:0 26:0 27:1 30:1 31:1 41:0 42:1 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A22] = "32:0 33:1 34:0 35:0 36:0 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A22] = "32:0 33:1 34:0 35:0 36:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A22] = "32:0 33:1 34:0 35:0 36:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A23] = "32:0 33:1 34:0 35:1 36:0 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A23] = "32:0 33:1 34:0 35:1 36:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A23] = "32:0 33:1 34:0 35:1 36:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A24] = "32:0 33:1 34:0 35:0 36:1 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A24] = "32:0 33:1 34:0 35:0 36:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A24] = "32:0 33:1 34:0 35:0 36:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A25] = "32:0 33:1 34:0 35:1 36:1 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A25] = "32:0 33:1 34:0 35:1 36:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A25] = "32:0 33:1 34:0 35:1 36:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A26] = "32:0 33:1 34:1 37:0 38:0 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A26] = "32:0 33:1 34:1 37:0 38:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A26] = "32:0 33:1 34:1 37:0 38:0 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 

                            settingsTemp[InstrPort.VSA, DutPort.A28] = "32:0 33:1 34:1 37:0 38:1 41:1 42:0 43:1 44:0 66:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A28] = "32:0 33:1 34:1 37:0 38:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:0 66:1"; //  HB2(JEDI1) 2ND HAR //        
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A28] = "32:0 33:1 34:1 37:0 38:1 41:1 42:0 43:1 44:1 45:1 46:0 47:0 64:1 65:1"; // HB2(JEDI1) 3RD HAR // 


                            settingsTemp[InstrPort.VSG_Amp, DutPort.A14] = "23:1 24:0 25:0 26:0 27:0 28:0 29:0 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A15] = "23:1 24:0 25:0 26:0 27:0 28:1 29:0 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A16] = "23:1 24:0 25:0 26:0 27:0 28:0 29:1 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A17] = "23:1 24:0 25:0 26:0 27:0 28:1 29:1 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A18] = "23:1 24:0 25:0 26:0 27:1 30:0 31:0 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            //settingsTemp[InstrPort.VSG, DutPort.A15] = "23:1 24:0 25:0 26:0 27:0 28:1 29:0 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A20] = "23:1 24:0 25:0 26:0 27:1 30:0 31:1 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A21] = "23:1 24:0 25:0 26:0 27:1 30:1 31:1 00:1 01:1 02:1 03:1 08:1 09:1"; //  HB2(JEDI1) //

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A22] = "32:0 33:0 34:0 35:0 36:0 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A23] = "32:0 33:0 34:0 35:1 36:0 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A24] = "32:0 33:0 34:0 35:0 36:1 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A25] = "32:0 33:0 34:0 35:1 36:1 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A26] = "32:0 33:0 34:1 37:0 38:0 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A28] = "32:0 33:0 34:1 37:0 38:1 00:1 01:1 02:1 03:1 08:1 09:0"; //  HB2(JEDI1) //

                            //settingsTemp[InstrPort.N1, DutPort.A2] = "06:0 05:1 04:1 07:0"; // MB1_IN_NA //
                            //settingsTemp[InstrPort.N1, DutPort.A3] = "06:1 05:1 04:1 07:0"; // HB1_IN_NA //

                            //settingsTemp[InstrPort.N2, DutPort.A4] = "13:0 12:0 11:0 10:0 09:1"; // ANT_NA //
                            ///*settingsTemp[InstrPort.N4, DutPort.A6] = "10:1 09:0 08:0 27:1 24:0 19:1 18:1"; // 2G GSM ANT // 안나와서 수정 N2port*/
                            //settingsTemp[InstrPort.N2, DutPort.A6] = "10:1 09:1"; // 2G GSM ANT //

                            //settingsTemp[InstrPort.N3, DutPort.A7] = "14:0 15:0"; // MB1_RX_NA //
                            //settingsTemp[InstrPort.N4, DutPort.A8] = "16:0 17:0 18:0"; // MB2_RX_NA //

                            //settingsTemp[InstrPort.N5, DutPort.A10] = "20:0 21:0"; // HB1_RX_NA //
                            //settingsTemp[InstrPort.N6, DutPort.A11] = "22:0 23:0"; // HB2_RX_NA //

                            //settingsTemp[InstrPort.VSG_Cable, DutPort.A5] = "00:0 01:1 10:0 11:0 13:0";

                            //settingsTemp[InstrPort.NS, DutPort.A4] = "13:0 12:1"; // noise source


                            #endregion Switch Matrix Box Rev O2 Map

                        }


                        #endregion Rev O2

                        break;
                    case Rev.R:

                        # region SMPAD_RevR
                        {
                            settingsTemp[InstrPort.VSG, DutPort.A1] = "00:0 01:0 02:0 03:0 04:0 05:0 06:1";
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "00:0 01:0 02:0 03:0 04:0 05:1 06:1";
                            settingsTemp[InstrPort.VSG, DutPort.A3] = "00:0 01:0 02:0 03:1 07:1 09:1 10:1 11:0 12:0 13:1 14:1 16:0";  //VSG TO ANT1
                            settingsTemp[InstrPort.VSG, DutPort.A4] = "00:0 01:0 02:0 03:1 07:1 09:1 10:0 11:1 13:0 14:0 15:0 16:1";  //VSG TO ANT2
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A1] = "00:1 01:1 02:1 03:0 04:0 05:0 06:1";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A2] = "00:1 01:1 02:1 03:0 04:0 05:1 06:1";

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A3] = "00:1 01:1 02:1 03:1 07:1 09:1 10:1 11:0 12:0 13:1 14:1 16:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A4] = "00:1 01:1 02:1 03:1 07:1 09:1 10:0 11:1 13:0 14:0 15:0 16:1";

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A6] = "00:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A8] = "00:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A10] = "00:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A16] = "00:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A14] = "00:0";

                            settingsTemp[InstrPort.VSA, DutPort.A3] = "11:0 12:1 13:1 14:0 15:0 16:0 34:0 36:0 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A4] = "11:0 12:0 13:0 14:0 15:1 16:1 34:1 36:0 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "12:0 13:0 15:0 16:0 17:1 18:0 19:0 20:0 26:1 32:1 35:1 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "12:0 13:0 15:0 16:0 17:1 18:0 19:0 20:1 26:1 32:1 35:1 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A7] = "12:0 13:0 15:0 16:0 17:1 18:0 19:1 21:0 26:1 32:1 35:1 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A8] = "12:0 13:0 15:0 16:0 17:1 18:0 19:1 21:1 26:1 32:1 35:1 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A9] = "12:0 13:0 15:0 16:0 22:1 23:0 24:0 25:0 26:1 33:0 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "12:0 13:0 15:0 16:0 22:1 23:0 24:0 25:1 26:1 33:0 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "12:0 13:0 15:0 16:0 22:1 23:0 24:1 26:0 33:0 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A13] = "12:0 13:0 15:0 16:0 26:1 27:1 28:0 29:0 30:0 33:1 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "12:0 13:0 15:0 16:0 26:1 27:1 28:0 29:0 30:1 33:1 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "12:0 13:0 15:0 16:0 26:1 27:1 28:0 29:1 31:0 33:1 35:0 36:1 37:0 38:0";
                            settingsTemp[InstrPort.VSA, DutPort.A16] = "12:0 13:0 15:0 16:0 26:1 27:1 28:0 29:1 31:1 33:1 35:0 36:1 37:0 38:0";


                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A3] = "11:0 12:1 13:1 14:0 15:0 16:0 34:0 36:0 37:1 38:1 39:0 40:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "11:0 12:0 13:0 14:0 15:1 16:1 34:1 36:0 37:1 38:1 39:0 40:0";

                            settingsTemp[InstrPort.N1, DutPort.A1] = "00:1 01:0 04:1 05:0 06:0";
                            settingsTemp[InstrPort.N1, DutPort.A2] = "00:1 01:0 04:1 05:1 06:0";
                            settingsTemp[InstrPort.N2, DutPort.A3] = "11:1 12:0 13:1";
                            settingsTemp[InstrPort.N3, DutPort.A4] = "14:1 15:0 16:1";
                            settingsTemp[InstrPort.N4, DutPort.A5] = "17:1 18:1 19:0 20:0";
                            settingsTemp[InstrPort.N4, DutPort.A6] = "17:1 18:1 19:0 20:1";
                            settingsTemp[InstrPort.N4, DutPort.A7] = "17:1 18:1 19:1 21:0";
                            settingsTemp[InstrPort.N4, DutPort.A8] = "17:1 18:1 19:1 21:1";
                            settingsTemp[InstrPort.N5, DutPort.A9] = "22:1 23:1 24:0 25:0 26:1";
                            settingsTemp[InstrPort.N5, DutPort.A10] = "22:1 23:1 24:0 25:1 26:1";
                            settingsTemp[InstrPort.N5, DutPort.A11] = "22:1 23:1 24:1 26:0";
                            settingsTemp[InstrPort.N5, DutPort.A12] = "22:1 23:1 24:1 26:1";
                            settingsTemp[InstrPort.N6, DutPort.A13] = "27:1 28:1 29:0 30:0";
                            settingsTemp[InstrPort.N6, DutPort.A14] = "27:1 28:1 29:0 30:1";
                            settingsTemp[InstrPort.N6, DutPort.A15] = "27:1 28:1 29:1 31:0";
                            settingsTemp[InstrPort.N6, DutPort.A16] = "27:1 28:1 29:1 31:1";
                        }

                        # endregion

                        break;
                    case Rev.SE:

                        #region Rev SE
                        {
                            settingsTemp[InstrPort.P_VST, DutPort.VST_1] = "00:0 01:0";
                            settingsTemp[InstrPort.P_VST, DutPort.VST_2] = "00:1 01:1";
                            settingsTemp[InstrPort.P_FBAR, DutPort.FBAR_1] = "02:0 03:0 04:0 05:0 06:0 07:0 08:0 09:0 10:0 11:0";
                            settingsTemp[InstrPort.P_FBAR, DutPort.FBAR_2] = "02:1 03:1 04:1 05:1 06:1 07:1 08:1 09:1 10:1 11:1";
                        }
                        #endregion

                        break;
                    case Rev.C:

                        #region Rev C
                        {
                            string VSA_Bypass = " 28:0 30:0 29:0";
                            string VSA_HPF1_FC_Below_1G = " 28:1 29:0";
                            string VSA_HPF2_FC_1p65G_To_2p25G = " 28:1 29:1 31:0";
                            string VSA_HPF3_FC_2p25G_To_3G = " 28:1 29:1 31:1";
                            string PS_AMP_HPF6GHz = " 28:0 30:1 29:0";
                            string VSA_1_Switchpath = " 25:0 27:1";
                            string VSA_2_Switchpath = " 25:1 27:1";
                            string VSA_3_Switchpath = " 26:0 27:0";
                            string VSA_4_Switchpath = " 26:1 27:0";

                            #region Switch Matrix Box Rev C0 Map

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A1] = "00:0 01:0 03:0 04:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A2] = "00:0 01:0 03:1 05:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A3] = "00:0 01:1 08:0 09:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A4] = "00:0 01:1 08:1 10:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A5] = "00:1 02:0 13:0 15:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A6] = "00:1 02:0 13:1 14:0 16:0"; //Extra switch setting is because of S port
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A7] = "00:1 02:1 19:0 20:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A8] = "00:1 02:1 19:1 21:0";

                            settingsTemp[InstrPort.VSA, DutPort.A1] = "04:1 06:0 07:0" + VSA_1_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A2] = "05:1 06:1 07:0" + VSA_1_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A3] = "09:1 11:0 12:0" + VSA_2_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A4] = "10:1 11:1 12:0" + VSA_2_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "15:1 17:0 18:0" + VSA_3_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "16:1 17:1 18:0" + VSA_3_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A7] = "20:1 22:0 23:0 24:1" + VSA_4_Switchpath + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A8] = "21:1 22:1 23:0 24:1" + VSA_4_Switchpath + VSA_Bypass;

                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A1] = "04:1 06:0 07:0" + VSA_1_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A2] = "05:1 06:1 07:0" + VSA_1_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A3] = "09:1 11:0 12:0" + VSA_2_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A4] = "10:1 11:1 12:0" + VSA_2_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A5] = "15:1 17:0 18:0" + VSA_3_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A6] = "16:1 17:1 18:0" + VSA_3_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A7] = "20:1 22:0 23:0 24:1" + VSA_4_Switchpath + VSA_HPF1_FC_Below_1G;
                            settingsTemp[InstrPort.VSA_HPF1_FC_Below_1G, DutPort.A8] = "21:1 22:1 23:0 24:1" + VSA_4_Switchpath + VSA_HPF1_FC_Below_1G;

                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A1] = "04:1 06:0 07:0" + VSA_1_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A2] = "05:1 06:1 07:0" + VSA_1_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A3] = "09:1 11:0 12:0" + VSA_2_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A4] = "10:1 11:1 12:0" + VSA_2_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A5] = "15:1 17:0 18:0" + VSA_3_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A6] = "16:1 17:1 18:0" + VSA_3_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A7] = "20:1 22:0 23:0 24:1" + VSA_4_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;
                            settingsTemp[InstrPort.VSA_HPF2_FC_1p65G_To_2p25G, DutPort.A8] = "21:1 22:1 23:0 24:1" + VSA_4_Switchpath + VSA_HPF2_FC_1p65G_To_2p25G;

                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A1] = "04:1 06:0 07:0" + VSA_1_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A2] = "05:1 06:1 07:0" + VSA_1_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A3] = "09:1 11:0 12:0" + VSA_2_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A4] = "10:1 11:1 12:0" + VSA_2_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A5] = "15:1 17:0 18:0" + VSA_3_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A6] = "16:1 17:1 18:0" + VSA_3_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A7] = "20:1 22:0 23:0 24:1" + VSA_4_Switchpath + VSA_HPF3_FC_2p25G_To_3G;
                            settingsTemp[InstrPort.VSA_HPF3_FC_2p25G_To_3G, DutPort.A8] = "21:1 22:1 23:0 24:1" + VSA_4_Switchpath + VSA_HPF3_FC_2p25G_To_3G;

                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A1] = "04:1 06:0 07:0" + VSA_1_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A2] = "05:1 06:1 07:0" + VSA_1_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A3] = "09:1 11:0 12:0" + VSA_2_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A4] = "10:1 11:1 12:0" + VSA_2_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A5] = "15:1 17:0 18:0" + VSA_3_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A6] = "16:1 17:1 18:0" + VSA_3_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A7] = "20:1 22:0 23:0 24:1" + VSA_4_Switchpath + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A8] = "21:1 22:1 23:0 24:1" + VSA_4_Switchpath + PS_AMP_HPF6GHz;

                            // Network Analyzer connections
                            settingsTemp[InstrPort.N1, DutPort.A1] = "04:1 06:0 07:1";
                            settingsTemp[InstrPort.N1, DutPort.A2] = "05:1 06:1 07:1";

                            settingsTemp[InstrPort.N2, DutPort.A3] = "09:1 11:0 12:1";
                            settingsTemp[InstrPort.N2, DutPort.A4] = "10:1 11:1 12:1";

                            settingsTemp[InstrPort.N3, DutPort.A5] = "15:1 17:0 18:1";
                            settingsTemp[InstrPort.N3, DutPort.A6] = "16:1 17:1 18:1";

                            settingsTemp[InstrPort.N4, DutPort.A7] = "20:1 22:0 23:1";
                            settingsTemp[InstrPort.N4, DutPort.A8] = "21:1 22:1 23:1";
                        }

                        #endregion Switch Matrix Box Rev C0 Map

                        #endregion

                        break;

                    case Rev.CEX:

                        #region Rev CEX
                        {
                            string VSA_Bypass = " 28:0 30:0 29:0";
                            string VSA_HPF1p3GHz = " 28:1 29:0";
                            string VSA_HPF2p7GHz = " 28:1 29:1 31:0";
                            string VSA_HPF3p5GHz = " 28:1 29:1 31:1";
                            string PS_AMP_HPF6GHz = " 28:0 30:1 29:0";

                            //Dictionary.DoubleKey<InstrPort, DutPort, string> settingsTemp = new Dictionary.DoubleKey<InstrPort, DutPort, string>();

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A1] = "00:0 01:0 03:0 04:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A2] = "00:0 01:0 03:1 04:1 06:0";

                            settingsTemp[InstrPort.VSG_Amp, DutPort.A3] = "00:0 01:1 08:0 09:0 10:0 12:1 11:1";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A4] = "00:0 01:1 08:1 09:1 10:0 11:0 12:0";

                            settingsTemp[InstrPort.VSA, DutPort.A3] = "09:1 11:0 12:0 25:1 27:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A4] = "10:1 11:1 12:0 25:1 27:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "32:0 15:1 17:0 18:0 26:0 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "32:1 15:1 17:0 18:0 26:0 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A7] = "34:0 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A8] = "34:1 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.R] = "24:0 23:1 26:1 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_Bypass;

                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A3] = "09:1 11:0 12:0 25:1 27:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A5] = "32:0 15:1 17:0 18:0 26:0 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A6] = "32:1 15:1 17:0 18:0 26:0 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A7] = "34:0 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A8] = "34:1 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.R] = "24:0 23:1 26:1 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF1p3GHz;

                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A3] = "09:1 11:0 12:0 25:1 27:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "32:0 15:1 17:0 18:0 26:0 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "32:1 15:1 17:0 18:0 26:0 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A7] = "34:0 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A8] = "34:1 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.R] = "24:0 23:1 26:1 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A3] = "09:1 11:0 12:0 25:1 27:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A5] = "32:0 15:1 17:0 18:0 26:0 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A6] = "32:1 15:1 17:0 18:0 26:0 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A7] = "34:0 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A8] = "34:1 20:1 22:0 23:0 24:1 26:1 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.R] = "24:0 23:1 26:1 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:0 26:0 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:0 26:0 27:0" + VSA_HPF3p5GHz;

                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A3] = "09:1 11:0 12:0 25:1 27:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A5] = "32:0 15:1 17:0 18:0 26:0 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A6] = "32:1 15:1 17:0 18:0 26:0 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A7] = "34:0 20:1 22:0 23:0 24:1 26:1 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A8] = "34:1 20:1 22:0 23:0 24:1 26:1 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.R] = "24:0 23:1 26:1 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:0 26:0 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:0 26:0 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:0 26:0 27:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:0 26:0 27:0" + PS_AMP_HPF6GHz;

                            // Network Analyzer connections
                            settingsTemp[InstrPort.N1, DutPort.A1] = "04:1 06:0 07:1";
                            settingsTemp[InstrPort.N1, DutPort.A2] = "00:0 01:0 03:0 04:0 05:1 06:1 07:1";

                            settingsTemp[InstrPort.N2, DutPort.A3] = "9:1 11:0 12:1 10:0";
                            settingsTemp[InstrPort.N2, DutPort.A4] = "10:1 11:1 12:1 9:0";

                            settingsTemp[InstrPort.N3, DutPort.A5] = "32:0 15:1 17:0 18:1 16:0";
                            settingsTemp[InstrPort.N3, DutPort.A6] = "32:1 15:1 17:0 18:1 16:0";

                            settingsTemp[InstrPort.N4, DutPort.A7] = "34:0 20:1 22:0 23:1 21:0";
                            settingsTemp[InstrPort.N4, DutPort.A8] = "34:1 20:1 22:0 23:1 21:0";

                            settingsTemp[InstrPort.N3, DutPort.L1] = "36:0 40:0 44:0 16:1 17:1 18:1";
                            settingsTemp[InstrPort.N3, DutPort.L2] = "37:0 40:1 44:0 16:1 17:1 18:1";
                            settingsTemp[InstrPort.N3, DutPort.R1] = "38:0 41:0 44:1 16:1 17:1 18:1";
                            settingsTemp[InstrPort.N3, DutPort.R2] = "39:0 41:1 44:1 16:1 17:1 18:1";

                            settingsTemp[InstrPort.N4, DutPort.L1] = "36:1 42:0 45:0 21:1 22:1 23:1";
                            settingsTemp[InstrPort.N4, DutPort.L2] = "37:1 42:1 45:0 21:1 22:1 23:1";
                            settingsTemp[InstrPort.N4, DutPort.R1] = "38:1 43:0 45:1 21:1 22:1 23:1";
                            settingsTemp[InstrPort.N4, DutPort.R2] = "39:1 43:1 45:1 21:1 22:1 23:1";

                        }

                        #endregion

                        break;

                    case Rev.F:

                        #region Rev F
                        {

                            #region SWITCH PATH CONSTANT DEFINITIONS

                            const string VSA_THRU = " 36:0 39:0 38:0 40:0 41:0";  //Thru path, no HPF
                            const string VSA_HPF1p3GHz = " 36:0 39:0 38:1 40:0 41:0";
                            const string VSA_HPF2p7GHz = " 36:0 39:0 38:2 40:0 41:0";
                            const string VSA_HPF3p5GHz = " 36:0 39:0 38:3 40:0 41:0";
                            const string PS_AMP_HPF6GHz = " 36:0 39:0 38:0 40:1 41:1";

                            //making constants for these switches for easier readability
                            const string SW1_POS0 = "0:0 1:0";
                            const string SW1_POS1 = "0:1 1:0";
                            const string SW1_POS2 = "0:0 1:1";
                            const string SW1_POS3 = "0:1 1:1";

                            const string SW2_POS0 = "2:0 3:0";
                            const string SW2_POS1 = "2:1 3:0";
                            const string SW2_POS2 = "2:0 3:1";
                            const string SW2_POS3 = "2:1 3:1";

                            const string SW3_POS0 = "4:0 5:0";
                            const string SW3_POS1 = "4:1 5:0";
                            const string SW3_POS2 = "4:0 5:1";
                            const string SW3_POS3 = "4:1 5:1";

                            const string SW4_POS0 = "7:0 6:0";
                            const string SW4_POS1 = "7:0 6:1";
                            const string SW4_POS2 = "7:1 6:0";
                            const string SW4_POS3 = "7:1 6:1";

                            const string SW5_POS0 = "9:0 8:0";
                            const string SW5_POS1 = "9:0 8:1";
                            const string SW5_POS2 = "9:1 8:0";
                            const string SW5_POS3 = "9:1 8:1";

                            const string SW6_POS0 = "11:0 10:0";
                            const string SW6_POS1 = "11:0 10:1";
                            const string SW6_POS2 = "11:1 10:0";
                            const string SW6_POS3 = "11:1 10:1";

                            const string SW7_POS0 = "14:0 13:0";
                            const string SW7_POS1 = "14:0 13:1";
                            const string SW7_POS2 = "14:1 13:0";
                            const string SW7_POS3 = "14:1 13:1";

                            const string SW8_POS0 = "16:0 15:0";
                            const string SW8_POS1 = "16:0 15:1";
                            const string SW8_POS2 = "16:1 15:0";
                            const string SW8_POS3 = "16:1 15:1";

                            const string SW9_POS0 = "28:0 27:0";
                            const string SW9_POS1 = "28:0 27:1";
                            const string SW9_POS2 = "28:1 27:0";
                            const string SW9_POS3 = "28:1 27:1";

                            const string SW10_POS0 = "26:0 25:0";
                            const string SW10_POS1 = "26:0 25:1";
                            const string SW10_POS2 = "26:1 25:0";
                            const string SW10_POS3 = "26:1 25:1";

                            const string SW11_POS0 = "33:0 32:0";
                            const string SW11_POS1 = "33:0 32:1";
                            const string SW11_POS2 = "33:1 32:0";
                            const string SW11_POS3 = "33:1 32:1";

                            const string SW12_POS0 = "35:0 34:0";
                            const string SW12_POS1 = "35:0 34:0";
                            const string SW12_POS2 = "35:0 34:0";
                            const string SW12_POS3 = "35:0 34:0";

                            const string SW13_POS0 = "37:0 36:0";
                            const string SW13_POS1 = "37:0 36:1";
                            const string SW13_POS2 = "37:1 36:0";
                            const string SW13_POS3 = "37:1 36:1";

                            const string SW14_POS0 = "39:0 38:0";
                            const string SW14_POS1 = "39:0 38:1";
                            const string SW14_POS2 = "39:1 38:0";
                            const string SW14_POS3 = "39:1 38:1";

                            const string SW15_POS0 = "31:0 30:0";
                            const string SW15_POS1 = "31:0 30:1";
                            const string SW15_POS2 = "31:1 30:0";
                            const string SW15_POS3 = "31:1 30:1";

                            const string spacer = " ";

                            #endregion SWITCH PATH CONSTANT DEFINITIONS

                            //Dictionary.DoubleKey<InstrPort, DutPort, string> settingsTemp = new Dictionary.DoubleKey<InstrPort, DutPort, string>();

                            #region VSG TO DUTPORT PATHS
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A1] = SW1_POS0 + spacer + SW2_POS0 + spacer + SW3_POS0;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A2] = SW1_POS0 + spacer + SW2_POS0 + spacer + SW3_POS1;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A3] = SW1_POS0 + spacer + SW2_POS0 + spacer + SW3_POS2;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A4] = SW1_POS0 + spacer + SW2_POS0 + spacer + SW3_POS3; //"00:0 01:0 02:0 03:0 04:1 05:1"; //
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A5] = SW1_POS1 + spacer + SW4_POS0 + spacer + SW5_POS0;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A6] = SW1_POS1 + spacer + SW4_POS0 + spacer + SW5_POS1;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A7] = SW1_POS1 + spacer + SW4_POS0 + spacer + SW5_POS2;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A8] = SW1_POS1 + spacer + SW4_POS0 + spacer + SW5_POS3;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A9] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A10] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A11] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A12] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A13] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A14] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0";
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A15] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A16] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A17] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A18] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A19] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A20] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A21] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2;
                            settingsTemp[InstrPort.VSG_Amp, DutPort.A22] = SW1_POS2 + spacer + SW6_POS0 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3;
                            #endregion VSG TO DUTPORT PATHS

                            #region VSA_THRU TO DUTPORT PATHS
                            settingsTemp[InstrPort.VSA, DutPort.A1] = SW1_POS3 + spacer + SW2_POS1 + spacer + SW3_POS0 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A2] = SW1_POS3 + spacer + SW2_POS1 + spacer + SW3_POS1 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A3] = SW1_POS3 + spacer + SW2_POS1 + spacer + SW3_POS2 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A4] = SW1_POS3 + spacer + SW2_POS1 + spacer + SW3_POS3 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A5] = SW1_POS3 + spacer + SW4_POS1 + spacer + SW5_POS0 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A6] = SW1_POS3 + spacer + SW4_POS1 + spacer + SW5_POS1 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A7] = SW1_POS3 + spacer + SW4_POS1 + spacer + SW5_POS2 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A8] = SW1_POS3 + spacer + SW4_POS1 + spacer + SW5_POS3 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A9] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A10] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A11] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A12] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A13] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A14] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A15] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A16] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A17] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A18] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A19] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A20] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A21] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA, DutPort.A22] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:0 41:0";
                            #endregion VSA_THRU TO DUTPORT PATHS

                            #region VSA_HPF1p3GHz TO DUTPORT
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A1] = SW2_POS1 + spacer + SW3_POS0 + spacer + SW13_POS0 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A2] = SW2_POS1 + spacer + SW3_POS1 + spacer + SW13_POS0 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A3] = SW2_POS1 + spacer + SW3_POS2 + spacer + SW13_POS0 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A4] = SW2_POS1 + spacer + SW3_POS3 + spacer + SW13_POS0 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A5] = SW4_POS1 + spacer + SW5_POS0 + spacer + SW13_POS1 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A6] = SW4_POS1 + spacer + SW5_POS1 + spacer + SW13_POS1 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A7] = SW4_POS1 + spacer + SW5_POS2 + spacer + SW13_POS1 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A8] = SW4_POS1 + spacer + SW5_POS3 + spacer + SW13_POS1 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A9] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A10] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A11] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A12] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A13] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A14] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0" + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A15] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A16] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A17] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A18] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A19] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A20] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A21] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A22] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3 + spacer + SW13_POS2 + spacer + SW14_POS1 + spacer + "40:0 41:0";
                            #endregion VSA_HPF1p3GHz TO DUTPORT

                            #region VSA_HPF2p7GHz TO DUTPORT
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A1] = SW2_POS1 + spacer + SW3_POS0 + spacer + SW13_POS0 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A2] = SW2_POS1 + spacer + SW3_POS1 + spacer + SW13_POS0 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A3] = SW2_POS1 + spacer + SW3_POS2 + spacer + SW13_POS0 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = SW2_POS1 + spacer + SW3_POS3 + spacer + SW13_POS0 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = SW4_POS1 + spacer + SW5_POS0 + spacer + SW13_POS1 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = SW4_POS1 + spacer + SW5_POS1 + spacer + SW13_POS1 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A7] = SW4_POS1 + spacer + SW5_POS2 + spacer + SW13_POS1 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A8] = SW4_POS1 + spacer + SW5_POS3 + spacer + SW13_POS1 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A9] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A10] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A11] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A12] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A13] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0" + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A16] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A17] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A18] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A19] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A20] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A21] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A22] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3 + spacer + SW13_POS2 + spacer + SW14_POS2 + spacer + "40:0 41:0";
                            #endregion VSA_HPF2p7GHz TO DUTPORT

                            #region VSA_HPF3p5GHz TO DUTPORTS
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A1] = SW2_POS1 + spacer + SW3_POS0 + spacer + SW13_POS0 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A2] = SW2_POS1 + spacer + SW3_POS1 + spacer + SW13_POS0 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A3] = SW2_POS1 + spacer + SW3_POS2 + spacer + SW13_POS0 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A4] = SW2_POS1 + spacer + SW3_POS3 + spacer + SW13_POS0 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A5] = SW4_POS1 + spacer + SW5_POS0 + spacer + SW13_POS1 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A6] = SW4_POS1 + spacer + SW5_POS1 + spacer + SW13_POS1 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A7] = SW4_POS1 + spacer + SW5_POS2 + spacer + SW13_POS1 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A8] = SW4_POS1 + spacer + SW5_POS3 + spacer + SW13_POS1 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A9] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A10] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A11] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A12] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A13] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A14] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0" + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A15] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A16] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A17] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A18] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A19] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A20] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A21] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A22] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3 + spacer + SW13_POS2 + spacer + SW14_POS3 + spacer + "40:0 41:0";
                            #endregion VSA_HPF3p5GHz TO DUTPORTS

                            #region POWER SENSOR PORT TO DUTPORT
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A1] = SW2_POS1 + spacer + SW3_POS0 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A2] = SW2_POS1 + spacer + SW3_POS1 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A3] = SW2_POS1 + spacer + SW3_POS2 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A4] = SW2_POS1 + spacer + SW3_POS3 + spacer + SW13_POS0 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A5] = SW4_POS1 + spacer + SW5_POS0 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A6] = SW4_POS1 + spacer + SW5_POS1 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A7] = SW4_POS1 + spacer + SW5_POS2 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A8] = SW4_POS1 + spacer + SW5_POS3 + spacer + SW13_POS1 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A9] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A10] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A11] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A12] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A13] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A14] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0" + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A15] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A16] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A17] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A18] = SW6_POS1 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A19] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS0 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A20] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS1 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A21] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS2 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A22] = SW6_POS1 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "24:0" + spacer + SW12_POS3 + spacer + SW13_POS2 + spacer + SW14_POS0 + spacer + "40:1 41:0";
                            #endregion POWER SENSOR PORT TO DUTPORT

                            #region NETWORK ANALYZER TO DUTPORT PATHS
                            settingsTemp[InstrPort.N1, DutPort.A1] = SW3_POS0 + spacer + SW2_POS2;
                            settingsTemp[InstrPort.N1, DutPort.A2] = SW3_POS1 + spacer + SW2_POS2;
                            settingsTemp[InstrPort.N1, DutPort.A3] = SW3_POS2 + spacer + SW2_POS2;
                            settingsTemp[InstrPort.N1, DutPort.A4] = SW3_POS3 + spacer + SW2_POS2;

                            settingsTemp[InstrPort.N2, DutPort.A5] = SW5_POS0 + spacer + SW4_POS2;
                            settingsTemp[InstrPort.N2, DutPort.A6] = SW5_POS1 + spacer + SW4_POS2;
                            settingsTemp[InstrPort.N2, DutPort.A7] = SW5_POS2 + spacer + SW4_POS2;
                            settingsTemp[InstrPort.N2, DutPort.A8] = SW5_POS3 + spacer + SW4_POS2;

                            settingsTemp[InstrPort.N3, DutPort.A9] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS1 + spacer + "18:0";
                            settingsTemp[InstrPort.N3, DutPort.A10] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS2 + spacer + "19:0";
                            settingsTemp[InstrPort.N3, DutPort.A11] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS3 + spacer + "20:0";
                            settingsTemp[InstrPort.N3, DutPort.A12] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS0 + spacer + "21:0";
                            settingsTemp[InstrPort.N3, DutPort.A13] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS1 + spacer + "22:0";
                            settingsTemp[InstrPort.N3, DutPort.A14] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS2 + spacer + "23:0";

                            settingsTemp[InstrPort.N4, DutPort.A9] = SW15_POS2 + spacer + "29:0" + spacer + SW9_POS1 + spacer + "18:1";
                            settingsTemp[InstrPort.N4, DutPort.A10] = SW15_POS2 + spacer + "29:0" + spacer + SW9_POS2 + spacer + "19:1";
                            settingsTemp[InstrPort.N4, DutPort.A11] = SW15_POS2 + spacer + "29:0" + spacer + SW9_POS3 + spacer + "20:1";
                            settingsTemp[InstrPort.N4, DutPort.A12] = SW15_POS2 + spacer + "29:1" + spacer + SW10_POS0 + spacer + "21:1";
                            settingsTemp[InstrPort.N4, DutPort.A13] = SW15_POS2 + spacer + "29:1" + spacer + SW10_POS1 + spacer + "22:1";
                            settingsTemp[InstrPort.N4, DutPort.A14] = SW15_POS2 + spacer + "29:1" + spacer + SW10_POS2 + spacer + "23:1";

                            settingsTemp[InstrPort.N3, DutPort.A15] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS0;
                            settingsTemp[InstrPort.N3, DutPort.A16] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS1;
                            settingsTemp[InstrPort.N3, DutPort.A17] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS2;
                            settingsTemp[InstrPort.N3, DutPort.A18] = SW6_POS2 + spacer + "12:0" + spacer + SW7_POS0 + spacer + "17:0" + spacer + SW11_POS3;
                            settingsTemp[InstrPort.N3, DutPort.A19] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS3 + spacer + "24:0" + spacer + SW12_POS0;
                            settingsTemp[InstrPort.N3, DutPort.A20] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS3 + spacer + "24:0" + spacer + SW12_POS1;
                            settingsTemp[InstrPort.N4, DutPort.A21] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS3 + spacer + "24:0" + spacer + SW12_POS2;
                            settingsTemp[InstrPort.N4, DutPort.A22] = SW6_POS2 + spacer + "12:1" + spacer + SW8_POS3 + spacer + "24:0" + spacer + SW12_POS3;

                            #endregion NETWORK ANALYZER TO DUTPORT PATHS

                        }

                        #endregion

                        break;
                    case Rev.E:

                        # region SMPAD_RevE
                        {
                            string VSA_Bypass = " 74:0 75:0 78:0 79:0";
                            string VSA_HPF1p3GHz = " 74:1 76:0 78:1 79:0";
                            string VSA_HPF2p7GHz = " 74:1 76:1 77:0 79:1";
                            string VSA_HPF3p5GHz = " 74:1 76:1 77:1 79:1";
                            string PS_AMP_HPF6GHz = " 74:0 75:1 78:0 79:0";

                            settingsTemp[InstrPort.VSG, DutPort.A1] = "00:0 01:0 03:0 04:0 06:0";
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "00:0 01:0 03:0 04:0 06:1";
                            settingsTemp[InstrPort.VSG, DutPort.A3] = "00:0 01:0 03:1 05:0 07:0";
                            settingsTemp[InstrPort.VSG, DutPort.A4] = "00:0 01:0 03:1 05:0 07:1";
                            settingsTemp[InstrPort.VSG, DutPort.A5] = "00:0 01:1 10:0 11:0 13:0";
                            settingsTemp[InstrPort.VSG, DutPort.A6] = "00:0 01:1 10:0 11:0 13:1";
                            settingsTemp[InstrPort.VSG, DutPort.A7] = "00:0 01:1 10:1 12:0 14:0";
                            settingsTemp[InstrPort.VSG, DutPort.A8] = "00:0 01:1 10:1 12:0 14:1";
                            settingsTemp[InstrPort.VSG, DutPort.A9] = "00:1 02:0 17:0 19:0 21:0";
                            settingsTemp[InstrPort.VSG, DutPort.A10] = "00:1 02:0 17:1 20:0 22:0 23:0";
                            settingsTemp[InstrPort.VSG, DutPort.A11] = "00:1 02:0 17:1 20:0 22:0 23:1";
                            settingsTemp[InstrPort.VSG, DutPort.A12] = "00:1 02:0 17:1 20:0 22:1 24:0";
                            settingsTemp[InstrPort.VSG, DutPort.A13] = "00:1 02:0 17:1 20:0 22:1 24:1";
                            settingsTemp[InstrPort.VSG, DutPort.A14] = "00:1 02:1 27:0 28:0 30:0";
                            settingsTemp[InstrPort.VSG, DutPort.A15] = "00:1 02:1 27:1 29:0 33:0 35:0";
                            settingsTemp[InstrPort.VSG, DutPort.A16] = "00:1 02:1 27:1 29:0 33:1 35:0";
                            settingsTemp[InstrPort.VSG, DutPort.A17] = "00:1 02:1 27:1 29:0 34:0 35:1";
                            settingsTemp[InstrPort.VSG, DutPort.A18] = "00:1 02:1 27:1 29:0 34:1 35:1";



                            //DA_1
                            settingsTemp[InstrPort.VSG, DutPort.A19] = "00:1 02:0 17:0 19:0 21:1 37:0 38:0 40:0";
                            settingsTemp[InstrPort.VSG, DutPort.A20] = "00:1 02:0 17:0 19:0 21:1 37:0 38:1 41:0";
                            settingsTemp[InstrPort.VSG, DutPort.A21] = "00:1 02:0 17:0 19:0 21:1 37:1 39:0 45:0";
                            settingsTemp[InstrPort.VSG, DutPort.A22] = "00:1 02:0 17:0 19:0 21:1 37:1 39:1 46:0";

                            //VSA to RF Port
                            settingsTemp[InstrPort.VSA, DutPort.A1] = "04:1 06:0 08:0 09:0 47:0 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A2] = "04:1 06:1 08:0 09:0 47:0 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A3] = "05:1 07:0 08:1 09:0 47:0 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A4] = "05:1 07:1 08:1 09:0 47:0 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "11:1 13:0 15:0 16:0 47:1 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "11:1 13:1 15:0 16:0 47:1 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A7] = "12:1 14:0 15:1 16:0 47:1 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A8] = "12:1 14:1 15:1 16:0 47:1 73:1" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A9] = "19:1 21:0 25:0 26:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "20:1 22:0 23:0 25:1 26:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "20:1 22:0 23:1 25:1 26:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A12] = "20:1 22:1 24:0 25:1 26:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A13] = "20:1 22:1 24:1 25:1 26:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "28:1 30:0 31:0 32:0 72:1 73:0 74:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "29:1 31:1 32:0 33:0 35:0 72:1 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A16] = "29:1 31:1 32:0 33:1 35:0 72:1 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A17] = "29:1 31:1 32:0 34:0 35:1 72:1 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "29:1 31:1 32:0 34:1 35:1 72:1 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A19] = "19:1 21:1 25:0 26:0 37:0 38:0 40:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A20] = "19:1 21:1 25:0 26:0 37:0 38:1 41:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A21] = "19:1 21:1 25:0 26:0 37:1 39:0 45:0 72:0 73:0" + VSA_Bypass;
                            settingsTemp[InstrPort.VSA, DutPort.A22] = "19:1 21:1 25:0 26:0 37:1 39:1 46:0 72:0 73:0" + VSA_Bypass;
                            //settingsTemp[InstrPort.VSA, DutPort.VSAtoLOG_OUT] = "75:1 78:0 79:0";

                            //VSA thru VSA_HPF1p3GHz to RF Port
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A1] = "04:1 06:0 08:0 09:0 47:0 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A2] = "04:1 06:1 08:0 09:0 47:0 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A3] = "05:1 07:0 08:1 09:0 47:0 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A4] = "05:1 07:1 08:1 09:0 47:0 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A5] = "11:1 13:0 15:0 16:0 47:1 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A6] = "11:1 13:1 15:0 16:0 47:1 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A7] = "12:1 14:0 15:1 16:0 47:1 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A8] = "12:1 14:1 15:1 16:0 47:1 73:1" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A9] = "19:1 21:0 25:0 26:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A10] = "20:1 22:0 23:0 25:1 26:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A11] = "20:1 22:0 23:1 25:1 26:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A12] = "20:1 22:1 24:0 25:1 26:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A13] = "20:1 22:1 24:1 25:1 26:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A14] = "28:1 30:0 31:0 32:0 72:1 73:0 74:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A15] = "29:1 31:1 32:0 33:0 35:0 72:1 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A16] = "29:1 31:1 32:0 33:1 35:0 72:1 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A17] = "29:1 31:1 32:0 34:0 35:1 72:1 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A18] = "29:1 31:1 32:0 34:1 35:1 72:1 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A19] = "19:1 21:1 25:0 26:0 37:0 38:0 40:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A20] = "19:1 21:1 25:0 26:0 37:0 38:1 41:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A21] = "19:1 21:1 25:0 26:0 37:1 39:0 45:0 72:0 73:0" + VSA_HPF1p3GHz;
                            settingsTemp[InstrPort.VSA_HPF1p3GHz, DutPort.A22] = "19:1 21:1 25:0 26:0 37:1 39:1 46:0 72:0 73:0" + VSA_HPF1p3GHz;

                            //VSA thru VSA_HPF2p7GHz to RF Port
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A1] = "04:1 06:0 08:0 09:0 47:0 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A2] = "04:1 06:1 08:0 09:0 47:0 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A3] = "05:1 07:0 08:1 09:0 47:0 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "05:1 07:1 08:1 09:0 47:0 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "11:1 13:0 15:0 16:0 47:1 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "11:1 13:1 15:0 16:0 47:1 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A7] = "12:1 14:0 15:1 16:0 47:1 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A8] = "12:1 14:1 15:1 16:0 47:1 73:1" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A9] = "19:1 21:0 25:0 26:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A10] = "20:1 22:0 23:0 25:1 26:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A11] = "20:1 22:0 23:1 25:1 26:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A12] = "20:1 22:1 24:0 25:1 26:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A13] = "20:1 22:1 24:1 25:1 26:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = "28:1 30:0 31:0 32:0 72:1 73:0 74:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = "29:1 31:1 32:0 33:0 35:0 72:1 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A16] = "29:1 31:1 32:0 33:1 35:0 72:1 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A17] = "29:1 31:1 32:0 34:0 35:1 72:1 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A18] = "29:1 31:1 32:0 34:1 35:1 72:1 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A19] = "19:1 21:1 25:0 26:0 37:0 38:0 40:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A20] = "19:1 21:1 25:0 26:0 37:0 38:1 41:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A21] = "19:1 21:1 25:0 26:0 37:1 39:0 45:0 72:0 73:0" + VSA_HPF2p7GHz;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A22] = "19:1 21:1 25:0 26:0 37:1 39:1 46:0 72:0 73:0" + VSA_HPF2p7GHz;

                            //VSA thru VSA_HPF2p7GHz to RF Port
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A1] = "04:1 06:0 08:0 09:0 47:0 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A2] = "04:1 06:1 08:0 09:0 47:0 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A3] = "05:1 07:0 08:1 09:0 47:0 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A4] = "05:1 07:1 08:1 09:0 47:0 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A5] = "11:1 13:0 15:0 16:0 47:1 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A6] = "11:1 13:1 15:0 16:0 47:1 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A7] = "12:1 14:0 15:1 16:0 47:1 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A8] = "12:1 14:1 15:1 16:0 47:1 73:1" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A9] = "19:1 21:0 25:0 26:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A10] = "20:1 22:0 23:0 25:1 26:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A11] = "20:1 22:0 23:1 25:1 26:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A12] = "20:1 22:1 24:0 25:1 26:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A13] = "20:1 22:1 24:1 25:1 26:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A14] = "28:1 30:0 31:0 32:0 72:1 73:0 74:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A15] = "29:1 31:1 32:0 33:0 35:0 72:1 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A16] = "29:1 31:1 32:0 33:1 35:0 72:1 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A17] = "29:1 31:1 32:0 34:0 35:1 72:1 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A18] = "29:1 31:1 32:0 34:1 35:1 72:1 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A19] = "19:1 21:1 25:0 26:0 37:0 38:0 40:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A20] = "19:1 21:1 25:0 26:0 37:0 38:1 41:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A21] = "19:1 21:1 25:0 26:0 37:1 39:0 45:0 72:0 73:0" + VSA_HPF3p5GHz;
                            settingsTemp[InstrPort.VSA_HPF3p5GHz, DutPort.A22] = "19:1 21:1 25:0 26:0 37:1 39:1 46:0 72:0 73:0" + VSA_HPF3p5GHz;

                            //VSA thru PS_AMP_HPF6GHz to RF Port
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A1] = "04:1 06:0 08:0 09:0 47:0 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A2] = "04:1 06:1 08:0 09:0 47:0 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A3] = "05:1 07:0 08:1 09:0 47:0 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A4] = "05:1 07:1 08:1 09:0 47:0 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A5] = "11:1 13:0 15:0 16:0 47:1 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A6] = "11:1 13:1 15:0 16:0 47:1 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A7] = "12:1 14:0 15:1 16:0 47:1 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A8] = "12:1 14:1 15:1 16:0 47:1 73:1" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A9] = "19:1 21:0 25:0 26:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A10] = "20:1 22:0 23:0 25:1 26:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A11] = "20:1 22:0 23:1 25:1 26:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A12] = "20:1 22:1 24:0 25:1 26:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A13] = "20:1 22:1 24:1 25:1 26:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A14] = "28:1 30:0 31:0 32:0 72:1 73:0 74:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A15] = "29:1 31:1 32:0 33:0 35:0 72:1 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A16] = "29:1 31:1 32:0 33:1 35:0 72:1 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A17] = "29:1 31:1 32:0 34:0 35:1 72:1 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A18] = "29:1 31:1 32:0 34:1 35:1 72:1 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A19] = "19:1 21:1 25:0 26:0 37:0 38:0 40:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A20] = "19:1 21:1 25:0 26:0 37:0 38:1 41:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A21] = "19:1 21:1 25:0 26:0 37:1 39:0 45:0 72:0 73:0" + PS_AMP_HPF6GHz;
                            settingsTemp[InstrPort.PS_AMP_HPF6GHz, DutPort.A22] = "19:1 21:1 25:0 26:0 37:1 39:1 46:0 72:0 73:0" + PS_AMP_HPF6GHz;

                            // Network Analyzer connections
                            settingsTemp[InstrPort.N1, DutPort.A1] = "04:1 06:0 08:0 09:1";
                            settingsTemp[InstrPort.N1, DutPort.A2] = "04:1 06:1 08:0 09:1";
                            settingsTemp[InstrPort.N1, DutPort.A3] = "05:1 07:0 08:1 09:1";
                            settingsTemp[InstrPort.N1, DutPort.A4] = "05:1 07:1 08:1 09:1";

                            settingsTemp[InstrPort.N2, DutPort.A5] = "11:1 13:0 15:0 16:1";
                            settingsTemp[InstrPort.N2, DutPort.A6] = "11:1 13:1 15:0 16:1";
                            settingsTemp[InstrPort.N2, DutPort.A7] = "12:1 14:0 15:1 16:1";
                            settingsTemp[InstrPort.N2, DutPort.A8] = "12:1 14:1 15:1 16:1";

                            settingsTemp[InstrPort.N3, DutPort.A9] = "19:1 21:0 25:0 26:1";
                            settingsTemp[InstrPort.N3, DutPort.A10] = "20:1 22:0 23:0 25:1 26:1";
                            settingsTemp[InstrPort.N3, DutPort.A11] = "20:1 22:0 23:1 25:1 26:1";
                            settingsTemp[InstrPort.N3, DutPort.A12] = "20:1 22:1 24:0 25:1 26:1";
                            settingsTemp[InstrPort.N3, DutPort.A13] = "20:1 22:1 24:1 25:1 26:1";
                            settingsTemp[InstrPort.N3, DutPort.A19] = "19:1 21:1 25:0 26:1 37:0 38:0 40:0";
                            settingsTemp[InstrPort.N3, DutPort.A20] = "19:1 21:1 25:0 26:1 37:0 38:1 41:0";
                            settingsTemp[InstrPort.N3, DutPort.A21] = "19:1 21:1 25:0 26:1 37:1 39:0 45:0";
                            settingsTemp[InstrPort.N3, DutPort.A22] = "19:1 21:1 25:0 26:1 37:1 39:1 46:0";

                            settingsTemp[InstrPort.N4, DutPort.A14] = "28:1 30:0 31:0 32:1";
                            settingsTemp[InstrPort.N4, DutPort.A15] = "29:1 31:1 32:1 33:0 35:0";
                            settingsTemp[InstrPort.N4, DutPort.A16] = "29:1 31:1 32:1 33:1 35:0";
                            settingsTemp[InstrPort.N4, DutPort.A17] = "29:1 31:1 32:1 34:0 35:1";
                            settingsTemp[InstrPort.N4, DutPort.A18] = "29:1 31:1 32:1 34:1 35:1";
                            settingsTemp[InstrPort.N4, DutPort.A19] = "28:1 30:1 31:0 32:1 40:1 42:0 44:0";
                            settingsTemp[InstrPort.N4, DutPort.A20] = "28:1 30:1 31:0 32:1 41:1 42:1 44:0";
                            settingsTemp[InstrPort.N4, DutPort.A21] = "28:1 30:1 31:0 32:1 43:0 44:1 45:1";
                            settingsTemp[InstrPort.N4, DutPort.A22] = "28:1 30:1 31:0 32:1 43:1 44:1 46:1";
                        }

                        # endregion

                        break;

                    case Rev.JM:

                        #region Rev JM - Joker Modular Switch
                       
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "04:0 05:0 06:0 08:0 09:0 10:0 12:0 16:0 17:0 20:0 21:0 24:0 25:0 28:0 29:0"; // None

                            settingsTemp[InstrPort.N1, DutPort.A1] = "04:0 05:0 06:0";                                                  //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A2] = "04:0 05:1 06:0";                                                  //TX_MB2_NA
                            settingsTemp[InstrPort.N1, DutPort.A3] = "04:1 05:0 06:0";                                                  //TX_HB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A4] = "04:1 05:0 06:1";                                                  //TX_HB2_NA

                            settingsTemp[InstrPort.N2, DutPort.A5] = "08:0 09:0 10:0 12:0";                                             //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A6] = "08:0 09:1 10:0 12:0";                                             //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A7] = "08:1 09:0 10:0 12:0";                                             //ANT3_NA

                            settingsTemp[InstrPort.TERM, DutPort.A5] = "08:1 10:1";                                                     //ANT1_TERM
                            settingsTemp[InstrPort.TERM, DutPort.A6] = "08:1 10:1";                                                     //ANT2_TERM
                            settingsTemp[InstrPort.TERM, DutPort.A7] = "08:1 10:1";                                                     //ANT3_TERM

                            settingsTemp[InstrPort.NS, DutPort.A5] = "08:0 09:0 10:0 12:1";                                             //NS_ANT1
                            settingsTemp[InstrPort.NS, DutPort.A6] = "08:0 09:1 10:0 12:1";                                             //NS_ANT1
                            settingsTemp[InstrPort.NS, DutPort.A7] = "08:1 09:0 10:0 12:1";                                             //NS_ANT1

                            settingsTemp[InstrPort.N3, DutPort.A10] = "16:0 17:0";                                                      //OUT1_NA
                            settingsTemp[InstrPort.N3, DutPort.A11] = "16:1 17:0";                                                      //DRX_OUT_NA
                            settingsTemp[InstrPort.N4, DutPort.A12] = "20:0 21:0";                                                      //OUT2_NA
                            settingsTemp[InstrPort.N4, DutPort.A13] = "20:1 21:0";                                                      //MLB_IN_NA
                            settingsTemp[InstrPort.N5, DutPort.A14] = "24:0 25:0";                                                      //OUT3_NA
                            settingsTemp[InstrPort.N5, DutPort.A15] = "24:1 25:0";                                                      //GSM_IN_NA
                            settingsTemp[InstrPort.N6, DutPort.A16] = "28:0 29:0";                                                      //OUT4_NA
                            settingsTemp[InstrPort.N6, DutPort.A17] = "28:1 29:0";                                                      //MIMO_OUT_NA

                        #endregion

                        break;

                    case Rev.Y2DNightHawk:
                        #region Switch Matrix Box Rev Nighthawk
                        {
                            ///////////////////////////// for Night-Hwak Mario

                            const string VSG_Amp = "00:1 01:1 02:1 ";
                            const string VSG_Bypass = "00:0 01:0 02:0 ";
                            const string VSA_Bypass = "30:0 31:1 34:1 35:1 36:0";
                            const string VSA_HPF2p7GHz = "30:1 31:1 32:0 33:0 34:1 35:0 36:1";
                            const string VSA_HPF2p7GHz_Logout = "30:1 31:1 32:0 33:0 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Logout2 = "30:1 31:1 32:1 33:1 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Extout = "30:1 31:1 32:0 34:0 35:0 36:1"; //For H2 CPL method

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "00:0"; // Terminate ANT1 WITH 50ohm at NF-T1 externally//
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "00:0 01:0 03:0 04:1 08:0"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSG, DutPort.A12] = VSG_Amp + "03:0 04:0 05:0 07:0 64:0";                            //FBRx_IN
                            settingsTemp[InstrPort.VSG, DutPort.A2] = VSG_Amp + "03:0 04:0 05:1 06:0 07:0";                             //TX_n77_In1
                            settingsTemp[InstrPort.VSG, DutPort.A3] = VSG_Amp + "03:0 04:0 05:1 06:1 07:0";                             //TX_n79_In

                            settingsTemp[InstrPort.VSG, DutPort.A4] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:0";                   //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A14] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1 65:0";             //ANT2
                                                                                                                                        //settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1";                 //n77n79_OUT
                            settingsTemp[InstrPort.VSG, DutPort.A15] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1 65:1";                       //n77n79_OUT

                            //settingsTemp[InstrPort.VSG, DutPort.A4] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:0";                   //ANT1
                            //settingsTemp[InstrPort.VSG, DutPort.A5] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1";                   //ANT2
                            //settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 08:1 09:0 10:1 11:1";                             //ANT3

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A12] = VSG_Amp + "03:0 04:0 05:0 07:0 64:0";                            //FBRX_IN
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A2] = VSG_Amp + "03:0 04:0 05:1 06:0 07:0";                             //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A3] = VSG_Amp + "03:0 04:0 05:1 06:1 07:0";                             //TX_n79_In


                            settingsTemp[InstrPort.VSA, DutPort.A4] = "08:0 09:0 10:0 11:0 12:0 13:0 18:1 19:1 24:0 27:0 29:0 " + VSA_Bypass;                 //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 65:0 " + VSA_Bypass;           //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 65:1 " + VSA_Bypass;                     //ANT3

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "08:1 09:1 10:0 11:1 12:0 13:0 24:1 27:0 29:0 30:1 31:1 33:0 34:1 35:0 36:1";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = "08:1 09:1 10:0 11:1 12:0 13:1 24:1 27:0 29:0 30:1 31:1 33:1 34:1 35:0 36:1 65:0";   //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = "08:1 09:1 10:0 11:1 12:0 13:1 24:1 27:0 29:0 30:1 31:1 33:1 34:1 35:0 36:1 65:1";   //n77n79_OUT

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A4] = "08:0 09:0 10:0 11:0 12:0 13:0 18:1 19:1 24:0 27:0 29:0 " + VSA_Bypass;  //ANT1
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A4] = "08:0 09:0 10:0 11:0 12:0 13:0 18:1 19:1 24:0 27:0 29:0 30:1 31:1 33:0 34:1 35:0 36:1";  //ANT1

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A14] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 65:0 " + VSA_Bypass;     //ANT2
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A14] = "08:1 09:1 10:0 11:1 12:0 13:1 24:1 27:0 29:0 30:1 31:1 33:1 34:1 35:0 36:1 65:0";   //ANT2

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A15] = "08:0 09:0 10:0 11:0 12:0 13:1 18:1 19:1 24:0 27:0 29:0 65:1 " + VSA_Bypass;     //n77n79_OUT
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A15] = "08:1 09:1 10:0 11:1 12:0 13:1 24:1 27:0 29:0 30:1 31:1 33:1 34:1 35:0 36:1 65:1";  //n77n79_OUT

                            settingsTemp[InstrPort.VSA, DutPort.A16] = "08:1 09:1 10:1 11:1 14:0 15:1 25:0 28:0 29:1 17:0 21:0 23:0 24:1 27:0 66:0 " + VSA_Bypass;             //n77_OUT1                           
                            settingsTemp[InstrPort.VSA, DutPort.A24] = "08:1 09:1 10:1 11:1 16:0 17:1 25:1 28:0 29:1 15:0 21:0 23:0 24:1 27:0 18:1 19:1 70:0 " + VSA_Bypass;   //n79_OUT2
                            settingsTemp[InstrPort.VSA, DutPort.A19] = "08:1 09:1 10:1 11:1 20:0 21:1 26:0 28:1 29:1 15:0 17:0 23:0 24:1 27:0 67:1 " + VSA_Bypass;       //FBRx_OUT
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "08:1 09:1 10:1 11:1 20:0 21:1 26:0 28:1 29:1 15:0 17:0 23:0 24:1 27:0 67:0 " + VSA_Bypass;       //n79_OUT1
                            settingsTemp[InstrPort.VSA, DutPort.A20] = "08:1 09:1 10:1 11:1 22:0 23:1 26:1 28:1 29:1 15:0 17:0 21:0 24:1 27:0 68:0 " + VSA_Bypass;            //n77_OUT3 

                            /////////////// For Joker RF2

                            settingsTemp[InstrPort.N1, DutPort.A2] = "00:0 02:0 03:0 04:1 05:1 06:0 07:0";                             //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A3] = "00:0 02:0 03:0 04:1 05:1 06:1 07:0";                             //TX_HB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A12] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:0";                        //TX_MB2_NA
                            settingsTemp[InstrPort.N1, DutPort.A13] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:1";                        //TX_HB2_NA

                            settingsTemp[InstrPort.N2, DutPort.A4] = "08:0 09:1 10:0 12:0 13:0";                                       //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A14] = "08:0 09:1 10:0 12:0 13:1 65:0";                                  //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A15] = "08:0 09:1 10:0 12:0 13:1 65:1";
                            settingsTemp[InstrPort.N2, DutPort.A22] = "08:0 09:1 10:1 69:0"; 

                            settingsTemp[InstrPort.N3, DutPort.A16] = "17:0 66:0";                                                      //OUT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A24] = "15:0 70:0";                                                      //OUT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A18] = "21:0 67:0";                                                      //OUT3_NA
                            settingsTemp[InstrPort.N6, DutPort.A20] = "23:0 68:0";                                                      //OUT4_NA

                            settingsTemp[InstrPort.N3, DutPort.A17] = "17:0 66:1";                                                      //DRX_NA
                            settingsTemp[InstrPort.N4, DutPort.A25] = "15:0 70:1";                                                      //MLB_NA
                            settingsTemp[InstrPort.N5, DutPort.A19] = "21:0 67:1";                                                      //GSM_NA
                            settingsTemp[InstrPort.N6, DutPort.A21] = "23:0 68:1";                                                      //MIMO_NA

                            settingsTemp[InstrPort.NS, DutPort.A4] = "08:0 09:1 10:0 12:1 13:0";                                  //NS_ANT1
                            settingsTemp[InstrPort.NS, DutPort.A14] = "08:0 09:1 10:0 12:1 13:1 65:0";                             //NS_ANT2
                            settingsTemp[InstrPort.NS, DutPort.A15] = "08:0 09:1 10:0 12:1 13:1 65:1";                             //NS_ANT3


                            #endregion Switch Matrix Box Rev Nighthawk
                        }
                        break;

                    case Rev.MD_RF1:
                        #region Switch Matrix Box Rev Nighthawk
                        {
                            /////////////////////////// for Night-Hwak Mario

                            const string VSG_Amp = "00:1 01:1 02:1 ";
                            const string VSG_Bypass = "00:0 01:0 02:0 ";
                            const string VSA_Bypass = "30:0 31:1 34:1 35:1 36:0";
                            const string VSA_HPF2p7GHz = "30:1 31:1 32:0 33:0 34:1 35:0 36:1";
                            const string VSA_HPF2p7GHz_Logout = "30:1 31:1 32:0 33:0 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Logout2 = "30:1 31:1 32:1 33:1 34:1 35:0 36:1"; //For H2 CPL method
                            const string VSA_HPF2p7GHz_Extout = "30:1 31:1 32:0 34:0 35:0 36:1"; //For H2 CPL method

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "00:0"; // Terminate ANT1 WITH 50ohm at NF-T1 externally//
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "00:0 01:0 03:0 04:1 08:0"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            //Modular Switch Added by Mario
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "04:0 08:0 09:0";                                       //TX_n77_In1
                            settingsTemp[InstrPort.VSG, DutPort.A3] = "04:0 08:0 09:1";                                       //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A12] = "04:0 08:1 10:0";                                      //FBRx_IN

                            settingsTemp[InstrPort.VSG, DutPort.A4] = "04:1 16:1 17:0 18:0";                   //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A14] = "04:1 16:1 17:0 18:1";                  //ANT2
                                                                                                                                        //settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1";                 //n77n79_OUT
                            settingsTemp[InstrPort.VSG, DutPort.A15] = "04:1 16:1 17:1 19:0";                  //n77n79_OUT
                            settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL

                            //settingsTemp[InstrPort.VSG, DutPort.A4] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:0";                   //ANT1
                            //settingsTemp[InstrPort.VSG, DutPort.A5] = VSG_Amp + "03:1 08:1 09:0 10:0 11:0 12:0 13:1";                   //ANT2
                            //settingsTemp[InstrPort.VSG, DutPort.A6] = VSG_Amp + "03:1 08:1 09:0 10:1 11:1";                             //ANT3


                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A2] = "04:0 08:0 09:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A3] = "04:0 08:0 09:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A12] = "04:0 08:1 10:0";                           //FBRX_IN
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL


                            settingsTemp[InstrPort.VSA, DutPort.A4] = "16:0 17:0 18:0 05:0 36:1 38:1";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "16:0 17:0 18:1 05:0 36:1 38:1";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "16:0 17:1 19:0 05:0 36:1 38:1";               //ANT3
                            

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "16:0 17:0 18:0 5:1 28:0 29:0 36:0 37:0";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = "16:0 17:0 18:1 5:1 28:0 29:1 36:0 37:0";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = "16:0 17:1 19:0 5:1 28:1 30:0 36:0 37:0";   //n77n79_OUT

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A4] = "16:0 17:0 18:0 05:0 36:1 38:1";  //ANT1
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A4] = "16:0 17:0 18:0 5:1 28:0 29:0 36:0 37:0";  //ANT1

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A14] = "16:0 17:0 18:1 05:0 36:1 38:1";      //ANT2
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A14] = "16:0 17:0 18:1 5:1 28:0 29:1 36:0 37:0";   //ANT2

                            settingsTemp[InstrPort.HMUtoVSA, DutPort.A15] = "16:0 17:1 19:0 05:0 36:1 38:1";     //n77n79_OUT
                            settingsTemp[InstrPort.HMUtoVSA_HPF2p7GHz, DutPort.A15] = "16:0 17:1 19:0 5:1 28:1 30:0 36:0 37:0";  //n77n79_OUT

                            settingsTemp[InstrPort.VSA, DutPort.A16] = "32:0 33:0 36:1 38:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "32:0 33:1 36:1 38:0";            //n79_OUT1     
                            settingsTemp[InstrPort.VSA, DutPort.A20] = "32:1 34:0 36:1 38:0";            //n77_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A24] = "32:1 34:1 36:1 38:0";            //n79_OUT2

                            settingsTemp[InstrPort.VSA, DutPort.A19] = "36:0 37:1";       //FBRx_OUT
                                                        

                            /////////////// For Joker RF2

                            settingsTemp[InstrPort.N1, DutPort.A2] = "00:0 02:0 03:0 04:1 05:1 06:0 07:0";                             //TX_MB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A3] = "00:0 02:0 03:0 04:1 05:1 06:1 07:0";                             //TX_HB1_NA
                            settingsTemp[InstrPort.N1, DutPort.A12] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:0";                        //TX_MB2_NA
                            settingsTemp[InstrPort.N1, DutPort.A13] = "00:0 02:0 03:0 04:1 05:0 06:0 07:0 64:1";                        //TX_HB2_NA

                            settingsTemp[InstrPort.N2, DutPort.A4] = "08:0 09:1 10:0 12:0 13:0";                                       //ANT1_NA
                            settingsTemp[InstrPort.N2, DutPort.A14] = "08:0 09:1 10:0 12:0 13:1 65:0";                                  //ANT2_NA
                            settingsTemp[InstrPort.N2, DutPort.A15] = "08:0 09:1 10:0 12:0 13:1 65:1";
                            settingsTemp[InstrPort.N2, DutPort.A22] = "08:0 09:1 10:1 69:0";

                            settingsTemp[InstrPort.N3, DutPort.A16] = "17:0 66:0";                                                      //OUT1_NA
                            settingsTemp[InstrPort.N4, DutPort.A24] = "15:0 70:0";                                                      //OUT2_NA
                            settingsTemp[InstrPort.N5, DutPort.A18] = "21:0 67:0";                                                      //OUT3_NA
                            settingsTemp[InstrPort.N6, DutPort.A20] = "23:0 68:0";                                                      //OUT4_NA

                            settingsTemp[InstrPort.N3, DutPort.A17] = "17:0 66:1";                                                      //DRX_NA
                            settingsTemp[InstrPort.N4, DutPort.A25] = "15:0 70:1";                                                      //MLB_NA
                            settingsTemp[InstrPort.N5, DutPort.A19] = "21:0 67:1";                                                      //GSM_NA
                            settingsTemp[InstrPort.N6, DutPort.A21] = "23:0 68:1";                                                      //MIMO_NA

                            settingsTemp[InstrPort.NS, DutPort.A4] = "08:0 09:1 10:0 12:1 13:0";                                  //NS_ANT1
                            settingsTemp[InstrPort.NS, DutPort.A14] = "08:0 09:1 10:0 12:1 13:1 65:0";                             //NS_ANT2
                            settingsTemp[InstrPort.NS, DutPort.A15] = "08:0 09:1 10:0 12:1 13:1 65:1";                             //NS_ANT3


                            #endregion Switch Matrix Box Rev Nighthawk
                        }
                        break;

                    case Rev.EDAM_Modular_RF1:
                        #region Switch Matrix Box Rev EDAM
                        {                           
                            const string VSA_Bypass = "30:0 31:1 34:1 35:1 36:0"; //Not used
                            const string VSA_HPF2p7GHz = "30:1 31:1 32:0 33:0 34:1 35:0 36:1";  //Not used                          

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "05:1 18:0 17:0 16:0"; // Terminate ANT1 WITH 50ohm
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "04:1 16:1 17:1 19:1"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSG, DutPort.A2] = "04:0 08:0 09:0";                   //TX_n77_In
                            settingsTemp[InstrPort.VSG, DutPort.A3] = "04:0 08:0 09:1";                   //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A12] = "04:0 08:1 10:0";                  //FBRx_IN

                            settingsTemp[InstrPort.VSG, DutPort.A4] = "04:1 16:1 17:0 18:0";              //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A14] = "04:1 16:1 17:0 18:1";             //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A15] = "04:1 16:1 17:1 19:0";             //ANTU
                            settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL, Not used 50 Ohm Terminated

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A2] = "04:0 08:0 09:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A3] = "04:0 08:0 09:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A12] = "04:0 08:1 10:0";                           //FBRX_IN
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL

                            settingsTemp[InstrPort.VSA, DutPort.A4] = "16:0 17:0 18:0 05:0 36:1 38:1 40:0 41:0";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A14] = "16:0 17:0 18:1 05:0 36:1 38:1 40:0 41:0";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A15] = "16:0 17:1 19:0 05:0 36:1 38:1 40:0 41:0";               //ANT3

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "16:0 17:0 18:0 5:1 28:0 29:0 36:0 37:0 40:1 41:1";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A14] = "16:0 17:0 18:1 5:1 28:0 29:1 36:0 37:0 40:1 41:1";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A15] = "16:0 17:1 19:0 5:1 28:1 30:0 36:0 37:0 40:1 41:1";   //n77n79_OUT

                            settingsTemp[InstrPort.VSA, DutPort.A16] = "32:0 33:0 36:1 38:0 40:0 41:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "32:0 33:1 36:1 38:0 40:0 41:0";            //n79_OUT1     
                            settingsTemp[InstrPort.VSA, DutPort.A20] = "32:1 34:0 36:1 38:0 40:0 41:0";            //n77_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A24] = "32:1 34:1 36:1 38:0 40:0 41:0";            //n79_OUT2

                            settingsTemp[InstrPort.VSA, DutPort.A19] = "36:0 37:1 40:0 41:0";       //FBRx_OUT                         
                        }
                        #endregion Switch Matrix Box Rev EDAM
                        break;

                    case Rev.Modular_RF1_QUADSITE:
                        #region Switch Matrix Box Rev EDAM Quadsite
                        {
                            //Site 1
                            const string VSA_Bypass = "05:1 24:1 26:1"; //Not used
                            const string VSA_HPF2p7GHz = "16:1 18:1 24:0 25:0";  //Not used                          

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "05:1 12:0 13:0 14:0"; // Terminate ANT1 WITH 50ohm
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "04:1 12:1 13:1 15:1"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSG, DutPort.A1] = "04:0 08:0 09:0";                   //TX_n77_In
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "04:0 08:0 09:1";                   //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A7] = "04:0 08:1 10:1";                  //FBRx_IN

                            settingsTemp[InstrPort.VSG, DutPort.A4] = "04:1 12:1 13:0 14:0";              //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A5] = "04:1 12:1 13:0 14:1";             //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A6] = "04:1 12:1 13:1 15:0";             //ANTU
                            //settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL, Not used 50 Ohm Terminated

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A1] = "04:0 08:0 09:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A2] = "04:0 08:0 09:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A7] = "04:0 08:1 10:1";                           //FBRX_IN
                            //settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL

                            settingsTemp[InstrPort.VSA, DutPort.A4] = "24:1 26:1 18:0 05:0 12:0 13:0 14:0";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "24:1 26:1 18:0 05:0 12:0 13:0 14:1";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "24:1 26:1 18:0 05:0 12:0 13:1 15:0"; ;               //ANTU

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "24:0 25:0 16:0 17:0";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "24:0 25:0 16:0 17:1";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "24:0 25:0 16:1 18:0";   //ANTU

                            settingsTemp[InstrPort.VSA, DutPort.A8] = "24:1 26:0 20:0 21:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A9] = "24:1 26:0 20:0 21:1";            //n77_OUT2     
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "24:1 26:0 20:1 22:0";            //n79_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "24:1 26:0 20:1 22:1";            //n79_OUT4

                            settingsTemp[InstrPort.VSA, DutPort.A12] = "24:0 25:1";       //FBRx_OUT        

                            //Site 2
                            const string VSA_Bypass_1 = "36:1 38:1 57:1"; //Not used
                            const string VSA_HPF2p7GHz_1 = "36:0 37:0 44:1 46:1";  //Not used                          

                            settingsTemp[InstrPort.TERM, DutPort.A17] = "50:0 49:0 48:0 45:1"; // Terminate ANT1 WITH 50ohm
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "56:1 48:1 49:1 51:1"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass_1;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz_1;

                            settingsTemp[InstrPort.VSG, DutPort.A13] = "56:0 52:0 53:0";                   //TX_n77_In
                            settingsTemp[InstrPort.VSG, DutPort.A14] = "56:0 52:0 53:1";                   //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A16] = "56:0 52:1 54:1";                  //FBRx_IN

                            settingsTemp[InstrPort.VSG, DutPort.A17] = "56:1 48:1 49:0 50:0";              //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A18] = "56:1 48:1 49:0 50:1";             //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A19] = "56:1 48:1 49:1 51:0";             //ANTU
                            //settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL, Not used 50 Ohm Terminated

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "56:0 52:0 53:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A14] = "56:0 52:0 53:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A16] = "56:0 52:1 54:1";                           //FBRX_IN
                            //settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL

                            settingsTemp[InstrPort.VSA, DutPort.A17] = "36:1 38:1 57:0 48:0 49:0 50:0";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "36:1 38:1 57:0 48:0 49:0 50:1";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A19] = "36:1 38:1 57:0 48:0 49:1 51:0";               //ANTU

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A17] = "36:0 37:0 44:0 45:0";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A18] = "36:0 37:0 44:0 45:1";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A19] = "36:0 37:0 44:1 46:0";   //ANTU

                            settingsTemp[InstrPort.VSA, DutPort.A20] = "36:1 38:0 40:0 41:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A21] = "36:1 38:0 40:0 41:1";            //n77_OUT2    
                            settingsTemp[InstrPort.VSA, DutPort.A22] = "36:1 38:0 40:1 42:0";            //n79_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A23] = "36:1 38:0 40:1 42:1";            //n79_OUT4

                            settingsTemp[InstrPort.VSA, DutPort.A24] = "36:0 37:1";       //FBRx_OUT        
                        }
                        #endregion Switch Matrix Box Rev EDAM
                        break;

                    case Rev.NUWA_Modular_RF1_QUADSITE:
                        #region Switch Matrix Box Rev EDAM Quadsite
                        {
                            //Site 1
                            const string VSA_Bypass = "05:1 24:1 26:1"; //Not used
                            const string VSA_HPF2p7GHz = "16:1 18:1 24:0 25:0";  //Not used                          

                            settingsTemp[InstrPort.TERM, DutPort.A4] = "05:1 12:0 13:0 14:0"; // Terminate ANT1 WITH 50ohm
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "04:1 12:1 13:1 15:1"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz;

                            settingsTemp[InstrPort.VSG, DutPort.A1] = "04:0 08:0 09:0";                   //TX_n77_In
                            settingsTemp[InstrPort.VSG, DutPort.A2] = "04:0 08:0 09:1";                   //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A7] = "04:0 08:1 10:1";                  //TX_n77_IN2

                            settingsTemp[InstrPort.VSG, DutPort.A4] = "04:1 12:1 13:0 14:0";              //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A5] = "04:1 12:1 13:0 14:1";             //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A6] = "04:1 12:1 13:1 15:0";             //ANTU
                            //settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL, Not used 50 Ohm Terminated

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A1] = "04:0 08:0 09:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A2] = "04:0 08:0 09:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A7] = "04:0 08:1 10:1";                           //TX_n77_IN2
                            //settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL

                            settingsTemp[InstrPort.VSA, DutPort.A4] = "24:1 26:1 18:0 05:0 12:0 13:0 14:0";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A5] = "24:1 26:1 18:0 05:0 12:0 13:0 14:1";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A6] = "24:1 26:1 18:0 05:0 12:0 13:1 15:0"; ;               //ANTU

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A4] = "24:0 25:0 16:0 17:0";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A5] = "24:0 25:0 16:0 17:1";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A6] = "24:0 25:0 16:1 18:0";   //ANTU

                            settingsTemp[InstrPort.VSA, DutPort.A8] = "24:1 26:0 20:0 21:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A9] = "24:1 26:0 20:0 21:1";            //n77_OUT2     
                            settingsTemp[InstrPort.VSA, DutPort.A10] = "24:1 26:0 20:1 22:0";            //n79_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A11] = "24:1 26:0 20:1 22:1";            //n79_OUT4

                            settingsTemp[InstrPort.VSA, DutPort.A12] = "24:0 25:1";       //FBRx_OUT        

                            //Site 2
                            const string VSA_Bypass_1 = "36:1 38:1 57:1"; //Not used
                            const string VSA_HPF2p7GHz_1 = "36:0 37:0 44:1 46:1";  //Not used                          

                            settingsTemp[InstrPort.TERM, DutPort.A17] = "50:0 49:0 48:0 45:1"; // Terminate ANT1 WITH 50ohm
                            settingsTemp[InstrPort.NONE, DutPort.NONE] = "56:1 48:1 49:1 51:1"; // None
                            settingsTemp[InstrPort.VSA, DutPort.NONE] = VSA_Bypass_1;
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.NONE] = VSA_HPF2p7GHz_1;

                            settingsTemp[InstrPort.VSG, DutPort.A13] = "56:0 52:0 53:0";                   //TX_n77_In
                            settingsTemp[InstrPort.VSG, DutPort.A14] = "56:0 52:0 53:1";                   //TX_n79_In
                            settingsTemp[InstrPort.VSG, DutPort.A16] = "56:0 52:1 54:1";                  //TX_n77_IN2

                            settingsTemp[InstrPort.VSG, DutPort.A17] = "56:1 48:1 49:0 50:0";              //ANT1                            
                            settingsTemp[InstrPort.VSG, DutPort.A18] = "56:1 48:1 49:0 50:1";             //ANT2
                            settingsTemp[InstrPort.VSG, DutPort.A19] = "56:1 48:1 49:1 51:0";             //ANTU
                            //settingsTemp[InstrPort.VSG, DutPort.A13] = "04:0 08:1 10:1";                  //ANTL, Not used 50 Ohm Terminated

                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "56:0 52:0 53:0";                           //TX_n77_In_1
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A14] = "56:0 52:0 53:1";                            //TX_n79_In
                            settingsTemp[InstrPort.VSGtoHMU, DutPort.A16] = "56:0 52:1 54:1";                           //TX_n77_IN2
                            //settingsTemp[InstrPort.VSGtoHMU, DutPort.A13] = "04:0 08:1 10:1";                           //ANTL

                            settingsTemp[InstrPort.VSA, DutPort.A17] = "36:1 38:1 57:0 48:0 49:0 50:0";                //ANT1
                            settingsTemp[InstrPort.VSA, DutPort.A18] = "36:1 38:1 57:0 48:0 49:0 50:1";               //ANT2
                            settingsTemp[InstrPort.VSA, DutPort.A19] = "36:1 38:1 57:0 48:0 49:1 51:0";               //ANTU

                            // H2 with coupler & 3GHz HPF
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A17] = "36:0 37:0 44:0 45:0";         //ANT1
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A18] = "36:0 37:0 44:0 45:1";        //ANT2
                            settingsTemp[InstrPort.VSA_HPF2p7GHz, DutPort.A19] = "36:0 37:0 44:1 46:0";   //ANTU

                            settingsTemp[InstrPort.VSA, DutPort.A20] = "36:1 38:0 40:0 41:0";            //n77_OUT1      
                            settingsTemp[InstrPort.VSA, DutPort.A21] = "36:1 38:0 40:0 41:1";            //n77_OUT2    
                            settingsTemp[InstrPort.VSA, DutPort.A22] = "36:1 38:0 40:1 42:0";            //n79_OUT3     
                            settingsTemp[InstrPort.VSA, DutPort.A23] = "36:1 38:0 40:1 42:1";            //n79_OUT4

                            settingsTemp[InstrPort.VSA, DutPort.A24] = "36:0 37:1";       //FBRx_OUT        
                        }
                        #endregion Switch Matrix Box Rev EDAM
                        break;


                }

                StoreSwitchSettings(settingsTemp);
            }

            private void StoreSwitchSettings(Dictionary.DoubleKey<InstrPort, DutPort, string> SwitchPathSettings)
            {
                foreach (InstrPort instr in SwitchPathSettings.Keys)
                {
                    foreach (DutPort port in SwitchPathSettings[instr].Keys)
                    {
                        string[] SwitchNoAndStatus = SwitchPathSettings[instr, port].Split(' ');

                        Dictionary<int, int> SwitchSettings = new Dictionary<int, int>();

                        // Generate dictionary for switch no and switch on/off status
                        for (int i = 0; i < SwitchNoAndStatus.Length; i++)
                        {
                            string[] tempSwitchNoAndStatus = SwitchNoAndStatus[i].Split(':');
                            SwitchSettings[Convert.ToInt32(tempSwitchNoAndStatus[0])] = Convert.ToInt32(tempSwitchNoAndStatus[1]);
                        }

                        SwitchPathSettingsDict[port, instr] = SwitchSettings;

                    }
                }
            }

        }

        public class None : EqSwitchMatrixBase
        {
            public override int Initialize()
            {
                return 0;
            }

            public override int ActivatePath(PortCombo SwitchPath)
            {
                return 0;
            }

            public override int ActivatePath_NonMSB(PortCombo SwitchPath)
            {
                return 0;
            }

            public override bool IsDefined(DutPort dutPort, InstrPort instrPort)
            {
                return true;
            }
        }

        public enum Rev
        {
            C, R, CEX, F, SE, E, O2, J1, Y2, JM, Y2D, Y2DPN, Y2DNightHawk, MD_RF1, EDAM_Modular_RF1, Modular_RF1_QUADSITE, NUWA_Modular_RF1_QUADSITE, None
        }

        public enum InstrPort
        {
            VSA, HMUtoVSA, VSA_HPF1p3GHz, VSA_HPF2p7GHz, HMUtoVSA_HPF2p7GHz, VSA_HPF3p5GHz,
            VSA_HPF1_FC_Below_1G, VSA_HPF2_FC_1p65G_To_2p25G, VSA_HPF3_FC_2p25G_To_3G,
            SAIn,
            VSG_Amp, VSG, VSGtoHMU, SGIn,
            PS_AMP_HPF6GHz,
            N1, N2, N3, N4, N5, N6, N7, N8, N9, N10,
            P_VST, P_FBAR, PwrSensorIn_From_O2,
            TERM,
            Tx_Leakage,
            NS, NS1, NS2, NS3,
            NONE
        }

        public enum DutPort
        {
            A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, A20, A21, A22, A23, A24, A25, A26, A27, A28, L1, L2, L3, R1, R2, R3, R,
            VST_1, VST_2, FBAR_1, FBAR_2,
            SGCplr, SAOut, PwrSensor, NONE
        }

        public class PortCombo
        {
            public InstrPort instrPort;
            public DutPort dutPort;

            public PortCombo(DutPort dutPort, InstrPort instrPort)
            {
                this.instrPort = instrPort;
                this.dutPort = dutPort;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                PortCombo p = (PortCombo)obj;
                return (dutPort == p.dutPort) && (instrPort == p.instrPort);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 29 + dutPort.GetHashCode();
                    hash = hash * 29 + instrPort.GetHashCode();
                    return hash;
                }
            }
            public static bool operator ==(PortCombo x, PortCombo y)
            {
                return Object.Equals(x, y);
            }
            public static bool operator !=(PortCombo x, PortCombo y)
            {
                return !(x == y);
            }

        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetVolumeInformation(string Volume, StringBuilder VolumeName, uint VolumeNameSize, out uint SerialNumber, out uint SerialNumberLength, out uint flags, StringBuilder fs, uint fs_size);
        public static string GetSerialNumber()
        {
            string strSMsn = "";
            string verboseSerialNumber = "XXXXXXXX";

            try
            {
                bool blnDetectedSM = false;
                string strSMfolder = "SwitchMatrixInfo";
                string strSMfile = "SwitchMatrixSN";
                string strSMfileNamePath = "C:\\Avago.ATF.Common\\DataLog\\" + strSMfolder + "\\" + strSMfile;
                DriveInfo[] mydrives = DriveInfo.GetDrives();

                mydrives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToArray();

                if (mydrives.Length == 0)
                {
                    MessageBox.Show("No USB Device found. MagicBox is not being detected.", "MagicBox");
                }
                else
                {
                    foreach (DriveInfo drive in mydrives)
                    {
                        if (blnDetectedSM) break;

                        #region Method#1 - Detect all drives (incl. C, E (SD card)

                        uint serialNum, serialNumLength, flags;
                        StringBuilder volumename = new StringBuilder(256);
                        StringBuilder fstype = new StringBuilder(256);
                        bool ok = false;

                        foreach (string drives in Environment.GetLogicalDrives())
                        {
                            ok = GetVolumeInformation(drives, volumename, (uint)volumename.Capacity - 1, out serialNum,
                                                   out serialNumLength, out flags, fstype, (uint)fstype.Capacity - 1);
                            if (ok)
                            {
                                //if (drive.Name == drives)
                                if (drives == "D:\\")
                                {
                                    //Check if this is MagicBox, by detecting the MagicBox folder....
                                    if (Directory.Exists(drives + "ExpertCalSystem.Data\\MagicBox"))
                                    {
                                        string encryptedSNfile = drives + @"ExpertCalSystem.Data\MagicBox\SerialNumber.dat";
                                        //MagicBox detected
                                        blnDetectedSM = true;

                                        //Check if the SN is different / same
                                        if (File.Exists(encryptedSNfile))
                                        {
                                            using (StreamReader r = new StreamReader(encryptedSNfile))
                                            {
                                                string line;
                                                int line_index = 0;
                                                while ((line = r.ReadLine()) != null)
                                                {
                                                    switch (line_index)
                                                    {
                                                        case 0:
                                                            verboseSerialNumber = line;
                                                            break;

                                                        case 1:
                                                            strSMsn = Encrypt.DecryptString(line, serialNum.ToString());
                                                            break;
                                                    }
                                                    line_index++;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            ok = false;
                        }

                        #endregion Method#1 - Detect all drives (incl. C, E (SD card)

                        if (verboseSerialNumber != strSMsn)
                        {
                            strSMsn = "INVALID";
                        }
                    }
                }
            }
            catch { }

            return strSMsn;
        }

        public static void SetSerialNumber(string serial_number)
        {
            try
            {
                bool blnDetectedSM = false;
                string strSMfolder = "SwitchMatrixInfo";
                string strSMfile = "SwitchMatrixSN";
                string strSMfileNamePath = "C:\\Avago.ATF.Common\\DataLog\\" + strSMfolder + "\\" + strSMfile;
                DriveInfo[] mydrives = DriveInfo.GetDrives();

                mydrives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToArray();

                if (mydrives.Length == 0)
                {
                    MessageBox.Show("No USB Device found. MagicBox is not being detected.", "MagicBox");
                }
                else
                {
                    foreach (DriveInfo drive in mydrives)
                    {
                        if (blnDetectedSM) break;

                        #region Method#1 - Detect all drives (incl. C, E (SD card)

                        uint serialNum, serialNumLength, flags;
                        StringBuilder volumename = new StringBuilder(256);
                        StringBuilder fstype = new StringBuilder(256);
                        bool ok = false;

                        foreach (string drives in Environment.GetLogicalDrives())
                        {
                            ok = GetVolumeInformation(drives, volumename, (uint)volumename.Capacity - 1, out serialNum,
                                                   out serialNumLength, out flags, fstype, (uint)fstype.Capacity - 1);
                            if (ok)
                            {
                                //if (drive.Name == drives)
                                if (drives == "D:\\")
                                {
                                    //Check if this is MagicBox, by detecting the MagicBox folder....
                                    if (Directory.Exists(drives + "ExpertCalSystem.Data\\MagicBox"))
                                    {
                                        string encryptedSNfile = drives + @"ExpertCalSystem.Data\MagicBox\SerialNumber.dat";
                                        //MagicBox detected
                                        blnDetectedSM = true;

                                        if (File.Exists(encryptedSNfile))
                                            File.Delete(encryptedSNfile);

                                        Thread.Sleep(10);

                                        using (StreamWriter w = new StreamWriter(encryptedSNfile, false))
                                        {
                                            w.WriteLine(serial_number);
                                            w.WriteLine(Encrypt.EncryptString(serial_number, serialNum.ToString()));
                                            w.Close();
                                        }

                                        File.SetAttributes(encryptedSNfile, FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
                                        break;
                                    }
                                }
                            }
                            ok = false;
                        }

                        #endregion Method#1 - Detect all drives (incl. C, E (SD card)
                    }
                }
            }
            catch { }
        }
    }


    public enum Operation
    {

        VSGtoTRX1,
        VSGtoTRX2,
        VSGtoTRX3,
        VSGtoTRX4,
        VSGtoTRX5,
        VSGtoANT1,
        VSGtoANT2,
        VSGtoANT_UAT,
        VSAtoANT1,
        VSAtoANT2,
        VSAtoRX1,
        VSAtoRX2,
        VSAtoRX3,
        VSAtoRX4,
        VSAtoRX5,
        ANTtoTERM,
        MeasureH2_ANT1,
        MeasureH2_ANT2,
        MeasureH2_ANT3,
        MeasureH2_ANT_UAT,
        MeasureH3_ANT1,
        MeasureH3_ANT2,
        MeasureH3_ANT3,
        MeasureH3_ANT_UAT,
        MeasureCpl,
        ENAtoRFIN,
        ENAtoRFOUT,
        ENAtoRFOUT_ANT1,
        ENAtoRFOUT_ANT2,
        ENAtoTX_Diff_Plus,
        ENAtoTX_Diff_Neg,
        ENAtoTX,
        ENAto2GTX,
        ENAtoRX,
        ENAtoRX2,
        ENAtoTRX2,
        ENAtoTRX3,
        ENAtoDCS_PCS_RX,
        ENAtoDRX,
        ENAtoCPL,
        MeasureH3_PS,
        MeasureH3_VSA,
        P_VSTtoVST_1,
        P_VSTtoVST_2,
        P_FBARtoFBAR_1,
        P_FBARtoFBAR_2,

        // Burhan - Added for SPAR Calibration

        // Add for O2M
        SGCplrToSGIn,
        SAOutToSAIn,
        PwrSensorToPwrSensorIn_From_O2,

        // Used for Joker..
        VSGtoTX,
        VSGtoTX1,
        VSGtoTX2,
        VSGtoTX3,
        VSGtoTXFor_HMU,
        VSGtoTX2For_HMU,
        VSGtoANT,
        VSGtoANT3,
        VSGtoANT4,
        VSAtoANT,
        VSAtoANT3,
        VSAtoANT4,
        HMUtoVSAtoANT,
        VSAtoANT_UAT,
        VSAtoRX,
        TxLeakage,
        MeasureH2,

        NStoAnt,
        NStoAnt2,
        NS1toAnt1,
        NS2toAnt2,
        NS3toAnt3,

        N1toTx,
        N2toTx,
        N2toAnt,
        N3toAnt,
        N4toAnt,
        N5toAnt,
        N3toRx,
        N4toRx,
        N5toRx,
        N6toRx,
        N7toRx,
        N8toRx,
        N9toRx,

        NONE,
    }

    public static class Encrypt
    {
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private const string initVector = "smpadmagicbox!@#";
        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;
        //Encrypt
        public static string EncryptString(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        //Decrypt
        public static string DecryptString(string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}
