using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.Extensions;
using System.IO;
using Kooboo.Globalization;
using Kooboo.CMS.Sites.Versioning;
using Kooboo.CMS.Search;
using Kooboo.CMS.Search.Models;
using System.Collections.Specialized;
using Kooboo.CMS.Sites.View;
namespace Kooboo.CMS.Sites.Services
{
    public class PageConverter : IObjectConverter
    {
        public KeyValuePair<string, string> GetKeyField(object o)
        {
            Page page = (Page)o;
            return new KeyValuePair<string, string>("PageName", page.FullName);
        }

        public Search.Models.IndexObject GetIndexObject(object o)
        {
            IndexObject indexObject = null;

            Page page = (Page)o;
            NameValueCollection storeFields = new NameValueCollection();
            NameValueCollection sysFields = new NameValueCollection();

            sysFields.Add("SiteName", page.Site.FullName);
            sysFields.Add("PageName", page.FullName);

            string title = "";
            StringBuilder body = new StringBuilder();

            if (page.HtmlMeta != null && !string.IsNullOrEmpty(page.HtmlMeta.HtmlTitle))
            {
                title = page.HtmlMeta.HtmlTitle;
            }
            if (!string.IsNullOrEmpty(page.ContentTitle))
            {
                body.AppendFormat(title);

                title = page.ContentTitle;
            }

            if (page.PagePositions != null)
            {
                foreach (var item in page.PagePositions.Where(it => (it is HtmlBlockPosition) || (it is HtmlPosition)))
                {
                    if (item is HtmlBlockPosition)
                    {
                        HtmlBlockPosition htmlBlockPosition = (HtmlBlockPosition)item;
                        var htmlBlock = new HtmlBlock(page.Site, htmlBlockPosition.BlockName).LastVersion().AsActual();
                        if (htmlBlock != null)
                        {
                            body.Append(" " + Kooboo.Extensions.StringExtensions.StripAllTags(htmlBlock.Body));
                        }
                    }
                    else
                    {
                        HtmlPosition htmlPosition = (HtmlPosition)item;
                        body.Append(" " + Kooboo.Extensions.StringExtensions.StripAllTags(htmlPosition.Html));
                    }
                }
            }


            indexObject = new IndexObject()
            {
                Title = title,
                Body = body.ToString(),
                StoreFields = storeFields,
                SystemFields = sysFields,
                NativeType = typeof(Page).AssemblyQualifiedNameWithoutVersion()
            };

            return indexObject;
        }

        public object GetNativeObject(System.Collections.Specialized.NameValueCollection fields)
        {
            var siteName = fields["SiteName"];
            var pageName = fields["PageName"];
            return new Page(Page_Context.Current.PageRequestContext.Site, pageName);
        }

        public string GetUrl(object nativeObject)
        {
            Page page = (Page)nativeObject;
            return Page_Context.Current.FrontUrl.PageUrl(page.FullName).ToString();
        }
    }


    public class PageManager : PathResourceManagerBase<Page>
    {
        static PageManager()
        {
            Converters.Register(typeof(Page), new PageConverter());
        }
        public IVersionLogger<Models.Page> VersiongLogger
        {
            get
            {
                return VersionManager.ResolveVersionLogger<Models.Page>();
            }
        }
        public IPageProvider PageProvider
        {
            get
            {
                return (IPageProvider)this.Provider;
            }
        }
        public IPagePublishingQueueProvider PagePublishingProvider
        {
            get
            {
                return Providers.PagePublishingProvider;
            }
        }
        #region Page

        public override IEnumerable<Page> All(Site site, string filterName)
        {
            var pages = Provider.All(site);
            if (!string.IsNullOrEmpty(filterName))
            {
                pages = pages.Select(it => it.AsActual())
                   .Where(it => it.Name.Contains(filterName, StringComparison.CurrentCultureIgnoreCase)
                       || (it.Navigation != null && !string.IsNullOrEmpty(it.Navigation.DisplayText) && it.Navigation.DisplayText.Contains(filterName, StringComparison.CurrentCultureIgnoreCase)));
            }

            foreach (var page in pages)
            {
                yield return page.AsActual();
            }
        }
        public IEnumerable<Page> All(Site site, string parentPageName, string filterName)
        {
            IEnumerable<Page> pages;
            if (string.IsNullOrEmpty(parentPageName))
            {
                pages = All(site, filterName);
            }
            else
            {
                pages = ChildPages(site, parentPageName, filterName);
            }
            return pages.OrderBy(it => it.Navigation.Order);
        }
        public IEnumerable<Page> ChildPages(Site site, string parentPageName, string filterName)
        {
            var parentPage = new Page(site, PageHelper.SplitFullName(parentPageName).ToArray());
            var pages = ((IPageProvider)this.Provider).ChildPages(parentPage.LastVersion());

            if (!string.IsNullOrEmpty(filterName))
            {
                pages = pages.Select(it => it.AsActual())
                   .Where(it => it.Name.Contains(filterName, StringComparison.CurrentCultureIgnoreCase)
                       || (it.Navigation != null && !string.IsNullOrEmpty(it.Navigation.DisplayText) && it.Navigation.DisplayText.Contains(filterName, StringComparison.CurrentCultureIgnoreCase)));
            }

            foreach (var page in pages)
            {
                yield return Provider.Get(page);
            }

        }
        public override Page Get(Site site, string fullName)
        {
            var page = Provider.Get((new Page(site, PageHelper.SplitFullName(fullName).ToArray())).LastVersion());

            return page;
        }
        public Page Get(Site site, string name, string parent)
        {
            string fullName = name;
            if (!string.IsNullOrEmpty(parent))
            {
                fullName = PageHelper.CombineFullName(new string[] { parent, fullName });
            }
            return Get(site, fullName);
        }

        private bool HasSameName(Site site, string fullName)
        {
            if (site.Parent != null)
            {
                var query = Get(site.Parent, fullName);
                if (query != null)
                {
                    throw new ItemAlreadyExistsException();
                }
                else
                {
                    return HasSameName(site.Parent, fullName);
                }
            }
            return false;
        }

        public virtual void Add(Site site, string parentPageName, Page page)
        {
            page.Site = site;
            if (!string.IsNullOrEmpty(parentPageName))
            {
                var parentPage = new Page(site, PageHelper.SplitFullName(parentPageName).ToArray());
                page.Parent = parentPage;
            }

            if (page.Exists() || HasSameName(site, page.FullName))
            {
                throw new ItemAlreadyExistsException();
            }

            if (page.IsDefault == true)
            {
                ResetDefaultPage(site, page);
            }

            Provider.Add(page);

            if (page.Searchable)
            {
                SearchHelper.OpenService(site.GetRepository()).Add(page);
            }

            VersionManager.LogVersion(page);
        }

        public virtual Page GetDefaultPage(Site site)
        {
            return ((IPageProvider)Provider).GetDefaultPage(site);
        }

        public virtual SiteMap GetSiteMap(Site site)
        {
            var rootPage = GetDefaultPage(site);

            if (rootPage == null)
            {
                return new SiteMap();
            }

            var rootNode = new SiteMapNode() { Page = rootPage.AsActual() };

            rootNode.Children = All(site, "")
                .Select(it => GetPageNode(site, it))
                .OrderBy(it => it.Page.Navigation.Order);

            return new SiteMap() { Root = rootNode };
        }

        private SiteMapNode GetPageNode(Site site, Page page)
        {
            SiteMapNode node = new SiteMapNode() { Page = page.AsActual() };

            node.Children = ChildPages(site, page.FullName, "").Select(it => GetPageNode(site, it)).OrderBy(it => it.Page.Navigation.Order);

            return node;
        }

        public virtual Page Copy(Site site, string sourcePageFullName, string destPageFullName)
        {
            var destPage = PageHelper.Parse(site, destPageFullName);
            if (destPage.Exists())
            {
                throw new KoobooException("The page already exists.".Localize());
            }
            var page = this.PageProvider.Copy(site, sourcePageFullName, destPageFullName);

            page = page.AsActual();

            // Reset the display text.
            if (page.Navigation == null)
            {
                page.Navigation = new Navigation();
            }
            page.Navigation.DisplayText = "";
            page.IsDefault = false;

            return page;
        }

        public override void Update(Site site, Page @new, Page old)
        {
            @new.Site = site;
            base.Update(site, @new, old);
            if (@new.IsDefault == true)
            {
                ResetDefaultPage(site, @new);
            }

            if (@new.Searchable)
            {
                SearchHelper.OpenService(site.GetRepository()).Update(@new);
            }
            else
            {
                SearchHelper.OpenService(site.GetRepository()).Delete(@new);
            }
            VersionManager.LogVersion(@new);
        }

        private void ResetDefaultPage(Site site, Page currentDefaultPage)
        {
            foreach (var page in All(site, ""))
            {
                var actualPage = page.AsActual();
                if (actualPage != currentDefaultPage && actualPage.IsDefault == true)
                {
                    actualPage.IsDefault = false;
                    base.Update(site, actualPage, actualPage);
                }
            }
        }


        public override void Remove(Site site, Page o)
        {
            base.Remove(site, o);

            SearchHelper.OpenService(site.GetRepository()).Delete(o);
        }
        #endregion

        #region Localize
        public virtual void Localize(string fullName, string fromSite, Site currentSite)
        {
            var sourceSite = new Site(SiteHelper.SplitFullName(fromSite));

            var sourcePage = new Page(sourceSite, PageHelper.SplitFullName(fullName).ToArray());

            PageProvider.Localize(sourcePage, currentSite);

        }

        #endregion

        #region Import/export
        public override void Export(Site site, string name, System.IO.Stream outputStream)
        {
            ((IImportProvider<Page>)Provider).Export(new Page[] { Get(site, name) }, outputStream);
        }
        public override void ExportAll(Site site, System.IO.Stream outputStream)
        {
            ((IImportProvider<Page>)Provider).Export(All(site, ""), outputStream);
        }
        protected override string GetImportPhysicalPath(Site site, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var dummy = ModelActivatorFactory<Page>.GetActivator().Create(site, "dummy");
                return dummy.BasePhysicalPath;
            }
            else
            {
                var dummy = ModelActivatorFactory<Page>.GetActivator().Create(site, name);
                return dummy.PhysicalPath;
            }
        }
        #endregion

        #region IsStaticPage
        public virtual bool IsStaticPage(Site site, Page page)
        {
            page = page.AsActual();
            if (page.PageType == PageType.Default)
            {
                foreach (var item in AggregateDataRules(site, page))
                {
                    if (item.DataRule.HasAnyParameters())
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (page.PageType == PageType.Dynamic)
                {
                    return false;
                }
            }
            return true;
        }
        private IEnumerable<DataRuleSetting> AggregateDataRules(Site site, Page page)
        {
            page = page.AsActual();
            IEnumerable<DataRuleSetting> datarules = page.DataRules ?? new List<DataRuleSetting>();
            if (page.PagePositions != null)
            {
                var viewPositions = page.PagePositions.Where(it => it is ViewPosition).OrderBy(it => it.Order);
                foreach (ViewPosition viewPosition in viewPositions)
                {
                    var view = new Models.View(site, viewPosition.ViewName).LastVersion();

                    if (view.Exists())
                    {
                        datarules = datarules.Concat(view.AsActual().DataRules ?? new List<DataRuleSetting>());
                    }
                }

            }

            return datarules;
        }
        #endregion

        #region Move

        public virtual void Move(Site site, string pageFullName, string newParent)
        {
            Page page = new Page(site, pageFullName);
            if (string.IsNullOrEmpty(newParent))
            {
                if (page.Parent == null)
                {
                    throw new KoobooException("The page is a root page already.".Localize());
                }
            }

            PageProvider.Move(site, pageFullName, newParent);
        }
        #endregion

        #region Publish
        public virtual bool HasDraft(string pageName)
        {
            var page = new Page(Site.Current, pageName);
            return HasDraft(page);
        }
        public virtual bool HasDraft(Page page)
        {
            var draft = ServiceFactory.PageManager.PageProvider.GetDraft(page);
            return !object.ReferenceEquals(page, draft);
        }
        public virtual void Publish(Page page, bool publishDraft, string userName)
        {
            Publish(page, false, publishDraft, false, DateTime.Now, DateTime.Now, userName);
        }
        public virtual void Publish(Page page, bool publishSchedule, bool publishDraft, bool period, DateTime publishDate, DateTime offlineDate, string userName)
        {
            if (!publishSchedule)
            {
                page = page.AsActual();
                if (publishDraft)
                {
                    page = PageProvider.GetDraft(page);
                    PageProvider.RemoveDraft(page);
                }
                page.Published = true;
                page.UserName = userName;
                PageProvider.Update(page, page);
                VersionManager.LogVersion(page);
            }
            else
            {
                PagePublishingQueueItem publishingItem = new PagePublishingQueueItem()
                {
                    Site = page.Site,
                    PageName = page.FullName,
                    PublishDraft = publishDraft,
                    CreationUtcDate = DateTime.UtcNow,
                    UtcDateToPublish = publishDate.ToUniversalTime(),
                    Period = period,
                    UtcDateToOffline = offlineDate.ToUniversalTime(),
                    UserName = userName
                };
                PagePublishingProvider.Add(publishingItem);
            }
        }
        public virtual void Offline(Page page, string userName)
        {
            page = page.AsActual();
            page.Published = false;
            page.UserName = userName;
            PageProvider.Update(page, page);
            VersionManager.LogVersion(page);
        }
        #endregion

    }
}
