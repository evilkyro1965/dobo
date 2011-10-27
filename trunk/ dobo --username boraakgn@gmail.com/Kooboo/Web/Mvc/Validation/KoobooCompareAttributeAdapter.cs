using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Kooboo.ComponentModel.DataAnnotations;

namespace Kooboo.Web.Mvc
{
    public class KoobooCompareAttributeAdapter : DataAnnotationsModelValidator<KoobooCompareAttribute>
    {
        public KoobooCompareAttributeAdapter(ModelMetadata metadata, ControllerContext context, KoobooCompareAttribute attribute)
            : base(metadata, context, attribute)
        {
            
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = base.ErrorMessage;
            rule.ValidationType = "compare";
            rule.ValidationParameters["compareTo"] = Attribute.CompareTo;
            return new ModelClientValidationRule[] { rule };
        }
    }
}
