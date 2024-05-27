using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using Avago.ATF.Shares; 


namespace TestPlanDriver
{
    public class CalHandlerScanner
    {
        private readonly static string _CALPlugInFileDef = ATFMiscConstants.PATH_HANDLER_PLUGINS_PATH + "CalHandlerPluginTemplate.dll";

        public static List<string> CollectAllHandlerTypes(ref string err)
        {
            List<string> calTypeList = new List<string>(); 

            // Load the Default Handlers 
            if (File.Exists(_CALPlugInFileDef))
            {
                try
                {
                    Assembly owningAssembly = Assembly.LoadFile(_CALPlugInFileDef);
                    if (null != owningAssembly)
                    {
                        foreach (Type type in owningAssembly.GetTypes())
                        {
                            if (type.IsAbstract)
                                continue;
                            if (type.IsDefined(typeof(HandlerPlugInAttribute), true))
                            {
                                if (type.Name.StartsWith(HandlerConstants.Tag_CalHandlerName_PREFIX))
                                {
                                    if (calTypeList.Contains(type.Name))
                                    {
                                        err = "The CAL Handler PlugIn loaded contains Duplicated CAL Handler Type Names: " + type.Name;
                                        return null; 
                                    }
                                    
                                    calTypeList.Add(type.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    err = "Loading CAL Handlers Plug-In '" + _CALPlugInFileDef + "' failed: " + ex.Message;
                    return null; 
                }
            }
                        
            if (calTypeList.Count < 1)
            {
                err = "Loading CAL Handlers Plug-Ins get 0 Match";
                return null; 
            }

            return calTypeList; 
        }

    }
}
