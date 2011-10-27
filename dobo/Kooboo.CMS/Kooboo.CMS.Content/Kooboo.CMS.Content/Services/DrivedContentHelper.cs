using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using System.Collections.Specialized;
using Kooboo.CMS.Content.Query;

namespace Kooboo.CMS.Content.Services
{
    public static class DrivedContentHelper
    {
        private static List<string> contentBasicFields = new List<string>()
            {"Id",
            "UUID",
            "Repository",
            "FolderName",
            "UserKey",
            "UtcCreationDate",
            "UtcLastModificationDate",
            "Published",
            "OriginalUUID",
            "OriginalRepository",
            "OriginalFolder",
            "IsLocalized"
            };        
        public static NameValueCollection ExcludeBasicFields(ContentBase content)
        {
            NameValueCollection values = new NameValueCollection();
            foreach (var key in content.Keys.Where(key => !contentBasicFields.Contains(key, StringComparer.CurrentCultureIgnoreCase)))
            {
                if (content[key] != null)
                {
                    values[key] = content[key].ToString();
                }
            }
            return values;
        }
        public static IEnumerable<TextContent> GetContentsByOriginalUUID(TextFolder folder, string originalUUID)
        {
            return folder.CreateQuery().WhereEquals("OriginalUUID", originalUUID);
        }
    }
}
