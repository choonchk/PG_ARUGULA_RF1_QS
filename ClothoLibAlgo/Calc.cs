using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using IqWaveform;
using Ionic.Zip;
using fftwN;


using NationalInstruments.ModularInstruments.Interop;

namespace ClothoLibAlgo
{
    public class AclrResults
    {
        public double centerChannelPower = -999;
        public List<AdjCh> adjacentChanPowers = new List<AdjCh>();
    }

    public class SPAR_Correction
    {
        public const double pi = 3.14159265359;

        public struct Real_Imag
        {
            public double Real;
            public double Imag;
        }

        public struct Mag_Angle
        {
            public double Mag;
            public double Angle;
        }

        public Real_Imag conv_MagAngle_to_RealImag(Mag_Angle Z1)
        {
            Real_Imag tmp = new Real_Imag();
            tmp.Real = Z1.Mag * Math.Cos(Z1.Angle * (Math.PI / 180));
            tmp.Imag = Z1.Mag * Math.Sin(Z1.Angle * (Math.PI / 180));
            return (tmp);
        }
    }

    public class FastEVMAnalysis // [BurhanEVM]
    {
        private const double pi = 3.14159265359;

        // Input
        public int Record_Number_Total = 5; // This number will affect the test speed

        public int Input_Pilot_Length = 50;
        public int Input_First_Header_Search_Guess_Point = 10;
        public int Input_First_Header_Search_Window_Width_NOP = 10;
        public double Avoid_Region_Percentage = 50;
        public int BlockSplit_Number = 20; // Number of block to use

        public int MicroTune_Cycle = 1;
        public int EVM_Sampling_Percentage = 10;

        public double Angle_Search_Mag_Limit_Multiplier = 0.3; // limiter value to make decision and calculation for angle offset

        public bool EVM_ACLR_Perform_MicroTune = true; // Turn on or off micro tuning process

        public double Waveform_SamplingRate_Hz = 0;
        public double Waveform_Bandwidth_Cutoff_Hz = 0;
        public bool LowPassFilter_Activate = false;

        public bool NOP_Multiplierx2_Activate = false;
        public double NOP_Multiplier_Value = 1;

        public double IQ_Min_Limit_Percentage = 20;
        public double IQ_Max_Limit_Percentage = 60;

        public double Block_Averaging_Number = 3;

        public string IQ_Capture_Filename = "Test1";

        private double[,] OriIQData = new double[3, 2500];
        private double[,] RefIQx = new double[3, 2500];
        private int RefIQxNOP = 0;
        private int OriIQDataNum = 0;
        private int OriIQDataNOP = 0;

        public FastEVMAnalysis()
        {
            Record_Number_Total = 1; // This number will affect the test speed

            Input_Pilot_Length = 150;
            Input_First_Header_Search_Guess_Point = 10;
            Input_First_Header_Search_Window_Width_NOP = 10;

            EVM_Iteration_Value = 2;
        }

        public double FastEVM_Calculate_Method2(niComplexNumber[,] iqTraceData, double[,] OriIQData, int OriIQDataNum, int OriIQDataNOP, double[,] RefIQx, int RefIQxNOP)
        {
            this.OriIQData = OriIQData;
            this.OriIQDataNum = OriIQDataNum;
            this.OriIQDataNOP = OriIQDataNOP;
            this.RefIQx = RefIQx;
            this.RefIQxNOP = RefIQxNOP;

            bool Successful_1 = false;
            string TempReturn_BlockSplit = "";

            double[] BlockSplit_MinValue = new double[BlockSplit_Number];

            double[,] BlockSplit_FewValue = new double[BlockSplit_Number, Record_Number_Total];
            double[] BlockSplit_TempValue = new double[Record_Number_Total];

            for (int BlockSplit_Counter = 0; BlockSplit_Counter < BlockSplit_Number; BlockSplit_Counter++)
            {
                BlockSplit_MinValue[BlockSplit_Counter] = 9e9; // Fill in with very large number
            }

            string[] tmp_split2;

            // Use IQ block method in this case
            for (int Record_Number_Counter = 0; Record_Number_Counter < Record_Number_Total; Record_Number_Counter++)
            {
                Successful_1 = EVM_Generate_Measured_IQ_PwrSearch_Ver4p1(0, Input_Pilot_Length, Input_First_Header_Search_Guess_Point, Input_First_Header_Search_Window_Width_NOP, Record_Number_Counter, iqTraceData);
                TempReturn_BlockSplit = EVM_Extract_BlockSplit_ReturnResult(BlockSplit_Number);
                tmp_split2 = TempReturn_BlockSplit.Split(',').ToArray();

                for (int BlockSplit_Counter = 0; BlockSplit_Counter < BlockSplit_Number; BlockSplit_Counter++)
                {
                    BlockSplit_FewValue[BlockSplit_Counter, Record_Number_Counter] = Convert.ToDouble(tmp_split2[BlockSplit_Counter]);
                }
            }

            if (Block_Averaging_Number < 1) Block_Averaging_Number = 1;
            if (Block_Averaging_Number > Record_Number_Total) Block_Averaging_Number = Record_Number_Total;

            for (int BlockSplit_Counter = 0; BlockSplit_Counter < BlockSplit_Number; BlockSplit_Counter++)
            {
                for (int Record_Number_Counter = 0; Record_Number_Counter < Record_Number_Total; Record_Number_Counter++)
                {
                    BlockSplit_TempValue[Record_Number_Counter] = BlockSplit_FewValue[BlockSplit_Counter, Record_Number_Counter];

                    if (Double.IsNaN(BlockSplit_TempValue[Record_Number_Counter]))
                    {
                        BlockSplit_TempValue[Record_Number_Counter] = 9e9;
                    }
                }

                Array.Sort(BlockSplit_TempValue);

                double Temp_Calc = 0;

                for (int Final_Counter = 0; Final_Counter < Block_Averaging_Number; Final_Counter++)
                {
                    Temp_Calc = Temp_Calc + BlockSplit_TempValue[Final_Counter];
                }

                BlockSplit_MinValue[BlockSplit_Counter] = Temp_Calc / Block_Averaging_Number;
            }

            Array.Sort(BlockSplit_MinValue);

            double Temp_Calc1 = 0;
            double Temp_Counter1 = 0;
            int Avoid_Region;

            Avoid_Region = Convert.ToInt32(Convert.ToDouble(BlockSplit_Number) * (Avoid_Region_Percentage / 100));
            for (int BlockSplit_Counter = 0; BlockSplit_Counter < (BlockSplit_Number - Avoid_Region); BlockSplit_Counter++)
            {
                Temp_Calc1 = Temp_Calc1 + BlockSplit_MinValue[BlockSplit_Counter];
                Temp_Counter1 = Temp_Counter1 + 1;
            }

            FastEVM_Result = Temp_Calc1 / Temp_Counter1;

            return (FastEVM_Result);
        }


        public double FastEVM_Calculate(niComplexNumber[,] iqTraceData, double[,] OriIQData, int OriIQDataNum, int OriIQDataNOP, double[,] RefIQx, int RefIQxNOP)
        {
            double Final_EVM_Number = 9e9;
            int Final_Record_Selected = 0;

            this.OriIQData = OriIQData;
            this.OriIQDataNum = OriIQDataNum;
            this.OriIQDataNOP = OriIQDataNOP;
            this.RefIQx = RefIQx;
            this.RefIQxNOP = RefIQxNOP;

            bool Successful_1 = false;
            double Start_Location_Percentage = 10;
            string Temp_1 = "";

            if (true)
            {
                // Make a fast decision which IQ trace to use ... point to reduce processing as much as possible
                // Run thru a very short EVM run and decide which trace from this short evm result
                for (int Record_Number_Counter = 0; Record_Number_Counter < Record_Number_Total; Record_Number_Counter++)
                {
                    Successful_1 = EVM_Generate_Measured_IQ_PwrSearch_Ver4p1(0, Input_Pilot_Length, Input_First_Header_Search_Guess_Point, Input_First_Header_Search_Window_Width_NOP, Record_Number_Counter,
                        iqTraceData);

                    EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(Start_Location_Percentage, EVM_Sampling_Percentage); // Set sampling at 20% of the total IQ length

                    if (Final_EVM_Number > FastEVM_Result)
                    {
                        Final_EVM_Number = FastEVM_Result;
                        Final_Record_Selected = Record_Number_Counter;
                    }
                    //Console.WriteLine("EVM_Smpl_" + Record_Number_Counter.ToString().Trim() + "=" + FastEVM_Result.ToString());
                    Temp_1 = Temp_1 + FastEVM_Result.ToString() + ",";
                }

                Successful_1 = EVM_Generate_Measured_IQ_PwrSearch_Ver4p1(0, Input_Pilot_Length, Input_First_Header_Search_Guess_Point, Input_First_Header_Search_Window_Width_NOP, Final_Record_Selected,
                    iqTraceData);
                //EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(0,100);
                EVM_Extract_BlockSplit(5);

                Temp_1 = Temp_1 + Final_EVM_Number.ToString().Trim() + "," + FastEVM_Result.ToString();
                //Console.WriteLine("EVM_Smpl=>," + Temp_1.Trim());
            }
            else
            {
                FastEVM_Result = 999;

                Successful_1 = EVM_Generate_Measured_IQ_PwrSearch_Ver4p1(0, Input_Pilot_Length, Input_First_Header_Search_Guess_Point, Input_First_Header_Search_Window_Width_NOP, Record_Number_Total - 1,
                    iqTraceData);
                //EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p5_Spd_ClothoTransfered(100);
                EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(100);
                //EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(20);
            }

            return (FastEVM_Result);
        }

        public double Get_FastEVM_Result()
        {
            return (FastEVM_Result);
        }

        private double Test_Header_Angle_Upper;
        private double Test_Header_Angle_Middle;
        private double Test_Header_Angle_Lower;

        private double Delta_Angle = 9e99;

        private int Pilot_Temp_NOP;

        private double FastEVM_Result;

        private double[,] Measured_Output;

        private SPAR_Correction SPAR_Correction_Local = new SPAR_Correction(); // Needed for Thru model S2P calculation 

        public int EVM_Iteration_Value = 2;

        public int IQ_Averaging_Level = 3;

        private string EVM_Extract_BlockSplit_ReturnResult(int BlockSplit)
        {
            int NumberOfPoint_Ref_IQ_temp = 100 / BlockSplit;
            double EVM_Capture = 0;
            string Temp_1 = "";

            for (int j = 0; j < (BlockSplit); j++)
            {
                // Larger than array filtering done in this function
                EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(j * NumberOfPoint_Ref_IQ_temp, NumberOfPoint_Ref_IQ_temp);
                EVM_Capture = EVM_Capture + FastEVM_Result;
                Temp_1 = Temp_1 + FastEVM_Result.ToString() + ",";
            }

            FastEVM_Result = EVM_Capture / Convert.ToDouble(BlockSplit);
            return (Temp_1.Trim());
        }

        private void EVM_Extract_BlockSplit(int BlockSplit)
        {
            int NumberOfPoint_Ref_IQ_temp = 100 / BlockSplit;
            double EVM_Capture = 0;
            string Temp_1 = "";

            for (int j = 0; j < (BlockSplit); j++)
            {
                // Larger than array filtering done in this function
                EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(j * NumberOfPoint_Ref_IQ_temp, NumberOfPoint_Ref_IQ_temp);
                EVM_Capture = EVM_Capture + FastEVM_Result;
                Temp_1 = Temp_1 + FastEVM_Result.ToString() + ",";

            }

            FastEVM_Result = EVM_Capture / Convert.ToDouble(BlockSplit);

            Temp_1 = Temp_1 + FastEVM_Result.ToString();

            Console.WriteLine("EVM_Smpl=>," + Temp_1.Trim());
        }

        private void EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(double Start_Location_Percentage, double IQ_Length_Percentage)
        {
            double[,] tmp_Ref_IQx = RefIQx;
            int NumberOfPoint_Ref_IQ = RefIQxNOP;

            int Frame_NOP;
            int NumberOfPoint_Measurement_IQ = Convert.ToInt32((Measured_Output.Length / 3) * (IQ_Length_Percentage / 100));
            int NumberOfPoint_Ref_IQ_temp = Convert.ToInt32(NumberOfPoint_Ref_IQ * (IQ_Length_Percentage / 100));
            int StartPoint_Ref_IQ_temp = Convert.ToInt32(NumberOfPoint_Ref_IQ * (Start_Location_Percentage / 100));

            if (StartPoint_Ref_IQ_temp < 0) StartPoint_Ref_IQ_temp = 0; // Make sure the start point larger than zero

            // If the end point larger than the array then adjust the Start Point accordingly
            if ((NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp) > (NumberOfPoint_Ref_IQ))
            {
                StartPoint_Ref_IQ_temp = (NumberOfPoint_Ref_IQ) - NumberOfPoint_Ref_IQ_temp;
            }

            if (false) // For debugging only
            {
                string[] tmp_string6 = new string[NumberOfPoint_Measurement_IQ];

                for (int i = 0; i < NumberOfPoint_Measurement_IQ; i++)
                {
                    tmp_string6[i] = Convert.ToString(Measured_Output[0, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[1, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[2, i]).Trim();
                }
                File.WriteAllLines("c:\\temp\\measured_debug.csv", tmp_string6);
            }

            // Make sure the NOP for both file is the same
            if (Convert.ToInt32(NumberOfPoint_Ref_IQ_temp) != NumberOfPoint_Measurement_IQ)
            {
                MessageBox.Show("Ref_IQ and Measured_IQ having different NOP !!!", "Error", MessageBoxButtons.OK);
            }
            else
            {
                Delta_Angle = 9e99;

                //Frame_NOP = NumberOfPoint_Measurement_IQ;
                Frame_NOP = NumberOfPoint_Ref_IQ_temp;

                // Calculate parameters
                double[] xValues_I = new double[Frame_NOP];
                double[] yValues_I = new double[Frame_NOP];
                double[] xValues_Q = new double[Frame_NOP];
                double[] yValues_Q = new double[Frame_NOP];

                Pilot_Temp_NOP = NumberOfPoint_Ref_IQ_temp;

                int k = 0;
                int IQ_Sign_Multiplier = 1;

                // Decide if need to make 180 Deg angle correction 
                for (int j = (StartPoint_Ref_IQ_temp); j < NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp; j++)
                {
                    // make sure high enough magnitude for decision making
                    if ((Math.Sqrt(Math.Pow(tmp_Ref_IQx[0, j], 2) + Math.Pow(tmp_Ref_IQx[1, j], 2))) > 0.3)
                    {
                        if ((tmp_Ref_IQx[0, j] > 0) && (Measured_Output[0, j] < 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }

                        if ((tmp_Ref_IQx[0, j] < 0) && (Measured_Output[0, j] > 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }
                        break;
                    }
                }

                // Use averaging method (as low pass filtering)
                for (int j = (StartPoint_Ref_IQ_temp); j < (NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp); j++)
                {
                    // Transfer data into EVM array
                    if (((j == (0 + StartPoint_Ref_IQ_temp)) || (j == ((NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp) - 1))) || (IQ_Averaging_Level <= 1))
                    {
                        xValues_I[k] = tmp_Ref_IQx[0, j];
                        yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        xValues_Q[k] = tmp_Ref_IQx[1, j];
                        yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }

                    else if (((j == (1 + StartPoint_Ref_IQ_temp)) || (j == ((NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp) - 2))) || (IQ_Averaging_Level <= 3))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1]) / 3;
                        yValues_I[k] = ((Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1]) / 3) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1]) / 3;
                        yValues_Q[k] = ((Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1]) / 3) * IQ_Sign_Multiplier;
                    }

                    else if (((j == (2 + StartPoint_Ref_IQ_temp)) || (j == ((NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp) - 3))) || (IQ_Averaging_Level <= 5))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 2] + tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1] + tmp_Ref_IQx[0, j + 2]) / 5;
                        yValues_I[k] = ((Measured_Output[0, j - 2] + Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1] + Measured_Output[0, j + 2]) / 5) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 2] + tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1] + tmp_Ref_IQx[1, j + 2]) / 5;
                        yValues_Q[k] = ((Measured_Output[1, j - 2] + Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1] + Measured_Output[1, j + 2]) / 5) * IQ_Sign_Multiplier;
                    }

                    else if (((j == (3 + StartPoint_Ref_IQ_temp)) || (j == ((NumberOfPoint_Ref_IQ_temp + StartPoint_Ref_IQ_temp) - 4))) || (IQ_Averaging_Level <= 7))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 3] + tmp_Ref_IQx[0, j - 2] + tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1] + tmp_Ref_IQx[0, j + 2] + tmp_Ref_IQx[0, j + 3]) / 7;
                        yValues_I[k] = ((Measured_Output[0, j - 3] + Measured_Output[0, j - 2] + Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1] + Measured_Output[0, j + 2] + Measured_Output[0, j + 3]) / 7) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 3] + tmp_Ref_IQx[1, j - 2] + tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1] + tmp_Ref_IQx[1, j + 2] + tmp_Ref_IQx[1, j + 3]) / 7;
                        yValues_Q[k] = ((Measured_Output[1, j - 3] + Measured_Output[1, j - 2] + Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1] + Measured_Output[1, j + 2] + Measured_Output[1, j + 3]) / 7) * IQ_Sign_Multiplier;
                    }

                    else // This will be an error and need warning message
                    {
                        xValues_I[k] = tmp_Ref_IQx[0, j];
                        yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        xValues_Q[k] = tmp_Ref_IQx[1, j];
                        yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }

                    k += 1;
                }

                if (NOP_Multiplierx2_Activate == true)
                {
                    for (int d = 0; d < NOP_Multiplier_Value; d++)
                    {
                        int UpSample_Multiplier = 2;

                        // Transfer array data to temp array
                        double[] xValues_I_Temp = new double[xValues_I.Length];
                        double[] yValues_I_Temp = new double[xValues_I.Length];
                        double[] xValues_Q_Temp = new double[xValues_I.Length];
                        double[] yValues_Q_Temp = new double[xValues_I.Length];

                        for (int j = 0; j < xValues_I_Temp.Length; j++)
                        {
                            xValues_I_Temp[j] = xValues_I[j];
                            yValues_I_Temp[j] = yValues_I[j];
                            xValues_Q_Temp[j] = xValues_Q[j];
                            yValues_Q_Temp[j] = yValues_Q[j];
                        }

                        // Re-dimension arrays
                        Array.Resize(ref xValues_I, xValues_I_Temp.Length * UpSample_Multiplier);
                        Array.Resize(ref yValues_I, xValues_I_Temp.Length * UpSample_Multiplier);
                        Array.Resize(ref xValues_Q, xValues_I_Temp.Length * UpSample_Multiplier);
                        Array.Resize(ref yValues_Q, xValues_I_Temp.Length * UpSample_Multiplier);

                        // Insert zeros based on x time conversion
                        for (int j = 0; j < xValues_I_Temp.Length * UpSample_Multiplier; j++)
                        {
                            if ((j % UpSample_Multiplier) == 0)
                            {
                                xValues_I[j] = xValues_I_Temp[j / UpSample_Multiplier];
                                yValues_I[j] = yValues_I_Temp[j / UpSample_Multiplier];
                                xValues_Q[j] = xValues_Q_Temp[j / UpSample_Multiplier];
                                yValues_Q[j] = yValues_Q_Temp[j / UpSample_Multiplier];
                            }
                            else
                            {
                                xValues_I[j] = 0;
                                yValues_I[j] = 0;
                                xValues_Q[j] = 0;
                                yValues_Q[j] = 0;
                            }
                        }

                        Frame_NOP = xValues_I_Temp.Length * UpSample_Multiplier;
                        Pilot_Temp_NOP = xValues_I_Temp.Length * UpSample_Multiplier;

                        // Run through low pass butterworth filtering
                        xValues_I = Butterworth(xValues_I, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                        yValues_I = Butterworth(yValues_I, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                        xValues_Q = Butterworth(xValues_Q, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                        yValues_Q = Butterworth(yValues_Q, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                    }
                }

                if (LowPassFilter_Activate == true && NOP_Multiplierx2_Activate == false)
                {
                    // Run through low pass butterworth filtering
                    xValues_I = Butterworth(xValues_I, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                    yValues_I = Butterworth(yValues_I, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                    xValues_Q = Butterworth(xValues_Q, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                    yValues_Q = Butterworth(yValues_Q, 1 / Waveform_SamplingRate_Hz, Waveform_Bandwidth_Cutoff_Hz);
                }

                double IQ_Peak_Value = -9e9;
                double Temp_IQ_Value = 0;

                for (int j = 0; j < xValues_I.Length; j++) // Search for the Max Peak value
                {
                    Temp_IQ_Value = Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                    if (IQ_Peak_Value < Temp_IQ_Value) IQ_Peak_Value = Temp_IQ_Value;
                }

                int Iteration_Value = EVM_Iteration_Value;

                for (int v = 0; v < Iteration_Value; v++) // Iterate phase few times
                {

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 1. Convert from Polar to Vector (Both Full Wfm and Pilot Wfm)
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Total_Angle_Offset = 0;

                    double[] Ref_Angle = new double[xValues_I.Length];
                    double[] Measured_Angle = new double[xValues_I.Length];

                    double[] Ref_Mag = new double[xValues_I.Length];
                    double[] Measured_Mag = new double[xValues_I.Length];

                    double Maximum_Ref_Mag = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        // Phase conversion
                        Ref_Angle[j] = Math.Atan2(xValues_Q[j], xValues_I[j]) * (180 / Math.PI);
                        Measured_Angle[j] = Math.Atan2(yValues_Q[j], yValues_I[j]) * (180 / Math.PI);

                        // Magnitude conversion
                        Ref_Mag[j] = Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                        Measured_Mag[j] = Math.Sqrt(Math.Pow(yValues_I[j], 2) + Math.Pow(yValues_Q[j], 2));

                        if (Maximum_Ref_Mag < Ref_Mag[j])
                        {
                            Maximum_Ref_Mag = Ref_Mag[j];
                        }
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 2. Find Phase offset between Ref and Measured Phase() => Search offset from Pilot and apply to Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Max_Angle = -9e9;
                    double Min_Angle = 9e9;

                    // Use pilot signal to extract phase offset
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Delta_Angle = 9e99;

                        Test_Header_Angle_Upper = Measured_Angle[j] + 360;
                        Test_Header_Angle_Middle = Measured_Angle[j];
                        Test_Header_Angle_Lower = Measured_Angle[j] - 360;

                        if (Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Upper;
                        }

                        if (Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Middle;
                        }

                        if (Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Lower;
                        }
                    }

                    Total_Angle_Offset = 0;
                    int Angle_Counter = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        if (Ref_Mag[j] > (Maximum_Ref_Mag * Angle_Search_Mag_Limit_Multiplier))
                        {
                            Total_Angle_Offset = Total_Angle_Offset + (Ref_Angle[j] - Measured_Angle[j]);
                            Angle_Counter = Angle_Counter + 1;
                        }
                    }

                    Total_Angle_Offset = Total_Angle_Offset / Angle_Counter;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Angle[j] = Measured_Angle[j] + Total_Angle_Offset;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 3. Perform linear regression for Mag() => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    // Perform linear regression using pilot signal
                    double rSquared_Mag, intercept_Mag, slope_Mag;

                    LinearRegression(Measured_Mag, Ref_Mag, out rSquared_Mag, out intercept_Mag, out slope_Mag);

                    // Apply the coef to the full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Mag[j] = (Measured_Mag[j] * slope_Mag) + intercept_Mag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 4. Convert back to Polar from Vector ( Both Full Wfm and Pilot Wfm )
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    SPAR_Correction.Real_Imag Var_One_B;
                    SPAR_Correction.Mag_Angle Var_Two_B;

                    // Apply for full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Var_Two_B.Angle = Measured_Angle[j];
                        Var_Two_B.Mag = Measured_Mag[j];
                        Var_One_B = SPAR_Correction_Local.conv_MagAngle_to_RealImag(Var_Two_B);
                        yValues_I[j] = Var_One_B.Real;
                        yValues_Q[j] = Var_One_B.Imag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 5. Perform micro-tune for both R and I independently => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double[] x_offset_I_Value = new double[Pilot_Temp_NOP];
                    double[] x_offset_Q_Value = new double[Pilot_Temp_NOP];

                    double c;
                    double b;
                    double a;

                    double x_offset_Pos = 0;
                    double x_offset_Neg = 0;

                    // Apply on the full waveform
                    double[] meas_pilotValues_I_Temp = new double[xValues_I.Length];
                    double[] meas_pilotValues_Q_Temp = new double[xValues_I.Length];

                    //bool EVM_ACLR_Perform_MicroTune = true;

                    if (EVM_ACLR_Perform_MicroTune == true)
                    {
                        for (int z = 0; z < MicroTune_Cycle; z++)
                        {
                            // *************************
                            // Start X_Offset extraction
                            // *************************
                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                // ***************
                                // Calculate for I
                                // ***************

                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                //
                                x_offset_I_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Neg;
                                }

                                // ***************
                                // Calculate for Q
                                // ***************

                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                //
                                x_offset_Q_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Neg;
                                }
                            }

                            double X_Offset_Total = 0;
                            int X_Offset_Counter = 0;
                            double Y_Offset_Total = 0;
                            int Y_Offset_Counter = 0;

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                if (x_offset_I_Value[j] <= 1 && x_offset_I_Value[j] >= -1)
                                {
                                    X_Offset_Total = X_Offset_Total + x_offset_I_Value[j];
                                    X_Offset_Counter = X_Offset_Counter + 1;
                                }
                                //
                                if (x_offset_Q_Value[j] <= 1 && x_offset_Q_Value[j] >= -1)
                                {
                                    Y_Offset_Total = Y_Offset_Total + x_offset_Q_Value[j];
                                    Y_Offset_Counter = Y_Offset_Counter + 1;
                                }
                            }

                            double x_offset_Result = X_Offset_Total / Convert.ToDouble(X_Offset_Counter); // Independent Coef for I and Q !!!
                            double Y_offset_Result = Y_Offset_Total / Convert.ToDouble(Y_Offset_Counter);

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                meas_pilotValues_I_Temp[j] = (a * Math.Pow(x_offset_Result, 2)) + (b * x_offset_Result) + c;
                                //
                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                meas_pilotValues_Q_Temp[j] = (a * Math.Pow(Y_offset_Result, 2)) + (b * Y_offset_Result) + c;
                            }

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                yValues_I[j] = meas_pilotValues_I_Temp[j]; // Pass back to the original array
                                yValues_Q[j] = meas_pilotValues_Q_Temp[j];
                            }

                        }
                    }

                    if (v == Iteration_Value - 1) // Start calculating EVM result if this is the last iteration cycle
                    {
                        // %%%%%%%%%%%%%%%%
                        // 6. Calculate EVM
                        // %%%%%%%%%%%%%%%%

                        if (1 == 0) // For debugging only
                        {
                            string[] tmp_string6 = new string[Frame_NOP];

                            for (int i = 0; i < Frame_NOP; i++)
                            {
                                //tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_Q[i]).Trim();
                                tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                                 Convert.ToString(yValues_Q[i]).Trim();
                            }
                            //File.WriteAllLines("c:\\temp\\measured_debug_IQ_Compare.csv", tmp_string6);
                            File.WriteAllLines("c:\\temp\\" + IQ_Capture_Filename + ".csv", tmp_string6);
                        }

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        double IQ_Lower_Limit = 0;
                        double IQ_Upper_Limit = 0;
                        double Temp_IQ_Value2 = 0;
                        double Temp_IQ_Value3 = 0;

                        IQ_Lower_Limit = (IQ_Min_Limit_Percentage / 100.0) * IQ_Peak_Value;
                        IQ_Upper_Limit = (IQ_Max_Limit_Percentage / 100.0) * IQ_Peak_Value;

                        for (int j = 0 + (IQ_Averaging_Level + 3); j < Frame_NOP - (IQ_Averaging_Level + 3); j++) // <= Remove feq points from the trace eadge from final calculation
                        {
                            Temp_IQ_Value2 = Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2);
                            Temp_IQ_Value3 = Math.Sqrt(Temp_IQ_Value2);
                            //
                            if ((Temp_IQ_Value3 > IQ_Lower_Limit) && (Temp_IQ_Value3 < IQ_Upper_Limit))
                            {
                                Sum_Total = Sum_Total + Temp_IQ_Value2;
                                Sum_Residual = Sum_Residual + (Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                            }
                        }

                        // Extract EVM
                        FastEVM_Result = Math.Sqrt(Sum_Residual / Sum_Total) * 100;
                    }
                }
            }
        }

        private void EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered(double IQ_Length_Percentage)
        {
            double[,] tmp_Ref_IQx = RefIQx;
            int NumberOfPoint_Ref_IQ = RefIQxNOP;

            int Frame_NOP;
            int NumberOfPoint_Measurement_IQ = Convert.ToInt32((Measured_Output.Length / 3) * (IQ_Length_Percentage / 100));
            int NumberOfPoint_Ref_IQ_temp = Convert.ToInt32(NumberOfPoint_Ref_IQ * (IQ_Length_Percentage / 100));

            if (false) // For debugging only
            {
                string[] tmp_string6 = new string[NumberOfPoint_Measurement_IQ];

                for (int i = 0; i < NumberOfPoint_Measurement_IQ; i++)
                {
                    tmp_string6[i] = Convert.ToString(Measured_Output[0, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[1, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[2, i]).Trim();
                }
                File.WriteAllLines("c:\\temp\\measured_debug.csv", tmp_string6);
            }

            // Make sure the NOP for both file is the same
            if (Convert.ToInt32(NumberOfPoint_Ref_IQ_temp) != NumberOfPoint_Measurement_IQ)
            {
                MessageBox.Show("Ref_IQ and Measured_IQ having different NOP !!!", "Error", MessageBoxButtons.OK);
            }
            else
            {
                Delta_Angle = 9e99;

                //Frame_NOP = NumberOfPoint_Measurement_IQ;
                Frame_NOP = NumberOfPoint_Ref_IQ_temp;

                // Calculate parameters
                double[] xValues_I = new double[Frame_NOP];
                double[] yValues_I = new double[Frame_NOP];
                double[] xValues_Q = new double[Frame_NOP];
                double[] yValues_Q = new double[Frame_NOP];

                Pilot_Temp_NOP = NumberOfPoint_Ref_IQ_temp;

                int k = 0;
                int IQ_Sign_Multiplier = 1;

                // Decide if need to make 180 Deg angle correction 
                for (int j = (NumberOfPoint_Ref_IQ_temp - Frame_NOP); j < NumberOfPoint_Ref_IQ_temp; j++)
                {
                    // make sure high enough magnitude for decision making
                    if ((Math.Sqrt(Math.Pow(tmp_Ref_IQx[0, j], 2) + Math.Pow(tmp_Ref_IQx[1, j], 2))) > 0.3)
                    {
                        if ((tmp_Ref_IQx[0, j] > 0) && (Measured_Output[0, j] < 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }

                        if ((tmp_Ref_IQx[0, j] < 0) && (Measured_Output[0, j] > 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }
                        break;
                    }
                }

                // Use averaging method (as low pass filtering)
                for (int j = (NumberOfPoint_Ref_IQ_temp - Frame_NOP); j < NumberOfPoint_Ref_IQ_temp; j++)
                {
                    // Transfer data into EVM array
                    //if ((j == (NumberOfPoint_Ref_IQ_temp - Frame_NOP)) || (j == (NumberOfPoint_Ref_IQ_temp - 1)))
                    if (((j == 0) || (j == (NumberOfPoint_Ref_IQ_temp - 1))) || (IQ_Averaging_Level <= 1))
                    {
                        xValues_I[k] = tmp_Ref_IQx[0, j];
                        yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        xValues_Q[k] = tmp_Ref_IQx[1, j];
                        yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }

                    else if (((j == (1)) || (j == (NumberOfPoint_Ref_IQ_temp - 2))) || (IQ_Averaging_Level <= 3))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1]) / 3;
                        yValues_I[k] = ((Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1]) / 3) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1]) / 3;
                        yValues_Q[k] = ((Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1]) / 3) * IQ_Sign_Multiplier;
                    }

                    else if (((j == (2)) || (j == (NumberOfPoint_Ref_IQ_temp - 3))) || (IQ_Averaging_Level <= 5))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 2] + tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1] + tmp_Ref_IQx[0, j + 2]) / 5;
                        yValues_I[k] = ((Measured_Output[0, j - 2] + Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1] + Measured_Output[0, j + 2]) / 5) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 2] + tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1] + tmp_Ref_IQx[1, j + 2]) / 5;
                        yValues_Q[k] = ((Measured_Output[1, j - 2] + Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1] + Measured_Output[1, j + 2]) / 5) * IQ_Sign_Multiplier;
                    }

                    else if (((j == (3)) || (j == (NumberOfPoint_Ref_IQ_temp - 4))) || (IQ_Averaging_Level <= 7))
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 3] + tmp_Ref_IQx[0, j - 2] + tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1] + tmp_Ref_IQx[0, j + 2] + tmp_Ref_IQx[0, j + 3]) / 7;
                        yValues_I[k] = ((Measured_Output[0, j - 3] + Measured_Output[0, j - 2] + Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1] + Measured_Output[0, j + 2] + Measured_Output[0, j + 3]) / 7) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 3] + tmp_Ref_IQx[1, j - 2] + tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1] + tmp_Ref_IQx[1, j + 2] + tmp_Ref_IQx[1, j + 3]) / 7;
                        yValues_Q[k] = ((Measured_Output[1, j - 3] + Measured_Output[1, j - 2] + Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1] + Measured_Output[1, j + 2] + Measured_Output[1, j + 3]) / 7) * IQ_Sign_Multiplier;
                    }

                    else // This will be an error and need warning message
                    {
                        xValues_I[k] = tmp_Ref_IQx[0, j];
                        yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        xValues_Q[k] = tmp_Ref_IQx[1, j];
                        yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }

                    k += 1;
                }

                int Iteration_Value = EVM_Iteration_Value;

                for (int v = 0; v < Iteration_Value; v++) // Iterate phase few times
                {

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 1. Convert from Polar to Vector (Both Full Wfm and Pilot Wfm)
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Total_Angle_Offset = 0;

                    double[] Ref_Angle = new double[xValues_I.Length];
                    double[] Measured_Angle = new double[xValues_I.Length];

                    double[] Ref_Mag = new double[xValues_I.Length];
                    double[] Measured_Mag = new double[xValues_I.Length];

                    double Maximum_Ref_Mag = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        // Phase conversion
                        Ref_Angle[j] = Math.Atan2(xValues_Q[j], xValues_I[j]) * (180 / Math.PI);
                        Measured_Angle[j] = Math.Atan2(yValues_Q[j], yValues_I[j]) * (180 / Math.PI);

                        // Magnitude conversion
                        Ref_Mag[j] = Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                        Measured_Mag[j] = Math.Sqrt(Math.Pow(yValues_I[j], 2) + Math.Pow(yValues_Q[j], 2));

                        if (Maximum_Ref_Mag < Ref_Mag[j])
                        {
                            Maximum_Ref_Mag = Ref_Mag[j];
                        }
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 2. Find Phase offset between Ref and Measured Phase() => Search offset from Pilot and apply to Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Max_Angle = -9e9;
                    double Min_Angle = 9e9;

                    // Use pilot signal to extract phase offset
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Delta_Angle = 9e99;

                        Test_Header_Angle_Upper = Measured_Angle[j] + 360;
                        Test_Header_Angle_Middle = Measured_Angle[j];
                        Test_Header_Angle_Lower = Measured_Angle[j] - 360;

                        if (Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Upper;
                        }

                        if (Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Middle;
                        }

                        if (Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Lower;
                        }
                    }

                    Total_Angle_Offset = 0;
                    int Angle_Counter = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        if (Ref_Mag[j] > (Maximum_Ref_Mag * Angle_Search_Mag_Limit_Multiplier))
                        {
                            Total_Angle_Offset = Total_Angle_Offset + (Ref_Angle[j] - Measured_Angle[j]);
                            Angle_Counter = Angle_Counter + 1;
                        }
                    }

                    Total_Angle_Offset = Total_Angle_Offset / Angle_Counter;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Angle[j] = Measured_Angle[j] + Total_Angle_Offset;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 3. Perform linear regression for Mag() => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    // Perform linear regression using pilot signal
                    double rSquared_Mag, intercept_Mag, slope_Mag;

                    LinearRegression(Measured_Mag, Ref_Mag, out rSquared_Mag, out intercept_Mag, out slope_Mag);

                    // Apply the coef to the full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Mag[j] = (Measured_Mag[j] * slope_Mag) + intercept_Mag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 4. Convert back to Polar from Vector ( Both Full Wfm and Pilot Wfm )
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    SPAR_Correction.Real_Imag Var_One_B;
                    SPAR_Correction.Mag_Angle Var_Two_B;

                    // Apply for full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Var_Two_B.Angle = Measured_Angle[j];
                        Var_Two_B.Mag = Measured_Mag[j];
                        Var_One_B = SPAR_Correction_Local.conv_MagAngle_to_RealImag(Var_Two_B);
                        yValues_I[j] = Var_One_B.Real;
                        yValues_Q[j] = Var_One_B.Imag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 5. Perform micro-tune for both R and I independently => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double[] x_offset_I_Value = new double[Pilot_Temp_NOP];
                    double[] x_offset_Q_Value = new double[Pilot_Temp_NOP];

                    double c;
                    double b;
                    double a;

                    double x_offset_Pos = 0;
                    double x_offset_Neg = 0;

                    // Apply on the full waveform
                    double[] meas_pilotValues_I_Temp = new double[xValues_I.Length];
                    double[] meas_pilotValues_Q_Temp = new double[xValues_I.Length];

                    //bool EVM_ACLR_Perform_MicroTune = true;

                    if (EVM_ACLR_Perform_MicroTune == true)
                    {
                        for (int z = 0; z < MicroTune_Cycle; z++)
                        {
                            // *************************
                            // Start X_Offset extraction
                            // *************************
                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                // ***************
                                // Calculate for I
                                // ***************

                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                //
                                x_offset_I_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Neg;
                                }

                                // ***************
                                // Calculate for Q
                                // ***************

                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                //
                                x_offset_Q_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Neg;
                                }
                            }

                            double X_Offset_Total = 0;
                            int X_Offset_Counter = 0;
                            double Y_Offset_Total = 0;
                            int Y_Offset_Counter = 0;

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                if (x_offset_I_Value[j] <= 1 && x_offset_I_Value[j] >= -1)
                                {
                                    X_Offset_Total = X_Offset_Total + x_offset_I_Value[j];
                                    X_Offset_Counter = X_Offset_Counter + 1;
                                }
                                //
                                if (x_offset_Q_Value[j] <= 1 && x_offset_Q_Value[j] >= -1)
                                {
                                    Y_Offset_Total = Y_Offset_Total + x_offset_Q_Value[j];
                                    Y_Offset_Counter = Y_Offset_Counter + 1;
                                }
                            }

                            double x_offset_Result = X_Offset_Total / Convert.ToDouble(X_Offset_Counter); // Independent Coef for I and Q !!!
                            double Y_offset_Result = Y_Offset_Total / Convert.ToDouble(Y_Offset_Counter);

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                meas_pilotValues_I_Temp[j] = (a * Math.Pow(x_offset_Result, 2)) + (b * x_offset_Result) + c;
                                //
                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                meas_pilotValues_Q_Temp[j] = (a * Math.Pow(Y_offset_Result, 2)) + (b * Y_offset_Result) + c;
                            }

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                yValues_I[j] = meas_pilotValues_I_Temp[j]; // Pass back to the original array
                                yValues_Q[j] = meas_pilotValues_Q_Temp[j];
                            }

                        }
                    }

                    if (v == Iteration_Value - 1) // Start calculating EVM result if this is the last iteration cycle
                    {
                        // %%%%%%%%%%%%%%%%
                        // 6. Calculate EVM
                        // %%%%%%%%%%%%%%%%

                        if (false) // For debugging only
                        {
                            string[] tmp_string6 = new string[Frame_NOP];

                            for (int i = 0; i < Frame_NOP; i++)
                            {
                                //tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_Q[i]).Trim();
                                tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                                 Convert.ToString(yValues_Q[i]).Trim();
                            }
                            File.WriteAllLines("c:\\temp\\measured_debug_IQ_Compare.csv", tmp_string6);
                        }

                        /*

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        for (int j = 0 + 1; j < Frame_NOP - 1; j++)
                        {
                            Sum_Total = Sum_Total + Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + Math.Sqrt(Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = (Sum_Residual / Sum_Total) * 100;

                        */

                        /*

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        double Sum_Total2 = 0;
                        double Sum_Residual2 = 0;

                        //for (int j = 0 + 1; j < Frame_NOP - 1; j++)
                        for (int j = 0 + 3; j < Frame_NOP - 3; j++) // <= Remove feq points from the trace eadge from final calculation
                        {
                            Sum_Total = Sum_Total + Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + Math.Sqrt(Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                            Sum_Total2 = Sum_Total2 + (Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual2 = Sum_Residual2 + (Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = (Sum_Residual / Sum_Total) * 100;
                        FastEVM_Result2 = Math.Sqrt(Sum_Residual2 / Sum_Total2) * 100;

                        */

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        //IQ_Averaging_Level

                        //for (int j = 0 + 3; j < Frame_NOP - 3; j++) // <= Remove feq points from the trace eadge from final calculation
                        for (int j = 0 + IQ_Averaging_Level; j < Frame_NOP - IQ_Averaging_Level; j++) // <= Remove feq points from the trace eadge from final calculation
                        {
                            Sum_Total = Sum_Total + (Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + (Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = Math.Sqrt(Sum_Residual / Sum_Total) * 100;
                    }
                }
            }
        }

        private void EVM_Extract_EVM_ACLR_From_Train_Signal_Ver4p6_ClothoTransfered_v1(double IQ_Length_Percentage)
        {
            double[,] tmp_Ref_IQx = RefIQx;
            int NumberOfPoint_Ref_IQ = RefIQxNOP;

            int Frame_NOP;
            int NumberOfPoint_Measurement_IQ = Convert.ToInt32((Measured_Output.Length / 3) * (IQ_Length_Percentage / 100));
            int NumberOfPoint_Ref_IQ_temp = Convert.ToInt32(NumberOfPoint_Ref_IQ * (IQ_Length_Percentage / 100));

            if (false) // For debugging only
            {
                string[] tmp_string6 = new string[NumberOfPoint_Measurement_IQ];

                for (int i = 0; i < NumberOfPoint_Measurement_IQ; i++)
                {
                    tmp_string6[i] = Convert.ToString(Measured_Output[0, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[1, i]).Trim() + "," +
                                     Convert.ToString(Measured_Output[2, i]).Trim();
                }
                File.WriteAllLines("c:\\temp\\measured_debug.csv", tmp_string6);
            }

            // Make sure the NOP for both file is the same
            if (Convert.ToInt32(NumberOfPoint_Ref_IQ_temp) != NumberOfPoint_Measurement_IQ)
            {
                MessageBox.Show("Ref_IQ and Measured_IQ having different NOP !!!", "Error", MessageBoxButtons.OK);
            }
            else
            {
                Delta_Angle = 9e99;

                //Frame_NOP = NumberOfPoint_Measurement_IQ;
                Frame_NOP = NumberOfPoint_Ref_IQ_temp;

                // Calculate parameters
                double[] xValues_I = new double[Frame_NOP];
                double[] yValues_I = new double[Frame_NOP];
                double[] xValues_Q = new double[Frame_NOP];
                double[] yValues_Q = new double[Frame_NOP];

                Pilot_Temp_NOP = NumberOfPoint_Ref_IQ_temp;

                int k = 0;
                int IQ_Sign_Multiplier = 1;

                // Decide if need to make 180 Deg angle correction 
                for (int j = (NumberOfPoint_Ref_IQ_temp - Frame_NOP); j < NumberOfPoint_Ref_IQ_temp; j++)
                {
                    // make sure high enough magnitude for decision making
                    if ((Math.Sqrt(Math.Pow(tmp_Ref_IQx[0, j], 2) + Math.Pow(tmp_Ref_IQx[1, j], 2))) > 0.3)
                    {
                        if ((tmp_Ref_IQx[0, j] > 0) && (Measured_Output[0, j] < 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }

                        if ((tmp_Ref_IQx[0, j] < 0) && (Measured_Output[0, j] > 0))
                        {
                            if ((tmp_Ref_IQx[1, j] > 0) && (Measured_Output[1, j] < 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                            if ((tmp_Ref_IQx[1, j] < 0) && (Measured_Output[1, j] > 0))
                            {
                                IQ_Sign_Multiplier = -1;
                            }
                        }
                        break;
                    }
                }

                // Use averaging method (as low pass filtering)
                for (int j = (NumberOfPoint_Ref_IQ_temp - Frame_NOP); j < NumberOfPoint_Ref_IQ_temp; j++)
                {
                    // Transfer data into EVM array
                    if ((j == (NumberOfPoint_Ref_IQ_temp - Frame_NOP)) || (j == (NumberOfPoint_Ref_IQ_temp - 1)))
                    {
                        xValues_I[k] = tmp_Ref_IQx[0, j];
                        yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        xValues_Q[k] = tmp_Ref_IQx[1, j];
                        yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }
                    else
                    {
                        xValues_I[k] = (tmp_Ref_IQx[0, j - 1] + tmp_Ref_IQx[0, j] + tmp_Ref_IQx[0, j + 1]) / 3;
                        yValues_I[k] = ((Measured_Output[0, j - 1] + Measured_Output[0, j] + Measured_Output[0, j + 1]) / 3) * IQ_Sign_Multiplier;
                        xValues_Q[k] = (tmp_Ref_IQx[1, j - 1] + tmp_Ref_IQx[1, j] + tmp_Ref_IQx[1, j + 1]) / 3;
                        yValues_Q[k] = ((Measured_Output[1, j - 1] + Measured_Output[1, j] + Measured_Output[1, j + 1]) / 3) * IQ_Sign_Multiplier;
                        //xValues_I[k] = tmp_Ref_IQx[0, j];
                        //yValues_I[k] = Measured_Output[0, j] * IQ_Sign_Multiplier;
                        //xValues_Q[k] = tmp_Ref_IQx[1, j];
                        //yValues_Q[k] = Measured_Output[1, j] * IQ_Sign_Multiplier;
                    }

                    k += 1;
                }

                int Iteration_Value = EVM_Iteration_Value;

                for (int v = 0; v < Iteration_Value; v++) // Iterate phase few times
                {

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 1. Convert from Polar to Vector (Both Full Wfm and Pilot Wfm)
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Total_Angle_Offset = 0;

                    double[] Ref_Angle = new double[xValues_I.Length];
                    double[] Measured_Angle = new double[xValues_I.Length];

                    double[] Ref_Mag = new double[xValues_I.Length];
                    double[] Measured_Mag = new double[xValues_I.Length];

                    double Maximum_Ref_Mag = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        // Phase conversion
                        Ref_Angle[j] = Math.Atan2(xValues_Q[j], xValues_I[j]) * (180 / Math.PI);
                        Measured_Angle[j] = Math.Atan2(yValues_Q[j], yValues_I[j]) * (180 / Math.PI);

                        // Magnitude conversion
                        Ref_Mag[j] = Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                        Measured_Mag[j] = Math.Sqrt(Math.Pow(yValues_I[j], 2) + Math.Pow(yValues_Q[j], 2));

                        if (Maximum_Ref_Mag < Ref_Mag[j])
                        {
                            Maximum_Ref_Mag = Ref_Mag[j];
                        }
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 2. Find Phase offset between Ref and Measured Phase() => Search offset from Pilot and apply to Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double Max_Angle = -9e9;
                    double Min_Angle = 9e9;

                    // Use pilot signal to extract phase offset
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Delta_Angle = 9e99;

                        Test_Header_Angle_Upper = Measured_Angle[j] + 360;
                        Test_Header_Angle_Middle = Measured_Angle[j];
                        Test_Header_Angle_Lower = Measured_Angle[j] - 360;

                        if (Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Upper;
                        }

                        if (Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Middle;
                        }

                        if (Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Ref_Angle[j]);
                            Measured_Angle[j] = Test_Header_Angle_Lower;
                        }
                    }

                    Total_Angle_Offset = 0;
                    int Angle_Counter = 0;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        if (Ref_Mag[j] > (Maximum_Ref_Mag * Angle_Search_Mag_Limit_Multiplier))
                        {
                            Total_Angle_Offset = Total_Angle_Offset + (Ref_Angle[j] - Measured_Angle[j]);
                            Angle_Counter = Angle_Counter + 1;
                        }
                    }

                    Total_Angle_Offset = Total_Angle_Offset / Angle_Counter;

                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Angle[j] = Measured_Angle[j] + Total_Angle_Offset;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 3. Perform linear regression for Mag() => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    // Perform linear regression using pilot signal
                    double rSquared_Mag, intercept_Mag, slope_Mag;

                    LinearRegression(Measured_Mag, Ref_Mag, out rSquared_Mag, out intercept_Mag, out slope_Mag);

                    // Apply the coef to the full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Measured_Mag[j] = (Measured_Mag[j] * slope_Mag) + intercept_Mag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 4. Convert back to Polar from Vector ( Both Full Wfm and Pilot Wfm )
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    SPAR_Correction.Real_Imag Var_One_B;
                    SPAR_Correction.Mag_Angle Var_Two_B;

                    // Apply for full waveform
                    for (int j = 0; j < xValues_I.Length; j++)
                    {
                        Var_Two_B.Angle = Measured_Angle[j];
                        Var_Two_B.Mag = Measured_Mag[j];
                        Var_One_B = SPAR_Correction_Local.conv_MagAngle_to_RealImag(Var_Two_B);
                        yValues_I[j] = Var_One_B.Real;
                        yValues_Q[j] = Var_One_B.Imag;
                    }

                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                    // 5. Perform micro-tune for both R and I independently => Search offset from Pilot and apply on Full Wfm
                    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                    double[] x_offset_I_Value = new double[Pilot_Temp_NOP];
                    double[] x_offset_Q_Value = new double[Pilot_Temp_NOP];

                    double c;
                    double b;
                    double a;

                    double x_offset_Pos = 0;
                    double x_offset_Neg = 0;

                    // Apply on the full waveform
                    double[] meas_pilotValues_I_Temp = new double[xValues_I.Length];
                    double[] meas_pilotValues_Q_Temp = new double[xValues_I.Length];

                    //bool EVM_ACLR_Perform_MicroTune = true;

                    if (EVM_ACLR_Perform_MicroTune == true)
                    {
                        for (int z = 0; z < MicroTune_Cycle; z++)
                        {
                            // *************************
                            // Start X_Offset extraction
                            // *************************
                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                // ***************
                                // Calculate for I
                                // ***************

                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_I[j])))) / (2 * a);
                                //
                                x_offset_I_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_I_Value[j] = x_offset_Neg;
                                }

                                // ***************
                                // Calculate for Q
                                // ***************

                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                //
                                x_offset_Pos = ((-1 * b) + Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                x_offset_Neg = ((-1 * b) - Math.Sqrt((Math.Pow(b, 2)) - (4 * a * (c - xValues_Q[j])))) / (2 * a);
                                //
                                x_offset_Q_Value[j] = -999;
                                //
                                // Result only when only one of the solution with in -1 and 1 value, else ignore ...
                                if ((x_offset_Pos <= 1 && x_offset_Pos >= -1) && (x_offset_Neg < -1 || x_offset_Neg > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Pos;
                                }
                                //
                                if ((x_offset_Neg <= 1 && x_offset_Neg >= -1) && (x_offset_Pos < -1 || x_offset_Pos > 1))
                                {
                                    x_offset_Q_Value[j] = x_offset_Neg;
                                }
                            }

                            double X_Offset_Total = 0;
                            int X_Offset_Counter = 0;
                            double Y_Offset_Total = 0;
                            int Y_Offset_Counter = 0;

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                if (x_offset_I_Value[j] <= 1 && x_offset_I_Value[j] >= -1)
                                {
                                    X_Offset_Total = X_Offset_Total + x_offset_I_Value[j];
                                    X_Offset_Counter = X_Offset_Counter + 1;
                                }
                                //
                                if (x_offset_Q_Value[j] <= 1 && x_offset_Q_Value[j] >= -1)
                                {
                                    Y_Offset_Total = Y_Offset_Total + x_offset_Q_Value[j];
                                    Y_Offset_Counter = Y_Offset_Counter + 1;
                                }
                            }

                            double x_offset_Result = X_Offset_Total / Convert.ToDouble(X_Offset_Counter); // Independent Coef for I and Q !!!
                            double Y_offset_Result = Y_Offset_Total / Convert.ToDouble(Y_Offset_Counter);

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                c = yValues_I[j];
                                b = (yValues_I[j + 1] - yValues_I[j - 1]) / 2;
                                a = yValues_I[j + 1] - b - c;
                                meas_pilotValues_I_Temp[j] = (a * Math.Pow(x_offset_Result, 2)) + (b * x_offset_Result) + c;
                                //
                                c = yValues_Q[j];
                                b = (yValues_Q[j + 1] - yValues_Q[j - 1]) / 2;
                                a = yValues_Q[j + 1] - b - c;
                                meas_pilotValues_Q_Temp[j] = (a * Math.Pow(Y_offset_Result, 2)) + (b * Y_offset_Result) + c;
                            }

                            for (int j = 0 + 1; j < xValues_I.Length - 1; j++)
                            {
                                yValues_I[j] = meas_pilotValues_I_Temp[j]; // Pass back to the original array
                                yValues_Q[j] = meas_pilotValues_Q_Temp[j];
                            }

                        }
                    }

                    if (v == Iteration_Value - 1) // Start calculating EVM result if this is the last iteration cycle
                    {
                        // %%%%%%%%%%%%%%%%
                        // 6. Calculate EVM
                        // %%%%%%%%%%%%%%%%

                        if (false) // For debugging only
                        {
                            string[] tmp_string6 = new string[Frame_NOP];

                            for (int i = 0; i < Frame_NOP; i++)
                            {
                                //tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                //                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                //                 Convert.ToString(yValues_Q[i]).Trim();
                                tmp_string6[i] = Convert.ToString(xValues_I[i]).Trim() + "," +
                                                 Convert.ToString(xValues_Q[i]).Trim() + "," +
                                                 Convert.ToString(yValues_I[i]).Trim() + "," +
                                                 Convert.ToString(yValues_Q[i]).Trim();
                            }
                            File.WriteAllLines("c:\\temp\\measured_debug_IQ_Compare.csv", tmp_string6);
                        }

                        /*

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        for (int j = 0 + 1; j < Frame_NOP - 1; j++)
                        {
                            Sum_Total = Sum_Total + Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + Math.Sqrt(Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = (Sum_Residual / Sum_Total) * 100;

                        */

                        /*

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        double Sum_Total2 = 0;
                        double Sum_Residual2 = 0;

                        //for (int j = 0 + 1; j < Frame_NOP - 1; j++)
                        for (int j = 0 + 3; j < Frame_NOP - 3; j++) // <= Remove feq points from the trace eadge from final calculation
                        {
                            Sum_Total = Sum_Total + Math.Sqrt(Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + Math.Sqrt(Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                            Sum_Total2 = Sum_Total2 + (Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual2 = Sum_Residual2 + (Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = (Sum_Residual / Sum_Total) * 100;
                        FastEVM_Result2 = Math.Sqrt(Sum_Residual2 / Sum_Total2) * 100;

                        */

                        double Sum_Total = 0;
                        double Sum_Residual = 0;

                        for (int j = 0 + 3; j < Frame_NOP - 3; j++) // <= Remove feq points from the trace eadge from final calculation
                        {
                            Sum_Total = Sum_Total + (Math.Pow(xValues_I[j], 2) + Math.Pow(xValues_Q[j], 2));
                            Sum_Residual = Sum_Residual + (Math.Pow(xValues_I[j] - yValues_I[j], 2) + Math.Pow(xValues_Q[j] - yValues_Q[j], 2));
                        }

                        // Extract EVM
                        FastEVM_Result = Math.Sqrt(Sum_Residual / Sum_Total) * 100;
                    }
                }
            }
        }

        // ********************************************
        // Fits a line to a collection of (x,y) points.
        // ********************************************
        private void LinearRegression(double[] xVals, double[] yVals, out double rSquared, out double yIntercept, out double slope)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            double x;
            double y;

            for (int i = 0; i < xVals.Length; i++)
            {
                x = xVals[i];
                y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            double count = xVals.Length;
            double ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            double ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            double sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

        private void LinearRegression_v1(double[] xVals, double[] yVals, out double rSquared, out double yIntercept, out double slope)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (var i = 0; i < xVals.Length; i++)
            {
                var x = xVals[i];
                var y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var count = xVals.Length;
            var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

        //public niComplexNumber[] data_1;
        //public niComplexNumber[] data_2;
        //public niComplexNumber[] data_3;
        //public niComplexNumber[] data_4;
        //public niComplexNumber[] data_5;

        private int NumberOfPoint_Original_IQ;

        private double RFPower_DCSupply_Sync_Ratio; // Use this variable value to sync RF power and DC Supply timing block

        private double RFPower_Final_Value_TimeDomain;

        private string EVM_First_Header_Marker_Str;
        private string EVM_Second_Header_Marker_Str;
        private string EVM_IQ_Train_Length_Str;

        public void Average_IQin(niComplexNumber[,] Data_In)
        {
            Measured_Output = new Double[3, NumberOfPoint_Original_IQ];

            // Read in the IQ signal from VSA
            int NumberOfPoint2 = Data_In.Length / 5;

            for (int i = 0; i < NumberOfPoint2; i++)
            {
                Data_In[0, i].Real = (Data_In[0, i].Real + Data_In[1, i].Real + Data_In[2, i].Real + Data_In[3, i].Real + Data_In[4, i].Real) / 5;
                Data_In[0, i].Imaginary = (Data_In[0, i].Imaginary + Data_In[1, i].Imaginary + Data_In[2, i].Imaginary + Data_In[3, i].Imaginary + Data_In[4, i].Imaginary) / 5;
            }
        }

        private bool EVM_Generate_Measured_IQ_PwrSearch_Ver4p1(double PowerTargetdBm, int Pilot_Length, int First_Header_Search_Guess_Point, int First_Header_Search_Window_Width_NOP, int Record_Number,
            niComplexNumber[,] Data_In)
        {

            double[,] tmp_header = OriIQData;
            int Counter_Header = OriIQDataNum;
            int NumberOfPoint_Original_IQ = OriIQDataNOP;
            // *****************************************************************************
            // The below step just to create simulation Ref IQ file for code development ...
            // *****************************************************************************
            Measured_Output = new Double[3, NumberOfPoint_Original_IQ];

            // Read in the IQ signal from VSA
            int NumberOfPoint2 = Data_In.Length / Record_Number_Total;
            //
            // [0] = I dataset
            // [1] = Q dataset
            // [2] = Header merit
            double[,] Train_Content = new double[4, NumberOfPoint2];

            for (int i = 0; i < NumberOfPoint2; i++)
            {
                Train_Content[0, i] = Data_In[Record_Number, i].Real;
                Train_Content[1, i] = Data_In[Record_Number, i].Imaginary;
            }

            /*
            for (int i = 0; i < NumberOfPoint2; i++)
            {
                if (Record_Number == 0)
                {
                    Train_Content[0, i] = Data_In[0, i].Real;
                    Train_Content[1, i] = Data_In[0, i].Imaginary;
                }
                if (Record_Number == 1)
                {
                    Train_Content[0, i] = Data_In[1, i].Real;
                    Train_Content[1, i] = Data_In[1, i].Imaginary;
                }
                if (Record_Number == 2)
                {
                    Train_Content[0, i] = Data_In[2, i].Real;
                    Train_Content[1, i] = Data_In[2, i].Imaginary;
                }
                if (Record_Number == 3)
                {
                    Train_Content[0, i] = Data_In[3, i].Real;
                    Train_Content[1, i] = Data_In[3, i].Imaginary;
                }
                if (Record_Number == 4)
                {
                    Train_Content[0, i] = Data_In[4, i].Real;
                    Train_Content[1, i] = Data_In[4, i].Imaginary;
                }

                if (Record_Number == 5)
                {
                    Train_Content[0, i] = Data_In[5, i].Real;
                    Train_Content[1, i] = Data_In[5, i].Imaginary;
                }
                if (Record_Number == 6)
                {
                    Train_Content[0, i] = Data_In[6, i].Real;
                    Train_Content[1, i] = Data_In[6, i].Imaginary;
                }
                if (Record_Number == 7)
                {
                    Train_Content[0, i] = Data_In[7, i].Real;
                    Train_Content[1, i] = Data_In[7, i].Imaginary;
                }
                if (Record_Number == 8)
                {
                    Train_Content[0, i] = Data_In[8, i].Real;
                    Train_Content[1, i] = Data_In[8, i].Imaginary;
                }
                if (Record_Number == 9)
                {
                    Train_Content[0, i] = Data_In[9, i].Real;
                    Train_Content[1, i] = Data_In[9, i].Imaginary;
                }
            }
            */

            // ********************
            // Capture header array 
            // ********************

            double[] Test_Header_Angle = new double[Counter_Header];
            double[] Reference_Header_Angle = new double[Counter_Header];

            int counter_j = 0;

            for (int j = 0; j < Counter_Header; j++)
            {
                Reference_Header_Angle[counter_j] = Math.Atan2(Convert.ToDouble(tmp_header[1, j]), Convert.ToDouble(tmp_header[0, j])) * (180 / Math.PI);
                counter_j = counter_j + 1;
            }

            int First_Header_Marker = -9999;
            int Second_Header_Marker = -9999;

            int Final_First_Header_Marker = -9999;
            int Final_Second_Header_Marker = -9999;

            double Merit_Search_Value_FirstHeader = -9999;
            double Merit_Search_Value_SecondHeader = -9999;

            bool Within_First_Header_Window;
            bool Within_Second_Header_Window;

            double Test_Header_Angle_Upper;
            double Test_Header_Angle_Middle;
            double Test_Header_Angle_Lower;
            double Delta_Angle = 9e99;

            double Max_Angle = -9e9;
            double Min_Angle = 9e9;

            bool Power_Search_Flag = true;

            Second_Header_Marker = First_Header_Search_Guess_Point - Pilot_Length;

            double Power_Total_Watt_Previous_Pass = -9e9;

            // Loop through the IQ dataset and search for the marching power
            if (Power_Search_Flag == true)
            {
                // Set the new start and stop search points
                First_Header_Search_Guess_Point = Second_Header_Marker + Pilot_Length;

                Merit_Search_Value_FirstHeader = -9999;
                Merit_Search_Value_SecondHeader = -9999;

                // Check the correlation merit, use the RSquare as merit number
                for (int i = 0; i < NumberOfPoint2; i++)
                {
                    Within_First_Header_Window = false;
                    Within_Second_Header_Window = false;

                    // First header search window
                    if (i >= (First_Header_Search_Guess_Point - First_Header_Search_Window_Width_NOP) && i <= (First_Header_Search_Guess_Point + First_Header_Search_Window_Width_NOP))
                    {
                        Within_First_Header_Window = true;
                    }

                    // Second header search window
                    if (i >= (First_Header_Search_Guess_Point - First_Header_Search_Window_Width_NOP + NumberOfPoint_Original_IQ) && i <= (First_Header_Search_Guess_Point + First_Header_Search_Window_Width_NOP + NumberOfPoint_Original_IQ))
                    {
                        Within_Second_Header_Window = true;
                    }

                    if ((Within_First_Header_Window == true) || (Within_Second_Header_Window == true))
                    {
                        // Header first
                        counter_j = 0;

                        // Make sure not exceeding the dataset length ...
                        if ((Counter_Header + i) < NumberOfPoint2)
                        {
                            Max_Angle = -9e9;
                            Min_Angle = 9e9;

                            // Populate both arrays
                            for (int j = 0; j < Counter_Header; j++)
                            {
                                Test_Header_Angle[counter_j] = Math.Atan2(Convert.ToDouble(Train_Content[1, j + i]), Convert.ToDouble(Train_Content[0, j + i])) * (180 / Math.PI);

                                Delta_Angle = 9e99;

                                Test_Header_Angle_Upper = Test_Header_Angle[counter_j] + 360;
                                Test_Header_Angle_Middle = Test_Header_Angle[counter_j];
                                Test_Header_Angle_Lower = Test_Header_Angle[counter_j] - 360;

                                if (Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Upper;
                                }

                                if (Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Middle;
                                }

                                if (Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Lower;
                                }

                                if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                                {
                                    Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                                }

                                if (Max_Angle < (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                                {
                                    Max_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                                }

                                if (Min_Angle > (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                                {
                                    Min_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                                }

                                counter_j += 1;

                            }

                            double Angle_Limit_Setting = 300; // 300
                            double Angle_Separation_Setting = 45; // 45

                            // Manage remaining issue with 360 max delta which some time is an issue
                            if ((Max_Angle - Min_Angle) > Angle_Limit_Setting) // If the answer greated than 300Deg separation
                            {
                                for (int j = 0; j < Counter_Header; j++)
                                {
                                    Test_Header_Angle[j] = Test_Header_Angle[j] - Angle_Separation_Setting; // create reasonable size degree separation
                                    if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                                    {
                                        Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                                    }
                                    Test_Header_Angle[j] = Test_Header_Angle[j] + Angle_Separation_Setting; // re-nstate the offset back after re-alignment
                                }
                            }

                            // Scan for Header directly (in this case both I and Q dataset are used)
                            Train_Content[2, i] = RSquare_Value(Reference_Header_Angle, Test_Header_Angle); // Save the RSquare merit results for later process use

                            if (Within_First_Header_Window == true)
                            {
                                if (Convert.ToDouble(Train_Content[2, i]) > Merit_Search_Value_FirstHeader)
                                {
                                    Merit_Search_Value_FirstHeader = Convert.ToDouble(Train_Content[2, i]);
                                    First_Header_Marker = i;
                                }
                            }

                            if (Within_Second_Header_Window == true)
                            {
                                if (Convert.ToDouble(Train_Content[2, i]) > Merit_Search_Value_SecondHeader)
                                {
                                    Merit_Search_Value_SecondHeader = Convert.ToDouble(Train_Content[2, i]);
                                    Second_Header_Marker = i;
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }

                // Added to remove program error and letting code to extract out EVM from this trace as well !!!
                if (NumberOfPoint_Original_IQ != (Second_Header_Marker - First_Header_Marker))
                {
                    Second_Header_Marker = First_Header_Marker + NumberOfPoint_Original_IQ; // Force the secondary header to have the required NOP ...
                }

                int Counter_YYY = 0;

                for (int i = First_Header_Marker; i < (Second_Header_Marker); i++)
                {
                    Measured_Output[0, Counter_YYY] = Train_Content[0, i];
                    Measured_Output[1, Counter_YYY] = Train_Content[1, i];
                    Measured_Output[2, Counter_YYY] = tmp_header[2, Counter_YYY];
                    Counter_YYY = Counter_YYY + 1;
                }
            }

            bool Sync_Status = true;

            // Make sure the length is the same 
            if (NumberOfPoint_Original_IQ != (Second_Header_Marker - First_Header_Marker))
            {
                Sync_Status = false; // Meaning Sync fail
            }
            else // Is the length the same then start populating Ref_IQ and its mask
            {
                // **************************
                // Capture the marker results
                // **************************
                EVM_First_Header_Marker_Str = First_Header_Marker.ToString().Trim();
                EVM_Second_Header_Marker_Str = Second_Header_Marker.ToString().Trim();
                EVM_IQ_Train_Length_Str = NumberOfPoint_Original_IQ.ToString().Trim();
            }

            return (Sync_Status);
        }

        private bool EVM_Generate_Measured_IQ_PwrSearch_Ver4p1_Old_23Sept2021(double PowerTargetdBm, int Pilot_Length, int First_Header_Search_Guess_Point, int First_Header_Search_Window_Width_NOP, int Record_Number,
    niComplexNumber[,] Data_In)
        {

            double[,] tmp_header = OriIQData;
            int Counter_Header = OriIQDataNum;
            int NumberOfPoint_Original_IQ = OriIQDataNOP;
            // *****************************************************************************
            // The below step just to create simulation Ref IQ file for code development ...
            // *****************************************************************************
            Measured_Output = new Double[3, NumberOfPoint_Original_IQ];

            // Read in the IQ signal from VSA
            int NumberOfPoint2 = Data_In.Length / 10;
            //
            // [0] = I dataset
            // [1] = Q dataset
            // [2] = Header merit
            double[,] Train_Content = new double[4, NumberOfPoint2];

            for (int i = 0; i < NumberOfPoint2; i++)
            {
                if (Record_Number == 0)
                {
                    Train_Content[0, i] = Data_In[0, i].Real;
                    Train_Content[1, i] = Data_In[0, i].Imaginary;
                }
                if (Record_Number == 1)
                {
                    Train_Content[0, i] = Data_In[1, i].Real;
                    Train_Content[1, i] = Data_In[1, i].Imaginary;
                }
                if (Record_Number == 2)
                {
                    Train_Content[0, i] = Data_In[2, i].Real;
                    Train_Content[1, i] = Data_In[2, i].Imaginary;
                }
                if (Record_Number == 3)
                {
                    Train_Content[0, i] = Data_In[3, i].Real;
                    Train_Content[1, i] = Data_In[3, i].Imaginary;
                }
                if (Record_Number == 4)
                {
                    Train_Content[0, i] = Data_In[4, i].Real;
                    Train_Content[1, i] = Data_In[4, i].Imaginary;
                }

                if (Record_Number == 5)
                {
                    Train_Content[0, i] = Data_In[5, i].Real;
                    Train_Content[1, i] = Data_In[5, i].Imaginary;
                }
                if (Record_Number == 6)
                {
                    Train_Content[0, i] = Data_In[6, i].Real;
                    Train_Content[1, i] = Data_In[6, i].Imaginary;
                }
                if (Record_Number == 7)
                {
                    Train_Content[0, i] = Data_In[7, i].Real;
                    Train_Content[1, i] = Data_In[7, i].Imaginary;
                }
                if (Record_Number == 8)
                {
                    Train_Content[0, i] = Data_In[8, i].Real;
                    Train_Content[1, i] = Data_In[8, i].Imaginary;
                }
                if (Record_Number == 9)
                {
                    Train_Content[0, i] = Data_In[9, i].Real;
                    Train_Content[1, i] = Data_In[9, i].Imaginary;
                }

            }

            // ********************
            // Capture header array 
            // ********************

            double[] Test_Header_Angle = new double[Counter_Header];
            double[] Reference_Header_Angle = new double[Counter_Header];

            int counter_j = 0;

            for (int j = 0; j < Counter_Header; j++)
            {
                Reference_Header_Angle[counter_j] = Math.Atan2(Convert.ToDouble(tmp_header[1, j]), Convert.ToDouble(tmp_header[0, j])) * (180 / Math.PI);
                counter_j = counter_j + 1;
            }

            int First_Header_Marker = -9999;
            int Second_Header_Marker = -9999;

            int Final_First_Header_Marker = -9999;
            int Final_Second_Header_Marker = -9999;

            double Merit_Search_Value_FirstHeader = -9999;
            double Merit_Search_Value_SecondHeader = -9999;

            bool Within_First_Header_Window;
            bool Within_Second_Header_Window;

            double Test_Header_Angle_Upper;
            double Test_Header_Angle_Middle;
            double Test_Header_Angle_Lower;
            double Delta_Angle = 9e99;

            double Max_Angle = -9e9;
            double Min_Angle = 9e9;

            bool Power_Search_Flag = true;

            Second_Header_Marker = First_Header_Search_Guess_Point - Pilot_Length;

            double Power_Total_Watt_Previous_Pass = -9e9;

            // Loop through the IQ dataset and search for the marching power
            if (Power_Search_Flag == true)
            {
                // Set the new start and stop search points
                First_Header_Search_Guess_Point = Second_Header_Marker + Pilot_Length;

                Merit_Search_Value_FirstHeader = -9999;
                Merit_Search_Value_SecondHeader = -9999;

                // Check the correlation merit, use the RSquare as merit number
                for (int i = 0; i < NumberOfPoint2; i++)
                {
                    Within_First_Header_Window = false;
                    Within_Second_Header_Window = false;

                    // First header search window
                    if (i >= (First_Header_Search_Guess_Point - First_Header_Search_Window_Width_NOP) && i <= (First_Header_Search_Guess_Point + First_Header_Search_Window_Width_NOP))
                    {
                        Within_First_Header_Window = true;
                    }

                    // Second header search window
                    if (i >= (First_Header_Search_Guess_Point - First_Header_Search_Window_Width_NOP + NumberOfPoint_Original_IQ) && i <= (First_Header_Search_Guess_Point + First_Header_Search_Window_Width_NOP + NumberOfPoint_Original_IQ))
                    {
                        Within_Second_Header_Window = true;
                    }

                    if ((Within_First_Header_Window == true) || (Within_Second_Header_Window == true))
                    {
                        // Header first
                        counter_j = 0;

                        // Make sure not exceeding the dataset length ...
                        if ((Counter_Header + i) < NumberOfPoint2)
                        {
                            Max_Angle = -9e9;
                            Min_Angle = 9e9;

                            // Populate both arrays
                            for (int j = 0; j < Counter_Header; j++)
                            {
                                Test_Header_Angle[counter_j] = Math.Atan2(Convert.ToDouble(Train_Content[1, j + i]), Convert.ToDouble(Train_Content[0, j + i])) * (180 / Math.PI);

                                Delta_Angle = 9e99;

                                Test_Header_Angle_Upper = Test_Header_Angle[counter_j] + 360;
                                Test_Header_Angle_Middle = Test_Header_Angle[counter_j];
                                Test_Header_Angle_Lower = Test_Header_Angle[counter_j] - 360;

                                if (Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Upper;
                                }

                                if (Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Middle;
                                }

                                if (Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]) < Delta_Angle)
                                {
                                    Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]);
                                    Test_Header_Angle[counter_j] = Test_Header_Angle_Lower;
                                }

                                if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                                {
                                    Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                                }

                                if (Max_Angle < (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                                {
                                    Max_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                                }

                                if (Min_Angle > (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                                {
                                    Min_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                                }

                                counter_j += 1;

                            }

                            double Angle_Limit_Setting = 300; // 300
                            double Angle_Separation_Setting = 45; // 45

                            // Manage remaining issue with 360 max delta which some time is an issue
                            if ((Max_Angle - Min_Angle) > Angle_Limit_Setting) // If the answer greated than 300Deg separation
                            {
                                for (int j = 0; j < Counter_Header; j++)
                                {
                                    Test_Header_Angle[j] = Test_Header_Angle[j] - Angle_Separation_Setting; // create reasonable size degree separation
                                    if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                                    {
                                        Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                                    }
                                    Test_Header_Angle[j] = Test_Header_Angle[j] + Angle_Separation_Setting; // re-nstate the offset back after re-alignment
                                }
                            }

                            // Scan for Header directly (in this case both I and Q dataset are used)
                            Train_Content[2, i] = RSquare_Value(Reference_Header_Angle, Test_Header_Angle); // Save the RSquare merit results for later process use

                            if (Within_First_Header_Window == true)
                            {
                                if (Convert.ToDouble(Train_Content[2, i]) > Merit_Search_Value_FirstHeader)
                                {
                                    Merit_Search_Value_FirstHeader = Convert.ToDouble(Train_Content[2, i]);
                                    First_Header_Marker = i;
                                }
                            }

                            if (Within_Second_Header_Window == true)
                            {
                                if (Convert.ToDouble(Train_Content[2, i]) > Merit_Search_Value_SecondHeader)
                                {
                                    Merit_Search_Value_SecondHeader = Convert.ToDouble(Train_Content[2, i]);
                                    Second_Header_Marker = i;
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }

                // Added to remove program error and letting code to extract out EVM from this trace as well !!!
                if (NumberOfPoint_Original_IQ != (Second_Header_Marker - First_Header_Marker))
                {
                    Second_Header_Marker = First_Header_Marker + NumberOfPoint_Original_IQ; // Force the secondary header to have the required NOP ...
                }

                int Counter_YYY = 0;

                for (int i = First_Header_Marker; i < (Second_Header_Marker); i++)
                {
                    Measured_Output[0, Counter_YYY] = Train_Content[0, i];
                    Measured_Output[1, Counter_YYY] = Train_Content[1, i];
                    Measured_Output[2, Counter_YYY] = tmp_header[2, Counter_YYY];
                    Counter_YYY = Counter_YYY + 1;
                }
            }

            bool Sync_Status = true;

            // Make sure the length is the same 
            if (NumberOfPoint_Original_IQ != (Second_Header_Marker - First_Header_Marker))
            {
                Sync_Status = false; // Meaning Sync fail
            }
            else // Is the length the same then start populating Ref_IQ and its mask
            {
                // **************************
                // Capture the marker results
                // **************************
                EVM_First_Header_Marker_Str = First_Header_Marker.ToString().Trim();
                EVM_Second_Header_Marker_Str = Second_Header_Marker.ToString().Trim();
                EVM_IQ_Train_Length_Str = NumberOfPoint_Original_IQ.ToString().Trim();
            }

            return (Sync_Status);
        }

        // Attempt to use RSquare for sync purposes ...
        public double RSquare_Value(double[] xVals, double[] yVals)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (int i = 0; i < xVals.Length; i++)
            {
                double x = xVals[i];
                double y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            //
            double count = xVals.Length;
            //
            double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            double dblR = rNumerator / Math.Sqrt(rDenom);
            //
            return (dblR * dblR * 100); // RSequare unit in Percentage
        }

        public double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            if (indata == null) return null;
            if (CutOff == 0) return indata;
            double Samplingrate = 1 / deltaTimeinsec;
            long dF2 = indata.Length - 1;
            double[] Dat2 = new double[dF2 + 4]; // 4 extra points front and back
            double[] data = indata;
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];
            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc;
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2] + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD
            double[] DatZt = new double[dF2 + 2];

            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4] + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }
            return data;
        }
    }

    public class FastEVMAnalysis_ACLR // [BurhanEVM]
    {
        private const double pi = 3.14159265359;

        public int Record_Number_Total = 5; // Number of captured IQ traces

        private double[] xR = new double[10];
        private double[] yR = new double[10];

        public FastEVMAnalysis_ACLR() // IQ.Waveform iqWaveform, niComplexNumber[,] iqTraceData, int Record_Number_Total_Input)
        {
            /*
                int NumberOfPoint2 = iqTraceData.Length / Record_Number_Total_Input;

                // [0] = I dataset
                // [1] = Q dataset
                // [2] = Header merit
                double[,] Train_Content = new double[4, NumberOfPoint2];

                int Record_Number = 0;

                for (int i = 0; i < NumberOfPoint2; i++)
                {
                    Train_Content[0, i] = iqTraceData[Record_Number, i].Real;
                    Train_Content[1, i] = iqTraceData[Record_Number, i].Imaginary;
                }
        */
        }

        public AclrResults Capture_ACLR(IQ.Waveform iqWaveform, niComplexNumber[,] iqTraceData, int Record_Number_Total_Input)
        {
            return calc_psd(iqWaveform, iqTraceData, Record_Number_Total_Input);

            /*
            int NumberOfPoint2 = iqTraceData.Length / Record_Number_Total_Input;

            // [0] = I dataset
            // [1] = Q dataset
            // [2] = Header merit
            double[,] Train_Content = new double[4, NumberOfPoint2];

            int Record_Number = 0;

            for (int i = 0; i < NumberOfPoint2; i++)
            {
                Train_Content[0, i] = iqTraceData[Record_Number, i].Real;
                Train_Content[1, i] = iqTraceData[Record_Number, i].Imaginary;
            }
            */

            // ***********************************************************************************************************************
            // ***********************************************************************************************************************
            /*
            AclrResults aclrResults = new AclrResults();

            aclrResults.centerChannelPower = 0; // GetCenterChannelPower(1);

            for (int i = 0; i < iqWaveform.AclrSettings.Name.Count; i++)
            {
                AdjCh res = new AdjCh();

                res.Name = iqWaveform.AclrSettings.Name[i];
                res.lowerDbc = 0; // GetBandPower(-iqWaveform.AclrSettings.OffsetHz[i], iqWaveform.AclrSettings.BwHz[i]) - aclrResults.centerChannelPower;
                res.upperDbc = 0; // GetBandPower(iqWaveform.AclrSettings.OffsetHz[i], iqWaveform.AclrSettings.BwHz[i]) - aclrResults.centerChannelPower;

                aclrResults.adjacentChanPowers.Add(res);
            }

            return aclrResults;
            */
            // ***********************************************************************************************************************
            // ***********************************************************************************************************************

        }

        private AclrResults calc_psd(IQ.Waveform iqWaveform, niComplexNumber[,] iqTraceData, int Record_Number_Total_Input)
        {
            var im_arr_null = 0;
            var re_arr_null = 0;

            int NumberOfPoint_X = iqTraceData.Length / Record_Number_Total_Input;

            // 1.2 as the measurement setting for VST set to 1.2x then original length
            // 0.95 meaning 95% of the IQ length avoiding the edge of the IQ which pretty much not too stable
            NumberOfPoint_X = Convert.ToInt32((NumberOfPoint_X / 1.2) * 0.95);

            double[] re_arr = new double[NumberOfPoint_X];
            double[] im_arr = new double[NumberOfPoint_X];
            double[] re_num = new double[NumberOfPoint_X];
            double[] im_num = new double[NumberOfPoint_X];
            double[] x = new double[NumberOfPoint_X];
            double[] y = new double[NumberOfPoint_X];

            double[] ACLR_RefCH_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR1_Result_Lower_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR1_Result_Upper_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR2_Result_Lower_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR2_Result_Upper_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR3_Result_Lower_dB_Final = new double[Record_Number_Total_Input];
            double[] ACLR3_Result_Upper_dB_Final = new double[Record_Number_Total_Input];

            for (int IQ_Trace_Run = 0; IQ_Trace_Run < Record_Number_Total_Input; IQ_Trace_Run++)
            {
                for (int i = 0; i < NumberOfPoint_X; i++)
                {
                    re_arr[i] = iqTraceData[IQ_Trace_Run, i + 50].Real; // Moved by 50 pts to avoid any risk for unstable IQ data stream
                    im_arr[i] = iqTraceData[IQ_Trace_Run, i + 50].Imaginary; // Moved by 50 pts to avoid any risk for unstable IQ data stream
                }

                if (re_arr.Length != im_arr.Length)
                {
                    if (im_arr.Length == 1 && re_arr.Length > 1)
                    {
                        im_arr_null = 1;
                    }
                    else if (re_arr.Length == 1 && im_arr.Length > 1)
                    {
                        re_arr_null = 1;
                    }
                    else
                    {
                        MessageBox.Show("Real and Imaginary components must have either same number of elements, or either the Imaginary or the Real component must have no elements!");
                        return null;
                    }
                }

                int data_size = 0;

                if (im_arr_null == 1)
                {
                    data_size = re_arr.Length;
                }
                else if (re_arr_null == 1)
                {
                    data_size = im_arr.Length;
                }
                else
                {
                    data_size = re_arr.Length;
                }

                for (int k = 0; k < data_size; k++)
                {
                    if (re_arr_null == 0)
                    {
                        re_num[k] = re_arr[k];
                    }
                    else
                    {
                        re_num[k] = 0;
                    }

                    if (im_arr_null == 0)
                    {
                        im_num[k] = im_arr[k];
                    }
                    else
                    {
                        im_num[k] = 0;
                    }

                    if (re_arr_null == 0)
                    {
                        x[k] = re_arr[k];
                    }
                    else
                    {
                        x[k] = 0;
                    }

                    if (im_arr_null == 0)
                    {
                        y[k] = im_arr[k];
                    }
                    else
                    {
                        y[k] = 0;
                    }
                }

                var m = Math.Ceiling(Math.Log(x.Length) / Math.Log(2));

                var Nfft = Math.Pow(2, m);

                // Override
                Nfft = data_size;

                // Below if statement will not be executed due to override above 
                if (Nfft > data_size)
                {
                    for (int k = data_size; k < Nfft; k++)
                    {
                        x[k] = 0;
                        y[k] = 0;
                        //
                        re_num[k] = 0;
                        im_num[k] = 0;
                    }
                }

                int Nbins = 2048;
                int Nov = 0;
                double fs = Convert.ToDouble(iqWaveform.VsaIQrate);

                if (Nbins > Nfft | Nbins <= 0)
                {
                    MessageBox.Show("Number of FFT bins must be less than input sample size and greater than 0");
                    return null;
                }

                // Reset the values of x and y
                Array.Clear(x, 0, x.Length);
                Array.Clear(y, 0, y.Length);

                Nfft = NumberOfPoint_X;

                for (int k = 0; k < Nfft; k++)
                {
                    x[k] = re_num[k];
                    y[k] = im_num[k];
                }

                var nbl = Math.Floor(Math.Log(Nbins) / Math.Log(2));

                var Nbin2 = Math.Pow(2, nbl);

                if (Nbin2 != Nbins)
                {
                    MessageBox.Show("Number of FFT bins must be integer power of 2");
                    return null;
                }

                if (Nov < 0 | Nov > Nbins / 2)
                {
                    MessageBox.Show("Number of overlap bins must be between 0 and half of FFT bins inclusive");
                    return null;
                }

                int offset = Nbins - Nov;

                var nfr = Math.Floor((Nfft - Nbins) / offset) + 1;

                double[] pacc = new double[Nbins];
                double[] pacc_xR = new double[Nbins];
                double[] pacc_yR = new double[Nbins];

                for (int k = 0; k < Nbins; k++)
                {
                    pacc[k] = 0;
                    pacc_xR[k] = 0;
                    pacc_yR[k] = 0;
                }
                double[] xfr = new double[Nbins];
                double[] yfr = new double[Nbins];

                int idx1 = 0;
                int idx2 = 0;

                double[] wn = new double[Nbins];

                for (var k = 0; k < nfr; k++)
                {
                    idx1 = k * offset;
                    idx2 = idx1 + Nbins - 1;

                    Array.Clear(xfr, 0, xfr.Length);
                    Array.Clear(yfr, 0, yfr.Length);

                    wn = window_gen(Nbins, 3); // <= purposely set the setting to option #3

                    int Counter_1 = 0;

                    for (var z = idx1; z <= idx2; z++)
                    {
                        xfr[Counter_1] = x[z];
                        yfr[Counter_1] = y[z];
                        Counter_1 = Counter_1 + 1;
                    }

                    for (var z = 0; z < xfr.Length; z++)
                    {
                        xfr[z] = xfr[z] * wn[z];
                        yfr[z] = yfr[z] * wn[z];
                    }

                    FFT(1, nbl, xfr, yfr);

                    for (int z = 0; z < Nbins; z++)
                    {
                        pacc[z] = pacc[z] + (xR[z] * xR[z]) + (yR[z] * yR[z]);
                        pacc_xR[z] = pacc_xR[z] + xR[z];
                        pacc_yR[z] = pacc_yR[z] + yR[z];
                    }

                }

                // Normalize by number of frames
                for (int k = 0; k < pacc.Length; k++)
                {
                    pacc[k] = pacc[k] / nfr;
                    pacc_xR[k] = pacc_xR[k] / nfr;
                    pacc_yR[k] = pacc_yR[k] / nfr;
                }

                double[] pacc_dB = new double[Nbins];

                for (int k = 0; k < pacc.Length; k++)
                {
                    pacc_dB[k] = 10 * (Math.Log(pacc[k] + 1e-20) / Math.Log(10));
                }

                bool is_dB_on = true; // Just set to dB in our case

                //var msg_str = "Number of frames averaged over is " + nfr + " and sampling frequency is " + fs + "<br>";

                // Create plots
                double[,] rset = new double[Nbins, 2];

                if (is_dB_on)
                {
                    for (int k = 0; k < pacc.Length; k++)
                    {
                        rset[k, 1] = pacc_dB[k];
                    }
                }
                else
                {
                    for (int k = 0; k < pacc.Length; k++)
                    {
                        rset[k, 1] = pacc[k];
                    }
                }

                if (fs > 0)
                {
                    for (int k = 0; k < pacc.Length; k++)
                    {
                        rset[k, 0] = k * fs / Nbins;
                    }
                }

                double[,] tmpset = new double[Nbins, 6];

                for (int k = 0; k < pacc.Length; k++)
                {
                    tmpset[k, 0] = rset[k, 0];
                    //
                    double AAA = (k + Math.Floor(Convert.ToDouble(Nbins / 2))) % Nbins;
                    tmpset[k, 1] = rset[Convert.ToInt32(AAA), 1];
                    //
                    tmpset[k, 2] = pacc_xR[Convert.ToInt32(AAA)];
                    tmpset[k, 3] = pacc_yR[Convert.ToInt32(AAA)];
                    //
                    tmpset[k, 4] = (Math.Sqrt(pacc_xR[Convert.ToInt32(AAA)] * pacc_xR[Convert.ToInt32(AAA)]) + (pacc_yR[Convert.ToInt32(AAA)] * pacc_yR[Convert.ToInt32(AAA)]));
                    tmpset[k, 4] = 10 * (Math.Log(tmpset[k, 4] + 1e-20) / Math.Log(10));
                    //
                    tmpset[k, 5] = (pacc_xR[Convert.ToInt32(AAA)] * pacc_xR[Convert.ToInt32(AAA)]) + (pacc_yR[Convert.ToInt32(AAA)] * pacc_yR[Convert.ToInt32(AAA)]);
                    tmpset[k, 5] = 10 * Math.Log10(tmpset[k, 5] + 1e-20);
                }

                if (fs > 0)
                {
                    for (int k = 0; k < pacc.Length; k++)
                    {
                        tmpset[k, 0] = tmpset[k, 0] - fs / 2;
                    }
                }
                else
                {
                    for (int k = 0; k < pacc.Length; k++)
                    {
                        tmpset[k, 0] = tmpset[k, 0] - Math.Floor(Convert.ToDouble(Nbins / 2));
                    }
                }

                // *******************
                // Calculate ACLR here
                // *******************

                // Define and set the measurement/extraction setting
                double ACLR_RefCH_LowerFreqSide_Hz = 0;
                double ACLR_RefCH_UpperFreqSide_Hz = 0;

                double ACLR1_Lower_LowerFreqSide_Hz = 0;
                double ACLR1_Lower_UpperFreqSide_Hz = 0;
                double ACLR1_Upper_LowerFreqSide_Hz = 0;
                double ACLR1_Upper_UpperFreqSide_Hz = 0;

                double ACLR2_Lower_LowerFreqSide_Hz = 0;
                double ACLR2_Lower_UpperFreqSide_Hz = 0;
                double ACLR2_Upper_LowerFreqSide_Hz = 0;
                double ACLR2_Upper_UpperFreqSide_Hz = 0;

                double ACLR3_Lower_LowerFreqSide_Hz = 0;
                double ACLR3_Lower_UpperFreqSide_Hz = 0;
                double ACLR3_Upper_LowerFreqSide_Hz = 0;
                double ACLR3_Upper_UpperFreqSide_Hz = 0;

                double ACLR_RefCH_dB = 0;

                double ACLR1_Result_Lower_dB = 0;
                double ACLR1_Result_Upper_dB = 0;
                double ACLR2_Result_Lower_dB = 0;
                double ACLR2_Result_Upper_dB = 0;
                double ACLR3_Result_Lower_dB = 0;
                double ACLR3_Result_Upper_dB = 0;

                ACLR_RefCH_LowerFreqSide_Hz = -1 * iqWaveform.AclrSettings.BwHz[2] / 2;
                ACLR_RefCH_UpperFreqSide_Hz = iqWaveform.AclrSettings.BwHz[2] / 2;

                ACLR1_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[0]) - (iqWaveform.AclrSettings.BwHz[0] / 2);
                ACLR1_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[0]) + (iqWaveform.AclrSettings.BwHz[0] / 2);
                ACLR1_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[0]) - (iqWaveform.AclrSettings.BwHz[0] / 2);
                ACLR1_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[0]) + (iqWaveform.AclrSettings.BwHz[0] / 2);

                ACLR2_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[1]) - (iqWaveform.AclrSettings.BwHz[1] / 2);
                ACLR2_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[1]) + (iqWaveform.AclrSettings.BwHz[1] / 2);
                ACLR2_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[1]) - (iqWaveform.AclrSettings.BwHz[1] / 2);
                ACLR2_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[1]) + (iqWaveform.AclrSettings.BwHz[1] / 2);

                ACLR3_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[2]) - (iqWaveform.AclrSettings.BwHz[2] / 2);
                ACLR3_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[2]) + (iqWaveform.AclrSettings.BwHz[2] / 2);
                ACLR3_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[2]) - (iqWaveform.AclrSettings.BwHz[2] / 2);
                ACLR3_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[2]) + (iqWaveform.AclrSettings.BwHz[2] / 2);

                // Prepare linear scale dataset

                double[,] rset_ACLR = new double[Nbins, 2];

                for (int k = 0; k < pacc.Length; k++)
                {
                    rset_ACLR[k, 1] = pacc[k];
                }

                for (int k = 0; k < pacc.Length; k++)
                {
                    rset_ACLR[k, 0] = k * fs / Nbins;
                }

                double[,] tmpset_ACLR = new double[Nbins, 2];

                for (int k = 0; k < pacc.Length; k++)
                {
                    tmpset_ACLR[k, 0] = rset_ACLR[k, 0];
                    //
                    double AAA = (k + Math.Floor(Convert.ToDouble(Nbins / 2))) % Nbins;
                    tmpset_ACLR[k, 1] = rset_ACLR[Convert.ToInt32(AAA), 1];
                }

                for (int k = 0; k < pacc.Length; k++)
                {
                    tmpset_ACLR[k, 0] = tmpset_ACLR[k, 0] - fs / 2;
                }

                // Scan through all the freq range
                for (int k = 0; k < pacc.Length; k++)
                {
                    // Reference Channel Power
                    if ((tmpset_ACLR[k, 0] >= ACLR_RefCH_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR_RefCH_UpperFreqSide_Hz))
                    {
                        ACLR_RefCH_dB = ACLR_RefCH_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }

                    // ACLR1_Lower
                    if ((tmpset_ACLR[k, 0] >= ACLR1_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR1_Lower_UpperFreqSide_Hz))
                    {
                        ACLR1_Result_Lower_dB = ACLR1_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }
                    // ACLR1_Upper
                    if ((tmpset_ACLR[k, 0] >= ACLR1_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR1_Upper_UpperFreqSide_Hz))
                    {
                        ACLR1_Result_Upper_dB = ACLR1_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }

                    // ACLR2_Lower
                    if ((tmpset_ACLR[k, 0] >= ACLR2_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR2_Lower_UpperFreqSide_Hz))
                    {
                        ACLR2_Result_Lower_dB = ACLR2_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }
                    // ACLR2_Upper
                    if ((tmpset_ACLR[k, 0] >= ACLR2_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR2_Upper_UpperFreqSide_Hz))
                    {
                        ACLR2_Result_Upper_dB = ACLR2_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }

                    // ACLR3_Lower
                    if ((tmpset_ACLR[k, 0] >= ACLR3_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR3_Lower_UpperFreqSide_Hz))
                    {
                        ACLR3_Result_Lower_dB = ACLR3_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }
                    // ACLR3_Upper
                    if ((tmpset_ACLR[k, 0] >= ACLR3_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR3_Upper_UpperFreqSide_Hz))
                    {
                        ACLR3_Result_Upper_dB = ACLR3_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                    }

                }

                // Calculate results

                ACLR_RefCH_dB = 10 * (Math.Log(ACLR_RefCH_dB + 1e-20) / Math.Log(10));

                ACLR1_Result_Lower_dB = 10 * (Math.Log(ACLR1_Result_Lower_dB + 1e-20) / Math.Log(10));
                ACLR1_Result_Upper_dB = 10 * (Math.Log(ACLR1_Result_Upper_dB + 1e-20) / Math.Log(10));
                //
                ACLR2_Result_Lower_dB = 10 * (Math.Log(ACLR2_Result_Lower_dB + 1e-20) / Math.Log(10));
                ACLR2_Result_Upper_dB = 10 * (Math.Log(ACLR2_Result_Upper_dB + 1e-20) / Math.Log(10));
                //
                ACLR3_Result_Lower_dB = 10 * (Math.Log(ACLR3_Result_Lower_dB + 1e-20) / Math.Log(10));
                ACLR3_Result_Upper_dB = 10 * (Math.Log(ACLR3_Result_Upper_dB + 1e-20) / Math.Log(10));

                ACLR1_Result_Lower_dB = ACLR_RefCH_dB - ACLR1_Result_Lower_dB;
                ACLR1_Result_Upper_dB = ACLR_RefCH_dB - ACLR1_Result_Upper_dB;
                //
                ACLR2_Result_Lower_dB = ACLR_RefCH_dB - ACLR2_Result_Lower_dB;
                ACLR2_Result_Upper_dB = ACLR_RefCH_dB - ACLR2_Result_Upper_dB;
                //
                ACLR3_Result_Lower_dB = ACLR_RefCH_dB - ACLR3_Result_Lower_dB;
                ACLR3_Result_Upper_dB = ACLR_RefCH_dB - ACLR3_Result_Upper_dB;
                //
                // Move result values
                ACLR_RefCH_dB_Final[IQ_Trace_Run] = ACLR_RefCH_dB;

                ACLR1_Result_Lower_dB_Final[IQ_Trace_Run] = ACLR1_Result_Lower_dB;
                ACLR1_Result_Upper_dB_Final[IQ_Trace_Run] = ACLR1_Result_Upper_dB;
                //
                ACLR2_Result_Lower_dB_Final[IQ_Trace_Run] = ACLR2_Result_Lower_dB;
                ACLR2_Result_Upper_dB_Final[IQ_Trace_Run] = ACLR2_Result_Upper_dB;
                //
                ACLR3_Result_Lower_dB_Final[IQ_Trace_Run] = ACLR3_Result_Lower_dB;
                ACLR3_Result_Upper_dB_Final[IQ_Trace_Run] = ACLR3_Result_Upper_dB;
            }

            AclrResults aclrResults = new AclrResults();

            aclrResults.centerChannelPower = ACLR_RefCH_dB_Final.Sum() / Record_Number_Total_Input;

            AdjCh res = new AdjCh();

            for (int i = 0; i < iqWaveform.AclrSettings.Name.Count; i++)
            {

                res.Name = iqWaveform.AclrSettings.Name[i];

                if (i == 0)
                {
                    res.lowerDbc = (-1 * ACLR1_Result_Lower_dB_Final.Sum()) / Record_Number_Total_Input;
                    res.upperDbc = (-1 * ACLR1_Result_Upper_dB_Final.Sum()) / Record_Number_Total_Input;
                }

                if (i == 1)
                {
                    res.lowerDbc = (-1 * ACLR2_Result_Lower_dB_Final.Sum()) / Record_Number_Total_Input;
                    res.upperDbc = (-1 * ACLR2_Result_Upper_dB_Final.Sum()) / Record_Number_Total_Input;
                }

                if (i == 2)
                {
                    res.lowerDbc = (-1 * ACLR3_Result_Lower_dB_Final.Sum()) / Record_Number_Total_Input;
                    res.upperDbc = (-1 * ACLR3_Result_Upper_dB_Final.Sum()) / Record_Number_Total_Input;
                }

                aclrResults.adjacentChanPowers.Add(res);
            }

            return aclrResults;

        }

        private AclrResults calc_psd_Old_25Oct2021(IQ.Waveform iqWaveform, niComplexNumber[,] iqTraceData, int Record_Number_Total_Input)
        {
            var im_arr_null = 0;
            var re_arr_null = 0;

            int NumberOfPoint_X = iqTraceData.Length / Record_Number_Total_Input;

            double[] re_arr = new double[NumberOfPoint_X];
            double[] im_arr = new double[NumberOfPoint_X];
            double[] re_num = new double[NumberOfPoint_X];
            double[] im_num = new double[NumberOfPoint_X];
            double[] x = new double[NumberOfPoint_X];
            double[] y = new double[NumberOfPoint_X];

            int Record_Number = 0;

            for (int i = 0; i < NumberOfPoint_X; i++)
            {
                re_arr[i] = iqTraceData[Record_Number, i].Real;
                im_arr[i] = iqTraceData[Record_Number, i].Imaginary;
            }

            if (re_arr.Length != im_arr.Length)
            {
                if (im_arr.Length == 1 && re_arr.Length > 1)
                {
                    im_arr_null = 1;
                }
                else if (re_arr.Length == 1 && im_arr.Length > 1)
                {
                    re_arr_null = 1;
                }
                else
                {
                    MessageBox.Show("Real and Imaginary components must have either same number of elements, or either the Imaginary or the Real component must have no elements!");
                    return null;
                }
            }

            int data_size = 0;

            if (im_arr_null == 1)
            {
                data_size = re_arr.Length;
            }
            else if (re_arr_null == 1)
            {
                data_size = im_arr.Length;
            }
            else
            {
                data_size = re_arr.Length;
            }

            for (int k = 0; k < data_size; k++)
            {
                if (re_arr_null == 0)
                {
                    re_num[k] = re_arr[k];
                }
                else
                {
                    re_num[k] = 0;
                }

                if (im_arr_null == 0)
                {
                    im_num[k] = im_arr[k];
                }
                else
                {
                    im_num[k] = 0;
                }

                if (re_arr_null == 0)
                {
                    x[k] = re_arr[k];
                }
                else
                {
                    x[k] = 0;
                }

                if (im_arr_null == 0)
                {
                    y[k] = im_arr[k];
                }
                else
                {
                    y[k] = 0;
                }
            }

            var m = Math.Ceiling(Math.Log(x.Length) / Math.Log(2));

            var Nfft = Math.Pow(2, m);

            // Override
            Nfft = data_size; // <= ????

            // Below if statement will not be executed due to override above 
            if (Nfft > data_size)
            {
                for (int k = data_size; k < Nfft; k++)
                {
                    x[k] = 0;
                    y[k] = 0;
                    //
                    re_num[k] = 0;
                    im_num[k] = 0;
                }
            }

            int Nbins = 2048; // 256;
            int Nov = 0;
            double fs = Convert.ToDouble(iqWaveform.VsaIQrate);  // 368.64e6;

            if (Nbins > Nfft | Nbins <= 0)
            {
                MessageBox.Show("Number of FFT bins must be less than input sample size and greater than 0");
                return null;
            }

            // Reset the values of x and y
            Array.Clear(x, 0, x.Length);
            Array.Clear(y, 0, y.Length);

            Nfft = NumberOfPoint_X;

            for (int k = 0; k < Nfft; k++)
            {
                x[k] = re_num[k];
                y[k] = im_num[k];
            }

            var nbl = Math.Floor(Math.Log(Nbins) / Math.Log(2));

            var Nbin2 = Math.Pow(2, nbl);

            if (Nbin2 != Nbins)
            {
                MessageBox.Show("Number of FFT bins must be integer power of 2");
                return null;
            }

            if (Nov < 0 | Nov > Nbins / 2)
            {
                MessageBox.Show("Number of overlap bins must be between 0 and half of FFT bins inclusive");
                return null;
            }

            int offset = Nbins - Nov;

            var nfr = Math.Floor((Nfft - Nbins) / offset) + 1;

            double[] pacc = new double[Nbins];
            double[] pacc_xR = new double[Nbins];
            double[] pacc_yR = new double[Nbins];

            for (int k = 0; k < Nbins; k++)
            {
                pacc[k] = 0;
                pacc_xR[k] = 0;
                pacc_yR[k] = 0;
            }
            double[] xfr = new double[Nbins];
            double[] yfr = new double[Nbins];

            int idx1 = 0;
            int idx2 = 0;

            double[] wn = new double[Nbins];

            for (var k = 0; k < nfr; k++)
            {
                idx1 = k * offset;
                idx2 = idx1 + Nbins - 1;

                Array.Clear(xfr, 0, xfr.Length);
                Array.Clear(yfr, 0, yfr.Length);

                //wn = window_gen(Nbins, Convert.ToInt32(txt_ACLR_WindowCode.Text)); // <= purposely set the setting to option #3
                wn = window_gen(Nbins, 3); // <= purposely set the setting to option #3

                int Counter_1 = 0;

                for (var z = idx1; z <= idx2; z++)
                {
                    xfr[Counter_1] = x[z];
                    yfr[Counter_1] = y[z];
                    Counter_1 = Counter_1 + 1;
                }

                for (var z = 0; z < xfr.Length; z++)
                {
                    xfr[z] = xfr[z] * wn[z];
                    yfr[z] = yfr[z] * wn[z];
                }

                FFT(1, nbl, xfr, yfr);

                for (int z = 0; z < Nbins; z++)
                {
                    pacc[z] = pacc[z] + (xR[z] * xR[z]) + (yR[z] * yR[z]);
                    pacc_xR[z] = pacc_xR[z] + xR[z];
                    pacc_yR[z] = pacc_yR[z] + yR[z];
                }

            }

            // Normalize by number of frames
            for (int k = 0; k < pacc.Length; k++)
            {
                pacc[k] = pacc[k] / nfr;
                pacc_xR[k] = pacc_xR[k] / nfr;
                pacc_yR[k] = pacc_yR[k] / nfr;
            }

            double[] pacc_dB = new double[Nbins];

            for (int k = 0; k < pacc.Length; k++)
            {
                pacc_dB[k] = 10 * (Math.Log(pacc[k] + 1e-20) / Math.Log(10));
            }

            bool is_dB_on = true; // Just set to dB in our case

            var msg_str = "Number of frames averaged over is " + nfr + " and sampling frequency is " + fs + "<br>";

            // Create plots
            double[,] rset = new double[Nbins, 2];

            if (is_dB_on)
            {
                for (int k = 0; k < pacc.Length; k++)
                {
                    rset[k, 1] = pacc_dB[k];
                }
            }
            else
            {
                for (int k = 0; k < pacc.Length; k++)
                {
                    rset[k, 1] = pacc[k];
                }
            }

            if (fs > 0)
            {
                for (int k = 0; k < pacc.Length; k++)
                {
                    rset[k, 0] = k * fs / Nbins;
                }
            }

            double[,] tmpset = new double[Nbins, 6];

            for (int k = 0; k < pacc.Length; k++)
            {
                tmpset[k, 0] = rset[k, 0];
                //
                double AAA = (k + Math.Floor(Convert.ToDouble(Nbins / 2))) % Nbins;
                tmpset[k, 1] = rset[Convert.ToInt32(AAA), 1];
                //
                tmpset[k, 2] = pacc_xR[Convert.ToInt32(AAA)];
                tmpset[k, 3] = pacc_yR[Convert.ToInt32(AAA)];
                //
                tmpset[k, 4] = (Math.Sqrt(pacc_xR[Convert.ToInt32(AAA)] * pacc_xR[Convert.ToInt32(AAA)]) + (pacc_yR[Convert.ToInt32(AAA)] * pacc_yR[Convert.ToInt32(AAA)]));
                tmpset[k, 4] = 10 * (Math.Log(tmpset[k, 4] + 1e-20) / Math.Log(10));
                //
                tmpset[k, 5] = (pacc_xR[Convert.ToInt32(AAA)] * pacc_xR[Convert.ToInt32(AAA)]) + (pacc_yR[Convert.ToInt32(AAA)] * pacc_yR[Convert.ToInt32(AAA)]);
                tmpset[k, 5] = 10 * Math.Log10(tmpset[k, 5] + 1e-20);
            }

            if (fs > 0)
            {
                for (int k = 0; k < pacc.Length; k++)
                {
                    tmpset[k, 0] = tmpset[k, 0] - fs / 2;
                }
            }
            else
            {
                for (int k = 0; k < pacc.Length; k++)
                {
                    tmpset[k, 0] = tmpset[k, 0] - Math.Floor(Convert.ToDouble(Nbins / 2));
                }
            }

            // *******************
            // Calculate ACLR here
            // *******************

            // Define and set the measurement/extraction setting
            double ACLR_RefCH_LowerFreqSide_Hz = 0;
            double ACLR_RefCH_UpperFreqSide_Hz = 0;

            double ACLR1_Lower_LowerFreqSide_Hz = 0;
            double ACLR1_Lower_UpperFreqSide_Hz = 0;
            double ACLR1_Upper_LowerFreqSide_Hz = 0;
            double ACLR1_Upper_UpperFreqSide_Hz = 0;

            double ACLR2_Lower_LowerFreqSide_Hz = 0;
            double ACLR2_Lower_UpperFreqSide_Hz = 0;
            double ACLR2_Upper_LowerFreqSide_Hz = 0;
            double ACLR2_Upper_UpperFreqSide_Hz = 0;

            double ACLR3_Lower_LowerFreqSide_Hz = 0;
            double ACLR3_Lower_UpperFreqSide_Hz = 0;
            double ACLR3_Upper_LowerFreqSide_Hz = 0;
            double ACLR3_Upper_UpperFreqSide_Hz = 0;

            double ACLR_RefCH_dB = 0;

            double ACLR1_Result_Lower_dB = 0;
            double ACLR1_Result_Upper_dB = 0;
            double ACLR2_Result_Lower_dB = 0;
            double ACLR2_Result_Upper_dB = 0;
            double ACLR3_Result_Lower_dB = 0;
            double ACLR3_Result_Upper_dB = 0;

            ACLR_RefCH_LowerFreqSide_Hz = -1 * iqWaveform.AclrSettings.BwHz[2] / 2;
            ACLR_RefCH_UpperFreqSide_Hz = iqWaveform.AclrSettings.BwHz[2] / 2;

            ACLR1_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[0]) - (iqWaveform.AclrSettings.BwHz[0] / 2);
            ACLR1_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[0]) + (iqWaveform.AclrSettings.BwHz[0] / 2);
            ACLR1_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[0]) - (iqWaveform.AclrSettings.BwHz[0] / 2);
            ACLR1_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[0]) + (iqWaveform.AclrSettings.BwHz[0] / 2);

            ACLR2_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[1]) - (iqWaveform.AclrSettings.BwHz[1] / 2);
            ACLR2_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[1]) + (iqWaveform.AclrSettings.BwHz[1] / 2);
            ACLR2_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[1]) - (iqWaveform.AclrSettings.BwHz[1] / 2);
            ACLR2_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[1]) + (iqWaveform.AclrSettings.BwHz[1] / 2);

            ACLR3_Lower_LowerFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[2]) - (iqWaveform.AclrSettings.BwHz[2] / 2);
            ACLR3_Lower_UpperFreqSide_Hz = (-1 * iqWaveform.AclrSettings.OffsetHz[2]) + (iqWaveform.AclrSettings.BwHz[2] / 2);
            ACLR3_Upper_LowerFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[2]) - (iqWaveform.AclrSettings.BwHz[2] / 2);
            ACLR3_Upper_UpperFreqSide_Hz = (iqWaveform.AclrSettings.OffsetHz[2]) + (iqWaveform.AclrSettings.BwHz[2] / 2);

            // Prepare linear scale dataset

            double[,] rset_ACLR = new double[Nbins, 2];

            for (int k = 0; k < pacc.Length; k++)
            {
                rset_ACLR[k, 1] = pacc[k];
            }

            for (int k = 0; k < pacc.Length; k++)
            {
                rset_ACLR[k, 0] = k * fs / Nbins;
            }

            double[,] tmpset_ACLR = new double[Nbins, 2];

            for (int k = 0; k < pacc.Length; k++)
            {
                tmpset_ACLR[k, 0] = rset_ACLR[k, 0];
                //
                double AAA = (k + Math.Floor(Convert.ToDouble(Nbins / 2))) % Nbins;
                tmpset_ACLR[k, 1] = rset_ACLR[Convert.ToInt32(AAA), 1];
            }

            for (int k = 0; k < pacc.Length; k++)
            {
                tmpset_ACLR[k, 0] = tmpset_ACLR[k, 0] - fs / 2;
            }

            // Scan through all the freq range
            for (int k = 0; k < pacc.Length; k++)
            {
                // Reference Channel Power
                if ((tmpset_ACLR[k, 0] >= ACLR_RefCH_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR_RefCH_UpperFreqSide_Hz))
                {
                    ACLR_RefCH_dB = ACLR_RefCH_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }

                // ACLR1_Lower
                if ((tmpset_ACLR[k, 0] >= ACLR1_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR1_Lower_UpperFreqSide_Hz))
                {
                    ACLR1_Result_Lower_dB = ACLR1_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }
                // ACLR1_Upper
                if ((tmpset_ACLR[k, 0] >= ACLR1_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR1_Upper_UpperFreqSide_Hz))
                {
                    ACLR1_Result_Upper_dB = ACLR1_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }

                // ACLR2_Lower
                if ((tmpset_ACLR[k, 0] >= ACLR2_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR2_Lower_UpperFreqSide_Hz))
                {
                    ACLR2_Result_Lower_dB = ACLR2_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }
                // ACLR2_Upper
                if ((tmpset_ACLR[k, 0] >= ACLR2_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR2_Upper_UpperFreqSide_Hz))
                {
                    ACLR2_Result_Upper_dB = ACLR2_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }

                // ACLR3_Lower
                if ((tmpset_ACLR[k, 0] >= ACLR3_Lower_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR3_Lower_UpperFreqSide_Hz))
                {
                    ACLR3_Result_Lower_dB = ACLR3_Result_Lower_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }
                // ACLR3_Upper
                if ((tmpset_ACLR[k, 0] >= ACLR3_Upper_LowerFreqSide_Hz) && (tmpset_ACLR[k, 0] <= ACLR3_Upper_UpperFreqSide_Hz))
                {
                    ACLR3_Result_Upper_dB = ACLR3_Result_Upper_dB + tmpset_ACLR[k, 1]; // Sum up all the power within the specified channel
                }

            }

            // Calculate results

            ACLR_RefCH_dB = 10 * (Math.Log(ACLR_RefCH_dB + 1e-20) / Math.Log(10));

            ACLR1_Result_Lower_dB = 10 * (Math.Log(ACLR1_Result_Lower_dB + 1e-20) / Math.Log(10));
            ACLR1_Result_Upper_dB = 10 * (Math.Log(ACLR1_Result_Upper_dB + 1e-20) / Math.Log(10));
            //
            ACLR2_Result_Lower_dB = 10 * (Math.Log(ACLR2_Result_Lower_dB + 1e-20) / Math.Log(10));
            ACLR2_Result_Upper_dB = 10 * (Math.Log(ACLR2_Result_Upper_dB + 1e-20) / Math.Log(10));
            //
            ACLR3_Result_Lower_dB = 10 * (Math.Log(ACLR3_Result_Lower_dB + 1e-20) / Math.Log(10));
            ACLR3_Result_Upper_dB = 10 * (Math.Log(ACLR3_Result_Upper_dB + 1e-20) / Math.Log(10));

            ACLR1_Result_Lower_dB = ACLR_RefCH_dB - ACLR1_Result_Lower_dB;
            ACLR1_Result_Upper_dB = ACLR_RefCH_dB - ACLR1_Result_Upper_dB;
            //
            ACLR2_Result_Lower_dB = ACLR_RefCH_dB - ACLR2_Result_Lower_dB;
            ACLR2_Result_Upper_dB = ACLR_RefCH_dB - ACLR2_Result_Upper_dB;
            //
            ACLR3_Result_Lower_dB = ACLR_RefCH_dB - ACLR3_Result_Lower_dB;
            ACLR3_Result_Upper_dB = ACLR_RefCH_dB - ACLR3_Result_Upper_dB;

            AclrResults aclrResults = new AclrResults();

            aclrResults.centerChannelPower = ACLR_RefCH_dB; // 0; // GetCenterChannelPower(1);

            AdjCh res = new AdjCh();

            for (int i = 0; i < iqWaveform.AclrSettings.Name.Count; i++)
            {

                res.Name = iqWaveform.AclrSettings.Name[i];

                if (i == 0)
                {
                    res.lowerDbc = ACLR1_Result_Lower_dB;
                    res.upperDbc = ACLR1_Result_Upper_dB;
                }

                if (i == 1)
                {
                    res.lowerDbc = ACLR2_Result_Lower_dB;
                    res.upperDbc = ACLR2_Result_Upper_dB;
                }

                if (i == 2)
                {
                    res.lowerDbc = ACLR3_Result_Lower_dB;
                    res.upperDbc = ACLR3_Result_Upper_dB;
                }

                aclrResults.adjacentChanPowers.Add(res);
            }

            return aclrResults;

        }

        /*
        This computes a complex-to-complex FFT 
        x and y are the real and imaginary arrays of 2^m points.
        dir =  1 gives forward transform
        dir = -1 gives reverse transform 
        */

        private int FFT(int dir, double m, double[] xinp, double[] yinp)
        {
            int n;

            /* Calculate the number of points */
            n = 1;
            for (int i = 0; i < m; i++)
            {
                n *= 2;
            }

            Array.Resize(ref xR, xinp.Length);
            Array.Resize(ref yR, yinp.Length);

            Array.Clear(xR, 0, xR.Length);
            Array.Clear(yR, 0, yR.Length);

            for (int i = 0; i < n; i++)
            {
                xR[i] = xinp[i];
                yR[i] = yinp[i];
            }

            double tx;
            double ty;
            int k;

            int i2;

            /* Do the bit reversal */
            i2 = n >> 1;
            int j = 0;
            for (int i = 0; i < n - 1; i++)
            {
                if (i < j)
                {
                    tx = xR[i];
                    ty = yR[i];
                    xR[i] = xR[j];
                    yR[i] = yR[j];
                    xR[j] = tx;
                    yR[j] = ty;
                }
                k = i2;
                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            /* Compute the FFT */
            double c1 = -1.0;
            double c2 = 0.0;
            //double l2 = 1;
            int l2 = 1;

            int l1;

            double u1;
            double u2;

            double t1;
            double t2;

            int i1;
            double z;

            for (int l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0;
                u2 = 0.0;
                for (j = 0; j < l1; j++)
                {
                    for (int i = j; i < n; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * xR[i1] - u2 * yR[i1];
                        t2 = u1 * yR[i1] + u2 * xR[i1];
                        xR[i1] = xR[i] - t1;
                        yR[i1] = yR[i] - t2;
                        xR[i] += t1;
                        yR[i] += t2;
                    }
                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = Math.Sqrt((1.0 - c1) / 2.0);
                if (dir == 1)
                    c2 = -c2;
                c1 = Math.Sqrt((1.0 + c1) / 2.0);
            }

            /* Scaling for reverse transform */
            if (dir == -1)
            {
                for (int i = 0; i < n; i++)
                {
                    xR[i] /= n;
                    yR[i] /= n;
                }
            }

            return (1);
        }

        /* Generate the window function */
        private double[] window_gen(int Nfft, int InputNo)
        {
            double[] res = new double[Nfft];

            if (InputNo == 1)
            {
                for (var n = 0; n < Nfft; n++)
                    res[n] = 1;
            }
            else if (InputNo == 2)
            {
                for (var n = 0; n < Nfft; n++)
                    res[n] = 0.54 - (0.46 * Math.Cos((2 * Math.PI * n) / (Nfft - 1)));
            }
            else if (InputNo == 3)
            {
                var kx = (Nfft - 1) / 2.0;

                for (var n = 0; n < Nfft; n++)
                    res[n] = 1 - Math.Abs((n - kx) / kx);
            }
            return (res);
        }

    }

    public class SpectralAnalysis
    {
        private readonly IQ.Waveform iqWaveform;
        private readonly niComplexNumber[] iqTraceData;

        private double[] mag;  // fft result
        private int N;
        private double rbw;

        public SpectralAnalysis(IQ.Waveform iqWaveform, niComplexNumber[] iqTraceData)
        {
            this.iqWaveform = iqWaveform;
            this.iqTraceData = iqTraceData;
            
            DoTheFft();
        }

        private void DoTheFft()
        {
            try
            {
                if (iqTraceData == null || iqTraceData.Length == 0) return;

                N = iqTraceData.Length;
                rbw = (double)iqWaveform.VsaIQrate / (double)N;

                double[] ReIm = new double[2 * N];
                for (int i = 0; i < N; i++)
                {
                    ReIm[2 * i] = iqTraceData[i].Real * iqWaveform.fftWindow[i];
                    ReIm[2 * i + 1] = iqTraceData[i].Imaginary * iqWaveform.fftWindow[i];
                }

                double[] mag1 = fftw.fft_mag(ReIm, fftwEnums.fftw_flags.Estimate);

                mag = new double[N];

                Array.Copy(mag1, 0, mag, N / 2, N / 2);
                Array.Copy(mag1, N / 2, mag, 0, N / 2);
            }
            catch (Exception e)
            {

            }
        }

        public double[] GetDbmSpectrum()
        {
            double[] magDbm = new double[N];

            for (int i = 0; i < N; i++)
            {
                magDbm[i] = 10.0 * Math.Log10(mag[i] * mag[i] / 2.0 * 1000.0 / 50.0);
            }

            return magDbm;
        }

        public double GetCenterChannelPower(int centerChanBwMultiplier = 1)
        {
            return GetBandPower(0, iqWaveform.RefChBW * centerChanBwMultiplier);
        }

        public AclrResults GetAclrResults()
        {
            AclrResults aclrResults = new AclrResults();

            aclrResults.centerChannelPower = GetCenterChannelPower(1);

            for (int i = 0; i < iqWaveform.AclrSettings.Name.Count; i++)
            {
                AdjCh res = new AdjCh();

                res.Name = iqWaveform.AclrSettings.Name[i];
                res.lowerDbc = GetBandPower(-iqWaveform.AclrSettings.OffsetHz[i], iqWaveform.AclrSettings.BwHz[i]) - aclrResults.centerChannelPower;
                res.upperDbc = GetBandPower(iqWaveform.AclrSettings.OffsetHz[i], iqWaveform.AclrSettings.BwHz[i]) - aclrResults.centerChannelPower;

                aclrResults.adjacentChanPowers.Add(res);
            }

            return aclrResults;
        }

        public double GetBandPower(double freqOffsetHz, double bandwidthHz)
        {
            int BinStart = N / 2 + (int)((freqOffsetHz - bandwidthHz / 2.0)/ rbw);
            int BinStop = N / 2 + (int)((freqOffsetHz + bandwidthHz / 2.0) / rbw);

            if (BinStart < 0 || BinStart > N - 1 || BinStop < 0 || BinStop > N - 1)
            {
                throw new Exception("VSA sampling rate is too low to support " + iqWaveform.ModulationStd + " " + iqWaveform.WaveformName + ", freq offset " + freqOffsetHz + "Hz, bandwidth " + bandwidthHz + "Hz\n\n");
            }

            double integratedPower = 0;

            for (int bin = BinStart; bin <= BinStop; bin++)
            {
                integratedPower += mag[bin] * mag[bin];
            }

            double integratedPowerDbm = 10.0 * Math.Log10(integratedPower / 2.0 * 1000.0 / 50.0);

            return integratedPowerDbm;
        }

        public void ShowPlot(IQ.Waveform IQwvfrm, string testName)
        {
            AclrResults aclResults = GetAclrResults();

            double[] dBmSpectrum = GetDbmSpectrum();

            string title = testName + "\n"
                + "Ref Chan: " + (IQwvfrm.RefChBW / 1e6).ToString("0.000000") + "MHz,  " + aclResults.centerChannelPower.ToString("0.00") + "dBm\n"
                + "Offset:                 Bandwidth:             Lower:          Upper:" + "\n";
            for (int i = 0; i < IQwvfrm.AclrSettings.BwHz.Count; i++)
            {
                title += (IQwvfrm.AclrSettings.OffsetHz[i] / 1e6).ToString("00.000000") + "MHz   " + (IQwvfrm.AclrSettings.BwHz[i] / 1e6).ToString("00.000000") + "MHz     " + aclResults.adjacentChanPowers[i].lowerDbc.ToString("00.00") + "dBc     " + aclResults.adjacentChanPowers[i].upperDbc.ToString("00.00") + "dBc\n";
            }

            double[] xVals = new double[dBmSpectrum.Length];
            xVals[0] = (double)-IQwvfrm.VsaIQrate / 2.0 / 1e6;
            double rbw = (double)IQwvfrm.VsaIQrate / N / 1e6;  // MHz
            for (int i = 1; i < xVals.Length; i++)
            {
                xVals[i] = xVals[i - 1] + rbw;
            }

            double maxVal = dBmSpectrum.Max();
            double minVal = dBmSpectrum.Min();

            double[] offAreas = new double[xVals.Length];
            for (int i = 0; i < offAreas.Length; i++)
            {
                bool isInArea = false;
                double xVal = xVals[i] * 1e6;

                if ((xVal > -IQwvfrm.RefChBW / 2.0) & (xVal < IQwvfrm.RefChBW / 2.0))
                {
                    isInArea = true;
                }
                else
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if ((xVal > -IQwvfrm.AclrSettings.OffsetHz[k] - IQwvfrm.AclrSettings.BwHz[k] / 2.0) & (xVal < -IQwvfrm.AclrSettings.OffsetHz[k] + IQwvfrm.AclrSettings.BwHz[k] / 2.0))
                        {
                            isInArea = true;
                            break;
                        }
                        if ((xVal > IQwvfrm.AclrSettings.OffsetHz[k] - IQwvfrm.AclrSettings.BwHz[k] / 2.0) & (xVal < IQwvfrm.AclrSettings.OffsetHz[k] + IQwvfrm.AclrSettings.BwHz[k] / 2.0))
                        {
                            isInArea = true;
                            break;
                        }
                    }
                }

                if (isInArea)
                    offAreas[i] = maxVal;
                else
                    offAreas[i] = minVal;
            }

            Calc.Charts.CreateChart("", title, xVals, dBmSpectrum, xVals, offAreas, "Freq Offset (MHz)", "Power (dBm)", 1, double.NaN, double.NaN, SeriesChartType.Line, false, true, false);
        }
    }


    public static class Calc
    {
        public enum ChannelBandwidth
        {
            Fundamental = 1, H2 = 2, H3 = 3
        }

        public static double InterpLinear(double[] array, double xVal)
        {
            if (xVal <= 0) return array[0];
            if (xVal >= array.Length - 1) return array.Last();

            double lowerX = Math.Floor(xVal);
            double upperX = lowerX + 1;
            double lowerY = array[(int)lowerX];
            double upperY = array[(int)upperX];
            double yInterp = (lowerY + (xVal - lowerX) * (upperY - lowerY) / (upperX - lowerX));

            return yInterp;
        }

        public class Regression
        {
            public class SimpleLinear
            {
                public double Slope, Intercept, R, R2, MSE;

                public SimpleLinear(double[] x, double[] y)
                {
                    try
                    {
                        if (x.Length != y.Length)
                        {
                            MessageBox.Show("Error: x and y arrays must have same length", "Regression.SimpleLinear");
                            return;
                        }

                        double xAvg = 0;
                        double yAvg = 0;
                        double x2Avg = 0;
                        double y2Avg = 0;
                        double xyAvg = 0;

                        for (int i = 0; i < y.Length; i++)
                        {
                            xAvg += x[i];
                            x2Avg += x[i] * x[i];
                            yAvg += y[i];
                            y2Avg += y[i] * y[i];
                            xyAvg += x[i] * y[i];
                        }

                        xAvg /= y.Length;
                        x2Avg /= y.Length;
                        yAvg /= y.Length;
                        y2Avg /= y.Length;
                        xyAvg /= y.Length;

                        Slope = (xyAvg - xAvg * yAvg) / (x2Avg - xAvg * xAvg);
                        Intercept = yAvg - Slope * xAvg;
                        R = Slope * Math.Sqrt((x2Avg - xAvg * xAvg) / (y2Avg - yAvg * yAvg));
                        R2 = R * R;

                        #region Mean Square Error of fit

                        double[] yFitted = new double[y.Length];

                        for (int i = 0; i < y.Length; i++) yFitted[i] = GetFittedValue(x[i]);

                        MSE = MeanSquareError(y, yFitted);

                        #endregion
                    }
                    catch (Exception e)
                    {

                    }
                }

                public double GetFittedValue(double x)
                {
                    return Slope * x + Intercept;
                }
            }

            public class SimpleExponential
            {
                public double Alpha, Beta, R, R2, MSE;

                public SimpleExponential(double[] x, double[] y)
                {
                    try
                    {
                        if (x.Length != y.Length)
                        {
                            MessageBox.Show("Error: x and y arrays must have same length", "Regression.SimpleExponential");
                            return;
                        }

                        # region  remove y values <=0

                        List<double> xPositivesOnlyList = new List<double>();
                        List<double> yPositivesOnlyList = new List<double>();

                        for (int i = 0; i < x.Length; i++)
                        {
                            if (Math.Sign(y[i]) == 1)
                            {
                                xPositivesOnlyList.Add(x[i]);
                                yPositivesOnlyList.Add(y[i]);
                            }
                        }

                        #endregion

                        // get log(y)
                        for (int i = 0; i < xPositivesOnlyList.Count; i++)
                        {
                            yPositivesOnlyList[i] = Math.Log(yPositivesOnlyList[i]);
                        }

                        SimpleLinear slr = new SimpleLinear(xPositivesOnlyList.ToArray(), yPositivesOnlyList.ToArray());

                        Alpha = Math.Exp(slr.Intercept);
                        Beta = slr.Slope;

                        double[] yFit = new double[x.Length];

                        for (int i = 0; i < x.Length; i++) yFit[i] = GetFittedValue(x[i]);

                        #region Check the fit (R, R2, MSE)

                        SimpleLinear slr2 = new SimpleLinear(y, yFit);

                        R = slr2.R;
                        R2 = slr2.R2;
                        MSE = MeanSquareError(y, yFit);

                        #endregion
                    }
                    catch (Exception e)
                    {

                    }
                }

                public double GetFittedValue(double x)
                {
                    return Alpha * Math.Exp(Beta * x);
                }
            }

            public class SimplePower
            {
                public double Alpha, Beta, R, R2, MSE;

                public SimplePower(double[] x, double[] y)
                {
                    try
                    {
                        if (x.Length != y.Length)
                        {
                            MessageBox.Show("Error: x and y arrays must have same length", "Regression.SimpleExponential");
                            return;
                        }

                        # region  remove x & y values <=0

                        List<double> xPositivesOnlyList = new List<double>();
                        List<double> yPositivesOnlyList = new List<double>();

                        for (int i = 0; i < x.Length; i++)
                        {
                            if (Math.Sign(x[i]) == 1 && Math.Sign(y[i]) == 1)
                            {
                                xPositivesOnlyList.Add(x[i]);
                                yPositivesOnlyList.Add(y[i]);
                            }
                        }

                        #endregion

                        // get log(y)
                        for (int i = 0; i < xPositivesOnlyList.Count; i++)
                        {
                            xPositivesOnlyList[i] = Math.Log(xPositivesOnlyList[i]);
                            yPositivesOnlyList[i] = Math.Log(yPositivesOnlyList[i]);
                        }

                        SimpleLinear slr = new SimpleLinear(xPositivesOnlyList.ToArray(), yPositivesOnlyList.ToArray());

                        Alpha = Math.Exp(slr.Intercept);
                        Beta = slr.Slope;

                        double[] yFit = new double[x.Length];

                        for (int i = 0; i < x.Length; i++) yFit[i] = GetFittedValue(x[i]);

                        #region Check the fit (R, R2, MSE)

                        SimpleLinear slr2 = new SimpleLinear(y, yFit);

                        R = slr2.R;
                        R2 = slr2.R2;
                        MSE = MeanSquareError(y, yFit);

                        #endregion
                    }
                    catch (Exception e)
                    {

                    }
                }

                public double GetFittedValue(double x)
                {
                    return Alpha * Math.Pow(x, Beta);
                }
            }

            public static double[] MovingAverage(double[] data, int period)
            {
                try
                {
                    if (period % 2 == 0) period++;   // ensure period is odd

                    double[] output = new double[data.Length];
                    double movingAverage = 0;

                    for (int i = 0; i < period / 2; i++)
                    {
                        movingAverage = 0;

                        int thisCount = period / 2 + i + 1;

                        for (int j = 0; j < thisCount; j++)
                        {
                            movingAverage += data[j] / thisCount;
                        }
                        output[i] = movingAverage;
                    }

                    movingAverage = 0;

                    for (int i = 0; i < period; i++)
                    {
                        movingAverage += data[i];
                    }

                    output[period / 2] = movingAverage / period;

                    for (int i = period / 2 + 1; i < data.Length - period / 2; i++)
                    {
                        int lowerI = i - period / 2 - 1;
                        int upperI = i + period / 2;

                        movingAverage -= data[i - period / 2 - 1];
                        movingAverage += data[i + period / 2];

                        output[i] = movingAverage / period;
                    }

                    for (int i = data.Length - period / 2; i < data.Length; i++)
                    {
                        movingAverage = 0;

                        int thisCount = period / 2 + data.Length - i;

                        for (int j = i - period / 2; j < data.Length; j++)
                        {
                            movingAverage += data[j] / thisCount;
                        }
                        output[i] = movingAverage;
                    }

                    return output;
                }
                catch (Exception e)
                {
                    return new double[8];
                }
            }

            public static double[] MovingRMS(double[] data, int period)
            {
                try
                {
                    if (period % 2 == 0) period++;   // ensure period is odd

                    double[] output = new double[data.Length];
                    double movingMeanSquare = 0;

                    for (int i = 0; i < period / 2; i++)
                    {
                        movingMeanSquare = 0;

                        int thisCount = period / 2 + i + 1;

                        for (int j = 0; j < thisCount; j++)
                        {
                            movingMeanSquare += data[j] * data[j] / thisCount;
                        }
                        output[i] = Math.Sqrt(movingMeanSquare);
                    }

                    movingMeanSquare = 0;

                    for (int i = 0; i < period; i++)
                    {
                        movingMeanSquare += data[i] * data[i];
                    }

                    output[period / 2] = Math.Sqrt(movingMeanSquare / period);

                    for (int i = period / 2 + 1; i < data.Length - period / 2; i++)
                    {
                        int lowerI = i - period / 2 - 1;
                        int upperI = i + period / 2;

                        movingMeanSquare -= data[i - period / 2 - 1] * data[i - period / 2 - 1];
                        movingMeanSquare += data[i + period / 2] * data[i + period / 2];

                        output[i] = Math.Sqrt(movingMeanSquare / period);
                    }

                    for (int i = data.Length - period / 2; i < data.Length; i++)
                    {
                        movingMeanSquare = 0;

                        int thisCount = period / 2 + data.Length - i;

                        for (int j = i - period / 2; j < data.Length; j++)
                        {
                            movingMeanSquare += data[j] * data[j] / thisCount;
                        }
                        output[i] = Math.Sqrt(movingMeanSquare);
                    }

                    return output;
                }
                catch (Exception e)
                {
                    return new double[8];
                }
            }

            private static double MeanSquareError(double[] x, double[] y)
            {
                if (x.Length != y.Length)
                {
                    MessageBox.Show("Error: x and y arrays must have same length", "Regression.MeanSquareError");
                    return 0;
                }

                double mse = 0;

                for (int i = 0; i < x.Length; i++)
                {
                    mse += Math.Pow(x[i] - y[i], 2);
                }

                mse /= x.Length;

                return mse;
            }
        }

        public class NStestConditions
        {
            public static void Define(string testName, double freqOffsetHz, double bandwidthHz)
            {
                NStestConditions.Mem.Add(testName, new NStestConditions(freqOffsetHz, bandwidthHz));
            }
            public NStestConditions(double FreqOffsetHz, double BandwidthHz)
            {
                freqOffsetHz = FreqOffsetHz;
                bandwidthHz = BandwidthHz;
            }
            public static Dictionary.Ordered<string, NStestConditions> Mem = new Dictionary.Ordered<string, NStestConditions>();
            public double freqOffsetHz;
            public double bandwidthHz;
        }

        public static double DcLeakageExtrapolation(double[] Trace, double TraceLengthSeconds)
        {
            const double skipLengthSeconds = 0.01;
            const double maxTraceLengthForFitting = 0.1;
            const double extrapolateToSeconds = 0.5;
            const bool useRegressionSpline = true;

            try
            {
                double[] x = new double[Trace.Length];
                double aperture = TraceLengthSeconds / (double)x.Length;
                for (int i = 0; i < x.Length; i++) x[i] = (double)i * aperture;

                int skipPoints = (int)(skipLengthSeconds / aperture);  // ignore the first and last few milliseconds.

                double[] fittedArray = new double[x.Length];

                if (useRegressionSpline)
                {
                    alglib.spline1dinterpolant spline = new alglib.spline1dinterpolant();
                    alglib.spline1dfitreport report = new alglib.spline1dfitreport(); int info;
                    alglib.spline1dfitpenalized(x.Skip(skipPoints).ToArray(), Trace.Skip(skipPoints).ToArray(), (int)(100.0 * TraceLengthSeconds), -6, out info, out spline, out report);
                    for (int i = 0; i < x.Length; i++) fittedArray[i] = alglib.spline1dcalc(spline, x[i]);
                }
                else
                {
                    int period = (int)((double)Trace.Length * 0.002 / TraceLengthSeconds);
                    if (period > 1) fittedArray = Regression.MovingRMS(Trace, period);
                    else fittedArray = Trace.ToArray();
                }

                int takePoints = (int)(Math.Min(maxTraceLengthForFitting, TraceLengthSeconds) / aperture) - skipPoints * 2;
                if ((double)takePoints * aperture < 0.01) MessageBox.Show("DC Leakage trace must be at least " + ((double)skipLengthSeconds * 2.0 + 0.01) + " seconds.", "DcLeakageExtrapolation", MessageBoxButtons.OK, MessageBoxIcon.Error);

                double scaler = Math.Abs(fittedArray[skipPoints + takePoints - 1]);
                double maxYoffset = fittedArray[skipPoints + takePoints - 1];

                Dictionary<double, Tuple<double, double, double>> MSEs = new Dictionary<double, Tuple<double, double, double>>();
                Dictionary<double, double[]> x_offsetted = new Dictionary<double, double[]>();
                Dictionary<double, double[]> fittedArray_offsetted = new Dictionary<double, double[]>();

                for (double xOffset = 0e-3; xOffset <= 100e-3; xOffset += 5e-3)
                {
                    if (!x_offsetted.ContainsKey(xOffset))
                    {
                        x_offsetted[xOffset] = new double[x.Length];
                        for (int i = 0; i < x.Length; i++) x_offsetted[xOffset][i] = x[i] + xOffset;
                        x_offsetted[xOffset] = x_offsetted[xOffset].Skip(skipPoints).Take(takePoints).ToArray();
                    }

                    for (double yOffset = 0; Math.Abs(yOffset) <= Math.Abs(maxYoffset); yOffset += maxYoffset / 50)
                    {
                        if (!fittedArray_offsetted.ContainsKey(yOffset))
                        {
                            fittedArray_offsetted[yOffset] = new double[x.Length];
                            for (int i = 0; i < x.Length; i++) fittedArray_offsetted[yOffset][i] = (fittedArray[i] - yOffset) / scaler;
                            fittedArray_offsetted[yOffset] = fittedArray_offsetted[yOffset].Skip(skipPoints).Take(takePoints).ToArray();
                        }

                        Calc.Regression.SimplePower expReg = new Calc.Regression.SimplePower(x_offsetted[xOffset], fittedArray_offsetted[yOffset]);

                        MSEs[expReg.MSE] = new Tuple<double, double, double>(xOffset, yOffset, expReg.GetFittedValue(extrapolateToSeconds + xOffset) * scaler + yOffset);
                    }
                }

                if (false)
                {
                    double lowestMSE = MSEs.Keys.Min();
                    //double lowestMSE = MSEs.Keys.Max();
                    double finalXoffset = MSEs[lowestMSE].Item1;
                    double finalYoffset = MSEs[lowestMSE].Item2;
                    double finalI = MSEs[lowestMSE].Item3;
                    //finalXoffset = 0e-3;
                    //finalYoffset = 0e-8;

                    double[] x_finaloffsetted = new double[x.Length];
                    for (int i = 0; i < x.Length; i++) x_finaloffsetted[i] = x[i] + finalXoffset;

                    double[] fittedArray_finalOffsetted = new double[x.Length];
                    for (int i = 0; i < x.Length; i++) fittedArray_finalOffsetted[i] = (fittedArray[i] - finalYoffset) / scaler;
                    //for (int i = 0; i < x.Length; i++) fittedArray_finalOffsetted[i] = (dcLeakageTrace[pinName][i] - finalYoffset) / scaler;

                    Calc.Regression.SimplePower expRegFinal = new Calc.Regression.SimplePower(x_finaloffsetted.Skip(skipPoints).Take(takePoints).ToArray(), fittedArray_finalOffsetted.Skip(skipPoints).Take(takePoints).ToArray());

                    Console.WriteLine("Time\tData\tFullFit\tPartialFit " + takePoints * aperture + "s");
                    for (int i = 0; i < x.Length; i++)
                    {
                        Console.WriteLine(x[i] + "\t" + Trace[i] + "\t" + fittedArray[i] + "\t" + (expRegFinal.GetFittedValue(x_finaloffsetted[i]) * scaler + finalYoffset));
                    }
                }

                return MSEs[MSEs.Keys.Min()].Item3;
            }
            catch (Exception e)
            {
                return 1;
            }
        }

        public static class Charts
        {
            public static void CreateChart(string fileName, string chartTitle, double[] xPoints1, double[] yPoints1, double[] xPoints2, double[] yPoints2, string xAxis, string yAxis, double xAxisInterval, double yAxisMin, double yAxisMax, SeriesChartType chartType, bool showLegend, bool showChart, bool saveFile)
            {
                int plotHeight = 650;
                int plotWidth = 1000;

                Chart chart1 = new Chart();
                chart1.Name = "chart1";
                chart1.Titles.Add(chartTitle);
                chart1.Height = plotHeight;
                chart1.Width = plotWidth;
                chart1.ChartAreas.Add(chartTitle);
                chart1.Legends.Add(new Legend());

                //Series series1 = new Series();
                //series1.Name = "Series1";
                //series1.ChartType = chartType;
                //chart1.Series.Add(series1);
                string series1Name = "series1";
                chart1.Series.Add(series1Name);
                chart1.Series[series1Name].ChartType = chartType;
                chart1.ChartAreas[chartTitle].AxisX.Title = xAxis;
                chart1.ChartAreas[chartTitle].AxisY.Title = yAxis;

                chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 16);
                chart1.Titles[0].Alignment = System.Drawing.ContentAlignment.TopLeft;
                chart1.ChartAreas[chartTitle].AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisX.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);
                chart1.ChartAreas[chartTitle].AxisY.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);

                if (xPoints1.Length != 0)
                {
                    if (xPoints2.Length != 0)
                    {
                        chart1.ChartAreas[chartTitle].AxisX.Maximum = Math.Ceiling(Math.Max(xPoints1.Max(), xPoints2.Max()));
                        chart1.ChartAreas[chartTitle].AxisX.Minimum = Math.Floor(Math.Min(xPoints1.Min(), xPoints2.Min()));
                    }
                    else
                    {
                        chart1.ChartAreas[chartTitle].AxisX.Maximum = Math.Ceiling(xPoints1.Max());
                        chart1.ChartAreas[chartTitle].AxisX.Minimum = Math.Floor(xPoints1.Min());
                    }
                }

                if (!xAxisInterval.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisX.Interval = xAxisInterval;  // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.Interval;  // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval / 4;
                }
                if (!yAxisMin.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Minimum = yAxisMin;
                if (!yAxisMax.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Maximum = yAxisMax;
                if (!yAxisMin.Equals(double.NaN) & !yAxisMax.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisY.Interval = (yAxisMax - yAxisMin) / 10;     // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.Interval;     // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval / 4;
                }
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineColor = System.Drawing.Color.Gray;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineColor = System.Drawing.Color.Gray;


                chart1.Series[series1Name].MarkerSize = 2;   // set the point plots point size
                chart1.Series[series1Name].BorderWidth = 2;   // set the line plot line size

                chart1.Series[series1Name].IsVisibleInLegend = showLegend;   // display or hide the legend

                if (yPoints1.Length > 0)
                {
                    if (xPoints1.Length != 0)
                    {

                        int p = Math.Min(xPoints1.Length, yPoints1.Length);
                        for (int i = 0; i < p; i++)
                        {
                            chart1.Series[series1Name].Points.AddXY(xPoints1[i], yPoints1[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < yPoints1.Length; i++)
                        {
                            chart1.Series[series1Name].Points.AddY(yPoints1[i]);
                        }
                    }
                }
                else
                {
                    foreach (double y in yPoints1)
                    {
                        chart1.Series[series1Name].Points.AddY(y);
                    }

                }

                // add second series if exists
                if (yPoints2.Length > 0)
                {
                    string series2Name = "series2";
                    chart1.Series.Add(series2Name);
                    chart1.Series[series2Name].ChartType = SeriesChartType.Line;
                    chart1.Series[series2Name].BorderWidth = 2;   // set the line plot line size
                    chart1.Series[series2Name].IsVisibleInLegend = false;   // display or hide the legend

                    if (xPoints2.Length > 0)
                    {
                        SortedDictionary<double, double> series2Dic = new SortedDictionary<double, double>();   // must sort otherwise the lines connect non-continously

                        int p = Math.Min(xPoints2.Length, yPoints2.Length);
                        //double s = xPoints1.Max();

                        for (int i = 0; i < p; i++)
                        {
                            series2Dic.Add(xPoints2[i], yPoints2[i]);
                        }

                        foreach (double key in series2Dic.Keys)
                        {
                            chart1.Series[series2Name].Points.AddXY(key, series2Dic[key]);
                        }
                    }
                    else
                    {
                        foreach (double val in yPoints2)
                        {
                            chart1.Series[series2Name].Points.AddY(val);
                        }
                    }
                }


                if (saveFile)
                {
                    chart1.SaveImage(@"C:\" + fileName + ".gif", ChartImageFormat.Gif);   // gif is the smallest format
                }

                if (showChart)
                {
                    Form chart = new Form();
                    //chart.SuspendLayout();
                    //chart.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    //chart.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    chart.ClientSize = new System.Drawing.Size(plotWidth, plotHeight);
                    chart.Controls.Add(chart1);
                    //chart.Text = "chart text";
                    //chart.ResumeLayout(false);
                    chart.ShowDialog();
                }
            }

            public static void CreateChartList(string fileName, string chartTitle, List<double[]> xPoints, List<double[]> yPoints, string xAxis, string yAxis, double xAxisInterval, double yAxisMin, double yAxisMax, SeriesChartType chartType, bool showLegend, bool showChart, bool saveFile)
            {
                int plotHeight = 650;
                int plotWidth = 1000;

                Chart chart1 = new Chart();
                chart1.Name = "chart1";
                chart1.Titles.Add(chartTitle);
                chart1.Height = plotHeight;
                chart1.Width = plotWidth;
                chart1.ChartAreas.Add(chartTitle);
                chart1.Legends.Add(new Legend());

                chart1.ChartAreas[chartTitle].AxisX.Title = xAxis;
                chart1.ChartAreas[chartTitle].AxisY.Title = yAxis;

                chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 16);
                chart1.ChartAreas[chartTitle].AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisX.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);
                chart1.ChartAreas[chartTitle].AxisY.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);

                List<int> xlengthsList = new List<int>(xPoints.Count());
                List<double> maxXlist = new List<double>(xPoints.Count());
                List<double> minXlist = new List<double>(xPoints.Count());
                for (int i = 0; i < xPoints.Count(); i++)
                {
                    xlengthsList.Add(xPoints[i].Count());
                    maxXlist.Add(xPoints[i].Max());
                    minXlist.Add(xPoints[i].Min());
                }

                if (xlengthsList.Max() > 0)
                {
                    chart1.ChartAreas[chartTitle].AxisX.Maximum = maxXlist.Max();
                    chart1.ChartAreas[chartTitle].AxisX.Minimum = minXlist.Min();
                }



                if (!xAxisInterval.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisX.Interval = xAxisInterval;  // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.Interval;  // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval / 4;
                }
                if (!yAxisMin.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Minimum = yAxisMin;
                if (!yAxisMax.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Maximum = yAxisMax;
                if (!yAxisMin.Equals(double.NaN) & !yAxisMax.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisY.Interval = (yAxisMax - yAxisMin) / 10;     // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.Interval;     // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval / 4;
                }
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineColor = System.Drawing.Color.Gray;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineColor = System.Drawing.Color.Gray;



                //if (yPoints1.Length > 0)
                //{
                //    if (xPoints1.Length != 0)
                //    {

                //        int p = Math.Min(xPoints1.Length, yPoints1.Length);
                //        for (int i = 0; i < p; i++)
                //        {
                //            chart1.Series[series1Name].Points.AddXY(xPoints1[i], yPoints1[i]);
                //        }
                //    }
                //    else
                //    {
                //        for (int i = 0; i < yPoints1.Length; i++)
                //        {
                //            chart1.Series[series1Name].Points.AddY(yPoints1[i]);
                //        }
                //    }
                //}
                //else
                //{
                //    foreach (double y in yPoints1)
                //    {
                //        chart1.Series[series1Name].Points.AddY(y);
                //    }

                //}

                // add second series if exists
                //if (yPoints2.Length > 0)
                //{
                //    string series2Name = "series2";
                //    chart1.Series.Add(series2Name);
                //    chart1.Series[series2Name].ChartType = SeriesChartType.Line;
                //    chart1.Series[series2Name].BorderWidth = 2;   // set the line plot line size
                //    chart1.Series[series2Name].IsVisibleInLegend = false;   // display or hide the legend

                //    if (xPoints2.Length > 0)
                //    {
                //        SortedDictionary<double, double> series2Dic = new SortedDictionary<double, double>();   // must sort otherwise the lines connect non-continously

                //        int p = Math.Min(xPoints2.Length, yPoints2.Length);
                //        //double s = xPoints1.Max();

                //        for (int i = 0; i < p; i++)
                //        {
                //            series2Dic.Add(xPoints2[i], yPoints2[i]);
                //        }

                //        foreach (double key in series2Dic.Keys)
                //        {
                //            chart1.Series[series2Name].Points.AddXY(key, series2Dic[key]);
                //        }
                //    }
                //    else
                //    {
                //        foreach (double val in yPoints2)
                //        {
                //            chart1.Series[series2Name].Points.AddY(val);
                //        }
                //    }
                //}

                for (int series = 0; series < yPoints.Count(); series++)
                {
                    string seriesName = "series2" + series.ToString();
                    chart1.Series.Add(seriesName);
                    chart1.Series[seriesName].ChartType = chartType;
                    chart1.Series[seriesName].MarkerSize = 2;   // set the point plots point size
                    chart1.Series[seriesName].BorderWidth = 2;   // set the line plot line size
                    chart1.Series[seriesName].IsVisibleInLegend = showLegend;   // display or hide the legend

                    if (xPoints[series].Length > 0)
                    {
                        SortedDictionary<double, double> seriesDic = new SortedDictionary<double, double>();   // must sort otherwise the lines connect non-continously

                        int points = Math.Min(xPoints[series].Length, yPoints[series].Length);

                        for (int point = 0; point < points; point++)
                        {
                            seriesDic.Add(xPoints[series][point], yPoints[series][point]);
                        }

                        foreach (double key in seriesDic.Keys)
                        {
                            chart1.Series[seriesName].Points.AddXY(key, seriesDic[key]);
                        }
                    }
                    else
                    {
                        foreach (double val in yPoints[series])
                        {
                            chart1.Series[seriesName].Points.AddY(val);
                        }

                    }

                    //if (xPoints[series].Length != 0)
                    //{
                    //    int p = Math.Min(xPoints[series].Length, yPoints[series].Length);
                    //    for (int i = 0; i < p; i++)
                    //    {
                    //        chart1.Series[series1Name].Points.AddXY(xPoints1[i], yPoints1[i]);
                    //    }

                    //}

                }


                if (saveFile)
                {
                    chart1.SaveImage(@"C:\" + fileName + ".gif", ChartImageFormat.Gif);   // gif is the smallest format
                }

                if (showChart)
                {
                    Form chart = new Form();
                    //chart.SuspendLayout();
                    //chart.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    //chart.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    chart.ClientSize = new System.Drawing.Size(plotWidth, plotHeight);
                    chart.Controls.Add(chart1);
                    //chart.Text = "chart text";
                    //chart.ResumeLayout(false);
                    chart.ShowDialog();
                }
            }

            public static void CreateChartPinSweep(string fileName, string chartTitle, Dictionary<string, double[]> xPoints, Dictionary<string, double[]> yPoints, string xAxis, string yAxis, double xAxisInterval, double yAxisMin, double yAxisMax, Dictionary<string, SeriesChartType> chartTypes, Dictionary<string, int> markerSizes, Dictionary<string, bool> showDataLabels, bool showLegend, bool showChart, bool saveFile)
            {
                int plotHeight = 650;
                int plotWidth = 1000;

                Chart chart1 = new Chart();
                chart1.Name = "chart1";
                chart1.Titles.Add(chartTitle);
                chart1.Height = plotHeight;
                chart1.Width = plotWidth;
                chart1.ChartAreas.Add(chartTitle);
                chart1.Legends.Add(new Legend());

                chart1.ChartAreas[chartTitle].AxisX.Title = xAxis;
                chart1.ChartAreas[chartTitle].AxisY.Title = yAxis;

                chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 16);
                chart1.ChartAreas[chartTitle].AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisX.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);
                chart1.ChartAreas[chartTitle].AxisY.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);

                List<int> xlengthsList = new List<int>(xPoints.Count());
                List<double> maxXlist = new List<double>(xPoints.Count());
                List<double> minXlist = new List<double>(xPoints.Count());
                //for (int i = 0; i < xPoints.Count(); i++)
                foreach (string seriesName in xPoints.Keys)
                {
                    xlengthsList.Add(xPoints[seriesName].Count());
                    maxXlist.Add(xPoints[seriesName].Max());
                    minXlist.Add(xPoints[seriesName].Min());
                }

                if (xlengthsList.Max() > 0)
                {
                    chart1.ChartAreas[chartTitle].AxisX.Maximum = Math.Ceiling(maxXlist.Max());
                    chart1.ChartAreas[chartTitle].AxisX.Minimum = Math.Floor(minXlist.Min());
                }



                if (!xAxisInterval.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisX.Interval = xAxisInterval;  // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.Interval;  // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval / 4;
                }
                if (!yAxisMin.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Minimum = yAxisMin;
                if (!yAxisMax.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Maximum = yAxisMax;
                if (!yAxisMin.Equals(double.NaN) & !yAxisMax.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisY.Interval = 1; // (yAxisMax - yAxisMin) / 10;     // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.Interval;     // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval / 4;
                }
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineColor = System.Drawing.Color.Gray;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineColor = System.Drawing.Color.Gray;

                foreach (string seriesName in yPoints.Keys)
                {
                    //string seriesName = "series2" + series.ToString();
                    chart1.Series.Add(seriesName);
                    chart1.Series[seriesName].ChartType = chartTypes.ContainsKey(seriesName) ? chartTypes[seriesName] : SeriesChartType.Line;
                    chart1.Series[seriesName].MarkerSize = markerSizes.ContainsKey(seriesName) ? markerSizes[seriesName] : 2;   // set the point plots point size
                    chart1.Series[seriesName].MarkerStyle = MarkerStyle.Cross;   // set the point plots point size
                    chart1.Series[seriesName].BorderWidth = markerSizes.ContainsKey(seriesName) ? markerSizes[seriesName] : 2;   // set the line plot line size
                    chart1.Series[seriesName].IsVisibleInLegend = showLegend;   // display or hide the legend
                    if (showDataLabels[seriesName]) chart1.Series[seriesName].Label = seriesName + "\nGain: " + yPoints[seriesName][0].ToString("00.00") + "dB" + "\nPin: " + xPoints[seriesName][0].ToString("00.00") + "dBm";   //"Y = #VALY\nX = #VALX";

                    if (xPoints[seriesName].Length > 0)
                    {
                        SortedDictionary<double, double> seriesDic = new SortedDictionary<double, double>();   // must sort otherwise the lines connect non-continously

                        int points = Math.Min(xPoints[seriesName].Length, yPoints[seriesName].Length);

                        for (int point = 0; point < points; point++)
                        {
                            seriesDic.Add(xPoints[seriesName][point], yPoints[seriesName][point]);
                        }

                        foreach (double key in seriesDic.Keys)
                        {
                            chart1.Series[seriesName].Points.AddXY(key, seriesDic[key]);
                        }
                    }
                    else
                    {
                        foreach (double val in yPoints[seriesName])
                        {
                            chart1.Series[seriesName].Points.AddY(val);
                        }

                    }

                    //if (xPoints[series].Length != 0)
                    //{
                    //    int p = Math.Min(xPoints[series].Length, yPoints[series].Length);
                    //    for (int i = 0; i < p; i++)
                    //    {
                    //        chart1.Series[series1Name].Points.AddXY(xPoints1[i], yPoints1[i]);
                    //    }

                    //}

                }


                if (saveFile)
                {
                    chart1.SaveImage(fileName, ChartImageFormat.Jpeg);   // gif is the smallest format
                }

                if (showChart)
                {
                    Form chart = new Form();
                    //chart.SuspendLayout();
                    //chart.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    //chart.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    chart.ClientSize = new System.Drawing.Size(plotWidth, plotHeight);
                    chart.Controls.Add(chart1);
                    //chart.Text = "chart text";
                    //chart.ResumeLayout(false);
                    chart.ShowDialog();
                }
            }

            public static void CreateChartAMAM(string fileName, string chartTitle, double[] xPoints1, double[] yPoints1, double[] xPoints2, double[] yPoints2, string xAxis, string yAxis, double xAxisInterval, double yAxisMin, double yAxisMax, SeriesChartType chartType, bool showLegend, bool showChart, bool saveFile)
            {
                int plotHeight = 650;
                int plotWidth = 1000;

                Chart chart1 = new Chart();
                chart1.Name = "chart1";
                chart1.Titles.Add(chartTitle);
                chart1.Height = plotHeight;
                chart1.Width = plotWidth;
                chart1.ChartAreas.Add(chartTitle);
                chart1.Legends.Add(new Legend());

                //Series series1 = new Series();
                //series1.Name = "Series1";
                //series1.ChartType = chartType;
                //chart1.Series.Add(series1);
                string series1Name = "series1";
                chart1.Series.Add(series1Name);
                chart1.Series[series1Name].ChartType = chartType;
                chart1.ChartAreas[chartTitle].AxisX.Title = xAxis;
                chart1.ChartAreas[chartTitle].AxisY.Title = yAxis;

                chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 16);
                chart1.ChartAreas[chartTitle].AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14);
                chart1.ChartAreas[chartTitle].AxisX.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);
                chart1.ChartAreas[chartTitle].AxisY.LabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);

                if (xPoints1.Length != 0)
                {
                    if (xPoints2.Length != 0)
                    {
                        chart1.ChartAreas[chartTitle].AxisX.Maximum = Math.Ceiling(Math.Max(xPoints1.Max(), xPoints2.Max()));
                        chart1.ChartAreas[chartTitle].AxisX.Minimum = Math.Floor(Math.Min(xPoints1.Min(), xPoints2.Min()));
                    }
                    else
                    {
                        chart1.ChartAreas[chartTitle].AxisX.Maximum = Math.Ceiling(xPoints1.Max());
                        chart1.ChartAreas[chartTitle].AxisX.Minimum = Math.Floor(xPoints1.Min());
                    }
                }

                if (!xAxisInterval.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisX.Interval = xAxisInterval;  // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.Interval;  // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisX.MajorGrid.Interval / 4;
                }
                if (!yAxisMin.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Minimum = yAxisMin;
                if (!yAxisMax.Equals(double.NaN)) chart1.ChartAreas[chartTitle].AxisY.Maximum = yAxisMax;
                if (!yAxisMin.Equals(double.NaN) & !yAxisMax.Equals(double.NaN))
                {
                    chart1.ChartAreas[chartTitle].AxisY.Interval = (yAxisMax - yAxisMin) / 10;     // sets the interval of major labels
                    chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.Interval;     // sets the interval of major gridlines
                    chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Interval = chart1.ChartAreas[chartTitle].AxisY.MajorGrid.Interval / 4;
                }
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.Enabled = true;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
                chart1.ChartAreas[chartTitle].AxisX.MinorGrid.LineColor = System.Drawing.Color.Gray;
                chart1.ChartAreas[chartTitle].AxisY.MinorGrid.LineColor = System.Drawing.Color.Gray;


                chart1.Series[series1Name].MarkerSize = 2;   // set the point plots point size
                chart1.Series[series1Name].BorderWidth = 2;   // set the line plot line size

                chart1.Series[series1Name].IsVisibleInLegend = showLegend;   // display or hide the legend

                if (yPoints1.Length > 0)
                {
                    if (xPoints1.Length != 0)
                    {

                        int p = Math.Min(xPoints1.Length, yPoints1.Length);
                        for (int i = 0; i < p; i++)
                        {
                            chart1.Series[series1Name].Points.AddXY(xPoints1[i], yPoints1[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < yPoints1.Length; i++)
                        {
                            chart1.Series[series1Name].Points.AddY(yPoints1[i]);
                        }
                    }
                }
                else
                {
                    foreach (double y in yPoints1)
                    {
                        chart1.Series[series1Name].Points.AddY(y);
                    }

                }

                // add second series if exists
                if (yPoints2.Length > 0)
                {
                    string series2Name = "series2";
                    chart1.Series.Add(series2Name);
                    chart1.Series[series2Name].ChartType = SeriesChartType.Line;
                    chart1.Series[series2Name].BorderWidth = 2;   // set the line plot line size
                    chart1.Series[series2Name].IsVisibleInLegend = false;   // display or hide the legend

                    if (xPoints2.Length > 0)
                    {
                        SortedDictionary<double, double> series2Dic = new SortedDictionary<double, double>();   // must sort otherwise the lines connect non-continously

                        int p = Math.Min(xPoints2.Length, yPoints2.Length);
                        //double s = xPoints1.Max();

                        for (int i = 0; i < p; i++)
                        {
                            series2Dic.Add(xPoints2[i], yPoints2[i]);
                        }

                        foreach (double key in series2Dic.Keys)
                        {
                            chart1.Series[series2Name].Points.AddXY(key, series2Dic[key]);
                        }
                    }
                    else
                    {
                        foreach (double val in yPoints2)
                        {
                            chart1.Series[series2Name].Points.AddY(val);
                        }
                    }
                }


                if (saveFile)
                {
                    //chart1.SaveImage(thisETdata.FileDirectory + fileName + ".gif", ChartImageFormat.Gif);   // gif is the smallest format
                }

                if (showChart)
                {
                    Form chart = new Form();
                    //chart.SuspendLayout();
                    //chart.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    //chart.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    chart.ClientSize = new System.Drawing.Size(plotWidth, plotHeight);
                    chart.Controls.Add(chart1);
                    //chart.Text = "chart text";
                    //chart.ResumeLayout(false);
                    chart.ShowDialog();
                }
            }

        }

        public static void WriteIqDebugFile(string filePath, ref niComplexNumber[] iqData)
        {
            try
            {
                using (StreamWriter debugFile = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    foreach (niComplexNumber n in iqData)
                    {
                        debugFile.WriteLine(n.Real + "," + n.Imaginary);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }

}
