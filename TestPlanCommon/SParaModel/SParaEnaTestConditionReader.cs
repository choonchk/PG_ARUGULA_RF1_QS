using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// Actually not a Ena Reader. Should rename to SParaTestConditionReader.
    /// </summary>
    public class SParaEnaTestConditionReader
    {
        private Dictionary<string, string> m_t;

        public SParaEnaTestConditionReader(Dictionary<string, string> t)
        {
            m_t = t;
        }

        public string ReadTcfData(string key)
        {
            try
            {
                return m_t[key];
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + key + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

        public string ReadTcfData(string key, bool ExemptCustomReg)
        {
            try
            {
                if (key == "CUSTOM_REG") return "" ;

                return m_t[key];
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + key + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

        public bool ReadTcfDataBoolean(string key)
        {
            return CStr2Bool(ReadTcfData(key));
        }

        public double ReadTcfDataDouble(string key)
        {
            double result = convertStr2Val(ReadTcfData(key));
            return result;
        }

        public double ReadTcfDataDouble2(string headerName)
        {
            string numericValue = GetStr_Zero(m_t, headerName);
            double r = convertStr2Val(numericValue);
            return r;
        }

        public int ReadTcfDataInt(string theKey)
        {
            string valStr = "";
            try
            {
                valStr = m_t[theKey];
                if (valStr.ToUpper() == "X") return 0;
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            int valInt = 0;
            try
            {
                valInt = Convert.ToInt16(valStr);
            }
            catch
            {
                MessageBox.Show("Test Condition File contains non-number \"" + valStr + "\" in column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return 0;
            }

            return valInt;
        }

        public List<int> ReadTcfDataIntList(string theKey)
        {
            List<int> result = new List<int>();

            string valStr;
            try
            {
                valStr = m_t[theKey];
                if (valStr.ToUpper() == "X") return result;
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
                return result;
            }

            string[] list = valStr.Split(new char[] {','});
            foreach (string numericItem in list)
            {
                int convertedNum = Convert.ToInt32(numericItem.Trim());
                result.Add(convertedNum);
            }

            return result;
        }

        public bool CInt2Bool(string key)
        {
            int Input = ReadTcfDataInt(key);
            if (Input == 0)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        /// <summary>
        /// Convert 5 G to 5000000000 Convert 2 khz to 2000.
        /// </summary>
        public double GetFrequency(string key)
        {
            string input = ReadTcfData(key);
            double frequency = convertStr2Val(input);
            return frequency;
        }

        private string GetStr_Zero(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                if (dic[theKey] == "") return "0";
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
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
            //else if(tmpStr.Length  ==  3)
            //{
            //    tmpVal = Convert.ToDouble(tmpStr[1]);
            //}


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
    }
}