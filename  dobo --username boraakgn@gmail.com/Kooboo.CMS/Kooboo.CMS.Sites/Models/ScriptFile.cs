using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Kooboo.CMS.Sites.Models
{
    public class ScriptFile : FileResource
    {
        public ScriptFile() { }
        public ScriptFile(string physicalPath)
            : base(physicalPath)
        {
        }
        public ScriptFile(Site site, string fileName)
            :base(site,fileName)
        { 
            
        }

        public override IEnumerable<string> RelativePaths
        {
            get { return new string[] { "Scripts" }; }
        }

        /// <summary>
        /// Parses the object.
        /// </summary>
        /// <param name="relativePaths">The relative paths. <example>{"site1","scripts","js.js"}</example></param>
        /// <returns>
        /// the remaining paths.<example>{"site1"}</example>
        /// </returns>
        internal override IEnumerable<string> ParseObject(IEnumerable<string> relativePaths)
        {
            //call base return {"site1","scripts"}
            relativePaths = base.ParseObject(relativePaths);

            return relativePaths.Take(relativePaths.Count() - 1);
        }  
    }
}
