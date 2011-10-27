using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Caching;
namespace Kooboo.CMS.Sites.Persistence.Caching
{
    public abstract class CacheAllProviderBase<T> : CacheObjectProviderBase<T>
       where T : class,IPersistable
    {
        private IProvider<T> innerRepository;
        public CacheAllProviderBase(IProvider<T> inner)
            : base(inner)
        {
            this.innerRepository = inner;
        }
        public IQueryable<T> All(Models.Site site)
        {
            var cacheKey = GetCacheKey(site);
            var o = (IQueryable<T>)site.ObjectCache().Get(cacheKey);
            if (o == null)
            {
                o = innerRepository.All(site);
                if (o == null)
                {
                    return o;
                }
                site.ObjectCache().Add(cacheKey, o, CacheProviderFactory.DefaultCacheItemPolicy);
            }
            return o;
        }

        public override void Add(T item)
        {
            base.Add(item);
            item.Site.ObjectCache().Remove(GetCacheKey(item.Site));
        }
        public override void Update(T @new, T old)
        {
            base.Update(@new, old);
            @new.Site.ObjectCache().Remove(GetCacheKey(@new.Site));
        }
        public override void Remove(T item)
        {
            base.Remove(item);
            item.Site.ObjectCache().Remove(GetCacheKey(item.Site));
        }

        protected abstract string GetCacheKey(Site site);

    }
}
