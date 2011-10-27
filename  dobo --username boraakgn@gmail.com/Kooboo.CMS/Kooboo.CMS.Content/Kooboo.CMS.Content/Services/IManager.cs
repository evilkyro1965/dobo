using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Persistence;
using Kooboo.CMS.Content.Models;
using Kooboo.Extensions;
namespace Kooboo.CMS.Content.Services
{
    public interface IManager<T>
        where T : class
    {
        IEnumerable<T> All(Repository repository, string filterName);
        T Get(Repository repository, string name);
        void Update(Repository repository, T @new, T @old);
        void Add(Repository repository, T item);
        void Remove(Repository repository, T item);
    }

    public abstract class RepositoryElementManager<T> : IManager<T>
        where T : class,IRepositoryElement
    {
        public virtual IProvider<T> GetDBProvider()
        {
            return Providers.DefaultProviderFactory.GetProvider<IProvider<T>>();
        }

        public virtual IEnumerable<T> All(Repository repository, string filterName)
        {
            var r = GetDBProvider().All(repository);
            if (!string.IsNullOrEmpty(filterName))
            {
                r = r.Where(it => it.Name.Contains(filterName, StringComparison.CurrentCultureIgnoreCase));
            }

            return r;
        }

        public abstract T Get(Repository repository, string name);

        public virtual void Update(Repository repository, T @new, T @old)
        {
            var dbProvider = GetDBProvider();
            old.Repository = repository;
            @new.Repository = repository;
            if (dbProvider.Get(old) == null)
            {
                throw new ItemDoesNotExistException();
            }
            dbProvider.Update(@new, @old);
        }

        public virtual void Add(Repository repository, T o)
        {
            var dbProvider = GetDBProvider();
            o.Repository = repository;
            if (dbProvider.Get(o) != null)
            {
                throw new ItemAlreadyExistsException();
            }

            dbProvider.Add(o);
        }

        public virtual void Remove(Repository repository, T o)
        {
            var dbProvider = GetDBProvider();
            o.Repository = repository;
            if (dbProvider.Get(o) == null)
            {
                throw new ItemDoesNotExistException();
            }

            dbProvider.Remove(o);
        }
    }
}
