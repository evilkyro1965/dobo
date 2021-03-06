﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Web.Areas.Contents.Models.SelectableViewModel>" %>
<form id="selectedForm" action="">
<div class="table-container clearfix">
	<table class="datasource">
		<thead>
			<tr>
			<th class="checkbox"></th>
					<th>Title</th>
		<th>Date</th>
		<th>Published</th>

			</tr>
		</thead>
		<tbody>
			<% 
			var index = 0;                
			foreach (dynamic item in Model.Contents)
				{
			%>
				<tr <% if(index%2!=0) {%>class="even" <%} %>>
				<td>
					<%:Html.CheckBox("chkContent",false,new {value = item["UUID"],@class="checkboxList"}) %>
				</td>
					<td><%:item.Title ?? ""%></td>
		<td class="date"><%:DateTime.Parse(item["UtcCreationDate"].ToString()).ToShortDateString()%></td>
		<td class="action"><span class="o-icon <%=item.Published?"tick":"cross"%>"></span></td>			
			</tr>
			<%index++;
				}
			%>
		</tbody>
	</table>
	<div class="pager">
		<%:Html.Pager(Model.Contents) %>
	</div>
</div>
</form>