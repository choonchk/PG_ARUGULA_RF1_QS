using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using EqLib;
using ClothoLibAlgo;


namespace TestLib
{
    public static class CableCal
    {
        public static bool runCableCal = false;
        private static List<Dictionary<string, float>> CalFactorQuickRetrieve = new List<Dictionary<string, float>>();
        private static List<double> calMasterFreqsMHz = new List<double>();
        public static string calFileDir;
        public static List<CalFile> allCalFiles = new List<CalFile>();


        public static float GetCF(byte site, string Band, Operation operation, double freq)
        {
            // For Quadsite, single Controller dual sites 	
            string BandTemp = Band;
            byte SiteTemp = site;
            if (site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + site.ToString();
                SiteTemp = 0;
            }
            EqSwitchMatrix.PortCombo portCombo = Eq.Site[SiteTemp].SwMatrix.GetPath(BandTemp, operation);

            return GetCF(site, portCombo, freq);
        }

        public static float GetCF(byte site, EqSwitchMatrix.PortCombo portCombo, double freq)
        {
            string calFileName = "";

            try
            {
                calFileName = CalFile.GetName(site, portCombo);

                float calFactor = 0;

                if (CalFactorQuickRetrieve[site].TryGetValue(calFileName + "_" + freq, out calFactor))
                {
                    return calFactor;
                }
                else
                {
                    var calFile =
                        (from calfile in allCalFiles
                         where calfile.site == site
                         && calfile.portCombo == portCombo
                         select calfile).First();

                    calFactor = calFile.GetCalfactor(freq);

                    CalFactorQuickRetrieve[site].Add(calFileName + "_" + freq, calFactor);
                }
                return calFactor;
            }
            catch (Exception e)
            {
                MessageBox.Show("No Calfactor loaded for " + calFileName + ", Freq " + freq + "\n\n" + e.ToString(), "GetCalfactor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        static string dateCode = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", DateTime.Now);

        public static int CalibrateSourcePath(byte site, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, Operation operation = Operation.VSGtoTX)
        {
            return CalibrateSourcePath(site, "", onboardAttenDb, powerLevel, maxFreqMHz, CalLimitLow, CalLimitHigh, operation);
        }

        public static int CalibrateSourcePath(byte site, string band, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, Operation operation = Operation.VSGtoTX)
        {
           
            try
            {
                // For Quadsite, single Controller dual sites
                string BandTemp = band;
                byte SiteTemp = site;
                EqSwitchMatrix.PortCombo sourcePorts1;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                    sourcePorts1 = Eq.Site[SiteTemp].SwMatrix.ActivatePath("N77_1", Operation.VSGtoTX1); //mario
                }
                else
                {
                    sourcePorts1 = Eq.Site[SiteTemp].SwMatrix.ActivatePath("N77", Operation.VSGtoTX1); //mario
                }

                EqSwitchMatrix.PortCombo sourcePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation); 

                CalFile thisCalFile = new CalFile(site, sourcePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;

                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    MessageBox.Show("Site " + (site + 1) + " Source Calibration\n\nPlease connect the Power Sensor to " + sourcePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration");

                    float CalResultMin = 999;
                    float CalResultMax = -999;

                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {
                        float calVal = 0;

                        foreach (double freq in calMasterFreqsMHz)
                        {
                            if (freq <= maxFreqMHz)   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {


                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = freq * 1e6;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);
                                Eq.Site[site].RF.SG.ApplyChange();

                                Eq.Site[site].PM.SetupMeasurement(freq, 0.001, 10);
                                Thread.Sleep(10);

                                float measVal = (float)Eq.Site[site].PM.Measure();
                                calVal = measVal - powerLevel;

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;

                            }

                            swCalDataFile.Write(freq + "," + calVal + "\n");
                        }
                    }

                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }
                }

                // Save a backup
                string backupFile = strTargetCalDataFile;
                backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                File.Delete(backupFile);
                File.Copy(strTargetCalDataFile, backupFile);

                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Abort();

                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static int CalibrateMeasurePath(byte site, Operation operation, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, EqSwitchMatrix.PortCombo sourcePath = null)
        {
            return CalibrateMeasurePath(site, "", operation, onboardAttenDb, powerLevel, maxFreqMHz, CalLimitLow, CalLimitHigh, sourcePath);
        }

        public static int CalibrateMeasurePath(byte site, string band, Operation operation, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, EqSwitchMatrix.PortCombo sourcePort = null)
        {
            try
            {
                // For Quadsite, single Controller dual sites 	
                string BandTemp = band;
                byte SiteTemp = site;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                }

                EqSwitchMatrix.PortCombo measurePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

                CalFile thisCalFile = new CalFile(site, measurePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;

                Dictionary<double, float> sourceCalFactors = GetCalfactorFileContents(site, sourcePort);

                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    if (sourcePort != null)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath(sourcePort);
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between " + sourcePort.dutPort.ToString() + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }
                    else
                    {
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between the cable end of the VSG" + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }

                    float CalResultMin = 999;
                    float CalResultMax = -999;

                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {
                        float calVal = 0;

                        foreach (double freq in calMasterFreqsMHz)
                        {
                            if (freq <= maxFreqMHz)   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));
                                Eq.Site[site].RF.SG.CenterFrequency = freq * 1e6;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);

                                double measVal = 0f;
                                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, freq, 0, 0, site);
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);

                                    measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);
                                }
                                else
                                {
                                    Eq.Site[site].RF.SA.CenterFrequency = freq * 1e6;
                                    Eq.Site[site].RF.SA.ReferenceLevel = powerLevel + sourceCalFactors[freq];
                                    Eq.Site[site].RF.SA.ExternalGain = 0;

                                    Thread.Sleep(10);
                                    measVal = Eq.Site[site].RF.SA.MeasureChanPower();
                                }

                                calVal = (float)measVal - (powerLevel + sourceCalFactors[freq]);

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;

                            }

                            swCalDataFile.Write(freq + "," + calVal + "\n");
                        }
                    }

                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }
                }

                // Save a backup
                string backupFile = strTargetCalDataFile;
                backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                File.Delete(backupFile);
                File.Copy(strTargetCalDataFile, backupFile);

                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Abort();

                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static int CalibrateSourcePathwithHMU_Over6Ghz(byte site, string band, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, Operation operation)
        {
            try
            {
                // For Quadsite, single Controller dual sites 	
                string BandTemp = band;
                byte SiteTemp = site;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                }

                EqSwitchMatrix.PortCombo sourcePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

                CalFile thisCalFile = new CalFile(site, sourcePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;


                Eq.Site[site].RF.SG.Abort();

                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.CenterFrequency = 2500 * 1e6;

                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);

                powerLevel = -25; //org -10

                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    MessageBox.Show("Site " + (site + 1) + " Source Calibration\n\nPlease connect the Power Sensor to " + sourcePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration");

                    float CalResultMin = 999;
                    float CalResultMax = -999;


                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {


                        float calVal = 0;

                        double SA_offset = 0f;
                        double SG_offset = 0f;
                        double Return_SG_offset = 0f;
                        float measVal = 0f;
                        double Gain = 0f;

                        foreach (double freq in calMasterFreqsMHz)
                        {
                            if (freq < 6000)   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {
                                //  Eq.EqMXA.INITIALIZATION(0);

                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = freq * 1e6;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);


                                double Infreq = 0f;
                                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);
                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out Infreq);

                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(Infreq);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);


                                Eq.Site[site].PM.SetupMeasurement(freq, 0.001, 10);
                                Thread.Sleep(10);

                                measVal = (float)Eq.Site[site].PM.Measure();
                                calVal = measVal - powerLevel;

                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;

                            }

                            else if (freq >= 6000)
                            {
                                if (freq == 6000)
                                {
                                    MessageBox.Show("Site " + (site + 1) + " Please Convert source-port from VST to HMU and " + sourcePorts.dutPort.ToString() +
                                        " connect to MXA for > 6Ghz. Do connect VST OUT to HMU IFIN also.\n\nThen click OK"); //mario
                                }
                                double Infreq = 0f;
                                double rfExtddRxOutFreq = 0f;
                                float Mease = 0f;

                                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);
                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out Infreq);

                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(Infreq);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);

                                Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();
                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);
                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = (powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor)));


                                Eq.Site[site].RF.SG.CenterFrequency = Infreq;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);

                                if (freq > 8000)
                                {
                                }
                                double value = 0;
                                try
                                {
                                    Eq.EqMXA.AMPLITUDE_REF_LEVEL(17); //org 10
                                    Eq.EqMXA.SPAN(0.2);
                                    Eq.EqMXA.CENTER_FREQUENCY(freq);

                                    Eq.EqMXA.TRIGGER_SINGLE();
                                    Eq.EqMXA.TRIGGER_IMM();

                                    Eq.EqMXA.OPERATION_COMPLETE();

                                    value = Eq.EqMXA.MEASURE_PEAK_POINT();
                                }
                                catch
                                {
                                    MessageBox.Show("MXA connection error!");
                                } 
                                
                                calVal = Convert.ToSingle(value) - powerLevel;

                                //if (freq < 8000)
                                //{
                                //    //ChoonChin (20200929) - no MXA use for cable cal
                                //    Eq.Site[site].PM.SetupMeasurement(freq, 0.001, 10);
                                //    Thread.Sleep(10);
                                //    measVal = (float)Eq.Site[site].PM.Measure();
                                //}
                                //else
                                //{
                                //    measVal = 0;
                                //}                                
                                //calVal = measVal - powerLevel;
                                ////

                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;

                            }

                        }
                    }

                    string backupFile = strTargetCalDataFile;
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                    File.Delete(backupFile);
                    File.Copy(strTargetCalDataFile, backupFile);

                    Eq.Site[site].RF.SG.Level = -100;
                    Eq.Site[site].RF.SG.Abort();

                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }


                }


                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static int CalibrateMeasurePathWithHMU(byte site, string band, Operation operation, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, EqSwitchMatrix.PortCombo sourcePort, double Ref_Level)
        {
            try
            {
                // For Quadsite, single Controller dual sites 	
                string BandTemp = band;
                byte SiteTemp = site;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                }

                EqSwitchMatrix.PortCombo measurePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

                CalFile thisCalFile = new CalFile(site, measurePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;

                Dictionary<double, float> sourceCalFactors = GetCalfactorFileContents(site, sourcePort);


                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);

                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    if (sourcePort != null)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath(sourcePort);
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between " + sourcePort.dutPort.ToString() + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }
                    else
                    {
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between the cable end of the VSG" + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }

                    float CalResultMin = 999;
                    float CalResultMax = -999;

                    strTargetCalDataFile = calFileDir + thisCalFile.name;


                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {
                        float calVal = 0;
                        double SA_offset = 0f;
                        double SG_offset = 0f;
                        double measVal = 0f;

                        foreach (double freq in calMasterFreqsMHz)
                        {
                            if (freq <= 6000)   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {

                                double rfExtddRxOutFreq = 0f;

                                if (operation == Operation.MeasureH2_ANT1 || operation == Operation.MeasureH2_ANT2 || operation == Operation.MeasureH2_ANT3)
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();
                                }
                                else
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXBypass(0);
                                }

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);




                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));
                                Eq.Site[site].RF.SG.CenterFrequency = freq * 1e6;

                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);



                                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                                {

                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, freq, Ref_Level, 0, site);
                                    Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                    Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);
                                    Thread.Sleep(10);

                                    measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);
                                }
                                else
                                {
                                    //Eq.Site[site].RF.SA.CenterFrequency = freq.Freq * 1e6;
                                    //Eq.Site[site].RF.SA.ReferenceLevel = powerLevel + sourceCalFactors[freq];
                                    //Eq.Site[site].RF.SA.ExternalGain = 0;

                                    //Thread.Sleep(10);
                                    //measVal = Eq.Site[site].RF.SA.MeasureChanPower();
                                }

                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);


                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;
                            }
                            else
                            {
                                double rfExtddRxOutFreq = 0f;

                                if (operation == Operation.MeasureH2_ANT1 || operation == Operation.MeasureH2_ANT2 || operation == Operation.MeasureH2_ANT3)
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();
                                }
                                else
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXBypass(0);
                                }

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);


                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));
                                Eq.Site[site].RF.SG.CenterFrequency = freq * 1e6;

                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);


                                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, freq, Ref_Level, 0, site);
                                    Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                    Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);
                                    Thread.Sleep(10);

                                    measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);
                                }
                                else
                                {
                                    //Eq.Site[site].RF.SA.CenterFrequency = freq.Freq * 1e6;
                                    //Eq.Site[site].RF.SA.ReferenceLevel = powerLevel + sourceCalFactors[freq];
                                    //Eq.Site[site].RF.SA.ExternalGain = 0;

                                    //Thread.Sleep(10);
                                    //measVal = Eq.Site[site].RF.SA.MeasureChanPower();
                                }

                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);


                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;
                            }


                            string backupFile = strTargetCalDataFile;
                            backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                            backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                            Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                            File.Delete(backupFile);
                            File.Copy(strTargetCalDataFile, backupFile);

                            Eq.Site[site].RF.SG.Level = -100;
                            Eq.Site[site].RF.SG.Abort();
                        }
                        break;
                    }

                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }
                }



                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static int CalibrateMeasurePathWithHMU_Over6Ghz(byte site, string band, Operation operation, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, EqSwitchMatrix.PortCombo sourcePort, double Rev_Level)
        {
            try
            {
                // For Quadsite, single Controller dual sites 	
                string BandTemp = band;
                byte SiteTemp = site;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                }

                EqSwitchMatrix.PortCombo measurePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

                CalFile thisCalFile = new CalFile(site, measurePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;

                Dictionary<double, float> sourceCalFactors = GetCalfactorFileContents(site, sourcePort);

                //   Eq.Site[site].RF.SG.SetLofreq(0);
                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);
                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    if (sourcePort != null)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath(sourcePort);
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between " + sourcePort.dutPort.ToString() + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }
                    else
                    {
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between the cable end of the VSG" + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }

                    float CalResultMin = 999;
                    float CalResultMax = -999;

                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {
                        float calVal = 0;
                        double SA_offset = 0f;
                        double SG_offset = 0f;
                        double measVal = 0f;
                        double Gain = 0f;
                        double Inf = 0f;

                        foreach (double freq in calMasterFreqsMHz)
                        {
                          

                            int k = 0;

                            if (freq < 6000 )   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {

                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out Inf);
                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(Inf);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);

                                //     Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter

                                double rfExtddRxOutFreq = 0f;

                                if (operation == Operation.MeasureH2_ANT1 || operation == Operation.MeasureH2_ANT2 || operation == Operation.MeasureH2_ANT3)
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();
                                }
                                else 
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXBypass(0);
                                }

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);                                


                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = Inf;

                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);



                                Thread.Sleep(10);

                                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec_Calibration (0, freq, Rev_Level, 0, site);
                                    Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                    Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);


                                    measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);



                                }
                                else
                                {
                                    //Eq.Site[site].RF.SA.CenterFrequency = freq.Freq * 1e6;
                                    //Eq.Site[site].RF.SA.ReferenceLevel = powerLevel + sourceCalFactors[freq];
                                    //Eq.Site[site].RF.SA.ExternalGain = 0;

                                    //Thread.Sleep(10);
                                    //measVal = Eq.Site[site].RF.SA.MeasureChanPower();
                                }

                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);
                                if(freq == 4175)
                                {

                                }

                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;



                                k++;
                            }
                            else if (freq >= 6000)
                            {
                                //if (freq == 6030)
                                //{
                                //    MessageBox.Show("Please Convert source-port from VST to HMU \n\nThen click OK"); //mario
                                //}

                                powerLevel = -10;

                                double rfExtddRxOutFreq = 0f;
                                double InFreq = 0f;


                                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);

                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out InFreq);

                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(InFreq);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);

                                Eq.Site[site].RF.SG.Abort();

                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = InFreq;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);


                                Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();

                                //     Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(10000 * 1e6, 1, out rfExtddRxOutFreq);

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);

                                Thread.Sleep(100);

                                if (freq > 9970)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, -30, 0, site);
                                }

                                else if (freq > 9000)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, -30, 0, site);
                                }
                                else if (freq > 8500)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, -20, 0, site);
                                }

                                else
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, -15, 0, site);
                                }


                                Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);


                                measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);


                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);

                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;
                                k++;
                            }
                        }
                    }
                    string backupFile = strTargetCalDataFile;
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                    File.Delete(backupFile);
                    File.Copy(strTargetCalDataFile, backupFile);

                    Eq.Site[site].RF.SG.Level = -100;
                    Eq.Site[site].RF.SG.Abort();

                    break;


                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }
                }



                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }
        public static int CalibrateMeasurePathWithHMU_Over6Ghz_2nd(byte site, string band, Operation operation, OnboardAtten onboardAttenDb, float powerLevel, double maxFreqMHz, float CalLimitLow, float CalLimitHigh, EqSwitchMatrix.PortCombo sourcePort, double Rev_Level)
        {
            try
            {
                // For Quadsite, single Controller dual sites 	
                string BandTemp = band;
                byte SiteTemp = site;
                if (site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + site.ToString();
                    SiteTemp = 0;
                }

                EqSwitchMatrix.PortCombo measurePorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

                CalFile thisCalFile = new CalFile(site, measurePorts, onboardAttenDb);

                if (allCalFiles.ContainsCalFile(thisCalFile))
                    return 0;

                allCalFiles.Add(thisCalFile);

                if (!runCableCal) return 0;

                Dictionary<double, float> sourceCalFactors = GetCalfactorFileContents(site, sourcePort);

                //   Eq.Site[site].RF.SG.SetLofreq(0);
                powerLevel = -25; //org -10
                Eq.Site[site].RF.SG.Abort();
                Eq.Site[site].RF.SetActiveWaveform("CW", "", false);
                Eq.Site[site].RF.SG.Level = -100;
                Eq.Site[site].RF.SG.Initiate();

                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);
                string strTargetCalDataFile = calFileDir + thisCalFile.name;

                while (true)
                {
                    if (sourcePort != null)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath(sourcePort);
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between " + sourcePort.dutPort.ToString() + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }
                    else
                    {
                        MessageBox.Show("Site " + (site + 1) + " Calibration: Please connect a thru between the cable end of the VSG" + " & " + measurePorts.dutPort.ToString() + "\n\nThen click OK", "Cable Calibration - Measure Path");
                    }

                    float CalResultMin = 999;
                    float CalResultMax = -999;

                    using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
                    {
                        float calVal = 0;
                        double SA_offset = 0f;
                        double SG_offset = 0f;
                        double measVal = 0f;
                        double Gain = 0f;
                        double Inf = 0f;

                        foreach (double freq in calMasterFreqsMHz)
                        {


                            int k = 0;

                            if (freq < 6000)   // only measure calVal if freq is in SG range, otherwise use prevous calVal
                            {

                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out Inf);
                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(Inf);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);

                                //     Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter

                                double rfExtddRxOutFreq = 0f;

                                if (operation == Operation.MeasureH2_ANT1 || operation == Operation.MeasureH2_ANT2 || operation == Operation.MeasureH2_ANT3)
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();
                                }
                                else
                                {
                                    Eq.Site[site].RF.RFExtd.ConfigureRXBypass(0);
                                }

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);

                                Eq.Site[site].RF.SG.Abort();
                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = Inf;

                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);



                                Thread.Sleep(10);

                                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, freq, Rev_Level, 0, site);
                                    Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                    Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);


                                    measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);



                                }
                                else
                                {
                                    //Eq.Site[site].RF.SA.CenterFrequency = freq.Freq * 1e6;
                                    //Eq.Site[site].RF.SA.ReferenceLevel = powerLevel + sourceCalFactors[freq];
                                    //Eq.Site[site].RF.SA.ExternalGain = 0;

                                    //Thread.Sleep(10);
                                    //measVal = Eq.Site[site].RF.SA.MeasureChanPower();
                                }

                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);


                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;



                                k++;
                            }
                            else if (freq >= 6000)
                            {
                                if (freq == 6000)
                                {
                                    MessageBox.Show("Site " + (site + 1) + " Please Convert source-port from VST to HMU. Do connect VST OUT to HMU IFIN also. \n\nThen click OK"); //mario
                                }

                                powerLevel = -25; //org -10

                                double rfExtddRxOutFreq = 0f;
                                double InFreq = 0f;


                                Eq.Site[site].RF.RFExtd.ConfigureTXPort(1);

                                Eq.Site[site].RF.RFExtd.ConfigureCalibrationTone(freq * 1e6, out InFreq);

                                Eq.Site[site].RF.RFExtd.ConfigureTXInputFreq(InFreq);
                                Eq.Site[site].RF.RFExtd.ConfigureTXOutputFreq(freq * 1e6);

                                Eq.Site[site].RF.SG.Abort();

                                Eq.Site[site].RF.SG.Level = powerLevel - (20 * Math.Log10(Eq.Site[site].RF.SG.Scaling_Factor));

                                Eq.Site[site].RF.SG.CenterFrequency = InFreq;
                                Eq.Site[site].RF.SG.SetLofreq(0 * 1e6);


                                Eq.Site[site].RF.RFExtd.ConfigureRXDownconversion();

                                //     Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(10000 * 1e6, 1, out rfExtddRxOutFreq);

                                Eq.Site[site].RF.RFExtd.ConfigureHarmonicConverter(freq * 1e6, 1, out rfExtddRxOutFreq);

                                Thread.Sleep(100);                                

                                if (freq > 9970)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, 5, 0, site);
                                }

                                else if (freq > 9000)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, 10, 0, site);
                                }
                                else if (freq > 8500)
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, 15, 0, site);
                                }

                                else
                                {
                                    Eq.Site[site].RF.CRfmxCHP_FOR_CAL.ConfigureSpec(0, rfExtddRxOutFreq / 1e6, 15, 0, site);
                                }

                                if (freq == 8040)
                                { }
                                Eq.Site[site].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
                                Eq.Site[site].RF.CRfmxCHP_FOR_CAL.InitiateSpec(0);
                                Eq.Site[site].RF.InstrSession.GetDownconverterFrequencyOffset("", out SA_offset);


                                measVal = Eq.Site[site].RF.CRfmxCHP_FOR_CAL.RetrieveResults(0);


                                calVal = (float)measVal - (float)(powerLevel + sourceCalFactors[freq]);

                                swCalDataFile.Write(freq + "," + calVal + "\n");

                                if (calVal < CalResultMin)
                                    CalResultMin = calVal;
                                if (calVal > CalResultMax)
                                    CalResultMax = calVal;
                                k++;
                            }
                        }
                    }
                    string backupFile = strTargetCalDataFile;
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\Backup");
                    backupFile = backupFile.Insert(backupFile.LastIndexOf('\\'), "\\" + dateCode);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                    File.Delete(backupFile);
                    File.Copy(strTargetCalDataFile, backupFile);

                    Eq.Site[site].RF.SG.Level = -100;
                    Eq.Site[site].RF.SG.Abort();

                    break;


                    if ((CalResultMin > CalLimitLow) && (CalResultMax < CalLimitHigh))
                    {
                        break;
                    }
                    else
                    {
                        DialogResult res = MessageBox.Show("Calibration results are out of limits.", "Cable Calibration", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);
                        if (res == DialogResult.Cancel) break;
                    }
                }



                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }
        public static int CalibrateMeasurePathWithNA(byte site, string band, Operation operation, OnboardAtten onboardAttenDb)
        {
            // For Quadsite, single Controller dual sites 	
            string BandTemp = band;
            byte SiteTemp = site;
            if (site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + site.ToString();
                SiteTemp = 0;
            }

            EqSwitchMatrix.PortCombo harmonicPorts = Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, operation);

            CalFile thisCalFile = new CalFile(site, harmonicPorts, onboardAttenDb);

            if (allCalFiles.ContainsCalFile(thisCalFile))
                return 0;

            allCalFiles.Add(thisCalFile);

            if (!runCableCal) return 0;

            MessageBox.Show("Connect " + harmonicPorts.dutPort + " cable to ENA Port 1 cable.\n\n Connect ENA Port 2 cable to the power sensor port of the switchmatrix box. \n\n Click OK when finished.", "Cable Calibration - Harmonic Measurement");

            Eq.Site[0].EqNetAn.defineTracex(1, 1, "S21");

            List<double> freqData = Eq.Site[0].EqNetAn.ReadFreqList_Chan(1);
            double[] traceData = Eq.Site[0].EqNetAn.ReadENATrace(1, 1);

            string strTargetCalDataFile = calFileDir + thisCalFile.name;

            using (StreamWriter swCalDataFile = new StreamWriter(strTargetCalDataFile))
            {
                for (int i = 0; i < traceData.Length; i++)
                {
                    swCalDataFile.Write(freqData[i] + "," + (traceData[i] - (float)onboardAttenDb) + "\n");
                }
            }

            return 1;
        }

        public static int Initialize(bool runCableCal, string calibrationDirectoryName, string MasterFreqFile_fullPath)
        {
            CableCal.runCableCal = runCableCal;
            calFileDir = @"C:\Avago.ATF.Common.x64\CableCalibration\" + calibrationDirectoryName + "\\";
            ATFCrossDomainWrapper.Cal_SwitchInterpolationFlag(true);

            if (!runCableCal && !Directory.Exists(calFileDir))
            {
                MessageBox.Show("Cable Calibration has not been run.\nMust run Cable Calibration.", "Cable Calibration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                runCableCal = true;
                Directory.CreateDirectory(calFileDir);
            }

            for (int site = 0; site < Eq.NumSites; site++)
            {
                CalFactorQuickRetrieve.Add(new Dictionary<string, float>());
            }

            if (!runCableCal) return 0;

            #region Read Master Cal Freq List
            try
            {
                calMasterFreqsMHz.Clear();

                foreach (string line in File.ReadAllLines(MasterFreqFile_fullPath))
                {
                    calMasterFreqsMHz.Add(Convert.ToDouble(line.Trim().Split(',').First()));
                }

                calMasterFreqsMHz.Sort();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading Calibration Master Frequency list\n\n" + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion

            return 0;
        }

        public static Dictionary<double, float> GetCalfactorFileContents(byte site, EqSwitchMatrix.PortCombo portCombo)
        {
            string calFileName = portCombo == null ?
                "S" + site + "_VSGPowerCal.csv" :
                CalFile.GetName(site, portCombo);

            Dictionary<double, float> sourceCalFactors = new Dictionary<double, float>();

            try
            {
                foreach (string line in File.ReadAllLines(calFileDir + calFileName))
                {
                    string[] splitter = line.Trim().Split(',');
                    sourceCalFactors.Add(Convert.ToDouble(splitter[0]), Convert.ToSingle(splitter[1]));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading cable cal file\n\n" + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return sourceCalFactors;
        }

        public static void LoadAllCalibratedFiles()
        {
            foreach (CalFile calFile in allCalFiles)
            {
                calFile.Load();
            }
        }

        public class CalFile
        {
            public readonly byte site;
            public readonly EqSwitchMatrix.PortCombo portCombo;
            public readonly OnboardAtten onboardAtten;
            public readonly string name;

            public CalFile(byte site, EqSwitchMatrix.PortCombo portCombo, OnboardAtten onboardAtten)
            {
                this.site = site;
                this.portCombo = portCombo;
                this.onboardAtten = onboardAtten;
                this.name = GetName(site, portCombo);
            }

            public bool Load()
            {
                try
                {
                    ATFCrossDomainWrapper.Cal_LoadCalData(name, calFileDir + name);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }
            }

            public float GetCalfactor(double freq)
            {
                float calFactor = (float)ATFCrossDomainWrapper.Cal_GetCalData1D(name, freq);

                calFactor -= (float)onboardAtten;

                return calFactor;
            }

            public static string GetName(byte site, EqSwitchMatrix.PortCombo portCombo)
            {
                return "S" + site + "_" + portCombo.instrPort.ToString() + "_" + portCombo.dutPort.ToString() + ".csv";
            }
        }

        public static bool ContainsCalFile(this List<CalFile> allCalFiles, CalFile targetCalFile)
        {
            var matchingCalFiles =
                from calfile in allCalFiles
                where calfile.site == targetCalFile.site
                && calfile.portCombo == targetCalFile.portCombo
                select calfile;

            System.Diagnostics.Debug.Assert(matchingCalFiles.Count() <= 1, "Multiple cal files of same properties");

            return matchingCalFiles.Count() > 0;
        }

        public enum OnboardAtten
        {
            None = 0,
            Atten1dB = 1,
            Atten3dB = 3,
            Atten6dB = 6,
            Atten10dB = 10,
            Atten20dB = 20,
        }
    }
}
