using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.CMS.Sites.Models;
using System.Reflection;

namespace Kooboo.CMS.Sites.Services
{
    public class AssemblyManager
    {
        public void Upload(Site site, string fileName, Stream stream)
        {
            AssemblyFile assemblyFile = new AssemblyFile(site, fileName);
            //assemblyFile.
            assemblyFile.Save(stream);
            DeleteFromBin(fileName);
            EnsureAssembliesExistsInBin(site);
        }
        public void Delete(Site site, string fileName)
        {
            AssemblyFile assemblyFile = new AssemblyFile(site, fileName);
            if (assemblyFile.Exists())
            {
                assemblyFile.Delete();
            }
            DeleteFromBin(fileName);
        }

        private void DeleteFromBin(string fileName)
        {
            var binFile = GetAssemblyBinFilePath(fileName);
            if (File.Exists(binFile))
            {
                File.Delete(binFile);
            }
        }

        public IEnumerable<Type> GetTypes(Site site)
        {
            IEnumerable<Type> types = new Type[0];
            foreach (var assembly in GetAssemblies(site))
            {
                types = types.Concat(assembly.GetTypes());
            }
            return types;
        }

        public IEnumerable<Type> GetTypes(Site site, Type type)
        {
            return GetTypes(site).Where(it => type.IsAssignableFrom(it) && !it.IsInterface && !it.IsAbstract);
        }

        public IEnumerable<Type> GetTypes(Site site, string fileName)
        {
            var assembly = GetAssembly(site, fileName);
            if (assembly != null)
            {
                return assembly.GetTypes();
            }
            return new Type[0];
        }

        public IEnumerable<object> GetTypeInstances(Site site, Type type)
        {
            return GetTypes(site, type).Select(it => Activator.CreateInstance(it));
        }

        public Assembly GetAssembly(Site site, string fileName)
        {
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (CompareAssembly(item, fileName))
                {
                    return item;
                }
            }
            return null;
        }
        private bool CompareAssembly(Assembly assembly, string fileName)
        {
            //Path.GetFileName(assembly.Location) does not support in medium trust level.
            try
            {
                return !assembly.IsDynamic && !assembly.GlobalAssemblyCache && Path.GetFileName(assembly.EscapedCodeBase).EqualsOrNullEmpty(fileName, StringComparison.CurrentCultureIgnoreCase);
            }
            catch
            { return false; }
        }
        public IEnumerable<Assembly> GetAssemblies(Site site)
        {
            yield return typeof(AssemblyManager).Assembly;

            var assemblyFiles = GetFiles(site);
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemblyFiles.Any(it => CompareAssembly(item, it.FileName)))
                {
                    yield return item;
                }
            }
        }

        public void EnsureAssembliesExistsInBin(Site site)
        {
            var files = GetFiles(site);
            foreach (var file in files)
            {
                var fileInBin = GetAssemblyBinFilePath(file.FileName);
                if (!File.Exists(fileInBin))
                {
                    File.Copy(file.PhysicalPath, fileInBin);
                }
            }
            foreach (var file in files)
            {
                var assembly = GetAssembly(site, file.FileName);
                if (assembly == null)
                {
                    var fileInBin = GetAssemblyBinFilePath(file.FileName);
                    Assembly.LoadFrom(fileInBin);
                }
            }
        }
        private string GetAssemblyBinFilePath(string fileName)
        {
            return Path.Combine(Settings.BinDirectory, fileName);
        }

        public IEnumerable<AssemblyFile> GetFiles(Site site)
        {
            AssemblyFile dummy = new AssemblyFile(site, "dummy.dll");
            if (Directory.Exists(dummy.BasePhysicalPath))
            {
                foreach (var file in Directory.EnumerateFiles(dummy.BasePhysicalPath, "*.dll"))
                {
                    yield return new AssemblyFile(site, Path.GetFileName(file));
                }
            }
        }
    }
}
