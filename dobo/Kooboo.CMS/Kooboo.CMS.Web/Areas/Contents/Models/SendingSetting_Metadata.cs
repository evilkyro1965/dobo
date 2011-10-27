using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.CMS.Content.Models;
using Kooboo.Web.Mvc.Grid;
using System.ComponentModel.DataAnnotations;
using Kooboo.Web.Mvc;
using Kooboo.CMS.Web.Models;
using Kooboo.Globalization;
using System.Web.Mvc;
using System.ComponentModel;

namespace Kooboo.CMS.Web.Areas.Contents.Models
{
    //public class Broadcasting_PublishedRender : IItemColumnRender
    //{
    //    public IHtmlString Render(object dataItem, object value, System.Web.Mvc.ViewContext viewContext)
    //    {
    //        string s = "All";
    //        var published = (bool?)value;
    //        if (published.HasValue)
    //        {
    //            if (published.Value == true)
    //            {
    //                s = "Published";
    //            }
    //            else
    //            {
    //                s = "Unpublished";
    //            }
    //        }

    //        return new HtmlString(s.Localize());
    //    }
    //}

    public class SendReceivedColumnRender : BooleanColumnRender
    {
        public override IHtmlString Render(object dataItem, object value, ViewContext viewContext)
        {
            var sendingSetting = (SendingSetting)dataItem;
            var urlHelper = new UrlHelper(viewContext.RequestContext);
            var tip = "Change send received content setting".Localize();
            string url = urlHelper.Action("ChangeSendReceived", viewContext.RequestContext.AllRouteValues().Merge("Name", sendingSetting.Name));
            return new HtmlString(string.Format(@"<a class=""o-icon {0} actionCommand  "" href=""{1}"" title=""{2}"">{1}</a>"
                   , GetIconClass(value)
                   , url
                   , tip));

        }
    }
    [Grid(Checkable = true, IdProperty = "Name")]
    //[GridAction(ActionName = "Edit", RouteValueProperties = "Name", Order = 1, Class = "o-icon edit dialog-link")]
    public class SendingSetting_Metadata
    {
        public Repository Repository { get; set; }

        public string Name { get; set; }

        [GridColumn(Order = 1)]
        [UIHint("SingleFolderTree")]
        [DataSource(typeof(FolderTreeDataSource))]
        [Display(Name = "Folder name")]
        [RemoteEx("IsNameAvailable", "*", RouteFields = "RepositoryName")]
        public string FolderName { get; set; }

        [GridColumn(Order = 2, ItemRenderType = typeof(SendReceivedColumnRender))]
        [Display(Name = "Send recieved content")]
        [Description("Broadcasting out contents that received from other websites")]
        public bool? SendReceived { get; set; }
        //[UIHint("PublishStatus")]
        //[Display(Name = "Content status")]
        //[GridColumn(Order = 2, ItemRenderType = typeof(Broadcasting_PublishedRender))]
        //public bool? Published { get; set; }

        //[GridColumn(Order = 3)]
        //[UIHint("AcceptAction")]
        //[Display(Name = "Accept action")]
        //public ContentAction AcceptAction { get; set; }
    }
}