﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Kooboo.Web.Url;
using Kooboo.CMS.Sites.Persistence.FileSystem;

namespace Kooboo.CMS.Sites.Services
{
    public static class FileOrderHelper
    {
        public static string OrderFileName = "Order.txt";
        public static IEnumerable<string> OrderFiles(string baseDir, IEnumerable<string> fileNames)
        {
            string orderFile = GetOrderFile(baseDir);
            if (File.Exists(orderFile))
            {
                var lines = File.ReadAllLines(orderFile);
                var ordered = lines.Where(it => fileNames.Any(f => f.EqualsOrNullEmpty(it, StringComparison.CurrentCultureIgnoreCase)));
                return ordered.Concat(fileNames.Except(lines, StringComparer.CurrentCultureIgnoreCase));
            }
            return fileNames;
        }

        public static void SaveFilesOrder(string baseDir, IEnumerable<string> filesOrder)
        {
            var orderFile = Path.Combine(baseDir, FileOrderHelper.OrderFileName);
            if (File.Exists(orderFile))
            {
                File.SetAttributes(orderFile, FileAttributes.Normal);
            }
            File.WriteAllLines(orderFile, filesOrder.ToArray());
            File.SetAttributes(orderFile, FileAttributes.Hidden);
        }
        public static string GetOrderFile(string baseDir)
        {
            return Path.Combine(baseDir, OrderFileName);
        }
    }
    public class FileEntry : FileResource
    {
        public FileEntry()
        {
        }
        public FileEntry(DirectoryResource rootDir, string relativePath)
            : base("")
        {
            this.physicalPath = Path.Combine(rootDir.PhysicalPath, relativePath);
            this.virtualPath = UrlUtility.GetVirtualPath(physicalPath);

            this.basePhysicalPath = Path.GetDirectoryName(this.physicalPath);
            var paths = this.virtualPath.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            this.baseVirtualPath = UrlUtility.Combine(paths.Take(paths.Count() - 1).ToArray());
            this.FileName = paths.Last();
            if (this.FileName.Contains('.'))
            {
                this.Name = FileName.Substring(0, FileName.IndexOf("."));
                this.FileExtension = FileExtension.Substring(FileExtension.IndexOf(".") + 1);
            }
            else
            {
                this.Name = FileName;
            }

            this.RootDir = rootDir;
            //this.RelativePath = relativeVirtualPath;
        }

        protected FileEntry(Site site, string fileName)
            : base(site, Path.GetFileNameWithoutExtension(fileName))
        {
            this.FileExtension = Path.GetExtension(fileName);
        }

        //public FileEntry(DirectoryResource rootDir, string fileName)
        //    : this(directory.Site, fileName)
        //{
        //    this.virtualPath = UrlUtility.Combine(directory.VirtualPath, fileName);
        //    this.physicalPath = Path.Combine(directory.PhysicalPath, fileName);
        //    this.basePhysicalPath = directory.PhysicalPath;
        //    this.baseVirtualPath = directory.VirtualPath;

        //    this.RootDir = rootDir;
        //}

        public override IEnumerable<string> RelativePaths
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        private string physicalPath = null;
        public override string PhysicalPath
        {
            get { return physicalPath; }
        }
        private string virtualPath = null;
        public override string VirtualPath
        {
            get
            {
                return virtualPath;
            }
        }
        private string basePhysicalPath = null;
        public override string BasePhysicalPath
        {
            get
            {
                return basePhysicalPath;
            }
        }
        private string baseVirtualPath = null;
        public override string BaseVirtualPath
        {
            get
            {
                return baseVirtualPath;
            }
        }

        private string name;
        public override string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileName(this.physicalPath);
                }
                return name;
            }
            set
            {
                name = value;
            }
        }

        public decimal FileSize { get; set; }
        public DateTime CreateDate { get; set; }

        public DirectoryResource RootDir { get; set; }
        /// <summary>
        /// the relative path from current root dir
        /// <example>
        /// The full virtual path: '..\Cms_Data\Sites\aaa\Sites\cn1\Themes\Theme1', the RelativeVirtualPathFromRoot will be: 'Theme1'
        /// Hide '..\Cms_Data\Sites\aaa\Sites\cn1\Themes\'
        /// </example>
        /// </summary>
        public string RelativePath
        {
            get
            {
                if (RootDir == null)
                {
                    throw new KoobooException("The root dir is null.");
                }
                return this.PhysicalPath.Remove(0, RootDir.PhysicalPath.Length + 1);
            }
        }
    }
    public class DirectoryEntry : DirectoryResource
    {
        public DirectoryEntry()
        {
        }
        public DirectoryEntry(DirectoryResource rootDir, string relativePath)
            : base("")
        {
            this.physicalPath = Path.Combine(rootDir.PhysicalPath, relativePath);
            this.virtualPath = UrlUtility.GetVirtualPath(physicalPath);
            this.basePhysicalPath = Path.GetDirectoryName(this.physicalPath);
            var paths = this.virtualPath.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            this.baseVirtualPath = UrlUtility.Combine(paths.Take(paths.Count() - 1).ToArray());

            this.RootDir = rootDir;
        }
        public DirectoryEntry(DirectoryResource rootDir, DirectoryResource parent, string name)
            : base(parent.Site, name)
        {
            this.virtualPath = UrlUtility.Combine(parent.VirtualPath, name);
            this.physicalPath = Path.Combine(parent.PhysicalPath, name);
            this.basePhysicalPath = parent.PhysicalPath;
            this.baseVirtualPath = parent.VirtualPath;

            this.RootDir = rootDir;
        }
        protected DirectoryEntry(Site site, string name)
            : base(site, name)
        {
        }

        #region override
        public override IEnumerable<string> RelativePaths
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        private string physicalPath = null;
        public override string PhysicalPath
        {
            get { return physicalPath; }
        }
        private string virtualPath = null;
        public override string VirtualPath
        {
            get
            {
                return virtualPath;
            }
        }
        private string basePhysicalPath = null;
        public override string BasePhysicalPath
        {
            get
            {
                return basePhysicalPath;
            }
        }
        private string baseVirtualPath = null;
        public override string BaseVirtualPath
        {
            get
            {
                return baseVirtualPath;
            }
        }

        private string name;
        public override string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileName(this.physicalPath);
                }
                return name;
            }
            set
            {
                name = value;
            }
        }
        #endregion

        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            this.Name = relativePaths.Last();
            //this._relativePaths = relativePaths.Take(relativePaths.Count() - 2);
            return new string[0];
        }

        public DirectoryResource RootDir { get; set; }
        /// <summary>
        /// the relative path from current root dir
        /// <example>
        /// The full virtual path: '..\Cms_Data\Sites\aaa\Sites\cn1\Themes\Theme1', the RelativeVirtualPathFromRoot will be: 'Theme1'
        /// Hide '..\Cms_Data\Sites\aaa\Sites\cn1\Themes\'
        /// </example>
        /// </summary>
        public string RelativePath
        {
            get
            {
                if (RootDir == null)
                {
                    throw new KoobooException("The root dir is null.");
                }
                return this.physicalPath.Remove(0, RootDir.PhysicalPath.Length + 1);
            }
        }
    }
    public class CustomFileBaseDirectory : DirectoryResource
    {
        public CustomFileBaseDirectory(Site site)
            : base(site, CustomFile.PATH_NAME)
        {

        }
        protected CustomFileBaseDirectory()
            : base()
        { }
        protected CustomFileBaseDirectory(string physicalPath)
            : base(physicalPath)
        {
        }
        protected CustomFileBaseDirectory(Site site, string name)
            : base(site, name)
        {
        }
        public override IEnumerable<string> RelativePaths
        {
            get { return new string[0]; }
        }

        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            return relativePaths;
        }
    }
    public class ThemeBaseDirectory : DirectoryResource
    {
        public ThemeBaseDirectory(Site site)
            : base(site, Theme.PATH_NAME)
        {

        }
        protected ThemeBaseDirectory()
            : base()
        { }
        protected ThemeBaseDirectory(string physicalPath)
            : base(physicalPath)
        {
        }
        protected ThemeBaseDirectory(Site site, string name)
            : base(site, name)
        {
        }
        public override IEnumerable<string> RelativePaths
        {
            get { return new string[0]; }
        }

        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            return relativePaths;
        }
    }
    public abstract class FileManager
    {
        protected abstract Models.DirectoryResource GetRootDir(Site site);

        public string GetRelativePath(string parentRelativePath, string name)
        {
            return string.IsNullOrEmpty(parentRelativePath) ? name : parentRelativePath + Path.DirectorySeparatorChar + name;
        }
        #region Directory
        public virtual DirectoryResource GetDirectory(Site site, string relativePath)
        {
            var dir = GetRootDir(site);
            if (!string.IsNullOrEmpty(relativePath))
                dir = new DirectoryEntry(dir, relativePath);
            return dir;
        }
        public virtual IEnumerable<DirectoryResource> GetDirectories(Site site, string relativePath)
        {
            var dir = GetDirectory(site, relativePath);
            if (dir.Exists())
            {
                foreach (var item in IO.IOUtility.EnumerateDirectoriesExludeHidden(dir.PhysicalPath))
                {                    
                    yield return new DirectoryEntry(GetRootDir(site), GetRelativePath(relativePath, item.Name)) { LastUpdateDate = item.LastWriteTime };
                }
            }
        }



        public virtual void DeleteDirectory(Site site, string relativePath)
        {
            var dir = new DirectoryEntry(GetRootDir(site), relativePath);
            dir.Delete();
        }
        public virtual void AddDirectory(Site site, string parentRelativePath, string name)
        {
            DirectoryEntry dir = new DirectoryEntry(GetRootDir(site), GetRelativePath(parentRelativePath, name));
            System.IO.Directory.CreateDirectory(dir.PhysicalPath);
        }

        public virtual DirectoryResource RenameDirectory(Site site, string relativePath, string newName)
        {
            //DirectoryEntry @new = new DirectoryEntry(GetRootDir(site), new_RelativeVirtualPath);
            DirectoryEntry dir = new DirectoryEntry(GetRootDir(site), relativePath);
            dir.Rename(newName);
            return dir;
        }

        #endregion

        #region File

        public virtual IEnumerable<FileEntry> GetFiles(Site site, string dirRelativePath)
        {
            var dir = GetDirectory(site, dirRelativePath);
            if (dir.Exists())
            {
                var files = EnumerateFiles(dir.PhysicalPath);
                files = FileOrderHelper.OrderFiles(dir.PhysicalPath, files);
                return files.Select(it => new FileInfo(Path.Combine(dir.PhysicalPath, it)))
                    .Select(it => new FileEntry(GetRootDir(site), GetRelativePath(dirRelativePath, it.Name))
                {
                    FileSize = it.Length,
                    Name = it.Name,
                    FileExtension = it.Extension,
                    FileName = it.FullName,
                    CreateDate = it.LastWriteTimeUtc
                });
            }
            return new FileEntry[0];
        }
        private IEnumerable<string> EnumerateFiles(string dir)
        {
            foreach (var item in System.IO.Directory.EnumerateFiles(dir))
            {
                FileInfo fi = new FileInfo(item);
                if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    yield return Path.GetFileName(item);
            }
        }
        public virtual void SaveFileOrders(Site site, string dirRelativePath, IEnumerable<string> filesOrder)
        {
            var baseDir = GetDirectory(site, dirRelativePath);
            FileOrderHelper.SaveFilesOrder(baseDir.PhysicalPath, filesOrder);
        }

        public virtual FileEntry GetFile(Site site, string relativePath)
        {
            FileEntry entry = new FileEntry(GetRootDir(site), relativePath);
            if (entry.Exists())
            {
                var fi = new FileInfo(entry.PhysicalPath);
                entry.Name = fi.Name;
                entry.FileName = fi.FullName;
                entry.FileExtension = fi.Extension;
                entry.Read();

            }
            return entry;
        }

        public virtual FileEntry EditFile(Site site, string dirRelativePath, string oldRelativePath, string oldFileName, string body)
        {
            var @new = new FileEntry(GetRootDir(site), GetRelativePath(dirRelativePath, oldFileName));
            var old = new FileEntry(GetRootDir(site), oldRelativePath);
            File.Move(old.PhysicalPath, @new.PhysicalPath);
            @new.Body = body;
            @new.Save();
            return @new;
        }

        public virtual void AddFile(Site site, string dirRelativePath, string fileName, Stream fileStream)
        {
            var file = new FileEntry(GetRootDir(site), GetRelativePath(dirRelativePath, fileName));
            if (fileStream != null)
            {
                file.Save(fileStream);
            }
            else
            {
                file.Save();
            }
        }

        public virtual void AddFile(Site site, string dirRelativePath, string fileName, string body)
        {
            var file = new FileEntry(GetRootDir(site), GetRelativePath(dirRelativePath, fileName));
            file.Body = body;
            file.Save();
        }

        public virtual void DeleteFile(Site site, string fileRelativePath)
        {
            var file = new FileEntry(GetRootDir(site), fileRelativePath);
            file.Delete();
        }

        #endregion

        #region Import & Export
        public void Import(Site site, string directoryPath, Stream zipStream, bool @overrided)
        {
            ImportHelper.Import(site, GetDirectory(site, directoryPath).PhysicalPath, zipStream, @overrided);
        }
        #endregion
    }
}
