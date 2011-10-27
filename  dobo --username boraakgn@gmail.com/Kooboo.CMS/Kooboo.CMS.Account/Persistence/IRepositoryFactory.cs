using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Account.Persistence
{
    public interface IRepositoryFactory
    {
        T GetRepository<T>()
            where T : class;
    }
    public static class RepositoryFactory
    {
        public static IRepositoryFactory Factory { get; set; }
        static RepositoryFactory()
        {
            Factory = new FileSystem.RepositoryFactory();
        }
        public static IRoleRepository RoleRepository
        {
            get
            {
                return Factory.GetRepository<IRoleRepository>();
            }
        }
        public static IUserRepository UserRepository
        {
            get
            {
                return Factory.GetRepository<IUserRepository>();
            }
        }
    }
}
