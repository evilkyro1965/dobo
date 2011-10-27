<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Web.Areas.Contents.Models.TextContentGrid>" %>
<%@Import Namespace="Kooboo.CMS.Content.Query"%>
<%@Import Namespace="Kooboo.CMS.Content.Models"%>
<%
	var schema = (Kooboo.CMS.Content.Models.Schema)ViewData["Schema"];
	var folder = (Kooboo.CMS.Content.Models.TextFolder)ViewData["Folder"];

	var folderPermission = Kooboo.CMS.Web.Authorizations.AuthorizationHelpers.Authorize(ViewContext.RequestContext, Kooboo.CMS.Account.Models.Permission.Contents_FolderPermission);
    var allowedEdit = (bool)ViewData["AllowedEdit"] ;
    var allowView = (bool)ViewData["AllowedView"];
%>

<h3 class="title">
<% if(ViewData["Folder"] !=null){%>
<%:string.IsNullOrEmpty( folder.DisplayName)?folder.Name:folder.DisplayName%>
<%}else{%>

<%}%>

</h3>

<div class="command clearfix">
    <%if(ViewData["Folder"]==null){%>
    <%: Html.ActionLink("Create ".Localize() + schema.Name, "Create", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new { @class="button dialog-link", title="Create ".Localize() + schema.Name}))%>
    <%}else{%>
        <div class="dropdown-button">
		        <span><%="Create New".Localize()%></span>
		        <div class="hide">
		        <ul>
			        <li><%: Html.ActionLink(schema.Name, "Create", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new { @class="doc dialog-link", title="Create ".Localize() + schema.Name}))%></li>
			        <% if (folderPermission)
			        {%>
			        <li><%: Html.ActionLink("Folder".Localize(), "Create","TextFolder", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new { @class="folder dialog-link", title="Create Folder".Localize()}))%></li>
			        <%}%>
		        </ul>
		        </div>
	        </div>
    <%}%>
	<% if(allowedEdit) {%> 
        <%: Html.ActionLink("Delete".Localize(), "Delete", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new { @class="button delete" ,confirmMsg="Are you sure you want to remove these items?".Localize(),alertMsg="Please select items?".Localize()}))%>
    <%} %>
<%:Html.Partial("Search")%>
</div>
<div class="table-container">
	<table>
		<thead>
			<tr>
				<th class="optional-selector checkbox">
					<div>
						<input type="checkbox"  class=" select-all"/>
						<ul class="hide">
							<li>Select:</li>
							<li class="all"><a href="javascript:;">All Elements</a></li>
							<li class="docs"><a href="javascript:;">Only Documents</a></li>
							<% if (folderPermission)
							{%>
							<li class="folders"><a href="javascript:;">Only Folders</a></li>
							<%}%>
							<li class="none"><a href="javascript:;">None</a></li>
						</ul>
					</div>
				</th>
						<th>Adı</th>
		<th>Telefon</th>
		<th>Fax</th>
		<th>E-Posta</th>
		<th>Tarih</th>
		<th>Yayınlandı</th>

                <%if(folder.EmbeddedFolders != null){%>
                <% foreach(var s in folder.EmbeddedFolders){%>
                <th><%: Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(new TextFolder(Repository.Current,s)).FriendlyText %></th>
                <%}%>
                
                <%}%>
                <% if(Repository.Current.AsActual().EnableVersioning.Value == true){%>
				<th class="action">
					<%="Versions".Localize()%>
				</th>
                <%}%>
				<th class="action">
					<%="Edit".Localize()%>
				</th>
			</tr>
		</thead>
		<tbody>
			<% 
			var index = 0;  
            if(Model.ChildFolders!=null)
			foreach (dynamic item in Model.ChildFolders)
			{
			%>
			<tr class= "<%= index%2!=0? "even" : ""%> folderTr" >
				<td>
				<% if (folderPermission)
				{%>
					<input type="checkbox" name="Selected" class="select folders" id="<%= item.FullName %>" value="<%= item.FullName %>" />
				<%}%>
				</td>
				<td>
                <% if (folderPermission)
			    {%>
                    <a class="f-icon folder" title="<%=""%>" href="<%=this.Url.Action("Index",ViewContext.RequestContext.AllRouteValues().Merge("FolderName", (object)(item.FullName)).Merge("FullName",(object)(item.FullName)))%>" >  <%:item.Name%></a>
                <%}%></td>  
				<td colspan="6"></td>
				<%if(folder.EmbeddedFolders != null){%>
                <td colspan="<%:folder.EmbeddedFolders.Count()%>"></td>                
                <%}%>
				<td class="action">
					<% if (folderPermission)
					{%>
					<a class="o-icon edit dialog-link" title ="<%="Edit Folder".Localize()%>" href="<%=this.Url.Action("Edit", "TextFolder",ViewContext.RequestContext.AllRouteValues().Merge("FolderName", (object)(item.FullName)).Merge("FullName",(object)(item.FullName)))%>" >Edit</a>
					<%}%>
				</td>
			</tr>
			<%index++;
			}
			foreach (dynamic item in Model.Contents)
			{
			%>
			<tr class= "<%= index%2!=0? "even" : ""%> docTr" >
				<td>
					<input type="checkbox" name="Selected" class="select docs" id="<%= item.UUID %>" value="<%= item.UUID %>" />
				</td>
						<td><a class="f-icon document dialog-link " title="<%="Edit ".Localize() + "CompanyProfile"%>" href="<%=this.Url.Action("Edit","TextContent",ViewContext.RequestContext.AllRouteValues().Merge("UserKey", (object)(item.UserKey)).Merge("UUID",(object)(item.UUID)))%>" ><%:item["Name"] ?? ""%></a></td>
		<td><%:item.Phone ?? ""%></td>
		<td><%:item.Fax ?? ""%></td>
		<td><%:item.EMail ?? ""%></td>
		<td class="date"><%:DateTime.Parse(item["UtcCreationDate"].ToString()).ToLocalTime().ToShortDateString()%></td>
		<td class="action"><a href="<%:Url.Action("Publish","TextContent",ViewContext.RequestContext.AllRouteValues().Merge("UserKey", (object)(item.UserKey)).Merge("UUID",(object)(item.UUID)))%>" class="boolean-ajax-link o-icon <%=(item.Published!=null && item.Published == true)?"tick":"cross"%>" confirmMsg="<%:"Are you sure you want to publish this item?"%>"  unconfirmMsg="<%:"Are you sure you want to unpublish this item?"%>"></a></td>
                
                <%
                if(folder.EmbeddedFolders !=null){%>
            
            <%foreach(var s in folder.EmbeddedFolders){%>
            <td>
                            <%:Html.ActionLink(s +" (" +((TextContent)item).Children(s).Count()+")","SubContent",ViewContext.RequestContext.AllRouteValues().Merge("ParentFolder",ViewContext.RequestContext.GetRequestValue("FolderName")).Merge("Folder",s).Merge("FolderName",s).Merge("parentUUID",(object)(item.UUID)),new RouteValueDictionary(new{ @class="dialog-link", title=s}))%>    
                </td>
                <%}%>

                <%}%>
                
                <% if(Repository.Current.AsActual().EnableVersioning.Value == true){%>
				<td class="action ">
					<a class="o-icon version dialog-link" title="<%="Versions".Localize()%>" href="<%=this.Url.Action("Versions",ViewContext.RequestContext.AllRouteValues().Merge("UserKey", (object)(item.UserKey)).Merge("UUID",(object)(item.UUID)))%>" >Version</a>
				</td>
                <%}%>
				<td class="action">
                    <% if(allowedEdit){%>
                    <a class="o-icon edit dialog-link" title="<%="Edit ".Localize() + schema.Name%>" href="<%=this.Url.Action("Edit", ViewContext.RequestContext.AllRouteValues().Merge("UserKey", (object)(item.UserKey)).Merge("UUID",(object)(item.UUID)))%>" >Edit</a>
                    <%} %>
					
				</td>
			</tr>
			<%index++;
			}
			%>
		</tbody>
	</table>
	<div class="pagination">
		<%:Html.Pager(Model.Contents) %>
	</div>
</div>
<script language="javascript" type="text/javascript">
	kooboo.cms.content.textcontent.initGrid('<%="Are you sure you want to delete these items?".Localize()%>','<%="You have not select any item!"%>');
</script>
