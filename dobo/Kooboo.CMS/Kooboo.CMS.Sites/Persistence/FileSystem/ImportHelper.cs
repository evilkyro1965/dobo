using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using System.IO;
using Ionic.Zip;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
    public  static class ImportHelper
    {
        public static void Export(IEnumerable<PathResource> sources, Stream outputStream)
        {
            using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
            {

                foreach (var item in sources)
                {
                    if (item.Exists())
                    {
                        if (item is DirectoryResource)
                        {
                            zipFile.AddDirectory(item.PhysicalPath, item.Name);
                        }
                        else
                        {
                            zipFile.AddFile(item.PhysicalPath, "");
                        }
                    }                    
                }

                zipFile.Save(outputStream);
            }
        }

        public static void Import(Site site, string destDir, Stream zipStream, bool @override)
        {
            using (ZipFile zipFile = ZipFile.Read(zipStream, Encoding.UTF8))
            {
                ExtractExistingFileAction action = ExtractExistingFileAction.DoNotOverwrite;
                if (@override)
                {
                    action = ExtractExistingFileAction.OverwriteSilently;
                }
                zipFile.ExtractAll(destDir, action);
            }
        }
    }
}
