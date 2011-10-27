using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Kooboo.CMS.Account.Persistence.FileSystem
{
    public class RepositoryFactory : IRepositoryFactory
    {
        static Hashtable providers = new Hashtable();
        static RepositoryFactory()
        {
            providers.Add(typeof(RoleRepository), new RoleRepository());
            providers.Add(typeof(UserRepository), new UserRepository());
        }
        public T GetRepository<T>() where T : class
        {
            foreach (var item in providers.Values)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }
            return null;
        }
    }
}
