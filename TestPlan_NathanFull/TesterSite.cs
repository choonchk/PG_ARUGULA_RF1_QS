using System;
using System.Collections.Generic;
using EqLib;
using MPAD_TestTimer;
using TestPlanCommon.CommonModel;

namespace TestPlan_NathanFull.Tester
{
    public class SeoulRf1Rf2Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site); 
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }


        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.J1;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4154_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4139_02.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4143_01.0"));
            return DCpinSmuResourceTempList;
        }
    }

    public class PenangRf2Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "DIO_0";
        }

        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Y2D;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vlna", "NI4143.1"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbiast", "NI4143.2"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcpl", "NI4143.3"));

            return DCpinSmuResourceTempList;
        }
    }

    public class PenangJokerRf2Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            switch (visaAlias)
            {
                case "NI6570":
                    mSiteVisaAlias = "NI6570";
                    break;
            }
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "DIO_0";
        }

        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Y2D;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4143.1"));

            return DCpinSmuResourceTempList;
        }
    }

    public class PenangRf1Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }

        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Y2;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4143.1"));
            return DCpinSmuResourceTempList;
        }
    }

    public class SjHls2Rf2Tester1 : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            //What does the handler name have to do with the HSDIO?
            //What is the thinking behind this?
            //return "NI6570_01";
            return GlobalVariables.HSDIOAlias; 
        }

        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            //return EqSwitchMatrix.Rev.Y2DPN;
            return GlobalVariables.SwitchMatrixBox;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4143_01.1"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vlna", "NI4143_01.1"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbiast", "NI4143_01.2"));
            //DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcpl", "NI4143_01.3"));

            return DCpinSmuResourceTempList;
        }
    }

    public class SeoulRf2Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }


        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Y2DPN;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4143_01.1"));
            return DCpinSmuResourceTempList;
        }
    }

    public class SeoulRf1Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }


        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Y2;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc2", "NI4139_02.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4139_03.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4139_04.0"));
            return DCpinSmuResourceTempList;
        }
    }

    public class SjNightHawkRf1Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            switch (visaAlias)
            {
                case "NI6570_01":
                    mSiteVisaAlias = "NI6570_01";
                    break;
            }
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }


        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.MD_RF1;
            //return EqSwitchMatrix.Rev.Y2DNightHawk;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc2", "NI4139_02.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4139_03.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4139_04.0"));
            return DCpinSmuResourceTempList;
        }
    }

    public class PgNathanRf1Tester : ITesterSite
    {
        public string GetVisaAlias(string visaAlias, byte site)
        {
            string mSiteVisaAlias = String.Format("{0}_{1}", visaAlias, site);
            return mSiteVisaAlias;
        }

        public string GetHandlerName()
        {
            return "MANUAL";
        }


        public EqSwitchMatrix.Rev GetSwitchMatrixRevision()
        {
            return EqSwitchMatrix.Rev.Modular_RF1_QUADSITE;
        }

        public List<KeyValuePair<string, string>> GetSmuSetting()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc2", "NI4139_02.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4139_03.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4139_04.0"));
            return DCpinSmuResourceTempList;
        }
    }


}

