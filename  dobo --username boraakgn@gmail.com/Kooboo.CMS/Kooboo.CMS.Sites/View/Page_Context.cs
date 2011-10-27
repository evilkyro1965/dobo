using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Web.Mvc;
using System.Web;
using Kooboo.CMS.Sites.Extension;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites.DataRule;
using System.Text.RegularExpressions;
using Kooboo.Dynamic;
using Kooboo.Reflection;
using Kooboo.Web.Mvc;
using Kooboo.CMS.Sites.Services;
namespace Kooboo.CMS.Sites.View
{
    public class Page_Context
    {
        public static Page_Context Current
        {
            get
            {
                if (CallContext.Current.GetObject<Page_Context>() == null)
                {
                    CallContext.Current.RegisterObject<Page_Context>(new Page_Context());
                }
                return CallContext.Current.GetObject<Page_Context>();
            }
        }
        protected Page_Context()
        {
            Styles = new List<IHtmlString>();
            Scripts = new List<IHtmlString>();
        }

        #region Page position context && view parameters
        public PagePositionContext PositionContext { get; internal set; }
        #region wrapping position parameters
        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return PositionContext.Parameters;
            }
        }
        public dynamic this[string parameterName]
        {
            get
            {
                var parameter = Parameters.Where(it => it.Name.EqualsOrNullEmpty(parameterName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (parameter == null)
                {
                    return null;
                }
                return parameter.Value;
            }
        }
        public T GetParameterValue<T>(string parameterName)
        {
            var parameter = this[parameterName];
            if (parameter != null)
            {
                return (T)parameter.Value;
            }
            return default(T);
        }
        #endregion
        #endregion

        #region Properties

        public string ContentTitle { get; set; }

        private HtmlMeta htmlMeta = new HtmlMeta();
        public HtmlMeta HtmlMeta
        {
            get
            {
                return htmlMeta;
            }
        }

        private FrontUrlHelper frontUrl;
        public FrontUrlHelper FrontUrl
        {
            get
            {
                if (frontUrl == null)
                {
                    frontUrl = new FrontUrlHelper(this.PageRequestContext, this.Url);
                }
                return frontUrl;
            }
        }

        private UrlHelper url;
        public System.Web.Mvc.UrlHelper Url
        {
            get
            {
                if (url == null)
                {
                    url = new UrlHelper(this.ControllerContext.RequestContext);
                }
                return url;
            }
        }

        public PageRequestContext PageRequestContext
        {
            get;
            private set;
        }
        public ControllerContext ControllerContext { get; private set; }


        public IList<IHtmlString> Styles { get; private set; }
        public IList<IHtmlString> Scripts { get; private set; }

        internal bool InlineEditing
        {
            get
            {
                return
                    this.PageRequestContext.RequestChannel != Web.FrontRequestChannel.Design
                    && this.PageRequestContext.Site.InlineEditing.HasValue
                    && this.PageRequestContext.Site.InlineEditing.Value == true
                    && ServiceFactory.UserManager.Authorize(Page_Context.Current.PageRequestContext.Site, Page_Context.Current.ControllerContext.HttpContext.User.Identity.Name, Account.Models.Permission.Sites_Page_InlineEditPermission);
            }
        }

        public bool EnabledInlineEditing(EditingType type)
        {
            var enabled = InlineEditing;
            if (enabled)
            {
                if (this.PageRequestContext.RequestChannel == Web.FrontRequestChannel.Draft)
                {
                    enabled = false;
                    if ((type & EditingType.Page) == EditingType.Page)
                    {
                        enabled = true;
                    }
                }
            }
            return enabled;
        }
        #endregion

        #region InitContext
        public bool Initialized { get; private set; }
        public void InitContext(PageRequestContext pageRequestContext, ControllerContext controllerContext)
        {
            this.PageRequestContext = pageRequestContext;
            this.ControllerContext = controllerContext;

            this.Initialized = true;
        }
        #endregion

        #region ExecutePlugin
        public virtual ActionResult ExecutePlugins()
        {
            var page = PageRequestContext.Page.AsActual();

            if (page.Plugins != null)
            {
                foreach (var plugin in page.Plugins)
                {
                    var result = ExecutePlugin(plugin, null);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            var viewPositions = page.PagePositions.Where(it => it is ViewPosition).OrderBy(it => it.Order);
            foreach (ViewPosition viewPosition in viewPositions)
            {
                var view = new Models.View(PageRequestContext.Site, viewPosition.ViewName).LastVersion();
                if (!view.Exists())
                {
                    throw new KoobooException(string.Format("The view does not exists.Name:'{0}'", view.Name));
                }
                view = view.AsActual();
                if (view.Plugins != null)
                {
                    var positionContext = new PagePositionContext() { PagePosition = viewPosition, ViewData = GetPositionViewData(viewPosition.PagePositionId) };

                    foreach (var plugin in view.Plugins)
                    {
                        var result = ExecutePlugin(plugin, positionContext);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }
        private ActionResult ExecutePlugin(string pluginType, PagePositionContext positionContext)
        {
            var type = Type.GetType(pluginType);
            if (type == null)
            {
                throw new KoobooException(string.Format("The plugin type '{0}' could not be found.", pluginType));
            }
            var plugin = (IPagePlugin)Activator.CreateInstance(type);
            return plugin.Execute(this, positionContext);
        }
        #endregion

        #region DataRules & ViewData
        private IDictionary<string, ViewDataDictionary> positionsViewData = new Dictionary<string, ViewDataDictionary>();
        public ViewDataDictionary GetPositionViewData(string positionId)
        {
            if (!positionsViewData.ContainsKey(positionId))
            {
                positionsViewData[positionId] = new ViewDataDictionary();
            }
            return positionsViewData[positionId];
        }
        public void SetPositionViewData(string positionId, ViewDataDictionary viewData)
        {
            positionsViewData[positionId] = viewData;
        }
        private IEnumerable<Models.View> GetViewsInPage(Site site, Page page)
        {
            var viewPositions = page.AsActual().PagePositions.Where(it => it is ViewPosition).OrderBy(it => it.Order);
            foreach (ViewPosition viewPosition in viewPositions)
            {
                var view = new Models.View(site, viewPosition.ViewName).LastVersion();
                if (!view.Exists())
                {
                    throw new KoobooException(string.Format("The view '{0}' does not exists.Name.", view.Name));
                }
                yield return view;
            }
        }
        public virtual void ExecuteDataRules()
        {
            var pageViewData = ControllerContext.Controller.ViewData;

            var site = PageRequestContext.Site.AsActual();
            var page = PageRequestContext.Page.AsActual();

            var dataRuleContext = new DataRuleContext(PageRequestContext.Site, PageRequestContext.Page) { ValueProvider = PageRequestContext.GetValueProvider() };
            if (page.DataRules != null)
            {
                DataRuleExecutor.Execute(pageViewData, dataRuleContext, page.DataRules);
            }
            var viewPositions = page.PagePositions.Where(it => it is ViewPosition).OrderBy(it => it.Order);
            foreach (ViewPosition viewPosition in viewPositions)
            {
                var view = new Models.View(site, viewPosition.ViewName).LastVersion();
                if (!view.Exists())
                {
                    throw new KoobooException(string.Format("The view does not exists.Name:'{0}'", view.Name));
                }
                var positionViewData = (ViewDataDictionary)GetPositionViewData(viewPosition.PagePositionId).Merge(pageViewData);
                view = view.AsActual();
                var dataRules = view.DataRules;
                if (dataRules != null)
                {
                    var valueProvider = PageRequestContext.GetValueProvider();
                    valueProvider.Insert(0, new ViewParameterValueProvider(view.Parameters));
                    dataRuleContext.ValueProvider = valueProvider;
                    DataRuleExecutor.Execute(positionViewData, dataRuleContext, dataRules);
                }
                if (positionViewData.Model == null)
                {
                    positionViewData.Model = positionViewData.Values.FirstOrDefault();
                }
                SetPositionViewData(viewPosition.PagePositionId, positionViewData);
            }
        }
        #endregion

        #region InitializeTitleHtmlMeta

        public virtual void InitializeTitleHtmlMeta()
        {
            //check if the ContentTitle was set by page plugin.
            if (string.IsNullOrEmpty(this.ContentTitle) && !string.IsNullOrEmpty(PageRequestContext.Page.ContentTitle))
            {
                this.ContentTitle = EvaluateStringFormulas(PageRequestContext.Page.ContentTitle);
            }

            if (PageRequestContext.Page.HtmlMeta != null)
            {
                if (string.IsNullOrEmpty(this.htmlMeta.HtmlTitle))
                {
                    this.htmlMeta.HtmlTitle = EvaluateStringFormulas(PageRequestContext.Page.HtmlMeta.HtmlTitle);
                }
                if (string.IsNullOrEmpty(this.htmlMeta.Author))
                {
                    this.htmlMeta.Author = EvaluateStringFormulas(PageRequestContext.Page.HtmlMeta.Author);
                }
                if (string.IsNullOrEmpty(this.htmlMeta.Description))
                {
                    this.htmlMeta.Description = EvaluateStringFormulas(PageRequestContext.Page.HtmlMeta.Description);
                }
                if (string.IsNullOrEmpty(this.htmlMeta.Keywords))
                {
                    this.htmlMeta.Keywords = EvaluateStringFormulas(PageRequestContext.Page.HtmlMeta.Keywords);
                }
                if (PageRequestContext.Page.HtmlMeta.Customs != null)
                {
                    foreach (var item in PageRequestContext.Page.HtmlMeta.Customs)
                    {
                        this.htmlMeta.Customs[item.Key] = EvaluateStringFormulas(item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Kooboo_{title}
        /// </summary>
        /// <param name="formulas"></param>
        /// <returns></returns>
        protected virtual string EvaluateStringFormulas(string formulas)
        {
            if (string.IsNullOrEmpty(formulas))
            {
                return null;
            }
            var matches = Regex.Matches(formulas, "{(?<Name>[^{^}]+)}");
            var s = formulas;
            foreach (Match match in matches)
            {
                var value = GetFieldValue(match.Groups["Name"].Value);
                s = s.Replace(match.Value, value == null ? "" : value.ToString());
            }
            return s;
        }
        #endregion

        #region GetFieldValue
        public virtual object GetFieldValue(string fieldName)
        {
            object value = GetFieldValueFromViewData(ControllerContext.Controller.ViewData, fieldName);
            if (value == null)
            {
                foreach (var item in positionsViewData.Values)
                {
                    value = GetFieldValueFromViewData(item, fieldName);
                    if (value != null)
                    {
                        break;
                    }
                }
            }
            return value;
        }
        private object GetFieldValueFromViewData(ViewDataDictionary viewData, string fieldName)
        {
            object value = null;
            string viewDataKey = null;
            fieldName = ParseFieldName(fieldName, out viewDataKey);
            if (string.IsNullOrEmpty(viewDataKey))
            {
                if (viewData.ContainsKey(fieldName))
                {
                    value = viewData[fieldName];
                }
                if (value == null && viewData.Model != null)
                {
                    value = GetFieldValueFromObject(viewData.Model, fieldName);
                }
                if (value == null)
                {
                    foreach (var item in viewData.Values)
                    {
                        value = GetFieldValueFromObject(item, fieldName);
                        if (value != null)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (viewData.ContainsKey(viewDataKey))
                {
                    value = GetFieldValueFromObject(viewData[viewDataKey], fieldName);
                }
            }
            return value;

        }
        private string ParseFieldName(string fieldName, out string viewDataKey)
        {
            var pointIndex = fieldName.IndexOf('.');
            viewDataKey = null;
            if (pointIndex != -1)
            {
                viewDataKey = fieldName.Substring(0, pointIndex);
                if (pointIndex + 1 <= fieldName.Length)
                {
                    fieldName = fieldName.Substring(pointIndex + 1);
                }
            }
            return fieldName;
        }
        private object GetFieldValueFromObject(Object o, string fieldName)
        {
            if (o is DynamicDictionary)
            {
                return ((DynamicDictionary)o)[fieldName];
            }
            try
            {
                return o.Members().Properties[fieldName];
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Module
        public virtual bool ExecuteModuleControllerAction()
        {
            var modulePositions = PageRequestContext.Page.AsActual().PagePositions.Where(it => it is ModulePosition).OrderBy(it => it.Order);
            IDictionary<string, ModuleActionInvokedContext> moduleActionResults = new Dictionary<string, ModuleActionInvokedContext>();
            foreach (ModulePosition modulePosition in modulePositions)
            {
                var site = Page_Context.Current.PageRequestContext.Site;
                var moduleUrl = Page_Context.Current.PageRequestContext.ModuleUrlContext.GetModuleUrl(modulePosition.PagePositionId);

                ModuleActionInvokedContext result = ModuleExecutor.InvokeAction(this.ControllerContext, site, moduleUrl, modulePosition);
                if (ModuleActionResultExecutor.IsExclusiveResult(result.ActionResult))
                {
                    ModuleActionResultExecutor.ExecuteExclusiveResult(result.ControllerContext, result.ActionResult);
                    return false;
                }
                moduleActionResults.Add(modulePosition.PagePositionId, result);
            }

            ModuleResults = moduleActionResults;
            return true;
        }

        public IDictionary<string, ModuleActionInvokedContext> ModuleResults { get; private set; }
        #endregion

        public void CheckContext()
        {
            if (this.Initialized == false)
            {
                throw new NotSupportedException();
            }
        }
    }
}
