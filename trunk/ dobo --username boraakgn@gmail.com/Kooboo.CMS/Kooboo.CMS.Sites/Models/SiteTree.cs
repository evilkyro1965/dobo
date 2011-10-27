﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.CMS.Sites.Models
{
    public class SiteNode
    {
        public Site Site { get; set; }
        public bool IsOnLine
        {
            get
            {
                return Services.ServiceFactory.SiteManager.IsOnline(Site.Name);
            }
        }
        public IEnumerable<SiteNode> Children { get; set; }
    }
    public class SiteTree
    {
        public SiteNode Root { get; set; }
    }
}
