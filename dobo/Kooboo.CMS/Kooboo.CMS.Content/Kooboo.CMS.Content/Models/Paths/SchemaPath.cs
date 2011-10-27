using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.Web.Url;
using System.IO;
using Kooboo.CMS.Form;


namespace Kooboo.CMS.Content.Models.Paths
{
    public class SchemaPath : IPath
    {
        public SchemaPath(Schema schema)
        {
            RepositoryPath repositoryPath = new RepositoryPath(schema.Repository);
            var basePhysicalPath = GetBaseDir(schema.Repository);
            this.PhysicalPath = Path.Combine(basePhysicalPath, schema.Name);
            this.SettingFile = Path.Combine(PhysicalPath, "setting.config");
            VirtualPath = UrlUtility.Combine(repositoryPath.VirtualPath, PATH_NAME, schema.Name);
        }
        public static string GetBaseDir(Repository repository)
        {
            return Path.Combine(new RepositoryPath(repository).PhysicalPath, PATH_NAME);
        }
        const string PATH_NAME = "Schemas";
        public static string CUSTOM_TEMPLATES = "CustomTemplates";

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
            return File.Exists(this.SettingFile);
        }

        public void Rename(string newName)
        {
            IO.IOUtility.RenameFile(this.PhysicalPath, @newName + ".config");
        }

        #endregion

        #region Template file
        public string GridVirtualPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.Grid);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.Grid);
                }
                return virtualPath;
            }
        }
        public string CreateVirutalPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.Create);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.Create);
                }
                return virtualPath;
            }
        }

        public string UpdateVirtualPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.Update);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.Update);
                }
                return virtualPath;
            }
        }

        public string SelectableVirtualPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.Selectable);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.Selectable);
                }
                return virtualPath;
            }
        }

        public string ListVirtualPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.List);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.List);
                }
                return virtualPath;
            }
        }
        public string DetailVirtualPath
        {
            get
            {
                var virtualPath = GetCustomTemplateFileVirtualPath(FormType.Detail);
                if (string.IsNullOrEmpty(virtualPath))
                {
                    virtualPath = GetFormFileVirtualPath(FormType.Detail);
                }
                return virtualPath;
            }
        }

        public string GetFormFileVirtualPath(FormType type)
        {
            var razorTemplate = GetFormFilePhysicalPath(type);
            var templateVirtualPath = UrlUtility.Combine(this.VirtualPath, string.Format("{0}.ascx", type));
            if (File.Exists(razorTemplate))
            {
                templateVirtualPath = UrlUtility.Combine(this.VirtualPath, string.Format("{0}.cshtml", type));
            }
            return templateVirtualPath;
        }
        public string GetFormFilePhysicalPath(FormType type)
        {
            return Path.Combine(this.PhysicalPath, string.Format("{0}.cshtml", type));
        }

        public string GetCustomTemplateFileVirtualPath(FormType type)
        {
            string fileVirtualPath = "";
            string filePhysicalPath = Path.Combine(this.PhysicalPath, CUSTOM_TEMPLATES, string.Format("{0}.cshtml", type));
            if (File.Exists(filePhysicalPath))
            {
                fileVirtualPath = UrlUtility.Combine(this.VirtualPath, CUSTOM_TEMPLATES, string.Format("{0}.cshtml", type));
            }
            return fileVirtualPath;
        }
        #endregion
    }
}
