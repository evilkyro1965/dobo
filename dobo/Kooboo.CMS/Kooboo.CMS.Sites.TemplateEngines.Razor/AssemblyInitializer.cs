﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
[assembly: System.Web.PreApplicationStartMethod(typeof(Kooboo.CMS.Sites.TemplateEngines.Razor.AssemblyInitializer), "Initialize")]
namespace Kooboo.CMS.Sites.TemplateEngines.Razor
{
    public static class AssemblyInitializer
    {
        public static void Initialize()
        {
            Kooboo.CMS.Sites.View.TemplateEngines.RegisterEngine(new RazorTemplateEngine());
        }
    }
}
