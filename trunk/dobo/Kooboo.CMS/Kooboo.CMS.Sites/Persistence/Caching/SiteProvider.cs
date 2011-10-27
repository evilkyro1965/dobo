using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

using Kooboo.CMS.Sites.Caching;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.Globalization;
using Ionic.Zip;
using Kooboo.CMS.Content.Caching;

namespace Kooboo.CMS.Sites.Persistence.Caching
{
    public class SiteProvider : ISiteProvider
    {
        private ISiteProvider innerRepository;
        public SiteProvider(ISiteProvider inner)
        {
            this.innerRepository = inner;
        }
        public Models.Site GetSiteByHostNameNPath(string hostName, string mappingPath)
        {
            string cacheKey = string.Format("GetSiteByHostNameNPath:HostName-{0}:MappingPath-{1}", hostName.ToLower(), mappingPath.ToLower());
            return GetCachedData<Site>(cacheKey, () => innerRepository.GetSiteByHostNameNPath(hostName, mappingPath));
        }

        private static T GetCachedData<T>(string cacheKey, Func<T> cacheData)
            where T : class
        {
            T data = default(T);
            var cached = CacheManagerFactory.DefaultCacheManager.GlobalObjectCache().Get(cacheKey);
            if (!(cached is CachedNullValue))
            {
                data = cached as T;
                if (data == null)
                {
                    data = cacheData();
                    if (data == null)
                    {
                        CacheManagerFactory.DefaultCacheManager.GlobalObjectCache().Add(cacheKey, CachedNullValue.Value, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                    else
                    {
                        CacheManagerFactory.DefaultCacheManager.GlobalObjectCache().Add(cacheKey, data, CacheProviderFactory.DefaultCacheItemPolicy);
                    }
                }

            }
            return data
;
        }

        public Models.Site GetSiteByHostName(string hostName)
        {
            string cacheKey = string.Format("GetSiteByHostName:HostName-{0}", hostName.ToLower());
            return GetCachedData<Site>(cacheKey, () => innerRepository.GetSiteByHostName(hostName));
        }

        public IEnumerable<Models.Site> AllSites()
        {
            string cacheKey = "AllSites";
            return GetCachedData<Site[]>(cacheKey, () => innerRepository.AllSites().ToArray());
        }

        public IEnumerable<Models.Site> AllRootSites()
        {
            string cacheKey = "AllRootSites";
            return GetCachedData<Site[]>(cacheKey, () => innerRepository.AllRootSites().ToArray());
        }

        public IEnumerable<Models.Site> ChildSites(Models.Site site)
        {
            return innerRepository.ChildSites(site);
        }

        public IQueryable<Models.Site> All(Models.Site site)
        {
            return innerRepository.All(site);
        }

        public Models.Site Get(Models.Site dummy)
        {
            var cacheKey = GetCacheKey(dummy);
            var site = (Site)dummy.ObjectCache().Get(cacheKey);
            if (site == null)
            {
                site = innerRepository.Get(dummy);
                if (site == null)
                {
                    return site;
                }
                dummy.ObjectCache().Add(cacheKey, site, CacheProviderFactory.DefaultCacheItemPolicy);
            }
            return site;
        }

        private static string GetCacheKey(Models.Site site)
        {
            var cacheKey = string.Format("Site:{0}", site.FullName.ToLower());
            return cacheKey;
        }

        public void Add(Models.Site item)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();

            innerRepository.Add(item);
        }

        public void Update(Models.Site @new, Models.Site old)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();
            var cacheKey = GetCacheKey(@new);
            @new.ObjectCache().Remove(cacheKey);

            innerRepository.Update(@new, old);

        }

        public void Remove(Models.Site item)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();

            innerRepository.Remove(item);

            item.ClearCache();
        }


        public void Offline(Site site)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();
            innerRepository.Offline(site);
        }

        public void Online(Site site)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();
            innerRepository.Online(site);
        }

        public bool IsOnline(Site site)
        {        
            return innerRepository.IsOnline(site);
        }

        public Site Create(Site parentSite, string siteName, Stream packageStream, string repositoryName)
        {
            CacheManagerFactory.DefaultCacheManager.ClearGlobalObjectCache();
            return innerRepository.Create(parentSite, siteName, packageStream, repositoryName);
        }

        public void Initialize(Site site)
        {
            innerRepository.Initialize(site);
        }

        public void Export(Site site, Stream outputStream)
        {
            innerRepository.Export(site, outputStream);
        }
    }
}
