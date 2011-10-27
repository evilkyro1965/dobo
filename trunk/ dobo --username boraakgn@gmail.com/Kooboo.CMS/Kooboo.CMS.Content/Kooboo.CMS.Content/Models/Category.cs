using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Models
{
    public class Category
    {
        public string ContentUUID { get; set; }
        public string CategoryFolder { get; set; }
        public string CategoryUUID { get; set; }

        public override bool Equals(object obj)
        {
            var c = (Category)obj;
            if (this.ContentUUID.EqualsOrNullEmpty(c.ContentUUID, StringComparison.CurrentCultureIgnoreCase) &&
                this.CategoryFolder.EqualsOrNullEmpty(c.CategoryFolder, StringComparison.CurrentCultureIgnoreCase) &&
                this.CategoryUUID.EqualsOrNullEmpty(c.CategoryUUID, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
