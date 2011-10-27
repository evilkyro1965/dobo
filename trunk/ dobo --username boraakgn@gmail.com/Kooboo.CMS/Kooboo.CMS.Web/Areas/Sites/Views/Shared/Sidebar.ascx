<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Kooboo.CMS.Web.Areas.Sites" %>
<div class="block">
    <h4 class="title">
        <span>
            <%:"Site manager".Localize() %></span><%: Html.ActionLink("Site cluster", "Index", new { controller = "Home", area = "sites" }, new { @class = "o-icon sites right", title = "Go to site cluster".Localize() })%></h4>
    <div class="switch clearfix">
        <% if (!string.IsNullOrWhiteSpace(ViewContext.RequestContext.GetRequestValue("SiteName")))
           {
               var currentSite = Kooboo.CMS.Sites.Models.Site.Current;
               var currentSiteName = currentSite == null ?
                                Request["SiteName"] : (string.IsNullOrEmpty(currentSite.DisplayName) ? Request["SiteName"] : currentSite.DisplayName);
               var isOnline = Kooboo.CMS.Sites.Services.ServiceFactory.SiteManager.IsOnline(Request["siteName"]);
               var onLineName = currentSiteName;
               var offlineName = currentSiteName + "(Offline)".Localize();
               var displayName = isOnline ? onLineName : offlineName;
        %>
        <span class="title" id="current-sitename" offline="<%:offlineName %>" online="<%:onLineName %>"
            title="<%= displayName%>">
            <%:displayName %></span>
        <%}
           else
           { %>
        <span>
            <%:"Hiçbir site seçili değil".Localize()%></span>
        <%} %>
        <% if (ServiceFactory.UserManager.IsAdministrator(Page.User.Identity.Name))
           {%>
        <%: Html.ActionLink("Create", "Create", "Site", new { area="Sites", siteName=ViewContext.RequestContext.GetRequestValue("siteName")}, new { @class = "icon icon-create dialog-link", title = ViewContext.RequestContext.GetRequestValue("siteName")==null? "Create a new site".Localize():"Create a sub site".Localize() })%>
        <%} %>
        <a class="icon icon-switch" href="#">Switch</a>
        <ul class="list">
            <%:Html.Partial(AreaHelpers.CombineAreaFileVirtualPath("Sites", "Views", "Shared", "SitesNavTree.ascx"), Kooboo.CMS.Web.Areas.Sites.Models.SiteDropDownListTree.BuildTree(ViewContext.RequestContext).SetActiveItem(ViewContext.RequestContext.GetRequestValue("SiteName")))%>
        </ul>
    </div>
    <%if (!string.IsNullOrEmpty(ViewContext.RequestContext.GetRequestValue("SiteName")))
      {%>
    <div class="menu">
        <%: Html.Partial("Menu", Kooboo.Web.Mvc.Menu.MenuFactory.BuildMenu(ViewContext.Controller.ControllerContext,"Sites").SetCurrentSite(this.ViewContext))%>
    </div>
    <% } %>
</div>
