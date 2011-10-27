using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Sites.Models;

using System.IO;
using System.ComponentModel.Composition;
using Kooboo.CMS.Sites.Services;

namespace Kooboo.CMS.Sites.Persistence.FileSystem
{
	[Export(typeof(IScriptProvider))]
	public class ScriptProvider : IScriptProvider
	{
		static System.Threading.ReaderWriterLockSlim locker = new System.Threading.ReaderWriterLockSlim();

		public IQueryable<ScriptFile> All(Site site)
		{
			return AllEnumerable(site).AsQueryable();
		}
		public IEnumerable<ScriptFile> AllEnumerable(Site site)
		{
			var baseDir = GetScriptBasePath(site);
			if (Directory.Exists(baseDir))
			{
				var fileNames = EnumerateScripts(baseDir);

				fileNames = FileOrderHelper.OrderFiles(baseDir, fileNames);

				return fileNames.Select(it => new ScriptFile(site, it));
			}
			return new ScriptFile[0];
		}
		private IEnumerable<string> EnumerateScripts(string baseDir)
		{
			foreach (var file in Directory.EnumerateFiles(baseDir, "*.js"))
			{
				yield return Path.GetFileName(file);
			}
		}

		protected void Save(ScriptFile item)
		{
			item.Save();
		}

		#region IRepository<ScriptFile> Members


		public ScriptFile Get(ScriptFile dummy)
		{
			throw new NotImplementedException();
		}

		public void Add(ScriptFile item)
		{
			Save(item);
		}

		public void Update(ScriptFile @new, ScriptFile old)
		{
			Save(@new);
		}

		public void Remove(ScriptFile item)
		{
			if (item.Exists())
			{
				item.Delete();
			}
		}

		#endregion

		#region IImportRepository Members

		public void Export(IEnumerable<ScriptFile> sources, System.IO.Stream outputStream)
		{
			ImportHelper.Export(sources, outputStream);
		}
		public void Import(Site site, string destDir, System.IO.Stream zipStream, bool @override)
		{
			ImportHelper.Import(site, destDir, zipStream, @override);
		}
		#endregion

		public void SaveOrders(Site site, IEnumerable<string> filesOrder)
		{
			var baseDir = GetScriptBasePath(site);
			FileOrderHelper.SaveFilesOrder(baseDir, filesOrder);

		}

		private string GetScriptBasePath(Site site)
		{
			ScriptFile dummy = ModelActivatorFactory<ScriptFile>.GetActivator().CreateDummy(site);
			return dummy.BasePhysicalPath;
		}
	}
}
