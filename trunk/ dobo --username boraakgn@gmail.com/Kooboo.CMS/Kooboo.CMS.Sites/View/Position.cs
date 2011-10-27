using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Kooboo.CMS.Sites.Models;
using System.Web.Mvc;
namespace Kooboo.CMS.Sites.View
{

    public class Position : UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var positions = Page_Context.Current.PageRequestContext.Page.PagePositions;
            if (positions != null)
            {
                AddContents(positions.Where(it => it.LayoutPositionId.Equals(this.ID, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        private void AddContents(IEnumerable<PagePosition> positions)
        {
            foreach (var position in positions)
            {
                Page_Context.Current.PositionContext = new PagePositionContext() { PagePosition = position };

                if (position is ViewPosition)
                {
                    Page_Context.Current.PositionContext.ViewData = Page_Context.Current.GetPositionViewData(position.PagePositionId);
                    LiteralControl literal = new LiteralControl();
                    literal.Text = ((ViewPage)this.Page).Html.FrontHtml().RenderView((ViewPosition)position).ToString();
                    this.Controls.Add(literal);
                }
                else if (position is ModulePosition)
                {
                    LiteralControl literal = new LiteralControl();
                    literal.Text = ((ViewPage)this.Page).Html.FrontHtml().RenderModule((ModulePosition)position);
                    this.Controls.Add(literal);
                }
                else if (position is HtmlPosition)
                {
                    LiteralControl literal = new LiteralControl();
                    literal.Text = ((HtmlPosition)position).Html;
                    this.Controls.Add(literal);
                }
                else if (position is ContentPosition)
                {
                    LiteralControl literal = new LiteralControl();
                    literal.Text = ((ViewPage)this.Page).Html.FrontHtml().RenderContentPosition((ContentPosition)position).ToString();
                    this.Controls.Add(literal);
                }
            }
        }
        private void AddView(ViewPosition viewPosition)
        {
            Kooboo.CMS.Sites.Models.View View = new Kooboo.CMS.Sites.Models.View(Page_Context.Current.PageRequestContext.Site, viewPosition.ViewName).LastVersion();

            ViewUserControl control = (ViewUserControl)this.LoadControl(View.TemplateFileVirutalPath);
            if (string.IsNullOrEmpty(viewPosition.PagePositionId))
            {
                control.ID = string.Format("{0}_{1}", this.ID, (this.Controls.Count + 1));
            }
            else
            {
                control.ID = viewPosition.PagePositionId;
            }
            control.ViewData = Page_Context.Current.GetPositionViewData(viewPosition.PagePositionId);
            this.Controls.Add(control);
        }
    }
}
