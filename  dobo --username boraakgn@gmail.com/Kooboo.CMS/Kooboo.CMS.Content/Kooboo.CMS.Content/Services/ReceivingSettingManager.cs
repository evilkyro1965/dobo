using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Persistence;

namespace Kooboo.CMS.Content.Services
{
    public class ReceivingSettingManager : ManagerBase<ReceivingSetting>
    {
        public override ReceivingSetting Get(Repository repository, string name)
        {
            return GetProvider(repository).Get(new ReceivingSetting { Repository = repository, Name = name });
        }

        private IReceivingSettingProvider GetProvider(Repository repository)
        {
            return (IReceivingSettingProvider)GetDBProvider();
        }

        #region Process

        public virtual void ReceiveContent(Repository repository, TextContent originalContent, ContentAction action)
        {
            //bool processed = false;
            var allReceivers = All(repository, "").Select(o => Get(repository, o.Name));
            foreach (var receivingSetting in allReceivers)
            {
                if (CheckSetting(originalContent, receivingSetting, action))
                {
                    ProcessMessage(repository, originalContent, receivingSetting.ReceivingFolder, receivingSetting.KeepStatus, action);

                    //processed = true;
                }
            }
            //if (processed == false)
            //{
            //    ReceivedMessage message = new ReceivedMessage()
            //    {
            //        Repository = repository,

            //        Id = UniqueIdGenerator.GetInstance().GetBase32UniqueId(10),
            //        SendingRepository = receivedContent.Repository,
            //        SendingFolder = receivedContent.FolderName,
            //        ContentUUID = receivedContent.UUID,
            //        ContentType = receivedContent.ContentType,
            //        Summarize = receivedContent.GetSummary(),
            //        Action = action,
            //        UtcReceivedDate = DateTime.Now.ToUniversalTime()
            //    };
            //    Services.ServiceFactory.ReceivedMessageManager.Add(repository, message);
            //}
        }

        public virtual void ProcessMessage(Repository repository, TextContent originalContent, string receivingFolder, bool keepStatus, ContentAction action)
        {

            var contentManager = Services.ServiceFactory.TextContentManager;

            var targetFolder = new TextFolder(repository, receivingFolder).AsActual();

            if ((ContentAction.Add & action) == action && (originalContent.Published.HasValue && originalContent.Published.Value == true))
            {
                contentManager.Copy(originalContent, targetFolder, keepStatus, true, null);
            }
            else if ((ContentAction.Update & action) == action)
            {
                UpdateAction(repository, originalContent, contentManager, targetFolder, keepStatus);
            }
            else if ((ContentAction.Delete & action) == action)
            {
                DeleteAction(repository, originalContent, contentManager, targetFolder);
            }

        }

        private static void DeleteAction(Repository repository, TextContent originalContent, TextContentManager contentManager, TextFolder targetFolder)
        {
            foreach (var drivedContent in DrivedContentHelper.GetContentsByOriginalUUID(targetFolder, originalContent.UUID))
            {
                if (drivedContent.IsLocalized != null && drivedContent.IsLocalized.Value == false)
                {
                    contentManager.Delete(repository, targetFolder, drivedContent.UUID);
                }
            }
        }

        private static void UpdateAction(Repository repository, TextContent originalContent, TextContentManager contentManager, TextFolder targetFolder, bool keepStatus)
        {
            var contents = DrivedContentHelper.GetContentsByOriginalUUID(targetFolder, originalContent.UUID).ToArray();

            if (originalContent.Published.HasValue && originalContent.Published.Value == true && contents.Length == 0)
            {
                contentManager.Copy(originalContent, targetFolder, keepStatus, true, null);
            }
            else
            {
                foreach (var drivedContent in contents)
                {
                    if (drivedContent.IsLocalized != null && drivedContent.IsLocalized.Value == false)
                    {
                        var values = DrivedContentHelper.ExcludeBasicFields(originalContent);
                        if (keepStatus)
                        {
                            if (originalContent.Published.HasValue)
                            {
                                values["Published"] = originalContent.Published.Value.ToString();
                            }
                        }
                        contentManager.Update(repository, targetFolder, drivedContent.UUID, values);
                    }
                }
            }
        }

        private bool CheckSetting(ContentBase content, ReceivingSetting receivingSetting, ContentAction action)
        {
            if (receivingSetting.ReceivingFolder == null && receivingSetting.ReceivingFolder.Length == 0)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(receivingSetting.SendingRepository) && string.Compare(receivingSetting.SendingRepository, content.Repository, true) != 0)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(receivingSetting.SendingFolder) && string.Compare(receivingSetting.SendingFolder, content.FolderName, true) != 0)
            {
                return false;
            }
            //if (receivingSetting.Published.HasValue && content.Published != receivingSetting.Published.Value)
            //{
            //    return false;
            //}

            //if ((receivingSetting.AcceptAction & action) != action)
            //{
            //    return false;
            //}

            return true;
        }
        #endregion

    }
}
