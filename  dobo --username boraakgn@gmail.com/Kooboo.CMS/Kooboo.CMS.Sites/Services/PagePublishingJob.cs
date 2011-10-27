﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kooboo.Job;
using Kooboo.CMS.Sites.Models;
using System.Web;

namespace Kooboo.CMS.Sites.Services
{
    public class PagePublishingJob : IJob
    {
        public void Execute(object executionState)
        {
            foreach (var site in ServiceFactory.SiteManager.All())
            {
                PublishSitePages(site);
            }

        }
        private void PublishSitePages(Site site)
        {
            var queue = ServiceFactory.PageManager.PagePublishingProvider.All(site).Select(it => it.AsActual());
            foreach (var item in queue)
            {
                var page = new Page(site, item.PageName).AsActual();
                var removeQueueItem = false;
                if (DateTime.UtcNow > item.UtcDateToPublish)
                {
                    if (page.Published == false)
                    {
                        ServiceFactory.PageManager.Publish(page, item.PublishDraft, item.UserName);
                        removeQueueItem = true;
                    }

                    if (item.Period)
                    {
                        removeQueueItem = false;
                        if (DateTime.UtcNow > item.UtcDateToOffline)
                        {
                            if (page.Published == true)
                            {
                                ServiceFactory.PageManager.Offline(page,item.UserName);
                                removeQueueItem = true;
                            }
                        }
                    }
                }


                if (removeQueueItem)
                {
                    ServiceFactory.PageManager.PagePublishingProvider.Remove(item);
                }
            }
        }
        public void Error(Exception e)
        {
            Kooboo.HealthMonitoring.Log.LogException(e);
        }
    }
    public class StartPagePublishingJobModule : IHttpModule
    {
        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            Job.Jobs.Instance.AttachJob("PagePublishingJob", new PagePublishingJob(), 60000, null);
        }
    }
}
