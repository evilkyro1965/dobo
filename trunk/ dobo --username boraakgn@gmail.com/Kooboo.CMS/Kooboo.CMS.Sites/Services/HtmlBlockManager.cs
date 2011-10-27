using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.CMS.Sites.Versioning;

namespace Kooboo.CMS.Sites.Services
{
    public class HtmlBlockManager : PathResourceManagerBase<HtmlBlock>
    {
        public IVersionLogger<Models.HtmlBlock> VersiongLogger
        {
            get
            {
                return VersionManager.ResolveVersionLogger<Models.HtmlBlock>();
            }
        }

        public override HtmlBlock Get(Site site, string name)
        {
            return new HtmlBlock(site, name).AsActual();
        }

        public void Localize(string name, string fromSite, Site targetSite)
        {
            var sourceSite = new Site(SiteHelper.SplitFullName(fromSite));
            var source = new Models.HtmlBlock(sourceSite, name);
            ((IHtmlBlockProvider)Provider).Localize(source, targetSite);
        }

        public override void Add(Site site, HtmlBlock o)
        {
            base.Add(site, o);

            VersionManager.LogVersion(o);
        }
        public override void Update(Site site, HtmlBlock @new, HtmlBlock old)
        {
            base.Update(site, @new, old);

            VersionManager.LogVersion(@new);
        }
    }
}
