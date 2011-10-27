using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Account.Services;
using Kooboo.CMS.Account.Models;
using Kooboo.CMS.Web.Areas.Account.Models;
using Kooboo.CMS.Web.Models;
using Kooboo.Globalization;
using Kooboo.CMS.Sites;
namespace Kooboo.CMS.Web.Areas.Account.Controllers
{
    [Kooboo.CMS.Web.Authorizations.RequiredLogOnAttribute(RequiredAdministrator = true, Order = 1)]
    public class RolesController : ControllerBase
    {
        //
        // GET: /Account/Roles/
        [HttpGet]
        public ActionResult Index()
        {
            return View(ServiceFactory.RoleManager.All());
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreateRoleModel(new Role(), ServiceFactory.RoleManager.AllPermissions()));
        }
        [HttpPost]
        public ActionResult Create(CreateRoleModel model)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                var role = ServiceFactory.RoleManager.Get(model.Name);
                if (ModelState.IsValid)
                {
                    ServiceFactory.RoleManager.Add(model.ToRole());
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }
        [HttpGet]
        public ActionResult Edit(string name)
        {
            var role = ServiceFactory.RoleManager.Get(name);
            return View(new CreateRoleModel(role, ServiceFactory.RoleManager.AllPermissions()));
        }
        [HttpPost]
        public ActionResult Edit(CreateRoleModel model)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    ServiceFactory.RoleManager.Update(model.Name, model.ToRole());
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }
        public ActionResult Delete(Role[] model)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                foreach (var role in model)
                {
                    ServiceFactory.RoleManager.Delete(role.Name);
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }


        public ActionResult IsNameAvailable(string name)
        {
            if (ServiceFactory.RoleManager.Get(name) != null)
            {
                return Json(string.Format("{0} is not available.".Localize(), name), JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
