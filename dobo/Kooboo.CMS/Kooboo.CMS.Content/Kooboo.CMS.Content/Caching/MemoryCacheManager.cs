using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Kooboo.CMS.Content.Caching
{
    public class MemoryCacheManager : CacheManager
    {
        static IDictionary<string, MemoryCache> objectCaches = new Dictionary<string, MemoryCache>();
        const string GLOBAL_CACHE_NAME = "GlobalCache";
        static MemoryCache globalCache;
        static object globalLocker = new object();

        public override ObjectCache GetObjectCache(string name)
        {
            if (!objectCaches.ContainsKey(name))
            {
                lock (objectCaches)
                {
                    if (!objectCaches.ContainsKey(name))
                    {
                        MemoryCache memoryCache = new MemoryCache(name);
                        objectCaches.Add(name, memoryCache);
                    }
                }
            }
            return objectCaches[name];
        }

        protected override void RemoveObjectCache(string name)
        {
            if (objectCaches.ContainsKey(name))
            {
                lock (objectCaches)
                {
                    if (objectCaches.ContainsKey(name))
                    {

                        MemoryCache memoryCache = objectCaches[name];
                        objectCaches.Remove(name);

                        memoryCache.Dispose();
                    }
                }
            }
        }

        public override ObjectCache GlobalObjectCache()
        {
            if (globalCache == null)
            {
                lock (globalLocker)
                {
                    if (globalCache == null)
                    {
                        globalCache = new MemoryCache(GLOBAL_CACHE_NAME);
                    }
                }
            }
            return globalCache;
        }

        public override void ClearGlobalObjectCache()
        {
            lock (globalLocker)
            {
                if (globalCache != null)
                {
                    var cache = globalCache;
                    globalCache = null;
                    cache.Dispose();
                }
            }
        }
    }
}
