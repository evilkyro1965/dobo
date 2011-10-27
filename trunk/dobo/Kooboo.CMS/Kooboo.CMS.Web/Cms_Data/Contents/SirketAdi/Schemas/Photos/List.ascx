<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<IDictionary<string,object>>>" %>
<h3>Photos</h3>
<ul class="list">
    <%foreach (var item in Model)
      {%>
        <li><%: Html.FrontHtml().PageLink(item["Name"], "Photos/detail", new { userKey = item["UserKey"] }, new { @class="title" })%>
        </li>
      <%} %>
</ul>