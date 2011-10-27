﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Models;
using Kooboo.Web.Mvc;

namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
	public class ManagerControllerBase : ControllerBase
	{

		public ManagerControllerBase()
		{
			RepositoryManager = ServiceFactory.RepositoryManager;
		}
		public RepositoryManager RepositoryManager { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (Repository == null)
			{
				filterContext.Result = RedirectToAction("index", "home");
			}
			base.OnActionExecuting(filterContext);
		}
		public Repository Repository
		{
			get
			{
				return Repository.Current;
			}
			set
			{
				Repository.Current = value;
			}
		}
		protected override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (filterContext.Result is RedirectToRouteResult)
			{
				((RedirectToRouteResult)filterContext.Result).RouteValues["repositoryName"] = filterContext.RequestContext.GetRequestValue("repositoryName");
			}   

			base.OnResultExecuting(filterContext);
		}

		protected virtual ActionResult RedirectToIndex()
		{
			var routes = this.ControllerContext.RequestContext.AllRouteValues();
			routes.Remove("name");
			return RedirectToAction("Index", routes);
		}
	}
}
