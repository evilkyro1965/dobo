
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.ModuleTemplate.Models;
using Kooboo.Globalization;
using Kooboo.CMS.Sites;
namespace Kooboo.CMS.ModuleTemplate.Controllers
{
    public class AdminController : ModuleControllerBase
    {      
        public ActionResult Settings(string moduleName, string siteName)
        {
            ModuleInfo_Metadata moduleInfo = new ModuleInfo_Metadata(moduleName, siteName);

            return View(moduleInfo);
        }

        [HttpPost]
        public ActionResult Settings(string moduleName, string siteName, ModuleInfo_Metadata moduleInfo)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    ModuleInfo.SaveModuleSetting(moduleName, siteName, moduleInfo.Settings);
                    resultEntry.AddMessage("Module setting has been changed.".Localize());
                }

            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }
            return Json(resultEntry);
        }
    }
}
