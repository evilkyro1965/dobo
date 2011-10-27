using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Kooboo.CMS.Form.Html.Controls
{
    public interface IControl
    {
        string Name { get; }

        string Render(IColumn column);

        bool IsFile { get; }

        string GetValue(object oldValue, string newValue);
    }
}
