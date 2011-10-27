<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Sites.Services.Namespace<Kooboo.CMS.Sites.Models.View>>" %>
<%@ Import Namespace="Kooboo.CMS.Sites.Models" %>
<%
    var hasChildNs = Model.ChildNamespaces != null && Model.ChildNamespaces.Count() > 0;
    var hasChildEt = Model.Entries != null && Model.Entries.Count() > 0;
    var hasChild = hasChildNs || hasChildEt;
%>
<%if (!string.IsNullOrEmpty(Model.Name))
  {%>
<li class="namespace<%if (hasChild){%> has-child active<%}%>"><span class="title">
    <%=Model.Name ?? "root"%></span>
    <ul>
<%}%>
        <%if (hasChildNs)
          { %>
        <%foreach (var ns in Model.ChildNamespaces)
          { %>
        <%:Html.Partial("ViewNamespace", ns)%>
        <%} %>
        <%} %>

        <%if (hasChildEt)
          { %>
        <%foreach (var et in Model.Entries)
          { %>
        <%:Html.Partial("ViewItem",  et.EntryObject.AsActual())%>
        <%} %>
        <%} %>

<%if (!string.IsNullOrEmpty(Model.Name))
  {%>
    </ul>
</li>
<%}%>