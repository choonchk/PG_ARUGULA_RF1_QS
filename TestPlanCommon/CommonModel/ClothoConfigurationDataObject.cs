using Avago.ATF.StandardLibrary;

namespace TestPlanCommon.CommonModel
{
    public class ClothoConfigurationDataObject
    {
        public string ClothoRootDir { get; set; }
        public string ConfigXmlPath { get; set; }

        public void Initialize()
        {
            ClothoRootDir = GetTestPlanPath();
            ConfigXmlPath = @"C:\Avago.ATF.4.1.0\System\Configuration\ATFConfig.xml";
        }

        private static string GetTestPlanPath()
        {
            string basePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "");

            if (basePath == "")   // Lite Driver mode
            {
                string tcfPath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");

                int pos1 = tcfPath.IndexOf("TestPlans") + "TestPlans".Length + 1;
                int pos2 = tcfPath.IndexOf('\\', pos1);

                basePath = tcfPath.Remove(pos2);
            }

            return basePath + "\\";
        }
    }
}