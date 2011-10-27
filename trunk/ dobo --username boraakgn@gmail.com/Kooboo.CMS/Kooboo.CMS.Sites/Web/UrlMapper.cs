using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.Text.RegularExpressions;

namespace Kooboo.CMS.Sites.Web
{
    public interface IUrlMapper
    {
        bool Map(Site site, string inputUrl, out string outputUrl, out System.Net.HttpStatusCode statusCode);
    }
    public static class UrlMapperFactory
    {
        static UrlMapperFactory()
        {
            Default = new DefaultUrlMapper();
        }
        public static IUrlMapper Default { get; set; }
    }
    public class DefaultUrlMapper : IUrlMapper
    {
        #region IUrlMapper Members

        public bool Map(Site site, string inputUrl, out string outputUrl, out System.Net.HttpStatusCode statusCode)
        {
            outputUrl = string.Empty;
            statusCode = System.Net.HttpStatusCode.OK;
            if (string.IsNullOrEmpty(inputUrl))
            {
                return false;
            }
            var mapSettings = Services.ServiceFactory.UrlRedirectManager.All(site, "");
            inputUrl = inputUrl.Trim('/');
            foreach (var setting in mapSettings)
            {
                var inputPattern = setting.InputUrl.Trim('/');
                if (setting.Regex)
                {
                    if (Regex.IsMatch(inputUrl, inputPattern, RegexOptions.IgnoreCase))
                    {
                        outputUrl = Regex.Replace(inputUrl, inputPattern, setting.OutputUrl, RegexOptions.IgnoreCase);
                        statusCode = (System.Net.HttpStatusCode)((int)setting.RedirectType);
                        return true;
                    }
                }
                else
                {
                    if (inputUrl.EqualsOrNullEmpty(inputPattern, StringComparison.CurrentCultureIgnoreCase))
                    {
                        outputUrl = setting.OutputUrl;
                        statusCode = (System.Net.HttpStatusCode)((int)setting.RedirectType);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
