using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Account.Models;

namespace Kooboo.CMS.Account.Persistence.SqlSever.Mapping
{
    public class UserMapping : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<User>
    {
        public UserMapping()
        {
            this.HasKey(it => it.Email);
            this.Ignore(it => it.Password);
            this.Ignore(it => it.IsDummy);
            this.Property(it => it.CustomFieldsXml).HasColumnType("text");

            this.ToTable("CMS_Account_Users");
        }
    }
}
