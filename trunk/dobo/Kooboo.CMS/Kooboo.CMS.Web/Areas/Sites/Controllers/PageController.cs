using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Services;
using Kooboo.Web;
using Kooboo.Web.Mvc;
using Kooboo.Globalization;

using Kooboo.CMS.Content.Query;
using ContentService = Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Sites.DataRule;
using Kooboo.CMS.Web.Models;
using Kooboo.CMS.Sites;
using Kooboo.CMS.Sites.View;
using Kooboo.CMS.Web.Areas.Sites.Models;
namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
    [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Sites", Group = "Page", Name = "Edit", Order = 1)]
    public class PageController : PathResourceControllerBase<Page, PageManager>
    {
        Repository Repository
        {
            get
            {
                return GerSiteRepository(Site.Repository);
            }
        }

        public static Repository GerSiteRepository(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            return new Repository(name).AsActual();
        }

        #region CURD Localize
        
        public override System.Web.Mvc.ActionResult Index(string search)
        {
            var layoutList = ServiceFactory.LayoutManager.All(Site, "");

            ViewData["LayoutList"] = layoutList;
            return View(List(search));
        }

        public override ActionResult Create()
        {
            if (!string.IsNullOrWhiteSpace(Site.Repository))
            {
                GenerateFolderListDicViewData();
            }

            bool isDefault = false;
            bool.TryParse(ControllerContext.RequestContext.GetRequestValue("IsDefault"), out isDefault);
            var page = new Page()
            {
                Layout = ControllerContext.RequestContext.GetRequestValue("layout"),
                IsDefault = isDefault
            };

            return View(page);
        }

        [HttpPost]
        public override ActionResult Create(Page model)
        {

            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                if (ModelState.IsValid)
                {
                    var parentPage = ControllerContext.RequestContext.GetRequestValue("parentPage");
                    Page parent = null;
                    if (!string.IsNullOrWhiteSpace(parentPage))
                    {
                        parent = PageHelper.Parse(Site, parentPage);
                    }

                    model.Parent = parent;

                    SavePosition(model);

                    model.UserName = User.Identity.Name;

                    Manager.Add(Site, parentPage, model);

                    entry.SetSuccess();

                    entry.RedirectUrl = GetReturnUrl();

                }


            }
            catch (Exception e)
            {
                entry.AddException(e);
                entry.SetFailed();
            }

            return Json(entry);

        }


        //public ActionResult CreateTemp(string parentPage, string layout, string returnUrl)
        //{
        //    Page parent = null;
        //    if (!string.IsNullOrWhiteSpace(parentPage))
        //    {
        //        parent = new Page(Site, PageHelper.SplitFullName(parentPage).ToArray());
        //    }
        //    var newPage = Manager.CreateByLayout(Site, parent, layout);

        //    return RedirectToAction("Edit", Request.RequestContext.AllRouteValues().Merge("fullName", newPage.FullName));
        //}
        public override ActionResult Edit(string name)
        {
            #region

            if (!string.IsNullOrWhiteSpace(Site.Repository))
            {
                GenerateFolderListDicViewData();
            }

            #endregion
            var o = Get(name);
            return View(o);
        }
        public ActionResult Draft(string name)
        {
            #region

            if (!string.IsNullOrWhiteSpace(Site.Repository))
            {
                GenerateFolderListDicViewData();
            }

            #endregion
            var o = Get(name);
            return View(ServiceFactory.PageManager.PageProvider.GetDraft(o));
        }
        [HttpPost]
        public ActionResult Draft(Page newModel, string old_key, bool? preview, bool? published)
        {
            JsonResultEntry result = new JsonResultEntry();
            try
            {
                newModel.Site = Site;

                var old = Manager.Get(Site, old_key);

                SavePosition(newModel);

                newModel.Published = false;
                newModel.UserName = User.Identity.Name;
                Manager.PageProvider.SaveAsDraft(newModel);

                if (published.HasValue && published.Value == true)
                {
                    Manager.Publish(old, true, User.Identity.Name);

                    result.RedirectUrl = GetReturnUrl();
                }
                result.AddMessage("The item has been saved.".Localize());
            }
            catch (Exception e)
            {
                result.AddException(e);
            }

            return Json(result);
        }

        [HttpPost]
        public override ActionResult Edit(Page newModel, string old_key)
        {
            JsonResultEntry result = new JsonResultEntry();
            try
            {
                newModel.Site = Site;
                newModel.UserName = User.Identity.Name;

                var old = Manager.Get(Site, old_key);

                SavePosition(newModel);

                var saveAsDraft = this.Request.Form["SaveAsDraft"];

                result.RedirectUrl = GetReturnUrl();

                if (!string.IsNullOrEmpty(saveAsDraft) && saveAsDraft.ToLower() == "true")
                {
                    Manager.PageProvider.SaveAsDraft(newModel);
                    result.Model = new { preview = true };
                    //result.RedirectUrl = Url.Action("Draft", ControllerContext.RequestContext.AllRouteValues());
                }
                else
                {
                    var setPublished = Request.Form["SetPublished"];
                    if (!string.IsNullOrEmpty(setPublished))
                    {
                        var published = false;
                        bool.TryParse(setPublished, out published);
                        newModel.Published = published;
                    }
                    else
                    {
                        result.RedirectUrl = null;
                        result.AddMessage("The item has been saved.".Localize());
                    }
                    Manager.Update(Site, newModel, old);
                }

                if (string.IsNullOrEmpty(result.RedirectUrl))
                {
                    result.Model = FrontUrlHelper.Preview(Url, Kooboo.CMS.Sites.Models.Site.Current, newModel, null).ToString();
                }

            }
            catch (Exception e)
            {
                result.AddException(e);
            }

            return Json(result);
        }

        [HttpPost]
        public ActionResult ChangeLayout(string fullName, string layout)
        {
            var entry = new JsonResultEntry();

            try
            {
                var old = Manager.Get(Site, fullName);
                old.Layout = layout;
                old.UserName = User.Identity.Name;
                Manager.Update(Site, old, old);
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }

        private void SavePosition(Page newModel)
        {
            var json = Request.Form["PagePositionsJson"];
            if (!string.IsNullOrEmpty(json))
            {
                var positions = PageDesignController.ParsePagePositions(json);
                newModel.PagePositions = positions;
            }
        }

        protected override ActionResult RedirectToIndex()
        {
            string fullName = Request["fullName"];
            return RedirectToIndex(fullName);
        }
        protected override ActionResult RedirectToIndex(string newFullName)
        {
            var routes = this.ControllerContext.RequestContext.AllRouteValues();
            if (!string.IsNullOrEmpty(newFullName))
            {
                routes["fullName"] = newFullName;
            }
            return RedirectToAction("Index", routes);
        }
        protected override Page Get(string name)
        {
            string fullName = Request["fullName"];
            return Manager.Get(Site, fullName);
        }
        protected override void Update(Page @new, string oldFullName)
        {
            throw new NotSupportedException("The implement is in Edit action method.");
            //var old = Manager.Get(Site, oldFullName);
            //@new = Manager.Get(Site, oldFullName);
            //if (TryUpdateModel(@new))
            //{
            //    Manager.Update(Site, @new, old);
            //}
        }
        protected override void Remove(Page model)
        {
            model.Site = Site;
            Manager.Remove(Site, model);
        }
        protected override IEnumerable<Page> List(string search)
        {
            string fullName = Request["fullName"];

            if (fullName != null)
            {
                ViewData["page"] = Manager.Get(Site, fullName);
            }


            var list = Manager.All(Site, fullName, search);

            return list;
        }

        public ActionResult Localize(string fullName, string fromSite)
        {
            Manager.Localize(fullName, fromSite, Site);

            var fullNameArray = PageHelper.SplitFullName(fullName);

            if (!string.IsNullOrWhiteSpace(this.Request.RequestContext.GetRequestValue("ReturnUrl")))
            {
                return Redirect(this.Request.RequestContext.GetRequestValue("ReturnUrl"));
            }
            return RedirectToAction("Index", new { fullName = PageHelper.CombineFullName(fullNameArray.Take(fullNameArray.Count() - 1)) });


        }
        #endregion

        #region import/export
        public override void Export(Page[] model)
        {
            //var fullNameArray = model.Select(o => o.FullName);
            //var selected = Manager.All(Site, "").Where(o => fullNameArray.Contains(o.FullName));

            var fileName = GetZipFileName();
            Response.AttachmentHeader(fileName);

            Manager.ExportSelected(Site, model, Response.OutputStream);
        }

        protected override string GetZipFileName()
        {
            var fullName = Request.RequestContext.GetRequestValue("FullName");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "Pages.zip";
            }

            return "Page." + fullName + ".zip";
        }



        public override ActionResult Import(string fullName, bool @override)
        {
            return base.Import(fullName, @override);
        }

        #endregion

        #region DataRule Settings
        public ActionResult DataRuleFolderGrid()
        {
            return View();
        }

        private void GenerateFolderListDicViewData()
        {
            var folderTree = ContentService.ServiceFactory.TextFolderManager.FolderTrees(Repository);

            ViewData["FolderTree"] = folderTree;
        }



        public ActionResult GetFolderInfo(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return null;
            }

            var folder = Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(new TextFolder(Repository, FolderHelper.SplitFullName(folderPath)));

            if (folder == null)
            {
                return null;
            }

            var subfolders = ContentService.ServiceFactory.TextFolderManager.ChildFolders(folder);

            var schema = ContentService.ServiceFactory.SchemaManager.Get(Repository, folder.SchemaName);

            var folderInfo = new
            {
                Folder = folder,
                Schema = schema,
                ContentList = folder.CreateQuery(),
                FolderList = subfolders,
                CategoryFolders = folder.Categories == null ? (new List<TextFolder>()) : folder.Categories.Select(o => FolderHelper.Parse<TextFolder>(Repository, o.FolderName))
            };

            return Json(folderInfo);
        }


        public ActionResult DataRuleGridForms(DataRuleSetting[] DataRules)
        {
            return View(DataRules);
        }
        #endregion

        #region IsNameAvailable
        public ActionResult IsNameAvailable(string name, string parentPage, string old_Key)
        {
            if (old_Key == null || !name.EqualsOrNullEmpty(old_Key, StringComparison.CurrentCultureIgnoreCase))
            {
                string fullName = PageHelper.CombineFullName(new[]
                {
                    parentPage,
                    name
                });
                if (Manager.Get(Site, fullName) != null)
                {
                    return Json("The name already exists.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult IsIdentifierAvailable(string fullName, PageRoute route)
        {
            if (route != null && !string.IsNullOrEmpty(route.Identifier))
            {
                var page = Kooboo.CMS.Sites.Persistence.Providers.PageProvider.GetPageByUrlIdentifier(Site, route.Identifier);
                if (page != null)
                {
                    if (string.IsNullOrEmpty(fullName) || !page.FullName.EqualsOrNullEmpty(fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Json("The page url identifier already exists.", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region COPY PAGE
        public ActionResult CopyPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CopyPage(string sourcePage, string parentPage, string name)
        {
            JsonResultEntry entry = new JsonResultEntry();

            var destPage = PageHelper.CombineFullName(new string[] { parentPage, name });
            try
            {
                Manager.Copy(Site, sourcePage, destPage);
                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }
            return Json(entry);
        }
        #endregion

        #region Move Page
        public ActionResult MovePage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MovePage(string fullName, string parentPage)
        {
            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                Manager.Move(Site, fullName, parentPage);
                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }
            return Json(entry);
        }
        #endregion

        #region Page Selector
        public ActionResult PageSelector()
        {
            return View();
        }
        #endregion

        #region Publish
        public ActionResult Publish(string fullName)
        {
            PagePublishViewModel model = new PagePublishViewModel() { FullName = fullName, PublishDate = DateTime.Now, OfflineDate = DateTime.Now, PublishTime = "00:00", OfflineTime = "00:00" };
            return View(model);
        }
        [HttpPost]
        public ActionResult Publish(PagePublishViewModel model)
        {
            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                if (ModelState.IsValid)
                {
                    var page = new Page(Site, model.FullName);
                    var publishDate = DateTime.Parse(model.PublishDate.ToShortDateString() + " " + model.PublishTime);
                    var offlineDate = DateTime.Parse(model.OfflineDate.ToShortDateString() + " " + model.OfflineTime);
                    ServiceFactory.PageManager.Publish(page, model.PublishSchedule, model.PublishDraft, model.Period, publishDate, offlineDate, User.Identity.Name);
                }
                entry.RedirectToOpener = true;
                entry.RedirectUrl = GetReturnUrl();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        [HttpPost]
        public ActionResult Offline(string fullName)
        {
            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                if (ModelState.IsValid)
                {
                    var page = new Page(Site, fullName);
                    ServiceFactory.PageManager.Offline(page, User.Identity.Name);
                }
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        #endregion

        #region Version
        public ActionResult Version(string fullName)
        {
            var page = new Page(Site, fullName);
            var model = Manager.VersiongLogger.AllVersions(page);
            return View(model);
        }

        public ActionResult Revert(string fullName, int version)
        {
            var entry = new JsonResultEntry();
            var page = new Page(Site, fullName);

            try
            {
                Manager.VersiongLogger.Revert(page, version);
                entry.SetSuccess().AddMessage("Revert Successfully.");
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }

        public ActionResult PreviewVersion(string fullName, int version)
        {

            if (!string.IsNullOrWhiteSpace(Site.Repository))
            {
                GenerateFolderListDicViewData();
            }
            var page = new Page(Site, fullName);
            var model = Manager.VersiongLogger.GetVersion(page, version);
            return View(model);
        }
        #endregion
    }
}
