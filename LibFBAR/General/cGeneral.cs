using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ivi.Visa.Interop;

namespace LibFBAR
{
    public class cGeneral
    {
        public void DisplayError(string ClassName, string ErrParam, string ErrDesc)
        {
            MessageBox.Show("Class Name: " + ClassName + "\nParameters: " + ErrParam + "\n\nErrorDesciption: \n"
                + ErrDesc, "Error found in Class " + ClassName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }
        public void DisplayMessage(string ClassName, string DescParam, string DescDetail)
        {
            MessageBox.Show("Class Name: " + ClassName + "\nParameters: " + DescParam + "\n\nDesciption: \n"
                + DescDetail, ClassName, MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
        public static void Pause(int MilliSecondsToPauseFor)
        {

            System.DateTime ThisMoment = System.DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(0, 0, 0, 0, MilliSecondsToPauseFor);
            System.DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = System.DateTime.Now;
            }
        }
        public static void Pause(double MilliSecondsToPauseFor)
        {

            System.DateTime ThisMoment = System.DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(0, 0, 0, 0, (int)MilliSecondsToPauseFor);
            System.DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = System.DateTime.Now;
            }
        }
        public static int MatchChannel2Array(int Channel)
        {
            switch (Channel)
            {
                case 0:
                    return (0);
                case 1:
                    return (1);
                case 2:
                    return (2);
                case 4:
                    return (3);
                case 6:
                    return (4);
                case 7:
                    return (5);
                case 8:
                    return (6);
                default:
                    return (-999);
            }
        }
        public bool CInt2Bool(int Input)
        {
            if (Input == 0)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }
        public int CBool2Int(bool Input)
        {
            if (Input == true)
            {
                return (1);
            }
            else
            {
                return (0);
            }
        }
        public bool CStr2Bool(string Input)
        {
            if (Input.Trim() == "1" || Input.ToUpper().Trim() == "YES" || Input.ToUpper().Trim() == "ON" || Input.ToUpper().Trim() == "V")
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public string convertStr2StrVal(string input)
        {
            string[] tmpStr;
            double tmpVal;

            tmpStr = input.Split(' ');

            tmpVal = Convert.ToDouble(tmpStr[0]);

            if (tmpStr.Length < 2)
            {
                return (tmpStr[0]);
            }
            else
            {
                switch (tmpStr[1].Substring(1, 1))
                {
                    case "G":
                        return ((tmpVal * (10 ^ 9)).ToString());
                    case "M":
                        return ((tmpVal * (10 ^ 6)).ToString());
                    case "K":
                    case "k":
                        return ((tmpVal * (10 ^ 3)).ToString());
                    case "m":
                        return ((tmpVal * (10 ^ -3)).ToString());
                    case "u":
                        return ((tmpVal * (10 ^ -6)).ToString());
                    case "n":
                        return ((tmpVal * (10 ^ -9)).ToString());
                    case "p":
                        return ((tmpVal * (10 ^ -12)).ToString());
                    default:
                        return (tmpVal.ToString());
                }
            }
        }
        public double convertStr2Val(string input)
        {
            string[] tmpStr;
            double tmpVal;
            string tmpChar;
            tmpStr = input.Split(' ');

            tmpVal = Convert.ToDouble(tmpStr[0]);

            if (tmpStr.Length < 2)
            {
                return (Convert.ToDouble(tmpStr[0]));
            }
            else
            {
                if (tmpStr[1].Length == 1)
                {
                    tmpChar = tmpStr[1];
                }
                else
                {
                    tmpChar = tmpStr[1].Substring(0, 1);
                }
                switch (tmpChar)
                {
                    case "G":
                        return (Math.Round(tmpVal * 1000000000));
                    case "M":
                        return (Math.Round(tmpVal * Math.Pow(10, 6)));
                    case "K":
                    case "k":
                        return (Math.Round(tmpVal * Math.Pow(10, 3)));
                    case "m":
                        return ((tmpVal * Math.Pow(10, -3)));
                    case "u":
                        return ((tmpVal * Math.Pow(10, -6)));
                    case "n":
                        return ((tmpVal * Math.Pow(10, -9)));
                    case "p":
                        return ((tmpVal * Math.Pow(10, -12)));
                    default:
                        return (tmpVal);
                }
            }
        }
        public bool convertAutoStr2Bool(string input)
        {
            if (input.ToUpper() == "AUTO")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool convertEnableDisable2Bool(string input)
        {
            if (input.ToUpper() == "ENABLE")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string convertInt2ExcelColumn(int Column)
        {
            int iRemain;
            int iValue;
            iValue = Column / 26;
            iRemain = Column % 26;
            return (cInt2ExcelColumn(iValue) + cInt2ExcelColumn(iRemain));
        }
        string cInt2ExcelColumn(int input)
        {
            string strOut;
            strOut = "";
            switch (input)
            {
                case 1:
                    strOut = "A";
                    break;
                case 2:
                    strOut = "B";
                    break;
                case 3:
                    strOut = "C";
                    break;
                case 4:
                    strOut = "D";
                    break;
                case 5:
                    strOut = "E";
                    break;
                case 6:
                    strOut = "F";
                    break;
                case 7:
                    strOut = "G";
                    break;
                case 8:
                    strOut = "H";
                    break;
                case 9:
                    strOut = "I";
                    break;
                case 10:
                    strOut = "J";
                    break;
                case 11:
                    strOut = "K";
                    break;
                case 12:
                    strOut = "L";
                    break;
                case 13:
                    strOut = "M";
                    break;
                case 14:
                    strOut = "N";
                    break;
                case 15:
                    strOut = "O";
                    break;
                case 16:
                    strOut = "P";
                    break;
                case 17:
                    strOut = "Q";
                    break;
                case 18:
                    strOut = "R";
                    break;
                case 19:
                    strOut = "S";
                    break;
                case 20:
                    strOut = "T";
                    break;
                case 21:
                    strOut = "U";
                    break;
                case 22:
                    strOut = "V";
                    break;
                case 23:
                    strOut = "W";
                    break;
                case 24:
                    strOut = "X";
                    break;
                case 25:
                    strOut = "Y";
                    break;
                case 26:
                    strOut = "Z";
                    break;
            }
            return (strOut);
        }
    }
    
    public static class cExtract
    {
        //KCC -Modified to merge with PA TCF
        private static cExcel.cExcel_Lib Excel = new cExcel.cExcel_Lib();
        public static int ModeSet = 0;

        //KCC - Sheet name for FBAR
        private const string
            sTCF = "Test_Condition_FBAR",
            sSegment = "Segment",
            sFixture = "Fixture Analysis",
            sTrace = "Trace",
            sDC = "DC_Channel_Setting",
            sCal = "Calibration Procedure";

        //KCC - Sheet number for FBAR
        private static int
            iTCF = 5,
            iSegment = 6,
            iFixture = 7,
            iTrace = 8,
            iDC = 9,
            iCal = 10;

        public static void Load_File(string FileName)
        {
            if ((FileName != "") || (FileName != null))
            {
                
                Excel.Load_Excel(FileName);
                ModeSet = 1;
            }
        }
        public static void Close_File()
        {
            if (ModeSet == 1) Excel.Close_Excel();
        }

        //public static string Get_Data(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data(Worksheet, Row, Column));
        //}
        public static string Get_Data(string Worksheet, int Row, int Column)
        {
            int SheetNumber = 0;
            switch (Worksheet)
            {
                case sTCF:
                    SheetNumber = iTCF;
                    break;

                case sSegment:
                    SheetNumber = iSegment;
                    break;

                case sFixture:
                    SheetNumber = iFixture;
                    break;

                case sTrace:
                    SheetNumber = iTrace;
                    break;

                case sDC:
                    SheetNumber = iDC;
                    break;

                case sCal:
                    SheetNumber = iCal;
                    break;

            }
            
            string Temp = ""; 
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(SheetNumber, Row, Column); 
            }
            catch(Exception ex)
            {
                if (!(Temp == "")) MessageBox.Show("Excel issue " + ex.Message); 
            }

            return Temp; 
        }

        public static string Get_Data(int Worksheet, int Row, int Column)
        {
            string Temp = "";
            try
            {
                Temp = (Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(Worksheet, Row, Column));
            }
            catch (Exception ex)
            {
                if (!(Temp == "")) MessageBox.Show("Excel issue " + ex.Message);
            }

            return Temp;
        }

        //public static string Get_Data2(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data2(Worksheet, Row, Column));
        //}
        public static string Get_Data2(string Worksheet, int Row, int Column)
        {
            int SheetNumber = 0;
            switch (Worksheet)
            {
                case sTCF:
                    SheetNumber = iTCF;
                    break;

                case sSegment:
                    SheetNumber = iSegment;
                    break;

                case sFixture:
                    SheetNumber = iFixture;
                    break;

                case sTrace:
                    SheetNumber = iTrace;
                    break;

                case sDC:
                    SheetNumber = iDC;
                    break;

                case sCal:
                    SheetNumber = iCal;
                    break;
            }
            string Temp = "";

            try
            {
                Temp = (Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(SheetNumber, Row, Column));
            }
            catch(Exception ex)
            {
                if (!(Temp == "")) MessageBox.Show("Excel issue " + ex.Message);
            }
            return Temp;
        }

        //public static string Get_Data_Zero(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data_Zero(Worksheet, Row, Column));
        //}
        public static string Get_Data_Zero(string Worksheet, int Row, int Column)
        {
            int SheetNumber = 0;
               
            switch (Worksheet)
            {
                case sTCF:
                    SheetNumber = iTCF;
                    break;

                case sSegment:
                    SheetNumber = iSegment;
                    break;

                case sFixture:
                    SheetNumber = iFixture;
                    break;

                case sTrace:
                    SheetNumber = iTrace;
                    break;

                case sDC:
                    SheetNumber = iDC;
                    break;

                case sCal:
                    SheetNumber = iCal;
                    break;
            }

            string Temp = "";
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(SheetNumber, Row, Column);
            }
            catch (Exception ex)
            {
                if ((Temp == "") || (Temp == null))
                {
                    Temp = "0";
                }
                else MessageBox.Show("Excel issue " + ex.Message);
            }
            return Temp;
        }

        public static string Get_Data_Zero(int Worksheet, int Row, int Column)
        {
            string Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(Worksheet, Row, Column);
            if ((Temp == "") || (Temp == null))
            {
                return ("0");
            }
            else
            {
                return (Temp);
            }
        }

        //public static int Get_Data_Int(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data_Int(Worksheet, Row, Column));
        //}
        public static int Get_Data_Int(string Worksheet, int Row, int Column)
        {
            int SheetNumber = 0;
            switch (Worksheet)
            {
                case sTCF:
                    SheetNumber = iTCF;
                    break;

                case sSegment:
                    SheetNumber = iSegment;
                    break;

                case sFixture:
                    SheetNumber = iFixture;
                    break;

                case sTrace:
                    SheetNumber = iTrace;
                    break;

                case sDC:
                    SheetNumber = iDC;
                    break;

                case sCal:
                    SheetNumber = iCal;
                    break;
            }

            string Temp = "";
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(SheetNumber, Row, Column);
            }
            catch (Exception ex)
            {
                if ((Temp == "") || (Temp == null))
                {
                    return (0);
                }
                else MessageBox.Show("Excel issue " + ex.Message);
            }
            return (int.Parse(Temp));   
        }

        public static int Get_Data_Int(int Worksheet, int Row, int Column)
        {
            string Temp = "";
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(Worksheet, Row, Column);
            }
            catch(Exception ex)
            { 
                if ((Temp == "") || (Temp == null))
                {
                    return (0);
                }
                else MessageBox.Show("Excel issue " + ex.Message);
            }    
            return (int.Parse(Temp));
        }

        //public static double Get_Data_Double(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data_Double(Worksheet, Row, Column));
        //}
        public static double Get_Data_Double(string Worksheet, int Row, int Column)
        {
            int SheetNumber = 0;
            switch (Worksheet)
            {
                case sTCF:
                    SheetNumber = iTCF;
                    break;

                case sSegment:
                    SheetNumber = iSegment;
                    break;

                case sFixture:
                    SheetNumber = iFixture;
                    break;

                case sTrace:
                    SheetNumber = iTrace;
                    break;

                case sDC:
                    SheetNumber = iDC;
                    break;

                case sCal:
                    SheetNumber = iCal;
                    break;
            }
            string Temp = "";
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(SheetNumber, Row, Column);
            }
            catch(Exception ex)
            { 
                if ((Temp == "") || (Temp == null))
                {
                    return (0);
                }
                else MessageBox.Show("Excel issue " + ex.Message);
            }    
            return (double.Parse(Temp));
        }

        public static double Get_Data_Double(int Worksheet, int Row, int Column)
        {
            string Temp = "";
            try
            {
                Temp = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input(Worksheet, Row, Column);
            }
            catch (Exception ex)
            {
                if ((Temp == "") || (Temp == null))
                {
                    return (0);
                }
                else MessageBox.Show("Excel issue " + ex.Message);
            }
            return (double.Parse(Temp));
        }

        public static bool Sheet_Exist(string Sheet_Name)
        {
            return (Excel.Sheet_Exist(Sheet_Name));
        }
    }
}
