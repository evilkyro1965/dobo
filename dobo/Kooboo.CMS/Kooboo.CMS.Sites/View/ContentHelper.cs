﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Services;

namespace Kooboo.CMS.Sites.View
{
    public class ContentHelper
    {
        public static MediaFolder MediaFolder(string folderName)
        {
            return new MediaFolder(Repository.Current, folderName);
        }
        [Obsolete]
        public static MediaFolder NewMediaFolderObject(string folderName)
        {
            return MediaFolder(folderName);
        }
        
        public static TextFolder TextFolder(string folderName)
        {
            return new TextFolder(Repository.Current, folderName);
        }
        
        [Obsolete]
        public static TextFolder NewTextFolderObject(string folderName)
        {
            return TextFolder(folderName);
        }
        
        public static Schema Schema(string schemaName)
        {
            return new Schema(Repository.Current, schemaName);
        }
        [Obsolete]
        public static Schema NewSchemaObject(string schemaName)
        {
            return Schema(schemaName);
        }
        public static IEnumerable<string> SplitMultiFiles(string files)
        {
            if (string.IsNullOrEmpty(files))
            {
                return new string[0];
            }
            return files.Split('|').Select(it => Kooboo.Web.Url.UrlUtility.ResolveUrl(it)).ToArray();
        }

        /*Bora Akgün 27.10.2011*/
        ///<summary>
        ///</summary>
        ///<param name="folderName"></param>
        ///<param name="uuid"></param>
        ///<returns></returns>
        public static List<CategoryContents> GetCategories(string folderName, string uuid)
        {
            return ServiceFactory.TextContentManager.QueryCategories(Repository.Current, folderName, uuid).ToList();
        }
    }
}
