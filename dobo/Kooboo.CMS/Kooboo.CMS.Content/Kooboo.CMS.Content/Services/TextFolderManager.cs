using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Services
{
    public class TextFolderManager : FolderManager<TextFolder>
    {
        //public IEnumerable<TextFolder> AllTextFolder(Repository repository, string selfFullName = "")
        //{
        //    var all = this.All(repository, "");
        //    return all.Where(o => o.AsActual() is TextFolder && o.FullName != selfFullName).Select(o => (TextFolder)o.AsActual());
        //}
    }
}
