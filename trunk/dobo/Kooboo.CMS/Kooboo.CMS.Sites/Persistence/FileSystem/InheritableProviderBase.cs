﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

using System.IO;
using Kooboo.IO;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public abstract class InheritableProviderBase<T> : ObjectFileProvider<T>
        where T : PathResource, IPersistable, IInheritable<T>
    {
        public override IQueryable<T> All(Models.Site site)
        {
            return AllEnumerable(site).AsQueryable();
        }
        public virtual IEnumerable<T> AllEnumerable(Models.Site site)
        {
            return IInheritableHelper.All<T>(site);
        }


        public override void Update(T @new, T old)
        {
            if (!@new.Equals(old) && old.Exists())
            {
                old.Rename(@new.Name);
            }
            Save(@new);
        }
        public override void Remove(T item)
        {
            string dir = item.PhysicalPath;
            GetLocker().EnterWriteLock();
            try
            {

                IOUtility.DeleteDirectory(dir, true);

            }
            finally
            {
                GetLocker().ExitWriteLock();
            }
        }


        #region IImportRepository
        public void Export(IEnumerable<T> sources, System.IO.Stream outputStream)
        {
            //GetLocker().EnterReadLock();
            //try
            //{
            ImportHelper.Export(sources.OfType<PathResource>(), outputStream);
            //}
            //finally
            //{
            //    GetLocker().ExitReadLock();
            //}
        }
        public void Import(Site site, string destDir, System.IO.Stream zipStream, bool @override)
        {
            GetLocker().EnterWriteLock();
            try
            {
                ImportHelper.Import(site, destDir, zipStream, @override);
            }
            finally
            {
                GetLocker().ExitWriteLock();
            }

        }
        #endregion
    }
}
