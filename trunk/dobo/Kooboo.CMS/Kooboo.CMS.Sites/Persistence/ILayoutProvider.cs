using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Persistence
{
    public interface ILayoutProvider : IProvider<Layout>, IImportProvider<Layout>, ILocalizableProvider<Layout>
    {
        Models.Layout Copy(Site site, string sourceName, string destName);
        //IEnumerable<LayoutSample> AllSamples();
        //LayoutSample GetLayoutSample(string name);
    }
}
