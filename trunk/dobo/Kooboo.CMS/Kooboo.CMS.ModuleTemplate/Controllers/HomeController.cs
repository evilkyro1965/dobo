﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.ModuleTemplate.Models;

namespace Kooboo.CMS.ModuleTemplate.Controllers
{

    [HandleError]
    public class HomeController : ModuleControllerBase
    {
        public ActionResult Index(string category)
        {
            List<News> news = new List<News>();
            if (!string.IsNullOrEmpty(category))
            {
                for (int i = 0; i < 10; i++)
                {
                    news.Add(new News() { Id = i, Title = string.Format("{0} - news {1}", category, i), Body = "news body" });
                }
            }


            return View(news);
        }
        public ActionResult Detail(int id)
        {
            return View(new News() { Id = id, Title = "News" + id.ToString(), Body = "news body" });
        }
        public ActionResult Categories()
        {
            return View();
        }
        public ActionResult About()
        {
            return View("about");
        }

        public ActionResult Download()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.WriteByte(0);
            return File(ms, "application/zip", "file.zip");
        }
        public ActionResult OutputJson()
        {
            return Json(new { Id = 1, Name = "Name1" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PartialRender()
        {
            return View();
        }
        public ActionResult AjaxView()
        {
            return PartialView();
        }
    }
}
