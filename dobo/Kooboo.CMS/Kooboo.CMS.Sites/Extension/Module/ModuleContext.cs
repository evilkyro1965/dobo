using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.Web.Routing;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModuleContext
    {
        public static ModuleContext Create(Site site, string moduleName, ModuleSettings moduleSettings)
        {
            return new ModuleContext(site, moduleName, moduleSettings);
        }
        protected ModuleContext(Site site, string moduleName, ModuleSettings moduleSettings)
        {
            this.Site = site;
            ModuleName = moduleName;
            this.ModuleSettings = moduleSettings;
        }
        public string ModuleName { get; private set; }
        public ModulePath ModulePath
        {
            get
            {
                return new ModulePath(this.ModuleName);
            }
        }
        public Site Site { get; private set; }
        public RouteCollection RouteTable
        {
            get
            {
                return RouteTables.GetRouteTable(this.ModuleName);
            }
        }

        public ModuleSettings ModuleSettings { get; private set; }
    }
}
