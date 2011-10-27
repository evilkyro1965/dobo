<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%
    var schema = (Kooboo.CMS.Content.Models.Schema)ViewData["Schema"];
    var allowedEdit = (bool)ViewData["AllowedEdit"] ;
    var allowedView = (bool)ViewData["AllowedView"];
%>
<% using (Html.BeginForm(ViewContext.RequestContext.AllRouteValues()["action"].ToString(), ViewContext.RequestContext.AllRouteValues()["controller"].ToString(), ViewContext.RequestContext.AllRouteValues(), FormMethod.Post, new RouteValueDictionary(new { enctype = "application/x-www-form-urlencoded" })))
{%>
<div class="common-form">
<fieldset>
<table><tr>
            <td>
            <label for="Name"><%: "Resim Adı"%></label>
            </td>
            <td>
            <input id="Name" name="Name" type="text" value="<%= Model.Name ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Name"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Description"><%: "Açıklama"%></label>
            </td>
            <td>
            <input id="Description" name="Description" type="text" value="<%= Model.Description ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Description"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Path"><%: "Adres"%></label>
            </td>
            <td>
            <input id="Path" name="Path" type="file" value="<%= Model.Path ?? ""%>"  displayValue="<%= Model.Path ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Path"], null)%>
            </td>          
            </tr><% 
            if (allowedEdit) { %>
                <tr>
            <td>
            <label for="Published"><%: "Yayınlandı"%></label>
            </td>
            <td>
            <input name="Published" type="checkbox" <%:Convert.ToBoolean(Model.Published)?"checked":""%> value="true"/>
                                    <input type="hidden" value="false" name="Published"/>
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Published"], null)%>
            </td>          
            </tr>
            <%}%>
<%:Html.Action("Categories", ViewContext.RequestContext.AllRouteValues())%>
</table>
</fieldset>
<p class="buttons"><button type="submit">Save</button></p>
<%} %>
</div>