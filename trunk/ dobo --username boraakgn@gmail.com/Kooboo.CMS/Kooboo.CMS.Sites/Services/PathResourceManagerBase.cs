using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Persistence;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.Extensions;

namespace Kooboo.CMS.Sites.Services
{
    public interface IManager<T>
    {
        IProvider<T> Provider { get; }

        IEnumerable<T> All(Site site, string filterName);
        T Get(Site site, string name);
        void Update(Site site, T @new, T @old);
        void Add(Site site, T item);
        void Remove(Site site, T item);

        //void Export(Site site, string name, Stream outputStream);
        //void Import(Site site, string name, Stream zipStream, bool @override);
    }
    public abstract class PathResourceManagerBase<T> : IManager<T>
        where T : PathResource
    {
        public virtual IProvider<T> Provider
        {
            get
            {
                return Providers.ProviderFactory.GetRepository<IProvider<T>>();
            }
        }

        public virtual IEnumerable<T> All(Site site, string filterName)
        {
            var r = Provider.All(site);
            if (!string.IsNullOrEmpty(filterName))
            {
                r = r.Where(it => it.Name.Contains(filterName, StringComparison.CurrentCultureIgnoreCase));
            }

            return r;
        }

        public abstract T Get(Site site, string name);

        public virtual void Update(Site site, T @new, T @old)
        {
            if (string.IsNullOrEmpty(@new.Name))
            {
                throw new NameIsReqiredException();
            }

            old.Site = site;
            @new.Site = site;
            if (!old.Exists())
            {
                throw new ItemDoesNotExistException();
            }
            Provider.Update(@new, @old);
        }

        public virtual void Add(Site site, T o)
        {
            if (string.IsNullOrEmpty(o.Name))
            {
                throw new NameIsReqiredException();
            }

            o.Site = site;
            if (o.Exists())
            {
                throw new ItemAlreadyExistsException();
            }

            Provider.Add(o);
        }

        public virtual void Remove(Site site, T o)
        {
            //if (string.IsNullOrEmpty(o.Name))
            //{
            //    throw new NameIsReqiredException();
            //}

            o.Site = site;
            //if (!o.Exists())
            //{
            //    throw new ItemDoesNotExistException();
            //}
            if (o.Exists())
            {
                Provider.Remove(o);
            }

        }

        public virtual void ExportSelected(Site site, T[] selected, Stream outputStream)
        {
            //var nameArray = selected.Select(o => o.Name);
            //var selectedModel = All(site, "").Where(o => nameArray.Contains(o.Name));
            foreach (var item in selected)
            {
                item.Site = site;
            }
            ((IImportProvider<T>)Provider).Export(selected, outputStream);
        }

        public virtual void ExportAll(Site site, Stream outputStream)
        {
            if (Provider is IImportProvider<T>)
            {
                ((IImportProvider<T>)Provider).Export(All(site, ""), outputStream);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public virtual void Export(Site site, string name, Stream outputStream)
        {
            if (Provider is IImportProvider<T>)
            {
                ((IImportProvider<T>)Provider).Export(All(site, name).Where(it => it.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)), outputStream);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public virtual void Import(Site site, string name, Stream zipStream, bool @override)
        {
            if (Provider is IImportProvider<T>)
            {
                ((IImportProvider<T>)Provider).Import(site, GetImportPhysicalPath(site, name), zipStream, @override);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        protected virtual string GetImportPhysicalPath(Site site, string name)
        {
            var dummy = ModelActivatorFactory<T>.GetActivator().CreateDummy(site);
            return dummy.BasePhysicalPath;
        }
    }
}
