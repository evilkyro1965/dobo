using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.CMS.Content.Models;
using System.IO;
using Kooboo.Web.Url;

namespace Kooboo.CMS.Content.Versioning
{
    public class ContentVersionPath : IPath
    {
        public ContentVersionPath(TextContent content)
        {
            var contentPath = new TextContentPath(content);
            this.PhysicalPath = Path.Combine(contentPath.PhysicalPath, VersionPathName);
            this.VirtualPath = UrlUtility.Combine(contentPath.VirtualPath, VersionPathName);
        }
        private static string VersionPathName = "~versions";
        #region IPath Members

        public string PhysicalPath
        {
            get;
            private set;
        }

        public string VirtualPath
        {
            get;
            private set;
        }

        public string SettingFile
        {
            get;
            private set;
        }

        public bool Exists()
        {
            return Directory.Exists(this.PhysicalPath);
        }

        public void Rename(string newName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
