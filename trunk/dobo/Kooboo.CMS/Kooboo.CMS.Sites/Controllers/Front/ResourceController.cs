using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Models;
using Kooboo.IO;
using System.Web;
using System.IO;
using System.IO.Compression;
using Kooboo.CMS.Sites.Parsers.ThemeRule;
using Kooboo.Web.Mvc.WebResourceLoader;
using Kooboo.Drawing;

namespace Kooboo.CMS.Sites.Controllers.Front
{
    public class ResourceController : FrontControllerBase
    {
        #region Scripts
        public ActionResult Scripts()
        {
            var scripts = Persistence.Providers.ScriptsProvider.All(Site);

            Output(CompressJavascript(scripts.Select(it => it.PhysicalPath)), "text/javascript", 2592000, "*");

            return null;
        }
        private string CompressJavascript(IEnumerable<string> jsFiles)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var file in jsFiles)
            {
                sb.Append(IOUtility.ReadAsString(file));
            }

            return Kooboo.Web.Script.JSMin.Minify(sb.ToString());

        }
        #region ModuleScripts
        public ActionResult ModuleScripts(string moduleName)
        {
            var scripts = Services.ServiceFactory.ModuleManager.AllScripts(moduleName);

            Output(CompressJavascript(scripts.Select(it => it.PhysicalPath)), "text/javascript", 2592000, "*");

            return null;
        }
        #endregion
        #endregion

        #region Themes
        public ActionResult Theme(string name)
        {
            string cssHackBody;
            var styles = ThemeRuleParser.Parse(new Theme(Site, Site.Theme).LastVersion(), out cssHackBody);
            Output(CompressCss(styles), "text/css", 2592000, "*");
            return null;
        }
        private string CompressCss(IEnumerable<IPath> cssFiles)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var file in cssFiles)
            {
                using (FileStream fileStream = new FileStream(file.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var content = fileStream.ReadString();
                    sb.AppendFormat("{0}\n", CSSMinify.Minify(Url, file.VirtualPath, Request.Url.AbsolutePath, content));
                }
            }
            return sb.ToString();
        }
        public ActionResult ModuleTheme(string moduleName, string themeName)
        {
            string themeRuleBody;
            var styles = Services.ServiceFactory.ModuleManager.AllThemeFiles(moduleName, themeName, out themeRuleBody);
            Output(CompressCss(styles), "text/css", 2592000, "*");
            return null;
        }
        #endregion

        #region robots.txt
        public ActionResult RobotsTxt()
        {
            Robots_Txt robotTxt = new Robots_Txt(Site);
            var body = robotTxt.Read();
            return Content(body);
        }
        #endregion

        #region File
        public ActionResult File(string name)
        {
            var dir = Path.GetDirectoryName(name);
            CustomFile file;
            if (string.IsNullOrEmpty(dir))
            {
                file = new CustomFile(Site, name);
            }
            else
            {
                CustomDirectory customDir = new CustomDirectory(Site, dir).LastVersion();
                file = new CustomFile(customDir, Path.GetFileName(name));
            }
            file = file.LastVersion();
            if (file.Exists())
            {
                SetCache(Response, 2592000, "*");
                return File(file.PhysicalPath, IOUtility.MimeType(file.PhysicalPath));
            }
            return null;
        }
        #endregion

        #region Output
        private void Output(string content, string contentType, int cacheDuration, params string[] varyByParams)
        {
            HttpResponseBase response = Response;
            response.ContentType = contentType;
            Stream output = response.OutputStream;

            // Compress
            string acceptEncoding = Request.Headers["Accept-Encoding"];

            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                acceptEncoding = acceptEncoding.ToLowerInvariant();

                if (acceptEncoding.Contains("gzip"))
                {
                    response.AddHeader("Content-encoding", "gzip");
                    output = new GZipStream(output, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("deflate"))
                {
                    response.AddHeader("Content-encoding", "deflate");
                    output = new DeflateStream(output, CompressionMode.Compress);
                }
            }

            // Write output
            using (StreamWriter sw = new StreamWriter(output))
            {
                sw.WriteLine(content);
            }

            SetCache(response, cacheDuration, varyByParams);

        }

        private void SetCache(HttpResponseBase response, int cacheDuration, params string[] varyByParams)
        {
            // Cache
            if (cacheDuration > 0)
            {
                DateTime timestamp = HttpContext.Timestamp;

                HttpCachePolicyBase cache = response.Cache;
                int duration = cacheDuration;

                cache.SetCacheability(HttpCacheability.Public);
                cache.SetExpires(timestamp.AddSeconds(duration));
                cache.SetMaxAge(new TimeSpan(0, 0, duration));
                cache.SetValidUntilExpires(true);
                cache.SetLastModified(timestamp);
                cache.VaryByHeaders["Accept-Encoding"] = true;
                foreach (var p in varyByParams)
                {
                    cache.VaryByParams[p] = true;
                }
                cache.SetOmitVaryStar(true);
            }
        }
        #endregion

        #region ResizeImage
        private static object resizeImageLocker = new object();
        public ActionResult ResizeImage(string url, int width, int height, bool? preserverAspectRatio, int? quality)
        {
            var imageFullPath = Server.MapPath(HttpUtility.UrlDecode(url));
            preserverAspectRatio = preserverAspectRatio ?? true;
            quality = quality ?? 80;
            var cachingPath = GetCachingFilePath(imageFullPath, width, height, preserverAspectRatio.Value, quality.Value);

            if (!System.IO.File.Exists(cachingPath))
            {
                lock (resizeImageLocker)
                {
                    if (!System.IO.File.Exists(cachingPath))
                    {
                        var dir = Path.GetDirectoryName(cachingPath);
                        IOUtility.EnsureDirectoryExists(dir);
                        var success = ImageTools.ResizeImage(imageFullPath, cachingPath, width, height, preserverAspectRatio.Value, quality.Value);
                        if (!success)
                        {
                            cachingPath = imageFullPath;
                        }
                    }
                }

            }
            return File(cachingPath, IOUtility.MimeType(imageFullPath));
        }
        private string GetCachingFilePath(string imagePath, int width, int height, bool preserverAspectRatio, int quality)
        {
            string cms_dataPath = Path.Combine(Kooboo.Settings.BaseDirectory, "Cms_Data");
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string newFileName = fileName + "-" + width.ToString() + "-" + height.ToString() + "-" + preserverAspectRatio.ToString() + "-" + quality.ToString();
            string imageCachingPath = Path.Combine(Kooboo.Settings.BaseDirectory, "Cms_Data", "ImageCaching");
            string cachingPath = imageCachingPath + imagePath.Substring(cms_dataPath.Length);
            return Path.Combine(Path.GetDirectoryName(cachingPath), newFileName + Path.GetExtension(imagePath));
        }
        #endregion
    }
}
