using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form.Html.Controls
{
    public class RadioList : ControlBase
    {
        public override string Name
        {
            get { return "RadioList"; }
        }

        protected override string RenderInput(IColumn column)
        {
            StringBuilder sb = new StringBuilder(string.Format(@"@{{ var radioDefaultValue_{0} = @""{1}"";}}", column.Name, column.DefaultValue.EscapeQuote()));

            if (column.SelectionItems != null)
            {
                var index = 0;
                foreach (var item in column.SelectionItems)
                {
                    var id = column.Name + "_" + index.ToString();
                    index++;
                    sb.AppendFormat(@"
<input id=""{0}"" name=""{1}"" type=""radio"" value=""@(@""{2}"")""  @((Model.{1} != null && Model.{1}.ToString().ToLower() == @""{2}"".ToLower()) || (Model.{1} == null && radioDefaultValue_{1}.ToLower() == @""{2}"".ToLower()) ? ""checked"" : """")/><label for=""{0}"">{3}</label>"
                        , id, column.Name, item.Value.EscapeQuote(), item.Text);
                }
            }

            return sb.ToString();
        }
    }
}
