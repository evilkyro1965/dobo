using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Services;
using Kooboo.CMS.Sites;
using Kooboo.CMS.Sites.Models;
using Kooboo.Globalization;
namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
    [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "HtmlBlock", Order = 1)]
    public class HtmlBlockController : PathResourceControllerBase<Kooboo.CMS.Sites.Models.HtmlBlock, HtmlBlockManager>
    {
        public ActionResult Localize(string name, string fromSite)
        {
            Manager.Localize(name, fromSite, Site);
            return RedirectToIndex();
        }


        public ActionResult IsNameAvailable(string name, string old_Key)
        {
            if (old_Key == null || !name.EqualsOrNullEmpty(old_Key, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Manager.Get(Site, name) != null)
                {
                    return Json("The name already exists.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #region Version
        public ActionResult Version(string name)
        {
            var htmlBlock = new HtmlBlock(Site, name);
            var model = Manager.VersiongLogger.AllVersions(htmlBlock);
            return View(model);
        }

        public ActionResult Revert(string name, int version)
        {
            var entry = new JsonResultEntry();
            var htmlBlock = new HtmlBlock(Site, name);

            try
            {
                Manager.VersiongLogger.Revert(htmlBlock, version);
                entry.SetSuccess().AddMessage("Revert Successfully.".Localize());
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }

        public ActionResult PreviewVersion(string name, int version)
        {
            var page = new HtmlBlock(Site, name);
            var model = Manager.VersiongLogger.GetVersion(page, version);
            return View(model);
        }
        #endregion
    }
}
