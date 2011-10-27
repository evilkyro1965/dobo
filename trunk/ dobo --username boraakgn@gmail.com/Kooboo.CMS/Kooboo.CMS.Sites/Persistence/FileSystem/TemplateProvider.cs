﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Kooboo.CMS.Sites.Models;
using Kooboo.IO;
using System.IO;
using Kooboo.CMS.Sites.Versioning;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public abstract class TemplateProvider<T> : InheritableProviderBase<T>, IProvider<T>
        where T : Template, IPersistable, IInheritable<T>
    {
        public abstract class TemplateVersionLogger : FileVersionLogger<T>
        {
            protected abstract TemplateProvider<T> GetTemplateProvider();
            public override void LogVersion(T o)
            {
                var locker = GetLocker();
                locker.EnterWriteLock();
                try
                {
                    VersionPath versionPath = new VersionPath(o, NextVersionId(o));
                    IOUtility.EnsureDirectoryExists(versionPath.PhysicalPath);
                    var versionDataFile = Path.Combine(versionPath.PhysicalPath, Path.GetFileName(o.DataFile));
                    var versionTemplateFile = Path.Combine(versionPath.PhysicalPath, Path.GetFileName(o.TemplateFileName));
                    File.Copy(o.DataFile, versionDataFile);
                    File.Copy(o.PhysicalTemplateFileName, versionTemplateFile);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            public override T GetVersion(T o, int version)
            {
                o = o.AsActual();
                VersionPath versionPath = new VersionPath(o, version);
                var versionDataFile = Path.Combine(versionPath.PhysicalPath, Path.GetFileName(o.DataFile));
                var versionTemplateFile = Path.Combine(versionPath.PhysicalPath, Path.GetFileName(o.TemplateFileName));
                T template = null;
                if (File.Exists(versionDataFile))
                {
                    var provider = GetTemplateProvider();
                    template = provider.Deserialize(o, versionDataFile);
                    template.Body = IOUtility.ReadAsString(versionTemplateFile);
                    template.Init(o);
                }
                return template;
            }

            protected abstract System.Threading.ReaderWriterLockSlim GetLocker();
        }
        public override T Get(T dummyObject)
        {
            if (!dummyObject.Exists())
            {
                return null;
            }
            var template = base.Get(dummyObject);
            template.Init(dummyObject);            
            return template;
        }

        protected override void Save(T item)
        {
            base.Save(item);         
        }
    }
}
