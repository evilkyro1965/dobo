using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using System.IO;

namespace Kooboo.CMS.Content.Persistence
{
    public interface ISchemaProvider : IProvider<Schema>, IImportProvider<Schema>
    {
        void Initialize(Schema schema);

        Schema Create(Repository repository, string schemaName, Stream templateStream);

        Schema Copy(Repository repository, string sourceName, string destName);
    }
}