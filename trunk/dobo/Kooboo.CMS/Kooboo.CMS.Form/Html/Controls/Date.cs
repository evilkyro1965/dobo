﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Kooboo.CMS.Form.Html.Controls
{
    public class Date : Input
    {
        public override string Name
        {
            get { return "Date"; }
        }
        public override string Type
        {
            get { return "text"; }
        }

        protected override string RenderInput(IColumn column)
        {
            string input = string.Format(@"<input id=""{0}"" name=""{0}"" type=""{1}"" value=""@(Model.{0} ==null ? """" : Model.{0}.ToShortDateString())"" {2}/>", column.Name, Type, Kooboo.CMS.Form.Html.ValidationExtensions.GetUnobtrusiveValidationAttributeString(column));
            return input + string.Format(@"<script language=""javascript"" type=""text/javascript"">$(function(){{$(""#{0}"").datepicker();}});</script>", column.Name);
        }
    }
}
