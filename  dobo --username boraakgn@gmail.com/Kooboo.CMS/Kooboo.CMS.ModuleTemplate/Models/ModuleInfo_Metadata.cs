using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.CMS.Sites.Extension.Module;

namespace Kooboo.CMS.ModuleTemplate.Models
{
    public class ModuleInfo_Metadata
    {
        public ModuleInfo_Metadata()
        {

        }
        public ModuleInfo_Metadata(string moduleName, string siteName)
        {
            var moduleInfo = ModuleInfo.Get(moduleName);

            this.ModuleName = moduleInfo.ModuleName;
            this.Version = moduleInfo.Version;
            this.KoobooCMSVersion = moduleInfo.KoobooCMSVersion;

            this.Settings = ModuleInfo.GetSiteModuleSettings(moduleName, siteName);
        }
        public string ModuleName { get; set; }
        public string Version { get; set; }
        public string KoobooCMSVersion { get; set; }
        public ModuleSettings Settings { get; set; }
    }
}