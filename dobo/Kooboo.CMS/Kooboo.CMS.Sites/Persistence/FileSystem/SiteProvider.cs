using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Kooboo.IO;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Ionic.Zip;
using Kooboo.Globalization;
using Kooboo.Web.Url;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    [Export(typeof(ISiteProvider))]
    public class SiteProvider : ISiteProvider
    {
        static string Offline_File = "offline.txt";

        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();

        #region IRepository<Site> Members

        [Obsolete("Please use AllSites or ChildSites instead.")]
        public IQueryable<Models.Site> All(Site site)
        {
            throw new NotImplementedException("Please use AllSites or ChildSites instead.");
        }

        public void Add(Models.Site item)
        {
            Save(item);
        }

        private static void Save(Models.Site item)
        {
            locker.EnterWriteLock();
            try
            {
                IOUtility.EnsureDirectoryExists(item.PhysicalPath);
                Serialization.Serialize(item, item.DataFile);
                ((IPersistable)item).OnSaved();
            }
            finally
            {
                locker.ExitWriteLock();
            }

        }

        public void Update(Models.Site @new, Models.Site old)
        {
            Save(@new);
        }

        public void Remove(Models.Site item)
        {
            locker.EnterWriteLock();
            try
            {
                IOUtility.DeleteDirectory(item.PhysicalPath, true);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public Site Get(Site dummyObject)
        {
            if (!dummyObject.Exists())
            {
                return null;
            }
            locker.EnterReadLock();
            try
            {
                var site = Serialization.DeserializeSettings<Site>(dummyObject.DataFile);
                ((IPersistable)site).Init(dummyObject);
                return site;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        #endregion

        #region Query Members

        public Site GetSiteByHostNameNPath(string hostName, string sitePath)
        {
            return GetSiteByPredicate(it => (it.Domains != null && it.Domains.Length > 0)
                && it.Domains.Contains(hostName, StringComparer.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(it.SitePath) && it.SitePath.Equals(sitePath, StringComparison.InvariantCultureIgnoreCase));
        }

        private Site GetSiteByPredicate(Func<Site, bool> predicate)
        {
            foreach (var site in Providers.SiteProvider.AllSites())
            {
                if (site.Exists())
                {
                    var detail = site.AsActual();
                    if (predicate(detail))
                    {
                        return detail;
                    }
                }
            }
            return null;
        }

        public Site GetSiteByHostName(string hostName)
        {
            return GetSiteByPredicate(it => (it.Domains != null && it.Domains.Length > 0) &&
                it.Domains.Contains(hostName, StringComparer.OrdinalIgnoreCase) && string.IsNullOrEmpty(it.SitePath));
        }


        public IEnumerable<Site> AllSites()
        {
            return CascadedChildSites(null);
        }
        private IEnumerable<Site> CascadedChildSites(Site site)
        {
            var childSites = ChildSites(site);
            foreach (var child in childSites.AsEnumerable())
            {
                childSites = childSites.Concat(CascadedChildSites(child));
            }
            return childSites;
        }

        public IEnumerable<Site> ChildSites(Site site)
        {
            //if the site is null, get the root sites.
            string baseDir = Site.RootBasePhysicalPath;
            if (site != null)
            {
                baseDir = site.ChildSitesBasePhysicalPath;
            }
            if (Directory.Exists(baseDir))
            {
                foreach (var dir in IO.IOUtility.EnumerateDirectoriesExludeHidden(baseDir))
                {
                    if (File.Exists(Path.Combine(dir.FullName, "setting.config")))
                    {
                        yield return new Site(site, dir.Name);
                    }
                }
            }
        }

        public IEnumerable<Site> AllRootSites()
        {
            return ChildSites(null);
        }

        #endregion


        #region Online/Offline

        public void Offline(Site site)
        {
            var offlineFile = GetOfflineFile(site);
            if (!File.Exists(offlineFile))
            {
                File.WriteAllText(offlineFile, "The site is offline, please remove this file to take it online.".Localize());
            }
        }
        public void Online(Site site)
        {
            var offlineFile = GetOfflineFile(site);
            if (File.Exists(offlineFile))
            {
                File.Delete(offlineFile);
            }
        }
        public bool IsOnline(Site site)
        {
            var offlineFile = GetOfflineFile(site);
            return !File.Exists(offlineFile);
        }
        private string GetOfflineFile(Site site)
        {
            return Path.Combine(site.PhysicalPath, Offline_File);
        }

        #endregion

        #region Export/Import
        /// <summary>
        /// 1. Extract the site files.
        /// 2. Create and initialize the repository if the repository doest not exsits.
        /// </summary>
        /// <param name="parentSite"></param>
        /// <param name="siteName"></param>
        /// <param name="packageStream"></param>
        /// <returns></returns>
        public Site Create(Site parentSite, string siteName, System.IO.Stream packageStream, string repositoryName)
        {
            Site site = new Site(parentSite, siteName);
            if (site.Exists())
            {
                throw new KoobooException("The site already exists.");
            }
            using (ZipFile zipFile = ZipFile.Read(packageStream, Encoding.UTF8))
            {
                var action = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractAll(site.PhysicalPath, action);

                site = CreateSiteRepository(site, repositoryName);
            }
            return site;
        }

        private Site CreateSiteRepository(Site site, string newRepositoryName)
        {
            //Create the repository if the repository does not exists.
            site = site.AsActual();
            if (!string.IsNullOrEmpty(newRepositoryName))
            {
                if (CMS.Content.Services.ServiceFactory.RepositoryManager.Get(newRepositoryName) == null)
                {
                    var repositoryFile = GetSiteRepositoryFile(site);
                    if (File.Exists(repositoryFile))
                    {
                        using (FileStream fs = new FileStream(repositoryFile, FileMode.Open, FileAccess.Read))
                        {
                            CMS.Content.Services.ServiceFactory.RepositoryManager.Create(newRepositoryName, fs);
                        }
                        try
                        {
                            File.Delete(repositoryFile);
                        }
                        catch (Exception e)
                        {
                            Kooboo.HealthMonitoring.Log.LogException(e);
                        }

                    }
                }
            }
            site.Repository = newRepositoryName;
            Save(site);
            foreach (var childSite in ChildSites(site))
            {
                CreateSiteRepository(childSite, newRepositoryName + "-" + childSite.Name);
            }

            return site;
        }

        public void Initialize(Site site)
        {

        }

        /// <summary>
        /// 1. export repository as a zip file.
        /// 2. offline the site.
        /// 3. zip the site directory as a zip file.
        /// 4. online the site.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="outputStream"></param>
        public void Export(Site site, System.IO.Stream outputStream)
        {
            ISiteProvider siteRepository = Providers.SiteProvider;
            siteRepository.Offline(site);
            try
            {
                using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
                {
                    //zipFile.ZipError += new EventHandler<ZipErrorEventArgs>(zipFile_ZipError);

                    zipFile.AddDirectory(site.PhysicalPath);

                    ExportSiteRepository(site, zipFile);

                    zipFile.ZipErrorAction = ZipErrorAction.Skip;

                    zipFile.Save(outputStream);
                }
            }
            finally
            {
                siteRepository.Online(site);
            }
        }

        private void ExportSiteRepository(Site site, ZipFile zipFile)
        {
            site = site.AsActual();

            if (!string.IsNullOrEmpty(site.Repository))
            {
                if (site.Parent == null || (site.Parent != null
                    && !site.Parent.AsActual().Repository.EqualsOrNullEmpty(site.Repository, StringComparison.CurrentCultureIgnoreCase)))
                {
                    MemoryStream ms = new MemoryStream();

                    Kooboo.CMS.Content.Services.ServiceFactory.RepositoryManager.Export(site.Repository, ms);

                    ms.Position = 0;

                    var entryName = GetSiteRepositoryEntryName(site);

                    if (zipFile.ContainsEntry(entryName))
                    {
                        zipFile.RemoveEntry(entryName);
                    }
                    zipFile.AddEntry(entryName, ms);
                }
            }
            foreach (var childSite in ChildSites(site))
            {
                ExportSiteRepository(childSite, zipFile);
            }
        }

        private static string GetSiteRepositoryEntryName(Site site)
        {
            var entryName = site.Repository + ".zip";
            if (site.Parent != null)
            {
                entryName = UrlUtility.Combine(site.VirtualPath.Remove(0, site.Parent.VirtualPath.Length + 1), entryName);
            }
            return entryName;
        }
        private static string GetSiteRepositoryFile(Site site)
        {
            var entryName = site.Repository + ".zip";
            entryName = Path.Combine(site.PhysicalPath, entryName);
            return entryName;
        }
        #endregion
    }
}
