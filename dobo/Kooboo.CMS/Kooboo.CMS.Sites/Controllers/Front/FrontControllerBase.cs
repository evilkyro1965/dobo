using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Web;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Extension;
using Kooboo.Web.Url;
using System.Web;
using System.Net;
using Kooboo.CMS.Sites.View;
using Kooboo.Web.Mvc;
using Kooboo.Globalization;

namespace Kooboo.CMS.Sites.Controllers.Front
{

    public class FriendlyErrorResult : RedirectResult
    {
        public enum RedirectType
        {
            Redirect,
            Transfer
        }
        public static RedirectType DefaultRedirectType = RedirectType.Redirect;

        private class TransferMvcHttpHandler : MvcHttpHandler
        {
            protected override void ProcessRequest(HttpContext httpContext)
            {
                HttpContextBase context = new FrontHttpContextWrapper(httpContext);
                base.ProcessRequest(context);
            }
        }
        public FriendlyErrorResult(string errorUrl, int statusCode)
            : base(errorUrl)
        {
            this.StatusCode = statusCode;
        }
        public int StatusCode { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var httpContext = HttpContext.Current;

            if (DefaultRedirectType == RedirectType.Redirect)
            {
                base.ExecuteResult(context);
            }
            else
            {
                httpContext.RewritePath(Url, false);

                IHttpHandler httpHandler = new TransferMvcHttpHandler();
                httpHandler.ProcessRequest(httpContext);                
                httpContext.Response.StatusCode = StatusCode;
            }
        }
    }
    public class FrontControllerBase : Controller
    {

        public FrontHttpContextWrapper FrontPageHttpContext
        {
            get
            {
                return (FrontHttpContextWrapper)this.HttpContext;
            }
        }
        public FrontHttpRequestWrapper FrontHttpRequest
        {
            get
            {
                return (FrontHttpRequestWrapper)this.FrontPageHttpContext.Request;
            }
        }
        public Site Site
        {
            get
            {
                return FrontHttpRequest.Site;
            }
        }

        public string RequestUrl
        {
            get
            {
                return FrontHttpRequest.RequestUrl;
            }
        }
        protected override void ExecuteCore()
        {
            if (Site != null &&
                (FrontHttpRequest.RequestChannel == FrontRequestChannel.Host || FrontHttpRequest.RequestChannel == FrontRequestChannel.HostNPath))
            {
                if (!Persistence.Providers.SiteProvider.IsOnline(Site))
                {
                    throw new KoobooException("The site is offline.".Localize());
                }
            }
            if (this.Site == null)
            {
                OnSiteNotExists();
            }
            else
            {
                SiteEventDispatcher.OnPreSiteRequestExecute(Site, this.HttpContext);

                base.ExecuteCore();
            }
        }
        protected virtual void OnSiteNotExists()
        {
            throw new HttpException(0x194, string.Format(SR.System_Web_Resources.GetString("Path_not_found"), new object[] { HttpContext.Request.Path }));
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Kooboo.HealthMonitoring.Log.LogException(filterContext.Exception);

            HttpErrorStatusCode statusCode = HttpErrorStatusCode.InternalServerError_500;
            HttpException httpException = filterContext.Exception as HttpException;

            if (httpException != null)
            {
                statusCode = (HttpErrorStatusCode)httpException.GetHttpCode();
            }
            if (Site != null)
            {
                var customError = Services.ServiceFactory.CustomErrorManager.Get(Site, statusCode.ToString());

                if (customError != null)
                {
                    var errorUrl = customError.RedirectUrl;

                    var redirectUrl = FrontUrlHelper.WrapperUrl(Url.Content(errorUrl), Site, FrontHttpRequest.RequestChannel).ToString();
                    if (!string.IsNullOrEmpty(errorUrl) && !errorUrl.TrimStart('~').TrimStart('/').TrimEnd('/').EqualsOrNullEmpty(this.Request.AppRelativeCurrentExecutionFilePath.TrimStart('~').TrimStart('/').TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl = redirectUrl.AddQueryParam("errorpath", this.Request.RawUrl)
                            .AddQueryParam("statusCode", ((int)statusCode).ToString());

                        filterContext.Result = new FriendlyErrorResult(redirectUrl, (int)statusCode);
                        filterContext.ExceptionHandled = true;
                    }
                }
            }
            else
            {
                if (statusCode == HttpErrorStatusCode.NotFound_404)
                {
                    filterContext.Result = RedirectTo404();
                    filterContext.ExceptionHandled = true;
                }
            }
            base.OnException(filterContext);
        }
        protected virtual ActionResult RedirectTo404()
        {
            var notFoundUrl = "~/404";
            notFoundUrl = notFoundUrl.AddQueryParam("errorpath", this.Request.RawUrl);

            return new FriendlyErrorResult(notFoundUrl, 404);
        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            if (Site != null)
            {
                SiteEventDispatcher.OnPostSiteRequestExecute(Site, this.HttpContext);
            }
        }
    }
}
