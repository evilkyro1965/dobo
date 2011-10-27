using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class UrlRedirectsFile : FileResource
    {
        public UrlRedirectsFile(string physicalPath)
            : base(physicalPath)
        {

        }
        public UrlRedirectsFile(Site site)
            : base(site, "UrlRedirects.config")
        {

        }

        public override IEnumerable<string> RelativePaths
        {
            get { yield return ""; }
        }
    }
}
