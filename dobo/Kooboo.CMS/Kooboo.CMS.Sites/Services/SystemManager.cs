﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Management;
using System.IO;
using Kooboo.CMS.Sites.Models;
using System.Net.Sockets;
using Kooboo.Extensions;

namespace Kooboo.CMS.Sites.Services
{
    public class DiagnosisResult
    {
        public WebApplicationInformation WebApplicationInformation { get; set; }
        public string ContentProvider { get; set; }
        public DiagnosisItem[] DiagnosisItems { get; set; }
    }
    public enum DiagnosisResultType
    {
        Passed,
        Failed,
        Warning
    }
    public class DiagnosisItem
    {
        public string Name { get; set; }
        public DiagnosisResultType Result { get; set; }
        public string Message { get; set; }
    }


    public class SystemManager
    {
        public virtual DiagnosisResult Diagnosis(Site site)
        {
            DiagnosisResult result = new DiagnosisResult();

            result.WebApplicationInformation = WebBaseEvent.ApplicationInformation;
            result.ContentProvider = Kooboo.CMS.Content.Persistence.Providers.DefaultProviderFactory.Name;

            result.DiagnosisItems = new[] {
                CheckCms_Data(),
                CheckDbConnection(),
                CheckSmtp(site),
                CheckDomain(site)
            };

            return result;
        }

        private DiagnosisItem CheckCms_Data()
        {
            DiagnosisItem item = new DiagnosisItem() { Name = "Cms_Data folder read/write permission" };
            try
            {
                string cms_dataFolder = Path.Combine(Kooboo.Settings.BaseDirectory, "Cms_Data");
                var tempFileName = Path.Combine(cms_dataFolder, "test.txt");
                File.WriteAllText(tempFileName, "Test if Kooboo cms has read/write permission on this folder.");
                File.Delete(tempFileName);
                item.Result = DiagnosisResultType.Passed;
            }
            catch
            {
                item.Result = DiagnosisResultType.Failed;
                item.Message = "Please verify that the IIS ASP.NET user of Kooboo CMS application has read/write permission on this folder.";
            }

            return item;
        }

        private DiagnosisItem CheckDbConnection()
        {
            DiagnosisItem item = new DiagnosisItem() { Name = "Content db connectivity" };
            bool passed = true;
            try
            {
                passed = Kooboo.CMS.Content.Persistence.Providers.RepositoryProvider.TestDbConnection();
            }
            catch
            {
                passed = false;
            }


            if (passed)
            {
                item.Result = DiagnosisResultType.Passed;
            }
            else
            {
                item.Result = DiagnosisResultType.Failed;
                item.Message = "The connection string of content repository was not correctly configured, please check online documentation to configure content providers";
            }
            return item;
        }

        private DiagnosisItem CheckSmtp(Site site)
        {
            DiagnosisItem item = new DiagnosisItem() { Name = "SMTP connectivity" };
            if (site.Smtp == null || string.IsNullOrEmpty(site.Smtp.Host))
            {
                item.Result = DiagnosisResultType.Warning;
                item.Message = @"The SMTP server was not correctly setup, please configure it under System\Settings\SMTP";
            }


            return item;
        }

        private DiagnosisItem CheckDomain(Site site)
        {
            DiagnosisItem item = new DiagnosisItem() { Name = "Site domain setting" };
            if (site.Domains == null || site.Domains.Where(it => !string.IsNullOrWhiteSpace(it)).Count() == 0)
            {
                item.Result = DiagnosisResultType.Warning;
                item.Message = @"No domain is assigned for this website, please configure it under System\Settings";
            }
            else
            {
                foreach (var domain in site.Domains)
                {
                    if (domain.Contains("http://", StringComparison.OrdinalIgnoreCase) || domain.Contains(":"))
                    {
                        item.Result = DiagnosisResultType.Failed;
                        item.Message = "Domain values do not require protocol and port in URL.";
                        break;
                    }
                }
            }
            return item;
        }
    }
}
