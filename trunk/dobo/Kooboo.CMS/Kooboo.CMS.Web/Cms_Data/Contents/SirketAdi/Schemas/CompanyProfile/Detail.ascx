<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<% if (Model != null)
{ %>
<div>
	<h3 class="title"><%=Model.Name ?? ""%></h3>
	<div class="content">
		<div><%=Model.Phone ?? ""%></div>
		<div><%=Model.Fax ?? ""%></div>
		<div><%=Model.EMail ?? ""%></div>
		<div><%=Model.Address ?? ""%></div>
		<div><%=Model.Longitude ?? ""%></div>
		<div><%=Model.Latitude ?? ""%></div>
		<div><%=Model.Image ?? ""%></div>
		<div><%=Model.Logo ?? ""%></div>
	</div>
</div>
<%} %>