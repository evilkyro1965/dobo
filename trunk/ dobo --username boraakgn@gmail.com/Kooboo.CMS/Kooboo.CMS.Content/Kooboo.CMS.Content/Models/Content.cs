using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.Dynamic;
using Kooboo.Extensions;
using Kooboo.Web.Url;
using System.IO;
using Kooboo.CMS.Content.Models.Paths;
namespace Kooboo.CMS.Content.Models
{

    public enum ContentType
    {
        Unknown,
        Text,
        Media
    }

    public class ContentFile
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public Stream Stream { get; set; }
    }
    public class ContentBase : DynamicDictionary, IPersistable
    {
        public ContentBase(IDictionary<string, object> dictionary)
            : base(dictionary)
        {
            this.Id = string.Empty;
        }
        public ContentBase()
        {
            this.UserKey = "";
            this.Id = string.Empty;

            this.UtcCreationDate = DateTime.UtcNow;
            this.UtcLastModificationDate = DateTime.UtcNow;

            this.UUID = UUIDGenerator.DefaultGenerator.Generate(this);
        }
        public ContentBase(string repository, string folderName)
            : this()
        {
            this.Repository = repository;
            this.FolderName = folderName;
            this.Id = string.Empty;

            this.UtcCreationDate = DateTime.UtcNow;
            this.UtcLastModificationDate = DateTime.UtcNow;
        }
        public string Id
        {
            get
            {
                if (this.ContainsKey("Id") && base["Id"] != null)
                {
                    return base["Id"].ToString();
                }
                return string.Empty;
            }
            set
            {
                base["Id"] = value;
            }
        }
        public string UUID
        {
            get
            {
                if (this.ContainsKey("UUID") && base["UUID"] != null)
                {
                    return base["UUID"].ToString();
                }
                return string.Empty;
            }
            set
            {
                base["UUID"] = value;
            }
        }
        public string Repository
        {
            get
            {
                if (this.ContainsKey("Repository") && base["Repository"] != null)
                {
                    return base["Repository"].ToString();
                }
                return null;
            }
            set
            {
                base["Repository"] = value;
            }
        }

        public string FolderName
        {
            get
            {
                if (this.ContainsKey("FolderName") && base["FolderName"] != null)
                {
                    return base["FolderName"].ToString();
                }
                return null;
            }
            set
            {
                base["FolderName"] = value;
            }
        }

        public string UserKey
        {
            get
            {
                if (this.ContainsKey("UserKey") && base["UserKey"] != null)
                {
                    return base["UserKey"] == null ? "" : base["UserKey"].ToString();
                }
                return null;
            }
            set
            {
                base["UserKey"] = value;
            }
        }

        public DateTime UtcCreationDate
        {
            get
            {
                if (this.ContainsKey("UtcCreationDate") && base["UtcCreationDate"] != null)
                {
                    return (DateTime)base["UtcCreationDate"];
                }
                return DateTime.MinValue;
            }
            set
            {
                base["UtcCreationDate"] = value;
            }
        }

        public DateTime UtcLastModificationDate
        {
            get
            {
                if (this.ContainsKey("UtcLastModificationDate") && base["UtcLastModificationDate"] != null)
                {
                    return (DateTime)base["UtcLastModificationDate"];
                }
                return DateTime.MinValue;
            }
            set
            {
                base["UtcLastModificationDate"] = value;
            }
        }

        public bool? Published
        {
            get
            {
                if (this.ContainsKey("Published") && base["Published"] != null)
                {
                    return (bool)base["Published"];
                }
                return null;
            }
            set
            {
                base["Published"] = value;
            }
        }

        public string UserId
        {
            get
            {
                if (this.ContainsKey("UserId") && base["UserId"] != null)
                {
                    return base["UserId"] == null ? "" : base["UserId"].ToString();
                }
                return null;
            }
            set
            {
                base["UserId"] = value;
            }
        }
        #region IPersistable Members

        private bool isDummy = true;
        bool IPersistable.IsDummy
        {
            get { return isDummy; }
        }

        void IPersistable.Init(IPersistable source)
        {
            isDummy = false;
        }

        void IPersistable.OnSaved()
        {
            isDummy = false;
        }

        void IPersistable.OnSaving()
        {
            OnSaving();
        }
        protected virtual void OnSaving()
        {
            if (string.IsNullOrEmpty(this.UUID))
            {
                this.UUID = UUIDGenerator.DefaultGenerator.Generate(this);
            }
        }

        #endregion

        public virtual ContentType ContentType
        {
            get
            {
                return Models.ContentType.Unknown;
            }
        }
    }

    public class TextContent : ContentBase
    {
        public TextContent(IDictionary<string, object> dictionary)
            : base(dictionary)
        {
            this.Id = string.Empty;
        }
        public TextContent()
            : base()
        {
            this.Id = string.Empty;
        }
        public TextContent(string repository, string schemaName, string folderName)
            : base(repository, folderName)
        {
            this.Id = string.Empty;
            this.SchemaName = schemaName;
        }

        public string SchemaName
        {
            get
            {
                if (this.ContainsKey("SchemaName") && base["SchemaName"] != null)
                {
                    return base["SchemaName"].ToString();
                }
                return null;
            }
            set
            {
                base["SchemaName"] = value;
            }
        }

        public string ParentFolder
        {
            get
            {
                if (this.ContainsKey("ParentFolder") && base["ParentFolder"] != null)
                {
                    return base["ParentFolder"].ToString();
                }
                return null;
            }
            set
            {
                base["ParentFolder"] = value;
            }
        }
        public string ParentUUID
        {
            get
            {
                if (this.ContainsKey("ParentUUID") && base["ParentUUID"] != null)
                {
                    return base["ParentUUID"].ToString();
                }
                return null;
            }
            set
            {
                base["ParentUUID"] = value;
            }
        }

        public override ContentType ContentType
        {
            get
            {
                return ContentType.Text;
            }
        }
        public string OriginalUUID
        {
            get
            {
                if (this.ContainsKey("OriginalUUID") && base["OriginalUUID"] != null)
                {
                    return base["OriginalUUID"].ToString();
                }
                return null;
            }
            set
            {
                base["OriginalUUID"] = value;
            }
        }
        public string OriginalRepository
        {
            get
            {
                if (this.ContainsKey("OriginalRepository") && base["OriginalRepository"] != null)
                {
                    return base["OriginalRepository"].ToString();
                }
                return null;
            }
            set
            {
                base["OriginalRepository"] = value;
            }
        }
        public string OriginalFolder
        {
            get
            {
                if (this.ContainsKey("OriginalFolder") && base["OriginalFolder"] != null)
                {
                    return base["OriginalFolder"].ToString();
                }
                return null;
            }
            set
            {
                base["OriginalFolder"] = value;
            }
        }
        public bool? IsLocalized
        {
            get
            {
                if (this.ContainsKey("IsLocalized") && base["IsLocalized"] != null)
                {
                    return (bool)base["IsLocalized"];
                }
                return null;
            }
            set
            {
                base["IsLocalized"] = value;
            }
        }

        public int Sequence
        {
            get
            {
                if (this.ContainsKey("Sequence") && base["Sequence"] != null)
                {
                    return Convert.ToInt32(base["Sequence"]);
                }
                return 0;
            }
            set
            {
                base["Sequence"] = value;
            }
        }

        public bool HasAttachment()
        {
            var schema = this.GetSchema();
            foreach (var column in schema.AsActual().Columns.Where(it => string.Compare(it.ControlType, "File", true) == 0))
            {
                var value = this[column.Name];
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<ContentFile> ContentFiles { get; set; }

        protected override void OnSaving()
        {
            base.OnSaving();
            this.UserKey = UserKeyGenerator.DefaultGenerator.Generate(this);
        }


        #region override object
        public override bool Equals(object obj)
        {
            if (!(obj is ContentBase))
            {
                return false;
            }
            var c = (ContentBase)obj;
            if (this.UUID.EqualsOrNullEmpty(c.UUID, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    public class MediaContent : ContentBase
    {
        public MediaContent(IDictionary<string, object> dictionary)
            : base(dictionary)
        {

        }
        public MediaContent()
            : base()
        { }
        public MediaContent(string repository, string folderName)
            : base(repository, folderName)
        {

        }

        public string FileName
        {
            get
            {
                if (this.ContainsKey("FileName") && base["FileName"] != null)
                {
                    return (string)base["FileName"];
                }
                return null;
            }
            set
            {
                base["FileName"] = value;
            }

        }

        public string VirtualPath
        {
            get
            {
                if (this.ContainsKey("VirtualPath") && base["VirtualPath"] != null)
                {
                    return (string)base["VirtualPath"];
                }
                return null;
            }
            set
            {
                base["VirtualPath"] = value;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return UrlUtility.MapPath(Uri.UnescapeDataString(this.VirtualPath));
            }
        }

        public string Url
        {
            get
            {
                return UrlUtility.ResolveUrl(this.VirtualPath);
            }
        }

        public override ContentType ContentType
        {
            get
            {
                return ContentType.Media;
            }
        }

        public ContentFile ContentFile { get; set; }

        public double Size { get; set; }

        public string FileType
        {
            get
            {
                var extension = Path.GetExtension(this.FileName);
                if (string.IsNullOrEmpty(extension))
                {
                    return "Unknow";
                }
                return extension.Substring(extension.LastIndexOf(".") + 1);
            }
        }
    }
}
