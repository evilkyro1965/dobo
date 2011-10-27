using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models.Binder;
using Kooboo.CMS.Form;
using Kooboo.CMS.Form.Html;
using Kooboo.CMS.Form.Html.Controls;
namespace Kooboo.CMS.Content.Models
{
    public static class SchemaExtensions
    {
        public static TextContent DefaultContent(this Schema schema)
        {
            return TextContentBinder.DefaultBinder.Default(schema);
        }
        public static string GenerateForm(this Schema schema, FormType formType)
        {
            ISchema iSchema = schema.AsActual();

            return iSchema.Generate(formType.ToString());
        }
        public static IControl GetControlType(this Column column)
        {
            IControl control = ControlHelper.Resolve(column.ControlType);
            return control;
        }
    }
}
