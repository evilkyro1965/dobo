﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Services;
using Kooboo.Web;
using Kooboo.CMS.Web.Models;
using Kooboo.CMS.Sites;
namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
    [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Sites", Group = "System", Name = "Custom error", Order = 1)]
    public class CustomErrorController : IManagerControllerBase<CustomError, CustomErrorManager>
    {
        #region import/export
        protected string GetZipFileName()
        {
            return "CustomError.zip";
        }

        public ActionResult Import(bool @override)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    Manager.Import(Site, Request.Files[0].InputStream, @override);
                }
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }
            return Json(resultEntry, "text/plain", System.Text.Encoding.UTF8);

        }


        public ActionResult Export()
        {
            var fileName = GetZipFileName();
            Response.AttachmentHeader(fileName);
            Manager.Export(Site, Response.OutputStream);
            return null;
        }

        #endregion


        public ActionResult IsStatusCodeAvailable(string statusCode, string old_Key)
        {
            var enumValue = Enum.Parse(typeof(HttpErrorStatusCode), statusCode);
            var enumString = enumValue.ToString();
            if (!enumString.EqualsOrNullEmpty(old_Key, StringComparison.CurrentCultureIgnoreCase))
            {
                if (ServiceFactory.CustomErrorManager.Get(Site, enumString) != null)
                {
                    return Json("The status code is already exists.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
