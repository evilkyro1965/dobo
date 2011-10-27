<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Sites.Models.View>" %>
<li class="viewitem">
    <%var id = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(16); %>
    <input type="checkbox" id="<%=id%>" value="<%=Model.Name%>" name="ViewName" />
    <label class="viewname" for="<%=id%>">
        <%=Model.Name%></label>
    <%if (Model.Parameters != null && Model.Parameters.Count > 0)
      { %>
    <div class="parameters" style="display: none;">
        <table>
            <%foreach (var p in Model.Parameters)
              { %>
            <tr>
                <th>
                    <%=p.Name%>:
                </th>
                <td>
                    <input type="hidden" kb_name="Name" value="<%=p.Name%>" />
                    <input type="hidden" kb_name="DataType" value="<%=p.DataType%>" />
                    <%if (p.DataType == Kooboo.Data.DataType.Bool)
                      {
                          var check = (p.Value != null && p.Value.ToString().ToLower() == "true") ? "checked=\"checked\"" : "";
                    %>
                    <input kb_name="Value" type="checkbox" value="true" <%=check%> />
                    <%}
                      else
                      {%>
                    <input kb_name="Value" type="text" value="<%=p.Value is DateTime? ((DateTime)p.Value).ToLocalTime().ToShortDateString():p.Value%>" />
                    <%} %>
                </td>
            </tr>
            <%} %>
        </table>
    </div>
    <%} %>
</li>
