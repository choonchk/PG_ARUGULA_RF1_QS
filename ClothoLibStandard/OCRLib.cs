using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using Excel = Microsoft.Office.Interop.Excel;

namespace ClothoLibStandard
{
    /*****************************************************************************************/
    //Version:
    //1.0.0 - Created by KCC for WSD OCR:
    //1.0.1 - For SDI format
    //1.0.2 - Option for capture all
    //1.0.3 - For Post OCR
    //1.0.4 - Skipped
    //1.0.5 - For Pre OCR, ignored 'F' entries.
    //1.0.6 - Check before merge, report additional rows, removed capture all
    //1.0.7 - Remove not matching bug

    public class FileStream_OCR
    {
        StreamWriter sw;

        public void CreateNewFolder(string Path)
        {

            if (System.IO.Directory.Exists(Path) == false)
            {
                System.IO.Directory.CreateDirectory(Path);
            }
        }

        public string FileBrowser()
        {
            string FileSelect = "";
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                FileSelect = OFD.FileName.ToString();
            }
            return FileSelect;
        }

        public void WriteLineToTextFile(string FileName, string Text)
        {
            sw = new StreamWriter(FileName, true);
            sw.WriteLine(Text);
            sw.Close();
        }

        public void RunApplication(string directory)
        {
            Process ProcessBat;
            ProcessBat = new Process();
            ProcessBat = Process.Start(directory);
            ProcessBat.WaitForExit(24000);
            ProcessBat.Close();
        }

        public void DeleteFile(string filename)
        {
            File.Delete(filename);
        }

        public void CopyFile(string Source, string Destination)
        {
            File.Copy(Source, Destination);
        }

        public int CheckTextFileLineNumber(string filename)
        {
            int linenumber = 0;
            using (StreamReader r = new StreamReader(filename))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    linenumber++;
                }
            }
            return linenumber;
        }

        public ArrayList ReadTextContentByLine_ArrayList(string filename, int linenumber)
        {
            ArrayList Result = new ArrayList();
            using (StreamReader r = new StreamReader(filename))
            {
                int line =0;
                string text = "";
                while ((line < linenumber))
                {
                    text = r.ReadLine();
                    if (text != "")
                    {
                        Result.Add(text);
                    }
                    line++;
                }
            }
            return Result;
        }

        public List<string> ReadTextContentByLine_List(string filename, int linenumber)
        {
            List<string> Result = new List<string>();
            using (StreamReader r = new StreamReader(filename))
            {
                int line = 0;
                string text = "";
                while ((line < linenumber))
                {
                    text = r.ReadLine();
                    if (text != "")
                    {
                        Result.Add(text);
                    }
                    line++;
                }
            }
            return Result;
        }

        public string ReadTextFileLine(string filename, int linenumber)
        {
            string text = "";
            int line = 0;
            using (StreamReader r = new StreamReader(filename))
            {  
                while ((line < linenumber - 1))
                {
                    text = r.ReadLine();
                    line++;
                }
                text = r.ReadLine();
            }
            return text;
        }

        public int ReadTextLineNumberWithWord(string filename, string word)
        {
            int linenumber = 1;
            string text = "";
            using (StreamReader r = new StreamReader(filename))
            {
                while (true)
                {
                    text = r.ReadLine();
                    if (text != null)
                    {
                        if (text.Contains(word))
                        {
                            break;
                        }
                    }
                    linenumber++;
                }
            }
            return linenumber;
        } 
    }

    public class ExcelApplication_OCR
    {
        //Interop Method - Read
        public int ReadExcelRow(string filename)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return rCnt;
        }

        public int ReadExcelRowStartWithWord(string filename, string word)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            string svalue;
            bool done = false;
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    if (str != null)
                    {
                        svalue = (string)str;
                        if (svalue.Contains(word))
                            done = true;
                    }
                }
                if (done)
                    break;
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return rCnt;
        }

        public string ReadExcelColumn(string filename, int row)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            string value = "";
            int rCnt = row;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = row; rCnt <= row; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    value = (string)str;
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return value;
        }
                
        public ArrayList ReadExcelColumn_ArrayList(string filename, int row)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            ArrayList value = new ArrayList();
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = row; rCnt <= row; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;                        
                    value.Add(str);
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return value;
        }

        public ArrayList ReadExcelColumnWithSize_ArrayList(string filename, int startrow, int endrow)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            ArrayList value = new ArrayList();
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = startrow; rCnt <= endrow; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;                       
                    value.Add(str);
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return value;
        }
        
        public List<string> ReadExcelColumnWithSize_List(string filename, int startrow, int endrow)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            List<string> value = new List<string>();
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = startrow; rCnt <= endrow; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    value.Add((string)str);
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return value;
        }

        public ArrayList ReadExcelColumnWithRowAndKeyword_ArrayList(string filename, int startrow, int endrow, string keyword)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            object str;
            ArrayList value = new ArrayList();
            int rCnt = 0;
            int cCnt = 0;

            xlApp = new Excel.ApplicationClass();
            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            range = xlWorkSheet.UsedRange;

            for (rCnt = startrow; rCnt <= endrow; rCnt++)
            {
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
                    if (str != null && str.ToString().Contains(keyword))
                    {
                        value.Add(str);
                    }
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return value;
        }
        
        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
        
        //OLEDB Method - Write
        public void CreateExcelWithIndex(string filename, string tablename, string index)
        {
            System.Data.OleDb.OleDbConnection CN = new System.Data.OleDb.OleDbConnection();
            CN.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties=Excel 8.0";
            CN.Open();
            System.Data.OleDb.OleDbCommand CM = CN.CreateCommand();
            string Command;
            //Create Table                
            Command = string.Format("CREATE TABLE {0} ({1})", tablename, index);
            CM.CommandText = Command;
            CM.ExecuteNonQuery();
            Command = "";
        }

        public void InsertIntoExcel(string filename, string tablename, string word)
        {
            System.Data.OleDb.OleDbConnection CN = new System.Data.OleDb.OleDbConnection();
            CN.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties=Excel 8.0";
            CN.Open();
            System.Data.OleDb.OleDbCommand CM = CN.CreateCommand();
            string Command;
            //Insert to Table                
            Command = string.Format("INSERT INTO [{0}] VALUES ({1})", tablename, word);
            CM.CommandText = Command;
            CM.ExecuteNonQuery();
            CN.Close();
        }

        public void InsertIntoExcel(string filename, string tablename, ArrayList Name, ArrayList Value)
        {
            System.Data.OleDb.OleDbConnection CN = new System.Data.OleDb.OleDbConnection();
            CN.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties=Excel 8.0";
            CN.Open();
            System.Data.OleDb.OleDbCommand CM = CN.CreateCommand();
            string Command;            
            //Insert to Table
            int i, length = 0;
            length = Name.Count;
            for (i = 0; i < length; i++)
            {
                Command = string.Format("INSERT INTO [{0}] VALUES ('{1}', '{2}')", tablename, Name[i].ToString(), Value[i].ToString());
                CM.CommandText = Command;
                CM.ExecuteNonQuery();
            }

            CN.Close();
        }

        public void InsertIntoExcel(string filename, string tablename, ArrayList Name, ArrayList Value1, ArrayList Value2)
        {
            System.Data.OleDb.OleDbConnection CN = new System.Data.OleDb.OleDbConnection();
            CN.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties=Excel 8.0";
            CN.Open();
            System.Data.OleDb.OleDbCommand CM = CN.CreateCommand();
            string Command;

            //Insert to Table
            int i, length = 0;
            length = Name.Count;
            for (i = 0; i < length; i++)
            {
                Command = string.Format("INSERT INTO [{0}] VALUES ('{1}', '{2}', '{3}')", tablename, Name[i].ToString(), Value1[i].ToString(), Value2[i].ToString());
                CM.CommandText = Command;
                CM.ExecuteNonQuery();
            }

            CN.Close();
        }

        public void InsertIntoExcel(string filename, string tablename, List<string> Name, List<string> Value1, List<string> Value2)
        {
            System.Data.OleDb.OleDbConnection CN = new System.Data.OleDb.OleDbConnection();
            CN.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";Extended Properties=Excel 8.0";
            CN.Open();
            System.Data.OleDb.OleDbCommand CM = CN.CreateCommand();
            string Command;

            //Insert to Table
            int i, length = 0;
            length = Name.Count;
            for (i = 0; i < length; i++)
            {
                Command = string.Format("INSERT INTO [{0}] VALUES ('{1}', '{2}', '{3}')", tablename, Name[i].ToString(), Value1[i].ToString(), Value2[i].ToString());
                CM.CommandText = Command;
                CM.ExecuteNonQuery();
            }

            CN.Close();
        }

        public void ConvertExcelToCSV(string sourceExcel, string tablename, string targetCSV)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sourceExcel + ";Extended Properties=Excel 8.0";
            OleDbConnection conn = null;
            StreamWriter wrtr = null;
            OleDbCommand cmd = null;
            OleDbDataAdapter da = null;
            try
            {
                conn = new OleDbConnection(strConn);
                conn.Open();

                cmd = new OleDbCommand("SELECT * FROM [" + tablename + "$]", conn);
                cmd.CommandType = CommandType.Text;
                wrtr = new StreamWriter(targetCSV);

                da = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string rowString = "";
                    for (int y = 0; y < dt.Columns.Count; y++)
                    {
                        rowString += "\"" + dt.Rows[x][y].ToString() + "\",";
                    }
                    wrtr.WriteLine(rowString);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Console.ReadLine();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Dispose();
                cmd.Dispose();
                da.Dispose();
                wrtr.Close();
                wrtr.Dispose();
            }
        }      
    }

    public class OcrMethod
    {
        public void MergeOCRWithFullResults(string OCRFileLocation, string ResultFileLocation)
        {
            //Fields
            FileStream_OCR _file = new FileStream_OCR();
            ExcelApplication_OCR _excel = new ExcelApplication_OCR();
            string[] aOCR_Date;
            string[] aOCR_Time;
            List<string> lOCR_Date = new List<string>();
            List<string> lOCR_Time = new List<string>();
            List<string> lOCR_Serial = new List<string>();
            List<string> lOCR_CSV = new List<string>();
            List<string[]> lOCRNew = new List<string[]>();
            List<string[]> lDatum = new List<string[]>();

            int
                iUnitCount = 0,
                iFileNameCount = 0,
                iFileNameLoop = 0,
                iResultData = 0,
                iOCRData = 0,
                iOCRColumn = 0,
                iParam = 0,
                iParamData = 1,
                iLimits = 0,
                iPID = 0,
                iDatum = 0,
                iDatumCount = 0,
                iOCRRow = 0,
                iOCRReject = 0,
                iExcelColumn = 0,
                iExcelRowTotal = 0,
                iExcelRowDatum = 0;

            string
                sMisc = "",
                sParam = "",
                sOutputFileLocation = "",
                sCurrentLocation = "",
                sFileName = "",
                sResultFileName="",
                sCaptureResult = "PID";

            string[]
                aOutputFileLocation = null,
                aFileName = null,
                aParam = null,
                aDatum = null,
                aDatumCondition = null;            

            //Create new file name
            aOutputFileLocation = ResultFileLocation.Split('\\');
            iFileNameCount = aOutputFileLocation.Count();

            //Commented this 2 line because clotho 2.1.x will reports the result.csv filename including IP address example with IP192.10.10.10 instead IP192101010 - Shaz 22/03/2013
            //aFileName = aOutputFileLocation[iFileNameCount - 1].Split('.');
            //sFileName = aFileName[0] + "_WithOCR.CSV";
            sResultFileName = aOutputFileLocation[iFileNameCount - 1].Remove(aOutputFileLocation[iFileNameCount - 1].Length - 4, 4);
            sFileName = sResultFileName + "_WithOCR.CSV";
            
            
            //Temporary location for file operation
            sCurrentLocation = "C:\\" + sFileName;
            while (iFileNameLoop != iFileNameCount - 1)
            {
                sOutputFileLocation += aOutputFileLocation[iFileNameLoop] + "\\";
                iFileNameLoop++;
            }
            sOutputFileLocation += sFileName;

            //Find Result's parameter and PID's row number
            iParam = _file.ReadTextLineNumberWithWord(ResultFileLocation, "Parameter");
            iPID = _file.ReadTextLineNumberWithWord(ResultFileLocation, "PID");

            //Find OCR row number
            iOCRRow = _file.CheckTextFileLineNumber(OCRFileLocation);
            lOCR_CSV = _file.ReadTextContentByLine_List(OCRFileLocation, iOCRRow);

            //Serialize OCR data, removes '/' and spaces for SDI  
            while (iOCRData != iOCRRow)
            {
                lOCRNew.Add(lOCR_CSV[iOCRData].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                aOCR_Date = lOCRNew[iOCRData][0].Split('.');
                aOCR_Time = lOCRNew[iOCRData][2].Split(':');
                //Join string[]
                lOCR_Date.Add(aOCR_Date[0] + aOCR_Date[1] + aOCR_Date[2]);
                lOCR_Time.Add(aOCR_Time[0] + aOCR_Time[1] + aOCR_Time[2]);

                if (lOCRNew[iOCRData][4].ToUpper().Contains("F"))
                {
                    //For Post OCR
                    //lOCR_Serial.Add("NA");

                    //For Pre OCR
                    //Check for rejects
                    iOCRReject++;
                }
                else
                {
                    ////Remove "f" from 00000f
                    //if (lOCRNew[iOCRData][4].ToUpper().Contains("F"))
                    //{
                    //    lOCRNew[iOCRData][4] = lOCRNew[iOCRData][4].Remove(5, 1);
                    //    lOCR_Serial.Add(lOCRNew[iOCRData][4]);
                    //}
                    //else
                    //{
                        lOCR_Serial.Add(lOCRNew[iOCRData][4]);
                    //}
                }  
                iOCRData++;                
            }

            //Split Parameter's csv into arrays
            sParam = _file.ReadTextFileLine(ResultFileLocation, iParam);

            //Check if CSV corrupted
            if (sParam.Contains(","))
            {
                //Parameter split
                aParam = sParam.Split(',');
                //Count column number from array
                iExcelColumn = aParam.Count();
                iExcelRowTotal = _file.CheckTextFileLineNumber(ResultFileLocation);
                iExcelRowDatum = iExcelRowTotal - iParam;

                //Redim arrays
                aDatum = new string[iExcelRowDatum];
                aDatumCondition = new string[iExcelRowDatum];

                ////Check for option, capture all or PASS only
                //if (CaptureAll)
                //{
                //    sCaptureResult = "PID";
                //}
                //else
                //{
                //    sCaptureResult = "PASS_ALL+";
                //}

                //If OCR row == Result then proceed
                if ((iExcelRowTotal - iPID + 1) == (iOCRRow - iOCRReject))
                {
                    //Populate data with OCR, append OCR to result data
                    while (iResultData != iExcelRowDatum)
                    {
                        aDatum[iResultData] = _file.ReadTextFileLine(ResultFileLocation, iPID + iResultData);

                        if (aDatum[iResultData] != null && aDatum[iResultData].Contains(sCaptureResult) && aDatum[iResultData].Contains(",") && iUnitCount != iOCRRow)
                        {
                            lDatum.Add(aDatum[iUnitCount].Split(','));
                            iDatumCount = lDatum[iUnitCount].Count();
                            //Join string for SDI
                            aDatumCondition[iUnitCount] = string.Join(",", lDatum[iUnitCount], 0, iDatumCount - 5);
                            aDatumCondition[iUnitCount] += "," + lOCR_Date[iUnitCount] + "," + lOCR_Time[iUnitCount] + "," + lOCR_Serial[iUnitCount] + ",";
                            aDatumCondition[iUnitCount] += string.Join(",", lDatum[iUnitCount], iDatumCount - 5, 5);
                            iUnitCount++;
                        }
                        iResultData++;
                    }

                    //Delete if file exist
                    if (File.Exists(sOutputFileLocation))
                    {
                        File.Delete(sOutputFileLocation);
                    }
                    sParam = "";

                    //Print Misc before Parameter
                    while (iParamData != iParam)
                    {
                        sMisc = _file.ReadTextFileLine(ResultFileLocation, iParamData);
                        _file.WriteLineToTextFile(sCurrentLocation, sMisc);
                        iParamData++;
                    }

                    //Print Parameter + OCR column
                    while (iOCRColumn != iExcelColumn - 5) //-5 if inserted between PassFail
                    {
                        sParam += aParam[iOCRColumn] + ",";
                        iOCRColumn++;
                    }
                    sParam = sParam + "OcrDate,OcrTime,OcrSerial,";

                    while (iOCRColumn != iExcelColumn - 1)
                    {
                        sParam += aParam[iOCRColumn] + ",";
                        iOCRColumn++;
                    }
                    sParam = sParam + "SWBinName";

                    _file.WriteLineToTextFile(sCurrentLocation, sParam);
                    iLimits = iParam + 1;

                    //Print limits
                    while (iLimits != iPID)
                    {
                        sMisc = _file.ReadTextFileLine(ResultFileLocation, iLimits);
                        _file.WriteLineToTextFile(sCurrentLocation, sMisc);
                        iLimits++;
                    }

                    //Print Datum
                    while (iDatum != iUnitCount)
                    {
                        _file.WriteLineToTextFile(sCurrentLocation, aDatumCondition[iDatum]);
                        iDatum++;
                    }

                    //Copy file to result directory
                    _file.CopyFile(sCurrentLocation, sOutputFileLocation);
                    try
                    {
                        _file.DeleteFile(sCurrentLocation);
                    }
                    catch
                    {
                    }

                    MessageBox.Show("OCR merging successful! Please check output file for details.", "Operation completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (((iExcelRowTotal - iPID + 1) - (iOCRRow - iOCRReject)) > 1)
                    {
                        MessageBox.Show("Unit count not matched, additional " + ((iExcelRowTotal - iPID + 1) - (iOCRRow - iOCRReject)).ToString() + " row(s) on TEST RESULT.", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Unit count not matched, additional " + ((iOCRRow - iOCRReject) - (iExcelRowTotal - iPID + 1)).ToString() + " row(s) on OCR.", "Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }                    
                }
            }

            //Exit if CSV file corrupted
            else
            {
                MessageBox.Show("Result file(csv) corrupted, please check!", "File reading error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Garbage collector, flushes out used memories
            try
            {
                GC.Collect();
            }
            catch { }            
        }
    }
        
    #region Thread_ReadExcel

    //Input properties
    [Obsolete("No longer used")]
    public class Thread_RunExcel_Input
    {
        public string _ResultFileLocation { get; set; }
    }

    //Output properties
    [Obsolete("No longer used")]
    public class Thread_RunExcel_Output
    {
        public int _PIDStartLine { get; set; }
        public int _PIDDataArraySize { get; set; }
        public List<string> _PIDDataArrayList { get; set; }
    }

    //Thread class
    [Obsolete("No longer used")]
    public class Thread_RunExcel
    {
        public Thread_RunExcel_Input input { get { return _input; } }
        private Thread_RunExcel_Input _input;
        public Thread_RunExcel_Output output { get { return _output; } }
        private Thread_RunExcel_Output _output;
        private ManualResetEvent _mre;
        ExcelApplication_OCR _excel = new ExcelApplication_OCR();

        //Constructor
        [Obsolete("No longer used")]
        public Thread_RunExcel(Thread_RunExcel_Input input, ManualResetEvent mre)
        {
            _input = input;
            _mre = mre;
        }

        //Thread call back method
        [Obsolete("No longer used")]
        public void ThreadPoolCallback(Object ThreadInfo)
        {
            _output = DoWork(_input);
            _mre.Set();
        }

        //Do work method
        [Obsolete("No longer used")]
        private Thread_RunExcel_Output DoWork(Thread_RunExcel_Input input)
        {
            //Get PID line number, PID array size, and PID list<>
            Thread_RunExcel_Output Results = new Thread_RunExcel_Output();
            Results._PIDStartLine = _excel.ReadExcelRowStartWithWord(input._ResultFileLocation, "PID");
            int _PIDEndLine = _excel.ReadExcelRow(input._ResultFileLocation);
            int _PIDSize = _PIDEndLine - Results._PIDStartLine;
            Results._PIDDataArrayList = _excel.ReadExcelColumnWithSize_List(input._ResultFileLocation, Results._PIDStartLine, _PIDEndLine);
            Results._PIDDataArrayList.Remove(null);
            Results._PIDDataArraySize = Results._PIDDataArrayList.Count;
            return Results;
        }
    }    

    #endregion

    #region Thread_ReadText

    //Input properties
    [Obsolete("No longer used")]
    public class Thread_RunText_Input
    {
        public string _OCRFileLocation { get; set; }
    }

    //Output properties
    [Obsolete("No longer used")]
    public class Thread_RunText_Output
    {
        public int _OCRSize { get; set; }
        public List<string> _OCRDataArrayList { get; set; }
    }

    //Thread class
    [Obsolete("No longer used")]
    public class Thread_RunText
    {
        public Thread_RunText_Input input { get { return _input; } }
        private Thread_RunText_Input _input;
        public Thread_RunText_Output output { get { return _output; } }
        private Thread_RunText_Output _output;
        private ManualResetEvent _mre;
        FileStream_OCR _file = new FileStream_OCR();

        //Constructor
        [Obsolete("No longer used")]
        public Thread_RunText(Thread_RunText_Input input, ManualResetEvent mre)
        {
            _input = input;
            _mre = mre;
        }
        //Thread call back method
        [Obsolete("No longer used")]
        public void ThreadPoolCallback(Object ThreadInfo)
        {
            _output = DoWork(_input);
            _mre.Set();
        }
        //Do work method
        [Obsolete("No longer used")]
        private Thread_RunText_Output DoWork(Thread_RunText_Input input)
        {
            //Get OCR file size, OCR list<>
            Thread_RunText_Output Results = new Thread_RunText_Output();
            Results._OCRSize = _file.CheckTextFileLineNumber(input._OCRFileLocation);
            Results._OCRDataArrayList = _file.ReadTextContentByLine_List(input._OCRFileLocation, Results._OCRSize);
            //Count OCR line after removing empty line
            Results._OCRSize = Results._OCRDataArrayList.Count;
            return Results;
        }
    }

    #endregion
}

    

        



