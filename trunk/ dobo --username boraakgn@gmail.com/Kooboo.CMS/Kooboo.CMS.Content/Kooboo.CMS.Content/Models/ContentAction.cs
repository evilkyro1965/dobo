using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Models
{
    [Flags]
    public enum ContentAction
    {
        Add = 0x1,
        Update = 0x2,
        Delete = 0x4,
        PreAdd = 0x8,
        PreUpdate = 16,
        PreDelete = 32
    }
}
