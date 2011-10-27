using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html.Controls
{
    public class CheckBoxList : ControlBase
    {
        public override string Name
        {
            get { return "CheckBoxList"; }
        }

        protected override string RenderInput(IColumn column)
        {
            StringBuilder sb = new StringBuilder(string.Format(@"@{{ var checkBoxListDefaultValue_{0} = @""{1}"".Split(new []{{','}},StringSplitOptions.RemoveEmptyEntries);
                        var values_{0} = new string[0];
                        if(!string.IsNullOrEmpty(Model.{0}))
                        {{
                            values_{0}=Model.{0}.Split(new []{{','}},StringSplitOptions.RemoveEmptyEntries);
                        }}
                        }}", column.Name, column.DefaultValue.EscapeQuote()));

            if (!string.IsNullOrEmpty(column.SelectionFolder))
            {
                sb.AppendFormat(@"
                        @{{
                           var query_{0} = new TextFolder(Repository.Current, ""{1}"").CreateQuery();
                           var index_{0} = 0;
                        }}
                        @foreach (var item in query_{0})
                        {{
                            var id = ""{1}"" + index_{0}.ToString();
                             <input id=""@id"" name=""{0}"" type=""checkbox"" value=""@item.UserKey""  @((Model.{0} == null && checkBoxListDefaultValue_{0}.Contains(@item.UserKey, StringComparer.OrdinalIgnoreCase)) || (Model.{0} != null && values_{0}.Contains(@item.UserKey, StringComparer.OrdinalIgnoreCase)) ? ""checked"" : """")/><label
                            for=""@id"">@item.GetSummary()</label>
                            index_{0}++;
                        }}
                        ", column.Name, column.SelectionFolder);
            }
            else
            {
                if (column.SelectionItems != null)
                {
                    var index = 0;
                    foreach (var item in column.SelectionItems)
                    {
                        var id = column.Name + "_" + index.ToString();
                        index++;
                        sb.AppendFormat(@"
<input id=""{0}"" name=""{1}"" type=""checkbox"" value=""@(@""{2}"")""  @((Model.{1} == null && checkBoxListDefaultValue_{1}.Contains(@""{2}"",StringComparer.OrdinalIgnoreCase))  || (Model.{1} != null && values_{1}.Contains(@""{2}"",StringComparer.OrdinalIgnoreCase)) ? ""checked"" : """")/><label for=""{0}"">{3}</label>"
                            , id, column.Name, item.Value.EscapeQuote(), item.Text);
                    }
                }
            }


            return sb.ToString();
        }
    }
}
