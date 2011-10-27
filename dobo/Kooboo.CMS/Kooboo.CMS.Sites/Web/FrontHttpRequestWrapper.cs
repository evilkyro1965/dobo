
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.View;
using Kooboo.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Globalization;

namespace Kooboo.CMS.Sites.Web
{
    public class FrontHttpRequestWrapper : System.Web.HttpRequestWrapper
    {
        private HttpRequest _request;
        public FrontHttpRequestWrapper(HttpRequest httpRequest)
            : base(httpRequest)
        {
            //applicationPath = base.ApplicationPath;

            //HttpContext.Current.Items["ApplicationPath"] = applicationPath;

            this._request = httpRequest;

            ResolveSite();


        }
        #region override
        string appRelativeCurrentExecutionFilePath;
        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return appRelativeCurrentExecutionFilePath;
            }
        }
        #endregion

        public Site Site
        {
            get
            {
                return Site.Current;
            }
            private set
            {
                Site.Current = value;
            }
        }

        public FrontRequestChannel RequestChannel
        {
            get;
            set;
        }
        public string RequestUrl { get; private set; }

        private static string GetRawHostWithoutPort(HttpRequest request)
        {
            return request.Url.Host;
            //var host = request.Headers["Host"];

            //var portIndex = host.IndexOf(":");
            //if (portIndex > 0)
            //{
            //    return host.Substring(0, portIndex);
            //}
            //return host;
        }
        private void ResolveSite()
        {
            // "~/site1/index"
            appRelativeCurrentExecutionFilePath = base.AppRelativeCurrentExecutionFilePath;

            if (!string.IsNullOrEmpty(this.PathInfo))
            {
                appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath.TrimEnd('/') + "/" + PathInfo;
            }
            //trim "~/"
            var trimedPath = appRelativeCurrentExecutionFilePath.Substring(2);

            //if the RawUrl is not start with the debug site url.
            //http://www.site1.com/index
            //http://www.site1.com/en/index
            var siteProvider = Persistence.Providers.SiteProvider;
            if (!trimedPath.StartsWith(SiteHelper.PREFIX_FRONT_DEBUG_URL, StringComparison.InvariantCultureIgnoreCase))
            {
                #region RequestByHostName
                var host = GetRawHostWithoutPort(_request);
                if (!string.IsNullOrEmpty(trimedPath))
                {
                    var path = trimedPath.Split('/');
                    Site = siteProvider.GetSiteByHostNameNPath(host, path[0]);
                    if (Site != null)
                    {
                        RequestChannel = FrontRequestChannel.HostNPath;
                        sitePath = path[0];
                        //applicationPath = base.ApplicationPath.TrimEnd('/') + "/" + sitePath;

                        if (path.Length == 1)
                        {
                            appRelativeCurrentExecutionFilePath = "~/";
                            RequestUrl = "";
                        }
                        else
                        {
                            RequestUrl = Kooboo.Web.Url.UrlUtility.Combine(path.Skip(1).ToArray());
                            appRelativeCurrentExecutionFilePath = "~/" + RequestUrl;
                        }


                    }
                }
                if (Site == null)
                {
                    Site = siteProvider.GetSiteByHostName(host);
                    if (Site != null)
                    {
                        RequestChannel = FrontRequestChannel.Host;
                        RequestUrl = trimedPath;
                    }
                }
                #endregion
            }
            else
            {
                //dev~site1/index
                var path = trimedPath.Split('/');
                var sitePaths = SiteHelper.SplitFullName(path[0].Substring(SiteHelper.PREFIX_FRONT_DEBUG_URL.Count()));

                Site = siteProvider.Get(Site.ParseSiteFromRelativePath(sitePaths));
                if (Site != null)
                {
                    RequestChannel = FrontRequestChannel.Debug;
                }

                RequestUrl = Kooboo.Web.Url.UrlUtility.Combine(path.Skip(1).ToArray());
                appRelativeCurrentExecutionFilePath = "~/" + RequestUrl;
                //sitePath = path[0];
                //applicationPath = base.ApplicationPath.TrimEnd('/') + "/" + sitePath;
            }

            if (Site != null)
            {
                //set current site repository
                Kooboo.CMS.Content.Models.Repository.Current = Site.GetRepository();

                if (!string.IsNullOrEmpty(RequestUrl))
                {
                    System.Net.HttpStatusCode statusCode;
                    string inputUrl = RequestUrl;
                    if (!string.IsNullOrEmpty(this.Url.Query))
                    {
                        inputUrl = inputUrl + this.Url.Query;
                    }
                    string redirectUrl;
                    if (UrlMapperFactory.Default.Map(Site, inputUrl, out redirectUrl, out statusCode))
                    {
                        UrlRedirect(statusCode, redirectUrl);
                    }

                }
                if (!string.IsNullOrEmpty(Site.Culture))
                {
                    var culture = new CultureInfo(Site.Culture);
                    Thread.CurrentThread.CurrentCulture = culture;
                }
            }

            //decode the request url. for chinese character
            this.RequestUrl = HttpUtility.UrlDecode(this.RequestUrl);

        }

        private void UrlRedirect(System.Net.HttpStatusCode statusCode, string redirectUrl)
        {
            var response = this.RequestContext.HttpContext.Response;
            response.StatusCode = (int)statusCode;
            if (!Uri.IsWellFormedUriString(redirectUrl, UriKind.Absolute))
            {
                redirectUrl = FrontUrlHelper.WrapperUrl(redirectUrl, this.Site, this.RequestChannel).ToString();
            }
            response.AddHeader("Location", redirectUrl);
            response.AddHeader("Connection", "close");
            response.End();
        }
        //string applicationPath;
        //public override string ApplicationPath
        //{
        //    get
        //    {
        //        return applicationPath;
        //    }
        //}
        string sitePath;
        public virtual string SitePath
        {
            get
            {
                return sitePath;
            }
        }
    }
}
