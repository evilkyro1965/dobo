using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites;

namespace Kooboo.CMS.ModuleTemplate.Controllers
{
    public class InstallerController : ModuleControllerBase
    {
        //
        // GET: /Install/
        [HttpGet]
        public ActionResult Install()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Install(string moduleName, FormCollection form)
        {
            JsonResultEntry result = new JsonResultEntry() { RedirectToOpener = false };

            var parentUrlHelper = new UrlHelper(PageControllerContext.RequestContext);
            result.RedirectUrl = parentUrlHelper.Action("InstallationCompleted", new { controller = "ModuleManagement", ModuleName = moduleName, Message = "" });

            return Json(result);
        }
        [HttpGet]
        public ActionResult Uninstall()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Uninstall(string moduleName)
        {
            JsonResultEntry result = new JsonResultEntry() { RedirectToOpener = false };

            var parentUrlHelper = new UrlHelper(PageControllerContext.RequestContext);
            result.RedirectUrl = parentUrlHelper.Action("UninstallModule", new { controller = "ModuleManagement", ModuleName = moduleName, Message = "" });

            return Json(result);
        }
    }
}
