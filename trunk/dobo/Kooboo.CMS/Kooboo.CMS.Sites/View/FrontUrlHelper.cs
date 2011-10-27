//# define Page_Trace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Web;
using Kooboo.Globalization;
namespace Kooboo.CMS.Sites.View
{
    public class InvalidPageRouteException : Exception, IKoobooException
    {
        public InvalidPageRouteException(Page page)
            : base(
            string.Format("This exception is caused by the wrong setting of the url route of this page. Please check this setting to correct the error. Page name:{0}".Localize(), page))
        {
            this.Page = page;
        }
        public Page Page { get; private set; }
    }

    public class FrontUrlHelper
    {
        public FrontUrlHelper(PageRequestContext pageRequestContext, UrlHelper url)
        {
            this.PageRequestContext = pageRequestContext;
            this.Url = url;
        }

        public PageRequestContext PageRequestContext { get; private set; }

        public UrlHelper Url { get; private set; }

        public static IHtmlString WrapperUrl(string url, Site site, FrontRequestChannel channel)
        {
            if (string.IsNullOrEmpty(url))
            {
                return new HtmlString(url);
            }
            var applicationPath = HttpContext.Current.Request.ApplicationPath.TrimStart(new char[] { '/' });
            if (!url.StartsWith("/") && !string.IsNullOrEmpty(applicationPath))
            {
                url = "/" + applicationPath + "/" + url;
            }
            var urlSplit = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var sitePath = "";
            if (channel == FrontRequestChannel.HostNPath)
            {
                sitePath = site.AsActual().SitePath;
            }
            else if (channel == FrontRequestChannel.Debug || channel == FrontRequestChannel.Design || channel == FrontRequestChannel.Unknown)
            {
                sitePath = SiteHelper.PREFIX_FRONT_DEBUG_URL + site.FullName;
            }
            IEnumerable<string> urlPaths = urlSplit;
            if (!string.IsNullOrEmpty(sitePath))
            {
                if (urlSplit.Length > 0 && applicationPath.EqualsOrNullEmpty(urlSplit[0], StringComparison.OrdinalIgnoreCase))
                {
                    urlPaths = new string[] { applicationPath, sitePath }.Concat(urlSplit.Skip(1));
                }
                else
                {
                    urlPaths = new string[] { sitePath }.Concat(urlSplit);
                }
            }

            url = "/" + string.Join("/", urlPaths.ToArray());
            return new HtmlString(url);
        }

        public IHtmlString WrapperUrl(string url)
        {
            return FrontUrlHelper.WrapperUrl(url, this.PageRequestContext.Site, this.PageRequestContext.RequestChannel);
        }

        #region ResourceUrl
        /// <summary>
        ///  the site scripts URL.
        /// </summary>
        /// <returns></returns>
        public IHtmlString SiteScriptsUrl()
        {
            Site site = this.PageRequestContext.Site;
            return this.WrapperUrl(this.Url.Action("scripts", "Resource", new { version = site.Version, area = "" }));
        }

        /// <summary>
        /// the site theme URL.
        /// </summary>
        /// <returns></returns>
        public IHtmlString SiteThemeUrl()
        {
            Site site = this.PageRequestContext.Site;
            return this.WrapperUrl(this.Url.Action("theme", "Resource", new { name = site.Theme, version = site.Version, area = "" }));
        }
        /// <summary>
        /// Get the media content url.
        /// </summary>
        /// <param name="fullFoldername"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IHtmlString MediaContentUrl(string fullFoldername, string fileName)
        {
            var mediaFolder = new Kooboo.CMS.Content.Models.MediaFolder(Kooboo.CMS.Content.Models.Repository.Current, fullFoldername);
            var folderPath = new Kooboo.CMS.Content.Models.Paths.FolderPath(mediaFolder);
            var contentUrl = Kooboo.Web.Url.UrlUtility.Combine(folderPath.VirtualPath, fileName);
            return new HtmlString(this.Url.Content(contentUrl));
        }

        /// <summary>
        /// The file URL.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <returns></returns>
        public IHtmlString FileUrl(string relativeFilePath)
        {
            Site site = this.PageRequestContext.Site;
            var dir = Path.GetDirectoryName(relativeFilePath);
            CustomFile file;
            if (string.IsNullOrEmpty(dir))
            {
                file = new CustomFile(site, relativeFilePath);
            }
            else
            {
                CustomDirectory customDir = new CustomDirectory(site, dir).LastVersion();
                file = new CustomFile(customDir, Path.GetFileName(relativeFilePath));
            }
            file = file.LastVersion();
            return new HtmlString(this.Url.Content(file.VirtualPath));
        }

        /// <summary>
        /// Modules the scripts URL.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns></returns>
        public IHtmlString ModuleScriptsUrl(string moduleName)
        {
            Site site = this.PageRequestContext.Site;
            return this.WrapperUrl(this.Url.Action("ModuleScripts", "Resource", new { moduleName = moduleName, version = site.Version, area = "" }));
        }
        /// <summary>
        /// Modules the theme URL.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="themeName">Name of the theme.</param>
        /// <returns></returns>
        public IHtmlString ModuleThemeUrl(string moduleName, string themeName)
        {
            Site site = this.PageRequestContext.Site;
            return this.WrapperUrl(this.Url.Action("ModuleTheme", "Resource", new { moduleName = moduleName, themeName = themeName, version = site.Version, area = "" }));
        }
        /// <summary>
        /// Resizes the image URL.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public IHtmlString ResizeImageUrl(string imagePath, int width, int height)
        {
            return ResizeImageUrl(imagePath, width, height, null, null);
        }
        /// <summary>
        /// Resizes the image URL.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="preserverAspectRatio">The preserver aspect ratio.</param>
        /// <param name="quality">The quality.</param>
        /// <returns></returns>
        public IHtmlString ResizeImageUrl(string imagePath, int width, int height, bool? preserverAspectRatio, int? quality)
        {
            return this.WrapperUrl(this.Url.Action("ResizeImage", "Resource", new { url = imagePath, area = "", width = width, height = height, preserverAspectRatio = preserverAspectRatio, quality = quality }));
        }

        /// <summary>
        /// the file URL under the theme of current site.
        /// </summary>
        /// <param name="relativeUrl">The relative URL.<example>images/logo.png</example></param>
        /// <returns></returns>
        public IHtmlString ThemeFileUrl(string relativeUrl)
        {
            var site = this.PageRequestContext.Site;
            IHtmlString url = new HtmlString("");
            if (!string.IsNullOrEmpty(site.Name))
            {
                Theme theme = new Theme(site, site.Theme).LastVersion();
                var themeFileUrl = Kooboo.Web.Url.UrlUtility.Combine(theme.VirtualPath, relativeUrl);
                url = new HtmlString(this.Url.Content(themeFileUrl));
            }

            return url;

        }
        #endregion

        #region PageUrl
        public IHtmlString PageUrl(string urlMapKey)
        {
            //System.Diagnostics.Contracts.Contract.Requires(!string.IsNullOrEmpty(urlMapKey));

            return this.PageUrl(urlMapKey, null);
        }

        public IHtmlString PageUrl(string urlMapKey, object values)
        {
#if Page_Trace
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            var url = GeneratePageUrl(urlMapKey, values, (site, key) => null);

#if Page_Trace
            stopwatch.Stop();
            HttpContext.Current.Response.Write(string.Format("PageUrl,{0}.</br>", stopwatch.Elapsed));
#endif
            return url;
        }

        #endregion

        internal IHtmlString GeneratePageUrl(string urlMapKey, object values, Func<Site, string, Page> findPage)
        {

            //System.Diagnostics.Contracts.Contract.Requires(!string.IsNullOrEmpty(urlMapKey));
            var site = this.PageRequestContext.Site;

            Page page = null;
            var urlKeyMap = Persistence.Providers.UrlKeyMapProvider.Get(new UrlKeyMap(site, urlMapKey));
            if (urlKeyMap != null)
            {
                if (!string.IsNullOrEmpty(urlKeyMap.PageFullName))
                {
                    page = new Page(site, PageHelper.SplitFullName(urlKeyMap.PageFullName).ToArray());
                }
                else
                    page = null;
            }
            if (page == null)
            {
                page = new Page(site, PageHelper.SplitFullName(urlMapKey).ToArray()).LastVersion();
                if (!page.Exists())
                {
                    page = findPage(site, urlMapKey);
                    string pageFullName = "";
                    if (page != null && page.Exists())
                    {
                        pageFullName = page.FullName;
                    }
                    if (urlKeyMap == null)
                        Services.ServiceFactory.UrlKeyMapManager.Add(site, new UrlKeyMap(site, urlMapKey) { PageFullName = pageFullName });
                }
            }

            if (page != null && page.Exists())
            {

                var url = GeneratePageUrl(page, values);

                return url;
            }
            else
            {
                return new HtmlString("");
            }
        }

        internal IHtmlString GeneratePageUrl(Page page, object values)
        {
            return GeneratePageUrl(this.Url, this.PageRequestContext.Site, page, values, this.PageRequestContext.RequestChannel);
        }
        public static IHtmlString GeneratePageUrl(UrlHelper urlHelper, Site site, Page page, object values, FrontRequestChannel channel)
        {
            RouteValueDictionary routeValues = RouteValuesHelpers.GetRouteValues(values);

            page = page.AsActual();

            var pageRoute = page.Route.ToMvcRoute();

            routeValues = RouteValuesHelpers.MergeRouteValues(pageRoute.Defaults, routeValues);

            var routeVirtualPath = pageRoute.GetVirtualPath(urlHelper.RequestContext, routeValues);
            if (routeVirtualPath == null)
            {
                throw new InvalidPageRouteException(page);
            }
            //string contentUrl = routeVirtualPath.VirtualPath;//don't decode the url. why??
            //if do not decode the url, the route values contains Chinese character will cause bad request.
            string contentUrl = HttpUtility.UrlDecode(routeVirtualPath.VirtualPath);
            string pageUrl = contentUrl;
            if (!string.IsNullOrEmpty(contentUrl) || (string.IsNullOrEmpty(pageUrl) && !page.IsDefault))
            {
                pageUrl = Kooboo.Web.Url.UrlUtility.Combine(page.VirtualPath, contentUrl);
            }
            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = urlHelper.Content("~/");
            }
            else
            {
                pageUrl = HttpUtility.UrlDecode(
                urlHelper.RouteUrl("Page", new { PageUrl = new HtmlString(pageUrl) }));
            }
            var url = FrontUrlHelper.WrapperUrl(pageUrl, site, channel);

            return url;
        }

        public static IHtmlString Preview(UrlHelper urlHelper, Site site, Page page)
        {
            return Preview(urlHelper, site, page, null);
        }
        public static IHtmlString Preview(UrlHelper urlHelper, Site site, Page page, object values)
        {
            page = page.AsActual();
            var pageUrl = urlHelper.Content("~/");
            if (page != null && !page.IsDefault)
            {
                pageUrl = urlHelper.Content("~/" + page.VirtualPath);
            }
            var previewUrl = FrontUrlHelper.WrapperUrl(pageUrl, site, FrontRequestChannel.Unknown).ToString();
            if (values != null)
            {
                RouteValueDictionary routeValues = new RouteValueDictionary(values);

                foreach (var item in routeValues)
                {
                    if (item.Value != null)
                    {
                        previewUrl = Kooboo.Web.Url.UrlUtility.AddQueryParam(previewUrl, item.Key, item.Value.ToString());
                    }
                }
            }
            return new HtmlString(previewUrl);
        }
        // Using the first domain setting as preview link.
        //public static IHtmlString Preview(UrlHelper urlHelper, Site site, Page page, object values)
        //{
        //    site = site.AsActual();
        //    var siteDomain = site.Domains == null ? "" : site.Domains.Where(it => !string.IsNullOrEmpty(it)).FirstOrDefault();
        //    if (!string.IsNullOrEmpty(siteDomain))
        //    {
        //        var pageUrl = "/";
        //        if (page != null && !page.IsDefault)
        //        {
        //            pageUrl = "/" + page.VirtualPath;
        //        }
        //        if (!string.IsNullOrEmpty(site.SitePath))
        //        {
        //            pageUrl = "/" + site.SitePath + pageUrl;
        //        }
        //        if (urlHelper.RequestContext.HttpContext.Request.ApplicationPath != "/")
        //        {
        //            pageUrl = urlHelper.RequestContext.HttpContext.Request.ApplicationPath + pageUrl;
        //        }
        //        var requestUrl = urlHelper.RequestContext.HttpContext.Request.Url;
        //        return new HtmlString(requestUrl.Scheme + "://" + siteDomain + (requestUrl.Port == 80 ? "" : ":" + requestUrl.Port.ToString()) + pageUrl);
        //    }
        //    else
        //    {
        //        page = page.AsActual();
        //        var pageUrl = urlHelper.Content("~/");
        //        if (page != null && !page.IsDefault)
        //        {
        //            pageUrl = urlHelper.Content("~/" + page.VirtualPath);
        //        }
        //        return FrontUrlHelper.WrapperUrl(pageUrl, site, FrontRequestChannel.Unknown);
        //    }
        //    //return GeneratePageUrl(urlHelper, site, page, values, FrontRequestChannel.Debug);
        //}

        #region ViewUrl
        public IHtmlString ViewUrl(string viewName)
        {
            System.Diagnostics.Contracts.Contract.Requires(!string.IsNullOrEmpty(viewName));

            return ViewUrl(viewName, null);
        }

        public IHtmlString ViewUrl(string viewName, object values)
        {
            return GeneratePageUrl(viewName, values, (site, key) =>
            {
                var pages = Persistence.Providers.PageProvider.All(site);
                foreach (var page in pages)
                {
                    var result = FindPage(page, key);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            });
        }
        private static Page FindPage(Page page, string viewName)
        {
            var actualPage = page.AsActual();
            if (actualPage != null && actualPage.PagePositions.Where(it => it is ViewPosition && ((ViewPosition)it).ViewName.EqualsOrNullEmpty(viewName, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
            {
                return actualPage;
            }
            var childPages = Persistence.Providers.PageProvider.ChildPages(page);
            foreach (var child in childPages)
            {
                var result = FindPage(child, viewName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        #endregion
    }
}
