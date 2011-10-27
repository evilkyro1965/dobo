using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.Web.Url;
using Kooboo.CMS.Web.Areas.Contents.Models;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Models.Binder;
using Kooboo.Web.Mvc.Paging;
using Kooboo.CMS.Content.Query.Expressions;
using System.Collections.Specialized;
using Kooboo.Extensions;


using Kooboo.Web.Mvc;
using Kooboo.Web.Script.Serialization;
using Kooboo.CMS.Content.Versioning;
using DiffPlex;
using DiffPlex.DiffBuilder;
using System.Text;
using Kooboo.CMS.Web.Models;
using System.Diagnostics;
using Kooboo.CMS.Sites;
using System.IO;

using Kooboo.Globalization;
using Kooboo.CMS.Sites.DataRule;
namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{

    [ValidateInput(false)]
    public class TextContentController : ContentControllerBase
    {
        private class CategoryValue
        {
            public string CategoryFolderName { get; set; }
            public string Value { get; set; }
            public string OldValue { get; set; }
        }
        public class ContentSorter
        {
            public string UUID { get; set; }
            public int Sequence { get; set; }
        }


        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        public virtual ActionResult Index(string folderName, string parentUUID, string parentFolder, string search
            , IEnumerable<WhereClause> whereClause, int? page, int? pageSize)
        {
            //compatible with the Folder parameter changed to FolderName.
            folderName = folderName ?? this.ControllerContext.RequestContext.GetRequestValue("Folder");

            TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
            var schema = textFolder.GetSchema().AsActual();

            SchemaPath schemaPath = new SchemaPath(schema);
            ViewData["Folder"] = textFolder;
            ViewData["Schema"] = schema;
            ViewData["SchemaView"] = schemaPath.GridVirtualPath;
            ViewData["WhereClause"] = whereClause;

            SetPermissionData(textFolder);

            IEnumerable<TextFolder> childFolders = new TextFolder[0];
            if (!page.HasValue || page.Value <= 1)
            {
                childFolders = ServiceFactory.TextFolderManager.ChildFolders(textFolder, search).Select(it => it.AsActual());
            }
            IContentQuery<TextContent> query = textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null));
            if (!string.IsNullOrWhiteSpace(parentUUID))
            {
                query = query.WhereEquals("ParentUUID", parentUUID);
            }
            if (!string.IsNullOrEmpty(search))
            {
                IWhereExpression exp = new FalseExpression();
                foreach (var item in schema.Columns.Where(it => it.ShowInGrid))
                {
                    exp = new OrElseExpression(exp, (new WhereContainsExpression(null, item.Name, search)));
                }
                if (exp != null)
                {
                    query = query.Where(exp);
                }

            }
            if (whereClause != null && whereClause.Count() > 0)
            {
                var expression = WhereClauseToContentQueryHelper.Parse(whereClause, new MVCValueProviderWrapper(ValueProvider));
                query = query.Where(expression);
            }
            if (childFolders != null)
            {
                childFolders = childFolders.Where(it => Kooboo.CMS.Content.Services.ServiceFactory.WorkflowManager.AvailableViewContent(it, User.Identity.Name));
            }
            page = page ?? 1;
            pageSize = pageSize ?? 50;

            var pagedList = query.ToPageList(page.Value, pageSize.Value);

            IEnumerable<TextContent> contents = pagedList.ToArray();

            if (Repository.EnableWorkflow == true)
            {
                contents = ServiceFactory.WorkflowManager.GetPendWorkflowItemForContents(Repository, contents.ToArray(), User.Identity.Name);
            }

            var workflowContentPagedList = new PagedList<TextContent>(contents, page.Value, pageSize.Value, pagedList.TotalItemCount);
            ViewData["ContentPagedList"] = workflowContentPagedList;
            return View(new TextContentGrid()
            {
                ChildFolders = childFolders.ToArray(),
                Contents = workflowContentPagedList
            });
        }

        public virtual ActionResult SubContent(string parentFolder, string folderName, string parentUUID, IEnumerable<WhereClause> whereClause, string search, int? page, int? pageSize)
        {
            return Index(folderName, parentUUID, parentFolder, search, whereClause, page, pageSize);
        }
        public virtual ActionResult Selectable(string categoryFolder, int? page, int? pageSize)
        {
            var textFolder = (TextFolder)(FolderHelper.Parse<TextFolder>(Repository, categoryFolder).AsActual());
            Schema schema = new Schema(Repository, textFolder.SchemaName).AsActual();
            SchemaPath schemaPath = new SchemaPath(schema);
            ViewData["Folder"] = textFolder;
            ViewData["Schema"] = schema;
            ViewData["SchemaView"] = schemaPath.SelectableVirtualPath;

            return View(textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null)).ToPageList(page ?? 1, pageSize ?? 50));
        }
        #region CUD

        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpGet]
        public virtual ActionResult Create(string folderName, string parentFolder)
        {
            TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
            var schema = textFolder.GetSchema().AsActual();

            SchemaPath schemaPath = new SchemaPath(schema);
            ViewData["Folder"] = textFolder;
            ViewData["Schema"] = schema;
            ViewData["SchemaView"] = schemaPath.CreateVirutalPath;
            SetPermissionData(textFolder);

            var content = schema.DefaultContent();
            content = Kooboo.CMS.Content.Models.Binder.TextContentBinder.DefaultBinder.Bind(schema, content, Request.QueryString, true, false);

            return View(content);
        }

        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpPost]
        public virtual ActionResult Create(string folderName, string parentFolder, string parentUUID, FormCollection form)
        {
            JsonResultEntry resultEntry = new JsonResultEntry() { Success = true };
            try
            {
                if (ModelState.IsValid)
                {
                    TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                    var schema = textFolder.GetSchema().AsActual();

                    SchemaPath schemaPath = new SchemaPath(schema);
                    IEnumerable<TextContent> addedCategories;
                    IEnumerable<TextContent> removedCategories;

                    ParseCategories(form, out addedCategories, out removedCategories);
                    ContentBase content;

                    content = ServiceFactory.TextContentManager.Add(Repository, textFolder, parentFolder, parentUUID, form, Request.Files, addedCategories, User.Identity.Name);

                    resultEntry.Success = true;
                }
            }
            catch (RuleViolationException ruleEx)
            {
                foreach (var item in ruleEx.Issues)
                {
                    resultEntry.AddFieldError(item.PropertyName, item.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                resultEntry.SetFailed().AddException(e);
            }
            resultEntry.AddModelState(ViewData.ModelState);
            return Json(resultEntry);


        }


        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpGet]
        public virtual ActionResult Edit(string folderName, string parentFolder, string uuid)
        {
            TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
            var schema = textFolder.GetSchema().AsActual();

            SchemaPath schemaPath = new SchemaPath(schema);
            ViewData["Folder"] = textFolder;
            ViewData["Schema"] = schema;
            ViewData["SchemaView"] = schemaPath.UpdateVirtualPath;
            SetPermissionData(textFolder);
            var content = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            if (content != null)
            {
                content = ServiceFactory.WorkflowManager.GetPendWorkflowItemForContent(Repository, content, User.Identity.Name);
            }
            return View(content);
        }


        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpPost]
        public virtual ActionResult Edit(string folderName, string parentFolder, string uuid, FormCollection form, bool? localize)
        {
            JsonResultEntry resultEntry = new JsonResultEntry() { Success = true };
            try
            {
                if (ModelState.IsValid)
                {
                    TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                    var schema = textFolder.GetSchema().AsActual();

                    SchemaPath schemaPath = new SchemaPath(schema);
                    IEnumerable<TextContent> addedCategories;
                    IEnumerable<TextContent> removedCategories;

                    ParseCategories(form, out addedCategories, out removedCategories);
                    ContentBase content;
                    //if (textFolder != null)
                    //{
                    content = ServiceFactory.TextContentManager.Update(Repository, textFolder, uuid, form,
                    Request.Files, DateTime.UtcNow, addedCategories, removedCategories, User.Identity.Name);

                    if (localize.HasValue && localize.Value == true)
                    {
                        ServiceFactory.TextContentManager.Localize(textFolder, uuid);
                    }
                    //}
                    //else
                    //{
                    //    content = ServiceFactory.TextContentManager.Update(Repository, schema, uuid, form,
                    //    Request.Files, User.Identity.Name);
                    //}

                    resultEntry.Success = true;
                }
            }
            catch (RuleViolationException violationException)
            {
                foreach (var item in violationException.Issues)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
                resultEntry.Success = false;
            }
            catch (Exception e)
            {
                resultEntry.SetFailed().AddException(e);
            }
            resultEntry.AddModelState(ViewData.ModelState);
            return Json(resultEntry);
        }
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        public virtual ActionResult Delete(string folderName, string parentFolder, string[] docArr, string[] folderArr)
        {
            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                var schema = textFolder.GetSchema().AsActual();

                if (docArr != null)
                {
                    foreach (var doc in docArr)
                    {
                        if (string.IsNullOrEmpty(doc)) continue;

                        ServiceFactory.TextContentManager.Delete(Repository, textFolder, doc);
                    }

                }

                if (folderArr != null)
                {
                    foreach (var f in folderArr)
                    {
                        if (string.IsNullOrEmpty(f)) continue;
                        var folderPathArr = FolderHelper.SplitFullName(f);
                        ServiceFactory.TextFolderManager.Remove(Repository, new TextFolder(Repository, folderPathArr));
                    }
                }


                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }
            return Json(entry);

        }

        private void ParseCategories(FormCollection form, out IEnumerable<TextContent> addedCategory, out IEnumerable<TextContent> removedCategory)
        {
            List<TextContent> added = new List<TextContent>();
            List<TextContent> removed = new List<TextContent>();
            foreach (var item in GetCategories(form))
            {
                var categoryFolder = new TextFolder(Repository, FolderHelper.SplitFullName(item.CategoryFolderName));
                string[] newValues = item.Value.Split(',');
                string[] oldValues = item.OldValue.Split(',');

                var addValues = newValues.Except(oldValues, StringComparer.CurrentCultureIgnoreCase);
                var removedValues = oldValues.Except(newValues, StringComparer.CurrentCultureIgnoreCase);

                added.AddRange(addValues.Select(v => new TextContent(Repository.Name, "", item.CategoryFolderName) { UUID = v }));
                removed.AddRange(removedValues.Select(v => new TextContent(Repository.Name, "", item.CategoryFolderName) { UUID = v }));

                //ServiceFactory.TextContentManager.AddCategories(Repository, content, addValues.Select(v => new TextContent(Repository.Name, "", item.CategoryFolderName) { UUID = v }).ToArray());
                //ServiceFactory.TextContentManager.RemoveCategories(Repository, content, removedValues.Select(v => new TextContent(Repository.Name, "", item.CategoryFolderName) { UUID = v }).ToArray());
            }
            addedCategory = added;
            removedCategory = removed;
        }
        private IEnumerable<CategoryValue> GetCategories(FormCollection form)
        {
            foreach (var key in form.AllKeys)
            {
                if (key.StartsWith("cat_") && key.EndsWith("_value"))
                {
                    var category = key.Substring(4, key.Length - 10);
                    var oldKey = key + "_old";
                    yield return new CategoryValue() { CategoryFolderName = category, Value = form[key], OldValue = form[oldKey] };
                }
            }
        }


        private void SetPermissionData(TextFolder folder)
        {
            var workflowManager = Kooboo.CMS.Content.Services.ServiceFactory.WorkflowManager;

            ViewData["AllowedEdit"] = workflowManager.AvailableToPublish(folder, User.Identity.Name);

            ViewData["AllowedView"] = workflowManager.AvailableViewContent(folder, User.Identity.Name);

        }
        #endregion

        public virtual ActionResult SelectCategories(string folderName, string selected, int? page, int? pageSize, string search, IEnumerable<WhereClause> whereClause)
        {
            var textFolder = (TextFolder)(FolderHelper.Parse<TextFolder>(Repository, folderName).AsActual());

            var singleChoice = string.Equals("True", Request.RequestContext.GetRequestValue("SingleChoice"), StringComparison.OrdinalIgnoreCase);

            Schema schema = new Schema(Repository, textFolder.SchemaName).AsActual();
            SchemaPath schemaPath = new SchemaPath(schema);
            ViewData["Folder"] = textFolder;
            ViewData["Schema"] = schema;
            ViewData["SchemaView"] = schemaPath.SelectableVirtualPath;
            ViewData["WhereClause"] = whereClause;


            var query = textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null));

            if (!string.IsNullOrEmpty(search))
            {
                IWhereExpression exp = new FalseExpression();
                foreach (var item in schema.Columns.Where(it => it.ShowInGrid))
                {
                    exp = new OrElseExpression(exp, (new WhereContainsExpression(null, item.Name, search)));
                }
                if (exp != null)
                {
                    query = query.Where(exp);
                }
            }
            if (whereClause != null && whereClause.Count() > 0)
            {
                var expression = WhereClauseToContentQueryHelper.Parse(whereClause, new MVCValueProviderWrapper(ValueProvider));
                query = query.Where(expression);
            }

            var contents = query.ToPageList(page ?? 1, pageSize ?? 50);
            SelectableViewModel viewModel = new SelectableViewModel() { Contents = contents, SingleChoice = singleChoice };

            if (Request.IsAjaxRequest())
            {
                return PartialView("", viewModel);
            }
            else
            {
                IEnumerable<TextContent> selectedContents = new TextContent[0];
                if (!string.IsNullOrEmpty(selected))
                {
                    string[] selectedArr = selected.Split(',');
                    IContentQuery<TextContent> selectedQuery = textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null));
                    foreach (var userKey in selectedArr)
                    {
                        selectedQuery = selectedQuery.Or((IWhereExpression)textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null)).WhereEquals("UUID", userKey).Expression);
                    }

                    selectedContents = selectedQuery;
                }
                viewModel.Selected = selectedContents;
            }

            return View(viewModel);


        }
        public virtual ActionResult Categories(string uuid, string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return null;
            }

            return PartialView(ServiceFactory.TextContentManager.QueryCategories(Repository, folderName, uuid));
        }

        #region Version Diff

        public virtual ActionResult Versions(string folderName, string parentFolder, string uuid)
        {
            TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
            var schema = textFolder.GetSchema().AsActual();

            var textContent = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            var versions = VersionManager.AllVersionInfos(textContent);
            return View(versions);
        }
        public virtual ActionResult Diff(string folderName, string parentFolder, string uuid, int v1, int v2)
        {
            TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
            var schema = textFolder.GetSchema().AsActual();

            var textContent = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();


            var versions = VersionManager.AllVersionInfos(textContent);

            var version1 = versions.Where(o => o.Version == v1).FirstOrDefault();
            var version2 = versions.Where(o => o.Version == v2).FirstOrDefault();

            var sideBySideDiffBuilder = new SideBySideDiffBuilder(new Differ());

            var model = new TextContentDiffModel() { UUID = uuid, Version1Name = v1, Version2Name = v2 };


            foreach (var k in textContent.Keys)
            {
                var version1Text = version1.TextContent[k] != null ? version1.TextContent[k].ToString() : "";

                var version2Text = version2.TextContent[k] != null ? version2.TextContent[k].ToString() : "";

                var diffModel = sideBySideDiffBuilder.BuildDiffModel(version1Text, version2Text);

                model.Version1.Add(k, diffModel.OldText);
                model.Version2.Add(k, diffModel.NewText);

            }


            return View(model);
        }
        [HttpPost]
        public virtual ActionResult RevertTo(string folderName, string schemaName, string uuid, int version)
        {
            var entry = new JsonResultEntry();
            try
            {
                ServiceFactory.TextContentManager.RevertTo(Repository, folderName, schemaName, uuid, version, User.Identity.Name);
                entry.SetSuccess().AddMessage("Revert successfully.");
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }

        #endregion

        #region Publish
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpPost]
        public virtual ActionResult Publish(string folderName, string parentFolder, string uuid)
        {
            var entry = new JsonResultEntry();

            try
            {
                TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                var schema = textFolder.GetSchema().AsActual();

                TextContent textContent = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
                var published = (bool?)textContent["Published"];
                if (published.HasValue && published.Value == true)
                {
                    ServiceFactory.TextContentManager.Update(Repository, schema, uuid, new string[] { "Published" }, new object[] { false }, User.Identity.Name);
                }
                else
                {
                    ServiceFactory.TextContentManager.Update(Repository, schema, uuid, new string[] { "Published" }, new object[] { true }, User.Identity.Name);
                }


            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        #endregion

        #region Temporary Files

        public ActionResult TempFile(string sourceUrl, string previewUrl, int x = 0, int y = 0, int width = 0, int height = 0)
        {
            ViewData["SourceUrl"] = sourceUrl;
            ViewData["PreviewUrl"] = previewUrl;
            ViewData["CropParam"] = new
            {
                Url = sourceUrl,
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
            return View();
        }

        [HttpPost]
        public ActionResult TempFile()
        {
            var entry = new JsonResultEntry();

            try
            {

                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var postFile = Request.Files[0];
                    var repositoryPath = new RepositoryPath(Repository);
                    var tempPath = Kooboo.Web.Url.UrlUtility.Combine(repositoryPath.VirtualPath, "TempFiles");
                    Kooboo.IO.IOUtility.EnsureDirectoryExists(Server.MapPath(tempPath));

                    var fileUrl = Kooboo.Web.Url.UrlUtility.Combine(tempPath, Guid.NewGuid() + Path.GetFileName(postFile.FileName));

                    postFile.SaveAs(Server.MapPath(fileUrl));
                    entry.Model = Url.Content(fileUrl);
                }
                else
                {
                    entry.SetFailed().AddMessage("Please upload an image file.");
                }
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }



        #endregion

        #region Order

        public ActionResult Top(string folderName, string uuid)
        {
            JsonResultEntry result = new JsonResultEntry();
            try
            {
                ServiceFactory.TextContentManager.Top(Repository, folderName, uuid);
            }
            catch (Exception e)
            {
                result.AddException(e);
            }

            return Json(result);
        }

        [HttpPost]
        public ActionResult Sort(IEnumerable<ContentSorter> list, string folderName)
        {
            var entry = new JsonResultEntry();

            try
            {
                TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                var schema = textFolder.GetSchema().AsActual();
                foreach (var c in list)
                {
                    ServiceFactory.TextContentManager.Update(Repository, schema, c.UUID, new string[] { "Sequence" }, new object[] { c.Sequence }, User.Identity.Name);
                }

            }
            catch (Exception e)
            {

            }

            return Json(entry);
        }

        [HttpPost]
        public ActionResult CrossPageSort(ContentSorter sourceContent, string folderName, int? page, int? pageSize, bool up = true)
        {
            var entry = new JsonResultEntry();
            try
            {
                page = page ?? 1;
                pageSize = pageSize ?? 50;
                TextFolder textFolder = new TextFolder(Repository, folderName).AsActual();
                var schema = textFolder.GetSchema().AsActual();
                IContentQuery<TextContent> query = textFolder.CreateQuery().OrderBy(textFolder.DefaultOrderExpression(null));
                TextContent destContent;
                if (up)
                {
                    destContent = query.First();
                    if (destContent.UUID == sourceContent.UUID)
                    {
                        throw new Exception("This item is already the first one.".Localize());
                    }
                    destContent = query.Take((page.Value - 1) * pageSize.Value).Last();
                }
                else
                {
                    destContent = query.Last();
                    if (destContent.UUID == sourceContent.UUID)
                    {
                        throw new Exception("This item is already the last one.".Localize());
                    }
                    destContent = query.Skip(page.Value * pageSize.Value).First();
                }

                ServiceFactory.TextContentManager.Update(Repository, schema, sourceContent.UUID, new string[] { "Sequence" }, new object[] { destContent["Sequence"] }, User.Identity.Name);
                ServiceFactory.TextContentManager.Update(Repository, schema, destContent.UUID, new string[] { "Sequence" }, new object[] { sourceContent.Sequence }, User.Identity.Name);
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }
            return Json(entry);
        }

        #endregion

    }

    //public static class TextContentWorkflowVisible
    //{
    //    public static bool NeedProcess(this TextContent content, System.Web.Mvc.ControllerBase controllerBase)
    //    {
    //        var controller = (Controller)controllerBase;
    //        var Manager = ServiceFactory.WorkflowManager;

    //        var items = Manager.GetPendingWorkflowItems(Repository.Current, controller.User.Identity.Name);

    //        return items.Where(o => o.ContentUUID == content.UUID).Count() > 0;
    //    }

    //    public static PendingWorkflowItem NeedProcessItem(this TextContent content, System.Web.Mvc.ControllerBase controllerBase)
    //    {
    //        var controller = (Controller)controllerBase;
    //        var Manager = ServiceFactory.WorkflowManager;

    //        var items = Manager.GetPendingWorkflowItems(Repository.Current, controller.User.Identity.Name);

    //        return items.Where(o => o.ContentUUID == content.UUID).FirstOrDefault();
    //    }

    //    public static IEnumerable<PendingWorkflowItem> NeedProcessItems(this System.Web.Mvc.ControllerBase controllerBase)
    //    {
    //        var controller = (Controller)controllerBase;
    //        var Manager = ServiceFactory.WorkflowManager;

    //        var items = Manager.GetPendingWorkflowItems(Repository.Current, controller.User.Identity.Name);

    //        return items;
    //    }
    //}
}
