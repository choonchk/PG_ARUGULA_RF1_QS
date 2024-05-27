using System;
using System.Collections.Generic;
using Avago.ATF.StandardLibrary;

namespace TestPlanCommon.CommonModel
{
    public class TcfSheetReader
    {
        int numRows;
        int numColumns;
        public Tuple<bool, string, string[,]> allContents;
        public List<Dictionary<string, string>> testPlan;
        public List<string> Header;
        public int headerRow;
        public Dictionary<string, string> TableVertical;

        public TcfSheetReader(string sheetName, int numRows, int numCols)
        {
            this.numRows = numRows;
            this.numColumns = numCols;

            allContents = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(sheetName, 1, 1, numRows, numCols);

            if (allContents.Item1 == false)
            {
                throw new Exception("Error reading Excel Range\n\n" + allContents.Item2);
            }

            GetHeader();
            GetTestPlan();
        }

        public void Create(string sheetName, int numRows, int numCols)
        {
            this.numRows = numRows;
            this.numColumns = numCols;

            allContents = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(sheetName, 1, 1, numRows, numCols);

            if (allContents.Item1 == false)
            {
                throw new Exception("Error reading Excel Range\n\n" + allContents.Item2);
            }

            GetHeader();
            GetTestPlan();
        }

        public void CreateTableVertical(string sheetName)
        {
            this.numRows = 100;
            this.numColumns = 10;
            allContents = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(sheetName, 1, 1, numRows, numColumns);

            if (allContents.Item1 == false)
            {
                throw new Exception("Error reading Excel Range\n\n" + allContents.Item2);
            }

            TableVertical = new Dictionary<string, string>();

            int iStartRow = 0;
            int iStopRow = 0;
            for (int row = 1; row < numRows; row++)
            {
                string enableCell = allContents.Item3[row, 0];
                if (enableCell.ToUpper() == "#START")
                {
                    iStartRow = row + 1;
                }
                if (enableCell.ToUpper() == "#END")
                {
                    iStopRow = row;
                }
            }

            for (int row = iStartRow; row < iStopRow; row++)
            {
                string value = allContents.Item3[row, 1];
                string headerName = allContents.Item3[row, 0].Trim();
                TableVertical.Add(headerName, value);
            }

        }

        private void GetHeader()
        {
            Header = new List<string>();

            for (int row = 1; row < numRows; row++)
            {
                string enableCell = allContents.Item3[row, 0];

                if (enableCell.ToUpper() == "#START")
                {
                    headerRow = row;

                    for (int column = 0; column < numColumns; column++)
                    {
                        string value = allContents.Item3[row, column];
                        Header.Add(value.Trim());
                        if (value.ToUpper() == "#END") break;
                    }
                    break;
                }
            }
        }

        private void GetTestPlan()
        {
            testPlan = new List<Dictionary<string, string>>();

            for (int row = headerRow + 1; row < numRows; row++)
            {
                string enableCell = allContents.Item3[row, 0];
                string TestModeCell = allContents.Item3[row, 1];

                if (enableCell.ToUpper() == "#END") break;

                if (enableCell.Trim().ToUpper() != "X" & TestModeCell.Trim() != "")
                {
                    Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

                    for (int i = 0; i < Header.Count; i++)
                    {
                        if (Header[i] != "")
                        {
                            string value = allContents.Item3[row, i];
                            currentTestDict.Add(Header[i], value);
                        }
                    }

                    testPlan.Add(currentTestDict);
                }
            }
        }

#if false
        public Dictionary.Ordered<string, string[]> GetDcResourceDefinitions()
        {
            Dictionary.Ordered<string, string[]> DcResourceTempList = new Dictionary.Ordered<string, string[]>();

            for (int col = 0; col < Header.Count; col++)
            {
                string head = Header[col];

                if (head.ToUpper().StartsWith("V."))
                {
                    string dcPinName = head.Replace("V.", "");

                    DcResourceTempList[dcPinName] = new string[Eq.NumSites];

                    for (byte site = 0; site < Eq.NumSites; site++)
                    {
                        DcResourceTempList[dcPinName][site] = allContents.Item3[headerRow - 1 - site, col].Trim();
                    }
                }
            }

            return DcResourceTempList;
        }

#endif
    }


}