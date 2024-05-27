using System.Collections.Generic;
using CalLib;
using EqLib;
using GuCal;
using System;

namespace TestPlanCommon.CommonModel
{
    public class GuCalibrationModel
    {
        public GU.UI UItype = GU.UI.FullAutoCal;

        // CCT : Entry Point.
        public bool GuCalibration(IATFTest clothoInterface)
        {
            //string destCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
            //int mustIccGuCalCached = ATFCrossDomainWrapper.GetIntFromCache(PublishTags.PUBTAG_MUST_IccGuCal, -1);
            //string isEngineeringMode = m_tcfReader.TCF_Setting["GU_EngineeringMode"];

            //return GuCalibration(clothoInterface, destCFpath, mustIccGuCalCached, isEngineeringMode);
            return false;
        }

        public void SetTesterType(string testerType)
        {
            GU.testertype = testerType;
        }

        /// <summary>
        /// Joker variant.
        /// </summary>
        public bool[] GuCalibration(IATFTest clothoInterface,
            string destCFpath, int mustIccGuCalCached, string isEngineeringMode,
            string ProductTagName)
        {
            bool[] isSuccess = new bool[Eq.NumSites];
            string ProductTagGU = "";

            ProductTagGU = ProductTagName;

            GU.DoInit_afterCustomCode(mustIccGuCalCached, UItype, false, false, ProductTagGU, @"C:\Avago.ATF.Common\Results", Eq.NumSites);

            bool ProductionMode = isEngineeringMode == "FALSE" ? true : false;

            if (UItype == GU.UI.FullAutoCal && GU.GuInitSuccess)
            {
                for (byte siteNo = 0; siteNo < Eq.NumSites; siteNo++ )
                {
                    isSuccess[siteNo] = true;
                    GU.siteNo = siteNo;
                    GU.loggedMessages.Clear();
                    AutoCal myAutoCal = new AutoCal(clothoInterface, false, siteNo); // Always Enable GUCAL Skip Button

                    myAutoCal.guTrayCoord = new Dictionary<int, string>();

                    int guDutIndex = 0;
                    for (int row = 0; row < 5; row++)   // define a bunch of GU tray coordinates, starting below cal substrates, 7 columns wide
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            myAutoCal.guTrayCoord.Add(guDutIndex++, col + "," + row);
                        }
                    }



                    myAutoCal.ShowForm();

                    // Engineering mode. Skip Button bybasses failed GU
                    if (isEngineeringMode != "TRUE")
                    {
                        // Production Mode. Skip Button fails program load
                        isSuccess[siteNo] = !GU.thisProductsGuStatus.IsVerifyExpired(siteNo) && GU.thisProductsGuStatus.VerifyIsOptional(siteNo);
                        // Note: Not &= but set directly.
                        //m_modelTpState.programLoadSuccess = isSuccess;
                    }
                }
            }
            else
            {
                //In the case of no GUREFDATA and GuInit () failed, set GUcalstatus of each sites to Passed. To enable generation of test parameters headers
                for (byte siteNo = 0; siteNo < Eq.NumSites; siteNo++)
                {
                    isSuccess[siteNo] = true;
                }
            }

            return isSuccess;
        }

    }
}