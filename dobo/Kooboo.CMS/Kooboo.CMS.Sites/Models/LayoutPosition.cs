﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Kooboo.CMS.Sites.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class LayoutPosition
    {
        [DataMember(Order = 1)]
        public string ID { get; set; }
        //[DataMember(Order = 2)]
        //public ViewCompatibility Compatibility { get; set; }
    }
}
