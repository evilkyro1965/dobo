﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content.Query.Expressions
{
    public class WhereClauseExpression : Expression, IWhereExpression
    {
        public WhereClauseExpression(IExpression expression, string whereClause)
            : base(expression)
        {
            this.WhereClause = whereClause;
        }
        public string WhereClause { get; private set; }
    }
}
