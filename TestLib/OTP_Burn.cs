using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EqLib;
using Avago.ATF.StandardLibrary;
using WSD.Utility.OtpModule;
using System.Threading;



namespace TestLib
{
    public class OtpBurnProcedure
    {
        private int eFuseLockByteNum;
        private string efuseCtlReg_hex;
        private Dictionary<string, int> readReg_to_efuseByteNum = new Dictionary<string, int>();    //  < register for read, efuse byte number for burn >
        private Dictionary<int, string> efuseByteNum_to_readReg = new Dictionary<int, string>();    //  < efuse byte number for burn,  register for read >

        public Dictionary<string, string> OTP_Registers = new Dictionary<string, string>();//***************************************************************************************************************

        public void DefineControls(string efuseCtlReg_hex, int eFuseLockByteNum)
        {
            this.efuseCtlReg_hex = efuseCtlReg_hex;
            this.eFuseLockByteNum = eFuseLockByteNum;
        }

        public void DefineEfuseByteNums(string readReg_hex, int efuseByteNum)
        {
            this.readReg_to_efuseByteNum[readReg_hex] = efuseByteNum;
            this.efuseByteNum_to_readReg[efuseByteNum] = readReg_hex;
        }

        public void Burn(byte site, string readReg_hex, string data_hex)
        {
            Burn(site, readReg_to_efuseByteNum[readReg_hex], data_hex, eFuseLockByteNum, efuseCtlReg_hex);
        }

        private void Burn(byte site, int efuseDataByteNum, string data_hex, int eFuseLockByteNum, string efuseCtlReg_hex, bool invertData = false)
        {
            try
            {
                int data_dec = Convert.ToInt32(data_hex, 16);

                if (data_dec > 255)
                {
                    MessageBox.Show("Error: Cannot burn decimal values greater than 255", "BurnOTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // burn the data
                for (int bit = 0; bit < 8; bit++)
                {
                    int bitVal = (int)Math.Pow(2, bit);

                    if ((bitVal & data_dec) == (invertData ? 0 : bitVal))
                    {
                        for (int programMode = 1; programMode >= 0; programMode--)
                        {
                            int burnDataDec = (programMode << 7) + (efuseDataByteNum << 3) + bit;
                            Eq.Site[site].HSDIO.RegWrite(efuseCtlReg_hex, burnDataDec.ToString("X"), false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "OTP Burn Error");
            }

        }
        
        public bool Burn(byte site, out int mfg_id, out int module_id, out int rev_id)
        {
            //try
            //{

                #region HSDIO Sendvector

                int MOD_ID_Bits = 0;
                int MFG_ID_Bits = 0;
                int REV_ID_Bits = 0;

                String ReadData_MOD_ID_LSB = "999999";
                String ReadData_MOD_ID_MSB = "999999";
                String ReadData_MFG_ID_LSB = "999999";
                String ReadData_MFG_ID_MSB = "999999";
                String ReadData_REV_ID = "999999";
                String ReadData_LOCK = "999999";
                String mfg_id_str = "999999";
                string vectorName = "";

                mfg_id = 999999;
                module_id = 999999;
                rev_id = 999999;

                ReadData_MOD_ID_LSB = ReadOTPRegister(site, "MOD_ID_LSB_READ");

                ReadData_MOD_ID_MSB = ReadOTPRegister(site, "MOD_ID_MSB_READ");

                ReadData_MFG_ID_LSB = ReadOTPRegister(site, "MFG_ID_LSB_READ");

                ReadData_MFG_ID_MSB = ReadOTPRegister(site, "MFG_ID_MSB_READ");

                ReadData_REV_ID = ReadOTPRegister(site, "REV_ID_READ");

                ReadData_LOCK = ReadOTPRegister(site, "LOCK_BIT_READ");

                if (ReadData_MOD_ID_LSB != "" || ReadData_MOD_ID_MSB != "" || ReadData_MFG_ID_LSB != "" || ReadData_MFG_ID_MSB != "" || ReadData_MFG_ID_LSB != "") // if already burned
                {
                    return false; // return if OTP already burned
                }

                //PaTest.SmuResources["Vbatt"].ForceVoltage(5.5, 1);

                /*****************************************************************************************************************************************************************************************************/
                /* New serial id query that returns _otp_unit_id */

                // Two approach of reading mfg_id
                // (a) If already burnt in EEPROM, read from there by h/w API. 
                // (b) Not available in EEPROM, need scan into Clotho MainUI and read from Cross Domain Cache. 
                // For our cases, follow Option (b) 

                // Read mfg_id from Cross Domain Cache 
                // NOTE at this moment, MFG_ID is string for Clotho; Need parse to int so Clotho can write into result file
                // In the future Clotho will upgrade to directly handle MFG_ID as int

                string err = "";

                // get and convert MFG_ID from Clotho to an int
                if (mfg_id_str != "0")  // "0" means Mod_ID was not specified in Clotho and we set static mfg_id_str to default "0"
                    mfg_id_str = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_MFG_ID, "");

                int MfgID = 999999;
                try
                {
                    MfgID = Int32.Parse(mfg_id_str);

                }
                catch (Exception ex2)
                {
                    MessageBox.Show("Invalid MFG_ID was entered. 0 will be used as default", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mfg_id_str = "0"; // will retain this value for future parts so message doesnt keep poping up
                    MfgID = 0;
                }


                if (MfgID > (uint)Math.Pow(2.0, MFG_ID_Bits))
                {
                    MessageBox.Show("Requested MFG_ID: " + MfgID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Now that valid mfg_id is ready, get Module ID
                int ModuleID = 0;
                try
                {
                    Tuple<bool, int, string> unique_id_ret = SerialProvider.GetNextModuleID(MfgID);

                    if (!unique_id_ret.Item1)
                    {
                        err = unique_id_ret.Item3;
                        MessageBox.Show("Module ID Server is not responding. Fix or Disable Module ID burn in TCF. \n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    else
                    {
                        ModuleID = unique_id_ret.Item2;
                    }
                }

                catch
                { // ID Server may be down
                    MessageBox.Show("Exit Test Plan Run, Module ID Server is not responding. Disable Module ID burn in TCF\n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }

                /*****************************************************************************************************************************************************************************************************/


                /* Program Module (serial) ID */
                //if (ModuleID > 16382)
                if (ModuleID > (uint)Math.Pow(2.0, MOD_ID_Bits))
                {
                    MessageBox.Show("Issued Module ID: " + Convert.ToString(ModuleID) + " is larger than OTP register capacity", "Exit Test Plan Run, Module_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }


                char[] Unit_id = (Convert.ToString(Convert.ToInt32(ModuleID), 2).PadLeft(MOD_ID_Bits, '0')).ToCharArray();  //convert to Binary string 
                System.Array.Reverse(Unit_id);

                foreach (char bit in Unit_id)
                {
                    if (Unit_id[Convert.ToInt16(bit)] == '1')
                    {
                        Eq.Site[site].HSDIO.SendVector(GetVectorName("MOD_ID_BIT" + bit.ToString()));
                    }
                }


                /* Program MFG ID */

                char[] mfg_id_char = (Convert.ToString(Convert.ToInt32(MfgID), 2).PadLeft(MFG_ID_Bits, '0')).ToCharArray();  //convert to Binary string 
                System.Array.Reverse(mfg_id_char);

                foreach (char bit in mfg_id_char)
                {
                    if (mfg_id_char[Convert.ToInt16(bit)] == '1')
                    {
                        Eq.Site[site].HSDIO.SendVector(GetVectorName("MFG_ID_BIT" + bit.ToString()));
                    }
                }

                /* Program Rev ID */

                string rev_ID = "";

//                rev_ID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "");
                if (rev_ID != "")
                {
                    if (OTP_Registers.TryGetValue("rev_ID", out vectorName))
                        Eq.Site[site].HSDIO.SendVector(vectorName);
                    else
                    {
                        MessageBox.Show("Unknown REV_ID: " + rev_ID + " Check Package Tag", "Exit Test Plan Run, Unknown REV_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    char[] rev_id_char = (Convert.ToString(Convert.ToInt32(rev_ID), 2).PadLeft(REV_ID_Bits, '0')).ToCharArray();  //convert to Binary string 
                    System.Array.Reverse(rev_id_char);

                    foreach (char bit in rev_id_char)
                    {
                        if (rev_id_char[Convert.ToInt16(bit)] == '1')
                        {
                            Eq.Site[site].HSDIO.SendVector(GetVectorName("REV_ID_BIT" + bit.ToString()));
                        }
                    }

                }


                // lock OTP
                Eq.Site[site].HSDIO.SendVector("RegE3_Bit0");

                //PaTest.SmuResources["Vbatt"].ForceVoltage(3.8, 0.2);

                // reset VIO and verify burn
                Eq.Site[site].HSDIO.SendVector("VIOOFF");
              //  Thread.Sleep(5);

                Eq.Site[site].HSDIO.SendVector("VIOON");
               // Thread.Sleep(5);


                //Read_Unit_Number = 0;
                ReadData_MOD_ID_LSB = ReadOTPRegister(site, "MOD_ID_LSB_READ");

                ReadData_MOD_ID_MSB = ReadOTPRegister(site, "MOD_ID_MSB_READ");

                ReadData_MFG_ID_LSB = ReadOTPRegister(site, "MFG_ID_LSB_READ");

                ReadData_MFG_ID_MSB = ReadOTPRegister(site, "MFG_ID_MSB_READ");

                ReadData_REV_ID = ReadOTPRegister(site, "REV_ID_READ");

                ReadData_LOCK = ReadOTPRegister(site, "LOCK_BIT_READ");

                //verify burn successful  and return pass/fail
                mfg_id = (Convert.ToInt32(ReadData_MFG_ID_MSB, 16) * 256) + Convert.ToInt32(ReadData_MFG_ID_LSB, 16);
                module_id = (Convert.ToInt32(ReadData_MOD_ID_MSB, 16) * 256) + Convert.ToInt32(ReadData_MOD_ID_LSB, 16);
                rev_id = Convert.ToInt32(ReadData_REV_ID, 16);

                if ((ModuleID != module_id) || (MfgID != mfg_id) || (Convert.ToInt32(rev_ID) != rev_id))
                    return false;
                else
                    return true;


            //}


            //catch (Exception e)
            //{
            //    MessageBox.Show(e.ToString(), "OTP Burn Error");
            //    return false;
            //}
                #endregion HSDIO Sendvector
        }


        private string GetVectorName(string tag)
        {
            string vectorName = "";
            if (!OTP_Registers.TryGetValue(tag, out vectorName))
                MessageBox.Show("Requested Register information for tag " + tag + " was not found in OTP_Registers_Part_Specific dictionary", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return vectorName;
        }

        private string ReadOTPRegister(int site ,string register)
        {
            string vectorName = "";
            if (!OTP_Registers.TryGetValue(register, out vectorName))
                if (vectorName == "NA" || vectorName == "") // register was not defined
                    return "";
            else
                    return Eq.Site[site].HSDIO.RegRead(vectorName);
            return "";
        }



        public void BurnLockBit(byte site)
        {
            BurnLockBit(site, eFuseLockByteNum, efuseCtlReg_hex);
        }

        private void BurnLockBit(byte site, int eFuseLockByteNum, string efuseCtlReg_hex)
        {
            try
            {
                for (int programMode = 1; programMode >= 0; programMode--)
                {
                    int bit = 0;
                    int burnDataDec = (programMode << 7) + (eFuseLockByteNum << 3) + bit;
                    Eq.Site[0].HSDIO.RegWrite(efuseCtlReg_hex, burnDataDec.ToString("X"), false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "OTP Burn Lock Bit Error");
            }

        }

        public string ReadLockBit(byte site)
        {
            return Eq.Site[0].HSDIO.RegRead(efuseByteNum_to_readReg[eFuseLockByteNum]);
        }
    }

    public class MfgLotNum
    {
        private bool enable;
        public int decimalVal;
        public string MfgLotNumber;
        public Dictionary<string, string> regData;    //  [readReg_hex, data_hex]
        public int NumOfByte = 2;
        private string mfgLotNum_LSBreg_hex;
        private string mfgLotNum_MSBreg_hex;
        private string mfgLotNum_ExtraBitReg_hex;

        public void DefineRegisters(string mfgLotNum_ExtraBitReg_hex, string mfgLotNum_MsbReg_hex, string mfgLotNum_LsbReg_hex)
        {
            this.mfgLotNum_LSBreg_hex = mfgLotNum_LsbReg_hex;
            this.mfgLotNum_MSBreg_hex = mfgLotNum_MsbReg_hex;
            this.mfgLotNum_ExtraBitReg_hex = mfgLotNum_ExtraBitReg_hex;
        }

        public void PromptForEntry(bool enable)
        {
            if (!(this.enable = enable)) return;

            int maxMfgLotNumber_dec = (int)Math.Pow(2, NumOfByte * 8) - 1;
            int requiredDigits = 0;
            for (int i = maxMfgLotNumber_dec; i > 0; i = i / 10)
            {
                requiredDigits++;
            }
            while (true)
            {    

                MfgLotNumber = Microsoft.VisualBasic.Interaction.InputBox("Please Scan Mfg-Lot-Number\n    Then press OK\n\nMust be decimal.\nMust be " + requiredDigits + " digits.\nMinimum value is 00001\nMaximum value is" + maxMfgLotNumber_dec, "Manufacturing Lot Number", "00000", 200, 200);
               // ModuleSerialFile.MfgLogNum = MfgLotNumber;  need to fix KH
                try
                {
                    int MfgLotNumber_dec = Convert.ToInt32(MfgLotNumber);

                    regData = new Dictionary<string, string>();

                    if (MfgLotNumber_dec >= 1 & MfgLotNumber_dec <= maxMfgLotNumber_dec & MfgLotNumber.Length == requiredDigits)
                    {
                        decimalVal = MfgLotNumber_dec;

                        regData[mfgLotNum_LSBreg_hex] = ((MfgLotNumber_dec >> (8 * 0)) & 255).ToString("X");
                        regData[mfgLotNum_MSBreg_hex] = ((MfgLotNumber_dec >> (8 * 1)) & 255).ToString("X");
                        if (!string.IsNullOrEmpty(mfgLotNum_ExtraBitReg_hex)) regData[mfgLotNum_ExtraBitReg_hex] = ((MfgLotNumber_dec >> (8 * 2)) & 1).ToString("X");

                        return;
                    }
                }
                catch
                {
                }

                MessageBox.Show(MfgLotNumber + " is not a valid entry", "Manufacturing Lot Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Burn(byte site, OtpBurnProcedure myOTP)
        {
            myOTP.Burn(site, mfgLotNum_LSBreg_hex, regData[mfgLotNum_LSBreg_hex]);
            myOTP.Burn(site, mfgLotNum_MSBreg_hex, regData[mfgLotNum_MSBreg_hex]);
            if (!string.IsNullOrEmpty(mfgLotNum_ExtraBitReg_hex)) myOTP.Burn(site, mfgLotNum_ExtraBitReg_hex, regData[mfgLotNum_ExtraBitReg_hex]);
        }

        public int Read(byte site)
        {
            try
            {
                string readDataLSB = Eq.Site[site].HSDIO.RegRead(mfgLotNum_LSBreg_hex);
                int readDataLSB_dec = (Convert.ToInt32(readDataLSB, 16) & 255) << (8 * 0);

                string readDataMSB = Eq.Site[site].HSDIO.RegRead(mfgLotNum_MSBreg_hex);
                int readDataMSB_dec = (Convert.ToInt32(readDataMSB, 16) & 255) << (8 * 1);

                string readDataExtraBit = Eq.Site[site].HSDIO.RegRead(mfgLotNum_ExtraBitReg_hex);
                int readDataExtraBit_dec = (Convert.ToInt32(readDataExtraBit, 16) & 1) << (8 * 2);

                return readDataLSB_dec + readDataMSB_dec + readDataExtraBit_dec;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
    public class ModuleSerialNum
    {
        private bool enable;
        public int decimalVal;
        public Dictionary<string, string> regData;    //  [readReg_hex, data_hex]

        private string moduleSerialNum_LSBreg_hex;
        private string moduleSerialNum_MSBreg_hex;
        private string moduleSerialNum_ExtraBitReg_hex;

        public void DefineRegisters(string moduleSerialNum_MsbReg_hex, string moduleSerialNum_LsbReg_hex, string moduleSerialNum_ExtraBitReg_hex = "")
        {
            this.moduleSerialNum_LSBreg_hex = moduleSerialNum_LsbReg_hex;
            this.moduleSerialNum_MSBreg_hex = moduleSerialNum_MsbReg_hex;
            this.moduleSerialNum_ExtraBitReg_hex = moduleSerialNum_ExtraBitReg_hex;
        }

        public void ModuleSNtoHex(int moduleSerialNum_dec)
        {
            //if (!(this.enable = enable)) return;

            int maxmoduleSerialNum_dec = (int)Math.Pow(2, 17) - 1;
            int requiredDigits = 6;

            while (true)
            {
                //string moduleSerialNum=""; //= Microsoft.VisualBasic.Interaction.InputBox("Please Scan Mfg-Lot-Number\n    Then press OK\n\nMust be decimal.\nMust be " + requiredDigits + " digits.\nMinimum value is 000001\nMaximum value is" + maxMfgLotNumber_dec, "Manufacturing Lot Number", "000000", 200, 200);

                try
                {
                    string moduleSerialNum_hex = moduleSerialNum_dec.ToString("X");

                    regData = new Dictionary<string, string>();

                    if (moduleSerialNum_dec >= 1 & moduleSerialNum_dec <= maxmoduleSerialNum_dec)
                    {
                        decimalVal = moduleSerialNum_dec;

                        regData[moduleSerialNum_LSBreg_hex] = ((moduleSerialNum_dec >> (8 * 0)) & 255).ToString("X");
                        regData[moduleSerialNum_MSBreg_hex] = ((moduleSerialNum_dec >> (8 * 1)) & 255).ToString("X");
                        //regData[moduleSerialNum_ExtraBitReg_hex] = ((moduleSerialNum_dec >> (8 * 2)) & 1).ToString("X");

                        return;
                    }
                }
                catch
                {
                }

                MessageBox.Show(moduleSerialNum_dec + " is not a valid entry", "Manufacturing Lot Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Burn(byte site, OtpBurnProcedure myOTP)
        {
            myOTP.Burn(site, moduleSerialNum_LSBreg_hex, regData[moduleSerialNum_LSBreg_hex]);
            myOTP.Burn(site, moduleSerialNum_MSBreg_hex, regData[moduleSerialNum_MSBreg_hex]);
            if (moduleSerialNum_ExtraBitReg_hex!="") myOTP.Burn(site, moduleSerialNum_ExtraBitReg_hex, regData[moduleSerialNum_ExtraBitReg_hex]);
        }

        public int Read(byte site)
        {
            try
            {

                string readDataLSB = Eq.Site[site].HSDIO.RegRead(moduleSerialNum_LSBreg_hex);
                int readDataLSB_dec = (Convert.ToInt32(readDataLSB, 16) & 255) << (8 * 0);

                string readDataMSB = Eq.Site[site].HSDIO.RegRead(moduleSerialNum_MSBreg_hex);
                int readDataMSB_dec = (Convert.ToInt32(readDataMSB, 16) & 255) << (8 * 1);

                //string readDataExtraBit = Eq.Site[site].HSDIO.RegRead(moduleSerialNum_ExtraBitReg_hex);
                //int readDataExtraBit_dec = (Convert.ToInt32(readDataExtraBit, 16) & 1) << (8 * 2);
                //return readDataLSB_dec + readDataMSB_dec + readDataExtraBit_dec;
                return readDataLSB_dec + readDataMSB_dec ;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }

}
