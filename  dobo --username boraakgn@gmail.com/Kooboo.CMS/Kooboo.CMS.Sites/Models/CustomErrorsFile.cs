using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class CustomErrorsFile : FileResource
    {
        public CustomErrorsFile(string physicalPath)
            : base(physicalPath)
        {

        }
        public CustomErrorsFile(Site site)
            : base(site, "CustomErrors.config")
        {

        }

        public override IEnumerable<string> RelativePaths
        {
            get { yield return ""; }
        }
    }
}
