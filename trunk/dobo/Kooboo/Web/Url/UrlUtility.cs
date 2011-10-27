﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace Kooboo.Web.Url
{
    public static class UrlUtility
    {
        public static string UrlSeparatorChar = "/";
        /// <summary>
        /// Combines the specified virtual paths. Like of <see cref="System.IO.Path.Combine"/>.
        /// </summary>
        /// <param name="virtualPaths">The virtual paths.<example>string[] {"path1","path2","path3"}</example></param>
        /// <returns> <value>path1/path2/path3</value> </returns>
        public static string Combine(params string[] virtualPaths)
        {
            if (virtualPaths.Length < 1)
                return null;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < virtualPaths.Length; i++)
            {
                var path = virtualPaths[i];
                if (String.IsNullOrEmpty(path))
                    continue;

                if (i > 0)
                {
                    // Not first one trim start '/'
                    path = path.TrimStart('/');
                    builder.Append("/");
                }
                if (i < virtualPaths.Length - 1)
                {
                    // Not last one trim end '/'
                    path = path.TrimEnd('/');
                }
                if (!path.Contains('/'))
                {
                    path = Uri.EscapeUriString(path);
                }
                builder.Append(path);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Return full url start with http.
        /// </summary>
        /// <param name="relativeUrl">Url start with "~"</param>
        /// <returns></returns>
        public static string ToHttpAbsolute(string relativeUrl)
        {
            UriBuilder url = new UriBuilder(HttpContext.Current.Request.Url);
            var queryIndex = relativeUrl.IndexOf("?");
            if (queryIndex != -1 && queryIndex != relativeUrl.Length)
            {
                url.Query = relativeUrl.Substring(queryIndex + 1);
                relativeUrl = relativeUrl.Substring(0, queryIndex);
            }

            url.Path = VirtualPathUtility.ToAbsolute(relativeUrl);

            return url.Uri.AbsoluteUri.ToString();
        }

        /// <summary>
        /// Equal to <see cref="System.Web.Mvc.UrlHelper.Content"/>  AND <see cref="System.Web.UI.Control.ResolveUrl"/>
        /// <remarks>
        /// Independent of HttpContext
        /// </remarks>
        /// </summary>
        /// <param name="relativeUrl">The URL. <example>~/a/b</example> </param>
        /// <returns><value>/a/b OR {virtualPath}/a/b</value></returns>
        public static string ResolveUrl(string relativeUrl)
        {
            if (HttpContext.Current != null && relativeUrl.StartsWith("~"))
            {
                //For FrontHttpRequestWrapper
                string applicationPath = HttpContext.Current.Items["ApplicationPath"] != null ? HttpContext.Current.Items["ApplicationPath"].ToString() : HttpContext.Current.Request.ApplicationPath;
                if (applicationPath == "/")
                {
                    return relativeUrl.Remove(0, 1);
                }
                else
                {
                    return applicationPath + relativeUrl.Remove(0, 1);
                }
            }
            return relativeUrl;
        }

        /// <summary>
        /// Wrap <see cref="System.Web.HttpServerUtilityBase.MapPath"/>
        /// </summary>
        /// <remarks>
        /// Independent of HttpContext
        /// </remarks>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string MapPath(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            var physicalPath = System.Web.Hosting.HostingEnvironment.MapPath(url);
            if (physicalPath == null)
            {
                physicalPath = url.TrimStart('~').Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

                physicalPath = Path.Combine(Kooboo.Settings.BaseDirectory, physicalPath);
            }
            return physicalPath;
        }

        public static string CombineQueryString(string baseUrl, params string[] queries)
        {
            string query = String.Join(String.Empty, queries).TrimStart('?');
            if (!String.IsNullOrEmpty(query))
            {
                if (baseUrl.Contains('?'))
                {
                    return baseUrl + query;
                }
                else
                {
                    return baseUrl + "?" + query;
                }
            }
            else
            {
                return baseUrl;
            }
        }

        public static string RemoveQuery(string url, params string[] names)
        {
            string result = url;
            foreach (var each in names)
            {
                result = ReplaceQuery(url, each, String.Empty);
            }
            return result;
        }

        public static string ReplaceQuery(string url, string name, string newQuery)
        {
            return Regex.Replace(url, String.Format(@"&?\b{0}=[^&]*", name), newQuery, RegexOptions.IgnoreCase);
        }

        public static string EnsureHttpHead(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            if (!Regex.IsMatch(url, @"^\w"))
                return url;

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;

            return "http://" + url;
        }


        public static string GetVirtualPath(string physicalPath)
        {
            string rootpath = MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");
            return "~/" + physicalPath;
        }


        public static string AddQueryParam(
    this string source, string key, string value)
        {
            string delim;
            if ((source == null) || !source.Contains("?"))
            {
                delim = "?";
            }
            else if (source.EndsWith("?") || source.EndsWith("&"))
            {
                delim = string.Empty;
            }
            else
            {
                delim = "&";
            }

            return source + delim + HttpUtility.UrlEncode(key)
                + "=" + HttpUtility.UrlEncode(value);
        }
    }
}
