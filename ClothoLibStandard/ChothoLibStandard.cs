using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Ivi.Visa.Interop;
using System.Runtime.InteropServices;


namespace ClothoLibStandard
{

    public class LibMeas : Lib_Var
    {
        EquipSG myLibSg;
        EquipSA myLibSa;
        EquipPM myLibPM;

        Stopwatch Speedo = new Stopwatch();
        HiPerfTimer timer = new HiPerfTimer();
        public LibMeas(EquipSG _myLibSg, EquipSA _myLibSa, EquipPM _myLibPM)
        {
            myLibSg = _myLibSg;
            myLibSa = _myLibSa;
            myLibPM = _myLibPM;
        }

        public LibMeas()
        {

        }

        ~LibMeas() { }

        public void SaMeasAclr12Lu2Project(string strFreqUnit, string strChBw, string strChSpacing, string strRbw, int noOfSweepPoint,
            ref double varTestResultAclr1L, ref double varTestResultAclr1U, ref double varTestResultAclr1, ref double varTestResultAclr2L, ref double varTestResultAclr2U, ref double varTestResultAclr2)
        {
            int varNAC = 2;

            float varChBandWidth = Convert.ToSingle(strChBw);
            if (strFreqUnit == "MHz")
                varChBandWidth = varChBandWidth * (float)FreqUnit.MHz;

            // Channel spacing in kHz
            float varChSpacing = Convert.ToSingle(strChSpacing);
            if (strFreqUnit == "MHz")
                varChSpacing = varChSpacing * (float)FreqUnit.MHz;

            float varSaRbw = Convert.ToSingle(strRbw);
            if (strFreqUnit == "MHz")
                varSaRbw = varSaRbw * (float)FreqUnit.MHz;
            else if (strFreqUnit == "kHz")
                varSaRbw = varSaRbw * (float)FreqUnit.KHz;


            // NTDP
            float varNTDP = (varChBandWidth / (varChSpacing * (2 * varNAC + 1) / (Convert.ToSingle(noOfSweepPoint) - 1))) / 2;
            // RBWF
            float varRBWF = (varChSpacing * (2 * varNAC + 1) / (Convert.ToSingle(noOfSweepPoint) - 1)) / varSaRbw;

            int
                i = 0,
                i2 = 0,
                varArrPoint = 0;

            float[] arrSaTraceData = null;
            double[] arrSaTraceDataDouble = new double[noOfSweepPoint];
            // arrSaTraceDataDouble.Initialize();

            double
                varIntPower = 0,
                varChPower = 0,
                varAclp1L = 0,
                varAclp1U = 0,
                varAclp2L = 0,
                varAclp2U = 0,
                varAclr1L = 0,
                varAclr1U = 0,
                varAclr1 = 0,
                varAclr2L = 0,
                varAclr2U = 0,
                varAclr2 = 0;

            // int varIdSa01 = 3;
            int varIdSa01 = 1;

            bool result = myLibSa.OPERATION_COMPLETE();

            myLibSa.WRITE(":TRAC? TRACE1");
            arrSaTraceData = myLibSa.READ_IEEEBlock(IEEEBinaryType.BinaryType_R4);

            // dBm to mW -> mW = 10^(dBm/10)

            int varSaSweepPoint = noOfSweepPoint;
            //if (varIdSa01 == 3)
            //    varSaSweepPoint = 1001;
            //else if (varIdSa01 == 1)
            //    varSaSweepPoint = 601;

            for (i = 0; i < varSaSweepPoint; i++)
                arrSaTraceDataDouble[i] = Math.Pow(10, (arrSaTraceData[i] / 10));

            // defIntegral(x,a-NTDP,a+NTDP)*RBWF


            // Channel Power: ArrPoint = 300 / 500
            // varArrPoint = 500;
            if (varIdSa01 == 3)
                varArrPoint = 500;
            else if (varIdSa01 == 1)
                varArrPoint = 300;

            varIntPower = 0;
            for (i2 = varArrPoint - (int)varNTDP; i2 <= varArrPoint + (int)varNTDP; i2++)
                varIntPower = varIntPower + arrSaTraceDataDouble[i2];
            // mW to dBm -> 10*log10(a)
            varChPower = 10 * Math.Log10(varIntPower * varRBWF);


            // ACLP1L: ArrPoint = 180                       

            if (varIdSa01 == 3)
                varArrPoint = 300;
            else if (varIdSa01 == 1)
                varArrPoint = 180;

            varIntPower = 0;
            for (i2 = varArrPoint - (int)varNTDP; i2 <= varArrPoint + (int)varNTDP; i2++)
                varIntPower = varIntPower + arrSaTraceDataDouble[i2];
            // mW to dBm -> 10*log10(a)
            varAclp1L = 10 * Math.Log10(varIntPower * varRBWF);


            // ACLP1U: ArrPoint = 420
            if (varIdSa01 == 3)
                varArrPoint = 700;
            else if (varIdSa01 == 1)
                varArrPoint = 420;

            varIntPower = 0;
            for (i2 = varArrPoint - (int)varNTDP; i2 <= varArrPoint + (int)varNTDP; i2++)
                varIntPower = varIntPower + arrSaTraceDataDouble[i2];
            // mW to dBm -> 10*log10(a)
            varAclp1U = 10 * Math.Log10(varIntPower * varRBWF);


            // ACLP2L: ArrPoint = 60
            if (varIdSa01 == 3)
                varArrPoint = 100;
            else if (varIdSa01 == 1)
                varArrPoint = 60;

            varIntPower = 0;
            for (i2 = varArrPoint - (int)varNTDP; i2 <= varArrPoint + (int)varNTDP; i2++)
                varIntPower = varIntPower + arrSaTraceDataDouble[i2];
            // mW to dBm -> 10*log10(a)
            varAclp2L = 10 * Math.Log10(varIntPower * varRBWF);


            // ACLP2U: ArrPoint = 540
            if (varIdSa01 == 3)
                varArrPoint = 900;
            else if (varIdSa01 == 1)
                varArrPoint = 540;

            varIntPower = 0;
            for (i2 = varArrPoint - (int)varNTDP; i2 <= varArrPoint + (int)varNTDP; i2++)
                varIntPower = varIntPower + arrSaTraceDataDouble[i2];
            // mW to dBm -> 10*log10(a)
            varAclp2U = 10 * Math.Log10(varIntPower * varRBWF);

            //varAclr1L = varAclp1L - varChPower;
            //varAclr1U = varAclp1U - varChPower;
            //varAclr2L = varAclp2L - varChPower;
            //varAclr2U = varAclp2U - varChPower;

            varTestResultAclr1L = varAclp1L - varChPower;
            varTestResultAclr1U = varAclp1U - varChPower;
            varTestResultAclr2L = varAclp2L - varChPower;
            varTestResultAclr2U = varAclp2U - varChPower;


            if (varTestResultAclr1L >= varTestResultAclr1U)
                varTestResultAclr1 = varTestResultAclr1L;
            else
                varTestResultAclr1 = varTestResultAclr1U;

            if (varTestResultAclr2L >= varTestResultAclr2U)
                varTestResultAclr2 = varTestResultAclr2L;
            else
                varTestResultAclr2 = varTestResultAclr2U;


            //varTestTimeStop = Timing.CounterValue;
            //varTestTimeElapsed = Timing.CalculateElapsedSeconds(varTestTimeStart, varTestTimeStop);
            //Debug.WriteLine("** TestTime - ACLR data output = " + varTestTimeElapsed);

        }

        public void SaInternalACP(string strFreqUnit, string strChBw, ref double varTestResultAclr1L, ref double varTestResultAclr1U, ref double varTestResultAclr1, ref double varTestResultAclr2L, ref double varTestResultAclr2U, ref double varTestResultAclr2)
        {
            int varNAC = 2;

            float varChBandWidth = Convert.ToSingle(strChBw);
            if (strFreqUnit == "MHz")
                varChBandWidth = varChBandWidth * 1000000;

            //// Channel spacing in kHz
            //float varChSpacing = Convert.ToSingle(strChSpacing);
            //if (strFreqUnit == "MHz")
            //    varChSpacing = varChSpacing * 1000000;

            //float varSaRbw = Convert.ToSingle(strRbw);
            //if (strFreqUnit == "MHz")
            //    varSaRbw = varSaRbw * 1000000;
            //else if (strFreqUnit == "kHz")
            //    varSaRbw = varSaRbw * 1000;


            //// NTDP
            //float varNTDP = (varChBandWidth / (varChSpacing * (2 * varNAC + 1) / (Convert.ToSingle(noOfSweepPoint) - 1))) / 2;
            //// RBWF
            //float varRBWF = (varChSpacing * (2 * varNAC + 1) / (Convert.ToSingle(noOfSweepPoint) - 1)) / varSaRbw;


            int i = 0;
            int i2 = 0;
            int varArrPoint = 0;

            float[] arrSaTraceData = null;
            //double[] arrSaTraceDataDouble = new double[noOfSweepPoint];
            // arrSaTraceDataDouble.Initialize();

            double varIntPower = 0;

            double varChPower = 0;

            double varAclp1L = 0;
            double varAclp1U = 0;

            double varAclp2L = 0;
            double varAclp2U = 0;

            double varAclr1L = 0;
            double varAclr1U = 0;
            double varAclr1 = 0;

            double varAclr2L = 0;
            double varAclr2U = 0;
            double varAclr2 = 0;

            myLibSa.WRITE("FETC:ACP?");
            //arrSaTraceData = myLibSa.READ_IEEEBlock(IEEEBinaryType.BinaryType_R4);

            // int varIdSa01 = 3;


            varTestResultAclr1L = arrSaTraceData[1] - arrSaTraceData[0];
            varTestResultAclr1U = arrSaTraceData[2] - arrSaTraceData[0];
            varTestResultAclr2L = arrSaTraceData[3] - arrSaTraceData[0];
            varTestResultAclr2U = arrSaTraceData[4] - arrSaTraceData[0];

            myLibSa.WRITE(":INIT:SAN");
            //if (varTestResultAclr1L >= varTestResultAclr1U)
            //    varTestResultAclr1 = varTestResultAclr1L;
            //else
            //    varTestResultAclr1 = varTestResultAclr1U;

            //if (varTestResultAclr2L >= varTestResultAclr2U)
            //    varTestResultAclr2 = varTestResultAclr2L;
            //else
            //    varTestResultAclr2 = varTestResultAclr2U;


            //varTestTimeStop = Timing.CounterValue;
            //varTestTimeElapsed = Timing.CalculateElapsedSeconds(varTestTimeStart, varTestTimeStop);
            //Debug.WriteLine("** TestTime - ACLR data output = " + varTestTimeElapsed);

        }

        public void SearchPoutProject(float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 0;
            float[]
                arrSaTraceData = null;
            string
                t1,
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {

                varInputMaxPout = TargetPout - ExpectedGain + 10;
                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                myLibSg.SET_OUTPUT_POWER(varInitSgOutput);

                // SG Delay
                //if (DelaySg > 0)   Thread.Sleep(DelaySg);    // Add settlng time for SG

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;

                while (varLoop < 10)
                //while (varLoop < 2)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement

                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;

                    // Delay for SG
                    Thread.Sleep((int)ºPsearchDelay);


                    //bool result = myLibSa.OPERATION_COMPLETE();

                    if (true) // PSA
                    {
                        //myLibSa.WRITE(":TRAC? TRACE1");
                        //arrSaTraceData = myLibSa.READ_IEEEBlock(IEEEBinaryType.BinaryType_R4);
                        //Thread.Sleep(8);
                        //for (int i = 0; i < arrSaTraceData.Length; i++)
                        //    varAverageTraceData = varAverageTraceData + arrSaTraceData[i];

                        //varAverageTraceData = varAverageTraceData / arrSaTraceData.Length; // +varOutputSgCal;


                        //myLibSa.WRITE(":FORM ASC");
                        varAverageTraceData = myLibSa.WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME");
                        //myLibSa.WRITE(":FORM REAL,32");

                    }
                    else // MXA                   
                    {
                        //myLibSa.WRITE(":FORM ASC");
                        varAverageTraceData = myLibSa.WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME");
                        //myLibSa.WRITE(":FORM REAL,32");
                    }

                    //varAverageTraceData = varAverageTraceData + varOutputSgCal;
                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData;



                    // TargetOutput-0.04<=CurrentPout AND CurrentPout<=TargetOutput+0.04
                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        // Is the current Pout within the spec, 0.04 dBm ???
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + TargetPout - varCurrentPout;

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                                varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);

                            myLibSg.SET_OUTPUT_POWER(varSGMaxPout);

                        }
                        else if (varNewSgOutput < -60)
                            myLibSg.SET_OUTPUT_POWER(-60);
                        else
                            myLibSg.SET_OUTPUT_POWER(varNewSgOutput);

                        //// Delay for SG
                        //if (DelaySg > 0) Thread.Sleep(Convert.ToInt16(ºPsearchDelay));
                    }

                    varLoop++;
                }

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 10) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;

                //float _meas;


            }
        }

        public void SearchPoutUSBProject(EquipPM _myLibPS, float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 1;
            float[]
                arrSaTraceData = null;
            string
                t1,
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;
            bool strPoutMeetFlag = false;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {
                varInputMaxPout = TargetPout - ExpectedGain + 10;
                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                myLibSg.SET_OUTPUT_POWER(varInitSgOutput);

                // SG Delay
                //if (DelaySg > 0) Thread.Sleep(DelaySg);    // Add settlng time for SG
                //Thread.Sleep(10);

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;


                //varAverageTraceData = _myLibPS.WRITE_READ_SINGLE("Fetc?") + LossAntPath;
                //if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                //{
                //    // Is the current Pout within the spec, 0.04 dBm ???
                //    varCurrentPin = varCurrentPin - varInputSgCal;
                //    strPoutMeetFlag = true;
                //}


                while (varLoop < 10)
                //while (varLoop < 2)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement

                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;

                    //bool result = myLibSa.OPERATION_COMPLETE();

                    Speedo.Reset();
                    Speedo.Start();

                    Thread.Sleep((int)ºPsearchDelay);
                    //Thread.Sleep(50);


                    long ElapsedTime1 = Speedo.ElapsedMilliseconds;

                    varAverageTraceData = _myLibPS.WRITE_READ_SINGLE(":FETCH?") + LossAntPath;

                    long ElapsedTime2 = Speedo.ElapsedMilliseconds;
                    Speedo.Stop();
                    long ElapsedTime3 = ElapsedTime2 - ElapsedTime1;

                    //varAverageTraceData = varAverageTraceData + varOutputSgCal;
                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData;



                    // TargetOutput-0.04<=CurrentPout AND CurrentPout<=TargetOutput+0.04
                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        // Is the current Pout within the spec, 0.04 dBm ???
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + (TargetPout - varCurrentPout);

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                            {
                                if (varCurrentPin > 17)
                                {
                                    myLibSg.SET_OUTPUT_POWER(-20);
                                }
                                else
                                {
                                    varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);
                                    myLibSg.SET_OUTPUT_POWER(varNewSgOutput);
                                }
                            }
                            else
                                myLibSg.SET_OUTPUT_POWER(-10);

                        }
                        else if (varNewSgOutput < -60)
                            myLibSg.SET_OUTPUT_POWER(-60);
                        else
                            myLibSg.SET_OUTPUT_POWER(varNewSgOutput);

                        // Delay for SG


                    }

                    varLoop++;
                }

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 10) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;
            }
        }

        public void SearchPoutUSBProject(EquipUSB_PS _myLibPS, float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 1;
            float[]
                arrSaTraceData = null;
            string
                t1,
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;
            bool strPoutMeetFlag = false;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {
                varInputMaxPout = TargetPout - ExpectedGain + 10;
                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                myLibSg.SET_OUTPUT_POWER(varInitSgOutput);

                // SG Delay
                //if (DelaySg > 0) Thread.Sleep(DelaySg);    // Add settlng time for SG
                //Thread.Sleep(200);

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;


                //varAverageTraceData = _myLibPS.WRITE_READ_SINGLE("Fetc?") + LossAntPath;
                //if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                //{
                //    // Is the current Pout within the spec, 0.04 dBm ???
                //    varCurrentPin = varCurrentPin - varInputSgCal;
                //    strPoutMeetFlag = true;
                //}


                while (varLoop < 20)
                //while (varLoop < 2)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement

                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;

                    //bool result = myLibSa.OPERATION_COMPLETE();

                    Speedo.Reset();
                    Speedo.Start();

                    Thread.Sleep((int)ºPsearchDelay);
                    //Thread.Sleep(50);
                    Speedo.Stop();

                    long ElapsedTime1 = Speedo.ElapsedMilliseconds;

                    Speedo.Reset();
                    Speedo.Start();

                    varAverageTraceData = _myLibPS.WRITE_READ_SINGLE("READ?") + LossAntPath;
                    Speedo.Stop();
                    long ElapsedTime2 = Speedo.ElapsedMilliseconds;

                    long ElapsedTime3 = ElapsedTime2 - ElapsedTime1;

                    //varAverageTraceData = varAverageTraceData + varOutputSgCal;
                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData;



                    // TargetOutput-0.04<=CurrentPout AND CurrentPout<=TargetOutput+0.04
                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        // Is the current Pout within the spec, 0.04 dBm ???
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + (TargetPout - varCurrentPout);

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                                varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);

                            myLibSg.SET_OUTPUT_POWER(varSGMaxPout);

                        }
                        else if (varNewSgOutput < -60)
                            myLibSg.SET_OUTPUT_POWER(-60);
                        else
                            myLibSg.SET_OUTPUT_POWER(varNewSgOutput);

                        // Delay for SG


                    }

                    varLoop++;
                }

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 10) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;
            }
        }

        public void SearchburstRSPoutUSBProject(EquipRSNRPZ _myLibPS, float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 1;
            float[]
                arrSaTraceData = null;
            string
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {

                //if (ºDUT_Counter > 1)
                //    varInputMaxPout = ºPre_Pin;
                //else
                varInputMaxPout = TargetPout - ExpectedGain + 10;

                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                myLibSg.SET_OUTPUT_POWER(varInitSgOutput);

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;

                //if (TargetPout > 6)ºmyLibRS_PS.range_setRange(1, 2);
                //else if (TargetPout > -14) ºmyLibRS_PS.range_setRange(1, 1);
                //else ºmyLibRS_PS.range_setRange(1, 0);

                while (varLoop < 20)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement

                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;

                    timer.wait(ºPsearchDelay);

                    //Speedo.Reset();
                    //Speedo.Start();
                    varAverageTraceData = RSPM_Meas(_myLibPS) + LossAntPath;

                    //Speedo.Stop();
                    //long ElapsedTime2 = Speedo.ElapsedMilliseconds;

                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData;

                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + (TargetPout - varCurrentPout);

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                                varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);
                            if (varNewSgOutput < 17)
                            {
                                myLibSg.SET_OUTPUT_POWER(varSGMaxPout);
                            }
                            else
                            {
                                myLibSg.SET_OUTPUT_POWER(-10);
                                varLoop = 20;
                            }

                        }
                        else if (varNewSgOutput < -60)
                            myLibSg.SET_OUTPUT_POWER(-60);
                        else
                            myLibSg.SET_OUTPUT_POWER(varNewSgOutput);
                    }

                    varLoop++;
                }

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 10) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;
            }
        }

        public void SearchRSNoburstPoutUSBProject(EquipRSNRPZ _myLibPS, float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,                
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 1;
            float[]
                arrSaTraceData = null;
            string
                t1,
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;
            bool strPoutMeetFlag = false;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {

                //if (ºDUT_Counter > 1)
                //    varInputMaxPout = ºPre_Pin;
                //else
                varInputMaxPout = TargetPout - ExpectedGain + 10;

                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                myLibSg.SET_OUTPUT_POWER(varInitSgOutput);

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;

                //timer.wait(500);
                //if (TargetPout > 6) ºmyLibRS_PS.range_setRange(1, 2);
                //else if (TargetPout > -14) ºmyLibRS_PS.range_setRange(1, 1);
                //else ºmyLibRS_PS.range_setRange(1, 0);

                while (varLoop < 20)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement

                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;
                    
                    string _waveform = Convert.ToString(ºWaveform);
                    timer.wait(ºPsearchDelay);
                    //if (_waveform == "LTE10M50RB_MCS6") Thread.Sleep(100);
                    varAverageTraceData = RSPM_NoBurstMeas(_myLibPS) + LossAntPath;

                   

                    //varAverageTraceData = varAverageTraceData + varOutputSgCal;
                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData; 

                    // TargetOutput-0.04<=CurrentPout AND CurrentPout<=TargetOutput+0.04
                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        // Is the current Pout within the spec, 0.04 dBm ???
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + (TargetPout - varCurrentPout);

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                                varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);

                            myLibSg.SET_OUTPUT_POWER(varSGMaxPout);
                            //myLibSg.SET_OUTPUT_POWER(0);

                        }
                        else if (varNewSgOutput < -60)
                            myLibSg.SET_OUTPUT_POWER(-60);
                        else
                            myLibSg.SET_OUTPUT_POWER(varNewSgOutput);
                    }

                    myLibSg.OPERATION_COMPLETE();
                    varLoop++;
                }                          

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 20) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;
            }
        }

        public float RSPM_Meas(EquipRSNRPZ _EquipRSPS)
        {
            //Speedo.Reset();
            //Speedo.Start();

            #region scope mode
            bool meas_complete = true;

            //   _EquipRSPS.chans_initiate();
            //    System.DateTime tout = System.DateTime.Now.AddSeconds(5);
            //    do
            //    {
            //        _EquipRSPS.chan_isMeasurementComplete(1, out meas_complete);
            //    } while (!meas_complete && (System.DateTime.Now.CompareTo(tout) < 0));

            //    if (meas_complete)
            //    {
            //        double[] result = new double[8];
            //        int count = 0;

            //        _EquipRSPS.meass_fetchBufferMeasurement(1, 16, result, out count);
            //        if (count > 0)
            //        {
            //            double meas_value;

            //            long ElapsedTime2 = Speedo.ElapsedMilliseconds;

            //            Speedo.Stop();
            //            return Convert.ToSingle(meas_value = (10 * Math.Log(Math.Abs(result.Average())) / Math.Log(10)) + 30.0);

            //        }
            //        else
            //        {
            //            throw new System.Runtime.InteropServices.ExternalException("BURST RSPM_Meas Measurement Error Occured", 0);
            //        }
            //    }
            //    else
            //    {
            //        throw new System.Runtime.InteropServices.ExternalException("BURST RSPM_Meas Measurement Timeout Occured", 0);
            //    }
            //}


            #endregion

            #region timeslot
            System.DateTime begin_time = System.DateTime.Now;

            for (int i = 0; i < 5; i++)
            {

                _EquipRSPS.chans_initiate();
                System.DateTime tout = System.DateTime.Now.AddSeconds(1);
                do
                {
                    _EquipRSPS.chan_isMeasurementComplete(1, out meas_complete);
                    System.Threading.Thread.Sleep(0);
                } while (!meas_complete && (System.DateTime.Now.CompareTo(tout) < 0));

                if (meas_complete)
                {
                    double[] result = new double[5];
                    int count = 0;

                    _EquipRSPS.meass_fetchBufferMeasurement(1, 5, result, out count);
                    if (count > 0)
                    {
                        double meas_value;
                        double Avg_result;
                        if (count == 2)
                        {
                            Avg_result = ((result[0] + result[1]) / 2);
                        }
                        else
                        {
                            Avg_result = (result[0]);
                        }
                        //meas_value = (10 * Math.Log(Math.Abs(result[0])) / Math.Log(10)) + 30.0;
                        meas_value = (10 * Math.Log(Math.Abs(Avg_result)) / Math.Log(10)) + 30.0;
                        //return  meas_value.ToString();
                        //Update();
                        //measResults[i] = meas_value;
                        return (float)meas_value;

                    }
                    else
                    {
                        throw new System.Runtime.InteropServices.ExternalException("Measurement Error Occured", 0);
                    }
                }
                else
                {
                    break;
                    throw new System.Runtime.InteropServices.ExternalException("Measurement Timeout Occured", 0);
                }
            }


            #endregion
            return -999;
        }

        public float RSPM_NoBurstMeas(EquipRSNRPZ _EquipRSPS)
        {
            bool meas_complete;
            ºmyLibRS_PS.chans_initiate();
            System.DateTime tout = System.DateTime.Now.AddSeconds(5);
            do
            {
                ºmyLibRS_PS.chan_isMeasurementComplete(1, out meas_complete);

            } while (!meas_complete && (System.DateTime.Now.CompareTo(tout) < 2));

            if (meas_complete)
            {
                double result, meas_value;
                ºmyLibRS_PS.meass_fetchMeasurement(1, out result);
                meas_value = 10 * Math.Log(Math.Abs(result)) / Math.Log(10) + 30.0;
                return (float)meas_value;
            }
            else
            {

                //throw new System.Runtime.InteropServices.ExternalException("NON BURST RSPM_Meas Measurement Timeout Occured", 0);
            }

            return -999;
        }

        public void MeasPoutZeroSpan(float LossAntPath, ref float Pout)
        {
            int i = 0;
            float varAverageTraceData = 0;
            float[] arrSaTraceData = null;

            //myLibSa.OPERATION_COMPLETE();
            //Thread.Sleep(10);

            //myLibSa.WRITE(":FORM ASC");
            varAverageTraceData = myLibSa.WRITE_READ_SINGLE(":CALC:DATA1:COMP? DME");
            //myLibSa.WRITE(":FORM REAL,32");
            //myLibSa.WRITE(":TRAC? TRACE1");
            //arrSaTraceData = myLibSa.READ_IEEEBlock(IEEEBinaryType.BinaryType_R4);

            //for (i = 0; i < arrSaTraceData.Length; i++)
            //    varAverageTraceData = varAverageTraceData + arrSaTraceData[i];

            //varAverageTraceData = varAverageTraceData / arrSaTraceData.Length; // +varOutputSgCal;

            //varAverageTraceData = varAverageTraceData / arrSaTraceData.Length; // +varOutputSgCal;
            // varAverageTraceData = varAverageTraceData + varOutputSgCal + (float)0.45;
            //varAverageTraceData = varAverageTraceData;//+ LossAntPath;

            Pout = varAverageTraceData;

            //Pout = (float)myLibSa.MEASURE_MEAN_POINT();

            ºPout = Pout;
        }

        //PXI
        public void SearchPoutProjectPXI(PxiSg mySg01, PxiSa mySa01, float TargetPout, float LossInput, float LossAntPath, float ExpectedGain, int DelaySg, string ModulationMode, string Waveform, int poutSearchAverage, ref int ServoCount)
        {
            int
                varLoop = 1;            // Pout search loop control to limit the pout search cycle
            float
                Pin = 0,
                Pout = 0,
                Gain = 0,
                varInputSgCal = LossInput,
                varOutputSgCal = LossAntPath,
                varPoutTolerance = ºSearch_tolerance,
                varInitSgOutput = 0,  // Initial SG Pout setting
                varNewSgOutput = 0,   // New SG Pout value for next pout search
                varInputMaxPout = 0,
                varSGMaxPout = 0,
                varCurrentPout = 0,
                varCurrentPin = 0,
                varAverageTraceData = 0;
            ºPin = 0;
            ºPout = 0;
            ºGain = 0;
            ºvarLoop = 1;
            float[]
                arrSaTraceData = null;
            string
                t1,
                strCurrentPout = null,   // Measured current Pout
                strCurrentPin = null,    // Measured(PS) or Calculated(SG) current Pin
                varTestResult = null;
            bool strPoutMeetFlag = false;

            if (false)
            {
                Debug.WriteLine("\nTarget Pout was found.");
            }
            else
            {

                //if (ºDUT_Counter > 1)
                //    varInputMaxPout = ºPre_Pin;
                //else
                varInputMaxPout = TargetPout - ExpectedGain + 10;

                varSGMaxPout = TargetPout - ExpectedGain + varInputSgCal + 10;

                // SG initial power setting
                varInitSgOutput = TargetPout - ExpectedGain + varInputSgCal; // Initial SG output. Calculated from (Target Pout - Estimated Gain)            

                //myLibSg.SET_OUTPUT_POWER(varInitSgOutput);    
                mySg01.PoweLevel(varInitSgOutput, ModulationMode, ºPXI_Waveform);

                varCurrentPout = 0;
                varNewSgOutput = varCurrentPin = varInitSgOutput;

                //timer.wait(500);
                //if (TargetPout > 6) ºmyLibRS_PS.range_setRange(1, 2);
                //else if (TargetPout > -14) ºmyLibRS_PS.range_setRange(1, 1);
                //else ºmyLibRS_PS.range_setRange(1, 0);

                // SA Setting
                long samplePerRecord = 0;
                if (ModulationMode == "WCDMA" || ModulationMode == "LTE" || ModulationMode == "LTETDD" || ModulationMode == "1XEVDO" || ModulationMode == "TDSCDMA")
                {
                    mySa01.RfSaConfHw("Pout", ModulationMode, poutSearchAverage, ref samplePerRecord);
                }

                while (varLoop < 20)
                {

                    varCurrentPin = varNewSgOutput; // Current Pin Measurement  

                    // Current Pout Measurement
                    strCurrentPout = null;
                    arrSaTraceData = null;
                    varTestResult = null;
                    varAverageTraceData = 0;

                    timer.wait(ºPsearchDelay);

                    //Measurement
                    if (ModulationMode == "WCDMA")
                    {
                        mySa01.MeasPoutWcdmaChp(samplePerRecord, ref varAverageTraceData);
                    }
                    else if (ModulationMode == "1XEVDO")
                    {
                        mySa01.MeasPoutEvdoChp(samplePerRecord, ref varAverageTraceData);
                    }
                    else if (ModulationMode == "LTE" || ModulationMode == "LTETDD")
                    {
                        Stopwatch myWatchSub = new Stopwatch();
                        myWatchSub.Reset();
                        myWatchSub.Start();

                        mySa01.MeasPoutLteChp(samplePerRecord, ref varAverageTraceData);

                        myWatchSub.Stop();
                        long testTimeSub = myWatchSub.ElapsedMilliseconds;
                    }
                    else if (ModulationMode == "TDSCDMA")
                    {
                        Stopwatch myWatchSub = new Stopwatch();
                        myWatchSub.Reset();
                        myWatchSub.Start();

                        mySa01.MeasPoutTdscdmaChp(samplePerRecord, ref varAverageTraceData);

                        myWatchSub.Stop();
                        long testTimeSub = myWatchSub.ElapsedMilliseconds;
                    }
                    else if (ModulationMode == "GSM")
                    {
                        // mySa01.MeasPoutGsm(ref varAverageTraceData);
                        // mySa01.MeasPoutGsmEdgeIq(ref varAverageTraceData);
                        mySa01.MeasPoutGsmTxP(ref varAverageTraceData);
                    }
                    else if (ModulationMode == "EDGE")
                    {
                        //bool maskTestResult = false;
                        //mySa01.MeasPoutEdgePvt(ref varAverageTraceData, ref maskTestResult);
                        //mySa01.MeasPoutGsmEdgeIq(ref varAverageTraceData);
                        mySa01.MeasPoutGsmTxP(ref varAverageTraceData);
                    }
                    else
                    {
                        MessageBox.Show("No supported modulation mode.", "SearchPoutProjectShamu Modulation type error.");
                    }

                    varAverageTraceData += LossAntPath;

                    ////KCC - For Portal HPM gain issue
                    //if (varCurrentPout >= 20)
                    //{
                    //    //CTM
                    //    float PrevPwr = 100;
                    //    timer.wait(500);

                    //    //while (Math.Abs(varAverageTraceData - PrevPwr) > 0.02)
                    //    //{
                    //    //    PrevPwr = varAverageTraceData;
                    //    //    varAverageTraceData = RSPM_NoBurstMeas(_myLibPS);
                    //    //    timer.wait(10);
                    //    //}
                    //    varAverageTraceData = varAverageTraceData + LossAntPath;
                    //}                    
                    //varAverageTraceData = varAverageTraceData + varOutputSgCal;

                    strCurrentPout = Convert.ToString(varAverageTraceData);
                    varCurrentPout = varAverageTraceData;

                    // TargetOutput-0.04<=CurrentPout AND CurrentPout<=TargetOutput+0.04
                    if ((varCurrentPout >= (TargetPout - varPoutTolerance)) && (varCurrentPout <= (TargetPout + varPoutTolerance)))
                    {
                        // Is the current Pout within the spec, 0.04 dBm ???
                        varCurrentPin = varCurrentPin - varInputSgCal;
                        break;
                    }
                    else
                    {
                        varNewSgOutput = varNewSgOutput + (TargetPout - varCurrentPout);

                        if (varNewSgOutput > varSGMaxPout)
                        {
                            if (varCurrentPin > varInputMaxPout)
                                varNewSgOutput = varNewSgOutput - (varCurrentPin - varInputMaxPout);

                            //myLibSg.SET_OUTPUT_POWER(varSGMaxPout);
                            mySg01.PoweLevel(varSGMaxPout, ModulationMode, ºPXI_Waveform);

                        }
                        else if (varNewSgOutput < -60)
                            //myLibSg.SET_OUTPUT_POWER(-60);
                            mySg01.PoweLevel(-60, ModulationMode, ºPXI_Waveform);
                        else
                            //myLibSg.SET_OUTPUT_POWER(varNewSgOutput);
                            mySg01.PoweLevel(varNewSgOutput, ModulationMode, ºPXI_Waveform);
                    }

                    varLoop++;
                }

                Pin = varCurrentPin;
                Pout = varCurrentPout;
                Gain = Pout - Pin;

                if (varLoop >= 20) varCurrentPin = varCurrentPin - varInputSgCal;

                ºPin = Pin; ºPout = Pout; ºGain = Gain; ºvarLoop = varLoop;
            }

        }
    }

}
