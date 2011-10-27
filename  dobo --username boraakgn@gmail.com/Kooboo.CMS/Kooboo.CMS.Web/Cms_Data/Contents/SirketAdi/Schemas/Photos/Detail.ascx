<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<% if (Model != null)
{ %>
<div>
	<h3 class="title"><%=Model.Name ?? ""%></h3>
	<div class="content">
		<div><%=Model.Description ?? ""%></div>
		<div><%=Model.Path ?? ""%></div>
	</div>
</div>
<%} %>