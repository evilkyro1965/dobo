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
            <label for="Name"><%: "Adı"%></label>
            </td>
            <td>
            <input id="Name" name="Name" type="text" value="<%= Model.Name ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Name"], null)%>
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
            <label for="Fax"><%: "Fax"%></label>
            </td>
            <td>
            <input id="Fax" name="Fax" type="text" value="<%= Model.Fax ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Fax"], null)%>
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
            
<textarea name="Address" id="Address" class="Address tinymce" media_library_url="<%:Url.Action("Selection","MediaContent",ViewContext.RequestContext.AllRouteValues())%>"  media_library_title ="<%:"Selected Files".Localize()%>" rows="10" cols="100"><%= Model.Address ?? "" %></textarea>

            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Address"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Longitude"><%: "Longitude"%></label>
            </td>
            <td>
            <input id="Longitude" name="Longitude" type="text" value="<%= Model.Longitude ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Longitude"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Latitude"><%: "Latitude"%></label>
            </td>
            <td>
            <input id="Latitude" name="Latitude" type="text" value="<%= Model.Latitude ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Latitude"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Image"><%: "Resim"%></label>
            </td>
            <td>
            <input id="Image" name="Image" type="file" value="<%= Model.Image ?? ""%>"  displayValue="<%= Model.Image ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Image"], null)%>
            </td>          
            </tr><tr>
            <td>
            <label for="Logo"><%: "Şirket Logosu"%></label>
            </td>
            <td>
            <input id="Logo" name="Logo" type="file" value="<%= Model.Logo ?? ""%>"  displayValue="<%= Model.Logo ?? ""%>" />
            <%: Html.ValidationMessageForColumn(((ISchema)ViewData["Schema"])["Logo"], null)%>
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
<p class="buttons"><button type="submit">Save</button> <a href="javascript:;" class="dialog-close button"><%:"Close".Localize()%></a> </p>
<%} %>
</div>