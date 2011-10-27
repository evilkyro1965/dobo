using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using System.IO;
using System.Globalization;


namespace Kooboo.CMS.Sites.Extension.Module
{
    internal static class HttpHandlerUtil
    {
        internal class ServerExecuteHttpHandlerWrapper : System.Web.UI.Page
        {
            private readonly IHttpHandler _httpHandler;
            internal IHttpHandler InnerHandler
            {
                get
                {
                    return this._httpHandler;
                }
            }
            public ServerExecuteHttpHandlerWrapper(IHttpHandler httpHandler)
            {
                this._httpHandler = httpHandler;
            }
            public override void ProcessRequest(HttpContext context)
            {
                HttpHandlerUtil.ServerExecuteHttpHandlerWrapper.Wrap(delegate
                {
                    this._httpHandler.ProcessRequest(context);
                }
                );
            }
            protected static void Wrap(Action action)
            {
                HttpHandlerUtil.ServerExecuteHttpHandlerWrapper.Wrap<object>(delegate
                {
                    action();
                    return null;
                }
                );
            }
            protected static TResult Wrap<TResult>(Func<TResult> func)
            {
                TResult result;
                try
                {
                    result = func();
                }
                catch (HttpException ex)
                {
                    if (ex.GetHttpCode() == 500)
                    {
                        throw;
                    }
                    HttpException ex2 = new HttpException(500, SR.System_Web_Mvc_Resources.GetString("ViewPageHttpHandlerWrapper_ExceptionOccurred"), ex);
                    throw ex2;
                }
                return result;
            }
        }
        private sealed class ServerExecuteHttpHandlerAsyncWrapper : HttpHandlerUtil.ServerExecuteHttpHandlerWrapper, IHttpAsyncHandler, IHttpHandler
        {
            private readonly IHttpAsyncHandler _httpHandler;
            public ServerExecuteHttpHandlerAsyncWrapper(IHttpAsyncHandler httpHandler)
                : base(httpHandler)
            {
                this._httpHandler = httpHandler;
            }
            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
            {
                return HttpHandlerUtil.ServerExecuteHttpHandlerWrapper.Wrap<IAsyncResult>(() => this._httpHandler.BeginProcessRequest(context, cb, extraData));
            }
            public void EndProcessRequest(IAsyncResult result)
            {
                HttpHandlerUtil.ServerExecuteHttpHandlerWrapper.Wrap(delegate
                {
                    this._httpHandler.EndProcessRequest(result);
                }
                );
            }
        }
        public static IHttpHandler WrapForServerExecute(IHttpHandler httpHandler)
        {
            IHttpAsyncHandler httpAsyncHandler = httpHandler as IHttpAsyncHandler;
            if (httpAsyncHandler == null)
            {
                return new HttpHandlerUtil.ServerExecuteHttpHandlerWrapper(httpHandler);
            }
            return new HttpHandlerUtil.ServerExecuteHttpHandlerAsyncWrapper(httpAsyncHandler);
        }
    }

    internal static class ChildActionExtensions
    {
        #region ChildActionExtensions
        internal class ChildActionMvcHandler : MvcHandler
        {
            public ChildActionMvcHandler(RequestContext context)
                : base(context)
            {
            }
            //protected internal override void AddVersionHeader(HttpContextBase httpContext)
            //{
            //}
        }

        internal static MvcHtmlString Action(HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            MvcHtmlString result;
            using (StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture))
            {
                ActionHelper(htmlHelper, actionName, controllerName, routeValues, stringWriter);
                result = MvcHtmlString.Create(stringWriter.ToString());
            }
            return result;
        }

        internal static void RenderAction(HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            ActionHelper(htmlHelper, actionName, controllerName, routeValues, htmlHelper.ViewContext.Writer);
        }
        internal static void ActionHelper(HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, TextWriter textWriter)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException("htmlHelper");
            }
            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException(SR.System_Web_Mvc_Resources.GetString("Common_NullOrEmpty"), "actionName");
            }
            RouteValueDictionary routeValueDictionary = routeValues;
            routeValues = MergeDictionaries(new RouteValueDictionary[]
			{
				routeValues, 
				htmlHelper.ViewContext.RouteData.Values
			});
            routeValues["action"] = actionName;
            if (!string.IsNullOrEmpty(controllerName))
            {
                routeValues["controller"] = controllerName;
            }
            bool flag = false;
            VirtualPathData virtualPathForArea = htmlHelper.RouteCollection
                .GetVirtualPathForArea(htmlHelper.ViewContext.RequestContext, null, routeValues);
            if (virtualPathForArea == null)
            {
                throw new InvalidOperationException(SR.System_Web_Mvc_Resources.GetString("Common_NoRouteMatched"));
            }
            if (flag)
            {
                routeValues.Remove("area");
                if (routeValueDictionary != null)
                {
                    routeValueDictionary.Remove("area");
                }
            }
            if (routeValueDictionary != null)
            {
                routeValues[Guid.NewGuid().ToString()] = new DictionaryValueProvider<object>(routeValueDictionary, CultureInfo.InvariantCulture);
            }
            RouteData routeData = CreateRouteData(virtualPathForArea.Route, routeValues, virtualPathForArea.DataTokens, htmlHelper.ViewContext);
            HttpContextBase httpContext = htmlHelper.ViewContext.HttpContext;
            var parentModuleRequestContext = (ModuleRequestContext)htmlHelper.ViewContext.RequestContext;
            RequestContext context = new ModuleRequestContext(httpContext, routeData, parentModuleRequestContext.ModuleContext) { PageControllerContext = parentModuleRequestContext.PageControllerContext };
            ChildActionMvcHandler httpHandler = new ChildActionMvcHandler(context);
            httpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(httpHandler), textWriter, true);
        }
        private static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, RouteValueDictionary dataTokens, ViewContext parentViewContext)
        {
            RouteData routeData = new RouteData();
            foreach (KeyValuePair<string, object> current in routeValues)
            {
                routeData.Values.Add(current.Key, current.Value);
            }
            foreach (KeyValuePair<string, object> current2 in dataTokens)
            {
                routeData.DataTokens.Add(current2.Key, current2.Value);
            }
            routeData.Route = route;
            routeData.DataTokens["ParentActionViewContext"] = parentViewContext;
            return routeData;
        }
        private static RouteValueDictionary MergeDictionaries(params RouteValueDictionary[] dictionaries)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            foreach (RouteValueDictionary current in
                from d in dictionaries
                where d != null
                select d)
            {
                foreach (KeyValuePair<string, object> current2 in current)
                {
                    if (!routeValueDictionary.ContainsKey(current2.Key))
                    {
                        routeValueDictionary.Add(current2.Key, current2.Value);
                    }
                }
            }
            return routeValueDictionary;
        }
        #endregion
    }

    public class ModuleHtmlHelper : HtmlHelper
    {
        public ModuleHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : base(viewContext, viewDataContainer)
        {
        }
        public ModuleHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(viewContext, viewDataContainer, routeCollection)
        {
        }
        #region ChildActionExtensions
        public MvcHtmlString Action(string actionName)
        {
            return this.Action(actionName, null, null);
        }
        public MvcHtmlString Action(string actionName, object routeValues)
        {
            return this.Action(actionName, null, new RouteValueDictionary(routeValues));
        }
        public MvcHtmlString Action(string actionName, RouteValueDictionary routeValues)
        {
            return this.Action(actionName, null, routeValues);
        }
        public MvcHtmlString Action(string actionName, string controllerName)
        {
            return this.Action(actionName, controllerName, null);
        }
        public MvcHtmlString Action(string actionName, string controllerName, object routeValues)
        {
            return this.Action(actionName, controllerName, new RouteValueDictionary(routeValues));
        }
        public MvcHtmlString Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return ChildActionExtensions.Action(this, actionName, controllerName, routeValues);
        }
        public void RenderAction(string actionName)
        {
            this.RenderAction(actionName, null, null);
        }
        public void RenderAction(string actionName, object routeValues)
        {
            this.RenderAction(actionName, null, new RouteValueDictionary(routeValues));
        }
        public void RenderAction(string actionName, RouteValueDictionary routeValues)
        {
            this.RenderAction(actionName, null, routeValues);
        }
        public void RenderAction(string actionName, string controllerName)
        {
            this.RenderAction(actionName, controllerName, null);
        }
        public void RenderAction(string actionName, string controllerName, object routeValues)
        {
            this.RenderAction(actionName, controllerName, new RouteValueDictionary(routeValues));
        }
        public void RenderAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            ChildActionExtensions.ActionHelper(this, actionName, controllerName, routeValues, this.ViewContext.Writer);
        }
        #endregion
    }

    public class ModuleHtmlHelper<TModel> : HtmlHelper<TModel>
    {
        private ViewDataDictionary<TModel> _viewData;
        public new ViewDataDictionary<TModel> ViewData
        {
            get
            {
                return this._viewData;
            }
        }
        public ModuleHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }
        public ModuleHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(viewContext, viewDataContainer, routeCollection)
        {
            this._viewData = new ViewDataDictionary<TModel>(viewDataContainer.ViewData);
        }

        #region ChildActionExtensions
        public MvcHtmlString Action(string actionName)
        {
            return this.Action(actionName, null, null);
        }
        public MvcHtmlString Action(string actionName, object routeValues)
        {
            return this.Action(actionName, null, new RouteValueDictionary(routeValues));
        }
        public MvcHtmlString Action(string actionName, RouteValueDictionary routeValues)
        {
            return this.Action(actionName, null, routeValues);
        }
        public MvcHtmlString Action(string actionName, string controllerName)
        {
            return this.Action(actionName, controllerName, null);
        }
        public MvcHtmlString Action(string actionName, string controllerName, object routeValues)
        {
            return this.Action(actionName, controllerName, new RouteValueDictionary(routeValues));
        }
        public MvcHtmlString Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return ChildActionExtensions.Action(this, actionName, controllerName, routeValues);
        }
        public void RenderAction(string actionName)
        {
            this.RenderAction(actionName, null, null);
        }
        public void RenderAction(string actionName, object routeValues)
        {
            this.RenderAction(actionName, null, new RouteValueDictionary(routeValues));
        }
        public void RenderAction(string actionName, RouteValueDictionary routeValues)
        {
            this.RenderAction(actionName, null, routeValues);
        }
        public void RenderAction(string actionName, string controllerName)
        {
            this.RenderAction(actionName, controllerName, null);
        }
        public void RenderAction(string actionName, string controllerName, object routeValues)
        {
            this.RenderAction(actionName, controllerName, new RouteValueDictionary(routeValues));
        }
        public void RenderAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            ChildActionExtensions.ActionHelper(this, actionName, controllerName, routeValues, this.ViewContext.Writer);
        }
        #endregion
    }

    public static class ModuleViewHelperExtension
    {
        public static HtmlHelper ModuleHtml(this HtmlHelper htmlHelper)
        {
            CheckContext(htmlHelper.ViewContext.RequestContext);
            var moduleRequestContext = (ModuleRequestContext)htmlHelper.ViewContext.RequestContext;
            return new ModuleHtmlHelper(htmlHelper.ViewContext, htmlHelper.ViewDataContainer, moduleRequestContext.ModuleContext.RouteTable);
        }
        private static void CheckContext(RequestContext requestContext)
        {
            if (!(requestContext is ModuleRequestContext))
            {
                throw new Exception("ModuleHtml extension method can only be use in module");
            }
        }
        public static ModuleHtmlHelper<TModel> ModuleHtml<TModel>(this HtmlHelper<TModel> htmlHelper)
        {
            CheckContext(htmlHelper.ViewContext.RequestContext);
            var moduleRequestContext = (ModuleRequestContext)htmlHelper.ViewContext.RequestContext;
            return new ModuleHtmlHelper<TModel>(htmlHelper.ViewContext, htmlHelper.ViewDataContainer, moduleRequestContext.ModuleContext.RouteTable);
        }

        public static UrlHelper ModuleUrl(this UrlHelper urlHelper)
        {
            CheckContext(urlHelper.RequestContext);
            var moduleRequestContext = (ModuleRequestContext)urlHelper.RequestContext;
            return new UrlHelper(urlHelper.RequestContext, moduleRequestContext.ModuleContext.RouteTable);
        }

        public static AjaxHelper ModuleAjax(this AjaxHelper ajaxHelper)
        {
            CheckContext(ajaxHelper.ViewContext.RequestContext);
            var moduleRequestContext = (ModuleRequestContext)ajaxHelper.ViewContext.RequestContext;
            return new AjaxHelper(ajaxHelper.ViewContext, ajaxHelper.ViewDataContainer, moduleRequestContext.ModuleContext.RouteTable);
        }
        public static AjaxHelper<TModel> ModuleAjax<TModel>(this AjaxHelper<TModel> ajaxHelper)
        {
            CheckContext(ajaxHelper.ViewContext.RequestContext);
            var moduleRequestContext = (ModuleRequestContext)ajaxHelper.ViewContext.RequestContext;
            return new AjaxHelper<TModel>(ajaxHelper.ViewContext, ajaxHelper.ViewDataContainer, moduleRequestContext.ModuleContext.RouteTable);
        }
    }
}
