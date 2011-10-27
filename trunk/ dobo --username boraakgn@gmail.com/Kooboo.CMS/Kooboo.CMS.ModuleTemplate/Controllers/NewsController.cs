using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.ModuleTemplate.Models;
using Kooboo.CMS.ModuleTemplate.RepositoryPattern;
using Kooboo.Web.Mvc;
using Kooboo.CMS.Sites;

namespace Kooboo.CMS.ModuleTemplate.Controllers
{
    public class NewsController : ModuleControllerBase
    {
        IRepository<News> repository;
        public NewsController(IRepository<News> repository)
        {
            this.repository = repository;
        }
        public NewsController()
            : this(new NewsRepository())
        {
        }

        //
        // GET: /News/

        public ActionResult Index()
        {
            return View(repository.All());
        }


        //
        // GET: /Default1/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Default1/Create

        [HttpPost]
        public ActionResult Create(News news)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ViewData.ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    repository.Add(news);
                    resultEntry.RedirectUrl = Url.Action("Index", this.ControllerContext.RequestContext.AllRouteValues());
                }

            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }

        //
        // GET: /Default1/Edit/5

        public ActionResult Edit(int id)
        {
            var news = repository.ById(id);
            return View(news);
        }

        //
        // POST: /Default1/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, News news)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ViewData.ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    repository.Update(news);
                    resultEntry.RedirectUrl = Url.Action("Index", this.ControllerContext.RequestContext.AllRouteValues());
                }

            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost]
        public ActionResult Delete(News[] model)
        {
            JsonResultEntry resultEntry = new JsonResultEntry();
            try
            {
                foreach (var item in model)
                {
                    repository.Delete(item);
                }
                resultEntry.RedirectUrl = Url.Action("Index", ControllerContext.RequestContext.AllRouteValues().Merge("id", null));
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }
            return Json(resultEntry);

        }
    }
}
