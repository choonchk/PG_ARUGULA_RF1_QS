using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avago.ATF.Logger;
using Avago.ATF.StandardLibrary;
using MPAD_TestTimer;

namespace TestPlanCommon.CommonModel
{
    public class TestPlanStateModel
    {
        public bool Spara_Site { get; set; }
        public bool Pa_Site { get; set; }
        /// <summary>
        /// False if any error is encountered in ATFInit().
        /// </summary>
        public bool programLoadSuccess { get; set; }
        public bool programUnloaded { get; set; }
        public bool litedrivermode { get; set; }

        public int fbar_tester_id = 0;

        private TesterManager m_modelTester;

        public ValidationDataObject ValidationDataObject { get; set; }

        private string currentTestResultFileName;

        public string CurrentTestResultFileName
        {
            get { return currentTestResultFileName.Replace(".CSV", ""); }
            set { currentTestResultFileName = value; }
        }

        public ITesterSite TesterSite
        {
            get { return m_modelTester.CurrentTester; }
        }

        public TestPlanStateModel()
        {
            Spara_Site = true;     // scope: Init+Test
            Pa_Site = false;       // scope: Init+Test
            programLoadSuccess = true;
            ValidationDataObject = new ValidationDataObject();
        }

        /// <summary>
        /// handle tester variation.
        /// </summary>
        /// <param name="currentTester"></param>
        public void SetTesterSite(ITesterSite currentTester)
        {
            m_modelTester = new TesterManager(currentTester);
        }

        public void SetUnloaded()
        {
            programUnloaded = true;
        }

        public void SetCurrentTestResult(string fileName)
        {
            currentTestResultFileName = fileName;
        }

        public void SetLoadFail()
        {
            programLoadSuccess = false;
        }

        public void SetLoadFail(bool isPass)
        {
            programLoadSuccess = programLoadSuccess && isPass;
        }

        public void SetLoadFail(ValidationDataObject vdo)
        {
            programLoadSuccess = programLoadSuccess && vdo.IsValidated;
            PromptManager.Instance.ShowError(vdo);
        }

        /// <summary>
        /// Check ValidationDataObject.IsValidated after call this.
        /// </summary>
        /// <param name="errorMessage"></param>
        public void SetLoadFail(string errorMessage)
        {
            ValidationDataObject.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Return true if tester is not defined.
        /// </summary>
        public bool CheckTesterType(string configXmlPath)
        {
            #region Check Test Site
            string tester_type = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_TYPE, "").Trim().ToUpper();

            switch (tester_type)
            {
                case "":
                    if (File.Exists(configXmlPath))
                    {
                        XmlDocument clothoConfig = new XmlDocument();
                        clothoConfig.Load(configXmlPath);

                        XmlNodeList clothoConfigNodes = clothoConfig.SelectNodes("/ATFConfiguration/ToolSection/ConfigItem");

                        foreach (XmlNode xn in clothoConfigNodes)
                        {
                            if (xn.Attributes["name"].Value.ToString().Trim() == "TesterType")
                            {
                                if (xn.Attributes["value"].Value.ToString().Trim().ToUpper() == "BE-PXI-NI")
                                {
                                    Pa_Site = true;
                                    Spara_Site = false;
                                }
                                else if (xn.Attributes["value"].Value.ToString().Trim().ToUpper() == "BE-PXI-KEYSIGHT")
                                {
                                    Pa_Site = false;
                                    Spara_Site = true;
                                }
                                else
                                {
                                    Pa_Site = false;
                                    Spara_Site = false;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        Pa_Site = false;
                        Spara_Site = false;
                    }

                    break;
                case "BE-PXI-NI":
                    Pa_Site = true;
                    Spara_Site = false;
                    break;
                case "BE-PXI-KEYSIGHT":
                    Pa_Site = false;
                    Spara_Site = true;
                    break;
                default:
                    Pa_Site = false;
                    Spara_Site = false;
                    break;
            }

            bool isCondition1 = Pa_Site == false && Spara_Site == false;
            return isCondition1;

            #endregion
        }

        /// <summary>
        /// Return true if tester is not defined.
        /// </summary>
        public bool CheckTesterType2(string configXmlPath)
        {
            #region Check Test Site
            string tester_type = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_TYPE, "").Trim().ToUpper();

            string zrootpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "");


            switch (tester_type)
            {
                case "":
                    if (File.Exists(configXmlPath))
                    {
                        XmlDocument clothoConfig = new XmlDocument();
                        clothoConfig.Load(configXmlPath);

                        XmlNodeList clothoConfigNodes = clothoConfig.SelectNodes("/ATFConfiguration/ToolSection/ConfigItem");

                        foreach (XmlNode xn in clothoConfigNodes)
                        {
                            if (xn.Attributes["name"].Value.ToString().Trim() == "TesterType")
                            {
                                if (xn.Attributes["value"].Value.ToString().Trim().ToUpper() == "BE-PXI-NI")
                                {
                                    Pa_Site = true;
                                    Spara_Site = true;
                                }
                                else if (xn.Attributes["value"].Value.ToString().Trim().ToUpper() == "BE-PXI-KEYSIGHT")
                                {
                                    Pa_Site = false;
                                    Spara_Site = true;
                                }
                                else
                                {
                                    Pa_Site = false;
                                    Spara_Site = false;
                                }
                            }

                            if (xn.Attributes["name"].Value.ToString().Trim() == "TesterID")
                            {
                                string[] ID = xn.Attributes["value"].Value.ToString().Trim().ToUpper().Split('-');

                                //TODO CCT Commented out for Joker.
                                //TCF_Setting["TesterID"] = ID[ID.Length - 1];


                                string tester_id = xn.Attributes["value"].Value.ToString().Trim().ToUpper();
                                if (tester_id.EndsWith("-01") == true)
                                {
                                    fbar_tester_id = 1;
                                }
                                else if (tester_id.EndsWith("-02") == true)
                                {
                                    fbar_tester_id = 2;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        Pa_Site = false;
                        Spara_Site = false;
                    }

                    break;
                case "BE-PXI-NI":
                    Pa_Site = true;
                    Spara_Site = true;
                    break;
                case "BE-PXI-KEYSIGHT":
                    Pa_Site = false;
                    Spara_Site = true;
                    break;
                default:
                    Pa_Site = false;
                    Spara_Site = false;
                    break;
            }

            bool isCondition1 = Pa_Site == false && Spara_Site == false;
            return isCondition1;

            #endregion
        }

        public string GetTesterId(string configXmlPath)
        {
            string result = "";
            if (File.Exists(configXmlPath))
            {
                XmlDocument clothoConfig = new XmlDocument();
                clothoConfig.Load(configXmlPath);

                XmlNodeList clothoConfigNodes = clothoConfig.SelectNodes("/ATFConfiguration/ToolSection/ConfigItem");

                foreach (XmlNode xn in clothoConfigNodes)
                {
                    if (xn.Attributes["name"].Value.ToString().Trim() == "TesterID")
                    {
                        string[] ID = xn.Attributes["value"].Value.ToString().Trim().ToUpper().Split('-');

                        result = ID[ID.Length - 1];

                        break;
                    }
                }
            }

            return result;
        }

        #region PA TP calls.

        public void SetTesterType(string testerType)
        {
            switch (testerType)
            {
                case "PA":
                    Pa_Site = true;
                    Spara_Site = false;
                    break;
                case "FBAR":
                    Pa_Site = false;
                    Spara_Site = true;
                    break;
                case "BOTH":
                    Pa_Site = true;
                    Spara_Site = true;
                    break;
            }
        }
        #endregion

        public Dictionary<string, string> GetAtfConfig(string configXmlPath)
        {
            Dictionary<string, string> configList = new Dictionary<string, string>();

            if (!File.Exists(configXmlPath)) return configList;

            XmlDocument clothoConfig = new XmlDocument();
            clothoConfig.Load(configXmlPath);

            XmlNodeList clothoConfigNodes = clothoConfig.SelectNodes("/ATFConfiguration/ToolSection/ConfigItem");

            if (clothoConfigNodes == null) return configList;

            foreach (XmlNode xn in clothoConfigNodes)
            {
                XmlAttribute nameValue = xn.Attributes["value"];
                string v1 = String.Empty;
                if (nameValue != null)
                {
                    v1 = nameValue.Value;
                }
                configList.Add(xn.Attributes["name"].Value, v1);
            }

            clothoConfigNodes = clothoConfig.SelectNodes("/ATFConfiguration/SystemSection/ConfigItem");

            if (clothoConfigNodes == null) return configList;

            foreach (XmlNode xn in clothoConfigNodes)
            {
                XmlAttribute nameValue = xn.Attributes["value"];
                string v1 = String.Empty;
                if (nameValue != null)
                {
                    v1 = nameValue.Value;
                }
                configList.Add(xn.Attributes["name"].Value, v1);
            }

            return configList;
        }

    }

    public class TestPlanBase
    {
        protected ClothoConfigurationDataObject m_doClotho1;
        protected TestPlanStateModel m_modelTpState;

        public virtual string DoATFInit(string args)
        {
            LoggingManager.Instance.SetService(ATFLogControl.Instance);
            m_doClotho1 = new ClothoConfigurationDataObject();
            m_doClotho1.Initialize();

            return String.Empty;
        }

        protected LoggingManager Log
        {
            get { return LoggingManager.Instance; }
        }

        protected PromptManager MessageBox
        {
            get { return PromptManager.Instance; }
        }
    }
}
