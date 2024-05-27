using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibMQTTDriver;
using Avago.ATF.CrossDomainAccess;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Web.Helpers;
using MPAD_TestTimer;
using System.Text.RegularExpressions;


namespace ProductionLib2
{
    public struct SLot_info
    {
        public LotinfoItem LotInfo;

        public struct LotinfoItem
        {
            public string packagename { get; set; }
            public string lotid { get; set; }
            public string sublotid { get; set; }
            public string operatorid { get; set; }
            public string mfgid { get; set; }
            public string handlerid { get; set; }
            public bool alive { get; set; }
            public string timestamp { get; set; }
        }
    }

    public struct SUnit_Info
    {
        public UnitinfoItem UnitInfo;

        public struct UnitinfoItem
        {
            public string packagename { get; set; }
            public string lotid { get; set; }
            public string sublotid { get; set; }
            public int pid { get; set; }
            public string handlerid { get; set; }
            public string testerid { get; set; }
            public int site { get; set; }
            public int arm { get; set; }
            public double contactx { get; set; }
            public double contacty { get; set; }
            public double contactz { get; set; }
            public double handler_force_setting { get; set; }
            public double handler_ep_offset { get; set; }
            public int touchdown_count { get; set; }
            public double loadboard_temperature { get; set; }
            public string timestamp { get; set; }
        }
    }   

    public struct STest_Info
    {
        public TestinfoItem TestInfo;

        public struct TestinfoItem
        {
            public string packagename { get; set; }
            public string lotid { get; set; }
            public string sublotid { get; set; }
            public int pid { get; set; }
            public string module_2DID { get; set; }
            public string handlerid { get; set; }
            public string testerid { get; set; }
            public int site { get; set; }
            public int arm { get; set; }
            public int TestStatus { get; set; }
            public double siteyield { get; set; }
            public Dictionary<string, string> Top_Reject_Parameter { get; set; }
            public string FirstFail_Parameter { get; set; }
            public int FirstFail_Parameter_Num { get; set; }
            public double test_time { get; set; }
            public string timestamp { get; set; }
        }
    }

    public struct SMQTTsetting
    {
        public bool MQTTflag;
        public bool UnitInfoMQTTflagMQ;
        public bool TestInfoMQTTflag;
        public bool LotInfoMQTTflag;
        public string Source_Server;
        public string Source_Username;
        public string Source_Password;
        public string Topic_Source;    //From other broker server
        public string Topic_LocalHost;  // Local host broker
        public int site;
    }

    public struct SSetLotProfile
    {
        public setlotprofile Sethandlerlotprofile;

        public struct setlotprofile
        {
            public string lotid { get; set; }
            public string sublotid { get; set; }
            public string devicename { get; set; }
            public string devicecode { get; set; }
        }
    }

    public class MQTT_MachineData
    {
        public LibMqttDriver client_source = new LibMqttDriver();
        private LibMqttDriver client_localhost = new LibMqttDriver();
        private Queue<ReceivedQueue> receivedQueue;
        private Mutex mutex = new Mutex();

        private SMQTTsetting _MQTTSetting;
        private SLot_info Lot_info;
        private SUnit_Info Unit_Info;
        private STest_Info Test_Info;
        private SSetLotProfile MQTTLotProfile;

        private string pubtopic_LotInfo = "";
        private string pubtopic_TestInfo = "";
        private string pubtopic_UnitInfo = "";
        private string pubtopic_LotProfile = "";

        private List<SLot_info> PrevLot_info = new List<SLot_info>();
        private List<SUnit_Info> PrevUnit_info = new List<SUnit_Info>();
        private List<STest_Info> PrevTest_info = new List<STest_Info>();

        private string str_MQTThandlerid;
        private string str_MQTTTesterid;
        private string preHandler_Arm = "0";
        private string prePIDx = "0";
        private string module_2DID_last = "-999";
        private Dictionary<string, RangeDef> _dicSpecRange;

        //Machine data
        string Handler_ArmNo = "-1";
        string Handler_EPValue = "-1";
        string Handler_EPoffset = "-1";
        string Handler_ContactX = "-1";
        string Handler_ContactY = "-1";
        string Handler_ContactZ = "-1";
        string Handler_TrayX = "-1";
        string Handler_TrayY = "-1";
        string Handler_TouchDownCount = "-1";
        double PlungerForce = -1;

        readonly int idx = -1;
        readonly int idlength = -1;
        readonly string _testpackagename = "";
        readonly string strLotID = "";
        readonly string strSubLotID = "";
        readonly string strOperatorID = "";
        readonly string strmfgID = "";
        readonly string MaxSiteNumber = "";
        public string strTesterid { get; private set; } = "";

        public MQTT_MachineData()
        {
            idx = (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, @"C:\DEBUG\NONE")).LastIndexOf("\\");
            idlength = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, @"C:\DEBUG\NONE").Length;
            _testpackagename = (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, @"C:\DEBUG\NONE")).Substring(idx + 1, idlength - (idx + 1));
            strLotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "PT0000000001");
            strSubLotID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "1A");
            strOperatorID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "A0001");
            strmfgID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_ASSEMBLY_ID, "MFG000000");
            MaxSiteNumber = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_Handler_MaxSitesNum, "2");
            strTesterid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "");    //Current PID
            _dicSpecRange = ATFSharedData.Instance.TestLimitData.TSF.TestLimitsRange;
        }

        public void MQTTInit(SMQTTsetting MQTTSetting)
        {
            _MQTTSetting = new SMQTTsetting();

            _MQTTSetting.MQTTflag = MQTTSetting.MQTTflag;
            _MQTTSetting.LotInfoMQTTflag = MQTTSetting.LotInfoMQTTflag;
            _MQTTSetting.UnitInfoMQTTflagMQ = MQTTSetting.UnitInfoMQTTflagMQ;
            _MQTTSetting.TestInfoMQTTflag = MQTTSetting.TestInfoMQTTflag;
            _MQTTSetting.Source_Username = MQTTSetting.Source_Username;
            _MQTTSetting.Source_Password = MQTTSetting.Source_Password;
            _MQTTSetting.Source_Server = MQTTSetting.Source_Server;
            _MQTTSetting.Topic_Source = MQTTSetting.Topic_Source;
            _MQTTSetting.Topic_LocalHost = MQTTSetting.Topic_LocalHost;
            _MQTTSetting.site = MQTTSetting.site;

            if (_MQTTSetting.MQTTflag)
            {
                try
                {
                    str_MQTThandlerid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "HTXXX");
                    if (str_MQTThandlerid == "") str_MQTThandlerid = "HTXXX";
                    Unit_Info.UnitInfo.handlerid = str_MQTThandlerid;
                }

                catch { str_MQTThandlerid = "HTXX"; }

                try
                {
                    str_MQTTTesterid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "HTXXX-01");
                    if (str_MQTTTesterid == "") str_MQTTTesterid = "HTXXX-01";
                }
                catch { str_MQTTTesterid = "HTXXX-01"; }

                var testersite = str_MQTTTesterid.Substring(str_MQTTTesterid.Length - 1);

                //Connect to broker;
                client_source.Connect("BRCMLocalhost_" + str_MQTTTesterid+"_"+_MQTTSetting.site, _MQTTSetting.Source_Server,
                    _MQTTSetting.Source_Username, _MQTTSetting.Source_Password, 0);

                LoggingManager.Instance.LogHighlight( string.Format("MQTT Site{0} --> On Connect: " + client_source.client.IsConnected.ToString(), _MQTTSetting.site));

                //define topic here (Later)
                //*******************************
                pubtopic_LotInfo = _MQTTSetting.Topic_Source + str_MQTThandlerid + "/" + str_MQTTTesterid + "/" + "LotInfo";
                pubtopic_TestInfo = _MQTTSetting.Topic_Source + str_MQTThandlerid + "/" + str_MQTTTesterid + "/" + "TestInfo";
                pubtopic_UnitInfo = _MQTTSetting.Topic_Source + str_MQTThandlerid + "/" + str_MQTTTesterid + "/" + "UnitInfo";
                pubtopic_LotProfile = "Hontech/HT7145/" + str_MQTTTesterid + "/" + "Sethandlerlotprofile";
                //*************************************
                //Init subcribe event trigger
                if (client_source.client != null)
                {
                    client_source.client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                    //client_source.Subcribe()
                }
            }
        }

        public void MQTTExexLotInfo(bool bln_alive, bool retainMssg)
        {
            Lot_info = new SLot_info();

            PrevLot_info.Clear();

            Lot_info.LotInfo.packagename = _testpackagename;
            Lot_info.LotInfo.lotid = strLotID;
            Lot_info.LotInfo.sublotid = strSubLotID;
            Lot_info.LotInfo.operatorid = strOperatorID;
            Lot_info.LotInfo.mfgid = strmfgID;
            Lot_info.LotInfo.alive = bln_alive;
            Lot_info.LotInfo.handlerid = str_MQTThandlerid;
            Lot_info.LotInfo.timestamp = client_source.GetTimestamp(DateTime.Now);

            var jsonMessage = Json.Encode(Lot_info);
            client_source.DataPublish(pubtopic_LotInfo, jsonMessage, 2, retainMssg);

        }

        public void MQTTExecTestInfo(byte testSite, int TmpUnitNo)
        {        
            
            int testersite = 0;
            string[] strFirstFail_Parameter = new string[2];
            string[] strFirstFail_Parameter_num = new string[2];
            string[] strTopRejectParameterCount = new string[2];
            string[] strteststatus = new string[2];
            string[] siteyield = new string[2];
            string[] strTopRejectParameter = new string[2];
            List<string> listTestStatusA = new List<string>();
            List<string> listTopRejectParameter = new List<string>();
            List<string> lstTopRejPara_T = new List<string>();
            List<string> listTopRejectParameterCount = new List<string>();
            List<string> listTesttime = new List<string>();
            Test_Info.TestInfo.Top_Reject_Parameter = new Dictionary<string, string>();

            int debugIndex = 0;

            try
            {
                //Retrieve module 2DID from camera
                string Key2DID = string.Format("2DID_OTPID_{0}", testSite + 1);
                string Camera2DIDstr = ATFCrossDomainWrapper.GetStringFromCache(Key2DID, "00000000000000000000");
                Camera2DIDstr = Regex.Replace(Camera2DIDstr, @"[^\d]", "");

                if (TmpUnitNo == 1)
                {
                    PrevTest_info.Clear();
                    module_2DID_last = Camera2DIDstr;
                    preHandler_Arm = ATFCrossDomainWrapper.GetStringFromCache("ARM_NO_" + (testSite + 1), "0");  //Current
                    prePIDx = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0000");
                }
                else if (TmpUnitNo > 1)
                {

                    PrevTest_info.Clear();
                    if (Test_Info.TestInfo.Top_Reject_Parameter.Count > 0) Test_Info.TestInfo.Top_Reject_Parameter.Clear();
                    debugIndex++;
                    int pidx = Convert.ToInt32(prePIDx) + testSite;    //Current PID
                    string strTesterid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "");    //Current PID
                    if (strTesterid == "") strTesterid = "HT001-01";

                    if (MaxSiteNumber == "1")
                    {
                        testersite = int.Parse(strTesterid.Substring(strTesterid.Length - 1));
                    }
                    else
                    {
                        testersite = testSite + 1;
                    }

                    debugIndex++;
                    Handler_ArmNo = preHandler_Arm;  //get pre
                    strteststatus = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_PASSFAIL_SITE, "0").Split(';'); //Previous Pass/Fail result
                    siteyield = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_YIELD_SITE, "0000").Split(';'); //Previous Yield result   
                    if (short.Parse(MaxSiteNumber) > 1)
                    {
                        strTopRejectParameter = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TOP_REJECT_PARAM_SITE, "").Split(';'); //Previous top reject parameter                                                                                                                                
                        if (strTopRejectParameter[0] == "")     //For No any failure on testing. 100% yield set to some dummy value
                        {
                            string strParamDefault = ";";
                            strTopRejectParameter = strParamDefault.Split(';');
                        }
                    }
                    else
                        strTopRejectParameter = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TOP_REJECT_PARAM, "").Split(';'); //Previous top reject parameter                                                                                                                              

                    listTopRejectParameter = strTopRejectParameter[testSite].Split(',').ToList();

                    //Check whether the number count
                    if (short.Parse(MaxSiteNumber) > 1)
                    {
                        strTopRejectParameterCount = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TOP_REJECT_COUNT_SITE, "").Split(';'); //Previous top reject parameter
                        if (strTopRejectParameterCount[0] == "")
                        {
                            string strParamCountDefault = ";";
                            strTopRejectParameterCount = strParamCountDefault.Split(';');
                        }
                    }
                    else
                        strTopRejectParameterCount = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TOP_REJECT_COUNT, "").Split(';'); //Previous top reject parameter


                    //if (strTopRejectParameterCount[testSite] == "") strTopRejectParameterCount[testSite] = "0,0,0";

                    listTopRejectParameterCount = strTopRejectParameterCount[testSite].Split(',').ToList();

  
                    strFirstFail_Parameter = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_FIRST_REJECT_PARAM_SITE, "").Split(';'); //Previous first fail parameter
                    if (strFirstFail_Parameter[0] == "")
                    {
                        string strFirstParamDefault = "NA;NA";
                        strFirstFail_Parameter = strFirstParamDefault.Split(';');
                    }


                    //Check whether the number count & Retrieve fail parameter number
                    strFirstFail_Parameter_num = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_FIRST_REJECT_PARAM_NUM_SITE, "").Split(';'); //Previous First fail parameter n
                    if (strFirstFail_Parameter_num[0] == "")
                    {
                        string strParamCountDefault = "0;0";
                        strFirstFail_Parameter_num = strParamCountDefault.Split(';');
                    }


                    debugIndex++;
                    //********************Exclude MIPI fail flag unwanted parameter**********************
                    #region remove failure parameter
                    //Hard removed it
                    //for (int xx = 0; xx < listTopRejectParameter.Count; xx++)
                    //{
                    //    LoggingManager.Instance.LogInfoTestPlan("Before TOP REJECT PARAMETER: " + listTopRejectParameter[xx]);
                    //    LoggingManager.Instance.LogInfoTestPlan("Before TOP REJECT PARAMETER Count: " + listTopRejectParameterCount[xx]);
                    //}
                    //LoggingManager.Instance.LogInfoTestPlan("Before TOP REJECT PARAMETER Length: " + listTopRejectParameter.Count);
                    //LoggingManager.Instance.LogInfoTestPlan("Before TOP REJECT PARAMETER Length: " + listTopRejectParameterCount.Count);

                    //LoggingManager.Instance.LogInfoTestPlan("Before Remove: " + debugIndex++);
                    lstTopRejPara_T = strTopRejectParameter[testSite].Split(',').ToList();

                    foreach (string paramx in lstTopRejPara_T)
                    {
                        switch (paramx)
                        {
                            case "M_Flag_LockBit":
                            case "M_FLAG_LOCKBIT_RX":
                            case "M_OTP_READ-RX-PASS-FLAG":
                            case "M_OTP_READ-RF2-PASS-FLAG":
                            case "M_RF2-PASS-FLAG":
                            case "M_OTP_MFG-ID-READ-ERROR":
                            case "M_OTP_READ-NFR-OUTLIER-FLAG_NOTE_FULLCLK_x":
                            case "M_OTP_READ-RF1-OUTLIER-FLAG_NOTE_FULLCLK_x":
                            case "M_RF1-OUTLIER-FLAG":
                            case "OUTLIER_SUMVAL":
                            case "M_OTP_READ-MFG-ID-ERROR":
                            case "M_OTP_MODULE_ID_2DID_DELTA":
                                int indxM_Param = listTopRejectParameter.IndexOf(paramx);
                                listTopRejectParameter.Remove(paramx);
                                //LoggingManager.Instance.LogInfoTestPlan("Index param:" + indxM_Param);
                                listTopRejectParameterCount.RemoveAt(indxM_Param);

                                break;
                        }

                    }

                    //for (int xx = 0; xx < listTopRejectParameter.Count; xx++)
                    //{
                    //    LoggingManager.Instance.LogInfoTestPlan("after TOP REJECT PARAMETER: " + listTopRejectParameter[xx]);
                    //    LoggingManager.Instance.LogInfoTestPlan("after TOP REJECT PARAMETER Count: " + listTopRejectParameterCount[xx]);
                    //}
                    //LoggingManager.Instance.LogInfoTestPlan("after TOP REJECT PARAMETER Length: " + listTopRejectParameter.Count);
                    //LoggingManager.Instance.LogInfoTestPlan("after TOP REJECT PARAMETER Length: " + listTopRejectParameterCount.Count);
                    #endregion
                    //************************************************************************


                    int ListTopRejectParaLengthx = listTopRejectParameter.Count();
                    if (ListTopRejectParaLengthx >= 3)
                        ListTopRejectParaLengthx = 3;

                    string[] strtesttime = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TESTTIME_SITE, "0000").Split(';'); //Previous test time                                                                               
                    if (strtesttime[testSite] == "")
                    {
                        //Dummy test time
                        strtesttime[testSite] = "0.001";
                    }

                    //Parse to struct class and publish to Broker
                    Test_Info.TestInfo.packagename = _testpackagename;
                    Test_Info.TestInfo.lotid = strLotID;
                    Test_Info.TestInfo.sublotid = strSubLotID;
                    Test_Info.TestInfo.pid = pidx;
                    Test_Info.TestInfo.handlerid = str_MQTThandlerid;
                    Test_Info.TestInfo.testerid = strTesterid;
                    Test_Info.TestInfo.module_2DID = module_2DID_last;
                    Test_Info.TestInfo.site = testersite;
                    Test_Info.TestInfo.arm = int.Parse(Handler_ArmNo);
                    Test_Info.TestInfo.TestStatus = int.Parse(strteststatus[testSite]);
                    Test_Info.TestInfo.siteyield = Convert.ToDouble(siteyield[testSite]);
                    Test_Info.TestInfo.FirstFail_Parameter = strFirstFail_Parameter[testSite];
                    Test_Info.TestInfo.FirstFail_Parameter_Num = int.Parse(strFirstFail_Parameter_num[testSite]);
                    double TestParamUSL = 0;
                    double TestParamLSL = 0;

                    //Only publish 3 top reject params
                    for (int iTRP = 0; iTRP < 3; iTRP++)
                    {
                        if ( listTopRejectParameter != null && (iTRP <= ListTopRejectParaLengthx-1) && listTopRejectParameter[iTRP] != "")
                        {
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1), listTopRejectParameter[iTRP]);
                            TestParamUSL = _dicSpecRange[listTopRejectParameter[iTRP]].TheMax;
                            TestParamLSL = _dicSpecRange[listTopRejectParameter[iTRP]].TheMin;
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "HighL", TestParamUSL.ToString());
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "LowL", TestParamLSL.ToString());
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "Count", listTopRejectParameterCount[iTRP]);
                        }
                        else
                        {
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1), "NA" + (iTRP + 1));
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "HighL", "NA");
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "LowL", "NA");
                            Test_Info.TestInfo.Top_Reject_Parameter.Add("TopRejectParam" + (iTRP + 1) + "Count", "0");
                        }
                    }

                    Test_Info.TestInfo.test_time = Convert.ToDouble(strtesttime[testSite]);
                    Test_Info.TestInfo.timestamp = client_source.GetTimestamp(DateTime.Now);
                    //Publish Unit info to Inari server
                    var jsonMessage = Json.Encode(Test_Info);
                    client_source.DataPublish(pubtopic_TestInfo, jsonMessage.ToString());

                    LoggingManager.Instance.LogHighlight(string.Format("MQTT Site{0}--> On Connect: " + client_source.client.IsConnected.ToString(), testSite));

                    preHandler_Arm = ATFCrossDomainWrapper.GetStringFromCache("ARM_NO_" + (testSite + 1), "0");  //Store for next publish
                    prePIDx = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0000");
                    module_2DID_last = Camera2DIDstr;
                }

            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogError(string.Format("Add Test Info Error: {0} ", debugIndex) + ex);
            }

        }

        public void MQTTExecUnitInfo(byte testSite, int TmpUnitNo, double loadboardtemperature)
        {
            int testersite = 0;
            Unit_Info = new SUnit_Info();
            try
            {
                if (strTesterid == "") strTesterid = "HT001-01";

                if (MaxSiteNumber == "1")
                {
                    testersite = int.Parse(strTesterid.Substring(strTesterid.Length - 1));
                }
                else
                {
                    testersite = testSite + 1;
                }

                if(TmpUnitNo >1)
                {
                    //Publish Unit info to Inari server
                    var jsonMessage = Json.Encode(PrevUnit_info[0]);
                    client_source.DataPublish(pubtopic_UnitInfo, jsonMessage.ToString());
                }

                PrevUnit_info.Clear();

                int pidx = Convert.ToInt32(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "0000")) + testSite;
                Handler_EPValue = ATFCrossDomainWrapper.GetStringFromCache("EP_VALUE_" + (testSite + 1), "0");
                PlungerForce = Convert.ToDouble(ATFCrossDomainWrapper.GetStringFromCache("PLUNGFORCE_" + (testSite + 1), "0"));
                Handler_ContactX = ATFCrossDomainWrapper.GetStringFromCache("CONTACTX_" + (testSite + 1), "0");
                Handler_ContactY = ATFCrossDomainWrapper.GetStringFromCache("CONTACTY_" + (testSite + 1), "0");
                Handler_ContactZ = ATFCrossDomainWrapper.GetStringFromCache("CONTACTZ_" + (testSite + 1), "0");
                Handler_EPoffset = ATFCrossDomainWrapper.GetStringFromCache("EPOFFSET_" + (testSite + 1), "0");
                Handler_TrayX = ATFCrossDomainWrapper.GetStringFromCache("TRAYCOORDINATE_X_" + (testSite + 1), "0");
                Handler_TrayY = ATFCrossDomainWrapper.GetStringFromCache("TRAYCOORDINATE_Y_" + (testSite + 1), "0");
                Handler_TouchDownCount = ATFCrossDomainWrapper.GetStringFromCache("TOUCHDOWNCOUNT_" + (testSite + 1), "0");
                double LB_Temperature = loadboardtemperature;
                Handler_ArmNo = ATFCrossDomainWrapper.GetStringFromCache("ARM_NO_" + (testSite + 1), "0");  //Current

                Unit_Info.UnitInfo.packagename = _testpackagename;
                Unit_Info.UnitInfo.lotid = strLotID;
                Unit_Info.UnitInfo.sublotid = strSubLotID;
                Unit_Info.UnitInfo.pid = pidx;
                Unit_Info.UnitInfo.contactx = Convert.ToDouble(Handler_ContactX);
                Unit_Info.UnitInfo.contacty = Convert.ToDouble(Handler_ContactY);
                Unit_Info.UnitInfo.contactz = Convert.ToDouble(Handler_ContactZ);
                Unit_Info.UnitInfo.arm = int.Parse(Handler_ArmNo);
                Unit_Info.UnitInfo.handler_force_setting = Convert.ToDouble(PlungerForce);
                Unit_Info.UnitInfo.handler_ep_offset = Convert.ToDouble(Handler_EPoffset);
                Unit_Info.UnitInfo.touchdown_count = int.Parse(Handler_TouchDownCount);
                Unit_Info.UnitInfo.loadboard_temperature = Convert.ToDouble(LB_Temperature);
                Unit_Info.UnitInfo.handlerid = str_MQTThandlerid;
                Unit_Info.UnitInfo.testerid = strTesterid;
                Unit_Info.UnitInfo.site = testersite;
                Unit_Info.UnitInfo.timestamp = client_source.GetTimestamp(DateTime.Now);

                PrevUnit_info.Add(Unit_Info);               
            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogError(string.Format("MQTTExecUnitInf Error: {0} ", ex));
            }
        }

        public void MQTTSetLotprofile(string devicename, bool retainMssg = false)
        {
            MQTTLotProfile = new SSetLotProfile();

            int idxtestpackage = _testpackagename.IndexOf("_");
            string _devicecode = _testpackagename.Substring(0, idxtestpackage);

            MQTTLotProfile.Sethandlerlotprofile.lotid = strLotID;
            MQTTLotProfile.Sethandlerlotprofile.sublotid = strSubLotID;
            MQTTLotProfile.Sethandlerlotprofile.devicename = devicename;
            MQTTLotProfile.Sethandlerlotprofile.devicecode = _devicecode;
            Lot_info.LotInfo.handlerid = str_MQTThandlerid;
            Lot_info.LotInfo.timestamp = client_source.GetTimestamp(DateTime.Now);

            var jsonMessage = Json.Encode(MQTTLotProfile);
            client_source.DataPublish(pubtopic_LotProfile, jsonMessage, 0, retainMssg);
        }

        public void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Receive MQTT event from broker and queue the message

            try
            {
                receivedQueue.Enqueue(new ReceivedQueue(e.Topic, Encoding.UTF8.GetString(e.Message)));
            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogError(string.Format("Client_MqttMsgPublishReceived Info Error: {0} ", ex));
            }
        }
    }

    public class ReceivedQueue
    {
        private string _msg { get; } = "";
        private string _topic { get; } = "";

        public ReceivedQueue(string topic, string message)
        {
            _topic = topic;
            _msg = message;
        }
    }
}
