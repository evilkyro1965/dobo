using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class UrlKeyMapsFile : FileResource
    {
        public UrlKeyMapsFile(string physicalPath)
            : base(physicalPath)
        {

        }
        public UrlKeyMapsFile(Site site)
            : base(site, "UrlKeyMaps.config")
        {

        }

        public override IEnumerable<string> RelativePaths
        {
            get { yield return ""; }
        }
    }
}
