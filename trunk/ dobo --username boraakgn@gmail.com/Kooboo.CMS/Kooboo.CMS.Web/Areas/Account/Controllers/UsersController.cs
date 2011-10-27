﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Account.Services;
using Kooboo.CMS.Account.Models;
using Kooboo.CMS.Web.Areas.Account.Models;
using Kooboo.CMS.Web.Models;
using Kooboo.Globalization;
using Kooboo.CMS.Web.Authorizations;
using Kooboo.CMS.Sites;
using Kooboo.Web.Mvc.Paging;
namespace Kooboo.CMS.Web.Areas.Account.Controllers
{
    [Kooboo.CMS.Web.Authorizations.RequiredLogOnAttribute(RequiredAdministrator = true, Order = 99)]
    public class UsersController : ControllerBase
    {
        //
        // GET: /Account/Users/

        public ActionResult Index(string search, int? page, int? pageSize)
        {
            var users = ServiceFactory.UserManager.All();
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(it => it.UserName.Contains(search));
            }
            return View(users.ToPagedList<User>(page ?? 1, pageSize ?? 50));
        }

        //
        // GET: /Account/Users/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Account/Users/Create

        [HttpPost]
        public ActionResult Create(CreateUserModel model, FormCollection collection)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    ServiceFactory.UserManager.Add(model.ToUser());
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);

        }

        //
        // GET: /Account/Users/Edit/5

        public ActionResult Edit(string userName)
        {
            return View(new CreateUserModel(ServiceFactory.UserManager.Get(userName)));
        }

        //
        // POST: /Account/Users/Edit/5

        [HttpPost]
        public ActionResult Edit(CreateUserModel model, FormCollection collection)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    ServiceFactory.UserManager.Update(model.UserName, model.ToUser());
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }

        //
        // GET: /Account/Users/Delete/5

        public ActionResult Delete(User[] model)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                foreach (var user in model)
                {
                    ServiceFactory.UserManager.Delete(user.UserName);
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }

        public ActionResult IsNameAvailable(string userName)
        {
            if (ServiceFactory.UserManager.Get(userName) != null)
            {
                return Json(string.Format("{0} is not available.".Localize(), userName), JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [RequiredLogOn(Order = 1, Exclusive = true)]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [RequiredLogOn(Order = 1, Exclusive = true)]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var entry = new JsonResultEntry();

            try
            {
                entry.Success = ServiceFactory.UserManager.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (!entry.Success)
                {
                    entry.AddMessage("Old password is wrong , please input old password again!");
                }
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
    }
}
