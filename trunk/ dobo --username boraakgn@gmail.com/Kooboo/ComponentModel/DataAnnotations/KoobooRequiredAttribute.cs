using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Kooboo.Globalization;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class KoobooRequiredAttribute : KoobooValidationAttribute
    {
        public KoobooRequiredAttribute(string message)
            : base(message)
        {
        }

        public bool AllowEmptyStrings { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }
            string str = value as string;
            if ((str != null) && !this.AllowEmptyStrings)
            {
                return (str.Trim().Length != 0);
            }
            return true;
        }
    }
}
