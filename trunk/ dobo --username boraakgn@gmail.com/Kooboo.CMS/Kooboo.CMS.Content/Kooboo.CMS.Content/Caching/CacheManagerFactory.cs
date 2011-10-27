using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Caching
{
    public static class CacheManagerFactory
    {
        private static CacheManager cacheManager;
        static CacheManagerFactory()
        {
            DefaultCacheManager = new MemoryCacheManager();
        }
        public static CacheManager DefaultCacheManager
        {
            get
            {
                return cacheManager;
            }
            set
            {
                cacheManager = value;
            }
        }
    }
}
