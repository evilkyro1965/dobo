//# define Page_Trace
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.CMS.Sites.Caching;
using Kooboo.CMS.Sites.DataRule;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Parsers.ThemeRule;
using Kooboo.CMS.Sites.Services;
using Kooboo.CMS.Sites.Web;
using Kooboo.Globalization;
using Kooboo.Web;
using Kooboo.Web.Mvc;
using Kooboo.Web.Mvc.Html;
using Kooboo.Web.Mvc.Paging;
namespace Kooboo.CMS.Sites.View
{
    public class FrontHtmlHelper
    {
        public FrontHtmlHelper(Page_Context context, HtmlHelper html)
        {
            this.PageContext = context;
            this.Html = html;
        }

        public Page_Context PageContext { get; private set; }

        public HtmlHelper Html { get; private set; }

        #region Position
        public IHtmlString Position(string positionID)
        {
            if (PageContext.PageRequestContext.RequestChannel == FrontRequestChannel.Design)
            {
                return new PageDesignHolder(this, positionID);
            }
            else
            {
                var positions = PageContext.PageRequestContext.Page.PagePositions;
                if (positions == null || positions.Count == 0)
                {
                    return new HtmlString("");
                }
                var htmlStrings = RenderPositionContents(positions.Where(it => it.LayoutPositionId.Equals(positionID, StringComparison.InvariantCultureIgnoreCase)));
                return new AggregateHtmlString(htmlStrings);
            }
        }
        private IEnumerable<IHtmlString> RenderPositionContents(IEnumerable<PagePosition> positions)
        {
            foreach (var position in positions.OrderBy(it => it.Order))
            {
                Page_Context.Current.PositionContext = new PagePositionContext() { PagePosition = position };

                if (position is ViewPosition)
                {
                    Page_Context.Current.PositionContext.ViewData = Page_Context.Current.GetPositionViewData(position.PagePositionId);

                    yield return this.RenderView(((ViewPosition)position));
                }
                else if (position is ModulePosition)
                {
                    yield return new HtmlString(RenderModule((ModulePosition)position));
                }
                else if (position is HtmlPosition)
                {
                    yield return RenderHtmlPosition((HtmlPosition)position);
                }
                else if (position is ContentPosition)
                {
                    yield return RenderContentPosition((ContentPosition)position);
                }
                else if (position is HtmlBlockPosition)
                {
                    yield return RenderHtmlBlockPosition((HtmlBlockPosition)position);
                }
            }
        }
        private IHtmlString RenderHtmlBlockPosition(HtmlBlockPosition htmlBlockPosition)
        {
            return RenderHtmlBlock(htmlBlockPosition.BlockName);
        }
        private IHtmlString RenderHtmlPosition(HtmlPosition htmlPosition)
        {
            string html = htmlPosition.Html;
            if (PageContext.EnabledInlineEditing(EditingType.Page) && PageContext.PageRequestContext.Page.IsLocalized(PageContext.PageRequestContext.Site))
            {
                if (PageContext.PageRequestContext.Page.Published.Value == false
                    || (PageContext.PageRequestContext.Page.Published.Value == true && ServiceFactory.UserManager.Authorize(Kooboo.CMS.Sites.Models.Site.Current, PageContext.ControllerContext.HttpContext.User.Identity.Name, Kooboo.CMS.Account.Models.Permission.Sites_Page_PublishPermission)))
                {
                    html = string.Format("<var editType='html' positionId='{0}' positionName='{1}' style='display:none' start></var>{2}<var style='display:none' end></var>", htmlPosition.PagePositionId, HttpUtility.HtmlAttributeEncode(htmlPosition.ToString()), html);
                }
            }
            return new HtmlString(html);
        }
        internal IHtmlString RenderView(ViewPosition viewPosition)
        {
            string cacheKey = null;
            var site = PageContext.PageRequestContext.Site;
            if (viewPosition.EnabledCache)
            {
                cacheKey = GetOutputCacheKey(PageContext.ControllerContext.HttpContext, PageContext.PageRequestContext.Page, viewPosition);
                var outputCache = PageContext.PageRequestContext.Site.ObjectCache().Get(cacheKey);
                if (outputCache != null)
                {
                    return new HtmlString(outputCache.ToString());
                }
            }
            var htmlString = RenderView(viewPosition.ViewName, Page_Context.Current.GetPositionViewData(viewPosition.PagePositionId), false);

            if (!string.IsNullOrEmpty(cacheKey))
            {
                PageContext.PageRequestContext.Site.ObjectCache().Add(cacheKey, htmlString, viewPosition.OutputCache.ToCachePolicy());
            }

            return htmlString;
        }
        private static string GetOutputCacheKey(HttpContextBase httpContext, Page page, ViewPosition viewPosition)
        {
            var cacheKey = string.Format("View OutputCache - Full page name:{0};Raw request url:{1};PagePositionId:{2};ViewName:{3};LayoutPositionId:{4}"
                , page.FullName, httpContext.Request.RawUrl, viewPosition.PagePositionId, viewPosition.ViewName, viewPosition.LayoutPositionId);
            return cacheKey;
        }
        internal string RenderModule(ModulePosition modulePosition)
        {
            ModuleActionInvokedContext actionInvokedContext = GetModuleActionResult(modulePosition.PagePositionId);
            if (actionInvokedContext != null)
            {
                var actionResultExecuted = ModuleExecutor.ExecuteActionResult(actionInvokedContext);
                return actionResultExecuted.ResultHtml;
            }
            return string.Empty;
        }
        private ModuleActionInvokedContext GetModuleActionResult(string pagePositionId)
        {
            IDictionary<string, ModuleActionInvokedContext> moduleActionResults = this.PageContext.ModuleResults;
            if (moduleActionResults.ContainsKey(pagePositionId))
            {
                return moduleActionResults[pagePositionId];
            }
            return null;
        }

        internal IHtmlString RenderContentPosition(ContentPosition contentPosition)
        {
            var site = this.PageContext.PageRequestContext.Site;
            var repository = site.GetRepository();
            if (repository == null)
            {
                throw new KoobooException("The repository for site is null.");
            }
            var dataRule = contentPosition.DataRule;
            var dataRuleContext = new DataRuleContext(this.PageContext.PageRequestContext.Site,
                this.PageContext.PageRequestContext.Page) { ValueProvider = this.PageContext.PageRequestContext.GetValueProvider() };
            var contentQuery = dataRule.Execute(dataRuleContext);
            string viewPath = "";
            SchemaPath schemaPath = new SchemaPath(dataRule.GetSchema(repository));
            Object model = contentQuery;
            switch (contentPosition.Type)
            {
                case ContentPositionType.Detail:
                    viewPath = schemaPath.DetailVirtualPath;
                    model = contentQuery.FirstOrDefault();
                    break;
                case ContentPositionType.List:
                default:
                    viewPath = schemaPath.ListVirtualPath; ;
                    break;
            }
            return RenderViewInternal(this.Html, viewPath, null, model);
        }
        #endregion

        #region Partial Render

        #region Render htmlblock
        public IHtmlString RenderHtmlBlock(string htmlBlockName)
        {
            var htmlBlock = Kooboo.CMS.Sites.Models.IPersistableExtensions.AsActual((new HtmlBlock(PageContext.PageRequestContext.Site, htmlBlockName).LastVersion()));
            var htmlString = new HtmlString("");
            if (htmlBlock != null)
            {
                var body = htmlBlock.Body;
                if (PageContext.EnabledInlineEditing(EditingType.Page) && htmlBlock.IsLocalized(PageContext.PageRequestContext.Site))
                {
                    if (PageContext.PageRequestContext.Page.Published == false
                        || (PageContext.PageRequestContext.Page.Published == true && ServiceFactory.UserManager.Authorize(Kooboo.CMS.Sites.Models.Site.Current, PageContext.ControllerContext.HttpContext.User.Identity.Name, Kooboo.CMS.Account.Models.Permission.Sites_Page_PublishPermission)))
                    {
                        body = string.Format("<var editType='htmlBlock' positionId='{0}' positionName='{1}' blockName='{2}' style='display:none' start></var>{3}<var style='display:none' end></var>", "", "", HttpUtility.HtmlAttributeEncode(htmlBlock.Name), body);
                    }
                }
                htmlString = new HtmlString(body);
            }

            return htmlString;

        }
        #endregion
        #region RenderView
        public IHtmlString RenderView(string viewName, ViewDataDictionary viewData)
        {
            return RenderView(viewName, viewData, true);
        }

        public IHtmlString RenderView(string viewName, ViewDataDictionary viewData, bool executeDataRule)
        {
            Kooboo.CMS.Sites.Models.View view = Kooboo.CMS.Sites.Models.IPersistableExtensions.AsActual(new Kooboo.CMS.Sites.Models.View(this.PageContext.PageRequestContext.Site, viewName).LastVersion());

            if (executeDataRule)
            {
                view = Kooboo.CMS.Sites.Models.IPersistableExtensions.AsActual(view);
                viewData = new ViewDataDictionary(viewData);
                var pageRequestContext = this.PageContext.PageRequestContext;
                if (view.DataRules != null)
                {
                    var valueProvider = pageRequestContext.GetValueProvider();
                    valueProvider.Insert(0, new ViewParameterValueProvider(view.Parameters));
                    var dataRuleContext = new DataRuleContext(pageRequestContext.Site, pageRequestContext.Page) { ValueProvider = valueProvider };
                    DataRuleExecutor.Execute(viewData, dataRuleContext, view.DataRules);
                }
            }

            return RenderViewInternal(this.Html, view.TemplateFileVirutalPath, viewData, null);
        }

        internal static IHtmlString RenderViewInternal(HtmlHelper htmlHelper, string viewPath, ViewDataDictionary viewData, object model)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                throw new ArgumentException("", "viewPath");
            }
            ViewDataDictionary dictionary = null;
            if (model == null)
            {
                if (viewData == null)
                {
                    dictionary = new ViewDataDictionary(htmlHelper.ViewData);
                }
                else
                {
                    dictionary = new ViewDataDictionary(viewData);
                }
            }
            else if (viewData == null)
            {
                dictionary = new ViewDataDictionary(model);
            }
            else
            {
                dictionary = new ViewDataDictionary(viewData)
                {
                    Model = model
                };
            }
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);

            ViewContext viewContext = new ViewContext(htmlHelper.ViewContext, htmlHelper.ViewContext.View, dictionary, htmlHelper.ViewContext.TempData, writer);
            TemplateEngines.GetEngineByFileExtension(Path.GetExtension(viewPath)).CreateView(htmlHelper.ViewContext.Controller.ControllerContext, viewPath, null).Render(viewContext, writer);

            return new HtmlString(writer.ToString());
        }
        #endregion

        #region RenderModule
        public IHtmlString RenderModule(string moduleName, string moduleController, string moduleAction)
        {
            return RenderModule(moduleName, moduleController, moduleAction, null);
        }
        public IHtmlString RenderModule(string moduleName, string moduleController, string moduleAction, object routeValues)
        {
            var routeDictionary = new RouteValueDictionary();
            if (routeValues != null)
            {
                routeDictionary = new RouteValueDictionary(routeValues);
            }
            ModulePosition position = new ModulePosition()
            {
                Exclusive = false
                ,
                PagePositionId = moduleName
                ,
                ModuleName = moduleName
                ,
                Entry = new Entry() { Controller = moduleController, Action = moduleAction, Values = routeDictionary }
            };
            return RenderModule(moduleName, null, position);
        }
        public IHtmlString RenderModule(string moduleName, string moduleUrl)
        {
            ModulePosition position = new ModulePosition()
            {
                Exclusive = false
                ,
                PagePositionId = moduleName
                ,
                ModuleName = moduleName
                ,
                Entry = new Entry()
            };
            return RenderModule(moduleName, moduleUrl, position);
        }
        public IHtmlString RenderModule(string moduleName, string moduleUrl, ModulePosition modulePosition)
        {
            var result = ModuleExecutor.InvokeAction(PageContext.ControllerContext, PageContext.PageRequestContext.Site, moduleUrl, modulePosition);
            var actionResultExecuted = ModuleExecutor.ExecuteActionResult(result);
            return new HtmlString(actionResultExecuted.ResultHtml);
        }
        #endregion

        //public static IHtmlString RenderStaticContent(this FrontHtmlHelper frontHtml, string contentName)
        //{
        //    return null;
        //}
        #endregion

        #region Register scripts & styles
        /// <summary>
        /// Includes a script file to current page.
        /// </summary>    
        /// <param name="script">The script.</param>
        public void IncludeScript(string script)
        {
            this.PageContext.Scripts.Add(this.Html.Script(script));
        }
        /// <summary>
        /// Includes the stylesheet.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="style">The style.</param>
        public void IncludeStylesheet(string style)
        {
            this.PageContext.Styles.Add(this.Html.Stylesheet(style));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="media"></param>
        public void IncludeStylesheet(string style, string media)
        {
            this.PageContext.Styles.Add(this.Html.Stylesheet(style, media));
        }
        /// <summary>
        /// Registers the scripts to the view.
        /// </summary>
        /// <returns></returns>
        public IHtmlString RegisterScripts()
        {
            return new AggregateHtmlString(this.IncludeSiteScripts()
                .Concat(this.PageContext.Scripts)
                .Concat(this.IncludeModulesScripts())
                .Concat(this.IncludeInlineEditScripts())
                .Distinct(new IHtmlStringComparer()));
        }
        private IEnumerable<IHtmlString> IncludeInlineEditScripts()
        {
            if (PageContext.PageRequestContext.RequestChannel != FrontRequestChannel.Design)
            {
                if (Page_Context.Current.InlineEditing)
                {
                    yield return InlineEditVariablesScript();
                    yield return Kooboo.Web.Mvc.WebResourceLoader.MvcExtensions.ExternalResources(this.Html, "Sites", "inlinejs", null);
                }
            }
        }
        private IHtmlString InlineEditVariablesScript()
        {
            return this.Html.Script(PageContext.Url.Action("Variables"
                    , new
                    {
                        controller = "InlineEditing",
                        repositoryName = Repository.Current == null ? "" : Repository.Current.Name,
                        siteName = Site.Current.FullName,
                        pageName = PageContext.PageRequestContext.Page.FullName,
                        area = "Sites",
                        _draft_ = PageContext.ControllerContext.RequestContext.GetRequestValue("_draft_")
                    }));
        }

        private IEnumerable<IHtmlString> IncludeSiteScripts()
        {
            var site = this.PageContext.PageRequestContext.Site;
            if (this.PageContext.PageRequestContext.Page.EnableScript)
            {
                if (this.PageContext.PageRequestContext.Site.EnableJquery)
                {
                    yield return Kooboo.Web.Mvc.WebResourceLoader.MvcExtensions.ExternalResources(this.Html, null, "jQuery", null);
                }

                var scripts = Persistence.Providers.ScriptsProvider.All(site);
                if (scripts != null && scripts.Count() > 0)
                {
                    if (site.Mode == ReleaseMode.Debug)
                    {
                        foreach (var script in scripts)
                        {
                            yield return this.Html.Script(script.VirtualPath);
                        }
                    }
                    else
                    {
                        yield return this.Html.Script(this.PageContext.FrontUrl.SiteScriptsUrl().ToString());
                    }
                }
            }
        }
        private IEnumerable<IHtmlString> IncludeModulesScripts()
        {
            var site = this.PageContext.PageRequestContext.Site;
            if (this.PageContext.PageRequestContext.Page.EnableScript)
            {
                if (Page_Context.Current.ModuleResults != null)
                {
                    if (site.Mode == ReleaseMode.Debug)
                    {
                        foreach (ModuleActionInvokedContext actionInvoked in Page_Context.Current.ModuleResults.Values)
                        {
                            ModuleControllerBase moduleController = (ModuleControllerBase)actionInvoked.ControllerContext.Controller;
                            foreach (var script in ServiceFactory.ModuleManager.AllScripts(moduleController.ModuleContext.ModuleName))
                            {
                                yield return this.Html.Script(script.VirtualPath);
                            }
                        }
                    }
                    else
                    {
                        foreach (ModuleActionInvokedContext actionInvoked in Page_Context.Current.ModuleResults.Values)
                        {
                            ModuleControllerBase moduleController = (ModuleControllerBase)actionInvoked.ControllerContext.Controller;
                            yield return this.Html.Script(this.PageContext.FrontUrl.ModuleScriptsUrl(moduleController.ModuleContext.ModuleName).ToString());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Registers the styles to the view.
        /// </summary>     
        /// <returns></returns>
        public IHtmlString RegisterStyles()
        {
            return new AggregateHtmlString(this.IncludeThemeStyles()
                .Concat(this.PageContext.Styles)
                .Concat(this.IncludeModuleThemeStyles())
                .Concat(this.IncludeInlineEditStyles())
                .Distinct(new IHtmlStringComparer()));
        }
        private IEnumerable<IHtmlString> IncludeInlineEditStyles()
        {
            if (PageContext.PageRequestContext.RequestChannel != FrontRequestChannel.Design)
            {
                if (Page_Context.Current.InlineEditing)
                {
                    yield return Kooboo.Web.Mvc.WebResourceLoader.MvcExtensions.ExternalResources(this.Html, "Sites", "inlinecss", null);
                }
            }
        }

        private IEnumerable<IHtmlString> IncludeThemeStyles()
        {
            var site = this.PageContext.PageRequestContext.Site;
            if (!string.IsNullOrEmpty(site.Theme) && this.PageContext.PageRequestContext.Page.EnableTheming)
            {
                if (this.PageContext.PageRequestContext.Site.EnableJquery)
                {
                    yield return Kooboo.Web.Mvc.WebResourceLoader.MvcExtensions.ExternalResources(this.Html, null, "jQuery-Styles", null);
                }

                string themeRuleBody;

                var styles = ThemeRuleParser.Parse(new Theme(site, site.Theme).LastVersion(), out themeRuleBody);

                if (styles != null && styles.Count() > 0)
                {
                    if (site.Mode == ReleaseMode.Debug)
                    {
                        foreach (var style in styles)
                        {
                            yield return this.Html.Stylesheet(style.VirtualPath);
                        }
                    }
                    else
                    {
                        yield return this.Html.Stylesheet(this.PageContext.FrontUrl.SiteThemeUrl().ToString());
                    }
                }

                yield return new HtmlString(themeRuleBody);
            }
        }
        private IEnumerable<IHtmlString> IncludeModuleThemeStyles()
        {
            var site = this.PageContext.PageRequestContext.Site;
            if (this.PageContext.PageRequestContext.Page.EnableTheming)
            {
                if (Page_Context.Current.ModuleResults != null)
                {
                    foreach (ModuleActionInvokedContext actionInvoked in Page_Context.Current.ModuleResults.Values)
                    {
                        ModuleControllerBase moduleController = (ModuleControllerBase)actionInvoked.ControllerContext.Controller;
                        if (moduleController.ModuleContext.ModuleSettings != null && !string.IsNullOrEmpty(moduleController.ModuleContext.ModuleSettings.ThemeName))
                        {
                            string themeRuleBody;
                            var styles = ServiceFactory.ModuleManager.AllThemeFiles(moduleController.ModuleContext.ModuleName,
                                    moduleController.ModuleContext.ModuleSettings.ThemeName, out themeRuleBody);

                            if (site.Mode == ReleaseMode.Debug)
                            {
                                foreach (var style in styles)
                                {
                                    yield return this.Html.Stylesheet(style.VirtualPath);
                                }
                            }
                            else
                            {
                                yield return this.Html.Script(this.PageContext.FrontUrl.
                                    ModuleThemeUrl(moduleController.ModuleContext.ModuleName, moduleController.ModuleContext.ModuleSettings.ThemeName)
                                    .ToString());
                            }
                            yield return new HtmlString(themeRuleBody);
                        }
                    }
                }
            }
        }
        #endregion

        #region Title & meta
        [Obsolete("Use HtmlTitle")]
        public IHtmlString Title()
        {
            return HtmlTitle();
        }
        public IHtmlString HtmlTitle()
        {
            if (!string.IsNullOrEmpty(this.PageContext.HtmlMeta.HtmlTitle))
            {
                return new HtmlString(string.Format("<title>{0}</title>", Kooboo.Extensions.StringExtensions.StripAllTags(this.PageContext.HtmlMeta.HtmlTitle)));
            }
            return new HtmlString("");
        }
        public IHtmlString Meta()
        {
            AggregateHtmlString htmlStrings = new AggregateHtmlString();
            var htmlMeta = this.PageContext.HtmlMeta;
            if (htmlMeta != null)
            {
                if (!string.IsNullOrEmpty(htmlMeta.Author))
                {
                    htmlStrings.Add(BuildMeta("author", htmlMeta.Author));
                }

                if (!string.IsNullOrEmpty(htmlMeta.Description))
                {
                    htmlStrings.Add(BuildMeta("description", htmlMeta.Description));
                }

                if (!string.IsNullOrEmpty(htmlMeta.Keywords))
                {
                    htmlStrings.Add(BuildMeta("keywords", htmlMeta.Keywords));
                }
                foreach (var item in htmlMeta.Customs)
                {
                    htmlStrings.Add(BuildMeta(item.Key, item.Value));
                }
            }
            return htmlStrings;
        }
        private static IHtmlString BuildMeta(string name, string value)
        {
            return new HtmlString(string.Format("<meta name=\"{0}\" content=\"{1}\" />", name, Kooboo.Extensions.StringExtensions.StripAllTags(value)));
        }
        #endregion

        #region PageLink

        public IHtmlString PageLink(object linkText, string urlMapKey)
        {
            return this.PageLink(linkText, urlMapKey, null);
        }
        public IHtmlString PageLink(object linkText, string urlMapKey, object routeValues)
        {
            return this.PageLink(linkText, urlMapKey, routeValues, null);
        }
        public IHtmlString PageLink(object linkText, string urlMapKey, object routeValues, object htmlAttributes)
        {
#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            var htmlValues = RouteValuesHelpers.GetRouteValues(htmlAttributes);
            string url = this.PageContext.FrontUrl.PageUrl(urlMapKey, routeValues).ToString();


            TagBuilder builder = new TagBuilder("a")
            {
                InnerHtml = linkText == null ? string.Empty : HttpUtility.HtmlEncode(linkText)
            };
            builder.MergeAttributes<string, object>(htmlValues);
            builder.MergeAttribute("href", url);
            var html = new HtmlString(builder.ToString(TagRenderMode.Normal));

#if Page_Trace
            stopwatch.Stop();
            HttpContext.Current.Response.Write(string.Format("PageLink,{0}.</br>", stopwatch.Elapsed));
#endif
            return html;
        }
        #endregion

        #region ViewLink
        public IHtmlString ViewLink(object linkText, string viewName)
        {
            return this.ViewLink(linkText, viewName, null);
        }
        public IHtmlString ViewLink(object linkText, string viewName, object routeValues)
        {
            return this.ViewLink(linkText, viewName, routeValues, null);
        }
        public IHtmlString ViewLink(object linkText, string viewName, object routeValues, object htmlAttributes)
        {
            var htmlValues = RouteValuesHelpers.GetRouteValues(htmlAttributes);

            string url = this.PageContext.FrontUrl.ViewUrl(viewName, routeValues).ToString();


            TagBuilder builder = new TagBuilder("a")
            {
                InnerHtml = linkText != null ? HttpUtility.HtmlEncode(linkText) : string.Empty
            };

            builder.MergeAttributes<string, object>(htmlValues);
            builder.MergeAttribute("href", url);
            var html = new HtmlString(builder.ToString(TagRenderMode.Normal));

            return html;
        }
        #endregion

        #region Pager
        internal class CmsPagerBuilder : PagerBuilder
        {
            private RouteValueDictionary routeValues;
            private IPagedList _pageList = null;
            private readonly PagerOptions _pagerOptions;
            private FrontHtmlHelper frontHtml;
            internal CmsPagerBuilder(FrontHtmlHelper html, IPagedList pageList,
                PagerOptions pagerOptions, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
                : base(html.Html, null, null, pageList, pagerOptions, null, routeValues, htmlAttributes)
            {
                frontHtml = html;
                this.routeValues = routeValues;


                _pageList = pageList;
                this._pagerOptions = pagerOptions;
            }
            protected override string GenerateUrl(int pageIndex)
            {
                //return null if  page index larger than total page count or page index is current page index
                if (pageIndex > _pageList.TotalPageCount || pageIndex == _pageList.CurrentPageIndex)
                    return null;

                var routeValues = frontHtml.PageContext.PageRequestContext.AllQueryString.ToRouteValues()
                    .Merge(_pagerOptions.PageIndexParameterName, pageIndex);
                return frontHtml.PageContext.Url.FrontUrl()
                    .GeneratePageUrl(frontHtml.PageContext.PageRequestContext.Page, routeValues).ToString();
            }
        }
        public IHtmlString Pager(object model)
        {
            return Pager(model, null);
        }
        public IHtmlString Pager(object model, PagerOptions options)
        {
            return Pager(model, null, options, null);
        }
        //Optional parameter does not support NVelocity
        public IHtmlString Pager(object model, object routeValues, PagerOptions options, object htmlAttributes)
        {
            options = options ?? new PagerOptions();

            if ((model is DataRulePagedList))
            {
                options.PageIndexParameterName = ((DataRulePagedList)model).PageIndexParameterName;
            }

            var pagedList = (IPagedList)model;

            var builder = new CmsPagerBuilder
             (
                 this,
                 pagedList,
                 options,
                 RouteValuesHelpers.GetRouteValues(routeValues),
                 RouteValuesHelpers.GetRouteValues(htmlAttributes)
             );
            return new HtmlString(builder.RenderPager().ToString());
        }
        #endregion

        #region ResizeImage
        public IHtmlString ResizeImage(string imagePath, int width, int height)
        {
            return ResizeImage(imagePath, width, height, null, null, null);
        }
        public IHtmlString ResizeImage(string imagePath, int width, int height, string alt)
        {
            return ResizeImage(imagePath, width, height, null, null, alt);
        }
        public IHtmlString ResizeImage(string imagePath, int width, int height, bool? preserverAspectRatio, int? quality, string alt)
        {
            return new HtmlString(string.Format("<img src='{0}' alt='{1}' />"
                , this.PageContext.FrontUrl.ResizeImageUrl(imagePath, width, height, preserverAspectRatio, quality)
                , alt));
        }
        #endregion

    }
}
