using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Kooboo.ComponentModel.DataAnnotations;

namespace Kooboo.Web.Mvc
{
    public class KoobooRegularExpressionAttributeAdapter : DataAnnotationsModelValidator<KoobooRegularExpressionAttribute>
    {
        public KoobooRegularExpressionAttributeAdapter(ModelMetadata metadata, ControllerContext context, KoobooRegularExpressionAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRegexRule(base.ErrorMessage, Attribute.Pattern) };
        }
    }
}
