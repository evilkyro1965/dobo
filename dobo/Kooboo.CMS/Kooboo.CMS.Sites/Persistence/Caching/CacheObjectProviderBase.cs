using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Caching;
using Kooboo.CMS.Content.Caching;
namespace Kooboo.CMS.Sites.Persistence.Caching
{
    public abstract class CacheObjectProviderBase<T>
        where T : class,IPersistable
    {
        private IProvider<T> innerRepository;
        public CacheObjectProviderBase(IProvider<T> inner)
        {
            this.innerRepository = inner;
        }
        public virtual T Get(T dummy)
        {
            var cacheKey = GetCacheKey(dummy);
            T o = default(T);
            var cached = dummy.Site.ObjectCache().Get(cacheKey);
            if (!(cached is CachedNullValue))
            {
                o = cached as T;
                if (o == null)
                {
                    o = innerRepository.Get(dummy);
                    if (o == null)
                    {
                        dummy.Site.ObjectCache().Add(cacheKey, CachedNullValue.Value, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                    else
                    {
                        dummy.Site.ObjectCache().Add(cacheKey, o, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                }
            }

            return o;
        }

        protected abstract string GetCacheKey(T o);

        protected virtual void ClearObjectCache(T o)
        {
            var cacheKey = GetCacheKey(o);
            o.Site.ObjectCache().Remove(cacheKey);
        }

        public virtual void Add(T item)
        {
            ClearObjectCache(item);

            innerRepository.Add(item);

        }



        public virtual void Update(T @new, T old)
        {
            ClearObjectCache(@old);

            innerRepository.Update(@new, old);
        }

        public virtual void Remove(T item)
        {
            ClearObjectCache(item);
            innerRepository.Remove(item);
        }

    }
}
