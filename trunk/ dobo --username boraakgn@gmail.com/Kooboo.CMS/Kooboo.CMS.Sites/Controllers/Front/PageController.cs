//# define Page_Trace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Web;
using System.Web;
using Kooboo.CMS.Sites.View;
using System.IO;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.CMS.Sites.Caching;
using System.Diagnostics;
using Kooboo.Web.Mvc;

namespace Kooboo.CMS.Sites.Controllers.Front
{
    public class PageExecutionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            base.OnActionExecuting(filterContext);

            var controllerContext = filterContext.Controller.ControllerContext;

            var pageRequestContext = InitPageRequestContext(controllerContext);

            Page_Context.Current.InitContext(pageRequestContext, controllerContext);

#if Page_Trace
            stopwatch.Stop();
            filterContext.HttpContext.Response.Write(string.Format("PageExecutionFilterAttribute.OnActionExecuting, {0}ms.</br>", stopwatch.ElapsedMilliseconds));
#endif

        }

        private PageRequestContext InitPageRequestContext(ControllerContext controllerContext)
        {
            FrontControllerBase frontController = (FrontControllerBase)controllerContext.Controller;
            string pageUrl = frontController.RequestUrl;

#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            //pageUrl: product/detail/product1
            var page = Persistence.Providers.PageProvider.GetPageByUrl(frontController.Site, pageUrl);

#if Page_Trace
            stopwatch.Stop();
            controllerContext.HttpContext.Response.Write(string.Format("GetPageByUrl, {0}ms.</br>", stopwatch.ElapsedMilliseconds));
#endif
            if (page == null)
            {
                throw new HttpException(0x194, string.Format(SR.System_Web_Resources.GetString("Path_not_found"), new object[] { frontController.Request.Path }));
            }

            var draft = controllerContext.RequestContext.GetRequestValue("_draft_");
            if (!string.IsNullOrEmpty(draft) && draft.ToLower() == "true")
            {
                page = Services.ServiceFactory.PageManager.PageProvider.GetDraft(page);
                frontController.FrontHttpRequest.RequestChannel = FrontRequestChannel.Draft;
            }
            else
            {
                EnsurePagePublished(frontController, page);
            }
            var requestUrl = "";
            if (!string.IsNullOrEmpty(pageUrl))
            {
                requestUrl = pageUrl.Substring(page.VirtualPath.Length).TrimStart('/');
            }

            return new PageRequestContext(controllerContext, frontController.Site, page, frontController.FrontHttpRequest.RequestChannel, requestUrl);
        }

        protected virtual void EnsurePagePublished(Controller controller, Page page)
        {
            if (page.Published.Value == false)
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    throw new HttpException(0x194, string.Format(SR.System_Web_Resources.GetString("Path_not_found"), new object[] { controller.Request.Path }));
                }
            }

        }
    }
    public class OutputCacheFilterAttribute : PageExecutionFilterAttribute
    {
        private class OutputWriterWrapper : TextWriter
        {
            internal OutputWriterWrapper(TextWriter inner)
            {
                _inner = inner;
            }
            TextWriter _inner;
            StringBuilder sb = new StringBuilder();
            public override void Close()
            {
                base.Close();
                _inner.Close();
                sb.Clear();
            }
            public override void Flush()
            {
                base.Flush();
                _inner.Flush();
            }
            public override void Write(char value)
            {
                // base.Write(value);
                _inner.Write(value);
                sb.Append(value);
            }
            public override void Write(char[] buffer)
            {
                //base.Write(buffer);
                _inner.Write(buffer);
                sb.Append(buffer);
            }
            public override void Write(string value)
            {
                // base.Write(value);
                _inner.Write(value);
                sb.Append(value);
            }
            public override void Write(char[] buffer, int index, int count)
            {
                //  base.Write(buffer, index, count);
                _inner.Write(buffer, index, count);
                sb.Append(buffer, index, count);
            }
            public override void WriteLine()
            {
                //   base.WriteLine();
                _inner.WriteLine();
                sb.AppendLine();
            }
            public override Encoding Encoding
            {
                get { return _inner.Encoding; }
            }

            public override string ToString()
            {
                return sb.ToString();
            }
            public string GetHtml()
            {
                return sb.ToString();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {


#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            base.OnActionExecuting(filterContext);
            var page = Page_Context.Current.PageRequestContext.Page;
            var site = Page_Context.Current.PageRequestContext.Site;
            if (Page_Context.Current.PageRequestContext.Page.AsActual().EnabledCache)
            {
                var cacheKey = GetOutputCacheKey(filterContext.HttpContext, page);
                var outputCache = site.ObjectCache().Get(cacheKey);
                if (outputCache != null)
                {
                    filterContext.Result = new ContentResult() { Content = outputCache.ToString(), ContentType = "text/html" };
                }
                else
                {
                    filterContext.HttpContext.Response.Output = new OutputWriterWrapper(filterContext.HttpContext.Response.Output);
                }
            }
#if Page_Trace
            stopwatch.Stop();
            filterContext.HttpContext.Response.Write(string.Format("OutputCacheFilterAttribute.OnActionExecuting, {0}ms.</br>", stopwatch.ElapsedMilliseconds));
#endif
        }

        private static string GetOutputCacheKey(HttpContextBase httpContext, Page page)
        {
            var cacheKey = string.Format("Page OutputCache-Full page name:{0};Raw request url:{1}", page.FullName, httpContext.Request.RawUrl);
            return cacheKey;
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            if (filterContext.Exception == null && filterContext.HttpContext.Response.ContentType.ToLower().Contains("text/html"))
            {
                var site = Page_Context.Current.PageRequestContext.Site;
                var page = Page_Context.Current.PageRequestContext.Page;
                var cacheKey = GetOutputCacheKey(filterContext.HttpContext, page);
                var cacheOutputWrapper = filterContext.HttpContext.Response.Output as OutputWriterWrapper;
                if (cacheOutputWrapper != null)
                {
                    site.ObjectCache().Add(cacheKey, cacheOutputWrapper.GetHtml(), page.AsActual().OutputCache.ToCachePolicy());
                }
            }
#if Page_Trace
            stopwatch.Stop();
            filterContext.HttpContext.Response.Write(string.Format("OutputCacheFilterAttribute.OnResultExecuted, {0}ms.</br>", stopwatch.ElapsedMilliseconds));
#endif
        }
    }
    public class PageController : FrontControllerBase
    {
        public Page_Context PageContext
        {
            get
            {
                return Page_Context.Current;
            }
        }
        [OutputCacheFilter]
        public virtual ActionResult Entry()
        {
#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            var actionResult = Page_Context.Current.ExecutePlugins();
            if (actionResult != null)
            {
                return actionResult;
            }
#if Page_Trace
            stopwatch.Stop();
            Response.Write(string.Format("ExecutePlugins, {0}ms.</br>", stopwatch.ElapsedMilliseconds));
            stopwatch.Reset();
            stopwatch.Start();
#endif
            Page_Context.Current.ExecuteDataRules();

#if Page_Trace
            stopwatch.Stop();
            Response.Write(string.Format("ExecuteDataRules, {0}ms</br>", stopwatch.ElapsedMilliseconds));
            stopwatch.Reset();
            stopwatch.Start();
#endif
            Page_Context.Current.InitializeTitleHtmlMeta();

#if Page_Trace
            stopwatch.Stop();
            Response.Write(string.Format("InitializeTitleHtmlMeta, {0}ms</br>", stopwatch.ElapsedMilliseconds));
            stopwatch.Reset();
            stopwatch.Start();
#endif
            if (Page_Context.Current.ExecuteModuleControllerAction())
            {
                return ViewPage();
            }
#if Page_Trace
            stopwatch.Stop();
            Response.Write(string.Format("ViewPage, {0}ms</br>", stopwatch.ElapsedMilliseconds));
#endif
            return null;
        }

        protected virtual ActionResult ViewPage()
        {
            var layout = (new Layout(Site, PageContext.PageRequestContext.Page.Layout).LastVersion()).AsActual();

            ViewResult viewResult = new FrontViewResult(ControllerContext, layout.FileExtension.ToLower(), layout.TemplateFileVirutalPath);

            if (viewResult != null)
            {
                viewResult.ViewData = this.ViewData;
                viewResult.TempData = this.TempData;
            }


            return viewResult;
        }

        protected override void OnSiteNotExists()
        {
            RedirectTo404().ExecuteResult(this.ControllerContext);            
            //throw new HttpException(0x194, string.Format(SR.System_Web_Resources.GetString("Path_not_found"), new object[] { HttpContext.Request.Path }));
        }
    }
}
