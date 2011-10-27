using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Models.Paths;
using System.IO;
using Kooboo.IO;
using Kooboo.Web.Url;
namespace Kooboo.CMS.Content.Persistence.Default
{
    public static class TextContentFileHelper
    {
        public static void StoreFiles(this TextContent content)
        {
            var schema = content.GetSchema();
            if (content.ContentFiles != null)
            {
                schema = schema.AsActual();
                foreach (var stream in content.ContentFiles)
                {
                    var column = schema[stream.Name];
                    if (column != null)
                    {
                        var controlType = column.GetControlType();
                        if (controlType != null && controlType.IsFile && stream.Stream.Length > 0 && !string.IsNullOrEmpty(stream.FileName))
                        {
                            var extension = Path.GetExtension(stream.FileName);
                            var fileName = Kooboo.Extensions.StringExtensions.NormalizeUrl(Path.GetFileNameWithoutExtension(stream.FileName)) + extension;
                            TextContentPath contentPath = new TextContentPath(content);
                            string filePath = Path.Combine(contentPath.PhysicalPath, fileName);
                            stream.Stream.SaveAs(filePath, true);

                            var filVirtualPath = UrlUtility.Combine(contentPath.VirtualPath, fileName);
                            content[column.Name] = controlType.GetValue(content[column.Name] == null ? "" : content[column.Name].ToString(), filVirtualPath);
                        };

                    }
                }
            }
        }

        public static void DeleteFiles(this TextContent content)
        {
            var contentPath = new TextContentPath(content);
            try
            {
                if (Directory.Exists(contentPath.PhysicalPath))
                {
                    IOUtility.DeleteDirectory(contentPath.PhysicalPath, true);
                }
            }
            catch (Exception e)
            {
                Kooboo.HealthMonitoring.Log.LogException(e);
            }

        }
    }
}
