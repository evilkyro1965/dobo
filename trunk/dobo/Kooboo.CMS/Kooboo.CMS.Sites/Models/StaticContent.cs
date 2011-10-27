using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class StaticContent
    {
        #region IPageContent Members

        public string Name
        {
            get;
            set;
        }

        #endregion

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
