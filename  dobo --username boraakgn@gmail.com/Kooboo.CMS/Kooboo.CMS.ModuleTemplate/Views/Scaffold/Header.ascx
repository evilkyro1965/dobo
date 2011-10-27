<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<div id="header" class="clearfix">
    <div class="topbar clearfix">
        <ul class="left">
            <li><a href="#">CMS</a></li>
            <li><a href="#">Communicator</a></li>
            <li><a href="#">E-Commerce</a></li>
        </ul>
        <ul class="right">
            <li><strong>
                <%: ViewContext.HttpContext.User.Identity.Name %></strong></li>
        </ul>
    </div>
    <h1 class="logo">
        <img src="<%:Url.Content("~/Views/Scaffold/Images/logo.png") %>" alt="LOGO" /></h1>
</div>
