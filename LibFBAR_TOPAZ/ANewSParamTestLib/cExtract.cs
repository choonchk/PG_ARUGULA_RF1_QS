using System;
using System.Windows.Forms;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// TO BE OBSOLETED.
    /// </summary>
    public static class cExtract
    {
        //KCC - Sheet name for FBAR
        private const string
            sTCF = "Condition_FBAR",
            sSegment = "Segment",
            sFixture = "Fixture Analysis",
            sTrace = "Trace",
            sDC = "DC_Channel_Setting",
            sCal = "Calibration Procedure";

        //KCC - Sheet number for FBAR
        private static int
            iTCF = 3,
            iFixture = 5,
            iTrace = 4,
            iSegment = 6,
            iDC = 7,
            iCal = 8;


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

        //public static int Get_Data_Int(string Worksheet, int Row, int Column)
        //{
        //    return (Excel.Get_Data_Int(Worksheet, Row, Column));
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

    }
}
