using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Parsers.ThemeRule
{
    public static class ThemeRuleParser
    {
        public static IThemeRuleParser Parser = new RegularCssHackFileParser();
        public static IEnumerable<ThemeFile> Parse(Theme theme, out string themeRuleBody)
        {
            IEnumerable<ThemeFile> themeFiles = Persistence.Providers.ThemeProvider.AllStyles(theme.LastVersion());
            ThemeRuleFile cssHackFile = new ThemeRuleFile(theme);
            if (!cssHackFile.Exists())
            {
                themeRuleBody = "";
                return themeFiles;
            }

            string themeBaseUrl = Kooboo.Web.Url.UrlUtility.ResolveUrl(theme.VirtualPath);
            var themeRuleFiles = Parser.Parse(cssHackFile.Read(), (fileVirtualPath) => Kooboo.Web.Url.UrlUtility.Combine(themeBaseUrl, fileVirtualPath), out themeRuleBody);

            return themeFiles.Where(it => !themeRuleFiles.Any(cf => cf.EqualsOrNullEmpty(it.FileName, StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}
