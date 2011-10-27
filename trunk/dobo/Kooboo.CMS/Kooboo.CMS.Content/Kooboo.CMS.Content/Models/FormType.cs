using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Models
{
    [Flags]
    public enum FormType
    {
        Grid = 1,
        Create = 2,
        Update = 4,
        Selectable = 8,
        List = 16,
        Detail = 32
    }
}
