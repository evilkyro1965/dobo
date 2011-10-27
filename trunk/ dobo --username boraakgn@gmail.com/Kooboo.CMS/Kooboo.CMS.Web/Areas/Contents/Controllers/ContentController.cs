using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Services;

namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
   
    public class ContentController : ContentControllerBase
    {
        //
        // GET: /Content/Content/

        //TextFolderManager TextFolderManager
        //{
        //    get { return ServiceFactory.TextFolderManager; }
        //}

        public ActionResult Index(string search, string fullName)
        {
            //var model = FolderManager.All(Repository, search, fullName);

            //return View(model);

            return RedirectToAction("Index", "Folder", new { Folder = fullName });
        }

    }
}
