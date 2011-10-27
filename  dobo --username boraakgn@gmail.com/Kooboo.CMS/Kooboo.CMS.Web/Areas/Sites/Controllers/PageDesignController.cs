﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Collections.Specialized;

namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
    using Kooboo.Data;
    using Kooboo.Globalization;
    using Kooboo.Web.Mvc;
    using Kooboo.Web.Mvc.WebResourceLoader;
    using Kooboo.CMS.Web.Authorizations;
    using Kooboo.CMS.Sites.View;
    using Kooboo.CMS.Sites.Models;
    using Kooboo.CMS.Sites.Services;
    using Kooboo.CMS.Sites.DataRule;
    using Kooboo.CMS.Content.Query;
    using Kooboo.CMS.Sites.Extension.Module;

    [Authorization(AreaName = "Sites", Group = "Page", Name = "Edit", Order = 1)]
    public class PageDesignController : AdminControllerBase
    {
        public ActionResult Design(string pageName, string layout, bool? draft, int? version)
        {
            var page = new Page(Site, "DesignPage") { IsDummy = false, Layout = layout, EnableTheming = true, EnableScript = true };
            if (!string.IsNullOrEmpty(pageName))
            {
                page = PageHelper.Parse(Site, pageName);
            }

            if (draft.HasValue && draft.Value == true)
            {
                page = ServiceFactory.PageManager.PageProvider.GetDraft(page);
            }
            if (version.HasValue)
            {
                page = Kooboo.CMS.Sites.Versioning.VersionManager.GetVersion(page, version.Value);
            }

            // new context
            var requestContext = new PageRequestContext(this.ControllerContext, Site, page, CMS.Sites.Web.FrontRequestChannel.Design, string.Empty);

            // init context
            Page_Context.Current.InitContext(requestContext, ControllerContext);

            // ret
            return ViewPage();
        }
        private ActionResult ViewPage()
        {
            var layout = (new Layout(Site, Page_Context.Current.PageRequestContext.Page.Layout).LastVersion()).AsActual();

            ViewResult viewResult = new FrontViewResult(ControllerContext, layout.FileExtension.ToLower(), layout.TemplateFileVirutalPath);

            if (viewResult != null)
            {
                viewResult.ViewData = this.ViewData;
                viewResult.TempData = this.TempData;
            }

            return viewResult;
        }

        public ActionResult Settings()
        {
            var settings = jsSerializer.Serialize(new
            {
                #region urls
                processViewUrl = Url.Action("ProcessView", "PageDesign", new { siteName = Site.FullName }),
                processHtmlUrl = Url.Action("ProcessHtml", "PageDesign", new { siteName = Site.FullName }),
                processModuleUrl = Url.Action("ProcessModule", "PageDesign", new { siteName = Site.FullName }),
                processFolderUrl = Url.Action("ProcessFolder", "PageDesign", new { siteName = Site.FullName }),
                processHtmlBlockUrl = Url.Action("ProcessHtmlBlock", "PageDesign", new { siteName = Site.FullName }),
                virtualPath = Url.Content("~"),
                #endregion

                #region anchor
                positionAnchor_js = new
                {
                    addViewTitle = "Add a view".Localize(),
                    addModuleTitle = "Add a module".Localize(),
                    addFolderTitle = "Add a data folder".Localize(),
                    addHtmlTitle = "Add HTML".Localize(),
                    addHtmlBlockTitle = "Add a HTML block".Localize()
                },
                #endregion

                #region design
                design_js = new
                {
                    removeConfirm = "Are you sure you want to remove this item?".Localize(),
                    // add
                    addViewTitle = "Add a view".Localize(),
                    addHtmlTitle = "Add HTML".Localize(),
                    addModuleTitle = "Add a module".Localize(),
                    addFolderTitle = "Add a data folder".Localize(),
                    addHtmlBlockTitle = "Add a HTML block".Localize(),
                    // edit
                    editViewTitle = "Edit view".Localize(),
                    editHtmlTitle = "Edit HTML".Localize(),
                    editModuleTitle = "Edit module".Localize(),
                    editFolderTitle = "Edit data folder".Localize(),
                    editHtmlBlockTitle = "Edit HTML block".Localize(),
                    // btn
                    editBtnTitle = "Edit".Localize(),
                    removeBtnTitle = "Remove".Localize(),
                    expandBtnTitle = "Expand".Localize(),
                    closeBtnTitle = "Close".Localize(),
                    moveBtnTitle = "Move".Localize()
                }
                #endregion
            });

            // ret
            return JavaScript(string.Format("var __designer = {0};", settings));
        }

        #region static helpers

        public static JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

        private static T ParseJson<T>(string json)
        {
            var ret = default(T);
            if (!string.IsNullOrEmpty(json))
                ret = jsSerializer.Deserialize<T>(json);
            return ret;
        }

        private static T ParseEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        private static List<Parameter> ParseViewParameters(string clientJson)
        {
            var parameters = ParseJson<List<Parameter>>(clientJson);
            return ParseViewParameters(parameters);
        }

        private static List<Parameter> ParseViewParameters(List<Parameter> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var p in parameters)
                {
                    if (p.Value == null)
                        continue;

                    // reset
                    var o = p.Value;
                    p.Value = null;

                    // valid
                    var arr = o as string[];
                    var strval = (arr != null) ? arr.FirstOrDefault() : o.ToString();
                    if (string.IsNullOrEmpty(strval))
                        continue;

                    // parse
                    strval = HttpUtility.UrlDecode(strval);
                    if (p.DataType == DataType.DateTime) { strval = strval.Replace('+', ' '); }
                    p.Value = DataTypeHelper.ParseValue(p.DataType, strval, false);
                }
            }
            return parameters;
        }

        private static Entry ParseModuleEntry(string action, string controller)
        {
            if (string.IsNullOrWhiteSpace(action) ||
                string.IsNullOrWhiteSpace(controller))
            {
                return null;
            }

            return new Entry()
            {
                Action = action,
                Controller = controller
            };
        }

        // public method
        public static List<PagePosition> ParsePagePositions(string clientJson)
        {
            clientJson = HttpUtility.UrlDecode(clientJson);

            var positions = new List<PagePosition>();
            var list = ParseJson<List<Dictionary<string, object>>>(clientJson);

            foreach (var item in list)
            {
                PagePosition pos = null;
                var t = item.Str("Type");
                if (t == PageDesignViewContent.TypeKey)
                {
                    pos = new ViewPosition()
                    {
                        ViewName = item.Str("ViewName"),
                        Parameters = ParseViewParameters(item.Str("Parameters")),
                        OutputCache = ParseJson<CacheSettings>(item.Str("OutputCache"))
                    };
                }
                else if (t == PageDesignModuleContent.TypeKey)
                {
                    pos = new ModulePosition()
                    {
                        ModuleName = item.Str("ModuleName"),
                        Exclusive = (item.Str("Exclusive") == "true"),
                        Entry = ParseModuleEntry(item.Str("EntryAction"), item.Str("EntryController"))
                    };
                }
                else if (t == PageDesignFolderContent.TypeKey)
                {
                    pos = new ContentPosition()
                    {
                        Type = ParseEnum<ContentPositionType>(item.Str("ContentPositionType")),
                        DataRule = ParseJson<FolderDataRule>(item.Str("DataRule"))
                    };
                }
                else if (t == PageDesignHtmlContent.TypeKey)
                {
                    pos = new HtmlPosition()
                    {
                        Html = item.Str("Html")
                    };
                }
                else if (t == PageDesignHtmlBlockContent.TypeKey)
                {
                    pos = new HtmlBlockPosition()
                    {
                        BlockName = item.Str("BlockName")
                    };
                }
                // add
                if (pos != null)
                {
                    pos.LayoutPositionId = item.Str("LayoutPositionId");
                    pos.PagePositionId = item.Str("PagePositionId");
                    pos.Order = item.Int("Order");
                    positions.Add(pos);
                }
            }
            // ret
            return positions;
        }

        #endregion

        /****************** Design Pages ***********************/

        private bool IsGet()
        {
            ViewData["IsEdit"] = Request.QueryString["IsEdit"];
            return (Request.HttpMethod.ToUpper() == "GET");
        }

        public ActionResult ProcessView(ViewPosition pos)
        {
            if (IsGet())
            {
                var manager = ServiceFactory.GetService<ViewManager>();
                ViewData["Policys"] = Enum.GetNames(typeof(ExpirationPolicy));
                return View(manager.GetNamespace(Site));
            }
            else
            {
                pos.Parameters = ParseViewParameters(pos.Parameters);
                return Json(new { html = new PageDesignViewContent(pos).ToHtmlString() });
            }
        }

        public ActionResult ProcessHtml(HtmlPosition pos)
        {
            if (IsGet()) { return View(); }
            return Json(new { html = new PageDesignHtmlContent(pos).ToHtmlString() });
        }

        public ActionResult ProcessModule(ModulePosition pos)
        {
            if (IsGet())
            {
                var manager = ServiceFactory.GetService<ModuleManager>();
                var modules = manager.AllModulesForSite(Site.FullName);
                var model = new List<NameValueCollection>();
                foreach (var name in modules)
                {
                    var item = new NameValueCollection();
                    item.Add("ModuleName", name);
                    model.Add(item);
                    var setting = ModuleInfo.GetSiteModuleSettings(name, Site.FullName);
                    if (setting.Entry != null)
                    {
                        item.Add("EntryAction", setting.Entry.Action);
                        item.Add("EntryController", setting.Entry.Controller);
                    }
                    var info = ModuleInfo.Get(name);
                    if (info.EntryOptions != null)
                    {
                        var options = new List<object>();
                        foreach (var op in info.EntryOptions)
                        {
                            options.Add(new
                            {
                                Name = op.Name,
                                EntryAction = op.Entry.Action,
                                EntryController = op.Entry.Controller
                            });
                        }
                        var serializer = new JavaScriptSerializer();
                        var optionsJson = serializer.Serialize(options);
                        item.Add("EntryOptions", HttpUtility.HtmlAttributeEncode(optionsJson));
                    }
                }
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(pos.Entry.Action) ||
                string.IsNullOrWhiteSpace(pos.Entry.Controller))
            {
                pos.Entry = null;
            }
            return Json(new { html = new PageDesignModuleContent(pos).ToHtmlString() });
        }

        public ActionResult ProcessFolder()
        {
            var repository = PageController.GerSiteRepository(Site.Repository);
            var folderTree = Kooboo.CMS.Content.Services.ServiceFactory.TextFolderManager.FolderTrees(repository);
            ViewData["IsEdit"] = Request.QueryString["IsEdit"];
            //ViewData["FolderTypes"] = Enum.GetNames(typeof(ContentPositionType));
            ViewData["UserKeyQuery"] = Url.Action("QueryUserKey", "PageDesign", new { siteName = Site.FullName });
            return View(folderTree);
        }

        public ActionResult QueryUserKey(string folderName, string term)
        {
            var dataSource = new UserKeyDatasource();
            return Json(dataSource.GetSelectListItems(
                ControllerContext.RequestContext, term)
                    .Select(it => new { label = it.Text, category = it.Value }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProcessFolder(string PagePositionId, int Order, string Top, string Type, string UserKey)
        {
            var pos = new ContentPosition();
            pos.Type = ParseEnum<ContentPositionType>(Type);
            pos.PagePositionId = PagePositionId;
            pos.Order = Order;

            var dataRule = new FolderDataRule()
            {
                FolderName = Request.Form["DataRule.FolderName"],
                Top = Top
            };

            if (pos.Type == ContentPositionType.Detail)
            {
                dataRule.WhereClauses = new List<WhereClause>()
                {
                    new WhereClause()
                    {
                         FieldName ="UserKey",
                         Operator = Operator.Equals,
                         Value1 = string.IsNullOrEmpty(UserKey) ? "{UserKey}" : UserKey
                    }
                };
            }

            pos.DataRule = dataRule;

            return Json(new { html = new PageDesignFolderContent(pos).ToHtmlString() });
        }

        public ActionResult ProcessHtmlBlock(HtmlBlockPosition pos)
        {
            if (IsGet())
            {
                return View(ServiceFactory.HtmlBlockManager.All(this.Site, null));
            }
            else
            {
                var htmlBlock = pos.GetHtmlBlock(Site);
                return Json(new { html = new PageDesignHtmlBlockContent(pos, htmlBlock.Body).ToHtmlString() });
            }
        }
    }

    class UserKeyDatasource : ISelectListDataSource
    {
        #region ISelectListDataSource Members

        public IEnumerable<SelectListItem> GetSelectListItems(RequestContext requestContext, string filter = null)
        {
            string folderName = requestContext.GetRequestValue("folderName");
            var siteName = requestContext.GetRequestValue("siteName");
            var repository = new Site(siteName).GetRepository();
            var textFolder = Kooboo.CMS.Content.Models.FolderHelper.Parse<Kooboo.CMS.Content.Models.TextFolder>(repository, folderName);
            var contentQuery = textFolder.CreateQuery().WhereContains("UserKey", filter);
            return contentQuery.Select(it => new SelectListItem()
            {
                Text = it.UserKey,
                Value = it.UserKey
            }).ToList();
        }

        #endregion
    }

    static class DictionaryExtension
    {
        public static string Str(this Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                return dict[key].ToString();
            }
            else
            {
                return null;
            }
        }

        public static int Int(this Dictionary<string, object> dict, string key)
        {
            var ret = 0;
            int.TryParse(dict.Str(key), out ret);
            return ret;
        }

        public static T val<T>(this Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return (T)dict[key];
            }
            else
            {
                return default(T);
            }
        }
    }
}