using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.Globalization;
namespace Kooboo.CMS.Content.Models
{
    public static class FolderExtensions
    {
        public static Schema GetSchema(this TextFolder folder)
        {
            var schemaName = folder.AsActual().SchemaName;
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new KoobooException(string.Format("The folder of '{0}' is not a content folder.".Localize(), folder.FriendlyName));
            }
            return new Schema(folder.Repository, schemaName);
        }
    }
}
