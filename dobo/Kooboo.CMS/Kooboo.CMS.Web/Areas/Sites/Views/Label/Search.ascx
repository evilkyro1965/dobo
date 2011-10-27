<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<form id="search" action="">
<%--<div style="float: left;">
    <%: Html.DropDownList("Culture", (new Kooboo.CMS.Web.Areas.Sites.Models.ElementCategoryCulturesSelectListDataSource()).GetSelectListItems(ViewContext),"Culture", new { id = "searchCulture" })%>
</div>--%>
<div style="float: left;">
    <input type="text" name="search" id="searchBox" />
    <input type="submit" value="<%="Search" %>" />
</div>
<%: Html.Hidden("category",ViewContext.RequestContext.GetRequestValue("category")) %>
</form>
<script type="text/javascript">
    $(function ($) {
        $("#searchBox").focus();
        $("#searchBox").val('<%= Request["search"] %>');
        $("#searchCulture").Watermark("Culture");
        $("#searchCulture").val('<%= Request["Culture"] %>');
        $("#searchCulture").change(function () {
            $("#search").submit();
        });
    });
</script>
