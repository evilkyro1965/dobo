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
<table><% 
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
<p class="buttons"><button type="submit">Save</button> <a href="javascript:;" class="dialog-close button"><%:"Close".Localize()%></a> </p>
<%} %>
</div>