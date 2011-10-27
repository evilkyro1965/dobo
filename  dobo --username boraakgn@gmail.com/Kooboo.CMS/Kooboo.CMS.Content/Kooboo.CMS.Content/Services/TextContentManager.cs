﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence;
using System.Collections.Specialized;
using Kooboo.CMS.Content.Models.Binder;
using System.Web;
using Kooboo.CMS.Content.Models.Paths;
using System.IO;
using Kooboo.IO;
using Kooboo.Web.Url;
namespace Kooboo.CMS.Content.Services
{
    public class CategoryContents
    {
        public TextFolder CategoryFolder { get; internal set; }

        public IEnumerable<TextContent> Contents { get; internal set; }

        public string Display
        {
            get
            {
                var schema = new Schema(CategoryFolder.Repository, CategoryFolder.AsActual().SchemaName);
                var displayName = "UserKey";
                var summaried = schema.AsActual().GetSummarizeColumn();
                if (summaried != null)
                {
                    displayName = summaried.Name;
                }

                return CombinesValue(displayName);
            }
        }

        private string CombinesValue(string field)
        {
            if (Contents == null)
            {
                return string.Empty;
            }
            return string.Join(",", Contents.Select(it => it[field].ToString()).ToArray());
        }

        public string Value
        {
            get
            {
                return CombinesValue("UUID");
            }
        }

        public bool SingleChoice { get; set; }
    }
    public class TextContentManager
    {
        #region ByFolder
        public virtual ContentBase Add(Repository repository, TextFolder folder, NameValueCollection values, HttpFileCollectionBase files,
          IEnumerable<TextContent> categories, string userid = "")
        {
            return Add(repository, folder, null, null, values, files, categories, userid);
        }
        public ContentBase Add(Repository repository, TextFolder folder, string parentFolder, string parentUUID, NameValueCollection values
            , HttpFileCollectionBase files, IEnumerable<TextContent> categories, string userid = "")
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            var textFolder = (TextFolder)folder.AsActual();
            var schema = new Schema(repository, textFolder.SchemaName);
            var textContent = new TextContent(repository.Name, schema.Name, textFolder.FullName);

            textContent = TextContentBinder.DefaultBinder.Bind(schema, textContent, values);

            textContent.ParentFolder = parentFolder;
            textContent.ParentUUID = parentUUID;
            textContent.UserId = userid;
            textContent.UtcLastModificationDate = textContent.UtcCreationDate = DateTime.Now.ToUniversalTime();

            textContent.ContentFiles = GetPostFiles(files);

            EventBus.Content.ContentEvent.Fire(ContentAction.PreAdd, textContent);

            contentProvider.Add(textContent);

            if (categories != null)
            {
                AddCategories(repository, textContent, categories.ToArray());
            }


            EventBus.Content.ContentEvent.Fire(ContentAction.Add, textContent);

            return textContent;
        }

        private IEnumerable<ContentFile> GetPostFiles(HttpFileCollectionBase files)
        {
            if (files != null)
                for (var i = 0; i < files.Count; i++)
                {
                    yield return new ContentFile { FileName = Path.GetFileName(files[i].FileName), Stream = files[i].InputStream, Name = files.Keys[i] };
                }
        }

        public virtual ContentBase Update(Repository repository, Folder folder, string uuid, NameValueCollection values, string userid = "")
        {
            return Update(repository, folder, uuid, values, null, DateTime.UtcNow, null, null);
        }
        public virtual ContentBase Update(Repository repository, Folder folder, string uuid, NameValueCollection values, HttpFileCollectionBase files,
         DateTime modificationDate, IEnumerable<TextContent> addedCateogries, IEnumerable<TextContent> removedCategories, string userid = "")
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            var textFolder = (TextFolder)folder.AsActual();
            var schema = new Schema(repository, textFolder.SchemaName);
            var textContent = textFolder.CreateQuery().WhereEquals("UUID", uuid).First();
            var old = new TextContent(textContent);

            textContent = TextContentBinder.DefaultBinder.Update(schema, textContent, values);
            textContent.Repository = repository.Name;
            textContent.UserId = userid;
            textContent.UtcLastModificationDate = modificationDate;

            var parentFolder = values["ParentFolder"];
            if (!string.IsNullOrEmpty(parentFolder))
            {
                textContent.ParentFolder = parentFolder;
            }
            var parentUUID = values["ParentUUID"];
            if (!string.IsNullOrEmpty(parentUUID))
            {
                textContent.ParentUUID = parentUUID;
            }

            if (files != null)
            {
                textContent.ContentFiles = GetPostFiles(files);
            }
            //SaveFiles(textContent, schema, files);
            EventBus.Content.ContentEvent.Fire(ContentAction.PreUpdate, textContent);

            contentProvider.Update(textContent, old);

            if (addedCateogries != null)
            {
                AddCategories(repository, textContent, addedCateogries.ToArray());
            }
            if (removedCategories != null)
            {
                RemoveCategories(repository, textContent, removedCategories.ToArray());
            }

            EventBus.Content.ContentEvent.Fire(ContentAction.Update, textContent);
            return textContent;
        }

        public virtual void Delete(Repository repository, Folder folder, string uuid)
        {
            var textFolder = (TextFolder)folder;
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            textFolder = textFolder.AsActual();
            var textContent = textFolder.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            if (textContent != null)
            {
                EventBus.Content.ContentEvent.Fire(ContentAction.PreDelete, textContent);

                contentProvider.Delete(textContent);

                EventBus.Content.ContentEvent.Fire(ContentAction.Delete, textContent);
            }
        }
        #endregion

        #region Category
        public virtual void ClearCategories(Repository repository, TextContent textContent)
        {
            Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>().ClearCategories(textContent);
        }
        public virtual void AddCategories(Repository repository, TextContent textContent, params TextContent[] categories)
        {
            var relations = categories.Select(it => new Category()
            {
                ContentUUID = textContent.UUID,
                CategoryFolder = it.FolderName,
                CategoryUUID = it.UUID
            });
            Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>().AddCategories(textContent, relations.ToArray());
        }
        public virtual void RemoveCategories(Repository repository, TextContent textContent, params TextContent[] categories)
        {
            var relations = categories.Select(it => new Category()
            {
                ContentUUID = textContent.UUID,
                CategoryFolder = it.FolderName,
                CategoryUUID = it.UUID
            });
            Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>().DeleteCategories(textContent, relations.ToArray());
        }

        public virtual IEnumerable<CategoryContents> QueryCategories(Repository repository, string folder, string uuid)
        {
            var textFolder = (TextFolder)(FolderHelper.Parse<TextFolder>(repository, folder).AsActual());
            if (textFolder != null)
            {
                if (textFolder.Categories != null && textFolder.Categories.Count() > 0)
                {
                    foreach (var c in textFolder.Categories)
                    {
                        var categoryFolder = new TextFolder(repository, FolderHelper.SplitFullName(c.FolderName));
                        IEnumerable<TextContent> categoryContents = null;

                        if (!string.IsNullOrEmpty(uuid))
                        {
                            categoryContents = textFolder.CreateQuery().WhereEquals("UUID", uuid).Categories(categoryFolder);
                        }


                        yield return new CategoryContents() { CategoryFolder = categoryFolder, Contents = categoryContents, SingleChoice = c.SingleChoice };
                    }
                }
            }
        }
        #endregion

        #region BySchema

        public virtual TextContent Add(Repository repository, Schema schema, string parentUUID, NameValueCollection values, HttpFileCollectionBase files,
           string userid = "")
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            var textContent = new TextContent(repository.Name, schema.Name, null);
            textContent.ParentUUID = parentUUID;

            textContent = TextContentBinder.DefaultBinder.Bind(schema, textContent, values);
            textContent.UtcLastModificationDate = textContent.UtcCreationDate = DateTime.Now.ToUniversalTime();
            textContent.UserId = userid;

            if (files != null)
            {
                textContent.ContentFiles = GetPostFiles(files);
            }
            EventBus.Content.ContentEvent.Fire(ContentAction.PreAdd, textContent);

            contentProvider.Add(textContent);

            EventBus.Content.ContentEvent.Fire(ContentAction.Add, textContent);

            return textContent;
        }
        public virtual TextContent Update(Repository repository, Schema schema, string uuid, NameValueCollection values,
            HttpFileCollectionBase files, string userid = "")
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            var textContent = schema.CreateQuery().WhereEquals("UUID", uuid).First();
            var old = new TextContent(textContent);

            textContent = TextContentBinder.DefaultBinder.Update(schema, textContent, values);
            textContent.UtcLastModificationDate = DateTime.Now.ToUniversalTime();
            textContent.UserId = userid;
            if (files != null)
            {
                textContent.ContentFiles = GetPostFiles(files);
            }

            EventBus.Content.ContentEvent.Fire(ContentAction.PreUpdate, textContent);

            contentProvider.Update(textContent, old);

            EventBus.Content.ContentEvent.Fire(ContentAction.Update, textContent);

            return textContent;
        }
        public virtual void Delete(Repository repository, Schema schema, string uuid)
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            var textContent = schema.CreateQuery().WhereEquals("UUID", uuid).First();

            EventBus.Content.ContentEvent.Fire(ContentAction.PreDelete, textContent);
            contentProvider.Delete(textContent);

            EventBus.Content.ContentEvent.Fire(ContentAction.Delete, textContent);
        }
        #endregion

        #region Broadcasting

        public virtual ContentBase Copy(TextContent originalContent, TextFolder textFolder, bool keepStatus, bool keepUUID, NameValueCollection values)
        {
            textFolder = textFolder.AsActual();
            var repository = textFolder.Repository;
            var schema = new Schema(repository, textFolder.SchemaName);
            var copyedContent = new TextContent(originalContent);
            copyedContent.Id = "";
            copyedContent.UUID = "";
            if (keepUUID)
            {
                copyedContent.UUID = originalContent.UUID;
            }
            copyedContent.UtcCreationDate = DateTime.Now.ToUniversalTime();
            copyedContent.Repository = textFolder.Repository.Name;
            copyedContent.FolderName = textFolder.FullName;
            copyedContent.SchemaName = textFolder.SchemaName;
            copyedContent.OriginalUUID = originalContent.UUID;
            copyedContent.OriginalRepository = originalContent.Repository;
            copyedContent.OriginalFolder = originalContent.FolderName;
            copyedContent.IsLocalized = false;

            if (values != null)
            {
                originalContent = TextContentBinder.DefaultBinder.Bind(schema, copyedContent, values);
            }

            if (!keepStatus)
            {
                copyedContent.Published = false;
            }

            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
            contentProvider.Add(copyedContent);


            if (textFolder.Categories != null && textFolder.Categories.Count > 0)
            {
                var originalRepository = originalContent.GetRepository();
                var originalFolder = originalContent.GetFolder().AsActual();
                var originalCategories = QueryCategories(originalRepository, originalFolder.FullName, originalContent.UUID);

                List<TextContent> categories = new List<TextContent>();

                foreach (var category in originalCategories)
                {
                    foreach (var originalCategoryContent in category.Contents)
                    {
                        foreach (var categoryFolder in textFolder.Categories)
                        {
                            var categoryContent = (new TextFolder(textFolder.Repository, categoryFolder.FolderName)).CreateQuery()
                                .WhereEquals("UUID", originalCategoryContent.UUID).FirstOrDefault();
                            if (categoryContent != null)
                            {
                                categories.Add(categoryContent);
                                break;
                            }
                        }
                    }
                }

                AddCategories(repository, copyedContent, categories.ToArray());
            }

            return copyedContent;
        }

        public virtual void Localize(TextFolder textFolder, string uuid)
        {
            Update(textFolder.Repository, textFolder.GetSchema(), uuid, new string[] { "IsLocalized" }, new object[] { true });
        }
        #endregion

        #region version

        public virtual void RevertTo(Repository repository, string folder, string schemaName, string uuid, int version, string user)
        {
            var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();

            var textContent = new TextContent(repository.Name, schemaName, folder) { UUID = uuid };
            var versionInfo = Versioning.VersionManager.GetVersion(textContent, version);
            var @new = versionInfo.TextContent;
            @new.UserId = user;
            @new.UtcLastModificationDate = DateTime.Now;

            contentProvider.Update(@new, textContent);
            ClearCategories(repository, textContent);

            if (versionInfo.Categories != null)
            {
                AddCategories(repository, textContent, versionInfo.Categories
                    .Where(it => it.Contents != null)
                    .SelectMany(it => it.Contents.Select(c => new TextContent(repository.Name, "", it.FolderName) { UUID = c.UUID }))
                        .ToArray());
            }

            if (string.IsNullOrEmpty(textContent.OriginalUUID))
                EventBus.Content.ContentEvent.Fire(ContentAction.Update, @new);
        }
        #endregion

        #region Front-end
        public virtual void Update(Repository repository, Schema schema, string uuid, IEnumerable<string> fieldNames, IEnumerable<object> fieldValues, string userName = "")
        {
            var content = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            if (content != null)
            {
                var names = fieldNames.ToArray();
                var values = fieldValues.ToArray();
                for (int i = 0; i < names.Length; i++)
                {
                    content[names[i]] = values[i];
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    content.UserId = userName;
                }

                content.UtcLastModificationDate = DateTime.UtcNow;

                EventBus.Content.ContentEvent.Fire(ContentAction.PreUpdate, content);

                Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>().Update(content, content);

                EventBus.Content.ContentEvent.Fire(ContentAction.Update, content);

            }
        }

        public virtual void Update(Repository repository, Schema schema, string uuid, string fieldName, object value, string userName = "")
        {
            Update(repository, schema, uuid, new[] { fieldName }, new[] { value }, userName);
        }

        public virtual TextContent Copy(Schema schema, string uuid)
        {
            var repository = schema.Repository;
            var content = schema.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            if (content != null)
            {
                var textContent = new TextContent(content);
                textContent.UUID = "";
                textContent.UtcCreationDate = DateTime.Now.ToUniversalTime();
                //textContent.OriginalUUID = content.UUID;
                textContent.UtcLastModificationDate = DateTime.UtcNow;
                textContent.Published = null;

                EventBus.Content.ContentEvent.Fire(ContentAction.PreAdd, content);

                var contentProvider = Providers.DefaultProviderFactory.GetProvider<ITextContentProvider>();
                contentProvider.Add(textContent);

                var categories = this.QueryCategories(repository, content.FolderName, uuid);
                this.AddCategories(repository, textContent, categories.SelectMany(it => it.Contents).ToArray());

                EventBus.Content.ContentEvent.Fire(ContentAction.Add, textContent);

                return textContent;
            }
            return null;
        }
        #endregion

        #region Order

        public virtual void Top(Repository repository, string folderName, string uuid)
        {
            var folder = new TextFolder(repository, folderName).AsActual();
            var content = folder.CreateQuery().WhereEquals("UUID", uuid).FirstOrDefault();
            if (content != null)
            {

            }
        }

        #endregion
    }
}
