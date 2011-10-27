﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence.Sqlce.QueryProcessor
{
    public interface IContentQueryTranslator<T>
        where T : ContentBase
    {
        IQueryExecutor<T> Translate(Query.IContentQuery<T> contentQuery);
    }
}
