using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence.Default;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Persistence.Sqlce.QueryProcessor;

namespace Kooboo.CMS.Content.Persistence.Sqlce
{
    public class TextContentProvider : ITextContentProvider
    {
        TextContentDbCommands dbCommands = new TextContentDbCommands();
        #region ITextContentProvider Members

        public void AddCategories(Models.TextContent content, params Models.Category[] categories)
        {
            SQLCeHelper.ExecuteNonQuery(content.GetRepository().GetConnectionString(),
                categories.Select(it => dbCommands.AddCategory(content.GetRepository(), it)).ToArray());
        }

        public void DeleteCategories(Models.TextContent content, params Models.Category[] categories)
        {
            SQLCeHelper.ExecuteNonQuery(content.GetRepository().GetConnectionString(),
                 categories.Select(it => dbCommands.DeleteCategory(content.GetRepository(), it)).ToArray());
        }
        public void ClearCategories(TextContent content)
        {
            SQLCeHelper.ExecuteNonQuery(content.GetRepository().GetConnectionString(), dbCommands.ClearCategoreis(content));
        }
        #endregion

        #region IContentProvider<TextContent> Members

        public void Add(Models.TextContent content)
        {
            content.StoreFiles();

            ((IPersistable)content).OnSaving();
            var command = dbCommands.Add(content);
            var schema = content.GetSchema();
            SQLCeHelper.ExecuteNonQuery(content.GetRepository().GetConnectionString(), command);
            ((IPersistable)content).OnSaved();
        }

        public void Update(Models.TextContent @new, Models.TextContent old)
        {
            @new.StoreFiles();

            ((IPersistable)@new).OnSaving();
            var command = dbCommands.Update(@new);
            var schema = @new.GetSchema();
            SQLCeHelper.ExecuteNonQuery(@new.GetRepository().GetConnectionString(), command);
            ((IPersistable)@new).OnSaved();
        }

        public void Delete(Models.TextContent content)
        {
            var command = dbCommands.Delete(content);
            var schema = content.GetSchema();
            SQLCeHelper.ExecuteNonQuery(content.GetRepository().GetConnectionString(), command);
            TextContentFileHelper.DeleteFiles(content);
        }

        public object Execute(Query.IContentQuery<Models.TextContent> query)
        {
            var translator = new QueryProcessor.TextContentTranslator();
            var executor = translator.Translate(query);
            return executor.Execute();
        }

        #endregion


        #region Import/Export
        public IEnumerable<IDictionary<string, object>> ExportSchemaData(Schema schema)
        {
            var connectionString = schema.Repository.GetConnectionString();
            string sql = string.Format("SELECT * FROM [{0}] ", schema.GetTableName());
            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            SqlCeConnection connection;
            using (var reader = SQLCeHelper.ExecuteReader(connectionString, new SqlCeCommand() { CommandText = sql }, out connection))
            {
                try
                {
                    while (reader.Read())
                    {
                        list.Add(reader.ToContent<TextContent>(new TextContent()));
                    }
                }
                finally
                {
                    reader.Close();
                    connection.Close();
                }
            }
            return list;
        }

        public IEnumerable<Category> ExportCategoryData(Repository repository)
        {
            var connectionString = repository.GetConnectionString();
            string sql = string.Format("SELECT UUID,CategoryFolder,CategoryUUID FROM [{0}] ", repository.GetCategoryTableName());
            List<Category> list = new List<Category>();
            SqlCeConnection connection;
            using (var reader = SQLCeHelper.ExecuteReader(connectionString, new SqlCeCommand() { CommandText = sql }, out connection))
            {
                try
                {
                    while (reader.Read())
                    {
                        Category category = new Category();
                        category.ContentUUID = reader.GetString(0);
                        category.CategoryFolder = reader.GetString(1);
                        category.CategoryUUID = reader.GetString(2);
                        list.Add(category);
                    }
                }
                finally
                {
                    reader.Close();
                    connection.Close();
                }
            }
            return list;
        }

        public void ImportSchemaData(Schema schema, IEnumerable<IDictionary<string, object>> data)
        {
            SQLCeHelper.ExecuteNonQuery(schema.Repository.GetConnectionString(),
             data.Select(it => dbCommands.Add(GetContent(schema, it))).ToArray());

        }
        private static TextContent GetContent(Schema schema, IDictionary<string, object> item)
        {
            var content = new TextContent(item);
            content.Repository = schema.Repository.Name;
            return content;
        }
        public void ImportCategoryData(Repository repository, IEnumerable<Category> data)
        {
            SQLCeHelper.ExecuteNonQuery(repository.GetConnectionString(),
               data.Select(it => dbCommands.AddCategory(repository, it)).ToArray());
        }
        #endregion

        #region ExecuteQuery

        public void ExecuteNonQuery(Repository repository, string queryText, params KeyValuePair<string, object>[] parameters)
        {
            var connectionString = repository.GetConnectionString();

            var command = new System.Data.SqlServerCe.SqlCeCommand(queryText);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters.Select(it => new System.Data.SqlServerCe.SqlCeParameter() { ParameterName = it.Key, Value = it.Value }).ToArray());
            }

            SQLCeHelper.ExecuteNonQuery(connectionString, command);
        }

        public IEnumerable<IDictionary<string, object>> ExecuteQuery(Repository repository, string queryText, params KeyValuePair<string, object>[] parameters)
        {
            var connectionString = repository.GetConnectionString();

            var command = new System.Data.SqlServerCe.SqlCeCommand(queryText);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters.Select(it => new System.Data.SqlServerCe.SqlCeParameter() { ParameterName = it.Key, Value = it.Value }).ToArray());
            }
            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            SqlCeConnection connection;
            using (var dataReader = SQLCeHelper.ExecuteReader(connectionString, command, out connection))
            {
                try
                {
                    while (dataReader.Read())
                    {
                        TextContent content = new TextContent();
                        dataReader.ToContent(content);
                        list.Add(content);
                    }
                }
                finally
                {
                    dataReader.Close();
                    connection.Close();
                }
            }
            return list;
        }

        public object ExecuteScalar(Repository repository, string queryText, params KeyValuePair<string, object>[] parameters)
        {
            var connectionString = repository.GetConnectionString();

            var command = new System.Data.SqlServerCe.SqlCeCommand(queryText);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters.Select(it => new System.Data.SqlServerCe.SqlCeParameter() { ParameterName = it.Key, Value = it.Value }).ToArray());
            }

            return SQLCeHelper.ExecuteScalar(connectionString, command);
        }
        #endregion
    }
}
