using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence
{
    public interface IFolderProvider<T> : IProvider<T>, IImportProvider<Folder>
        where T : Folder
    {
        IQueryable<T> ChildFolders(T parent);
    }

    public interface ITextFolderProvider : IFolderProvider<TextFolder>
    {
        IQueryable<TextFolder> BySchema(Schema schema);
    }
    public interface IMediaFolderProvider : IFolderProvider<MediaFolder>
    {

    }
}
