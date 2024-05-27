using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ProductionLib2
{
    public class WebServiceQuery
    {
        public const int maxnum = 10;

        public static string cStrBaseUrl = "";
        public static string[] LotInfoArray = new string[maxnum];
        public static string LotID = "";
        public static bool Webcheck = true;

        public static string[] GetWebInfo()
        {
                string lotidWebaddress = "MFGID_Test?LotCode=" + LotID.Trim().ToUpper() + "&Department=TSFCS";
                
                string[] InfoArrayTemp = new string[maxnum];

                string[] InfoArray = new string[maxnum];

                string stdut = "";

                for (int iRow = 0; iRow < maxnum; iRow++)
                {
                    InfoArray[iRow] = "NULL";
                }

                try
                {
                    var client = new HttpClient();

                    client.BaseAddress = new Uri(cStrBaseUrl);
                    //HTTP GET
                    var responseTask = client.GetAsync(lotidWebaddress);
                    responseTask.Wait();
                    var result = responseTask.Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var temp = readTask.Result;

                        InfoArrayTemp = temp.Split(',');

                        bool isAlphaBet = false;
                        bool isNumeric = false;
                        string c = "";
                        int i = 0;

                        foreach (var info in InfoArrayTemp)
                        {
                            stdut = info.ToString();
                            string[] a = stdut.Split(':');

                            c = "";

                            foreach (var b in a[0])
                            {
                                isAlphaBet = Regex.IsMatch(b.ToString().ToUpper(), "[A-Z]", RegexOptions.IgnoreCase);
                                isNumeric = Regex.IsMatch(b.ToString(), "[0-9]", RegexOptions.IgnoreCase);

                                if (isAlphaBet == true || isNumeric == true || b == '_')
                                {
                                    c = c + b;
                                }
                            }

                            InfoArray[i] = c;

                            i++;

                            c = "";

                            foreach (var b in a[1])
                            {
                                isAlphaBet = Regex.IsMatch(b.ToString().ToUpper(), "[A-Z]", RegexOptions.IgnoreCase);
                                isNumeric = Regex.IsMatch(b.ToString(), "[0-9]", RegexOptions.IgnoreCase);

                                if (isAlphaBet == true || isNumeric == true || b == '-')
                                {
                                    c = c + b;
                                }
                            }

                            InfoArray[i] = c;

                            i++;
                        }
                    }
                }
                catch (Exception e)
                {
                   Webcheck = false;
                }
               

                return InfoArray;
        }

        public static bool DisplayInariWebListNames(string lotid, string serverUrl = "http://192.168.10.99:8087/api/")
        {
            LotID = lotid;
            cStrBaseUrl = serverUrl;
            LotInfoArray = GetWebInfo();

            return Webcheck;
        }       
    }
}
