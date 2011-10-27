
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Sites;
using Kooboo.CMS.Web.Areas.Contents.Models;
using Kooboo.Web.Mvc;

namespace Kooboo.CMS.Web.Areas.Contents.Controllers
{
    [Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Contents", Group = "", Name = "Broadcasting", Order = 1)]
    public class ReceivingSettingController : ManagerControllerBase
    {
        //
        // GET: /Contents/ReceiveSetting/

        private ReceivingSettingManager Manager
        {
            get
            {
                return ServiceFactory.ReceiveSettingManager;
            }
        }

        public ActionResult Index(string search)
        {
            var model = Manager.All(Repository, search).Select(o => Manager.Get(Repository, o.Name));
            return View(model);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ReceivingSetting model, string repository)
        {
            var entry = new JsonResultEntry();
            if (ModelState.IsValid)
            {
                try
                {
                    model.Name = Kooboo.UniqueIdGenerator.GetInstance().GetBase32UniqueId(10);
                    Manager.Add(Repository, model);
                    entry.SetSuccess();
                }
                catch (Exception e)
                {
                    entry.AddException(e);
                }
            }
            else
            {
                entry.AddModelState(ModelState);
            }

            return Json(entry);
        }

        public ActionResult Edit(string name)
        {
            var model = Manager.Get(Repository, name);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ReceivingSetting model)
        {
            var entry = new JsonResultEntry();
            if (ModelState.IsValid)
            {
                try
                {
                    model.Repository = Repository;
                    Manager.Update(Repository, model, model);
                    entry.SetSuccess();
                }
                catch (Exception e)
                {
                    entry.AddException(e);
                }
            }
            else
            {
                entry.AddModelState(ModelState);
            }
            return Json(entry);
        }

        public ActionResult Delete(ReceivingSetting[] Model)
        {
            var entry = new JsonResultEntry();

            try
            {
                if (Model != null)
                {
                    foreach (var m in Model)
                    {
                        m.Repository = Repository;
                        Manager.Remove(Repository, m);
                    }
                }
            }
            catch (Exception e)
            {
                entry.AddException(e);
            }
            return Json(entry);
        }

        public ActionResult GetFolderDataSource(string repository)
        {           
            var sourceRepository = new Repository(repository);

            var settingFolders = ServiceFactory.SendingSettingManager.All(sourceRepository, "");

            var items = settingFolders.Select(it => ServiceFactory.SendingSettingManager.Get(it.Repository, it.Name))
                .Select(it => new SelectListItemTree()
                {
                    Text = it.FolderName,
                    Value = it.FolderName
                }).ToArray();

            return Json(items);
        }
            
    }
}
