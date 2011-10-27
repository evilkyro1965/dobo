﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Kooboo.Web.Mvc
{
    public class AutoCompleteController : Controller
    {
        public ActionResult Index(string dataSourceType, string term)
        {
            if (string.IsNullOrEmpty(dataSourceType))
            {
                throw new ArgumentNullException("The dataSourceType can not be null.");
            }
            var type = Type.GetType(dataSourceType);
            if (type == null)
            {
                throw new Exception(string.Format("The type of \"{0}\" can not be found.", dataSourceType));
            }
            var dataSource = (ISelectListDataSource)Activator.CreateInstance(type);
            return Json(dataSource.GetSelectListItems(
                ControllerContext.RequestContext, term)
                    .Select(it => new { label = it.Text, category = it.Value }).ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}
