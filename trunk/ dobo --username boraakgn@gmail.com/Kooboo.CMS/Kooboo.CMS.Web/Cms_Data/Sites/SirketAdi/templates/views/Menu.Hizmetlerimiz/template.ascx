<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<ul class="list-1 p3">
    <% foreach(dynamic item in ViewBag.Hizmetlerimiz)
      {%>
    <li>
      <a href='<%:item.Link %>'><%:item.ShowName%></a>
    </li>
    <%}%>
</ul>