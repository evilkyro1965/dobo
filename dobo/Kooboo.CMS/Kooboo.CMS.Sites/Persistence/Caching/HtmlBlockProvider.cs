using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Persistence.Caching
{
    public class HtmlBlockProvider : CacheObjectProviderBase<HtmlBlock>, IHtmlBlockProvider
    {
        private IHtmlBlockProvider inner;
        public HtmlBlockProvider(IHtmlBlockProvider inner)
            : base(inner)
        {
            this.inner = inner;
        }
        protected override string GetCacheKey(HtmlBlock o)
        {
            return string.Format("HtmlBlock:{0}", o.Name.ToLower());
        }

        public IQueryable<HtmlBlock> All(Site site)
        {
            return inner.All(site);
        }

        public void Localize(HtmlBlock o, Site targetSite)
        {
            inner.Localize(o, targetSite);
        }
    }
}
