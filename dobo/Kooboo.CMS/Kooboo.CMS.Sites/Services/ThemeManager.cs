using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.CMS.Sites.Persistence.FileSystem;

namespace Kooboo.CMS.Sites.Services
{
    public class ThemeManager : PathResourceManagerBase<Theme>
    {
        internal ThemeManager()
        { }
        public IThemeProvider ThemeRepository
        {
            get { return (IThemeProvider)base.Provider; }
        }

        public override Theme Get(Site site, string name)
        {
            return ThemeRepository.Get(site, name);
        }


        public void Localize(string name, string fromSite, Site targetSite)
        {
            var sourceSite = ServiceFactory.SiteManager.GetSite(SiteHelper.SplitFullName(fromSite));
            var source = new Models.Theme(sourceSite, name);
            ILocalizableHelper.Localize<Models.Theme>(source, targetSite);
        }

        #region  Styles
        public IEnumerable<Style> AllStyles(Site site, string themeName)
        {
            return ThemeRepository.AllStyles(new Theme() { Name = themeName, Site = site });
        }
        public Style GetStyle(Site site, string themeName, string fileName)
        {
            var theme = new Theme() { Name = themeName, Site = site };
            var themeFile = new Style(theme, fileName);
            themeFile.Body = themeFile.Read();
            return themeFile;
        }
        public void CreateStyle(Site site, string themeName, string fileName, string body)
        {
            var theme = new Theme(site, themeName);
            var themeFile = new Style(theme, fileName);
            if (themeFile.Exists())
            {
                throw new ItemAlreadyExistsException();
            }
            themeFile.Save(body);
        }

        public void EditStyle(Site site, string themeName, string fileName, string body)
        {
            var theme = new Theme(site, themeName);
            var themeFile = new Style(theme, fileName);
            themeFile.Save(body);
        }
        public void DeleteStyle(Site site, string themeName, string fileName)
        {
            var theme = new Theme(site, themeName);
            var themeFile = new Style(theme, fileName);
            themeFile.Delete();
        }
        #endregion

        #region Images
        public IEnumerable<ThemeImageFile> AllImage(Theme theme)
        {
            return ThemeRepository.AllImages(theme);
        }
        public void DeleteImage(Site site, string themeName, string fileName)
        {
            Theme theme = new Theme(site, themeName);
            ThemeImageFile image = new ThemeImageFile(theme, fileName);
            if (!image.Exists())
            {
                throw new FriendlyException("image:" + fileName + " does not exist!");
            }
            image.Delete();
        }
        public void SaveImage(Site site, string themeName, string fileName, Stream stream)
        {
            Theme theme = new Theme(site, themeName);
            ThemeImageFile image = new ThemeImageFile(theme, fileName);
            image.Save(stream);
        }
        #endregion

        #region Rules
        public ThemeRuleFile GetRules(Site site, string themeName)
        {
            Theme theme = new Theme(site, themeName);
            var themeRuleFile = new ThemeRuleFile(theme);
            themeRuleFile.Read();

            return themeRuleFile;
        }
        public void SaveRules(Site site, string themeName, string rulesBody)
        {
            Theme theme = new Theme(site, themeName);
            var themeRuleFile = new ThemeRuleFile(theme) { Body = rulesBody };
            themeRuleFile.Save();
        }
        #endregion
    }
}
