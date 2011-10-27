using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.CMS.Content.Models.Paths;

namespace Kooboo.CMS.Content.Models
{
    public static class ContentExtensions
    {
        public static Repository GetRepository(this ContentBase content)
        {
            return new Repository(content.Repository);
        }
        public static TextFolder GetFolder(this TextContent content)
        {
            return new TextFolder(content.GetRepository(), FolderHelper.SplitFullName(content.FolderName));
        }
        public static MediaFolder GetFolder(this MediaContent content)
        {
            return new MediaFolder(content.GetRepository(), FolderHelper.SplitFullName(content.FolderName));
        }
        public static Schema GetSchema(this TextContent textContent)
        {
            return new Schema(textContent.GetRepository(), textContent.SchemaName);
        }
        public static string GetSummary(this ContentBase content)
        {
            if (content is TextContent)
            {
                var textContent = (TextContent)content;
                var schema = textContent.GetSchema().AsActual();
                var summarizeField = schema.GetSummarizeColumn();
                if (summarizeField != null && content.ContainsKey(summarizeField.Name))
                {
                    return content[summarizeField.Name] == null ? "" : content[summarizeField.Name].ToString();
                }
                return "";
            }
            else
            {
                var mediaContent = (MediaContent)content;
                return mediaContent.FileName;
            }
        }
        public static bool Exist(this MediaContent content)
        {
            var contentPath = new MediaContentPath(content);
            return File.Exists(contentPath.PhysicalPath);
        }
    }
}
