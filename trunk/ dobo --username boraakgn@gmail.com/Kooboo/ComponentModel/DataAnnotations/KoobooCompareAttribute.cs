using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Kooboo.Globalization;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class KoobooCompareAttribute : KoobooValidationAttribute
    {
        public KoobooCompareAttribute(string compareTo, string message)
            : base(message)
        {
            CompareTo = compareTo;
        }

        public string CompareTo { get; set; }

        public override bool IsValid(object value)
        {
            return true;  
        }

        //protected override System.ComponentModel.DataAnnotations.ValidationResult IsValid(object value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        //{
        //    var otherProperty = validationContext.ObjectType.GetProperty(CompareTo);
        //    object otherValue = otherProperty.GetValue(validationContext.ObjectInstance, null);
        //    if (otherValue == value)
        //    {
        //        return System.ComponentModel.DataAnnotations.ValidationResult.Success;
        //    }
        //    else
        //    {
        //        return new System.ComponentModel.DataAnnotations.ValidationResult(ErrorMessage);
        //    }
        //}
    }
}
