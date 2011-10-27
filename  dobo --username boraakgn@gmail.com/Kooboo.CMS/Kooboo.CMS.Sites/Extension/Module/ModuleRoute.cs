﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Kooboo.CMS.Sites.View;
using System.Web.Mvc;
using Kooboo.Web.Mvc;
namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModuleRoute : Route
    {
        public ModuleRoute(string url, IRouteHandler routeHandler) : base(url, routeHandler) { }
        public ModuleRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler) { }
        public ModuleRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler) { }
        public ModuleRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var virtualPath = base.GetVirtualPath(requestContext, values);

            if (requestContext.HttpContext is ModuleHttpContext)
            {
                var moduleUrl = virtualPath.VirtualPath;

                ModuleHttpContext httpContext = (ModuleHttpContext)(requestContext.HttpContext);
                var modulePosition = httpContext.ModulePosition;
                if (Page_Context.Current.Initialized)
                {
                    var modulePositionId = Page_Context.Current.PageRequestContext.ModuleUrlContext.GetModulePositionIdForUrl(modulePosition.ModuleName, modulePosition.PagePositionId, values);

                    var encodedModuleUrl = ModuleUrlHelper.Encode(moduleUrl);

                    var routeValues = Page_Context.Current.PageRequestContext.ModuleUrlContext.GetRouteValuesWithModuleUrl(modulePositionId, encodedModuleUrl, modulePosition.Exclusive);

                    virtualPath.VirtualPath = Page_Context.Current.FrontUrl.PageUrl(Page_Context.Current.PageRequestContext.Page.FullName, routeValues).ToString().TrimStart('/');
                }
                else
                {
                    ModuleRequestContext moduleRequestContext = (ModuleRequestContext)requestContext;

                    UrlHelper pageUrl = new UrlHelper(moduleRequestContext.PageControllerContext.RequestContext);

                    virtualPath.VirtualPath = pageUrl.Action(moduleRequestContext.PageControllerContext.RequestContext.RouteData.Values["action"].ToString()
                        , moduleRequestContext.PageControllerContext.RequestContext.AllRouteValues().Merge("ModuleUrl", moduleUrl)).TrimStart('/');
                }
            }

            return virtualPath;
        }
    }
}