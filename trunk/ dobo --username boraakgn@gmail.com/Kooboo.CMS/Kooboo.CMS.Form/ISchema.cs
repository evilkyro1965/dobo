﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Form
{
    public interface ISchema
    {
        string Name { get; set; }
        IEnumerable<IColumn> Columns { get; }
        IColumn this[string name] { get; }

        IColumn TitleColumn { get; }
    }
}
