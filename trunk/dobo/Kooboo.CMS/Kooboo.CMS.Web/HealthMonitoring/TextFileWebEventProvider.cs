using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using Kooboo.CMS.Sites.Models;
using System.Text;

namespace Kooboo.CMS.Web.HealthMonitoring
{
    public class TextFileWebEventProvider : WebEventProvider
    {
        private static string WebEventsDir = "WebEvents";
        private static object lockerHelper = new object();
        // Methods
        public TextFileWebEventProvider()
        {
        }

        public override void Flush()
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            lock (lockerHelper)
            {
                var fileName = GetLogFile();

                IO.IOUtility.EnsureDirectoryExists(Path.GetDirectoryName(fileName));

                File.AppendAllText(fileName, eventRaised.ToString().Replace("\n", Environment.NewLine));
                File.AppendAllText(fileName, "--------------------------------------------------------------------------------------------------------\r\n");
            }
        }
        private string GetLogFile()
        {
            if (Site.Current == null)
            {
                var filePath = GetLogFile(Path.Combine(Settings.BaseDirectory, "Cms_Data"));
                return filePath;
            }
            else
            {
                var filePath = GetLogFile(Site.Current.PhysicalPath);
                return filePath;
            }
        }
        private string GetLogFile(string baseDir)
        {
            var webEventsDir = Path.Combine(baseDir, WebEventsDir);
            var filePath = Path.Combine(webEventsDir, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log");
            return filePath;
        }
        public override void Shutdown()
        {

        }

    }
}