
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Specialized;

namespace Kooboo.CMS.Content.Models
{
    [DataContract]
    public class SendingSetting : IRepositoryElement, IPersistable
    {
        public SendingSetting()
        { }

        public SendingSetting(Repository repository, string folderName)
        {
            this.Repository = repository;
            this.FolderName = FolderName;
        }
        public Repository Repository { get; set; }

        public string Name
        {
            get
            {
                return this.FolderName;
            }
            set
            {
                this.FolderName = value;
            }
        }

        //[DataMember(Order = 2)]
        public string FolderName { get; set; }

        [DataMember(Order=1)]
        public bool? SendReceived { get; set; }

        //[DataMember(Order = 4)]
        //public bool? Published { get; set; }

        //[DataMember(Order = 6)]
        //public ContentAction AcceptAction { get; set; }


        #region IPersistable
        private bool isDummy = true;
        bool IPersistable.IsDummy
        {
            get { return isDummy; }
        }

        void IPersistable.Init(IPersistable source)
        {
            isDummy = false;
            this.Repository = ((SendingSetting)source).Repository;
            this.FolderName = ((SendingSetting)source).FolderName;
        }

        void IPersistable.OnSaved()
        {
            isDummy = false;
        }

        void IPersistable.OnSaving()
        {
        }
        #endregion
    }
}
