using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.Proxy;

using GuCal;
using TestLib;
using EqLib;

namespace CalLib
{

    public class AutoCal
    {
        public IATFTest myAtfTest;
        public bool ProductionMode;

        public AutoCal(IATFTest myAtfTest, bool productionMode)
        {
            this.myAtfTest = myAtfTest;
            this.ProductionMode = productionMode;
        }

        public enum CalType
        {
            VNA, Cable, GU
        }

        public Dictionary<int, string> guTrayCoord;

        public List<CalType> CalTypes;

        public void ShowForm()
        {
            Thread otherWindowHostingThread = new Thread(new ThreadStart(
                () => {
                    FrmFullAutoCal frm = new FrmFullAutoCal(this);

                    if (!GU.thisProductsGuStatus.VerifyIsOptional(0) && ProductionMode)
                    {
                        frm.btn_skip.IsEnabled = false;
                    }
                    else { frm.btn_skip.IsEnabled = true; }
                    frm.ShowDialog();
                }
            ));

            otherWindowHostingThread.SetApartmentState(ApartmentState.STA);
            otherWindowHostingThread.Start();
            otherWindowHostingThread.Join();
        }

        public void Execute(CancellationToken ct, IProgress<string> progressTitleMessage, IProgress<string> progressDetailMessage, IProgress<double> progressPercentValue)
        {
            if (CalTypes == null || CalTypes.Count == 0) return;

            foreach (CalType calType in CalTypes)
            {
                switch (calType)
                {
                    case CalType.VNA:

                        progressTitleMessage.Report("Running Network Analyzer Calibration...");

                        ExecuteNaCal();
                        break;

                    case CalType.Cable:
                        break;

                    case CalType.GU:

                        progressTitleMessage.Report("Running Golden Unit Calibration...");

                        if (Eq.Handler.Handler_Type == ATFRTE.Instance.HandlerType.ToString())
                            ExecuteAutoGUCal(ct, progressDetailMessage, progressPercentValue);
                        else
                            ExecuteGUCal(ct, progressDetailMessage, progressPercentValue);
<<<<<<< HEAD

=======
>>>>>>> origin/ModularMain-Joker

                        break;
                }
            }
        }

        private void ExecuteNaCal()
        {
            EqHSDIO.dutSlaveAddress = "F";
            // [Burhan]

            //string PCB_CALType = Interaction.InputBox("[PCB Sub Cal CalKit Table] => please enter 1\r\n\r\n[PCB Sub Cal dBase Table] => please enter 2\r\n\r\n[Not performing NA Cal] please press Cancel", "Network Analyzer Calibration", "2", 150, 150);
            string PCB_CALType = "2"; // Forced to run database calibration

            if ((PCB_CALType.ToUpper() == "1") || (PCB_CALType.ToUpper() == "2"))
            {
                // Redefine OTP for Spyro MIPI setting

                // MfgLotNum myMfgLotNum = new MfgLotNum();
                //   myMfgLotNum.DefineRegisters("F", "8", "9");

                // Perform AutoCalibration base on MIPI ID Tag for all the existing Test Site available
                MessageBox.Show("Please make sure all the PCB Cal units and Validation Units in tray and in place", "Start SPAR AutoCal", MessageBoxButtons.OK);
                //    Topaz_PCBSub_AutoCal.Extract_Eterm_With_AutoHandler_ForAutoCal(PCB_CALType, "1-2", myMfgLotNum); // Perform Site#1 and Site#2 Auto Calibration
                //   Topaz_PCBSub_AutoCal.Extract_Eterm_With_AutoHandler_ForAutoCal(PCB_CALType, "3-4", myMfgLotNum); // Perform Site#1 and Site#2 Auto Calibration
                MessageBox.Show("SPAR AutoCal Completed !!!!", "AutoCal", MessageBoxButtons.OK);
                byte site = 0;
                Eq.Site[site].EqENA.set_Memory_map();//Load the calibrated state file and perform the coupling setup.
                // Tray Map Terminate
                //if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
                //Eq.Handler.TrayMapTermination();
            }

            // place method for spar cal here
            /*
            if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
            Eq.Handler.TrayMapCoord("1,1;2,1;3,1;4,1;");

            Eq.Handler.TrayMapEOT("1,2,3,4");
            if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
            Eq.Handler.TrayMapTermination();
            */
            EqHSDIO.dutSlaveAddress = "E";

        }

        private void ExecuteAutoGUCal(CancellationToken ct, IProgress<string> progressObserver, IProgress<double> progressPercent)
        {
            string err = "";
            List<GuSnSequence> GuTrayMapSeq = GetHandlerTrayMapSeq(GU.dutIdLooseUserReducedList.Count, Eq.NumSites);

            int numTrayInsertions = 1;

            if (GU.GuMode.Contains(GU.GuModes.IccCorrVrfy))
                numTrayInsertions = 3;
            else if (GU.GuMode.Contains(GU.GuModes.CorrVrfy))
                numTrayInsertions = 2;

            try
            {
                using (CalHandlerProxy proxy = new CalHandlerProxy())
                {
                    Tuple<bool, string> hwInitRet = proxy.TheInstance_CAL.Init("");

                    #region GU Icc & Correlation
                    if (numTrayInsertions > 1 && hwInitRet.Item1)
                    {
                        for (int trayInsertion = 0; trayInsertion < numTrayInsertions - 1; trayInsertion++)
                        {
                            string dut_list = "Load STD Unit ";
                            foreach (int dutSN in GU.dutIdLooseUserReducedList)
                            {
                                dut_list += "#" + dutSN + " ";
                            }
                            dut_list += " into input tray and then press OK";
                            MessageBox.Show(dut_list, "Auto GU Cal", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            for (int deviceInsertion = 0; deviceInsertion < GuTrayMapSeq.Count; deviceInsertion++)
                            {
                                if (!GU.runningGU.Contains(true)) return;

                                if (ct.IsCancellationRequested) return;

                                Tuple<bool, string> dutRet = proxy.TheInstance_CAL.PickupDUT("");

                                if (dutRet.Item1)
                                {
                                    Thread.Sleep(5);

                                    GU.DoBeforeTest(GuTrayMapSeq[deviceInsertion].DutIndexPerSite, deviceInsertion == 0, trayInsertion == 0, progressObserver);

                                    double percentDone = 100.0 * (deviceInsertion + trayInsertion * GuTrayMapSeq.Count) / (double)(GuTrayMapSeq.Count * numTrayInsertions);
                                    progressPercent.Report(percentDone);

                                    myAtfTest.DoATFTest("");

                                    //GU.StoreMeasuredData(ResultBuilder.results, ResultBuilder.SitesAndPhases.AllIndexOf(2).Intersect(GuTrayMapSeq[deviceInsertion].ActiveSites));
                                    GU.StoreMeasuredData(ResultBuilder.results, new List<int> { 0 });

                                    dutRet = proxy.TheInstance_CAL.PutbackDUT("");
                                    if (!dutRet.Item1)
                                    {
                                        err = string.Format("CAL Test Putback DUT #{0} Failure: {1}", deviceInsertion + 1, dutRet.Item2);
                                        ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                                    }
                                }
                                else
                                {
                                    err = string.Format("CAL Test Pickup DUT #{0} Failure: {1}", deviceInsertion + 1, dutRet.Item2);
                                    ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                                }
                            }

                            GU.AllDutsDoneCorrelation();
                        }
                    }
                    #endregion GU Icc & Correlation

                    // GU Verification only
                    #region GU Verification
                    if (hwInitRet.Item1)
                    {
                        string dut_list = "Load STD Unit ";
                        foreach (int dutSN in GU.dutIdLooseUserReducedList)
                        {
                            dut_list += "#" + dutSN + " ";
                        }
                        dut_list += " into input tray and then press OK";
                        MessageBox.Show(dut_list, "Auto GU Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        for (int deviceInsertion = 0; deviceInsertion < GuTrayMapSeq.Count; deviceInsertion++)
                        {
                            if (!GU.runningGU.Contains(true)) return;

                            if (ct.IsCancellationRequested) return;

                            Tuple<bool, string> dutRet = proxy.TheInstance_CAL.PickupDUT("");

                            if (dutRet.Item1)
                            {
                                Thread.Sleep(5);
                                GU.DoBeforeTest(GuTrayMapSeq[deviceInsertion].DutIndexPerSite, deviceInsertion == 0, numTrayInsertions - 1 == 0, progressObserver);

                                double percentDone = 100.0 * (deviceInsertion + (numTrayInsertions - 1) * GuTrayMapSeq.Count) / (double)(GuTrayMapSeq.Count * numTrayInsertions);
                                progressPercent.Report(percentDone);
                                GU.GuMode.SetAll(GU.GuModes.Vrfy);
                                myAtfTest.DoATFTest("");
                                GU.GuMode.SetAll(GU.GuModes.CorrVrfy);
                                //GU.StoreMeasuredData(ResultBuilder.results, ResultBuilder.SitesAndPhases.AllIndexOf(2).Intersect(GuTrayMapSeq[deviceInsertion].ActiveSites));
                                GU.StoreMeasuredData(ResultBuilder.results, new List<int> { 0 });

                                dutRet = proxy.TheInstance_CAL.PutbackDUT("");
                                if (!dutRet.Item1)
                                {
                                    err = string.Format("CAL Test Putback DUT #{0} Failure: {1}", deviceInsertion + 1, dutRet.Item2);
                                    ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                                }
                            }
                            else
                            {
                                err = string.Format("CAL Test Pickup DUT #{0} Failure: {1}", deviceInsertion + 1, dutRet.Item2);
                                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                            }
                        }
                        GU.AllDutsDoneVerification();
                    }
                    else
                    {
                        err = string.Format("CAL Handler HW Init Failure: {0}", hwInitRet.Item2);
                        ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                    }
                    #endregion GU Verification

                    // Complete all access to handler, shut down
                    Tuple<bool, string> shutdownRet = proxy.TheInstance_CAL.FreeResources("");
                    if (!shutdownRet.Item1)
                    {
                        err = string.Format("CAL Handler FreeResources Failure: {0}", shutdownRet.Item2);
                        ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
                    }
                    else
                    {
                        ATFLogControl.Instance.Log(LogLevel.HighLight, LogSource.eTestPlan, "CAL Handler FreeResources Complete");
                    }
                }
            }
            catch (Exception ex)
            {
                err = string.Format("AutoGUCal Exception: {0}", ex.Message);
                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, err);
            }
        }

        private void ExecuteGUCal(CancellationToken ct, IProgress<string> progressObserver, IProgress<double> progressPercent)
        {
            List<GuSnSequence> GuTrayMapSeq = GetHandlerTrayMapSeq(GU.dutIdLooseUserReducedList.Count, Eq.NumSites);

            int numTrayInsertions = 1;

            if (GU.GuMode.Contains(GU.GuModes.IccCorrVrfy))
                numTrayInsertions = 3;
            else if (GU.GuMode.Contains(GU.GuModes.CorrVrfy))
                numTrayInsertions = 2;

            #region GU Icc & Correlation
            if (numTrayInsertions > 1)
            {
                for (int trayInsertion = 0; trayInsertion < numTrayInsertions - 1; trayInsertion++)
                {
                    if (Eq.Handler.Handler_Type != "S1" &&
                        Eq.Handler.Handler_Type != "S9" &&
                        Eq.Handler.Handler_Type != "MANUAL")
                    {
                        string dut_list = "Load STD Unit ";
                        foreach (int dutSN in GU.dutIdLooseUserReducedList)
                        {
                            dut_list += "#" + dutSN + " ";
                        }
                        dut_list += " into input tray and then press OK";
                        MessageBox.Show(dut_list, "Auto GU Cal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    for (int deviceInsertion = 0; deviceInsertion < GuTrayMapSeq.Count; deviceInsertion++)
                    {
                        if (!GU.runningGU.Contains(true)) return;

                        if (ct.IsCancellationRequested) return;

                        if (Eq.Handler.Handler_Type == "S1" ||
                            Eq.Handler.Handler_Type == "S9")
                        {
                            Eq.Handler.CheckSRQStatusByte(72);
                            Eq.Handler.TrayMapCoord(GuTrayMapSeq[deviceInsertion].HandlerSotCmd);
                        }
                        else if (Eq.Handler.Handler_Type != "MANUAL")
                        {
                            Eq.Handler.WaitForDUT();
                        }

                        Thread.Sleep(5);

                        GU.DoBeforeTest(GuTrayMapSeq[deviceInsertion].DutIndexPerSite, deviceInsertion == 0, trayInsertion == 0, progressObserver);

                        double percentDone = 100.0 * (deviceInsertion + trayInsertion * GuTrayMapSeq.Count) / (double)(GuTrayMapSeq.Count * numTrayInsertions);
                        progressPercent.Report(percentDone);

                        myAtfTest.DoATFTest("");

                        //GU.StoreMeasuredData(ResultBuilder.results, ResultBuilder.SitesAndPhases.AllIndexOf(2).Intersect(GuTrayMapSeq[deviceInsertion].ActiveSites));
                        GU.StoreMeasuredData(ResultBuilder.results, new List<int> { 0 });

                        //UpdateSitesAndPhases();
                        //myAtfTest.DoATFTest("");

                        if (Eq.Handler.Handler_Type == "S1" ||
                            Eq.Handler.Handler_Type == "S9")
                        {
                            if (deviceInsertion < (GuTrayMapSeq.Count - 1))
                                Eq.Handler.TrayMapCoord(GuTrayMapSeq[deviceInsertion + 1].HandlerSotCmd);
                            else
                                Eq.Handler.TrayMapCoord(GuTrayMapSeq[0].HandlerSotCmd);
                            Eq.Handler.TrayMapEOT(GuTrayMapSeq[deviceInsertion].HandlerEotCmd);

                            Eq.Handler.CheckSRQStatusByte(72);
                            //if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
                        }
                    }
                    //  if (trayInsertion == numTrayInsertions)
                    //      progressPercent.Report(100); // just to show 100% done

                    //GU.AllDutsDone();
                    GU.AllDutsDoneCorrelation();
                }
            }
            #endregion GU Icc & Correlation

            // GU Verification only
            #region GU Verification
            if (Eq.Handler.Handler_Type != "S1" &&
                Eq.Handler.Handler_Type != "S9" &&
                Eq.Handler.Handler_Type != "MANUAL")
            {
                string dut_list = "Load STD Unit ";
                foreach (int dutSN in GU.dutIdLooseUserReducedList)
                {
                    dut_list += "#" + dutSN + " ";
                }
                dut_list += " into input tray and then press OK";
                MessageBox.Show(dut_list, "Auto GU Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            for (int deviceInsertion = 0; deviceInsertion < GuTrayMapSeq.Count; deviceInsertion++)
            {
                if (!GU.runningGU.Contains(true)) return;

                if (ct.IsCancellationRequested) return;

                if (Eq.Handler.Handler_Type == "S1" ||
                    Eq.Handler.Handler_Type == "S9")
                {
                    Eq.Handler.CheckSRQStatusByte(72);
                    Eq.Handler.TrayMapCoord(GuTrayMapSeq[deviceInsertion].HandlerSotCmd);
                }
                else if (Eq.Handler.Handler_Type != "MANUAL")
                {
                    Eq.Handler.WaitForDUT();
                }

                Thread.Sleep(5);
                GU.DoBeforeTest(GuTrayMapSeq[deviceInsertion].DutIndexPerSite, deviceInsertion == 0, numTrayInsertions - 1 == 0, progressObserver);

                double percentDone = 100.0 * (deviceInsertion + (numTrayInsertions - 1) * GuTrayMapSeq.Count) / (double)(GuTrayMapSeq.Count * numTrayInsertions);
                progressPercent.Report(percentDone);
                GU.GuMode.SetAll(GU.GuModes.Vrfy);
                myAtfTest.DoATFTest("");
                GU.GuMode.SetAll(GU.GuModes.CorrVrfy);
                //GU.StoreMeasuredData(ResultBuilder.results, ResultBuilder.SitesAndPhases.AllIndexOf(2).Intersect(GuTrayMapSeq[deviceInsertion].ActiveSites));
                GU.StoreMeasuredData(ResultBuilder.results, new List<int> { 0 });

                if (Eq.Handler.Handler_Type == "S1" ||
                    Eq.Handler.Handler_Type == "S9")
                {
                    if (deviceInsertion < (GuTrayMapSeq.Count - 1))
                        Eq.Handler.TrayMapCoord(GuTrayMapSeq[deviceInsertion + 1].HandlerSotCmd);
                    else
                        Eq.Handler.TrayMapCoord(GuTrayMapSeq[0].HandlerSotCmd);
                    Eq.Handler.TrayMapEOT(GuTrayMapSeq[deviceInsertion].HandlerEotCmd);

                    Eq.Handler.CheckSRQStatusByte(72);
                    //if (Eq.Handler.CheckSRQStatusByte(72)) Eq.Handler.CheckSRQStatusByte(72);
                }
            }
            GU.AllDutsDoneVerification();
            #endregion GU Verification
        }

        private List<GuSnSequence> GetHandlerTrayMapSeq(int numDuts, int MaxSites)
        {
            // this method requires GU devices to be arranged in consecutive AutoCal.guTrayCoord locations

            List<int> GuDutIndices = new List<int>();
            for (int idx = 0; idx < numDuts; idx++) GuDutIndices.Add(idx);

            return GetHandlerTrayMapSeq(GuDutIndices, MaxSites);
        }

        private List<GuSnSequence> GetHandlerTrayMapSeq(List<int> DutIndices, int MaxSites)
        {
            // this method allows GU devices to be arranged in non-consecutive AutoCal.guTrayCoord locations

            while (DutIndices.Count % MaxSites != 0)   // we need groups of MaxSites
            {
                DutIndices.Add(-1);
            }

            int numGroups = DutIndices.Count / MaxSites;

            List<GuSnSequence> GuTrayMapSequence = new List<GuSnSequence>();

            for (int trayMapPlunge = 0; trayMapPlunge < DutIndices.Count; trayMapPlunge++)
            {
                List<int> DutIndexPerSite = ListShift(DutIndices.Skip(MaxSites * (trayMapPlunge % numGroups)).Take(MaxSites).ToList(), -(trayMapPlunge / numGroups));

                GuTrayMapSequence.Add(new GuSnSequence(guTrayCoord, DutIndexPerSite));
            }

            return GuTrayMapSequence;
        }

        private List<int> ListShift(List<int> list, int shiftIndex)
        {
            int len = list.Count;

            if (shiftIndex % len == 0) return list.ToList();

            List<int> shiftedList = new List<int>(len);

            for (int i = 0; i < len; i++) shiftedList.Add(0);

            int j = 0;

            int iStart = -shiftIndex % len, iStop = iStart + len;

            for (int i = iStart; i < iStop; i++)
            {
                if (i >= len) shiftedList[j++] = list[i - len];
                else if (i < 0) shiftedList[j++] = list[i + len];
                else shiftedList[j++] = list[i];
            }

            return shiftedList;
        }

        private class GuSnSequence
        {
            public List<int> DutIndexPerSite;
            public Dictionary<int, string> guTrayCoord;

            public string HandlerSotCmd
            {
                get
                {
                    string cmd = "";

                    for (int site = 0; site < DutIndexPerSite.Count; site++)
                    {
                        if (DutIndexPerSite[site] == -1)
                        {
                            cmd += "-1,-1";
                            //cmd += "-1,-1;";
                        }
                        else
                        {
                            //cmd += guTrayCoord[DutIndexPerSite[site]] + ";";   // ";" does not work on S1  KH 6/29/2017  May need to swith based on handler type Think this worked for S9
                            cmd += guTrayCoord[DutIndexPerSite[site]];
                        }
                    }

                    return cmd;
                }
                set
                {

                }
            }

            public string HandlerEotCmd
            {
                get
                {
                    string cmd = "";

                    for (int site = 0; site < DutIndexPerSite.Count; site++)
                    {
                        if (DutIndexPerSite[site] == -1)
                        {
                            cmd += "A";
                        }
                        else
                        {
                            cmd += (site + 1);
                        }

                        if (site != DutIndexPerSite.Count - 1) cmd += ",";
                    }

                    return cmd;
                }
                set
                {

                }
            }

            public List<int> ActiveSites
            {
                get
                {
                    List<int> activeSites = new List<int>();

                    for (int site = 0; site < DutIndexPerSite.Count; site++)
                    {
                        if (DutIndexPerSite[site] != -1)
                        {
                            activeSites.Add(site);
                        }
                    }

                    return activeSites;
                }
                set
                {

                }
            }

            public GuSnSequence(Dictionary<int, string> guTrayCoord, int NumSites)
            {
                this.guTrayCoord = guTrayCoord;

                DutIndexPerSite = new List<int>();

                for (int site = 0; site < NumSites; site++)
                {
                    DutIndexPerSite.Add(-1);
                }
            }

            public GuSnSequence(Dictionary<int, string> guTrayCoord, List<int> DutIndexPerSite)
            {
                this.guTrayCoord = guTrayCoord;
                this.DutIndexPerSite = DutIndexPerSite;
            }
        }

        public void UpdateSitesAndPhases()
        {
            int statenew = 0;

            // cycle the SitesAndPhases
            for (int site = 0; site < Eq.NumSites; site++)
            {
                statenew = ResultBuilder.SitesAndPhases[site];
                statenew++;
                if (statenew > 2)
                    statenew -= 3;

                ResultBuilder.SitesAndPhases[site] = statenew;
            }
        }
        public void ResetSitesAndPhases()
        {
            ResultBuilder.SitesAndPhases = new List<int>();

            for (int site = 0; site < Eq.NumSites; site++)
            {
                // SiteIdx is 0-based 
                if ((site % 2) == 0)
                    ResultBuilder.SitesAndPhases.Add(1);
                else
                    ResultBuilder.SitesAndPhases.Add(0);
            }
        }
    }

}
