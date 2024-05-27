using System.Collections.Generic;
using EqLib;
using MPAD_TestTimer;

namespace TestPlanCommon.CommonModel
{
    public class TesterManager
    {
        public ITesterSite CurrentTester;

        public TesterManager(ITesterSite testerLocation)
        {
            CurrentTester = testerLocation;
        }
    }

    /// <summary>
    /// Provide flexibility for variation between testers and sites.
    /// </summary>
    public interface ITesterSite
    {
        string GetVisaAlias(string visaAlias, byte site);
        string GetHandlerName();

        EqSwitchMatrix.Rev GetSwitchMatrixRevision();
        List<KeyValuePair<string, string>> GetSmuSetting();

    }

    /// <summary>
    /// Here for now, may not be needed.
    /// </summary>
    public interface IEquipmentInitializer
    {
        void SetTester(ITesterSite tester);
        void InitializeSwitchMatrix(bool isMagicBox);
        Dictionary<string, string> Digital_Definitions_Part_Specific { get; }
        bool InitializeHSDIO();
        bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion);
        bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion,
           Dictionary<string, string> TXQC, Dictionary<string, string> RXQC, TestLib.MipiTestConditions testConditions);
        void InitializeSmu();
        ValidationDataObject InitializeDC(ClothoLibAlgo.Dictionary.Ordered<string, string[]> DcResourceTempList);
        void InitializeChassis();
        void InitializeRF();
        void InitializeHandler(string handlerType, string visaAlias);


    }
}
