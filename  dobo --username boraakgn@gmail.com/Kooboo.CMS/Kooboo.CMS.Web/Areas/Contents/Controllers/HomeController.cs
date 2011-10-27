using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kooboo.Web.Mvc;
using Kooboo.Web.Script.Serialization;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Query;
using Kooboo.Extensions;
using Kooboo.CMS.Web.Authorizations;


namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
    [RequiredLogOn]
    public class HomeController : ControllerBase
    {
        //
        // GET: /Content/Home/


        private enum ContentEditStatus
        {
            Disable,
            Enable,
            Done
        }

        private class ContentEditStep
        {
            public ContentEditStatus Status { get; set; }
            public string ActionUrl { get; set; }
        }

        private class ContentMapModel
        {
            public ContentEditStep Repository
            {
                get
                {
                    return repository;
                }
                set
                {
                    repository = value;
                }
            }
            private ContentEditStep repository = new ContentEditStep();

            public ContentEditStep ContentType
            {
                get
                {
                    return contentType;
                }
                set
                {
                    contentType = value;
                }
            }
            private ContentEditStep contentType = new ContentEditStep();

            public ContentEditStep TextFolder
            {
                get
                {
                    return textFolder;
                }
                set
                {
                    textFolder = value;
                }
            }
            private ContentEditStep textFolder = new ContentEditStep();

            public ContentEditStep TextContent
            {
                get
                {
                    return textContent;
                }
                set
                {
                    textContent = value;
                }
            }
            private ContentEditStep textContent = new ContentEditStep();

            public ContentEditStep MediaContent
            {
                get
                {
                    return mediaContent;
                }
                set
                {
                    mediaContent = value;
                }
            }
            private ContentEditStep mediaContent = new ContentEditStep();
        }

        public ActionResult Index()
        {
            var repositoryName = this.Request.RequestContext.GetRequestValue("repositoryName");//ControllerContext.RouteData.Values["repositoryName"];

            ContentMapModel mapModel = new ContentMapModel();

            if (string.IsNullOrEmpty(repositoryName))
            {
                mapModel.Repository = new ContentEditStep
                {
                    Status = ContentEditStatus.Enable,
                    ActionUrl = this.Url.Action("Create", "Repository")
                };
            }
            else
            {
                var allRequestValue = Request.RequestContext.AllRouteValues();

                var repository = ServiceFactory.RepositoryManager.Get(repositoryName);


                mapModel.Repository.Status = ContentEditStatus.Disable;
                var schema = ServiceFactory.SchemaManager.All(repository, "");
                if (CMS.Sites.Services.ServiceFactory.UserManager.Authorize(CMS.Sites.Models.Site.Current, User.Identity.Name, CMS.Account.Models.Permission.Contents_SchemaPermission))
                {
                    mapModel.ContentType.Status = ContentEditStatus.Done;

                    mapModel.ContentType.ActionUrl = this.Url.Action("Index", "Schema", allRequestValue);
                }
                else
                {
                    mapModel.ContentType.Status = ContentEditStatus.Disable;
                }

                if (schema != null && schema.Count() > 0)
                {
                    mapModel.TextFolder.Status = ContentEditStatus.Enable;
                    mapModel.TextFolder.ActionUrl = this.Url.Action("Index", "TextFolder", allRequestValue);
                }
                else
                {
                    mapModel.ContentType.Status = ContentEditStatus.Enable;
                }

                var textFolder = ServiceFactory.TextFolderManager.All(repository, null);

                if (textFolder != null && textFolder.Count() > 0)
                {
                    if (CMS.Sites.Services.ServiceFactory.UserManager.Authorize(CMS.Sites.Models.Site.Current, User.Identity.Name, CMS.Account.Models.Permission.Contents_ContentPermission))
                    {
                        mapModel.TextFolder.Status = ContentEditStatus.Done;
                        mapModel.TextContent.Status = ContentEditStatus.Enable;

                        mapModel.TextContent.ActionUrl = Url.Action("Index", "TextFolder", allRequestValue);
                    }
                }
                if (CMS.Sites.Services.ServiceFactory.UserManager.Authorize(CMS.Sites.Models.Site.Current, User.Identity.Name, CMS.Account.Models.Permission.Contents_ContentPermission))
                {
                    mapModel.MediaContent.Status = ContentEditStatus.Enable;
                    mapModel.MediaContent.ActionUrl = Url.Action("Index", "MediaContent", allRequestValue);
                }

                var mediaFolder = ServiceFactory.MediaFolderManager.All(repository, null);

                foreach (var t in textFolder)
                {
                    if (!string.IsNullOrEmpty(t.SchemaName))
                    {
                        var textContent = t.CreateQuery();
                        if (textContent != null && textContent.Count() > 0)
                        {
                            mapModel.TextContent.Status = ContentEditStatus.Done;
                            break;
                        }
                    }
                }

                foreach (var t in mediaFolder)
                {
                    var mediaContent = t.CreateQuery();
                    if (mediaContent != null && mediaContent.Count() > 0)
                    {
                        mapModel.MediaContent.Status = ContentEditStatus.Done;
                        break;
                    }
                }


            }



            return View(mapModel);
        }

    }
}
