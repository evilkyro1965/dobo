﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence;

namespace Kooboo.CMS.Content.Services
{
    public class SendingSettingManager : ManagerBase<SendingSetting>
    {
        public override SendingSetting Get(Repository repository, string name)
        {
            return GetProvider(repository).Get(new SendingSetting { Repository = repository, Name = name });
        }

        private ISendingSettingProvider GetProvider(Repository repository)
        {
            return (ISendingSettingProvider)GetDBProvider();
        }
    }
}
