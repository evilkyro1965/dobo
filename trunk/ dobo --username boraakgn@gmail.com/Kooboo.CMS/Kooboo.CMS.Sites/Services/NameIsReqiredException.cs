﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Services
{
    public class NameIsReqiredException : FriendlyException
    {
        public NameIsReqiredException() : base("Name is requried") { }
    }
}
