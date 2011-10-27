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
            <label for="Name"><%: "Ad"%></label>
            </td>
            <td>
            <input id="Name" name="Name" type="text" value="<%= Model.Name ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Name"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="EMail"><%: "E-Posta"%></label>
            </td>
            <td>
            <input id="EMail" name="EMail" type="text" value="<%= Model.EMail ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["EMail"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Address"><%: "Adres"%></label>
            </td>
            <td>
            <input id="Address" name="Address" type="text" value="<%= Model.Address ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Address"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Phone"><%: "Telefon"%></label>
            </td>
            <td>
            <input id="Phone" name="Phone" type="text" value="<%= Model.Phone ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Phone"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Message"><%: "Mesaj"%></label>
            </td>
            <td>
            <textarea name="Message" rows="10" cols="100"><%: Model.Message ?? "" %></textarea> 
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Message"], null)%>
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