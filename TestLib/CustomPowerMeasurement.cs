using System;
using System.Windows.Forms;
using EqLib;
using ClothoLibAlgo;
using System.Threading;



namespace TestLib
{
    public class CustomPowerMeasurement
    {
        public static Dictionary.Ordered<string, CustomPowerMeasurement> Mem = new Dictionary.Ordered<string, CustomPowerMeasurement>();
        public readonly string name;
        public readonly string band;
        public readonly bool isBandSpecific;
        public readonly double expectedLevelDbc;
        public Operation measurePath;
        public readonly Units logUnits;
        public MeasureWith measurementInstrument;
        public Calc.ChannelBandwidth channelBandwidth;

        public static void Define(string TestName, string Band, Operation MeasurePath, double ExpectedLevelDbc, Units LogUnits, MeasureWith MeasurementInstrument)
        {
            CustomPowerMeasurement.Mem.Add(TestName, new CustomPowerMeasurement(TestName, Band, true, MeasurePath, ExpectedLevelDbc, LogUnits, MeasurementInstrument));
        }

        public static void Define(string TestName, Operation MeasurePath, double ExpectedLevelDbc, Units LogUnits, MeasureWith MeasurementInstrument)
        {
            CustomPowerMeasurement.Mem.Add(TestName, new CustomPowerMeasurement(TestName, "", false, MeasurePath, ExpectedLevelDbc, LogUnits, MeasurementInstrument));
        }

        private CustomPowerMeasurement(string Name, string Band, bool IsBandSpecific, Operation MeasurePath, double ExpectedLevelDbc, Units LogUnits, MeasureWith MeasurementInstrument)
        {
            this.name = Name;
            this.band = Band;
            this.isBandSpecific = IsBandSpecific;
            this.measurePath = MeasurePath;
            this.expectedLevelDbc = ExpectedLevelDbc;
            this.logUnits = LogUnits;
            this.measurementInstrument = MeasurementInstrument;
            if (MeasurePath == Operation.MeasureH2_ANT_UAT || MeasurePath == Operation.MeasureH2_ANT2)
                channelBandwidth = Calc.ChannelBandwidth.H2;

            else if (MeasurePath == Operation.MeasureH3_ANT_UAT || MeasurePath == Operation.MeasureH3_ANT2 || MeasurePath == Operation.MeasureH3_PS || MeasurePath == Operation.MeasureH3_VSA)
                channelBandwidth = Calc.ChannelBandwidth.H3;

            else
                channelBandwidth = Calc.ChannelBandwidth.Fundamental;
        }

        public void Execute(RfTestBase test)
        {
            try
            {
                string bandToUse = test.TestCon.Band;

                // For Quadsite, single Controller dual sites 	
                string BandTemp = test.TestCon.Band;
                byte SiteTemp = test.Site;
                if (test.Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + test.Site.ToString();
                    SiteTemp = 0;
                }

                measurePath = test.TestCon.customVsaOperation;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.Band, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                //if(measurePath != Operation.MeasureCpl)//removed By hosein
                //Eq.Site[test.Site].SwMatrix.ActivatePath("",Operation.ANTtoTERM);  // removed By hosein
                if (measurePath != Operation.MeasureCpl && measurePath != Operation.MeasureH2_ANT1 && measurePath != Operation.MeasureH2_ANT2)  // added by hosein
                {
                    if (test.Site.Equals(0) == false)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath("_" + test.Site.ToString(), Operation.ANTtoTERM);
                    }
                    else
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath("", Operation.ANTtoTERM);
                    }
                }
                

                Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, measurePath);
                if (test.TestCon.FreqSG * (double)channelBandwidth < Eq.Site[test.Site].RF.MaxFreq) measurementInstrument = MeasureWith.VSA;
                else measurementInstrument = MeasureWith.PowerSensor;
                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:
                        double expectedLevelDbc_safetyClipped = test.TestCon.expectedLevelDbc;

                        if (channelBandwidth != Calc.ChannelBandwidth.Fundamental)
                        {
                            float calfactor_harmonic = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                            float calfactor_fundamentalPath = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG);
                            float fundamentalAttenuation = calfactor_harmonic - calfactor_fundamentalPath;

                            expectedLevelDbc_safetyClipped = Math.Max(expectedLevelDbc, -fundamentalAttenuation);
                        }

                        double poutEstimate = test.TestCon.TargetPout.HasValue ?
                            test.TestCon.TargetPout.Value :
                            test.TestCon.TargetPin.Value + test.TestCon.ExpectedGain;

                        double saRefLevel = poutEstimate + expectedLevelDbc_safetyClipped;

                        Thread.Sleep(1);

                       // if(EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)   //removed by hosein
                        //{
                         //   Eq.Site[test.Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxH2.GetSpecIteration(), -30));
                        
                        //    EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[test.Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxH2.GetSpecIteration()));

                         //   test.TestResult.CustomTestDbm["H2"] = _RFmxResult.averageChannelPower;
                       // }
                
                       // else
                       // {
                            Eq.Site[test.Site].RF.SA.CenterFrequency = test.TestCon.FreqSG * 1e6 * (double)channelBandwidth;
                            Eq.Site[test.Site].RF.SA.ExternalGain = outputPathGain;
                            Eq.Site[test.Site].RF.SA.ReferenceLevel = saRefLevel;

                            test.TestResult.IQdataCustom.Add(this.name + "_" + test.TestCon.TestParaName, Eq.Site[test.Site].RF.SA.MeasureIqTrace(false));
                            
                        // }


                        //double measdBm = Eq.Site[test.Site].RF.SA.MeasureChanPower();   // for debugging, normally power gets calculated in CalcResults   //
                        //test.TestResult.CustomTestDbm[this.name + "_" + test.TestCon.TestParaName] = measdBm;


                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name] = Eq.Site[test.Site].PM.Measure() - outputPathGain;

                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.Execute");
            }
        }  // original one kept but overrided by hosein
        public void Execute(RfTestBase test, string strCustom, byte site)
        {
            try
            {
                string bandToUse = test.TestCon.Band;
                // For Quadsite, single Controller dual sites 	
                string BandTemp = test.TestCon.Band;
                byte SiteTemp = test.Site;
                if (test.Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + test.Site.ToString();
                    SiteTemp = 0;
                }

                measurePath = test.TestCon.customVsaOperation;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.Band, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);

                if (measurePath != Operation.MeasureCpl && measurePath != Operation.MeasureH2_ANT1 && measurePath != Operation.MeasureH2_ANT2)
                {
                    if (test.Site.Equals(0) == false)
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath("_" + test.Site.ToString(), Operation.ANTtoTERM);
                    }
                    else
                    {
                        Eq.Site[SiteTemp].SwMatrix.ActivatePath("", Operation.ANTtoTERM);
                    }
                }
                Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, measurePath);
                if (test.TestCon.FreqSG * (double)channelBandwidth < Eq.Site[test.Site].RF.MaxFreq) measurementInstrument = MeasureWith.VSA;
                else measurementInstrument = MeasureWith.PowerSensor;
                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:
                        double expectedLevelDbc_safetyClipped = test.TestCon.expectedLevelDbc;

                        if (channelBandwidth != Calc.ChannelBandwidth.Fundamental)
                        {
                            float calfactor_harmonic = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                            float calfactor_fundamentalPath = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG);
                            float fundamentalAttenuation = calfactor_harmonic - calfactor_fundamentalPath;

                            expectedLevelDbc_safetyClipped = Math.Min(expectedLevelDbc_safetyClipped, -fundamentalAttenuation);
                        }

                        double poutEstimate = test.TestCon.TargetPout.HasValue ?
                            test.TestCon.TargetPout.Value :
                            test.TestCon.TargetPin.Value + test.TestCon.ExpectedGain;

                        double saRefLevel = poutEstimate + expectedLevelDbc_safetyClipped;
                        if (test.TestCon.ModulationStd.Equals("LTE") && (measurePath.Equals(Operation.MeasureH2_ANT2) || measurePath.Equals(Operation.MeasureH2_ANT1)))
                            Eq.Site[test.Site].RF.SetActiveWaveform("", "CW", false, false);// set smaller RBW at VSA to have lower noise floor
                        double H2freq = test.TestCon.FreqSG - 4.4;
                        //if (test.TestCon.ModulationStd.Equals("LTE") && test.TestCon.WaveformName.Equals("10M1RB") && (measurePath.Equals(Operation.MeasureH2_ANT2) || measurePath.Equals(Operation.MeasureH2_ANT1)))
                        //    Eq.Site[test.Site].RF.SA.CenterFrequency = H2freq * 1e6 * (double)channelBandwidth;
                       // else
                        //    Eq.Site[test.Site].RF.SA.CenterFrequency = test.TestCon.LeakageFreq * 1e6 * (double)channelBandwidth;
                        Eq.Site[test.Site].RF.SA.ExternalGain = outputPathGain;
                        Eq.Site[test.Site].RF.SA.ReferenceLevel = saRefLevel;

                        //double measdBm = 0;   // for better correlation with Rnd
                        //if (test.TestCon.LeakageFreq != test.TestCon.FreqSG)
                        //    measdBm = Eq.Site[test.Site].RF.SA.MeasureChanPower(1);   // for better correlation with Rnd
                        // else
                        // {
                        //measdBm = Eq.Site[test.Site].RF.SA.MeasureChanPower(false);
                       
                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name] = Eq.Site[test.Site].PM.Measure() - outputPathGain;

                        break;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.Execute");
            }
        }  // copied from Hallasan by Hosein

        public void Execute(RfTestBase test,string strCustom)
        {
            try
            {
                //Eq.Site[test.Site].RF.SG.SetLofreq();
                int multiplier = 1;

                if (strCustom == "H2") { multiplier = 2; }
                else if (strCustom == "H3") { multiplier = 3; }
                else { multiplier = 1; }

                string bandToUse = test.TestCon.Band;
                // For Quadsite, single Controller dual sites 	
                string BandTemp = test.TestCon.Band;
                byte SiteTemp = test.Site;
                if (test.Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + test.Site.ToString();
                    SiteTemp = 0;
                }

                measurePath = test.TestCon.customVsaOperation;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.Band, measurePath, test.TestCon.FreqSG * multiplier);
                if (measurePath != Operation.MeasureH2)
                    //  Eq.Site[test.Site].SwMatrix.ActivatePath("",Operation.ANTtoTERM);
                Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, measurePath);

                // Case NightHawk - measure with VSA.
                //if (test.TestCon.FreqSG * (double)channelBandwidth < Eq.Site[test.Site].RF.MaxFreq) measurementInstrument = MeasureWith.VSA;
                //else measurementInstrument = MeasureWith.PowerSensor;
                measurementInstrument = MeasureWith.VSA;

                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:
                        double expectedLevelDbc_safetyClipped = expectedLevelDbc;

                        if (channelBandwidth != Calc.ChannelBandwidth.Fundamental)
                        {
                            float calfactor_harmonic = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG * multiplier);
                            float calfactor_fundamentalPath = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG);
                            float fundamentalAttenuation = calfactor_harmonic - calfactor_fundamentalPath;

                            expectedLevelDbc_safetyClipped = Math.Max(expectedLevelDbc, -fundamentalAttenuation);
                        }

                        double poutEstimate = test.TestCon.TargetPout.HasValue ?
                            test.TestCon.TargetPout.Value :
                            test.TestCon.TargetPin.Value + test.TestCon.ExpectedGain;

                        double saRefLevel = poutEstimate + expectedLevelDbc_safetyClipped;

                        Thread.Sleep(1);
                        double rfExtddRxOutFreq = 0f;
                        

                        if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                        {
                            if(strCustom == "H2")
                            {
                                // Case NightHawk - added as below.
                                if (test.TestCon.FreqSG * 2 > 6000)
                                {
                                    Eq.Site[test.Site].RF.RFExtd.ConfigureRXDownconversion(); //Mario
                                    Eq.Site[test.Site].RF.RFExtd.ConfigureHarmonicConverter((test.TestCon.FreqSG * 1e6) * 2, 1, out rfExtddRxOutFreq);
                                }

                                Thread.Sleep(10); //20200604 Mario added for the stability
                                Eq.Site[test.Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxH2.GetSpecIteration(), -10)); //org -30                                                              

                                EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[test.Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxH2.GetSpecIteration()));

                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = _RFmxResult.averageChannelPower - outputPathGain;

                                // Eq.Site[test.Site].SwMatrix.ActivatePath("MANUAL", Operation.NONE);

                                //if (test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] < -55)

                                Eq.Site[test.Site].RF.RFExtd.ConfigureHarmonicConverter(test.TestCon.FreqSG * 1e6, 1, out rfExtddRxOutFreq);  //Added by Hosein
                                Eq.Site[test.Site].RF.CRfmxH2.SpecIteration();
                            }
                            else if (strCustom == "H3")  //Added by Hosein
                            {
                                if (test.TestCon.FreqSG * 3 > 6000)
                                {
                                    Eq.Site[test.Site].RF.RFExtd.ConfigureRXDownconversion(); //Mario
                                    Eq.Site[test.Site].RF.RFExtd.ConfigureHarmonicConverter((test.TestCon.FreqSG * 1e6) * 3, 1, out rfExtddRxOutFreq);
                                }

                                Thread.Sleep(10); //20200604 Mario added for the stability

                                //Eq.Site[test.Site].HSDIO.SendMipiCommands(test.TestCon.MipiCommands);

                                Eq.Site[test.Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar3rd, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxH3.GetSpecIteration(), -30));

                                EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[test.Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar3rd, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxH3.GetSpecIteration()));

                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = _RFmxResult.averageChannelPower - outputPathGain;

                                Eq.Site[test.Site].RF.RFExtd.ConfigureHarmonicConverter(test.TestCon.FreqSG * 1e6, 1, out rfExtddRxOutFreq);
                                Eq.Site[test.Site].RF.CRfmxH3.SpecIteration();
                            }
                            else
                            {
                                
                                Eq.Site[test.Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxChp, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxCHP.GetSpecIteration(), -30));  //added  by hosein 12/28/2019                            
                                //Thread.Sleep(1);
                                EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[test.Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxChp, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxCHP.GetSpecIteration())); //added  by hosein 12/28/2019
                                //Thread.Sleep(1);
                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = _RFmxResult.averageChannelPower - outputPathGain; //added  by hosein 12/28/2019
                                //Eq.Site[test.Site].RF.RFExtd.ConfigureHarmonicConverter(test.TestCon.FreqSG * 1e6, 1, out rfExtddRxOutFreq); //added  by hosein 12/28/2019
                                Eq.Site[test.Site].RF.CRfmxCHP.SpecIteration();
                            }
                        }
                                              
                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name + strCustom] = Eq.Site[test.Site].PM.Measure() - outputPathGain;

                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.Execute");
            }
        }

        public void Execute(RfTestBase test, string strCustom, int Txleakage_Iteration)
        {
            try
            {                
                string bandToUse = test.TestCon.Band;

                //mario
                //if (test.TestCon.TxleakageCondition[Txleakage_Iteration].SpecNumber.Contains("SPACING") && test.TestCon.FreqSG == 1910)
                //{
                //    test.TestCon.FreqSG = 1922;
                //}
                //if (test.TestCon.TxleakageCondition[Txleakage_Iteration].SpecNumber.Contains("SPACING") && test.TestCon.FreqSG == 1780)
                //{
                //    test.TestCon.FreqSG = 1795;
                //}

                measurePath = test.TestCon.TxleakageCondition[Txleakage_Iteration].ePort;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.TxleakageCondition[Txleakage_Iteration].RxActiveBand, measurePath, test.TestCon.FreqSG);

                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:
                        double expectedLevelDbc_safetyClipped = expectedLevelDbc;

                        if (channelBandwidth != Calc.ChannelBandwidth.Fundamental)
                        {
                            float calfactor_harmonic = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                            float calfactor_fundamentalPath = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG);
                            float fundamentalAttenuation = calfactor_harmonic - calfactor_fundamentalPath;

                            expectedLevelDbc_safetyClipped = Math.Max(expectedLevelDbc, -fundamentalAttenuation);
                        }

                        double poutEstimate = test.TestCon.TargetPout.HasValue ?
                            test.TestCon.TargetPout.Value :
                            test.TestCon.TargetPin.Value + test.TestCon.ExpectedGain;

                        double saRefLevel = poutEstimate + expectedLevelDbc_safetyClipped;


                        if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                        {
                     
                            Eq.Site[test.Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxTxleakage, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxTxleakage.GetSpecIteration(), -30));

                            EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[test.Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxTxleakage, new EqRF.Config(Eq.Site[test.Site].RF.CRfmxTxleakage.GetSpecIteration()));

                            test.TestResult.CustomTestDbm[this.name + strCustom + "_" + test.TestCon.TestParaName + test.TestCon.TxleakageCondition[Txleakage_Iteration].SpecNumber] = _RFmxResult.averageChannelPower - outputPathGain; //Txleakage TxRx spacing measurement by Mario 
                                
                            
                            if (test.TestResult.CustomTestDbm[this.name + strCustom + "_" + test.TestCon.TestParaName + test.TestCon.TxleakageCondition[Txleakage_Iteration].SpecNumber] < -60)
                            {

                            }
                        }

                        else
                        {
                            Eq.Site[test.Site].RF.SA.CenterFrequency = test.TestCon.FreqSG * 1e6 * (double)channelBandwidth;
                            Eq.Site[test.Site].RF.SA.ExternalGain = outputPathGain;
                            Eq.Site[test.Site].RF.SA.ReferenceLevel = saRefLevel;

                            test.TestResult.IQdataCustom.Add(this.name + strCustom + "_" + test.TestCon.TestParaName, Eq.Site[test.Site].RF.SA.MeasureIqTrace(false));

                        }


                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name + strCustom] = Eq.Site[test.Site].PM.Measure() - outputPathGain;

                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.Execute");
            }
        }

        public static void ExecuteAll(RfTestBase test)
        {
            Eq.Site[test.Site].RF.SG.SetLofreq(-40 * 1e6);

            foreach (string testName in CustomPowerMeasurement.Mem.Keys)
            {
                if (test.TestCon.TestCustom[testName])
                {
                    if (!testName.Contains("TxLeakage"))
                    {
                        CustomPowerMeasurement.Mem[testName].Execute(test, testName);                         
                    }
                    else
                    { 
                        int Count = 0;
                      
                        foreach (RfTestCondition.cTxleakageCondition currTxleakage in test.TestCon.TxleakageCondition)
                        {
                            // For Quadsite, single Controller dual sites 	
                            string BandTemp = currTxleakage.RxActiveBand;
                            byte SiteTemp = test.Site;
                            if (test.Site.Equals(0) == false)
                            {
                                BandTemp = BandTemp + "_" + test.Site.ToString();
                                SiteTemp = 0;
                            }

                            Eq.Site[test.Site].HSDIO.SendMipiCommands(currTxleakage.Mipi);
                            Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, currTxleakage.ePort);

                            CustomPowerMeasurement.Mem[testName].Execute(test, currTxleakage.RxActiveBand + currTxleakage.Port, Count);
                            Count++;

                            Eq.Site[test.Site].RF.CRfmxTxleakage.SpecIteration();

                        }


                    }
                }
            }
        }

        public void ExecuteDummy(RfTestBase test, string strCustom)
        {
            try
            {
                string bandToUse = test.TestCon.Band;
                // For Quadsite, single Controller dual sites 	
                string BandTemp = test.TestCon.Band;
                byte SiteTemp = test.Site;
                if (test.Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + test.Site.ToString();
                    SiteTemp = 0;
                }

                measurePath = test.TestCon.customVsaOperation;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.Band, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                if (measurePath != Operation.MeasureH2)
                    //  Eq.Site[test.Site].SwMatrix.ActivatePath("",Operation.ANTtoTERM);
                Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, measurePath);

                // Case NightHawk - measure with VSA.
                //if (test.TestCon.FreqSG * (double)channelBandwidth < Eq.Site[test.Site].RF.MaxFreq) measurementInstrument = MeasureWith.VSA;
                //else measurementInstrument = MeasureWith.PowerSensor;
                measurementInstrument = MeasureWith.VSA;

                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:
                        double expectedLevelDbc_safetyClipped = expectedLevelDbc;

                        if (channelBandwidth != Calc.ChannelBandwidth.Fundamental)
                        {
                            float calfactor_harmonic = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG * (double)channelBandwidth);
                            float calfactor_fundamentalPath = CableCal.GetCF(test.Site, bandToUse, measurePath, test.TestCon.FreqSG);
                            float fundamentalAttenuation = calfactor_harmonic - calfactor_fundamentalPath;

                            expectedLevelDbc_safetyClipped = Math.Max(expectedLevelDbc, -fundamentalAttenuation);
                        }

                        double poutEstimate = test.TestCon.TargetPout.HasValue ?
                            test.TestCon.TargetPout.Value :
                            test.TestCon.TargetPin.Value + test.TestCon.ExpectedGain;

                        double saRefLevel = poutEstimate + expectedLevelDbc_safetyClipped;

                        if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                        {
                            if (strCustom == "H2")
                            {                     
                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = 999;

                                Eq.Site[test.Site].RF.CRfmxH2.SpecIteration();
                            }
                            else if (strCustom == "H3")  //Added by Hosein
                            {
                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = 999;

                                Eq.Site[test.Site].RF.CRfmxH3.SpecIteration();
                            }
                            else
                            {
                                test.TestResult.CustomTestDbm[strCustom + "_" + test.TestCon.TestParaName] = 999;

                                Eq.Site[test.Site].RF.CRfmxCHP.SpecIteration();
                            }
                        }
                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name + strCustom] = 999;

                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.ExecuteDummy");
            }
        }

        public void ExecuteDummy(RfTestBase test, string strCustom, int Txleakage_Iteration)
        {
            try
            {
                string bandToUse = test.TestCon.Band;

                measurePath = test.TestCon.TxleakageCondition[Txleakage_Iteration].ePort;// get VsaOperation from TCF instead of fixed in TestPlan.
                float outputPathGain = CableCal.GetCF(test.Site, test.TestCon.TxleakageCondition[Txleakage_Iteration].RxActiveBand, measurePath, test.TestCon.FreqSG);

                switch (measurementInstrument)
                {
                    case MeasureWith.VSA:

                        if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                        {
                            //test.TestResult.CustomTestDbm[this.name + strCustom + "_" + test.TestCon.TestParaName] = 999;
                            test.TestResult.CustomTestDbm[this.name + strCustom + "_" + test.TestCon.TestParaName + test.TestCon.TxleakageCondition[Txleakage_Iteration].SpecNumber] = 999;
                        }
                        else
                        {
                            test.TestResult.IQdataCustom.Add(this.name + strCustom + "_" + test.TestCon.TestParaName, new NationalInstruments.ModularInstruments.Interop.niComplexNumber[Eq.Site[test.Site].RF.SA.NumberOfSamples]);
                        }
                        break;

                    case MeasureWith.PowerSensor:
                        test.TestResult.CustomTestDbm[name + strCustom] = 999;
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CustomTest.ExecuteDummy");
            }
        }

        public static void ExecuteAllDummy(RfTestBase test)
        {
            foreach (string testName in CustomPowerMeasurement.Mem.Keys)
            {
                if (test.TestCon.TestCustom[testName])
                {
                    if (!testName.Contains("TxLeakage"))
                    {
                        CustomPowerMeasurement.Mem[testName].ExecuteDummy(test, testName);
                    }
                    else
                    {
                        int Count = 0;
                        foreach (RfTestCondition.cTxleakageCondition currTxleakage in test.TestCon.TxleakageCondition)
                        {
                            // For Quadsite, single Controller dual sites 	
                            string BandTemp = currTxleakage.RxActiveBand;
                            byte SiteTemp = test.Site;
                            if (test.Site.Equals(0) == false)
                            {
                                BandTemp = BandTemp + "_" + test.Site.ToString();
                                SiteTemp = 0;
                            }
                            Eq.Site[test.Site].HSDIO.SendMipiCommands(currTxleakage.Mipi);
                            Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, currTxleakage.ePort);

                            CustomPowerMeasurement.Mem[testName].ExecuteDummy(test, currTxleakage.RxActiveBand + currTxleakage.Port, Count);
                            Count++;

                            Eq.Site[test.Site].RF.CRfmxTxleakage.SpecIteration();
                        }


                    }
                }
            }
        }

        public enum Units
        {
            dBm,
            dBc
        }

        public enum MeasureWith
        {
            VSA,
            PowerSensor
        }
    }
}
