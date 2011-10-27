using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Kooboo.CMS.Sites.Models.Resource
{
    public class Scripts : CatalogResource
    {
        public Scripts(Site site)
        {
            this.site = site;
        }
        public override string Name
        {
            get
            {
                return "scripts";
            }
            set
            {
                //base.Name = value;
            }
        }
        public IEnumerable<ScriptFile> ScriptFiles { get; set; }

        public override IEnumerable<string> RelativePaths
        {
            get { return new string[] { }; }
        }

        private Site site;
        public override Site Site
        {
            get { return site; }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            return relativePaths.Take(relativePaths.Count() - 1);
        }
    }
}
