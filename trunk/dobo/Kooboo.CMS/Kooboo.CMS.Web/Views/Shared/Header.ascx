<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<div id="header" class="clearfix">
    <h1 class="logo">
        <a href="<%=Url.Action("SiteMap","Home",new {area="Sites"}) %>">
            <img src="<%=Url.Content("~/Images/logo.png") %>" alt="LOGO" /></a></h1>
    <ul class="quicklink">
        <li><strong>
            <%: ViewContext.HttpContext.User.Identity.Name %></strong></li>
        <li>
            <%:Html.ActionLink("Change Password".Localize(), "ChangePassword", new { area = "account", controller = "Users" }, new { @class = "dialog-link", title = "Change password".Localize() })%>
        </li>
        <%if (ServiceFactory.UserManager.IsAdministrator(Page.User.Identity.Name))
          { %>
        <li>
            <%: Html.ActionLink("Users".Localize(), "index", new { controller="Users",area="account" }) %></li>
        <li>
            <%: Html.ActionLink("Roles".Localize(), "index", new { controller = "Roles", area = "account" })%></li>
        <li>
            <%: Html.ActionLink("Modules".Localize(), "Index", new { controller = "ModuleManagement", area = "Sites" })%></li>
        <%} %>
        <li class="last">
            <%: Html.ActionLink("Sign Out".Localize(), "SignOut", new { controller = "Logon", area = "account" })%></li>
    </ul>
</div>
