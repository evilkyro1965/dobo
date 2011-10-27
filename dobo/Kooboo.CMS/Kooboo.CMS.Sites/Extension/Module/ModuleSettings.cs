﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Runtime.Serialization;

namespace Kooboo.CMS.Sites.Extension.Module
{
    [KnownType(typeof(string[]))]
    public class Entry
    {

        public string Controller { get; set; }

        public string Action { get; set; }

        public RouteValueDictionary Values { get; set; }
    }

    public class ModuleSettings
    {
        public ModuleSettings()
        {
            CustomSettings = new Dictionary<string, string>();
        }

        public string ThemeName { get; set; }

        public Entry Entry { get; set; }

        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
