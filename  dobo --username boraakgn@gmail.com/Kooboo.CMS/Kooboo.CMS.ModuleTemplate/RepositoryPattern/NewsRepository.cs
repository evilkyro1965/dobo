﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.CMS.ModuleTemplate.Models;

namespace Kooboo.CMS.ModuleTemplate.RepositoryPattern
{
    public class NewsRepository : IRepository<News>
    {
        static List<News> newsList = new List<News>();
        public News ById(int id)
        {
            return newsList.Where(it => it.Id == id).FirstOrDefault();
        }

        public IQueryable<News> All()
        {
            return newsList.AsQueryable();
        }

        public void Add(News entity)
        {
            var id = newsList.Count + 1;
            entity.Id = id;
            newsList.Add(entity);
        }

        public void Update(News entity)
        {
            var old = ById(entity.Id);
            if (old != null)
            {
                var index = newsList.IndexOf(old);
                newsList.RemoveAt(index);
                newsList.Insert(index, entity);
            }
        }

        public void Delete(News entity)
        {
            var old = ById(entity.Id);
            if (old != null)
            {
                newsList.Remove(old);
            }
        }
    }
}