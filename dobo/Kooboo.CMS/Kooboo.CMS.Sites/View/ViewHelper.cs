using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Globalization;
using Kooboo.CMS.Content.Models;
using System.Web;
using Kooboo.CMS.Sites.Services;
namespace Kooboo.CMS.Sites.View
{
    /// <summary>
    /// For NVelocity
    /// </summary>
    public class ViewHelper
    {
        public static IHtmlString Label(string defaultValue)
        {
            return Label(defaultValue, defaultValue);
        }
        public static IHtmlString Label(string defaultValue, string key)
        {
            return Label(defaultValue, key, "");
        }
        public static IHtmlString Label(string defaultValue, string key, string category)
        {
            return defaultValue.Label(key, category);
        }

        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
        public static DateTime ToLocalDate(DateTime dateTime)
        {
            return dateTime.ToLocalTime();
        }
        public static string DateTimeToString(DateTime dateTime, string format)
        {
            return dateTime.ToString(format);
        }

        #region InlineEdit
        public static IHtmlString Edit(TextContent data, string fieldName)
        {
            if (data == null || !Page_Context.Current.EnabledInlineEditing(EditingType.Content)
                || !Kooboo.CMS.Content.Services.ServiceFactory.WorkflowManager.AvailableToEditContent(data, Page_Context.Current.ControllerContext.HttpContext.User.Identity.Name))
            {
                return new HtmlString("");
            }
            return new HtmlString(string.Format("editType='field' schema='{0}' uuid='{1}' fieldName='{2}'", data.SchemaName, data.UUID, fieldName));
        }
        public static IHtmlString Edit(TextContent data)
        {
            if (data == null || !Page_Context.Current.EnabledInlineEditing(EditingType.Content)
                || !Kooboo.CMS.Content.Services.ServiceFactory.WorkflowManager.AvailableToEditContent(data, Page_Context.Current.ControllerContext.HttpContext.User.Identity.Name))
            {
                return new HtmlString("");
            }
            return new HtmlString(string.Format("editType='list' schema='{0}' uuid='{1}' published='{2}' editUrl='{3}' summary='{4}'",
                data.SchemaName, data.UUID, data.Published
                , Page_Context.Current.Url.Action("Edit", new
                {
                    controller = "TextContent",
                    Area = "Contents",
                    RepositoryName = data.Repository,
                    SiteName = Page_Context.Current.PageRequestContext.Site.FullName,
                    FolderName = data.FolderName,
                    UUID = data.UUID
                }), HttpUtility.HtmlAttributeEncode(data.GetSummary())));
        }
        public static IHtmlString EditField(TextContent data, string fieldName)
        {
            if (data == null || !Page_Context.Current.EnabledInlineEditing(EditingType.Content)
                || !Kooboo.CMS.Content.Services.ServiceFactory.WorkflowManager.AvailableToEditContent(data, Page_Context.Current.ControllerContext.HttpContext.User.Identity.Name))
            {
                return new HtmlString(data[fieldName] == null ? "" : data[fieldName].ToString());
            }
            var format = "<var editType='field' schema='{0}' uuid='{1}' fieldName='{2}' style='display:none' start></var>{3}<var style='display:none' end></var>";
            return new HtmlString(string.Format(format, data.SchemaName, data.UUID, fieldName, data[fieldName]));
        }

        #endregion
    }
}
