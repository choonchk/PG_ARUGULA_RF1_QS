using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;

namespace LibFBAR
{
    public class cMM
    {
        #region "Enumerations"
       
        #endregion
        #region "Structure"
        
        #endregion
        #region "Flags"
        static bool Fail_Flag;
        #endregion

        public static string ClassName = "Multi-Market Class";

        static LibFBAR.cFunction Math_Func = new LibFBAR.cFunction();
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  13/02/2012       KKL             New and example for Multi-Market class new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        #region "Equipment Declarations"
        static FormattedIO488 ioMXA = new FormattedIO488();
        protected static cMXA_N9020A MXA = new cMXA_N9020A();
        protected static string NA_Name;
        public FormattedIO488 parseNA_IO
        {
            set
            {
                ioMXA = value;
                InitEquipment();
            }
        }
        public void InitEquipment()
        {
            MXA.parseIO = ioMXA;
            MXA.Init(ioMXA);
            NA_Name = "MXA N9020A";
        }  //Modify this for new equipment 
        public void InitEq(string address)
        {
            if (address != "" && address != null)
            {
                MXA.Address = address;
                MXA.OpenIO();
            }
        }
        #endregion

        #region "Results Declarations"
        private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        public s_Result[] Result_setting
        {
            get
            {
                return SaveResult;
            }
        }     // Getting the internal data to public
        public void Init(int Tests)
        {
            SaveResult = new s_Result[Tests + 1];
        }
        public void Clear_Results()
        {
            for (int iClear = 0; iClear < SaveResult.Length; iClear++)
            {
                if (SaveResult[iClear].b_MultiResult)
                {
                    for (int iSubClear = 0; iSubClear < SaveResult[iClear].Multi_Results.Length; iSubClear++)
                    {
                        SaveResult[iClear].Multi_Results[iSubClear].Result_Data = 0;
                    }
                }
                SaveResult[iClear].Result_Data = 0;
            }
        }
        #endregion





        public cTestClasses[] TestClass;
        public class cTestClasses
        {
            public cNF_Gain NF_Gain;

            public interface iTestFunction
            {
                void RunTest();
                void InitSettings();
                void MeasureResult();
            }
            public class cNF_Gain : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "NF Gain Function Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public string Misc_Settings;

                public string StateFile;
                public double Frequency;
                public double Loss_Input;
                public double Loss_Output;
                public double NF_Offset;
                public double Gain_Offset;
                public int Average_Data_Count;
                

                // Internal Variables
                private double NF_Data;
                private double Gain_Data;

                #endregion

                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    if (Average_Data_Count == 0)
                    {
                        Average_Data_Count = 2;
                    }

                    // Initializing Multiple Results Output
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = true;
                    SaveResult[TestNo].Multi_Results = new s_mRslt[2];
                    SaveResult[TestNo].Multi_Results[1].Enable = true;
                    SaveResult[TestNo].Multi_Results[0].Result_Header = "NF";
                    SaveResult[TestNo].Multi_Results[1].Enable = true;
                    SaveResult[TestNo].Multi_Results[1].Result_Header = "Gain";
                }
                public void RunTest()
                {
                    if (Fail_Flag)
                    {
                        SaveResult[TestNo].Multi_Results[0].Result_Data = -999;
                        SaveResult[TestNo].Multi_Results[1].Result_Data = -999;
                    }
                    else
                    {
                        MeasureResult();
                        SetResult();
                    }
                }

                public void MeasureResult()
                {
                    MXA.BasicCommand.SendCommand("INST:SEL NFIGURE");
                    if (StateFile != "" && StateFile != null)
                    {
                        MXA.BasicCommand.SendCommand("MMEM:LOAD:STAT '" + StateFile + "'");
                        MXA.BasicCommand.SendCommand("SENS:AVER:COUN " + Average_Data_Count);
                        MXA.BasicCommand.SendCommand("INIT:IMM");
                        MXA.BasicCommand.System.Operation_Complete();

                        NF_Data = Convert.ToDouble( MXA.BasicCommand.ReadCommand("FETCH:SCAL:CORR:NFIG?"));
                        Gain_Data = Convert.ToDouble(MXA.BasicCommand.ReadCommand("FETCH:SCAL:CORR:GAIN?"));
                    }
                }
                public void SetResult()
                {
                    // Parsing Multiple Results
                    SaveResult[TestNo].Multi_Results[0].Result_Data = NF_Data;
                    SaveResult[TestNo].Multi_Results[1].Result_Data = Gain_Data;
                }
            }
        }
    }
}
