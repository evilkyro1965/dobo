﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.Collections;
using System.IO;
using Kooboo.Web.Url;
namespace Kooboo.CMS.Sites.Models
{
    public class CustomFile : FileResource, IInheritable<CustomFile>
    {
        public CustomFile()
        { }
        public CustomFile(string physicalPath)
            : base(physicalPath)
        {
        }
        public CustomFile(Site site, string fileName)
            : base(site, fileName)
        {

        }
        public CustomFile(CustomDirectory dir, string fileName)
            : base(dir.Site, fileName)
        {
            this.Directory = dir;
        }
        public override string PhysicalPath
        {
            get
            {
                if (this.Directory != null)
                {
                    return Path.Combine(Directory.PhysicalPath, this.FileName);
                }
                return base.PhysicalPath;
            }
        }
        public override string VirtualPath
        {
            get
            {
                if (this.Directory != null)
                {
                    return UrlUtility.Combine(Directory.VirtualPath, this.FileName);
                }
                return base.VirtualPath;
            }
        }

        public CustomDirectory Directory { get; set; }
        private string fileType;
        public string FileType
        {
            get
            {
                if (string.IsNullOrEmpty(fileType))
                {
                    fileType = this.FileExtension;
                    fileType = fileType.Contains(".") ? fileType.Substring(1) : fileType;
                }
                return fileType;
            }
            set
            {
                fileType = value;
            }
        }


        internal const string PATH_NAME = "Files";
        public override IEnumerable<string> RelativePaths
        {
            get
            {
                if (Directory == null)
                {
                    return new string[] { PATH_NAME };
                }
                else
                {
                    return Directory.RelativePaths;
                }
            }
        }

        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            this.FileName = relativePaths.Last();
            var pathNameIndex = relativePaths.IndexOf(CustomFile.PATH_NAME, StringComparer.InvariantCultureIgnoreCase);
            var count = relativePaths.Count();
            if (pathNameIndex + 2 < count)
            {
                this.Directory = new CustomDirectory();
                this.Directory.ParseObject(relativePaths.Take(count - 1));
            }
            return relativePaths.Take(pathNameIndex);
        }

        #region IInheritable<ImageFile> Members

        public CustomFile LastVersion()
        {
            var lastVersion = this;
            while (!lastVersion.Exists())
            {
                if (lastVersion.Site.Parent == null)
                {
                    break;
                }
                lastVersion = new CustomFile(lastVersion.Site.Parent, this.FileName);
            }
            return lastVersion;
        }
        public bool IsLocalized(Site site)
        {
            throw new NotImplementedException();
        }
        public bool HasParentVersion()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
