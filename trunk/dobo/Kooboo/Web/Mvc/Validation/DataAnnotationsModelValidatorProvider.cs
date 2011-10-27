using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kooboo.ComponentModel.DataAnnotations;

namespace Kooboo.Web.Mvc
{
    public class DataAnnotationsModelValidatorProvider
    {
        public static void RegisterKoobooValidators()
        {
            System.Web.Mvc.DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(KoobooRequiredAttribute), typeof(KoobooRequiredAttributeAdapter));

            System.Web.Mvc.DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(KoobooRegularExpressionAttribute), typeof(KoobooRegularExpressionAttributeAdapter));

            System.Web.Mvc.DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(KoobooCompareAttribute), typeof(KoobooCompareAttributeAdapter));
        }
    }
}
