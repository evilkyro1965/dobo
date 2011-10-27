using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.IO;
using System.ComponentModel.Composition;
using Kooboo.CMS.Sites.DataRule;
using Kooboo.Globalization;
using Kooboo.CMS.Sites.Versioning;
using Kooboo.CMS.Sites.Caching;
namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    [Export(typeof(IPageProvider))]
    public class PageProvider : InheritableProviderBase<Page>, IPageProvider, ILocalizableProvider<Page>
    {
        public class PageVersionLogger : FileVersionLogger<Page>
        {
            static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();
            public override void LogVersion(Page o)
            {
                locker.EnterWriteLock();
                try
                {
                    VersionPath versionPath = new VersionPath(o, NextVersionId(o));
                    IOUtility.EnsureDirectoryExists(versionPath.PhysicalPath);
                    var versionDataFile = Path.Combine(versionPath.PhysicalPath, o.DataFileName);
                    File.Copy(o.DataFile, versionDataFile);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            public override Page GetVersion(Page o, int version)
            {
                VersionPath versionPath = new VersionPath(o, version);
                var versionDataFile = Path.Combine(versionPath.PhysicalPath, o.DataFileName);
                Page page = null;
                if (File.Exists(versionDataFile))
                {
                    PageProvider provider = new PageProvider();
                    page = provider.Deserialize(o, versionDataFile);
                    ((IPersistable)page).Init(o);
                }
                return page;
            }

            public override void Revert(Page o, int version)
            {
                var versionData = GetVersion(o, version);
                if (versionData != null)
                {
                    Providers.PageProvider.Update(versionData, o);
                }
            }
        }

        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();
        protected override IEnumerable<Type> KnownTypes
        {
            get
            {
                return new Type[]{
                typeof(PagePosition),
                typeof(ViewPosition),
                typeof(ModulePosition),
                typeof(HtmlPosition),
                typeof(ContentPosition),
                typeof(HtmlBlockPosition),
                typeof(DataRuleBase),
                typeof(FolderDataRule),
                typeof(SchemaDataRule),
                typeof(CategoryDataRule)
                //typeof(CategorizableDataRule),
                };
            }
        }
        #region IPageRepository Members

        public IQueryable<Models.Page> ChildPages(Models.Page parentPage)
        {
            return ChildPagesEnumerable(parentPage).AsQueryable();
        }
        public IEnumerable<Models.Page> ChildPagesEnumerable(Models.Page parentPage)
        {
            List<Page> list = new List<Page>();
            if (parentPage.Exists())
            {
                string baseDir = parentPage.PhysicalPath;

                foreach (var dir in IO.IOUtility.EnumerateDirectoriesExludeHidden(baseDir).Where(it => !it.Name.EqualsOrNullEmpty(Versioning.VersionBasePath.VersionFolderName, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add(ModelActivatorFactory<Page>.GetActivator().Create(dir.FullName));
                }
            }
            return list;
        }

        #endregion

        #region Localize & Move Menbers
        public void Localize(Page sourcePage, Site targetSite)
        {
            var sourceSite = sourcePage.Site;
            var nameArr = PageHelper.SplitFullName(sourcePage.FullName).ToArray();

            var destPage = new Page(targetSite, nameArr);

            string pageFolderPath = "";
            var nCount = nameArr.Count();

            pageFolderPath = Path.Combine(nameArr.Take(nCount - 1).ToArray());
            var destPath = Path.Combine(destPage.PhysicalPath, pageFolderPath);

            ILocalizableHelper.CopyFiles(sourcePage.PhysicalPath, destPath);
        }
        public void Move(Site site, string pageFullName, string newParent)
        {
            GetLocker().EnterWriteLock();

            try
            {
                var page = PageHelper.Parse(site, pageFullName);
                Page parentPage = null;
                if (!string.IsNullOrEmpty(newParent))
                {
                    parentPage = PageHelper.Parse(site, newParent);
                    if (parentPage == page.Parent || parentPage == page)
                    {
                        throw new KoobooException(string.Format("The page is under '{0}' already".Localize(), newParent));
                    }
                }
                Page newPage = null;
                if (parentPage != null)
                {
                    newPage = new Page(parentPage, page.Name);
                }
                else
                {
                    newPage = new Page(site, page.Name);
                }
                if (newPage.Exists())
                {
                    throw new KoobooException(string.Format("The page '{0}' already exists in '{1}'".Localize(), page.Name, parentPage.FriendlyName));
                }
                Directory.Move(page.PhysicalPath, newPage.PhysicalPath);
            }
            finally
            {
                GetLocker().ExitWriteLock();
            }
        }
        #endregion

        #region IPageRepository Relation Members

        private IQueryable<Page> AllPagesNested(Site site)
        {
            return All(site).SelectMany(it => AllPagesNested(it));
        }
        private IEnumerable<Page> AllPagesNested(Page parent)
        {
            var childPages = ChildPages(parent);
            return new[] { parent }.Concat(childPages.SelectMany(it => AllPagesNested(it)));
        }
        public IQueryable<Page> ByLayout(Layout layout)
        {
            return AllPagesNested(layout.Site)
                .Select(it => it.AsActual())
                .Where(it => it.Layout.EqualsOrNullEmpty(layout.Name, StringComparison.CurrentCultureIgnoreCase))
                .AsQueryable();
        }

        public IQueryable<Page> ByView(Models.View view)
        {
            return AllPagesNested(view.Site).Select(it => it.AsActual())
                .Where(it => it.PagePositions != null &&
                    it.PagePositions.Any(p => p is ViewPosition && ((ViewPosition)p).ViewName.EqualsOrNullEmpty(view.Name, StringComparison.CurrentCultureIgnoreCase)))
                    .AsQueryable();
        }

        public IQueryable<Page> ByModule(Site site, string moduleName)
        {
            return AllPagesNested(site).Select(it => it.AsActual())
                 .Where(it => it.PagePositions != null &&
                     it.PagePositions.Any(p => p is ModulePosition && ((ModulePosition)p).ModuleName.EqualsOrNullEmpty(moduleName, StringComparison.CurrentCultureIgnoreCase)))
                     .AsQueryable();
        }

        #endregion

        protected override System.Threading.ReaderWriterLockSlim GetLocker()
        {
            return locker;
        }


        #region Copy
        public Page Copy(Site site, string sourcePageFullName, string newPageFullName)
        {
            GetLocker().EnterWriteLock();

            try
            {
                var page = PageHelper.Parse(site, sourcePageFullName);
                var newPage = PageHelper.Parse(site, newPageFullName);

                IOUtility.CopyDirectory(page.PhysicalPath, newPage.PhysicalPath);

                return newPage;
            }
            finally
            {
                GetLocker().ExitWriteLock();
            }
        }

        #endregion

        #region Front-end

        public Page GetDefaultPage(Site site)
        {
            var pages = All(site);
            foreach (var page in pages)
            {
                var pageDetail = page.AsActual();
                if (pageDetail.IsDefault)
                {
                    return pageDetail;
                }
            }
            return pages.FirstOrDefault();
        }

        public Page GetPageByUrl(Site site, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Providers.PageProvider.GetDefaultPage(site);
            }

            string[] paths = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return GetPage(site, paths);
        }

        private Page GetPage(Site site, string[] pagePaths)
        {
            Page page = null;
            Page previousPage = null;
            for (int i = 1; i <= pagePaths.Length; i++)
            {
                var paths = pagePaths.Take(i).ToArray();

                page = GetPageByPathsNoRecursion(site, paths);

                if (page == null)
                {
                    if (previousPage != null)
                    {
                        //check if the previous page have route setting.
                        //if not, we can consider the page can not be found.
                        previousPage = previousPage.AsActual();
                        if (previousPage.Route == null || string.IsNullOrEmpty(previousPage.Route.RoutePath))
                        {
                            previousPage = null;
                        }
                    }
                    break;
                }
                else
                {
                    previousPage = page.LastVersion();
                }
            }
            if (previousPage != null)
            {
                previousPage = previousPage.AsActual();
            }
            return previousPage;
        }
        private Page GetPageByPathsNoRecursion(Site site, string[] pagePaths)
        {
            string urlPath = Kooboo.Web.Url.UrlUtility.Combine(pagePaths);

            string cachedKey = "GetPageByPathsNoRecursion:" + urlPath.ToLower();

            var page = site.ObjectCache().Get(cachedKey) as Page;

            if (page == null)
            {
                // performance leaks, one path will take 18ms...
                page = GetPageByUrlIdentifier(site, urlPath);
                if (page == null)
                {
                    page = new Page(site, pagePaths);
                    var last = page.LastVersion();
                    if (!last.Exists())
                    {
                        page = null;
                    }
                }
                if (page != null)
                {
                    var dummyPage = new Page(page.Site, page.FullName);
                    site.ObjectCache().Add(cachedKey, dummyPage, new System.Runtime.Caching.CacheItemPolicy() { SlidingExpiration = TimeSpan.Parse("00:30:00") });
                }
            }

            return page;
        }

        public Page GetPageByUrlIdentifier(Site site, string identifier)
        {
            foreach (var page in Providers.PageProvider.All(site))
            {
                var found = GetPageByUrlIdentifier(page, identifier);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        private Page GetPageByUrlIdentifier(Page page, string identifier)
        {
            page = page.AsActual();
            if (page.Route != null && page.Route.Identifier.EqualsOrNullEmpty(identifier, StringComparison.OrdinalIgnoreCase))
            {
                return page;
            }
            foreach (var item in Providers.PageProvider.ChildPages(page))
            {
                var found = GetPageByUrlIdentifier(item, identifier);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        #endregion

        #region Draft
        public Page GetDraft(Page page)
        {
            var draftDataFile = GetDraftDataFile(page);
            Page draft = page;
            if (File.Exists(draftDataFile))
            {
                draft = Deserialize(page, draftDataFile);
                ((IPersistable)draft).Init(page);
            }
            return draft;
        }
        private static string GetDraftDataFile(Page page)
        {
            return page.DataFile + ".draft";
        }
        public void SaveAsDraft(Page page)
        {
            Serialize(page, GetDraftDataFile(page));
        }


        public void RemoveDraft(Page page)
        {
            var draftDataFile = GetDraftDataFile(page);
            GetLocker().EnterWriteLock();
            try
            {
                if (File.Exists(draftDataFile))
                {
                    File.Delete(draftDataFile);
                }
            }
            finally
            {
                GetLocker().ExitWriteLock();
            }
        }
        #endregion
    }
}
