﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kooboo.Extensions;


namespace Kooboo.CMS.Form.Html
{
    public class GridForm : ISchemaForm
    {
        string Table = @"@model Kooboo.CMS.Web.Areas.Contents.Models.TextContentGrid
@using Kooboo.CMS.Content.Query
@using Kooboo.CMS.Content.Models
@using Kooboo.CMS.Web.Areas.Contents.Controllers
@{{
	var schema = (Kooboo.CMS.Content.Models.Schema)ViewData[""Schema""];
	var folder = (Kooboo.CMS.Content.Models.TextFolder)ViewData[""Folder""];
    var routes = ViewContext.RequestContext.AllRouteValues();

	var folderPermission = Kooboo.CMS.Web.Authorizations.AuthorizationHelpers.Authorize(ViewContext.RequestContext, Kooboo.CMS.Account.Models.Permission.Contents_FolderPermission);
    var allowedEdit = (bool)ViewData[""AllowedEdit""] ;
    var allowedView = (bool)ViewData[""AllowedView""];
    {3}
}}

<h3 class=""title"">
@if(ViewData[""Folder""] !=null){{
    @folder.FriendlyText
}}
</h3>

<div class=""command clearfix"">
    @if(ViewData[""Folder""]==null){{
        @Html.ActionLink(""Create "".Localize() + folder.FriendlyText, ""Create"", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new {{ @class=""button dialog-link"", title=""Create "".Localize() + schema.Name}}))
    }}else{{
        <div class=""dropdown-button"">
		        <span>@(""Create New"".Localize())</span>
		        <div class=""hide"">
		        <ul>
			        <li>@Html.ActionLink(folder.FriendlyText, ""Create"", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new {{ @class=""doc dialog-link"", title=""Create "".Localize() + schema.Name}}))</li>
			        @if (folderPermission)
			        {{
			            <li>@Html.ActionLink(""Folder"".Localize(), ""Create"",""TextFolder"", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new {{ @class=""folder dialog-link"", title=""Create Folder"".Localize()}}))</li>
			        }}
		        </ul>
		        </div>
	        </div>
            
    }}
    
    
   @Html.ActionLink(""Delete"".Localize(), ""Delete"", ViewContext.RequestContext.AllRouteValues(),new RouteValueDictionary(new {{ @class=""button delete"" ,confirmMsg=""Are you sure you want to remove these items?"".Localize(),alertMsg=""Please select items?"".Localize()}}))

@if(!string.IsNullOrEmpty(routes[""FolderName""] as string) && folderPermission){{
        <text>@Html.ActionLink(""Setting"".Localize(), ""Edit"", ""TextFolder"",ViewContext.RequestContext.AllRouteValues().Merge(""FullName"",routes[""FolderName""]).Merge(""FolderName"",null).Merge(""Folder"",null),new RouteValueDictionary(new {{ @class=""button dialog-link"" ,title=""Folder setting"".Localize()}}))</text>
    }}
   @Html.Partial(""Search"")
</div>
<div class=""table-container"">
	<table>
		<thead>
			<tr>
				<th class=""optional-selector checkbox draggable"">
					<div>
						<input type=""checkbox""  class="" select-all""/>
						<ul class=""hide"">
							<li>Select:</li>
							<li class=""all""><a href=""javascript:;"">All Elements</a></li>
							<li class=""docs""><a href=""javascript:;"">Only Documents</a></li>
							@if (folderPermission)
							{{
							<li class=""folders""><a href=""javascript:;"">Only Folders</a></li>
							}}
							<li class=""none""><a href=""javascript:;"">None</a></li>
						</ul>
					</div>
				</th>
				{0}
                
                @if(folder.EmbeddedFolders != null){{
                    foreach(var s in folder.EmbeddedFolders){{
                    <th class=""action"">@Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(new TextFolder(Repository.Current,s)).FriendlyText</th>
                    }}                
                }}

                @if(Repository.Current.EnableWorkflow && folder.EnabledWorkflow)
                {{
                    <th class=""action""> @(""Workflow"".Localize()) </th>
                }}
                @if(Repository.Current.AsActual().EnableVersioning.Value == true)
                {{
				    <th class=""action"">
					    @(""Versions"".Localize())
				    </th>
                }}
				<th class=""action"">
					@(""Edit"".Localize())
				</th>
			</tr>
		</thead>
		<tbody>
            @{{var index = 0; }}
            @if(Model.ChildFolders!=null)
            {{                
			    foreach (dynamic item in Model.ChildFolders)
			    {{			
			<tr class= ""@(index%2!=0? ""even"" : """") folderTr"" >
				<td class=""undraggable"">
				@if (folderPermission)
				{{
					<input type=""checkbox"" name=""Selected"" class=""select folders"" id=""@item.FullName"" value=""@item.FullName"" />
				}}
				</td>
				<td>
                @if(!string.IsNullOrEmpty(item.SchemaName)){{
                    <a class=""f-icon folder"" href=""@this.Url.Action(""Index"",ViewContext.RequestContext.AllRouteValues().Merge(""FolderName"", (object)(item.FullName)).Merge(""FullName"",(object)(item.FullName)))"" >
                        @Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(item).FriendlyText</a>
                }}
                else {{
                    <a class=""f-icon folder"" href=""@this.Url.Action(""Index"",ViewContext.RequestContext.AllRouteValues().Merge(""controller"",""TextFolder"").Merge(""FolderName"", (object)(item.FullName)).Merge(""FullName"",(object)(item.FullName)))"" >
                        @Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(item).FriendlyText</a>
                }}
                </td>  
				<td colspan=""{2}""></td>
                @if(Repository.Current.EnableWorkflow && folder.EnabledWorkflow){{
                <td colspan=""1""></td>
                }}
                @if(Repository.Current.AsActual().EnableVersioning.Value == true){{
                <td colspan=""1""></td>
                }}
				@if(folder.EmbeddedFolders != null){{
                <td colspan=""@folder.EmbeddedFolders.Count()""></td>
                }}
				<td class=""action"">
					@if (folderPermission)
					{{
					<a class=""o-icon edit dialog-link"" title =""@(""Edit folder"".Localize())"" href=""@this.Url.Action(""Edit"", ""TextFolder"",ViewContext.RequestContext.AllRouteValues().Merge(""FolderName"", (object)(item.FullName)).Merge(""FullName"",(object)(item.FullName)))"" >Edit</a>
					}}
				</td>
			</tr>
			        index++;
                }}
            
            }}	
            </tbody>
            <tbody>		
			@foreach (dynamic item in Model.Contents)
			{{
                var workflowItem  = item._WorkflowItem_;
                var hasWorkflowItem = workflowItem!=null;
                var availableEdit = hasWorkflowItem || (!hasWorkflowItem && allowedEdit);
                var editTitle= ""Edit "".Localize() + schema.Name + ((item.IsLocalized!=null &&item.IsLocalized==false) ?"" From:"".Localize() + item.OriginalRepository.ToString() + ""."" + item.OriginalFolder.ToString():"""" );
		    <tr class= ""@(index%2!=0? ""even"" : """") docTr @((item.IsLocalized!=null &&item.IsLocalized==false)?""unlocalized"" :"""") @(hasWorkflowItem?""hasWorkflowItem"":"""")"" >
			    <td class=""draggable"">
                @if(availableEdit)
                {{
				    <input type=""checkbox"" name=""Selected"" class=""select docs"" id=""@item.UUID"" value=""@item.UUID"" />
                }}
			    </td>
			    {1}
                
                @if(folder.EmbeddedFolders !=null){{            
                    foreach(var s in folder.EmbeddedFolders){{
                        var embeddedFolder =Kooboo.CMS.Content.Models.IPersistableExtensions.AsActual(new TextFolder(Repository.Current,s));
                <td class=""action"">
                            @Html.ActionLink(embeddedFolder.FriendlyText + "" ("" + ((TextContent)item).Children(s).Count() + "")"", ""SubContent"", ""TextContent"", new {{ SiteName = ViewContext.RequestContext.GetRequestValue(""SiteName""), RepositoryName = ViewContext.RequestContext.GetRequestValue(""RepositoryName""), ParentFolder = ViewContext.RequestContext.GetRequestValue(""FolderName""), Folder = s, FolderName = s, parentUUID = (object)(item.UUID) }}, new {{ @class = ""dialog-link"", title = embeddedFolder.FriendlyText }})
                </td>
                    }}
                }}
                @if(Repository.Current.EnableWorkflow && folder.EnabledWorkflow){{
                    <td class=""action"">
                    @if(hasWorkflowItem)
                    {{
                        <a href=""@Url.Action(""Process"",""PendingWorkflow"",ViewContext.RequestContext.AllRouteValues().Merge(""UserKey"", (object)(item.UserKey)).Merge(""UUID"",(object)(item.UUID)).Merge(""RoleName"", (object)(workflowItem.RoleName)).Merge(""Name"", (object)(workflowItem.Name)))"" title=""@(""Process workflow"".Localize())"" class=""o-icon process dialog-link"">@(""Process workflow"".Localize())</a>
                    }}
                    else{{
                        <a href=""@Url.Action(""WorkflowHistory"",""PendingWorkflow"",ViewContext.RequestContext.AllRouteValues().Merge(""UserKey"", (object)(item.UserKey)).Merge(""UUID"",(object)(item.UUID)))"" title=""@(""View workflow history"".Localize())"" class=""o-icon workflow dialog-link"">@(""View workflow history"".Localize())</a>                      
                    }}
                    </td>
                }}
                @if(Repository.Current.AsActual().EnableVersioning.Value == true){{
			    <td class=""action "">
				    <a class=""o-icon version dialog-link"" title=""@(""Versions"".Localize())"" href=""@this.Url.Action(""Versions"",ViewContext.RequestContext.AllRouteValues().Merge(""UserKey"", (object)(item.UserKey)).Merge(""UUID"",(object)(item.UUID)))"" >@(""Version"").Localize())</a>
			    </td>
                }}
			    <td class=""action"">                
                    <input type=""hidden"" name=""Sequence"" value=""@item.Sequence""/>
                    <a class=""o-icon edit dialog-link"" title=""@editTitle"" href=""@this.Url.Action(""Edit"", ViewContext.RequestContext.AllRouteValues().Merge(""UserKey"", (object)(item.UserKey)).Merge(""UUID"",(object)(item.UUID)))"" >@(""Edit"".Localize())</a>				                  
			    </td>
		    </tr>
			    index++;
       		}}
		</tbody>
	</table>
	<div class=""pagination"">
		@Html.Pager(Model.Contents)
	</div>
</div>
<script language=""javascript"" type=""text/javascript"">
	kooboo.cms.content.textcontent.initGrid('@(""Are you sure you want to delete these items?"".Localize())','@(""You have not select any item!"".Localize())');
</script>
";
        #region ISchemaTemplate Members

        public string Generate(ISchema schema)
        {
            StringBuilder sb_head = new StringBuilder();

            StringBuilder sb_body = new StringBuilder();

            StringBuilder sb_categoryData = new StringBuilder();


            int colspan = 0;
            foreach (var item in schema.Columns.OrderBy(it => it.Order))
            {
                if (item.ShowInGrid)
                {
                    string columnValue = string.Format("@(item.{0} ?? \"\")", item.Name);
                    if (!string.IsNullOrEmpty(item.SelectionFolder))
                    {
                        sb_categoryData.AppendFormat(@"
                          var {0}_data = (new TextFolder(Repository.Current,""{1}"")).CreateQuery().ToArray();
                         ", item.Name, item.SelectionFolder);

                        columnValue = string.Format(@"@{{
                        string {0}_rawValue = (item.{0} ?? """").ToString();
                        string[] {0}_value = {0}_rawValue.Split(new[] {{ ',' }}, StringSplitOptions.RemoveEmptyEntries);

                        var {0}_categories = {0}_data.Where(it =>
                            {0}_value.Any(s =>
                                s.EqualsOrNullEmpty(it.UserKey, StringComparison.OrdinalIgnoreCase))).ToArray();}}
                        @if ({0}_categories.Length > 0)
                        {{
                            @string.Join("","", {0}_categories.Select(it => it.GetSummary()))
                        }}
                        else
                        {{
                            {1}
                        }}", item.Name, columnValue);
                    }
                    sb_head.AppendFormat("\t\t<th class=\"{2} {0}\">{1}</th>\r\n", item.Name.ToLower(), string.IsNullOrEmpty(item.Label) ? item.Name : item.Label, colspan > 0 ? "common" : "");
                    if (item.Name.EqualsOrNullEmpty("Published", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb_body.AppendFormat("\t\t<td class=\"action\">@if(allowedEdit){{<a href=\"@Url.Action(\"Publish\",\"TextContent\",ViewContext.RequestContext.AllRouteValues().Merge(\"UserKey\", (object)(item.UserKey)).Merge(\"UUID\",(object)(item.UUID)))\" class=\"boolean-ajax-link o-icon @((item.Published!=null && item.Published == true)?\"tick\":\"cross\")\" confirmMsg=\"@(\"Are you sure you want to publish this item?\".Localize())\"  unconfirmMsg=\"@(\"Are you sure you want to unpublish this item?\".Localize())\"></a>}} else {{<span class='o-icon @((item.Published!=null && item.Published == true)?\"tick\":\"cross\")'></span>}}</td>");
                    }
                    else if (item.Name.EqualsOrNullEmpty("UtcCreationDate", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb_body.AppendFormat("\t\t<td class=\"date\">@(DateTime.Parse(item[\"{0}\"].ToString()).ToLocalTime().ToShortDateString())</td>\r\n", item.Name);
                    }
                    else if (item.DataType == Data.DataType.DateTime)
                    {
                        sb_body.AppendFormat("\t\t<td class=\"date\">@(item[\"{0}\"] == null?\"\":DateTime.Parse(item[\"{0}\"].ToString()).ToShortDateString())</td>\r\n", item.Name);
                    }
                    else if (ControlHelper.IsImage(item.ControlType))
                    {
                        sb_body.AppendFormat("\t\t<td><img src='@Url.Content(item.{0} ?? \"\")' alt='' height='80' width='120'/></td>\r\n", item.Name);
                    }
                    else
                    {
                        if (colspan == 0)
                        {
                            sb_body.AppendFormat("\t\t<td><a class=\"f-icon document dialog-link \" title=\"@editTitle\" href=\"@this.Url.Action(\"Edit\",\"TextContent\",ViewContext.RequestContext.AllRouteValues().Merge(\"UserKey\", (object)(item.UserKey)).Merge(\"UUID\",(object)(item.UUID)))\" >{1}</a></td>\r\n", schema.Name, columnValue);
                        }
                        else
                        {
                            sb_body.AppendFormat("\t\t<td>{0}</td>\r\n", columnValue);
                        }

                    }

                    colspan++;
                }
            }

            return string.Format(Table, sb_head, sb_body, colspan - 1, sb_categoryData);
        }

        #endregion
    }
}