﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Query.Expressions
{
    public class WhereNotInExpression : WhereFieldExpression
    {
        public WhereNotInExpression(IExpression expression, string fieldName, object[] values)
            : base(expression, fieldName)
        {
            this.Values = values;
        }
        public object[] Values { get; set; }
    }
}
