using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    /// <summary>
    /// The content can be added to page <example>View,Module</example>
    /// </summary>
    public interface IPositionView
    {
        string Name { get; set; }
        ViewCompatibility Compatibility { get; }

        IEnumerable<string> GetPlugins { get; }
    }
}
