using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Web.Models;
using Kooboo.Web.Mvc;
using Kooboo.CMS.Content.Models.Binder;
using Kooboo.CMS.Sites;

namespace Kooboo.CMS.Web.Areas
{
    [ValidateAntiForgeryToken]
    [ValidateInput(false)]
    public class SubmissionControllerBase : AreaControllerBase
    {
        protected ActionResult ReturnActionResult(object model, RuleViolationException exception)
        {
            var jsonResult = this.ControllerContext.RequestContext.GetRequestValue("JsonResult");
            string redirectUrl = this.ControllerContext.RequestContext.GetRequestValue("RedirectUrl");

            if (jsonResult.EqualsOrNullEmpty("true", StringComparison.OrdinalIgnoreCase))
            {
                if (exception != null)
                    exception.FillIssues(this.ModelState);
                JsonResultEntry resultEntry = new JsonResultEntry(this.ModelState) { Model = model, RedirectUrl = redirectUrl };
                return Json(resultEntry);
            }
            if (exception != null)
            {
                throw exception;
            }

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            if (this.Request.UrlReferrer != null)
            {
                return Redirect(this.Request.UrlReferrer.OriginalString);
            }
            return new EmptyResult();
        }
    }
}