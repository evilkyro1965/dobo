using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html
{
    public static class HtmlCodeHelper
    {
        /// <summary>
        /// abc"cde => abc""cde, 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EscapeQuote(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            return s.Replace("\"", "\"\"");
        }
        public static string RazorHtmlEncode(this string s)
        {
            return System.Web.HttpUtility.HtmlEncode(s).Replace("@", "@@");
        }
        public static string RazorHtmlAttributeEncode(this string s)
        {
            return System.Web.HttpUtility.HtmlAttributeEncode(s).Replace("@", "@@");
        }
    }
}
