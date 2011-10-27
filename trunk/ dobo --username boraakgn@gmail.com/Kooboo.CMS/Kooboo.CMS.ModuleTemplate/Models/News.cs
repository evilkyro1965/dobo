using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.Web.Mvc.Grid;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Kooboo.CMS.ModuleTemplate.Models
{
    [GridAction(DisplayName = "Edit", ActionName = "Edit", RouteValueProperties = "Id", Order = 1, Class = "o-icon edit", Title = "Edit")]
    [Grid(Checkable = true, IdProperty = "Id")]
    public class News
    {
        [GridColumnAttribute()]
        public int Id { get; set; }
        [Required]
        [GridColumnAttribute()]
        public string Title { get; set; }
        [DataType("Tinymce")]
        public string Body { get; set; }
    }
}