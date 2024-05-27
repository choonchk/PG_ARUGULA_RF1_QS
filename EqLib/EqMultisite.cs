using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClothoLibAlgo;

namespace EqLib
{
    public class Eq
    {
        public static Eq[] Site;
        public static byte NumSites;
        public static byte [] EnabledSites;
        public List<Chassis.Chassis_base> Chassis;
        public KeysightReferenceClock.iRFCLK EqClk;
        public EqRF.iEqRF RF;
        public Dictionary<string, EqDC.iEqDC> DC = new Dictionary<string, EqDC.iEqDC>();
        public EqHSDIO.EqHSDIObase HSDIO;
        public EqPM.iEqPM PM;
        public EqSwitchMatrix.EqSwitchMatrixBase SwMatrix;
        public EqSwitchMatrix.EqSwitchMatrixBase SwMatrixSplit;
        public NA.NetworkAnalyzerAbstract EqNA;
        public Eq_ENA.ENA_base EqENA;
        public static EquipSA EqMXA;
        public Eq_ENA.NetAn_Base EqNetAn;
        public static EqHandler.iEqHandler Handler;
        public static SplitTestPhase CurrentSplitTestPhase = SplitTestPhase.NoSplitTest;
        public static string InstrumentInfo = "";

        public static void SetNumSites(byte numSites)
        {
            Site = new Eq[numSites];
            
            for (byte site = 0; site < numSites; site++)
            {
                Site[site] = new Eq();
            }

            NumSites = numSites;
        }
    }

    public enum SplitTestPhase
    {
        NoSplitTest, PhaseA, PhaseB
    }
}
