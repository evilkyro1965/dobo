﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.Extensions;

namespace Kooboo.CMS.Sites.Services
{
    public class UserManager : ManagerBase<User>
    {
        public override IEnumerable<User> All(Site site, string filterName)
        {
            var result = Provider.All(site);
            if (!string.IsNullOrEmpty(filterName))
            {
                result = result.Where(it => it.UserName.Contains(filterName, StringComparison.CurrentCultureIgnoreCase));
            }
            return result;
        }

        public override User Get(Site site, string name)
        {
            return Provider.Get(new User() { Site = site, UserName = name });
        }

        public override void Update(Site site, User @new, User old)
        {
            @new.Site = site;
            @old.Site = site;
            Provider.Update(@new, @old);
        }

        public virtual bool Authorize(Site site, string userName, Kooboo.CMS.Account.Models.Permission permission)
        {
            string contextKey = "Permission:" + permission.ToString();
            var allow = CallContext.Current.GetObject<bool?>(contextKey);
            if (!allow.HasValue)
            {
                allow = false;

                if (IsAdministrator(userName))
                {
                    allow = true;
                }
                else if (site != null)
                {
                    var siteUser = this.Get(site, userName);

                    if (siteUser != null && siteUser.Roles != null)
                    {
                        allow = siteUser.Roles.Select(it => Kooboo.CMS.Account.Services.ServiceFactory.RoleManager.Get(it))
                                .Any(it => it.HasPermission(permission));
                    }
                }

                CallContext.Current.RegisterObject(contextKey, allow);
            }
            return allow.Value;
        }

        public virtual bool IsAdministrator(string userName)
        {
            var accountUser = Kooboo.CMS.Account.Services.ServiceFactory.UserManager.Get(userName);
            if (accountUser == null)
            {
                return false;
            }
            return accountUser.IsAdministrator;
        }

        public virtual bool AllowCreatingSite(string userName)
        {
            return IsAdministrator(userName);
        }

        public virtual bool Authorize(Site site, string userName)
        {
            if (IsAdministrator(userName))
            {
                return true;
            }
            else if (site != null)
            {
                var siteUser = this.Get(site, userName);

                return siteUser != null;
            }
            return false;
        }
    }
}
