using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Ionic.Zip;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence.SqlServer
{
    public class SchemaProvider : Default.SchemaProvider
    {

        #region IProvider<Schema> Members

        public override void Update(Models.Schema @new, Models.Schema old)
        {
            base.Update(@new, old);
            SchemaManager.Update(@new, old);
        }

        public override void Remove(Models.Schema item)
        {
            base.Remove(item);
            SchemaManager.Delete(item);
        }

        #endregion

        #region IImportProvider Members

        public override void Import(Models.Repository repository, string destDir, System.IO.Stream zipStream, bool @override)
        {
            List<string> schemaNames = new List<string>();

            using (var zipFile = ZipFile.Read(zipStream, Encoding.UTF8))
            {

                foreach (var entry in zipFile.Entries)
                {
                    if (entry.FileName.IndexOf('/') == entry.FileName.Length - 1)
                    {
                        schemaNames.Add(entry.FileName.Substring(0, entry.FileName.Length - 1));
                    }
                }
            }
            zipStream.Position = 0;
            base.Import(repository, destDir, zipStream, @override);

            foreach (var name in schemaNames)
            {
                Initialize(new Models.Schema(repository, name));
            }
        }

        #endregion

        public override void Initialize(Models.Schema schema)
        {
            SchemaManager.Add(schema);
        }
    }
}
