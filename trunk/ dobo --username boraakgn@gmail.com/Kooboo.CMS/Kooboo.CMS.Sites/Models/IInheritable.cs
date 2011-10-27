using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public interface IInheritable
    {
        Site Site { get; }
        bool HasParentVersion();
    }
    public interface IInheritable<T> : IInheritable
    {
        T LastVersion();
    }

    public static class IInheritableExtensions
    {

        public static bool IsLocalized(this IInheritable inheritable, Site site)
        {
            return inheritable.Site.Equals(site);
        }
    }
}
