﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Sites.Models.PageRoute>" %>
<fieldset>    
    <table>
        <tbody>
            <%:Html.EditorFor(m => m.Identifier, new { @class = "medium" })%>
            <%:Html.EditorFor(m => m.RoutePath, new { @class = "medium" })%>
            <%:Html.EditorFor(m=>m.Defaults) %>
        </tbody>
    </table>
</fieldset>
