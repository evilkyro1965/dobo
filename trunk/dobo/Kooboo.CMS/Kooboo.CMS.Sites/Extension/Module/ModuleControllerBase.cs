using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModuleControllerBase : Controller
    {
        static ModuleControllerBase()
        {

        }
        public ModuleControllerBase()
        {
            this.TempDataProvider = new ModuleSessionStateTempDataProvider();
        }

        public ModuleContext ModuleContext { get; private set; }
        private bool enableTheming = true;
        public bool EnableTheming
        {
            get
            {
                return enableTheming;
            }
            set
            {
                this.enableTheming = value;
            }
        }
        private bool enableScript = true;
        public bool EnableScript
        {
            get
            {
                return enableScript;
            }
            set
            {
                enableScript = value;
            }
        }

        private UrlHelper _urlHelper;

        public new UrlHelper Url
        {
            get
            {
                if (this._urlHelper == null)
                {
                    this._urlHelper = new UrlHelper(this.ControllerContext.RequestContext, ModuleContext.RouteTable);
                }
                return this._urlHelper;
            }
        }

        public ControllerContext PageControllerContext { get; private set; }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            this.Initialize((ModuleRequestContext)requestContext);                       
        }
        //protected override void Execute(System.Web.Routing.RequestContext requestContext)
        //{
        //    throw new NotSupportedException();
        //}
        public virtual void Initialize(ModuleRequestContext moduleRequestContext)
        {
            this.ModuleContext = moduleRequestContext.ModuleContext;

            this.ControllerContext = new ControllerContext(moduleRequestContext, this);

            this.PageControllerContext = moduleRequestContext.PageControllerContext;


            var valueProvider = new ValueProviderCollection();
            valueProvider.Add(new ModuleFormValueProvider(this.ControllerContext));
            valueProvider.Add(new ModuleQueryStringValueProvider(this.ControllerContext));
            valueProvider.Add(new RouteDataValueProvider(this.ControllerContext));
            if (Kooboo.CMS.Sites.View.Page_Context.Current.PageRequestContext != null)
            {
                valueProvider.Add(new NameValueCollectionValueProvider(Kooboo.CMS.Sites.View.Page_Context.Current.PageRequestContext.AllQueryString, System.Globalization.CultureInfo.InvariantCulture));
            }
            valueProvider.Add(PageControllerContext.Controller.ValueProvider);
            this.ValueProvider = valueProvider;
        }

    }
}
