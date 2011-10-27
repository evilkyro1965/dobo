using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Persistence
{
    public interface IViewProvider : IProvider<Kooboo.CMS.Sites.Models.View>, ILocalizableProvider<Kooboo.CMS.Sites.Models.View>, IImportProvider<Kooboo.CMS.Sites.Models.View>
    {
        Models.View Copy(Site site, string sourceName, string destName);
    }
}
