﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Query.Expressions
{
    public class OrderExpression : WhereFieldExpression
    {
        public OrderExpression(IExpression expression, string fieldName, bool descending) :
            base(expression,fieldName)
        {
            this.Descending = descending;
        }
        
        public bool Descending { get; set; }
    }
}
