using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.RFToolkits.Interop;
using NationalInstruments.ModularInstruments.niLTESG;

using Avago.ATF.StandardLibrary;

namespace ClothoLibStandard
{
    public class PxiSg
    {
        niGSMSG gsmsgSession;
        niRFSG rfsgSession;
        niEDGESG edgeSgSession;
        niWCDMASG wcdmaSession;
        NiLteSg lteSession;
                
        string GsmScript = @"script GsmWaveform
  repeat forever
    generate GsmNormalBurst marker0 (0)
    generate GsmNormalBurst
    repeat 6
      generate GsmIdleBurst
    end repeat
   end repeat
    end script";
        
        string EdgeScript = @"script EdgeWaveform
  repeat forever
    generate EDGENormalBurst marker0 (0)
    generate EDGENormalBurst
    repeat 6
      generate EDGEIdleBurst
    end repeat
   end repeat
    end script";

        string scriptWcdma = @"script GenerateULDPCH
repeat forever
 generate GTC3 marker0 (0)
end repeat
end script";

        string scriptWcdmaGtc1New = @"script GenerateGtc1New
repeat forever
 generate Gtc1New marker0 (0)
end repeat
end script";

        string scriptWcdmaGtc3 = @"script WcdmaGtc3Signal
repeat forever
 generate WcdmaGtc3 marker0 (0)
end repeat
end script";


        string scriptLte = @"script LTEUplinkSignal
repeat forever
 generate LTEUplink marker0 (0, 15360, 30720, 46080, 61440, 76800, 92160, 107520, 122880, 138240)
end repeat
end script";

        string scriptLte2 = @"script LTEUplinkSignal2
repeat forever
 generate LTEUplink2 marker0 (0, 15360, 30720, 46080, 61440, 76800, 92160, 107520, 122880, 138240)
end repeat
end script";

        string scriptLte3 = @"script LTEUplinkSignal3
repeat forever
 generate LTEUplink3 marker0 (0, 15360, 30720, 46080, 61440, 76800, 92160, 107520, 122880, 138240)
end repeat
end script";

        string scriptEvdo = @"script ScriptEvdo
   repeat forever
      generate EvDo marker0 (0)
   end repeat
end script";

        string scriptTdscdmaTs1s1p28MHz = @"script TdscdmaTs1s1p28MHz
repeat forever
 generate TdscdmaTs1 marker0 (19022)
end repeat
end script";

        string scriptLteTdd = @"script LteTddSignal
repeat forever
 generate LteTdd marker0 (30686, 107486)
end repeat
end script";

        niComplexNumber[] lteWaveform;

        public int CloseGsmEdge()
        {
            try
            {
                // Close the RFSG 
                if (rfsgSession != null)
                    rfsgSession.Dispose();
                // Close GSMSG session
                if (gsmsgSession != null)
                    gsmsgSession.Close();
                // Close EDGE session
                if (edgeSgSession != null)
                    edgeSgSession.Close();
                

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int ModulationChange(string previousModulationName, string newModulationName)
        {
            try
            {
                if (previousModulationName != "WCDMA" && newModulationName == "WCDMA")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    wcdmaSession.RFSGConfigureScript(rfsgSession.Handle, null, scriptWcdmaGtc1New , -100);                    
                    rfsgSession.Initiate();                    
                }
                else if (previousModulationName != "LTE" && newModulationName == "LTE")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "LTEUplinkSignal");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName != "1xEvDO" && newModulationName == "1XEVDO")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "ScriptEvdo");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 19660800);
                    rfsgSession.ConfigureSignalBandwidth(1228800);
                    
                    rfsgSession.Initiate();
                }
                else if (previousModulationName == "GSM" && newModulationName == "CW")
                {
                    // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName == "EDGE" && newModulationName == "CW")
                {
                    //rfsgSession.Abort();
                    //gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, -100);                    
                    //rfsgSession.Initiate();
                    //rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName == "CW" && newModulationName == "GSM")
                {
                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, -100);
                    rfsgSession.Initiate();
                    rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger0");
                }
                else if (previousModulationName == "EDGE" && newModulationName == "GSM")
                {
                    rfsgSession.Abort();
                    gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, -100);
                    rfsgSession.Initiate();
                    // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger0");
                }
                else if (previousModulationName == "CW" && newModulationName == "EDGE")
                {
                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    edgeSgSession.RFSGConfigureScript(rfsgSession.Handle, EdgeScript, -100);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName == "GSM" && newModulationName == "EDGE")
                {
                    rfsgSession.Abort();
                    edgeSgSession.RFSGConfigureScript(rfsgSession.Handle, EdgeScript, -100);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName == "" && newModulationName == "CW")
                {
                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);
                    rfsgSession.Initiate();
                }
                else if (previousModulationName != "CW" && newModulationName == "CW")
                {
                    rfsgSession.Abort();
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);
                    rfsgSession.Initiate();
                }


                ATFCrossDomainWrapper.StoreStringToCache("MOD", newModulationName);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                return -1;
            }
        }

        public int ModulationChange2(string previousModulationName, string newModulationName, string waveform)
        {
            try
            {
                if (previousModulationName != newModulationName)                    // db added this
                {
                    rfsgSession.Abort();                                            // db added this
                    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);    // db added this
                }

                if (waveform == "10M12RB38S" && newModulationName == "LTE")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "LTEUplinkSignal");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);
                    rfsgSession.Initiate();
                }
                else if (waveform == "10M20RB" && newModulationName == "LTE")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "LTEUplinkSignal2");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);
                    rfsgSession.Initiate();
                }
                else if (waveform == "10M12RB" && newModulationName == "LTE")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "LTEUplinkSignal3");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.Initiate();
                }
                else if (waveform == "TDSCDMA_TS1_1P28MHZ" && newModulationName == "TDSCDMA")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "TdscdmaTs1s1p28MHz");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 20480000);
                    rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.7);
                    rfsgSession.Initiate();
                }
                else if (waveform == "GTC1" && newModulationName == "WCDMA")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "WcdmaGtc3Signal");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 7680000);
                    rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);
                    rfsgSession.Initiate();
                }
                else if (waveform == "10M12RB" && newModulationName == "LTETDD")
                {
                    rfsgSession.Abort();
                    //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                    rfsgSession.SetString(niRFSGProperties.SelectedScript, "LteTddSignal");
                    rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                    rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);
                    rfsgSession.Initiate();
                }
                else
                {
                    MessageBox.Show("Error");
                }

                ATFCrossDomainWrapper.StoreStringToCache("MOD", newModulationName);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                return -1;
            }
        }

        public int ModulationEnable(bool State)
        {
            try
            {
                //if (State)
                //    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                //else
                //    rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);

                // rfsgSession.ConfigureIQEnabled(State);


                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
            
        }
        
        public int Freq(float SetFreq)
        {
            // bool booltemp = rfsgSession.GetBoolean(niRFSGProperties.LocalOscillatorOut0Enabled);

            try
            {
                // double tempresult = rfsgSession.GetDouble(niRFSGProperties.PowerLevel);

                rfsgSession.SetDouble(niRFSGProperties.Frequency, null, SetFreq);
                // rfsgSession.Initiate();
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetFreq");
                return -1;
            }
        }

        public int InitializeGsmEdge()
        {
            try
            {
                gsmsgSession = new niGSMSG(niGSMSGConstants.ToolkitVersion100);
                rfsgSession = new niRFSG("VSG", false, true);
                edgeSgSession = new niEDGESG(niEDGESGConstants.ToolkitVersion100);

                // Additional 10 MHz clock output
                rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.RefOutStr);

                // Set Properties to the GSM Generation Session 
                gsmsgSession.SetCarrierMode(String.Empty, niGSMSGConstants.CarrierModeBurst);
                gsmsgSession.SetPayloadControlTsc(String.Empty, 0);
                gsmsgSession.SetPayloadControlPnOrder(String.Empty, 15);
                gsmsgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                // Set Properties to the EDGE Generation Session 
                edgeSgSession.SetCarrierMode(String.Empty, niEDGESGConstants.CarrierModeBurst);
                edgeSgSession.SetPayloadControlTsc(String.Empty, 0);
                edgeSgSession.SetPayloadControlPnOrder(String.Empty, 15);
                edgeSgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                ConfigureRFSGSession(1000000000);
                double powerLevel = -80;

                //Create and download GSM waveforms
                gsmsgSession.SetBurstType(String.Empty, niGSMSGConstants.NormalBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmNormalBurst");
                gsmsgSession.SetBurstType("", niGSMSGConstants.IdleBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmIdleBurst");

                //Create and download CW waveform
                //gsmsgSession.SetBurstType("", niGSMSGConstants.NormalBurstCw);
                //gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "CWWaveform");

                rfsgSession.WriteScript(GsmScript);

                //Create and download EDGE waveforms
                edgeSgSession.SetBurstType("", niEDGESGConstants.NormalBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGENormalBurst");
                edgeSgSession.SetBurstType("", niEDGESGConstants.IdleBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGEIdleBurst");
                
                rfsgSession.WriteScript(EdgeScript);

                gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, powerLevel);
                // edgeSgSession.RFSGConfigureScript(rfsgSession.Handle, EdgeScript, powerLevel);
                rfsgSession.Initiate();

                // Waveform to CW
                //rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), this.ToString());

                return -1;
            }
        }

        public int InitializeGsmEdgeCw()
        {
            try
            {
                rfsgSession = new niRFSG("VSG", false, true);
                gsmsgSession = new niGSMSG(niGSMSGConstants.ToolkitVersion100);                
                edgeSgSession = new niEDGESG(niEDGESGConstants.ToolkitVersion100);


                // Additional 10 MHz clock output
                //rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.RefOutStr);
                rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.ClkOutStr);
                

                // Set Properties to the GSM Generation Session 
                gsmsgSession.SetCarrierMode(String.Empty, niGSMSGConstants.CarrierModeBurst);
                gsmsgSession.SetPayloadControlTsc(String.Empty, 0);
                gsmsgSession.SetPayloadControlPnOrder(String.Empty, 15);
                gsmsgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                // Set Properties to the EDGE Generation Session 
                edgeSgSession.SetCarrierMode(String.Empty, niEDGESGConstants.CarrierModeBurst);
                edgeSgSession.SetPayloadControlTsc(String.Empty, 0);
                edgeSgSession.SetPayloadControlPnOrder(String.Empty, 15);
                edgeSgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                ConfigureRFSGSession(1000000000);
                double powerLevel = -80;

                //Create and download GSM waveforms
                gsmsgSession.SetBurstType(String.Empty, niGSMSGConstants.NormalBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmNormalBurst");
                gsmsgSession.SetBurstType("", niGSMSGConstants.IdleBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmIdleBurst");
                
                //Create and download CW waveform
                gsmsgSession.SetBurstType("", niGSMSGConstants.NormalBurstCw);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "CWWaveform");

                rfsgSession.WriteScript(GsmScript);

                //Create and download EDGE waveforms
                edgeSgSession.SetBurstType("", niEDGESGConstants.NormalBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGENormalBurst");
                edgeSgSession.SetBurstType("", niEDGESGConstants.IdleBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGEIdleBurst");
                rfsgSession.WriteScript(EdgeScript);

                gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, powerLevel);


                // rfsgSession.SetString(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ConfigureDigitalEdgeStartTrigger(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);
                
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                
                rfsgSession.Initiate();

                // Waveform to CW
                // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), this.ToString());

                return -1;
            }
        }

        public int InitializeGsmEdgeWcdmaLteCw()
        {
            try
            {
                //rfsgSession.Abort();
                if (rfsgSession == null)
                    rfsgSession = new niRFSG("VSG", false, true);
                //gsmsgSession = new niGSMSG(niGSMSGConstants.ToolkitVersion100);
                //edgeSgSession = new niEDGESG(niEDGESGConstants.ToolkitVersion100);
                if (wcdmaSession == null)
                    wcdmaSession = new niWCDMASG(niWCDMASGConstants.ToolkitCompatibilityVersion010000);

                // Additional 10 MHz clock output
                rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.RefOutStr);


                /*

                // Set Properties to the GSM Generation Session 
                gsmsgSession.SetCarrierMode(String.Empty, niGSMSGConstants.CarrierModeBurst);
                gsmsgSession.SetPayloadControlTsc(String.Empty, 0);
                gsmsgSession.SetPayloadControlPnOrder(String.Empty, 15);
                gsmsgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                // Set Properties to the EDGE Generation Session 
                edgeSgSession.SetCarrierMode(String.Empty, niEDGESGConstants.CarrierModeBurst);
                edgeSgSession.SetPayloadControlTsc(String.Empty, 0);
                edgeSgSession.SetPayloadControlPnOrder(String.Empty, 15);
                edgeSgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                */
                 
                ConfigureRFSGSession(1000000000);
                double powerLevel = -80;

                /*

                //Create and download GSM waveforms
                gsmsgSession.SetBurstType(String.Empty, niGSMSGConstants.NormalBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmNormalBurst");
                gsmsgSession.SetBurstType("", niGSMSGConstants.IdleBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmIdleBurst");

                //Create and download CW waveform
                gsmsgSession.SetBurstType("", niGSMSGConstants.NormalBurstCw);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "CWWaveform");

                rfsgSession.WriteScript(GsmScript);

                //Create and download EDGE waveforms
                edgeSgSession.SetBurstType("", niEDGESGConstants.NormalBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGENormalBurst");
                edgeSgSession.SetBurstType("", niEDGESGConstants.IdleBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGEIdleBurst");
                rfsgSession.WriteScript(EdgeScript);

                gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, powerLevel);

                */

                // rfsgSession.SetString(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ConfigureDigitalEdgeStartTrigger(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);
                rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);

                #region WCDMA GTC1 New

                InitializeWcdmaGtc1New();

                #endregion WCDMA GTC1 New

                #region WCDMA GTC3
                // InitializeWcdmaGtc3();
                #endregion WCDMA GTC3

                #region LTE

                //////////////////////////
                // LTE
                //////////////////////////

                int numPusch = 10;
                int oldWaveformLength = 0;
                
                string waveformName = "LTEUplink";
                double headroom = 0;
                               

                if (lteSession == null)
                    lteSession = new NiLteSg(niLTESGConstants.ToolkitCompatibilityVersion010000);

                lteSession.SetSystemBandwidth(null, 10000000.0);
                lteSession.SetCyclicPrefixMode(null, 0);
                lteSession.SetCellId(null, 0);
                lteSession.SetOversamplingFactor(null, 1);

                lteSession.SetAutoHeadroomEnabled(null, 1);
                lteSession.SetClipRate(null, 0);
                lteSession.SetHeadroom("antenna0", 12);

                lteSession.SetDuplexMode(null, niLTESGConstants.DuplexModeUlFdd);

                lteSession.SetUl3gppCyclicShiftEnabled(null, 1);
                lteSession.SetUlPuschNDmrs1(null, 0);
                lteSession.SetUlPuschDeltaSs(null, 0);

                lteSession.SetUlNumberOfPuschChannels(null, numPusch);
                
                for (int i = 0; i < numPusch; i++)
                {
                    string channel = "pusch" + i.ToString();

                    lteSession.SetUlPuschSubframeNumber(channel, i);
                    lteSession.SetUlPuschNDmrs2(channel, 0);
                    lteSession.SetUlPuschCyclicShiftIndex0(channel, 0);
                    lteSession.SetUlPuschCyclicShiftIndex1(channel, 0);
                    lteSession.SetUlPuschResourceBlockOffset(channel, 38);
                    lteSession.SetUlPuschNumberOfResourceBlocks(channel, 12);
                    lteSession.SetUlPuschModulationScheme(channel, 0);
                    lteSession.SetUlPuschScramblingEnabled(channel, niLTESGConstants.True);
                    lteSession.SetUlPuschPower(channel, 0);
                }
                
                // If the hardware has LO leakage, you can set upConverterCenterFrequencyOffset to non zero value to avoid the leakage in the band.
                // The recommended value is 10MHz.
                //int upConverterCenterFrequencyOffset = 0;
                //int upConverterCenterFrequency = (int)carrierFrequency + upConverterCenterFrequencyOffset;

                //rfsgSession.SetDouble(niRFSGProperties.UpconverterCenterFrequency, upConverterCenterFrequency);
                //rfsgSession.ConfigurePowerLevelType(niRFSGConstants.PeakPower);
                //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);

                double t0, dt;
                int waveformSize, generationDone, actualNumSamples;

                // Allocate the array of Waveform size
                lteSession.CreateWaveformComplexF64(
                            niLTESGConstants.True,
                            out t0,
                            out dt,
                            null,
                            0,
                            out waveformSize,
                            out generationDone);

                if (oldWaveformLength == 0 || oldWaveformLength < waveformSize)
                {
                    lteWaveform = new niComplexNumber[waveformSize];
                    oldWaveformLength = waveformSize;
                }

                // Create and download waveform
                lteSession.CreateWaveformComplexF64(
                            niLTESGConstants.True,
                            out t0,
                            out dt,
                            lteWaveform,
                            waveformSize,
                            out actualNumSamples,
                            out generationDone);

                rfsgSession.WriteArbWaveformComplexF64(
                            waveformName,
                            actualNumSamples,
                            lteWaveform,
                            false);

                double actualHeadroom, iqRate;

                lteSession.GetActualHeadroom("antenna0", out actualHeadroom);
                iqRate = 1 / dt;

                // Display actual headroom in panel
                // headroomNumeric.Value = (decimal)actualHeadroom;

                lteSession.RFSGStoreHeadroom(
                            rfsgSession.Handle,
                            null,
                            waveformName,
                            actualHeadroom);

                lteSession.RFSGStoreIQRate(
                            rfsgSession.Handle,
                            null,
                            waveformName,
                            iqRate);

                // Configure Script
                lteSession.RFSGRetrieveMinimumHeadroomAllWaveforms(
                            rfsgSession.Handle,
                            null,
                            scriptLte,
                            out headroom);

                lteSession.RFSGRetrieveIQRateAllWaveforms(
                            rfsgSession.Handle,
                            null,
                            scriptLte,
                            out iqRate);

                rfsgSession.SetDouble(niRFSGProperties.IqRate, iqRate);
                rfsgSession.SetDouble(niRFSGProperties.PowerLevel, powerLevel + headroom);

                // Obtain script name
                // string scriptName = (scriptLte.Split(' ', '\n'))[1];
                string scriptName = "LTEUplinkSignal";

                // Initiate generation
                rfsgSession.WriteScript(scriptLte);
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptName);
                // rfsgSession.ConfigureOutputEnabled(true);

                #endregion LTE

                #region 1xEV-DO

                //////////////////////////////////
                // 1xEv-DO
                /////////////////////////////////

                

                // Initialize member

                // rfsgSession.ConfigureGenerationMode(niRFSGConstants.ArbWaveform);
                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(1228800);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 19660800);
                double actualIqRateEvdo = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                FileInfo fCalEquipSource = new FileInfo(@"C:\Avago.ATF.1.2.0\Data\TestPlans\PebbleBeach\Waveform\EVDO_IQ_Data.txt");
                StreamReader srCalEquipSource = new StreamReader(fCalEquipSource.ToString());

                string strTemp = "";
                double[] dataI = new double[524288];
                double[] dataQ = new double[524288];
                string[] stringIq = new string[1048576];
                double[] dataIq;
                char[] delimiter = new char [2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                while ((strTemp = srCalEquipSource.ReadLine()) != null)
                {
                    stringIq = strTemp.Split(delimiter);                    
                }
                
                for (int i = 0; i < 524288; i++)
                {
                    dataI[i] = Convert.ToDouble(stringIq[i*2]);
                    dataQ[i] = Convert.ToDouble(stringIq[i*2+1]);
                }

                rfsgSession.WriteArbWaveform("EvDo", 524288, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptEvdo);

                string scriptNameEvdo = "ScriptEvdo";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameEvdo);
                
                srCalEquipSource.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];
                
                #endregion 1xEV-DO

                rfsgSession.Initiate();

                //double tempIqRate = rfsgSession.GetDouble(niRFSGProperties.IqRate);
                //double tempArbClockRate = rfsgSession.GetDouble(niRFSGProperties.ArbSampleClockRate);
                // rfsgSession.GetDouble(niRFSGProperties.
                // rfsgSession.SetDouble(niRFSGProperties.PowerLevel, powerLevel);
                //rfsgSession.Abort();
                // Waveform to CW
                // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), this.ToString());

                return -1;
            }
        }

        public int InitializeGsmEdgeWcdmaLteCwTempJongsoo()
        {
            try
            {
                if (rfsgSession == null)
                    rfsgSession = new niRFSG("VSG", false, true);
                //gsmsgSession = new niGSMSG(niGSMSGConstants.ToolkitVersion100);
                //edgeSgSession = new niEDGESG(niEDGESGConstants.ToolkitVersion100);
                if (wcdmaSession == null)
                    wcdmaSession = new niWCDMASG(niWCDMASGConstants.ToolkitCompatibilityVersion010000);

                // Additional 10 MHz clock output
                // rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.RefOutStr);
                // rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, "RefOut");
                rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.ClkOutStr);
                

                /*

                // Set Properties to the GSM Generation Session 
                gsmsgSession.SetCarrierMode(String.Empty, niGSMSGConstants.CarrierModeBurst);
                gsmsgSession.SetPayloadControlTsc(String.Empty, 0);
                gsmsgSession.SetPayloadControlPnOrder(String.Empty, 15);
                gsmsgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                // Set Properties to the EDGE Generation Session 
                edgeSgSession.SetCarrierMode(String.Empty, niEDGESGConstants.CarrierModeBurst);
                edgeSgSession.SetPayloadControlTsc(String.Empty, 0);
                edgeSgSession.SetPayloadControlPnOrder(String.Empty, 15);
                edgeSgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                */

                ConfigureRFSGSession(1000000000);
                double powerLevel = -80;

                /*

                //Create and download GSM waveforms
                gsmsgSession.SetBurstType(String.Empty, niGSMSGConstants.NormalBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmNormalBurst");
                gsmsgSession.SetBurstType("", niGSMSGConstants.IdleBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmIdleBurst");

                //Create and download CW waveform
                gsmsgSession.SetBurstType("", niGSMSGConstants.NormalBurstCw);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "CWWaveform");

                rfsgSession.WriteScript(GsmScript);

                //Create and download EDGE waveforms
                edgeSgSession.SetBurstType("", niEDGESGConstants.NormalBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGENormalBurst");
                edgeSgSession.SetBurstType("", niEDGESGConstants.IdleBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGEIdleBurst");
                rfsgSession.WriteScript(EdgeScript);

                gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, powerLevel);

                */

                // rfsgSession.SetString(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ConfigureDigitalEdgeStartTrigger(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);
                
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);

                #region WCDMA GTC1 New

                InitializeWcdmaGtc1New();

                #endregion WCDMA GTC1 New

                #region WCDMA GTC3
                // InitializeWcdmaGtc3();
                #endregion WCDMA GTC3

                #region LTE

                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                double actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                FileInfo fWaveIData = new FileInfo(@"C:\Data\Jongsoo\10M_12RB_RBStart38\I_LTE_QPSK_10M12RB38S.txt");
                FileInfo fWaveQData = new FileInfo(@"C:\Data\Jongsoo\10M_12RB_RBStart38\Q_LTE_QPSK_10M12RB38S.txt");
                StreamReader srWaveIData = new StreamReader(fWaveIData.ToString());
                StreamReader srWaveQData = new StreamReader(fWaveQData.ToString());

                string strTemp = "";
                double[] dataI = new double[153600];
                double[] dataQ = new double[153600];
                string[] stringIq = new string[1048576];
                
                char[] delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte);

                string scriptNameLte = "ScriptLte";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);
                
                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];


                
                #endregion LTE

                #region LTE2
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Data\Jongsoo\10M_20RB\I_LTEFU_QPSK_10M20RB.txt");
                fWaveQData = new FileInfo(@"C:\Data\Jongsoo\10M_20RB\Q_LTEFU_QPSK_10M20RB.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[153600];
                dataQ = new double[153600];
                stringIq = new string[1048576];
                
                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink2", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte2);

                scriptNameLte = "ScriptLte2";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];


                
                #endregion LTE2
                
                #region LTE3
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Portal\Waveform\LTE_FDD_UL_QPSK_10M_12RB\I_LTE_QPSK_10M12RB_091215.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\Portal\Waveform\LTE_FDD_UL_QPSK_10M_12RB\Q_LTE_QPSK_10M12RB_091215.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[153600];
                dataQ = new double[153600];
                stringIq = new string[1048576];

                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink3", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte3);

                scriptNameLte = "ScriptLte3";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion LTE3
                
                #region WCDMA GTC1
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(5000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 7680000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\WCDMA_GTC1\I_WCDMA_GTC1_20100208a.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\WCDMA_GTC1\Q_WCDMA_GTC1_20100208a.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                int iqDataPoirt = 76800;

                strTemp = "";
                dataI = new double[iqDataPoirt];
                dataQ = new double[iqDataPoirt];

                for (int i = 0; i < iqDataPoirt; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("WcdmaGtc3", iqDataPoirt, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptWcdmaGtc3);
                                
                rfsgSession.SetString(niRFSGProperties.SelectedScript, "WcdmaGtc3Signal");
                
                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion WCDMA GTC1
                
                #region 1xEV-DO

                //////////////////////////////////
                // 1xEv-DO
                /////////////////////////////////



                // Initialize member

                // rfsgSession.ConfigureGenerationMode(niRFSGConstants.ArbWaveform);
                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(1228800);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 19660800);
                double actualIqRateEvdo = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                FileInfo fCalEquipSource = new FileInfo(@"C:\Avago.ATF.1.2.0\Data\TestPlans\PebbleBeach\Waveform\EVDO_IQ_Data.txt");
                StreamReader srCalEquipSource = new StreamReader(fCalEquipSource.ToString());

                strTemp = "";
                dataI = new double[524288];
                dataQ = new double[524288];
                stringIq = new string[1048576];
                double[] dataIq;
                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                while ((strTemp = srCalEquipSource.ReadLine()) != null)
                {
                    stringIq = strTemp.Split(delimiter);
                }

                for (int i = 0; i < 524288; i++)
                {
                    dataI[i] = Convert.ToDouble(stringIq[i * 2]);
                    dataQ[i] = Convert.ToDouble(stringIq[i * 2 + 1]);
                }

                rfsgSession.WriteArbWaveform("EvDo", 524288, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptEvdo);

                string scriptNameEvdo = "ScriptEvdo";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameEvdo);

                srCalEquipSource.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];

                #endregion 1xEV-DO
                
                #region TD-SCDMA

                //////////////////////////
                // TD-SCDMA
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(1280000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 20480000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.7);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\TD-SCDMA\TS1_1M28\I_TDSCDMA_TS1_1p28MHz.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\TD-SCDMA\TS1_1M28\Q_TDSCDMA_TS1_1p28MHz.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[102400];
                dataQ = new double[102400];

                for (int i = 0; i < 102400; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("TdscdmaTs1", 102400, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptTdscdmaTs1s1p28MHz);

                scriptNameLte = "TdscdmaTs1s1p28MHz";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion TD-SCDMA
                
                #region LTE-TDD

                //////////////////////////
                // TD-SCDMA
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\LTE_TDD_UL_QPSK_10M_12RB\I_LTETU_QPSK_10M12RB.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.2.0.1\Data\TestPlans\UltraFighter\Waveform\LTE_TDD_UL_QPSK_10M_12RB\Q_LTETU_QPSK_10M12RB.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                iqDataPoirt = 153600;

                strTemp = "";
                dataI = new double[iqDataPoirt];
                dataQ = new double[iqDataPoirt];

                for (int i = 0; i < iqDataPoirt; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LteTdd", iqDataPoirt, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLteTdd);

                rfsgSession.SetString(niRFSGProperties.SelectedScript, "LteTddSignal");

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];

                #endregion LTE-TDD


                rfsgSession.Initiate();

                //double tempIqRate = rfsgSession.GetDouble(niRFSGProperties.IqRate);
                //double tempArbClockRate = rfsgSession.GetDouble(niRFSGProperties.ArbSampleClockRate);
                // rfsgSession.GetDouble(niRFSGProperties.
                // rfsgSession.SetDouble(niRFSGProperties.PowerLevel, powerLevel);
                //rfsgSession.Abort();
                // Waveform to CW
                // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), this.ToString());

                return -1;
            }
        }

        public int InitializeGsmEdgeWcdmaLteCwATM()
        {
            try
            {
                if (rfsgSession == null)
                    rfsgSession = new niRFSG("VSG", false, true);
                //gsmsgSession = new niGSMSG(niGSMSGConstants.ToolkitVersion100);
                //edgeSgSession = new niEDGESG(niEDGESGConstants.ToolkitVersion100);
                if (wcdmaSession == null)
                    wcdmaSession = new niWCDMASG(niWCDMASGConstants.ToolkitCompatibilityVersion010000);

                // Additional 10 MHz clock output
                // rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.RefOutStr);
                // rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, "RefOut");
                rfsgSession.SetString(niRFSGProperties.ExportedRefClockOutputTerminal, niRFSGConstants.ClkOutStr);


                /*

                // Set Properties to the GSM Generation Session 
                gsmsgSession.SetCarrierMode(String.Empty, niGSMSGConstants.CarrierModeBurst);
                gsmsgSession.SetPayloadControlTsc(String.Empty, 0);
                gsmsgSession.SetPayloadControlPnOrder(String.Empty, 15);
                gsmsgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                // Set Properties to the EDGE Generation Session 
                edgeSgSession.SetCarrierMode(String.Empty, niEDGESGConstants.CarrierModeBurst);
                edgeSgSession.SetPayloadControlTsc(String.Empty, 0);
                edgeSgSession.SetPayloadControlPnOrder(String.Empty, 15);
                edgeSgSession.SetPayloadControlPnSeed(String.Empty, 2147483647);

                */

                ConfigureRFSGSession(1000000000);
                double powerLevel = -80;

                /*

                //Create and download GSM waveforms
                gsmsgSession.SetBurstType(String.Empty, niGSMSGConstants.NormalBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmNormalBurst");
                gsmsgSession.SetBurstType("", niGSMSGConstants.IdleBurst);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GsmIdleBurst");

                //Create and download CW waveform
                gsmsgSession.SetBurstType("", niGSMSGConstants.NormalBurstCw);
                gsmsgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "CWWaveform");

                rfsgSession.WriteScript(GsmScript);

                //Create and download EDGE waveforms
                edgeSgSession.SetBurstType("", niEDGESGConstants.NormalBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGENormalBurst");
                edgeSgSession.SetBurstType("", niEDGESGConstants.IdleBurst);
                edgeSgSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "EDGEIdleBurst");
                rfsgSession.WriteScript(EdgeScript);

                gsmsgSession.RFSGConfigureScript(rfsgSession.Handle, GsmScript, powerLevel);

                */

                // rfsgSession.SetString(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ConfigureDigitalEdgeStartTrigger(niRFSGProperties.DigitalEdgeStartTriggerSource, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);

                // rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.Pfi0Str);
                rfsgSession.ExportSignal(niRFSGConstants.MarkerEvent, niRFSGConstants.MarkerEvent0, niRFSGConstants.PxiTrig0Str);

                #region WCDMA GTC1 New

                InitializeWcdmaGtc1New();

                #endregion WCDMA GTC1 New

                #region WCDMA GTC3
                // InitializeWcdmaGtc3();
                #endregion WCDMA GTC3

                #region LTE

                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                double actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                FileInfo fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_12RB_RBStart38\I_LTE_QPSK_10M12RB38S.txt");
                FileInfo fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_12RB_RBStart38\Q_LTE_QPSK_10M12RB38S.txt");
                StreamReader srWaveIData = new StreamReader(fWaveIData.ToString());
                StreamReader srWaveQData = new StreamReader(fWaveQData.ToString());

                string strTemp = "";
                double[] dataI = new double[153600];
                double[] dataQ = new double[153600];
                string[] stringIq = new string[1048576];

                char[] delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte);

                string scriptNameLte = "ScriptLte";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion LTE

                #region LTE2
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_20RB\I_LTEFU_QPSK_10M20RB.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_20RB\Q_LTEFU_QPSK_10M20RB.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[153600];
                dataQ = new double[153600];
                stringIq = new string[1048576];

                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink2", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte2);

                scriptNameLte = "ScriptLte2";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion LTE2

                #region LTE3
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_12RB\I_LTE_QPSK_10M12RB_091215.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_FDD_UL\QPSK\10M_12RB\Q_LTE_QPSK_10M12RB_091215.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[153600];
                dataQ = new double[153600];
                stringIq = new string[1048576];

                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                for (int i = 0; i < 153600; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LTEUplink3", 153600, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLte3);

                scriptNameLte = "ScriptLte3";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion LTE3

                #region WCDMA GTC1
                //////////////////////////
                // LTE
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(5000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 7680000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\WCDMA\Up_Link\GTC1\I_WCDMA_GTC1_20100208a.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\WCDMA\Up_Link\GTC1\Q_WCDMA_GTC1_20100208a.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                int iqDataPoirt = 76800;

                strTemp = "";
                dataI = new double[iqDataPoirt];
                dataQ = new double[iqDataPoirt];

                for (int i = 0; i < iqDataPoirt; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("WcdmaGtc3", iqDataPoirt, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptWcdmaGtc3);

                rfsgSession.SetString(niRFSGProperties.SelectedScript, "WcdmaGtc3Signal");

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion WCDMA GTC1

                #region 1xEV-DO

                //////////////////////////////////
                // 1xEv-DO
                /////////////////////////////////



                // Initialize member

                // rfsgSession.ConfigureGenerationMode(niRFSGConstants.ArbWaveform);
                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(1228800);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 19660800);
                double actualIqRateEvdo = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                FileInfo fCalEquipSource = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\CDMA\1xEvDO\Rev-A_4096\EVDO_IQ_Data.txt");
                StreamReader srCalEquipSource = new StreamReader(fCalEquipSource.ToString());

                strTemp = "";
                dataI = new double[524288];
                dataQ = new double[524288];
                stringIq = new string[1048576];
                double[] dataIq;
                delimiter = new char[2];
                delimiter[0] = ' ';
                delimiter[1] = '\t';
                //varNumOfCalFreqList = 0;
                while ((strTemp = srCalEquipSource.ReadLine()) != null)
                {
                    stringIq = strTemp.Split(delimiter);
                }

                for (int i = 0; i < 524288; i++)
                {
                    dataI[i] = Convert.ToDouble(stringIq[i * 2]);
                    dataQ[i] = Convert.ToDouble(stringIq[i * 2 + 1]);
                }

                rfsgSession.WriteArbWaveform("EvDo", 524288, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptEvdo);

                string scriptNameEvdo = "ScriptEvdo";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameEvdo);

                srCalEquipSource.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];

                #endregion 1xEV-DO

                #region TD-SCDMA

                //////////////////////////
                // TD-SCDMA
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(1280000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 20480000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.7);

                //rfsgSession.SetInt32(niRFSGProperties.DirectDownload, niRFSGConstants.Enable);
                //rfsgSession.SetInt32(niRFSGProperties.ArbOnboardSampleClockMode, niRFSGConstants.HighResolution);
                //rfsgSession.AllocateArbWaveform("EvDo", 524288);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\TD-SCDMA\TS1_1M28\I_TDSCDMA_TS1_1p28MHz.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\TD-SCDMA\TS1_1M28\Q_TDSCDMA_TS1_1p28MHz.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                strTemp = "";
                dataI = new double[102400];
                dataQ = new double[102400];

                for (int i = 0; i < 102400; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("TdscdmaTs1", 102400, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptTdscdmaTs1s1p28MHz);

                scriptNameLte = "TdscdmaTs1s1p28MHz";
                rfsgSession.SetString(niRFSGProperties.SelectedScript, scriptNameLte);

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];



                #endregion TD-SCDMA

                #region LTE-TDD

                //////////////////////////
                // TD-SCDMA
                //////////////////////////

                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                rfsgSession.ConfigureSignalBandwidth(9000000);
                // rfsgSession.ConfigurePowerLevelType(niRFSGConstants.AveragePower);
                rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000);
                actualIqRateLte = rfsgSession.GetDouble(niRFSGProperties.IqRate);

                rfsgSession.SetDouble(niRFSGProperties.ArbWaveformSoftwareScalingFactor, 0.92);

                // IQ data loading
                fWaveIData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_TDD_UL\TDD_QPSK_10M12RB\I_LTETU_QPSK_10M12RB.txt");
                fWaveQData = new FileInfo(@"C:\Avago.ATF.Common\Production\TestPlans\PORTAL\Waveform\LTE_TDD_UL\TDD_QPSK_10M12RB\Q_LTETU_QPSK_10M12RB.txt");
                srWaveIData = new StreamReader(fWaveIData.ToString());
                srWaveQData = new StreamReader(fWaveQData.ToString());

                iqDataPoirt = 153600;

                strTemp = "";
                dataI = new double[iqDataPoirt];
                dataQ = new double[iqDataPoirt];

                for (int i = 0; i < iqDataPoirt; i++)
                {
                    dataI[i] = Convert.ToDouble(srWaveIData.ReadLine());
                    dataQ[i] = Convert.ToDouble(srWaveQData.ReadLine());
                }

                rfsgSession.WriteArbWaveform("LteTdd", iqDataPoirt, dataI, dataQ, false);
                //rfsgSession.WriteScript(scriptLte);
                rfsgSession.WriteScript(scriptLteTdd);

                rfsgSession.SetString(niRFSGProperties.SelectedScript, "LteTddSignal");

                srWaveIData.Close();
                srWaveQData.Close();
                strTemp = "";
                dataI = new double[1];
                dataQ = new double[1];

                #endregion LTE-TDD
                
                rfsgSession.Initiate();

                //double tempIqRate = rfsgSession.GetDouble(niRFSGProperties.IqRate);
                //double tempArbClockRate = rfsgSession.GetDouble(niRFSGProperties.ArbSampleClockRate);
                // rfsgSession.GetDouble(niRFSGProperties.
                // rfsgSession.SetDouble(niRFSGProperties.PowerLevel, powerLevel);
                //rfsgSession.Abort();
                // Waveform to CW
                // rfsgSession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger1");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), this.ToString());

                return -1;
            }
        }

        public int InitializeCw()
        {
            try
            {                
                rfsgSession = new niRFSG("VSG", false, true);
                rfsgSession.ConfigureRF(1000000000, -100);
                rfsgSession.ConfigureGenerationMode(niRFSGConstants.Cw);
                rfsgSession.Initiate();
                
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int InitializeWcdmaGtc1New()
        {
            try
            {
                // Set Generation Session to UL Mode 
                wcdmaSession.SetDuplexMode(null, 1);
                wcdmaSession.SetUlFrameType(null, 1);

                // Set ScrambleCodeType 
                wcdmaSession.SetUlScramblingCodeType(null, 0);

                //Set ScramblingCode 
                wcdmaSession.SetUlScramblingCode(null, 0);

                // Set Channel Properties 
                wcdmaSession.SetUlPhysicalChannelType("pch0", 0);
                wcdmaSession.SetUlChannelSpreadingCode("pch0", 0);
                wcdmaSession.SetUlChannelBranch("pch0", niWCDMASGConstants.UlBranchQ);
                wcdmaSession.SetUlChannelRelativePower("pch0", -5.46);
                wcdmaSession.SetUlDpcchSlotFormat("pch0", 0);
                wcdmaSession.SetUlPhysicalChannelType("pch1", niWCDMASGConstants.UlPhysicalChannelTypeDpdch);
                wcdmaSession.SetUlChannelSpreadingCode("pch1", 16);
                wcdmaSession.SetUlChannelBranch("pch1", 0);
                wcdmaSession.SetUlChannelRelativePower("pch1", 0);
                wcdmaSession.SetUlDpcchSlotFormat("pch1", 0);

                // Set Number of Channels based on DPDCH Enabled 
                wcdmaSession.SetNumberOfPhysicalUlChannels(null, 2);

                // Set Headroom Properties 
                wcdmaSession.SetAutoHeadroomEnabled(null, 1);
                wcdmaSession.SetHeadroom(null, 0);

                // Set Impairments 
                wcdmaSession.SetCarrierFrequencyOffset(null, 0);
                wcdmaSession.SetQuadratureSkew(null, 0);
                wcdmaSession.SetIdcOffset(null, 0);
                wcdmaSession.SetQdcOffset(null, 0);
                wcdmaSession.SetIqGainImbalance(null, 0);
                wcdmaSession.SetAwgnEnabled(null, 0);
                wcdmaSession.SetCarrierToNoiseRatio(null, 50);

                // Configure the RFSG 
                //rfsgSession.ConfigureRefClock(clockSource, 10e+6);
                //rfsgSession.ExportSignal(niRFSGConstants.RefClock, string.Empty, exportTerminal);
                //rfsgSession.SetDouble(niRFSGProperties.Frequency, null, carrierFrequency);
                //rfsgSession.ConfigurePowerLevelType(niRFSGConstants.PeakPower);
                //rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                //rfsgSession.SetDouble(niRFSGProperties.ExternalGain, null, -externalAttenuation);

                // If the hardware has LO leakage, you can set upConverterCenterFrequencyOffset to non zero value to avoid the leakage in the band 
                //upConverterCenterFrequencyOffset = 0;
                //upConverterCenterFrequency = carrierFrequency + upConverterCenterFrequencyOffset;
                //rfsgSession.SetDouble(niRFSGProperties.UpconverterCenterFrequency, null, upConverterCenterFrequency);

                // Create and download waveform 
                string waveformNameGtc1New = "Gtc1New";
                wcdmaSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, waveformNameGtc1New);

                // Initiate Generation 
                wcdmaSession.RFSGConfigureScript(rfsgSession.Handle, null, scriptWcdmaGtc1New, -100);
                
//                string scriptWcdmaGtc1New = @"script GenerateGtc1New
//repeat forever
// generate Gtc1New marker0 (0)
//end repeat
//end script";


                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int InitializeWcdmaGtc3()
        {
            try
            {

                // Initialize member

                // Set Generation Session to UL Mode 
                wcdmaSession.SetDuplexMode(null, niWCDMASGConstants.DuplexModeUlOnlyFdd);
                wcdmaSession.SetUlFrameType(null, niWCDMASGConstants.UlFrameTypeNonPrach);

                // Set ScrambleCodeType 
                // scrambleCodeType = 0;
                wcdmaSession.SetUlScramblingCodeType(null, 0);

                //Set ScramblingCode 
                // scramblingCode = 0;
                wcdmaSession.SetUlScramblingCode(null, 0);

                //Set Number of Channels
                wcdmaSession.SetNumberOfPhysicalUlChannels(null, 5);

                // Set Channel Properties for DPCCH
                ConfigureUL_DPCCH();

                // Set Channel Properties for DPDCH
                ConfigureUL_DPDCH();

                // Set Channel Properties for HSDPCCH
                ConfigureUL_HSPDCCH();

                // Set Channel Properties for EDPCCH
                ConfigureUL_EDPCCH();

                // Set Channel Properties for EDPDCH
                ConfigureUL_EDPDCH();

                // Set Headroom Properties 
                // autoHeadroomEnabled = 1;
                wcdmaSession.SetAutoHeadroomEnabled(null, 1);
                // headroom = 0.0;
                wcdmaSession.SetHeadroom(null, 0);

                // Set Impairments 
                // carrierFrequencyOffset = 0.0;
                wcdmaSession.SetCarrierFrequencyOffset(null, 0);
                // quadratureSkew = 0.0;
                wcdmaSession.SetQuadratureSkew(null, 0);
                // IDCOffset = 0.0;
                wcdmaSession.SetIdcOffset(null, 0);
                // QDCOffset = 0.0;
                wcdmaSession.SetQdcOffset(null, 0);
                // IQGainImbalance = 0.0;
                wcdmaSession.SetIqGainImbalance(null, 0);
                // AWGNEnabled = 0;
                wcdmaSession.SetAwgnEnabled(null, 0);
                // CNR = 50.0;
                wcdmaSession.SetCarrierToNoiseRatio(null, 50);

                // Create and download waveform 
                // waveformName = "GTC3";
                wcdmaSession.RFSGCreateAndDownloadWaveform(rfsgSession.Handle, "GTC3");

                // Initiate Generation                 
                wcdmaSession.RFSGConfigureScript(rfsgSession.Handle, null, scriptWcdma, -100);
                // rfsgSession.Abort();
                // rfsgSession.ConfigureGenerationMode(niRFSGConstants.ArbWaveform);
                // rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
                //rfsgSession.SetDouble(niRFSGProperties.IqRate, 92159999.999999657);
                //rfsgSession.SetDouble(niRFSGProperties.ArbSampleClockRate, 368639999.99999863);
                //rfsgSession.SetDouble(niRFSGProperties.SignalBandwidth, 100);
                // rfsgSession.Initiate();

                //double tempIqRate = rfsgSession.GetDouble(niRFSGProperties.IqRate); // 92159999.999999657
                //double tempArbClockRate = rfsgSession.GetDouble(niRFSGProperties.ArbSampleClockRate); // 368639999.99999863
                //double tempSignalBandwidth = rfsgSession.GetDouble(niRFSGProperties.SignalBandwidth); // 100.0

                
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int MeasTempArb(ref double equipTemp, bool enableCommit)
        {
            try
            {
                if (enableCommit)
                    rfsgSession.Commit();
                equipTemp = rfsgSession.GetDouble(niRFSGProperties.ArbTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempArb");
                return 0;
            }
        }

        public int MeasTempLo(ref double equipTemp, bool enableCommit)
        {
            try
            {
                if (enableCommit)
                    rfsgSession.Commit();
                equipTemp = rfsgSession.GetDouble(niRFSGProperties.LoTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempLo");
                return 0;
            }
        }

        public int MeasTempUpConv(ref double equipTemp, bool enableCommit)
        {
            try
            {
                if (enableCommit)
                    rfsgSession.Commit();

                equipTemp = rfsgSession.GetDouble(niRFSGProperties.UpconverterTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempUpConv");
                return 0;
            }
        }


        /// <summary>
        /// Outpu enable function
        /// </summary>
        /// <returns></returns>
        public int OutputEnable(bool condition)
        {
            return 0;
        }

        public int PoweLevel(float setPowerLevel, string modulation, string Waveform)
        {
            try
            {
                double corrFator = 0;

                // CloseGsmEdge();
                if (modulation == "WCDMA")
                {
                    if (Waveform == "GTC1")
                        corrFator = 3.41219;
                    else
                        MessageBox.Show("No waveform information is available.");

                    // wcdmaSession.RFSGConfigurePowerLevel(rfsgSession.Handle, null, scriptWcdmaGtc1New, setPowerLevel-0.99);
                    // wcdmaSession.RFSGConfigurePowerLevel(rfsgSession.Handle, null, scriptWcdmaGtc3, setPowerLevel);                    
                    // rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + 9.4849245776178286);
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + corrFator);
                }
                else if (modulation == "LTE")
                {
                    //if (Waveform == "10M12RB")
                        corrFator = 7.6049245776178286;
                    //else
                    //    MessageBox.Show("No waveform information is available.");

                    //double iqRate = 0;
                    //lteSession.RFSGRetrieveIQRateAllWaveforms(
                    //        rfsgSession.Handle,
                    //        null,
                    //        scriptLte,
                    //        out iqRate);
                    // lteSession.RFSGConfigurePowerLevel(rfsgSession.Handle, null, scriptLte, setPowerLevel);

                    // rfsgSession.Abort();                    
                    // rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + corrFator);
                    // rfsgSession.Initiate();

                }
                else if (modulation == "LTETDD")
                {
                    if (Waveform == "10M12RB")
                        corrFator = 8.04058;
                    else
                        MessageBox.Show("No waveform information is available.");

                    //double iqRate = 0;
                    //lteSession.RFSGRetrieveIQRateAllWaveforms(
                    //        rfsgSession.Handle,
                    //        null,
                    //        scriptLte,
                    //        out iqRate);
                    // lteSession.RFSGConfigurePowerLevel(rfsgSession.Handle, null, scriptLte, setPowerLevel);

                    // rfsgSession.Abort();                    
                    // rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    // rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel);
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + corrFator);
                    // rfsgSession.Initiate();

                }
                else if (modulation == "TDSCDMA")
                {
                    if (Waveform == "TDSCDMA_TS1_1P28MHZ")
                        corrFator = 5.406;
                    else
                        MessageBox.Show("No waveform information is available.");

                    //double iqRate = 0;
                    //lteSession.RFSGRetrieveIQRateAllWaveforms(
                    //        rfsgSession.Handle,
                    //        null,
                    //        scriptLte,
                    //        out iqRate);
                    // lteSession.RFSGConfigurePowerLevel(rfsgSession.Handle, null, scriptLte, setPowerLevel);

                    // rfsgSession.Abort();                    
                    // rfsgSession.SetDouble(niRFSGProperties.IqRate, 15360000.0);
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + corrFator);
                    // rfsgSession.Initiate();

                }
                else if (modulation == "1XEVDO")
                {
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, setPowerLevel + 10.03);
                }
                else if (modulation == "GSM")
                {
                    Thread.Sleep(1);
                }
                else if (modulation == "GSM")
                {
                    //Stopwatch myStopWatch = new Stopwatch();
                    //myStopWatch.Reset();
                    //myStopWatch.Start();

                    //for (int i = 0; i < 1000; i++)
                    //{
                    gsmsgSession.RFSGConfigurePowerLevel(rfsgSession.Handle, setPowerLevel, GsmScript);
                    //}

                    //myStopWatch.Stop();
                    //long testTime = myStopWatch.ElapsedMilliseconds;
                }
                else if (modulation == "EDGE")
                {
                    //Stopwatch myStopWatch = new Stopwatch();
                    //myStopWatch.Reset();
                    //myStopWatch.Start();

                    //for (int i = 0; i < 1000; i++)
                    //{
                    // edgeSgSession.RFSGConfigurePowerLevel(rfsgSession.Handle, setPowerLevel, EdgeScript);

                    rfsgSession.Abort();
                    edgeSgSession.RFSGConfigureScript(rfsgSession.Handle, EdgeScript, setPowerLevel);
                    rfsgSession.Initiate();
                    //}

                    //myStopWatch.Stop();
                    //long testTime = myStopWatch.ElapsedMilliseconds;
                }
                else if (modulation == "CW")
                {
                    rfsgSession.SetDouble(niRFSGProperties.PowerLevel, (double)setPowerLevel);
                }
                
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "PowerLevel");
                return -1;
            }
        }

        private void ConfigureRFSGSession(double carrierFrequency)
        {
            rfsgSession.Abort();
            rfsgSession.ConfigureGenerationMode(niRFSGConstants.Script);
            rfsgSession.ConfigurePowerLevelType(niRFSGConstants.PeakPower);
            rfsgSession.SetDouble(niRFSGProperties.Frequency, carrierFrequency);
        }

        public int PerformThermalCorrection()
        {
            int result = 0;
            try
            {
                // rfsaSession.Initiate();
                result = rfsgSession.PerformThermalCorrection();
                // rfsaSession.Abort();
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "PerformThermalCorrection");
                return result;
            }
        }

        public int SelfCalibrate()
        {
            int result = 0;

            try
            {
                if (rfsgSession != null)
                    rfsgSession.Dispose();

                rfsgSession = new niRFSG("VSG", false, true);

                result = rfsgSession.SelfCal();
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SelfCalibrate");
                return result;                
            }
        }

        private void ConfigureUL_DPCCH()
        {
            wcdmaSession.SetUlPhysicalChannelType("pch0", niWCDMASGConstants.UlPhysicalChannelTypeDpcch);
            wcdmaSession.SetUlChannelSpreadingCode("pch0", 0);
            wcdmaSession.SetUlChannelBranch("pch0", niWCDMASGConstants.UlBranchQ);
            wcdmaSession.SetUlChannelRelativePower("pch0", -19.39);
            //According to TS 25.211 Slot format #0 and SF=256 corresponds to DPCCH@15ksps  
            wcdmaSession.SetUlDpcchSlotFormat("pch0", niWCDMASGConstants.UlDpcchSlotFormat0Sf256);
        }
        private void ConfigureUL_DPDCH()
        {
            wcdmaSession.SetUlPhysicalChannelType("pch1", niWCDMASGConstants.UlPhysicalChannelTypeDpdch);
            wcdmaSession.SetUlChannelSpreadingCode("pch1", 1);
            wcdmaSession.SetUlChannelBranch("pch1", niWCDMASGConstants.UlBranchI);
            wcdmaSession.SetUlChannelRelativePower("pch1", -13.93);
            //According to TS 25.211 slot format #6 and SF=4 corresponds to DPDCH@960ksps
            wcdmaSession.SetUlDpdchSlotFormat("pch1", niWCDMASGConstants.UlDpdchSlotFormat6Sf4);
        }

        private void ConfigureUL_HSPDCCH()
        {
            wcdmaSession.SetUlPhysicalChannelType("pch2", niWCDMASGConstants.UlPhysicalChannelTypeHsdpcch);
            wcdmaSession.SetUlChannelSpreadingCode("pch2", 64);
            wcdmaSession.SetUlChannelBranch("pch2", niWCDMASGConstants.UlBranchQ);
            wcdmaSession.SetUlChannelRelativePower("pch2", -19.39);
            wcdmaSession.SetUlChannelDataType("pch2", niWCDMASGConstants.DataTypeUserDefinedBits);
            //int[] ACK = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            //int[] CQI = { 0,1,1,0,1 };//CQI = 13
            //User defined bitPatteren = ACK + CQI
            int[] UserDefinedBitPattern = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1 };
            wcdmaSession.SetUlChannelUserDefinedBits("pch2", UserDefinedBitPattern);
        }

        private void ConfigureUL_EDPCCH()
        {
            wcdmaSession.SetUlPhysicalChannelType("pch3", niWCDMASGConstants.UlPhysicalChannelTypeEdpcch);
            wcdmaSession.SetUlChannelSpreadingCode("pch3", 1);
            wcdmaSession.SetUlChannelBranch("pch3", niWCDMASGConstants.UlBranchI);
            wcdmaSession.SetUlChannelRelativePower("pch3", -17.34);
        }

        private void ConfigureUL_EDPDCH()
        {
            wcdmaSession.SetUlPhysicalChannelType("pch4", niWCDMASGConstants.UlPhysicalChannelTypeEdpdch);
            wcdmaSession.SetUlChannelSpreadingCode("pch4", 2);
            wcdmaSession.SetUlChannelBranch("pch4", niWCDMASGConstants.UlBranchI);
            wcdmaSession.SetUlChannelRelativePower("pch4", -0.37);
            //According to TS 25.211 slot format #6 and SF=4 corresponds to EDPDCH@960ksps
            wcdmaSession.SetUlEdpdchSlotFormat("pch4", niWCDMASGConstants.EdpdchSlotFormat6Sf4);
        }
    }
}
