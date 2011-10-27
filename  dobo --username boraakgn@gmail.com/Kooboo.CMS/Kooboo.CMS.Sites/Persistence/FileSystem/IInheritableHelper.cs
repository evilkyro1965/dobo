﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

using System.IO;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public static class IInheritableHelper
    {
        public static IEnumerable<T> All<T>(Site site)
            where T : PathResource, IInheritable<T>
        {
            List<T> results = new List<T>();

            while (site != null)
            {
                var tempResults = AllInternal<T>(site);
                if (results.Count == 0)
                {
                    results.AddRange(tempResults);
                }
                else
                {
                    foreach (var item in tempResults)
                    {
                        if (!results.Any(it => it.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            results.Add(item);
                        }
                    }
                }
                site = site.Parent;
            }
            return results;
        }

        private static IEnumerable<T> AllInternal<T>(Site site)
             where T : PathResource, IInheritable<T>
        {
            string baseDir = ModelActivatorFactory<T>.GetActivator().CreateDummy(site).BasePhysicalPath;
            if (Directory.Exists(baseDir))
            {
                foreach (var dir in IO.IOUtility.EnumerateDirectoriesExludeHidden(baseDir).Where(it => !it.Name.EqualsOrNullEmpty(Versioning.VersionBasePath.VersionFolderName, StringComparison.OrdinalIgnoreCase)))
                {
                    var o = ModelActivatorFactory<T>.GetActivator().Create(dir.FullName);
                    if (o.Exists())
                    {
                        yield return o;
                    }
                }
            }
        }
    }
}
