using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Web.Areas.Account.Models;
using Kooboo.Connect;
using Kooboo.CMS.Sites;
namespace Kooboo.CMS.Web.Areas.Account.Controllers
{
    public class LogOnController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(LoginModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (UserServices.ValidateUser(loginModel.UserName, loginModel.Password) != null)
                {
                    System.Web.Security.FormsAuthentication.SetAuthCookie(loginModel.UserName, loginModel.RememberMe);

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return Redirect(System.Web.Security.FormsAuthentication.DefaultUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Username and/or password are incorrect.");
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult Ajax(LoginModel loginModel, int redirect)
        {
            var resultData = new JsonResultEntry();
            try
            {
                if (UserServices.ValidateUser(loginModel.UserName, loginModel.Password) != null)
                {
                    System.Web.Security.FormsAuthentication.SetAuthCookie(loginModel.UserName, loginModel.RememberMe);
                    if (redirect == 0)
                    {
                        resultData.RedirectUrl = Request.UrlReferrer.ToString();
                    }
                    else
                    {
                        resultData.RedirectUrl = System.Web.Security.FormsAuthentication.DefaultUrl;
                    }

                }
                else
                {
                    ModelState.AddModelError("UserName", "Username and/or password are incorrect.");
                }

                resultData.AddModelState(ModelState);
            }
            catch (Exception e)
            {
                resultData.AddException(e);
            }
            return Json(resultData);
        }
        public ActionResult SignOut(string returnUrl)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = System.Web.Security.FormsAuthentication.LoginUrl;
            }
            return Redirect(returnUrl);
        }

    }
}
