using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.CMS.Account.Persistence.SqlSever;
[assembly: System.Web.PreApplicationStartMethod(typeof(Kooboo.CMS.Account.Persistence.SqlSever.AssemblyInitializer), "Initialize")]
namespace Kooboo.CMS.Account.Persistence.SqlSever
{
    public static class AssemblyInitializer
    {
        public static void Initialize()
        {
            Kooboo.CMS.Account.Persistence.RepositoryFactory.Factory = new Kooboo.CMS.Account.Persistence.SqlSever.RepositoryFactory();
        }
    }
}
