using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;

namespace Kooboo.CMS.Sites.Web
{
    public class FrontUrlRoutingModule : UrlRoutingModule
    {
        protected override void Init(HttpApplication application)
        {
            application.PostResolveRequestCache += new EventHandler(application_PostResolveRequestCache);
        }

        void application_PostResolveRequestCache(object sender, EventArgs e)
        {
            HttpContextBase context = new FrontHttpContextWrapper(((HttpApplication)sender).Context);
            this.PostResolveRequestCache(context);
        }
    }
}
