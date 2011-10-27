<%@ Page Title="" Language="C#" MasterPageFile="~/Areas/Contents/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.IEnumerable<Kooboo.CMS.Content.Models.Folder>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>
      <%var routes = ViewContext.RequestContext.AllRouteValues();
          routes["fullName"] = Request["fullName"]??Request["Folder"];
             %>
    <div class="command">
        <%:Html.Partial("FolderNavigation") %>
    </div>
</asp:Content>
