<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<% if (Model != null)
{ %>
<div>
	<h3 class="title"><%=Model.Name ?? ""%></h3>
	<div class="content">
		<div><%=Model.EMail ?? ""%></div>
		<div><%=Model.Address ?? ""%></div>
		<div><%=Model.Phone ?? ""%></div>
		<div><%=Model.Message ?? ""%></div>
	</div>
</div>
<%} %>