<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Kooboo.CMS.Web.Areas.Account.Models.LoginModel>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%:"Giriş yap".Localize()%></title>
    <%: Html.ExternalResources("skinStyles") %>
    <%: Html.ExternalResources("siteStyles")%>
    <%: Html.ExternalResources("commonStyles")%>
    <%: Html.ExternalResources("logonScripts")%>
    <script language="javascript" type="text/javascript">
        $(function () {
            $('input:visible').first().focus();
        });
    </script>
</head>
<body>
    <div id="body-container">
        <div id="header" class="clearfix">
            <h1 class="logo">
                <a href="<%: Url.Content("~/Account/Logon") %>">
                    <img src="<%: Url.Content("~/images/logo.png") %>" alt="logo" /></a></h1>
            <span class="slogan"><em></em></span>
        </div>
        <div id="main-container" class="clearfix">
            <div id="main">
                <div class="inner-main">
                    <h3 class="title"><%:"İçerik yönetim sistemi hakkında".Localize() %></h3>
                    <div class="content">
                    </div>
                </div>
            </div>
            <div id="sidebar">
                <div class="block login">
                    <div class="common-form">
                        <h6 class="title">
                            <%:"Sign in".Localize() %></h6>
                        <% Html.EnableClientValidation(); %>
                        <%:Html.ValidationSummary(true) %>
                        <% using (Html.BeginForm())
                           {  %>
                        <table>
                            <tbody>
                                <%:Html.EditorFor(m => m.UserName) %>
                                <%:Html.EditorFor(m => m.Password) %>
                                <%:Html.EditorFor(m => m.RememberMe) %>
                                <tr>
                                    <td>
                                    </td>
                                    <td>
                                        <button type="submit">
                                            <%:"Sign in".Localize() %></button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <%} %>
                    </div>
                </div>
            </div>
        </div>
        <div id="footer">
            <p>
              <%:"İçerik yönetim sistemi".Localize()%> | &copy; 2011</p>
        </div>
    </div>
</body>
</html>
