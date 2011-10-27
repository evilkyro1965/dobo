using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html
{
    public class SelectableForm : ISchemaForm
    {
        static string Table = @"@model Kooboo.CMS.Web.Areas.Contents.Models.SelectableViewModel
@using Kooboo.CMS.Content.Query
@using Kooboo.CMS.Content.Models
@using Kooboo.CMS.Web.Areas.Contents.Controllers
@{{
    {2}
}}
<div class=""command clearfix"">
    @Html.Partial(""Search"")
</div>
<form id=""selectedForm"" action="""">
<div class=""table-container clearfix"">
	<table class=""datasource"">
		<thead>
			<tr>
			<th class=""checkbox""></th>
			{0}
			</tr>
		</thead>
		<tbody>		
            @{{var index = 0;}}	
			@foreach (dynamic item in Model.Contents)
			{{			
			<tr class=@((index%2!=0)? ""even"" : """")>
				<td>
                    @if(Model.SingleChoice){{
                        <input type=""radio"" value='@item[""UUID""]' name=""chkContent""/> 
                    }}else{{
                       <text>@Html.CheckBox(""chkContent"",false,new {{value = item[""UUID""],@class=""checkboxList""}})</text> 
                    }}
					
				</td>
			{1}			
			</tr>
			index++;
			}}
		</tbody>
	</table>
	<div class=""pager"">
		@Html.Pager(Model.Contents)
	</div>
</div>
</form>";
        public string Generate(ISchema schema)
        {

            StringBuilder sb_head = new StringBuilder();
            StringBuilder sb_body = new StringBuilder();
            StringBuilder sb_categoryData = new StringBuilder();
            foreach (var item in schema.Columns)
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
                    sb_head.AppendFormat("\t\t<th class=\"{2} {0}\">{1}</th>\r\n", item.Name.ToLower(), string.IsNullOrEmpty(item.Label) ? item.Name : item.Label, "common");
                    if (item.Name.EqualsOrNullEmpty("Published", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb_body.AppendFormat("\t\t<td class=\"action\"><span class=\"o-icon @((item.Published!=null && item.Published == true)?\"tick\":\"cross\")\"></span></td>");
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
                        sb_body.AppendFormat("\t\t<td>{0}</td>\r\n", columnValue);
                    }

                }
            }
            return string.Format(Table, sb_head, sb_body, sb_categoryData);
        }
    }
}
