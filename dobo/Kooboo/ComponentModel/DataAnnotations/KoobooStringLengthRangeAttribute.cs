using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class KoobooStringLengthRangeAttribute : KoobooValidationAttribute
    {
        public KoobooStringLengthRangeAttribute(int min, int max, string message)
            : base(message)
        {
            this.Min = min;
            this.Max = max;
        }

        public int Min
        {
            get;
            private set;
        }

        public int Max
        {
            get;
            private set;
        }

        public override bool IsValid(object value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                var len = stringValue.Length;
                return (len >= this.Min) && (len <= this.Max);
            }
            return base.IsValid(value);
        }
    }
}
