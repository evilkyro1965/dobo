using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Account.Models;

namespace Kooboo.CMS.Account.Persistence.SqlSever.Mapping
{
    public class PermissionMapping : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Permission>
    {
        public PermissionMapping()
        {
            this.HasKey(it => new { it.Id, it.RoleName });

            this.ToTable("CMS_Account_Permissions");
        }
    }
}
