using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Caching;
namespace Kooboo.CMS.Content.Persistence.Caching
{
    public abstract class CacheProviderBase<T>
        where T : class, IRepositoryElement
    {
        protected IProvider<T> innerProvider;
        public CacheProviderBase(IProvider<T> inner)
        {
            this.innerProvider = inner;
        }
        public virtual T Get(T dummy)
        {
            var cacheKey = GetCacheKey(dummy);
            T o = default(T);
            var cached = dummy.Repository.ObjectCache().Get(cacheKey);
            if (!(cached is CachedNullValue))
            {
                o = cached as T;
                if (o == null)
                {
                    o = innerProvider.Get(dummy);
                    if (o == null)
                    {
                        dummy.Repository.ObjectCache().Add(cacheKey, CachedNullValue.Value, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                    else
                    {
                        dummy.Repository.ObjectCache().Add(cacheKey, o, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                }
            }

            return o;
        }

        protected abstract string GetCacheKey(T o);

        protected virtual void ClearObjectCache(T o)
        {
            var cacheKey = GetCacheKey(o);
            o.Repository.ObjectCache().Remove(cacheKey);
        }

        public virtual void Add(T item)
        {
            ClearObjectCache(item);
            innerProvider.Add(item);
        }

        public virtual void Update(T @new, T old)
        {
            ClearObjectCache(old);

            innerProvider.Update(@new, old);
        }

        public virtual void Remove(T item)
        {
            innerProvider.Remove(item);
            ClearObjectCache(item);
        }

    }
}
