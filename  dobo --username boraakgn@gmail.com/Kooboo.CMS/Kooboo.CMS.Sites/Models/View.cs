using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Kooboo.CMS.Sites.Models
{
    public class Parameter
    {
        public string Name { get; set; }
        public Data.DataType DataType { get; set; }
        public object Value { get; set; }
    }
    [DataContract]
    public class View : Template, IInheritable<View>
    {
        public View() { }
        public View(Site site, string name)
            : base(site, name)
        {
        }
        public View(string physicalPath)
            : base(physicalPath)
        {
        }
        protected override string TemplatePathName
        {
            get { return "Views"; }
        }

        private string fileExtension = string.Empty;
        [DataMember(Order = 1)]
        public override string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(fileExtension))
                {
                    return Kooboo.CMS.Sites.View.TemplateEngines.GetEngineByName(this.EngineName).GetFileExtensionForView();
                }
                return fileExtension;
            }
            set
            {
                fileExtension = value;
            }
        }

        private List<DataRuleSetting> dataRules = new List<DataRuleSetting>();
        [DataMember(Order = 2)]
        public List<DataRuleSetting> DataRules
        {
            get
            {
                if (dataRules == null)
                {
                    dataRules = new List<DataRuleSetting>();
                }
                return dataRules;
            }
            set
            {
                dataRules = value;
            }
        }

        private List<string> plugins = new List<string>();
        [DataMember(Order = 27)]//
        public List<string> Plugins
        {
            get
            {
                return plugins;
            }
            set
            {
                plugins = value;
            }
        }

        private List<Parameter> parameters = new List<Parameter>();
        [DataMember(Order = 29)]//
        public List<Parameter> Parameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        #region IInheritable<View> Members

        public View LastVersion()
        {
            var lastVersion = this;
            while (!lastVersion.Exists())
            {
                if (lastVersion.Site.Parent == null)
                {
                    break;
                }
                lastVersion = new View(lastVersion.Site.Parent, this.Name);
            }
            return lastVersion;
        }     
        public bool HasParentVersion()
        {
            var parentSite = this.Site.Parent;
            while (parentSite != null)
            {
                var view = new View(parentSite, this.Name);
                if (view.Exists())
                {
                    return true;
                }
                parentSite = parentSite.Parent;
            }
            return false;
        }
        #endregion
    }
}
