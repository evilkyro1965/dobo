using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Services;

namespace Kooboo.CMS.Web.Areas.Sites.Controllers
{
	[Kooboo.CMS.Web.Authorizations.Authorization(AreaName = "Sites", Group = "Development", Name = "Script", Order = 1)]
	public class ScriptController : PathResourceControllerBase<ScriptFile, ScriptManager>
	{
		public override ActionResult Edit(string fileName)
		{
			return base.Edit(fileName);
		}
		/// <summary>
		/// for remote validation
		/// </summary>
		/// <param name="name"></param>
		/// <param name="old_Key"></param>
		/// <returns></returns>
		public ActionResult IsNameAvailable(string name, string fileExtension, string old_Key)
		{
			string fileName = name + fileExtension;
			if (old_Key == null || !fileName.EqualsOrNullEmpty(old_Key, StringComparison.CurrentCultureIgnoreCase))
			{
				if (Manager.Get(Site, fileName) != null)
				{
					return Json("The name already exists.", JsonRequestBehavior.AllowGet);
				}
			}
			return Json(true, JsonRequestBehavior.AllowGet);
		}
		public ActionResult Sort(string directoryPath, IEnumerable<string> filesOrder)
		{
			Manager.SaveOrder(Site, filesOrder);
			return null;
		}
	}
}
