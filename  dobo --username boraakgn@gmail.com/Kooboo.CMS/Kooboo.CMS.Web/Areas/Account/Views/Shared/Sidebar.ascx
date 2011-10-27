<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Kooboo.CMS.Web.Areas.Contents" %>
<div class="block">
    <h4>Kullanıcı Yönetimi</h4>
    <div class="menu">
        <%: Html.Partial("Menu", Kooboo.Web.Mvc.Menu.MenuFactory.BuildMenu(ViewContext.Controller.ControllerContext).SetCurrentRepository(this.ViewContext))%>
    </div>
</div>
