using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.Web.Mvc.WebResourceLoader;
using System.ComponentModel.Composition;
using Kooboo.IO;
using Kooboo.CMS.Sites.Services;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    [Export(typeof(IThemeProvider))]
    public class ThemeProvider : IThemeProvider
    {
        #region IThemeRepository Members
        public IQueryable<Style> AllStyles(Theme theme)
        {
            return AllStylesEnumerable(theme).AsQueryable();
        }
        public IEnumerable<Style> AllStylesEnumerable(Theme theme)
        {
            if (theme.Exists())
            {
                ThemeFile dummy = new ThemeFile(theme, "dummy");
                var baseDir = dummy.BasePhysicalPath;
                if (Directory.Exists(baseDir))
                {
                    var fileNames = EnumerateCssFiles(baseDir);

                    fileNames = FileOrderHelper.OrderFiles(baseDir, fileNames);

                    return fileNames.Select(it => new Style(theme, it));
                }
            }
            return new Style[0];
        }
        private IEnumerable<string> EnumerateCssFiles(string dir)
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*.css"))
            {
                yield return Path.GetFileName(file);
            }
        }

        public ThemeRuleFile GetCssHack(Theme theme)
        {
            return new ThemeRuleFile(theme.LastVersion());
        }
        public IQueryable<ThemeImageFile> AllImages(Theme theme)
        {
            return AllImagesEnumerable(theme).AsQueryable();
        }

        public IEnumerable<ThemeImageFile> AllImagesEnumerable(Theme theme)
        {
            theme = theme.LastVersion();
            if (theme.Exists())
            {
                ThemeImageFile dummy = new ThemeImageFile(theme, "dummy");
                var baseDir = dummy.BasePhysicalPath;
                if (Directory.Exists(baseDir))
                {
                    foreach (var file in Directory.EnumerateFiles(baseDir))
                    {
                        yield return new ThemeImageFile(theme, Path.GetFileName(file));
                    }
                }
            }
        }


        public IQueryable<Theme> All(Models.Site site)
        {
            return IInheritableHelper.All<Theme>(site).AsQueryable();
        }


        public Theme Get(Site site, string name)
        {
            var all = this.All(site);
            return all.Where(o => o.Name == name && o.Site == site).FirstOrDefault();
        }


        #endregion

        public void Add(Theme item)
        {
            IOUtility.EnsureDirectoryExists(item.PhysicalPath);
        }

        public void Update(Theme @new, Theme old)
        {

        }

        public void Remove(Theme item)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(item.PhysicalPath);
            di.Delete(true);
        }

        #region IRepository<Theme> Members


        public Theme Get(Theme dummy)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IImportRepository Members

        public void Export(IEnumerable<Theme> sources, System.IO.Stream outputStream)
        {
            ImportHelper.Export(sources, outputStream);
        }
        public void Import(Site site, string destDir, System.IO.Stream zipStream, bool @override)
        {
            ImportHelper.Import(site, destDir, zipStream, @override);
        }
        #endregion
    }
}
