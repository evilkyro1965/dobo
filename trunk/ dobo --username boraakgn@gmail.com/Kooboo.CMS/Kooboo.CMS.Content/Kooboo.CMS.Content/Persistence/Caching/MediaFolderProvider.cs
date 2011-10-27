using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Caching;
namespace Kooboo.CMS.Content.Persistence.Caching
{
    public class MediaFolderProvider : CacheProviderBase<MediaFolder>, IMediaFolderProvider
    {
        private IMediaFolderProvider inner;
        public MediaFolderProvider(IMediaFolderProvider innerProvider)
            : base(innerProvider)
        {
            inner = innerProvider;
        }


        public IQueryable<MediaFolder> ChildFolders(MediaFolder parent)
        {
            return inner.ChildFolders(parent);
        }

        public IQueryable<MediaFolder> All(Repository repository)
        {
            return inner.All(repository);
        }

        public void Export(Repository repository, IEnumerable<Folder> models, System.IO.Stream outputStream)
        {
            inner.Export(repository, models, outputStream);
        }

        public void Import(Repository repository, string destDir, System.IO.Stream zipStream, bool @override)
        {
            inner.Import(repository, destDir, zipStream, @override);
            repository.ClearCache();
        }

        protected override string GetCacheKey(MediaFolder o)
        {
            return "MediaFolder" + o.FullName.ToLower();
        }
    }
}
