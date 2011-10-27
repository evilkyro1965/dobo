using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Sites;
using Kooboo.CMS.Web.Areas.Contents.Models;
using Kooboo.CMS.Web.Authorizations;
using Kooboo.CMS.Web.Models;
using Kooboo.Globalization;
using Kooboo.Web.Mvc;
namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
    [LargeFileAuthorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
    public class MediaContentController : ContentControllerBase
    {
        //
        // GET: /Contents/Midia/

        MediaContentManager ContentManager
        {
            get
            {
                return ServiceFactory.MediaContentManager;
            }
        }

        MediaFolderManager FolderManager
        {
            get
            {
                return ServiceFactory.MediaFolderManager;
            }
        }
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 99)]
        public ActionResult Index(string folderName, string search, int? page, int? pageSize)
        {
            return MediaContentGrid(folderName, search, page, pageSize);
        }

        #region Create
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        public ActionResult Create(string folder)
        {
            return View();
        }
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpPost]
        public ActionResult Create(string folderName, FormCollection form)
        {
            JsonResultEntry resultEntry = new JsonResultEntry(ModelState);
            try
            {
                HttpFileCollectionBase files = Request.Files;
                SaveFile(folderName, files);
            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }
            return Json(resultEntry);

            //return View();
        }

        private void SaveFile(string folderName, HttpFileCollectionBase files)
        {
            if (files != null)
            {
                var @overrided = string.Equals(Request.Form["Overrided"], "true", StringComparison.OrdinalIgnoreCase);
                var currentFolder = FolderManager.Get(Repository, folderName);
                foreach (var f in files.AllKeys)
                {
                    var file = files[f];
                    if (file.ContentLength > 0)
                    {
                        //if upload from ie filename will be fullpath include disk symbol 
                        var fileName = file.FileName.Contains("\\") ? file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1) : file.FileName;

                        ContentManager.Add(Repository, currentFolder, fileName, file.InputStream, overrided, User.Identity.Name);
                    }
                }
            }
        }

        public JsonResult ValidFileExisted(IEnumerable<string> files, string folderName)
        {
            List<object> result = new List<object>();
            foreach (var f in files)
            {
                var content = new MediaContent()
                {
                    Repository = Repository.Name,
                    FileName = f,
                    FolderName = folderName
                };
                result.Add(new
                {
                    Item = f,
                    Exist = content.Exist(),
                    Data = content
                });
            }
            return Json(result);
        }
        #endregion

        #region Delete
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        public ActionResult Delete(string folderName, string uuid)
        {
            var folder = FolderManager.Get(Repository, folderName);
            var folderPath = FolderHelper.SplitFullName(folderName);
            ContentManager.Delete(Repository, folder, uuid);

            return RedirectToAction("Index", new { folderName = folderName });
        }
        // Kooboo.CMS.Account.Models.Permission.Contents_ContentPermission
        [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Content", Order = 1)]
        [HttpPost]
        public ActionResult DeleteSelect(string folderName, string[] folders, string[] files)
        {

            var entry = new JsonResultEntry();

            try
            {
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    if (folders != null)
                        foreach (var f in folders)
                        {
                            if (string.IsNullOrEmpty(f)) continue;
                            var folderPath = FolderHelper.SplitFullName(f);
                            FolderManager.Remove(Repository, new MediaFolder(Repository, folderPath));
                        }
                }
                else
                {
                    var folder = FolderManager.Get(Repository, folderName).AsActual();
                    if (folders != null)
                        foreach (var f in folders)
                        {
                            if (string.IsNullOrEmpty(f)) continue;
                            var folderPath = FolderHelper.SplitFullName(f);
                            FolderManager.Remove(Repository, new MediaFolder(Repository, folderPath));
                        }

                    if (files != null)
                        foreach (var f in files)
                        {
                            if (string.IsNullOrEmpty(f)) continue;
                            ContentManager.Delete(Repository, folder, f);
                        }
                }

                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.SetFailed().AddException(e);
            }

            return Json(entry);

            //return RedirectToAction("Index", new { folderName = folderName });
        }
        #endregion

        #region Publish
        [HttpPost]
        public ActionResult Publish(string uuid, string folderName, bool published)
        {
            var folder = FolderManager.Get(Repository, folderName);
            JsonResultEntry entry = new JsonResultEntry();
            try
            {
                ContentManager.Update(folder, uuid, new string[] { "Published" }, new object[] { published });
                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        #endregion

        #region  selection

        public ActionResult Selection(string folderName, string search, int? page, int? pageSize)
        {

            return MediaContentGrid(folderName, search, page, pageSize ?? 20);
        }

        #endregion

        #region Import
        public ActionResult Import(string folderName, bool @overrided)
        {
            var entry = new JsonResultEntry();
            try
            {
                FolderManager.Import(Repository, Request.Files[0].InputStream, folderName, @overrided);
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        #endregion

        #region MediaContentGrid

        private ActionResult MediaContentGrid(string folderName, string search, int? page, int? pageSize)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                var folders = FolderManager.All(Repository, search, "");
                return View(new MediaContentGrid
                {
                    ChildFolders = folders
                });
            }
            else
            {
                IEnumerable<MediaFolder> childFolders = new MediaFolder[0];
                if (!page.HasValue || page.Value <= 1)
                {
                    childFolders = FolderManager.All(Repository, search, folderName);
                }

                var currentFolder = FolderManager.Get(Repository, folderName);
                var contentQuery = currentFolder.CreateQuery();
                if (!string.IsNullOrEmpty(search))
                {
                    contentQuery = contentQuery.WhereContains("FileName", search);
                }
                return View(new MediaContentGrid
                {
                    ChildFolders = childFolders,
                    Contents = contentQuery.ToPageList(page ?? 0, pageSize ?? 50)
                });
            }
        }

        #endregion

        #region Image

        public ActionResult ImageDetailInfo(string folderName, string fileName)
        {
            var folder = FolderHelper.Parse<MediaFolder>(Repository, folderName);

            FolderPath path = new FolderPath(folder);

            var fullName = System.IO.Path.Combine(new string[] { path.PhysicalPath, fileName });

            object jsonData = new { Success = false };
            try
            {
                using (var img = System.Drawing.Image.FromFile(fullName))
                {
                    jsonData = new
                    {
                        Success = true,
                        FileName = System.IO.Path.GetFileName(fullName),
                        //FileUrl = url,
                        Width = img.Width,
                        Height = img.Height
                    };
                }
            }
            catch { }

            // ret
            return Json(jsonData);
        }

        #endregion

        #region Large File
        public ActionResult LargeFile()
        {
            return View();
        }

        [HttpPost]
        [LargeFileAuthorization(Order = 1)]
        public ActionResult LargeFile(FormCollection form, string folder)
        {
            var f = folder.Substring(folder.LastIndexOf('/') + 1);
            SaveFile(f, Request.Files);
            return View();
        }

        [HttpPost]
        public ActionResult CheckLargeFile()
        {
            return Json(0);
        }
        #endregion

        #region TextFile
        public ActionResult TextFile(string folderName, string fileName)
        {
            MediaContent content = new MediaContent(Repository.Name, folderName);
            content.FileName = fileName;

            var contentPath = new MediaContentPath(content);

            string body = Kooboo.IO.IOUtility.ReadAsString(contentPath.PhysicalPath);

            return View(new TextFileModel
            {
                Title = fileName,
                Body = body
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TextFile(string folderName, string fileName, string body)
        {
            var entry = new JsonResultEntry();

            try
            {
                MediaContent content = new MediaContent(Repository.Name, folderName);
                content.FileName = fileName;

                var contentPath = new MediaContentPath(content);

                Kooboo.IO.IOUtility.SaveStringToFile(contentPath.PhysicalPath, body);
                entry.SetSuccess();
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        #endregion

        #region Rename & Move
        /// <summary>
        /// Renames the specified folder name.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="uuid">The UUID.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ActionResult RenameFile(string folderName, string uuid, string name)
        {
            JsonResultEntry entry = new JsonResultEntry();

            try
            {
                var mediaFolder = new MediaFolder(Repository, folderName);
                ServiceFactory.MediaContentManager.Update(mediaFolder, uuid, new[] { "FileName" }, new object[] { name });
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }
        [HttpGet]
        public ActionResult MoveFile(string folderName, string uuid)
        {
            return View(new MoveMediaContent());
        }
        [HttpPost]
        public ActionResult MoveFile(string folderName, string uuid, string targetFolder)
        {
            JsonResultEntry entry = new JsonResultEntry();

            try
            {
                var source = new MediaFolder(Repository, folderName);
                var target = new MediaFolder(Repository, targetFolder);
                ServiceFactory.MediaContentManager.Move(source, uuid, target);
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }

            return Json(entry);
        }

        public ActionResult TargetFolderAvailable(string folderName, string targetFolder)
        {
            if (folderName.EqualsOrNullEmpty(targetFolder, StringComparison.OrdinalIgnoreCase))
            {
                return Json("The target folder is the same as the source folder.".Localize());
            }
            return Json(true);
        }
        #endregion
    }

    public class TextFileModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }


}
