using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace Kooboo
{
    /// <summary>
    /// To reader system resources
    /// </summary>
    public static class SR
    {
        static SR()
        {
            System_Web_Resources = new ResourceManager("System.Web", typeof(System.Web.HttpContext).Assembly);
            System_Web_Mvc_Resources = new ResourceManager("System.Web.Mvc.Resources.MvcResources", typeof(System.Web.Mvc.Controller).Assembly);
            System_ComponentModel_DataAnnotations_Resources = new ResourceManager("System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources", typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly);            
            System_Linq_Resources = new ResourceManager("System.Linq", typeof(System.Linq.Enumerable).Assembly);
        }
        public static ResourceManager System_Linq_Resources { get; private set; }
        public static ResourceManager System_Web_Resources { get; private set; }
        public static ResourceManager System_Web_Mvc_Resources { get; private set; }
        public static ResourceManager System_ComponentModel_DataAnnotations_Resources { get; private set; }
    }
}
