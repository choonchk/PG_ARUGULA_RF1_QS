using System;
using System.Threading;
using EqLib;

namespace SparamTestLib
{
    public class SparamDcTest : SparamTestCase.TestCaseAbstract 
    {
        protected EqDC.iEqDC _EqSMU;

        public EqDC.iEqDC EqSmu
        {
            get { return _EqSMU; }
            set { _EqSMU = value; }
        }

        public override bool Initialize(bool finalScript)
        {
            _Result = new SResult();

            if (TcfHeader.ToUpper().Contains("LEAKAGE"))
            {
                Array.Resize(ref _Result.Result, 9);
                Array.Resize(ref _Result.Header, 9);

                _Result.Header[0] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_VBATT");
                _Result.Result[0] = -9999;
                _Result.Header[1] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_VCC");
                _Result.Result[1] = -9999;
                _Result.Header[2] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_VLNA");
                _Result.Result[2] = -9999;
                _Result.Header[3] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_VIO1");
                _Result.Result[3] = -9999;
                _Result.Header[4] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_SDATA1");
                _Result.Result[4] = -9999;
                _Result.Header[5] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_SCLK1");
                _Result.Result[5] = -9999;
                _Result.Header[6] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_VIO2");
                _Result.Result[6] = -9999;
                _Result.Header[7] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_SDATA2");
                _Result.Result[7] = -9999;
                _Result.Header[8] = TcfHeader.Replace("LEAKAGE", "LEAKAGE_SCLK2");
                _Result.Result[8] = -9999;
            }
            else
            {
                Array.Resize(ref _Result.Result, 1);
                Array.Resize(ref _Result.Header, 1);

                _Result.Header[0] = TcfHeader;
                _Result.Result[0] = -9999;
            }
            _Result.Enable = true;

          
            

            return true;

        }

        public override int RunTest()
        {
            double rtnVal = 0;

            SResult rtnResult = _Result;

            //_EqSMU.MeasureCurrent(10);

            //_Result.Result[0] = rtnVal;

            bool Flag = false;
            string Vt = "";

            string ParameterHeader = _Result.Header[0];

            //Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.1);
            //Eq.Site[0].DC["Vcc"].ForceVoltage(0, 0.1);
            //Eq.Site[0].DC["Vlna"].ForceVoltage(0, 0.1);
            //Eq.Site[0].DC["Vbiast"].ForceVoltage(0, 0.1);
            //Eq.Site[0].DC["Vlna"].ForceVoltage(0, 0.1);

            if (ParameterHeader.Contains("TEMP"))
            {
                //int Temp = 0;
                //HSDIO.Instrument.SendVectorTEMP(ref Temp);

                //string Hex = Convert.ToString(Convert.ToInt32(Temp), 16).ToUpper();
                //int TempCount = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
                //RsltData = (TempCount - 1) * (Convert.ToSingle(150f / 254f)) - 20;
                //SaveResult[TestNo].Result_Data = RsltData;
                //SaveResult[TestNo].Enable = true;
                //SaveResult[TestNo].Result_Header = "MIPI_TEMP1";
            }
            else if (ParameterHeader.ToUpper().Contains("LEAKAGE"))
            {
                
                Read_Bias(ref rtnResult, _Result.Header[0]);

            }
            else if (ParameterHeader.ToUpper().Contains("IDLECURRENT_LNA"))
            {            
                Read_Bias_Icq(Band, ref rtnResult);
            }
            else if (ParameterHeader.ToUpper().Contains("IDLECURRENT_BIAST"))
            {
                Read_BiasT_Icq(PowerMode, ref rtnResult);             
            }
            else if (ParameterHeader.ToUpper().Contains("IDLECURRENT_CPL"))
            {
                Read_CPL_Icq(PowerMode, ref rtnResult);
            }
            else
            {


            }


            Eq.Site[0].DC["Vbatt"].ForceVoltage(0, 0.1);
            Eq.Site[0].DC["Vcc"].ForceVoltage(0, 0.1);
            Eq.Site[0].DC["Vlna"].ForceVoltage(0, 0.1);

            _Result = rtnResult;
            return 0;



        }

        private void Read_Bias(ref SResult NA_Leakage, string headerstring)
        {


            double data = 0f;
            int MipiRead = 0;

            Thread.Sleep(200);   /// back

            NA_Leakage.Enable = true;

            string headerholder = headerstring;

            //Array.Resize(ref NA_Leakage.Result, 11);
            //Array.Resize(ref NA_Leakage.Header, 11);
            foreach (string pinName in SmuSettingsDictNA.Keys)
                Eq.Site[0].DC[pinName].ForceVoltage(SmuSettingsDictNA[pinName].Volts, SmuSettingsDictNA[pinName].Current);
            

            foreach (string key in SmuSettingsDictNA.Keys)
            {
                //PaTest.SmuResources[DC_matching[iRead].Address].MeasureCurrent(1, false, ref data);
                data = Eq.Site[0].DC[key].MeasureCurrent(1);
                
                
                switch (key)
                {
                    case "Vbatt":
                        NA_Leakage.Result[0] = Math.Abs(data);
                        //NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper()); // _4.5V"; //example is VBATT_LEAKAGE_4.5
                        MipiRead++;
                        break;

                    case "Vcc":
                        NA_Leakage.Result[1] = Math.Abs(data);
                        //NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper());
                        //MipiRead++;
                        break;

                    case "Vlna":
                        NA_Leakage.Result[2] = Math.Abs(data);
                        //NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper());
                        //MipiRead++;
                        break;

                    //case "Vdd":
                    //    NA_Leakage.Result[] = Math.Abs(data);
                    //    NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper());
                    //    MipiRead++;
                        //break;

                }

                
            }

            Thread.Sleep(2);

            foreach (string key in SmuSettingsDictNA.Keys)
            {
                switch (key)
                {
                    case "Sdata1":
                    case "Sclk1":
                    case "Vio1":
                    case "Sdata2":
                    case "Sclk2":
                    case "Vio2":
                        //PaTest.SmuResources[key].ForceVoltage(1.95, 0.0000019);
                        Eq.Site[0].DC[key].ForceVoltage(1.95, 0.0000019);
                        break;
                }
            }



            foreach (string key in SmuSettingsDictNA.Keys)
            {
                data = Eq.Site[0].DC[key].MeasureCurrent(1);
                switch (key)
                {
                    case "Sdata1":
                        NA_Leakage.Result[4] = Math.Abs(data);
                        break;
                    case "Sclk1":
                        NA_Leakage.Result[5] = Math.Abs(data);
                        break;
                    case "Sdata2":
                        NA_Leakage.Result[7] = Math.Abs(data);
                        break;
                    case "Sclk2":
                        NA_Leakage.Result[8] = Math.Abs(data);
                        break;
                    case "Vio2":
                        NA_Leakage.Result[6] = Math.Abs(data);
                        break;
                        //PaTest.SmuResources[key].MeasureCurrent(1, false, ref data);

                        //NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper().Replace("_", ""));
                        //NA_Leakage.Result[5] = Math.Abs(data);
                        ////Mipi/*Read++;*/
                        //break;

                    case "Vio1":
                        //PaTest.SmuResources[key].MeasureCurrent(1, false, ref data);
                        //data = Eq.Site[0].DC[key].MeasureCurrent(1);
                        //NA_Leakage.Header[MipiRead] = headerholder.Replace("LEAKAGE", "LEAKAGE_" + key.ToUpper().Replace("_", ""));
                        NA_Leakage.Result[3] = Math.Abs(data);
                        //MipiRead++;
                        break;

                }

            }

            foreach (string key in SmuSettingsDictNA.Keys)
            {
                switch (key)
                {
                    case "Sdata1":
                    case "Sclk1":
                    case "Vio1":
                    case "Sdata2":
                    case "Sclk2":
                    case "Vio2":
                        //PaTest.SmuResources[key].PostLeakageTest();
                        Eq.Site[0].DC[key].PostLeakageTest();
                        MipiRead++;
                        break;
                }

            }



        }

        private void Read_Bias_Icq(string Band, ref SResult NA_Leakage)
        {

                double data = 0f;

                Eq.Site[0].DC["Vlna"].SetupCurrentMeasure(0.0003, TriggerLine.None);

                Thread.Sleep(1);  //follows PA test               

                data = Eq.Site[0].DC["Vlna"].MeasureCurrent(1);

                NA_Leakage.Result[0] = data;

        }

        private void Read_BiasT_Icq(string Band, ref SResult NA_Leakage)
        {
            
                double data = 0f;

                //PaTest.SmuResources[DC_matching[2].Address].SetupCurrentMeasure(false, false);
                Eq.Site[0].DC["Vbiast"].SetupCurrentMeasure(0.0003, TriggerLine.None);
                Thread.Sleep(50);  //follows PA test               

                //PaTest.SmuResources[DC_matching[2].Address].MeasureCurrent(2, false, ref data); //2 = Vdd
                data = Eq.Site[0].DC["Vbiast"].MeasureCurrent(1);

                NA_Leakage.Result[0] = Math.Abs(data);

        }

        private void Read_CPL_Icq(string Band, ref SResult NA_Leakage)
        {
            
                double data = 0f;

                Eq.Site[0].DC["Vcpl"].SetupCurrentMeasure(0.0003, TriggerLine.None);
                Thread.Sleep(2);  //follows PA test               

                //PaTest.SmuResources[DC_matching[4].Address].MeasureCurrent(1, false, ref data); //2 = Vdd
                data = Eq.Site[0].DC["Vcpl"].MeasureCurrent(1);
                NA_Leakage.Result[0] = data;
        }



    }
}
