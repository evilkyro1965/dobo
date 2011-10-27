using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
[assembly: System.Web.PreApplicationStartMethod(typeof(Kooboo.CMS.Content.Persistence.SqlServer.AssemblyInitializer), "Initialize")]
namespace Kooboo.CMS.Content.Persistence.SqlServer
{
    public static class AssemblyInitializer
    {
        public static void Initialize()
        {
            Kooboo.CMS.Content.Persistence.Providers.DefaultProviderFactory = new SqlServer.ProviderFactory();
        }
    }
}
