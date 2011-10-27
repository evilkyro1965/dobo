<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<% if (Model != null)
{ %>
<div>
	<h3 class="title"><%=Model.Title ?? ""%></h3>
	<div class="content">
		<div><%=Model.Summary ?? ""%></div>
		<div><%=Model.Description ?? ""%></div>
	</div>
</div>
<%} %>