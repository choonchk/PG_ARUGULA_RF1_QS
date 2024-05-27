using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WSD.Utility.FcmParameter.Parser;

namespace Header_Processor
{
    public class Header_Processor
    {
        public SortedDictionary<int, string> HeaderEntries;

        public List<KeyValuePair<string, Category>> ble2 = new List<KeyValuePair<string, Category>>();

        public string Output_Header(string HeaderSeqFile, string InputTerms, Dictionary<string, string> refDict = null)
        {
            string zoutput = "";            
            var sepList = new List<string>();
            SortedDictionary<int, string> zOrderedCatSeq = new SortedDictionary<int, string>();
            List<string> zmatches = new List<string>();
            int gh = zmatches.ToArray().Length;
            string[] zmatches_array = zmatches.ToArray();

            List<KeyValuePair<string, ParameterKey>> zParameters = new List<KeyValuePair<string, ParameterKey>>();
            List<KeyValuePair<string, Category>> zCategories = new List<KeyValuePair<string, Category>>();
            List<string> zBreakdownErrors = new List<string>();
            List<string> CATID_List = new List<string>();
            bool zSuxes = WSD.Utility.FcmParameter.Parser.HeaderBreakdown.BreakDownHeader(InputTerms , out zParameters, out zCategories, out zBreakdownErrors);
            int r = 0;
            Console.WriteLine("\r\n" + "Parameters Start");
            foreach (KeyValuePair<string, ParameterKey> bal in zParameters)
            {
                Console.WriteLine(bal);
            }
            Console.WriteLine("\r\n" + "Categories Start");
            if (zSuxes) //zSuxes is true if BreakDownHeader does not have an error
            {
                string[] zvals;
                char[] zdelimiter = new char[] { '\t' };

                Dictionary<string, string> CATIDs = new Dictionary<string, string>();
                foreach (KeyValuePair<string, Category> ble in zCategories)
                {
                    Console.WriteLine(ble);
                    CATID_List.Add(ble.Key + "\t" + ble.Value.ToString().Trim());
                    ble2.Add(ble);

                }


                //Create the List<string> that will take into account the multiple entries for a given category
                //Concatenate the common entries


                //foreach (KeyValuePair<string, Category> ble in zCategories)
                //{
                //    Console.WriteLine(ble);
                //    zvals = (ble.Value.ToString()).Split(zdelimiter);
                //    CATIDs.Add(ble.Key, "CategoryType_" + zvals[1].Trim() + ",CATID_" + zvals[0]);


                //}
                //string zres = "Dictionary Contents:" + "\r\n\r\n\r\n";
                //foreach(KeyValuePair<string, string> b in CATIDs)
                //{
                //    zres += b.Key + "    >>    " + b.Value + "\r\n";
                //}
                //MessageBox.Show(zres, "RESULT");
            }
            else
            {
                string zmes = "List of errors from the BreakDownHeader method: " + "\r\n\r\n";
                foreach (string zerr in zBreakdownErrors)
                {
                    zmes += zerr + "\r\n";
                }
                MessageBox.Show("Error with input header elements processing." + "\r\n\r\n"
                    + zmes, "HEADER BREAKDOWN ERROR");
            }

            #region Suck in the PE Seqence
            using (var file = new StreamReader(HeaderSeqFile))
            {
                string line;
                string playmen;
                var delimiters = new string[] { "\r\n" };

                while ((line = file.ReadLine()) != null)
                {

                    var segments = line.Split(delimiters, StringSplitOptions.None);
                    foreach (var segment in segments)
                    {
                        Console.WriteLine(segment);
                        if (!segment.Contains("Prescribed_Order"))
                        {
                            sepList.Add(segment);
                            zOrderedCatSeq.Add(Convert.ToInt32(segment.Split(',')[0]), segment.Split(',')[1]);
                        }

                    }

                }

                file.Close();

            }
            #endregion Suck in the PE Seqence
                        
            foreach (KeyValuePair<int, string> zcats in zOrderedCatSeq)
            {
                foreach (KeyValuePair<string, Category> s in ble2)
                {
                    if (s.Value.ToString().Contains(zcats.Value))
                    {
                        zmatches.Add(s.Key.ToString().Trim());
                        Console.WriteLine(s.Key.ToString().Trim() + "\r\n");
                    }
                }
            }
            zmatches_array = zmatches.ToArray();
            for (int x = 0; x < zmatches_array.Length; x++)
            {
                if (x != zmatches_array.Length - 1)
                {
                    zoutput += zmatches[x].ToUpper() + "_";
                }
                else
                {
                    zoutput += zmatches_array[x].ToUpper();
                }

            }
            
            return zoutput;
        }

        public string GD_Compliance_Check(List<string> CompiledHeadersList, out List<string> zErrorsList, string fileinfoitems = "", string compliancefile = @"C:\Temp\MM.csv")
        {
            List<List<string>> ErrorsList = new List<List<string>>();
            List<string> zErrorsListA = new List<string>();
            Dictionary<string, List<string>> ErrorsCatalogue = new Dictionary<string, List<string>>();
            List<KeyValuePair<string, ParameterKey>> zParameters = new List<KeyValuePair<string, ParameterKey>>();
            List<KeyValuePair<string, Category>> zCategories = new List<KeyValuePair<string, Category>>();
            List<string> zBreakdownErrors = new List<string>();
            string currentkey = "";
            try
            {
                foreach (string x in CompiledHeadersList)
                {
                    currentkey = x;
                    bool zSuxes = WSD.Utility.FcmParameter.Parser.HeaderBreakdown.BreakDownHeader(x, out zParameters, out zCategories, out zBreakdownErrors);
                    if (!zSuxes)
                    {
                        ErrorsCatalogue.Add(x, zBreakdownErrors);
                        
                        zErrorsListA.AddRange(zBreakdownErrors);
                    }
                }


                string finalresult = WriteCompliancFile(compliancefile, ErrorsCatalogue, fileinfoitems);

                zErrorsList = zErrorsListA;
                return (zErrorsList.Count>0 ? "Headers not GD compliant":"");
            }
            catch (Exception e)
            {

                MessageBox.Show("EXCEPTION:  " + "\r\n\r\n\r\n\r\n"+ e.ToString() + "\r\n\r\n\r\n\r\n" +
                    "currentkey is:  " + currentkey + "\r\n\r\n\r\n\r\n" + "DUPLICATE HEADERS PROBLEM!!!" + "\r\n\r\n" + "Program loading will Abort", "GD_Compliance_Check Error");
                zErrorsList = zBreakdownErrors;
                return "DUPLICATE HEADERS PROBLEM detected during GD Compliance Check";
                //throw;
            }
        }

        public string WriteCompliancFile(string zFile, Dictionary<string, List<string>> ErrorsCatalogue, string fileinfoitems = "")
        {
            using (StreamWriter compliancefile = new StreamWriter(zFile, false))
            {
                compliancefile.WriteLine("Time Stamp:," + DateTime.Now + "\r\n");
                compliancefile.WriteLine("Description:," + fileinfoitems + "\r\n");
                compliancefile.WriteLine("#Start");
                compliancefile.WriteLine("Item,Error");

                foreach (KeyValuePair<string, List<string>> zErrorsDict in ErrorsCatalogue)
                {
                    string zflatcsv = String.Join(",",  zErrorsDict.Value.Select(x => x.ToString()).ToArray());
                    compliancefile.WriteLine(zErrorsDict.Key + "," + zflatcsv + "\r\n");
                }
                //List<int> myValues;
                //string csv = String.Join(",", myValues.Select(x => x.ToString()).ToArray());
                compliancefile.WriteLine("#End");
                compliancefile.Close();
            }

            return "";
        }
                

        private string HeaderValueMuncher(string TestConditionValue, int HeaderEntryIndex, string zCats)
        {
            string retval = TestConditionValue;

            if (TestConditionValue != "")
            {
                switch (zCats)
                {
                    case "Test Mode":
                        switch (TestConditionValue.ToUpper())
                        {
                            case "FBAR":
                            //case "DC":
                            case "COMMON":
                                retval = "F";
                                break;
                            //case "DC":
                            //    retval = "DC";
                            //    break;
                            //case "COMMON":
                            //    retval = "COM";
                            //    break;
                            case "DC":
                                retval = "DC";
                                break;
                            case "RF":
                            default:
                                retval = "PT";
                                break;

                        }

                        break;
                    case "Channel Number":
                        retval = "CH" + TestConditionValue;
                        break;
                    case "N-Parameter-Class":
                        // This line is not GE compliant.
                        //retval = retval.Replace("-", "_");
                        break;
                    case "N_Mode":
                        //nothing to be done here yet
                        break;
                    case "Temp":
                        //nothing to be done here yet
                        break;
                    case "TUNABLE_BAND":
                        //nothing to be done here yet
                        break;
                    case "Selected_Port":
                        //nothing to be done here yet
                        break;
                    case "TX_Out":
                    case "RX_Output1":
                        retval = "OUT" + TestConditionValue;
                        break;
                    case "Power_Mode":
                        retval = (TestConditionValue == "RX" ? "" : TestConditionValue);
                        break;
                    case "S-Parameter":

                        break;
                    case "Start_Freq":
                    case "Stop_Freq":
                        retval = retval.Replace(" ", "") + "Hz";
                        break;
                    case "Switch_ANT":

                        break;
                    case "Modulation":

                        break;
                    case "Waveform":

                        break;
                    case "Pout":
                        retval = TestConditionValue + "dBm";
                        break;
                    case "Freq":
                        retval = TestConditionValue + "MHz";
                        break;
                    case "ParameterNote":
                        retval = (TestConditionValue != "" ? "_NOTE_" + TestConditionValue : "");
                        break;
                    case "CPL_Ctrl":
                        retval = (TestConditionValue != "" ? TestConditionValue : "");
                        break;
                }
            }

            return retval;
        }

        public string HeaderMuncher(Dictionary<string, string> TestConditions, string zVcc, string zVdd, string zTxreg, string zRxreg)
        {
            string zfinalheader = "";
            string zmunchedval = "";
            var lastkey = HeaderEntries.Keys.Last();
            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                zmunchedval = HeaderValueMuncher(GetStr(TestConditions, x.Value), x.Key, x.Value);
                if (GetStr(TestConditions, x.Value) == "") { continue; }
                zfinalheader += zmunchedval + "_";
            }
            //zfinalheader += zVcc + "Vcc_" + zVdd + "Vlna_" + "DACQ1x" + zTxreg + "_" + zRxreg;
            return zfinalheader;
        }

        public string HeaderMuncher(Dictionary<string, string> TestConditions, string zVcc, string zVdd, string zVBatt, string zVBiast, string zVcpl, string zTxreg, string zRxreg)
        {
            string zfinalheader = "";
            string zmunchedval = "";
            var lastkey = HeaderEntries.Keys.Last();
            foreach (KeyValuePair<int, string> x in HeaderEntries)
            {
                zmunchedval = HeaderValueMuncher(GetStr(TestConditions, x.Value), x.Key, x.Value);
                if (GetStr(TestConditions, x.Value) == "") { continue; }
                zfinalheader += zmunchedval + "_";
            }
            //zfinalheader += zVcc + "Vcc_" + zVdd + "Vlna_" + zVBatt + "Vbatt_" + zVBiast + "Vbiast_" + zVcpl + "Vcpl_" + "DACQ1x" + zTxreg + "_" + zRxreg;
            return zfinalheader;
        }

        public string GetStr(Dictionary<string, string> dic, string theKey)
        {
            try
            {
                return dic[theKey];
            }
            catch
            {
                MessageBox.Show("Test Condition File doesn't contain column \"" + theKey + "\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //programLoadSuccess = false;
            }

            return "";
        }

    }
}
