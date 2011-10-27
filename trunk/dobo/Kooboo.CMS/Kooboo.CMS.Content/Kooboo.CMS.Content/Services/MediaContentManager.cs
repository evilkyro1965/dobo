using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Persistence;
using Kooboo.CMS.Content.Models;
using System.Collections.Specialized;
using System.IO;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.Web.Url;
using Kooboo.IO;
using Kooboo.CMS.Content.Query;
namespace Kooboo.CMS.Content.Services
{
    public class MediaContentManager
    {
        private IMediaContentProvider GetContentProvider(Repository repository)
        {
            return Providers.DefaultProviderFactory.GetProvider<IMediaContentProvider>();
        }

        #region ByFolder
        public MediaContent Add(Repository repository, MediaFolder binaryFolder, string fileName, Stream fileStream, bool @overrided, string userId = "")
        {
            IsAllowExtension(fileName, binaryFolder.AllowedExtensions);
            var contentProvider = GetContentProvider(repository);

            var binaryContent = new MediaContent(repository.Name, binaryFolder.FullName);

            binaryContent.UserId = userId;
            binaryContent.Published = true;

            binaryContent.ContentFile = new ContentFile()
            {
                Name = fileName,
                FileName = fileName,
                Stream = fileStream
            };

            binaryContent.UtcLastModificationDate = binaryContent.UtcCreationDate = DateTime.UtcNow;
            contentProvider.Add(binaryContent, @overrided);

            return binaryContent;
        }

        //private static string SaveFile(MediaFolder binaryFolder, string fileName, Stream fileStream)
        //{
        //    var folderPath = new FolderPath(binaryFolder);
        //    var fileVirtualPath = UrlUtility.Combine(folderPath.VirtualPath, fileName);
        //    var filePhysicalPath = Path.Combine(folderPath.PhysicalPath, fileName);
        //    fileStream.SaveAs(filePhysicalPath, false);
        //    return fileVirtualPath;
        //}

        public MediaContent Update(Repository repository, MediaFolder binaryFolder, string uuid, string fileName, Stream fileStream, string userid = "")
        {
            IsAllowExtension(fileName, binaryFolder.AllowedExtensions);
            var contentProvider = GetContentProvider(repository);

            var binaryContent = binaryFolder.CreateQuery().WhereEquals("UUID", uuid).First();
            var old = new MediaContent(binaryContent);
            binaryContent.UserId = userid;
            binaryContent.UtcLastModificationDate = DateTime.Now.ToUniversalTime();
            binaryContent.ContentFile = new ContentFile()
             {
                 Name = fileName,
                 FileName = fileName,
                 Stream = fileStream
             };

            contentProvider.Update(binaryContent, old);

            return binaryContent;
        }

        public void Update(MediaFolder folder, string uuid, IEnumerable<string> fieldNames, IEnumerable<object> fieldValues)
        {
            var mediaFolder = (MediaFolder)folder;
            var content = mediaFolder.CreateQuery().WhereEquals("uuid", uuid).FirstOrDefault();

            if (content != null)
            {
                var newContent = new MediaContent(content);
                var names = fieldNames.ToArray();
                var values = fieldValues.ToArray();
                for (int i = 0; i < names.Length; i++)
                {
                    newContent[names[i]] = values[i];
                }

                GetContentProvider(null).Update(newContent, content);
            }

        }
        private bool IsAllowExtension(string fileName, IEnumerable<string> extensionArr)
        {
            if (extensionArr != null)
            {
                var extension = fileName.Substring(fileName.LastIndexOf('.') + 1);
                if (!extensionArr.Contains(extension))
                {
                    throw new FriendlyException("Current folder doesn't support " + extension);
                }
            }
            return true;
        }

        public void Delete(Repository repository, Folder folder, string uuid)
        {
            var contentProvider = GetContentProvider(repository);
            var binaryFolder = (MediaFolder)folder;
            var binaryContent = binaryFolder.CreateQuery().WhereEquals("UUID", uuid).First();
            contentProvider.Delete(binaryContent);

        }
        #endregion
        public void Move(MediaFolder sourceFolder, string uuid, MediaFolder targetFolder)
        {
             GetContentProvider(null).Move(sourceFolder, uuid, targetFolder);
        }
    }
}
