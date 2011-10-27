<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%
    var schema = (Kooboo.CMS.Content.Models.Schema)ViewData["Schema"];
%>
<% using (Html.BeginForm(ViewContext.RequestContext.AllRouteValues()["action"].ToString(), ViewContext.RequestContext.AllRouteValues()["controller"].ToString(), ViewContext.RequestContext.AllRouteValues(), FormMethod.Post, new RouteValueDictionary(new { enctype = "application/x-www-form-urlencoded" })))
{%>
<div class="common-form">
<fieldset>
<table><tr>
            <td>
            <label for="Title"><%: "Title"%></label>
            </td>
            <td>
            <input id="Title" name="Title" type="text" value="<%= Model.Title ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Title"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Published"><%: "Published"%></label>
            </td>
            <td>
            <input name="Published" type="checkbox" <%:Convert.ToBoolean(Model.Published)?"checked":""%> value="true"/>
                                    <input type="hidden" value="false" name="Published"/>
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Published"], null)%>
            </td>          
            </tr><% if (Kooboo.CMS.Content.Models.Repository.Current.EnableManuallyUserKey)
                        {  %>
<tr>
            <td>
            <label for="UserKey"><%: "User Key"%></label><a href="javascript:;" class="tooltip-link" title="<%: "An user and SEO friendly content key, it is mostly used to customize the page URL" %>"></a>
            </td>
            <td>
            <input id="UserKey" name="UserKey" type="text" value="<%= Model.UserKey ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["UserKey"], null)%>
            </td>          
            </tr><%} %>

<%:Html.Action("Categories", ViewContext.RequestContext.AllRouteValues())%>
</table>
</fieldset>
<p class="buttons"><button type="submit">Save</button></p>
<%} %>
</div>