using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html.Controls
{
    public class Empty : IControl
    {
        public string Name
        {
            get
            {
                return "Empty";
            }
        }

        public string Render(IColumn column)
        {
            return string.Format(@"<input type=""hidden"" name=""{0}"" value=""@(Model.{0} ?? """")""/>", column.Name);
        }

        public bool IsFile
        {
            get { return false; }
        }

        public string GetValue(object oldValue, string newValue)
        {
            return newValue;
        }
    }
}
