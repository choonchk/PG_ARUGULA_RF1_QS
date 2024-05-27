using System;
using System.Collections.Generic;
using Avago.ATF.StandardLibrary;

namespace SParamTestCommon
{
    /// <summary>
    /// Calibration Procedure Sheet.
    /// </summary>
    public class TcfSheetCalProcedure
    {
        private int numRows;
        private int numColumns;
        public Tuple<bool, string, string[,]> allContents;
        public List<Dictionary<string, string>> testPlan;
        public List<string> Header;
        public int headerRow;
        public Dictionary<string, string> TableVertical;

        public TcfSheetCalProcedure(string sheetName, int numRows, int numCols)
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

        private int m_currentRowIndex;

        public void SetCurrentRow(int rowNo)
        {
            m_currentRowIndex = rowNo;
        }

        public int GetDataInt(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            if (string.IsNullOrEmpty(result)) return 0;
            return int.Parse(result);
        }

        public string GetData(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            return result;
        }

        private void GetHeader()
        {
            Header = new List<string>();

            for (int row = 0; row < numRows; row++)
            {
                string enableCell = allContents.Item3[row, 0];

                if (enableCell.ToUpper() != "#START") continue;
                headerRow = row;

                for (int column = 1; column < numColumns; column++)
                {
                    string value = allContents.Item3[row, column];
                    if (value.ToUpper() == "#END") break;
                    if (!String.IsNullOrEmpty(value))
                    {
                        Header.Add(value.Trim());
                    }
                }
                break;
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

                if (enableCell.Trim().ToUpper() != "X")
                {
                    Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

                    for (int i = 0; i < Header.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(Header[i]))
                        {
                            string value = allContents.Item3[row, i + 1];
                            currentTestDict.Add(Header[i], value);
                        }
                    }

                    testPlan.Add(currentTestDict);
                }
            }
        }
    }

    /// <summary>
    /// DC Channel Setting Sheet.
    /// </summary>
    public class TcfSheetDcChannelSetting
    {
        private int numRows;
        private int numColumns;
        public Tuple<bool, string, string[,]> allContents;
        public List<Dictionary<string, string>> testPlan;
        public List<string> Header;
        public int headerRow;
        public Dictionary<string, string> TableVertical;

        public TcfSheetDcChannelSetting(string sheetName, int numRows, int numCols)
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

        private int m_currentRowIndex;

        public void SetCurrentRow(int rowNo)
        {
            m_currentRowIndex = rowNo;
        }

        public int GetDataInt(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            if (string.IsNullOrEmpty(result)) return 0;
            return int.Parse(result);
        }

        public string GetData(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            return result;
        }

        private void GetHeader()
        {
            Header = new List<string>();

            for (int row = 0; row < numRows; row++)
            {
                string enableCell = allContents.Item3[row, 0];

                if (enableCell.ToUpper() != "#START") continue;
                headerRow = row + 1;

                for (int column = 0; column < numColumns; column++)
                {
                    string value = allContents.Item3[headerRow, column];
                    if (String.IsNullOrEmpty(value))
                    {
                        break;
                    }
                    Header.Add(value.Trim());
                }
                break;
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

                if (enableCell.Trim().ToUpper() != "X")
                {
                    Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

                    for (int i = 0; i < Header.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(Header[i]))
                        {
                            string value = allContents.Item3[row, i];
                            currentTestDict.Add(Header[i], value);
                        }
                    }

                    testPlan.Add(currentTestDict);
                }
            }
        }
    }

    public class TcfSheetTrace
    {
        private int numRows;
        private int numColumns;
        public Tuple<bool, string, string[,]> allContents;
        public List<Dictionary<string, string>> testPlan;
        public List<string> Header;
        public int headerRow;

        public TcfSheetTrace(string sheetName, int numRows, int numCols)
        {
            this.numRows = numRows;
            numColumns = numCols;

            allContents = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(sheetName, 1, 1, numRows, numCols);

            if (allContents.Item1 == false)
            {
                throw new Exception("Error reading Excel Range\n\n" + allContents.Item2);
            }

            GetHeader();
            GetTestPlan();
        }

        private int m_currentRowIndex;

        public void SetCurrentRow(int rowNo)
        {
            m_currentRowIndex = rowNo;
        }

        public int GetDataInt(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            if (string.IsNullOrEmpty(result)) return 0;
            return int.Parse(result);
        }

        public string GetData(string key)
        {
            string result = testPlan[m_currentRowIndex][key];
            return result;
        }

        private void GetHeader()
        {
            Header = new List<string>();
            headerRow = 0;

            for (int column = 0; column < numColumns; column++)
            {
                string value = allContents.Item3[0, column];
                if (!String.IsNullOrEmpty(value))
                {
                    Header.Add(value.Trim());
                }

                bool isEnd = value == "#EndTrace";
                if (isEnd) break;
            }
        }

        private void GetTestPlan()
        {
            testPlan = new List<Dictionary<string, string>>();

            for (int row = headerRow + 1; row < numRows; row++)
            {
                //string channel = allContents.Item3[row, 0];
                //string ports = allContents.Item3[row, 1];
                //string traceNumberSetting = allContents.Item3[row, 2];
                bool isEnd = allContents.Item3[row, 0] == "#EndTrace";
                if (isEnd) break;

                Dictionary<string, string> currentTestDict = new Dictionary<string, string>();

                for (int i = 0; i < Header.Count; i++)
                {
                    if (!String.IsNullOrEmpty(Header[i]))
                    {
                        string value = allContents.Item3[row, i];
                        currentTestDict.Add(Header[i], value);
                    }
                }

                testPlan.Add(currentTestDict);
            }
        }
    }

    public class TcfSheetSegmentTable
    {
        private int numRows;
        private int numColumns;
        public Tuple<bool, string, string[,]> allContents;
        public List<Dictionary<string, string>> testPlan;
        public List<string> Header;
        public int headerRow;
        public Dictionary<string, string> TableVertical;

        public TcfSheetSegmentTable(string sheetName, int numRows, int numCols)
        {
            this.numRows = numRows;
            numColumns = numCols;

            allContents = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(sheetName, 1, 1, numRows, numCols);

            if (allContents.Item1 == false)
            {
                throw new Exception("Error reading Excel Range\n\n" + allContents.Item2);
            }
        }

        public int GetDataInt(int row, int col)
        {
            string result = allContents.Item3[row, col];
            if (string.IsNullOrEmpty(result)) return 0;
            return int.Parse(result);
        }

        public double GetDataDouble(int row, int col)
        {
            string result = allContents.Item3[row, col];
            if (string.IsNullOrEmpty(result)) return 0;
            return double.Parse(result);
        }

        /// <summary>
        /// Convert 5 G to 5000000000 Convert 2 khz to 2000.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public double GetFrequency(int row, int col)
        {
            string input = GetData(row, col);
            double frequency = convertStr2Val(input);
            return frequency;
        }

        public string GetData(int row, int col)
        {
            string result = allContents.Item3[row, col];
            return result;
        }

        private double convertStr2Val(string input)
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
            //else if(tmpStr.Length  ==  3)
            //{
            //    tmpVal = Convert.ToDouble(tmpStr[1]);
            //}


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

        public double GetFrequency2(bool _isDiva, int row, int col) //To support Diva and Topaz at same TCF with different IFBW... 
        {
            string input = GetData(row, col);
            int Calculator = 1;
            if (!_isDiva)
            {
                string[] tmpStr = input.Split(' ');
                if (tmpStr.Length > 2)
                {
                    string strTmp = tmpStr[2].Replace( "@/", "");
                    Calculator = int.Parse(strTmp);                   

                    input = string.Format("{0} {1}", tmpStr[0], tmpStr[1]);
                }
            }

            double frequency = convertStr2Val(input) / Calculator;
            return frequency;
        }
    }

}