using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.Persistence;

namespace Kooboo.CMS.Sites.Services
{
	public class ScriptManager : PathResourceManagerBase<ScriptFile>
	{
		public IScriptProvider ScriptRepository
		{
			get
			{
				return (IScriptProvider)base.Provider;
			}
		}
		public override ScriptFile Get(Site site, string name)
		{
			ScriptFile script = new ScriptFile(site, name);
			if (!script.Exists())
			{
				return null;
			}
			script.Body = script.Read();
			return script;
		}
		public void SaveOrder(Site site, IEnumerable<string> fileOrders)
		{
			ScriptRepository.SaveOrders(site, fileOrders);
		}
	}
}
