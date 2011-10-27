﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public class UrlKeyMapProvider : ListFileRepository<UrlKeyMap>, IUrlKeyMapProvider
    {
        static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();

        #region IImportRepository Members

        public void Export(Site site, System.IO.Stream outputStream)
        {
            locker.EnterReadLock();
            try
            {
                ImportHelper.Export(new[] { new UrlKeyMapsFile(site) }, outputStream);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Import(Site site, System.IO.Stream zipStream, bool @override)
        {
            locker.EnterWriteLock();
            try
            {
                ImportHelper.Import(site, new UrlKeyMapsFile(site).BasePhysicalPath, zipStream, @override);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        #endregion

        protected override string GetFile(Site site)
        {
            return new UrlKeyMapsFile(site).PhysicalPath;
        }

        protected override System.Threading.ReaderWriterLockSlim GetLocker()
        {
            return locker;
        }
    }
}
