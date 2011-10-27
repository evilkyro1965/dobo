using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.Web.Url;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Kooboo.CMS.Content.Models
{
    [DataContract]
    [Serializable]
    public class Repository : IPersistable
    {
        public static Repository Current
        {
            get
            {
                return CallContext.Current.GetObject<Repository>("Current_Repository");
            }
            set
            {
                CallContext.Current.RegisterObject("Current_Repository", value);
            }
        }
        public Repository()
        {
        }

        public Repository(string name)
        {
            this.Name = name;
        }

        [XmlIgnore]
        public string Name { get; set; }
        [DataMember(Order = 3)]
        public string DisplayName { get; set; }
        [DataMember(Order = 7)]
        public string DBProvider { get; set; }

        [DataMember(Order = 9)]
        public bool EnableBroadcasting { get; set; }

        [DataMember(Order = 11)]
        public bool EnableCustomTemplate { get; set; }
        //[DataMember(Order = 13)]
        [Obsolete]
        public bool EnableManuallyUserKey { get; set; }
        private string userKeyReplacePattern;
        [DataMember(Order = 15)]
        public string UserKeyReplacePattern
        {
            get
            {
                if (string.IsNullOrEmpty(userKeyReplacePattern))
                {
                    return "[^\\w\\d_-]+";
                }
                return userKeyReplacePattern;
            }
            set
            {
                if (value != "[ ;@=$,#%&*{}\\:<>?/+\"\'!|]{1,}")
                {
                    userKeyReplacePattern = value;
                }
                
            }
        }
        private string userKeyHyphens;
        [DataMember(Order = 17)]
        public string UserKeyHyphens
        {
            get
            {
                if (string.IsNullOrEmpty(userKeyHyphens))
                {
                    return "-";
                }
                return userKeyHyphens;
            }
            set
            {
                userKeyHyphens = value;
            }
        }
        private bool? enableVersioning = true;
        [DataMember(Order = 19)]
        public bool? EnableVersioning
        {
            get
            {
                if (enableVersioning == null)
                {
                    enableVersioning = true;
                }
                return enableVersioning;
            }
            set
            {
                enableVersioning = value;
            }
        }

        [DataMember(Order = 20)]
        public bool EnableWorkflow { get; set; }
        //#region path
        //const string ROOT_PATH = "ContentRepositories";
        //public string PhysicalPath
        //{
        //    get
        //    {
        //        return Path.Combine(ROOT_PATH, this.Name);
        //    }
        //}
        //public string VirtualPath
        //{
        //    get
        //    {
        //        return UrlUtility.Combine("~/", ROOT_PATH, this.Name);
        //    }
        //}
        //#endregion

        #region IPersistable Members
        bool isDummy = true;
        public bool IsDummy
        {
            get { return isDummy; }
        }

        void IPersistable.Init(IPersistable source)
        {
            isDummy = false;
            this.Name = ((Repository)source).Name;
        }

        void IPersistable.OnSaved()
        {

        }

        void IPersistable.OnSaving()
        {

        }

        #endregion

        #region override object
        public static bool operator ==(Repository obj1, Repository obj2)
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
        public static bool operator !=(Repository obj1, Repository obj2)
        {
            return !(obj1 == obj2);
        }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var repository = (Repository)obj;
                if (string.Compare(this.Name, repository.Name, true) == 0)
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
            return this.Name;
        }
        #endregion
    }
}
