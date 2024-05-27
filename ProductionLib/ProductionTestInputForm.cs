using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Globalization;
using MPAD_TestTimer;

namespace ProductionLib2
{
    public partial class ProductionTestInputForm : Form
    {
        private string T_productTag = "";
        private string T_LotID = "";
        private string T_SubLotID = "";
        private string T_OperatorID = "";
        private string T_HandlerID = "";
        private string T_ContactorID = "";
        private string T_DeviceID = "";
        private string T_LoadBoardID = "";
        private string T_MfgLotID = "";
        private string T_RevID = "";
        private bool bIsEngineeringSample = false;
        private string addSublotID = "";
        private string addDeviceID = "";
        private string webQueryValidation = "";
        private string webServerURL = "";
        private string Clotho_User1 = "";
        public string  Date;
        public string Shift;
        public int PassCharCount = 0;
        public bool EnablePassword = false;

        //Web Service 2.0 - DH

        public string web_lotid = "PTXXXXXXXXXX";
        public string web_targetdevice = "AFEM-XXXX-AP1";
        public string web_MFGlot = "999999";
        public string web_handlerid = "NULL";
        public string web_2DID = "99999999";

        public bool WebserviceByPass = false;

        public string Set_Title
        {
            set
            {
                this.Text = value;
            }
        }
        public string productTag
        {
            get
            {
                return T_productTag;
            }
            set
            {
                T_productTag = value;
            }
        }
        public string LotID
        {
            get
            {
                return T_LotID;
            }
            set
            {
                T_LotID = value;
            }
        }
        public string Data2DID
        {
            get
            {
                return web_2DID;
            }
            set
            {
                web_2DID = value;
            }
        }

        public bool WebByPass
        {
            get
            {
                return WebserviceByPass;
            }
            set
            {
                WebserviceByPass = value;
            }
        }

        public string ContactorID
        {
            get
            {
                return T_ContactorID;
            }
            set
            {
                T_ContactorID = value;
            }
        }
        public string HandlerID
        {
            get
            {
                return T_HandlerID;
            }
            set
            {
                T_HandlerID = value;
            }
        }
        public string OperatorID
        {
            get
            {
                return T_OperatorID;

            }
            set
            {
                T_OperatorID = value;
            }
        }
        public string SublotID
        {
            get
            {
                return T_SubLotID;

            }
            set
            {
                T_SubLotID = value;
            }
        }
        public string LoadBoardID
        {
            get
            {
                return T_LoadBoardID;
            }
            set
            {
                T_LoadBoardID = value;
            }
        }
        public string MfgLotID
        {
            get
            {
                return T_MfgLotID;
            }
            set
            {
                T_MfgLotID = value;
            }
        }
        public string RevID
        {
            get
            {
                return T_RevID;
            }
        }
        public string DeviceID
        {
            get
            {
                return T_DeviceID;
            }
            set
            {
                T_DeviceID = value;
            }
        }

        public ProductionTestInputForm(string addSublotID = "", string addDeviceID = "", string webQueryValidation = "", string webServerURL = "",
            string Clotho_User = "")
        {
            InitializeComponent();
            this.addSublotID = addSublotID;
            this.addDeviceID = addDeviceID;
            this.webQueryValidation = webQueryValidation;
            this.webServerURL = webServerURL;
            this.Clotho_User1 = Clotho_User.ToUpper();
        }

        public ProductionTestInputForm(bool IsEngineeringGUI)
        {
            InitializeComponent();
            bIsEngineeringSample = IsEngineeringGUI;

            if (!bIsEngineeringSample)
            {
                this.Text = "Production Test Information Input";
                lblMfgID.Text = "Mfg Lot ID";
                txtMfgLotID.Enabled = false;
            }
            else
            {
                this.Text = "Engineering Sample Test Information Input";
                lblMfgID.Text = "Rev ID";
            }
        }

        private void FrmDataInput_Load(object sender, EventArgs e)
        {
            DateTime Timenow = DateTime.Now;
            Date = Timenow.ToString("ddMMyy");
            string sttime = Timenow.ToString("HHmmss");
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstWeekDay = DayOfWeek.Monday;
            Calendar calendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;
            int currentWeek = calendar.GetWeekOfYear(Timenow, weekRule, firstWeekDay);
            double lgtime = Convert.ToDouble(sttime);
            if (lgtime > 170000 || lgtime < 70000) Shift = "N";
            else if (lgtime > 70000 || lgtime < 170000) Shift = "M";

            this.txtOperatorID.Select();
            //Lib_Var.ºAdminLevel = false;

            txtOperatorID.Text = T_OperatorID;
            txtLotID.Text = T_LotID;
            txtSubLotID.Text = T_SubLotID;
            txtHandlerID.Text = T_HandlerID;
            txtMfgLotID.Text = T_MfgLotID;
            txtDeviceID.Text = T_DeviceID;
        }
        
        #region KeyPressEvent - set focus
        
        //1 - Operator ID
        private void txtOperatorID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                txtLotID.Focus();
            }

            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar) && (Convert.ToByte(e.KeyChar) != 0x08))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }
        
        //2 - Lot ID
        private void txtLotID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                //txtMfgLotID.Focus();  // Jedi burns MFG LOT ID at assembly
                txtDeviceID.Focus();
            }
            else if (!Char.IsLetter(e.KeyChar) && !Char.IsDigit(e.KeyChar) && (e.KeyChar.ToString() != "-"))
            {
               e.KeyChar = Convert.ToChar(0);
            }
        }
        
        //3 - Mfg ID
        private void txtMfgLotID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                txtDeviceID.Focus();
            }
            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar) && (e.KeyChar.ToString() != "-") && (e.KeyChar.ToString() != "_"))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }

        //4 - Device ID
        private void txtDeviceID_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                txtSubLotID.Focus();
            }
            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar) && (e.KeyChar.ToString() != "-") && (e.KeyChar.ToString() != "_"))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }

        //5 - Sub Lot ID
        private void txtSubLotID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                txtHandlerID.Focus();
            }
            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar) && (e.KeyChar.ToString() != "-"))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }

        //6 - Handler ID
        private void txtHandlerID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                //txtContactorID.Focus();
                button1.Focus();
            }
            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }

        //7 - Contactor ID
        private void txtContactorID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.Focus();
            }
            else if (!Char.IsLetter(e.KeyChar) && !Char.IsDigit(e.KeyChar) && (e.KeyChar.ToString() != "-"))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }
        
        //8 - Load board ID
        private void txtLbID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.Focus();
            }
            else if (!Char.IsDigit(e.KeyChar) && !Char.IsLetter(e.KeyChar) && (e.KeyChar.ToString() != "-"))
            {
                e.KeyChar = Convert.ToChar(0);
            }
        }

        #endregion KeyPressEvent

        #region EnterEvent

        private void txtOperatorID_Enter(object sender, EventArgs e)
        {
            txtOperatorID.SelectAll();
        }
        private void txtLotID_Enter(object sender, EventArgs e)
        {
            txtLotID.SelectAll();
        }
        private void txtMfgLotID_Enter(object sender, EventArgs e)
        {
            txtMfgLotID.SelectAll();
        }
        private void txtDeviceID_Enter(object sender, EventArgs e)
        {
            txtDeviceID.SelectAll();
        }
        private void txtSubLotID_Enter(object sender, EventArgs e)
        {
            txtSubLotID.SelectAll();
        }              
        private void txtHandlerID_Enter(object sender, EventArgs e)
        {
            txtHandlerID.SelectAll();
        }

        #endregion EnterEvent

        #region MouseDownEvent

        private void txtOperatorID_MouseDown(object sender, MouseEventArgs e)
        {
            txtOperatorID.SelectAll();
        }

        private void txtLotID_MouseDown(object sender, MouseEventArgs e)
        {
            txtLotID.SelectAll();
        }

        private void txtMfgLotID_MouseDown(object sender, MouseEventArgs e)
        {
            txtMfgLotID.SelectAll();
        }

        private void txtDeviceID_MouseDown_1(object sender, MouseEventArgs e)
        {
            txtDeviceID.SelectAll();
        }

        private void txtSubLotID_MouseDown(object sender, MouseEventArgs e)
        {
            txtSubLotID.SelectAll();
        }
        
        private void txtHandlerID_MouseDown(object sender, MouseEventArgs e)
        {
            txtHandlerID.SelectAll();
        }

        #endregion MouseDownEvent

        #region KeydownEvent

        private void txtOperatorID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                txtLotID.Focus();
        }  
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;

            if (e.KeyCode == Keys.Tab)
                //txtMfgLotID.Focus();
                txtDeviceID.Focus();
        }
        private void txtMfgLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                txtDeviceID.Focus();
        }
        private void txtDeviceID_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;

            if (e.KeyCode == Keys.Tab)
                txtSubLotID.Focus();
        } 
        private void txtSubLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                txtHandlerID.Focus();
        }
        
        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                txtOperatorID.Focus();
        }

        #endregion KeydownEvent
                       
        #region Other Event
        
        //Picture click enable password char  
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PassCharCount++;

            if (PassCharCount % 2 != 0) //odd   
            {
                EnablePassword = true;
                txtOperatorID.PasswordChar = '*';
            }
            else //even
            {
                EnablePassword = false;
                txtOperatorID.PasswordChar = '\0';
            }
        }

        #endregion Other Event
        
        //Ok button - Entry checking
        private void button1_Click(object sender, EventArgs e)
        {
            //Set to true if entry field is N/A
            bool passflag_OPID = false;
            bool passflag_HandlerID = false;
            bool passflag_LotID = false;
            bool passflag_SubLotID = false;
            bool passflag_MfgID = false;
            bool passflag_RevID = false;
            bool passflag_DeviceID = false; //Jupiter only (can be used as Assembly ID for other products)
            bool Webservice_Flag = false;

            //Admin mode
            if (EnablePassword == true && txtOperatorID.Text.ToUpper() == "AVGO1500")
            {
                txtOperatorID.Text = "A0001";
                txtLotID.Text = "PT0000000001";
                if (productTag.ToUpper().Contains("-QA"))
                {
                    txtSubLotID.Text = "1AQA";
                }
                else
                {
                    txtSubLotID.Text = "1A";
                }
                //txtHandlerID.Text = "HT001";

                //txtMfgLotID.Text = bIsEngineeringSample ? "BOM_Z9Z" : "000001";

                //passflag_RevID = true;
                T_MfgLotID = txtMfgLotID.Text = txtMfgLotID.Text == "" ? "000001" : txtMfgLotID.Text;
                //passflag_MfgID = true;

                txtDeviceID.Text = "AFEM-8220-TS";
            }

            if (webQueryValidation == "TRUE") //&& !txtLotID.Text.ToLower().Contains("-e"))
            {
                if (productTag.ToUpper().Contains("-QA"))
                {
                    if (txtLotID.Text.Contains("GUCAL") || txtLotID.Text.Contains("SUBCAL") || txtLotID.Text.Contains("LOOSE") || txtLotID.Text.Contains("VERIFY"))
                    {
                        WebserviceByPass = true;
                    }

                    if (txtLotID.Text.ToUpper().Contains("PT1234567890") || txtLotID.Text.ToUpper().Contains("PT0123456789") || txtLotID.Text.ToUpper().Contains("PT0000000001"))
                    {
                        WebserviceByPass = true;
                    }
                }

                if ((txtLotID.Text.StartsWith("PT") || txtLotID.Text.StartsWith("FT")) && WebserviceByPass == false)
                {
                    if (Clotho_User1 == "PPUSER" || Clotho_User1 == "SUSER")
                    {
                        Webservice_Flag = false;
                    }
                    else
                    {
                        Webservice_Flag = true;
                        bool querySuccess = false;
                        string txtlotdummy = "";

                        //QA Webservice Criteria - DH
                        if (productTag.ToUpper().Contains("-QA"))
                        {
                            txtlotdummy = txtLotID.Text;
                            if (txtLotID.Text.Length > 14)
                            {
                                txtLotID.Text = txtLotID.Text.Substring(0, txtLotID.Text.Length - 3);
                            }

                            if (txtLotID.Text.Length == 13)
                            {
                                txtLotID.Text = txtlotdummy;
                            }
                        }

                        if (webServerURL != "")
                        {
                            querySuccess = WebServiceQuery.DisplayInariWebListNames(txtLotID.Text, webServerURL);
                        }
                        else
                        {
                            querySuccess = WebServiceQuery.DisplayInariWebListNames(txtLotID.Text);
                        }

                        web_lotid = WebServiceQuery.LotInfoArray[1].Trim().ToUpper();
                        web_targetdevice = WebServiceQuery.LotInfoArray[3].Trim().ToUpper();
                        web_MFGlot = WebServiceQuery.LotInfoArray[5].Trim().ToUpper();
                        web_handlerid = WebServiceQuery.LotInfoArray[7].Trim().ToUpper();
                        web_2DID = WebServiceQuery.LotInfoArray[9].Trim().ToUpper();

                        //Web Service 2.0 - DH

                        LoggingManager.Instance.LogInfoTestPlan("web_lotid is " + web_lotid);
                        LoggingManager.Instance.LogInfoTestPlan("web_targetdevice is " + web_targetdevice);
                        LoggingManager.Instance.LogInfoTestPlan("web_MFGlot is " + web_MFGlot);
                        LoggingManager.Instance.LogInfoTestPlan("web_handlerid is " + web_handlerid);
                        LoggingManager.Instance.LogInfoTestPlan("web_2DID is " + web_2DID);

                        if ((web_lotid.ToLower() == "null") || (web_targetdevice.ToLower() == "null") || (web_MFGlot.ToLower() == "null") ||
                            (web_handlerid.ToLower() == "null") || (web_2DID.ToLower() == "null"))
                        {
                            querySuccess = false;
                        }

                        if (querySuccess == false)
                        {
                            WebQueryBox msg = new WebQueryBox();
                            msg.DialogResult = DialogResult.Retry;

                            while (msg.DialogResult == DialogResult.Retry || msg.DialogResult == DialogResult.Cancel)
                            {
                                msg.ShowDialog();
                            }
                            if (msg.DialogResult == DialogResult.OK)
                            {
                                Webservice_Flag = false;
                                WebserviceByPass = true;
                            }
                        }
                    }
                }
            }

            if (Webservice_Flag == false)
            {
                WebserviceByPass = true;
            }

            #region OperatorID checking

            List<bool> rxOpID = new List<bool>();

            //Inari:
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[I]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[P]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[N]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[W]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[D]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[L]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[R]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[A]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[C]\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^INT\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^FWI\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^FWN\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^FWM\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^FWP\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^ISK\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^FWR\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^T\d{4}"));
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^AM\d{1,8}")); //Amkor
            rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^[0-9]{1,6}")); //ASEKr
	        rxOpID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtOperatorID.Text, @"^D\d{4}")); //ASEKH

            foreach (bool rxOP in rxOpID)
            {
                if (rxOP)
                {
                    T_OperatorID = txtOperatorID.Text;
                    passflag_OPID = true;
                    break;
                }
            }

            if (!passflag_OPID)
                MessageBox.Show("No matching for Operator ID " + "(" + txtOperatorID.Text + ")" + ", please re-enter!");

            #endregion

            #region LotID checking

            List<bool> rxLotID = new List<bool>();

            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^STDUNIT\d{2}-\d{6}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^STDUNIT\d{3}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^BINCHECK-\d{1,10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^ENGR-\d{1,10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^BINCHECK_\d{6}_\w{1}"));   //BINCHECK_050716_M
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MERGE_\d{6}_\w{1}"));   //MERGE_050716_M

            //PA
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^PT123456789$"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^PT\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^PT\d{10}-\w{1}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^PT\d{10}-\w{2}"));

            //ASEKr
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{8}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\w{8}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{8}-\w{1,5}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{3}\w{2}\d{3}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{1,4}\w{1,3}\d{1,4}"));

	        //ASEKH
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^T\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^T\w{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^T\d{10}-\w{1,5}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^T\d{3}\w{2}\d{3}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^T\d{1,4}\w{1,3}\d{1,4}"));

            //EBR
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^B\d{3}\w{2}\d{3}\w{2}$"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^B\d{3}\w{2}\d{3}\w{1}$"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^B\d{3}\w{2}\d{3}$"));

            //Amkor
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^M\d{8}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^M\d{8}-\w{1,5}"));
            
            //Fbar
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^FT\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^FT\d{10}-\w{1}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^FT\d{10}-\w{2}"));

            //MM
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MT\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MU\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MI\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MC\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^MA\d{10}"));
            rxLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^VT\d{10}"));

            foreach (bool rxLot in rxLotID)
            {
                if (rxLot)
                {
                    if (txtLotID.Text.Contains("STDUNIT"))
                    {
                        T_LotID = txtLotID.Text + "-" + Date + "-" + Shift;
                    }
                    else if (txtLotID.Text.Contains("ENG"))
                    {
                        if (txtLotID.Text.Length > 13) T_LotID = txtLotID.Text.Remove(13);
                        else T_LotID = txtLotID.Text;
                    }
                    else
                    {
                        if (txtLotID.Text.Length > 14) T_LotID = txtLotID.Text.Remove(14);
                        else T_LotID = txtLotID.Text;
                    }

                    passflag_LotID = true;
                    break;
                }
            }

            if (Webservice_Flag == true)
            {
                if (web_lotid != txtLotID.Text)
                {
                    passflag_LotID = false;
                    MessageBox.Show("No matching for Lot ID (" + txtLotID.Text + ") from Inari Web Service" + ", please re-enter!");
                }
                else if (!passflag_LotID)
                {
                    MessageBox.Show("No matching for Lot ID " + "(" + txtLotID.Text + ")" + ", please re-enter!");
                }
            }
            else
            {
                if (!passflag_LotID)
                {
                    MessageBox.Show("No matching for Lot ID " + "(" + txtLotID.Text + ")" + ", please re-enter!");
                }
            }
            #endregion

            #region MfgID checking

            if (!bIsEngineeringSample)
            {
                #region MFG ID check
#if true
                passflag_RevID = true;

                int iMfgID = 0;
                List<bool> rxMfgID = new List<bool>();

                rxMfgID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtMfgLotID.Text.ToUpper(), @"^[0-9]{6}"));
                rxMfgID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtMfgLotID.Text.ToUpper(), @"^[0-9]{5}"));

                foreach (bool rxMfg in rxMfgID)
                {
                    if (rxMfg)
                    {
                        T_MfgLotID = txtMfgLotID.Text;
                        passflag_MfgID = true;
                        break;
                    }
                }

                try
                {
                    iMfgID = Convert.ToInt32(txtMfgLotID.Text);
                }
                catch
                {
                    passflag_MfgID = false;
                }

                if (iMfgID > 131071)
                {
                    passflag_MfgID = false;
                    MessageBox.Show("Mfg Lot Number cannot bigger than 131071 <= " + "(" + txtMfgLotID.Text + ")" + ", please re-enter!");
                }

                passflag_MfgID = true;

                if (Webservice_Flag == true)
                {
                    if (web_MFGlot != txtMfgLotID.Text)
                    {
                        passflag_MfgID = false;
                        if (passflag_LotID == true)
                        {
                            MessageBox.Show("No matching for MfgLot ID " + "(" + txtMfgLotID.Text + ")" + ", please re-enter!");
                        }
                    }
                    else
                    {
                        passflag_MfgID = true;
                    }
                }
                else
                {
                    if (passflag_MfgID == false)
                    {
                        MessageBox.Show("No matching for MfgLot ID " + "(" + txtMfgLotID.Text + ")" + ", please re-enter!");
                    }
                }
#endif

                #endregion MFG ID Check

                //passflag_RevID = true;
                //passflag_MfgID = true;

            }
            #endregion MfgID Check

            #region RevID Check
            else
            {
                passflag_MfgID = true;

                List<bool> rxRevID = new List<bool>();

                rxRevID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtMfgLotID.Text.ToUpper(), @"^BOM_[A-Z]{1}[0-9]{1}[A-Z]{1}"));

                foreach (bool rxRev in rxRevID)
                {
                    if (rxRev)
                    {
                        try
                        {
                            T_RevID = txtMfgLotID.Text.ToUpper().Replace("BOM_", "").Trim().Substring(0, 3);
                        }
                        catch { }

                        passflag_RevID = true;
                        break;
                    }
                }

                if (!passflag_RevID)
                    MessageBox.Show("Rev ID does not match the required format, please re-enter!");
            }
            #endregion

            #region Device ID checking

            List<bool> rxDeviceID = new List<bool>();
            rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^AFEM-\d{4}-TS$")); //Only allow AFEM-8220-TS
            rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^AFEM-\d{4}-MS$")); //Only allow AFEM-8220-MS
            rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^ENGR-\d{4}-TS$")); //Only allow ENGR-8220-TS
            rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^ENGR-\d{4}-MS$")); //Only allow ENGR-8220-MS

            if (addDeviceID != "")
            {
                string[] deviceIDArray = addDeviceID.Split(',');

                //To Support custom deviceID
                foreach (string ID in deviceIDArray)
                {
                    rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^AFEM-\d{4}-"+ ID + "$")); 
                    rxDeviceID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^ENGR-\d{4}-"+ ID + "$")); 
                }
            }

            foreach (bool rxDevID in rxDeviceID)
            {
                if (rxDevID)
                {
                    T_DeviceID = txtDeviceID.Text;
                    passflag_DeviceID = true;
                    break;
                }
            }

            //ASEkr checking:
            bool KA, KE = false;
            bool A_SL, AC_SL = false;
            KA = System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{3}[K]\w{0}[A]\w{0}\d{3}$"); //from lot ID
            KE = System.Text.RegularExpressions.Regex.IsMatch(txtLotID.Text, @"^W\d{3}[K]\w{0}[E]\w{0}\d{3}$"); //from lot ID
            A_SL = System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^AFEM-\d{4}-\w{0}[A]_SL$");
            AC_SL = System.Text.RegularExpressions.Regex.IsMatch(txtDeviceID.Text, @"^AFEM-\d{4}-\w{0}[A]\w{0}[C]_SL$");
            T_DeviceID = txtDeviceID.Text;

            if (KA)
            {
                passflag_DeviceID = (A_SL == true ? true : false);
            }
            else if (KE)
            {
                passflag_DeviceID = (AC_SL == true ? true : false);
            }

            if (Webservice_Flag == true)
            {
                if ((web_targetdevice.Length > 9) && (txtDeviceID.Text.Length > 9))
                {
                    if ((web_targetdevice.Substring(0, 9)) != (txtDeviceID.Text.Substring(0, 9)))
                    {
                        passflag_DeviceID = false;
                        if (passflag_LotID == true)
                        {
                            MessageBox.Show("No matching for Device ID " + "(" + txtDeviceID.Text + ")" + ", please re-enter!");
                        }
                    }
                }                
            }
            else
            {
                if (passflag_DeviceID == false)
                {
                    MessageBox.Show("No matching for Device ID " + "(" + txtDeviceID.Text + ")" + ", please re-enter!");
                }
            }
            #endregion

            #region Sublot ID checking

            List<bool> rxSubLotID = new List<bool>();

            if ((!productTag.ToUpper().Contains("-REQ")) && (!productTag.ToUpper().Contains("EVAL")))
            {
                if (productTag.ToUpper().Contains("-QA"))
                {
                    // for QA
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]QA$")); //1AQA
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]QE$")); //1AQE
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]COQ$")); //1ACOQ
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]COQE$")); //1ACOQE

                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]CCOQ$")); //1ACCOQ
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]CCOQE$")); //1ACCOQE

                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]ECCOQ$")); //1AECCOQ
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]ECCOQE$")); //1AECCOQE

                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]COQSOAK$")); //1ACOQSOAK
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]CCOQSOAK$")); //1ACCOQSOAK
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]ECCOQSOAK$")); //1AECCOQSOAK

                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]UHP$")); //1AUHP
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]TCP$")); //1ATCP
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]UH$")); //1AUH
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]TC$")); //1ATC
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]TC[1-9]$")); //1ATCx

                }
                else
                {
                    // for Production
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-3]\w{0}[A-D]$"));

                    // Misc
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^RE"));
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^LYT"));
                    rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^PE"));
                }

                if(addSublotID != "")
                {
                    string[] subLotIDArray = addSublotID.Split(',');

                    foreach (string ID in subLotIDArray)
                    {
                        rxSubLotID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtSubLotID.Text, @"^[1-5]\w{0}[A-D]" + ID + "$"));
                    }
                }

                foreach (bool rxSlotID in rxSubLotID)
                {
                    if (rxSlotID)
                    {
                        T_SubLotID = txtSubLotID.Text;
                        passflag_SubLotID = true;
                        break;
                    }
                }
            }
            else
            {
                T_SubLotID = txtSubLotID.Text;
                passflag_SubLotID = true;
            }

            if (!passflag_SubLotID)
            {
                MessageBox.Show("Invalid Sub lot ID " + "(" + txtSubLotID.Text + ")" + ", please re-enter!");
            }

            #endregion Sublot ID checking

            #region HandlerID checking

            List<bool> rxHandlerID = new List<bool>();
            rxHandlerID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtHandlerID.Text, @"^EIS\d{3}", RegexOptions.IgnoreCase));          
            rxHandlerID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtHandlerID.Text, @"^SRM\d{3}", RegexOptions.IgnoreCase));
            rxHandlerID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtHandlerID.Text, @"^S9\d{4}", RegexOptions.IgnoreCase));
            rxHandlerID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtHandlerID.Text, @"^HT\d{3}", RegexOptions.IgnoreCase));
            rxHandlerID.Add(System.Text.RegularExpressions.Regex.IsMatch(txtHandlerID.Text, @"^NS\d{4}", RegexOptions.IgnoreCase));

            //foreach (bool rxHandler in rxHandlerID)
            //{
            //    if (rxHandler)
            //    {
            //        T_HandlerID = txtHandlerID.Text;
            passflag_HandlerID = true;
            //        break;
            //    }
            //}

            // QA Test Program bypass Handler ID Check - DH
            if (Webservice_Flag == true && !(productTag.ToUpper().Contains("-QA")))
            {
                if (web_handlerid != txtHandlerID.Text)
                {
                    passflag_HandlerID = false;
                    if (passflag_LotID == true)
                    {
                        MessageBox.Show("No matching for Handler SN " + "(" + txtHandlerID.Text + ")" + ", please re-enter!");
                    }
                }
                else
                {
                    passflag_HandlerID = true;
                }
            }
            else
            {
                if (passflag_HandlerID == false)
                    MessageBox.Show("No matching for Handler SN " + "(" + txtHandlerID.Text + ")" + ", please re-enter!");
            }
            #endregion

            if (passflag_OPID && passflag_HandlerID && passflag_LotID && passflag_SubLotID && passflag_DeviceID && passflag_MfgID && passflag_RevID)
            {
                if (Webservice_Flag == true)
                {
                    if ((web_targetdevice.Substring(0, 9)) == productTag.ToUpper().Substring(0, 9))
                    {
                        this.DialogResult = DialogResult.OK;
                        this.FormClosing -= new System.Windows.Forms.FormClosingEventHandler(this.FrmDataInput_FormClosing);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No matching for Product ID " + "(" + web_targetdevice + ")" + ", please reload Clotho and scan in correct Program!");
                    }
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.FormClosing -= new System.Windows.Forms.FormClosingEventHandler(this.FrmDataInput_FormClosing);
                    this.Close();
                }
            }
        }

        private void FrmDataInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
