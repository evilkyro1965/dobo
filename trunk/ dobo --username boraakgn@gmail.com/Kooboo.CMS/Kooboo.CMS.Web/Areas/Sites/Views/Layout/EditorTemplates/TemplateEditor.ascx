<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<% ViewData.TemplateInfo.HtmlFieldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix.Replace(ViewData.ModelMetadata.PropertyName, "").Trim('.');
%>
<tr>
    <td>
        <%: Html.TextArea(ViewData.ModelMetadata.PropertyName, Model == null ? (string)(ViewBag.DefaultLayout == null ? "" : ViewBag.DefaultLayout.Template) : Model.ToString(), new { rows = 30, cols = 20 })%>
        <% if (!string.IsNullOrEmpty(ViewData.ModelMetadata.Description))
           {%>
        <a href="javascript:;" class="tooltip-link" title="<%: ViewData.ModelMetadata.Description %>">
        </a>
        <%} %>
    </td>
    <td style="width: 250px;">
        <div class="task-panel">
            <div class="task-block">
                <h3 class="title">
                    <span><%="Positions".Localize() %></span><span class="arrow"></span></h3>
                <div class="content">
                    <p class="buttons clearfix">
                        <a href="#" class="button addPosition">Add</a></p>
                    <ul class="list positions">
                    </ul>
                </div>
            </div>
            <div class="task-block">
                <h3 class="title">
                    <span><%="Layout helper".Localize() %></span><span class="arrow"></span></h3>
                <div class="content block-list">
                    <ul>
                        <li class="has-sub codeSample"><a href="javascript:;">
                            <%="HTML header".Localize() %></a>
                            <%: Html.Partial("CodeSnippets")%>
                        </li>
                        <li class="has-sub layoutSamples"><a href="javascript:;">
                            <%="Layouts".Localize() %></a>
                            <%:Html.Partial("LayoutSamples", ViewData["LayoutSamples"])%>
                        </li>
                        <li class="has-sub last viewTools"><a href="javascript:;">
                            <%="Add views".Localize() %></a>
                            <%:Html.Partial("ViewList",Kooboo.CMS.Web.Areas.Sites.Models.ViewDataSource.GetNamespace())%>
                        </li>
                    </ul>
                </div>
            </div>
            <%:Html.Partial("CodeHelper") %>
        </div>

        <%: Html.ValidationMessage(ViewData.ModelMetadata,null) %>
    </td>
</tr>
<%:Html.Partial("Layout.Script")%>