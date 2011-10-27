using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.Web.Url;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModulePath : IPath
    {
        static ModulePath()
        {
            BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cms_Data", ModuleDir);
            BaseVirutalPath = UrlUtility.Combine("~/", "Cms_Data", ModuleDir);
        }
        public static Func<string, string> PhysicalPathAccessor = moduleName => Path.Combine(BaseDirectory, moduleName);
        public static Func<string, string> VirtualPathAccessor = moduleName => UrlUtility.Combine(BaseVirutalPath, moduleName);
        public static string BaseDirectory { get; set; }
        public static string BaseVirutalPath { get; set; }
        static string ModuleDir = "Modules";
        public ModulePath(string moduleName)
        {
            Name = moduleName;
            PhysicalPath = PhysicalPathAccessor(moduleName);
            VirtualPath = VirtualPathAccessor(moduleName);
        }
        public string Name { get; private set; }
        public string PhysicalPath { get; private set; }
        public string VirtualPath { get; private set; }
    }
}
