using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Kooboo.Globalization;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class KoobooRegularExpressionAttribute : KoobooValidationAttribute
    {
        protected KoobooRegularExpressionAttribute(string message)
            : base(message)
        {
        }

        public KoobooRegularExpressionAttribute(string pattern, string message)
            : base(message)
        {
            Pattern = pattern;
        }
        
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string str = Convert.ToString(value, CultureInfo.CurrentCulture);
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }
            Match match = Regex.Match(str, Pattern);
            return ((match.Success && (match.Index == 0)) && (match.Length == str.Length));
        }

        public virtual string Pattern { get; set; }
    }
}
