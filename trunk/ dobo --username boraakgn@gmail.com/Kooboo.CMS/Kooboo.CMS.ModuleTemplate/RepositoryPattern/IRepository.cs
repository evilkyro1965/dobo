using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.ModuleTemplate.RepositoryPattern
{
    public interface IRepository<T>
        where T : class
    {
        T ById(int id);

        IQueryable<T> All();

        void Add(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
