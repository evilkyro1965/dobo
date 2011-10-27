using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.View;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites.Controllers.Front;

namespace Kooboo.CMS.ModuleTemplate.Controllers
{
    public class ScaffoldController : PageController
    {
        static ScaffoldController()
        {
            GenerateModuleInfo();
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            Site.InlineEditing = false;
        }

        public ActionResult Admin(string moduleName, string moduleUrl)
        {
            moduleName = "dev";
            ModuleAdminPosition position = new ModuleAdminPosition()
            {
                ModuleName = moduleName
            };
            if (string.IsNullOrEmpty(moduleUrl))
            {
                moduleUrl = "~/admin/Settings";
            }
            var moduleActionResult = ModuleExecutor.InvokeAction(this.ControllerContext, Site, moduleUrl, position);
            var moduleHtml = ModuleExecutor.ExecuteActionResult(moduleActionResult);
            return View(moduleHtml);
        }


        private static void GenerateModuleInfo()
        {
            ModuleInfo moduleInfo = new ModuleInfo();
            moduleInfo.ModuleName = "Sample";
            moduleInfo.Version = "3.0.1.0";
            moduleInfo.KoobooCMSVersion = "3.0.1.0";
            moduleInfo.InstallUrl = "~/installer/install";
            moduleInfo.UninstallUrl = "~/installer/uninstall";
            moduleInfo.DefaultSettings = new ModuleSettings()
            {
                ThemeName = "Default",
                Entry = new Entry()
                {
                    Controller = "Home",
                    Action = "Index"
                }
            };
            moduleInfo.EntryOptions = new EntryOption[]{
                new EntryOption(){ Name="NewsList",Entry = new Entry{ Controller="Home",Action ="Index"}},
                new EntryOption(){ Name="NewsCategories",Entry = new Entry{ Controller="Home",Action ="Categories"}},
                new EntryOption(){Name="ArticleCategories",Entry=new Entry{Controller="Article",Action="Categories"}},
                new EntryOption(){Name="ArticleList",Entry=new Entry{Controller="Article",Action="List"}},
            };
            moduleInfo.DefaultSettings.CustomSettings = new Dictionary<string, string>();
            moduleInfo.DefaultSettings.CustomSettings["Setting1"] = "Value1";
            ModuleInfo.Save(moduleInfo);
        }
    }
}
