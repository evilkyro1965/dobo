using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Sites.View
{
    public class SubmissionHelper
    {
        public static string CreateContentUrl()
        {
            return Page_Context.Current.Url.Action("Create", new { controller = "ContentService", area = "Contents", siteName = Page_Context.Current.PageRequestContext.Site.FullName, repositoryName = Repository.Current.Name });
        }
        public static string UpdateContentUrl()
        {
            return Page_Context.Current.Url.Action("Update", new { controller = "ContentService", area = "Contents", siteName = Page_Context.Current.PageRequestContext.Site.FullName, repositoryName = Repository.Current.Name });
        }
        public static string DeleteContentUrl(string schemaName, string uuid)
        {
            return Page_Context.Current.Url.Action("Delete", new { controller = "ContentService", area = "Contents", siteName = Page_Context.Current.PageRequestContext.Site.FullName, repositoryName = Repository.Current.Name, schemaName = schemaName, uuid = uuid });
        }

        public static string EmailUrl()
        {
            return Page_Context.Current.Url.Action("Email", new { controller = "Submission", area = "Sites", siteName = Page_Context.Current.PageRequestContext.Site.FullName, repositoryName = Repository.Current.Name });
        }
    }
}
