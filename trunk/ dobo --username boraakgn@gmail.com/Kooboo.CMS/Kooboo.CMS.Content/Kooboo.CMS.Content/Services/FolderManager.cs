using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence;
using System.IO;
using Kooboo.CMS.Content.Models.Paths;
using Kooboo.Extensions;

namespace Kooboo.CMS.Content.Services
{
    public class FolderTreeNode<T>
        where T : Folder
    {
        public FolderTreeNode()
        {

        }
        public T Folder { get; set; }

        public IEnumerable<FolderTreeNode<T>> Children { get; internal set; }

    }
    public class FolderManager<T> : ManagerBase<T>
        where T : Folder
    {

        public override T Get(Repository repository, string fullName)
        {
            return FolderHelper.Parse<T>(repository, fullName);
        }

        public IEnumerable<T> All(Repository repository, string filterName, string fullName)
        {
            if (!string.IsNullOrEmpty(fullName))
            {
                var parent = FolderHelper.Parse<T>(repository, fullName);
                return ChildFolders(parent, filterName);
            }
            var result = All(repository, filterName);
            return result;
        }
        public override IEnumerable<T> All(Models.Repository repository, string filterName)
        {
            var r = GetDBProvider().All(repository).Select(it => it.AsActual());
            if (!string.IsNullOrEmpty(filterName))
            {
                r = r.Where(it => it.Name.Contains(filterName, StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrEmpty(it.DisplayName) && it.DisplayName.Contains(filterName, StringComparison.OrdinalIgnoreCase)));
            }

            return r;
        }
        public IEnumerable<T> ChildFolders(T parent)
        {
            return ChildFolders(parent, "");
        }
        public IEnumerable<T> ChildFolders(T parent, string filterName)
        {
            var r = ((IFolderProvider<T>)this.GetDBProvider()).ChildFolders(parent).Select(it => it.AsActual());
            if (!string.IsNullOrEmpty(filterName))
            {
                r = r.Where(it => it.Name.Contains(filterName, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(it.DisplayName) && it.DisplayName.Contains(filterName, StringComparison.OrdinalIgnoreCase)));
            }
            return r;
        }

        public void Import(Repository repository, Stream stream, string fullName, bool @override)
        {
            var baseDir = FolderPath.GetBaseDir<T>(repository);
            if (!string.IsNullOrEmpty(fullName))
            {
                var folder = FolderHelper.Parse<T>(repository, fullName);
                FolderPath folderPath = new FolderPath(folder);
                baseDir = folderPath.PhysicalPath;
            }
            ((IImportProvider<Folder>)GetDBProvider()).Import(repository, baseDir, stream, @override);
        }

        public void Export(Repository repository, Stream stream, IEnumerable<Folder> model)
        {
            foreach (var item in model)
            {
                item.Repository = repository;
            }
            ((IImportProvider<Folder>)GetDBProvider()).Export(repository, model, stream);
        }

        public IEnumerable<FolderTreeNode<T>> FolderTrees(Repository repository)
        {
            return All(repository, "").Select(it => GetFolderTreeNode(it));
        }
        private FolderTreeNode<T> GetFolderTreeNode(T folder)
        {
            FolderTreeNode<T> treeNode = new FolderTreeNode<T>() { Folder = folder };
            treeNode.Children = ChildFolders(folder)
                .Select(it => GetFolderTreeNode(it));
            return treeNode;
        }
    }
}
