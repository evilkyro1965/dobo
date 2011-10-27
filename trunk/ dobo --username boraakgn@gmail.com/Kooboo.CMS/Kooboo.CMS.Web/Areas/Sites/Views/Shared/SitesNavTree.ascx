<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<Kooboo.Web.Mvc.SelectListItemTree>>" %>
<%
    var level = ViewData["level"] != null ? (int)ViewData["level"] : 0;
    foreach (var m in Model)
    {
        if (string.IsNullOrEmpty(m.Value)) { continue; }
%>
<li><a href="<%= Url.Action("SiteMap", new { controller="Home",siteName=m.Value,area="sites"})%>" title="<%:m.Text %>">
    <%for (var i = 0; i < level; i++)
      {%>&nbsp;&nbsp;&nbsp;&nbsp;<% } %><%:m.Text %></a></li>
<%if (m.Items != null)
  {
      var viewDataDictionary = new ViewDataDictionary();
      viewDataDictionary.Add("level", level + 1);
%>
<%:Html.Partial(AreaHelpers.CombineAreaFileVirtualPath("Sites","Views","Shared","SitesNavTree.ascx"), m.Items, viewDataDictionary)%>
<%} %>
<% } %>