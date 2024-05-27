using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;


namespace GuCal
{
    public class AllProductStatusFile : List<SingleProductStatus>
    {
        private string statusFileFullPath;
        private XmlSerializer myXmlSerializer;

        public static AllProductStatusFile ReadFromFileOrNew(string statusFileFullPath)
        {
            AllProductStatusFile myProductStatus = null;
            XmlSerializer myXmlSerializer = new XmlSerializer(typeof(AllProductStatusFile));

            if (File.Exists(statusFileFullPath))
            {
                using (StreamReader sr = new StreamReader(statusFileFullPath))
                {
                    myProductStatus = (AllProductStatusFile)myXmlSerializer.Deserialize(sr);
                }
            }
            else
            {
                myProductStatus = new AllProductStatusFile();
            }

            myProductStatus.statusFileFullPath = statusFileFullPath;
            myProductStatus.myXmlSerializer = myXmlSerializer;

            return myProductStatus;
        }

        public SingleProductStatus GetSingleProductStatus(string productId)
        {
            SingleProductStatus mySingleProductStatus = this.FirstOrDefault(x => x.productId == productId);

            if (mySingleProductStatus == null)
            {
                mySingleProductStatus = new SingleProductStatus(productId);
                this.Add(mySingleProductStatus);
            }

            return mySingleProductStatus;
        }


        public void SaveToFile()
        {
            if (!Directory.Exists(Path.GetDirectoryName(statusFileFullPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(statusFileFullPath));
            }

            this.Sort((x, y) => x.productId.CompareTo(y.productId));

            using (StreamWriter sw = new StreamWriter(statusFileFullPath))
            {
                myXmlSerializer.Serialize(sw, this);
            }
        }
    }

    public class SingleProductStatus
    {
        [XmlAttribute("ID")]
        public string productId;
        [XmlElement("MaxAllowedHoursBetweenVerify")]
        public double maxAllowedHoursBetweenVerify = 30;
        [XmlElement("MaxAllowedHoursBetweenCorr")]
        public double maxAllowedHoursBetweenCorr = 365 * 24;
        [XmlElement("MaxAllowedHoursBetweenIcc")]
        public double maxAllowedHoursBetweenIcc = 365 * 240;
        [XmlElement("MaxAllowedHoursBetweenDCCheck")]
        public double maxAllowedHoursBetweenDCCheck = 365 * 24;

        [XmlElement("MaxAllowedCorrFailures")]
        public int maxCorrelationFailures = 3;
        [XmlElement("MaxAllowedVerifyFailures")]
        public int maxVerificationFailures = 3;

        [XmlElement("SiteStatus")]
        public List<SingleSiteStatus> allSitesStatus;

        public SingleProductStatus()
        {
        }

        public SingleProductStatus(string ProductID)
        {
            this.productId = ProductID;

        }

        public SingleSiteStatus this[int site]
        {
            get
            {
                AddSiteIfSiteNotExist(site);
                 return allSitesStatus[site];
            }
            set
            {
                AddSiteIfSiteNotExist(site);
                allSitesStatus[site] = value;
            }
        }

        private void AddSiteIfSiteNotExist(int site)
        {
            if (allSitesStatus == null)
            {
                allSitesStatus = new List<SingleSiteStatus>();
            }

            while (allSitesStatus.Count <= site)
            {
                allSitesStatus.Add(new SingleSiteStatus(allSitesStatus.Count));
            }
        }

        public bool IsVerifyExpired(params int[] sites)
        {
            bool expired = false;

            foreach (int site in sites)
            {
                expired |= DateTime.Now > this[site].dateOfLastVerifyAttempt.AddHours(maxAllowedHoursBetweenVerify);
            }

            return expired;
        }

        public bool IsCorrExpired(params int[] sites)
        {
            bool expired = false;

            foreach (int site in sites)
            {
                expired |= DateTime.Now > this[site].dateOfLastCorrAttempt.AddHours(maxAllowedHoursBetweenCorr);
            }

            return expired;
        }

        public bool IsIccExpired(params int[] sites)
        {
            bool expired = false;

            foreach (int site in sites)
            {
                expired |= DateTime.Now > this[site].dateOfLastIccAttempt.AddHours(maxAllowedHoursBetweenIcc);
            }

            return expired;
        }

        public bool IsDCVerifyExpired(params int[] sites)
        {
            bool expired = false;

            foreach (int site in sites)
            {
                expired |= DateTime.Now > this[site].dateOfLastDCCheckAttempt.AddHours(maxAllowedHoursBetweenDCCheck);
            }

            return expired;
        }

        public bool IccIsOptional(int site)
        {
            if (GU.testertype.Contains("FBAR") || GU.testertype.Contains("SPARA"))
            {
                return false;
            }
            else
            {
                return
                    this[site].iccCalPassed
                    && !this.IsIccExpired(site)
                    && this[site].correlationFailures < maxCorrelationFailures
                    && this[site].verificationFailures < maxVerificationFailures + maxCorrelationFailures;
            }
        }

        public bool DCVerifyIsOptional(int site)
        {
            return
                this[site].dcCheckPassed
                && !this.IsDCVerifyExpired(site);

        }

        public bool CorrIsOptional(int site)
        {
            if (GU.testertype.Contains("FBAR") || GU.testertype.Contains("SPARA"))
            {
                return
                    this[site].correlationFactorsPassed
                    && !this.IsCorrExpired(site)
                    && this[site].verificationFailures < maxVerificationFailures;
            }
            else
            {
                return
                this[site].correlationFactorsPassed
                && !this.IsCorrExpired(site)
                && IccIsOptional(site)
                && this[site].verificationFailures < maxVerificationFailures;
            }
        }

        public bool VerifyIsOptional(int site)
        {
            return
                this[site].verificationPassed
                && !this.IsVerifyExpired(site)
                && CorrIsOptional(site);
         
        }
        public string IsCFFileVerify(int site)
        {
            if (this[site].cFFileCheck == null) return "NONE";
            else return this[site].cFFileCheck;
        }
        public class SingleSiteStatus
        {
            [XmlAttribute("Site")]
            public int site;
            [XmlElement("DateOfLastVerifyAttempt")]
            public DateTime dateOfLastVerifyAttempt = new DateTime(1970, 1, 1);
            [XmlElement("DateOfLastCorrAttempt")]
            public DateTime dateOfLastCorrAttempt = new DateTime(1970, 1, 1);
            [XmlElement("DateOfLastIccAttempt")]
            public DateTime dateOfLastIccAttempt = new DateTime(1970, 1, 1);
            [XmlElement("DateOfLastDCCheckAttempt")]
            public DateTime dateOfLastDCCheckAttempt = new DateTime(1970, 1, 1);

            [XmlElement("CFFileCheck")]
            public string cFFileCheck
            {
                get
                {
                    return _cFFileCheck;
                }
                set
                {
                    _cFFileCheck = value;
                }
            }

            private string _cFFileCheck;
            private bool _dcCheckPassed;
            private bool _iccCalPassed;
            private bool _correlationFactorsPassed;
            private bool _verificationPassed;

            [XmlElement("DCCheckPassed")]
            public bool dcCheckPassed
            {
                get
                {
                    return _dcCheckPassed;
                }
                set
                {
                    _dcCheckPassed = value;

                    if (value)
                    {
                        dcCheckFailures = 0;
                    }
                    else
                    {
                        dcCheckFailures++;
                    }
                }
            }

            [XmlElement("IccCalPassed")]
            public bool iccCalPassed
            {
                get
                {
                    return _iccCalPassed;
                }
                set
                {
                    _iccCalPassed = value;

                    if (value)
                    {
                        iccCalFailures = 0;
                    }
                    else
                    {
                        iccCalFailures++;
                    }
                }
            }
            [XmlElement("CorrelationFactorsPassed")]
            public bool correlationFactorsPassed
            {
                get
                {
                    return _correlationFactorsPassed;
                }
                set
                {
                    _correlationFactorsPassed = value;

                    if (value)
                    {
                        correlationFailures = 0;
                    }
                    else
                    {
                        correlationFailures++;
                    }
                }
            }
            [XmlElement("VerificationPassed")]
            public bool verificationPassed
            {
                get
                {
                    return _verificationPassed;
                }
                set
                {
                    _verificationPassed = value;

                    if (value)
                    {
                        verificationFailures = 0;
                    }
                    else
                    {
                        verificationFailures++;
                    }
                }
            }

            [XmlElement("DCCheckFailures")]
            public int dcCheckFailures;
            [XmlElement("IccCalFailures")]
            public int iccCalFailures;
            [XmlElement("VerificationFailures")]
            public int verificationFailures;
            [XmlElement("CorrelationFailures")]
            public int correlationFailures;

            public SingleSiteStatus()
            {
            }

            public SingleSiteStatus(int Site)
            {
                this.site = Site;
            }
        }
    }


    //ChoonChin (20200115) - For new 28 Ohm subcal flow
    public class AllProductSubcalStatusFile : List<SingleProductSubcalStatus>
    {
        private string statusSubFileFullPath;
        private XmlSerializer mySubXmlSerializer;

        public static AllProductSubcalStatusFile ReadFromFileOrNew(string statusSubFileFullPath)
        {
            AllProductSubcalStatusFile myProductSubStatus = null;
            XmlSerializer mySubXmlSerializer = new XmlSerializer(typeof(AllProductSubcalStatusFile));

            if (File.Exists(statusSubFileFullPath))
            {
                using (StreamReader sr = new StreamReader(statusSubFileFullPath))
                {
                    myProductSubStatus = (AllProductSubcalStatusFile)mySubXmlSerializer.Deserialize(sr);
                }
            }
            else
            {
                myProductSubStatus = new AllProductSubcalStatusFile();
            }

            myProductSubStatus.statusSubFileFullPath = statusSubFileFullPath;
            myProductSubStatus.mySubXmlSerializer = mySubXmlSerializer;

            return myProductSubStatus;
        }

        public SingleProductSubcalStatus GetSingleSubProductStatus(string productId)
        {
            SingleProductSubcalStatus mySingleSubProductStatus = this.FirstOrDefault(x => x.productId == productId);

            if (mySingleSubProductStatus == null)
            {
                mySingleSubProductStatus = new SingleProductSubcalStatus(productId);
                this.Add(mySingleSubProductStatus);
            }

            return mySingleSubProductStatus;
        }

        public void SaveToFile()
        {
            if (!Directory.Exists(Path.GetDirectoryName(statusSubFileFullPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(statusSubFileFullPath));
            }

            this.Sort((x, y) => x.productId.CompareTo(y.productId));

            using (StreamWriter sw = new StreamWriter(statusSubFileFullPath))
            {
                mySubXmlSerializer.Serialize(sw, this);
            }
        }
    }
    public class SingleProductSubcalStatus
    {
        [XmlAttribute("ID")]
        public string productId;
        [XmlElement("MaxAllowedHoursBetweenVerify")]
        public double maxAllowedHoursBetweenVerify = 30;

        [XmlElement("MaxAllowedVerifyFailures")]
        public int maxVerificationFailures = 3;

        [XmlElement("SiteStatus")]
        public List<SingleSiteStatus> allSitesStatus;

        public SingleProductSubcalStatus()
        {
        }

        public SingleProductSubcalStatus(string ProductID)
        {
            this.productId = ProductID;
        }

        public SingleSiteStatus this[int site]
        {
            get
            {
                AddSiteIfSiteNotExist(site);
                return allSitesStatus[site];
            }
            set
            {
                AddSiteIfSiteNotExist(site);
                allSitesStatus[site] = value;
            }
        }

        private void AddSiteIfSiteNotExist(int site)
        {
            if (allSitesStatus == null)
            {
                allSitesStatus = new List<SingleSiteStatus>();
            }

            while (allSitesStatus.Count <= site)
            {
                allSitesStatus.Add(new SingleSiteStatus(allSitesStatus.Count));
            }
        }

        public bool IsVerifyExpired(params int[] sites)
        {
            bool expired = false;

            foreach (int site in sites)
            {
                expired |= DateTime.Now > this[site].dateOfLastVerifyAttempt.AddHours(maxAllowedHoursBetweenVerify);
            }

            return expired;
        }

        public bool VerifyIsOptional(int site)
        {
            return
                this[site].verificationPassed
                && !this.IsVerifyExpired(site);
        }

        public bool VerifyIsPass(int site)
        {
            return
                this[site].verificationPassed;
        }

        public class SingleSiteStatus
        {
            [XmlAttribute("Site")]
            public int site;
            [XmlElement("DateOfLastVerifyAttempt")]
            public DateTime dateOfLastVerifyAttempt = new DateTime(1970, 1, 1);

            private bool _verificationPassed;

            [XmlElement("VerificationPassed")]
            public bool verificationPassed
            {
                get
                {
                    return _verificationPassed;
                }
                set
                {
                    _verificationPassed = value;

                    if (value)
                    {
                        verificationFailures = 0;
                    }
                    else
                    {
                        verificationFailures++;
                    }
                }
            }

            [XmlElement("VerificationFailures")]
            public int verificationFailures;

            public SingleSiteStatus()
            {
            }

            public SingleSiteStatus(int Site)
            {
                this.site = Site;
            }

            //Hardware IDs

            [XmlElement("Chassis")]
            public string ChassisSerial = "NA";

            [XmlElement("Vna")]
            public string VnaHostID = "NA";

            [XmlElement("SwitchMatrix")]
            public string SwitchMatrixID = "NA";

            [XmlElement("Loadboard")]
            public string LoadboardID = "NA";

            [XmlElement("Contactor")]
            public string ContactorID = "NA";

            [XmlElement("VnaTemperature")]
            public double VnaTemperature = 999;
        }
    }

}
