using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Kooboo.CMS.Sites.Models;
using System.Runtime.Serialization;
using Kooboo.Collections;
using Kooboo.Web.Url;
using System.IO;
using Kooboo.CMS.Sites.View;
using System.Collections.Specialized;
using System.Collections;
using Kooboo.Dynamic;
namespace Kooboo.CMS.Sites.Models
{
    [CollectionDataContract]
    public class SerializableNameValueCollection : IEnumerable
    {
        public SerializableNameValueCollection() : this(new NameValueCollection()) { }

        public SerializableNameValueCollection(NameValueCollection nvc)
        {
            InnerCollection = nvc;
        }

        public NameValueCollection InnerCollection { get; private set; }

        public void Add(object value)
        {
            var nvString = value as string;
            if (nvString != null)
            {
                var nv = nvString.Split(',');
                InnerCollection.Add(nv[0], nv[1]);
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (string key in InnerCollection)
            {
                yield return string.Format("{0},{1}", key, InnerCollection[key]);
            }
        }
    }
    public enum PageType
    {
        Default,
        Static,
        Dynamic
    }

    [DataContract]
    public class HtmlMeta
    {
        [DataMember(Order = 1)]
        public string Author { get; set; }
        [DataMember(Order = 3)]
        public string Keywords { get; set; }
        [DataMember(Order = 5)]
        public string Description { get; set; }

        private Dictionary<string, string> customs = new Dictionary<string, string>();
        [DataMember(Order = 7)]
        public Dictionary<string, string> Customs
        {
            get
            {
                return customs;
            }
            set
            {
                customs = value;
            }
        }
        [DataMember(Order = 8)]
        public string HtmlTitle { get; set; }
    }
    [DataContract]
    public class PageRoute
    {
        public static readonly PageRoute Default = new PageRoute();


        [DataMember(Order = 1)]
        public string Identifier { get; set; }

        [DataMember(Order = 2)]
        public string RoutePath { get; set; }

        [DataMember(Order = 3)]
        public IDictionary<string, string> Defaults { get; set; }

        public Route ToMvcRoute()
        {
            return new Route(RoutePath, RouteValuesHelpers.GetRouteValues(Defaults), null);
        }
    }
    [DataContract]
    public class Navigation
    {
        [DataMember(Order = 1)]
        public bool Show { get; set; }
        [DataMember(Order = 3)]
        public string DisplayText { get; set; }
        [DataMember(Order = 5)]
        public int Order { get; set; }

        private bool? showInCrumb;
        [DataMember(Order = 7)]
        public bool? ShowInCrumb
        {
            get
            {
                if (showInCrumb == null)
                {
                    showInCrumb = true;
                }
                return showInCrumb;
            }
            set
            {
                showInCrumb = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PagePermission
    {
        /// <summary>
        /// <example>?,*,User1,Role1</example>
        /// </summary>
        /// <value>The allowed.</value>
        [DataMember(Order = 1)]
        public string Allowed { get; set; }
        [DataMember(Order = 3)]
        public string Denied { get; set; }
    }

    public static class PageHelper
    {
        public static string CombineFullName(IEnumerable<string> pageNames)
        {
            return string.Join("~", pageNames.ToArray());
        }
        public static IEnumerable<string> SplitFullName(string fullName)
        {
            return fullName.Split(new char[] { '~', '/' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static Page Parse(Site site, string fullName)
        {
            return new Page(site, SplitFullName(fullName).ToArray());
        }
    }

    [DataContract]
    public class Page : DirectoryResource, IPersistable, IInheritable<Page>, Kooboo.CMS.Sites.Versioning.IVersionable
    {
        public Page()
        {

        }
        public Page(Page parent, string name)
            : base(parent.Site, name)
        {
            this.Parent = parent;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="pageNamePaths">The page name paths. <example>{"parent","child"}</example></param>
        public Page(Site site, params string[] pageNamePaths)
            : base(site, pageNamePaths.Last())
        {
            SetNamePath(pageNamePaths);
        }

        private void SetNamePath(IEnumerable<string> pageNamePaths)
        {
            this.Name = pageNamePaths.Last();
            if (pageNamePaths.Count() > 1)
            {
                this.Parent = new Page(Site, pageNamePaths.Take(pageNamePaths.Count() - 1).ToArray());
            }
        }
        public Page(Site site, string fullName) :
            this(site, PageHelper.SplitFullName(fullName).ToArray())
        {

        }
        public Page(string physicalPath)
            : base(physicalPath)
        {
        }
        [DataMember(Order = 2)]//
        public bool IsDefault { get; set; }
        [Obsolete("Use HtmlMeta.HtmlTitle")]
        [DataMember(Order = 3)]//
        public string Title { get; set; }
        private bool enableTheming = true;
        [DataMember(Order = 4)]//
        public bool EnableTheming
        {
            get
            {
                return enableTheming;
            }
            set
            {
                this.enableTheming = value;
            }
        }
        private bool enableScript = true;
        [DataMember(Order = 5)]//
        public bool EnableScript
        {
            get
            {
                return enableScript;
            }
            set
            {
                enableScript = value;
            }
        }
        [DataMember(Order = 6)]//
        public HtmlMeta HtmlMeta { get; set; }
        private PageRoute route = new PageRoute();
        [DataMember(Order = 7)]//
        public PageRoute Route
        {
            get
            {
                if (route == null)
                {
                    return PageRoute.Default;
                }
                return route;
            }
            set
            {
                route = value;
            }
        }
        Navigation navigation = new Navigation();
        [DataMember(Order = 8)]//
        public Navigation Navigation
        {
            get
            {
                return this.navigation;
            }
            set { this.navigation = value; }
        }
        PagePermission permission = new PagePermission();
        [DataMember(Order = 9)]//
        public PagePermission Permission
        {
            get { return this.permission; }
            set { this.permission = value; }
        }

        /// <summary>
        /// wrap for Layout
        /// </summary>
        /// <value>The name of the layout template.</value>
        [DataMember(Order = 10)]//
        public string Layout
        {
            get;
            set;
        }

        private List<PagePosition> pagePositions = new List<PagePosition>();
        [DataMember(Order = 20)]//
        public List<PagePosition> PagePositions
        {
            get { return this.pagePositions; }
            set { this.pagePositions = value; }
        }

        private List<DataRuleSetting> dataRules = new List<DataRuleSetting>();
        [DataMember(Order = 25)]//
        public List<DataRuleSetting> DataRules
        {
            get { return this.dataRules; }
            set
            {
                this.dataRules = value;
            }
        }

        private List<string> plugins = new List<string>();
        [DataMember(Order = 27)]//
        public List<string> Plugins
        {
            get
            {
                return plugins;
            }
            set
            {
                plugins = value;
            }
        }
        [DataMember(Order = 29)]
        public PageType PageType { get; set; }

        [DataMember(Order = 30)]
        public CacheSettings OutputCache { get; set; }

        public bool EnabledCache
        {
            get
            {
                return OutputCache != null && OutputCache.Duration > 0;
            }
        }
        [DataMember(Order = 36)]
        public DynamicDictionary CustomFields { get; set; }

        private bool? published;
        [DataMember(Order = 38)]
        public bool? Published
        {
            get
            {
                if (published == null)
                {
                    published = true;
                }
                return published;
            }
            set
            {
                published = value;
            }
        }

        [DataMember(Order = 39)]
        public string UserName { get; set; }

        [DataMember(Order = 40)]
        public string ContentTitle { get; set; }

        [DataMember(Order = 41)]
        public bool Searchable { get; set; }

        #region override PathResource

        public virtual string FullName
        {
            get
            {
                return PageHelper.CombineFullName(PageNamePaths);
            }
            set
            {
                var names = PageHelper.SplitFullName(value);

                SetNamePath(names);
            }
        }

        public string FriendlyName
        {
            get
            {
                return string.Join("/", PageNamePaths.ToArray());
            }
        }

        const string PATH_NAME = "Pages";
        public virtual IEnumerable<string> PageNamePaths
        {
            get
            {
                if (this.Parent == null)
                {
                    return new string[] { this.Name };
                }
                return this.Parent.PageNamePaths.Concat(new string[] { this.Name }); ;
            }
        }
        public override string VirtualPath
        {
            get
            {
                if (Route != null && !string.IsNullOrEmpty(route.Identifier))
                {
                    return route.Identifier;
                }
                return UrlUtility.Combine(PageNamePaths.ToArray());
            }
        }
        public override IEnumerable<string> RelativePaths
        {
            get
            {
                return new string[] { PATH_NAME }.Concat(PageNamePaths.Take(PageNamePaths.Count() - 1));
            }
        }

        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            this.Name = relativePaths.Last();
            var pathNameIndex = relativePaths.IndexOf(Page.PATH_NAME, StringComparer.InvariantCultureIgnoreCase);
            var count = relativePaths.Count();
            if (pathNameIndex + 2 < count)
            {
                this.Parent = new Page();
                return this.Parent.ParseObject(relativePaths.Take(count - 1));
            }
            return relativePaths.Take(pathNameIndex);
        }
        #endregion

        #region parent
        public Page Parent
        {
            get;
            set;
        }

        public override Site Site
        {
            get
            {
                if (this.Parent != null)
                {
                    return Parent.Site;
                }
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (this.Parent != null)
                {
                    this.Parent.Site = value;
                }
            }
        }
        #endregion

        #region IPersistable Members

        #region IDummy Members
        private bool isDummy = true;
        public bool IsDummy
        {
            get
            {
                return isDummy;
            }
            set
            {
                isDummy = value;
            }
        }

        #endregion

        void IPersistable.Init(IPersistable source)
        {
            var sourcePage = (Page)source;
            this.IsDummy = false;

            this.Name = sourcePage.Name;

            this.Site = sourcePage.Site;
            this.Parent = sourcePage.Parent;

            //compatible with the old verion after the Title move to HtmlMeta.HtmlTitle
            if (this.HtmlMeta != null)
            {
                this.HtmlMeta.HtmlTitle = this.Title;
            }
        }
        void IPersistable.OnSaved()
        {
            this.IsDummy = false;
        }

        public string DataFileName
        {
            get
            {
                return "Setting.config";
            }
        }
        public string DataFile
        {
            get { return Path.Combine(this.PhysicalPath, DataFileName); }
        }
        public void OnSaving()
        {
            //compatible with the old verion after the Title move to HtmlMeta.HtmlTitle
            if (this.HtmlMeta != null)
            {
                this.Title = this.HtmlMeta.HtmlTitle;
            }
            //if (string.IsNullOrEmpty(this.PageUrlKey))
            //{
            //    PageUrlKey = this.VirtualPath;
            //}           
        }
        #endregion

        #region Override object
        public static bool operator ==(Page obj1, Page obj2)
        {
            if (object.Equals(obj1, obj2) == true)
            {
                return true;
            }
            if (object.Equals(obj1, null) == true || object.Equals(obj2, null) == true)
            {
                return false;
            }
            return obj1.Equals(obj2);
        }
        public static bool operator !=(Page obj1, Page obj2)
        {
            return !(obj1 == obj2);
        }
        public override string ToString()
        {
            return this.FullName;
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                Page page = (Page)obj;
                if (this.Site == page.Site && page.FullName.EqualsOrNullEmpty(this.FullName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region IInheritable<Page> Members

        public Page LastVersion()
        {
            var lastVersion = this;
            while (!lastVersion.Exists())
            {
                if (lastVersion.Site.Parent == null)
                {
                    break;
                }
                lastVersion = new Page(lastVersion.Site.Parent, this.PageNamePaths.ToArray());
            }
            return lastVersion;
        }

        public bool IsLocalized(Site site)
        {
            return this.Site == site;
        }

        public bool HasParentVersion()
        {
            var parentSite = this.Site.Parent;
            while (parentSite != null)
            {
                var page = new Page(parentSite, this.FullName);
                if (page.Exists())
                {
                    return true;
                }
                parentSite = parentSite.Parent;
            }
            return false;
        }
        #endregion

        #region Link
        public string LinkText
        {
            get
            {
                if (this.Navigation != null && !string.IsNullOrEmpty(this.Navigation.DisplayText))
                {
                    return this.Navigation.DisplayText;
                }
                return this.Name;
            }
        }
        #endregion
        public override bool Exists()
        {
            return File.Exists(this.DataFile);
        }
    }
}
