using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.CMS.Content.Models;
using System.Collections.Specialized;
using Kooboo.Web.Mvc.Grid;
using System.ComponentModel.DataAnnotations;
using Kooboo.Web.Mvc;
using System.Text;
using Kooboo.CMS.Web.Models;
using Kooboo.Globalization;

namespace Kooboo.CMS.Web.Areas.Contents.Models
{
    [Grid(IdProperty = "Name", Checkable = true)]
    [GridAction(ActionName = "Edit", RouteValueProperties = "Name", Order = 1, Class = "o-icon edit dialog-link")]
    public class ReceivingSetting_Metadata
    {
        public string Repository { get; set; }

        public string Name
        {
            get;
            set;
        }

        [GridColumn(Order = 1)]
        [UIHint("DropDownList")]
        [DataSource(typeof(RepositoryDataSource))]
        [Display(Name = "Sending repository")]
        public string SendingRepository { get; set; }

        [GridColumn(Order = 2)]
        [UIHint("DropDownListTree")]
        [Display(Name = "Sending folder")]
        //[DataSource(typeof(FolderTreeDataSource))]
        public string SendingFolder { get; set; }

        //[GridColumn(Order = 3)]
        //[UIHint("AcceptAction")]
        //[Display(Name = "Accept action")]
        //public ContentAction AcceptAction { get; set; }

        //[GridColumn(Order = 4, ItemRenderType = typeof(Broadcasting_PublishedRender))]
        //[UIHint("PublishStatus")]
        //[Display(Name = "Accept conent status")]
        //public bool? Published { get; set; }

        [UIHint("SingleFolderTree")]
        [DataSource(typeof(FolderTreeDataSource))]
        [GridColumn(Order = 5)]
        [Display(Name = "Receiving folder")]
        public string ReceivingFolder { get; set; }

        [GridColumn(Order = 6, ItemRenderType = typeof(BooleanColumnRender))]
        [UIHint("KeepStatus")]
        [Display(Name = "Keep content stataus")]
        public bool KeepStatus { get; set; }
    }
    //public class SetPublishedRender : IItemColumnRender
    //{

    //    public IHtmlString Render(object dataItem, object value, System.Web.Mvc.ViewContext viewContext)
    //    {
    //        string s = "No";
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
    //public class ReceiveingFolderColumnRender : IItemColumnRender
    //{
    //    public IHtmlString Render(object dataItem, object value, System.Web.Mvc.ViewContext viewContext)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        var list = ((ReceivingSetting)dataItem).ReceivingFolder;
    //        var index = 0;
    //        foreach (var i in list)
    //        {
    //            sb.Append(i);
    //            if (list.Length != (index++) + 1)
    //                sb.Append(",");
    //        }
    //        return new HtmlString(sb.ToString());
    //    }
    //}
}