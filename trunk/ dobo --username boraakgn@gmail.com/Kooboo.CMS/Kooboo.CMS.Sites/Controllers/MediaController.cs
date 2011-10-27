using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Kooboo.IO;

namespace Kooboo.CMS.Sites.Controllers
{
    public class MediaController : Controller
    {
        public ActionResult Index(string url)
        {
            var physicalPath = this.ControllerContext.RequestContext.HttpContext.Server.MapPath(url);
            if (System.IO.File.Exists(physicalPath))
            {
                return File(physicalPath, IOUtility.MimeType(physicalPath));
            }
            return null;
        }
    }
}
