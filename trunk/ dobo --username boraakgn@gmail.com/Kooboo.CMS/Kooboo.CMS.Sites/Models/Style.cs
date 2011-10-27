using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class Style : ThemeFile
    {
        public Style() { }
        public Style(string physicalPath)
            : base(physicalPath)
        {
        }
        public Style(Theme theme, string fileName)
            : base(theme, fileName)
        {
        }
        public override string FileExtension
        {
            get
            {
                return ".css";
            }
            set
            {
                
            }
        }
    }
}
