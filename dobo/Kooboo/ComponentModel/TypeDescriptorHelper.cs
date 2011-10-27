using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Threading;

namespace Kooboo.ComponentModel
{
    public static class TypeDescriptorHelper
    {
        static Hashtable hashtable = new Hashtable();
        static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        static TypeDescriptorHelper()
        {

        }
        public static void RegisterMetadataType(Type type, Type metadataType)
        {
            locker.EnterWriteLock();

            hashtable[type] = metadataType;

            locker.ExitWriteLock();
        }
        public static ICustomTypeDescriptor Get(Type type)
        {
            try
            {
                locker.EnterReadLock();
                var metadataType = hashtable[type] as Type;
                ICustomTypeDescriptor descriptor = null;
                if (metadataType != null)
                {
                    descriptor = new AssociatedMetadataTypeTypeDescriptionProvider(type, metadataType).GetTypeDescriptor(type);
                }
                
                return descriptor;
            }
            finally
            {               
                locker.ExitReadLock();
            }
            
            
        }
    }
}
