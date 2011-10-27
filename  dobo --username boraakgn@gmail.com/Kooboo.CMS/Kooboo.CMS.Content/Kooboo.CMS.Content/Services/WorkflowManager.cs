﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence;
using Kooboo.CMS.Content.Query;
namespace Kooboo.CMS.Content.Services
{
    public class WorkflowManager : ManagerBase<Workflow>, IManager<Workflow>
    {
        IWorkflowProvider WorkflowProvider
        {
            get
            {
                return (IWorkflowProvider)this.GetDBProvider();
            }
        }
        IPendingWorkflowItemProvider PendingWorkflowItemProvider
        {
            get
            {
                return (IPendingWorkflowItemProvider)Providers.DefaultProviderFactory.GetProvider<IPendingWorkflowItemProvider>();
            }
        }
        public IWorkflowHistoryProvider WorkflowHistoryProvider
        {
            get
            {
                return (IWorkflowHistoryProvider)Providers.DefaultProviderFactory.GetProvider<IWorkflowHistoryProvider>();
            }
        }

        #region CRUD

        public IEnumerable<Workflow> All(Repository repository)
        {
            return WorkflowProvider.All(repository);
        }

        public override Workflow Get(Repository repository, string name)
        {
            Workflow workflow = new Workflow(repository, name);
            return WorkflowProvider.Get(workflow);
        }

        public void Delete(Workflow[] workflows, Repository repository)
        {
            foreach (var w in workflows)
            {
                w.Repository = repository;
                this.Remove(repository, w);
            }
        }

        #endregion

        public void StartWorkflow(Repository repository, string workflowName, TextContent content, string userName)
        {
            var workflow = Get(repository, workflowName);
            if (workflow.Items != null && workflow.Items.Length > 0)
            {
                var roles = GetRoles(userName);
                var workflowItem = workflow.Items.Where(it => roles.Any(r => r.EqualsOrNullEmpty(it.RoleName, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
                if (workflowItem != null)
                {
                    CreatePendingWorkflowItem(repository, content, userName, workflow, workflowItem, "");
                }
            }
        }


        #region PendingWorkflowItems
        public PendingWorkflowItem GetPendingWorkflowItem(Repository repository, string roleName, string id)
        {
            return PendingWorkflowItemProvider.Get(new PendingWorkflowItem { Repository = repository, RoleName = roleName, Name = id });
        }

        public IEnumerable<PendingWorkflowItem> GetPendingWorkflowItems(Repository repository, string userName)
        {
            var roles = GetRoles(userName);
            return roles.Select(it => PendingWorkflowItemProvider.All(repository, it)).SelectMany(it => it).ToArray();
        }

        private void CreatePendingWorkflowItem(Repository repository, TextContent content, string userName, Workflow workflow, WorkflowItem workflowItem, string previousComment)
        {
            PendingWorkflowItem pendingWorkflowItem = new PendingWorkflowItem()
            {
                Repository = repository,
                Name = content.UUID,
                WorkflowName = workflow.Name,
                WorkflowItemSequence = workflowItem.Sequence,
                ItemDisplayName = workflowItem.DisplayName,
                RoleName = workflowItem.RoleName,
                ContentFolder = content.FolderName,
                ContentUUID = content.UUID,
                ContentSummary = content.GetSummary(),
                CreationUtcDate = DateTime.UtcNow,
                CreationUser = userName,
                PreviousComment = previousComment
            };
            PendingWorkflowItemProvider.Add(pendingWorkflowItem);
        }

        public void ProcessPendingWorkflowItem(Repository repository, string workflowName, string roleName, string pendingWorkflowItemId, string userName, bool passed, string comment)
        {
            var pendingItem = PendingWorkflowItemProvider.Get(new PendingWorkflowItem() { RoleName = roleName, Name = pendingWorkflowItemId, Repository = repository });
            if (pendingItem != null)
            {
                var content = new TextFolder(repository, pendingItem.ContentFolder).CreateQuery().WhereEquals("UUID", pendingItem.ContentUUID).FirstOrDefault();
                if (content != null)
                {
                    var workflow = Get(repository, workflowName);
                    bool finished = false;
                    if (workflow != null)
                    {
                        WorkflowItem nextWorkflowItem = null;
                        if (passed)
                        {
                            nextWorkflowItem = GetNextWorkflowItem(repository, workflow, pendingItem.WorkflowItemSequence + 1);
                            if (nextWorkflowItem == null)
                            {
                                finished = true;
                            }
                        }
                        else
                        {
                            nextWorkflowItem = GetNextWorkflowItem(repository, workflow, pendingItem.WorkflowItemSequence - 1);
                        }
                        if (nextWorkflowItem != null)
                        {
                            CreatePendingWorkflowItem(repository, content, userName, workflow, nextWorkflowItem, comment);
                        }
                    }

                    if (finished)
                    {
                        var published = content.Published.HasValue ? content.Published.Value : false;
                        if (passed && !published)
                        {
                            ServiceFactory.TextContentManager.Update(repository, new Schema(repository, content.SchemaName), content.UUID
                                   , new string[] { "Published" }, new object[] { true }, userName);
                        }
                    }
                    WorkflowHistory history = new WorkflowHistory()
                    {
                        Id = WorkflowHistoryProvider.All(content).Count() + 1,
                        Repository = repository,
                        WorkflowName = pendingItem.WorkflowName,
                        WorkflowItemSequence = pendingItem.WorkflowItemSequence,
                        ItemDisplayName = pendingItem.ItemDisplayName,
                        ContentFolder = pendingItem.ContentFolder,
                        ContentUUID = pendingItem.ContentUUID,
                        ContentSummary = content.GetSummary(),
                        RoleName = pendingItem.RoleName,
                        Passed = passed,
                        ProcessingUtcDate = DateTime.UtcNow,
                        ProcessingUser = userName,
                        Finished = finished,
                        Comment = comment
                    };

                    WorkflowHistoryProvider.Add(history);
                }
                PendingWorkflowItemProvider.Remove(pendingItem);
            }

        }

        private WorkflowItem GetNextWorkflowItem(Repository repository, Workflow workflow, int workflowItemSequence)
        {
            WorkflowItem nextWorkflowItem = null;
            if (workflow != null && workflow.Items != null && workflow.Items.Length > 0)
            {
                nextWorkflowItem = workflow.Items.Where(it => it.Sequence == workflowItemSequence).FirstOrDefault();
            }
            return nextWorkflowItem;
        }

        public IEnumerable<TextContent> GetPendWorkflowItemForContents(Repository repository, TextContent[] contents, string userName)
        {
            var workflowItems = GetPendingWorkflowItems(repository, userName).Select(it => GetPendingWorkflowItem(repository, it.RoleName, it.Name)).ToArray();

            foreach (var content in contents)
            {
                var workflowItem = workflowItems.Where(it => content.UUID.Equals(it.ContentUUID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                content["_WorkflowItem_"] = workflowItem;
            }
            return contents;
        }
        public TextContent GetPendWorkflowItemForContent(Repository repository, TextContent textContent, string userName)
        {
            return GetPendWorkflowItemForContents(repository, new[] { textContent }, userName).First();
        }

        #endregion

        #region History & Permission

        public IEnumerable<WorkflowHistory> GetWorkflowHistory(Repository repository, TextContent content)
        {
            return WorkflowHistoryProvider.All(content);
        }

        public bool AvailableToPublish(TextFolder textFolder, string userName)
        {
            if (IsAdministrator(userName))
            {
                return true;
            }
            textFolder = textFolder.AsActual();

            bool allowed = false;
            if (!textFolder.EnabledWorkflow)
            {
                allowed = true;
            }
            else
            {
                var roles = GetRoles(userName);
                var workflow = Get(textFolder.Repository, textFolder.WorkflowName);
                if (workflow.Items == null || workflow.Items.Length == 0)
                {
                    allowed = false;
                }
                else
                {
                    var lastItem = workflow.Items.Last();
                    allowed = roles.Where(it => it.EqualsOrNullEmpty(lastItem.RoleName, StringComparison.OrdinalIgnoreCase)).Count() > 0;
                }
            }
            return allowed;
        }

        public bool AvailableViewContent(TextFolder textFolder, string userName)
        {
            if (IsAdministrator(userName))
            {
                return true;
            }
            textFolder = textFolder.AsActual();
            bool allowed = false;
            if (textFolder != null)
            {
                var roles = GetRoles(userName);
                if (!textFolder.EnabledWorkflow)
                {
                    if (textFolder.Roles == null || textFolder.Roles.Length == 0)
                    {
                        allowed = true;
                    }
                    else
                    {
                        allowed = textFolder.Roles.Where(it => roles.Any(r => it.EqualsOrNullEmpty(r, StringComparison.OrdinalIgnoreCase))).Count() > 0;
                    }
                }
                else
                {
                    var workflow = Get(textFolder.Repository, textFolder.WorkflowName);
                    if (workflow.Items == null || workflow.Items.Length == 0)
                    {
                        allowed = false;
                    }
                    else
                    {
                        allowed = workflow.Items.Any(it => roles.Any(r => r.EqualsOrNullEmpty(it.RoleName, StringComparison.OrdinalIgnoreCase)));
                    }
                }
            }

            return allowed;
        }

        public bool AvailableToEditContent(TextContent content, string userName)
        {
            var repository = content.GetRepository().AsActual();
            var folder = content.GetFolder();
            var available = AvailableViewContent(folder, userName);

            if (available == true && repository.EnableWorkflow == true)
            {
                available = AvailableToPublish(folder, userName);
                if (!available)
                {
                    var workflowItem = GetPendWorkflowItemForContent(content.GetRepository(), content, userName);
                    available = workflowItem["_WorkflowItem_"] != null;
                }
            }
            return available;
        }

        protected virtual bool IsAdministrator(string userName)
        {
            return false;
        }
        protected virtual string[] GetRoles(string userName)
        {
            return new string[0];
        }

        #endregion

    }
}
