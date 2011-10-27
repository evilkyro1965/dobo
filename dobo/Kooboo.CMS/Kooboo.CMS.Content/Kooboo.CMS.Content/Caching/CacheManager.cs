using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
namespace Kooboo.CMS.Content.Caching
{
    public class CachedNullValue
    {
        public CachedNullValue()
        {

        }
        public volatile static CachedNullValue Value = new CachedNullValue();

        public override bool Equals(object obj)
        {
            return true;
        }
        public override int GetHashCode()
        {
            return 0;
        }
        public static bool operator ==(CachedNullValue obj1, CachedNullValue obj2)
        {
            return true;
        }
        public static bool operator !=(CachedNullValue obj1, CachedNullValue obj2)
        {
            return false;
        }
    }
    public abstract class CacheManager
    {
        public abstract ObjectCache GetObjectCache(string cacheName);

        protected abstract void RemoveObjectCache(string cacheName);

        public virtual void Clear(string cacheName)
        {
            RemoveObjectCache(cacheName);
        }

        public abstract ObjectCache GlobalObjectCache();
        public abstract void ClearGlobalObjectCache();
    }
}
