using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Persistence;

namespace Kooboo.CMS.Content.Models
{
    public static class IPersistableExtensions
    {
        public static Repository AsActual(this Repository repository)
        {
            if (repository.IsDummy)
            {
                repository = Providers.RepositoryProvider.Get(repository);
            }
            return repository;
        }
        public static Schema AsActual(this Schema schema)
        {
            if (schema.IsDummy)
            {
                schema = Providers.DefaultProviderFactory.GetProvider<ISchemaProvider>().Get(schema);
            }
            return schema;
        }
        public static T AsActual<T>(this T folder)
            where T : Folder
        {
            if (folder.IsDummy)
            {
                if (folder is MediaFolder)
                {
                    folder.IsDummy = false;
                    return folder;
                }
                else
                {
                    folder = Providers.DefaultProviderFactory.GetProvider<ITextFolderProvider>().Get((TextFolder)(object)folder) as T;
                }

            }
            return folder;
        }

        public static Workflow AsActual(this Workflow workflow)
        {
            return Providers.DefaultProviderFactory.GetProvider<IWorkflowProvider>().Get(workflow);
        }
    }
}
