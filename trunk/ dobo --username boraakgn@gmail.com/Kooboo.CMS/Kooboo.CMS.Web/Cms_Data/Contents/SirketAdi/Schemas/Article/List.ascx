<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<IDictionary<string,object>>>" %>
<h3>Article</h3>
<ul class="list">
    <%foreach (var item in Model)
      {%>
        <li><%: Html.FrontHtml().PageLink(item["Title"], "Article/detail", new { userKey = item["UserKey"] })%>
        </li>
      <%} %>
</ul>