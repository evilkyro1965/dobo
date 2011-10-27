﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Content
{
    public class ItemDoesNotExistException : FriendlyException
    {
        public ItemDoesNotExistException()
            : base("The item does not exists.")
        {

        }
    }
}
