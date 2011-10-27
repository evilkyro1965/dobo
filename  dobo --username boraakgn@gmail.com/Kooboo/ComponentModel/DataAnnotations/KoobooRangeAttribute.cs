using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Kooboo.Globalization;

namespace Kooboo.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class KoobooRangeAttribute : KoobooValidationAttribute
    {
        private KoobooRangeAttribute(string message) 
            : base(message)
        {
        }

        public KoobooRangeAttribute(double minimum, double maximum, string message)
            : this(message)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.OperandType = typeof(double);
        }

        public KoobooRangeAttribute(int minimum, int maximum, string message)
            : this(message)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.OperandType = typeof(int);
        }

        public KoobooRangeAttribute(Type type, string minimum, string maximum, string message)
            : this(message)
        {
            this.OperandType = type;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        private Func<object, object> Conversion { get; set; }

        public object Maximum { get; set; }
 
        public object Minimum { get; set; }

        public Type OperandType { get; set; }

        public override bool IsValid(object value)
        {
            this.SetupConversion();
            if (value == null)
            {
                return true;
            }
            string str = value as string;
            if ((str != null) && string.IsNullOrEmpty(str))
            {
                return true;
            }
            object obj2 = null;
            try
            {
                obj2 = this.Conversion(value);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            IComparable minimum = (IComparable)this.Minimum;
            IComparable maximum = (IComparable)this.Maximum;
            return ((minimum.CompareTo(obj2) <= 0) && (maximum.CompareTo(obj2) >= 0));
        }

        private void Initialize(IComparable minimum, IComparable maximum, Func<object, object> conversion)
        {
            if (minimum.CompareTo(maximum) > 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.System_ComponentModel_DataAnnotations_Resources.GetString("RangeAttribute_MinGreaterThanMax"), new object[] { maximum, minimum }));
            }
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Conversion = conversion;
        }

        private void SetupConversion()
        {
            if (this.Conversion == null)
            {
                object minimum = this.Minimum;
                object maximum = this.Maximum;
                if ((minimum == null) || (maximum == null))
                {
                    throw new InvalidOperationException(SR.System_ComponentModel_DataAnnotations_Resources.GetString("RangeAttribute_Must_Set_Min_And_Max"));
                }
                Type type = minimum.GetType();
                if (type == typeof(int))
                {
                    this.Initialize((int)minimum, (int)maximum, delegate(object v)
                    {
                        return Convert.ToInt32(v, CultureInfo.InvariantCulture);
                    });
                }
                else if (type == typeof(double))
                {
                    this.Initialize((double)minimum, (double)maximum, delegate(object v)
                    {
                        return Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    });
                }
                else
                {
                    type = this.OperandType;
                    if (type == null)
                    {
                        throw new InvalidOperationException(SR.System_ComponentModel_DataAnnotations_Resources.GetString("RangeAttribute_Must_Set_Operand_Type"));
                    }
                    Type type2 = typeof(IComparable);
                    if (!type2.IsAssignableFrom(type))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.System_ComponentModel_DataAnnotations_Resources.GetString("RangeAttribute_ArbitraryTypeNotIComparable"), new object[] { type.FullName, type2.FullName }));
                    }
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    IComparable comparable = (IComparable)converter.ConvertFromString((string)minimum);
                    IComparable comparable2 = (IComparable)converter.ConvertFromString((string)maximum);
                    Func<object, object> conversion = delegate(object value)
                    {
                        if ((value != null) && (value.GetType() == type))
                        {
                            return value;
                        }
                        return converter.ConvertFrom(value);
                    };
                    this.Initialize(comparable, comparable2, conversion);
                }
            }
        }
    }
}
