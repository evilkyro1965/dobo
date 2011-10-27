using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.Runtime.Caching;
using Kooboo.CMS.Content.Caching;

namespace Kooboo.CMS.Sites.Caching
{
    public static class CacheExtensions
    {
        public static ObjectCache ObjectCache(this Site site)
        {
            return CacheManagerFactory.DefaultCacheManager.GetObjectCache(site.GetKey());
        }
        private static string GetKey(this Site site)
        {
            return "Site:" + site.FullName;
        }
        public static void ClearCache(this Site site)
        {
            CacheManagerFactory.DefaultCacheManager.Clear(site.GetKey());
        }
    }
}
