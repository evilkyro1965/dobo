using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Kooboo.CMS.Content.Persistence.Default;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence.MongoDB
{ 
    public class ProviderFactory : IProviderFactory
    {
        static Hashtable providers = new Hashtable();
        static ProviderFactory()
        {
            providers.Add(typeof(IRepositoryProvider), new RepositoryProvider());
            providers.Add(typeof(ISchemaProvider), new SchemaProvider());
            providers.Add(typeof(ITextFolderProvider), new TextFolderProvider());
            providers.Add(typeof(IMediaFolderProvider), new MediaFolderProvider());
            //providers.Add(typeof(IReceivedMessageProvider), new ReceivedMessageProvider());
            providers.Add(typeof(IReceivingSettingProvider), new ReceivingSettingProvider());
            providers.Add(typeof(IContentProvider<TextContent>), new TextContentProvider());
            providers.Add(typeof(IContentProvider<MediaContent>), new Kooboo.CMS.Content.Persistence.Default.MediaContentProvider());
            providers.Add(typeof(ISendingSettingProvider), new SendingSettingProvider());

            providers.Add(typeof(IWorkflowProvider), new WorkflowProvider());
            providers.Add(typeof(IWorkflowHistoryProvider), new WorkflowHistoryProvider());
            providers.Add(typeof(IPendingWorkflowItemProvider), new PendingWorkflowItemProvider());
        }

        #region IProviderFactory Members

        public string Name
        {
            get { return "MongoDB"; }
        }

        public T GetProvider<T>()
        {
            foreach (var item in providers.Values)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }
            return default(T);
        }

        #endregion
    }
}
