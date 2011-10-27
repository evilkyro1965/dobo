using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.Web.Mvc.Grid;
using Kooboo.CMS.Sites.Models;
using System.Web.Mvc;

namespace Kooboo.CMS.Web.Areas.Sites.Models
{
    public class InheritableGridActionVisibleArbiter : IVisibleArbiter
    {

        #region IGridActionVisibleArbiter Members

        public virtual bool IsVisible(object dataItem, System.Web.Mvc.ViewContext viewContext)
        {
            var inheritable = dataItem as IInheritable;
            if (inheritable != null)
            {
                return inheritable.IsLocalized(((Controllers.AdminControllerBase)viewContext.Controller).Site);
            }
            else
            {
                return true;
            }

        }

        #endregion
    }
    public class LocalizationRender : IGridItemActionRender
    {
        public GridItemAction Render(object dataItem, GridItemAction itemAction, ViewContext viewContext)
        {
            var inheritable = dataItem as IInheritable;
            if (inheritable != null)
            {
                var localized = inheritable.IsLocalized(((Controllers.AdminControllerBase)viewContext.Controller).Site);
                if (localized)
                {
                    var hasParent = inheritable.HasParentVersion();
                    if (hasParent)
                    {
                        itemAction.ActionName = "Unlocalize";
                        itemAction.Title = "Unlocalize";
                        itemAction.Class = "o-icon unlocalize actionCommand";
                        itemAction.ConfirmMessage = "Are you sure you want to unlocalize this item?";
                    }
                    else
                    {
                        itemAction.Title = "Localize";
                        itemAction.Visible = false;
                    }
                }

            }

            return itemAction;

        }
    }

}