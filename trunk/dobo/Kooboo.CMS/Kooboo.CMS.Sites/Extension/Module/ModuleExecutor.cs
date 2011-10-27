using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using Kooboo.CMS.Sites.Controllers.Front;
using Kooboo.CMS.Sites.View;
using System.Web.Routing;
using Kooboo.Web;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public static class ModuleExecutor
    {
        static ModuleControllerActionInvoker actionInvoker = new ModuleControllerActionInvoker();
        public static ModuleActionInvokedContext InvokeAction(ControllerContext controllerContext, Site site, string moduleUrl, ModulePosition modulePosition)
        {
            HttpContext context = HttpContext.Current;
            var moduleSettings = ModuleInfo.GetSiteModuleSettings(modulePosition.ModuleName, site == null ? "" : site.FullName);
            if (modulePosition.Entry != null)
            {
                moduleSettings.Entry = modulePosition.Entry;
            }
            var settings = moduleSettings;
            var positionId = modulePosition.PagePositionId;
            var moduleName = modulePosition.ModuleName;

            ModuleContext moduleContext = ModuleContext.Create(site, moduleName, settings);

            if (string.IsNullOrEmpty(moduleUrl))
            {
                if (settings != null && settings.Entry != null)
                {
                    moduleUrl = GetEntryUrl(context, moduleContext, settings.Entry);
                    if (!string.IsNullOrEmpty(moduleUrl) && !moduleUrl.StartsWith("~"))
                    {
                        moduleUrl = "~" + moduleUrl;
                    }
                }
            }
            if (string.IsNullOrEmpty(moduleUrl))
            {
                moduleUrl = "~/";
            }
            else if (moduleUrl[0] != '~')
            {
                moduleUrl = "~/" + moduleUrl.TrimStart('/');
            }

            var httpContext = new ModuleHttpContext(context
              , new ModuleHttpRequest(context.Request, moduleUrl), new ModuleHttpResponse(context.Response, modulePosition), modulePosition);

            var routeData = moduleContext.RouteTable.GetRouteData(httpContext);

            var requestContext = new ModuleRequestContext(httpContext, routeData, moduleContext) { PageControllerContext = controllerContext };

            string controllerName = requestContext.RouteData.GetRequiredString("controller");
            string actionName = requestContext.RouteData.GetRequiredString("action");
            IController controller = ControllerBuilder.Current.GetControllerFactory().CreateController(requestContext, controllerName);
            if (controller == null)
            {
                throw new Exception(string.Format("The module '{0}' controller for path '{1}' does not found or does not implement IController.", moduleName, moduleUrl));
            }
            if (!(controller is ModuleControllerBase))
            {
                throw new Exception(string.Format("The controller type '{0}' must be inherited from ModuleControllerBase.", controller.GetType().FullName));
            }
            ModuleControllerBase moduleController = (ModuleControllerBase)controller;

            //ControllerContext moduleControllerContext = new ControllerContext(requestContext, moduleController);

            moduleController.Initialize(requestContext);

            var result = actionInvoker.InvokeActionWithoutExecuteResult(moduleController.ControllerContext, actionName);
            if (result == null)
            {
                HandleUnknownAction(moduleController, actionName);
            }
            return result;
        }
        private class GetEntryUrlContextWrapper : HttpContextWrapper
        {
            private class GetEntryUrlRequestWrapper : ModuleHttpRequest
            {
                public GetEntryUrlRequestWrapper(HttpRequest request, string requestUrl)
                    : base(request, requestUrl)
                {

                }
            }
            HttpContext _context;
            string requestUrl;
            public GetEntryUrlContextWrapper(HttpContext context, string requestUrl)
                : base(context)
            {
                _context = context;
                this.requestUrl = requestUrl;
            }
            public override HttpRequestBase Request
            {
                get
                {
                    return new GetEntryUrlRequestWrapper(_context.Request, requestUrl);
                }
            }
        }
        private static string GetEntryUrl(HttpContext context, ModuleContext moduleContext, Entry entry)
        {
            var httpContext = new GetEntryUrlContextWrapper(context, "~/");

            var routeData = moduleContext.RouteTable.GetRouteData(httpContext);

            var requestContext = new ModuleRequestContext(httpContext, routeData, moduleContext);

            UrlHelper url = new UrlHelper(requestContext, moduleContext.RouteTable);

            return url.Action(entry.Action, entry.Controller, entry.Values);
        }
        private static void HandleUnknownAction(Controller controller, string actionName)
        {
            throw new HttpException(0x194, string.Format(SR.System_Web_Mvc_Resources.GetString("Controller_UnknownAction"), new object[] { actionName, controller.GetType().FullName }));
        }

        public static ModuleResultExecutedContext ExecuteActionResult(ModuleActionInvokedContext actionInvokedContext)
        {
            return actionInvoker.ExecuteActionResult(actionInvokedContext);
        }
    }
}
