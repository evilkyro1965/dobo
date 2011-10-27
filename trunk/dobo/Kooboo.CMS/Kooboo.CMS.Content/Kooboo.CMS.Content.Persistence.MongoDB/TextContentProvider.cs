﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDBQuery = MongoDB.Driver.Builders;
using Kooboo.CMS.Content.Persistence.Default;
using MongoDB.Driver.Builders;
using Kooboo.CMS.Content.Query;

namespace Kooboo.CMS.Content.Persistence.MongoDB
{
    public class TextContentProvider : ITextContentProvider
    {
        #region Categories
        public void ClearCategories(Models.TextContent content)
        {
            //((IPersistable)content).OnSaving();
            MongoCollection<BsonDocument> collection = content.GetRepository().GetCategoriesCollection();
            var query = new QueryDocument { { "ContentUUID", content.UUID } };
            collection.Remove(query);
            //((IPersistable)content).OnSaved();
        }

        public void AddCategories(Models.TextContent content, params Models.Category[] categories)
        {
            //((IPersistable)content).OnSaving();
            MongoCollection<BsonDocument> collection = content.GetRepository().GetCategoriesCollection();
            collection.InsertBatch(categories.Select(it => it.ToBsonDocument()).ToArray());
            //((IPersistable)content).OnSaved();
        }

        public void DeleteCategories(Models.TextContent content, params Models.Category[] categories)
        {
            //((IPersistable)content).OnSaving();
            MongoCollection<BsonDocument> collection = content.GetRepository().GetCategoriesCollection();
            var query = MongoDBQuery.Query.Or(categories.Select(it => new QueryComplete(it.ToBsonDocument())).ToArray());
            collection.Remove(query);
            //((IPersistable)content).OnSaved();
        }
        #endregion

        #region Import/export
        public IEnumerable<IDictionary<string, object>> ExportSchemaData(Models.Schema schema)
        {
            var collection = schema.GetCollection();
            return collection.FindAll().Select(it => it.ToContent());
        }

        public IEnumerable<Models.Category> ExportCategoryData(Models.Repository repository)
        {
            var collection = repository.GetCategoriesCollection();
            return collection.FindAll().Select(it => it.ToCategory());
        }

        public void ImportSchemaData(Models.Schema schema, IEnumerable<IDictionary<string, object>> data)
        {
            var dataCollection = schema.GetCollection();
            dataCollection.InsertBatch(data.Select(it => (new TextContent(it) { Repository = schema.Repository.Name }).ToBsonDocument()));
        }

        public void ImportCategoryData(Models.Repository repository, IEnumerable<Models.Category> data)
        {
            var dataCollection = repository.GetCategoriesCollection();
            dataCollection.InsertBatch(data.Select(it => it.ToBsonDocument()).ToArray());
        }
        #endregion

        #region insert/update/delete

        public void Add(Models.TextContent content)
        {
            content.Remove("_id");

            content.StoreFiles();

            ((IPersistable)content).OnSaving();
            var database = content.GetRepository().GetDatabase();
            MongoCollection<BsonDocument> collection = database.GetCollection(content.GetSchema().GetSchemaCollectionName());
            collection.Insert(content.ToBsonDocument());
            ((IPersistable)content).OnSaved();
        }

        public void Update(Models.TextContent @new, Models.TextContent old)
        {
            @new.StoreFiles();
            ((IPersistable)@new).OnSaving();
            var database = old.GetRepository().GetDatabase();
            MongoCollection<BsonDocument> collection = database.GetCollection(old.GetSchema().GetSchemaCollectionName());
            var query = new QueryDocument { { "UUID", old.UUID } };
            collection.Update(query, @new.ToUpdateDocument());
            ((IPersistable)@new).OnSaved();
        }

        public void Delete(Models.TextContent content)
        {
            var database = content.GetRepository().GetDatabase();
            MongoCollection<BsonDocument> collection = database.GetCollection(content.GetSchema().GetSchemaCollectionName());
            var query = new QueryDocument { { "UUID", content.UUID } };
            collection.Remove(query);
            TextContentFileHelper.DeleteFiles(content);
        }
        #endregion

        public object Execute(IContentQuery<Models.TextContent> query)
        {
            Query.MongoDBQueryTranslator translator = new Query.MongoDBQueryTranslator();
            return translator.Translate(query).Execute();
        }


        #region Execute query
        public void ExecuteNonQuery(Repository repository, string queryText, params  KeyValuePair<string, object>[] parameters)
        {
            throw new NotSupportedException("Not supported for mongoDB");
        }

        public IEnumerable<IDictionary<string, object>> ExecuteQuery(Repository repository, string queryText, params  KeyValuePair<string, object>[] parameters)
        {
            throw new NotSupportedException("Not supported for mongoDB");
        }

        public object ExecuteScalar(Repository repository, string queryText, params  KeyValuePair<string, object>[] parameters)
        {
            throw new NotSupportedException("Not supported for mongoDB");
        }
        #endregion
    }
}
