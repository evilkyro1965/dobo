using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Kooboo.Web.Mvc
{
    public static class ViewContextExtensions
    {
        public static bool IsHandledBy<TController>(this ViewContext context)
        {
            return context.Controller is TController;
        }
    }
}
