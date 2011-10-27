using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Kooboo.CMS.Account.Models
{
    [DataContract]
    public class User : IPersistable
    {
        public string UserName { get; set; }

        [DataMember(Order = 1)]
        public string Email { get; set; }

        [DataMember(Order = 3)]
        public string Password { get; set; }

        [DataMember(Order = 5)]
        public bool IsAdministrator { get; set; }

        [DataMember(Order = 6)]
        public string UICulture { get; set; }


        [DataMember(Order = 7)]
        public Dynamic.DynamicDictionary CustomFields { get; set; }

        [XmlIgnore]
        public string CustomFieldsXml
        {
            get
            {
                string xml = "";
                if (CustomFields != null)
                {
                    xml = Kooboo.Runtime.Serialization.DataContractSerializationHelper.SerializeAsXml(this.CustomFields);
                }
                return xml;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    CustomFields = Kooboo.Runtime.Serialization.DataContractSerializationHelper.DeserializeFromXml<Dynamic.DynamicDictionary>(value);
                }
            }
        }
        #region IPersistable

        private bool isDummy = true;
        public bool IsDummy
        {
            get { return isDummy; }
            set { isDummy = value; }
        }

        public void Init(IPersistable source)
        {
            this.UserName = ((User)source).UserName;
            isDummy = false;
        }

        public void OnSaved()
        {
            isDummy = false;
        }

        public void OnSaving()
        {

        }

        #endregion
    }
}
