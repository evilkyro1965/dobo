using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Kooboo.CMS.Content.Models
{
    public interface IPersistable
    {
        bool IsDummy { get; }
        void Init(IPersistable source); 
        void OnSaved();  
        void OnSaving();
    }
}
