using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Kooboo.ComponentModel.DataAnnotations;

namespace Kooboo.Web.Mvc
{
    public class KoobooRangeAttributeAdapter : DataAnnotationsModelValidator<KoobooRangeAttribute>
    {
        public KoobooRangeAttributeAdapter(ModelMetadata metadata, ControllerContext context, KoobooRangeAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRangeRule(base.ErrorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
    }
}
