using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using Keysight.KtMDsr.Interop;

namespace EqLib
{
    public partial class EqHSDIO
    {
        public class KeysightDSR : EqHSDIObase
        {
            public string WaitForTriggerPattern = "WaitForTrigger";
            public string TriggeredTestplanCache = "MyPatternExCache";
            public IKtMDsr2 driver;
            KtMDsrPatternSiteExecutionStateEnum TrigNotReady = KtMDsrPatternSiteExecutionStateEnum.KtMDsrPatternSiteExecutionStateWatchingForTrigger0;//KH
            KtMDsrPatternSiteExecutionStateEnum Complete = KtMDsrPatternSiteExecutionStateEnum.KtMDsrPatternSiteExecutionStateComplete;
            KtMDsrPatternSiteExecutionStateEnum CurrentState = KtMDsrPatternSiteExecutionStateEnum.KtMDsrPatternSiteExecutionStateWatchingForTrigger0;
            KtMDsrPatternSiteExecutionStateEnum Stopped = KtMDsrPatternSiteExecutionStateEnum.KtMDsrPatternSiteExecutionStateStopped;
            private const string mipiSiteAlias = "siteMipi";
            private const string eepromSiteAlias = "siteEeprom";
            private const string tempSenseSiteAlias = "siteTempSensor";
            public const string ppmuSiteAliasPrefix = "sitePpmu";
            private const string mipiDcLevelsAlias = "mipiDcLevels";
            private const string eepromDcLevelsAlias = "eepromDcLevels";
            private const string tempSenseDcLevelsAlias = "tempSensorDcLevels";
            private const string wftAlias = "wft", wftAllLowsAlias = "wftLows", wftRegIO = "wftRegIO", wftEeprom = "wftEeprom";
            private const string wftEepromRead= "wftEepromRead", wftEepromWrite = "wftEepromWrite", wftTempSense = "wftTempSensor", wftTempSenseSwitchOn = "wftTempSensorOn";
            private const string eepromReadAlias = "eepromRead", eepromWriteAlias = "eepromWrite", tempSenseAlias = "tempSensor", tempsenseSwitchOnAlias = "tempSensorOn";
            private int lastFailureCount = 0;
            private double Clock_Rate;
            private double compareDelay;
            private double stimulusDelay;
            private bool eepromReadWriteEnabled = false;
            private List<string> patternBurstsDefined = new List<string>();
            private string I2CVCCChanName = "VCC";
            private string I2CSDAChanName = "SDA";
            private string I2CSCKChanName = "SCK";
            private string I2CTEMPSENSORVCCChanName = "TSVCC";            
            private enum EepromOp
            {
                EraseWriteEnable, Read, Write
            }



            public override bool Initialize()
            {
                try
                {
                    Clock_Rate = 52e6;
                    double EEPROMClockRate = 2e5;
                    string compareDelayFile = @"C:/Avago.ATF.Common.x64/CableCalibration/M9195A_CompareDelay.txt";
                    usingMIPI = true;
                    try
                    {
                        if (!File.Exists(compareDelayFile)) File.WriteAllText(compareDelayFile, "35e-9");

                        compareDelay = Convert.ToDouble(File.ReadAllText(compareDelayFile));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("M9195A Compare Delay File issue!\n\nCannot run program, please contact engineer\n\n");
                    }

                    stimulusDelay = 1.5e-9;
                    double i2cStimulusDelay = 1e-6;

                    //PinNamesAndChans[TrigChanName] = "10";  // needs automation
                    //PinNamesAndChans[Vio_dummy] = "9";  // needs automation
                    PinNamesAndChans["Vrx"] = "6";  // needs automation
                    PinNamesAndChans["Vsh"] = "8";  // needs automation
                    
                    ////ChoonChin - Comment for Rev10 LB
                    PinNamesAndChans[I2CVCCChanName] = "0";
                    PinNamesAndChans[I2CSDAChanName] = "2";
                    PinNamesAndChans[I2CSCKChanName] = "1";
                    PinNamesAndChans[I2CTEMPSENSORVCCChanName] = "3";
                    driver = new KtMDsr();
                    driver.Initialize(VisaAlias, true, true, "Simulate=false");
                    //driver.Instrument.RemoveAllDynamicItems();
                    Eq.InstrumentInfo += GetInstrumentInfo();

                    #region Define MIPI pins

                    //string MipiSigNames = SdataChanName + ", " + SclkChanName + ", " + VioChanName + ", " + TrigChanName;    // put sdata first so that read PID shows 0's and 1's in readback array
                    string MipiSigNames = Sdata1ChanName + ", " + Sclk1ChanName;    // put sdata first so that read PID shows 0's and 1's in readback array
                    driver.Sites.Add(mipiSiteAlias);
                    //Console.WriteLine("Site:{0} driver hash code{1}",this.Site,driver.GetHashCode());
                    KtMDsrSignalDirectionEnum[] MipiSigDirections =
                    {   
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionInOut,      //SDATA 
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn,      //SCLK      
                        //KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn,       //VIO
                        //KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn,       //TRIG
                    };

                    driver.Signals.AddSignalList(MipiSigNames, MipiSigDirections);

                    //int[] mipiChanNums = new int[] { Convert.ToInt16(PinNamesAndChans[SdataChanName]), Convert.ToInt16(PinNamesAndChans[SclkChanName]), Convert.ToInt16(PinNamesAndChans[VioChanName]), Convert.ToInt16(PinNamesAndChans[TrigChanName]) };
                    int[] mipiChanNums = new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]) };
                    driver.Sites.Item[mipiSiteAlias].AssignSignalList(MipiSigNames, ref mipiChanNums);       //Maps signal names into channels
                    //Add software trigger source   modify by HKH 2015.09.22
                    driver.Sites.Item[mipiSiteAlias].Trigger0Source = "SoftwareTrigger0";
                    driver.Sites2.Item2[mipiSiteAlias].Marker0Destination = "PXI_TRIG0";

                    driver.Patterns.Add(WaitForTriggerPattern);
                    //driver.Patterns.Item[WaitForTriggerPattern].Vector(1, MipiSigNames, "0010", "wft");
                    driver.Patterns.Item[WaitForTriggerPattern].Vector(1, MipiSigNames, "00", "wft");
                    //driver.Patterns.Item[WaitForTriggerPattern].Vector(2, MipiSigNames, "0010", "wft");
                    driver.Patterns.Item[WaitForTriggerPattern].Vector(2, MipiSigNames, "00", "wft");
                    driver.Patterns.Item[WaitForTriggerPattern].WatchLoop(0, 2, 0, "Trigger0");
                    string WaitPattern = "Wait";
                    driver.Patterns.Add(WaitPattern);

                    for (int i = 0; i < 500; i++)
                        //driver.Patterns.Item[WaitPattern].Vector(i, MipiSigNames, "0010", "wftdelay");
                        driver.Patterns.Item[WaitPattern].Vector(i, MipiSigNames, "00", "wftdelay");

                    #endregion

                    #region Define PPMU pins

                    driver.Signals.AddSignalList("ppmuSingleChanSigList", new KtMDsrSignalDirectionEnum[] { KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionInOut });    //Add signal list to the site

                    for (int i = 0; i < 16; i++)
                    {
                        driver.Sites.Add(EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + i.ToString());
                        driver.Sites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + i.ToString()].AssignSignalList("ppmuSingleChanSigList", new int[] { i });
                    }

                    #endregion

                    #region Define EEPROM pins

                    string EepromSigNames = I2CSDAChanName + "," + I2CVCCChanName + "," + I2CSCKChanName;    // put sdata first so that read PID shows 0's and 1's in readback array

                    driver.Sites.Add(eepromSiteAlias);

                    KtMDsrSignalDirectionEnum[] EepromSigDirections =
                    {   
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionInOut,    //DO
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn,      //VCC
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn      //SK      
                    };

                    driver.Signals.AddSignalList(EepromSigNames, EepromSigDirections);
                    
                    int[] eepromChanNums = new int[] { Convert.ToInt16(PinNamesAndChans[I2CSDAChanName]), Convert.ToInt16(PinNamesAndChans[I2CVCCChanName]), Convert.ToInt16(PinNamesAndChans[I2CSCKChanName]) };
                    driver.Sites.Item[eepromSiteAlias].AssignSignalList(EepromSigNames, ref eepromChanNums);  //Maps signal names into channels

                    #endregion

                    #region Define TempSense pins
                    string TempSenseSigNames = I2CSDAChanName + "," + I2CTEMPSENSORVCCChanName + "," + I2CSCKChanName;

                    driver.Sites.Add(tempSenseSiteAlias);

                    KtMDsrSignalDirectionEnum[] TempSenseSigDirections =
                    {                        
                        KtMDsrSignalDirectionEnum.KtMDsrSignalDirectionIn     //VCC                        
                    };

                    driver.Signals.AddSignalList(I2CTEMPSENSORVCCChanName, TempSenseSigDirections);

                    int[] tempSenseChanNums = new int[] { Convert.ToInt16(PinNamesAndChans[I2CSDAChanName]), Convert.ToInt16(PinNamesAndChans[I2CTEMPSENSORVCCChanName]), Convert.ToInt16(PinNamesAndChans[I2CSCKChanName]) };
                    driver.Sites.Item[tempSenseSiteAlias].AssignSignalList(TempSenseSigNames, ref tempSenseChanNums);  //Maps signal names into channels

                    #endregion

                    driver.Channels.RemoteSenseEnable(mipiChanNums, false);

                    #region Set DC Levels

                    driver.DcLevels.Add(mipiDcLevelsAlias);       //define levels for each signals
                    driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(MipiSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, 1.8);
                    driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(MipiSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIL, 0);
                    driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(MipiSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOH, 0.6);
                    driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(MipiSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOL, 1.05);

                    driver.DcLevels.Add(eepromDcLevelsAlias);       //define levels for each signals
                    driver.DcLevels.get_Item(eepromDcLevelsAlias).SetDcLevel(EepromSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, 5);
                    driver.DcLevels.get_Item(eepromDcLevelsAlias).SetDcLevel(EepromSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIL, 0);
                    driver.DcLevels.get_Item(eepromDcLevelsAlias).SetDcLevel(EepromSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOH, 1);
                    driver.DcLevels.get_Item(eepromDcLevelsAlias).SetDcLevel(EepromSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOL, 2.4);

                    //driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SdataChanName, KtMDsrDcLevelEnum.KtMDsrDcLevelVIT, 1.8);   // for 50ohm Voltage Termination
                    driver.DcLevels.Add(tempSenseDcLevelsAlias);    //define levels for each signals
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(TempSenseSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, 3);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(TempSenseSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIL, 0);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(TempSenseSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOH, 2);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(TempSenseSigNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOL, 0.8);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(I2CTEMPSENSORVCCChanName, KtMDsrDcLevelEnum.KtMDsrDcLevelIOH, 1e-3);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(I2CTEMPSENSORVCCChanName, KtMDsrDcLevelEnum.KtMDsrDcLevelIOL, 1e-3);
                    driver.DcLevels.get_Item(tempSenseDcLevelsAlias).SetDcLevel(I2CTEMPSENSORVCCChanName, KtMDsrDcLevelEnum.KtMDsrDcLevelVCOM, 3);

                    #endregion

                    #region Waveform Tables (Timing)

                    #region Waveform table for general MIPI

                    driver.WaveformTables.Add(wftAlias);                                                               //Instatiate a Waveform table
                    KtMDsrWaveformTable WaveformTable = driver.WaveformTables.get_Item(wftAlias);                      //Create WFT object to work from
                    WaveformTable.Period = 1.0 / Clock_Rate;
                    Double[] timesWithCompare = { 0.0, 2E-9 };//Assign clock period    
                    WaveformTable.DefineWaveformCharacter("0", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTable.DefineWaveformCharacter("1", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTable.DefineWaveformCharacter("L", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTable.DefineWaveformCharacter("H", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    WaveformTable.DefineWaveformCharacter("X", Sdata1ChanName, timesWithCompare, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareUnknown });
                    //WaveformTable.DefineWaveformCharacter("Z", TrigChanName, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff });

                    driver.WaveformTables.Add("wftdelay");                                                               //Instatiate a Waveform table
                    KtMDsrWaveformTable WaveformTabledelay = driver.WaveformTables.get_Item("wftdelay");                      //Create WFT object to work from
                    WaveformTabledelay.Period = 1.0 / 50E6;                                                           //Assign clock period   
                    Double[] timesdelay = { 0.0 };
                    Double[] timesWithComparedelay = { 0.0, 2E-9 };
                    WaveformTabledelay.DefineWaveformCharacter("0", MipiSigNames, timesdelay, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTabledelay.DefineWaveformCharacter("1", MipiSigNames, timesdelay, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTabledelay.DefineWaveformCharacter("L", Sdata1ChanName, timesWithComparedelay, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTabledelay.DefineWaveformCharacter("H", Sdata1ChanName, timesWithComparedelay, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    //WaveformTabledelay.DefineWaveformCharacter("Z", TrigChanName, timesdelay, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff });
                    #endregion

                    #region Waveform table for Read PID and other readbacks which require interpreting readback array

                    driver.WaveformTables.Add(wftAllLowsAlias);                                                               //Instatiate a Waveform table
                    KtMDsrWaveformTable WaveformTableAllLows = driver.WaveformTables.get_Item(wftAllLowsAlias);                      //Create WFT object to work from                   
                    WaveformTableAllLows.Period = 1.0 / Clock_Rate;                                                           //Assign clock period    
                    WaveformTableAllLows.DefineWaveformCharacter("0", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableAllLows.DefineWaveformCharacter("1", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTableAllLows.DefineWaveformCharacter("L", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTableAllLows.DefineWaveformCharacter("H", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    //WaveformTableAllLows.DefineWaveformCharacter("Z", TrigChanName, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff });

                    #endregion

                    #region Waveform table for Reg Send

                    driver.WaveformTables.Add(wftRegIO);                                                               //Instatiate a Waveform table
                    KtMDsrWaveformTable WaveformTableRegSend = driver.WaveformTables.get_Item(wftRegIO);                      //Create WFT object to work from                   
                    WaveformTableRegSend.Period = 1.0 / (Clock_Rate / 2.0);                                                           //Assign clock period    

                    WaveformTableRegSend.DefineWaveformCharacter("0", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableRegSend.DefineWaveformCharacter("1", MipiSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTableRegSend.DefineWaveformCharacter("T", Sclk1ChanName, new double[] { 0, WaveformTableRegSend.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableRegSend.DefineWaveformCharacter("L", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTableRegSend.DefineWaveformCharacter("H", Sdata1ChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    WaveformTableRegSend.DefineWaveformCharacter("B", Sdata1ChanName, new double[] { 0, WaveformTableRegSend.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    //WaveformTableRegSend.DefineWaveformCharacter("Z", TrigChanName, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff });

                    #endregion

                    #region Waveform table for Eeprom

                    driver.WaveformTables.Add(wftEeprom);                                                               //Instantiate a Waveform table
                    KtMDsrWaveformTable WaveformTableEeprom = driver.WaveformTables.get_Item(wftEeprom);                //Create WFT object to work from                   
                    WaveformTableEeprom.Period = 1.0 / EEPROMClockRate;                                                           //Assign clock period    

                    WaveformTableEeprom.DefineWaveformCharacter("0", EepromSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableEeprom.DefineWaveformCharacter("1", EepromSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTableEeprom.DefineWaveformCharacter("T", I2CSCKChanName + "," + I2CSDAChanName, new double[] { 0, WaveformTableEeprom.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableEeprom.DefineWaveformCharacter("L", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTableEeprom.DefineWaveformCharacter("H", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    WaveformTableEeprom.DefineWaveformCharacter("S", I2CSDAChanName, new double[] { 0, WaveformTableEeprom.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    #endregion

                    #region Waveform table for Temp Sensor

                    driver.WaveformTables.Add(wftTempSense);
                    KtMDsrWaveformTable WaveformTableTempSense = driver.WaveformTables.get_Item(wftTempSense);
                    WaveformTableTempSense.Period = 1.0 / EEPROMClockRate;

                    WaveformTableTempSense.DefineWaveformCharacter("0", TempSenseSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableTempSense.DefineWaveformCharacter("1", TempSenseSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTableTempSense.DefineWaveformCharacter("T", I2CSCKChanName + "," + I2CSDAChanName, new double[] { 0, WaveformTableTempSense.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableTempSense.DefineWaveformCharacter("L", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTableTempSense.DefineWaveformCharacter("H", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    WaveformTableTempSense.DefineWaveformCharacter("S", I2CSDAChanName, new double[] { 0, WaveformTableTempSense.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });

                    driver.WaveformTables.Add(wftTempSenseSwitchOn);
                    KtMDsrWaveformTable WaveformTableTempSenseOn = driver.WaveformTables.get_Item(wftTempSenseSwitchOn);
                    WaveformTableTempSenseOn.Period = 1.0 / EEPROMClockRate;

                    WaveformTableTempSenseOn.DefineWaveformCharacter("0", TempSenseSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableTempSenseOn.DefineWaveformCharacter("1", TempSenseSigNames, new double[] { 0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });
                    WaveformTableTempSenseOn.DefineWaveformCharacter("T", I2CSCKChanName + "," + I2CSDAChanName, new double[] { 0, WaveformTableTempSense.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown });
                    WaveformTableTempSenseOn.DefineWaveformCharacter("L", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareLow });
                    WaveformTableTempSenseOn.DefineWaveformCharacter("H", I2CSDAChanName, new double[] { 0, 2e-9 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceOff, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventCompareHigh });
                    WaveformTableTempSenseOn.DefineWaveformCharacter("S", I2CSDAChanName, new double[] { 0, WaveformTableTempSense.Period / 2.0 }, new[] { KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceDown, KtMDsrWaveformTableEventEnum.KtMDsrWaveformTableEventForceUp });

                    #endregion

                    #endregion
                    double i2cCompareDelay = 2.34e-6;
                    driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]) }, compareDelay);
                    driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[I2CSDAChanName]), Convert.ToInt16(PinNamesAndChans[I2CVCCChanName]), Convert.ToInt16(PinNamesAndChans[I2CSCKChanName]), Convert.ToInt16(PinNamesAndChans[I2CTEMPSENSORVCCChanName]) }, i2cCompareDelay);
                    driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]) }, stimulusDelay);

                    i2cStimulusDelay = WaveformTableEeprom.Period / 4.0;
                    driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[I2CSCKChanName]) }, i2cStimulusDelay);   // for eeprom

                    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

                    return true;
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString(), "HSDIO MIPI");
                    return false;
                }

            }

            public override bool ReInitializeVIO(double violevel)
            {
                return true;
            }

            public override string GetInstrumentInfo()
            {
                return string.Format("HSDIO{0} = {1} r{2}; ", Site, driver.Identity.InstrumentModel, driver.Identity.InstrumentFirmwareRevision);
            }

            public override bool LoadVector(List<string> fullPaths, string nameInMemory)
            {
                try
                {
                    string tempDir = "VectorTemp";
                    Directory.CreateDirectory(tempDir);

                    //EqHSDIO.datalogResults[nameInMemory] = datalogResults;

                    nameInMemory = nameInMemory.Replace("_", "");

                    List<string> patterns = new List<string>();

                    //string waveformTable = nameInMemory.ToUpper().Contains("PID") || nameInMemory.ToUpper().Contains("TEMPSENSE") ? wftAllLowsAlias : wftAlias;   // load a special waveform table for PID, all compares are L
                    string waveformTable = nameInMemory.ToUpper().Contains("TEMPSENSE") ? wftAllLowsAlias : wftAlias;
                    foreach (string fullPath in fullPaths)
                    {
                        #region modify the Avago vec file to compatible with Keysight's LoadBulkData method

                        string[] allLines = File.ReadAllLines(fullPath);
                        //allLines[0] = "[" + SclkChanName + "," + SdataChanName + "," + VioChanName + "," + TrigChanName + "]";
                        allLines[0] = "[" + Sclk1ChanName + "," + Sdata1ChanName + "]";
                        for (int i = 1; i < allLines.Length; i++)
                        {
                            //allLines[i] = allLines[i] + ",Z";
                            allLines[i] = allLines[i].Substring(0, 3);
                        }

                        string tempModifiedPatternFile = tempDir + "\\" + Path.GetFileName(fullPath);
                        //string patternName = Path.GetFileNameWithoutExtension(fullPath);

                        string patternName = nameInMemory;

                        File.WriteAllLines(tempModifiedPatternFile, allLines);

                        #endregion

                        driver.PatternFile.LoadBulkData(tempModifiedPatternFile, patternName, "", waveformTable);

                        patterns.Add(patternName);
                    }

                    string patternBurstName = "burst_" + nameInMemory;

                    driver.PatternBursts.Add(patternBurstName);
                    driver.PatternBursts.get_Item(patternBurstName).Patterns = string.Join(",", patterns);

                    driver.PatternExecs.Add(nameInMemory);
                    driver.PatternExecs.get_Item(nameInMemory).DcLevels = mipiDcLevelsAlias;
                    driver.PatternExecs.get_Item(nameInMemory).PatternBurst = patternBurstName;
                    //if (nameInMemory == "TRIGON" || nameInMemory.Contains("TRIGSWREG"))
                    //    driver.Patterns.Item[nameInMemory].CrossReference(176, "Marker0");
                    driver.PatternSites.CompilePatternExec(mipiSiteAlias, nameInMemory, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, nameInMemory);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load vector file:\n" + fullPaths + "\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_MipiHiZ()
            {
                try
                {
                    int numLines = 8;

                    //string NibbleOrder = SclkChanName + "+" + SdataChanName + "+" + VioChanName + "+" + TrigChanName;
                    string NibbleOrder = Sclk1ChanName + "+" + Sdata1ChanName ;
                    driver.Patterns.Add(HiZ);
                    KtMDsrPattern Pattern = driver.Patterns.get_Item(HiZ);

                    for (int i = 0; i < numLines; i++)
                    {
                        Pattern.Vector(i, NibbleOrder, "00", wftAlias);
                    }

                    driver.PatternSites.CompilePattern(mipiSiteAlias, HiZ, mipiDcLevelsAlias, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, HiZ);

                    //EqHSDIO.datalogResults[HiZ] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi HiZ vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }


            public override bool LoadVector_MipiVioOff()
            {
                return true;
            }


            public override bool LoadVector_MipiVioOn()
            {
                return true;
            }

            public override bool LoadVector_RFOnOffTest(bool isNRZ = false) { return true; }

            public override bool LoadVector_MipiReset()
            {
                try
                {
                    double clockRate = 1.0 / driver.WaveformTables.get_Item(wftAlias).Period;

                    double secondsReset = 0.002;
                    int numLines = (int)(clockRate * secondsReset);

                    //string NibbleOrder = SclkChanName + "+" + SdataChanName + "+" + VioChanName + "+" + TrigChanName;
                    string NibbleOrder = Sclk1ChanName + "+" + Sdata1ChanName ;
                    driver.Patterns.Add(Reset);
                    KtMDsrPattern Pattern = driver.Patterns.get_Item(Reset);

                    for (int i = 0; i < numLines; i++)
                    {
                        if (i < numLines / 2)
                            Pattern.Vector(i, NibbleOrder, "00", wftAlias);
                        else
                            Pattern.Vector(i, NibbleOrder, "00", wftAlias);

                    }

                    driver.PatternSites.CompilePattern(mipiSiteAlias, Reset, mipiDcLevelsAlias, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, Reset);

                    //EqHSDIO.datalogResults[Reset] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi Reset vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_MipiRegIO()
            {
                try
                {
                    //string NibbleOrder = SclkChanName + "+" + SdataChanName + "+" + VioChanName + "+" + TrigChanName;
                    string NibbleOrder = Sclk1ChanName + "+" + Sdata1ChanName ;
                    driver.Patterns.Add(RegIO);
                    KtMDsrPattern Pattern = driver.Patterns.get_Item(RegIO);                                          //Create object for pattern access

                    int i = 0;

                    // Ensure SDATA starts out low
                    Pattern.Vector(i++, NibbleOrder, "00", wftRegIO);
                    driver.Patterns.Item[RegIO].LabelElement(i - 1, "Start:");

                    // Start Sequence Command (2 bits)
                    Pattern.Vector(i++, NibbleOrder, "01", wftRegIO);
                    Pattern.Vector(i++, NibbleOrder, "00", wftRegIO);

                    // Slave Address of our device (4 bits)
                    for (int j = 0; j < 4; j++) Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Write command (3 bits, 010 = write, 011 = read)
                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);
                    Pattern.Vector(i++, NibbleOrder, "T1", wftRegIO);
                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Register Address (5 bits)
                    for (int j = 0; j < 5; j++) Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Register Parity
                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Data (8 bits)
                    for (int j = 0; j < 8; j++) Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Data Parity
                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    // Bus Park
                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    driver.Patterns.Item[RegIO].LabelElement(i - 1, "Trig:");

                    // Trigger
                    for (int j = 0; j < 50; j++) Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    Pattern.Vector(i++, NibbleOrder, "T0", wftRegIO);

                    driver.PatternSites.CompilePattern(mipiSiteAlias, RegIO, mipiDcLevelsAlias, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, RegIO);

                    //EqHSDIO.datalogResults[RegIO] = false;

                    RegWrite("0", "0", false);   // to put trigger channel into high impedance state
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegIO vector\n\n" + e.ToString(), "HSDIO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            //public override bool LoadVector_RFOnOffTest(bool isNRZ = false) obs
            //{
            //    return true;
            //}

            public override bool LoadVector_RFOnOffTestRx(bool isNRZ = false) //Rx Trigger
            {
                return true;
            }


            public override bool LoadVector_RFOnOffSwitchTest(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest_With3TxPreMipi(bool isNRZ = false)
            {
                return true;
            }


            public override bool LoadVector_RFOnOffSwitchTestRx(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_WithPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                return true;
            }

            public override bool LoadVector_RFOnOffSwitchTest2Rx(bool isNRZ = false)
            {
                return true;
            }

            public override bool SendVector(string nameInMemory)
            {
                if (!usingMIPI || nameInMemory == null || nameInMemory == "") return true;

                if (nameInMemory.ToUpper().Contains("PID")) return true;   // The ReadPID vector will be sent in the InterpretPID method
                if (nameInMemory.ToUpper().Contains("TempSense")) return true;   // The ReadTempSense vector will be sent in the InterpretTempSense method
                try
                {
                    nameInMemory = nameInMemory.Replace("_", "");

                    driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]) }, compareDelay);
                    //driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[SdataChanName]) }, stimulusDelay);

                    driver.PatternSites.ActivateFromCache(nameInMemory);

                    try
                    {
                        driver.PatternSites.Item[nameInMemory].Initiate();
                        driver.PatternSites.Item[nameInMemory].WaitForAcquisitionComplete(0.5);
                        //KtMDsrPatternSiteExecutionStateEnum yo = driver.PatternSites.Item[nameInMemory].WaitForAcquisitionComplete(0.5);
                        //if (yo != KtMDsrPatternSiteExecutionStateEnum.KtMDsrPatternSiteExecutionStateComplete)
                        //{
                        //    Console.WriteLine("HSDIO wait results: " + yo);
                        //}
                        lastFailureCount = driver.PatternSites.Item[nameInMemory].ResultCount;
                        driver.PatternSites.Inactivate(nameInMemory);
                        return true;
                    }
                    catch (Exception e)
                    {
                        driver.PatternSites.get_Item(nameInMemory).Abort();
                        driver.PatternSites.Inactivate(nameInMemory);
                        return false;
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            public override bool Burn(string data_hex, bool invertData = false, int efuseDataByteNum = 0, string efuseCtlAddress = "C0")
            {

                return true;
            }
            public override bool SendVectorOTP(string TargetData, string CurrentData = "00", bool isEfuseBurn = false)
            {
                return true;
            }
            public static int countNum = 0;
            public static int countNum2 = 0;
            public override void SendNextVectors(bool firstTest, List<string> MipiWaveformNames)
            {
                try
                {
                    //if (MipiWaveformNames.Count == 0) return;
                    //string nameInMemory = string.Join(" ", MipiWaveformNames);
                    //SendVector(nameInMemory);

                    string TriggeredTestplanCache = "MyPatternExCache";
                    {
                        if (!FirstVector)
                        {
                            //.PatternFile.StoreStil("C:\\temp\\MyStil_new.stil");

                            //driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

                            driver.PatternSites.ActivateFromCache(TriggeredTestplanCache);

                            //while (driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != Stopped && driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != Complete) { }

                            driver.PatternSites.Item[TriggeredTestplanCache].Initiate();

                            //while (driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != TrigNotReady) { }
                            FirstVector = true;
                            countNum = 0;
                        }
                        while (driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != TrigNotReady)
                        {
                            var check11 = driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState;
                        }
                        countNum++;
                        driver.TriggerEvent.GenerateSoftwareEvents(1);
                        //var State = driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState;

                        if (LastVector)
                        {

                            //byte[] readData;

                            //driver.PatternSites.Item[TriggeredTestplanCache].WaitForAcquisitionComplete(1);
                            //readData = driver.PatternSites.Item[TriggeredTestplanCache].FetchResultByte();
                            //State = driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState;
                            while (driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != Complete)
                            {
                                driver.TriggerEvent.GenerateSoftwareEvents(1);
                            }

                            driver.PatternSites.Item[TriggeredTestplanCache].WaitForAcquisitionComplete(1);


                            //var Errors = driver.PatternSites.Item[TriggeredTestplanCache].ExecutionResult;
                            //var results = driver.PatternSites.Item[TriggeredTestplanCache].FetchResultByte();
                            //var ResultCount = driver.PatternSites.Item[TriggeredTestplanCache].ResultCount;
                            //var TrinaryResults = driver.PatternSites.Item[TriggeredTestplanCache].FetchTrinaryResult();

                            driver.PatternSites.Inactivate(TriggeredTestplanCache);
                            ////InterpretPID(TriggeredTestplanCache);
                            //InterpretPID("READUSID");
                            //SendVector("PDM");

                            //double clockRate = driver.WaveformTables.get_Item(wftAlias).Period;

                            return;
                        }
                        else
                        {

                            while (driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState != TrigNotReady)
                            {
                                var check = driver.PatternSites.get_Item(TriggeredTestplanCache).ExecutionState;
                            }
                        }

                    }
                    //TryAgain:

                    //     nameInMemory = string.Join(" ", MipiWaveformNames);

                    //    if (!SendVector(nameInMemory))//?????
                    //    {
                    //        while (!SendVector(EqHSDIO.Reset)) { }
                    //        goto TryAgain;
                    //    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            public override void SendMipiCommands(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, eMipiTestType _eMipiTestType = eMipiTestType.Write)
            {
                throw new NotImplementedException();
            }
            public override bool SendRFOnOffTestVector(bool flag, string[] SwTimeCustomArry)
            {
                throw new NotImplementedException();
            }
            public override void SetSourceWaveformArry(string customMIPIlist)
            {
                throw new NotImplementedException();
            }
            public override void AddVectorsToScript(List<string> namesInMemory, bool finalizeScript)
            {
                if (namesInMemory.Count <= 0) return;
                //string nameInMemory = string.Join(" ", namesInMemory);

                //string patternBurstName = "burst_" + nameInMemory;

                //if (patternBurstsDefined.Contains(patternBurstName)) return;

                //driver.PatternBursts.Add(patternBurstName);
                //patternBurstsDefined.Add(patternBurstName);
                //string patterns = string.Join(",", namesInMemory.Select(x => "burst_" + x));
                //driver.PatternBursts.get_Item(patternBurstName).Patterns = patterns;

                //driver.PatternExecs.Add(nameInMemory);
                //driver.PatternExecs.get_Item(nameInMemory).DcLevels = mipiDcLevelsAlias;
                //driver.PatternExecs.get_Item(nameInMemory).PatternBurst = patternBurstName;

                //driver.PatternSites.CompilePatternExec(mipiSiteAlias, nameInMemory, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, nameInMemory);
                countNum2++;
                string stemp = "";
                string nameInMemory = string.Join(" ", namesInMemory);
                string patternBurstName = "";
                if (namesInMemory.Count != 0)
                {

                    stemp = string.Join(",", namesInMemory.Select(x => x));
                    scriptFull += WaitForTriggerPattern + "," + stemp + ",";
                }
                if (finalizeScript == true)
                {
                    scriptFull = scriptFull.Substring(0, scriptFull.Length - 1);

                    string PatternExecName = "All";
                    patternBurstName = "burst_" + PatternExecName;

                    driver.PatternBursts.Add(patternBurstName);
                    driver.PatternBursts.get_Item(patternBurstName).Patterns = scriptFull;

                    driver.PatternExecs.Add(PatternExecName);
                    driver.PatternExecs.get_Item(PatternExecName).DcLevels = mipiDcLevelsAlias;
                    driver.PatternExecs.get_Item(PatternExecName).PatternBurst = patternBurstName;

                    driver.PatternSites.CompilePatternExec(mipiSiteAlias, PatternExecName, false, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, TriggeredTestplanCache);
                    //driver.PatternFile.StoreStil("C:\\temp\\MyStil.stil");
                }
                
            }

            public override int GetNumExecErrors(string nameInMemory)
            {
                return lastFailureCount;
            }

            //public override int InterpretPID(string nameInMemory)
            //{
            //    nameInMemory = nameInMemory.Replace("_", "");

            //    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithCompare;
            //    driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]), Convert.ToInt16(PinNamesAndChans[Vio1ChanName]) }, compareDelay);
            //    driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]) }, stimulusDelay);


            //    byte[] readData;

            //    while (true)   // workaround for Keysight HSDIO timeout issue
            //    {
            //        try
            //        {
            //            driver.PatternSites.ActivateFromCache(nameInMemory);
            //            driver.PatternSites.Item[nameInMemory].Initiate();
            //            driver.PatternSites.Item[nameInMemory].WaitForAcquisitionComplete(0.5);
            //            readData = driver.PatternSites.Item[nameInMemory].FetchResultByte();
            //            //short[] arrInt = driver.PatternSites.Item[nameInMemory].FetchResultInt16();
            //            //int fails = driver.PatternSites.Item[nameInMemory].ResultCount;
            //            driver.PatternSites.Inactivate(nameInMemory);
            //            break;
            //        }
            //        catch (Exception e)
            //        {
            //            driver.PatternSites.get_Item(nameInMemory).Abort();
            //            driver.PatternSites.Inactivate(nameInMemory);
            //            while (!SendVector(EqHSDIO.Reset)) { }
            //        }
            //    }

            //    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

            //    int numBits = 8;
            //    string pidBinary = "";

            //    for (int i = 0; i < numBits; i++)
            //    {
            //        int dataBitBoth = readData[2 * i] & readData[2 * i + 1];

            //        pidBinary += dataBitBoth;

            //        if (false)  // debug
            //        {
            //            Console.WriteLine("vectorLine: " + (2 * i) + "\treadData: " + Convert.ToString(readData[2 * i], 2) + "\tdataBit: " + readData[2 * i]);
            //            Console.WriteLine("vectorLine: " + (2 * i + 1) + "\treadData: " + Convert.ToString(readData[2 * i + 1], 2) + "\tdataBit: " + readData[2 * i + 1] + "\tcombinedBit: " + dataBitBoth + "\n");
            //        }
            //    }

            //    return Convert.ToInt32(pidBinary, 2);
            //}

            //public static double TempSenseRaw = 0;   // temporary, need to integrate this more cleanly

            //public override double InterpretTempSense(string nameInMemory)
            //{
            //    nameInMemory = nameInMemory.Replace("_", "");

            //    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithCompare;
            //    driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]), Convert.ToInt16(PinNamesAndChans[Vio1ChanName]) }, compareDelay);
            //    driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]) }, stimulusDelay);

            //    //HSDIO.SendVector(HSDIO.Reset);
            //    //HSDIO.RegWrite("F", "10", "0", false);  // ask Manolito why this is necessary
            //    //string hex = HSDIO.RegRead("F", "10");  // PM Trigger de-activate 
            //    //double newTempSenseRaw = Convert.ToInt32(hex, 16);
            //    //double newTempSenseResult = (double)(-20 + (0.58823529 * newTempSenseRaw));
            //    //driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithCompare;

            //    byte[] readData;

            //    while (true)  // workaround for Keysight HSDIO timeout issue
            //    {
            //        try
            //        {
            //            driver.PatternSites.ActivateFromCache(nameInMemory);
            //            driver.PatternSites.Item[nameInMemory].Initiate();
            //            driver.PatternSites.Item[nameInMemory].WaitForAcquisitionComplete(0.5);
            //            readData = driver.PatternSites.Item[nameInMemory].FetchResultByte();
            //            driver.PatternSites.Inactivate(nameInMemory);
            //            break;
            //        }
            //        catch (Exception e)
            //        {
            //            driver.PatternSites.get_Item(nameInMemory).Abort();
            //            driver.PatternSites.Inactivate(nameInMemory);
            //            while (!SendVector(EqHSDIO.Reset)) { }
            //        }
            //    }

            //    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

            //    int numBits = 8;
            //    string tempSenseBinary = "";

            //    for (int i = 0; i < numBits; i++)
            //    {
            //        int dataBitBoth = readData[2 * i] & readData[2 * i + 1];

            //        tempSenseBinary += dataBitBoth;

            //        if (false)  // debug
            //        {
            //            Console.WriteLine("vectorLine: " + (2 * i) + "\treadData: " + Convert.ToString(readData[2 * i], 2) + "\tdataBit: " + readData[2 * i]);
            //            Console.WriteLine("vectorLine: " + (2 * i + 1) + "\treadData: " + Convert.ToString(readData[2 * i + 1], 2) + "\tdataBit: " + readData[2 * i + 1] + "\tcombinedBit: " + dataBitBoth + "\n");
            //        }
            //    }

            //    double TempSenseRaw = Convert.ToInt32(tempSenseBinary, 2);

            //    //TempSenseRaw = 255 - TempSenseRaw;    //old formula
            //    //TempSenseRaw = 137 - (0.65 * (TempSenseRaw - 1));

            //    double TempSenseResult = (double)(-20 + (0.58823529 * TempSenseRaw));

            //    return TempSenseResult;
            //}

            public override void shmoo(string nameInMemory)
            {
                string SignalNames = driver.DcLevels.get_Item(mipiDcLevelsAlias).Signals;
                KtMDsrDcLevel dcLevel = driver.DcLevels.get_Item(mipiDcLevelsAlias);
                KtMDsrWaveformTable waveformTable = driver.WaveformTables.get_Item(wftAlias);
                int count = 0;

                //for (double Vvio = 1.8; Vvio >= 1.8; Vvio -= 0.1)  //    2.0 1.8 0.025
                //for (double Vvio = 1.8; Vvio >= 1.8; Vvio -= 0.1)  //    2.0 1.8 0.025
                //for (double delayComp = 10e-9; delayComp <= 60e-9; delayComp += 2e-9)  //    2.0 1.8 0.025
                for (double delayComp = 0e-9; delayComp <= 80e-9; delayComp += 4e-9)  //    2.0 1.8 0.025
                {
                    //for (double mipiRate = 26e6; mipiRate <= 26e6; mipiRate += 1e6)
                    for (double mipiRate = 26e6; mipiRate <= 26e6; mipiRate += 2e6)
                    {
                        try
                        {
                            driver.WaveformTables.get_Item(wftAlias).Period = 1.0 / (mipiRate * 2.0);                                                           //Assign clock period    

                            //driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, Vvio);
                            driver.Channels.ResponseDelayCompensation(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]), Convert.ToInt16(PinNamesAndChans[Sclk1ChanName]), Convert.ToInt16(PinNamesAndChans[Vio1ChanName]) }, compareDelay);
                            driver.Channels.StimulusDelay(new int[] { Convert.ToInt16(PinNamesAndChans[Sdata1ChanName]) }, stimulusDelay);
                            //driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, Vvio);

                            //{
                            driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, 1.8);
                            driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIL, 0);
                            driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOH, 0.6);
                            driver.DcLevels.get_Item(mipiDcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOL, 0.5);
                            //{
                            //    driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIH, 1.8);
                            //    driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVIL, 0);
                            //    driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOH, 0.5);
                            //    driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SignalNames, KtMDsrDcLevelEnum.KtMDsrDcLevelVOL, 0.4);

                            //    driver.DcLevels.get_Item(dcLevelsAlias).SetDcLevel(SdataChanName, KtMDsrDcLevelEnum.KtMDsrDcLevelVIT, double.NaN);   // for 50ohm Voltage Termination
                            //}

                            string myName = mipiRate.ToString() + "." + count++; //  V
                            ToString();
                            driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

                            driver.PatternSites.CompilePatternExec(mipiSiteAlias, nameInMemory, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, nameInMemory + myName);

                            driver.PatternSites.ActivateFromCache(nameInMemory + myName);
                            driver.PatternSites.Item[nameInMemory + myName].Initiate();
                            driver.PatternSites.Item[nameInMemory + myName].WaitForAcquisitionComplete(0.5);
                            int fails = driver.PatternSites.Item[nameInMemory + myName].ResultCount;
                            driver.PatternSites.Inactivate(nameInMemory + myName);

                            Console.Write(delayComp + "\t" + fails + "\t");
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    Console.WriteLine();
                }

            }
            public override void RegWriteMultiple(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {

            }

            public override void RegWrite(string registerAddress_hex, string data_hex, bool sendTrigger = false)
            {
                int registerAddress_dec = Convert.ToInt32(registerAddress_hex, 16);

                string slaveAddress_bin = Convert.ToString(Convert.ToInt32("0", 16), 2);
                string registerAddress_bin = Convert.ToString(registerAddress_dec, 2);
                string data_bin = Convert.ToString(Convert.ToInt32(data_hex, 16), 2);

                bool extendedWrite = registerAddress_dec > 31;    // any register address > 5 bits requires extended write

                bool reg0 = registerAddress_bin == "0";
                int registerAddressNumBits = extendedWrite ? 8 : 5;
                int dataNumBits = reg0 ? 7 : 8;

                slaveAddress_bin = slaveAddress_bin.PadLeft(4, '0');
                registerAddress_bin = registerAddress_bin.PadLeft(registerAddressNumBits, '0');
                data_bin = data_bin.PadLeft(dataNumBits, '0');

                const string startCommandSequence = "010";
                const string extendedWriteByteCount = "0000";  // number of bytes to be written - 1. This method only supports writing one byte.
                string writeCommand = extendedWrite ? "0000" : "010";

                string RFFECmd =
                    reg0 ?
                        startCommandSequence + FrameWithParity(slaveAddress_bin + "1" + data_bin) :
                    extendedWrite ?
                        startCommandSequence + FrameWithParity(slaveAddress_bin + writeCommand + extendedWriteByteCount) + FrameWithParity(registerAddress_bin) + FrameWithParity(data_bin) :
                        startCommandSequence + FrameWithParity(slaveAddress_bin + writeCommand + registerAddress_bin) + FrameWithParity(data_bin);

                RFFECmd = RFFECmd.PadRight(50, '0');

                try
                {
                    driver.PatternSites.ActivateFromCache(RegIO);
                    driver.PatternSites.Item[RegIO].SetSignal(RegIO, "Start:", 0, Sdata1ChanName, RFFECmd);

                    if (sendTrigger)
                        driver.PatternSites.Item[RegIO].SetSignal(RegIO, "Trig:", 0, TrigChanName, "0".PadLeft(51, '1'));
                    else
                        driver.PatternSites.Item[RegIO].SetSignal(RegIO, "Trig:", 0, TrigChanName, "0".PadLeft(51, '0'));

                    driver.PatternSites.Item[RegIO].Initiate();
                    driver.PatternSites.Item[RegIO].WaitForAcquisitionComplete(0.5);
                    driver.PatternSites.Inactivate(RegIO);
                }
                catch (Exception e)
                {
                    driver.PatternSites.get_Item(RegIO).Abort();
                    driver.PatternSites.Inactivate(RegIO);
                }

            }

            public override string RegRead(string registerAddress_hex, bool writeHalf = false)
            {
                int registerAddress_dec = Convert.ToInt32(registerAddress_hex, 16);

                string slaveAddress_bin = Convert.ToString(Convert.ToInt32(Eq.Site[Site].HSDIO.dutSlaveAddress, 16), 2);
                string registerAddress_bin = Convert.ToString(registerAddress_dec, 2);

                bool extendedRead = registerAddress_dec > 31;    // any register address > 5 bits requires extended read

                int registerAddressNumBits = extendedRead ? 8 : 5;
                int dataNumBits = 8;

                slaveAddress_bin = slaveAddress_bin.PadLeft(4, '0');
                registerAddress_bin = registerAddress_bin.PadLeft(registerAddressNumBits, '0');

                const string startCommandSequence = "010";
                const string extendedReadByteCount = "0000";  // number of bytes to be read - 1. This method only supports reading one byte.
                string readCmd = extendedRead ? "0010" : "011";

                string RFFECmd = extendedRead ?
                    startCommandSequence + FrameWithParity(slaveAddress_bin + readCmd + extendedReadByteCount) + FrameWithParity(registerAddress_bin) + "0".PadRight(18, 'L') :
                    startCommandSequence + FrameWithParity(slaveAddress_bin + readCmd + registerAddress_bin) + "0".PadRight(10, 'L');

                RFFECmd = RFFECmd.PadRight(50, '0');

                try
                {
                    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithCompare;

                    driver.PatternSites.ActivateFromCache(RegIO);
                    driver.PatternSites.Item[RegIO].SetSignal(RegIO, "Start:", 0, Sdata1ChanName, RFFECmd);

                    driver.PatternSites.Item[RegIO].SetSignal(RegIO, "Trig:", 0, TrigChanName, "0".PadLeft(51, '0'));

                    driver.PatternSites.Item[RegIO].Initiate();
                    driver.PatternSites.Item[RegIO].WaitForAcquisitionComplete(0.5);

                    byte[] readData = driver.PatternSites.Item[RegIO].FetchResultByte();

                    driver.PatternSites.Inactivate(RegIO);
                    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

                    string readback_hex = Convert.ToInt32(string.Join("", readData.Take(dataNumBits)), 2).ToString("X");
                    return readback_hex;
                }
                catch (Exception e)
                {
                    driver.PatternSites.get_Item(RegIO).Abort();
                    driver.PatternSites.Inactivate(RegIO);
                    return "error";
                }

            }

            private string FrameWithParity(string binaryString)
            {
                return binaryString + ((1 + (binaryString).Where(c => c == '1').Count()) % 2).ToString();
            }

            public override void Close()
            {
                driver.Close();
            }

            public override bool LoadVector_EEPROM()
            {
                try
                {

                    string NibbleOrder = I2CSDAChanName + "+" + I2CVCCChanName + "+" + I2CSCKChanName;
                    driver.Patterns.Add(eepromReadAlias);
                    KtMDsrPattern Pattern = driver.Patterns.get_Item(eepromReadAlias);   //Create object for pattern access

                    int i = 0;

                    for (int j = 0; j < 300; j++) Pattern.Vector(i++, NibbleOrder, "111", wftEeprom); //Idle

                    Pattern.Vector(i++, NibbleOrder, "T1T", wftEeprom); //Start Condition

                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101000)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101000)

                    Pattern.Vector(i++, NibbleOrder, "L1T", wftEeprom); //ACK

                    int RegAddressLabelIndex = i;

                    for (int j = 0; j < 8; j++) Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom);   // Write Register Address

                    driver.Patterns.Item[eepromReadAlias].LabelElement(RegAddressLabelIndex, "RegAddress:");

                    Pattern.Vector(i++, NibbleOrder, "L1T", wftEeprom); //ACK

                    Pattern.Vector(i++, NibbleOrder, "T1T", wftEeprom); //Start Condition

                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "01T", wftEeprom); //Write Device (10101001)
                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); //Write Device (10101001)

                    Pattern.Vector(i++, NibbleOrder, "L1T", wftEeprom); //ACK

                    for (int j = 0; j < 8; j++) Pattern.Vector(i++, NibbleOrder, "L1T", wftEeprom);

                    Pattern.Vector(i++, NibbleOrder, "11T", wftEeprom); // No ACK

                    Pattern.Vector(i++, NibbleOrder, "S11", wftEeprom); // Stop Condition

                    for (int j = 0; j < 300; j++) Pattern.Vector(i++, NibbleOrder, "111", wftEeprom); //Idle                        

                    driver.PatternSites.CompilePattern(eepromSiteAlias, eepromReadAlias, eepromDcLevelsAlias, true, 1e-9, KtMDsrPatternSiteSaveDestinationEnum.KtMDsrPatternSiteSaveDestinationCache, eepromReadAlias);

                    driver.WaveformTables.Remove(wftEeprom);
                    //EqHSDIO.datalogResults[Eeprom] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Eeprom vector\n\n" + e.ToString(), "HSDIO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private byte EepromIO(EepromOp Operation, byte registerAddress_dec = 0, byte dataWrite_dec = 0)
            {
                if (!eepromReadWriteEnabled)
                {
                    eepromReadWriteEnabled = true;
                    EepromIO(EepromOp.EraseWriteEnable);
                }

                string registerAddress_bin = Convert.ToString(registerAddress_dec, 2).PadLeft(9, '0');
                string dataWrite_bin = Convert.ToString(dataWrite_dec, 2).PadLeft(8, '0');

                string opcode = "";
                string dataRead_bin = "";

                switch (Operation)
                {
                    case EepromOp.EraseWriteEnable:
                        opcode = "00";
                        registerAddress_bin = "110000000";
                        dataWrite_bin = "".PadLeft(8, 'Z');
                        break;
                    case EepromOp.Write:
                        opcode = "01";
                        break;
                    case EepromOp.Read:
                        opcode = "10";
                        dataWrite_bin = "";
                        dataRead_bin = "".PadRight(8, 'L');
                        break;

                }

                string DIcmd = "1" + opcode + registerAddress_bin + dataWrite_bin;
                string DOcmd = "".PadRight(13, 'Z') + dataRead_bin;

                DIcmd = DIcmd.PadRight(21, 'Z');
                DOcmd = DOcmd.PadRight(21, 'Z');

                try
                {
                    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithCompare;

                    driver.PatternSites.ActivateFromCache(Eeprom);
                    driver.PatternSites.Item[Eeprom].SetSignal(Eeprom, "Start:", 0, DiChanName, DIcmd);
                    driver.PatternSites.Item[Eeprom].SetSignal(Eeprom, "Start:", 0, DoChanName, DOcmd);

                    driver.PatternSites.Item[Eeprom].Initiate();
                    driver.PatternSites.Item[Eeprom].WaitForAcquisitionComplete(0.5);

                    byte[] readData = driver.PatternSites.Item[Eeprom].FetchResultByte();

                    driver.PatternSites.Inactivate(Eeprom);
                    driver.PatternSites.WhatToLog = KtMDsrPatternSitesWhatToLogEnum.KtMDsrPatternSitesWhatToLogEveryCycleWithFailingCompare;

                    if (Operation == EepromOp.Read)
                    {
                        return Convert.ToByte(string.Join("", readData.Take(8)), 2);
                    }

                }
                catch (Exception e)
                {
                    driver.PatternSites.get_Item(Eeprom).Abort();
                    driver.PatternSites.Inactivate(Eeprom);
                }

                return 0;
            }

            public override string EepromRead()
            {
                List<byte> readDataList = new List<byte>();

                for (ushort reg = 0; reg < 256; reg++)
                {
                    byte readData = EepromIO(EepromOp.Read, (byte)reg);

                    if (readData == 0) break;

                    readDataList.Add(readData);
                }

                return string.Join("", System.Text.Encoding.UTF8.GetChars(readDataList.ToArray()));
            }

            public override void EepromWrite(string dataWrite)
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(dataWrite + '\0');

                if (byteArray.Length > 256)
                {
                    MessageBox.Show("Exceeded maximum data length of 255 characters,\nEEPROM will not be written.", "EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int tryWrite = 0; tryWrite < 5; tryWrite++)
                {
                    for (ushort reg = 0; reg < byteArray.Length; reg++)
                    {
                        EepromIO(EepromOp.Write, (byte)reg, byteArray[reg]);
                    }

                    if (EepromRead() == dataWrite)
                    {
                        MessageBox.Show("Writing & readback successful:\n\n    " + dataWrite, "EEPROM");
                        return;
                    }
                }

                MessageBox.Show("Writing NOT successful!", "EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            public override bool LoadVector_UNIO_EEPROM()
            {
                return false;
            }

            public override bool UNIO_EEPROMWriteID(UNIO_EEPROMType device, string dataWrite, int bus_no = 1)
            {
                return false;
            }

            public override bool UNIO_EEPROMWriteCounter(UNIO_EEPROMType device, uint count, int bus_no = 1)
            {
                return false;
            }

            public override bool UNIO_EEPROMFreeze(UNIO_EEPROMType device, int bus_no = 1)
            {
                return false;
            }

            public override string UNIO_EEPROMReadID(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override uint UNIO_EEPROMReadCounter(UNIO_EEPROMType device, int bus_no = 1)
            {
                return 0;
            }

            public override string UNIO_EEPROMReadSerialNumber(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override string UNIO_EEPROMReadMID(UNIO_EEPROMType device, int bus_no = 1)
            {
                return "";
            }

            public override void SendTRIGVectors()
            {
                //throw new NotImplementedException();
            }

            public override bool LoadVector_TEMPSENSEI2C(decimal TempSensorAddress)
            {
                return false;
            }

            public override double I2CTEMPSENSERead()
            {
                return 0;
            }

            public override TriggerLine TriggerOut { get; set; }
        }
    }

}