using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;

namespace Kooboo.CMS.Sites.Versioning
{
    public class VersionBasePath
    {
        public static string VersionFolderName = "~versions";
        public VersionBasePath(DirectoryResource dir)
        {
            this.PhysicalPath = Path.Combine(dir.PhysicalPath, VersionFolderName);
        }
        public string PhysicalPath { get; set; }
        public bool Exists()
        {
            return Directory.Exists(this.PhysicalPath);
        }
    }
    public class VersionPath
    {
        public VersionPath(DirectoryResource dir, int version)
        {
            var versionBasePath = new VersionBasePath(dir);
            this.PhysicalPath = Path.Combine(versionBasePath.PhysicalPath, version.ToString());
        }
        public string PhysicalPath { get; set; }
        public bool Exists()
        {
            return Directory.Exists(this.PhysicalPath);
        }
    }
}
