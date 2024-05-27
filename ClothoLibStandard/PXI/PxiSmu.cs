using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using NationalInstruments.ModularInstruments.Interop;
// using NationalInstruments.NI4882;

namespace ClothoLibStandard
{
    public class PxiSmu
    {
        nidcpower mySmu;

        public int Abort()
        {
            int error = -1;

            try
            {
                // error = mySmu.Disable();
                // error = mySmu.Commit();
                // mySmu.SetBoolean(nidcpowerProperties.OutputEnabled, false);
                error = mySmu.Abort();

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Abort");
                return error;
            }
        }

        public int Disable()
        {
            int error = -1;

            try
            {                
                // error = mySmu.Disable();
                // error = mySmu.Commit();
                // mySmu.SetBoolean(nidcpowerProperties.OutputEnabled, false);
                mySmu.SetBoolean(nidcpowerProperties.OutputEnabled, true);

                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int ConfigureCurrentLimit(string Channel_Number, double Limit)
        {
            int error = -1;

            try
            {
                error = mySmu.ConfigureCurrentLimit(Channel_Number, nidcpowerConstants.CurrentRegulate, Limit);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimit");
                return error;
            }
        }

        public int ConfigureCurrentLimitRange(string Channel_Number, double Range)
        {
            int error = -1;

            try
            {
                error = mySmu.ConfigureCurrentLimitRange(Channel_Number, Range);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimitRange");
                return error;
            }
        }

        public int ConfigureCurrentLimitAutoRange(string Channel_Number, bool State)
        {
            int error = -1;

            try
            {
                if (State)
                {
                    //int tempInt = mySmu.GetInt32(nidcpowerProperties.CurrentLimitAutorange, Channel_Number);
                    //tempInt = nidcpowerConstants.On;
                    mySmu.SetInt32(nidcpowerProperties.CurrentLimitAutorange, Channel_Number, nidcpowerConstants.On);
                }
                else
                    mySmu.SetInt32(nidcpowerProperties.CurrentLimitAutorange, Channel_Number, nidcpowerConstants.Off);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimitRange");
                return error;
            }
        }

        public int ConfigureOutputFunction(string Channel_Number, string Output_Mode)
        {
            int error = 0;

            try
            {
                if (Output_Mode.ToUpper() == "DCVOLTAGE")
                    error = mySmu.ConfigureOutputFunction(Channel_Number, nidcpowerConstants.DcVoltage);
                else
                {                    
                    throw new Exception("The Output mode is not supported.");
                }
                    
                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int ConfigureSense(string Channel_Number, string Sense_Mode)
        {
            int error = 0;

            try
            {
                if (Sense_Mode.ToUpper() == "REMOTE")
                    error = mySmu.ConfigureSense(Channel_Number, nidcpowerConstants.Remote);
                if (Sense_Mode.ToUpper() == "LOCAL")
                    error = mySmu.ConfigureSense(Channel_Number, nidcpowerConstants.Local);
                else
                {
                    throw new Exception("The sense mode is not supported.");
                }

                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int ConfigureSourceMode(bool Multiple_Meas, int samplesToAverage)
        {
            int error = 0;

            try
            {                
                error = mySmu.ConfigureSourceMode(nidcpowerConstants.SinglePoint);

                if (Multiple_Meas)
                {
                    mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);

                    // mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                    
                }
                                
                //mySmu.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                //error = mySmu.ConfigureDigitalEdgeMeasureTrigger("/SMU01/PXI_Trig0", nidcpowerConstants.Rising);
                //mySmu.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "/SMU01/PXI_Trig0");
                

                mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, samplesToAverage);

                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int ConfigureVoltageLevel(string Channel_Number, double Level)
        {
            int error = -1;
            
            try
            {                
                error = mySmu.ConfigureVoltageLevel(Channel_Number, Level);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureVoltageLevel");
                return error;
            }
        }

        public int ConfigureVoltageLevelAutoRange(string Channel_Number, bool State)
        {
            int error = -1;

            try
            {
                if (State)
                    mySmu.SetInt32(nidcpowerProperties.VoltageLevelAutorange, Channel_Number, nidcpowerConstants.On);
                else
                    mySmu.SetInt32(nidcpowerProperties.VoltageLevelAutorange, Channel_Number, nidcpowerConstants.Off);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimitRange");
                return error;
            }
        }

        public int ConfigureVoltageLevelRange(string Channel_Number, double Range)
        {
            int error = -1;

            try
            {
                error = mySmu.ConfigureVoltageLevelRange(Channel_Number, Range);

                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int Initialize(string Resource_Name, string Channels, bool Reset, string Option_String)
        {
            try
            {
                mySmu = new NationalInstruments.ModularInstruments.Interop.nidcpower(Resource_Name, Channels, Reset, Option_String);
                
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Initiazile");
                return -1;
            }
        }

        public int Initiate()
        {
            int error = -1;

            try
            {
                error = mySmu.Initiate();

                return 0;
            }
            catch (Exception e)
            {
                return error;
            }
        }

        public int MeasureCurrent(string Channel_Number, ref float Result)
        {
            int error = -1;
            double tempResult = 0;
            
            try
            {
                //int tempValue = nidcpowerConstants.PowerLineCycles;
                //mySmu.SetInt32(nidcpowerProperties.ApertureTimeUnits, Channel_Number, nidcpowerConstants.Seconds);
                //mySmu.SetDouble(nidcpowerProperties.ApertureTime, Channel_Number, 0.0001);
                double[] voltMulti = new double[100];
                double[] currentMulti = new double[100];

                error = mySmu.Measure(Channel_Number, nidcpowerConstants.MeasureCurrent, out tempResult);
                error = mySmu.MeasureMultiple(Channel_Number, voltMulti, currentMulti);
                Result = Convert.ToSingle(tempResult);

                //bool resultBool = false;
                //mySmu.QueryInCompliance("0", out resultBool);
                

                //double[] volt = new double[200];
                //double[] curr = new double[200];
                //ushort[] measComp = new ushort[200];
                //int actCount = 0;

                //error = mySmu.MeasureMultiple("0", volt, curr);
                //// error = mySmu.Measure("0", nidcpowerConstants.MeasureVoltage, 
                //// error = my

                //error = mySmu.FetchMultiple("0", 5, 200, volt, curr, measComp, out actCount);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrent");
                return error;
            }
        }

        public int MeasureCurrentTrace(string Channel_Number, int NumberOfTrace, float currentLimit, ref float Result)
        {
            int error = -1;
            double tempResult = 0;

            try
            {
                //error = mySmu.Measure(Channel_Number, nidcpowerConstants.MeasureCurrent, out tempResult);
                //Result = Convert.ToSingle(tempResult);

                //bool resultBool = false;
                //mySmu.QueryInCompliance("0", out resultBool);

                mySmu.Abort();
                mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 500);
                //int result = mySmu.GetInt32(nidcpowerProperties.SamplesToAverage);
                mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, 50);
                // int result = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                NumberOfTrace = 50;
                mySmu.Initiate();

                double[] volt = new double[NumberOfTrace];
                double[] curr = new double[NumberOfTrace];
                double[] curr2 = new double[NumberOfTrace];
                double[] curr3 = new double[NumberOfTrace];
                ushort[] measComp = new ushort[NumberOfTrace];
                int actCount = 0;

                //error = mySmu.MeasureMultiple("0", volt, curr);
                // error = mySmu.Measure("0", nidcpowerConstants.MeasureVoltage, 
                // error = my

                int fetchBackLog = mySmu.GetInt32(nidcpowerProperties.FetchBacklog);

                if (fetchBackLog != 0)
                {
                    double[] voltTemp = new double[fetchBackLog];
                    double[] currTemp = new double[fetchBackLog];
                    ushort[] measCompTemp = new ushort[fetchBackLog];
                    error = mySmu.FetchMultiple(Channel_Number, 5, fetchBackLog, voltTemp, currTemp, measCompTemp, out actCount);
                }

                // error = mySmu.ConfigureMeasPoint(NumberOfTrace);
                


                
                for (int i = 0; i < 1; i++)
                {
                    // int tempInt = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                }

                Stopwatch myWatch = new Stopwatch();
                long testTime = 0;
                myWatch.Reset();
                myWatch.Start();


                

                // ATFCrossDomainWrapper.StoreIntToCache("TraceLength", 100);
                                
                    for (int i = 0; i < 1; i++)
                    {
                        //mySmu.Abort();
                        //mySmu.SetDouble(nidcpowerProperties.SourceDelay, Channel_Number, 0.00003);
                        //mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 20);
                        //error = mySmu.ConfigureCurrentLimit(Channel_Number, nidcpowerConstants.CurrentRegulate, currentLimit);
                        //mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NumberOfTrace);
                        //mySmu.Initiate();
                    }

                    myWatch.Stop();
                    testTime = myWatch.ElapsedMilliseconds;
         


                // error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                Thread.Sleep(6);
                try
                {
                    error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);

                    int measCompTrigger = 0;

                    foreach (int measCompValue in measComp)
                        if (measCompValue == 1)
                            measCompTrigger = 1;

                    if (measCompTrigger == 1)
                        Thread.Sleep(1);
                }
                catch
                {
                    Thread.Sleep(6);
                    error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);
                }
                
                // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                double tempResultSum = 0;

                foreach (double t1 in curr)
                {
                    // tempResultSum = tempResultSum + t1;
                    tempResultSum = tempResultSum + t1;
                }
                
                Result = Convert.ToSingle(tempResultSum / curr.Length);

                if (Result == float.NaN)
                    Result = -999;

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrentTrace");
                return -1;
            }
        }

        public int MeasureCurrentTracePulse(string Channel_Number, int NumberOfTrace, float currentLimit, string modulationMode, ref float Result)
        {
            int error = -1;
            double tempResult = 0;

            try
            {
                //error = mySmu.Measure(Channel_Number, nidcpowerConstants.MeasureCurrent, out tempResult);
                //Result = Convert.ToSingle(tempResult);

                //bool resultBool = false;
                //mySmu.QueryInCompliance("0", out resultBool);

                mySmu.Abort();

                if (false)
                {

                    mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);                    
                    mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 20);
                    //int result = mySmu.GetInt32(nidcpowerProperties.SamplesToAverage);
                    mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, 200);
                    // int result = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                    NumberOfTrace = 100;
                    
                }
                else
                {
                    mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                    mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 1);

                    NumberOfTrace = 124;
                    mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NumberOfTrace);                    

                    mySmu.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);                    
                    error = mySmu.ConfigureDigitalEdgeMeasureTrigger("/SMU01/PXI_Trig0", nidcpowerConstants.Rising);
                    mySmu.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "/SMU01/PXI_Trig0");
                

                }

                mySmu.Initiate();

                
                
                


                double[] volt = new double[NumberOfTrace];
                double[] curr = new double[NumberOfTrace];
                double[] curr2 = new double[NumberOfTrace];
                double[] curr3 = new double[NumberOfTrace];
                ushort[] measComp = new ushort[NumberOfTrace];
                int actCount = 0;

                //error = mySmu.MeasureMultiple("0", volt, curr);
                // error = mySmu.Measure("0", nidcpowerConstants.MeasureVoltage, 
                // error = my

                // mySmu.MeasureMultiple(Channel_Number, volt, curr);



                int fetchBackLog = mySmu.GetInt32(nidcpowerProperties.FetchBacklog);

                if (fetchBackLog != 0)
                {
                    double[] voltTemp = new double[fetchBackLog];
                    double[] currTemp = new double[fetchBackLog];
                    ushort[] measCompTemp = new ushort[fetchBackLog];
                    error = mySmu.FetchMultiple(Channel_Number, 5, fetchBackLog, voltTemp, currTemp, measCompTemp, out actCount);
                }

                // error = mySmu.ConfigureMeasPoint(NumberOfTrace);




                for (int i = 0; i < 1; i++)
                {
                    // int tempInt = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                }

                Stopwatch myWatch = new Stopwatch();
                long testTime = 0;
                myWatch.Reset();
                myWatch.Start();




                // ATFCrossDomainWrapper.StoreIntToCache("TraceLength", 100);

                for (int i = 0; i < 1; i++)
                {
                    //mySmu.Abort();
                    //mySmu.SetDouble(nidcpowerProperties.SourceDelay, Channel_Number, 0.00003);
                    //mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 20);
                    //error = mySmu.ConfigureCurrentLimit(Channel_Number, nidcpowerConstants.CurrentRegulate, currentLimit);
                    //mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NumberOfTrace);
                    //mySmu.Initiate();
                }

                myWatch.Stop();
                testTime = myWatch.ElapsedMilliseconds;



                // error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                // error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                Thread.Sleep(3);
                try
                {
                    error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);
                    mySmu.Abort();
                }
                catch
                {
                    Thread.Sleep(1);
                    error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);
                }
                // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                foreach (ushort i in measComp)
                {
                    //if (i == 1)
                    //    MessageBox.Show("Current compliance");
                }

                double tempResultSum = 0;
                double tempMax = 0;

                foreach (double t1 in curr)
                {
                    // tempResultSum = tempResultSum + t1;
                    if (t1 > tempMax)
                        tempMax = t1;
                }

                if (true)
                {
                    for (int itemp = 0; itemp < 1; itemp++)
                    {
                        error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                        Thread.Sleep(3);
                        try
                        {
                            error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr2, measComp, out actCount);
                        }
                        catch
                        {
                            Thread.Sleep(1);
                            error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr2, measComp, out actCount);
                        }
                        // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                        foreach (ushort i in measComp)
                        {
                            //if (i == 1)
                            //    MessageBox.Show("Current compliance");
                        }

                        foreach (double t1 in curr2)
                        {
                            // tempResultSum = tempResultSum + t1;
                            if (t1 > tempMax)
                                tempMax = t1;
                        }
                    }
                }

                if (true)
                {
                    for (int itemp = 0; itemp < 1; itemp++)
                    {
                        error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                        Thread.Sleep(3);
                        try
                        {
                            error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr3, measComp, out actCount);
                        }
                        catch
                        {
                            Thread.Sleep(1);
                            error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr3, measComp, out actCount);
                        }
                        // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                        foreach (ushort i in measComp)
                        {
                            //if (i == 1)
                            //    MessageBox.Show("Current compliance");
                        }

                        foreach (double t1 in curr3)
                        {
                            // tempResultSum = tempResultSum + t1;
                            if (t1 > tempMax)
                                tempMax = t1;
                        }
                    }
                }



                double searchRange = 0.5;
                double maxCurrSearchLimit01 = tempMax * searchRange;

                int countBin01 = 0;

                foreach (double currentValue in curr)
                {
                    //if (currentValue == 0.139468)
                    //    Thread.Sleep(1);
                    if (currentValue > maxCurrSearchLimit01)
                    {
                        tempResultSum = tempResultSum + currentValue;
                        countBin01++;
                    }
                }

                if (true)
                {
                    foreach (double currentValue in curr2)
                    {
                        //if (currentValue == 0.139468)
                        //    Thread.Sleep(1);
                        if (currentValue > maxCurrSearchLimit01)
                        {
                            tempResultSum = tempResultSum + currentValue;
                            countBin01++;
                        }
                    }
                }

                if (true)
                {
                    foreach (double currentValue in curr3)
                    {
                        //if (currentValue == 0.139468)
                        //    Thread.Sleep(1);
                        if (currentValue > maxCurrSearchLimit01)
                        {
                            tempResultSum = tempResultSum + currentValue;
                            countBin01++;
                        }
                    }
                }


                Result = Convert.ToSingle(tempResultSum / countBin01);

                if (Result == float.NaN)
                    Thread.Sleep(1);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrentTrace");
                return -1;
            }
        }

        public int MeasureCurrentTracePulseDigitalEdgeTrigger(string Channel_Number, int NumberOfTrace, float currentLimit, string modulationMode, int measureCount, ref float Result)
        {
            int error = -1;
            double tempResult = 0;
            double tempResultTotal = 0;
            int indexCurrAvgStart = 0;
            int indexCurrAvgEnd = 0;
            int sampleToAvg = 0;
            // int measureCount = 0;
            

            try
            {
                //error = mySmu.Measure(Channel_Number, nidcpowerConstants.MeasureCurrent, out tempResult);
                //Result = Convert.ToSingle(tempResult);

                //bool resultBool = false;
                //mySmu.QueryInCompliance("0", out resultBool);

                mySmu.Abort();

                if (false)
                {

                    mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                    mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 20);
                    //int result = mySmu.GetInt32(nidcpowerProperties.SamplesToAverage);
                    mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, 200);
                    // int result = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                    NumberOfTrace = 100;

                }
                else
                {

                    if (modulationMode == "TDSCDMA")
                    {                        
                        sampleToAvg = 1;
                        indexCurrAvgStart = 5;
                        indexCurrAvgEnd = 137;
                        NumberOfTrace = 140;                                                
                    }
                    else if (modulationMode == "LTETDD")
                    {
                        sampleToAvg = 1;
                        indexCurrAvgStart = 1;
                        indexCurrAvgEnd = 450;
                        NumberOfTrace = 500;                                                
                    }

                    mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                    mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, sampleToAvg);
                                        
                    mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NumberOfTrace);

                    mySmu.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                    error = mySmu.ConfigureDigitalEdgeMeasureTrigger("/SMU01/PXI_Trig0", nidcpowerConstants.Rising);
                    mySmu.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "/SMU01/PXI_Trig0");
                    
                }


                for (int i = 0; i < measureCount; i++)
                {

                    mySmu.Initiate();

                    double[] volt = new double[NumberOfTrace];
                    double[] curr = new double[NumberOfTrace];
                    double[] curr2 = new double[NumberOfTrace];
                    double[] curr3 = new double[NumberOfTrace];
                    ushort[] measComp = new ushort[NumberOfTrace];
                    int actCount = 0;



                    int fetchBackLog = mySmu.GetInt32(nidcpowerProperties.FetchBacklog);

                    if (fetchBackLog != 0)
                    {
                        double[] voltTemp = new double[fetchBackLog];
                        double[] currTemp = new double[fetchBackLog];
                        ushort[] measCompTemp = new ushort[fetchBackLog];
                        error = mySmu.FetchMultiple(Channel_Number, 5, fetchBackLog, voltTemp, currTemp, measCompTemp, out actCount);
                    }

                    try
                    {
                        error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);
                        
                        mySmu.Abort();

                        int measCompTrigger = 0;

                        foreach (int measCompValue in measComp)
                            if (measCompValue == 1)
                                measCompTrigger = 1;

                        if (measCompTrigger == 1)
                            Thread.Sleep(1);
                    }
                    catch
                    {
                        Thread.Sleep(1);
                        // error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);
                    }
                    // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                    double tempResultSum = 0;

                    for (int i2 = indexCurrAvgStart; i2 <= indexCurrAvgEnd; i2++)
                    {
                        tempResultSum += curr[i2];
                    }

                    Result = Convert.ToSingle(tempResultSum / (indexCurrAvgEnd - indexCurrAvgStart + 1));
                    tempResultTotal += Result;
                }

                Result = Convert.ToSingle( tempResultTotal / measureCount);

                if (Result == float.NaN)
                    Result = 0;

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrentTrace");
                return -1;
            }
        }

        public int MeasureCurrentTraceDC(string Channel_Number, int NumberOfTrace, float currentLimit, ref float Result)
        {
            int error = -1;
            double tempResult = 0;

            try
            {
                //error = mySmu.Measure(Channel_Number, nidcpowerConstants.MeasureCurrent, out tempResult);
                //Result = Convert.ToSingle(tempResult);

                //bool resultBool = false;
                //mySmu.QueryInCompliance("0", out resultBool);

                mySmu.Abort();
                mySmu.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 500);
                //int result = mySmu.GetInt32(nidcpowerProperties.SamplesToAverage);
                mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, 10);
                // int result = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                NumberOfTrace = 10;
                mySmu.Initiate();

                double[] volt = new double[NumberOfTrace];
                double[] curr = new double[NumberOfTrace];
                ushort[] measComp = new ushort[NumberOfTrace];
                int actCount = 0;

                //error = mySmu.MeasureMultiple("0", volt, curr);
                // error = mySmu.Measure("0", nidcpowerConstants.MeasureVoltage, 
                // error = my

                // mySmu.Initiate();

                int fetchBackLog = mySmu.GetInt32(nidcpowerProperties.FetchBacklog);

                if (fetchBackLog != 0)
                {
                    double[] voltTemp = new double[fetchBackLog];
                    double[] currTemp = new double[fetchBackLog];
                    ushort[] measCompTemp = new ushort[fetchBackLog];
                    error = mySmu.FetchMultiple(Channel_Number, 5, fetchBackLog, voltTemp, currTemp, measCompTemp, out actCount);
                }

                // error = mySmu.ConfigureMeasPoint(NumberOfTrace);




                
                    // int tempInt = mySmu.GetInt32(nidcpowerProperties.MeasureRecordLength);
                

                // ATFCrossDomainWrapper.StoreIntToCache("TraceLength", 100);

                
                    //mySmu.Abort();
                    //mySmu.SetDouble(nidcpowerProperties.SourceDelay, Channel_Number, 0.00003);
                    //mySmu.SetInt32(nidcpowerProperties.SamplesToAverage, 20);
                    //error = mySmu.ConfigureCurrentLimit(Channel_Number, nidcpowerConstants.CurrentRegulate, currentLimit);
                    //mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NumberOfTrace);
                    //mySmu.Initiate();
                                
                error = mySmu.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                //mySmu.Abort();
                //mySmu.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                //error = mySmu.ConfigureDigitalEdgeMeasureTrigger("/SMU01/PXI_Trig0", nidcpowerConstants.Rising);
                //mySmu.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "/SMU01/PXI_Trig0");
                //mySmu.Initiate();

                
                Thread.Sleep(5);    // ???
                try
                {
                    error = mySmu.FetchMultiple(Channel_Number, 5, NumberOfTrace, volt, curr, measComp, out actCount);

                    int measCompTrigger = 0;

                    foreach (int measCompValue in measComp)
                        if (measCompValue == 1)
                            measCompTrigger = 1;

                    if (measCompTrigger == 1)
                        Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "FetchMultiple");
                }
                // mySmu.MeasureMultiple(Channel_Number, volt, curr);

                foreach (ushort i in measComp)
                {
                    //if (i == 1)
                    //    MessageBox.Show("Current compliance");
                }

                double tempResultSum = 0;
                double tempMax = 0;
                int countNo = 0;

                foreach (double t1 in curr)
                {
                    if (t1.ToString() == "NaN")
                        continue;                                            

                    tempResultSum = tempResultSum + t1;
                    countNo++;
                }

                Result = Convert.ToSingle(tempResultSum / NumberOfTrace);

                if (Result.ToString() == "NaN")
                    Thread.Sleep(1);



                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrentTrace");
                return -1;
            }
        }

        public int Output(string Channel, bool State)
        {
            try
            {
                mySmu.ConfigureOutputEnabled(Channel, State);               

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int ConfigureMeasPoint(int NoOfPoint)
        {

            // Thread.Sleep(1);

            mySmu.SetInt32(nidcpowerProperties.MeasureRecordLength, NoOfPoint);
            // ATFCrossDomainWrapper.StoreStringToCache("MOD", ModulationMode);
            

            // mySmu.MeasureMultiple("0", 


            return 0;
        }

        public int SetApertureTime(string Channel_Number, double time)
        {
            try
            {
                mySmu.SetDouble(nidcpowerProperties.ApertureTime, Channel_Number, time);
                double tempTime = mySmu.GetDouble(nidcpowerProperties.ApertureTime, "0");
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetAtertureTime");
                return -1;
            }
        }
        
        public int SetPowerLineFrequency(int frequency)
        {
            try
            {
                if (frequency == 50)                
                    mySmu.SetDouble(nidcpowerProperties.PowerLineFrequency, "0-3", nidcpowerConstants._50Hertz);
                else if (frequency == 60)
                    mySmu.SetDouble(nidcpowerProperties.PowerLineFrequency, "0-3", nidcpowerConstants._50Hertz);
                else
                    MessageBox.Show("The power line frequency is not supported.", "SetPowerLineFrequency");

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetPowerLineFrequency");
                return -1;
            }
        }

        public int SetTransientResponseFast(string Channel_Number, bool State   )
        {
            try
            {
                if (State)
                {
                    //int tempInt = mySmu.GetInt32(nidcpowerProperties.TransientResponse, Channel_Number);
                    //tempInt = nidcpowerConstants.Fast;
                    mySmu.SetInt32(nidcpowerProperties.TransientResponse, Channel_Number, nidcpowerConstants.Fast);
                    // int tempInt = mySmu.GetInt32(nidcpowerProperties.TransientResponse, Channel_Number);
                }
                else
                    mySmu.SetInt32(nidcpowerProperties.TransientResponse, Channel_Number, nidcpowerConstants.Normal);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetAtertureTime");
                return -1;
            }
        }

        public int SourceDelay(string Channel_Number, double delay)
        {
            try
            {
                // double tempSourceDelay = mySmu.GetDouble(nidcpowerProperties.SourceDelay, "0");
                mySmu.SetDouble(nidcpowerProperties.SourceDelay, Channel_Number, delay);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SourceDelay");
                return -1;
            }
        }

        public int QueryInCompliance(string Channel_Number, bool inCompliance)
        {
            try
            {
                mySmu.QueryInCompliance(Channel_Number, out inCompliance);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "QueryInCompliance");
                return -1;
            }
        }
    }
}
