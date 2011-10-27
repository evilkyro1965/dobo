using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Query.Expressions;

namespace Kooboo.CMS.Content.Models
{
    //public enum UrlParameterValueType
    //{
    //    Const,
    //    Content,
    //    QueryString
    //}
    //public class UrlParameterValue
    //{
    //    public string Value { get; set; }
    //    public UrlParameterValueType ValueType { get; set; }
    //}
    //public class UrlParameter
    //{
    //    public string Name { get; set; }
    //    public UrlParameterValue Value { get; set; }
    //}
    //public class FolderUrlSetting
    //{
    //    public string Url { get; set; }
    //    public IList<UrlParameter> Parameters { get; set; }
    //}
    public static class FolderHelper
    {
        public static string CombineFullName(IEnumerable<string> names)
        {
            return string.Join("~", names.ToArray());
        }
        public static IEnumerable<string> SplitFullName(string fullName)
        {
            return fullName.Split(new char[] { '~', '/' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static T Parse<T>(Repository repository, string fullName)
            where T : Folder
        {
            var names = SplitFullName(fullName);
            if (typeof(T) == typeof(CMS.Content.Models.TextFolder))
            {
                return (T)((object)new TextFolder(repository, names));
            }
            else
            {
                return (T)((object)new MediaFolder(repository, names));
            }



        }
    }
    [DataContract(Name = "Folder")]
    [KnownTypeAttribute(typeof(Folder))]
    public class Folder : IRepositoryElement, IPersistable
    {
        public Folder()
        { }
        public Folder(Repository repository, string name, Folder parent)
        {
            Repository = repository;
            this.Name = name;
            this.Parent = parent;
        }
        public Folder(Repository repository, string fullName) :
            this(repository, FolderHelper.SplitFullName(fullName))
        {

        }
        public Folder(Repository repository, IEnumerable<string> namePath)
        {
            if (namePath == null || namePath.Count() < 1)
            {
                throw new ArgumentException("The folder name path is invalid.", "namePath");
            }
            this.Repository = repository;
            this.Name = namePath.Last();
            if (namePath.Count() > 0)
            {
                foreach (var name in namePath.Take(namePath.Count() - 1))
                {
                    this.Parent = (Folder)Activator.CreateInstance(this.GetType(), repository, name, this.Parent);
                }
            }

        }
        public Repository Repository { get; set; }
        [XmlIgnore]
        public string Name { get; set; }
        [DataMember(Order = 3)]
        public string DisplayName { get; set; }
        [DataMember(Order = 5)]
        public DateTime UtcCreationDate { get; set; }
        [DataMember(Order = 7)]
        public string UserId { get; set; }

        #region IPersistable Members

        bool isDummy = true;
        public bool IsDummy
        {
            get { return isDummy; }
            set { isDummy = value; }
        }

        void IPersistable.Init(IPersistable source)
        {
            isDummy = false;
            this.Name = ((Folder)source).Name;
            this.Repository = ((Folder)source).Repository;
            this.Parent = ((Folder)source).Parent;
        }

        void IPersistable.OnSaved()
        {

        }

        void IPersistable.OnSaving()
        {

        }

        #endregion

        #region override object
        public static bool operator ==(Folder obj1, Folder obj2)
        {
            if (object.Equals(obj1, obj2) == true)
            {
                return true;
            }
            if (object.Equals(obj1, null) == true || object.Equals(obj2, null) == true)
            {
                return false;
            }
            return obj1.Equals(obj2);
        }
        public static bool operator !=(Folder obj1, Folder obj2)
        {
            return !(obj1 == obj2);
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var folder = (Folder)obj;
                if (this.Repository == folder.Repository && string.Compare(this.Name, folder.Name, true) == 0)
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return this.FriendlyText;
        }
        #endregion

        public Folder Parent { get; set; }

        public IEnumerable<string> NamePath
        {
            get
            {
                if (Parent == null)
                {
                    return new[] { this.Name };
                }
                else
                {
                    return Parent.NamePath.Concat(new[] { this.Name });
                }
            }
        }

        //private string fullName;
        public string FullName
        {
            get
            {
                return FolderHelper.CombineFullName(this.NamePath);
            }
            //set
            //{
            //    fullName = value;
            //}
        }
        public string FriendlyName
        {
            get
            {
                return string.Join("/", NamePath.ToArray());
            }
        }
        public string FriendlyText
        {
            get
            {
                if (string.IsNullOrEmpty(this.DisplayName))
                {
                    return this.Name;
                }
                return this.DisplayName;
            }
        }
    }

    public class CategoryFolder
    {
        public string FolderName { get; set; }
        public bool SingleChoice { get; set; }
    }
    public enum OrderDirection
    {
        Ascending,
        Descending
    }
    public class OrderSetting
    {
        public static readonly OrderSetting Default = new OrderSetting() { FieldName = "UtcCreationDate", Direction = OrderDirection.Descending };
        public string FieldName { get; set; }
        public OrderDirection Direction { get; set; }
    }
    //[DataContract(Name = "ContainerFolder")]
    //[KnownTypeAttribute(typeof(ContainerFolder))]
    //public class ContainerFolder : Folder
    //{
    //    public ContainerFolder()
    //    { }
    //    public ContainerFolder(Repository repository, string name)
    //        : base(repository, name)
    //    {

    //    }
    //    public ContainerFolder(Repository repository, string name, Folder parent)
    //        : base(repository, name, parent)
    //    {

    //    }
    //}
    //[DataContract(Name = "ContentFolder")]
    //[KnownTypeAttribute(typeof(ContentFolder))]
    //public class ContentFolder : Folder
    //{
    //    public ContentFolder()
    //    {

    //    }
    //    public ContentFolder(Repository repository, string name) : base(repository, name) { }
    //    public ContentFolder(Repository repository, string name, Folder parent)
    //        : base(repository, name, parent)
    //    {

    //    }

    //    //[DataMember(Order = 5)]
    //    //public FolderUrlSetting ListUrl { get; set; }
    //    //[DataMember(Order = 7)]
    //    //public FolderUrlSetting DetailUrl { get; set; }
    //}
    [DataContract(Name = "TextFolder")]
    [KnownTypeAttribute(typeof(TextFolder))]
    public class TextFolder : Folder
    {
        public TextFolder() { }
        public TextFolder(Repository repository, string fullName) : base(repository, fullName) { }
        public TextFolder(Repository repository, string name, Folder parent) : base(repository, name, parent) { }
        public TextFolder(Repository repository, IEnumerable<string> namePath) : base(repository, namePath) { }
        [DataMember(Order = 9)]
        public string SchemaName { get; set; }

        [DataMember(Order = 10)]
        public List<CategoryFolder> Categories { get; set; }

        [Obsolete("Use Categories instead of CategoryFolders")]
        [DataMember(Order = 11)]
        public string[] CategoryFolders
        {
            get
            {
                return new string[0];
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    Categories = value.Select(it => new CategoryFolder() { FolderName = it, SingleChoice = false }).ToList();
                }
            }
        }

        [DataMember(Order = 13)]
        public string[] EmbeddedFolders { get; set; }

        [DataMember(Order = 14)]
        public string WorkflowName { get; set; }

        [DataMember(Order = 15)]
        public string[] Roles { get; set; }

        private OrderSetting orderSetting;
        [DataMember(Order = 16)]
        public OrderSetting OrderSetting
        {
            get
            {
                if (orderSetting == null)
                {
                    orderSetting = new Models.OrderSetting() { FieldName = OrderSetting.Default.FieldName, Direction = OrderSetting.Default.Direction };
                }
                return orderSetting;
            }
            set
            {
                orderSetting = value;
            }
        }

        public bool EnabledWorkflow
        {
            get
            {
                return !string.IsNullOrEmpty(WorkflowName);
            }
        }
        //[DataMember(Order = 12)]
        //public IEnumerable<string> BinaryFolders { get; set; }

        public virtual OrderExpression DefaultOrderExpression(IExpression expression)
        {
            var orderExpression = new OrderExpression(expression, "UtcCreationDate", true);
            var folder = this.AsActual();
            if (folder.OrderSetting != null && !string.IsNullOrEmpty(folder.OrderSetting.FieldName))
            {
                if (folder.OrderSetting.Direction == OrderDirection.Ascending)
                {
                    orderExpression = new OrderExpression(null, folder.OrderSetting.FieldName, false);
                }
                else
                {
                    orderExpression = new OrderExpression(null, folder.OrderSetting.FieldName, true);
                }
            }
            return orderExpression;
        }

        public bool OrderBySequence
        {
            get
            {
                bool orderBySequence = false;
                if (OrderSetting != null)
                {
                    orderBySequence = OrderSetting.FieldName.EqualsOrNullEmpty("Sequence", StringComparison.OrdinalIgnoreCase);
                }
                return orderBySequence;
            }
        }
    }

    [DataContract(Name = "MediaFolder")]
    [KnownTypeAttribute(typeof(MediaFolder))]
    public class MediaFolder : Folder
    {
        public MediaFolder() { }
        public MediaFolder(Repository repository, string fullName) : base(repository, fullName) { }
        public MediaFolder(Repository repository, string name, Folder parent)
            : base(repository, name, parent)
        { }
        public MediaFolder(Repository repository, IEnumerable<string> namePath) : base(repository, namePath) { }

        [DataMember(Order = 5)]
        public string[] AllowedExtensions { get; set; }
    }
}
