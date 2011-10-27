﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.IoC
{
    public interface IActivatorContext
    {
        IEnumerable<CreateHandler> GetHandlers();
    }
}
