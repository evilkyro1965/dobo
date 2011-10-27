﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public class UserProvider : ObjectFileProvider<User>, IUserProvider
    {
        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();

        protected override System.Threading.ReaderWriterLockSlim GetLocker()
        {
            return locker;
        }

        private IEnumerable<User> AllEnumerable(Site site)
        {
            var basePath = User.DataFilePath.GetBasePath(site);
            if (Directory.Exists(basePath))
            {
                foreach (var filePath in Directory.EnumerateFiles(basePath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    yield return new User() { Site = site, UserName = fileName };
                }
            }
        }
        public override IQueryable<User> All(Site site)
        {
            GetLocker().EnterReadLock();
            try
            {
                return AllEnumerable(site).AsQueryable();
            }
            finally
            {
                GetLocker().ExitReadLock();
            }

        }
    }
}
