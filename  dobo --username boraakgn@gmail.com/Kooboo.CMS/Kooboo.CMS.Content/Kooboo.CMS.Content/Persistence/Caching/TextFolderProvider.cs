using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Caching;
namespace Kooboo.CMS.Content.Persistence.Caching
{
    public class TextFolderProvider : CacheProviderBase<TextFolder>, ITextFolderProvider
    {
        private ITextFolderProvider inner;
        public TextFolderProvider(ITextFolderProvider innerProvider)
            : base(innerProvider)
        {
            inner = innerProvider;
        }
        public IQueryable<TextFolder> BySchema(Schema schema)
        {
            return inner.BySchema(schema);
        }

        public IQueryable<TextFolder> ChildFolders(TextFolder parent)
        {
            return inner.ChildFolders(parent);
        }

        public IQueryable<TextFolder> All(Repository repository)
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

        protected override string GetCacheKey(TextFolder o)
        {
            return "TextFolder:" + o.FullName.ToLower();
        }
    }
}
