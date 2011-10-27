using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Models
{
    public interface IRepositoryElement
    {
        string Name { get; set; }
        Repository Repository { get; set; }
    }
}
