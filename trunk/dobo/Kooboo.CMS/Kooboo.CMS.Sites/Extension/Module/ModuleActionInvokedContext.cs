using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Kooboo.CMS.Sites.Extension.Module
{
    public class ModuleActionInvokedContext
    {
        public ModuleActionInvokedContext(ControllerContext controllerContext, ActionResult actionResult)
        {
            this.ControllerContext = controllerContext;
            this.ActionResult = actionResult;
        }
        public ControllerContext ControllerContext { get; private set; }
        public ActionResult ActionResult { get; private set; }
    }
}
