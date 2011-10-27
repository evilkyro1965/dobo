using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.Globalization;
namespace Kooboo.CMS.Sites.Services
{
    public class SiteManager : PathResourceManagerBase<Site>
    {
        public ISiteProvider SiteProvider
        {
            get
            {
                return ((ISiteProvider)this.Provider);
            }
        }
        public override void Add(Site parent, Site site)
        {
            site.Parent = parent;

            var query = Provider.Get(site);
            if (query != null)
            {
                throw new ItemAlreadyExistsException();
            }

            Provider.Add(site);
        }
        public override IEnumerable<Site> All(Site site, string filterName)
        {
            throw new NotImplementedException("Please use All(Method without any parameters) or ChildSites instead.");
        }
        public virtual Site GetSite(IEnumerable<string> namePath)
        {
            if (namePath == null || namePath.Count() == 0)
            {
                throw new ArgumentNullException("namePath");
            }
            Site dummySite = new Site(namePath);
            return Provider.Get(dummySite);
        }
        public virtual void Update(Site site)
        {
            Provider.Update(site, site);
        }

        public override Site Get(Site site, string name)
        {
            return Provider.Get(site);
        }

        public IEnumerable<Site> ChildSites(Site site)
        {
            var sites = ((ISiteProvider)this.Provider).ChildSites(site);

            foreach (var c in sites)
            {
                yield return Provider.Get(c);
            }
        }
        public IEnumerable<Site> All()
        {
            return ((ISiteProvider)this.Provider).AllSites();
        }
        public IEnumerable<Site> AllRootSites()
        {
            return ((ISiteProvider)this.Provider).AllRootSites();
        }

        public override void Remove(Site site, Site o)
        {
            throw new NotSupportedException("Please instead of using Remove(Site site).");
        }
        public void Remove(Site site, bool includeRepository)
        {
            if (includeRepository)
            {
                RemoveSiteRepository(site);
            }

            SiteProvider.Offline(site);

            SiteProvider.Remove(site);

        }

        private void RemoveSiteRepository(Site site)
        {
            var repository = site.AsActual().Repository;

            if (!string.IsNullOrEmpty(repository))
            {
                Kooboo.CMS.Content.Services.ServiceFactory.RepositoryManager.Remove(new Kooboo.CMS.Content.Models.Repository(repository));
            }
            foreach (var child in ChildSites(site))
            {
                RemoveSiteRepository(child);
            }
        }


        public IEnumerable<SiteTree> SiteTrees(string userName)
        {
            return AllRootSites().Select(it => GetSiteNode(it, userName))
                .Where(it => it != null)
                .Select(it => new SiteTree() { Root = it });
        }
        public SiteNode GetSiteNode(Site site, string userName)
        {
            if (ServiceFactory.UserManager.Authorize(site, userName))
            {
                SiteNode siteNode = new SiteNode() { Site = site.AsActual() };
                siteNode.Children = ChildSites(site)
                    .Select(it => GetSiteNode(it, userName))
                    .Where(it => it != null);
                return siteNode;
            }
            return null;
        }
        #region Export/Import
        private void CopyRepository(Kooboo.CMS.Content.Models.Repository sourceRepository, string name)
        {
            var repositoryManager = Kooboo.CMS.Content.Services.ServiceFactory.RepositoryManager;
            var repository = repositoryManager.Get(name);

            if (repository == null)
            {
                repositoryManager.Copy(sourceRepository, name);
            }
        }
        public virtual Site Create(Site parent, string siteName, string templateFullName, Site siteSetting, string userName)
        {
            if (string.IsNullOrEmpty(templateFullName))
            {
                if (parent == null)
                {
                    throw new KoobooException("Parent site is required.".Localize());
                }
                siteSetting.Parent = parent;

                Add(parent, siteSetting);

                if (!string.IsNullOrEmpty(siteSetting.Repository))
                {
                    CopyRepository(parent.GetRepository(), siteSetting.Repository);
                }
                return siteSetting;
            }
            else
            {
                var template = new ItemTemplate(templateFullName);
                var itemTemplate = ServiceFactory.SiteTemplateManager.GetItemTemplate(template.Category, template.TemplateName);
                if (itemTemplate == null)
                {
                    throw new KoobooException("The template does not exists.");
                }
                Site site = null;
                using (FileStream fs = new FileStream(itemTemplate.TemplateFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    site = Create(parent, siteName, siteSetting.Repository, fs, userName);
                }
                //copy site setting...
                site.Repository = siteSetting.Repository;
                site.DisplayName = siteSetting.DisplayName;
                site.Culture = siteSetting.Culture;
                site.Domains = siteSetting.Domains;
                site.SitePath = siteSetting.SitePath;
                site.Version = siteSetting.Version;
                site.Mode = siteSetting.Mode;
                Update(site);

                return site;
            }
        }
        public virtual Site Import(Site parent, string siteName, string repositoryName, string importedSiteName, string userName)
        {
            var template = new ItemTemplate(importedSiteName);
            var itemTemplate = ServiceFactory.ImportedSiteManager.GetItemTemplate(template.Category, template.TemplateName);
            if (itemTemplate == null)
            {
                throw new KoobooException("The imported site does not exists.");
            }
            Site site = null;
            using (FileStream fs = new FileStream(itemTemplate.TemplateFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                site = Import(parent, siteName, repositoryName, fs, userName);
            }
            return site;
        }
        public virtual Site Import(Site parent, string siteName, string repositoryName, Stream siteStream, string userName)
        {
            return Create(parent, siteName, repositoryName, siteStream, userName);
        }

        public virtual Site Create(Site parent, string siteName, string repositoryName, Stream siteStream, string userName)
        {
            var site = SiteProvider.Create(parent, siteName, siteStream, repositoryName);
            site.Repository = repositoryName;
            Update(site);
            SiteProvider.Initialize(site);
            SiteProvider.Online(site);

            return site;
        }

        public override void Export(Site site, string name, Stream outputStream)
        {
            throw new NotSupportedException("Instead of using Export(string fullSiteName, Stream outputStream)");
        }
        public override void ExportAll(Site site, Stream outputStream)
        {
            throw new NotSupportedException();
        }
        public override void ExportSelected(Site site, Site[] selected, Stream outputStream)
        {
            throw new NotSupportedException();
        }
        public virtual void Export(string fullSiteName, Stream outputStream)
        {
            var site = SiteHelper.Parse(fullSiteName);
            if (site.AsActual() == null)
            {
                throw new Exception("The site does not exists.");
            }
            SiteProvider.Export(site, outputStream);
        }

        public virtual void Offline(string siteName)
        {
            SiteProvider.Offline(SiteHelper.Parse(siteName));
        }
        public virtual void Online(string siteName)
        {
            SiteProvider.Online(SiteHelper.Parse(siteName));
        }
        public virtual bool IsOnline(string siteName)
        {
            return SiteProvider.IsOnline(SiteHelper.Parse(siteName));
        }
        #endregion
    }
}
