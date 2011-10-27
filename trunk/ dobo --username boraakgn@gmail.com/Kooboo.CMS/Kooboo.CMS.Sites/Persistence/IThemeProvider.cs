using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;


namespace Kooboo.CMS.Sites.Persistence
{
    public interface IThemeProvider : IProvider<Theme>, IImportProvider<Theme>
    {
        IQueryable<Style> AllStyles(Theme theme);

        ThemeRuleFile GetCssHack(Theme theme);

        IQueryable<ThemeImageFile> AllImages(Theme theme);

        Theme Get(Site site, string name);
    }
}
