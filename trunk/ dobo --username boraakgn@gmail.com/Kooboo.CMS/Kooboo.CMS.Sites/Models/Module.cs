using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace Kooboo.CMS.Sites.Models
{

    public class ModuleEntry
    {
        public string Controller { get; set; }
        public string Action { get; set; }

        public RouteValueDictionary Defaults { get; set; }
    }

    public class Module
    {
        public string Name { get; set; }

        public ModuleEntry Entry { get; set; }

        #region IPageContent Members


        public ViewCompatibility Compatibility
        {
            get { return ViewCompatibility.All; }
        }

        #endregion

        #region IPageContent Members


        public IEnumerable<string> GetPlugins
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
