using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Account.Models;

namespace Kooboo.CMS.Account.Services
{
    public class UserManager
    {
        public void Add(User User)
        {
            Persistence.RepositoryFactory.UserRepository.Add(User);
        }
        public void Delete(string userName)
        {
            Persistence.RepositoryFactory.UserRepository.Remove(new User() { UserName = userName });
        }
        public User Get(string userName)
        {
            return Persistence.RepositoryFactory.UserRepository.Get(new User() { UserName = userName });
        }
        public IQueryable<User> All()
        {
            return Persistence.RepositoryFactory.UserRepository.All();
        }
        public void Update(string userName, User newUser)
        {
            var old = Get(userName);
            Persistence.RepositoryFactory.UserRepository.Update(newUser, old);
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return Persistence.RepositoryFactory.UserRepository.ChangePassword(userName, oldPassword, newPassword);
        }
        public bool ChangePassword(string userName, string newPassword)
        {
            return Persistence.RepositoryFactory.UserRepository.ChangePassword(userName, newPassword);
        }
        public bool ValidateUser(string userName, string password)
        {
            return Persistence.RepositoryFactory.UserRepository.ValidateUser(userName, password);
        }
    }
}
