﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Interoperability.MetaWeblog;

namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
    public class MetaweblogController : Controller
    {
        //
        // GET: /Contents/Metaweblog/

        public ActionResult Index(string repositoryName)
        {
            IHttpHandler httpHandler = new MetaWeblogAPIHandler() { RepositoryName = repositoryName };
            httpHandler.ProcessRequest(System.Web.HttpContext.Current);
            return null;
        }

    }
}
