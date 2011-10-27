using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

namespace Kooboo.CMS.Sites.Persistence
{
	public interface IScriptProvider : IProvider<ScriptFile>, IImportProvider<ScriptFile>
	{
		void SaveOrders(Site site, IEnumerable<string> filesOrder);
		//IEnumerable<ScriptFile> All(Site site);
	}
}
