using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kooboo.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using Kooboo.Globalization;
using Kooboo.Web.Mvc;
using Kooboo.CMS.Web.Models;
using System.Web.Script.Serialization;
using System.IO;
using System.Security.Permissions;
using System.Security;
namespace Kooboo.CMS.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private class CustomWebFormViewEngine : WebFormViewEngine
        {
            public CustomWebFormViewEngine()
            {
                base.MasterLocationFormats = new string[] { "~/Views/{1}/{0}.master", "~/Views/Shared/{0}.master" };
                base.AreaMasterLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.master", "~/Areas/{2}/Views/Shared/{0}.master", "~/Views/Shared/{0}.master" };
                base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.aspx", "~/Views/{1}/{0}.ascx", "~/Views/Shared/{0}.aspx", "~/Views/Shared/{0}.ascx" };
                base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.aspx", "~/Areas/{2}/Views/{1}/{0}.ascx", "~/Areas/{2}/Views/Shared/{0}.aspx", "~/Areas/{2}/Views/Shared/{0}.ascx", "~/Views/Shared/{0}.ascx" };
                base.PartialViewLocationFormats = base.ViewLocationFormats;
                base.AreaPartialViewLocationFormats = base.AreaViewLocationFormats;

            }
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            Kooboo.Web.Mvc.Routing.RouteTableRegister.RegisterRoutes(routes);

            ModelMetadataProviders.Current = new KoobooDataAnnotationsModelMetadataProvider();

            ModelValidatorProviders.Providers.RemoveAt(0);
            ModelValidatorProviders.Providers.Insert(0, new KoobooDataAnnotationsModelValidatorProvider());

            //Job.Jobs.Instance.AttachJob("test", new Kooboo.Job.TestJob(), 1000, null, true);
        }

        protected void Application_Start()
        {
            //ViewEngine for module.            
            ViewEngines.Engines.Insert(0, new Kooboo.CMS.Sites.Extension.Module.ModuleRazorViewEngine());
            ViewEngines.Engines.Insert(1, new Kooboo.CMS.Sites.Extension.Module.ModuleWebFormViewEngine());
            ViewEngines.Engines.Insert(2, new CustomWebFormViewEngine());

            AreaRegistration.RegisterAllAreas();

            ModelBinders.Binders.Add(typeof(Dynamic.DynamicDictionary), new DynamicDictionaryBinder());

            RegisterRoutes(RouteTable.Routes);

            Kooboo.CMS.Content.Persistence.Providers.RepositoryProvider.TestDbConnection();
        }
    }
}