using System;
using System.Linq;
using System.Collections.Generic;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.View;
using Kooboo.Globalization;
using Kooboo.CMS.Sites.Caching;
using System.Web;
namespace Kooboo.CMS.Sites.Globalization
{
    public static class SiteLabel
    {
        public static IElementRepository GetElementRepository(Site site)
        {
            string cacheKey = "SiteLabelRepository";
            var repository = site.ObjectCache().Get(cacheKey);
            if (repository == null)
            {
                repository = new SiteLabelRepository(site);
                site.ObjectCache().Add(cacheKey, repository, new System.Runtime.Caching.CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.Parse("00:30:00")
                });
            }
            return (IElementRepository)repository;
        }

        #region Label with inline-editing.
        /// <summary>
        /// Label with inline-editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static IHtmlString Label(this string defaultValue)
        {
            return Label(defaultValue, defaultValue);
        }
        /// <summary>
        /// Label with inline-editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static IHtmlString Label(this string defaultValue, string key, string category = "")
        {
            //var pageViewContext = Page_Context.Current;

            //pageViewContext.CheckContext();

            return Label(defaultValue, key, category, Page_Context.Current.PageRequestContext.Site);
        }
        /// <summary>
        /// Label with inline-editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        public static IHtmlString Label(this string defaultValue, string key, string category, Site site)
        {
            string value = LabelValue(defaultValue, key, category, site);

            if (Kooboo.Settings.IsWebApplication && Page_Context.Current.EnabledInlineEditing(EditingType.Label))
            {
                value = string.Format("<var editType='label' key='{0}' category='{1}' style='display:none' start></var>{2}<var style='display:none' end></var>"
                    , HttpUtility.HtmlEncode(key)
                    , HttpUtility.HtmlEncode(category)
                    , value);
            }

            return new HtmlString(value);
        } 
        #endregion

        #region RawLabel Label without inline editing.
        /// <summary>
        /// Raws the label. Label without inline editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static IHtmlString RawLabel(this string defaultValue)
        {
            return RawLabel(defaultValue, defaultValue);
        }
        /// <summary>
        /// Raws the label. Label without inline editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static IHtmlString RawLabel(this string defaultValue, string key, string category = "")
        {
            return RawLabel(defaultValue, key, category, Page_Context.Current.PageRequestContext.Site);
        }
        /// <summary>
        /// Raws the label. Label without inline editing.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        public static IHtmlString RawLabel(this string defaultValue, string key, string category, Site site)
        {
            return new HtmlString(LabelValue(defaultValue, key, category, site));
        }

        #endregion

        private static string LabelValue(string defaultValue, string key, string category, Site site)
        {
            var repository = GetElementRepository(site);

            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            var element = repository.Get(key, category, culture.Name);

            string value = "";
            if (element == null)
            {
                element = new Element() { Name = key, Category = category, Culture = culture.Name, Value = defaultValue };

                repository.Add(element);

                value = element.Value;
            }
            else
            {
                value = element.Value;
            }
            return value;
        }
    }
}
