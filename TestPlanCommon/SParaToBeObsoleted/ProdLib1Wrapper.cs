using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using TestPlanCommon.SParaModel;
using ProductionLib;
using MPAD_TestTimer;
using sm = ProductionLib.clsSwitchMatrix;

namespace ToBeObsoleted
{
    /// <summary>
    /// Switch Matrix.
    /// </summary>
    public class ProdLib1Wrapper
    {
        private string m_zbandinfo;
        private string zbandinfo;
        private readonly sm.Rev m_revision;
        private ProdLib2Wrapper m_wrapper2;

        public ProdLib2Wrapper Wrapper2
        {
            get { return m_wrapper2; }
        }

        private Dictionary<string, clsSwitchMatrix.iSwitchBox> SwitchesDictionary;

        public ProdLib1Wrapper()
        {
            m_revision = sm.Rev.O2;

            SwitchesDictionary = new Dictionary<string, clsSwitchMatrix.iSwitchBox>(); //using the interface
            m_wrapper2 = new ProdLib2Wrapper();
        }

        //TODO Check if is ENA only?
        /// <summary>
        /// Required for calibration.
        /// </summary>
        /// <param name="zbandinfo2"></param>
        public void ActivateManualECal(string zbandinfo2)
        {
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N1toTx, SwitchesDictionary);
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N2toAnt, SwitchesDictionary);
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N3toRx, SwitchesDictionary);
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N4toRx, SwitchesDictionary);
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N5toRx, SwitchesDictionary);
            clsSwitchMatrix.Maps2.Activate(zbandinfo2, clsSwitchMatrix.Operation2.N6toRx, SwitchesDictionary);
        }

        //[Obsolete]
        ////TODO Check if there is result if not called.
        //public void CallcAlgoRunTest(SParaTestManager modelcAlgorithm)
        //{
        //    //modelcAlgorithm.Run_Tests();
        //}

        /// <summary>
        /// cFBAR still need this.
        /// </summary>
        public Dictionary<string, clsSwitchMatrix.iSwitchBox> SwitchesDictionary1
        {
            get { return SwitchesDictionary; }
        }

        private void SetCurrentBand(string bandInfo)
        {
            m_zbandinfo = bandInfo;
        }

        private void SetSM2(sm.Operation2 operation2, sm.DutPort dutPort, sm.InstrPort instrPort)
        {
            sm.Maps2.Define(m_zbandinfo, operation2, dutPort, instrPort, m_revision);
        }

        private void SetSM3(string bandInfo, sm.Operation2 operation2, sm.DutPort dutPort, sm.InstrPort instrPort)
        {
            sm.Maps2.Define(bandInfo, operation2, dutPort, instrPort, m_revision);
        }

        public void Initialize()
        {
            //This section instantiates an object for each switch to be used
            sm.SwitchMatrix2 zO2 = new sm.SwitchMatrix2();
            string O2_SN = "5000";
            zO2.Initialize(true, clsSwitchMatrix.Rev.O2, ref O2_SN);
            //clsSwitchMatrix.SwitchMatrix2 zY2 = new clsSwitchMatrix.SwitchMatrix2();
            //zY2.Initialize(true, clsSwitchMatrix.Rev.Y2, ref Y2_1_SN);
            //clsSwitchMatrix.ZTM15_1 zZTM15_1 = new clsSwitchMatrix.ZTM15_1();
            //zZTM15_1.Initialize(ref ZTM15_1_SN); //connects to the box using SN
            //clsSwitchMatrix.ZTM15_2 zZTM15_2 = new clsSwitchMatrix.ZTM15_2();
            //zZTM15_2.Initialize(ref ZTM15_2_SN); //connects to the box using SN                                  

            //This section stuffs the created objects from above to a dictionary that is going to be passed in
            //to the test for the switching
            SwitchesDictionary.Add("O2_" + O2_SN, zO2);
            //SwitchesDictionary.Add("Y2_"+Y2_1_SN, zY2);
            //SwitchesDictionary.Add("ZTM15_1_"+ZTM15_1_SN, zZTM15_1);
            //SwitchesDictionary.Add("ZTM15_2_"+ZTM15_2_SN, zZTM15_2);
        }
        
        public void SetSwitchDefinitionPerBand()
        {
            zbandinfo = "INTXMB_ANT1_OUTMB1_OUTMB2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A6, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A13, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            //NF setting
            SetSM2(sm.Operation2.ENAtoRFIN, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.ENAtoRFOUT, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.ENAtoRX, sm.DutPort.A6, sm.InstrPort.N3);


            zbandinfo = "INB34MIMO_OUTMB1_OUTMB2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A6, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A13, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A14, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INTXHB_ANT1_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            //NF setting
            SetSM2(sm.Operation2.ENAtoRFIN, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.ENAtoRFOUT, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.ENAtoRX, sm.DutPort.A11, sm.InstrPort.N4);

            //Added by CheeOn
            zbandinfo = "INTXHB_ANT1_OUT1_OUT4";
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A20, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2);

            //NF setting
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2);

            zbandinfo = "INANT1_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A20, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2);

            zbandinfo = "INANT2_OUT3_OUT4";
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INMBMIMO2_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A16, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);


            //====================================================

            zbandinfo = "INMBMIMO1_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A15, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INHBAUX2_OUTHBAUX1_OUTHBAUX2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A18, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INHBAUX1_OUTHBAUX1_OUTHBAUX2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A17, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INB21MIMO_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A19, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "INHBMIMO_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);


            //======================================================

            zbandinfo = "TRX1_ANT1_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            zbandinfo = "TRX4_ANT1_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INHB2G_ANT1";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INTXMB_ANT1_OUTMB1_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A6, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INTXMB_ANT1_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);


            //========================================================

            zbandinfo = "INTXMB_ANT1_OUTHBAUX1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INTXMB_ANT1_OUT1_OUTHBAUX2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INTXHB_ANT1_OUTHBAUX1_OUTHBAUX2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "TRX2_ANT1_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A23, sm.InstrPort.N6);

            zbandinfo = "TRX3_ANT1_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A24, sm.InstrPort.N6);


            //===========================================================

            zbandinfo = "TRX5_ANT1_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A9, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A12, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INTXMB_ANT2_OUTMB1_OUTMB2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A6, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A13, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            //NF setting
            SetSM2(sm.Operation2.ENAtoRFIN, sm.DutPort.A2, sm.InstrPort.N1);
            SetSM2(sm.Operation2.ENAtoRFOUT, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.ENAtoRX, sm.DutPort.A13, sm.InstrPort.N4);

            zbandinfo = "INTXHB_ANT2_OUT1_OUT2";
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2);

            zbandinfo = "INTXHB_ANT2_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            //NF setting
            SetSM2(sm.Operation2.ENAtoRFIN, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.ENAtoRFOUT, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.ENAtoRX, sm.DutPort.A8, sm.InstrPort.N3);

            //Added by Chee On
            zbandinfo = "INTXHB_ANT2_OUT3_OUT2";
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2);

            //NF setting
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2);
            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2);

            zbandinfo = "INANT2_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            //NF setting
            SetSM2(sm.Operation2.ENAtoRFIN, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.ENAtoRFOUT, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.ENAtoRX, sm.DutPort.A7, sm.InstrPort.N3);


            zbandinfo = "TRX1_ANT2_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A22, sm.InstrPort.N6);

            //===================================================

            zbandinfo = "TRX4_ANT2_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "INHB2G_ANT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A25, sm.InstrPort.N6);

            zbandinfo = "TRX2_ANT2_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A23, sm.InstrPort.N6);

            zbandinfo = "TRX3_ANT2_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A24, sm.InstrPort.N6);

            zbandinfo = "TRX5_ANT2_CPL";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            //============================================================

            zbandinfo = "NO_CHANNEL_ASSIGNED";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A6, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A21, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INMBMIMO2_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A16, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INMBMIMO1_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A15, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INB21MIMO_OUT3_OUT4";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A8, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A11, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A19, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INHBMIMO_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A1, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A5, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

            zbandinfo = "INTXHB_ANT1_OUT1_OUT2";
            SetCurrentBand(zbandinfo);
            SetSM2(sm.Operation2.N1toTx, sm.DutPort.A3, sm.InstrPort.N1);
            SetSM2(sm.Operation2.N2toAnt, sm.DutPort.A4, sm.InstrPort.N2);
            SetSM2(sm.Operation2.N3toRx, sm.DutPort.A7, sm.InstrPort.N3);
            SetSM2(sm.Operation2.N4toRx, sm.DutPort.A10, sm.InstrPort.N4);
            SetSM2(sm.Operation2.N5toRx, sm.DutPort.A20, sm.InstrPort.N5);
            SetSM2(sm.Operation2.N6toRx, sm.DutPort.A26, sm.InstrPort.N6);

        }
        
        public void SetSwitchMatrixNF()
        {
            //NF ==============================================================================
            SetSM3("NS", sm.Operation2.ENAtoRFOUT, sm.DutPort.A4, sm.InstrPort.NS); //Noise Source specific to ANT1
            SetSM3("NS_ANT2", sm.Operation2.ENAtoRFOUT, sm.DutPort.A5, sm.InstrPort.NS); //Noise Source specific to ANT2
            SetSM3("NS_INMBMIMO2", sm.Operation2.ENAtoRFOUT, sm.DutPort.A16, sm.InstrPort.NS2); //Noise Source specific to RX_IN_B25/3 MIMO
            SetSM3("NS_INB34MIMO", sm.Operation2.ENAtoRFOUT, sm.DutPort.A14, sm.InstrPort.NS2); //Noise Source specific to RX_IN_B21_AUX
            SetSM3("NS_INHBAUX1", sm.Operation2.ENAtoRFOUT, sm.DutPort.A17, sm.InstrPort.NS2); //Noise Source specific to RX_IN_B34
            SetSM3("NS_INHBAUX2", sm.Operation2.ENAtoRFOUT, sm.DutPort.A18, sm.InstrPort.NS2); //Noise Source specific to RX_IN_HB_MIMO
            SetSM3("NS_INB21MIMO", sm.Operation2.ENAtoRFOUT, sm.DutPort.A19, sm.InstrPort.NS2); //Noise Source specific to RX_IN_B1/4/66 MIMO
            SetSM3("NS_INHBMIMO", sm.Operation2.ENAtoRFOUT, sm.DutPort.A20, sm.InstrPort.NS2); //Noise Source specific to RX_IN_HB_AUX1
            SetSM3("NS_INMBMIMO1", sm.Operation2.ENAtoRFOUT, sm.DutPort.A15, sm.InstrPort.NS2); //Noise Source specific to RX_IN_HB_AUX1
            //================================================================================
        }

        public void SetSwitchMatrixObsolete()
        {
#if false

                            zbandinfo = "B3_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1

                            zbandinfo = "B66US_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1

                            zbandinfo = "B25_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1

                            zbandinfo = "B39_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1

                            zbandinfo = "B7_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B30_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B41PRX_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B40_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B2RX_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1

                            zbandinfo = "B41_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B32RX2_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.NONE, clsSwitchMatrix.InstrPort.NONE, clsSwitchMatrix.Rev.O2); //Not connected to anything
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1

                            zbandinfo = "B4_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1




                            zbandinfo = "B34RX_AUX";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT RX_IN_B34

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT RX_IN_B34                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1


                            zbandinfo = "B1_B3_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B25_B25_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B25_B66_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B25_B30_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B3_B3_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B3_B1_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B66_B2_B25_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B1_B1_MB1_MB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B1_B3_MB4RX2_MB4RX1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2

                            zbandinfo = "B1_B40_MB4RX2_HBAUX1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2

                            zbandinfo = "B3_B7_MB4RX1_HBAUX2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT

                            zbandinfo = "B7_B40_HBAUX2_HBAUX1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT


                            zbandinfo = "B7_AUX2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT


                            zbandinfo = "B66_B30_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2


                            zbandinfo = "B39_B41_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B3_B7_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B3_B40_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B3_B41P_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B1_B7_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B1_B40_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B1_B41PRX_MB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2


                            zbandinfo = "B7_B1_HB2_MB1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B30_B25_HB2_MB1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B30_B66_HB2_MB1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2


                            zbandinfo = "B7_B3_HB2_MB1_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B7_B40_HB1_HB2_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B30_B30_HB1_HB2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2



                            zbandinfo = "B40_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1

                            zbandinfo = "HB_SPLIT_CA";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "B40CARX_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2


                            zbandinfo = "B39_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A16, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_B34
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B32RX2_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.NONE, clsSwitchMatrix.InstrPort.NONE, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.ZTM64_COM, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.ZTM65_COM, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1
                                                                                                                                                                                          //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.ZTM66_COM, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //RX_IN_B25/3 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB                                      
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1

                            zbandinfo = "TRX1_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX1
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                                  
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX1

                            zbandinfo = "TRX1_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX1
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                                  
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A22, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX1




                            zbandinfo = "B32RX_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.NONE, clsSwitchMatrix.InstrPort.NONE, clsSwitchMatrix.Rev.O2); //Not connected to anything
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                                  
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2


                            zbandinfo = "B34_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB                                  
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B32RX_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.NONE, clsSwitchMatrix.InstrPort.NONE, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1


                            zbandinfo = "B3_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2




                            zbandinfo = "B66US_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B25_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B34_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            zbandinfo = "B7_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1



                            zbandinfo = "B30_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1




                            zbandinfo = "B40_AUX1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A20, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A20, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_HB_AUX1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A7, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB_AUX1_OUT

                            zbandinfo = "B4_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2



                            zbandinfo = "B2RX_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A12, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB2


                            zbandinfo = "B41_AUX2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT

                            zbandinfo = "B41_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1



                            zbandinfo = "B41PRX_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A3, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1


                            zbandinfo = "B1_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2

                            zbandinfo = "B1_MIMO_HB2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2


                            zbandinfo = "B3_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1

                            zbandinfo = "B3_MIMO_HB1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1


                            zbandinfo = "B4_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2

                            zbandinfo = "B4_MIMO_HB2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A8, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2


                            zbandinfo = "B66_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A13, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2

                            zbandinfo = "B66_MIMO_HB2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A18, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B1/4/66 MIMO                   
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //MB_4RX2


                            zbandinfo = "B25_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B25/3 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1

                            zbandinfo = "B25_MIMO_HB1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B25/3 MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A14, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_B25/3 MIMO                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1


                            zbandinfo = "B7_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A21, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_AUX2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A11, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB_AUX2_OUT

                            zbandinfo = "B21RX_AUX";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A15, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B21_AUX

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A15, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B21_AUX                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A9, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1

                            zbandinfo = "B21RX_AUX_HB1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A15, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B21_AUX

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A15, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //RX_IN_B21_AUX                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //MB_4RX1


                            zbandinfo = "HB_MIMO";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A17, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_MIMO

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A2, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_MB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A17, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //DUT_RX_IN_HB_MIMO                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A10, clsSwitchMatrix.InstrPort.N4, clsSwitchMatrix.Rev.O2); //HB2

                            zbandinfo = "TRX2_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A23, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A23, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX2


                            zbandinfo = "TRX2_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A23, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A23, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX2

                            zbandinfo = "TRX3_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3

                            zbandinfo = "TRX3_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3

                            zbandinfo = "TRX4_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX4

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX4

                            zbandinfo = "TRX4_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX4

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A25, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX4

                            zbandinfo = "TRX5_ANT1";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

                            zbandinfo = "TRX5_ANT2";
                            //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N3toRx, clsSwitchMatrix.DutPort.A6, clsSwitchMatrix.InstrPort.N3, clsSwitchMatrix.Rev.O2); //HB1
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N4toRx, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                                                                                                                                                                                           //clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N5toRx, clsSwitchMatrix.DutPort.A24, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX3
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N6toRx, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A19, clsSwitchMatrix.InstrPort.N5, clsSwitchMatrix.Rev.O2); //CPL
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

                            zbandinfo = "B2GHB_ANT1";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A4, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT1                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

                            zbandinfo = "B2GHB_ANT2";
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N1toTx, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.N2toAnt, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2

                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFIN, clsSwitchMatrix.DutPort.A1, clsSwitchMatrix.InstrPort.N1, clsSwitchMatrix.Rev.O2); //TX_IN_2G_HB
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRFOUT, clsSwitchMatrix.DutPort.A5, clsSwitchMatrix.InstrPort.N2, clsSwitchMatrix.Rev.O2); //ANT2                    
                            clsSwitchMatrix.Maps2.Define(zbandinfo, clsSwitchMatrix.Operation2.ENAtoRX, clsSwitchMatrix.DutPort.A26, clsSwitchMatrix.InstrPort.N6, clsSwitchMatrix.Rev.O2); //TRX5

#endif        
        }

    }

    /// <summary>
    /// Progressive zip and others. Cannot delete, prod best practice.
    /// </summary>
    public class ProdLib2Wrapper
    {
        private SMPAD_ID smpad_id;
        SplitResultBuffer fbar_split_results;
        SplitGUCalResultBuffer fbar_split_gucal_results;
        private ProgressiveZipDataObject m_pzDo;

        //Split Test:   
        string result_buffer_db_file = @"C:\Avago.ATF.Common\SplitResult\fbar.db";
        string gucal_result_buffer_db_file = @"C:\Avago.ATF.Common\SplitResult\%PRODUCT%_gucal_fbar.db";  // independent gu cal

        public ProdLib2Wrapper()
        {
            smpad_id = SMPAD_ID.Instance;
        }

        /// <summary>
        /// Required. ProdBestPractice.
        /// </summary>
        /// <param name="pzDo"></param>
        public void RunProgressiveZip(ProgressiveZipDataObject pzDo)
        {
            m_pzDo = pzDo;

            if (pzDo.IsProgressiveZipOn)
            {
                ProductionLib.FrmWorkerUI MoveFileworker = new ProductionLib.FrmWorkerUI();
                //Compress SNP for SDI
                MoveFileworker.Start("Copying wave files...", MoveTraceFiles, false);
            }
            else
            {
                // TODO Obsolete this.
                #region SNP File SDI Compression
                ProductionLib.FrmWorkerUI ZipWorker = new ProductionLib.FrmWorkerUI();

                //Compress SNP for SDI
                ZipWorker.Start("Zipping wave files...", CompressSNPforSDI, false);
                #endregion
            }
        }

        public void ReadSmPadId()
        {
            string smpad_model = "";
            string smpad_sn = "";

            SMPAD_ID.Instance.ReadID(out smpad_model, out smpad_sn);
        }

        public void InitializeVar()
        {
            SplitTestVariable.SplitTestEnable = false; //Set to false for full test
        }

        [Obsolete]
        public void UnitIdOtp(string _otp_LotID, string _otp_SubLotID)
        {
            ProductionLib.UnitID_OTP unitID_OTP;
            unitID_OTP = ProductionLib.UnitID_OTP.Instance(@"C:\Avago.ATF.Common\UnitIDOTP\UnitID_OTP.db", _otp_LotID, _otp_SubLotID);
        }

        public void UnitIdOtp2(bool isPa_Site, string _otp_LotID, string _otp_SubLotID)
        {
            #region Unit ID OTP

            if (isPa_Site)    // Unit ID OTP burning
            {
#if (DEBUG)
                    if (_otp_LotID == "")
                        _otp_LotID = "DEBUG";
#else
                if (_otp_LotID == "" || _otp_LotID == "DEBUG")
                {
                    _otp_LotID = "DEBUG";
                    PromptManager.Instance.ShowError("Enter Lot ID before loading test plan!", "Jedi");
                    string msg = "Unble to read Lot ID, abort test plan!";
                    LoggingManager.Instance.LogError(msg);
                    throw new Exception(msg);
                }
#endif
                if (_otp_SubLotID == "")
                    _otp_SubLotID = "1";
                else
                    _otp_SubLotID = _otp_SubLotID.Substring(0, 1);

                string msg2 = "Lot ID: " + _otp_LotID + " Sub Lot ID: " + _otp_SubLotID;
                LoggingManager.Instance.LogHighlight(msg2);

                UnitIdOtp(_otp_LotID, _otp_SubLotID);
            }
            #endregion Unit ID OTP
        }

        private void MoveTraceFiles(bool dummy)
        {
            MoveTraceFiles(m_pzDo);
        }

        /// <summary>
        /// ChoonChin - For progressive zip. Does not directly depend on ProductionLib.
        /// </summary>
        /// <param name="pzDo"></param>
        private void MoveTraceFiles(ProgressiveZipDataObject pzDo)
        {
            //Close zip, move file & delete source directory
            if (!pzDo.IsStopZip)
            {
                try
                {
                    myzip.Close();
                    File.Move(pzDo.TempFileName, pzDo.ActualFileName);
                    Directory.Delete(pzDo.ActiveDir, true);
                    //Put some sleep to indicate something is running
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }
            }
        }

        bool ProgressiveZipOn = true; //set to true to enable
        static bool FirstZip = false;
        static bool StopZip = false;
        string tempFileName, actualFileName, newactiveDir = string.Empty;
        string[] waveTraceFolders, Tracefiles;
        int TraceFileNeed = 0;
        int TraceFileCount = 0;
        System.IO.DirectoryInfo di;
        string TempZipPath = @"C:\Avago.ATF.Common\DataLog\";

        static string sdi_inbox_wave = @"C:\Avago.ATF.Common\Results\";
        ICSharpCode.SharpZipLib.Zip.ZipFile myzip;

        public void CreateTraceFiles(ProgressiveZipDataObject pzDo)
        {
            if (!ProgressiveZipOn || StopZip) return;
            //ChoonChin - For progressive zip
            string source_directory = pzDo.ActiveDir;

            if (pzDo.IsRunning) return;
            //Create first zip
            if (!Directory.Exists(source_directory)) return;
            try
            {
                waveTraceFolders = Directory.GetDirectories(source_directory, "*_CH*", SearchOption.TopDirectoryOnly);

                if (!FirstZip && !(Directory.GetFiles(waveTraceFolders[0]).Length == 0)) //Make sure is not empty folder
                {
                    newactiveDir = Path.GetDirectoryName(source_directory);
                    newactiveDir = newactiveDir.Substring(newactiveDir.LastIndexOf("\\") + 1, newactiveDir.Length - 1 - newactiveDir.LastIndexOf("\\"));

                    tempFileName = TempZipPath + newactiveDir + ".actraceo";
                    actualFileName = sdi_inbox_wave + newactiveDir + ".actraceo";

                    myzip = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(tempFileName);
                    myzip.NameTransform = new ICSharpCode.SharpZipLib.Zip.ZipNameTransform(source_directory);
                    FirstZip = true;
                    LoggingManager.Instance.LogInfo("First actraceo file created.");
                }

                if (!(Directory.GetFiles(waveTraceFolders[0]).Length == 0)) //Make sure is not empty folder
                {
                    myzip.BeginUpdate();

                    foreach (string waveTraceFolder in waveTraceFolders)
                    {
                        Tracefiles = Directory.GetFiles(waveTraceFolder);
                        foreach (string file in Tracefiles)
                        {
                            myzip.Add(file, ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated);
                        }
                    }

                    myzip.CommitUpdate();
                    TraceFileCount++;

                    //Delete trace files but not folder
                    foreach (string waveTraceFolder in waveTraceFolders)
                    {
                        di = new DirectoryInfo(waveTraceFolder);
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    }
                }

                if (TraceFileCount == TraceFileNeed)
                {
                    StopZip = true; //already complete trace file collection, close now and move.
                    myzip.Close();
                    File.Move(tempFileName, actualFileName);
                    Directory.Delete(pzDo.ActiveDir, true);
                    LoggingManager.Instance.LogInfo("All trace files successfully zipped and moved.");
                }
            }
            catch (Exception)
            {
                LoggingManager.Instance.LogInfo("Number of files is " + TraceFileCount);
                throw;
            }
        }

        [Obsolete]
        private void CompressSNPforSDI(bool dummy)
        {
            //m_pzDo.ModelcAlgorithm.SNP_SDI_Compression(m_pzDo.ActiveDir, m_pzDo.Sdi_Inbox_Wave);
        }

        [Obsolete]
        public void CreateSplitTestResultDatabase(string productTag)
        {
            // create split test result database
            List<TestParam> fbar_results = new List<TestParam>();
            fbar_split_results = SplitResultBuffer.Instance(result_buffer_db_file, fbar_results);

            // create split Icc calibration result database
            gucal_result_buffer_db_file = gucal_result_buffer_db_file.Replace("%PRODUCT%", productTag);
            fbar_split_gucal_results = SplitGUCalResultBuffer.Instance(gucal_result_buffer_db_file, fbar_results);
        }

        [Obsolete]
        public void CreateSplitTestResultDatabase2()
        {
#if false
                    if (SplitTestVariable.SplitTestEnable)
                    {
                        Aemulus_PXI.my482.Read_Temp(out load_board_temperature_FBAR);

                            #region Initial SQLite database for split test buffer

#if false
                {

                // perform dummy FBAR test to generate fbar_results parameter list
                List<TestParam> fbar_results = new List<TestParam>();

                //Run test
                //Run test       
                LibFbar.b_FirstTest = true;
                LibFbar.SNPFile.FileOutput_Enable = false;
                Unit_ID = 1;

                Test_FBAR(ref fbar_results);

                LibFbar.b_FirstTest = false;
                LibFbar.SNPFile.FileOutput_Enable = TraceFileEnable;
                Unit_ID = 0;

                LogToLogServiceAndFile(LogLevel.Info, "FBAR DB init!");

                // create split test result database
                fbar_split_results = SplitResultBuffer.Instance(result_buffer_db_file, fbar_results);

                // create split Icc calibration result database
                gucal_result_buffer_db_file = gucal_result_buffer_db_file.Replace("%PRODUCT%", ProductTag);
                fbar_split_gucal_results = SplitGUCalResultBuffer.Instance(gucal_result_buffer_db_file, fbar_results);
                }
#endif

                            #endregion Initial SQLite database for split test buffer
                    }
#endif        
        }
    }

    public class ProgressiveZipDataObject
    {
        public bool IsRunning { get; set; }
        public bool IsStopZip { get; set; }
        public string TempFileName { get; set; }
        public string ActualFileName { get; set; }
        public string ActiveDir { get; set; }
        public string Sdi_Inbox_Wave { get; set; }

        public bool IsProgressiveZipOn { get; set; }
        //public SParaTestManager ModelcAlgorithm { get; set; }

        public ProgressiveZipDataObject()
        {
            IsProgressiveZipOn = true;      //set to true to enable
            IsStopZip = false;
            Sdi_Inbox_Wave = @"C:\Avago.ATF.Common\Results\";
            // Note: these are not assigned.
            string tempFileName, actualFileName, newactiveDir = string.Empty;
        }
    }

    /// <summary>
    /// Simulate no calls to ProductionLib.
    /// </summary>
    public class StubProdLib1Wrapper
    {
        private string m_zbandinfo;
        private string zbandinfo;
        private readonly sm.Rev m_revision;
        private ProdLib2Wrapper m_wrapper2;

        public ProdLib2Wrapper Wrapper2
        {
            get { return m_wrapper2; }
        }

        private Dictionary<string, clsSwitchMatrix.iSwitchBox> SwitchesDictionary;

        public StubProdLib1Wrapper()
        {
            m_wrapper2 = new ProdLib2Wrapper();
        }

        /// <summary>
        /// cFBAR still need this.
        /// </summary>
        public Dictionary<string, clsSwitchMatrix.iSwitchBox> SwitchesDictionary1
        {
            get { return SwitchesDictionary; }
        }

        private void SetCurrentBand(string bandInfo)
        {
        }

        private void SetSM2(sm.Operation2 operation2, sm.DutPort dutPort, sm.InstrPort instrPort)
        {
        }

        private void SetSM3(string bandInfo, sm.Operation2 operation2, sm.DutPort dutPort, sm.InstrPort instrPort)
        {
        }

        public void Initialize()
        {
        }

        ///// <summary>
        ///// Required. ProdBestPractice.
        ///// </summary>
        //public bool CallcAlgoRunManualSw(SParaTestManager modelcAlgorithm, string SW_cmd)
        //{
        //    return true;
        //}

        //public void CallcAlgoRunTest(SParaTestManager modelcAlgorithm)
        //{
        //}

        public void ActivateManualECal(string zbandinfo2)
        {
        }

        public void SetSwitchDefinitionPerBand()
        {
        }

        public void SetSwitchMatrixNF()
        {
        }

        public void SetSwitchMatrixObsolete()
        {
        }



    }


}
