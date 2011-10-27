using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kooboo.CMS.Sites.Extension.Module;
using Kooboo.Web.Mvc;

namespace Kooboo.CMS.ModuleTemplate
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("{*images}", new {
               images=".*\\.(ico|jpg|png)(/.*)?"
            });

            routes.MapRoute(
               "Admin", // Route name
               "Admin", // URL with parameters
               new { controller = "Scaffold", action = "Admin" } // Parameter defaults
               , null
               , new[] { "Kooboo.CMS.ModuleTemplate.Controllers", "Kooboo.Web.Mvc", "Kooboo.Web.Mvc.WebResourceLoader" }
           );

            routes.MapRoute(
                "Page", // Route name
                "{*PageUrl}", // URL with parameters
                new { controller = "Scaffold", action = "Entry" } // Parameter defaults
                ,null
                , new[] { "Kooboo.CMS.ModuleTemplate.Controllers", "Kooboo.Web.Mvc", "Kooboo.Web.Mvc.WebResourceLoader" }
            );

            ModulePath.PhysicalPathAccessor = moduleName => AppDomain.CurrentDomain.BaseDirectory;
            ModulePath.VirtualPathAccessor = moduleName => "~/";
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            ModelMetadataProviders.Current = new KoobooDataAnnotationsModelMetadataProvider();

            ModelValidatorProviders.Providers.RemoveAt(0);
            ModelValidatorProviders.Providers.Insert(0, new KoobooDataAnnotationsModelValidatorProvider());
        }
    }
}