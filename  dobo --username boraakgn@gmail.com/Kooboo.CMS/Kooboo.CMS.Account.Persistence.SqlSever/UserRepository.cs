﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Account.Persistence;
using Kooboo.CMS.Account.Models;

namespace Kooboo.CMS.Account.Persistence.SqlSever
{
    /// <summary>
    /// 
    /// </summary>
    public class UserRepository : SqlServerRepositoryBase<User>, IUserRepository
    {
        public UserRepository()
            : base()
        {

        }
        public UserRepository(string connectionString)
            : base(connectionString)
        {
        }
        public override void Add(User item)
        {
            var r = Kooboo.Connect.UserServices.CreateUser(item.UserName, item.Password, item.Email);

            if (r == Connect.UserCreateStatus.Success)
            {
                item.Password = "******";
                base.Add(item);
            }
            else
            {
                throw new KoobooException("Create user failed, message:" + r.ToString());
            }
        }
        public override IQueryable<User> All()
        {
            return base.All().OrderBy(it => it.Email);
        }
        public bool ValidateUser(string userName, string password)
        {
            return Kooboo.Connect.UserServices.ValidateUser(userName, password) != null;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return Kooboo.Connect.UserServices.ChangePassword(userName, oldPassword, newPassword);
        }
        public bool ChangePassword(string userName, string newPassword)
        {
            return Kooboo.Connect.UserServices.ChangePassword(userName, newPassword);
        }


        public override void Update(Models.User @new, Models.User old)
        {
            var dbContext = GetDBContext();
            var update = Get(dbContext, old);
            update.Password = @new.Password;
            update.IsAdministrator = @new.IsAdministrator;
            update.CustomFields = @new.CustomFields;
            update.Email = @new.Email;

            dbContext.SaveChanges();
        }

        public override User Get(AccountDBContext dbContext, User dummy)
        {
            return dbContext.Users.Where(o => o.UserName == dummy.UserName).FirstOrDefault();
        }
    }
}
