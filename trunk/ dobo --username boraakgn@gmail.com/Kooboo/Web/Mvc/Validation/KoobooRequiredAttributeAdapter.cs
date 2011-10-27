using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Kooboo.ComponentModel.DataAnnotations;

namespace Kooboo.Web.Mvc
{
    public class KoobooRequiredAttributeAdapter : DataAnnotationsModelValidator<KoobooRequiredAttribute>
    {
        public KoobooRequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, KoobooRequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new ModelClientValidationRule[] { new ModelClientValidationRequiredRule(base.ErrorMessage) };
        }
    }
}
