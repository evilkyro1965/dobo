using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites
{
    public interface IPath
    {
        string PhysicalPath { get; }
        string VirtualPath { get; }
    }
}
