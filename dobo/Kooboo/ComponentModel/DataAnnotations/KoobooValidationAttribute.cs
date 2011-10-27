using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kooboo.Globalization;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class KoobooValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        public KoobooValidationAttribute(string message)
            : base(() => message.Localize())
        {
        }
    }
}
