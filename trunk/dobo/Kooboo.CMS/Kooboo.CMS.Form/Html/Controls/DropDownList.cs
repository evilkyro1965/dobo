using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html.Controls
{
    public class DropDownList : ControlBase
    {
        public override string Name
        {
            get { return "DropDownList"; }
        }

        protected override string RenderInput(IColumn column)
        {
            StringBuilder sb = new StringBuilder(string.Format(@"@{{ var dropDownDefault_{0} =  @""{1}"";}}
                <select name=""{0}"">", column.Name, column.DefaultValue.EscapeQuote()));

            if (!string.IsNullOrEmpty(column.SelectionFolder))
            {
                sb.AppendFormat(@"
                        @{{
                           var query_{0} = new TextFolder(Repository.Current, ""{1}"").CreateQuery();                         
                        }}
                        @foreach (var item in query_{0})
                        {{                            
                            <option value=""@item.UserKey"" @((Model.{0} != null && Model.{0}.ToString().ToLower() == @item.UserKey.ToLower()) || (Model.{0} == null && dropDownDefault_{0}.ToLower() == @item.UserKey.ToLower()) ? ""selected"" : """")>@item.GetSummary()</option>
                        }}
                        ", column.Name, column.SelectionFolder);
            }
            else
            {
                if (column.SelectionItems != null)
                {
                    foreach (var item in column.SelectionItems)
                    {
                        sb.AppendFormat(@"
                        <option value=""@(@""{1}"")"" @((Model.{0} != null && Model.{0}.ToString().ToLower() == @""{1}"".ToLower()) || (Model.{0} == null && dropDownDefault_{0}.ToLower() == @""{1}"".ToLower()) ? ""selected"" : """")>{2}</option>"
                            , column.Name, item.Value.EscapeQuote(), item.Text);
                    }
                }
            }

            sb.Append("</select>");

            return sb.ToString();
        }
    }
}
