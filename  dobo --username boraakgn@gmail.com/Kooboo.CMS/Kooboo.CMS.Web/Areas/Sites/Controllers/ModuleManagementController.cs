using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Services;
using Kooboo.CMS.Web.Areas.Sites.Models;
using Kooboo.CMS.Web.Models;
using System.Text;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.Web.Mvc;

namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
    [Kooboo.CMS.Web.Authorizations.RequiredLogOnAttribute(RequiredAdministrator = true, Order = 99)]
    public class ModuleManagementController : ControllerBase
    {
        public ModuleManagementController()
            : this(ServiceFactory.ModuleManager)
        { }

        public ModuleManagementController(ModuleManager moduleManager)
        {
            Manager = moduleManager;
        }
        public ModuleManager Manager { get; set; }
        //
        // GET: /Sites/ModuleManagement/

        public ActionResult Index()
        {
            return View(Manager.AllModuleInfo());
        }
        #region Install
        [HttpGet]
        public ActionResult Install()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Install(InstallModuleModel installModel)
        {
            JsonResultEntry result = new JsonResultEntry() { RedirectToOpener = false };
            try
            {
                if (ModelState.IsValid)
                {
                    var moduleFile = this.Request.Files["ModuleFile"];
                    if (moduleFile == null || moduleFile.ContentLength == 0)
                    {
                        ModelState.AddModelError("ModuleFile", "ModuleFile is null.");
                    }
                    else
                    {
                        StringBuilder log = new StringBuilder();

                        var moduleInfo = Manager.Install(installModel.ModuleName, moduleFile.InputStream, ref log);

                        if (moduleInfo == null && log.Length != 0)
                        {
                            result.AddMessage(log.ToString());
                        }
                        else
                        {
                            var moduleInstallUrl = moduleInfo.InstallUrl;
                            if (string.IsNullOrEmpty(moduleInstallUrl))
                            {
                                result.RedirectUrl = Url.Action("InstallationCompleted", new { moduleName = moduleInfo.ModuleName });
                            }
                            else
                            {
                                result.RedirectUrl = Url.Action("InstallScaffold", new { moduleName = moduleInfo.ModuleName, installUrl = moduleInstallUrl });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.AddException(e);
            }

            result.AddModelState(ModelState);
            return Json(result);
        }

        public ActionResult InstallScaffold(string moduleName, string installUrl)
        {
            ModuleAdminPosition position = new ModuleAdminPosition()
            {
                ModuleName = moduleName
            };
            if (string.IsNullOrEmpty(installUrl))
            {
                return RedirectToAction("InstallationCompleted");
            }
            var moduleActionResult = ModuleExecutor.InvokeAction(this.ControllerContext, null, installUrl, position);
            var moduleHtml = ModuleExecutor.ExecuteActionResult(moduleActionResult);
            return View(moduleHtml);
        }
        public ActionResult InstallationCompleted(string moduleName, string message)
        {
            ViewBag.ModuleName = moduleName;
            ViewBag.Message = message;
            return View();
        }

        #endregion

        #region Uninstall

        public ActionResult Uninstall(string moduleName)
        {
            var moduleInfo = Manager.Get(moduleName);
            if (!string.IsNullOrEmpty(moduleInfo.UninstallUrl))
            {
                return RedirectToAction("UninstallScaffold", new { moduleName = moduleInfo.ModuleName, installUrl = moduleInfo.UninstallUrl });
            }
            else
            {
                return RedirectToAction("UninstallModule", this.ControllerContext.RequestContext.AllRouteValues());
            }
        }
        public ActionResult UninstallModule(string moduleName)
        {
            var moduleInfo = Manager.Get(moduleName);
            return View(moduleInfo);
        }
        [HttpPost]
        public ActionResult UninstallModule(string moduleName, FormCollection form)
        {
            JsonResultEntry result = new JsonResultEntry() { RedirectToOpener = false };
            try
            {
                if (!string.IsNullOrEmpty(moduleName))
                {
                    Manager.Uninstall(moduleName);
                }
                result.RedirectUrl = Url.Action("UninstallationCompleted", new { moduleName = moduleName });
            }
            catch (Exception e)
            {
                result.AddException(e);
            }

            return Json(result);
        }
        public ActionResult UninstallScaffold(string moduleName, string installUrl)
        {
            ModuleAdminPosition position = new ModuleAdminPosition()
            {
                ModuleName = moduleName
            };
            if (string.IsNullOrEmpty(installUrl))
            {
                return RedirectToAction("UninstallationCompleted");
            }
            var moduleActionResult = ModuleExecutor.InvokeAction(this.ControllerContext, null, installUrl, position);
            var moduleHtml = ModuleExecutor.ExecuteActionResult(moduleActionResult);
            return View(moduleHtml);
        }

        public ActionResult UninstallationCompleted(string moduleName, string message)
        {
            ViewBag.ModuleName = moduleName;
            ViewBag.Message = message;
            return View();
        }
        #endregion

        public ActionResult Relations(string moduleName)
        {
            var model = Manager.AllSitesInModule(moduleName).Select(siteName => new RelationModel()
            {
                RelationName = siteName,
                RelationType = "Site"
            });
            return View("Relations", model);
        }

        public ActionResult IsNameAvailable(string moduleName)
        {
            if (Manager.Get(moduleName) != null)
            {
                return Json("The name already exists.", JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
