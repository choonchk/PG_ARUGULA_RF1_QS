using MPAD_TestTimer;
using System;
using System.Collections.Generic;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using SParamTestCommon;
using System.IO;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// TOPAZ equipment initializer.
    /// </summary>
    public class TopazEquipmentInitializer
    {
        /// <summary>
        /// Store measured data.
        /// </summary>
        public SParameterMeasurementDataModel DataModel { get; set;}
        /// <summary>
        /// Communicate with NA equipment.
        /// </summary>
        public TopazEquipmentDriver ENAEquipmentDriver { get; set; }

        public string NA_StateFile { get; set; }

        public void SetTraceData(TcfSheetTrace sheetTrace, TcfSheetSegmentTable sheetSegmentTable,
            Tuple<bool, string, string[,]> sheetTrace2) //seoul
        {
            DataModel = new SParameterMeasurementDataModel();
            DataModel.DefineTraceData(sheetTrace, sheetSegmentTable, sheetTrace2);
        }

        public void InitEquipment(string address)
        {
            ENAEquipmentDriver = new TopazEquipmentDriver();
            ENAEquipmentDriver.InitEquipment(address);
        }

        public void Initialize(bool isDivaInstrument, bool[] cprojectPortEnable, int CalColmnIndexNFset)
        {
            Initialize(NA_StateFile, isDivaInstrument, cprojectPortEnable, CalColmnIndexNFset);
        }

        /// <summary>
        /// Used for quick troubleshooting run.
        /// </summary>
        public void InitializeFast(bool isDivaInstrument, bool[] cprojectPortEnable, int CalColmnIndexNFset)
        {
            InitializeFast(NA_StateFile, isDivaInstrument, cprojectPortEnable, CalColmnIndexNFset);
        }

        public void Initialize(string stateFilePath, bool isDivaInstrument,
            bool[] cprojectPortEnable, int CalColmnIndexNFset)
        {
            // Load state file is a long operation.
            ENAEquipmentDriver.Load_StateFile(stateFilePath);

            DataModel.Init_Channel();
            // Load TCF - Segment table.
            DataModel.Init_SegmentParam(cprojectPortEnable, CalColmnIndexNFset, isDivaInstrument);

            /////////////////////////////Set new statefile
            //Read_TraceMatching2(cprojectPortEnable);
            ///////////////////////////////

            string[] ListFbar_channel = ENAEquipmentDriver.GetChannelList();

            //For DIVA
            if (isDivaInstrument)
            {
                ENAEquipmentDriver.Init_DIVA("6");
            }
            else
            {
                ENAEquipmentDriver.SetAvgFunction(false, ListFbar_channel.Length);
            }

            //SetTraceMatching();
            //Verify_SegmentParam();
            //Init_PortMatching(); //seoul

            ENAEquipmentDriver.Initialize(DataModel.SParamData);
            int totalChannel = DataModel.SParamData.Length;
            ENAEquipmentDriver.InitializeFrequencyList(ListFbar_channel, totalChannel);

            try
            {
                ENAEquipmentDriver.SetMemoryMap(ListFbar_channel, DataModel.SParamData, totalChannel);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Expected error if Read_TraceMatching2() is called.");
                LoggingManager.Instance.LogInfo(msg);
            }

            //SetTrigger(e_TriggerSource.BUS);
            if (!isDivaInstrument)
            {
                ENAEquipmentDriver.Sweep_Speed(e_Format.FAST, ListFbar_channel, totalChannel);
            }

            ENAEquipmentDriver.SetDCUSB(ListFbar_channel, totalChannel);
            //GroupNFDualBand();// NF DualBand Grouping For NF DualBandTest
            ENAEquipmentDriver.NFCH_Freq(ListFbar_channel, totalChannel); // NF Freq read from State file

            //DwellTime_Auto(false);
            ENAEquipmentDriver.SetTrigger(totalChannel);
        }

        private void InitializeFast(string stateFilePath, bool isDivaInstrument,
            bool[] cprojectPortEnable, int CalColmnIndexNFset)
        {
            DataModel.Init_Channel();
            // Load TCF - Segment table.
            DataModel.Init_SegmentParam(cprojectPortEnable, CalColmnIndexNFset);

            //For DIVA
            if (isDivaInstrument)
            {
                ENAEquipmentDriver.Init_DIVA("6");
            }

            string[] ListFbar_channel = ENAEquipmentDriver.GetChannelList();

            ENAEquipmentDriver.Initialize(DataModel.SParamData);
            int totalChannel = DataModel.SParamData.Length;
            ENAEquipmentDriver.InitializeFrequencyList(ListFbar_channel, totalChannel);
            ENAEquipmentDriver.SetMemoryMap(ListFbar_channel, DataModel.SParamData, totalChannel);
            if (!isDivaInstrument)
            {
                ENAEquipmentDriver.Sweep_Speed(e_Format.FAST, ListFbar_channel, totalChannel);
            }
            ENAEquipmentDriver.NFCH_Freq(ListFbar_channel, totalChannel); // NF Freq read from State file
        }

        public double Temp_Topaz()
        {
            return ENAEquipmentDriver.Temp_Topaz();
        }

        //ChoonChin - For Topaz temperature read back
        public string ReadTopazTemp(int Module)
        {
            return ENAEquipmentDriver.ReadTopazTemp(Module);
        }

        public bool GetVNAEquipmentType()
        {
            //string meup = 
            bool isDivaInstrument = false;
            string ConfigFilePath = @"C:\Users\Public\Documents\Network Analyzer\VnaConfig.txt";
            try
            {
                string ID_me = ENAEquipmentDriver.Identify_NA_Type();
                ID_me = ID_me.Substring(23, 6);
                if (ID_me.Contains("98")) isDivaInstrument = true;

                //if (File.Exists(ConfigFilePath))
                //{
                //    //Read
                //    StreamReader dSR = new StreamReader(ConfigFilePath);
                //    string RS = dSR.ReadLine();
                //    string[] aRS = RS.Split('=');

                //    if (aRS[1].ToUpper().Contains("TRUE"))
                //    {
                //        isDivaInstrument = true;
                //    }
                //    else
                //        isDivaInstrument = false;

                //    dSR.Close();
                //}
                //else
                //{
                //    //Not DIVA
                //    isDivaInstrument = false;
                //}
            }
            catch
            {
                //Not DIVA
                isDivaInstrument = false;
            }
            return isDivaInstrument;
        }

        /// <summary>
        /// Set new state file.
        /// </summary>
        private void Read_TraceMatching2(bool[] cprojectPortEnable)
        {
            List<StateFileDataObject> sfList = DataModel.InitChannelAndGetStateFileGenerationInput();
            ENAEquipmentDriver.SetNewStateFile(sfList, DataModel.SParamData.Length,
                cprojectPortEnable);
        }
    }
}